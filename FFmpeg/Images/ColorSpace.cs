namespace FFmpeg.Images;

/// <summary>
/// Represents various color space types.
/// These values are based on ISO/IEC 23091-2_2019 subclause 8.3.
/// </summary>
public enum ColorSpace : int
{
    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_RGB"/>
    RGB = AutoGen._AVColorSpace.AVCOL_SPC_RGB,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_BT709"/>
    BT709 = AutoGen._AVColorSpace.AVCOL_SPC_BT709,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_UNSPECIFIED"/>
    Unspecified = AutoGen._AVColorSpace.AVCOL_SPC_UNSPECIFIED,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_RESERVED"/>
    Reserved = AutoGen._AVColorSpace.AVCOL_SPC_RESERVED,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_FCC"/>
    FCC = AutoGen._AVColorSpace.AVCOL_SPC_FCC,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_BT470BG"/>
    BT470BG = AutoGen._AVColorSpace.AVCOL_SPC_BT470BG,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_SMPTE170M"/>
    SMPTE170M = AutoGen._AVColorSpace.AVCOL_SPC_SMPTE170M,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_SMPTE240M"/>
    SMPTE240M = AutoGen._AVColorSpace.AVCOL_SPC_SMPTE240M,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_YCGCO"/>
    YCGCO = AutoGen._AVColorSpace.AVCOL_SPC_YCGCO,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_YCGCO"/>
    YCOCG = AutoGen._AVColorSpace.AVCOL_SPC_YCOCG,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_BT2020_NCL"/>
    BT2020_NCL = AutoGen._AVColorSpace.AVCOL_SPC_BT2020_NCL,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_BT2020_CL"/>
    BT2020_CL = AutoGen._AVColorSpace.AVCOL_SPC_BT2020_CL,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_SMPTE2085"/>
    SMPTE2085 = AutoGen._AVColorSpace.AVCOL_SPC_SMPTE2085,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_CHROMA_DERIVED_NCL"/>
    ChromaDerivedNCL = AutoGen._AVColorSpace.AVCOL_SPC_CHROMA_DERIVED_NCL,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_CHROMA_DERIVED_CL"/>
    ChromaDerivedCL = AutoGen._AVColorSpace.AVCOL_SPC_CHROMA_DERIVED_CL,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_ICTCP"/>
    ICTCP = AutoGen._AVColorSpace.AVCOL_SPC_ICTCP,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_IPT_C2"/>
    IPT_C2 = AutoGen._AVColorSpace.AVCOL_SPC_IPT_C2,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_YCGCO_RE"/>
    YCGCO_RE = AutoGen._AVColorSpace.AVCOL_SPC_YCGCO_RE,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_YCGCO_RO"/>
    YCGCO_RO = AutoGen._AVColorSpace.AVCOL_SPC_YCGCO_RO,

    /// <inheritdoc cref="AutoGen._AVColorSpace.AVCOL_SPC_NB"/>
    __COUNT__ = AutoGen._AVColorSpace.AVCOL_SPC_NB
}
