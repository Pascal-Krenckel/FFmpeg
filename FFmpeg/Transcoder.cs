using FFmpeg.Audio;
using FFmpeg.Codecs;
using FFmpeg.Exceptions;
using FFmpeg.Filters;
using FFmpeg.Filters.AudioFilters;
using FFmpeg.Filters.VideoFilters;
using FFmpeg.Formats;
using FFmpeg.HW;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Diagnostics;
using System.Globalization;

namespace FFmpeg;

/// <summary>
/// Reads a <see cref="MediaSource"/> and writes a <see cref="MediaSink"/>, copying, re-encoding or
/// filtering each mapped stream, like the <c>ffmpeg</c> command line tool with simple filter graphs
/// (<c>-vf</c>/<c>-af</c>).
/// </summary>
/// <remarks>
/// <para>
/// Map the streams first (<see cref="CopyStream"/>, <see cref="TranscodeStream(int, Codec, string?, Action{CodecContext}?, IDictionary{string, string}?)"/>,
/// <see cref="FilterStream(int, string, Action{CodecContext}?, IDictionary{string, string}?)"/>), optionally set <see cref="Start"/> and <see cref="Duration"/>, then call
/// <see cref="Run"/>, <see cref="RunAsync"/> or <see cref="ProcessNext"/> repeatedly.
/// Input streams that are not mapped are discarded.
/// </para>
/// <para>
/// Every re-encoded audio/video output runs through its own filter graph:
/// <c>buffer → [trim] → user filter → [scale/fps/format constraints of the encoder] → buffersink</c>.
/// The graph is configured from the first decoded frame and rebuilt when the decoded format changes
/// mid-stream (e.g. a resolution change); the encoder is opened from the graph output, so encoder
/// properties that are left unset (size, pixel/sample format, sample rate, channel layout, time base, ...)
/// are taken from the filtered frames. Properties that are set are enforced by the graph.
/// Because encoders are opened lazily, the output header is written once every output is initialized;
/// packets produced before that are queued.
/// </para>
/// <para>
/// <see cref="Start"/> seeks to the keyframe before the start; re-encoded streams are then trimmed
/// frame-exactly, copied streams start at that keyframe (like <c>ffmpeg -ss</c> before <c>-i</c>, so
/// their first timestamps can be negative). Output timestamps start at 0 at <see cref="Start"/>.
/// </para>
/// <para>
/// The transcoder owns the source and the sink and disposes them. It is not thread-safe, except for
/// <see cref="TryGetPreviewFrame"/>, <see cref="Position"/> and the events, which may be used from another
/// thread while <see cref="RunAsync"/> is running.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using Transcoder t = Transcoder.Create("in.mkv", "out.mp4");
/// t.CopyStream(t.FindBestInputStream(MediaType.Audio));
/// t.TranscodeStream(t.FindBestInputStream(MediaType.Video), CodecID.MPEG4,
///     filter: "scale=1280:-2",
///     configure: enc => enc.BitRate = 4_000_000);
/// t.Start = TimeSpan.FromSeconds(10);
/// t.Duration = TimeSpan.FromMinutes(1);
/// t.Run();
/// </code>
/// </example>
public sealed unsafe class Transcoder : IDisposable
{
    /// <summary>
    /// The maximum number of packets queued while the output header cannot be written yet because an
    /// encoder has not been initialized (its input produced no frame so far). When it is exceeded, the
    /// remaining encoders are initialized from their decoder parameters.
    /// </summary>
    public const int MaxQueuedPackets = 1024;

    private readonly List<Output> outputs = [];
    private readonly Dictionary<int, Input> inputs = [];
    private readonly AVPacket readPacket = AVPacket.Allocate();
    private readonly AVPacket encodedPacket = AVPacket.Allocate();
    private readonly AVFrame decodedFrame = AVFrame.Allocate();
    private readonly AVFrame filteredFrame = AVFrame.Allocate();
    private readonly Subtitles.Subtitle subtitle = new();
    private readonly List<AVPacket> queuedPackets = [];

    private readonly object previewLock = new();
    private readonly AVFrame previewFrame = AVFrame.Allocate();
    private readonly Stopwatch previewClock = new();
    private bool previewSet;

    private readonly Stopwatch progressClock = new();
    private long positionTicks;

    private State state = State.Created;
    private int busy;
    private bool headerWritten;
    private long originUs;
    private long endUs = long.MaxValue;

    private enum State { Created, Running, Draining, Finished }

    #region Construction

