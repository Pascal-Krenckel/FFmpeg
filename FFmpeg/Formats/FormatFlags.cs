namespace FFmpeg.Formats;

/// <summary>
/// Flags used to specify various format options.
/// </summary>
[Flags]
public enum FormatFlags
{
    /// <summary>
    /// Format does not require a file.
    /// </summary>
    NoFile = ffmpeg.AVFMT_NOFILE,

    /// <summary>
    /// Format needs a number in filenames.
    /// </summary>
    NeedNumber = ffmpeg.AVFMT_NEEDNUMBER,

    /// <summary>
    /// Format shows IDs.
    /// </summary>
    ShowIds = ffmpeg.AVFMT_SHOW_IDS,

    /// <summary>
    /// Format does not have timestamps.
    /// </summary>
    NoTimestamps = ffmpeg.AVFMT_NOTIMESTAMPS,

    /// <summary>
    /// Format uses a generic index.
    /// </summary>
    GenericIndex = ffmpeg.AVFMT_GENERIC_INDEX,

    /// <summary>
    /// Format has discontinuous timestamps.
    /// </summary>
    TsDiscont = ffmpeg.AVFMT_TS_DISCONT,

    /// <summary>
    /// Format does not support binary search.
    /// </summary>
    NoBinSearch = ffmpeg.AVFMT_NOBINSEARCH,

    /// <summary>
    /// Format does not support general search.
    /// </summary>
    NoGenSearch = ffmpeg.AVFMT_NOGENSEARCH,

    /// <summary>
    /// Format does not support byte seeking.
    /// </summary>
    NoByteSeek = ffmpeg.AVFMT_NO_BYTE_SEEK,

    /// <summary>
    /// Format seeks to presentation timestamps.
    /// </summary>
    SeekToPts = ffmpeg.AVFMT_SEEK_TO_PTS,

    /// <summary>
    /// Format uses a global header.
    /// </summary>
    GlobalHeader = ffmpeg.AVFMT_GLOBALHEADER,

    /// <summary>
    /// Format supports variable frame rates.
    /// </summary>
    VariableFps = ffmpeg.AVFMT_VARIABLE_FPS,

    /// <summary>
    /// Format does not have dimensions.
    /// </summary>
    NoDimensions = ffmpeg.AVFMT_NODIMENSIONS,

    /// <summary>
    /// Format does not have streams.
    /// </summary>
    NoStreams = ffmpeg.AVFMT_NOSTREAMS,

    /// <summary>
    /// Format has non-strict timestamps.
    /// </summary>
    TsNonStrict = ffmpeg.AVFMT_TS_NONSTRICT,

    /// <summary>
    /// Format supports negative timestamps.
    /// </summary>
    TsNegative = ffmpeg.AVFMT_TS_NEGATIVE
}
