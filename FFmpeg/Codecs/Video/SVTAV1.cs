using FFmpeg.Codecs.Video.LibSVTAV1;
using FFmpeg.Formats;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Diagnostics.CodeAnalysis;

namespace FFmpeg.Codecs.Video;

/// <summary>
/// Settings for the SVT-AV1 encoder (<c>libsvtav1</c>).
/// </summary>
/// <remarks>
/// <para>
/// The SVT-AV1 specific settings are passed in <c>-svtav1-params</c>, which SVT-AV1 parses after FFmpeg's own
/// options, so they take precedence. SVT-AV1 reports parameters it does not know as a warning in the log.
/// SVT-AV1 needs frames of at least 64×64 pixels.
/// </para>
/// <para>
/// SVT-AV1 takes its frame rate from <see cref="VideoCodec.Framerate"/> or, if that is not set, from the time base
/// (1/time base), so a time base like 1/90000 fails without a frame rate.
/// </para>
/// </remarks>
public class SVTAV1 : VideoCodec
{
    private const string EncoderName = "libsvtav1";

    /// <summary>
    /// Initializes the settings with a size, time base and frame rate.
    /// </summary>
    /// <param name="width">The width of the video.</param>
    /// <param name="height">The height of the video.</param>
    /// <param name="timeBase">The time base for the video.</param>
    /// <param name="framerate">The frame rate for the video.</param>
    /// <exception cref="NotSupportedException">Thrown when the SVT-AV1 encoder is not found.</exception>
    public SVTAV1(int width, int height, Rational timeBase, Rational framerate) : base(FindEncoder(EncoderName), width, height, timeBase, framerate)
    {
    }

    /// <summary>
    /// Initializes the settings with a size and time base. If the time base is too large opening the codec will fail
    /// without specifying the frame rate.
    /// </summary>
    /// <param name="width">The width of the video.</param>
    /// <param name="height">The height of the video.</param>
    /// <param name="timeBase">The time base for the video.</param>
    /// <exception cref="NotSupportedException">Thrown when the SVT-AV1 encoder is not found.</exception>
    public SVTAV1(int width, int height, Rational timeBase) : this(width, height, timeBase, Rational.Zero)
    {
    }

    /// <summary>
    /// Initializes the settings without a size, time base or frame rate, e.g. for the <see cref="Transcoder"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when the SVT-AV1 encoder is not found.</exception>
    public SVTAV1() : base(FindEncoder(EncoderName))
    {
    }

    /// <summary>
    /// Gets a value indicating whether the SVT-AV1 encoder is supported.
    /// </summary>
    public static bool IsSupported => Codec.FindEncoder(EncoderName) != null;

    /// <summary>
    /// Gets or sets the profile to be used for encoding.
    /// </summary>
    public Profile? Profile { get; set; }

    /// <summary>
    /// Gets or sets the encoding preset: -1 (slowest, research) to 13 (fastest). Lower presets give smaller files.
    /// </summary>
    public int? Preset { get; set; }

    /// <summary>
    /// Gets or sets the bitrate control settings.
    /// </summary>
    public BitrateControl? BitrateControl { get; set; }

    /// <summary>
    /// Gets or sets the Constant Rate Factor (CRF): 0 to 70, lower is better quality. Setting it is equivalent to
    /// <c>--rc 0 --aq-mode 2 --qp x</c>.
    /// </summary>
    /// <remarks>For quarter steps (SVT-AV1 2.2 and later) use <see cref="Quality"/>.</remarks>
    [DisallowNull]
    public int? CRF
    {
        get => BitrateControl?.CRF;
        set
        {
            BitrateControl bitrateControl = BitrateControl ?? new BitrateControl();
            bitrateControl.CRF = value;
            BitrateControl = bitrateControl;
        }
    }

    /// <summary>
    /// Gets or sets the constant rate factor as SVT-AV1's <c>crf</c> parameter: 0 to 70 in quarter steps (e.g. 27.25),
    /// lower is better quality. It selects CRF rate control with adaptive quantization (aq-mode 2).
    /// </summary>
    /// <remarks>Applied after <see cref="BitrateControl"/>, so it overrides its rate control mode.</remarks>
    public double? Quality { get; set; }

    /// <summary>
    /// Specifies the metric to tune for.
    /// </summary>
    public TuningMetric? Tune { get; set; }

    /// <summary>
    /// Gets or sets the GOP (Group of Pictures) settings.
    /// </summary>
    public GOPSettings? GOPSettings { get; set; }

