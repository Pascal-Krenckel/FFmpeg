namespace FFmpeg.Images;
/// <summary>
/// Represents the color transfer characteristics, which define how color values are encoded and transformed.
/// These values match the ones defined by ISO/IEC 23091-2_2019 subclause 8.2.
/// </summary>
public enum ColorTransferCharacteristic : int
{
    /// <summary>
    /// Reserved value, not to be used.
    /// </summary>
    Reserved0 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_RESERVED0,

    /// <summary>
    /// ITU-R BT.709 transfer characteristics, also known as ITU-R BT1361, used for HD TV and some digital video standards.
    /// </summary>
    BT709 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_BT709,

    /// <summary>
    /// Unspecified transfer characteristics.
    /// </summary>
    Unspecified = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_UNSPECIFIED,

    /// <summary>
    /// Reserved transfer characteristics for future use.
    /// </summary>
    Reserved = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_RESERVED,

    /// <summary>
    /// ITU-R BT.470 System M (525-line NTSC) transfer characteristics, also known as ITU-R BT1700 625 PAL & SECAM.
    /// </summary>
    Gamma22 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_GAMMA22,

    /// <summary>
    /// ITU-R BT.470 System B/G (625-line PAL/SECAM) transfer characteristics, also known as ITU-R BT601.
    /// </summary>
    Gamma28 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_GAMMA28,

    /// <summary>
    /// ITU-R BT.601 transfer characteristics, used for standard definition video, also known as ITU-R BT1358 and ITU-R BT1700 NTSC.
    /// </summary>
    SMPTE170M = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_SMPTE170M,

    /// <summary>
    /// SMPTE 240M transfer characteristics, used in various video systems.
    /// </summary>
    SMPTE240M = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_SMPTE240M,

    /// <summary>
    /// Linear transfer characteristics, often used for linear color encoding.
    /// </summary>
    Linear = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_LINEAR,

    /// <summary>
    /// Logarithmic transfer characteristic with a 100:1 range.
    /// </summary>
    Log = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_LOG,

    /// <summary>
    /// Logarithmic transfer characteristic with a 100 * Sqrt(10):1 range.
    /// </summary>
    LogSqrt = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_LOG_SQRT,

    /// <summary>
    /// IEC 61966-2-4 transfer characteristics, used in various color management systems.
    /// </summary>
    IEC61966_2_4 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_IEC61966_2_4,

    /// <summary>
    /// ITU-R BT.1361 Extended Colour Gamut transfer characteristics.
    /// </summary>
    BT1361_ECG = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_BT1361_ECG,

    /// <summary>
    /// IEC 61966-2-1 transfer characteristics, used for sRGB or sYCC color spaces.
    /// </summary>
    IEC61966_2_1 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_IEC61966_2_1,

    /// <summary>
    /// ITU-R BT.2020 transfer characteristics for 10-bit systems.
    /// </summary>
    BT2020_10 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_BT2020_10,

    /// <summary>
    /// ITU-R BT.2020 transfer characteristics for 12-bit systems.
    /// </summary>
    BT2020_12 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_BT2020_12,

    /// <summary>
    /// SMPTE ST 2084 transfer characteristics for 10-, 12-, 14-, and 16-bit systems.
    /// </summary>
    SMPTE2084 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_SMPTE2084,

    /// <summary>
    /// Alias for SMPTE ST 2084 transfer characteristics.
    /// </summary>
    SMPTEST2084 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_SMPTEST2084,

    /// <summary>
    /// SMPTE ST 428-1 transfer characteristics.
    /// </summary>
    SMPTE428 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_SMPTE428,

    /// <summary>
    /// Alias for SMPTE ST 428-1 transfer characteristics.
    /// </summary>
    SMPTEST428_1 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_SMPTEST428_1,

    /// <summary>
    /// ARIB STD-B67 transfer characteristics, also known as "Hybrid log-gamma".
    /// </summary>
    ARIB_STD_B67 = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_ARIB_STD_B67,

    /// <summary>
    /// Not part of ABI, represents the number of transfer characteristic options available.
    /// </summary>
    __COUNT__ = AutoGen._AVColorTransferCharacteristic.AVCOL_TRC_NB
}

