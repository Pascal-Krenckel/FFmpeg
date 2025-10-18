namespace FFmpeg.IO;
/// <summary>
/// Flags used for seeking operations within a stream or file in FFmpeg.
/// These flags can be combined using a bitwise OR to modify the behavior of seeking functions.
/// </summary>
[Flags]
public enum AVSeek : int
{
    /// <summary>
    /// ORing this as the "whence" parameter to a seek function causes it to return the filesize without actually seeking anywhere.
    /// Supporting this is optional; if not supported, the seek function will return a negative value.
    /// </summary>
    Size = ffmpeg.AVSEEK_SIZE,

    /// <summary>
    /// Passing this flag as the "whence" parameter to a seek function allows seeking by any means necessary.
    /// This can include reopening, linear reading, or other normally unreasonable methods that can be extremely slow.
    /// </summary>
    Force = ffmpeg.AVSEEK_FORCE,

    /// <summary>
    /// Specifies the beginning of the stream as the reference point for the seek operation.
    /// </summary>
    /// <inheritdoc cref="SeekOrigin.Begin"/>
    Begin = SeekOrigin.Begin,

    /// <summary>
    /// Specifies the end of the stream as the reference point for the seek operation.
    /// </summary>
    /// <inheritdoc cref="SeekOrigin.End"/>
    End = SeekOrigin.End,

    /// <summary>
    /// Specifies the current position within the stream as the reference point for the seek operation.
    /// </summary>
    /// <inheritdoc cref="SeekOrigin.Current"/>
    Current = SeekOrigin.Current,
}

