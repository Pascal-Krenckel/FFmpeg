using System.Runtime.InteropServices;

namespace FFmpeg.Logging;

public static unsafe class Logger
{
    private static readonly object _lock = new();
    static Logger()
    {
        _ = LogFunc;
        //ffmpeg.av_log_set_callback(logCallback);
        ffmpeg.av_log_set_level(ffmpeg.AV_LOG_ERROR);

    }


    public static void Log(LogLevel logLevel, string message, params object[] args)
    {
        message = string.Format(message, args);
        if (!message.EndsWith("\n"))
            ffmpeg.av_log(null, (int)logLevel, message + "\n");
        else
            ffmpeg.av_log(null, (int)logLevel, message);
    }

    public static void Error(string message, params object[] args) => Log(LogLevel.Error, message, args);

    public static void Panic(string message, params object[] args) => Log(LogLevel.Panic, message, args);
    public static void Fatal(string message, params object[] args) => Log(LogLevel.Fatal, message, args);
    public static void Warning(string message, params object[] args) => Log(LogLevel.Warning, message, args);
    public static void Info(string message, params object[] args) => Log(LogLevel.Info, message, args);
    public static void Verbose(string message, params object[] args) => Log(LogLevel.Verbose, message, args);
    public static void Debug(string message, params object[] args) => Log(LogLevel.Debug, message, args);
    public static void Trace(string message, params object[] args) => Log(LogLevel.Trace, message, args);
    public static void Quiet(string message, params object[] args) => Log(LogLevel.Quiet, message, args);

    public static void Log(IFormatProvider formatProvider, LogLevel logLevel, string message, params object[] args)
    {
        message = string.Format(formatProvider, message, args);
        if (!message.EndsWith("\n"))
            ffmpeg.av_log(null, (int)logLevel, message + "\n");
        else
            ffmpeg.av_log(null, (int)logLevel, message);
    }

    public static void Error(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Error, message, args);

    public static void Panic(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Panic, message, args);

    public static void Fatal(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Fatal, message, args);

    public static void Warning(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Warning, message, args);

    public static void Info(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Info, message, args);

    public static void Verbose(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Verbose, message, args);

    public static void Debug(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Debug, message, args);

    public static void Trace(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Trace, message, args);

    public static void Quiet(IFormatProvider formatProvider, string message, params object[] args) => Log(formatProvider, LogLevel.Quiet, message, args);

    public static LogLevel LogLevel
    {
        get => (LogLevel)ffmpeg.av_log_get_level();
        set => ffmpeg.av_log_set_level((int)value);
    }

    private static readonly object @lock = new();
    private static unsafe void LogFunc(void* avcl, int level, string ch, byte* va_list)
    {
        if (level > ffmpeg.av_log_get_level() || ch == null)
            return;
        int printPrefix = 1;
        int size = ffmpeg.av_log_format_line2(avcl, level, ch, va_list, null, 0, &printPrefix);

        byte* bstr = stackalloc byte[size];
        printPrefix = 1;
        _ = ffmpeg.av_log_format_line2(avcl, level, ch, va_list, bstr, size, &printPrefix);

        string fmt = Marshal.PtrToStringUTF8((nint)bstr);

        OnLoggingReceived?.Invoke(fmt, (LogLevel)level);

    }

    public delegate void LogCallback(string errorMsg, LogLevel level);

    public static event LogCallback? OnLoggingReceived;

}
