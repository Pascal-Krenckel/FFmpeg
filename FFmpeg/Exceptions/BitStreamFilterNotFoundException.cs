using System;
using System.Collections.Generic;
using System.Text;
using FFmpeg.Utils;

namespace FFmpeg.Exceptions;
public class BitStreamFilterNotFoundException : FFmpegException
{
    public BitStreamFilterNotFoundException() : base(AVResult32.BitstreamFilterNotFound)
    {
    }

    public BitStreamFilterNotFoundException(string message) : base(AVResult32.BitstreamFilterNotFound, message)
    {
    }

    public BitStreamFilterNotFoundException(string message, Exception innerException) : base(AVResult32.BitstreamFilterNotFound, message, innerException)
    {
    }
}
