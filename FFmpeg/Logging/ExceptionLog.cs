using System.Text;

namespace FFmpeg.Logging;

public static class ExceptionLog
{
    [field: ThreadStatic]
    private static StringBuilder ExceptionString => field ??= new();

    static ExceptionLog() => Logger.OnLoggingReceived += Log_OnLoggingReceived;

    public static void Reset() => ExceptionString.Clear();

    private static void Log_OnLoggingReceived(string errorMsg, LogLevel level)
    {
        if (level <= LogLevel.Warning)
            _ = ExceptionString.AppendLine(errorMsg);
    }

    public static string GetLog() => ExceptionString.ToString();
}
