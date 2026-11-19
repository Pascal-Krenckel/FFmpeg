using FFmpeg.Utils;

namespace FFmpeg.Codecs.Video;

/// <summary>
/// Settings for the libvpx VP9 encoder (<c>libvpx-vp9</c>).
/// </summary>
/// <remarks>
/// <para>
/// Rate control: <see cref="Crf"/> alone gives constant quality (FFmpeg's <c>-crf 31 -b:v 0</c>; the settings set
/// <c>b</c> to 0, because libvpx otherwise caps the quality at FFmpeg's default bitrate of 200 kbit/s);
/// <see cref="Crf"/> with <see cref="Bitrate"/> gives constrained quality; <see cref="Bitrate"/> alone gives average
/// bitrate, and with <see cref="MinBitrate"/> = <see cref="MaxBitrate"/> = <see cref="Bitrate"/> constant bitrate.
/// </para>
/// <para>For speed use <see cref="RowMultithreading"/>, <see cref="TileColumns"/> and <see cref="CpuUsed"/>.</para>
/// </remarks>
public class VP9 : VideoCodec
{
    private const string EncoderName = "libvpx-vp9";

    /// <summary>
    /// Initializes the settings with a size, time base and frame rate.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="timeBase">The time base of the frames.</param>
    /// <param name="framerate">The frame rate, or <see cref="Rational.Zero"/>.</param>
    /// <exception cref="NotSupportedException">FFmpeg was built without libvpx.</exception>
    public VP9(int width, int height, Rational timeBase, Rational framerate) : base(FindEncoder(EncoderName), width, height, timeBase, framerate)
    {
    }

    /// <summary>
    /// Initializes the settings without a size, time base or frame rate, e.g. for the <see cref="Transcoder"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">FFmpeg was built without libvpx.</exception>
    public VP9() : base(FindEncoder(EncoderName))
    {
    }

    /// <summary>Gets whether the loaded FFmpeg libraries contain libvpx-vp9.</summary>
    public static bool IsSupported => Codec.FindEncoder(EncoderName) != null;

    /// <summary>Gets or sets the quality/speed mode (<c>deadline</c>).</summary>
    public VpxDeadline? Deadline { get; set; }

    /// <summary>Gets or sets the speed (<c>cpu-used</c>): -8 to 8, higher is faster and lower quality.</summary>
    public int? CpuUsed { get; set; }

    /// <summary>Gets or sets the constant rate factor (<c>crf</c>): 0 to 63, lower is better.</summary>
    public int? Crf { get; set; }

    /// <summary>Gets or sets the target bitrate in bit/s (<c>b</c>); see the remarks for how it combines with <see cref="Crf"/>.</summary>
    public long? Bitrate { get; set; }

    /// <summary>Gets or sets the maximum bitrate in bit/s (<c>maxrate</c>).</summary>
    public long? MaxBitrate { get; set; }

    /// <summary>Gets or sets the minimum bitrate in bit/s (<c>minrate</c>).</summary>
    public long? MinBitrate { get; set; }

    /// <summary>Gets or sets the rate control buffer size in bits (<c>bufsize</c>).</summary>
    public long? BufferSize { get; set; }

    /// <summary>Gets or sets the maximum distance between key frames in frames (<c>g</c>).</summary>
    public int? KeyFrameInterval { get; set; }

    /// <summary>Gets or sets the minimum distance between key frames in frames (<c>keyint_min</c>).</summary>
    public int? MinKeyFrameInterval { get; set; }

    /// <summary>Gets or sets the number of frames to look ahead (<c>lag-in-frames</c>), 0 to 25.</summary>
    public int? LagInFrames { get; set; }

    /// <summary>Gets or sets the use of alternate reference frames (<c>auto-alt-ref</c>): 0 off, 1 on, up to 6 for VP9.</summary>
    public int? AutoAltRef { get; set; }

    /// <summary>Gets or sets row based multi-threading (<c>row-mt</c>).</summary>
    public bool? RowMultithreading { get; set; }

    /// <summary>Gets or sets the number of tile columns as log2 (<c>tile-columns</c>), 0 to 6.</summary>
    public int? TileColumns { get; set; }

    /// <summary>Gets or sets the number of tile rows as log2 (<c>tile-rows</c>), 0 to 2.</summary>
    public int? TileRows { get; set; }

    /// <summary>Gets or sets lossless encoding (<c>lossless</c>).</summary>
    public bool? Lossless { get; set; }

    /// <summary>Gets or sets the adaptive quantization mode (<c>aq-mode</c>).</summary>
    public VP9AdaptiveQuantization? AdaptiveQuantization { get; set; }

    /// <summary>Gets or sets the content type to tune for (<c>tune-content</c>).</summary>
    public VP9TuneContent? TuneContent { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Deadline.HasValue)
            options.SetPrivate("deadline", Deadline.Value.Name());
        if (CpuUsed.HasValue)
            options.SetPrivate("cpu-used", CpuUsed.Value);
        if (Crf.HasValue)
            options.SetPrivate("crf", Crf.Value);
        // Constant quality needs b=0; FFmpeg's default bitrate (200 kbit/s) would turn crf into constrained quality.
        long? bitrate = Bitrate ?? (Crf.HasValue ? 0 : null);
        SetRateControl(options, bitrate, MaxBitrate, MinBitrate, BufferSize, KeyFrameInterval, MinKeyFrameInterval, null, null);
        if (LagInFrames.HasValue)
            options.SetPrivate("lag-in-frames", LagInFrames.Value);
        if (AutoAltRef.HasValue)
            options.SetPrivate("auto-alt-ref", AutoAltRef.Value);
        if (RowMultithreading.HasValue)
            options.SetPrivate("row-mt", RowMultithreading.Value);
        if (TileColumns.HasValue)
            options.SetPrivate("tile-columns", TileColumns.Value);
        if (TileRows.HasValue)
            options.SetPrivate("tile-rows", TileRows.Value);
        if (Lossless.HasValue)
            options.SetPrivate("lossless", Lossless.Value);
        if (AdaptiveQuantization.HasValue)
            options.SetPrivate("aq-mode", AdaptiveQuantization.Value.Name());
        if (TuneContent.HasValue)
            options.SetPrivate("tune-content", TuneContent.Value.Name());
    }
}
