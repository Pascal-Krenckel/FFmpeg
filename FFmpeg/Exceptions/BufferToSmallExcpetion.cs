using FFmpeg.Utils;

namespace FFmpeg.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an FFmpeg operation fails because the provided buffer is too small.
/// </summary>
/// <remarks>
/// This exception corresponds to the <see cref="AVResult32.BufferTooSmall"/> FFmpeg error code.
/// It is typically thrown when a destination buffer does not have sufficient capacity to hold the requested data.
/// </remarks>
public class BufferTooSmallException : FFmpegException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BufferTooSmallException"/> class.
    /// </summary>
    public BufferTooSmallException() : base(AVResult32.BufferTooSmall)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferTooSmallException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BufferTooSmallException(string message) : base(AVResult32.BufferTooSmall, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferTooSmallException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public BufferTooSmallException(string message, Exception innerException) : base(AVResult32.BufferTooSmall, message, innerException)
    {
    }
}
