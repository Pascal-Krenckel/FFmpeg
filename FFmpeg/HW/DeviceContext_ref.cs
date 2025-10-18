using FFmpeg.Utils;
using System.Diagnostics;

namespace FFmpeg.HW;

/// <summary>
/// Represents a pointer to an FFmpeg hardware device context buffer reference. 
/// This is used to manage hardware-accelerated contexts in FFmpeg.
/// </summary>
public readonly unsafe struct DeviceContext_ref : IEquatable<DeviceContext_ref>, Utils.IReference<DeviceContext>
{
    /// <summary>
    /// Pointer to the buffer reference for the hardware device context.
    /// </summary>
    internal readonly AutoGen._AVBufferRef** buffer;

    /// <summary>
    /// Determines whether the <see cref="DeviceContext_ref"/> is read-only.
    /// When <see langword="true"/>, the device context cannot be modified.
    /// </summary>
    private readonly bool isReadOnly;

    /// <summary>
    /// Indicates whether the device context is read-only.
    /// If <see langword="true"/>, attempting to modify the context will result in a <see cref="NotSupportedException"/>.
    /// </summary>
    public bool IsReadOnly => isReadOnly;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceContext_ref"/> struct.
    /// </summary>
    /// <param name="buffer">A pointer to the buffer reference containing the device context.</param>
    /// <param name="isReadOnly">Indicates if the context is read-only.</param>
    internal DeviceContext_ref(AutoGen._AVBufferRef** buffer, bool isReadOnly)
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
    public override bool Equals(object? obj) => obj == null ? *buffer == null : obj is DeviceContext_ref d && Equals(d);

    /// <summary>
    /// Determines whether the current <see cref="DeviceContext_ref"/> is equal to another instance of <see cref="DeviceContext_ref"/>.
    /// </summary>
    /// <param name="other">The other <see cref="DeviceContext_ref"/> to compare with.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(DeviceContext_ref other) => EqualityComparer<nint>.Default.Equals((nint)buffer, (nint)other.buffer);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine((nint)buffer);

    /// <inheritdoc />
    public DeviceContext? GetReferencedObject()
    {
        AutoGen._AVBufferRef* dev = ffmpeg.av_buffer_ref(*buffer);
        return buffer == null
            ? throw new OutOfMemoryException("Failed to reference the buffer for the new device context.")
            : new DeviceContext(dev);
    }

    /// <inheritdoc />
    public void SetReferencedObject(DeviceContext? device)
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
    /// Determines whether two <see cref="DeviceContext_ref"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(DeviceContext_ref left, DeviceContext_ref right) => EqualityComparer<DeviceContext_ref>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="DeviceContext_ref"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(DeviceContext_ref left, DeviceContext_ref right) => !(left == right);

    /// <summary>
    /// Determines whether two <see cref="DeviceContext_ref"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(DeviceContext_ref? left, DeviceContext_ref? right) => !left.HasValue
            ? !right.HasValue || *right.Value.buffer == null
            : !right.HasValue ? *left.Value.buffer == null : EqualityComparer<DeviceContext_ref>.Default.Equals(left.Value, right.Value);

    /// <summary>
    /// Determines whether two <see cref="DeviceContext_ref"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(DeviceContext_ref? left, DeviceContext_ref? right) => !(left == right);

}

