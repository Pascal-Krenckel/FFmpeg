using FFmpeg.AutoGen;

namespace FFmpeg.Utils;
/// <summary>
/// Represents different types of media streams.
/// Mapped from FFmpeg.AutoGen._AVMediaType enum.
/// </summary>
public enum MediaType : int
{
    /// <summary>
    /// Unknown media type. This is usually treated as Data.
    /// </summary>
    Unknown = _AVMediaType.AVMEDIA_TYPE_UNKNOWN,

    /// <summary>
    /// Video stream. Typically used for visual content.
    /// </summary>
    Video = _AVMediaType.AVMEDIA_TYPE_VIDEO,

    /// <summary>
    /// Audio stream. Typically used for sound content.
    /// </summary>
    Audio = _AVMediaType.AVMEDIA_TYPE_AUDIO,

    /// <summary>
    /// Opaque data stream. Used for continuous data without a specific media type.
    /// </summary>
    Data = _AVMediaType.AVMEDIA_TYPE_DATA,

    /// <summary>
    /// Subtitle stream. Used for text-based content like captions or translations.
    /// </summary>
    Subtitle = _AVMediaType.AVMEDIA_TYPE_SUBTITLE,

    /// <summary>
    /// Attachment stream. Used for sparse data such as images, fonts, or other assets attached to the media.
    /// </summary>
    Attachment = _AVMediaType.AVMEDIA_TYPE_ATTACHMENT,

    /// <summary>
    /// Total number of media types. This value is not a valid media type and is used internally for counting purposes.
    /// </summary>
    __COUNT__ = _AVMediaType.AVMEDIA_TYPE_NB
}

