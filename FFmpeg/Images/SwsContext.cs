using FFmpeg.Utils;

namespace FFmpeg.Images;
/// <summary>
/// Represents a context for scaling and converting image frames between different sizes and pixel formats.
/// This class provides an interface for configuring the source and destination image properties and performing the conversion using the FFmpeg scaling library.
/// </summary>
public sealed unsafe class SwsContext : Options.OptionQueryBase, IDisposable
{
    private AutoGen._SwsContext* context;

    /// <summary>
    /// Gets the width of the source image in pixels.
    /// </summary>
    public int SourceWidth { get; }

    /// <summary>
    /// Gets the height of the source image in pixels.
    /// </summary>
    public int SourceHeight { get; }

    /// <summary>
    /// Gets the pixel format of the source image.
    /// </summary>
    public PixelFormat SourceFormat { get; }

    public SwsAlgorithm Algorithm { get; }

    /// <summary>
    /// Calculates the buffer size required for the source image based on its dimensions, format, and alignment.
    /// </summary>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the buffer required to store the source image.</returns>
    /// <seealso cref="GetDestinationBufferSize(int)"/>
    public int GetSourceBufferSize(int align = 1) => GetBufferSize(SourceWidth, SourceHeight, SourceFormat, align);

    /// <summary>
    /// Calculates the buffer size required for the destination image based on its dimensions, format, and alignment.
    /// </summary>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the buffer required to store the destination image.</returns>
    /// <seealso cref="GetSourceBufferSize(int)"/>
    public int GetDestinationBufferSize(int align = 1) => GetBufferSize(DestinationWidth, DestinationHeight, DestinationFormat, align);

    /// <summary>
    /// Gets the size of a single line (row) in the source image for a given plane, considering the specified alignment.
    /// </summary>
    /// <param name="plane">The plane index of the image (e.g., 0 for Y plane in YUV format).</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the line in bytes.</returns>
    /// <seealso cref="GetDestinationLineSize(int, int)"/>
    public int GetSourceLineSize(int plane, int align = 1) => GetLineSize(SourceWidth, SourceFormat, plane, align);

    /// <summary>
    /// Gets the size of a single line (row) in the destination image for a given plane, considering the specified alignment.
    /// </summary>
    /// <param name="plane">The plane index of the image.</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the line in bytes.</returns>
    /// <seealso cref="GetSourceLineSize(int, int)"/>
    public int GetDestinationLineSize(int plane, int align = 1) => GetLineSize(DestinationWidth, DestinationFormat, plane, align);

    /// <summary>
    /// Gets the pixel format of the destination image.
    /// </summary>
    public PixelFormat DestinationFormat { get; }

    /// <summary>
    /// Gets the width of the destination image in pixels.
    /// </summary>
    public int DestinationWidth { get; }

    /// <summary>
    /// Gets the height of the destination image in pixels.
    /// </summary>
    public int DestinationHeight { get; }

    /// <summary>
    /// Provides a pointer to the underlying FFmpeg scaling context.
    /// </summary>
    protected override unsafe void* Pointer => context;

    /// <summary>
    /// Calculates the buffer size required for an image with the specified width, height, and pixel format.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="fmt">The pixel format of the image.</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the buffer required to store the image.</returns>
    public static int GetBufferSize(int width, int height, PixelFormat fmt, int align = 1) =>
        ffmpeg.av_image_get_buffer_size((AutoGen._AVPixelFormat)fmt, width, height, align);

    /// <summary>
    /// Gets the size of a single line (row) in an image with the specified width, pixel format, and plane, considering the given alignment.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="fmt">The pixel format of the image.</param>
    /// <param name="plane">The plane index of the image.</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the line in bytes.</returns>
    public static int GetLineSize(int width, PixelFormat fmt, int plane, int align = 1) =>
        ffmpeg.FFALIGN(ffmpeg.av_image_get_linesize((AutoGen._AVPixelFormat)fmt, width, plane), align);

