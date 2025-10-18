using FFmpeg.Utils;

namespace FFmpeg.IO;

/// <summary>
/// Represents a managed wrapper for the unmanaged FFmpeg AVIOContext structure.
/// This class provides access to various properties and methods related to AVIOContext.
/// </summary>
public unsafe class AVIOContext : IDisposable
{
    /// <summary>
    /// Pointer to the unmanaged AVIOContext structure.
    /// </summary>
    private AutoGen._AVIOContext** context;


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

    protected void SetContext(AutoGen._AVIOContext** context)
    {
        if (this.context == null && context != this.context)
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

    internal static AVResult32 Open(AutoGen._AVIOContext** pb, string filename, int flags, out AVIOContext? ioContext)
    {
        int res = ffmpeg.avio_open(pb, filename, flags);
        ioContext = res >= 0 ? new AVIOContext(pb) { close = true } : null;
        return res;
    }
}

