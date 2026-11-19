namespace FFmpeg.Codecs.Image;

/// <summary>
/// Settings for FFmpeg's BMP encoder (<c>bmp</c>), uncompressed.
/// </summary>
/// <remarks>
/// Supported pixel formats: BGRA, BGR24, RGB565, RGB555, RGB444, RGB8, BGR8, RGB4_BYTE, BGR4_BYTE, Gray8, PAL8 and
/// MonoBlack. The encoder has no options of its own.
/// </remarks>
public class Bmp : ImageCodec
{
    /// <summary>
    /// Initializes the settings; the size is taken from the frame.
    /// </summary>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the BMP encoder.</exception>
    public Bmp() : base(FindEncoder("bmp"))
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the BMP encoder.</exception>
    public Bmp(int width, int height) : base(FindEncoder("bmp"), width, height)
    {
    }
}
