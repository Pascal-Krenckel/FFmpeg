using FFmpeg.Logging;
using FFmpeg.Utils;

namespace FFmpeg.Exceptions;
/// <summary>
/// Represents errors that occur during FFmpeg operations.
/// </summary>
public class FFmpegException : Exception
{


    /// <summary>
    /// Gets the FFmpeg error code.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class with a specified error code.
    /// </summary>
    /// <param name="error">The FFmpeg error code.</param>
    public FFmpegException(int error) => ErrorCode = error;

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class with a specified error code and error message.
    /// </summary>
    /// <param name="error">The FFmpeg error code.</param>
    /// <param name="message">The message that describes the error.</param>
    public FFmpegException(int error, string message) : base(message) => ErrorCode = error;

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class with a specified error code, error message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="error">The FFmpeg error code.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FFmpegException(int error, string message, Exception innerException) : base(message, innerException) => ErrorCode = error;

    /// <summary>
    /// Throws an <see cref="FFmpegException"/> if the specified error code is negative.
    /// </summary>
    /// <param name="error">The FFmpeg error code.</param>
    /// <exception cref="FFmpegException">Thrown if the error code is negative.</exception>
    public static void ThrowIfError(int error)
    {
        if (error >= 0)
            return;
        if (error == AVResult32.TryAgain)
            return; // ToDo:
        if (error == AVResult32.OptionNotFound)
            throw new OptionNotFoundException();
        if (error == AVResult32.OutOfMemory)
            throw new OutOfMemoryException();
        if (error == AVResult32.InvalidArgument)
            throw new ArgumentException("Invalid Argument", ExceptionLog.GetLog());
        if (error == AVResult32.OutOfRange)
            throw new ArgumentOutOfRangeException();
        if (error == AVResult32.EndOfFile)
            throw new EndOfStreamException();
        if (error == AVResult32.ArgumentListTooLong)
            throw new ArgumentException();
        if (error == AVResult32.BrokenPipe)
            throw new System.IO.IOException();
        if (error == AVResult32.BitstreamFilterNotFound)
            throw new BitStreamFilterNotFoundException();
        if (error == AVResult32.Bug || error == AVResult32.Bug2)
            throw new FFmpegException(error, ExceptionLog.GetLog());
        if (error == AVResult32.BufferTooSmall)
            throw new BufferToSmallExcpetion();
        if (error == AVResult32.DecoderNotFound)
            throw new DecoderNotFoundException(ExceptionLog.GetLog());
        throw new FFmpegException(error, ExceptionLog.GetLog());
        // ToDo: error

    }


}

