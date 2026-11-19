using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.Images;

/// <summary>
/// Members that only work with fixed contexts (FFmpeg's legacy API).
/// </summary>
public unsafe partial class SwsContext
{
    private const string FixedProperty = "Only set for fixed contexts (the obsolete constructor); dynamic contexts take the size and format from each image.";
    private const string FixedConvert = "Only works with fixed contexts (the obsolete constructor). Use the overloads with AVFrame, Image or ImageInfo, which also work with dynamic contexts.";

    /// <summary>Gets the width of the source image of a fixed context; 0 for dynamic contexts.</summary>
    [Obsolete(FixedProperty)]
    public int SourceWidth => fixedSourceWidth;

    /// <summary>Gets the height of the source image of a fixed context; 0 for dynamic contexts.</summary>
    [Obsolete(FixedProperty)]
    public int SourceHeight => fixedSourceHeight;

    /// <summary>Gets the pixel format of the source image of a fixed context; <see cref="PixelFormat.None"/> for dynamic contexts.</summary>
    [Obsolete(FixedProperty)]
    public PixelFormat SourceFormat => fixedSourceFormat;

    /// <summary>Gets the width of the destination image of a fixed context; 0 for dynamic contexts.</summary>
    [Obsolete(FixedProperty)]
    public int DestinationWidth => fixedDestinationWidth;

    /// <summary>Gets the height of the destination image of a fixed context; 0 for dynamic contexts.</summary>
    [Obsolete(FixedProperty)]
    public int DestinationHeight => fixedDestinationHeight;

    /// <summary>Gets the pixel format of the destination image of a fixed context; <see cref="PixelFormat.None"/> for dynamic contexts.</summary>
    [Obsolete(FixedProperty)]
    public PixelFormat DestinationFormat => fixedDestinationFormat;

    /// <summary>Calculates the buffer size of the source image of a fixed context.</summary>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the buffer required to store the source image.</returns>
    [Obsolete(FixedProperty + " Use ImageInfo.BufferSize.")]
    public int GetSourceBufferSize(int align = 1) => GetBufferSize(fixedSourceWidth, fixedSourceHeight, fixedSourceFormat, align);

    /// <summary>Calculates the buffer size of the destination image of a fixed context.</summary>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the buffer required to store the destination image.</returns>
    [Obsolete(FixedProperty + " Use ImageInfo.BufferSize.")]
    public int GetDestinationBufferSize(int align = 1) => GetBufferSize(fixedDestinationWidth, fixedDestinationHeight, fixedDestinationFormat, align);

    /// <summary>Gets the line size of a plane of the source image of a fixed context.</summary>
    /// <param name="plane">The plane index of the image (e.g., 0 for Y plane in YUV format).</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the line in bytes.</returns>
    [Obsolete(FixedProperty + " Use ImageInfo.GetLineSize.")]
    public int GetSourceLineSize(int plane, int align = 1) => GetLineSize(fixedSourceWidth, fixedSourceFormat, plane, align);

    /// <summary>Gets the line size of a plane of the destination image of a fixed context.</summary>
    /// <param name="plane">The plane index of the image.</param>
    /// <param name="align">Optional memory alignment, default is 1.</param>
    /// <returns>The size of the line in bytes.</returns>
    [Obsolete(FixedProperty + " Use ImageInfo.GetLineSize.")]
    public int GetDestinationLineSize(int plane, int align = 1) => GetLineSize(fixedDestinationWidth, fixedDestinationFormat, plane, align);

    private void ThrowIfDynamic()
    {
        if (IsDynamic)
            throw new InvalidOperationException("This overload needs a fixed context, which knows the image sizes and formats. Use an overload with AVFrame, Image or ImageInfo.");
    }