    /// <summary>
    /// Gets or sets whether scene change detection is enabled.
    /// </summary>
    public bool? SceneChangeDetection
    {
        get => GOPSettings?.SceneChangeDetection;
        set
        {
            GOPSettings settings = GOPSettings ?? new GOPSettings();
            settings.SceneChangeDetection = value;
            GOPSettings = settings;
        }
    }

    /// <summary>
    /// Gets or sets the GOP size in frames.
    /// Use <see cref="KeyFrameIntervalAsTimeSpan"/> for seconds.
    /// </summary>
    public int? KeyFrameInterval
    {
        get => GOPSettings?.Frames;
        set
        {
            GOPSettings settings = GOPSettings ?? new GOPSettings();
            settings.Frames = value;
            GOPSettings = settings;
        }
    }

    /// <summary>
    /// Gets or sets the GOP size as a <see cref="TimeSpan"/> (whole seconds).
    /// Use <see cref="KeyFrameInterval"/> for frames.
    /// </summary>
    public TimeSpan? KeyFrameIntervalAsTimeSpan
    {
        get => GOPSettings?.AsTimeSpan;
        set
        {
            GOPSettings settings = GOPSettings ?? new GOPSettings();
            settings.AsTimeSpan = value;
            GOPSettings = settings;
        }
    }

    /// <summary>
    /// Gets or sets whether overlay pictures are enabled.
    /// </summary>
    public bool? EnableOverlays { get; set; }

    /// <summary>
    /// Gets or sets the screen content detection level.
    /// </summary>
    public ScreenContentMode? ScreenContentMode { get; set; }

    /// <summary>
    /// Gets or sets the level of film grain synthesis, 0 (off) to 50.
    /// </summary>
    public int? FilmGrain { get; set; }

    /// <summary>
    /// Gets or sets whether denoising is applied when film grain is enabled.
    /// </summary>
    public bool? FilmGrainDenoise { get; set; }

    /// <summary>
    /// Gets or sets whether fast decoding is enabled (level 1); see <see cref="FastDecodeLevel"/>.
    /// </summary>
    public bool? FastDecode { get; set; }

    /// <summary>
    /// Gets or sets the fast decode level: 0 (off), 1 or 2 (faster decoding, slightly larger files). Takes
    /// precedence over <see cref="FastDecode"/>.
    /// </summary>
    public int? FastDecodeLevel { get; set; }

    /// <summary>
    /// Gets or sets the level of parallelism (<c>lp</c>): 1 to 6 sets how many threads and pictures SVT-AV1 handles
    /// at once; 0 chooses from the number of cores.
    /// </summary>
    public int? LevelOfParallelism { get; set; }

    /// <summary>
    /// Gets or sets the level of parallelism (<c>lp</c>).
    /// </summary>
    /// <remarks>Since SVT-AV1 2.0 this is not the number of logical processors; see <see cref="LevelOfParallelism"/>.</remarks>
    [Obsolete("SVT-AV1's lp is the level of parallelism (0-6), not the number of processors. Use LevelOfParallelism.")]
    public int? Threads
    {
        get => LevelOfParallelism;
        set => LevelOfParallelism = value;
    }

    /// <summary>
    /// Gets additional SVT parameters that are not directly exposed through properties (<c>-svtav1-params</c>), e.g.
    /// <c>["enable-variance-boost"] = "1"</c>. They are applied after the typed settings.
    /// </summary>
    public Dictionary<string, string> AdditionalSVTParameters { get; } = [];

    /// <summary>
    /// Gets the SVT-AV1 parameters the settings translate to (the value of <c>-svtav1-params</c>).
    /// </summary>
    /// <returns>The parameters in the order they are applied.</returns>
    public Dictionary<string, string> GetSVTParameters()
    {
        Dictionary<string, string> svt = [];
        if (Preset.HasValue)
            svt["preset"] = Preset.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        BitrateControl?.AddToDictionary(svt);
        if (Quality.HasValue)
            svt["crf"] = Quality.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        if (Tune.HasValue)
            svt["tune"] = ((int)Tune.Value).ToString();
        GOPSettings?.AddToDictionary(svt);
        if (EnableOverlays.HasValue)
            svt["enable-overlays"] = EnableOverlays.Value ? "1" : "0";
        if (ScreenContentMode.HasValue)
            svt["scm"] = ((int)ScreenContentMode.Value).ToString();
        if (FilmGrain.HasValue)
            svt["film-grain"] = FilmGrain.Value.ToString();
        if (FilmGrainDenoise.HasValue)
            svt["film-grain-denoise"] = FilmGrainDenoise.Value ? "1" : "0";
        if (FastDecodeLevel.HasValue)
            svt["fast-decode"] = FastDecodeLevel.Value.ToString();
        else if (FastDecode.HasValue)
            svt["fast-decode"] = FastDecode.Value ? "1" : "0";
        if (LevelOfParallelism.HasValue)
            svt["lp"] = LevelOfParallelism.Value.ToString();
        foreach (KeyValuePair<string, string> pair in AdditionalSVTParameters)
            svt[pair.Key] = pair.Value;
        return svt;
    }

