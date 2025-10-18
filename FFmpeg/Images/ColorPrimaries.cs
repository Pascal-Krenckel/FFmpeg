namespace FFmpeg.Images;
/// <summary>
/// Represents the chromaticity coordinates of the source primaries, matching values defined by ISO/IEC 23091-2_2019 and ITU-T H.273.
/// </summary>
public enum ColorPrimaries : int
{
    /// <summary>
    /// Reserved value, not to be used.
    /// </summary>
    Reserved0 = AutoGen._AVColorPrimaries.AVCOL_PRI_RESERVED0,

    /// <summary>
    /// ITU-R BT.709 color primaries, used for HD TV and some modern digital video standards.
    /// This also corresponds to ITU-R BT.1361, IEC 61966-2-4, and SMPTE RP 177 Annex B.
    /// </summary>
    BT709 = AutoGen._AVColorPrimaries.AVCOL_PRI_BT709,

    /// <summary>
    /// Unspecified color primaries.
    /// </summary>
    Unspecified = AutoGen._AVColorPrimaries.AVCOL_PRI_UNSPECIFIED,

    /// <summary>
    /// Reserved value for future use.
    /// </summary>
    Reserved = AutoGen._AVColorPrimaries.AVCOL_PRI_RESERVED,

    /// <summary>
    /// ITU-R BT.470 System M (525-line NTSC) color primaries.
    /// Also used in FCC Title 47 Code of Federal Regulations 73.682 (a)(20).
    /// </summary>
    BT470M = AutoGen._AVColorPrimaries.AVCOL_PRI_BT470M,

    /// <summary>
    /// ITU-R BT.470 System B/G (625-line PAL/SECAM) color primaries.
    /// Also used in ITU-R BT.601-6 625, ITU-R BT.1358 625, and ITU-R BT.1700 625 PAL & SECAM standards.
    /// </summary>
    BT470BG = AutoGen._AVColorPrimaries.AVCOL_PRI_BT470BG,

    /// <summary>
    /// SMPTE 170M color primaries, used in NTSC video and similar to ITU-R BT.601 for 525-line systems.
    /// </summary>
    SMPTE170M = AutoGen._AVColorPrimaries.AVCOL_PRI_SMPTE170M,

    /// <summary>
    /// SMPTE 240M color primaries, similar to SMPTE 170M but with slightly different characteristics.
    /// Sometimes referred to as "SMPTE C" despite using D65 white point.
    /// </summary>
    SMPTE240M = AutoGen._AVColorPrimaries.AVCOL_PRI_SMPTE240M,

    /// <summary>
    /// Generic film color primaries, using color filters with Illuminant C.
    /// </summary>
    Film = AutoGen._AVColorPrimaries.AVCOL_PRI_FILM,

    /// <summary>
    /// ITU-R BT.2020 color primaries, used for Ultra HD (UHD) video.
    /// </summary>
    BT2020 = AutoGen._AVColorPrimaries.AVCOL_PRI_BT2020,

    /// <summary>
    /// SMPTE ST 428-1 color primaries, representing the CIE 1931 XYZ color space.
    /// </summary>
    SMPTE428 = AutoGen._AVColorPrimaries.AVCOL_PRI_SMPTE428,

    /// <summary>
    /// SMPTE ST 428-1 color primaries (alias for SMPTE428), used in D-Cinema systems.
    /// </summary>
    SMPTEST428_1 = AutoGen._AVColorPrimaries.AVCOL_PRI_SMPTEST428_1,

    /// <summary>
    /// SMPTE ST 431-2 color primaries, also known as DCI-P3, used in digital cinema.
    /// </summary>
    SMPTE431 = AutoGen._AVColorPrimaries.AVCOL_PRI_SMPTE431,

    /// <summary>
    /// SMPTE ST 432-1 color primaries, also known as Display P3 or P3 D65, used in modern displays like smartphones and computers.
    /// </summary>
    SMPTE432 = AutoGen._AVColorPrimaries.AVCOL_PRI_SMPTE432,

    /// <summary>
    /// EBU Tech 3213 color primaries, though rarely used, and related to one of the JEDEC P22 phosphors.
    /// </summary>
    EBU3213 = AutoGen._AVColorPrimaries.AVCOL_PRI_EBU3213,

    /// <summary>
    /// Alias for EBU Tech 3213 primaries, linked to JEDEC P22 phosphor color primaries.
    /// </summary>
    JEDECP22 = AutoGen._AVColorPrimaries.AVCOL_PRI_JEDEC_P22,

    /// <summary>
    /// Not part of ABI, and represents the number of color primary options available.
    /// </summary>
    __COUNT__ = AutoGen._AVColorPrimaries.AVCOL_PRI_NB
}
