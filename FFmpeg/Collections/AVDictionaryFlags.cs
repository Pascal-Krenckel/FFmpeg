namespace FFmpeg.Collections;

[Flags]
/// <summary>
/// Enumeration for FFmpeg dictionary options.
/// </summary>
internal enum AVDictionaryFlags
{
    None = 0,

    /// <summary>
    /// Only match exact keys.
    /// </summary>
    MatchCase = AutoGen.ffmpeg.AV_DICT_MATCH_CASE,

    /// <summary>
    /// Match keys with case-insensitive suffix.
    /// </summary>
    IgnoreSuffix = AutoGen.ffmpeg.AV_DICT_IGNORE_SUFFIX,

    /// <summary>
    /// Do not copy the key.
    /// </summary>
    DontStrdupKey = AutoGen.ffmpeg.AV_DICT_DONT_STRDUP_KEY,

    /// <summary>
    /// Do not copy the value.
    /// </summary>
    DontStrdupVal = AutoGen.ffmpeg.AV_DICT_DONT_STRDUP_VAL,

    /// <summary>
    /// Do not overwrite existing entries.
    /// </summary>
    DontOverwrite = AutoGen.ffmpeg.AV_DICT_DONT_OVERWRITE,

    /// <summary>
    /// Append to existing entry.
    /// </summary>
    Append = AutoGen.ffmpeg.AV_DICT_APPEND,

    /// <summary>
    /// Allow multiple entries with the same key.
    /// </summary>
    MultiKey = AutoGen.ffmpeg.AV_DICT_MULTIKEY,

}