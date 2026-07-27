using FFmpeg.Utils;

namespace FFmpeg.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an FFmpeg decoder cannot be found.
/// </summary>
/// <remarks>
/// This exception corresponds to the <see cref="AVResult32.DecoderNotFound"/> FFmpeg error code.
/// It is typically thrown when attempting to open a codec for decoding that is not available
/// in the current FFmpeg build or has not been registered.
/// </remarks>
public class DecoderNotFoundException : FFmpegException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DecoderNotFoundException"/> class.
    /// </summary>
    public DecoderNotFoundException() : base(AVResult32.DecoderNotFound)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DecoderNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DecoderNotFoundException(string message) : base(AVResult32.DecoderNotFound, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DecoderNotFoundException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public DecoderNotFoundException(string message, Exception innerException) : base(AVResult32.DecoderNotFound, message, innerException)
    {
    }
}