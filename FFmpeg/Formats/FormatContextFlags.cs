namespace FFmpeg.Formats;

/// <summary>
/// Flags that specify the characteristics of a format context in FFmpeg.
/// These flags provide information about the presence of headers and the seekability of the stream.
/// </summary>
[Flags]
public enum FormatContextFlags : int
{
    /// <summary>
    /// Indicates that no header is present in the format context. 
    /// Streams may be added dynamically after initialization. 
    /// This flag is useful for formats where the header is not known at the time of opening,
    /// and additional streams can be added or removed as needed.
    /// </summary>
    NoHeader = ffmpeg.AVFMTCTX_NOHEADER,

    /// <summary>
    /// Indicates that the stream is not seekable. 
    /// Attempts to call the seek function on this stream will fail. 
    /// This flag is commonly used for network protocols or streaming formats (e.g., HLS)
    /// where the seekability of the stream can change dynamically during playback.
    /// </summary>
    Unseekable = ffmpeg.AVFMTCTX_UNSEEKABLE,
}