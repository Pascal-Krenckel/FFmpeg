using FFmpeg.Utils;

namespace FFmpeg.Collections;

/// <summary>
/// Represents a wrapper for an AVBufferRef in FFmpeg, which manages reference-counted buffers.
/// Provides functionality for managing the lifetime, access, and manipulation of these buffers.
/// </summary>
public sealed unsafe class AVBuffer : IDisposable, IBuffer
{
    internal AutoGen._AVBufferRef* reference;
    AutoGen._AVBufferRef* IBuffer.Reference => reference;

    private AVBuffer() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AVBuffer"/> class with an existing buffer reference.
    /// </summary>
    /// <param name="ref">A pointer to an existing AVBufferRef.</param>
    internal AVBuffer(AutoGen._AVBufferRef* @ref) => reference = @ref;

    /// <summary>
    /// Gets the length of the buffer in bytes. Returns 0 if the buffer reference is <see langword="null"/>.
    /// </summary>
    public int Length => IsNull ? 0 : (int)reference->size;

    /// <summary>
    /// Gets a value indicating whether the buffer is read-only.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when the buffer reference is <see langword="null"/>.</exception>
    public bool IsReadOnly => IsNull ? throw new NullReferenceException() : AutoGen.ffmpeg.av_buffer_is_writable(reference) == 0;

    /// <summary>
    /// Gets the pointer to the buffer's data. Returns <see cref="IntPtr.Zero"/> if the buffer reference is <see langword="null"/>.
    /// </summary>
    public nint Data => IsNull ? IntPtr.Zero : (nint)reference->data;

    /// <summary>
    /// Gets the reference count of the buffer. Returns -1 if the buffer reference is <see langword="null"/>.
    /// </summary>
    public int ReferenceCount => IsNull ? -1 : AutoGen.ffmpeg.av_buffer_get_ref_count(reference);

    /// <summary>
    /// Gets a value indicating whether the buffer reference is <see langword="null"/>.
    /// </summary>
    public bool IsNull => reference == null;

    /// <summary>
    /// Indexer to access or modify a byte at the specified index in the buffer.
    /// </summary>
    /// <param name="index">The index of the byte to access or modify.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of bounds.</exception>
    public byte this[int index]
    {
        get => CheckIndex(index) ? reference->data[index] : throw new ArgumentOutOfRangeException(nameof(index)); set => reference->data[index] = CheckIndex(index) ? value : throw new ArgumentOutOfRangeException(nameof(index));
    }

    private bool CheckIndex(int index) => index >= 0 && index < Length;

