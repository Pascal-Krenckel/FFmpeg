namespace FFmpeg.Codecs.Video;

/// <summary>
/// The speed presets of x264 and x265 (option <c>preset</c>): slower presets give smaller files at the same quality.
/// </summary>
public enum SpeedPreset
{
    /// <summary><c>ultrafast</c></summary>
    UltraFast,
    /// <summary><c>superfast</c></summary>
    SuperFast,
    /// <summary><c>veryfast</c></summary>
    VeryFast,
    /// <summary><c>faster</c></summary>
    Faster,
    /// <summary><c>fast</c></summary>
    Fast,
    /// <summary><c>medium</c> (the default)</summary>
    Medium,
    /// <summary><c>slow</c></summary>
    Slow,
    /// <summary><c>slower</c></summary>
    Slower,
    /// <summary><c>veryslow</c></summary>
    VerySlow,
    /// <summary><c>placebo</c></summary>
    Placebo,
}

/// <summary>
/// x264 tunings (option <c>tune</c>). At most one of the content tunings (<see cref="Film"/> to <see cref="Ssim"/>)
/// can be combined with <see cref="FastDecode"/> and <see cref="ZeroLatency"/>.
/// </summary>
[Flags]
public enum X264Tune
{
    /// <summary>No tuning.</summary>
    None = 0,
    /// <summary><c>film</c>: high quality film content.</summary>
    Film = 1 << 0,
    /// <summary><c>animation</c>: cartoons.</summary>
    Animation = 1 << 1,
    /// <summary><c>grain</c>: preserve film grain.</summary>
    Grain = 1 << 2,
    /// <summary><c>stillimage</c>: slideshow-like content.</summary>
    StillImage = 1 << 3,
    /// <summary><c>psnr</c>: optimize for PSNR.</summary>
    Psnr = 1 << 4,
    /// <summary><c>ssim</c>: optimize for SSIM.</summary>
    Ssim = 1 << 5,
    /// <summary><c>fastdecode</c>: disable features that slow down decoding.</summary>
    FastDecode = 1 << 6,
    /// <summary><c>zerolatency</c>: no frame delay, for streaming.</summary>
    ZeroLatency = 1 << 7,
}

/// <summary>
/// H.264 profiles x264 can be restricted to (option <c>profile</c>).
/// </summary>
public enum X264Profile
{
    /// <summary><c>baseline</c>: no B-frames, no CABAC.</summary>
    Baseline,
    /// <summary><c>main</c></summary>
    Main,
    /// <summary><c>high</c></summary>
    High,
    /// <summary><c>high10</c>: up to 10 bit.</summary>
    High10,
    /// <summary><c>high422</c>: up to 10 bit 4:2:2.</summary>
    High422,
    /// <summary><c>high444</c>: up to 10 bit 4:4:4, lossless.</summary>
    High444,
}

/// <summary>
/// x264 adaptive quantization methods (option <c>aq-mode</c>).
/// </summary>
public enum X264AdaptiveQuantization
{
    /// <summary><c>none</c></summary>
    None,
    /// <summary><c>variance</c>: variance AQ (complexity mask), the default.</summary>
    Variance,
    /// <summary><c>autovariance</c></summary>
    AutoVariance,
    /// <summary><c>autovariance-biased</c>: auto-variance AQ with bias to dark scenes.</summary>
    AutoVarianceBiased,
}

/// <summary>
/// x265 tunings (option <c>tune</c>).
/// </summary>
public enum X265Tune
{
    /// <summary><c>psnr</c></summary>
    Psnr,
    /// <summary><c>ssim</c></summary>
    Ssim,
    /// <summary><c>grain</c>: preserve film grain.</summary>
    Grain,
    /// <summary><c>zerolatency</c>: no frame delay, for streaming.</summary>
    ZeroLatency,
    /// <summary><c>fastdecode</c>: disable features that slow down decoding.</summary>
    FastDecode,
    /// <summary><c>animation</c>: cartoons.</summary>
    Animation,
}

/// <summary>
/// libvpx quality/speed modes (option <c>deadline</c>).
/// </summary>
public enum VpxDeadline
{
    /// <summary><c>good</c>: the default, tune the speed with <see cref="VP9.CpuUsed"/>.</summary>
    Good,
    /// <summary><c>best</c>: slowest, mostly not better than <see cref="Good"/> with <c>cpu-used 0</c>.</summary>
    Best,
    /// <summary><c>realtime</c>: for live encoding.</summary>
    Realtime,
}

/// <summary>
/// libvpx-vp9 adaptive quantization modes (option <c>aq-mode</c>).
/// </summary>
public enum VP9AdaptiveQuantization
{
    /// <summary><c>none</c></summary>
    None,
    /// <summary><c>variance</c></summary>
    Variance,
    /// <summary><c>complexity</c></summary>
    Complexity,
    /// <summary><c>cyclic</c>: cyclic refresh.</summary>
    Cyclic,
    /// <summary><c>equator360</c>: for 360° video.</summary>
    Equator360,
}

/// <summary>
/// libvpx-vp9 content types (option <c>tune-content</c>).
/// </summary>
public enum VP9TuneContent
{
    /// <summary><c>default</c></summary>
    Default,
    /// <summary><c>screen</c>: screen captures.</summary>
    Screen,
    /// <summary><c>film</c></summary>
    Film,
}

internal static class VideoEncoderEnumNames
{
    public static string Name(this SpeedPreset preset) => preset.ToString().ToLowerInvariant();

    public static string Name(this X265Tune tune) => tune.ToString().ToLowerInvariant();

    public static string Name(this X264Profile profile) => profile.ToString().ToLowerInvariant();

    public static string Name(this VpxDeadline deadline) => deadline.ToString().ToLowerInvariant();

    public static string Name(this VP9AdaptiveQuantization mode) => mode.ToString().ToLowerInvariant();

    public static string Name(this VP9TuneContent content) => content.ToString().ToLowerInvariant();

    public static string Name(this X264AdaptiveQuantization mode) => mode switch
    {
        X264AdaptiveQuantization.AutoVarianceBiased => "autovariance-biased",
        _ => mode.ToString().ToLowerInvariant(),
    };

    public static string Name(this X264Tune tune)
    {
        const X264Tune Content = X264Tune.Film | X264Tune.Animation | X264Tune.Grain | X264Tune.StillImage | X264Tune.Psnr | X264Tune.Ssim;
        X264Tune content = tune & Content;
        if ((content & (content - 1)) != 0)
            throw new ArgumentException($"x264 accepts only one content tuning, got {content}.", nameof(tune));
        List<string> names = [];
        foreach (X264Tune flag in new[] { X264Tune.Film, X264Tune.Animation, X264Tune.Grain, X264Tune.StillImage, X264Tune.Psnr, X264Tune.Ssim, X264Tune.FastDecode, X264Tune.ZeroLatency })
        {
            if (tune.HasFlag(flag))
                names.Add(flag.ToString().ToLowerInvariant());
        }
        return string.Join(",", names);
    }
}
