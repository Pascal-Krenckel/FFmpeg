using FFmpeg.Utils;

namespace FFmpeg.Exceptions;

public class DecoderNotFoundException : FFmpegException
{
    public DecoderNotFoundException() : base(AVResult32.DecoderNotFound)
    {
    }

    public DecoderNotFoundException(string message) : base(AVResult32.DecoderNotFound, message)
    {
    }

    public DecoderNotFoundException(string message, Exception innerException) : base(AVResult32.DecoderNotFound, message, innerException)
    {
    }
}
