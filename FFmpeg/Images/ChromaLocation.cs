namespace FFmpeg.Images;

/// <summary>
/// Specifies the location of chroma samples in the video.
/// </summary>
public enum ChromaLocation : int
{
    /// <summary>
    /// Unspecified chroma location.
    /// </summary>
    Unspecified = AutoGen._AVChromaLocation.AVCHROMA_LOC_UNSPECIFIED,

    /// <summary>
    /// Chroma samples are located at the left of the luma samples. This is used in MPEG-2/4 4:2:0 and is the default for 4:2:0 in H.264.
    /// </summary>
    Left = AutoGen._AVChromaLocation.AVCHROMA_LOC_LEFT,

    /// <summary>
    /// Chroma samples are located at the center of the luma samples. This is used in MPEG-1 4:2:0, JPEG 4:2:0, and H.263 4:2:0.
    /// </summary>
    Center = AutoGen._AVChromaLocation.AVCHROMA_LOC_CENTER,

    /// <summary>
    /// Chroma samples are located at the top-left corner of the luma samples. This location is used in ITU-R 601, SMPTE 274M, 296M, S314M (DV 4:1:1), and MPEG2 4:2:2.
    /// </summary>
    TopLeft = AutoGen._AVChromaLocation.AVCHROMA_LOC_TOPLEFT,

    /// <summary>
    /// Chroma samples are located at the top of the luma samples.
    /// </summary>
    Top = AutoGen._AVChromaLocation.AVCHROMA_LOC_TOP,

    /// <summary>
    /// Chroma samples are located at the bottom-left corner of the luma samples.
    /// </summary>
    BottomLeft = AutoGen._AVChromaLocation.AVCHROMA_LOC_BOTTOMLEFT,

    /// <summary>
    /// Chroma samples are located at the bottom of the luma samples.
    /// </summary>
    Bottom = AutoGen._AVChromaLocation.AVCHROMA_LOC_BOTTOM,

    /// <summary>
    /// Not part of ABI, represents the number of chroma location options available.
    /// </summary>
    __COUNT__ = AutoGen._AVChromaLocation.AVCHROMA_LOC_NB
}
