namespace FFmpeg.Utils;

/// <summary>
/// Specifies flags for frames in FFmpeg.
/// </summary>
[Flags]
public enum FrameFlags : int
{
    /// <summary>
    /// Indicates that the frame data is corrupted.
    /// </summary>
    Corrupt = ffmpeg.AV_FRAME_FLAG_CORRUPT,

    /// <summary>
    /// Indicates that the frame should be discarded.
    /// </summary>
    Discard = ffmpeg.AV_FRAME_FLAG_DISCARD,

    /// <summary>
    /// Indicates that the frame is a key frame.
    /// </summary>
    Key = ffmpeg.AV_FRAME_FLAG_KEY,

    /// <summary>
    /// Indicates that the frame is interlaced.
    /// </summary>
    Interlaced = ffmpeg.AV_FRAME_FLAG_INTERLACED,

    /// <summary>
    /// A flag to mark frames where the top field is displayed first if the content is interlaced. 
    /// </summary>
    TopFieldFirst = ffmpeg.AV_FRAME_FLAG_TOP_FIELD_FIRST
}

