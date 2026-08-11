using FFmpeg.AutoGen;
using FFmpeg.Collections;
using FFmpeg.Unsafe;

namespace FFmpeg.SideData;

internal readonly unsafe struct FrameSideData_ref : IAVPointer<_AVFrameSideData>
{
    private readonly _AVFrameSideData** context;


    internal FrameSideData_ref(_AVFrameSideData** context) => this.context = context;


    public AVMultiDictionary_ref Metadata => new(&CheckDisposed()->metadata);

    unsafe _AVFrameSideData* IAVPointer<_AVFrameSideData>.Pointer => context == null ? null : *context;


    public FrameSideDataType FrameSideDataType => (FrameSideDataType)CheckDisposed()->type;

    private _AVFrameSideData* CheckDisposed() => context == null || *context == null ? throw new ObjectDisposedException(GetType().FullName) : *context;
}

