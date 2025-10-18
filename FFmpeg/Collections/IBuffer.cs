namespace FFmpeg.Collections;
/// <summary>
/// Defines the interface for buffer management, providing methods for 
/// accessing and manipulating data, managing reference counts, and handling 
/// buffer memory operations.
/// </summary>
public interface IBuffer
{
    /// <summary>
    /// Gets or sets the byte value at the specified index in the buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the byte to get or set.</param>
    /// <returns>The byte at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of bounds.</exception>
    /// <exception cref="NullReferenceException">Thrown when the buffer is <see langword="null"/>.</exception>
    byte this[int index] { get; set; }

    /// <summary>
    /// Gets the length of the buffer in bytes.
    /// </summary>
    /// <value>The number of bytes in the buffer.</value>
    int Length { get; }

    /// <summary>
    /// Gets a reference to the underlying buffer, represented as a pointer to <see cref="AutoGen._AVBufferRef"/>.
    /// </summary>
    /// <value>A pointer to the buffer reference.</value>
    internal unsafe AutoGen._AVBufferRef* Reference { get; }

    /// <summary>
    /// Gets the reference count of the buffer. Returns -1 if the buffer reference is <see langword="null"/>.
    /// </summary>
    /// <value>The number of references currently held to the buffer, or -1 if the reference is <see langword="null"/>.</value>
    int ReferenceCount { get; }

    /// <summary>
    /// Returns a span representing the buffer's byte data.
    /// </summary>
    /// <returns>A <see cref="Span{T}"/> of bytes representing the buffer's data.</returns>
    Span<byte> AsSpan();

    /// <summary>
    /// Creates a new copy of the current buffer with the same data.
    /// </summary>
    /// <returns>A new instance of <see cref="AVBuffer"/> containing a copy of the buffer data.</returns>
    /// <exception cref="NullReferenceException">Thrown when the buffer is <see langword="null"/>.</exception>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails during the copy.</exception>
    AVBuffer Copy();

    /// <summary>
    /// Clones the current buffer reference into a new buffer instance.
    /// </summary>
    /// <returns>A new <see cref="AVBuffer"/> that shares the same data reference as the current buffer.</returns>
    /// <exception cref="NullReferenceException">Thrown when the buffer is <see langword="null"/>.</exception>
    AVBuffer Clone();

    /// <summary>
    /// Ensures that the buffer is writable, creating a copy if necessary.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when the buffer is <see langword="null"/>.</exception>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails during write preparation.</exception>
    void MakeWriteable();

    /// <summary>
    /// Replaces the current buffer with another buffer.
    /// </summary>
    /// <param name="src">The source buffer that will replace the current buffer.</param>
    /// <exception cref="NullReferenceException">Thrown when the source buffer is <see langword="null"/>.</exception>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails during the replacement.</exception>
    void ReplaceWith(IBuffer src);

    /// <summary>
    /// Resizes the buffer to the specified size.
    /// </summary>
    /// <param name="size">The new size for the buffer in bytes.</param>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails during resizing.</exception>
    void Resize(int size);

    /// <summary>
    /// Attempts to make the buffer writable, and returns whether the operation was successful.
    /// </summary>
    /// <returns><see langword="true"/> if the buffer was successfully made writable; otherwise, <see langword="false"/>.</returns>
    bool TryMakeWriteable();

    /// <summary>
    /// Attempts to replace the current buffer with another buffer, and returns whether the operation was successful.
    /// </summary>
    /// <param name="src">The source buffer to replace the current one.</param>
    /// <returns><see langword="true"/> if the replacement was successful; otherwise, <see langword="false"/>.</returns>
    bool TryReplaceWith(IBuffer src);

    /// <summary>
    /// Attempts to resize the buffer to the specified size, and returns whether the operation was successful.
    /// </summary>
    /// <param name="size">The new size for the buffer in bytes.</param>
    /// <returns><see langword="true"/> if the resizing was successful; otherwise, <see langword="false"/>.</returns>
    bool TryResize(int size);

    /// <summary>
    /// Gets a value indicating whether the buffer reference is <see langword="null"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the underlying <see cref="AutoGen._AVBufferRef"/> pointer is <see langword="null"/>, 
    /// indicating the absence of a valid buffer reference; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This property helps in determining if the buffer has been allocated or assigned a valid reference. 
    /// A <see langword="null"/> reference typically indicates that the buffer has not been initialized or has been released.
    /// </remarks>
    bool IsNull { get; }

    /// <summary>
    /// Gets the pointer to the buffer's data. Returns <see cref="IntPtr.Zero"/> if the buffer reference is <see langword="null"/>.
    /// </summary>
    nint Data { get; }
}

