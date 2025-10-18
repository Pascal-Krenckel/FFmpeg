using FFmpeg.Formats;
using FFmpeg.Utils;

namespace FFmpeg.IO;

/// <summary>
/// Represents an I/O context for interacting with a .NET <see cref="Stream"/> object.
/// This class supports reading from, writing to, and seeking within the stream.
/// </summary>
public class IOStreamContext : IOContext
{
    private readonly Stream stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="IOStreamContext"/> class.
    /// </summary>
    /// <param name="context">The <see cref="FormatContext"/> to associate with this I/O context.</param>
    /// <param name="stream">The .NET <see cref="Stream"/> to use for I/O operations.</param>
    /// <param name="options">The I/O operations to support (reading, writing, seeking).</param>
    /// <param name="bufferSize">The size of the buffer to allocate for I/O operations. Defaults to 32,768 bytes.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> is <c>null</c>.</exception>
    public IOStreamContext(FormatContext context, Stream stream, IOOptions options, int bufferSize = 32768)
        : base(context, options, bufferSize) => this.stream = stream ?? throw new ArgumentNullException(nameof(stream));

    public IOStreamContext(Stream stream) : base() => this.stream = stream;

    /// <summary>
    /// Creates an <see cref="IOStreamContext"/> for reading from the specified stream.
    /// </summary>
    /// <param name="context">The <see cref="FormatContext"/> to associate with this I/O context.</param>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="bufferSize">The size of the buffer to allocate for I/O operations. Defaults to 32,768 bytes.</param>
    /// <returns>A new <see cref="IOStreamContext"/> instance configured for reading.</returns>
    public static IOStreamContext OpenRead(FormatContext context, Stream stream, int bufferSize = 32768)
        => new(context, stream, stream.CanSeek ? IOOptions.Read | IOOptions.Seek : IOOptions.Read, bufferSize);

    /// <summary>
    /// Creates an <see cref="IOStreamContext"/> for writing to the specified stream.
    /// </summary>
    /// <param name="context">The <see cref="FormatContext"/> to associate with this I/O context.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="bufferSize">The size of the buffer to allocate for I/O operations. Defaults to 32,768 bytes.</param>
    /// <returns>A new <see cref="IOStreamContext"/> instance configured for writing.</returns>
    public static IOStreamContext OpenWrite(FormatContext context, Stream stream, int bufferSize = 32768)
        => new(context, stream, stream.CanSeek ? IOOptions.Write | IOOptions.Seek : IOOptions.Write, bufferSize);

    /// <summary>
    /// Reads data from the stream into the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read data into.</param>
    /// <returns>The number of bytes read. Returns a non-negative value on success.</returns>
    protected override AVResult32 ReadPacket(Span<byte> buffer)
    {
        int bytesRead = stream.Read(buffer);
        return bytesRead; // Return the number of bytes read (>= 0)
    }

    /// <summary>
    /// Seeks to the specified offset in the stream.
    /// </summary>
    /// <param name="offset">The offset to seek to.</param>
    /// <param name="whence">The seek mode (e.g., from the start, current position, etc.).</param>
    /// <returns>The new position within the stream. Returns a non-negative value on success.</returns>
    protected override AVResult64 Seek(long offset, AVSeek whence)
    {
        // Force is not supported, as the stream can't be reopened.
        whence &= ~AVSeek.Force;
        if (whence == AVSeek.Size)
            return stream.Length;

        // Convert AVSeek to SeekOrigin for the stream seek method
        return stream.Seek(offset, (SeekOrigin)whence);
    }

    /// <summary>
    /// Writes data from the provided buffer into the stream.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to write.</param>
    /// <returns>The number of bytes written. Returns a non-negative value on success.</returns>
    protected override AVResult32 WritePacket(Span<byte> buffer)
    {
        stream.Write(buffer);
        return buffer.Length; // Return the number of bytes written (>= 0)
    }

    /// <summary>
    /// Releases the resources used by this instance of <see cref="IOStreamContext"/>.
    /// </summary>
    /// <param name="disposing">Indicates whether to release both managed and unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose the managed stream
            stream.Dispose();
        }

        // Call the base class Dispose method
        base.Dispose(disposing);
    }
}