    /// <summary>
    /// Initializes a new instance of the <see cref="SwsContext"/> class for scaling and converting images between different dimensions and pixel formats.
    /// </summary>
    /// <param name="srcW">The width of the source image in pixels.</param>
    /// <param name="srcH">The height of the source image in pixels.</param>
    /// <param name="srcFormat">The pixel format of the source image.</param>
    /// <param name="dstW">The width of the destination image in pixels.</param>
    /// <param name="dstH">The height of the destination image in pixels.</param>
    /// <param name="dstFormat">The pixel format of the destination image.</param>
    /// <param name="algorithm">The scaling algorithm to use during the conversion.</param>
    public SwsContext(int srcW, int srcH, PixelFormat srcFormat, int dstW, int dstH, PixelFormat dstFormat, SwsAlgorithm algorithm)
    {
        double* @params = stackalloc double[] { algorithm.Param1, algorithm.Param2 };

        context = ffmpeg.sws_getContext(
            srcW, srcH,
            (AutoGen._AVPixelFormat)srcFormat,
            dstW, dstH,
            (AutoGen._AVPixelFormat)dstFormat,
            (int)algorithm.AlgorithmFlags, null, null, @params
        );
        SourceWidth = srcW;
        SourceHeight = srcH;
        SourceFormat = srcFormat;
        DestinationWidth = dstW;
        DestinationHeight = dstH;
        DestinationFormat = dstFormat;
        Algorithm = algorithm;
    }


    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to the destination <see cref="AVFrame"/> using the current scaling context.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">The destination frame where the converted data will be stored.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, Image)"/>
    public AVResult32 Convert(AVFrame src, AVFrame dst) => AutoGen.ffmpeg.sws_scale_frame(context, dst.Frame, src.Frame); // <0 on error

    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to the destination <see cref="Image"/> using the current scaling context.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">The destination image where the converted data will be stored.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, AVFrame)"/>
    public AVResult32 Convert(AVFrame src, Image dst)
    {
        if (src.Height != SourceHeight || src.Width != SourceWidth || (PixelFormat)src.Format != SourceFormat)
            return AVResult32.InvalidArgument;
        if (dst.Height != DestinationHeight || dst.Width != DestinationWidth || dst.PixelFormat != DestinationFormat)
            return AVResult32.InvalidArgument;
        AutoGen.byte_ptrArray8 srcData = src.Frame->data;
        AutoGen.int_array8 srcLines = src.Frame->linesize;
        Span<IntPtr> dstData = stackalloc IntPtr[4];
        Span<int> dstLines = stackalloc int[4];
        _ = dst.GetPlanes(dstData, dstLines);
        fixed (void* dstDataPtr = dstData, dstLinesPtr = dstLines)
            return ffmpeg.sws_scale(context, (byte**)&srcData, (int*)&srcLines, 0, SourceHeight, (byte**)dstDataPtr, (int*)dstLinesPtr);
    }

    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to a destination buffer at a specified memory location.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">The destination buffer as a memory pointer.</param>
    /// <param name="dstAlign">Optional memory alignment for the destination buffer, default is 1.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, Span{byte}, int)"/>
    public AVResult32 Convert(AVFrame src, IntPtr dst, int dstAlign = 1)
    {
        if (src.Height != SourceHeight || src.Width != SourceWidth || (PixelFormat)src.Format != SourceFormat)
            return AVResult32.InvalidArgument;
        AutoGen.byte_ptrArray8 srcData = src.Frame->data;
        AutoGen.int_array8 srcLines = src.LineSize;
        AutoGen.byte_ptrArray4 dstData = new();
        AutoGen.int_array4 dstLines = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref dstData, ref dstLines, (byte*)dst, (AutoGen._AVPixelFormat)DestinationFormat, DestinationWidth, DestinationHeight, dstAlign);
        return res.IsError
            ? res
            : (AVResult32)ffmpeg.sws_scale(context, (byte**)&srcData, (int*)&srcLines, 0, SourceHeight, (byte**)&dstData, (int*)&dstLines);
    }


    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to a destination buffer represented as a <see cref="Span{byte}"/>.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">The destination buffer as a <see cref="Span{byte}"/>.</param>
    /// <param name="dstAlign">Optional memory alignment for the destination buffer, default is 1.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, IntPtr, int)"/>
    public AVResult32 Convert(AVFrame src, Span<byte> dst, int dstAlign = 1)
    {
        int size = GetDestinationBufferSize(dstAlign);
        if (dst.Length < size) return AVResult32.InvalidArgument;
        fixed (byte* dstPtr = dst)
            return Convert(src, (nint)dstPtr, dstAlign);
    }

    /// <summary>
    /// Converts the source <see cref="Image"/> to the destination <see cref="AVFrame"/> using the current scaling context.
    /// </summary>
    /// <param name="src">The source image to be converted.</param>
    /// <param name="dst">The destination frame where the converted data will be stored.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, Image)"/>
    public AVResult32 Convert(Image src, AVFrame dst)
    {
        if (dst.Height != SourceHeight || dst.Width != SourceWidth || (PixelFormat)dst.Format != SourceFormat)
            return AVResult32.InvalidArgument;
        if (src.Height != DestinationHeight || src.Width != DestinationWidth || src.PixelFormat != DestinationFormat)
            return AVResult32.InvalidArgument;
        AutoGen.byte_ptrArray8 dstData = dst.Frame->data;
        AutoGen.int_array8 dstLines = dst.Frame->linesize;
        Span<IntPtr> srcData = stackalloc IntPtr[4];
        Span<int> srcLines = stackalloc int[4];

        _ = src.GetPlanes(srcData, srcLines);
        if (!dst.HasBuffer) dst.GetBuffer().ThrowIfError();
        else dst.MakeWriteable().ThrowIfError();
        fixed (void* dstDataPtr = srcData, dstLinesPtr = srcLines)
            return ffmpeg.sws_scale(context, (byte**)dstDataPtr, (int*)dstLinesPtr, 0, SourceHeight, (byte**)&dstData, (int*)&dstLines);
    }

    /// <summary>
    /// Converts a source buffer from a specified memory location to the destination <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="src">The source buffer as a memory pointer.</param>
    /// <param name="dst">The destination frame where the converted data will be stored.</param>
    /// <param name="srcAlign">Optional memory alignment for the source buffer, default is 1.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, IntPtr, int)"/>
    public AVResult32 Convert(IntPtr src, AVFrame dst, int srcAlign = 1)
    {
        if (dst.Height != SourceHeight || dst.Width != SourceWidth || (PixelFormat)dst.Format != SourceFormat)
            return AVResult32.InvalidArgument;
        AutoGen.byte_ptrArray8 dstData = dst.Frame->data;
        AutoGen.int_array8 dstLines = dst.LineSize;
        AutoGen.byte_ptrArray4 srcData = new();
        AutoGen.int_array4 srcLines = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref srcData, ref srcLines, (byte*)src, (AutoGen._AVPixelFormat)SourceFormat, SourceWidth, SourceHeight, srcAlign);
        if (res.IsError) return res;
        if (!dst.HasBuffer) dst.GetBuffer().ThrowIfError();
        else dst.MakeWriteable().ThrowIfError();
        return ffmpeg.sws_scale(context, (byte**)&srcData, (int*)&srcLines, 0, SourceHeight, (byte**)&dstData, (int*)&dstLines);
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
        if (src.Length < size) return AVResult32.InvalidArgument;
        fixed (byte* srcPtr = src)
            return Convert((nint)srcPtr, dst, srcAlign);
    }

    /// <summary>
    /// Converts the source buffer to the destination buffer, both specified by pointers, with optional alignments for source and destination.
    /// </summary>
    /// <param name="src">Pointer to the source buffer.</param>
    /// <param name="dst">Pointer to the destination buffer.</param>
    /// <param name="srcAlign">Optional memory alignment for the source buffer, default is 1.</param>
    /// <param name="dstAlign">Optional memory alignment for the destination buffer, default is 1.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, ImageInfo, IntPtr, ImageInfo)"/>
    public AVResult32 Convert(IntPtr src, IntPtr dst, int srcAlign = 1, int dstAlign = 1)
    {
        AutoGen.byte_ptrArray4 srcData = new();
        AutoGen.int_array4 srcLines = new();
        AVResult32 res = AutoGen.ffmpeg.av_image_fill_arrays(ref srcData, ref srcLines, (byte*)src, (AutoGen._AVPixelFormat)SourceFormat, SourceWidth, SourceHeight, srcAlign);
        if (res.IsError) return res;
        AutoGen.byte_ptrArray4 dstData = new();
        AutoGen.int_array4 dstLines = new();
        res = AutoGen.ffmpeg.av_image_fill_arrays(ref dstData, ref dstLines, (byte*)dst, (AutoGen._AVPixelFormat)DestinationFormat, DestinationWidth, DestinationHeight, dstAlign);
        return res.IsError
            ? res
            : (AVResult32)AutoGen.ffmpeg.sws_scale(context, (byte**)&srcData, (int*)&srcLines, 0, SourceHeight, (byte**)&dstData, (int*)&dstLines);
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
    public AVResult32 Convert(IntPtr src, ImageInfo srcInfo, IntPtr dst, ImageInfo dstInfo)
    {
        if (srcInfo.Format != SourceFormat) throw new ArgumentException();
        if (srcInfo.Width != SourceWidth) throw new ArgumentException();
        if (srcInfo.Height != SourceHeight) throw new ArgumentException();
        if (dstInfo.Format != DestinationFormat) throw new ArgumentException();
        return dstInfo.Width != DestinationWidth
            ? throw new ArgumentException()
            : dstInfo.Height != DestinationHeight ? throw new ArgumentException() : Convert(src, dst, srcInfo.Alignment, dstInfo.Alignment);
    }

    /// <summary>
    /// Converts the source <see cref="Image"/> to the destination <see cref="Image"/>.
    /// </summary>
    /// <param name="src">The source image to be converted.</param>
    /// <param name="dst">The destination image where the converted data will be stored.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(IntPtr, ImageInfo, IntPtr, ImageInfo)"/>
    public AVResult32 Convert(Image src, Image dst) => Convert(src.Data, dst.Data, src.Info.Alignment, dst.Info.Alignment);

    /// <summary>
    /// Converts the source and destination planes using the current scaling context.
    /// </summary>
    /// <param name="srcPlanes">The source planes as an array of pointers.</param>
    /// <param name="srcLineSize">The line size of each source plane.</param>
    /// <param name="dstPlanes">The destination planes as an array of pointers.</param>
    /// <param name="dstLineSize">The line size of each destination plane.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(Span{byte}, AVFrame, int)"/>
    public AVResult32 Convert(ReadOnlySpan<IntPtr> srcPlanes, ReadOnlySpan<int> srcLineSize, ReadOnlySpan<IntPtr> dstPlanes, ReadOnlySpan<int> dstLineSize)
    {
        int srcPlaneCount = ffmpeg.av_pix_fmt_count_planes((AutoGen._AVPixelFormat)SourceFormat);
        int dstPlaneCount = ffmpeg.av_pix_fmt_count_planes((AutoGen._AVPixelFormat)DestinationFormat);
        if (srcPlanes.Length < srcPlaneCount)
            throw new ArgumentException();
        if (srcLineSize.Length < srcPlaneCount)
            throw new ArgumentException();
        if (dstPlanes.Length < dstPlaneCount)
            throw new ArgumentException();
        if (dstLineSize.Length < dstPlaneCount) throw new ArgumentException();

        fixed (void* srcPlanes_ptr = srcPlanes, srcLineSize_ptr = srcLineSize, dstPlanes_ptr = dstPlanes, dstLineSize_ptr = dstLineSize)
            return AutoGen.ffmpeg.sws_scale(context, (byte**)srcPlanes_ptr, (int*)srcLineSize_ptr, 0, SourceHeight, (byte**)dstPlanes_ptr, (int*)dstLineSize_ptr);
    }
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
    /// Frees the resources used by the <see cref="SwsContext"/> instance.
    /// </summary>
    public void Dispose()
    {
        ffmpeg.sws_freeContext(context);
        context = null;
    }

    /// <summary>
    /// Finalizer that calls the <see cref="Dispose"/> method to ensure resources are cleaned up.
    /// </summary>
    ~SwsContext() => Dispose();

    public static SwsContext CheckContext(SwsContext? context, Codecs.CodecContext src, Codecs.CodecContext dst)
    {
        if(context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }

    public static SwsContext CheckContext(SwsContext? context, AVFrame src, Codecs.CodecContext dst)
    {
        if (context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }

    public static SwsContext CheckContext(SwsContext? context, Codecs.CodecContext src, AVFrame dst)
    {
        if (context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }

    public static SwsContext CheckContext(SwsContext? context, AVFrame src, AVFrame dst)
    {
        if (context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }

}
