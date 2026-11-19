using FFmpeg.AutoGen;
using FFmpeg.Unsafe;
using FFmpeg.Utils;

namespace FFmpeg.Images;

/// <summary>
/// Scales images and converts their pixel format and colors (libswscale).
/// </summary>
/// <remarks>
/// <para>
/// <b>Dynamic contexts</b> (recommended by FFmpeg): created with <see cref="SwsContext()"/>,
/// <see cref="SwsContext(SwsAlgorithm)"/> or <see cref="Allocate"/>. They are not tied to an image size or format:
/// every <c>Convert</c> call describes both images (<see cref="AVFrame"/>, <see cref="Image"/> or
/// <see cref="ImageInfo"/>), and FFmpeg sets up its conversion on first use and only sets it up again when the
/// images or the settings change, so one context can serve a whole stream even if its format changes.
/// The settings (<see cref="Scaler"/>, <see cref="Flags"/>, <see cref="Threads"/>, <see cref="Dither"/>,
/// <see cref="Intent"/>, ...) can be changed at any time.
/// </para>
/// <para>
/// Frames are converted with their color properties: the YUV matrix (<see cref="AVFrame.ColorSpace"/>), the range,
/// the chroma location and, if they differ, the primaries and transfer function (e.g. HDR to SDR tone mapping, see
/// <see cref="Intent"/>). Properties the destination frame leaves unspecified are taken from the source and written
/// to the destination frame, as FFmpeg's <c>scale</c> filter does, so resizing or changing the pixel format keeps
/// the colors; set them on the destination frame to convert the colors. Raw buffers (<see cref="Image"/>,
/// <see cref="ImageInfo"/>) carry no color properties, so YUV is treated as BT.601 limited range there.
/// </para>
/// <para>
/// <b>Fixed contexts</b> (FFmpeg's legacy API, <c>sws_getContext</c>): created with the obsolete
/// <see cref="SwsContext(int, int, PixelFormat, int, int, PixelFormat, SwsAlgorithm)"/>. They only convert the size
/// and pixel formats they were created for, ignore the color properties of frames (BT.601 limited range for YUV) and
/// their settings cannot be changed.
/// </para>
/// <para>A context is not thread safe; use one context per thread.</para>
/// </remarks>
public sealed unsafe partial class SwsContext : Options.OptionQueryableBase, IDisposable, IAVPointer<_SwsContext>
{
    /// <summary>The scaler selection bits of <see cref="SwsFlags"/> (<c>SWS_FAST_BILINEAR</c> to <c>SWS_SPLINE</c>).</summary>
    private const SwsFlags ScalerFlagsMask = (SwsFlags)0x7FF;

    private _SwsContext* context;

    // Frames that describe raw buffers (Image, IntPtr + ImageInfo) for sws_scale_frame; allocated on first use.
    private _AVFrame* sourceView;
    private _AVFrame* destinationView;

    // Configuration of fixed (legacy) contexts.
    private readonly int fixedSourceWidth, fixedSourceHeight, fixedDestinationWidth, fixedDestinationHeight;
    private readonly PixelFormat fixedSourceFormat = PixelFormat.None, fixedDestinationFormat = PixelFormat.None;
    private readonly SwsAlgorithm fixedAlgorithm;

    unsafe _SwsContext* IAVPointer<_SwsContext>.Pointer => context;

    /// <summary>
    /// Provides a pointer to the underlying FFmpeg scaling context.
    /// </summary>
    protected override unsafe void* Pointer => context;

    private _SwsContext* Context => context != null ? context : throw new ObjectDisposedException(nameof(SwsContext));

    #region Creation

    /// <summary>
    /// Creates a dynamic context with FFmpeg's default settings (bicubic scaling).
    /// </summary>
    /// <exception cref="OutOfMemoryException">The context could not be allocated.</exception>
    public SwsContext()
    {
        context = ffmpeg.sws_alloc_context();
        if (context == null)
            throw new OutOfMemoryException("Failed to allocate the scaler context.");
        IsDynamic = true;
    }

