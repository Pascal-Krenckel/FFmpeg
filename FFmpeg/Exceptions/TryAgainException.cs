using FFmpeg.Utils;

namespace FFmpeg.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an FFmpeg operation cannot be completed
/// immediately and should be tried again.
/// </summary>
/// <remarks>
/// This exception corresponds to the <see cref="AVResult32.TryAgain"/> FFmpeg error code,
/// which is typically associated with the <c>EAGAIN</c> error condition.
/// It indicates that the operation is temporarily unable to proceed and may succeed
/// when attempted again later.
/// </remarks>
public class TryAgainException : FFmpegException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TryAgainException"/> class.
    /// </summary>
    public TryAgainException() : base(AVResult32.TryAgain)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TryAgainException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TryAgainException(string message) : base(AVResult32.TryAgain, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TryAgainException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public TryAgainException(string message, Exception innerException) : base(AVResult32.TryAgain, message, innerException)
    {
    }
}