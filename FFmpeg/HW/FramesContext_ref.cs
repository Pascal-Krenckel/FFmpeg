using FFmpeg.Utils;
using System.Diagnostics;

namespace FFmpeg.HW;
/// <summary>
/// Represents a pointer to an FFmpeg hardware device context buffer reference. 
/// This is used to manage hardware-accelerated contexts in FFmpeg.
/// </summary>
public readonly unsafe struct FramesContext_ref : IEquatable<FramesContext_ref>, Utils.IReference<FramesContext>
{
    /// <summary>
    /// Pointer to the buffer reference for the hardware device context.
    /// </summary>
    internal readonly AutoGen._AVBufferRef** buffer;

    /// <summary>
    /// Determines whether the <see cref="FramesContext_ref"/> is read-only.
    /// When <see langword="true"/>, the device context cannot be modified.
    /// </summary>
    private readonly bool isReadOnly;

    /// <summary>
    /// Indicates whether the device context is read-only.
    /// If <see langword="true"/>, attempting to modify the context will result in a <see cref="NotSupportedException"/>.
    /// </summary>
    public bool IsReadOnly => isReadOnly;

    /// <summary>
    /// Initializes a new instance of the <see cref="FramesContext_ref"/> struct.
    /// </summary>
    /// <param name="buffer">A pointer to the buffer reference containing the device context.</param>
    /// <param name="isReadOnly">Indicates if the context is read-only.</param>
    internal FramesContext_ref(AutoGen._AVBufferRef** buffer, bool isReadOnly)
    {
        Debug.Assert(buffer != null);
        this.buffer = buffer;
        this.isReadOnly = isReadOnly;
    }

    /// <summary>
    /// Determines whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is FramesContext_ref d && Equals(d);

    /// <summary>
    /// Determines whether the current <see cref="FramesContext_ref"/> is equal to another instance of <see cref="FramesContext_ref"/>.
    /// </summary>
    /// <param name="other">The other <see cref="FramesContext_ref"/> to compare with.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(FramesContext_ref other) => EqualityComparer<nint>.Default.Equals((nint)buffer, (nint)other.buffer);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine((nint)buffer);

    /// <inheritdoc />
    public FramesContext? GetReferencedObject()
    {
        AutoGen._AVBufferRef* dev = ffmpeg.av_buffer_ref(*buffer);
        return buffer == null
            ? throw new OutOfMemoryException("Failed to reference the buffer for the new device context.")
            : new FramesContext(dev);
    }

    /// <inheritdoc />
    public void SetReferencedObject(FramesContext? device)
    {
        if (device == null)
        {
            ffmpeg.av_buffer_unref(buffer);
        }
        else
        {
            if (IsReadOnly)
                throw new NotSupportedException("Cannot modify a read-only device context.");
            AVResult32 result = ffmpeg.av_buffer_replace(buffer, device.buffer);
            result.ThrowIfError();
        }
    }

    /// <summary>
    /// Determines whether two <see cref="FramesContext_ref"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(FramesContext_ref left, FramesContext_ref right) => EqualityComparer<FramesContext_ref>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="FramesContext_ref"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(FramesContext_ref left, FramesContext_ref right) => !(left == right);

    /// <summary>
    /// Determines whether two <see cref="FramesContext_ref"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(FramesContext_ref? left, FramesContext_ref? right) => EqualityComparer<FramesContext_ref?>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="FramesContext_ref"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(FramesContext_ref? left, FramesContext_ref? right) => !(left == right);

}
