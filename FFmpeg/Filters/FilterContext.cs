using FFmpeg.Audio;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;
/// <summary>
/// Represents a filter context in FFmpeg, which is used to manage filter instances in a filter graph.
/// </summary>
public unsafe class FilterContext : Options.OptionQueryBase
{
    /// <summary>
    /// Gets a pointer to the filter context, used for option queries.
    /// </summary>
    protected override unsafe void* Pointer => context;

    /// <summary>
    /// The underlying unmanaged FFmpeg filter context.
    /// </summary>
    internal AutoGen._AVFilterContext* context;

    /// <summary>
    /// Gets the filter associated with this context.
    /// </summary>
    public Filter Filter => new(context->filter);

    /// <summary>
    /// Gets the name of the filter context.
    /// </summary>
    public string Name => Marshal.PtrToStringUTF8((nint)context->name);

    /// <summary>
    /// Gets or sets the number of threads used by this filter context. A value of 0 or less indicates no thread is used.
    /// </summary>
    public int Threads { get => context->nb_threads; set => context->nb_threads = value; }

    /// <summary>
    /// Sets the parameters for the buffer source filter.
    /// </summary>
    /// <param name="parameters">The buffer source parameters to set.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 SetBufferSourceParameters(BufferSrcParameters parameters) => ffmpeg.av_buffersrc_parameters_set(context, parameters.parameters);

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterContext"/> class with the given FFmpeg filter context.
    /// </summary>
    /// <param name="context">Pointer to the FFmpeg filter context.</param>
    internal FilterContext(AutoGen._AVFilterContext* context) => this.context = context;
    /// <summary>
    /// Creates a new filter context using the specified filter, name, arguments, and filter graph.
    /// </summary>
    /// <param name="name">The name of the filter.</param>
    /// <param name="filter">The filter to use.</param>
    /// <param name="args">Optional arguments for initializing the filter.</param>
    /// <param name="graph">The filter graph to which this filter belongs.</param>
    /// <returns>A new <see cref="FilterContext"/> instance.</returns>
    public static FilterContext Create(string name, Filter filter, string? args, FilterGraph graph)
    {
        AutoGen._AVFilterContext* context;
        ((AVResult32)ffmpeg.avfilter_graph_create_filter(&context, filter.filter, name, args, null, graph.graph)).ThrowIfError();
        return new(context);
    }

    /// <summary>
    /// Gets or sets the hardware device context used by the filter.
    /// </summary>
    public HW.DeviceContext_ref HwDeviceContext => new(&context->hw_device_ctx, false);

    public int ExtraHWFrames
    {
        get => context->extra_hw_frames;
        set => context->extra_hw_frames = value;
    }

