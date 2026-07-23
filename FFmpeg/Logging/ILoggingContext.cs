using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Logging;

public interface ILoggingContext
{
    public unsafe void* AVClassPointer { get; } 

   
}
