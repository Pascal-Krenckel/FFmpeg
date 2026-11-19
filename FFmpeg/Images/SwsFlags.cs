namespace FFmpeg.Images;

/// <summary>
/// The flags of a <see cref="SwsContext"/> (<c>SwsContext.flags</c>, option <c>sws_flags</c>).
/// </summary>
/// <remarks>
/// The scaler flags (<see cref="FastBilinear"/> to <see cref="Spline"/>) select the scaling filter; only one may be set.
/// For dynamic contexts FFmpeg recommends <see cref="SwsContext.Scaler"/> instead, which overrides them unless it is
/// <see cref="SwsScaler.Auto"/>. <see cref="FastBilinear"/>, <see cref="Experimental"/> and <see cref="Bicublin"/>
/// have no <see cref="SwsScaler"/> equivalent.
/// </remarks>
[Flags]
public enum SwsFlags
{
    /// <summary>
    /// Fast bilinear scaling, prioritizing speed over quality.
    /// </summary>
    FastBilinear = AutoGen._SwsFlags.SWS_FAST_BILINEAR,

    /// <summary>
    /// Standard bilinear scaling, balanced quality and speed.
    /// </summary>
    Bilinear = AutoGen._SwsFlags.SWS_BILINEAR,

    /// <summary>
    /// Bicubic scaling, higher quality but slower performance.
    /// </summary>
    Bicubic = AutoGen._SwsFlags.SWS_BICUBIC,

    /// <summary>
    /// Experimental scaling algorithm.
    /// </summary>
    Experimental = AutoGen._SwsFlags.SWS_X,

    /// <summary>
    /// Point rescaling, or nearest neighbor, good for sharp edges.
    /// </summary>
    NearestNeighbor = AutoGen._SwsFlags.SWS_POINT,

    /// <summary>
    /// Area-based rescaling, typically used for downscaling.
    /// </summary>
    Area = AutoGen._SwsFlags.SWS_AREA,

    /// <summary>
    /// Bicublin rescaling, a hybrid of bilinear and bicubic.
    /// </summary>
    Bicublin = AutoGen._SwsFlags.SWS_BICUBLIN,

    /// <summary>
    /// Gaussian scaling filter, smoothens the output.
    /// </summary>
    Gauss = AutoGen._SwsFlags.SWS_GAUSS,

    /// <summary>
    /// Sinc rescaling filter, produces high-quality scaling.
    /// </summary>
    Sinc = AutoGen._SwsFlags.SWS_SINC,

    /// <summary>
    /// Lanczos scaling filter, known for high-quality output.
    /// </summary>
    Lanczos = AutoGen._SwsFlags.SWS_LANCZOS,

    /// <summary>
    /// Spline scaling filter, offers high-quality and smooth scaling.
    /// </summary>
    Spline = AutoGen._SwsFlags.SWS_SPLINE,

    /// <summary>
    /// Return an error on underspecified conversions (e.g. an unspecified color space or range) instead of
    /// assuming defaults. Dynamic contexts only.
    /// </summary>
    Strict = AutoGen._SwsFlags.SWS_STRICT,

    /// <summary>
    /// Print additional information during scaling.
    /// </summary>
    PrintInfo = AutoGen._SwsFlags.SWS_PRINT_INFO,

    /// <summary>
    /// Use full chroma interpolation for horizontal pixels.
    /// </summary>
    FullChromaInterpolation = AutoGen._SwsFlags.SWS_FULL_CHR_H_INT,

    /// <summary>
    /// Use full chroma input for horizontal pixels.
    /// </summary>
    FullChromaInput = AutoGen._SwsFlags.SWS_FULL_CHR_H_INP,

    /// <summary>
    /// Use accurate rounding.
    /// </summary>
    AccurateRounding = AutoGen._SwsFlags.SWS_ACCURATE_RND,

    /// <summary>
    /// Use bit-exact scaling, useful for testing.
    /// </summary>
    BitExact = AutoGen._SwsFlags.SWS_BITEXACT,

    /// <summary>
    /// Allow/prefer experimental new code paths (the unstable <see cref="SwsBackend"/>s). These may be faster, slower or
    /// produce different output, and their behaviour may change at any time; FFmpeg intends them for testing only.
    /// </summary>
    Unstable = AutoGen._SwsFlags.SWS_UNSTABLE,

    /// <summary>
    /// Has no effect (deprecated by FFmpeg).
    /// </summary>
    [Obsolete("This flag has no effect in FFmpeg.")]
    DirectBGR = AutoGen._SwsFlags.SWS_DIRECT_BGR,

    /// <summary>
    /// Apply error diffusion to the output (deprecated by FFmpeg).
    /// </summary>
    [Obsolete("Set SwsContext.Dither to SwsDither.ErrorDiffusion instead.")]
    ErrorDiffusion = AutoGen._SwsFlags.SWS_ERROR_DIFFUSION,
}
