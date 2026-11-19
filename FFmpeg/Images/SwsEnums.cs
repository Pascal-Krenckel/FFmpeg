namespace FFmpeg.Images;

/// <summary>
/// The scaling filter of a <see cref="SwsContext"/> (<c>SwsContext.scaler</c>, option <c>scaler</c>).
/// </summary>
/// <remarks>
/// Overrides the scaler selected by the scaler bits of <see cref="SwsContext.Flags"/> (see <see cref="SwsAlgorithm"/>)
/// unless it is <see cref="Auto"/>. Only used by dynamic contexts (<see cref="SwsContext.IsDynamic"/>).
/// </remarks>
public enum SwsScaler
{
    /// <summary>Use the scaler selected by <see cref="SwsContext.Flags"/> (bicubic by default).</summary>
    Auto = AutoGen._SwsScaler.SWS_SCALE_AUTO,

    /// <summary>Bilinear filtering.</summary>
    Bilinear = AutoGen._SwsScaler.SWS_SCALE_BILINEAR,

    /// <summary>2-tap cubic BC-spline; tune it with <see cref="SwsContext.ScalerParam1"/> (B) and <see cref="SwsContext.ScalerParam2"/> (C).</summary>
    Bicubic = AutoGen._SwsScaler.SWS_SCALE_BICUBIC,

    /// <summary>Nearest neighbor (point sampling).</summary>
    Point = AutoGen._SwsScaler.SWS_SCALE_POINT,

    /// <summary>Area averaging.</summary>
    Area = AutoGen._SwsScaler.SWS_SCALE_AREA,

    /// <summary>2-tap Gaussian approximation; <see cref="SwsContext.ScalerParam1"/> tunes the exponent.</summary>
    Gaussian = AutoGen._SwsScaler.SWS_SCALE_GAUSSIAN,

    /// <summary>Unwindowed sinc.</summary>
    Sinc = AutoGen._SwsScaler.SWS_SCALE_SINC,

    /// <summary>3-tap sinc/sinc; <see cref="SwsContext.ScalerParam1"/> tunes the window width.</summary>
    Lanczos = AutoGen._SwsScaler.SWS_SCALE_LANCZOS,

    /// <summary>Unwindowed natural cubic spline.</summary>
    Spline = AutoGen._SwsScaler.SWS_SCALE_SPLINE,
}

/// <summary>
/// The dither mode of a <see cref="SwsContext"/> (<c>SwsContext.dither</c>, option <c>sws_dither</c>).
/// </summary>
public enum SwsDither
{
    /// <summary>No dithering.</summary>
    None = AutoGen._SwsDither.SWS_DITHER_NONE,

    /// <summary>Selected automatically (the default).</summary>
    Auto = AutoGen._SwsDither.SWS_DITHER_AUTO,

    /// <summary>Ordered dither matrix.</summary>
    Bayer = AutoGen._SwsDither.SWS_DITHER_BAYER,

    /// <summary>Error diffusion; replaces the deprecated <see cref="SwsFlags.ErrorDiffusion"/>. Disables slice threading.</summary>
    ErrorDiffusion = AutoGen._SwsDither.SWS_DITHER_ED,

    /// <summary>Arithmetic addition dither.</summary>
    ArithmeticAddition = AutoGen._SwsDither.SWS_DITHER_A_DITHER,

    /// <summary>Arithmetic xor dither.</summary>
    ArithmeticXor = AutoGen._SwsDither.SWS_DITHER_X_DITHER,
}

/// <summary>
/// How a <see cref="SwsContext"/> handles alpha when converting to a format without alpha
/// (<c>SwsContext.alpha_blend</c>, option <c>alphablend</c>).
/// </summary>
public enum SwsAlphaBlend
{
    /// <summary>Ignore the alpha channel (the default).</summary>
    None = AutoGen._SwsAlphaBlend.SWS_ALPHA_BLEND_NONE,

    /// <summary>Blend onto a uniform color.</summary>
    Uniform = AutoGen._SwsAlphaBlend.SWS_ALPHA_BLEND_UNIFORM,

    /// <summary>Blend onto a checkerboard.</summary>
    Checkerboard = AutoGen._SwsAlphaBlend.SWS_ALPHA_BLEND_CHECKERBOARD,
}

/// <summary>
/// The rendering intent used when a <see cref="SwsContext"/> converts between different color primaries or
/// transfer functions, e.g. HDR to SDR (<c>SwsContext.intent</c>, option <c>intent</c>).
/// </summary>
public enum SwsIntent
{
    /// <summary>Perceptual tone and gamut mapping.</summary>
    Perceptual = AutoGen._SwsIntent.SWS_INTENT_PERCEPTUAL,

    /// <summary>Relative colorimetric clipping (the default).</summary>
    RelativeColorimetric = AutoGen._SwsIntent.SWS_INTENT_RELATIVE_COLORIMETRIC,

    /// <summary>Saturation mapping.</summary>
    Saturation = AutoGen._SwsIntent.SWS_INTENT_SATURATION,

    /// <summary>Absolute colorimetric clipping.</summary>
    AbsoluteColorimetric = AutoGen._SwsIntent.SWS_INTENT_ABSOLUTE_COLORIMETRIC,
}

/// <summary>
/// The implementations a dynamic <see cref="SwsContext"/> may use (<c>SwsContext.backends</c>, option
/// <c>sws_backends</c>).
/// </summary>
/// <remarks>
/// <see cref="Auto"/> lets FFmpeg choose: the stable backends, plus the unstable ones if
/// <see cref="SwsFlags.Unstable"/> is set. Fixed (legacy) contexts always use <see cref="Legacy"/>.
/// </remarks>
[Flags]
public enum SwsBackend
{
    /// <summary>Selected automatically (the default).</summary>
    Auto = 0,

    /// <summary>The established, format-specific code; the only stable backend.</summary>
    Legacy = AutoGen._SwsBackend.SWS_BACKEND_LEGACY,

    /// <summary>All stable backends.</summary>
    Stable = AutoGen._SwsBackend.SWS_BACKEND_STABLE,

    /// <summary>Template-based C reference implementation (unstable).</summary>
    C = AutoGen._SwsBackend.SWS_BACKEND_C,

    /// <summary>Fast path using memcpy/memset (unstable).</summary>
    Memcpy = AutoGen._SwsBackend.SWS_BACKEND_MEMCPY,

    /// <summary>Chained x86 SIMD kernels (unstable).</summary>
    X86 = AutoGen._SwsBackend.SWS_BACKEND_X86,

    /// <summary>Chained AArch64 NEON kernels (unstable).</summary>
    AArch64 = AutoGen._SwsBackend.SWS_BACKEND_AARCH64,

    /// <summary>Vulkan SPIR-V backend (unstable).</summary>
    SpirV = AutoGen._SwsBackend.SWS_BACKEND_SPIRV,

    /// <summary>All unstable backends.</summary>
    Unstable = AutoGen._SwsBackend.SWS_BACKEND_UNSTABLE,

    /// <summary>All backends.</summary>
    All = AutoGen._SwsBackend.SWS_BACKEND_ALL,
}
