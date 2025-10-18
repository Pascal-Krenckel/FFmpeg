namespace FFmpeg.Codecs;
/// <summary>
/// Enum representing codec properties (a combination of flags).
/// </summary>
[Flags]
public enum CodecProperties
{
    /// <summary>
    /// Codec uses only intra compression (video and audio codecs only).
    /// </summary>
    IntraOnly = 1 << 0,

    /// <summary>
    /// Codec supports lossy compression (audio and video codecs only).
    /// </summary>
    Lossy = 1 << 1,

    /// <summary>
    /// Codec supports lossless compression (audio and video codecs only).
    /// </summary>
    Lossless = 1 << 2,

    /// <summary>
    /// Codec supports frame reordering, meaning the coded order may differ from the presentation order.
    /// </summary>
    Reorder = 1 << 3,

    /// <summary>
    /// Video codec supports separate coding of fields in interlaced frames.
    /// </summary>
    Fields = 1 << 4,

    /// <summary>
    /// Subtitle codec is bitmap-based. Decoded subtitle data can be read from the AVSubtitleRect->pict field.
    /// </summary>
    BitmapSubtitle = 1 << 16,

    /// <summary>
    /// Subtitle codec is text-based. Decoded subtitle data can be read from the AVSubtitleRect->ass field.
    /// </summary>
    TextSubtitle = 1 << 17
}
