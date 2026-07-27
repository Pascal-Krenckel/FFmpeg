namespace FFmpeg.Unsafe;

/// <summary>
/// Represents an object that exposes a pointer to an underlying unmanaged FFmpeg structure.
/// </summary>
/// <typeparam name="T">
/// The unmanaged structure type pointed to by the native pointer.
/// </typeparam>
/// <remarks>
/// This interface is implemented by wrapper types that provide direct access to
/// the underlying native FFmpeg structure. It is primarily intended for advanced
/// scenarios where interoperability with the FFmpeg C API is required.
/// </remarks>
public unsafe interface IAVPointer<T> where T : unmanaged
{
    /// <summary>
    /// Gets the native pointer to the underlying unmanaged structure.
    /// </summary>
    /// <remarks>
    /// The returned pointer is owned by the implementing object and remains
    /// valid only for the lifetime of that object.
    /// </remarks>
    T* Pointer { get; }
}

