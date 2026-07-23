using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Logging;

/// <summary>
/// Provides access to the global FFmpeg logging system.
/// </summary>
/// <remarks>
/// FFmpeg logging is process-wide. Setting the log level, flags, or callback affects
/// all FFmpeg components using the same native library instance.
/// </remarks>
public static class Logger
{
    private static readonly AutoGen.av_log_set_callback_callback _callback;

    unsafe static Logger()
    {
        _callback = LogCallback;
        ffmpeg.av_log_set_callback(_callback);
    }

    /// <summary>
    /// Occurs when FFmpeg produces a log message.
    /// </summary>
    /// <remarks>
    /// If no handlers are registered, FFmpeg's default logging callback is used.
    /// Messages include the formatting applied by FFmpeg and may contain a trailing
    /// newline character.
    /// </remarks>
    public static event LogMessageEventHandler? MessageLogged;

    /// <summary>
    /// Gets or sets whether FFmpeg log prefixes are included in messages.
    /// </summary>
    /// <remarks>
    /// When enabled, messages may contain additional information such as the
    /// logging context name and other FFmpeg-generated prefixes.
    /// </remarks>
    public static bool IncludePrefix { get; set; } = true;

    private static unsafe void LogCallback(
        void* avcl,
        int logLevel,
        byte* format,
        byte* arguments)
    {
        if (logLevel > (int)Level)
            return;

        if (MessageLogged == null)
        {
            ffmpeg.av_log_default_callback(avcl, logLevel, format, arguments);
            return;
        }

        int printPrefix = IncludePrefix ? 1 : 0;

        int size = ffmpeg.av_log_format_line2(
            avcl,
            logLevel,
            format,
            arguments,
            null,
            0,
            &printPrefix);

        if (size <= 0)
            return;

        byte* buffer = stackalloc byte[size + 1];

        size = ffmpeg.av_log_format_line2(
            avcl,
            logLevel,
            format,
            arguments,
            buffer,
            size + 1,
            &printPrefix);

        string message = Encoding.UTF8.GetString(buffer, size);

        if (avcl != null)
        {
            string contextName = ffmpeg.av_default_item_name(avcl);
            ClassCategory category =
                (ClassCategory)ffmpeg.av_default_get_category(avcl);

            MessageLogged?.Invoke(
                message,
                (LogLevel)logLevel,
                category,
                contextName);
        }
        else
        {
            MessageLogged?.Invoke(
                message,
                (LogLevel)logLevel,
                ClassCategory.None,
                null);
        }
    }

    /// <summary>
    /// Gets or sets the minimum FFmpeg log level that will be emitted.
    /// </summary>
    public static LogLevel Level
    {
        get => (LogLevel)ffmpeg.av_log_get_level();
        set => ffmpeg.av_log_set_level((int)value);
    }

    /// <summary>
    /// Gets or sets FFmpeg logging behavior flags.
    /// </summary>
    public static LogFlags Options
    {
        get => (LogFlags)ffmpeg.av_log_get_flags();
        set => ffmpeg.av_log_set_flags((int)value);
    }

    /// <summary>
    /// Writes a message to the FFmpeg logging system.
    /// </summary>
    /// <param name="level">
    /// The FFmpeg log level assigned to the message.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    /// <remarks>
    /// This method writes the message exactly as provided and does not append a
    /// newline character.
    /// </remarks>
    public static unsafe void Write(LogLevel level, string message) => Write(null, level, message);

    /// <summary>
    /// Writes a message followed by a newline character to the FFmpeg logging system.
    /// </summary>
    /// <param name="level">
    /// The FFmpeg log level assigned to the message.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void WriteLine(LogLevel level, string message)
        => Write(level, message + "\n");

    /// <summary>
    /// Writes a debug-level message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Debug(string message) => WriteLine(LogLevel.Debug, message);

    /// <summary>
    /// Writes an informational message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Info(string message) => WriteLine(LogLevel.Info, message);

    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Error(string message) => WriteLine(LogLevel.Error, message);

    /// <summary>
    /// Writes a panic-level message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Panic(string message) => WriteLine(LogLevel.Panic, message);

    /// <summary>
    /// Writes a fatal-level message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Fatal(string message) => WriteLine(LogLevel.Fatal, message);

    /// <summary>
    /// Writes a verbose-level message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Verbose(string message) => WriteLine(LogLevel.Verbose, message);

    /// <summary>
    /// Writes a trace-level message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Trace(string message) => WriteLine(LogLevel.Trace, message);

    /// <summary>
    /// Writes a warning-level message.
    /// </summary>
    /// <param name="message">The message text to write.</param>
    public static void Warning(string message) => WriteLine(LogLevel.Warning, message);

    /// <summary>
    /// Writes a message to the FFmpeg logging system using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context, or <see langword="null"/>
    /// to use a context-free log message.
    /// </param>
    /// <param name="level">
    /// The FFmpeg log level assigned to the message.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    /// <remarks>
    /// The logging context allows FFmpeg to include object-specific information such as
    /// the component name and class category in the log callback.
    /// This method writes the message exactly as provided and does not append a newline.
    /// </remarks>
    public static unsafe void Write(
        ILoggingContext? loggingContext,
        LogLevel level,
        string message)
    {
        void* avcl = loggingContext != null && loggingContext.AVClassPointer != null
            ? loggingContext.AVClassPointer
            : null;

        message = message.Replace("%", "%%");
        ffmpeg.av_log(avcl, (int)level, message);
    }

    /// <summary>
    /// Writes a message followed by a newline character to the FFmpeg logging system
    /// using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context, or <see langword="null"/>
    /// to use a context-free log message.
    /// </param>
    /// <param name="level">
    /// The FFmpeg log level assigned to the message.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void WriteLine(
        ILoggingContext? loggingContext,
        LogLevel level,
        string message)
        => Write(loggingContext, level, message + "\n");

    /// <summary>
    /// Writes a debug-level message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Debug(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Debug, message);

    /// <summary>
    /// Writes an informational message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Info(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Info, message);

    /// <summary>
    /// Writes a warning-level message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Warning(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Warning, message);

    /// <summary>
    /// Writes an error-level message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Error(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Error, message);

    /// <summary>
    /// Writes a fatal-level message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Fatal(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Fatal, message);

    /// <summary>
    /// Writes a panic-level message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Panic(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Panic, message);

    /// <summary>
    /// Writes a verbose-level message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Verbose(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Verbose, message);

    /// <summary>
    /// Writes a trace-level message using a specific logging context.
    /// </summary>
    /// <param name="loggingContext">
    /// The FFmpeg object providing the logging context.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Trace(
        ILoggingContext loggingContext,
        string message)
        => WriteLine(loggingContext, LogLevel.Trace, message);
}

/// <summary>
/// Represents a handler for FFmpeg log messages.
/// </summary>
/// <param name="message">
/// The formatted log message produced by FFmpeg.
/// </param>
/// <param name="level">
/// The log level assigned to the message.
/// </param>
/// <param name="category">
/// The FFmpeg class category of the logging context.
/// </param>
/// <param name="contextName">
/// The name of the FFmpeg logging context, or <see langword="null"/> when unavailable.
/// </param>
public delegate void LogMessageEventHandler(
    string message,
    LogLevel level,
    ClassCategory category,
    string? contextName);