    /// <summary>
    /// Creates a transcoder that reads <paramref name="source"/> and writes <paramref name="sink"/>.
    /// The transcoder takes ownership of both and disposes them.
    /// </summary>
    /// <param name="source">The input.</param>
    /// <param name="sink">The output. Its global metadata is initialized from the input.</param>
    public Transcoder(MediaSource source, MediaSink sink)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Sink = sink ?? throw new ArgumentNullException(nameof(sink));
        Sink.Metadata.Init(source.Metadata);
    }

    /// <summary>Creates a transcoder from an opened demuxer and muxer.</summary>
    public static Transcoder Create(DemuxerContext source, MuxerContext sink) => new(new MediaSource(source), new MediaSink(sink));

    /// <summary>Creates a transcoder from an opened demuxer and muxer, decoding with the given hardware device type.</summary>
    public static Transcoder Create(DemuxerContext source, DeviceType hwSourceType, MuxerContext sink) => new(new MediaSource(source, hwSourceType), new MediaSink(sink));

    /// <summary>
    /// Opens <paramref name="inputUrl"/> and creates <paramref name="outputUrl"/>.
    /// </summary>
    /// <param name="inputUrl">The input file or URL.</param>
    /// <param name="outputUrl">The output file or URL.</param>
    /// <param name="outputFormat">The output format, or <see langword="null"/> to guess it from <paramref name="outputUrl"/>.</param>
    /// <param name="hwSourceType">The hardware device type used for decoding, if any.</param>
    public static Transcoder Create(string inputUrl, string outputUrl, OutputFormat? outputFormat = null, DeviceType hwSourceType = DeviceType.None)
    {
        MediaSource source = MediaSource.Open(inputUrl, deviceType: hwSourceType);
        try
        {
            MediaSink sink = MediaSink.Create(outputUrl, outputFormat)
                ?? throw new ArgumentException($"Could not create the output '{outputUrl}'.", nameof(outputUrl));
            return new Transcoder(source, sink);
        }
        catch
        {
            source.Dispose();
            throw;
        }
    }

    #endregion

    #region Streams and configuration

    /// <summary>Gets the input.</summary>
    public MediaSource Source { get; }

    /// <summary>Gets the output.</summary>
    public MediaSink Sink { get; }

    /// <summary>Gets the input streams.</summary>
    public IReadOnlyList<AVStream> InputStreams => Source.Streams;

    /// <summary>Gets the output streams, in the order they were mapped.</summary>
    public IReadOnlyList<AVStream> OutputStreams => Sink.Streams;

    /// <summary>
    /// Gets the encoder of each output stream, or <see langword="null"/> for copied streams.
    /// Encoders can be configured until they are opened (see the class remarks).
    /// </summary>
    public IReadOnlyList<CodecContext?> OutputCodecs => Enumerable.Range(0, Sink.Streams.Count).Select(i => outputs.Find(o => o.Index == i)?.Encoder).ToList();

    /// <summary>Finds the best input stream of the given type, or returns a negative error code.</summary>
    public int FindBestInputStream(MediaType mediaType) => Source.FindBestStream(mediaType);

    /// <summary>
    /// Gets or sets the input position to start at, relative to the start of the input. Default: 0.
    /// Can only be changed before processing starts.
    /// </summary>
    public TimeSpan Start
    {
        get;
        set
        {
            CheckConfigurable();
            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value));
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum output duration, or <see cref="TimeSpan.Zero"/> to transcode until the end
    /// of the input. Can only be changed before processing starts.
    /// </summary>
    public TimeSpan Duration
    {
        get;
        set
        {
            CheckConfigurable();
            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value));
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the input chapters inside the transcoded range are copied. Default: <see langword="true"/>.
    /// </summary>
    public bool CopyChapters { get; set; } = true;

    /// <summary>
    /// Adds an output stream that copies the input stream without decoding it (<c>-c copy</c>).
    /// </summary>
    /// <param name="inputIndex">The index of the input stream.</param>
    /// <returns>The index of the new output stream.</returns>
    /// <remarks>The codec tag is reset so the muxer can choose one that is valid for the output format.</remarks>
    public int CopyStream(int inputIndex)
    {
        CheckConfigurable();
        AVStream inputStream = GetInputStream(inputIndex);
        int outputIndex = Sink.Streams.Count;
        AVStream stream = Sink.AddStream(inputStream);
        CodecParameters_ref parameters = stream.CodecParameters;
        parameters.CodecTag = 0;
        CopyStreamProperties(inputStream, stream);
        AddOutput(new Output(outputIndex, inputIndex, inputStream.MediaType, null, null, null));
        return outputIndex;
    }

    /// <summary>
    /// Adds an output stream that decodes the input stream, runs it through an optional simple filter
    /// graph and encodes it with typed encoder settings, e.g. <see cref="Codecs.Video.X264"/> or
    /// <see cref="Codecs.Audio.Opus"/>.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="encoder">
    /// The encoder settings; they are applied when the stream is added (<see cref="EncoderSettings.Configure"/>), so
    /// later changes have no effect. Size, pixel format, frame rate, sample format, sample rate and channel layout
    /// that are set are enforced; unset ones are taken from the filter output.
    /// </param>
    /// <param name="filter">
    /// A filter graph description with one input and one output, as for <c>ffmpeg -vf</c>/<c>-af</c>, or
    /// <see langword="null"/>.
    /// </param>
    /// <returns>The index of the new output stream.</returns>
    /// <exception cref="ArgumentException">The encoder rejects a setting, or does not match the stream type.</exception>
    public int TranscodeStream(int inputIndex, EncoderSettings encoder, string? filter = null)
        => TranscodeStream(inputIndex, encoder.Codec, filter, encoder.Configure);

    /// <summary>
    /// Adds an output stream that decodes the input stream, runs it through an optional simple filter
    /// graph and encodes it with <paramref name="encoder"/>.
    /// </summary>
    /// <param name="inputIndex">The index of an audio, video or subtitle input stream.</param>
    /// <param name="encoder">The encoder to use.</param>
    /// <param name="filter">
    /// A filter graph description with one input and one output, as for <c>ffmpeg -vf</c>/<c>-af</c>
    /// (e.g. <c>"scale=1280:-2,fps=30"</c>), or <see langword="null"/>. Not supported for subtitles.
    /// </param>
    /// <param name="configure">
    /// Called with the new encoder context, e.g. to set the bit rate. Properties set here (size, pixel format,
    /// frame rate, sample format, sample rate, channel layout, time base) are enforced; unset ones are taken
    /// from the filter output when the encoder is opened.
    /// </param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened (e.g. <c>preset</c>, <c>crf</c>).</param>
    /// <returns>The index of the new output stream.</returns>
    public int TranscodeStream(int inputIndex, Codec encoder, string? filter = null, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
        => AddTranscodedStream(inputIndex, encoder, string.IsNullOrWhiteSpace(filter) ? null : new TextFilter(filter!), configure, encoderOptions);

    /// <summary>
    /// Adds an output stream that decodes the input stream, runs it through a filter graph created by
    /// <paramref name="filterFactory"/> and encodes it with <paramref name="encoder"/>.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="encoder">The encoder to use.</param>
    /// <param name="filterFactory">
    /// Returns the filter graph description (one input, one output) for the frames described by the
    /// <see cref="FilterGraphContext"/>, e.g. <c>ctx =&gt; $"scale={ctx.Width / 2}:-2"</c>. It is called
    /// whenever the transcoder builds the graph: before the first frame and again when the decoded
    /// format changes mid-stream (<see cref="FilterGraphContext.IsRebuild"/>).
    /// </param>
    /// <param name="configure">Called with the new encoder context; see <see cref="TranscodeStream(int, Codec, string?, Action{CodecContext}?, IDictionary{string, string}?)"/>.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    /// <returns>The index of the new output stream.</returns>
    public int TranscodeStream(int inputIndex, Codec encoder, Func<FilterGraphContext, string> filterFactory, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
        => AddTranscodedStream(inputIndex, encoder, new TextFactoryFilter(filterFactory ?? throw new ArgumentNullException(nameof(filterFactory))), configure, encoderOptions);

    /// <summary>
    /// Adds an output stream that decodes the input stream, runs it through a filter graph object created
    /// by <paramref name="filterFactory"/> and encodes it with <paramref name="encoder"/>.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="encoder">The encoder to use.</param>
    /// <param name="filterFactory">
    /// Creates a new, unconfigured filter graph with one unlinked input and one unlinked output for the
    /// frames described by the <see cref="FilterGraphContext"/>. It is called whenever the transcoder builds
    /// the graph: before the first frame and again when the decoded format changes mid-stream. The
    /// transcoder takes ownership of the returned graph and lists.
    /// This is the recommended way to use filter graph objects that cannot be described by a string.
    /// </param>
    /// <param name="configure">Called with the new encoder context; see <see cref="TranscodeStream(int, Codec, string?, Action{CodecContext}?, IDictionary{string, string}?)"/>.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    /// <returns>The index of the new output stream.</returns>
    public int TranscodeStream(int inputIndex, Codec encoder, FilterGraphFactory filterFactory, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
        => AddTranscodedStream(inputIndex, encoder, new GraphFactoryFilter(filterFactory ?? throw new ArgumentNullException(nameof(filterFactory))), configure, encoderOptions);

    /// <summary>
    /// Adds an output stream that decodes the input stream, runs it through <paramref name="filter"/>
    /// and encodes it with <paramref name="encoder"/>.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="encoder">The encoder to use.</param>
    /// <param name="inputs">The unlinked input of <paramref name="filter"/>; must contain exactly one entry.</param>
    /// <param name="filter">
    /// An unconfigured filter graph with one unlinked input and one unlinked output, e.g. from
    /// <see cref="FilterGraph.Create(out FilterInOutList?, string, out FilterInOutList?)"/> or built with
    /// <see cref="FilterContext.Create(string, Filter, string?, FilterGraph)"/>. It is used as a template:
    /// every graph the transcoder configures is a <see cref="FilterGraph.Copy"/> of it.
    /// </param>
    /// <param name="outputs">The unlinked output of <paramref name="filter"/>; must contain exactly one entry.</param>
    /// <param name="configure">Called with the new encoder context; see <see cref="TranscodeStream(int, Codec, string?, Action{CodecContext}?, IDictionary{string, string}?)"/>.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    /// <returns>The index of the new output stream.</returns>
    /// <remarks>
    /// <para>The transcoder takes ownership of <paramref name="inputs"/>, <paramref name="filter"/> and <paramref name="outputs"/>, also when the call fails.</para>
    /// <para>
    /// <b>Warning:</b> passing a filter graph object is possible, but not recommended if the graph may have
    /// to be set up again, which happens when the decoded format changes mid-stream (e.g. a resolution change).
    /// A configured graph cannot be flushed or reset, so the transcoder recreates it with
    /// <see cref="FilterGraph.Copy"/>, which rebuilds the filters only from their options and links:
    /// state that is not an option (e.g. hardware device or frames contexts) is lost, and options that
    /// were derived from the original format are kept unchanged. Prefer a filter description string or a
    /// factory (<see cref="TranscodeStream(int, Codec, FilterGraphFactory, Action{CodecContext}?, IDictionary{string, string}?)"/>),
    /// which create the graph from scratch for every build.
    /// </para>
    /// </remarks>
    public int TranscodeStream(int inputIndex, Codec encoder, FilterInOutList inputs, FilterGraph filter, FilterInOutList outputs, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
    {
        TemplateFilter? template = null;
        try
        {
            template = TemplateFilter.Create(inputs, filter, outputs);
            int index = AddTranscodedStream(inputIndex, encoder, template, configure, encoderOptions);
            template = null; // owned by the output now
            return index;
        }
        finally
        {
            // TemplateFilter.Create takes over the graph (and disposes it if it fails); the lists are
            // only needed to find the unlinked pads.
            inputs?.Dispose();
            outputs?.Dispose();
            template?.Dispose();
        }
    }

    private int AddTranscodedStream(int inputIndex, Codec encoder, StreamFilter? filter, Action<CodecContext>? configure, IDictionary<string, string>? encoderOptions)
    {
        CheckConfigurable();
        AVStream inputStream = GetInputStream(inputIndex);
        MediaType type = inputStream.MediaType;
        if (type is not (MediaType.Audio or MediaType.Video or MediaType.Subtitle))
            throw new NotSupportedException($"Input stream #{inputIndex} is a {type} stream; only audio, video and subtitle streams can be re-encoded. Use CopyStream instead.");
        if (!encoder.IsEncoder)
            throw new ArgumentException($"{encoder.Name} is not an encoder.", nameof(encoder));
        if (encoder.MediaType != type)
            throw new ArgumentException($"The encoder {encoder.Name} encodes {encoder.MediaType}, but input stream #{inputIndex} is a {type} stream.", nameof(encoder));
        if (type == MediaType.Subtitle && filter != null)
            throw new NotSupportedException("Subtitle streams cannot be filtered.");
        if (Source.CodecContexts[inputIndex].Codec.codec == null)
            throw new NotSupportedException($"There is no decoder for input stream #{inputIndex} ({inputStream.CodecParameters.CodecId}).");

        CodecContext context = CodecContext.Allocate(encoder);
        try
        {
            configure?.Invoke(context);
            int outputIndex = Sink.Streams.Count;
            AVStream stream = Sink.AddStream(encoder);
            CopyStreamProperties(inputStream, stream);
            Output output = new(outputIndex, inputIndex, type, context, filter ?? new TextFilter(null),
                encoderOptions == null ? null : [with(encoderOptions)])
            {
                UserFrameRate = context.FrameRate.IsValidTimeBase,
            };
            AddOutput(output);
            return outputIndex;
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="TranscodeStream(int, Codec, string?, Action{CodecContext}?, IDictionary{string, string}?)"/>
    /// <param name="inputIndex">The index of an audio, video or subtitle input stream.</param>
    /// <param name="encoder">The id of the codec to encode with; the default encoder for it is used.</param>
    /// <param name="filter">A filter graph description with one input and one output, or <see langword="null"/>.</param>
    /// <param name="configure">Called with the new encoder context.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    public int TranscodeStream(int inputIndex, CodecID encoder, string? filter = null, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
        => TranscodeStream(inputIndex, Codec.FindEncoder(encoder) ?? throw new NotSupportedException($"No encoder found for {encoder}."), filter, configure, encoderOptions);

    /// <summary>
    /// Adds an output stream that filters the input stream and re-encodes it with the default encoder
    /// of the input codec.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="filter">A filter graph description with one input and one output, e.g. <c>"hflip"</c>.</param>
    /// <param name="configure">Called with the new encoder context.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    /// <returns>The index of the new output stream.</returns>
    public int FilterStream(int inputIndex, string filter, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null) => string.IsNullOrWhiteSpace(filter)
            ? throw new ArgumentException("A filter is required.", nameof(filter))
            : TranscodeStream(inputIndex, GetInputEncoder(inputIndex), filter, configure, encoderOptions);

    /// <summary>
    /// Adds an output stream that filters the input stream with a graph from <paramref name="filterFactory"/>
    /// and re-encodes it with the default encoder of the input codec.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="filterFactory">See <see cref="TranscodeStream(int, Codec, Func{FilterGraphContext, string}, Action{CodecContext}?, IDictionary{string, string}?)"/>.</param>
    /// <param name="configure">Called with the new encoder context.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    /// <returns>The index of the new output stream.</returns>
    public int FilterStream(int inputIndex, Func<FilterGraphContext, string> filterFactory, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
        => TranscodeStream(inputIndex, GetInputEncoder(inputIndex), filterFactory, configure, encoderOptions);

    /// <summary>
    /// Adds an output stream that filters the input stream with a graph object from <paramref name="filterFactory"/>
    /// and re-encodes it with the default encoder of the input codec.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="filterFactory">See <see cref="TranscodeStream(int, Codec, FilterGraphFactory, Action{CodecContext}?, IDictionary{string, string}?)"/>.</param>
    /// <param name="configure">Called with the new encoder context.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    /// <returns>The index of the new output stream.</returns>
    public int FilterStream(int inputIndex, FilterGraphFactory filterFactory, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
        => TranscodeStream(inputIndex, GetInputEncoder(inputIndex), filterFactory, configure, encoderOptions);

    /// <summary>
    /// Adds an output stream that filters the input stream with a filter graph object and re-encodes it
    /// with the default encoder of the input codec.
    /// </summary>
    /// <param name="inputIndex">The index of an audio or video input stream.</param>
    /// <param name="inputs">The unlinked input of <paramref name="filter"/>; must contain exactly one entry.</param>
    /// <param name="filter">An unconfigured filter graph used as a template, see <see cref="TranscodeStream(int, Codec, FilterInOutList, FilterGraph, FilterInOutList, Action{CodecContext}?, IDictionary{string, string}?)"/>.</param>
    /// <param name="outputs">The unlinked output of <paramref name="filter"/>; must contain exactly one entry.</param>
    /// <param name="configure">Called with the new encoder context.</param>
    /// <param name="encoderOptions">Private encoder options passed when the encoder is opened.</param>
    /// <returns>The index of the new output stream.</returns>
    /// <remarks>
    /// <para>The transcoder takes ownership of <paramref name="inputs"/>, <paramref name="filter"/> and <paramref name="outputs"/>, also when the call fails.</para>
    /// <para>
    /// <b>Warning:</b> passing a filter graph object is possible, but not recommended if the graph may have to
    /// be set up again (when the decoded format changes mid-stream): it is then recreated with
    /// <see cref="FilterGraph.Copy"/>, which only keeps the filters' options and links. Prefer a filter
    /// string or a factory.
    /// </para>
    /// </remarks>
    public int FilterStream(int inputIndex, FilterInOutList inputs, FilterGraph filter, FilterInOutList outputs, Action<CodecContext>? configure = null, IDictionary<string, string>? encoderOptions = null)
    {
        Codec encoder;
        try
        {
            encoder = GetInputEncoder(inputIndex);
        }
        catch
        {
            inputs?.Dispose();
            filter?.Dispose();
            outputs?.Dispose();
            throw;
        }
        return TranscodeStream(inputIndex, encoder, inputs, filter, outputs, configure, encoderOptions);
    }

    private Codec GetInputEncoder(int inputIndex)
    {
        CodecID codecId = GetInputStream(inputIndex).CodecParameters.CodecId;
        return Codec.FindEncoder(codecId) ?? throw new NotSupportedException($"No encoder found for {codecId}; use TranscodeStream with another encoder.");
    }

    private AVStream GetInputStream(int inputIndex)
        => inputIndex >= 0 && inputIndex < Source.Streams.Count ? Source.Streams[inputIndex] : throw new ArgumentOutOfRangeException(nameof(inputIndex));

    private static void CopyStreamProperties(AVStream inputStream, AVStream outputStream)
    {
        outputStream.Metadata.Init(inputStream.Metadata);
        outputStream.Disposition = inputStream.Disposition;
    }

    private void AddOutput(Output output)
    {
        outputs.Add(output);
        if (!inputs.TryGetValue(output.InputIndex, out Input? input))
            inputs[output.InputIndex] = input = new Input(output.InputIndex, Source.Streams[output.InputIndex].MediaType);
        input.Outputs.Add(output);
    }

    private void CheckConfigurable()
    {
        CheckDisposed();
        if (state != State.Created)
            throw new InvalidOperationException("The transcoder has already started.");
    }

    #endregion

    #region Progress and preview

    /// <summary>
    /// Gets the output time written so far (the end of the latest packet, relative to <see cref="Start"/>).
    /// </summary>
    public TimeSpan Position => TimeSpan.FromTicks(Interlocked.Read(ref positionTicks));

    /// <summary>
    /// Gets the expected output duration, or <see cref="TimeSpan.Zero"/> if it is unknown.
    /// </summary>
    public TimeSpan ExpectedDuration
    {
        get
        {
            long inputUs = Source.FormatContext.Duration;
            TimeSpan remaining = inputUs > 0 ? TimeSpan.FromTicks(inputUs * 10) - Start : TimeSpan.Zero;
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;
            return Duration > TimeSpan.Zero && (remaining == TimeSpan.Zero || Duration < remaining) ? Duration : remaining;
        }
    }

    /// <summary>Gets whether all streams have been written and the trailer has been written.</summary>
    public bool IsFinished => state == State.Finished;

    /// <summary>Gets or sets the minimum time between two <see cref="ProgressChanged"/> events. Default: 500 ms.</summary>
    public TimeSpan ProgressInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Raised on the processing thread at most every <see cref="ProgressInterval"/>, and once when finished.
    /// </summary>
    public event EventHandler<TranscodingProgress>? ProgressChanged;

    /// <summary>
    /// Gets or sets the output stream used for preview frames. A negative value selects the first video
    /// output, or the first audio output if there is no video. Can only be changed before processing starts.
    /// </summary>
    public int PreviewOutputStreamIndex
    {
        get;
        set
        {
            CheckConfigurable();
            field = value;
        }
    } = -1;

    /// <summary>
    /// Gets or sets the minimum time between two preview frames. Default: 1 s. A negative value disables previews.
    /// </summary>
    /// <remarks>
    /// For re-encoded outputs the preview is the filtered frame passed to the encoder. For copied outputs
    /// it is a decoded keyframe; if the input is not decoded anyway, only keyframes are decoded for it.
    /// </remarks>
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Raised on the processing thread when a new preview frame is available.</summary>
    public event EventHandler? PreviewFrameUpdated;

    /// <summary>
    /// References the latest preview frame into <paramref name="destination"/>. Safe to call from any thread.
    /// </summary>
    /// <param name="destination">The frame to reference the preview into.</param>
    /// <returns><see langword="true"/> if a preview frame was available.</returns>
    public bool TryGetPreviewFrame(AVFrame destination)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        lock (previewLock)
        {
            if (!previewSet)
                return false;
            destination.Reference(previewFrame);
            return true;
        }
    }

    private bool PreviewDue => UpdateInterval >= TimeSpan.Zero && (!previewClock.IsRunning || previewClock.Elapsed >= UpdateInterval);

    private void SetPreview(AVFrame frame)
    {
        lock (previewLock)
        {
            previewFrame.Reference(frame);
            previewSet = true;
        }
        previewClock.Restart();
        PreviewFrameUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void ReportProgress(bool force)
    {
        if (!force && progressClock.IsRunning && progressClock.Elapsed < ProgressInterval)
            return;
        progressClock.Restart();
        ProgressChanged?.Invoke(this, new TranscodingProgress(Position, ExpectedDuration));
    }

    #endregion

    #region Running

    /// <summary>
    /// Processes the next input packet, or finishes the output once the input (or <see cref="Duration"/>) ends.
    /// </summary>
    /// <returns>0 while there is more to do, <see cref="AVResult32.EndOfFile"/> once the trailer has been written.</returns>
    /// <exception cref="FFmpegException">Reading, decoding, filtering, encoding or muxing failed.</exception>
    public AVResult32 ProcessNext()
    {
        Enter();
        try
        {
            return ProcessNextCore();
        }
        finally
        {
            Exit();
        }
    }

    /// <summary>
    /// Transcodes until the end of the input (or <see cref="Duration"/>).
    /// </summary>
    /// <param name="token">
    /// When cancelled, the transcoder stops reading, flushes what was decoded so far, writes the trailer
    /// (so the output is a valid, shorter file) and throws <see cref="OperationCanceledException"/>.
    /// </param>
    public void Run(CancellationToken token = default)
    {
        Enter();
        try
        {
            while (ProcessNextCore() != AVResult32.EndOfFile)
            {
                if (token.IsCancellationRequested)
                {
                    FinishCore();
                    token.ThrowIfCancellationRequested();
                }
            }
        }
        finally
        {
            Exit();
        }
    }

    /// <summary>Runs <see cref="Run"/> on a background thread.</summary>
    /// <param name="token">See <see cref="Run"/>.</param>
    public Task RunAsync(CancellationToken token = default)
        => Task.Factory.StartNew(() => Run(token), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    /// <summary>
    /// Stops reading the input, flushes all decoders, filters and encoders and writes the trailer,
    /// e.g. to end the recording of a live source. Does nothing if already finished.
    /// </summary>
    public void Finish()
    {
        Enter();
        try
        {
            FinishCore();
        }
        finally
        {
            Exit();
        }
    }

    private void Enter()
    {
        CheckDisposed();
        if (Interlocked.Exchange(ref busy, 1) == 1)
            throw new InvalidOperationException("The transcoder is already processing on another thread.");
    }

    private void Exit() => Volatile.Write(ref busy, 0);

    private AVResult32 ProcessNextCore()
    {
        switch (state)
        {
            case State.Finished:
                return AVResult32.EndOfFile;
            case State.Created:
                Initialize();
                return 0;
            case State.Draining:
                FinishCore();
                return AVResult32.EndOfFile;
        }

        AVResult32 res = ReadPacket(out Input? input);
        if (res == AVResult32.EndOfFile)
        {
            state = State.Draining;
            return 0;
        }
        res.ThrowIfError();
        HandlePacket(input!);
        ReportProgress(false);
        return 0;
    }

    private void Initialize()
    {
        if (outputs.Count == 0)
            throw new InvalidOperationException("No stream has been mapped.");

        foreach (Input input in inputs.Values)
            input.Decoding = input.Outputs.Exists(o => o.Encoder != null);

        long startTime = Source.FormatContext.StartTime;
        originUs = (startTime != ffmpeg.AV_NOPTS_VALUE ? startTime : 0) + (Start.Ticks / 10);
        if (Duration > TimeSpan.Zero)
            endUs = originUs + (Duration.Ticks / 10);

        // Seek before discarding the unmapped streams, as ffmpeg does: with all streams active the
        // demuxer seeks to a keyframe of the default (video) stream, and the other streams follow.
        if (Start > TimeSpan.Zero)
        {
            long seekUs = originUs - GetSeekPreroll();
            Source.FlushBuffers();
            AVResult32 res = ffmpeg.avformat_seek_file(Source.FormatContext.Context, -1, long.MinValue, seekUs, seekUs, 0);
            if (res.IsError)
                Logging.Logger.Warning($"[{nameof(Transcoder)}] Could not seek to {Start}: {res}");
        }

        for (int i = 0; i < Source.Streams.Count; i++)
        {
            if (!inputs.ContainsKey(i))
                Source.Streams[i].Discard = DiscardFlags.All;
        }

        // Subtitle encoders do not depend on decoded data: open them now, so that sparse subtitles
        // do not hold back the output header.
        foreach (Output output in outputs.Where(o => o.Encoder != null && o.Type == MediaType.Subtitle))
            OpenSubtitleEncoder(output);

        if (!outputs.Exists(o => o.Index == PreviewOutputStreamIndex))
        {
            Output? preview = outputs.Find(o => o.Type == MediaType.Video && !Source.Streams[o.InputIndex].Disposition.HasFlag(StreamDisposition.AttachedPicture))
                ?? outputs.Find(o => o.Type == MediaType.Audio);
            PreviewOutputStreamIndex = preview?.Index ?? -1;
        }

        state = State.Running;
        TryWriteHeader();
    }

    /// <summary>How much earlier than the start to seek, so decoders deliver the first wanted frame.</summary>
    private long GetSeekPreroll()
    {
        long prerollUs = 0;
        // Like ffmpeg: with B-frames the DTS can lie before the PTS, so seek a bit earlier.
        if (!Source.FormatContext.InputFormat.GetValueOrDefault().Flags.HasFlag(FormatFlags.SeekToPts)
            && Source.Streams.Any(s => s.CodecParameters.VideoDelay > 0))
            prerollUs = 3 * 1_000_000L / 23;

        // Audio decoders drop initial_padding samples after opening, wherever decoding starts (e.g.
        // the priming of an MP4 edit list), and some codecs need seek_preroll samples. Beyond what
        // ffmpeg does, seek early enough that neither cuts into the requested range.
        foreach (Input input in inputs.Values.Where(i => i.Decoding && i.Type == MediaType.Audio))
        {
            CodecParameters_ref parameters = Source.Streams[input.Index].CodecParameters;
            if (parameters.SampleRate > 0)
                prerollUs = Math.Max(prerollUs, ((long)Math.Max(parameters.InitialPadding, 0) + Math.Max(parameters.SeekPreroll, 0)) * 1_000_000L / parameters.SampleRate);
        }
        return prerollUs;
    }

    private AVResult32 ReadPacket(out Input? input)
    {
        while (true)
        {
            input = null;
            if (!inputs.Values.Any(i => !i.Finished))
                return AVResult32.EndOfFile;
            AVResult32 res = Source.ReadPacket(readPacket);
            if (res.IsError)
                return res;
            if (!inputs.TryGetValue(readPacket.StreamIndex, out input) || input.Finished)
                continue;
            if (endUs != long.MaxValue)
            {
                long ts = readPacket.DecompressionTimestamp != ffmpeg.AV_NOPTS_VALUE ? readPacket.DecompressionTimestamp : readPacket.PresentationTimestamp;
                // pts >= dts, so once the dts is past the end, nothing that follows is needed.
                if (ts != ffmpeg.AV_NOPTS_VALUE && ToMicroseconds(ts, readPacket.TimeBase) >= endUs)
                {
                    input.Finished = true;
                    continue;
                }
            }
            return res;
        }
    }

    private void FinishCore()
    {
        if (state == State.Finished)
            return;
        if (state == State.Created)
            Initialize();
        state = State.Draining;

        // 1. Flush the decoders.
        foreach (Input input in inputs.Values.Where(i => i.Decoding && i.Type is MediaType.Audio or MediaType.Video))
        {
            CodecContext decoder = Source.CodecContexts[input.Index];
            if (!decoder.IsOpen)
                continue;
            AVResult32 res = decoder.DrainDecoder();
            if (res.IsError && res != AVResult32.EndOfFile)
                res.ThrowIfError();
            ReceiveDecodedFrames(input, decoder);
        }

        // 2. Flush the filter graphs (initializing the outputs that never got a frame).
        foreach (Output output in outputs.Where(o => o.Encoder != null && o.Type is MediaType.Audio or MediaType.Video))
        {
            if (output.Graph == null)
                BuildGraph(output, SourceFormat.FromDecoder(Source.CodecContexts[output.InputIndex], Source.Streams[output.InputIndex]));
            SendToGraph(output, null);
            PullFilteredFrames(output);
        }

        TryWriteHeader();
        if (!headerWritten)
            throw new InvalidOperationException("The output could not be initialized.");

        // 3. Flush the encoders.
        foreach (Output output in outputs.Where(o => o.Encoder != null && o.Type is MediaType.Audio or MediaType.Video))
            Encode(output, null);

        Sink.WriteTrailer().ThrowIfError();
        state = State.Finished;
        ReportProgress(true);
    }

    #endregion

    #region Packet handling

    private void HandlePacket(Input input)
    {
        bool keyFrame = readPacket.Flags.HasFlag(PacketFlags.Key);

        foreach (Output output in input.Outputs)
        {
            if (output.Encoder != null)
                continue;
            if (!output.CopyStarted)
            {
                // Like ffmpeg, a copied stream starts at a keyframe.
                if (!keyFrame && output.Type == MediaType.Video)
                    continue;
                output.CopyStarted = true;
            }
            readPacket.CloneTo(encodedPacket);
            WritePacket(output, encodedPacket);

            if (output.Index == PreviewOutputStreamIndex && keyFrame && !input.Decoding && PreviewDue)
                DecodePreviewKeyframe(input);
        }

        if (!input.Decoding)
            return;

        CodecContext decoder = Source.CodecContexts[input.Index];
        if (input.Type == MediaType.Subtitle)
        {
            HandleSubtitlePacket(input, decoder);
            return;
        }

        AVResult32 res = decoder.SendPacket(readPacket);
        if (res == AVResult32.InvalidData)
        {
            Logging.Logger.Warning($"[{nameof(Transcoder)}] Skipping an invalid packet of input stream #{input.Index}.");
            return;
        }
        res.ThrowIfError();
        ReceiveDecodedFrames(input, decoder);
    }

    private void ReceiveDecodedFrames(Input input, CodecContext decoder)
    {
        while (true)
        {
            AVResult32 res = decoder.ReceiveFrame(decodedFrame);
            if (res.IsTryAgain || res == AVResult32.EndOfFile)
                return;
            if (res == AVResult32.InvalidData)
            {
                Logging.Logger.Warning($"[{nameof(Transcoder)}] Decoding error in input stream #{input.Index}.");
                continue;
            }
            res.ThrowIfError();

            if (decodedFrame.PresentationTimestamp == ffmpeg.AV_NOPTS_VALUE)
                decodedFrame.PresentationTimestamp = decodedFrame.BestEffortTimestamp;
            if (!decodedFrame.TimeBase.IsValidTimeBase)
                decodedFrame.TimeBase = Source.Streams[input.Index].TimeBase;

            foreach (Output output in input.Outputs)
            {
                if (output.Encoder == null)
                {
                    if (output.Index == PreviewOutputStreamIndex && PreviewDue)
                        SetPreview(decodedFrame);
                    continue;
                }
                FilterFrame(output, decodedFrame);
            }
        }
    }

    private void DecodePreviewKeyframe(Input input)
    {
        // The input is not decoded otherwise: decode this keyframe on its own and reset the decoder.
        CodecContext decoder = Source.CodecContexts[input.Index];
        if (decoder.Codec.codec == null || input.Type is not (MediaType.Video or MediaType.Audio))
            return;
        try
        {
            if (decoder.SendPacket(readPacket).IsError)
                return;
            AVResult32 res = decoder.ReceiveFrame(decodedFrame);
            if (res.IsTryAgain && !decoder.DrainDecoder().IsError)
                res = decoder.ReceiveFrame(decodedFrame);
            if (!res.IsError)
                SetPreview(decodedFrame);
        }
        finally
        {
            decoder.FlushBuffers();
            decodedFrame.Unreference();
        }
    }

    private void HandleSubtitlePacket(Input input, CodecContext decoder)
    {
        AVResult32 res = decoder.DecodeSubtitle(readPacket, subtitle);
        if (res.IsError)
        {
            Logging.Logger.Warning($"[{nameof(Transcoder)}] Could not decode a subtitle of input stream #{input.Index}: {res}");
            return;
        }
        if (subtitle.Count == 0 || subtitle.PresentationTime == ffmpeg.AV_NOPTS_VALUE)
            return;

        long startUs = subtitle.PresentationTime + (subtitle.StartDisplayTime * 1000L);
        long stopUs = subtitle.PresentationTime + (subtitle.EndDisplayTime * 1000L);
        if (stopUs <= originUs || startUs >= endUs)
            return;
        subtitle.PresentationTime -= originUs;

        foreach (Output output in input.Outputs.Where(o => o.Encoder != null))
        {
            output.Encoder!.EncodeSubtitle(encodedPacket, subtitle).ThrowIfError();
            if (encodedPacket.Size == 0)
                continue;
            // EncodeSubtitle returns timestamps relative to the output already.
            WritePacket(output, encodedPacket, shift: false);
        }
    }

    #endregion

    #region Filtering and encoding

    private void FilterFrame(Output output, AVFrame frame)
    {
        // The graph has ended (its end trim was reached); later frames, e.g. reordered ones, are not needed.
        if (output.GraphEnded)
            return;
        if (output.Graph == null)
        {
            BuildGraph(output, SourceFormat.FromFrame(frame, Source.CodecContexts[output.InputIndex]));
        }
        else if (!output.SourceFormat!.Matches(frame))
        {
            // The decoded format changed: flush the old graph and build a new one, like ffmpeg.
            CodecContext encoder = output.Encoder!;
            if (output.Type == MediaType.Audio && encoder.FrameSize > 0 && !encoder.Codec.Capabilities.HasFlag(CodecCapabilities.VariableFrameSize))
            {
                // Flushing would return a short frame, which encoders with a fixed frame size only
                // accept at the very end. Take the complete frames and drop the remainder (< 1 frame).
                PullFilteredFrames(output);
                Logging.Logger.Debug($"[{nameof(Transcoder)}] Audio format of input stream #{output.InputIndex} changed; dropping the samples of an incomplete frame.");
            }
            else
            {
                SendToGraph(output, null);
                PullFilteredFrames(output);
            }
            BuildGraph(output, SourceFormat.FromFrame(frame, Source.CodecContexts[output.InputIndex]));
        }

        SendToGraph(output, frame);
        PullFilteredFrames(output);
    }

    /// <summary>
    /// Sends a frame (or <see langword="null"/> to flush) to the buffer source. When the graph needs no more
    /// input it answers AVERROR_EOF; that ends the output instead of being an error.
    /// </summary>
    private static void SendToGraph(Output output, AVFrame? frame)
    {
        if (output.GraphEnded)
            return;
        AVResult32 res = output.BufferSource!.SendFrame(frame, keepRef: true);
        if (res == AVResult32.EndOfFile)
        {
            output.GraphEnded = true;
            return;
        }
        res.ThrowIfError();
    }

    private void PullFilteredFrames(Output output)
    {
        while (true)
        {
            AVResult32 res = output.BufferSink!.ReceiveFrame(filteredFrame);
            if (res.IsTryAgain || res == AVResult32.EndOfFile)
                return;
            res.ThrowIfError();
            if (output.Type == MediaType.Video && filteredFrame.Duration <= 0)
            {
                // Like ffmpeg: filters do not always keep frame durations; the encoder turns them
                // into packet durations, which the muxer needs for the last frame.
                Rational frameRate = output.BufferSink.FrameRate;
                if (frameRate.IsValidTimeBase && filteredFrame.TimeBase.IsValidTimeBase)
                    filteredFrame.Duration = Rational.Rescale(1, new Rational(frameRate.Denominator, frameRate.Numerator), filteredFrame.TimeBase);
            }
            if (output.Index == PreviewOutputStreamIndex && PreviewDue)
                SetPreview(filteredFrame);
            // Like ffmpeg: the decoder's frame type is not a request for a key frame. Encoders such as libx264 and
            // libsvtav1 turn every frame marked as I-frame into a key frame, which would copy the source's GOP.
            filteredFrame.PictureType = PictureType.None;
            Encode(output, filteredFrame);
        }
    }

    private void Encode(Output output, AVFrame? frame)
    {
        CodecContext encoder = output.Encoder!;
        if (!encoder.IsOpen)
            return; // never initialized (only possible when nothing was ever filtered)
        AVResult32 res = encoder.SendFrame(frame);
        if (res == AVResult32.EndOfFile && frame == null)
            return;
        res.ThrowIfError();
        while (true)
        {
            res = encoder.ReceivePacket(encodedPacket);
            if (res.IsTryAgain || res == AVResult32.EndOfFile)
                return;
            res.ThrowIfError();
            // Encoders with delay (e.g. MPEG-4) leave the duration unset; without it the last frame
            // has no length in the output. Use one frame, as the frame rate defines it.
            if (output.Type == MediaType.Video && encodedPacket.Duration <= 0
                && encoder.FrameRate.IsValidTimeBase && encodedPacket.TimeBase.IsValidTimeBase)
                encodedPacket.Duration = Rational.Rescale(1, new Rational(encoder.FrameRate.Denominator, encoder.FrameRate.Numerator), encodedPacket.TimeBase);
            WritePacket(output, encodedPacket);
        }
    }

    private void BuildGraph(Output output, SourceFormat format)
    {
        bool rebuild = output.SourceFormat != null;
        output.DisposeGraph();
        bool video = output.Type == MediaType.Video;
        // A configured graph cannot be flushed or reset, so every build starts from a fresh graph
        // that contains the user part (parsed description, copy of the template or factory result).
        FilterGraphContext context = new(Source.Streams[output.InputIndex], output.Index, output.Encoder!, rebuild)
        {
            TimeBase = format.TimeBase,
            Width = format.Width,
            Height = format.Height,
            PixelFormat = format.Video ? format.PixelFormat : PixelFormat.None,
            SampleAspectRatio = format.SampleAspectRatio,
            FrameRate = format.FrameRate,
            SampleFormat = format.Video ? SampleFormat.None : format.SampleFormat,
            SampleRate = format.SampleRate,
            Channels = format.ChannelLayout?.Channels ?? 0,
            ChannelLayout = format.ChannelLayout?.ToString(),
        };
        FilterGraph graph = output.Filter!.CreateGraph(context, out FilterContext userInput, out int userInputPad, out FilterContext userOutput, out int userOutputPad);
        try
        {
            FilterContext source;
            FilterContext sink;
            using (BufferSrcParameters parameters = format.CreateParameters())
            {
                source = video
                    ? VideoBufferSource.Create("transcoder_in", parameters, graph)
                    : AudioBufferSource.Create("transcoder_in", parameters, graph);
            }
            sink = video ? VideoBufferSink.Create("transcoder_out", graph) : AudioBufferSink.Create("transcoder_out", graph);

            FilterContext last = source;
            int lastPad = 0;
            int filterNumber = 0;
            void Append(string filterName, string args)
            {
                FilterContext next = FilterContext.Create($"transcoder_{filterName}_{filterNumber++}", Filter.GetFilterByName(filterName), args, graph);
                graph.Link(last, lastPad, next, 0).ThrowIfError();
                last = next;
                lastPad = 0;
            }

            // Like ffmpeg: the start is trimmed at the input (-ss before -i), so user filters only see
            // frames from the start on; the end is trimmed at the output (-t), after filters such as fps.
            if (Start > TimeSpan.Zero)
                Append(video ? "trim" : "atrim", $"start={FormatSeconds(originUs)}");

            graph.Link(last, lastPad, userInput, userInputPad).ThrowIfError();
            last = userOutput;
            lastPad = userOutputPad;

            foreach ((string name, string args) in GetEncoderConstraints(output))
                Append(name, args);

            if (endUs != long.MaxValue)
                Append(video ? "trim" : "atrim", $"end={FormatSeconds(endUs)}");

            graph.Link(last, lastPad, sink, 0).ThrowIfError();
            graph.Config().ThrowIfError();

            output.Graph = graph;
            output.BufferSource = source;
            output.BufferSink = sink;
            output.SourceFormat = format;
            graph = null!;
        }
        finally
        {
            graph?.Dispose();
        }

        if (!output.Encoder!.IsOpen)
            OpenEncoder(output);

        CodecContext encoder = output.Encoder;
        if (!video && encoder.FrameSize > 0 && !encoder.Codec.Capabilities.HasFlag(CodecCapabilities.VariableFrameSize))
            ((AudioBufferSink)output.BufferSink).SetFrameSize(encoder.FrameSize);
    }

    // Seconds with microsecond precision, without going through a double.
    private static string FormatSeconds(long microseconds)
        => string.Format(CultureInfo.InvariantCulture, "{0}.{1:D6}", microseconds / 1_000_000, Math.Abs(microseconds % 1_000_000));

    private static IEnumerable<(string Filter, string Args)> GetEncoderConstraints(Output output)
    {
        CodecContext encoder = output.Encoder!;
        Codec codec = encoder.Codec;
        // When rebuilding after a format change the encoder is open, so these enforce exactly what it was opened with.

        if (output.Type == MediaType.Video)
        {
            if (encoder.Width > 0 || encoder.Height > 0)
            {
                string w = encoder.Width > 0 ? encoder.Width.ToString(CultureInfo.InvariantCulture) : "-2";
                string h = encoder.Height > 0 ? encoder.Height.ToString(CultureInfo.InvariantCulture) : "-2";
                yield return ("scale", $"w={w}:h={h}");
            }
            if (output.UserFrameRate)
                yield return ("fps", $"fps={encoder.FrameRate.Numerator}/{encoder.FrameRate.Denominator}");

            IEnumerable<PixelFormat> formats = encoder.PixelFormat != PixelFormat.None
                ? [encoder.PixelFormat]
                : codec.SupportedPixelFormats.ToArray();
            string list = string.Join("|", formats.Select(f => f.ToFFmpegString()));
            if (list.Length > 0)
                yield return ("format", $"pix_fmts={list}");
        }
        else
        {
            List<string> args = [];
            string sampleFormats = encoder.SampleFormat != SampleFormat.None
                ? encoder.SampleFormat.ToFFmpegString()
                : string.Join("|", codec.SupportedSampleFormats.ToArray().Select(f => f.ToFFmpegString()));
            if (sampleFormats.Length > 0)
                args.Add($"sample_fmts={sampleFormats}");

            string sampleRates = encoder.SampleRate > 0
                ? encoder.SampleRate.ToString(CultureInfo.InvariantCulture)
                : string.Join("|", codec.SupportedSampleRates.ToArray().Select(r => r.ToString(CultureInfo.InvariantCulture)));
            if (sampleRates.Length > 0)
                args.Add($"sample_rates={sampleRates}");

            string layouts;
            if (encoder.ChannelLayout.Channels > 0)
            {
                using ChannelLayout layout = encoder.ChannelLayout.GetReferencedObject()!;
                layouts = layout.ToString();
            }
            else
            {
                layouts = string.Join("|", codec.ChannelLayouts.Select(l => l.ToString()));
            }
            if (layouts.Length > 0)
                args.Add($"channel_layouts={layouts}");

            if (args.Count > 0)
                yield return ("aformat", string.Join(":", args));
        }
    }

    private void OpenEncoder(Output output)
    {
        CodecContext encoder = output.Encoder!;
        IBufferSink sink = output.BufferSink!;

        if (output.Type == MediaType.Video)
        {
            if (encoder.Width <= 0)
                encoder.Width = sink.Width;
            if (encoder.Height <= 0)
                encoder.Height = sink.Height;
            if (encoder.PixelFormat == PixelFormat.None)
                encoder.PixelFormat = sink.PixelFormat;
            if (encoder.SampleAspectRatio.Numerator == 0)
                encoder.SampleAspectRatio = sink.PixelAspectRatio;
            if (!encoder.FrameRate.IsValidTimeBase && sink.FrameRate.IsValidTimeBase)
                encoder.FrameRate = sink.FrameRate;
            if (!encoder.TimeBase.IsValidTimeBase)
            {
                // Keep the exact time base of the filtered frames. Some encoders (MPEG-4 part 2, H.263)
                // only accept denominators up to 65535; like ffmpeg, use 1/frame rate for those.
                encoder.TimeBase = sink.TimeBase.Denominator > ushort.MaxValue && encoder.FrameRate.IsValidTimeBase
                    ? new Rational(encoder.FrameRate.Denominator, encoder.FrameRate.Numerator)
                    : sink.TimeBase;
            }
            if (encoder.ColorRange == ColorRange.Unspecified)
                encoder.ColorRange = sink.ColorRange;
            if (encoder.ColorSpace == ColorSpace.Unspecified)
                encoder.ColorSpace = sink.ColorSpace;
            SourceFormat? format = output.SourceFormat;
            if (format != null)
            {
                if (encoder.ColorPrimaries == ColorPrimaries.Unspecified)
                    encoder.ColorPrimaries = format.ColorPrimaries;
                if (encoder.ColorTransferCharacteristic == ColorTransferCharacteristic.Unspecified)
                    encoder.ColorTransferCharacteristic = format.ColorTransfer;
            }
        }
        else
        {
            if (encoder.SampleFormat == SampleFormat.None)
                encoder.SampleFormat = sink.SampleFormat;
            if (encoder.SampleRate <= 0)
                encoder.SampleRate = sink.SampleRate;
            if (encoder.ChannelLayout.Channels <= 0 && sink.TryGetChannelLayout(out ChannelLayout? layout))
            {
                using (layout)
                    encoder.ChannelLayout.SetReferencedObject(layout);
            }
            if (!encoder.TimeBase.IsValidTimeBase)
                encoder.TimeBase = new Rational(1, encoder.SampleRate);
        }

        OpenAndRegister(output);
    }

    private void OpenSubtitleEncoder(Output output)
    {
        CodecContext decoder = Source.CodecContexts[output.InputIndex];
        if (!decoder.IsOpen)
            decoder.Open(null).ThrowIfError();
        CodecContext encoder = output.Encoder!;
        if (!encoder.TimeBase.IsValidTimeBase)
            encoder.TimeBase = Rational.TIME_BASE;
        encoder.SubtitleHeader ??= decoder.SubtitleHeader;
        if (encoder.Width <= 0 && encoder.Height <= 0)
        {
            encoder.Width = decoder.Width;
            encoder.Height = decoder.Height;
        }
        OpenAndRegister(output);
    }

    private void OpenAndRegister(Output output)
    {
        CodecContext encoder = output.Encoder!;
        if (Sink.FormatContext.OutputFormat.GetValueOrDefault().Flags.HasFlag(FormatFlags.GlobalHeader))
            encoder.Flags |= CodecFlags.GlobalHeader;

        AVResult32 res = encoder.Open(null, output.EncoderOptions);
        if (res.IsError)
            throw new FFmpegException(res, $"Could not open the encoder {encoder.Codec.Name} for output stream #{output.Index}.");
        if (output.EncoderOptions?.Count > 0)
            Logging.Logger.Warning($"[{nameof(Transcoder)}] Unused encoder options for output stream #{output.Index}: {string.Join(", ", output.EncoderOptions.Keys)}");

        // Copies the parameters (including the extradata created by avcodec_open2) to the stream
        // and transfers ownership of the encoder to the sink.
        Sink.SetCodecContext(encoder, output.Index);
        output.Registered = true;
        TryWriteHeader();
    }

    #endregion

    #region Muxing

    private void WritePacket(Output output, AVPacket packet, bool shift = true)
    {
        packet.StreamIndex = output.Index;
        if (!packet.TimeBase.IsValidTimeBase)
            packet.TimeBase = Source.Streams[output.InputIndex].TimeBase;
        if (shift)
        {
            long offset = Rational.Rescale(originUs, Rational.TIME_BASE, packet.TimeBase);
            if (packet.PresentationTimestamp != ffmpeg.AV_NOPTS_VALUE)
                packet.PresentationTimestamp -= offset;
            if (packet.DecompressionTimestamp != ffmpeg.AV_NOPTS_VALUE)
                packet.DecompressionTimestamp -= offset;
        }

        if (packet.PresentationTimestamp != ffmpeg.AV_NOPTS_VALUE)
        {
            long end = packet.PresentationTimestamp + Math.Max(packet.Duration, 0);
            long ticks = Rational.Rescale(end, packet.TimeBase, new Rational(1, (int)TimeSpan.TicksPerSecond));
            if (ticks > Interlocked.Read(ref positionTicks))
                _ = Interlocked.Exchange(ref positionTicks, ticks);
        }

        if (!headerWritten)
        {
            queuedPackets.Add(packet.Clone());
            packet.Unreference();
            if (queuedPackets.Count > MaxQueuedPackets)
                InitializeRemainingOutputs();
            return;
        }
        WriteToSink(packet);
    }

    private void WriteToSink(AVPacket packet)
    {
        Output output = outputs.Find(o => o.Index == packet.StreamIndex)!;
        packet.RescaleTS(Sink.Streams[packet.StreamIndex].TimeBase);

        // Like ffmpeg's muxing stage: inputs can contain non-monotonic DTS (e.g. at splices), which
        // muxers reject. Bump the DTS (and the PTS if needed) instead of failing.
        FormatFlags flags = Sink.FormatContext.OutputFormat.GetValueOrDefault().Flags;
        if (!flags.HasFlag(FormatFlags.NoTimestamps) && packet.DecompressionTimestamp != ffmpeg.AV_NOPTS_VALUE)
        {
            if (output.LastDts != ffmpeg.AV_NOPTS_VALUE)
            {
                long min = output.LastDts + (flags.HasFlag(FormatFlags.TsNonStrict) ? 0 : 1);
                if (packet.DecompressionTimestamp < min)
                {
                    Logging.Logger.Warning($"[{nameof(Transcoder)}] Non-monotonic DTS in output stream #{output.Index}: {packet.DecompressionTimestamp} after {output.LastDts}, changed to {min}.");
                    if (packet.PresentationTimestamp != ffmpeg.AV_NOPTS_VALUE && packet.PresentationTimestamp < min)
                        packet.PresentationTimestamp = min;
                    packet.DecompressionTimestamp = min;
                }
            }
            output.LastDts = packet.DecompressionTimestamp;
        }
        Sink.WritePacket(packet).ThrowIfError();
    }

    private void InitializeRemainingOutputs()
    {
        foreach (Output output in outputs.Where(o => o.Encoder != null && !o.Encoder.IsOpen && o.Type is MediaType.Audio or MediaType.Video))
        {
            Logging.Logger.Warning($"[{nameof(Transcoder)}] No frame for output stream #{output.Index} after {MaxQueuedPackets} packets; initializing it from the decoder parameters.");
            BuildGraph(output, SourceFormat.FromDecoder(Source.CodecContexts[output.InputIndex], Source.Streams[output.InputIndex]));
        }
    }

    private void TryWriteHeader()
    {
        if (headerWritten || state == State.Created || outputs.Exists(o => o.Encoder != null && !o.Registered))
            return;

        if (CopyChapters)
            CopyChaptersToSink();

        Sink.WriteHeader().ThrowIfError();
        headerWritten = true;
        foreach (AVPacket packet in queuedPackets)
        {
            using (packet)
                WriteToSink(packet);
        }
        queuedPackets.Clear();
    }

    private void CopyChaptersToSink()
    {
        ChapterList target = Sink.FormatContext.Chapters;
        foreach (AVChapter chapter in Source.FormatContext.Chapters)
        {
            long offset = Rational.Rescale(originUs, Rational.TIME_BASE, chapter.TimeBase);
            long end = endUs == long.MaxValue ? long.MaxValue : Rational.Rescale(endUs, Rational.TIME_BASE, chapter.TimeBase);
            if (chapter.End <= offset || chapter.Start >= end)
                continue;
            long start = Math.Max(chapter.Start, offset) - offset;
            long stop = Math.Min(chapter.End, end) - offset;
            target.AddChapter(chapter.TimeBase, start, stop, chapter.Title ?? string.Empty);
        }
    }

    private static long ToMicroseconds(long timestamp, Rational timeBase) => Rational.Rescale(timestamp, timeBase, Rational.TIME_BASE);

    #endregion

    #region Dispose

    private bool disposed;

    private void CheckDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(Transcoder));
    }

    /// <summary>
    /// Releases the transcoder, the source and the sink. If the output header was written but the
    /// transcoding did not finish, the sink still writes the trailer.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        foreach (Output output in outputs)
            output.Dispose();
        foreach (AVPacket packet in queuedPackets)
            packet.Dispose();
        queuedPackets.Clear();
        Sink.Dispose();
        Source.Dispose();
        readPacket.Dispose();
        encodedPacket.Dispose();
        decodedFrame.Dispose();
        filteredFrame.Dispose();
        subtitle.Dispose();
        lock (previewLock)
        {
            previewFrame.Dispose();
            previewSet = false;
        }
    }

    #endregion

    #region Nested types

    private sealed class Input(int index, MediaType type)
    {
        public int Index { get; } = index;
        public MediaType Type { get; } = type;
        public List<Output> Outputs { get; } = [];
        public bool Decoding { get; set; }
        public bool Finished { get; set; }
    }

    private sealed class Output(int index, int inputIndex, MediaType type, CodecContext? encoder, StreamFilter? filter, Dictionary<string, string>? encoderOptions) : IDisposable
    {
        public int Index { get; } = index;
        public int InputIndex { get; } = inputIndex;
        public MediaType Type { get; } = type;
        public CodecContext? Encoder { get; } = encoder;
        public StreamFilter? Filter { get; } = filter;
        public Dictionary<string, string>? EncoderOptions { get; } = encoderOptions;
        public bool UserFrameRate { get; init; }

        /// <summary>The encoder was handed to the sink, which disposes it.</summary>
        public bool Registered { get; set; }
        public bool CopyStarted { get; set; }
        public long LastDts { get; set; } = ffmpeg.AV_NOPTS_VALUE;

        /// <summary>The buffer source reported end of file: the graph takes no more input.</summary>
        public bool GraphEnded { get; set; }

        public FilterGraph? Graph { get; set; }
        public IBufferSource? BufferSource { get; set; }
        public IBufferSink? BufferSink { get; set; }
        public SourceFormat? SourceFormat { get; set; }

        public void DisposeGraph()
        {
            Graph?.Dispose();
            Graph = null;
            BufferSource = null;
            BufferSink = null;
            SourceFormat?.Dispose();
            SourceFormat = null;
        }

        public void Dispose()
        {
            DisposeGraph();
            Filter?.Dispose();
            if (!Registered)
                Encoder?.Dispose();
        }
    }

    /// <summary>The user part of an output's filter graph; creates a new graph for every build.</summary>
    private abstract class StreamFilter : IDisposable
    {
        /// <summary>Creates a new graph with the user filters and returns their unlinked input and output pad.</summary>
        public abstract FilterGraph CreateGraph(FilterGraphContext context, out FilterContext input, out int inputPad, out FilterContext output, out int outputPad);

        public virtual void Dispose() { }

        protected static FilterGraph Parse(string? description, MediaType type, out FilterContext input, out int inputPad, out FilterContext output, out int outputPad)
        {
            description ??= type == MediaType.Video ? "null" : "anull";
            FilterGraph graph = FilterGraph.Allocate();
            try
            {
                graph.Parse(out FilterInOutList inputs, description, out FilterInOutList outputs).ThrowIfError();
                using (inputs)
                using (outputs)
                    GetPads(inputs, outputs, $"'{description}'", out input, out inputPad, out output, out outputPad);
                return graph;
            }
            catch
            {
                graph.Dispose();
                throw;
            }
        }

        protected static void GetPads(FilterInOutList inputs, FilterInOutList outputs, string what, out FilterContext input, out int inputPad, out FilterContext output, out int outputPad)
        {
            if (inputs == null || outputs == null || inputs.Count != 1 || outputs.Count != 1 || inputs[0].Filter == null || outputs[0].Filter == null)
                throw new ArgumentException($"The filter graph {what} must have exactly one unlinked input and one unlinked output.");
            input = inputs[0].Filter!;
            inputPad = inputs[0].PadIdx;
            output = outputs[0].Filter!;
            outputPad = outputs[0].PadIdx;
        }
    }

    /// <summary>A filter description, parsed for every build.</summary>
    private sealed class TextFilter(string? description) : StreamFilter
    {
        public override FilterGraph CreateGraph(FilterGraphContext context, out FilterContext input, out int inputPad, out FilterContext output, out int outputPad)
            => Parse(description, context.MediaType, out input, out inputPad, out output, out outputPad);
    }

    /// <summary>A filter description created for every build.</summary>
    private sealed class TextFactoryFilter(Func<FilterGraphContext, string> factory) : StreamFilter
    {
        public override FilterGraph CreateGraph(FilterGraphContext context, out FilterContext input, out int inputPad, out FilterContext output, out int outputPad)
        {
            string description = factory(context);
            return Parse(string.IsNullOrWhiteSpace(description) ? null : description, context.MediaType, out input, out inputPad, out output, out outputPad);
        }
    }

    /// <summary>A filter graph object created for every build.</summary>
    private sealed class GraphFactoryFilter(FilterGraphFactory factory) : StreamFilter
    {
        public override FilterGraph CreateGraph(FilterGraphContext context, out FilterContext input, out int inputPad, out FilterContext output, out int outputPad)
        {
            (FilterInOutList inputs, FilterGraph graph, FilterInOutList outputs) = factory(context);
            if (graph == null)
            {
                inputs?.Dispose();
                outputs?.Dispose();
                throw new InvalidOperationException($"The filter graph factory of output stream #{context.OutputStreamIndex} returned no graph.");
            }
            try
            {
                GetPads(inputs, outputs, $"of output stream #{context.OutputStreamIndex}", out input, out inputPad, out output, out outputPad);
                return graph;
            }
            catch
            {
                graph.Dispose();
                throw;
            }
            finally
            {
                inputs?.Dispose();
                outputs?.Dispose();
            }
        }
    }

    /// <summary>An unconfigured filter graph, copied for every build.</summary>
    private sealed class TemplateFilter : StreamFilter
    {
        private readonly FilterGraph graph;
        private readonly string inputFilter, outputFilter;
        private readonly int inputPad, outputPad;

        private TemplateFilter(FilterGraph graph, string inputFilter, int inputPad, string outputFilter, int outputPad)
        {
            this.graph = graph;
            this.inputFilter = inputFilter;
            this.inputPad = inputPad;
            this.outputFilter = outputFilter;
            this.outputPad = outputPad;
        }

        /// <summary>Takes ownership of <paramref name="graph"/>; disposes it if the arguments are invalid.</summary>
        public static TemplateFilter Create(FilterInOutList inputs, FilterGraph graph, FilterInOutList outputs)
        {
            try
            {
                if (graph == null)
                    throw new ArgumentNullException(nameof(graph));
                GetPads(inputs, outputs, "", out FilterContext input, out int inputPad, out FilterContext output, out int outputPad);
                TemplateFilter template = new(graph, input.Name, inputPad, output.Name, outputPad);
                // Fail now rather than at the first frame if the graph cannot be copied.
                using (FilterGraph copy = graph.Copy())
                {
                    if (copy.FindFilter(template.inputFilter) == null || copy.FindFilter(template.outputFilter) == null)
                        throw new ArgumentException("The unlinked input and output must be filters of the graph.");
                }
                return template;
            }
            catch
            {
                graph?.Dispose();
                throw;
            }
        }

        public override FilterGraph CreateGraph(FilterGraphContext context, out FilterContext input, out int inputPad, out FilterContext output, out int outputPad)
        {
            FilterGraph copy = graph.Copy();
            input = copy.FindFilter(inputFilter)!;
            inputPad = this.inputPad;
            output = copy.FindFilter(outputFilter)!;
            outputPad = this.outputPad;
            return copy;
        }

        public override void Dispose() => graph.Dispose();
    }

    /// <summary>The format a filter graph was configured for.</summary>
    private sealed class SourceFormat : IDisposable
    {
        public bool Video { get; private init; }
        public int Width { get; private init; }
        public int Height { get; private init; }
        public PixelFormat PixelFormat { get; private init; }
        public Rational SampleAspectRatio { get; private init; }
        public ColorSpace ColorSpace { get; private init; }
        public ColorRange ColorRange { get; private init; }
        public ColorPrimaries ColorPrimaries { get; private init; }
        public ColorTransferCharacteristic ColorTransfer { get; private init; }
        public Rational FrameRate { get; private init; }
        public SampleFormat SampleFormat { get; private init; }
        public int SampleRate { get; private init; }
        public ChannelLayout? ChannelLayout { get; private init; }
        public Rational TimeBase { get; private init; }

        public static SourceFormat FromFrame(AVFrame frame, CodecContext decoder) => frame.IsVideo
            ? new()
            {
                Video = true,
                Width = frame.Width,
                Height = frame.Height,
                PixelFormat = frame.PixelFormat,
                SampleAspectRatio = frame.PixelAspectRatio,
                ColorSpace = frame.ColorSpace,
                ColorRange = frame.ColorRange,
                ColorPrimaries = frame.ColorPrimaries,
                ColorTransfer = frame.ColorTransferCharacteristic,
                FrameRate = decoder.FrameRate,
                TimeBase = frame.TimeBase,
            }
            : new()
            {
                SampleFormat = frame.SampleFormat,
                SampleRate = frame.SampleRate,
                ChannelLayout = frame.ChannelLayout.GetReferencedObject(),
                TimeBase = frame.TimeBase,
            };

        public static SourceFormat FromDecoder(CodecContext decoder, AVStream stream)
        {
            if (stream.MediaType == MediaType.Video)
            {
                PixelFormat format = decoder.SoftwarePixelFormat != PixelFormat.None ? decoder.SoftwarePixelFormat : decoder.PixelFormat;
                return decoder.Width <= 0 || decoder.Height <= 0 || format == PixelFormat.None
                    ? throw new InvalidOperationException($"Input stream #{stream.Index} produced no frame and its decoder parameters are incomplete, so its encoder cannot be initialized.")
                    : new()
                    {
                        Video = true,
                        Width = decoder.Width,
                        Height = decoder.Height,
                        PixelFormat = format,
                        SampleAspectRatio = decoder.SampleAspectRatio,
                        ColorSpace = decoder.ColorSpace,
                        ColorRange = decoder.ColorRange,
                        ColorPrimaries = decoder.ColorPrimaries,
                        ColorTransfer = decoder.ColorTransferCharacteristic,
                        FrameRate = decoder.FrameRate,
                        TimeBase = stream.TimeBase,
                    };
            }
            return decoder.SampleRate <= 0 || decoder.SampleFormat == SampleFormat.None || decoder.ChannelLayout.Channels <= 0
                ? throw new InvalidOperationException($"Input stream #{stream.Index} produced no frame and its decoder parameters are incomplete, so its encoder cannot be initialized.")
                : new()
                {
                    SampleFormat = decoder.SampleFormat,
                    SampleRate = decoder.SampleRate,
                    ChannelLayout = decoder.ChannelLayout.GetReferencedObject(),
                    TimeBase = stream.TimeBase,
                };
        }

        public bool Matches(AVFrame frame) => Video
            ? frame.Width == Width && frame.Height == Height && frame.PixelFormat == PixelFormat
            : frame.SampleRate == SampleRate && frame.SampleFormat == SampleFormat && ChannelLayout != null && ChannelLayout.Equals(frame.ChannelLayout);

        public BufferSrcParameters CreateParameters()
        {
            BufferSrcParameters parameters = BufferSrcParameters.Allocate();
            parameters.TimeBase = TimeBase;
            if (Video)
            {
                parameters.Width = Width;
                parameters.Height = Height;
                parameters.PixelFormat = PixelFormat;
                parameters.SampleAspectRatio = SampleAspectRatio;
                parameters.ColorSpace = ColorSpace;
                parameters.ColorRange = ColorRange;
                if (FrameRate.IsValidTimeBase)
                    parameters.FrameRate = FrameRate;
            }
            else
            {
                parameters.SampleFormat = SampleFormat;
                parameters.SampleRate = SampleRate;
                parameters.ChannelLayout.SetReferencedObject(ChannelLayout);
            }
            return parameters;
        }

        public void Dispose() => ChannelLayout?.Dispose();
    }

    #endregion
}

/// <summary>
/// The progress of a <see cref="Transcoder"/>.
/// </summary>
/// <param name="position">The output time written so far.</param>
/// <param name="totalDuration">The expected output duration, or <see cref="TimeSpan.Zero"/> if unknown.</param>
public readonly struct TranscodingProgress(TimeSpan position, TimeSpan totalDuration)
{
    /// <summary>Gets the output time written so far.</summary>
    public TimeSpan Position { get; } = position;

    /// <summary>Gets the expected output duration, or <see cref="TimeSpan.Zero"/> if unknown.</summary>
    public TimeSpan TotalDuration { get; } = totalDuration;

    /// <summary>Gets the progress in percent (0–100), or <see langword="null"/> if the duration is unknown.</summary>
    public double? Percent => TotalDuration > TimeSpan.Zero
        ? Math.Clamp(Position.TotalMilliseconds / TotalDuration.TotalMilliseconds * 100, 0, 100)
        : null;

    /// <inheritdoc/>
    public override string ToString() => Percent is double p ? $"{Position} / {TotalDuration} ({p:F1} %)" : Position.ToString();
}

/// <summary>
/// Creates a new, unconfigured filter graph with exactly one unlinked input and one unlinked output,
/// for a <see cref="Transcoder"/> output stream or a <see cref="MediaPlayer.PlaybackEngine"/> stream.
/// </summary>
/// <param name="context">The stream and the format of the frames that will enter the graph.</param>
/// <returns>The unlinked input, the graph and the unlinked output; the transcoder takes ownership of all three.</returns>
/// <example>
/// <code>
/// transcoder.TranscodeStream(0, encoder, ctx =&gt;
/// {
///     FilterGraph graph = FilterGraph.Create(out FilterInOutList? inputs, $"scale={ctx.Width / 2}:-2", out FilterInOutList? outputs);
///     return (inputs!, graph, outputs!);
/// });
/// </code>
/// </example>
public delegate (FilterInOutList Inputs, FilterGraph Graph, FilterInOutList Outputs) FilterGraphFactory(FilterGraphContext context);

/// <summary>
/// Describes the filter graph a <see cref="Transcoder"/> or <see cref="MediaPlayer.PlaybackEngine"/> is
/// about to build: the input stream and the format of the decoded frames that will enter it.
/// </summary>
public sealed class FilterGraphContext
{
    internal FilterGraphContext(AVStream inputStream, int outputStreamIndex, CodecContext? encoder, bool isRebuild)
    {
        InputStream = inputStream;
        OutputStreamIndex = outputStreamIndex;
        Encoder = encoder;
        IsRebuild = isRebuild;
        MediaType = inputStream.MediaType;
    }

    /// <summary>Gets the input stream.</summary>
    public AVStream InputStream { get; }

    /// <summary>Gets the index of the <see cref="Transcoder"/> output stream the graph feeds, or -1 for a <see cref="MediaPlayer.PlaybackEngine"/>.</summary>
    public int OutputStreamIndex { get; }

    /// <summary>Gets the media type (audio or video).</summary>
    public MediaType MediaType { get; }

    /// <summary>
    /// Gets the encoder of the <see cref="Transcoder"/> output stream, or <see langword="null"/> for a
    /// <see cref="MediaPlayer.PlaybackEngine"/>. It is open when <see cref="IsRebuild"/> is <see langword="true"/>;
    /// the transcoder then converts the graph output to the format it was opened with.
    /// </summary>
    public CodecContext? Encoder { get; }

    /// <summary>
    /// Gets whether the graph is built again rather than for the first time: by a <see cref="Transcoder"/>
    /// because the decoded format changed mid-stream, by a <see cref="MediaPlayer.PlaybackEngine"/> after a seek.
    /// </summary>
    public bool IsRebuild { get; }

    /// <summary>Gets the time base of the frames entering the graph.</summary>
    public Rational TimeBase { get; internal init; }

    /// <summary>Gets the frame width (video).</summary>
    public int Width { get; internal init; }

    /// <summary>Gets the frame height (video).</summary>
    public int Height { get; internal init; }

    /// <summary>Gets the pixel format (video).</summary>
    public PixelFormat PixelFormat { get; internal init; }

    /// <summary>Gets the sample aspect ratio (video).</summary>
    public Rational SampleAspectRatio { get; internal init; }

    /// <summary>Gets the frame rate as known by the decoder (video); may be invalid.</summary>
    public Rational FrameRate { get; internal init; }

    /// <summary>Gets the sample format (audio).</summary>
    public SampleFormat SampleFormat { get; internal init; }

    /// <summary>Gets the sample rate (audio).</summary>
    public int SampleRate { get; internal init; }

    /// <summary>Gets the number of channels (audio).</summary>
    public int Channels { get; internal init; }

    /// <summary>Gets the channel layout description, e.g. <c>"5.1(side)"</c> (audio), usable in filter descriptions.</summary>
    public string? ChannelLayout { get; internal init; }
}
