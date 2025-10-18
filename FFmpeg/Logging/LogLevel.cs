using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Logging;

/// <summary>
/// Specifies the logging level used by FFmpeg, controlling the verbosity and type of log messages produced.
/// </summary>
public enum LogLevel : int
{
    /// <summary>
    /// No output will be printed. This effectively disables logging.
    /// Corresponds to <c>AV_LOG_QUIET</c> (-8).
    /// </summary>
    Quiet = ffmpeg.AV_LOG_QUIET, // -8

    /// <summary>
    /// A panic condition, meaning something went really wrong and the program is likely to crash.
    /// Corresponds to <c>AV_LOG_PANIC</c> (0).
    /// </summary>
    Panic = ffmpeg.AV_LOG_PANIC, // 0

    /// <summary>
    /// Fatal errors that indicate the program cannot recover and must exit.
    /// Corresponds to <c>AV_LOG_FATAL</c> (8).
    /// </summary>
    Fatal = ffmpeg.AV_LOG_FATAL, // 8

    /// <summary>
    /// Non-recoverable errors that cannot be fixed but don't immediately crash the application.
    /// Corresponds to <c>AV_LOG_ERROR</c> (16).
    /// </summary>
    Error = ffmpeg.AV_LOG_ERROR, // 16

    /// <summary>
    /// Warning messages about something that looks incorrect but is not critical.
    /// Corresponds to <c>AV_LOG_WARNING</c> (24).
    /// </summary>
    Warning = ffmpeg.AV_LOG_WARNING, // 24

    /// <summary>
    /// Standard informational messages, such as general progress updates.
    /// Corresponds to <c>AV_LOG_INFO</c> (32).
    /// </summary>
    Info = ffmpeg.AV_LOG_INFO, // 32

    /// <summary>
    /// Verbose informational messages providing additional details.
    /// Corresponds to <c>AV_LOG_VERBOSE</c> (40).
    /// </summary>
    Verbose = ffmpeg.AV_LOG_VERBOSE, // 40

    /// <summary>
    /// Debugging information useful primarily for developers working with the FFmpeg libraries.
    /// Corresponds to <c>AV_LOG_DEBUG</c> (48).
    /// </summary>
    Debug = ffmpeg.AV_LOG_DEBUG, // 48

    /// <summary>
    /// Extremely verbose debugging information, useful for development and troubleshooting deep issues.
    /// Corresponds to <c>AV_LOG_TRACE</c> (56).
    /// </summary>
    Trace = ffmpeg.AV_LOG_TRACE // 56
}
