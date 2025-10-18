using FFmpeg.Collections;
using FFmpeg.Utils;

namespace FFmpeg.Images;

/// <summary>
/// Represents an image stored in consecutive memory with utilities for handling image data and memory buffers.
/// </summary>
public sealed unsafe class Image : IDisposable
{
    private AVBuffer? buffer = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class with the specified <see cref="ImageInfo"/> and memory buffer.
    /// </summary>
    /// <param name="info">Information about the image such as dimensions, pixel format, and alignment.</param>
    /// <param name="buffer">The buffer that contains the image data.</param>
    private Image(ImageInfo info, AVBuffer buffer)
    {
        Info = info;
        this.buffer = buffer;
        Data = buffer.Data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class with the specified <see cref="ImageInfo"/> and data pointer.
    /// </summary>
    /// <param name="info">Information about the image such as dimensions, pixel format, and alignment.</param>
    /// <param name="data">A pointer to the raw image data.</param>
    private Image(ImageInfo info, IntPtr data)
    {
        Info = info;
        Data = data;
    }

    /// <summary>
    /// Gets the information about the image, such as its dimensions, format, and alignment.
    /// </summary>
    public ImageInfo Info { get; }

    /// <summary>
    /// Gets the pixel format of the image.
    /// </summary>
    public PixelFormat PixelFormat => Info.Format;

    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    public int Width => Info.Width;

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    public int Height => Info.Height;

    /// <summary>
    /// Gets the pointer to the raw image data.
    /// </summary>
    public IntPtr Data { get; }

    /// <summary>
    /// Retrieves the line sizes for each plane of the image and stores them in the provided span.
    /// </summary>
    /// <param name="lineSizes">The span to hold the line sizes for each plane of the image.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> result indicating success or failure. If successful, the number of planes is returned.
    /// </returns>
    public AVResult32 GetLineSize(Span<int> lineSizes)
    {
        int planes = Info.Planes;
        if (planes > lineSizes.Length)
            return AVResult32.InvalidArgument;
        for (int i = 0; i < planes; i++)
            lineSizes[i] = Info.GetLineSize(i);
        return planes;
    }

    /// <summary>
    /// Retrieves the pointers to each plane and their respective line sizes.
    /// </summary>
    /// <param name="planePtrs">The span to hold pointers to each plane of the image.</param>
    /// <param name="lineSizes">The span to hold the line sizes for each plane of the image.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> result indicating success or failure. If successful, the number of planes is returned.
    /// </returns>
    public AVResult32 GetPlanes(Span<IntPtr> planePtrs, Span<int> lineSizes)
    {
        int planes = Info.Planes;
        if (planes > planePtrs.Length || planes > lineSizes.Length)
            return AVResult32.InvalidArgument;
        AutoGen.byte_ptrArray4 ptrArray = new();
        AutoGen.int_array4 lineArray = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref ptrArray, ref lineArray, (byte*)Data, (AutoGen._AVPixelFormat)Info.Format, Width, Height, Info.Alignment);
        if (res.IsError)
            return res;
        for (int i = 0; i < planes; i++)
        {
            planePtrs[i] = (IntPtr)ptrArray[(uint)i];
            lineSizes[i] = lineArray[(uint)i];
        }
        return planes;
    }

    /// <summary>
    /// Retrieves the pointers to each plane of the image.
    /// </summary>
    /// <param name="planePtrs">The span to hold pointers to each plane of the image.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> result indicating success or failure. If successful, the number of planes is returned.
    /// </returns>
    public AVResult32 GetPlanePointers(Span<IntPtr> planePtrs)
    {
        int planes = Info.Planes;
        if (planes > planePtrs.Length)
            return AVResult32.InvalidArgument;
        AutoGen.byte_ptrArray4 ptrArray = new();
        AutoGen.int_array4 lineArray = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref ptrArray, ref lineArray, (byte*)Data, (AutoGen._AVPixelFormat)Info.Format, Width, Height, Info.Alignment);
        if (res.IsError)
            return res;
        for (int i = 0; i < planes; i++)
            planePtrs[i] = (IntPtr)ptrArray[(uint)i];
        return planes;
    }

    /// <summary>
    /// Creates a new <see cref="Image"/> with an allocated buffer based on the specified <see cref="ImageInfo"/>.
    /// </summary>
    /// <param name="info">The information used to describe the image.</param>
    /// <returns>A new <see cref="Image"/> with an allocated buffer.</returns>
    public static Image Create(ImageInfo info)
    {
        AVBuffer buffer = AVBuffer.AllocateZ(info.BufferSize);
        return new Image(info, buffer);
    }

    /// <summary>
    /// Creates a new <see cref="Image"/> by copying pixel data from the specified pointer.
    /// </summary>
    /// <param name="info">The image information such as dimensions and pixel format.</param>
    /// <param name="data">A pointer to the source pixel data to be copied.</param>
    /// <returns>A new <see cref="Image"/> with copied pixel data.</returns>
    public static Image FromPixelCopy(ImageInfo info, IntPtr data)
    {
        Image image = Create(info);
        Buffer.MemoryCopy((void*)data, (void*)image.Data, info.BufferSize, info.BufferSize);
        return image;
    }

    /// <summary>
    /// Creates a new <see cref="Image"/> from an <see cref="AVBuffer"/> containing pixel data.
    /// </summary>
    /// <param name="info">The image information such as dimensions and pixel format.</param>
    /// <param name="data">The buffer that contains the pixel data.</param>
    /// <returns>A new <see cref="Image"/> with the specified pixel data.</returns>
    public static Image FromPixels(ImageInfo info, AVBuffer data)
    {
        AVBuffer buffer = data.Clone();
        return new Image(info, buffer);
    }

    /// <summary>
    /// Creates a new <see cref="Image"/> using the provided pixel data pointer.
    /// </summary>
    /// <param name="info">The image information such as dimensions and pixel format.</param>
    /// <param name="data">A pointer to the pixel data.</param>
    /// <returns>A new <see cref="Image"/> using the specified pixel data pointer.</returns>
    public static Image FromPixels(ImageInfo info, IntPtr data) => new(info, data);

    /// <summary>
    /// Converts the current image to a different pixel format and returns the result as a new <see cref="Image"/>.
    /// </summary>
    /// <param name="dstFormat">The desired destination format for the conversion.</param>
    /// <returns>A new <see cref="Image"/> in the specified format.</returns>
    /// <exception cref="AVException">Thrown if the conversion fails.</exception>
    public Image ConvertTo(ImageInfo dstFormat)
    {
        Image img = Create(dstFormat);
        AVResult32 res = SwsContext.Convert(this, img, SwsAlgorithm.Bicubic());
        if (res.IsError)
        {
            img.Dispose();
            res.ThrowIfError();
        }
        return img;
    }

    /// <summary>
    /// Releases the resources used by the <see cref="Image"/>, including the underlying buffer.
    /// </summary>
    public void Dispose()
    {
        buffer?.Dispose();
        buffer = null;
    }
}
