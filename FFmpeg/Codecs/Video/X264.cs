using FFmpeg.Utils;

namespace FFmpeg.Codecs.Video;

/// <summary>
/// Settings for the x264 H.264 encoder (<c>libx264</c>).
/// </summary>
/// <remarks>
/// Choose one rate control: <see cref="Crf"/> (constant quality, recommended), <see cref="Qp"/> (constant
/// quantizer) or <see cref="Bitrate"/> (average bitrate, optionally limited with <see cref="MaxBitrate"/> and
/// <see cref="BufferSize"/>). <see cref="X264Parameters"/> are passed in <c>-x264-params</c> and override the typed
/// settings.
/// </remarks>
public class X264 : VideoCodec
{
    private const string EncoderName = "libx264";

    /// <summary>
    /// Initializes the settings with a size, time base and frame rate.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="timeBase">The time base of the frames.</param>
    /// <param name="framerate">The frame rate, or <see cref="Rational.Zero"/>.</param>
    /// <exception cref="NotSupportedException">FFmpeg was built without libx264.</exception>
    public X264(int width, int height, Rational timeBase, Rational framerate) : base(FindEncoder(EncoderName), width, height, timeBase, framerate)
    {
    }

    /// <summary>
    /// Initializes the settings without a size, time base or frame rate, e.g. for the <see cref="Transcoder"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">FFmpeg was built without libx264.</exception>
    public X264() : base(FindEncoder(EncoderName))
    {
    }

    /// <summary>Gets whether the loaded FFmpeg libraries contain libx264.</summary>
    public static bool IsSupported => Codec.FindEncoder(EncoderName) != null;

    /// <summary>Gets or sets the speed preset (<c>preset</c>).</summary>
    public SpeedPreset? Preset { get; set; }

    /// <summary>Gets or sets the tuning (<c>tune</c>).</summary>
    public X264Tune? Tune { get; set; }

    /// <summary>Gets or sets the profile the output is restricted to (<c>profile</c>).</summary>
    public X264Profile? Profile { get; set; }

    /// <summary>Gets or sets the level, e.g. 4.1 (<c>level</c>).</summary>
    public double? Level { get; set; }

    /// <summary>Gets or sets the constant rate factor (<c>crf</c>): 0 (lossless for 8 bit) to 51, 23 by default; lower is better.</summary>
    public double? Crf { get; set; }

    /// <summary>Gets or sets a constant quantizer (<c>qp</c>), 0 to 69.</summary>
    public int? Qp { get; set; }

    /// <summary>Gets or sets the average bitrate in bit/s (<c>b</c>).</summary>
    public long? Bitrate { get; set; }

    /// <summary>Gets or sets the maximum bitrate in bit/s for VBV (<c>maxrate</c>); needs <see cref="BufferSize"/>.</summary>
    public long? MaxBitrate { get; set; }

    /// <summary>Gets or sets the VBV buffer size in bits (<c>bufsize</c>).</summary>
    public long? BufferSize { get; set; }

    /// <summary>Gets or sets the maximum distance between key frames in frames (<c>g</c>).</summary>
    public int? KeyFrameInterval { get; set; }

    /// <summary>Gets or sets the minimum distance between key frames in frames (<c>keyint_min</c>).</summary>
    public int? MinKeyFrameInterval { get; set; }

    /// <summary>Gets or sets the maximum number of consecutive B-frames (<c>bf</c>).</summary>
    public int? BFrames { get; set; }

    /// <summary>Gets or sets the number of reference frames (<c>refs</c>).</summary>
    public int? ReferenceFrames { get; set; }

    /// <summary>Gets or sets the adaptive quantization method (<c>aq-mode</c>).</summary>
    public X264AdaptiveQuantization? AdaptiveQuantization { get; set; }

    /// <summary>Gets or sets the adaptive quantization strength (<c>aq-strength</c>), 1.0 by default.</summary>
    public double? AdaptiveQuantizationStrength { get; set; }

    /// <summary>Gets or sets the number of frames to look ahead for frame type and rate control (<c>rc-lookahead</c>).</summary>
    public int? Lookahead { get; set; }

    /// <summary>
    /// Gets x264 parameters passed in <c>-x264-params</c> (e.g. <c>["deblock"] = "-1,-1"</c>); they override the typed
    /// settings.
    /// </summary>
    public Dictionary<string, string> X264Parameters { get; } = [];

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Preset.HasValue)
            options.SetPrivate("preset", Preset.Value.Name());
        if (Tune.HasValue && Tune.Value != X264Tune.None)
            options.SetPrivate("tune", Tune.Value.Name());
        if (Profile.HasValue)
            options.SetPrivate("profile", Profile.Value.Name());
        if (Level.HasValue)
            options.SetContext("level", (long)Math.Round(Level.Value * 10));
        if (Crf.HasValue)
            options.SetPrivate("crf", Crf.Value);
        if (Qp.HasValue)
            options.SetPrivate("qp", Qp.Value);
        SetRateControl(options, Bitrate, MaxBitrate, null, BufferSize, KeyFrameInterval, MinKeyFrameInterval, BFrames, ReferenceFrames);
        if (AdaptiveQuantization.HasValue)
            options.SetPrivate("aq-mode", AdaptiveQuantization.Value.Name());
        if (AdaptiveQuantizationStrength.HasValue)
            options.SetPrivate("aq-strength", AdaptiveQuantizationStrength.Value);
        if (Lookahead.HasValue)
            options.SetPrivate("rc-lookahead", Lookahead.Value);
        options.SetPrivate("x264-params", X264Parameters);
    }
}
