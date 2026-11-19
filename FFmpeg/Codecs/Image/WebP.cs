using FFmpeg.Images;

namespace FFmpeg.Codecs.Image;

/// <summary>
/// Settings for the libwebp encoder (<c>libwebp</c>), lossy or lossless WebP.
/// </summary>
/// <remarks>
/// Supported pixel formats: BGRA, YUV420P and YUVA420P. Lossy WebP is always YUV 4:2:0; for <see cref="Lossless"/>
/// images BGRA is used unless another pixel format is set. Animated WebP needs the <c>libwebp_anim</c> encoder.
/// </remarks>
public class WebP : ImageCodec
{
    private const string EncoderName = "libwebp";

    /// <summary>
    /// Initializes the settings; the size is taken from the frame.
    /// </summary>
    /// <exception cref="NotSupportedException">FFmpeg was built without libwebp.</exception>
    public WebP() : base(FindEncoder(EncoderName))
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="NotSupportedException">FFmpeg was built without libwebp.</exception>
    public WebP(int width, int height) : base(FindEncoder(EncoderName), width, height)
    {
    }

    /// <summary>Gets whether the loaded FFmpeg libraries contain libwebp.</summary>
    public static bool IsSupported => Codec.FindEncoder(EncoderName) != null;

    /// <summary>Gets or sets lossless encoding (<c>lossless</c>).</summary>
    public bool? Lossless { get; set; }

    /// <summary>
    /// Gets or sets the quality (<c>quality</c>), 0 to 100, 75 by default: for lossy images higher is better, for
    /// lossless images higher is smaller and slower.
    /// </summary>
    public float? Quality { get; set; }

    /// <summary>Gets or sets the content preset (<c>preset</c>); it sets the defaults the other settings change.</summary>
    public WebPPreset? Preset { get; set; }

    /// <summary>Gets or sets the compression method (<c>compression_level</c>), 0 (fastest) to 6 (smallest), 4 by default.</summary>
    public int? CompressionLevel { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Lossless.HasValue)
            options.SetPrivate("lossless", Lossless.Value);
        if (Quality.HasValue)
            options.SetPrivate("quality", Quality.Value);
        if (Preset.HasValue)
            options.SetPrivate("preset", Preset.Value.Name());
        if (CompressionLevel.HasValue)
            options.SetContext("compression_level", CompressionLevel.Value);
    }

    /// <inheritdoc/>
    private protected override PixelFormat ChoosePixelFormat(PixelFormat source)
        => Lossless == true ? PixelFormat.BGRA : base.ChoosePixelFormat(source);
}
