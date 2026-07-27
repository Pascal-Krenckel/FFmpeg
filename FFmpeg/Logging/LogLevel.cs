namespace FFmpeg.Logging;

public enum LogLevel
{
    Info = ffmpeg.AV_LOG_INFO,
    Error = ffmpeg.AV_LOG_ERROR,
    Debug = ffmpeg.AV_LOG_DEBUG,
    Fatal = ffmpeg.AV_LOG_FATAL,
    Panic = ffmpeg.AV_LOG_PANIC,
    Quiet = ffmpeg.AV_LOG_QUIET,
    Trace = ffmpeg.AV_LOG_TRACE,
    Verbose = ffmpeg.AV_LOG_VERBOSE,
    Warning = ffmpeg.AV_LOG_WARNING,

}
