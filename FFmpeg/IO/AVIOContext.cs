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
    /// Pointer to the unmanaged AVIOContext structure.
    /// </summary>
    private AutoGen._AVIOContext** context;
    unsafe _AVIOContext* IAVPointer<_AVIOContext>.Pointer => *context;

    /// <summary>
    /// Gets a value indicating whether the end of the file (EOF) has been reached.
    /// </summary>
    public bool EOF => Convert.ToBoolean((*context)->eof_reached);

    /// <summary>
    /// Gets the error code associated with the AVIOContext.
    /// If no error occurred, this will be >=0.
    /// </summary>
    public AVResult32 ErrorCode => (*context)->error;
    private bool close = false;
    /// <summary>
    /// Initializes a new instance of the <see cref="AVIOContext"/> class with the specified AVIOContext pointer.
    /// </summary>
    /// <param name="ctx">Pointer to the unmanaged AVIOContext structure.</param>
    internal AVIOContext(AutoGen._AVIOContext** ctx) => context = ctx;

    // You have to use SetContext
    internal AVIOContext() { }

    /// <summary>
    /// Replaces the underlying unmanaged <see cref="AutoGen._AVIOContext"/>.
    /// </summary>
    /// <param name="context">
    /// A pointer to the new unmanaged context.
    /// </param>
    /// <remarks>
    /// If this instance already owns a different context, the previous context is released
    /// before the new one is assigned.
    /// This method is intended for derived classes that create custom AVIO contexts.
    /// </remarks>
    protected void SetContext(AutoGen._AVIOContext** context)
    {
        if (this.context != null && context != this.context)
            ffmpeg.avio_context_free(this.context);
        this.context = context;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the AVIOContext and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (context != null)
        {
            if (close)
                _ = ffmpeg.avio_closep(context);
            else
                ffmpeg.avio_context_free(context);

            context = null;
        }

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
    /// <param name="pb">
    /// Receives a pointer to the newly allocated unmanaged <see cref="AutoGen._AVIOContext"/>.
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
    internal static AVResult32 Open(AutoGen._AVIOContext** pb, string filename, int flags, out AVIOContext? ioContext)
    {
        int res = ffmpeg.avio_open(pb, filename, flags);
        ioContext = res >= 0 ? new AVIOContext(pb) { close = true } : null;
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
    public virtual void Flush() => ffmpeg.avio_flush(*context);
}