    private void ThrowIfNotFixedFor(ImageInfo srcInfo, ImageInfo dstInfo)
    {
        if (srcInfo.Format != fixedSourceFormat || srcInfo.Width != fixedSourceWidth || srcInfo.Height != fixedSourceHeight)
            throw new ArgumentException($"This fixed context converts {fixedSourceWidth}x{fixedSourceHeight} {fixedSourceFormat} images.", nameof(srcInfo));
        if (dstInfo.Format != fixedDestinationFormat || dstInfo.Width != fixedDestinationWidth || dstInfo.Height != fixedDestinationHeight)
            throw new ArgumentException($"This fixed context converts to {fixedDestinationWidth}x{fixedDestinationHeight} {fixedDestinationFormat} images.", nameof(dstInfo));
    }

    private AVResult32 ConvertFixed(IntPtr src, IntPtr dst, int srcAlign, int dstAlign)
    {
        byte_ptrArray4 srcData = new();
        int_array4 srcLines = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref srcData, ref srcLines, (byte*)src, (_AVPixelFormat)fixedSourceFormat, fixedSourceWidth, fixedSourceHeight, srcAlign);
        if (res.IsError)
            return res;
        byte_ptrArray4 dstData = new();
        int_array4 dstLines = new();
        res = ffmpeg.av_image_fill_arrays(ref dstData, ref dstLines, (byte*)dst, (_AVPixelFormat)fixedDestinationFormat, fixedDestinationWidth, fixedDestinationHeight, dstAlign);
        return res.IsError
            ? res
            : (AVResult32)ffmpeg.sws_scale(Context, (byte**)&srcData, (int*)&srcLines, 0, fixedSourceHeight, (byte**)&dstData, (int*)&dstLines);
    }

    /// <summary>
    /// Converts a frame into a buffer laid out like the destination of this fixed context.
    /// </summary>
    /// <param name="src">The source frame; it must match the source of this context.</param>
    /// <param name="dst">The destination buffer.</param>
    /// <param name="dstAlign">Optional memory alignment for the destination buffer, default is 1.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(AVFrame src, IntPtr dst, int dstAlign = 1)
    {
        ThrowIfDynamic();
        if (src.Height != fixedSourceHeight || src.Width != fixedSourceWidth || (PixelFormat)src.Format != fixedSourceFormat)
            return AVResult32.InvalidArgument;
        byte_ptrArray8 srcData = src.Frame->data;
        int_array8 srcLines = src.LineSize;
        byte_ptrArray4 dstData = new();
        int_array4 dstLines = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref dstData, ref dstLines, (byte*)dst, (_AVPixelFormat)fixedDestinationFormat, fixedDestinationWidth, fixedDestinationHeight, dstAlign);
        return res.IsError
            ? res
            : (AVResult32)ffmpeg.sws_scale(Context, (byte**)&srcData, (int*)&srcLines, 0, fixedSourceHeight, (byte**)&dstData, (int*)&dstLines);
    }

    /// <summary>
    /// Converts a buffer laid out like the source of this fixed context into a frame.
    /// </summary>
    /// <param name="src">The source buffer.</param>
    /// <param name="dst">The destination frame; it must match the destination of this context.</param>
    /// <param name="srcAlign">Optional memory alignment for the source buffer, default is 1.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(IntPtr src, AVFrame dst, int srcAlign = 1)
    {
        ThrowIfDynamic();
        if (dst.Height != fixedDestinationHeight || dst.Width != fixedDestinationWidth || (PixelFormat)dst.Format != fixedDestinationFormat)
            return AVResult32.InvalidArgument;
        byte_ptrArray4 srcData = new();
        int_array4 srcLines = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref srcData, ref srcLines, (byte*)src, (_AVPixelFormat)fixedSourceFormat, fixedSourceWidth, fixedSourceHeight, srcAlign);
        if (res.IsError)
            return res;
        if (!dst.HasBuffer)
            dst.CreateBuffer().ThrowIfError();
        byte_ptrArray8 dstData = dst.Frame->data;
        int_array8 dstLines = dst.LineSize;
        return ffmpeg.sws_scale(Context, (byte**)&srcData, (int*)&srcLines, 0, fixedSourceHeight, (byte**)&dstData, (int*)&dstLines);
    }

    /// <summary>
    /// Converts a buffer laid out like the source of this fixed context into a buffer laid out like its destination.
    /// </summary>
    /// <param name="src">Pointer to the source buffer.</param>
    /// <param name="dst">Pointer to the destination buffer.</param>
    /// <param name="srcAlign">Optional memory alignment for the source buffer, default is 1.</param>
    /// <param name="dstAlign">Optional memory alignment for the destination buffer, default is 1.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(IntPtr src, IntPtr dst, int srcAlign = 1, int dstAlign = 1)
    {
        ThrowIfDynamic();
        return ConvertFixed(src, dst, srcAlign, dstAlign);
    }

    /// <summary>
    /// Converts a frame into a span laid out like the destination of this fixed context.
    /// </summary>
    /// <param name="src">The source frame; it must match the source of this context.</param>
    /// <param name="dst">The destination span.</param>
    /// <param name="dstAlign">Optional memory alignment for the destination buffer, default is 1.</param>
    /// <returns>A non-negative value on success, <see cref="AVResult32.InvalidArgument"/> if the span is too small.</returns>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(AVFrame src, Span<byte> dst, int dstAlign = 1)
    {
        ThrowIfDynamic();
        if (dst.Length < GetBufferSize(fixedDestinationWidth, fixedDestinationHeight, fixedDestinationFormat, dstAlign))
            return AVResult32.InvalidArgument;
        fixed (byte* dstPtr = dst)
            return Convert(src, (nint)dstPtr, dstAlign);
    }

    /// <summary>
    /// Converts a span laid out like the source of this fixed context into a frame.
    /// </summary>
    /// <param name="src">The source span.</param>
    /// <param name="dst">The destination frame; it must match the destination of this context.</param>
    /// <param name="srcAlign">Optional memory alignment for the source buffer, default is 1.</param>
    /// <returns>A non-negative value on success, <see cref="AVResult32.InvalidArgument"/> if the span is too small.</returns>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(Span<byte> src, AVFrame dst, int srcAlign = 1)
    {
        ThrowIfDynamic();
        if (src.Length < GetBufferSize(fixedSourceWidth, fixedSourceHeight, fixedSourceFormat, srcAlign))
            return AVResult32.InvalidArgument;
        fixed (byte* srcPtr = src)
            return Convert((nint)srcPtr, dst, srcAlign);
    }

    /// <summary>
    /// Converts source planes into destination planes laid out like the source and destination of this fixed context.
    /// </summary>
    /// <param name="srcPlanes">The source planes.</param>
    /// <param name="srcStride">The line size of each source plane.</param>
    /// <param name="dstPlanes">The destination planes.</param>
    /// <param name="dstStride">The line size of each destination plane.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="ArgumentException">Fewer planes or strides than the pixel formats need.</exception>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(ReadOnlySpan<IntPtr> srcPlanes, ReadOnlySpan<int> srcStride, ReadOnlySpan<IntPtr> dstPlanes, ReadOnlySpan<int> dstStride)
    {
        ThrowIfDynamic();
        int srcPlaneCount = ffmpeg.av_pix_fmt_count_planes((_AVPixelFormat)fixedSourceFormat);
        int dstPlaneCount = ffmpeg.av_pix_fmt_count_planes((_AVPixelFormat)fixedDestinationFormat);
        if (srcPlanes.Length < srcPlaneCount || srcStride.Length < srcPlaneCount)
            throw new ArgumentException($"{fixedSourceFormat} needs {srcPlaneCount} planes and strides.", nameof(srcPlanes));
        if (dstPlanes.Length < dstPlaneCount || dstStride.Length < dstPlaneCount)
            throw new ArgumentException($"{fixedDestinationFormat} needs {dstPlaneCount} planes and strides.", nameof(dstPlanes));

        fixed (void* srcPlanes_ptr = srcPlanes, srcLineSize_ptr = srcStride, dstPlanes_ptr = dstPlanes, dstLineSize_ptr = dstStride)
            return ffmpeg.sws_scale(Context, (byte**)srcPlanes_ptr, (int*)srcLineSize_ptr, 0, fixedSourceHeight, (byte**)dstPlanes_ptr, (int*)dstLineSize_ptr);
    }

    /// <summary>
    /// Converts a frame into destination planes laid out like the destination of this fixed context.
    /// </summary>
    /// <param name="frame">The source frame; its pixel format must match the source of this context.</param>
    /// <param name="dstPlanes">The destination planes.</param>
    /// <param name="dstStride">The line size of each destination plane.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="ArgumentException">Another pixel format, or fewer planes or strides than the format needs.</exception>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(AVFrame frame, ReadOnlySpan<IntPtr> dstPlanes, ReadOnlySpan<int> dstStride)
    {
        ThrowIfDynamic();
        int dstPlaneCount = ffmpeg.av_pix_fmt_count_planes((_AVPixelFormat)fixedDestinationFormat);
        if (frame.PixelFormat != fixedSourceFormat)
            throw new ArgumentException($"This fixed context converts {fixedSourceFormat} frames.", nameof(frame));
        if (dstPlanes.Length < dstPlaneCount || dstStride.Length < dstPlaneCount)
            throw new ArgumentException($"{fixedDestinationFormat} needs {dstPlaneCount} planes and strides.", nameof(dstPlanes));

        int_array8* srcLineSize_ptr = &frame.Frame->linesize;
        byte** srcPlanes_ptr = frame.Frame->extended_data;

        fixed (void* dstPlanes_ptr = dstPlanes, dstLineSize_ptr = dstStride)
            return ffmpeg.sws_scale(Context, srcPlanes_ptr, (int*)srcLineSize_ptr, 0, fixedSourceHeight, (byte**)dstPlanes_ptr, (int*)dstLineSize_ptr);
    }

    /// <summary>
    /// Converts source planes laid out like the source of this fixed context into a frame.
    /// </summary>
    /// <param name="srcPlanes">The source planes.</param>
    /// <param name="srcStride">The line size of each source plane.</param>
    /// <param name="frame">The destination frame; it must match the destination of this context.</param>
    /// <returns>A non-negative value on success, <see cref="AVResult32.InvalidArgument"/> for another frame size.</returns>
    /// <exception cref="ArgumentException">Another pixel format, or fewer planes or strides than the format needs.</exception>
    /// <exception cref="InvalidOperationException">The context is dynamic.</exception>
    [Obsolete(FixedConvert)]
    public AVResult32 Convert(ReadOnlySpan<IntPtr> srcPlanes, ReadOnlySpan<int> srcStride, AVFrame frame)
    {
        ThrowIfDynamic();
        int srcPlaneCount = ffmpeg.av_pix_fmt_count_planes((_AVPixelFormat)fixedSourceFormat);
        if (frame.PixelFormat != fixedDestinationFormat)
            throw new ArgumentException($"This fixed context converts to {fixedDestinationFormat} frames.", nameof(frame));
        if (srcPlanes.Length < srcPlaneCount || srcStride.Length < srcPlaneCount)
            throw new ArgumentException($"{fixedSourceFormat} needs {srcPlaneCount} planes and strides.", nameof(srcPlanes));
        if (frame.Height != fixedDestinationHeight || frame.Width != fixedDestinationWidth)
            return AVResult32.InvalidArgument;

        if (!frame.HasBuffer)
            frame.CreateBuffer().ThrowIfError();

        int_array8* dstLineSize_ptr = &frame.Frame->linesize;
        byte** dstPlanes_ptr = frame.Frame->extended_data;

        fixed (void* srcPlanes_ptr = srcPlanes, srcLineSize_ptr = srcStride)
            return ffmpeg.sws_scale(Context, (byte**)srcPlanes_ptr, (int*)srcLineSize_ptr, 0, fixedSourceHeight, dstPlanes_ptr, (int*)dstLineSize_ptr);
    }
}
