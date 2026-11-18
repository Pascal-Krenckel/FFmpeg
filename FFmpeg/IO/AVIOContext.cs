using FFmpeg.AutoGen;
using FFmpeg.Unsafe;
using FFmpeg.Utils;

namespace FFmpeg.IO;

/// <summary>
/// Represents a managed wrapper around FFmpeg's unmanaged <see cref="AutoGen._AVIOContext"/> structure.
/// </summary>
/// <remarks>
/// An <see cref="AVIOContext"/> provides buffered input/output operations for media data.
/// It can represent file-based I/O, network streams, memory buffers, or custom user-defined
/// data sources. Instances own the underlying unmanaged context and release it when disposed.
/// </remarks>
public unsafe class AVIOContext : IDisposable, IAVPointer<AutoGen._AVIOContext>
{
    /// <summary>
    /// Pointer to the unmanaged AVIOContext structure owned by this instance.
    /// </summary>
    /// <remarks>
    /// This is the context itself, not the address of a format context's <c>pb</c> field:
    /// the format context can be freed by FFmpeg (e.g. when <c>avformat_open_input</c> fails),
    /// and a pointer into it would then dangle.
    /// </remarks>
    private AutoGen._AVIOContext* context;

    /// <summary>
    /// The format context whose <c>pb</c> field currently points at <see cref="context"/>, if any.
    /// </summary>
    private Formats.FormatContext? owner;

    /// <summary>
    /// <see langword="true"/> if the context was opened with <c>avio_open</c> and must be closed with <c>avio_closep</c>.
    /// </summary>
    private bool close;

    /// <summary>
    /// <see langword="true"/> if the I/O buffer was allocated by this wrapper and must be freed with the context.
    /// </summary>
    private bool ownsBuffer;

    unsafe _AVIOContext* IAVPointer<_AVIOContext>.Pointer => context;

    /// <summary>
    /// Gets the unmanaged context, or throws if it has already been released.
    /// </summary>
    private protected AutoGen._AVIOContext* NativeContext
        => context != null ? context : throw new ObjectDisposedException(GetType().Name);

    /// <summary>
    /// Gets a value indicating whether an unmanaged context is currently attached.
    /// </summary>
    private protected bool HasNativeContext => context != null;

    /// <summary>
    /// Gets a value indicating whether the end of the file (EOF) has been reached.
    /// </summary>
    public bool EOF => Convert.ToBoolean(NativeContext->eof_reached);

    /// <summary>
    /// Gets the error code associated with the AVIOContext.
    /// If no error occurred, this will be >=0.
    /// </summary>
    public AVResult32 ErrorCode => NativeContext->error;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVIOContext"/> class with the specified AVIOContext pointer.
    /// </summary>
    /// <param name="ctx">Pointer to the unmanaged AVIOContext structure.</param>
    /// <param name="owner">The format context whose <c>pb</c> field points at <paramref name="ctx"/>, if any.</param>
    /// <param name="close"><see langword="true"/> if the context was opened with <c>avio_open</c>.</param>
    internal AVIOContext(AutoGen._AVIOContext* ctx, Formats.FormatContext? owner, bool close)
    {
        context = ctx;
        this.owner = owner;
        this.close = close;
    }

    // You have to use SetContext
    internal AVIOContext() { }

    /// <summary>
    /// Replaces the underlying unmanaged <see cref="AutoGen._AVIOContext"/>.
    /// </summary>
    /// <param name="context">The new unmanaged context, allocated with <c>avio_alloc_context</c>.</param>
    /// <param name="owner">The format context whose <c>pb</c> field points at <paramref name="context"/>, if any.</param>
    /// <param name="ownsBuffer"><see langword="true"/> if the context's buffer must be freed together with the context.</param>
    /// <remarks>
    /// If this instance already owns a different context, the previous context is released
    /// before the new one is assigned.
    /// This method is intended for derived classes that create custom AVIO contexts.
    /// </remarks>
    private protected void SetContext(AutoGen._AVIOContext* context, Formats.FormatContext? owner, bool ownsBuffer)
    {
        if (this.context != context)
            ReleaseNative();
        this.context = context;
        this.owner = owner;
        this.ownsBuffer = ownsBuffer;
        close = false;
    }

    /// <summary>
    /// Frees the unmanaged context (if any) and detaches it from its owning format context.
    /// Safe to call more than once.
    /// </summary>
    /// <remarks>
    /// Format contexts call this on their finalizer path after they have closed themselves,
    /// because FFmpeg may still access <c>pb</c> while a format context is being closed.
    /// </remarks>
    internal void ReleaseNative()
    {
        if (context == null)
            return;

        // Never leave the format context pointing at freed memory.
        if (owner != null && owner.Context != null && owner.Context->pb == context)
            owner.Context->pb = null;

        AutoGen._AVIOContext* ctx = context;
        context = null;
        owner = null;

        if (close)
        {
            _ = ffmpeg.avio_closep(&ctx);
        }
        else
        {
            // FFmpeg may have replaced the buffer we passed in, so free the current one.
            if (ownsBuffer)
                ffmpeg.av_freep(&ctx->buffer);
            ffmpeg.avio_context_free(&ctx);
        }

        OnNativeReleased();
    }

    /// <summary>
    /// Called after the unmanaged context has been released.
    /// </summary>
    private protected virtual void OnNativeReleased() { }

    /// <summary>
    /// Releases the unmanaged resources used by the AVIOContext and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        // On the finalizer path, a format context that is still alive releases this context
        // itself once it has been closed (finalization order between the two is undefined).
        if (!disposing && owner != null && owner.Context != null)
            return;

        ReleaseNative();
    }

    /// <summary>
    /// Finalizer that releases unmanaged resources before the object is reclaimed by garbage collection.
    /// </summary>
    ~AVIOContext()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing).
        Dispose(disposing: false);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing).
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Opens an FFmpeg I/O context for the specified URL or file.
    /// </summary>
    /// <param name="owner">
    /// The format context whose <c>pb</c> field receives the newly opened <see cref="AutoGen._AVIOContext"/>.
    /// </param>
    /// <param name="filename">
    /// The URL or file path to open.
    /// </param>
    /// <param name="flags">
    /// The access mode and additional flags used when opening the context.
    /// </param>
    /// <param name="ioContext">
    /// When this method returns successfully, contains the managed wrapper around the opened
    /// I/O context; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating whether the operation succeeded.
    /// </returns>
    internal static AVResult32 Open(Formats.FormatContext owner, string filename, int flags, out AVIOContext? ioContext)
    {
        int res = ffmpeg.avio_open(&owner.Context->pb, filename, flags);
        ioContext = res >= 0 ? new AVIOContext(owner.Context->pb, owner, close: true) : null;
        return res;
    }
    // ToDo: avio_open2

    /// <summary>
    /// Flushes any buffered output data to the underlying destination.
    /// </summary>
    /// <remarks>
    /// This method has an effect only for writable I/O contexts.
    /// For read-only contexts, calling this method typically has no effect.
    /// </remarks>
    public virtual void Flush() => ffmpeg.avio_flush(NativeContext);
}

