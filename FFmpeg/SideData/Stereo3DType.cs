using FFmpeg.AutoGen;

namespace FFmpeg.SideData;

/// <summary>
/// Specifies how stereoscopic 3D views are arranged within a video frame.
/// </summary>
public enum Stereo3DType
{
    /// <summary>
    /// The video is not stereoscopic.
    /// </summary>
    TwoDimensional = _AVStereo3DType.AV_STEREO3D_2D,

    /// <summary>
    /// The left and right views are placed next to each other horizontally.
    /// </summary>
    SideBySide = _AVStereo3DType.AV_STEREO3D_SIDEBYSIDE,

    /// <summary>
    /// The left and right views are placed above and below each other vertically.
    /// </summary>
    TopBottom = _AVStereo3DType.AV_STEREO3D_TOPBOTTOM,

    /// <summary>
    /// The left and right views are alternated temporally in successive frames.
    /// </summary>
    FrameSequence = _AVStereo3DType.AV_STEREO3D_FRAMESEQUENCE,

    /// <summary>
    /// The left and right views are packed in a checkerboard-like pattern on a per-pixel basis.
    /// </summary>
    Checkerboard = _AVStereo3DType.AV_STEREO3D_CHECKERBOARD,

    /// <summary>
    /// The left and right views are placed next to each other horizontally.
    /// When upscaling, a checkerboard pattern is applied.
    /// </summary>
    SideBySideQuincunx = _AVStereo3DType.AV_STEREO3D_SIDEBYSIDE_QUINCUNX,

    /// <summary>
    /// The left and right views are packed on alternating lines, similar to interlaced video.
    /// </summary>
    Lines = _AVStereo3DType.AV_STEREO3D_LINES,

    /// <summary>
    /// The left and right views are packed on alternating columns.
    /// </summary>
    Columns = _AVStereo3DType.AV_STEREO3D_COLUMNS,

    /// <summary>
    /// The video is stereoscopic, but the arrangement of the views is unspecified.
    /// </summary>
    Unspecified = _AVStereo3DType.AV_STEREO3D_UNSPEC,
}