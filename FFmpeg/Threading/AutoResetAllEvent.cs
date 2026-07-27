namespace FFmpeg.Threading;

/// <summary>
/// Represents an asynchronous auto-reset event that releases all current waiters
/// when signaled and automatically resets for subsequent waits.
/// </summary>
/// <remarks>
/// Consumers wait by awaiting the <see cref="Task"/> property. Calling
/// <see cref="Notify"/> completes the current task, releasing all awaiting
/// callers, and immediately creates a new task for future waiters. Calling
/// <see cref="Cancel"/> cancels the current task and likewise resets the event.
/// </remarks>
public class AutoResetAllEvent : IDisposable
{
    private readonly object _lock = new();
    private TaskCompletionSource<int> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool disposedValue;
    /// <summary>
    /// Signals the event, completing the current wait task and releasing all
    /// awaiting callers.
    /// </summary>
    /// <remarks>
    /// After the current waiters have been released, the event is automatically
    /// reset by creating a new task for subsequent waiters.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The event has been disposed.
    /// </exception>
    public void Notify()
    {
        if (disposedValue)
            throw new ObjectDisposedException(nameof(AutoResetAllEvent));
        lock (_lock)
        {
            _ = tcs.TrySetResult(0);
            tcs = new TaskCompletionSource<int>();
            if (disposedValue)
                _ = tcs.TrySetCanceled();
        }
    }
    /// <summary>
    /// Gets a task that completes when the event is signaled or is canceled.
    /// </summary>
    /// <remarks>
    /// After the task completes, callers should obtain the <see cref="Task"/>
    /// property again before waiting for the next notification.
    /// </remarks>
    public Task Task => tcs.Task;

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _ = tcs.TrySetCanceled();
            }
            disposedValue = true;
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance.
    /// </summary>
    /// <remarks>
    /// Disposing the event cancels the current wait task so that any awaiting
    /// callers are released.
    /// </remarks>
    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Cancels the current wait task and resets the event.
    /// </summary>
    /// <remarks>
    /// All current waiters observe a canceled task. Future waiters receive a new
    /// task and are unaffected by the cancellation.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The event has been disposed.
    /// </exception>
    public void Cancel()
    {
        if (disposedValue)
            throw new ObjectDisposedException(nameof(AutoResetAllEvent));
        lock (_lock)
        {
            _ = tcs.TrySetCanceled();
            tcs = new TaskCompletionSource<int>();
            if (disposedValue)
                _ = tcs.TrySetCanceled();
        }
    }
}
