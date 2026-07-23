namespace FFmpeg.Logging;

/// <summary>
/// Provides extension methods for writing log messages using FFmpeg logging contexts.
/// </summary>
/// <remarks>
/// These methods forward messages to <see cref="Logger"/> while automatically using
/// the object implementing <see cref="ILoggingContext"/> as the FFmpeg logging context.
/// </remarks>
public static class LoggingContextExtensions
{
    /// <summary>
    /// Writes a message using the specified log level and FFmpeg logging context.
    /// </summary>
    /// <param name="context">
    /// The FFmpeg object used as the logging context.
    /// </param>
    /// <param name="level">
    /// The FFmpeg log level assigned to the message.
    /// </param>
    /// <param name="message">
    /// The message text to write.
    /// </param>
    public static void Log(
        this ILoggingContext context,
        LogLevel level,
        string message)
        => Logger.WriteLine(context, level, message);

    /// <summary>
    /// Writes a debug-level message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Debug(
        this ILoggingContext context,
        string message)
        => Logger.Debug(context, message);

    /// <summary>
    /// Writes a verbose-level message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Verbose(
        this ILoggingContext context,
        string message)
        => Logger.Verbose(context, message);

    /// <summary>
    /// Writes a trace-level message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Trace(
        this ILoggingContext context,
        string message)
        => Logger.Trace(context, message);

    /// <summary>
    /// Writes an informational message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Info(
        this ILoggingContext context,
        string message)
        => Logger.Info(context, message);

    /// <summary>
    /// Writes a warning-level message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Warning(
        this ILoggingContext context,
        string message)
        => Logger.Warning(context, message);

    /// <summary>
    /// Writes an error-level message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Error(
        this ILoggingContext context,
        string message)
        => Logger.Error(context, message);

    /// <summary>
    /// Writes a fatal-level message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Fatal(
        this ILoggingContext context,
        string message)
        => Logger.Fatal(context, message);

    /// <summary>
    /// Writes a panic-level message using this FFmpeg logging context.
    /// </summary>
    /// <param name="context">The FFmpeg logging context.</param>
    /// <param name="message">The message text to write.</param>
    public static void Panic(
        this ILoggingContext context,
        string message)
        => Logger.Panic(context, message);
}