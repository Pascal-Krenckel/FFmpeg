using FFmpeg.Images;

namespace FFmpeg.Codecs.Image;

/// <summary>
/// Settings for FFmpeg's JPEG encoder (<c>mjpeg</c>), as with <c>ffmpeg -i in.png -q:v 2 out.jpg</c>.
/// </summary>
/// <remarks>
/// <para>
/// Supported pixel formats: YUVJ420P, YUVJ422P, YUVJ444P and YUV420P, YUV422P, YUV444P. JPEG uses full range YUV:
/// limited range frames (e.g. decoded video) are converted to <see cref="ColorRange.JPEG"/>, also by the <see cref="Transcoder"/>.
/// Without a pixel format the one closest to the frame is used (YUV444P for RGB frames, so there is no chroma
/// subsampling); set <see cref="VideoCodec.PixelFormat"/> to <see cref="PixelFormat.YUV420P"/> for smaller files.
/// </para>
/// </remarks>
public class Jpeg : ImageCodec
{
    /// <summary>
    /// Initializes the settings; the size is taken from the frame.
    /// </summary>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the MJPEG encoder.</exception>
    public Jpeg() : base(FindEncoder("mjpeg"))
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the MJPEG encoder.</exception>
    public Jpeg(int width, int height) : base(FindEncoder("mjpeg"), width, height)
    {
    }

    /// <summary>
    /// Gets or sets the quantizer scale (<c>-q:v</c>): 2 (best quality, largest) to 31 (worst); 2 to 5 are common.
    /// </summary>
    /// <remarks>Without it the encoder uses its default bitrate (200 kbit/s for 25 fps, i.e. about 8 kB per image).</remarks>
    public int? QScale { get; set; }

    /// <summary>Gets or sets the Huffman table strategy (<c>huffman</c>); <see cref="JpegHuffmanTable.Optimal"/> by default.</summary>
    public JpegHuffmanTable? HuffmanTable { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (QScale.HasValue)
            options.SetQuality(QScale.Value);
        if (HuffmanTable.HasValue)
            options.SetPrivate("huffman", HuffmanTable.Value.Name());
    }
}
