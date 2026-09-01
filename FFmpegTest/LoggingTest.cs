using FFmpeg.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegTest;

[TestClass]
public class LoggingTest
{
    [TestInitialize]
    public void Initialize()
    {
        FFmpeg.FFmpegLoader.Initialize();
    }

    [TestMethod]
    public void TestLogging()
    {

        StringWriter sw = new();
        FFmpeg.Logging.Logger.Level = FFmpeg.Logging.LogLevel.Debug;
        FFmpeg.Logging.Logger.LogMessageReceived += LogMessage;
        Logger.WriteLine(FFmpeg.Logging.LogLevel.Debug, "Test message Debug");
        Logger.WriteLine(FFmpeg.Logging.LogLevel.Info, "Test message Info");
        Logger.WriteLine(FFmpeg.Logging.LogLevel.Trace, "Test message Trace");
        FFmpeg.Logging.Logger.LogMessageReceived -= LogMessage;

        Assert.HasCount(2, messages);
        Assert.AreEqual(FFmpeg.Logging.LogLevel.Debug, messages[0].Item1);
        Assert.AreEqual("Test message Debug\n", messages[0].Item2);
        Assert.AreEqual(FFmpeg.Logging.LogLevel.Info, messages[1].Item1);
        Assert.AreEqual("Test message Info\n", messages[1].Item2);
      
    }
    private List<(LogLevel, string)> messages = [];
    private void LogMessage(string message, LogLevel level, ClassCategory category, string? contextName)
    {
        messages.Add((level, message));
    }
}