    /// <summary>
    /// Creates a dynamic context that scales with <paramref name="algorithm"/>.
    /// </summary>
    /// <param name="algorithm">The scaling algorithm and its parameters.</param>
    /// <exception cref="OutOfMemoryException">The context could not be allocated.</exception>
    public SwsContext(SwsAlgorithm algorithm) : this() => Algorithm = algorithm;

    /// <summary>
    /// Creates a dynamic context with FFmpeg's default settings; the same as <see cref="SwsContext()"/>.
    /// </summary>
    /// <returns>A new dynamic context.</returns>
    public static SwsContext Allocate() => new();

    /// <summary>
    /// Creates a fixed context for one source and destination size and pixel format (FFmpeg's legacy
    /// <c>sws_getContext</c>).
    /// </summary>
    /// <remarks>
    /// A fixed context ignores the color properties of frames (YUV is treated as BT.601 limited range) and its
    /// settings cannot be changed. Use a dynamic context (<see cref="SwsContext(SwsAlgorithm)"/>) instead; it adapts
    /// to every image and handles color properties.
    /// </remarks>
    /// <param name="srcW">The width of the source image, in pixels.</param>
    /// <param name="srcH">The height of the source image, in pixels.</param>
    /// <param name="srcFormat">The pixel format of the source image.</param>
    /// <param name="dstW">The width of the destination image, in pixels.</param>
    /// <param name="dstH">The height of the destination image, in pixels.</param>
    /// <param name="dstFormat">The pixel format of the destination image.</param>
    /// <param name="algorithm">The scaling algorithm and associated parameters to use.</param>
    /// <exception cref="ArgumentException">FFmpeg cannot convert between these formats or sizes.</exception>
    [Obsolete("Fixed contexts use FFmpeg's legacy API. Use new SwsContext(algorithm), which adapts to every image and handles color properties.")]
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
        // sws_getContext returns NULL for unsupported formats or invalid sizes; without this
        // check the first Convert would pass NULL to sws_scale and crash the process.
        if (context == null)
            throw new ArgumentException($"Cannot create a scaler from {srcW}x{srcH} {srcFormat} to {dstW}x{dstH} {dstFormat}.");
        fixedSourceWidth = srcW;
        fixedSourceHeight = srcH;
        fixedSourceFormat = srcFormat;
        fixedDestinationWidth = dstW;
        fixedDestinationHeight = dstH;
        fixedDestinationFormat = dstFormat;
        fixedAlgorithm = algorithm;
    }

    /// <summary>
    /// Gets whether this is a dynamic context (FFmpeg's current API), which adapts to every image; otherwise it is a
    /// fixed context created with the obsolete
    /// <see cref="SwsContext(int, int, PixelFormat, int, int, PixelFormat, SwsAlgorithm)"/>.
    /// </summary>
    public bool IsDynamic { get; }

    #endregion

    #region Settings

    private void ThrowIfFixed()
    {
        if (!IsDynamic)
            throw new InvalidOperationException("The settings of a fixed context cannot be changed; FFmpeg only uses them in dynamic contexts (new SwsContext()).");
    }

    /// <summary>
    /// Gets or sets the scaling algorithm: the scaler bits of <see cref="Flags"/> and
    /// <see cref="ScalerParam1"/>/<see cref="ScalerParam2"/>.
    /// </summary>
    /// <remarks>A non-<see cref="SwsScaler.Auto"/> <see cref="Scaler"/> takes precedence.</remarks>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsAlgorithm Algorithm
    {
        get => IsDynamic ? new SwsAlgorithm(Flags & ScalerFlagsMask, ScalerParam1, ScalerParam2) : fixedAlgorithm;
        set
        {
            ThrowIfFixed();
            Flags = (Flags & ~ScalerFlagsMask) | value.AlgorithmFlags;
            ScalerParam1 = value.Param1;
            ScalerParam2 = value.Param2;
        }
    }

    /// <summary>
    /// Gets or sets the flags (<c>sws_flags</c>); <see cref="SwsFlags.Bicubic"/> by default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsFlags Flags
    {
        get => (SwsFlags)Context->flags;
        set
        {
            ThrowIfFixed();
            Context->flags = (uint)value;
        }
    }

    /// <summary>
    /// Gets or sets the scaling filter (<c>scaler</c>). <see cref="SwsScaler.Auto"/> (the default) uses the scaler
    /// selected by <see cref="Flags"/>/<see cref="Algorithm"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsScaler Scaler
    {
        get => (SwsScaler)Context->scaler;
        set
        {
            ThrowIfFixed();
            Context->scaler = (_SwsScaler)value;
        }
    }

    /// <summary>
    /// Gets or sets the filter used to up- and downsample subsampled chroma planes (<c>scaler_sub</c>).
    /// <see cref="SwsScaler.Auto"/> (the default) uses the same filter as <see cref="Scaler"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsScaler ChromaScaler
    {
        get => (SwsScaler)Context->scaler_sub;
        set
        {
            ThrowIfFixed();
            Context->scaler_sub = (_SwsScaler)value;
        }
    }

    /// <summary>
    /// Gets or sets the first tuning parameter of the scaler (<c>param0</c>): B for bicubic, the exponent for
    /// Gaussian, the window width for Lanczos. <see cref="SwsAlgorithm.DEFAULT_PARAM"/> uses the default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public double ScalerParam1
    {
        get => Context->scaler_params[0];
        set
        {
            ThrowIfFixed();
            Context->scaler_params[0] = value;
        }
    }

    /// <summary>
    /// Gets or sets the second tuning parameter of the scaler (<c>param1</c>): C for bicubic.
    /// <see cref="SwsAlgorithm.DEFAULT_PARAM"/> uses the default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public double ScalerParam2
    {
        get => Context->scaler_params[1];
        set
        {
            ThrowIfFixed();
            Context->scaler_params[1] = value;
        }
    }

    /// <summary>
    /// Gets or sets the number of threads (<c>threads</c>); 0 selects automatically. FFmpeg's default is 1.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public int Threads
    {
        get => Context->threads;
        set
        {
            ThrowIfFixed();
            Context->threads = value;
        }
    }

    /// <summary>
    /// Gets or sets the dither mode (<c>sws_dither</c>); <see cref="SwsDither.Auto"/> by default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsDither Dither
    {
        get => (SwsDither)Context->dither;
        set
        {
            ThrowIfFixed();
            Context->dither = (_SwsDither)value;
        }
    }

    /// <summary>
    /// Gets or sets how alpha is handled when converting to a format without alpha (<c>alphablend</c>);
    /// <see cref="SwsAlphaBlend.None"/> by default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsAlphaBlend AlphaBlend
    {
        get => (SwsAlphaBlend)Context->alpha_blend;
        set
        {
            ThrowIfFixed();
            Context->alpha_blend = (_SwsAlphaBlend)value;
        }
    }

    /// <summary>
    /// Gets or sets whether scaling is done in linear light (<c>gamma</c>).
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public bool GammaCorrect
    {
        get => Context->gamma_flag != 0;
        set
        {
            ThrowIfFixed();
            Context->gamma_flag = value ? 1 : 0;
        }
    }

    /// <summary>
    /// Gets or sets the rendering intent used when the source and destination have different color primaries or
    /// transfer functions, e.g. HDR to SDR (<c>intent</c>); <see cref="SwsIntent.RelativeColorimetric"/> by default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsIntent Intent
    {
        get => (SwsIntent)Context->intent;
        set
        {
            ThrowIfFixed();
            Context->intent = (int)value;
        }
    }

    /// <summary>
    /// Gets or sets the implementations that may be used (<c>sws_backends</c>); <see cref="SwsBackend.Auto"/> by
    /// default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Setting it on a fixed context.</exception>
    public SwsBackend Backends
    {
        get => (SwsBackend)Context->backends;
        set
        {
            ThrowIfFixed();
            Context->backends = (_SwsBackend)value;
        }
    }

    #endregion

    #region Capabilities

    /// <summary>Gets whether FFmpeg can read images in <paramref name="format"/>.</summary>
    /// <param name="format">A software pixel format.</param>
    /// <returns><see langword="true"/> if a stable backend supports it as input.</returns>
    public static bool IsSupportedInput(PixelFormat format) => ffmpeg.sws_test_format((_AVPixelFormat)format, 0) > 0;

    /// <summary>Gets whether FFmpeg can write images in <paramref name="format"/>.</summary>
    /// <param name="format">A software pixel format.</param>
    /// <returns><see langword="true"/> if a stable backend supports it as output.</returns>
    public static bool IsSupportedOutput(PixelFormat format) => ffmpeg.sws_test_format((_AVPixelFormat)format, 1) > 0;

    /// <summary>Gets whether a YUV matrix is supported.</summary>
    /// <param name="colorSpace">The matrix.</param>
    /// <param name="output"><see langword="true"/> to test the destination, <see langword="false"/> for the source.</param>
    /// <returns><see langword="true"/> if it is supported.</returns>
    public static bool IsSupported(ColorSpace colorSpace, bool output) => ffmpeg.sws_test_colorspace((_AVColorSpace)colorSpace, output ? 1 : 0) > 0;

    /// <summary>Gets whether a set of color primaries is supported.</summary>
    /// <param name="primaries">The primaries.</param>
    /// <param name="output"><see langword="true"/> to test the destination, <see langword="false"/> for the source.</param>
    /// <returns><see langword="true"/> if they are supported.</returns>
    public static bool IsSupported(ColorPrimaries primaries, bool output) => ffmpeg.sws_test_primaries((_AVColorPrimaries)primaries, output ? 1 : 0) > 0;

    /// <summary>Gets whether a transfer function is supported.</summary>
    /// <param name="transfer">The transfer function.</param>
    /// <param name="output"><see langword="true"/> to test the destination, <see langword="false"/> for the source.</param>
    /// <returns><see langword="true"/> if it is supported.</returns>
    public static bool IsSupported(ColorTransferCharacteristic transfer, bool output) => ffmpeg.sws_test_transfer((_AVColorTransferCharacteristic)transfer, output ? 1 : 0) > 0;

    /// <summary>Gets whether a frame (format, size and color properties) can be converted.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="output"><see langword="true"/> to test it as destination, <see langword="false"/> as source.</param>
    /// <returns><see langword="true"/> if it is supported.</returns>
    public static bool IsSupported(AVFrame frame, bool output) => ffmpeg.sws_test_frame(frame.Frame, output ? 1 : 0) > 0;

    /// <summary>
    /// Gets whether converting <paramref name="src"/> to <paramref name="dst"/> with a dynamic context would not
    /// change anything (same size, format and color properties).
    /// </summary>
    /// <remarks>
    /// Like <see cref="Convert(AVFrame, AVFrame)"/>, color properties <paramref name="dst"/> leaves unspecified count
    /// as those of <paramref name="src"/>; <paramref name="dst"/> is not changed.
    /// </remarks>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination frame.</param>
    /// <returns><see langword="true"/> if the conversion is a no-op.</returns>
    public static bool IsNoOp(AVFrame src, AVFrame dst)
    {
        _AVFrame* d = dst.Frame;
        if (!HasImage(src.Frame) || !HasImage(d))
            return false;
        (_AVColorPrimaries primaries, _AVColorTransferCharacteristic trc, _AVColorSpace colorSpace, _AVColorRange range, _AVChromaLocation location)
            = (d->color_primaries, d->color_trc, d->colorspace, d->color_range, d->chroma_location);
        try
        {
            InheritColorProperties(src.Frame, d);
            return ffmpeg.sws_is_noop(d, src.Frame) > 0;
        }
        finally
        {
            (d->color_primaries, d->color_trc, d->colorspace, d->color_range, d->chroma_location) = (primaries, trc, colorSpace, range, location);
        }
    }

    #endregion

    #region Frame conversion

    /// <summary>
    /// Converts <paramref name="src"/> into <paramref name="dst"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <paramref name="dst"/> must have its <see cref="AVFrame.Width"/>, <see cref="AVFrame.Height"/> and
    /// <see cref="AVFrame.PixelFormat"/> set. If it has no buffer, FFmpeg allocates one; if the conversion is a no-op
    /// (see <see cref="IsNoOp"/>), <paramref name="dst"/> then references the buffers of <paramref name="src"/>
    /// instead of copying them, so make it writable before changing its pixels.
    /// </para>
    /// <para>
    /// Dynamic contexts copy the color properties <paramref name="dst"/> leaves unspecified from
    /// <paramref name="src"/> (see the class remarks). If only one of the frames is marked interlaced, both are
    /// scaled as progressive frames, like FFmpeg's <c>scale</c> filter does by default; mark both as interlaced to
    /// scale the fields separately.
    /// </para>
    /// </remarks>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination frame.</param>
    /// <returns>A non-negative value on success, <see cref="AVResult32.InvalidArgument"/> if a frame has no size or
    /// format, or the FFmpeg error (e.g. ENOTSUP for an unsupported conversion).</returns>
    /// <seealso cref="Setup(AVFrame, AVFrame)"/>
    public AVResult32 Convert(AVFrame src, AVFrame dst)
        => IsDynamic ? ScaleFrame(src.Frame, dst.Frame, setupOnly: false) : ffmpeg.sws_scale_frame(Context, dst.Frame, src.Frame);

    /// <summary>
    /// Prepares the conversion from <paramref name="src"/> to <paramref name="dst"/> without converting, e.g. to
    /// check that it is supported or to do the setup work in advance. The buffers of the frames are not used.
    /// </summary>
    /// <param name="src">The source frame (size, format and color properties).</param>
    /// <param name="dst">The destination frame (size, format and color properties); unspecified color properties are
    /// copied from <paramref name="src"/> as in <see cref="Convert(AVFrame, AVFrame)"/>.</param>
    /// <returns>A non-negative value on success, otherwise the error the conversion would fail with.</returns>
    /// <exception cref="InvalidOperationException">The context is a fixed context.</exception>
    public AVResult32 Setup(AVFrame src, AVFrame dst)
        => IsDynamic
            ? ScaleFrame(src.Frame, dst.Frame, setupOnly: true)
            : throw new InvalidOperationException("Setup needs a dynamic context (new SwsContext()).");

    private AVResult32 ScaleFrame(_AVFrame* src, _AVFrame* dst, bool setupOnly)
    {
        // FFmpeg asserts (aborts the process) on frames without a valid format.
        if (!HasImage(src) || !HasImage(dst))
            return AVResult32.InvalidArgument;

        InheritColorProperties(src, dst);

        int srcFlags = src->flags, dstFlags = dst->flags;
        bool interlaceMismatch = ((srcFlags ^ dstFlags) & ffmpeg.AV_FRAME_FLAG_INTERLACED) != 0;
        if (interlaceMismatch)
        {
            src->flags &= ~ffmpeg.AV_FRAME_FLAG_INTERLACED;
            dst->flags &= ~ffmpeg.AV_FRAME_FLAG_INTERLACED;
        }
        try
        {
            return setupOnly ? ffmpeg.sws_frame_setup(Context, dst, src) : ffmpeg.sws_scale_frame(Context, dst, src);
        }
        finally
        {
            if (interlaceMismatch)
            {
                src->flags = srcFlags;
                dst->flags = dstFlags;
            }
        }
    }

    private static bool HasImage(_AVFrame* frame)
        => frame->width > 0 && frame->height > 0 && ffmpeg.av_pix_fmt_desc_get((_AVPixelFormat)frame->format) != null;

    /// <summary>
    /// Copies the color properties <paramref name="dst"/> leaves unspecified from <paramref name="src"/>, like
    /// FFmpeg's scale filter. Without this, swscale would treat them as BT.601/limited/BT.709 defaults and convert
    /// the colors (e.g. tone map HDR to SDR when only resizing).
    /// </summary>
    private static void InheritColorProperties(_AVFrame* src, _AVFrame* dst)
    {
        if (dst->color_primaries == _AVColorPrimaries.AVCOL_PRI_UNSPECIFIED)
            dst->color_primaries = src->color_primaries;
        if (dst->color_trc == _AVColorTransferCharacteristic.AVCOL_TRC_UNSPECIFIED)
            dst->color_trc = src->color_trc;

        _AVPixFmtDescriptor* srcDesc = ffmpeg.av_pix_fmt_desc_get((_AVPixelFormat)src->format);
        _AVPixFmtDescriptor* dstDesc = ffmpeg.av_pix_fmt_desc_get((_AVPixelFormat)dst->format);
        if (IsYuv(srcDesc) && IsYuv(dstDesc))
        {
            if (dst->colorspace == _AVColorSpace.AVCOL_SPC_UNSPECIFIED)
                dst->colorspace = src->colorspace;
            if (dst->color_range == _AVColorRange.AVCOL_RANGE_UNSPECIFIED)
                dst->color_range = src->color_range;
        }
        if (dst->chroma_location == _AVChromaLocation.AVCHROMA_LOC_UNSPECIFIED && IsSubsampled(srcDesc) && IsSubsampled(dstDesc))
            dst->chroma_location = src->chroma_location;
    }

    private static bool IsYuv(_AVPixFmtDescriptor* desc)
    {
        const ulong NotYuv = ffmpeg.AV_PIX_FMT_FLAG_RGB | ffmpeg.AV_PIX_FMT_FLAG_PAL | ffmpeg.AV_PIX_FMT_FLAG_BAYER
            | ffmpeg.AV_PIX_FMT_FLAG_XYZ | ffmpeg.AV_PIX_FMT_FLAG_HWACCEL;
        return desc != null && (desc->flags & NotYuv) == 0 && desc->nb_components >= 3;
    }

    private static bool IsSubsampled(_AVPixFmtDescriptor* desc)
        => desc != null && (desc->log2_chroma_w != 0 || desc->log2_chroma_h != 0);

    #endregion

    #region Buffer conversion (dynamic contexts)

    /// <summary>Points <paramref name="view"/> at a raw buffer laid out as <paramref name="info"/> describes.</summary>
    private AVResult32 Describe(ref _AVFrame* view, IntPtr data, ImageInfo info)
    {
        if (view == null)
        {
            view = ffmpeg.av_frame_alloc();
            if (view == null)
                throw new OutOfMemoryException("Failed to allocate a frame.");
        }
        else
        {
            ffmpeg.av_frame_unref(view);
        }
        if (data == IntPtr.Zero)
            return AVResult32.InvalidArgument;
        view->width = info.Width;
        view->height = info.Height;
        view->format = (int)info.Format;
        byte_ptrArray4 planes = new();
        int_array4 lines = new();
        AVResult32 res = ffmpeg.av_image_fill_arrays(ref planes, ref lines, (byte*)data, (_AVPixelFormat)info.Format, info.Width, info.Height, info.Alignment);
        if (res.IsError)
            return res;
        for (uint i = 0; i < 4; i++)
        {
            view->data[i] = planes[i];
            view->linesize[i] = lines[i];
        }
        return res;
    }

    private void ThrowIfNotDynamic(string method)
    {
        if (!IsDynamic)
            throw new InvalidOperationException($"{method} needs a dynamic context (new SwsContext()); fixed contexts only convert the sizes and formats they were created for.");
    }

    private AVResult32 ConvertViews(_AVFrame* src, _AVFrame* dst)
    {
        try
        {
            return ScaleFrame(src, dst, setupOnly: false);
        }
        finally
        {
            if (sourceView != null)
                ffmpeg.av_frame_unref(sourceView);
            if (destinationView != null)
                ffmpeg.av_frame_unref(destinationView);
        }
    }

    /// <summary>
    /// Converts a raw source buffer into a raw destination buffer.
    /// </summary>
    /// <param name="src">The source pixels, laid out as <paramref name="srcInfo"/> describes.</param>
    /// <param name="srcInfo">The size, format and alignment of the source.</param>
    /// <param name="dst">The destination buffer, at least <see cref="ImageInfo.BufferSize"/> bytes.</param>
    /// <param name="dstInfo">The size, format and alignment of the destination.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="ArgumentException">A fixed context was created for other sizes or formats.</exception>
    public AVResult32 Convert(IntPtr src, ImageInfo srcInfo, IntPtr dst, ImageInfo dstInfo)
    {
        if (!IsDynamic)
        {
            ThrowIfNotFixedFor(srcInfo, dstInfo);
            return ConvertFixed(src, dst, srcInfo.Alignment, dstInfo.Alignment);
        }
        AVResult32 res = Describe(ref sourceView, src, srcInfo);
        if (!res.IsError)
            res = Describe(ref destinationView, dst, dstInfo);
        return res.IsError ? res : ConvertViews(sourceView, destinationView);
    }

    /// <summary>
    /// Converts a frame into a raw destination buffer.
    /// </summary>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination buffer, at least <see cref="ImageInfo.BufferSize"/> bytes.</param>
    /// <param name="dstInfo">The size, format and alignment of the destination.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="InvalidOperationException">The context is a fixed context.</exception>
    public AVResult32 Convert(AVFrame src, IntPtr dst, ImageInfo dstInfo)
    {
        ThrowIfNotDynamic(nameof(Convert));
        AVResult32 res = Describe(ref destinationView, dst, dstInfo);
        return res.IsError ? res : ConvertViews(src.Frame, destinationView);
    }

    /// <summary>
    /// Converts a raw source buffer into a frame.
    /// </summary>
    /// <param name="src">The source pixels, laid out as <paramref name="srcInfo"/> describes.</param>
    /// <param name="srcInfo">The size, format and alignment of the source.</param>
    /// <param name="dst">The destination frame; see <see cref="Convert(AVFrame, AVFrame)"/>.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="InvalidOperationException">The context is a fixed context.</exception>
    public AVResult32 Convert(IntPtr src, ImageInfo srcInfo, AVFrame dst)
    {
        ThrowIfNotDynamic(nameof(Convert));
        AVResult32 res = Describe(ref sourceView, src, srcInfo);
        return res.IsError ? res : ConvertViews(sourceView, dst.Frame);
    }

    /// <summary>
    /// Converts a frame into a span.
    /// </summary>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination, at least <see cref="ImageInfo.BufferSize"/> bytes.</param>
    /// <param name="dstInfo">The size, format and alignment of the destination.</param>
    /// <returns>A non-negative value on success, <see cref="AVResult32.InvalidArgument"/> if
    /// <paramref name="dst"/> is too small, otherwise the FFmpeg error.</returns>
    /// <exception cref="InvalidOperationException">The context is a fixed context.</exception>
    public AVResult32 Convert(AVFrame src, Span<byte> dst, ImageInfo dstInfo)
    {
        int size = dstInfo.BufferSize;
        if (size < 0 || dst.Length < size)
            return AVResult32.InvalidArgument;
        fixed (byte* dstPtr = dst)
            return Convert(src, (IntPtr)dstPtr, dstInfo);
    }

    /// <summary>
    /// Converts a span into a frame.
    /// </summary>
    /// <param name="src">The source pixels, at least <see cref="ImageInfo.BufferSize"/> bytes.</param>
    /// <param name="srcInfo">The size, format and alignment of the source.</param>
    /// <param name="dst">The destination frame; see <see cref="Convert(AVFrame, AVFrame)"/>.</param>
    /// <returns>A non-negative value on success, <see cref="AVResult32.InvalidArgument"/> if
    /// <paramref name="src"/> is too small, otherwise the FFmpeg error.</returns>
    /// <exception cref="InvalidOperationException">The context is a fixed context.</exception>
    public AVResult32 Convert(ReadOnlySpan<byte> src, ImageInfo srcInfo, AVFrame dst)
    {
        int size = srcInfo.BufferSize;
        if (size < 0 || src.Length < size)
            return AVResult32.InvalidArgument;
        fixed (byte* srcPtr = src)
            return Convert((IntPtr)srcPtr, srcInfo, dst);
    }

    /// <summary>
    /// Converts an image into another image.
    /// </summary>
    /// <param name="src">The source image.</param>
    /// <param name="dst">The destination image.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error.</returns>
    /// <exception cref="ArgumentException">A fixed context was created for other sizes or formats.</exception>
    public AVResult32 Convert(Image src, Image dst) => Convert(src.Data, src.Info, dst.Data, dst.Info);

    /// <summary>
    /// Converts a frame into an image.
    /// </summary>
    /// <param name="src">The source frame.</param>
    /// <param name="dst">The destination image.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error; a fixed context returns
    /// <see cref="AVResult32.InvalidArgument"/> for sizes or formats it was not created for.</returns>
    public AVResult32 Convert(AVFrame src, Image dst)
    {
        if (IsDynamic)
            return Convert(src, dst.Data, dst.Info);

        if (src.Height != fixedSourceHeight || src.Width != fixedSourceWidth || (PixelFormat)src.Format != fixedSourceFormat)
            return AVResult32.InvalidArgument;
        if (dst.Height != fixedDestinationHeight || dst.Width != fixedDestinationWidth || dst.PixelFormat != fixedDestinationFormat)
            return AVResult32.InvalidArgument;
        AutoGen.byte_ptrArray8 srcData = src.Frame->data;
        AutoGen.int_array8 srcLines = src.Frame->linesize;
        Span<IntPtr> dstData = stackalloc IntPtr[4];
        Span<int> dstLines = stackalloc int[4];
        _ = dst.GetPlanes(dstData, dstLines);
        fixed (void* dstDataPtr = dstData, dstLinesPtr = dstLines)
            return ffmpeg.sws_scale(Context, (byte**)&srcData, (int*)&srcLines, 0, fixedSourceHeight, (byte**)dstDataPtr, (int*)dstLinesPtr);
    }

    /// <summary>
    /// Converts an image into a frame.
    /// </summary>
    /// <param name="src">The source image.</param>
    /// <param name="dst">The destination frame; see <see cref="Convert(AVFrame, AVFrame)"/>.</param>
    /// <returns>A non-negative value on success, otherwise the FFmpeg error; a fixed context returns
    /// <see cref="AVResult32.InvalidArgument"/> for sizes or formats it was not created for.</returns>
    public AVResult32 Convert(Image src, AVFrame dst)
    {
        if (IsDynamic)
            return Convert(src.Data, src.Info, dst);

        if (dst.Height != fixedDestinationHeight || dst.Width != fixedDestinationWidth || (PixelFormat)dst.Format != fixedDestinationFormat)
            return AVResult32.InvalidArgument;
        if (src.Height != fixedSourceHeight || src.Width != fixedSourceWidth || src.PixelFormat != fixedSourceFormat)
            return AVResult32.InvalidArgument;

        Span<IntPtr> srcData = stackalloc IntPtr[4];
        Span<int> srcLines = stackalloc int[4];
        _ = src.GetPlanes(srcData, srcLines);
        if (!dst.HasBuffer)
            dst.CreateBuffer().ThrowIfError();
        AutoGen.byte_ptrArray8 dstData = dst.Frame->data;
        AutoGen.int_array8 dstLines = dst.Frame->linesize;

        fixed (void* srcDataPtr = srcData, srcLinesPtr = srcLines)
            return ffmpeg.sws_scale(Context, (byte**)srcDataPtr, (int*)srcLinesPtr, 0, fixedSourceHeight, (byte**)&dstData, (int*)&dstLines);
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Releases the unmanaged resources associated with this scaling context.
    /// </summary>
    /// <remarks>
    /// After calling this method, the context can no longer be used for image
    /// conversion.
    /// </remarks>
    public void Dispose()
    {
        _SwsContext* ctx = context;
        ffmpeg.sws_free_context(&ctx);
        context = null;
        _AVFrame* frame = sourceView;
        ffmpeg.av_frame_free(&frame);
        sourceView = null;
        frame = destinationView;
        ffmpeg.av_frame_free(&frame);
        destinationView = null;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Ensures the unmanaged data gets released.
    /// </summary>
    ~SwsContext() => Dispose();

    #endregion
}
