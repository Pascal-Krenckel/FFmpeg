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

    // The delegates handed to native code must stay alive for as long as any
    // AVIOContext may call them. Storing them in static fields keeps them rooted;
    // a delegate created inline would be collectable as soon as the constructor
    // returned, and the next native read/write/seek would jump into freed memory.
    private static readonly AutoGen.avio_alloc_context_read_packet s_readDelegate = ReadPacket;
    private static readonly AutoGen.avio_alloc_context_write_packet s_writeDelegate = WritePacket;
    private static readonly AutoGen.avio_alloc_context_seek s_seekDelegate = Seek;
    private static readonly AutoGen.avio_alloc_context_read_packet_func s_read = s_readDelegate;
    private static readonly AutoGen.avio_alloc_context_write_packet_func s_write = s_writeDelegate;
    private static readonly AutoGen.avio_alloc_context_seek_func s_seek = s_seekDelegate;

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
    {
        FormatContext = null!;
        Attach(formatContext, options, buffer_size);
    }

    /// <summary>
    /// Initializes a new <see cref="IOContext"/>
    /// </summary>
    public IOContext() => FormatContext = null!;

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
        => Attach(formatContext, options, buffer_size);

    private void Attach(FormatContext formatContext, IOOptions options, int bufferSize)
    {
        if (formatContext == null)
            throw new ArgumentNullException(nameof(formatContext));
        if (formatContext.Context == null)
            throw new ObjectDisposedException(nameof(Formats.FormatContext));
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        // Release a native context this instance may still hold from an earlier attach;
        // this also detaches it from that earlier format context.
        ReleaseNative();

        // Release whatever I/O the format context used before.
        if (formatContext.ioContext != null && formatContext.ioContext != this)
            formatContext.ioContext.Dispose();
        if (formatContext.Context->pb != null)
            AutoGen.ffmpeg.avio_context_free(&formatContext.Context->pb);

        if (!gch.IsAllocated)
            gch = GCHandle.Alloc(this);

        AutoGen.avio_alloc_context_read_packet_func _read = options.HasFlag(IOOptions.Read)
            ? s_read
            : null;

        AutoGen.avio_alloc_context_write_packet_func _write = options.HasFlag(IOOptions.Write)
            ? s_write
            : null;

        AutoGen.avio_alloc_context_seek_func _seek = options.HasFlag(IOOptions.Seek)
            ? s_seek
            : null;

        byte* buffer = (byte*)AutoGen.ffmpeg.av_malloc((ulong)bufferSize);
        if (buffer == null)
            throw new OutOfMemoryException("Failed to allocate the I/O buffer.");

        AutoGen._AVIOContext* pb = AutoGen.ffmpeg.avio_alloc_context(
            buffer,
            bufferSize,
            Convert.ToInt32(options.HasFlag(IOOptions.Write)),
            (void*)GCHandle.ToIntPtr(gch),
            _read,
            _write,
            _seek
        );
        if (pb == null)
        {
            AutoGen.ffmpeg.av_free(buffer);
            throw new OutOfMemoryException("Failed to allocate the I/O context.");
        }

        SetContext(pb, formatContext, ownsBuffer: true);
        formatContext.Context->pb = pb;
        formatContext.ioContext = this;
        FormatContext = formatContext;
        disposedValue = false;

        if (pendingSeekable is Seekable seekable)
            Seekable = seekable;
    }

    private Seekable? pendingSeekable;
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
        // Exceptions must never propagate into native FFmpeg frames: on Linux/macOS
        // the runtime terminates the process, on Windows the behaviour is undefined.
        try
        {
            AVResult32 result = ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target!).ReadPacket(new Span<byte>(buffer, count));
            return result == 0 ? AVResult32.EndOfFile : result;
        }
        catch (Exception)
        {
            return AVResult32.IOError;
        }
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
    private static int WritePacket(void* opaque, byte* buffer, int count)
    {
        try
        {
            return ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target!).WritePacket(new Span<byte>(buffer, count));
        }
        catch (Exception)
        {
            return AVResult32.IOError;
        }
    }

    /// <summary>
    /// Static method to seek to a specific position in the stream.
    /// </summary>
    /// <param name="opaque">A pointer to the user data (this object).</param>
    /// <param name="offset">The offset to seek to.</param>
    /// <param name="whence">The seek mode (e.g., from the start, from the current position, etc.).</param>
    /// <returns>
    /// The new stream position, or a negative FFmpeg error code.
    /// </returns>
    private static long Seek(void* opaque, long offset, int whence)
    {
        try
        {
            return ((IOContext)GCHandle.FromIntPtr((nint)opaque).Target!).Seek(offset, (AVSeek)whence);
        }
        catch (Exception)
        {
            return AVResult64.IOError;
        }
    }

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
        get => FormatContext != null && FormatContext.Context != null && FormatContext.Context->pb != null
            ? FormatContext.Seekable
            : pendingSeekable ?? Seekable.None;
        protected set
        {
            // Constructors that defer InitContext() have no native context yet;
            // remember the value and apply it once the context is attached.
            pendingSeekable = value;
            if (FormatContext == null || FormatContext.Context == null || FormatContext.Context->pb == null)
                return;
            FormatContext.Context->pb->seekable = (int)value;
        }
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
            // Frees our own AVIOContext (and its buffer) and clears the owning format
            // context's pb. On the finalizer path a live format context does this itself
            // after closing; the GCHandle is freed in OnNativeReleased either way.
            base.Dispose(disposing);
            disposedValue = disposing || !gch.IsAllocated;
        }
    }

    /// <inheritdoc />
    private protected override void OnNativeReleased()
    {
        // Native code can no longer call back into this instance.
        if (gch.IsAllocated)
            gch.Free();
    }
    // No finalizer here: AVIOContext's finalizer already calls Dispose(false).
    #endregion
}