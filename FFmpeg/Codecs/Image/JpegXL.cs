namespace FFmpeg.Codecs.Image;

/// <summary>
/// Settings for the libjxl encoder (<c>libjxl</c>), lossy or lossless JPEG XL.
/// </summary>
/// <remarks>
/// Supported pixel formats include RGB24, RGBA, RGB48, RGBA64, Gray8, YA8, Gray16, YA16 and the float formats.
/// Animated JPEG XL needs the <c>libjxl_anim</c> encoder.
/// </remarks>
public class JpegXL : ImageCodec
{
    private const string EncoderName = "libjxl";

    /// <summary>
    /// Initializes the settings; the size is taken from the frame.
    /// </summary>
    /// <exception cref="NotSupportedException">FFmpeg was built without libjxl.</exception>
    public JpegXL() : base(FindEncoder(EncoderName))
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="NotSupportedException">FFmpeg was built without libjxl.</exception>
    public JpegXL(int width, int height) : base(FindEncoder(EncoderName), width, height)
    {
    }

    /// <summary>Gets whether the loaded FFmpeg libraries contain libjxl.</summary>
    public static bool IsSupported => Codec.FindEncoder(EncoderName) != null;

    /// <summary>
    /// Gets or sets the maximum Butteraugli distance (<c>distance</c>), the quality: 0 is lossless, 1 (the default) is
    /// visually lossless, up to 15; lower is better.
    /// </summary>
    public float? Distance { get; set; }

    /// <summary>Gets or sets the encoding effort (<c>effort</c>), 1 (fastest) to 9 (smallest), 7 by default.</summary>
    public int? Effort { get; set; }

    /// <summary>Gets or sets whether the modular mode is forced (<c>modular</c>) instead of VarDCT for lossy images.</summary>
    public bool? Modular { get; set; }

    /// <summary>Gets or sets whether lossy images use the XYB color space (<c>xyb</c>); on by default.</summary>
    public bool? Xyb { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Distance.HasValue)
            options.SetPrivate("distance", Distance.Value);
        if (Effort.HasValue)
            options.SetPrivate("effort", Effort.Value);
        if (Modular.HasValue)
            options.SetPrivate("modular", Modular.Value);
        if (Xyb.HasValue)
            options.SetPrivate("xyb", Xyb.Value);
    }
}
