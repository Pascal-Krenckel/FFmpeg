using FFmpeg.Utils;
using System.Runtime.CompilerServices;

namespace FFmpeg.Threading;

/// <summary>
/// Represents a deferred, single-execution function that can be cancelled before
/// execution begins.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="CancellableWorkItem"/> encapsulates an operation that is executed by
/// calling <see cref="Run"/>. The result of the operation is exposed through
/// <see cref="Task"/>.
/// </para>
/// <para>
/// Cancellation is only effective while the operation is waiting to be executed.
/// Once <see cref="Run"/> has started executing the function, cancellation will not
/// interrupt the operation. The function is expected to perform an operation that
/// cannot itself be cancelled.
/// </para>
/// <para>
/// <see cref="Run"/> and <see cref="Cancel"/> are thread-safe. Exactly one of them
/// will transition the operation out of its pending state. Subsequent calls have
/// no effect.
/// </para>
/// </remarks>
public sealed class CancellableWorkItem
{
    private readonly object _lock = new();
    private readonly TaskCompletionSource<AVResult32> tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenRegistration token;
    private readonly Func<AVResult32> action;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancellableWorkItem"/> class.
    /// </summary>
    /// <param name="action">
    /// The function to execute. The function is executed synchronously by
    /// <see cref="Run"/> and cannot be cancelled once execution has begun.
    /// </param>
    /// <param name="token">
    /// The token used to cancel the operation while it is waiting to be executed.
    /// </param>
    public CancellableWorkItem(Func<AVResult32> action, CancellationToken token)
    {
        this.action = action;
        this.token = token.Register(() => Cancel());
    }

    /// <summary>
    /// Cancels the operation if execution has not yet begun.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the operation was cancelled; otherwise,
    /// <see langword="false"/> if the operation had already been started,
    /// completed, or cancelled, or if another thread currently owns the operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Cancellation only succeeds while the operation is waiting to be executed.
    /// If <see cref="Run"/> has already claimed the operation, this method returns
    /// immediately without waiting for the operation to finish.
    /// </para>
    /// <para>
    /// If another call to <see cref="Run"/> or <see cref="Cancel"/> is currently
    /// executing, this method also returns immediately.
    /// </para>
    /// </remarks>
    public bool Cancel()
    {
        if (!Monitor.TryEnter(_lock))
            return false;

        try
        {
            token.Dispose();
            return tcs.TrySetCanceled();
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }

    /// <summary>
    /// Executes the operation if it has not already been cancelled, started, or completed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The operation is claimed atomically. If another thread has already claimed or
    /// cancelled the work item, this method returns without executing the operation.
    /// </para>
    /// <para>
    /// The function is executed synchronously on the calling thread and cannot be
    /// cancelled once execution has begun.
    /// </para>
    /// <para>
    /// If the function completes successfully, its result is exposed through
    /// <see cref="Task"/>. If the function throws an exception, <see cref="Task"/>
    /// completes in the faulted state.
    /// </para>
    /// </remarks>
    public void Run()
    {
        if (!Monitor.TryEnter(_lock))
            return;

        try
        {
            token.Dispose();
            _ = tcs.TrySetResult(action());
        }
        catch (Exception ex)
        {
            _ = tcs.TrySetException(ex);
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }

    /// <summary>
    /// Gets the task representing the completion of the operation.
    /// </summary>
    /// <value>
    /// A task that completes with the operation's result, faults if the operation
    /// throws an exception, or is cancelled if cancellation occurs before execution.
    /// </value>
    public Task<AVResult32> Task => tcs.Task;

    /// <summary>
    /// Gets an awaiter for the task representing this work item's completion.
    /// </summary>
    /// <returns>
    /// An awaiter that can be used to asynchronously wait for the operation to complete.
    /// </returns>
    /// <remarks>
    /// This member allows a <see cref="CancellableWorkItem"/> to be awaited directly.
    /// It is equivalent to calling <see cref="Task.GetAwaiter"/>.
    /// </remarks>
    public TaskAwaiter<AVResult32> GetAwaiter() => Task.GetAwaiter();


    /// <summary>
    /// Configures how continuations of the task representing this work item's completion
    /// are executed.
    /// </summary>
    /// <param name="captureContext">
    /// <see langword="true"/> to capture the current synchronization context when
    /// registering continuations; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// A configured awaitable for the operation's completion task.
    /// </returns>
    /// <remarks>
    /// This member allows <see cref="CancellableWorkItem"/> to be used directly with
    /// <c>await</c> while controlling whether the current synchronization context is
    /// captured.
    /// </remarks>
    public ConfiguredTaskAwaitable<AVResult32> ConfigureAwait(bool captureContext) =>
        Task.ConfigureAwait(captureContext);
}