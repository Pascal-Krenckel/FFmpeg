namespace FFmpeg.Utils;

/// <summary>
/// Enumeration for packet flags, corresponding to AV_PKT_FLAG values.
/// </summary>
[Flags]
public enum PacketFlags
{
    None = 0,
    /// <summary>
    /// The packet contains a keyframe.
    /// </summary>
    Key = ffmpeg.AV_PKT_FLAG_KEY,

    /// <summary>
    /// The packet content is corrupted.
    /// </summary>
    Corrupt = ffmpeg.AV_PKT_FLAG_CORRUPT,

    /// <summary>
    /// The packet is required to maintain valid decoder state but should be discarded after decoding.
    /// </summary>
    Discard = ffmpeg.AV_PKT_FLAG_DISCARD,

    /// <summary>
    /// The packet comes from a trusted source, allowing otherwise-unsafe constructs such as arbitrary pointers.
    /// </summary>
    Trusted = ffmpeg.AV_PKT_FLAG_TRUSTED,

    /// <summary>
    /// The packet contains frames that can be discarded by the decoder (non-reference frames).
    /// </summary>
    Disposable = ffmpeg.AV_PKT_FLAG_DISPOSABLE
}
