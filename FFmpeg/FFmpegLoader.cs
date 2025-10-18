namespace FFmpeg;

/// <summary>
/// Used to initialize the ffmpeg bindings
/// </summary>
public static class FFmpegLoader
{

    /// <summary>
    /// Initializes the bindings for ffmpeg.
    /// </summary>
    /// <param name="dir">The directory where the libraries are located. Use ';' as seperator for multiple directories. Use `null` to search for it.</param>
    /// <param name="throwIfFunctionNotFound">Whether an exception should be thrown when calling one of the functions couldn't be loaded.</param>
    /// <example>
    /// The following code searches AppContext.BaseDirectory/(ffmpeg/(win-x64|linux-arm32...)) and %PATH% <c>Initialize();</c>
    /// The following code specifies to different dirs <c>Initialize("dir1;dir2")</c>
    /// </example>

    public static void Initialize(string? dir = null, bool throwIfFunctionNotFound = true)
    {
        FFmpeg.AutoGen.ffmpeg.RootPath = dir;
        AutoGen.DynamicallyLoadedBindings.ThrowErrorIfFunctionNotFound = throwIfFunctionNotFound;
        AutoGen.DynamicallyLoadedBindings.Initialize();
    }
}
