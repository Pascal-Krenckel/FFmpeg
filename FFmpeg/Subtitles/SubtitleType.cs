namespace FFmpeg.Subtitles;
/// <summary>
/// Represents the type of subtitle, which defines how the subtitle data is structured or formatted.
/// </summary>
public enum SubtitleType : int
{
    /// <summary>
    /// No subtitle is present.
    /// </summary>
    None = FFmpeg.AutoGen._AVSubtitleType.SUBTITLE_NONE,

    /// <summary>
    /// A bitmap subtitle, where the subtitle is stored as an image. The <see cref="Subtitles.SubtitleRect.BitmapData"/>
    /// field will be populated with the bitmap data.
    /// </summary>
    Bitmap = FFmpeg.AutoGen._AVSubtitleType.SUBTITLE_BITMAP,

    /// <summary>
    /// Plain text subtitle. The <see cref="Subtitles.SubtitleRect.Text"/> field is set by the decoder 
    /// and is considered authoritative. The <see cref="Subtitles.SubtitleRect.ASS"/> and 
    /// <see cref="Subtitles.SubtitleRect.BitmapData"/> fields may contain approximations of the subtitle.
    /// </summary>
    Text = FFmpeg.AutoGen._AVSubtitleType.SUBTITLE_TEXT,

    /// <summary>
    /// Formatted text subtitle, typically in ASS/SSA format. The <see cref="Subtitles.SubtitleRect.ASS"/> 
    /// field is set by the decoder and is considered authoritative. The <see cref="Subtitles.SubtitleRect.BitmapData"/> 
    /// and <see cref="Subtitles.SubtitleRect.Text"/> fields may contain approximations of the subtitle.
    /// </summary>
    ASS = FFmpeg.AutoGen._AVSubtitleType.SUBTITLE_ASS
}