    /// <summary>
    /// Creates a source filter context using the specified parameters.
    /// </summary>
    /// <param name="name">The name of the filter.</param>
    /// <param name="parameters">The buffer source parameters to use.</param>
    /// <param name="graph">The filter graph to which this filter belongs.</param>
    /// <returns>A new <see cref="FilterContext"/> instance.</returns>
    public static FilterContext CreateSource(string name, BufferSrcParameters parameters, FilterGraph graph)
    {
        bool isVideo = !(parameters.Width == 0 && parameters.Height == 0);
        FilterContext? context = Allocate(name, isVideo ? Filter.VideoBufferSource : Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        context.SetBufferSourceParameters(parameters).ThrowIfError();
        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates a source filter context for video using the specified parameters.
    /// </summary>
    public static FilterContext CreateSource(string name, int width, int height, PixelFormat format, Rational timeBase, FilterGraph graph)
    {
        FilterContext? context = Allocate(name, Filter.VideoBufferSource, graph) ?? throw new ArgumentNullException();
        context.SetOption("pix_fmt", (AutoGen._AVPixelFormat)format).ThrowIfError();
        context.SetOption("video_size", width, height).ThrowIfError();
        context.SetOption("time_base", timeBase).ThrowIfError();
        context.Init().ThrowIfError();
        return context;
    }


    public static FilterContext CreateNullSink(string name, bool isVideo, FilterGraph filterGraph)
    {
        FilterContext context = Allocate(name, isVideo ? Filter.VideoNullSink : Filter.AudioNullSink, filterGraph) ?? throw new OutOfMemoryException();
        return context;
    }

    public static FilterContext CreateNullSource(string name, bool isVideo, FilterGraph filterGraph)
    {
        FilterContext context = Allocate(name, isVideo ? Filter.VideoNullSource : Filter.AudioNullSource, filterGraph) ?? throw new OutOfMemoryException();
        return context;
    }

    // ToDo:
    public static FilterContext CreateSink(string name, bool isVideo, FilterGraph graph)
    {
        FilterContext? context = Allocate(name, isVideo ? Filter.VideoBufferSink : Filter.AudioBufferSink, graph) ?? throw new ArgumentNullException();
        context.Init().ThrowIfError();
        return context;
    }
    public static FilterContext CreateSink(string name, MediaType mediaType, FilterGraph graph)
    {
        FilterContext? context = mediaType switch
        {
            MediaType.Video => Allocate(name, Filter.VideoBufferSink, graph) ?? throw new ArgumentNullException(),
            MediaType.Audio => Allocate(name, Filter.AudioBufferSink, graph) ?? throw new ArgumentNullException(),
            _ => throw new NotSupportedException()
        };
        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates a source filter context using the codec context settings.
    /// </summary>
    public static FilterContext CreateSource(string name, Codecs.CodecContext ctx, FilterGraph graph)
    {
        FilterContext? context = Allocate(name, ctx.CodecType == MediaType.Video ? Filter.VideoBufferSource : Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        using BufferSrcParameters @params = BufferSrcParameters.Allocate();

        @params.ChannelLayout.CopyFrom(ctx.ChannelLayout);
        @params.Width = ctx.Width;
        @params.Height = ctx.Height;
        @params.ColorRange = ctx.ColorRange;
        @params.ColorSpace = ctx.ColorSpace;
        @params.FrameRate = ctx.FrameRate;
        if (ctx.CodecType == MediaType.Video)
            @params.PixelFormat = ctx.PixelFormat;
        else
            @params.SampleFormat = ctx.SampleFormat;
        @params.SampleAspectRatio = ctx.SampleAspectRatio;
        //@params.SampleFormat = stream.SampleFormat; already set by PixelFormat
        @params.SampleRate = ctx.SampleRate;
        @params.TimeBase = ctx.TimeBase;

        context.SetBufferSourceParameters(@params).ThrowIfError();

        context.Init().ThrowIfError();
        return context;
    }


    /// <summary>
    /// Creates a source filter context using the stream parameters.
    /// </summary>
    public static FilterContext CreateSource(string name, Formats.AVStream stream, FilterGraph graph)
    {
        FilterContext? context = Allocate(name, stream.MediaType == MediaType.Video ? Filter.VideoBufferSource : Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        using BufferSrcParameters @params = BufferSrcParameters.Allocate();

        @params.ChannelLayout.CopyFrom(stream.CodecParameters.ChannelLayout);
        @params.Width = stream.CodecParameters.Width;
        @params.Height = stream.CodecParameters.Height;
        @params.ColorRange = stream.CodecParameters.ColorRange;
        @params.ColorSpace = stream.CodecParameters.ColorSpace;
        @params.FrameRate = stream.CodecParameters.FrameRate;
        if (stream.MediaType == MediaType.Video)
            @params.PixelFormat = stream.CodecParameters.PixelFormat;
        else
            @params.SampleFormat = stream.CodecParameters.SampleFormat;
        @params.SampleAspectRatio = stream.SampleAspectRatio;
        //@params.SampleFormat = stream.SampleFormat; already set by PixelFormat
        @params.SampleRate = stream.CodecParameters.SampleRate;
        @params.TimeBase = stream.TimeBase;

        context.SetBufferSourceParameters(@params).ThrowIfError();

        context.Init().ThrowIfError();
        return context;
    }


    /// <summary>
    /// Allocates a new filter context.
    /// </summary>
    public static FilterContext? Allocate(string name, Filter filter, FilterGraph graph)
    {
        AutoGen._AVFilterContext* context = ffmpeg.avfilter_graph_alloc_filter(graph.graph, filter.filter, name);
        return context == null ? null : new(context);
    }

    /// <summary>
    /// Creates a scale filter context.
    /// </summary>
    public static FilterContext CreateScaleFilter(string name, int width, int height, FilterGraph graph)
    {
        FilterContext? context = Allocate(name, Filter.Scale, graph) ?? throw new ArgumentNullException();
        context.Init($"{width}:{height}").ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates an FPS filter context.
    /// </summary>
    public static FilterContext CreateFPSFilter(string name, Rational fps, FilterGraph graph)
    {
        FilterContext? context = Allocate(name, Filter.FPS, graph) ?? throw new ArgumentNullException();
        context.Init(fps.ToString()).ThrowIfError();
        return context;
    }

    /// <summary>
    /// Initializes the filter context with the specified arguments.
    /// </summary>
    public AVResult32 Init(string? args) => ffmpeg.avfilter_init_str(context, args);

    /// <summary>
    /// Initializes the filter context without arguments.
    /// </summary>
    public AVResult32 Init() => Init(null as string);

    /// <summary>
    /// Initializes the filter context using a dictionary of options.
    /// </summary>
    public AVResult32 Init(Collections.AVDictionary dictionary)
    {
        if (dictionary == null)
            return Init(null as string);
        AutoGen._AVDictionary* dic = dictionary.dictionary;
        int res = ffmpeg.avfilter_init_dict(context, &dic);
        dictionary.dictionary = dic;
        return res;
    }

    /// <summary>
    /// Sends a frame to the buffer source filter.
    /// </summary>
    public AVResult32 SendFrame(AVFrame? frame, bool keepRef = false)
    {
        AutoGen._AVFrame* f = frame != null ? frame.Frame : null;
        return keepRef ? ffmpeg.av_buffersrc_write_frame(context, f) : ffmpeg.av_buffersrc_add_frame(context, f);
    }

    public AVResult32 Drain() => SendFrame(null);

    /// <summary>
    /// Gets a frame from the buffer sink filter.
    /// </summary>
    public AVResult32 ReceiveFrame(AVFrame frame)
    {
        frame.Unreference();
        int res = ffmpeg.av_buffersink_get_frame(context, frame.Frame);
        frame.TimeBase = ffmpeg.av_buffersink_get_time_base(context);
        frame.BestEffortTimestamp = frame.PresentationTimestamp;
        return res;
    }

    /// <summary>
    /// Returns a string representation of the filter context, including the filter and name.
    /// </summary>
    public override string ToString() => context->filter != null
        ? $"{Marshal.PtrToStringUTF8((nint)context->name)}({Marshal.PtrToStringUTF8((nint)context->filter->name)})"
        : $"{Marshal.PtrToStringUTF8((nint)context->name)}(_unset_)";

    // ToDo
    public SampleFormat BufferSinkSampleFormat() => (SampleFormat)ffmpeg.av_buffersink_get_format(context);

    public PixelFormat BufferSinkPixelFormat() => (PixelFormat)ffmpeg.av_buffersink_get_format(context);
    public MediaType BufferSinkType() => (MediaType)ffmpeg.av_buffersink_get_type(context);

    public Rational BufferSinkTimeBase() => ffmpeg.av_buffersink_get_time_base(context);

    public Rational BufferSinkFrameRate() => ffmpeg.av_buffersink_get_frame_rate(context);

    public int BufferSinkWidth() => ffmpeg.av_buffersink_get_w(context);

    public int BufferSinkHeight() => ffmpeg.av_buffersink_get_h(context);

    public Rational BufferSinkSampleAspectRatio() => ffmpeg.av_buffersink_get_sample_aspect_ratio(context);

    public ColorSpace BufferSinkColorSpace() => (ColorSpace)ffmpeg.av_buffersink_get_colorspace(context);
    public ColorRange BufferSinkColorRange() => (ColorRange)ffmpeg.av_buffersink_get_color_range(context);

    public int BufferSinkChannels() => ffmpeg.av_buffersink_get_channels(context);

    public AVResult32 BufferSinkChannelLayout(out Audio.ChannelLayout layout)
    {
        AutoGen._AVChannelLayout l;
        var res = ffmpeg.av_buffersink_get_ch_layout(context, &l);
        layout = new(l);
        return res;
    }

    public int BufferSinkSampleRate() => ffmpeg.av_buffersink_get_sample_rate(context);

    public uint InputCount => context->nb_inputs;
    public uint OutputCount => context->nb_outputs;

    public FilterPad GetInputFilterPad(int index) => index < 0 || index >= InputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter pads.")
            : new FilterPad(context->input_pads, index);

    public FilterPad GetOutputFilterPad(int index) => index < 0 || index >= OutputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter pads.")
            : new FilterPad(context->output_pads, index);

    public IEnumerable<FilterPad> InputFilterPads
    {
        get
        {
            for (int i = 0; i < InputCount; i++)
                yield return GetInputFilterPad(i);
        }
    }

    public IEnumerable<FilterPad> OutputFilterPads
    {
        get
        {
            for (int i = 0; i < OutputCount; i++)
                yield return GetOutputFilterPad(i);
        }
    }

    public IEnumerable<FilterLink> InputFilterLinks
    {
        get
        {
            for (int i = 0; i < InputCount; i++)
            {
                FilterLink? link = GetInputFilterLink(i);
                if (link != null)
                    yield return link;
            }
        }
    }

    public IEnumerable<FilterLink> OutputFilterLinks
    {
        get
        {
            for (int i = 0; i < OutputCount; i++)
            {
                FilterLink? link = GetOutputFilterLink(i);
                if (link != null)
                    yield return link;
            }
        }
    }

    public FilterLink? GetInputFilterLink(int index) => index < 0 || index >= InputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter links.")
            : context->inputs[index] != null ? new FilterLink(context->inputs[index]) : null;

    public FilterLink? GetOutputFilterLink(int index) => index < 0 || index >= OutputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter links.")
            : context->outputs[index] != null ? new FilterLink(context->outputs[index]) : null;

}

