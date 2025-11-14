using FFmpeg.IO;
using FFmpeg.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FFmpeg.Unmanaged;

public unsafe interface IAVPointer<T> where T : unmanaged
{
    T* Pointer { get; }
}

