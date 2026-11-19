using FFmpeg.Utils;

namespace FFmpeg.Codecs.Video;

/// <summary>
/// Settings for the x265 H.265/HEVC encoder (<c>libx265</c>).
/// </summary>
/// <remarks>
/// Choose one rate control: <see cref="Crf"/> (constant quality, recommended), <see cref="Qp"/> or
/// <see cref="Bitrate"/>. <see cref="X265Parameters"/> are passed in <c>-x265-params</c> and override the typed
/// settings.
/// </remarks>
public class X265 : VideoCodec
{
    private const string EncoderName = "libx265";

    /// <summary>
    /// Initializes the settings with a size, time base and frame rate.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="timeBase">The time base of the frames.</param>
    /// <param name="framerate">The frame rate, or <see cref="Rational.Zero"/>.</param>
    /// <exception cref="NotSupportedException">FFmpeg was built without libx265.</exception>
    public X265(int width, int height, Rational timeBase, Rational framerate) : base(FindEncoder(EncoderName), width, height, timeBase, framerate)
    {
    }

    /// <summary>
    /// Initializes the settings without a size, time base or frame rate, e.g. for the <see cref="Transcoder"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">FFmpeg was built without libx265.</exception>
    public X265() : base(FindEncoder(EncoderName))
    {
    }

    /// <summary>Gets whether the loaded FFmpeg libraries contain libx265.</summary>
    public static bool IsSupported => Codec.FindEncoder(EncoderName) != null;

    /// <summary>Gets or sets the speed preset (<c>preset</c>).</summary>
    public SpeedPreset? Preset { get; set; }

    /// <summary>Gets or sets the tuning (<c>tune</c>).</summary>
    public X265Tune? Tune { get; set; }

    /// <summary>
    /// Gets or sets the profile the output is restricted to (<c>profile</c>), e.g. <c>main</c>, <c>main10</c>,
    /// <c>main444-8</c>, <c>main422-10</c>, <c>mainstillpicture</c>.
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>Gets or sets the constant rate factor (<c>crf</c>): 0 to 51, 28 by default; lower is better.</summary>
    public double? Crf { get; set; }

    /// <summary>Gets or sets a constant quantizer (<c>qp</c>), 0 to 51.</summary>
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

    /// <summary>
    /// Gets x265 parameters passed in <c>-x265-params</c> (e.g. <c>["aq-mode"] = "3"</c>); they override the typed settings.
    /// </summary>
    public Dictionary<string, string> X265Parameters { get; } = [];

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Preset.HasValue)
            options.SetPrivate("preset", Preset.Value.Name());
        if (Tune.HasValue)
            options.SetPrivate("tune", Tune.Value.Name());
        if (Profile != null)
            options.SetPrivate("profile", Profile);
        if (Crf.HasValue)
            options.SetPrivate("crf", Crf.Value);
        if (Qp.HasValue)
            options.SetPrivate("qp", Qp.Value);
        SetRateControl(options, Bitrate, MaxBitrate, null, BufferSize, KeyFrameInterval, MinKeyFrameInterval, BFrames, ReferenceFrames);
        options.SetPrivate("x265-params", X265Parameters);
    }
}
