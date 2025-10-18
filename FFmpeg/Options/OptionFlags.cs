namespace FFmpeg.Options;

/// <summary>
/// Represents the flags that can be applied to AVOptions in FFmpeg.
/// These flags control how options are used during encoding, decoding, filtering, and other operations.
/// </summary>
[Flags]
public enum OptionFlags
{
    /// <summary>
    /// A generic parameter that can be set by the user for muxing or encoding.
    /// Corresponds to AV_OPT_FLAG_ENCODING_PARAM.
    /// </summary>
    EncodingParam = ffmpeg.AV_OPT_FLAG_ENCODING_PARAM,

    /// <summary>
    /// A generic parameter that can be set by the user for demuxing or decoding.
    /// Corresponds to AV_OPT_FLAG_DECODING_PARAM.
    /// </summary>
    DecodingParam = ffmpeg.AV_OPT_FLAG_DECODING_PARAM,

    /// <summary>
    /// A parameter specifically related to audio operations.
    /// Corresponds to AV_OPT_FLAG_AUDIO_PARAM.
    /// </summary>
    AudioParam = ffmpeg.AV_OPT_FLAG_AUDIO_PARAM,

    /// <summary>
    /// A parameter specifically related to video operations.
    /// Corresponds to AV_OPT_FLAG_VIDEO_PARAM.
    /// </summary>
    VideoParam = ffmpeg.AV_OPT_FLAG_VIDEO_PARAM,

    /// <summary>
    /// A parameter specifically related to subtitle operations.
    /// Corresponds to AV_OPT_FLAG_SUBTITLE_PARAM.
    /// </summary>
    SubtitleParam = ffmpeg.AV_OPT_FLAG_SUBTITLE_PARAM,

    /// <summary>
    /// The option is intended for exporting values to the caller.
    /// Corresponds to AV_OPT_FLAG_EXPORT.
    /// </summary>
    Export = ffmpeg.AV_OPT_FLAG_EXPORT,

    /// <summary>
    /// The option may not be set through the AVOptions API, only read.
    /// This flag only makes sense when AV_OPT_FLAG_EXPORT is also set.
    /// Corresponds to AV_OPT_FLAG_READONLY.
    /// </summary>
    ReadOnly = ffmpeg.AV_OPT_FLAG_READONLY,

    /// <summary>
    /// A generic parameter that can be set by the user for bitstream filtering.
    /// Corresponds to AV_OPT_FLAG_BSF_PARAM.
    /// </summary>
    BSFParam = ffmpeg.AV_OPT_FLAG_BSF_PARAM,

    /// <summary>
    /// A generic parameter that can be set by the user at runtime.
    /// Corresponds to AV_OPT_FLAG_RUNTIME_PARAM.
    /// </summary>
    RuntimeParam = ffmpeg.AV_OPT_FLAG_RUNTIME_PARAM,

    /// <summary>
    /// A generic parameter that can be set by the user for filtering.
    /// Corresponds to AV_OPT_FLAG_FILTERING_PARAM.
    /// </summary>
    FilteringParam = ffmpeg.AV_OPT_FLAG_FILTERING_PARAM,

    /// <summary>
    /// Notify if the option is deprecated; users should refer to the AVOption help text for more information.
    /// Corresponds to AV_OPT_FLAG_DEPRECATED.
    /// </summary>
    Deprecated = ffmpeg.AV_OPT_FLAG_DEPRECATED,

    /// <summary>
    /// Notify if option constants can also reside in child objects.
    /// Corresponds to AV_OPT_FLAG_CHILD_CONSTS.
    /// </summary>
    ChildConsts = ffmpeg.AV_OPT_FLAG_CHILD_CONSTS
}
