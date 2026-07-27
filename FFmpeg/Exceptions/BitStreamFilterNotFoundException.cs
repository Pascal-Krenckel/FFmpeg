using FFmpeg.Utils;

namespace FFmpeg.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an FFmpeg bitstream filter cannot be found.
/// </summary>
/// <remarks>
/// This exception corresponds to the <see cref="AVResult32.BitstreamFilterNotFound"/> FFmpeg error code.
/// It is typically thrown when attempting to create or access a bitstream filter that is unavailable in the current FFmpeg build.
/// </remarks>
public class BitStreamFilterNotFoundException : FFmpegException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BitStreamFilterNotFoundException"/> class.
    /// </summary>
    public BitStreamFilterNotFoundException() : base(AVResult32.BitstreamFilterNotFound)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitStreamFilterNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BitStreamFilterNotFoundException(string message) : base(AVResult32.BitstreamFilterNotFound, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitStreamFilterNotFoundException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public BitStreamFilterNotFoundException(string message, Exception innerException) : base(AVResult32.BitstreamFilterNotFound, message, innerException)
    {
    }
}