    /// <inheritdoc />
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Profile.HasValue)
            options.SetContext("profile", Profile.Value.ProfileID);
        // svtav1-params is a dictionary option: set as a dictionary, not as separate options (which libsvtav1
        // does not have and which FFmpeg would silently leave unused).
        options.SetPrivate("svtav1-params", GetSVTParameters());
    }

    /// <summary>
    /// Creates an AVIF image sequence from the specified video frames.
    /// </summary>
    /// <param name="output">
    /// The seekable output stream that receives the encoded AVIF data.
    /// </param>
    /// <param name="frames">
    /// The frames to encode into the AVIF container.
    /// All frames should use a consistent input resolution and pixel format.
    /// </param>
    /// <param name="closeStream">
    /// <see langword="true"/> to close <paramref name="output"/> when encoding is
    /// complete; otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="disposeFrames">
    /// <see langword="true"/> to dispose the supplied frames after they have been
    /// encoded; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="output"/> does not support seeking.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="frames"/> does not contain any frames.
    /// </exception>
    /// <remarks>
    /// This method creates an AVIF container using the
    /// <see cref="OutputFormat.AVIF"/> muxer and encodes the frames using the
    /// configured AV1 codec (typically libsvtav1).
    ///
    /// <para>
    /// The output stream must support seeking because the AVIF muxer requires
    /// random access while writing the container metadata.
    /// </para>
    ///
    /// <para>
    /// The output dimensions and pixel format are automatically inferred from the
    /// first frame when they have not been explicitly configured. Frames that do
    /// not match the configured output format are converted using a bicubic
    /// <see cref="SwsContext"/> conversion.
    /// </para>
    ///
    /// <para>
    /// Existing frame timestamps are preserved. Use the overload accepting a
    /// <see cref="TimeSpan"/> frame duration to create a constant frame-rate AVIF
    /// sequence with generated timestamps.
    /// </para>
    /// </remarks>
    public void CreateAvif(Stream output, IEnumerable<AVFrame> frames, bool closeStream = true, bool disposeFrames = false)
    {

        if (!output.CanSeek)
            throw new NotSupportedException("The stream has to be seekable for the AVIF format context");

        using MediaSink context = MediaSink.Create(output, OutputFormat.AVIF!.Value, closeStream)!;

        IEnumerator<AVFrame> enumerator = frames.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
                throw new ArgumentException("No frames found.");
            AVFrame firstFrame = enumerator.Current;
            if (!TimeBase.IsValidTimeBase)
                TimeBase = firstFrame.TimeBase;
            if (Width <= 0)
                Width = firstFrame.Width;
            if (Height <= 0)
                Height = firstFrame.Height;
            if (PixelFormat == PixelFormat.None)
                PixelFormat = Codec.GetBestPixelFormat(firstFrame.PixelFormat);
            using SwsContext swsContext = new(SwsAlgorithm.Bicubic());
            _ = context.AddStream(CreateCodecContext());
            context.WriteHeader().ThrowIfError();
            using AVFrame convertedFrame = AVFrame.Allocate();
            AVFrame frameToWrite;
            do
            {
                AVFrame current = enumerator.Current;
                if (current.Width != Width || current.Height != Height || current.PixelFormat != PixelFormat)
                {
                    // The encoder may still hold the previous converted frame (lookahead), so
                    // convert into a fresh buffer instead of overwriting the old one.
                    convertedFrame.Unreference();
                    convertedFrame.Width = Width;
                    convertedFrame.Height = Height;
                    convertedFrame.PixelFormat = PixelFormat;
                    convertedFrame.CreateBuffer().ThrowIfError();
                    swsContext.Convert(current, convertedFrame).ThrowIfError();
                    frameToWrite = convertedFrame;
                    current.CopyPropertiesTo(convertedFrame);
                }
                else
                    frameToWrite = current;
                context.WriteFrame(frameToWrite, 0).ThrowIfError();
                if (disposeFrames)
                    enumerator.Current.Dispose();
            } while (enumerator.MoveNext());

            context.WriteTrailer().ThrowIfError();
        }
        finally
        {
            if (disposeFrames)
            {
                enumerator.Current.Dispose();
                while (enumerator.MoveNext())
                    enumerator.Current.Dispose();
                enumerator.Dispose();
            }
        }

    }

    /// <summary>
    /// Creates an AVIF image sequence from the specified video frames using a fixed
    /// frame duration.
    /// </summary>
    /// <param name="output">
    /// The seekable output stream that receives the encoded AVIF data.
    /// </param>
    /// <param name="frames">
    /// The frames to encode into the AVIF container.
    /// </param>
    /// <param name="frameDuration">
    /// The duration of each frame in the generated AVIF sequence.
    /// </param>
    /// <param name="closeStream">
    /// <see langword="true"/> to close <paramref name="output"/> when encoding is
    /// complete; otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="disposeFrames">
    /// <see langword="true"/> to dispose the supplied frames after they have been
    /// encoded; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="output"/> does not support seeking.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="frames"/> does not contain any frames.
    /// </exception>
    /// <remarks>
    /// This method creates an AVIF animation sequence with a constant frame rate.
    /// Each frame receives an automatically generated presentation timestamp and
    /// duration based on <paramref name="frameDuration"/>.
    ///
    /// <para>
    /// The output stream must support seeking because the AVIF muxer requires
    /// random access while writing the container metadata.
    /// </para>
    ///
    /// <para>
    /// The output dimensions and pixel format are automatically inferred from the
    /// first frame when they have not been explicitly configured. Frames that do
    /// not match the configured output format are converted using a bicubic
    /// <see cref="SwsContext"/> conversion.
    /// </para>
    /// </remarks>
    public void CreateAvif(Stream output, IEnumerable<AVFrame> frames, TimeSpan frameDuration, bool closeStream = false, bool disposeFrames = false)
    {
        if (!output.CanSeek)
            throw new NotSupportedException("The stream has to be seekable for the AVIF format context");

        using MuxerContext context = MuxerContext.Open(output, OutputFormat.AVIF!.Value, closeStream)!;

        IEnumerator<AVFrame> enumerator = frames.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
                throw new ArgumentException("No frames found.");
            AVFrame firstFrame = enumerator.Current;
            TimeBase = frameDuration;
            if (Width == 0)
                Width = firstFrame.Width;
            if (Height == 0)
                Height = firstFrame.Height;
            if (PixelFormat == PixelFormat.None)
                PixelFormat = Codec.GetBestPixelFormat(firstFrame.PixelFormat);
            using SwsContext swsContext = new(SwsAlgorithm.Bicubic());
            using CodecContext codecContext = CreateCodecContext();
            _ = context.AddStream(codecContext);
            context.WriteHeader().ThrowIfError();
            using AVFrame convertedFrame = AVFrame.Allocate();
            using AVPacket packet = AVPacket.Allocate();
            AVFrame frameToWrite;
            AVResult32 result;
            long frameIndex = 0;
            do
            {
                AVFrame current = enumerator.Current;
                if (current.Width != Width || current.Height != Height || current.PixelFormat != PixelFormat)
                {
                    // The encoder may still hold the previous converted frame (lookahead), so
                    // convert into a fresh buffer instead of overwriting the old one.
                    convertedFrame.Unreference();
                    convertedFrame.Width = Width;
                    convertedFrame.Height = Height;
                    convertedFrame.PixelFormat = PixelFormat;
                    convertedFrame.CreateBuffer().ThrowIfError();
                    swsContext.Convert(current, convertedFrame).ThrowIfError();
                    frameToWrite = convertedFrame;
                    current.CopyPropertiesTo(convertedFrame);
                }
                else
                    frameToWrite = current;
                frameToWrite.PresentationTimestamp = frameIndex++;
                frameToWrite.TimeBase = codecContext.TimeBase;
                frameToWrite.Duration = 1;
                result = codecContext.Encode(frameToWrite, packet);
                // The encoder holds its own reference now, so the input can be released even
                // when no packet came out yet.
                if (disposeFrames)
                    enumerator.Current.Dispose();
                if (result.IsTryAgain)
                    continue;
                result.ThrowIfError();
                packet.Duration = 1;
                packet.StreamIndex = 0;
                context.WritePacketInterleaved(packet).ThrowIfError();
            } while (enumerator.MoveNext());

            codecContext.DrainEncoder().ThrowIfError();

            while (!(result = codecContext.ReceivePacket(packet)).IsError)
            {
                packet.Duration = 1;
                packet.StreamIndex = 0;
                context.WritePacketInterleaved(packet).ThrowIfError();
            }
            if (result != AVResult32.EndOfFile)
                result.ThrowIfError();
            context.WriteTrailer().ThrowIfError();
        }
        finally
        {
            if (disposeFrames)
            {

                enumerator.Current.Dispose();
                while (enumerator.MoveNext())
                    enumerator.Current.Dispose();
                enumerator.Dispose();
            }
        }
    }
}
