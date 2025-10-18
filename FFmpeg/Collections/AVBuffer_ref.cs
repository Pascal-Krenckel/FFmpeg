using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Collections;
/// <summary>
/// Represents a reference to an AVBuffer, providing a set of operations for handling
/// buffer memory and metadata in a safe manner, while leveraging the underlying AVBufferRef structure.
/// </summary>
public unsafe readonly struct AVBuffer_ref : IBuffer, IEquatable<AVBuffer_ref>, Utils.IReference<AVBuffer>
{
    /// <summary>
    /// Internal pointer to the underlying AVBufferRef structure.
    /// </summary>
    internal readonly AutoGen._AVBufferRef** buffer;

    /// <summary>
    /// Gets the reference to the underlying AVBufferRef structure.
    /// </summary>
    AutoGen._AVBufferRef* IBuffer.Reference => *buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVBuffer_ref"/> struct.
    /// </summary>
    /// <param name="buffer">Pointer to the buffer reference.</param>
    /// <exception cref="NullReferenceException">Thrown when the buffer is null.</exception>
    internal AVBuffer_ref(AutoGen._AVBufferRef** buffer)
    {
        if (buffer == null) throw new NullReferenceException();
        this.buffer = buffer;
    }


    /// <summary>
    /// Gets a value indicating whether the buffer is read-only.
    /// </summary>
    public bool IsReadOnly => !Convert.ToBoolean(ffmpeg.av_buffer_is_writable(*buffer));

    /// <summary>
    /// Gets the reference count of the buffer.
    /// </summary>
    /// <returns>Number of references held to the buffer, or -1 if the buffer is null.</returns>
    public int ReferenceCount => *buffer != null ? ffmpeg.av_buffer_get_ref_count(*buffer) : -1;

    /// <summary>
    /// Gets a value indicating whether the buffer is uninitialized (null).
    /// </summary>
    public bool IsNull => *buffer == null;

    /// <summary>
    /// Gets the pointer to the buffer's data.
    /// </summary>
    public nint Data => (*buffer != null) ? (nint)(*buffer)->data : IntPtr.Zero;

    /// <summary>
    /// Gets the size of the buffer's data in bytes.
    /// </summary>
    public int Length => (*buffer != null) ? (int)(*buffer)->size : 0;

    /// <summary>
    /// Gets the size of the buffer's data in long format.
    /// </summary>
    public long LongLength => (*buffer != null) ? (long)(*buffer)->size : 0;

    /// <summary>
    /// Provides indexed access to the buffer's byte data.
    /// </summary>
    /// <param name="index">The zero-based index into the buffer.</param>
    /// <returns>The byte value at the specified index.</returns>
    /// <exception cref="NullReferenceException">Thrown when the buffer is uninitialized.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of bounds.</exception>
    public byte this[int index]
    {
        get => IsNull ? throw new NullReferenceException() : (index < 0 || index >= (int)(*buffer)->size) ? throw new ArgumentOutOfRangeException(nameof(index)) : (**buffer).data[index];
        set
        {
            if (IsNull)
                throw new NullReferenceException();
            else if (index < 0 || index >= (int)(*buffer)->size)
                throw new ArgumentOutOfRangeException(nameof(index));
            else
                (**buffer).data[index] = value;
        }
    }

    /// <summary>
    /// Returns the buffer's data as a span of bytes.
    /// </summary>
    /// <returns>A <see cref="Span{Byte}"/> representing the buffer's data.</returns>
    /// <exception cref="NullReferenceException">Thrown when the buffer is uninitialized.</exception>
    public Span<byte> AsSpan() => IsNull ? [] : new((**buffer).data, (int)(**buffer).size);

    /// <summary>
    /// Copies the buffer data to a new <see cref="AVBuffer"/> instance.
    /// </summary>
    /// <returns>A new <see cref="AVBuffer"/> containing the copied data.</returns>
    /// <exception cref="NullReferenceException">Thrown when the buffer is uninitialized.</exception>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails.</exception>
    public AVBuffer Copy()
    {
        if (this == null)
            throw new NullReferenceException();
        var buffer = AVBuffer.Allocate(Length); // may throw OutOfMemoryException        
        AsSpan().CopyTo(buffer.AsSpan());
        return buffer;
    }

    /// <summary>
    /// Clones the current buffer reference into a new <see cref="AVBuffer"/> instance.
    /// </summary>
    /// <returns>A cloned instance of the buffer.</returns>
    /// <exception cref="NullReferenceException">Thrown when the buffer is uninitialized.</exception>
    public AVBuffer Clone()
    {
        if (IsNull) throw new NullReferenceException();
        return GetReferencedObject()!;
    }

    /// <summary>
    /// Makes the buffer writable by ensuring a unique copy is made if necessary.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when the buffer is uninitialized.</exception>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails during write preparation.</exception>
    public void MakeWriteable()
    {
        if (IsNull)
            throw new NullReferenceException();
        Exceptions.FFmpegException.ThrowIfError(ffmpeg.av_buffer_make_writable(buffer)); // should throw OutOfMemoryException if error
    }

    /// <summary>
    /// Replaces the current buffer with another buffer.
    /// </summary>
    /// <param name="src">The source buffer to replace with.</param>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails during the replacement.</exception>
    public void ReplaceWith(IBuffer src)
    {
        FFmpeg.Exceptions.FFmpegException.ThrowIfError(ffmpeg.av_buffer_replace(buffer, src.Reference)); // OutOfMemoryException
    }

    /// <summary>
    /// Resizes the buffer to the specified size.
    /// </summary>
    /// <param name="size">The new size for the buffer.</param>
    /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails during resizing.</exception>
    public void Resize(int size)
    {
        FFmpeg.Exceptions.FFmpegException.ThrowIfError(ffmpeg.av_buffer_realloc(buffer, (ulong)size)); // outOfMemoryException
    }

    /// <summary>
    /// Attempts to make the buffer writable. Returns true if successful.
    /// </summary>
    public bool TryMakeWriteable() => ffmpeg.av_buffer_make_writable(buffer) >= 0;

    /// <summary>
    /// Attempts to replace the current buffer with another buffer. Returns true if successful.
    /// </summary>
    public bool TryReplaceWith(IBuffer src) => ffmpeg.av_buffer_replace(buffer, src.Reference) >= 0;

    /// <summary>
    /// Attempts to resize the buffer to the specified size. Returns true if successful.
    /// </summary>
    public bool TryResize(int size) => ffmpeg.av_buffer_realloc(buffer, (ulong)size) >= 0;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is AVBuffer_ref ptr && Equals(ptr);

    /// <inheritdoc/>
    public bool Equals(AVBuffer_ref other) => buffer == other.buffer || *buffer == *other.buffer;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((nint)buffer);

    /// <summary>
    /// Retrieves the <see cref="AVBuffer"/> object currently referenced by this <see cref="AVBuffer_ref"/>.
    /// </summary>
    /// <returns>
    /// The <see cref="AVBuffer"/> if the buffer reference is valid; otherwise, <c>null</c> if the reference is null or uninitialized.
    /// </returns>
    /// <remarks>
    /// This method provides a safe way to access the buffer being referenced without exposing the underlying unmanaged pointer.
    /// If the reference is null (i.e., <see cref="IsNull"/> is <c>true</c>), it returns <c>null</c>.
    /// </remarks>
    public AVBuffer? GetReferencedObject()
    {
        if (IsNull) return null;
        return AVBuffer.FromExisting(*buffer);
    }

    /// <summary>
    /// Sets the reference to a new <see cref="AVBuffer"/> object or unlinks the current reference.
    /// </summary>
    /// <param name="buffer">
    /// The new <see cref="AVBuffer"/> object to reference, or <c>null</c> to release the current reference.
    /// </param>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if the system cannot allocate enough memory to create a new buffer reference when setting a non-null object.
    /// </exception>
    /// <remarks>
    /// This method updates the underlying <see cref="AVBufferRef"/> pointer to reference a new buffer object.
    /// If <paramref name="buffer"/> is <c>null</c>, the current reference is unlinked using <c>ffmpeg.av_buffer_unref</c>.
    /// If a new object is provided, the reference is updated with <c>ffmpeg.av_buffer_ref</c>, and an <see cref="OutOfMemoryException"/> is thrown if the allocation fails.
    /// </remarks>
    public void SetReferencedObject(AVBuffer? buffer)
    {
        ffmpeg.av_buffer_unref(this.buffer);
        if (buffer != null)
        {
            *this.buffer = ffmpeg.av_buffer_ref(buffer.reference);
            if (IsNull) throw new OutOfMemoryException();
        }
    }



    /// <summary>
    /// Determines if two <see cref="AVBuffer_ref"/> instances are equal.
    /// </summary>
    public static bool operator ==(AVBuffer_ref left, AVBuffer_ref right) => left.Equals(right);

    /// <summary>
    /// Determines if two <see cref="AVBuffer_ref"/> instances are not equal.
    /// </summary>
    public static bool operator !=(AVBuffer_ref left, AVBuffer_ref right) => !(left == right);
}

