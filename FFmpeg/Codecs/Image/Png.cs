namespace FFmpeg.Codecs.Image;

/// <summary>
/// Settings for FFmpeg's PNG encoder (<c>png</c>), lossless.
/// </summary>
/// <remarks>
/// Supported pixel formats: RGB24, RGBA, RGB48BE, RGBA64BE, PAL8, Gray8, Gray8A (YA8), Gray16BE, YA16BE and MonoBlack.
/// Without a pixel format the one closest to the frame is used (e.g. RGBA for BGRA frames, RGB24 for YUV frames).
/// </remarks>
public class Png : ImageCodec
{
    /// <summary>
    /// Initializes the settings; the size is taken from the frame.
    /// </summary>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the PNG encoder.</exception>
    public Png() : base(FindEncoder("png"))
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the PNG encoder.</exception>
    public Png(int width, int height) : base(FindEncoder("png"), width, height)
    {
    }

    /// <summary>Gets or sets the zlib compression level (<c>compression_level</c>), 0 (none) to 9 (smallest); zlib's default (6) if not set.</summary>
    public int? CompressionLevel { get; set; }

    /// <summary>Gets or sets the prediction filter (<c>pred</c>); <see cref="PngPrediction.Paeth"/> by default.</summary>
    public PngPrediction? Prediction { get; set; }

    /// <summary>Gets or sets the resolution in dots per inch written to the file (<c>dpi</c>), up to 65536.</summary>
    /// <remarks>Set either this or <see cref="DotsPerMeter"/>.</remarks>
    public int? Dpi { get; set; }

    /// <summary>Gets or sets the resolution in dots per meter written to the file (<c>dpm</c>), up to 65536.</summary>
    public int? DotsPerMeter { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (CompressionLevel.HasValue)
            options.SetContext("compression_level", CompressionLevel.Value);
        if (Prediction.HasValue)
            options.SetPrivate("pred", Prediction.Value.Name());
        if (Dpi.HasValue)
            options.SetPrivate("dpi", Dpi.Value);
        if (DotsPerMeter.HasValue)
            options.SetPrivate("dpm", DotsPerMeter.Value);
    }
}
