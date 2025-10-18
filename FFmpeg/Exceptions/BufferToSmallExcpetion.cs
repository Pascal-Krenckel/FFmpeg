using System;
using System.Collections.Generic;
using System.Text;
using FFmpeg.Utils;

namespace FFmpeg.Exceptions;
public class BufferToSmallExcpetion : FFmpegException
{
    public BufferToSmallExcpetion() : base(AVResult32.BufferTooSmall)
    {
    }

    public BufferToSmallExcpetion(string message) : base(AVResult32.BufferTooSmall, message)
    {
    }

    public BufferToSmallExcpetion(string message, Exception innerException) : base(AVResult32.BufferTooSmall, message, innerException)
    {
    }
}
