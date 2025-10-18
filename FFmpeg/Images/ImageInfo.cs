using FFmpeg.AutoGen;

namespace FFmpeg.Images;
/// <summary>
/// Represents image metadata including width, height, pixel format, and memory alignment.
/// </summary>
public readonly unsafe struct ImageInfo(int width, int height, PixelFormat format, int alignment)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageInfo"/> struct with the specified width, height, and pixel format.
    /// Default alignment is set to 1.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="format">The pixel format of the image.</param>
    public ImageInfo(int width, int height, PixelFormat format) : this(width, height, format, 1) { }

    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    public int Width { get; init; } = width;

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    public int Height { get; init; } = height;

    /// <summary>
    /// Gets the pixel format of the image.
    /// </summary>
    public PixelFormat Format { get; init; } = format;

    /// <summary>
    /// Gets the alignment of the image buffer in memory.
    /// Alignment affects the memory padding used to ensure proper alignment for optimized access.
    /// </summary>
    public int Alignment { get; init; } = alignment;

    /// <summary>
    /// Gets the total buffer size required for storing the image in bytes, considering width, height, format, and alignment.
    /// </summary>
    public int BufferSize => ffmpeg.av_image_get_buffer_size((_AVPixelFormat)Format, Width, Height, Alignment);

    /// <summary>
    /// Gets the number of planes in the image based on the pixel format.
    /// Different pixel formats may split the image data into multiple planes.
    /// </summary>
    public int Planes => ffmpeg.av_pix_fmt_count_planes((_AVPixelFormat)Format);

    /// <summary>
    /// Gets the size of each line (stride) in bytes for the specified plane.
    /// The line size represents the width of the image data for a single row in that plane.
    /// </summary>
    /// <param name="plane">The index of the plane for which to retrieve the line size.</param>
    /// <returns>The line size in bytes for the specified plane.</returns>
    public int GetLineSize(int plane) => ffmpeg.FFALIGN(ffmpeg.av_image_get_linesize((_AVPixelFormat)Format, Width, plane), Alignment);

    /// <summary>
    /// Creates a new <see cref="ImageInfo"/> with a modified width while keeping the other properties unchanged.
    /// </summary>
    /// <param name="width">The new width of the image.</param>
    /// <returns>A new <see cref="ImageInfo"/> with the updated width.</returns>
    public ImageInfo WithWidth(int width) => WithSize(width, Height);

    /// <summary>
    /// Creates a new <see cref="ImageInfo"/> with a modified height while keeping the other properties unchanged.
    /// </summary>
    /// <param name="height">The new height of the image.</param>
    /// <returns>A new <see cref="ImageInfo"/> with the updated height.</returns>
    public ImageInfo WithHeight(int height) => WithSize(Width, height);

    /// <summary>
    /// Creates a new <see cref="ImageInfo"/> with a modified size (width and height) while keeping other properties unchanged.
    /// </summary>
    /// <param name="width">The new width of the image.</param>
    /// <param name="height">The new height of the image.</param>
    /// <returns>A new <see cref="ImageInfo"/> with the updated size.</returns>
    public ImageInfo WithSize(int width, int height) => new(width, height, Format, Alignment);

    /// <summary>
    /// Creates a new <see cref="ImageInfo"/> with a modified pixel format while keeping other properties unchanged.
    /// </summary>
    /// <param name="format">The new pixel format of the image.</param>
    /// <returns>A new <see cref="ImageInfo"/> with the updated pixel format.</returns>
    public ImageInfo WithFormat(PixelFormat format) => new(Width, Height, format, Alignment);

    /// <summary>
    /// Creates a new <see cref="ImageInfo"/> with a modified memory alignment while keeping other properties unchanged.
    /// </summary>
    /// <param name="align">The new memory alignment for the image.</param>
    /// <returns>A new <see cref="ImageInfo"/> with the updated alignment.</returns>
    public ImageInfo WithAlignment(int align) => new(Width, Height, Format, align);
}

