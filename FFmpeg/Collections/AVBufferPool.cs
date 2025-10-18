namespace FFmpeg.Collections;

/// <summary>
/// Represents a pool of <see cref="AVBuffer"/> objects for efficient memory management.
/// </summary>
/// <remarks>
/// This class is a wrapper around the FFmpeg `AVBufferPool` structure, providing a mechanism to
/// allocate and manage a pool of buffers. It implements <see cref="IDisposable"/> to ensure that
/// resources are properly released when the pool is no longer needed.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="AVBufferPool"/> class with the specified buffer size.
/// </remarks>
/// <param name="size">The size of each buffer in the pool, in bytes.</param>
/// <remarks>
/// This constructor creates a new buffer pool using FFmpeg's `av_buffer_pool_init` function.
/// The pool will manage buffers of the specified size.
/// </remarks>
public sealed unsafe class AVBufferPool(int size) : IDisposable
{
    private AutoGen._AVBufferPool* pool = ffmpeg.av_buffer_pool_init((ulong)size, null);

    /// <summary>
    /// Rents a buffer from the pool.
    /// </summary>
    /// <returns>An <see cref="AVBuffer"/> instance representing the rented buffer.</returns>
    /// <remarks>
    /// This method retrieves a buffer from the pool using FFmpeg's `av_buffer_pool_get` function.
    /// The caller is responsible for managing the lifecycle of the returned buffer.
    /// </remarks>
    public AVBuffer Rent() => new(ffmpeg.av_buffer_pool_get(pool));

    /// <summary>
    /// Releases all resources used by the <see cref="AVBufferPool"/>.
    /// </summary>
    /// <remarks>
    /// This method is called when the pool is no longer needed, either explicitly through a call to
    /// <see cref="Dispose"/> or implicitly via the finalizer. It calls FFmpeg's `av_buffer_pool_uninit`
    /// function to free the memory associated with the buffer pool.
    /// </remarks>
    public void Dispose()
    {
        AutoGen._AVBufferPool* p = pool;
        ffmpeg.av_buffer_pool_uninit(&p);
        pool = p;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AVBufferPool"/> class.
    /// </summary>
    /// <remarks>
    /// The finalizer ensures that resources are released if <see cref="Dispose"/> is not called.
    /// </remarks>
    ~AVBufferPool() => Dispose();
}

