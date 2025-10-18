namespace FFmpeg.Formats;

/// <summary>
/// Enum representing the stream disposition flags, indicating special properties of a stream.
/// </summary>
[Flags]
public enum StreamDisposition
{
    /// <summary>
    /// The stream should be chosen by default among other streams of the same type,
    /// unless the user has explicitly specified otherwise.
    /// </summary>
    Default = ffmpeg.AV_DISPOSITION_DEFAULT,

    /// <summary>
    /// The stream is not in the original language.
    /// </summary>
    Dub = ffmpeg.AV_DISPOSITION_DUB,

    /// <summary>
    /// The stream is in the original language.
    /// </summary>
    Original = ffmpeg.AV_DISPOSITION_ORIGINAL,

    /// <summary>
    /// The stream is a commentary track.
    /// </summary>
    Commentary = ffmpeg.AV_DISPOSITION_COMMENT,

    /// <summary>
    /// The stream contains song lyrics.
    /// </summary>
    Lyrics = ffmpeg.AV_DISPOSITION_LYRICS,

    /// <summary>
    /// The stream contains karaoke audio.
    /// </summary>
    Karaoke = ffmpeg.AV_DISPOSITION_KARAOKE,

    /// <summary>
    /// The track should be used during playback by default, 
    /// useful for subtitle tracks that should be displayed 
    /// even when the user did not explicitly ask for subtitles.
    /// </summary>
    Forced = ffmpeg.AV_DISPOSITION_FORCED,

    /// <summary>
    /// The stream is intended for hearing-impaired audiences.
    /// </summary>
    HearingImpaired = ffmpeg.AV_DISPOSITION_HEARING_IMPAIRED,

    /// <summary>
    /// The stream is intended for visually-impaired audiences.
    /// </summary>
    VisuallyImpaired = ffmpeg.AV_DISPOSITION_VISUAL_IMPAIRED,

    /// <summary>
    /// The audio stream contains music and sound effects without voice.
    /// </summary>
    CleanEffects = ffmpeg.AV_DISPOSITION_CLEAN_EFFECTS,

    /// <summary>
    /// The stream is stored in the file as an attached picture/cover art.
    /// </summary>
    AttachedPicture = ffmpeg.AV_DISPOSITION_ATTACHED_PIC,

    /// <summary>
    /// The stream is sparse and contains thumbnail images, often corresponding to chapter markers.
    /// </summary>
    TimedThumbnails = ffmpeg.AV_DISPOSITION_TIMED_THUMBNAILS,

    /// <summary>
    /// The stream is intended to be mixed with a spatial audio track.
    /// </summary>
    NonDiegetic = ffmpeg.AV_DISPOSITION_NON_DIEGETIC,

    /// <summary>
    /// The subtitle stream contains captions, providing a transcription 
    /// and possibly a translation of the audio, typically intended for hearing-impaired audiences.
    /// </summary>
    Captions = ffmpeg.AV_DISPOSITION_CAPTIONS,

    /// <summary>
    /// The subtitle stream contains a textual description of the video content,
    /// typically intended for visually-impaired audiences or for situations where the video cannot be seen.
    /// </summary>
    Descriptions = ffmpeg.AV_DISPOSITION_DESCRIPTIONS,

    /// <summary>
    /// The subtitle stream contains time-aligned metadata that is not intended to be directly presented to the user.
    /// </summary>
    Metadata = ffmpeg.AV_DISPOSITION_METADATA,

    /// <summary>
    /// The stream is intended to be mixed with another stream before presentation,
    /// such as an image part of a HEIF grid or mix_type=0 in MPEG-TS.
    /// </summary>
    Dependent = ffmpeg.AV_DISPOSITION_DEPENDENT,

    /// <summary>
    /// The video stream contains still images.
    /// </summary>
    StillImage = ffmpeg.AV_DISPOSITION_STILL_IMAGE
}

