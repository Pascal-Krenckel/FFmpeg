using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.HW;
/// <summary>
/// Represents a hardware device context in FFmpeg, encapsulating the buffer reference for hardware acceleration.
/// </summary>
public unsafe class FramesContext : IDisposable, IEquatable<FramesContext?>
{
    /// <summary>
    /// A pointer to the FFmpeg buffer reference that holds the hardware device context data.
    /// </summary>
    internal AutoGen._AVBufferRef* buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FramesContext"/> class by creating a reference to the provided buffer.
    /// </summary>
    /// <param name="buffer">A pointer to the <see cref="AutoGen._AVBufferRef"/> buffer reference.</param>
    internal FramesContext(AutoGen._AVBufferRef* buffer) =>
        // Increase the reference count for the buffer
        this.buffer = buffer;

    /// <summary>
    /// Releases the resources used by this <see cref="FramesContext"/> instance and un-references the buffer.
    /// </summary>
    public void Dispose()
    {
        // Un-reference the buffer when disposing the object
        AutoGen._AVBufferRef* buf = buffer;
        ffmpeg.av_buffer_unref(&buf);
        buffer = null;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="FramesContext"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="FramesContext"/>.</param>
    /// <returns>true if the specified object is equal to the current <see cref="FramesContext"/>; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as FramesContext);

    /// <summary>
    /// Determines whether the specified <see cref="FramesContext"/> is equal to the current one.
    /// </summary>
    /// <param name="other">The <see cref="FramesContext"/> to compare with.</param>
    /// <returns>true if the specified <see cref="FramesContext"/> is equal to the current one; otherwise, false.</returns>
    public bool Equals(FramesContext? other) =>
        // Compare the data pointers of the buffers
        other is not null && EqualityComparer<nint>.Default.Equals((nint)buffer->data, (nint)other.buffer->data);

    /// <summary>
    /// Returns a hash code for the current <see cref="FramesContext"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="FramesContext"/>.</returns>
    public override int GetHashCode() => HashCode.Combine((nint)buffer->data);

    /// <summary>
    /// Determines whether two <see cref="FramesContext"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="FramesContext"/> to compare.</param>
    /// <param name="right">The second <see cref="FramesContext"/> to compare.</param>
    /// <returns>true if the specified instances are equal; otherwise, false.</returns>
    public static bool operator ==(FramesContext? left, FramesContext? right) => EqualityComparer<FramesContext?>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="FramesContext"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="FramesContext"/> to compare.</param>
    /// <param name="right">The second <see cref="FramesContext"/> to compare.</param>
    /// <returns>true if the specified instances are not equal; otherwise, false.</returns>
    public static bool operator !=(FramesContext? left, FramesContext? right) => !(left == right);
}
