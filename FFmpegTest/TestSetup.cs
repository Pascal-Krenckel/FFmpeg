namespace FFmpegTest;

/// <summary>
/// One-time setup shared by all tests.
/// </summary>
[TestClass]
public static class TestSetup
{
    /// <summary>
    /// Loads the FFmpeg libraries once, before any test runs, so no test depends on
    /// another test class having initialized them first.
    /// </summary>
    [AssemblyInitialize]
    public static void Initialize(TestContext _) => FFmpeg.FFmpegLoader.Initialize();
}

/// <summary>
/// Paths to the files in Test-Files, resolved against the test output directory so they
/// work regardless of the working directory and on every OS.
/// </summary>
internal static class TestFiles
{
    public static string Directory { get; } = Path.Combine(AppContext.BaseDirectory, "Test-Files");

    public static string VideoProbeJson => Get("mp4-example-video-download-full-hd-1920x1080.1min.mp4.json");
    public static string Bgra => Get("n123.bgra");
    public static string Yuv => Get("n123.yuv");
    public static string AudioS16le => Get("10s-11s.s16le.pcm");
    public static string AudioF32le => Get("10s-11s.f32le.pcm");

    public static string Get(string fileName) => Path.Combine(Directory, fileName);
}
