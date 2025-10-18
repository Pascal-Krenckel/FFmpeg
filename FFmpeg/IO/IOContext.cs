using FFmpeg.Formats;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.IO;
/// <summary>
/// Represents an abstract base class for handling custom I/O operations in FFmpeg.
/// This class encapsulates an FFmpeg AVIOContext and manages its lifecycle and memory management.
/// </summary>
public abstract unsafe class IOContext : AVIOContext
{
    private GCHandle gch;

    /// <summary>
    /// Gets the <see cref="Formats.FormatContext"/> associated with this I/O context.
    /// </summary>
    public FormatContext FormatContext { get; private set; }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="IOContext"/> class with the specified format context, I/O options, and buffer size.
    /// </summary>
    /// <param name="formatContext">The <see cref="Formats.FormatContext"/> to associate with this I/O context.</param>
    /// <param name="options">The I/O operations that this context will support, such as reading, writing, and seeking.</param>
    /// <param name="buffer_size">The size of the buffer to allocate for I/O operations. Defaults to 32,768 bytes.</param>
    public IOContext(FormatContext formatContext, IOOptions options, int buffer_size = 32768)
        : base(&formatContext.Context->pb)
    {
        FormatContext = formatContext;
        gch = GCHandle.Alloc(this);
        formatContext.ioContext?.Dispose();

        if (formatContext.Context->pb != null)
            AutoGen.ffmpeg.avio_context_free(&formatContext.Context->pb);

        AutoGen.avio_alloc_context_read_packet_func _read = options.HasFlag(IOOptions.Read)
            ? (AutoGen.avio_alloc_context_read_packet_func)IOContext.ReadPacket
            : null;

        AutoGen.avio_alloc_context_write_packet_func _write = options.HasFlag(IOOptions.Write)
            ? (AutoGen.avio_alloc_context_write_packet_func)IOContext.WritePacket
            : null;

        AutoGen.avio_alloc_context_seek_func _seek = options.HasFlag(IOOptions.Seek)
            ? (AutoGen.avio_alloc_context_seek_func)IOContext.Seek
            : null;

        formatContext.Context->pb = AutoGen.ffmpeg.avio_alloc_context(
            (byte*)AutoGen.ffmpeg.av_malloc((ulong)buffer_size),
            buffer_size,
            Convert.ToInt32(options.HasFlag(IOOptions.Write)),
            (void*)GCHandle.ToIntPtr(gch),
            _read,
            _write,
            _seek
        );

        FormatContext.ioContext = this;
    }
    public IOContext() { gch = GCHandle.Alloc(this); FormatContext = null!; }

    public void InitContext(FormatContext formatContext, IOOptions options, int buffer_size = 32768)
    {
        FormatContext = formatContext;
        gch = GCHandle.Alloc(this);
        formatContext.ioContext?.Dispose();

        if (formatContext.Context->pb != null)
            AutoGen.ffmpeg.avio_context_free(&formatContext.Context->pb);

        AutoGen.avio_alloc_context_read_packet_func _read = options.HasFlag(IOOptions.Read)
            ? (AutoGen.avio_alloc_context_read_packet_func)IOContext.ReadPacket
            : null;

        AutoGen.avio_alloc_context_write_packet_func _write = options.HasFlag(IOOptions.Write)
            ? (AutoGen.avio_alloc_context_write_packet_func)IOContext.WritePacket
            : null;

        AutoGen.avio_alloc_context_seek_func _seek = options.HasFlag(IOOptions.Seek)
            ? (AutoGen.avio_alloc_context_seek_func)IOContext.Seek
            : null;

        formatContext.Context->pb = AutoGen.ffmpeg.avio_alloc_context(
            (byte*)AutoGen.ffmpeg.av_malloc((ulong)buffer_size),
            buffer_size,
            Convert.ToInt32(options.HasFlag(IOOptions.Write)),
            (void*)GCHandle.ToIntPtr(gch),
            _read,
            _write,
            _seek
        );

        FormatContext.ioContext = this;
        SetContext(&FormatContext.Context->pb);
    }
    #endregion

    #region statics IO

    /// <summary>
    /// Static method to read data from the stream into the specified buffer.
    /// </summary>
    /// <param name="opaque">A pointer to the user data (this object).</param>
    /// <param name="buffer">The buffer to read data into.</param>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>The number of bytes read, or <see cref="AVResult32.EndOfFile"/> if the end of the file is reached.</returns>
    private static int ReadPacket(void* opaque, byte* buffer, int count)
    {
        AVResult32 result = ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target).ReadPacket(new Span<byte>(buffer, count));
        return result == 0 ? AVResult32.EndOfFile : result;
    }

    /// <summary>
    /// Static method to write data from the specified buffer into the stream.
    /// </summary>
    /// <param name="opaque">A pointer to the user data (this object).</param>
    /// <param name="buffer">The buffer containing the data to write.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <returns>The number of bytes written.</returns>
    private static int WritePacket(void* opaque, byte* buffer, int count) =>
        ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target).WritePacket(new Span<byte>(buffer, count));

    /// <summary>
    /// Static method to seek to a specific position in the stream.
    /// </summary>
    /// <param name="opaque">A pointer to the user data (this object).</param>
    /// <param name="offset">The offset to seek to.</param>
    /// <param name="whence">The seek mode (e.g., from the start, from the current position, etc.).</param>
    /// <returns>The new position within the stream, or a negative value on error.</returns>
    private static long Seek(void* opaque, long offset, int whence) =>
        ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target).Seek(offset, (AVSeek)whence);

    #endregion

    #region abstract IO

    /// <summary>
    /// Reads data into the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read data into.</param>
    /// <returns>The number of bytes read, or a negative value on error.</returns>
    protected abstract AVResult32 ReadPacket(Span<byte> buffer);

    /// <summary>
    /// Writes data from the provided buffer into the stream.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to write.</param>
    /// <returns>The number of bytes written, or a negative value on error.</returns>
    protected abstract AVResult32 WritePacket(Span<byte> buffer);

    /// <summary>
    /// Seeks to the specified offset in the stream.
    /// </summary>
    /// <param name="offset">The offset to seek to.</param>
    /// <param name="whence">The seek mode (e.g., from the start, from the current position, etc.).</param>
    /// <returns>The new position within the stream, or a negative value on error.</returns>
    protected abstract AVResult64 Seek(long offset, AVSeek whence);
    #endregion

    #region Dispose

    private bool disposedValue = false;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="IOContext"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (gch.IsAllocated)
                gch.Free();

            // Free the buffer and AVIOContext associated with the format context
            AutoGen.ffmpeg.av_freep(&FormatContext.Context->pb->buffer);
            AutoGen.ffmpeg.avio_context_free(&FormatContext.Context->pb);

            disposedValue = true;
            base.Dispose(disposing);
        }
    }

    ~IOContext() => Dispose(false);
    #endregion
}