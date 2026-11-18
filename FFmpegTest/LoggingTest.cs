using FFmpeg.Logging;

namespace FFmpegTest;

[TestClass]
[DoNotParallelize] // the logger and its level are process-wide
public class LoggingTest
{
    private readonly List<(LogLevel Level, string Message)> messages = [];
    private LogLevel previousLevel;

    [TestInitialize]
    public void Initialize()
    {
        previousLevel = Logger.Level;
        Logger.LogMessageReceived += LogMessage;
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Runs even when an assertion failed, so other tests see the original state.
        Logger.LogMessageReceived -= LogMessage;
        Logger.Level = previousLevel;
    }

    [TestMethod]
    public void TestLogging()
    {
        Logger.Level = LogLevel.Debug;
        Logger.WriteLine(LogLevel.Debug, "Test message Debug");
        Logger.WriteLine(LogLevel.Info, "Test message Info");
        Logger.WriteLine(LogLevel.Trace, "Test message Trace"); // above the level: filtered out

        Assert.HasCount(2, messages);
        Assert.AreEqual(LogLevel.Debug, messages[0].Level);
        Assert.AreEqual("Test message Debug\n", messages[0].Message);
        Assert.AreEqual(LogLevel.Info, messages[1].Level);
        Assert.AreEqual("Test message Info\n", messages[1].Message);
    }

    [TestMethod]
    public void LevelFiltersMessages()
    {
        Logger.Level = LogLevel.Warning;
        Logger.WriteLine(LogLevel.Info, "filtered");
        Logger.WriteLine(LogLevel.Error, "kept");

        Assert.HasCount(1, messages);
        Assert.AreEqual(LogLevel.Error, messages[0].Level);
        Assert.AreEqual("kept\n", messages[0].Message);
    }

    private void LogMessage(string message, LogLevel level, ClassCategory category, string? contextName)
        => messages.Add((level, message));
}
