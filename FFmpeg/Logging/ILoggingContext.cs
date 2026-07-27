namespace FFmpeg.Logging;

/// <summary>
/// Represents an object that exposes an FFmpeg <c>AVClass</c> for logging.
/// </summary>
/// <remarks>
/// This interface is implemented by types that can participate in FFmpeg's
/// logging system. The returned pointer is passed to FFmpeg logging functions
/// so that log messages can be associated with the corresponding native object.
/// </remarks>
public interface ILoggingContext
{
    /// <summary>
    /// Gets a pointer to the native <c>AVClass</c> associated with this object.
    /// </summary>
    /// <remarks>
    /// The returned pointer is intended for use with FFmpeg's logging API and
    /// should not be dereferenced or modified by user code.
    /// </remarks>
    unsafe void* AVClassPointer { get; }
}