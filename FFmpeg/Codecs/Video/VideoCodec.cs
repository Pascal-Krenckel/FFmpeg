using FFmpeg.Images;
using FFmpeg.Utils;
using System.Text;

namespace FFmpeg.Codecs.Video;

/// <summary>
/// Base class for typed video encoder settings such as <see cref="X264"/> or <see cref="SVTAV1"/>.
/// </summary>
/// <remarks>
/// A size, time base, frame rate or pixel format that is not set (0, an invalid rational, <see cref="PixelFormat.None"/>)
/// is left to the encoder context; see <see cref="EncoderSettings"/>.
/// </remarks>
public abstract class VideoCodec : EncoderSettings
{
    /// <summary>
    /// Initializes the settings with a size, time base and frame rate.
    /// </summary>
    /// <param name="codec">The video encoder.</param>
    /// <param name="width">The width of the video frame in pixels; 0 to leave it unset.</param>
    /// <param name="height">The height of the video frame in pixels; 0 to leave it unset.</param>
    /// <param name="timeBase">The time base of the frames.</param>
    /// <param name="framerate">The frame rate; <see cref="Rational.Zero"/> to leave it unset. Some encoders fail to
    /// open with a large time base (e.g. 1/90000) if it is not set.</param>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is not a video encoder.</exception>
    protected VideoCodec(Codec codec, int width, int height, Rational timeBase, Rational framerate) : base(codec)
    {
        if (codec.MediaType != MediaType.Video)
            throw new ArgumentException($"{codec.Name} is not a video encoder.", nameof(codec));
        Width = width;
        Height = height;
        TimeBase = timeBase;
        Framerate = framerate;
    }

    /// <summary>
    /// Initializes the settings with a size and time base.
    /// </summary>
    /// <param name="codec">The video encoder.</param>
    /// <param name="width">The width of the video frame in pixels; 0 to leave it unset.</param>
    /// <param name="height">The height of the video frame in pixels; 0 to leave it unset.</param>
    /// <param name="timeBase">The time base of the frames.</param>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is not a video encoder.</exception>
    protected VideoCodec(Codec codec, int width, int height, Rational timeBase) : this(codec, width, height, timeBase, Rational.Zero)
    { }

    /// <summary>
    /// Initializes the settings without a size, time base or frame rate, e.g. for the <see cref="Transcoder"/>, which takes
    /// them from the frames.
    /// </summary>
    /// <param name="codec">The video encoder.</param>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is not a video encoder.</exception>
    protected VideoCodec(Codec codec) : this(codec, 0, 0, Rational.Zero, Rational.Zero)
    { }

    /// <summary>
    /// Gets or sets the width of the video frame in pixels; 0 leaves it unset.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the video frame in pixels; 0 leaves it unset.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the time base of the frames; an invalid rational leaves it unset. <see cref="EncoderSettings.CreateCodecContext"/>
    /// uses 1/<see cref="Framerate"/> then.
    /// </summary>
    public Rational TimeBase { get; set; }

    /// <summary>
    /// Gets or sets the frame rate; an invalid rational leaves it unset.
    /// </summary>
    public Rational Framerate { get; set; }

    /// <summary>
    /// Gets or sets the pixel format; <see cref="PixelFormat.None"/> leaves it unset (<see cref="EncoderSettings.CreateCodecContext"/>
    /// then uses the first format the encoder supports).
    /// </summary>
    public PixelFormat PixelFormat { get; set; } = PixelFormat.None;

    /// <inheritdoc/>
    private protected override void ConfigureFormat(CodecContext context)
    {
        if (PixelFormat != PixelFormat.None)
        {
            ReadOnlySpan<PixelFormat> supported = Codec.SupportedPixelFormats;
            if (!supported.IsEmpty && !Contains(supported, PixelFormat))
                throw new ArgumentException($"{Codec.Name} does not support the pixel format {PixelFormat}.", nameof(PixelFormat));
            context.PixelFormat = PixelFormat;
        }
        if (Width > 0)
            context.Width = Width;
        if (Height > 0)
            context.Height = Height;
        if (TimeBase.IsValidTimeBase)
            context.TimeBase = TimeBase;
        if (Framerate.IsValidTimeBase)
            context.FrameRate = Framerate;
    }

    /// <inheritdoc/>
    private protected override void ApplyDefaults(CodecContext context)
    {
        if (context.PixelFormat == PixelFormat.None)
            context.PixelFormat = Codec.SupportedPixelFormats.IsEmpty ? PixelFormat.None : Codec.SupportedPixelFormats[0];
        if (!context.TimeBase.IsValidTimeBase && context.FrameRate.IsValidTimeBase)
            context.TimeBase = new Rational(context.FrameRate.Denominator, context.FrameRate.Numerator);
    }

    /// <inheritdoc/>
    private protected override void AppendFormat(StringBuilder sb)
    {
        if (Width > 0 && Height > 0)
            _ = sb.Append($" -s {Width}x{Height}");
        if (PixelFormat != PixelFormat.None)
            _ = sb.Append($" -pix_fmt {AutoGen.ffmpeg.av_get_pix_fmt_name((AutoGen._AVPixelFormat)PixelFormat)}");
        if (Framerate.IsValidTimeBase)
            _ = sb.Append($" -r {Framerate.Numerator}/{Framerate.Denominator}");
    }

    /// <summary>
    /// Sets the generic rate control and GOP options of the codec context that are set (<c>b</c>, <c>maxrate</c>,
    /// <c>minrate</c>, <c>bufsize</c>, <c>g</c>, <c>keyint_min</c>, <c>bf</c>, <c>refs</c>).
    /// </summary>
    private protected static void SetRateControl(EncoderOptions options, long? bitrate, long? maxBitrate, long? minBitrate, long? bufferSize,
        int? keyFrameInterval, int? minKeyFrameInterval, int? bFrames, int? referenceFrames)
    {
        if (bitrate.HasValue)
            options.SetContext("b", bitrate.Value);
        if (maxBitrate.HasValue)
            options.SetContext("maxrate", maxBitrate.Value);
        if (minBitrate.HasValue)
            options.SetContext("minrate", minBitrate.Value);
        if (bufferSize.HasValue)
            options.SetContext("bufsize", bufferSize.Value);
        if (keyFrameInterval.HasValue)
            options.SetContext("g", keyFrameInterval.Value);
        if (minKeyFrameInterval.HasValue)
            options.SetContext("keyint_min", minKeyFrameInterval.Value);
        if (bFrames.HasValue)
            options.SetContext("bf", bFrames.Value);
        if (referenceFrames.HasValue)
            options.SetContext("refs", referenceFrames.Value);
    }
}
