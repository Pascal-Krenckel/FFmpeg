namespace FFmpeg.Codecs.Image;

/// <summary>
/// PNG prediction filters (option <c>pred</c> of the <c>png</c> encoder).
/// </summary>
public enum PngPrediction
{
    /// <summary><c>none</c>: no prediction (fastest, largest).</summary>
    None,
    /// <summary><c>sub</c>: difference to the left pixel.</summary>
    Sub,
    /// <summary><c>up</c>: difference to the pixel above.</summary>
    Up,
    /// <summary><c>avg</c>: difference to the average of the left and upper pixel.</summary>
    Average,
    /// <summary><c>paeth</c>: Paeth predictor (the default).</summary>
    Paeth,
    /// <summary><c>mixed</c>: the best filter per row (slowest, usually smallest).</summary>
    Mixed,
}

/// <summary>
/// Huffman table strategies of the JPEG encoder (option <c>huffman</c> of <c>mjpeg</c>).
/// </summary>
public enum JpegHuffmanTable
{
    /// <summary><c>default</c>: the standard tables.</summary>
    Default,
    /// <summary><c>optimal</c>: tables computed for the image (the default), smaller files.</summary>
    Optimal,
}

/// <summary>
/// libwebp content presets (option <c>preset</c> of <c>libwebp</c>).
/// </summary>
public enum WebPPreset
{
    /// <summary><c>none</c>: no preset (the default).</summary>
    None,
    /// <summary><c>default</c>: libwebp's default preset.</summary>
    Default,
    /// <summary><c>picture</c>: digital pictures, like portraits or indoor shots.</summary>
    Picture,
    /// <summary><c>photo</c>: outdoor photographs with natural lighting.</summary>
    Photo,
    /// <summary><c>drawing</c>: hand or line drawings with high contrast details.</summary>
    Drawing,
    /// <summary><c>icon</c>: small, colorful images.</summary>
    Icon,
    /// <summary><c>text</c>: text-like content.</summary>
    Text,
}

/// <summary>
/// TIFF compressions (option <c>compression_algo</c> of the <c>tiff</c> encoder), all lossless.
/// </summary>
public enum TiffCompression
{
    /// <summary><c>raw</c>: uncompressed.</summary>
    Raw,
    /// <summary><c>packbits</c>: run length encoding (the default).</summary>
    PackBits,
    /// <summary><c>lzw</c>: LZW.</summary>
    Lzw,
    /// <summary><c>deflate</c>: zlib (Adobe deflate), usually the smallest.</summary>
    Deflate,
}

/// <summary>
/// Dithering modes of the <c>paletteuse</c> filter used for GIF images.
/// </summary>
public enum GifDither
{
    /// <summary>No dithering (banding, but small files).</summary>
    None,
    /// <summary><c>bayer</c>: ordered 8x8 Bayer dithering (deterministic, compresses well).</summary>
    Bayer,
    /// <summary><c>heckbert</c>: simple error diffusion.</summary>
    Heckbert,
    /// <summary><c>floyd_steinberg</c>: Floyd-Steinberg error diffusion.</summary>
    FloydSteinberg,
    /// <summary><c>sierra2</c>: Sierra v2 error diffusion.</summary>
    Sierra2,
    /// <summary><c>sierra2_4a</c>: Sierra v2 "Lite" error diffusion (the default).</summary>
    Sierra2_4A,
    /// <summary><c>sierra3</c>: Sierra v3 error diffusion.</summary>
    Sierra3,
    /// <summary><c>burkes</c>: Burkes error diffusion.</summary>
    Burkes,
    /// <summary><c>atkinson</c>: Atkinson error diffusion.</summary>
    Atkinson,
}

internal static class ImageEncoderEnumNames
{
    public static string Name(this PngPrediction prediction) => prediction == PngPrediction.Average ? "avg" : prediction.ToString().ToLowerInvariant();

    public static string Name(this JpegHuffmanTable table) => table.ToString().ToLowerInvariant();

    public static string Name(this WebPPreset preset) => preset.ToString().ToLowerInvariant();

    public static string Name(this TiffCompression compression) => compression.ToString().ToLowerInvariant();

    public static string Name(this GifDither dither) => dither switch
    {
        GifDither.None => "0", // paletteuse has no named constant for it
        GifDither.FloydSteinberg => "floyd_steinberg",
        GifDither.Sierra2_4A => "sierra2_4a",
        _ => dither.ToString().ToLowerInvariant(),
    };
}
