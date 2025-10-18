namespace FFmpeg.IO;
/// <summary>
/// Flags representing different input/output operations that can be performed on a stream or file.
/// These flags can be combined using a bitwise OR, except for <see cref="Read"/> and <see cref="Write"/>, which are mutually exclusive.
/// </summary>
[Flags]
public enum IOOptions
{
    /// <summary>
    /// Indicates that the stream or file is open for reading.
    /// Note: This flag cannot be combined with <see cref="Write"/>.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Indicates that the stream or file is open for writing.
    /// Note: This flag cannot be combined with <see cref="Read"/>.
    /// </summary>
    Write = 2,

    /// <summary>
    /// Indicates that the stream or file supports seeking.
    /// </summary>
    Seek = 4,
}
