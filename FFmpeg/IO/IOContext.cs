using FFmpeg.Formats;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.IO;
/// <summary>
/// Represents a custom FFmpeg I/O context backed by managed code.
/// </summary>
/// <remarks>
/// <see cref="IOContext"/> provides the callback implementations used by
/// FFmpeg's <c>AVIOContext</c>, allowing media data to be read from or written
/// to arbitrary managed sources such as <see cref="Stream"/> instances,
/// memory buffers, network transports, or custom storage.
/// </remarks>
public abstract unsafe class IOContext : AVIOContext
{
    private GCHandle gch;

    /// <summary>
    /// Gets the <see cref="Formats.FormatContext"/> associated with this I/O context.
    /// </summary>
    /// <remarks>
    /// This property is assigned when the I/O context is attached to a format
    /// context and remains valid until the context is disposed.
    /// </remarks>
    public FormatContext FormatContext { get; private set; }

    #region Constructors

    /// <summary>
    /// Initializes a new <see cref="IOContext"/> and attaches it to the specified
    /// format context.
    /// </summary>
    /// <param name="formatContext">The <see cref="Formats.FormatContext"/> to associate with this I/O context.</param>
    /// <param name="options">The I/O operations that this context will support, such as reading, writing, and seeking.</param>
    /// <param name="buffer_size">The size of the buffer to allocate for I/O operations. Defaults to 32,768 bytes.</param>
    protected IOContext(FormatContext formatContext, IOOptions options, int buffer_size = 32768)
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

    /// <summary>
    /// Initializes a new <see cref="IOContext"/>
    /// </summary>
    public IOContext() { FormatContext = null!; }

    /// <summary>
    /// Attaches this I/O context to the specified format context.
    /// </summary>
    /// <param name="formatContext">
    /// The format context that will use this custom I/O implementation.
    /// </param>
    /// <param name="options">
    /// Specifies which callback operations should be enabled.
    /// </param>
    /// <param name="buffer_size">
    /// The size, in bytes, of the internal FFmpeg I/O buffer.
    /// </param>
    /// <remarks>
    /// This method allocates a native <c>AVIOContext</c> and installs the managed
    /// callback functions required by FFmpeg.
    /// </remarks>
    public void InitContext(FormatContext formatContext, IOOptions options, int buffer_size = 32768)
    {
        FormatContext = formatContext;
        if (!gch.IsAllocated)
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

    /// <summary>
    /// Gets a value indicating whether this I/O context supports seeking.
    /// </summary>
    /// <remarks>
    /// If this property returns <see langword="false"/>, FFmpeg treats the stream
    /// as non-seekable.
    /// </remarks>
    public abstract bool CanSeek { get; }

    #region statics IO

    /// <summary>
    /// Static method to read data from the stream into the specified buffer.
    /// </summary>
    /// <param name="opaque">A pointer to the user data (this object).</param>
    /// <param name="buffer">The buffer to read data into.</param>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>
    /// The number of bytes read, or an FFmpeg error code if the operation failed.
    /// Returning zero indicates end-of-file and is automatically translated to
    /// <see cref="AVResult32.EndOfFile"/>.
    /// </returns>
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
    /// <returns>
    /// The number of bytes written, or a negative FFmpeg error code.
    /// </returns>
    private static int WritePacket(void* opaque, byte* buffer, int count) =>
        ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target).WritePacket(new Span<byte>(buffer, count));

    /// <summary>
    /// Static method to seek to a specific position in the stream.
    /// </summary>
    /// <param name="opaque">A pointer to the user data (this object).</param>
    /// <param name="offset">The offset to seek to.</param>
    /// <param name="whence">The seek mode (e.g., from the start, from the current position, etc.).</param>
    /// <returns>
    /// The new stream position, or a negative FFmpeg error code.
    /// </returns>
    private static long Seek(void* opaque, long offset, int whence) =>
        ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target).Seek(offset, (AVSeek)whence);

    #endregion

    #region abstract IO

    /// <summary>
    /// Reads data into the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read data into.</param>
    /// <returns>
    /// The number of bytes read, zero to indicate end-of-file, or a negative
    /// FFmpeg error code.
    /// </returns>
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
    /// <remarks>
    /// This method is called only if <see cref="CanSeek"/> is
    /// <see langword="true"/> and the context was initialized with
    /// <see cref="IOOptions.Seek"/>.
    /// </remarks>
    protected abstract AVResult64 Seek(long offset, AVSeek whence);

    /// <summary>
    /// Gets or sets the types of seeking supported by the underlying I/O context.
    /// </summary>
    /// <remarks>
    /// The value indicates which seeking operations are supported by the underlying
    /// <see cref="AVIOContext"/>. Derived classes can modify this value when creating
    /// or configuring a custom I/O context.
    /// </remarks>
    public Seekable Seekable
    {
        get => (Seekable)FormatContext.Context->pb->seekable;
        protected set => FormatContext.Context->pb->seekable = (int)value;
    }   
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
            if (FormatContext?.Context != null)
            {
                if (FormatContext.Context->pb != null)
                    AutoGen.ffmpeg.av_freep(&FormatContext.Context->pb->buffer);
                AutoGen.ffmpeg.avio_context_free(&FormatContext.Context->pb);
            }
            disposedValue = true;
            base.Dispose(disposing);
        }
    }
    /// <inheritdoc />
    ~IOContext() => Dispose(false);
    #endregion
}