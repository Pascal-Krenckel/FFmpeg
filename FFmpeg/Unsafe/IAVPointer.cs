namespace FFmpeg.Unsafe;

public unsafe interface IAVPointer<T> where T : unmanaged
{
    T* Pointer { get; }
}

