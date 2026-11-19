using FFmpeg.Images;

namespace FFmpeg.Codecs.Image;

/// <summary>
/// Settings for FFmpeg's TIFF encoder (<c>tiff</c>), lossless.
/// </summary>
/// <remarks>
/// Supported pixel formats: RGB24, RGB48LE, RGBA, RGBA64LE, PAL8, Gray8, YA8, Gray16LE, YA16LE, MonoBlack, MonoWhite and
/// planar YUV (420, 422, 440, 444, 410, 411; many readers do not support YUV TIFF).
/// </remarks>
public class Tiff : ImageCodec
{
    /// <summary>
    /// Initializes the settings; the size is taken from the frame.
    /// </summary>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the TIFF encoder.</exception>
    public Tiff() : base(FindEncoder("tiff"))
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the TIFF encoder.</exception>
    public Tiff(int width, int height) : base(FindEncoder("tiff"), width, height)
    {
    }

    /// <summary>Gets or sets the compression (<c>compression_algo</c>); <see cref="TiffCompression.PackBits"/> by default.</summary>
    public TiffCompression? Compression { get; set; }

    /// <summary>Gets or sets the resolution in dots per inch written to the file (<c>dpi</c>), 72 by default.</summary>
    public int? Dpi { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Compression.HasValue)
            options.SetPrivate("compression_algo", Compression.Value.Name());
        if (Dpi.HasValue)
            options.SetPrivate("dpi", Dpi.Value);
    }

    /// <inheritdoc/>
    private protected override PixelFormat ChoosePixelFormat(PixelFormat source)
        // TIFF can store YUV, but most readers cannot: choose among the RGB and gray formats.
        => source.FindBestPixelFormat(PixelFormat.RGB24, PixelFormat.RGBA, PixelFormat.RGB48LE, PixelFormat.RGBA64LE,
            PixelFormat.Gray8, PixelFormat.YA8, PixelFormat.Gray16LE, PixelFormat.YA16LE, PixelFormat.MonoBlack);
}
