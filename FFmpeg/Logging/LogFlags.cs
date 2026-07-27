namespace FFmpeg.Logging;

[Flags]
public enum LogFlags
{
    /// <summary>
    /// Skip repeated messages, this requires the user app to use av_log() instead of (f)printf as the 2 would otherwise interfere and lead to "Last message repeated x times" messages below (f)printf messages with some bad luck.
    /// <br/>Also to receive the last, "last repeated" line if any, the user app must call av_log(NULL, AV_LOG_QUIET, "%s", ""); at the end
    /// </summary>
    SkipRepeated = ffmpeg.AV_LOG_SKIP_REPEATED,
    /// <summary>
    /// Include system date and time in log output. 
    /// </summary>
    PrintDateTime = ffmpeg.AV_LOG_PRINT_DATETIME,
    /// <summary>
    /// Include the log severity in messages originating from codecs. <br/>
    /// Results in messages such as: [rawvideo @ 0xDEADBEEF][error] encode did not produce valid pts
    /// </summary>
    PrintLogLevel = ffmpeg.AV_LOG_PRINT_LEVEL,
    /// <summary>
    /// Include system time in log output. 
    /// </summary>
    PrintTime = ffmpeg.AV_LOG_PRINT_TIME,

}
