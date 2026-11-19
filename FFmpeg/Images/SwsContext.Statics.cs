using FFmpeg.Utils;

namespace FFmpeg.Images;

public unsafe partial class SwsContext
{
    /// <summary>
    /// Converts a frame into another frame with a temporary dynamic context.
    /// </summary>
    /// <remarks>
    /// To convert many images, keep one <see cref="SwsContext"/> instead: it only sets the conversion up again when
    /// the images change.
    /// </remarks>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination frame; see <see cref="Convert(AVFrame, AVFrame)"/>.</param>
    /// <param name="algorithm">The scaling algorithm.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    public static AVResult32 Convert(AVFrame src, AVFrame dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(algorithm);
        return context.Convert(src, dst);
    }

    /// <summary>
    /// Converts a raw buffer into another raw buffer with a temporary dynamic context.
    /// </summary>
    /// <param name="src">The source pixels, laid out as <paramref name="srcInfo"/> describes.</param>
    /// <param name="srcInfo">The size, format and alignment of the source.</param>
    /// <param name="dst">The destination buffer, at least <see cref="ImageInfo.BufferSize"/> bytes.</param>
    /// <param name="dstInfo">The size, format and alignment of the destination.</param>
    /// <param name="algorithm">The scaling algorithm.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    public static AVResult32 Convert(IntPtr src, ImageInfo srcInfo, IntPtr dst, ImageInfo dstInfo, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(algorithm);
        return context.Convert(src, srcInfo, dst, dstInfo);
    }

    /// <summary>
    /// Converts an image into another image with a temporary dynamic context.
    /// </summary>
    /// <param name="src">The source image.</param>
    /// <param name="dst">The destination image.</param>
    /// <param name="algorithm">The scaling algorithm.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    public static AVResult32 Convert(Image src, Image dst, SwsAlgorithm algorithm)
        => Convert(src.Data, src.Info, dst.Data, dst.Info, algorithm);

    /// <summary>
    /// Converts a frame into an image with a temporary dynamic context.
    /// </summary>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination image.</param>
    /// <param name="algorithm">The scaling algorithm.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    public static AVResult32 Convert(AVFrame src, Image dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(algorithm);
        return context.Convert(src, dst);
    }

    /// <summary>
    /// Converts an image into a frame with a temporary dynamic context.
    /// </summary>
    /// <param name="src">The source image.</param>
    /// <param name="dst">The destination frame; see <see cref="Convert(AVFrame, AVFrame)"/>.</param>
    /// <param name="algorithm">The scaling algorithm.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    public static AVResult32 Convert(Image src, AVFrame dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(algorithm);
        return context.Convert(src, dst);
    }

    /// <summary>
    /// Converts a frame into a raw buffer with a temporary dynamic context.
    /// </summary>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination buffer, at least <see cref="ImageInfo.BufferSize"/> bytes.</param>
    /// <param name="dstInfo">The size, format and alignment of the destination.</param>
    /// <param name="algorithm">The scaling algorithm.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    public static AVResult32 Convert(AVFrame src, IntPtr dst, ImageInfo dstInfo, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(algorithm);
        return context.Convert(src, dst, dstInfo);
    }

    /// <summary>
    /// Converts a raw buffer into a frame with a temporary dynamic context.
    /// </summary>
    /// <param name="src">The source pixels, laid out as <paramref name="srcInfo"/> describes.</param>
    /// <param name="srcInfo">The size, format and alignment of the source.</param>
    /// <param name="dst">The destination frame; see <see cref="Convert(AVFrame, AVFrame)"/>.</param>
    /// <param name="algorithm">The scaling algorithm.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    public static AVResult32 Convert(IntPtr src, ImageInfo srcInfo, AVFrame dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(algorithm);
        return context.Convert(src, srcInfo, dst);
    }

    /// <summary>
    /// Calculates the buffer size required for an image with the specified width, height, and pixel format.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="fmt">The pixel format of the image.</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the buffer required to store the image.</returns>
    public static int GetBufferSize(int width, int height, PixelFormat fmt, int align = 1) =>
        AutoGen.ffmpeg.av_image_get_buffer_size((AutoGen._AVPixelFormat)fmt, width, height, align);

    /// <summary>
    /// Gets the size of a single line (row) in an image with the specified width, pixel format, and plane, considering the given alignment.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="fmt">The pixel format of the image.</param>
    /// <param name="plane">The plane index of the image.</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the line in bytes.</returns>
    public static int GetLineSize(int width, PixelFormat fmt, int plane, int align = 1) =>
        AutoGen.ffmpeg.FFALIGN(AutoGen.ffmpeg.av_image_get_linesize((AutoGen._AVPixelFormat)fmt, width, plane), align);
}