    /// <summary>
    /// Returns a writable span of the buffer's data.
    /// </summary>
    /// <returns>A <see cref="Span{T}"/> representing the buffer's data. Returns an empty span if the buffer reference is <see langword="null"/>.</returns>
    public Span<byte> AsSpan() => IsNull ? Span<byte>.Empty : new(reference->data, Length);

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="AVBuffer"/>.
    /// </summary>
    public void Dispose()
    {
        AutoGen._AVBufferRef* @ref = reference;
        AutoGen.ffmpeg.av_buffer_unref(&@ref);
        reference = @ref;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure unmanaged resources are released when the object is garbage collected.
    /// </summary>
    ~AVBuffer()
    {
        Dispose();
    }

    /// <summary>
    /// Tries to make the buffer writable, if it is not already.
    /// </summary>
    /// <returns><see langword="true"/> if the buffer was successfully made writable; otherwise, <see langword="false"/>.</returns>
    public bool TryMakeWriteable()
    {
        if (IsNull)
            return false;
        AutoGen._AVBufferRef* @ref = reference;
        bool ret = AutoGen.ffmpeg.av_buffer_make_writable(&@ref) == 0;
        reference = @ref;
        return ret;
    }

    /// <summary>
    /// Ensures the buffer is writable, throwing an exception if it cannot be made writable.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if the buffer reference is <see langword="null"/>.</exception>
    /// <exception cref="OutOfMemoryException">Thrown when the buffer could not be made writable due to memory limitations.</exception>
    public void MakeWriteable()
    {
        if (IsNull)
            throw new NullReferenceException("Buffer reference is null.");
        if (!TryMakeWriteable())
            throw new OutOfMemoryException("Failed to make the buffer writable.");
    }

    /// <summary>
    /// Tries to resize the buffer to the specified size.
    /// </summary>
    /// <param name="size">The new size of the buffer in bytes.</param>
    /// <returns><see langword="true"/> if the buffer was successfully resized; otherwise, <see langword="false"/>.</returns>
    public bool TryResize(int size)
    {
        AutoGen._AVBufferRef* @ref = reference;
        bool ret = AutoGen.ffmpeg.av_buffer_realloc(&@ref, (ulong)size) == 0;
        reference = @ref;
        return ret;
    }

    /// <summary>
    /// Resizes the buffer to the specified size, throwing an exception if resizing fails.
    /// </summary>
    /// <param name="size">The new size of the buffer in bytes.</param>
    /// <exception cref="OutOfMemoryException">Thrown when the buffer could not be resized due to memory limitations.</exception>
    public void Resize(int size)
    {
        if (!TryResize(size))
            throw new OutOfMemoryException("Failed to resize the buffer.");
    }

    /// <summary>
    /// Tries to replace the current buffer with another buffer.
    /// </summary>
    /// <param name="src">The source buffer to replace with.</param>
    /// <returns><see langword="true"/> if the buffer was successfully replaced; otherwise, <see langword="false"/>.</returns>
    public bool TryReplaceWith(AVBuffer src)
    {
        AutoGen._AVBufferRef* @ref = reference;
        bool ret = AutoGen.ffmpeg.av_buffer_replace(&@ref, src.reference) == 0;
        reference = @ref;
        return ret;
    }

    /// <summary>
    /// Replaces the current buffer with another buffer, throwing an exception if replacement fails.
    /// </summary>
    /// <param name="src">The source buffer to replace with.</param>
    /// <exception cref="OutOfMemoryException">Thrown when the buffer could not be replaced due to memory limitations.</exception>
    public void ReplaceWith(AVBuffer src)
    {
        if (!TryReplaceWith(src))
            throw new OutOfMemoryException("Failed to replace the buffer.");
    }

    /// <summary>
    /// Clones the buffer, creating a new buffer that references the same data.
    /// </summary>
    /// <returns>A new <see cref="AVBuffer"/> that references the same data as the original.</returns>
    /// <exception cref="NullReferenceException">Thrown when the buffer reference is <see langword="null"/>.</exception>
    public AVBuffer Clone() => IsNull ? throw new NullReferenceException() : FromExisting(this);

    /// <summary>
    /// Allocates a new buffer of the specified size.
    /// </summary>
    /// <param name="size">The size of the buffer to allocate in bytes.</param>
    /// <returns>A new <see cref="AVBuffer"/> instance.</returns>
    /// <exception cref="OutOfMemoryException">Thrown when the buffer could not be allocated due to memory limitations.</exception>
    public static AVBuffer Allocate(int size)
    {
        AutoGen._AVBufferRef* b = ffmpeg.av_buffer_alloc((ulong)size);
        return b == null ? throw new OutOfMemoryException() : new(b);
    }

    /// <summary>
    /// Allocates a new zero-initialized buffer of the specified size.
    /// </summary>
    /// <param name="size">The size of the buffer to allocate in bytes.</param>
    /// <returns>A new <see cref="AVBuffer"/> instance with zeroed data.</returns>
    /// <exception cref="OutOfMemoryException">Thrown when the buffer could not be allocated due to memory limitations.</exception>
    public static AVBuffer AllocateZ(int size)
    {
        AutoGen._AVBufferRef* b = ffmpeg.av_buffer_allocz((ulong)size);
        return b == null ? throw new OutOfMemoryException() : new(b);
    }

    /// <summary>
    /// Creates a new buffer that references the same data as an existing buffer.
    /// </summary>
    /// <param name="buffer">The buffer to reference.</param>
    /// <returns>A new <see cref="AVBuffer"/> that references the same data as the input buffer.</returns>
    /// <exception cref="OutOfMemoryException">Thrown when the new buffer could not be created due to memory limitations.</exception>
    public static AVBuffer FromExisting(AVBuffer buffer)
    {
        AutoGen._AVBufferRef* b = ffmpeg.av_buffer_ref(buffer.reference);
        return b == null ? throw new OutOfMemoryException() : new(b);
    }

    internal static AVBuffer FromExisting(AutoGen._AVBufferRef* buffer)
    {
        AutoGen._AVBufferRef* b = ffmpeg.av_buffer_ref(buffer);
        return b == null ? throw new OutOfMemoryException() : new(b);
    }

    /// <summary>
    /// Copies the data from the current buffer into a new buffer.
    /// </summary>
    /// <returns>A new <see cref="AVBuffer"/> instance with copied data.</returns>
    /// <exception cref="NullReferenceException">Thrown if the current buffer reference is <see langword="null"/>.</exception>
    /// <exception cref="OutOfMemoryException">Thrown when the buffer could not be copied due to memory limitations.</exception>
    public AVBuffer Copy()
    {
        if (IsNull)
            throw new NullReferenceException();
        AVBuffer buffer = AVBuffer.Allocate(Length); // may throw OutOfMemoryException
        if (Length > 0)
            Buffer.MemoryCopy(reference->data, buffer.reference->data, Length, Length);
        return buffer;
    }

    /// <summary>
    /// Replaces the current buffer with another buffer, which can be any object implementing <see cref="IBuffer"/>.
    /// </summary>
    /// <param name="src">The source buffer implementing <see cref="IBuffer"/>.</param>
    /// <exception cref="NullReferenceException">Thrown when the source buffer is <see langword="null"/>.</exception>
    /// <exception cref="OutOfMemoryException">Thrown if the replace operation fails.</exception>
    public void ReplaceWith(IBuffer src)
    {
        if (src.IsNull)
            throw new NullReferenceException();
        AutoGen._AVBufferRef* @ref = reference;
        AVResult32 res = ffmpeg.av_buffer_replace(&@ref, src.Reference);
        res.ThrowIfError(); // may throw OutOfMemoryException
    }

    /// <summary>
    /// Tries to replace the current buffer with another buffer implementing <see cref="IBuffer"/>.
    /// </summary>
    /// <param name="src">The source buffer implementing <see cref="IBuffer"/>.</param>
    /// <returns><see langword="true"/> if the buffer was successfully replaced; otherwise, <see langword="false"/>.</returns>
    public bool TryReplaceWith(IBuffer src)
    {
        if (src.IsNull)
            return false;
        AutoGen._AVBufferRef* @ref = reference;
        return ffmpeg.av_buffer_replace(&@ref, src.Reference) >= 0;
    }
}
