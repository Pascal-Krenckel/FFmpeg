using FFmpeg.Utils;

namespace FFmpeg.Images;

public unsafe partial class SwsContext
{
    /// <summary>
    /// Converts the source buffer, described by <see cref="ImageInfo"/>, to the destination buffer, also described by <see cref="ImageInfo"/>, using the specified scaling algorithm.
    /// </summary>
    /// <param name="src">Pointer to the source buffer.</param>
    /// <param name="srcInfo">Information describing the source image.</param>
    /// <param name="dst">Pointer to the destination buffer.</param>
    /// <param name="dstInfo">Information describing the destination image.</param>
    /// <param name="algorithm">The scaling algorithm to use for the conversion.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, ImageInfo, IntPtr, ImageInfo)"/>
    public static AVResult32 Convert(IntPtr src, ImageInfo srcInfo, IntPtr dst, ImageInfo dstInfo, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(srcInfo.Width, srcInfo.Height, srcInfo.Format, dstInfo.Width, dstInfo.Height, dstInfo.Format, algorithm);
        return context.Convert(src, dst, srcInfo.Alignment, dstInfo.Alignment);
    }

    /// <summary>
    /// Converts the source <see cref="Image"/> to the destination <see cref="Image"/> using the specified scaling algorithm.
    /// </summary>
    /// <param name="src">The source image to be converted.</param>
    /// <param name="dst">The destination image where the converted data will be stored.</param>
    /// <param name="algorithm">The scaling algorithm to use for the conversion.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, ImageInfo, IntPtr, ImageInfo, SwsAlgorithm)"/>
    public static AVResult32 Convert(Image src, Image dst, SwsAlgorithm algorithm)
        => Convert(src.Data, src.Info, dst.Data, dst.Info, algorithm);

    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to the destination <see cref="Image"/> using the specified scaling algorithm.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">The destination image where the converted data will be stored.</param>
    /// <param name="algorithm">The scaling algorithm to use for the conversion.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, Image, SwsAlgorithm)"/>
    public static AVResult32 Convert(AVFrame src, Image dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(src.Width, src.Height, (PixelFormat)src.Format, dst.Width, dst.Height, dst.PixelFormat, algorithm);
        return context.Convert(src, dst);
    }

    /// <summary>
    /// Converts the source <see cref="Image"/> to the destination <see cref="AVFrame"/> using the specified scaling algorithm.
    /// </summary>
    /// <param name="src">The source image to be converted.</param>
    /// <param name="dst">The destination frame where the converted data will be stored.</param>
    /// <param name="algorithm">The scaling algorithm to use for the conversion.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(Image, AVFrame, SwsAlgorithm)"/>
    public static AVResult32 Convert(Image src, AVFrame dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, (PixelFormat)dst.Format, algorithm);
        return context.Convert(src, dst);
    }

    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to the destination buffer, described by <see cref="ImageInfo"/>, using the specified scaling algorithm.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">Pointer to the destination buffer.</param>
    /// <param name="dstInfo">Information describing the destination image.</param>
    /// <param name="algorithm">The scaling algorithm to use for the conversion.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, IntPtr, ImageInfo, SwsAlgorithm)"/>
    public static AVResult32 Convert(AVFrame src, IntPtr dst, ImageInfo dstInfo, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(src.Width, src.Height, (PixelFormat)src.Format, dstInfo.Width, dstInfo.Height, dstInfo.Format, algorithm);
        return context.Convert(src, dst, dstInfo.Alignment);
    }

    /// <summary>
    /// Converts the source buffer, described by <see cref="ImageInfo"/>, to the destination <see cref="AVFrame"/> using the specified scaling algorithm.
    /// </summary>
    /// <param name="src">Pointer to the source buffer.</param>
    /// <param name="srcInfo">Information describing the source image.</param>
    /// <param name="dst">The destination frame where the converted data will be stored.</param>
    /// <param name="algorithm">The scaling algorithm to use for the conversion.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, ImageInfo, AVFrame, SwsAlgorithm)"/>
    public static AVResult32 Convert(IntPtr src, ImageInfo srcInfo, AVFrame dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(srcInfo.Width, srcInfo.Height, srcInfo.Format, dst.Width, dst.Height, (PixelFormat)dst.Format, algorithm);
        return context.Convert(src, dst, srcInfo.Alignment);
    }

    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to a destination buffer represented as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">The destination buffer as a <see cref="Span{T}"/>.</param>
    /// <param name="dstAlign">Optional memory alignment for the destination buffer, default is 1.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, IntPtr, int)"/>
    public AVResult32 Convert(AVFrame src, Span<byte> dst, int dstAlign = 1)
    {
        int size = GetDestinationBufferSize(dstAlign);
        if (dst.Length < size)
            return AVResult32.InvalidArgument;
        fixed (byte* dstPtr = dst)
            return Convert(src, (nint)dstPtr, dstAlign);
    }

    /// <summary>
    /// Converts the source buffer, represented as a <see cref="Span{Byte}"/>, to the destination <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="src">The source buffer as a <see cref="Span{Byte}"/>.</param>
    /// <param name="dst">The destination frame where the converted data will be stored.</param>
    /// <param name="srcAlign">Optional memory alignment for the source buffer, default is 1.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, AVFrame, int)"/>
    public AVResult32 Convert(Span<byte> src, AVFrame dst, int srcAlign = 1)
    {
        int size = GetSourceBufferSize(srcAlign);
        if (src.Length < size)
            return AVResult32.InvalidArgument;
        fixed (byte* srcPtr = src)
            return Convert((nint)srcPtr, dst, srcAlign);
    }

    /// <summary>
    /// Converts the source buffer, described by <see cref="ImageInfo"/>, to the destination buffer, also described by <see cref="ImageInfo"/>.
    /// </summary>
    /// <param name="src">Pointer to the source buffer.</param>
    /// <param name="srcInfo">The information describing the source image.</param>
    /// <param name="dst">Pointer to the destination buffer.</param>
    /// <param name="dstInfo">The information describing the destination image.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, IntPtr, int, int)"/>
    public AVResult32 Convert(IntPtr src, ImageInfo srcInfo, IntPtr dst, ImageInfo dstInfo) => srcInfo.Format != SourceFormat
            ? throw new ArgumentException()
            : srcInfo.Width != SourceWidth
            ? throw new ArgumentException()
            : srcInfo.Height != SourceHeight
            ? throw new ArgumentException()
            : dstInfo.Format != DestinationFormat
            ? throw new ArgumentException()
            : dstInfo.Width != DestinationWidth
            ? throw new ArgumentException()
            : dstInfo.Height != DestinationHeight ? throw new ArgumentException() : Convert(src, dst, srcInfo.Alignment, dstInfo.Alignment);


    /// <summary>
    /// Converts the source <see cref="Image"/> to the destination <see cref="Image"/>.
    /// </summary>
    /// <param name="src">The source image to be converted.</param>
    /// <param name="dst">The destination image where the converted data will be stored.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, ImageInfo, IntPtr, ImageInfo)"/>
    public AVResult32 Convert(Image src, Image dst) => Convert(src.Data, dst.Data, src.Info.Alignment, dst.Info.Alignment);

}
