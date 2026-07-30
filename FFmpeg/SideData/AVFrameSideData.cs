using FFmpeg.AutoGen;
using FFmpeg.Collections;
using FFmpeg.Unsafe;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FFmpeg.SideData;

internal unsafe readonly struct FrameSideData_ref : IAVPointer<_AVFrameSideData>
{
    readonly _AVFrameSideData** context;


    internal FrameSideData_ref(_AVFrameSideData** context) => this.context = context;


    public AVMultiDictionary_ref Metadata => new(&CheckDisposed()->metadata);

    unsafe _AVFrameSideData* IAVPointer<_AVFrameSideData>.Pointer => context == null ? null : *context;


    public FrameSideDataType FrameSideDataType => (FrameSideDataType)CheckDisposed()->type;

    private _AVFrameSideData* CheckDisposed()
    {        
        if (context == null || *context == null)
            throw new ObjectDisposedException(GetType().FullName);
        return *context;        
    }
}

