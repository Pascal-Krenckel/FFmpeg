using System.Runtime.CompilerServices;

namespace FFmpeg.Threading;

/// <summary>
/// Provides extension methods that allow <see cref="WaitHandle"/> instances to be awaited
/// using the Task-based asynchronous programming model.
/// </summary>
/// <remarks>
/// This class bridges the traditional blocking <see cref="WaitHandle"/> API with
/// <see cref="Task"/> and <c>await</c> by internally using
/// <see cref="ThreadPool.RegisterWaitForSingleObject(WaitHandle, WaitOrTimerCallback, object?, int, bool)"/>.
/// The returned task completes when the wait handle is signaled or is canceled through a
/// provided <see cref="CancellationToken"/>.
/// </remarks>
public static class AwaitWaitHandleExtension
{
    /// <summary>
    /// Gets an awaiter that completes when the specified <see cref="WaitHandle"/> is signaled.
    /// </summary>
    /// <param name="waitHandle">The wait handle to await.</param>
    /// <returns>An awaiter that can be used with the <c>await</c> keyword.</returns>
    public static TaskAwaiter GetAwaiter(this WaitHandle waitHandle) => AsTask(waitHandle).GetAwaiter();
    /// <summary>
    /// Configures how continuations are scheduled after awaiting the specified
    /// <see cref="WaitHandle"/>.
    /// </summary>
    /// <param name="waitHandle">The wait handle to await.</param>
    /// <param name="continueOnCapturedContext">
    /// <see langword="true"/> to attempt to marshal the continuation back to the captured context;
    /// otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>A configured awaitable.</returns>
    public static ConfiguredTaskAwaitable ConfigureAwait(this WaitHandle waitHandle, bool continueOnCapturedContext) => AsTask(waitHandle).ConfigureAwait(continueOnCapturedContext);


    /// <summary>
    /// Converts a <see cref="WaitHandle"/> into a <see cref="Task"/> that completes when the handle is signaled.
    /// </summary>
    /// <param name="waitHandle">The wait handle to monitor.</param>
    /// <param name="token">
    /// A cancellation token that can be used to cancel the wait operation.
    /// </param>
    /// <returns>
    /// A task that completes successfully when the wait handle is signaled,
    /// or transitions to the canceled state when <paramref name="token"/> is canceled.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown if <paramref name="token"/> has already been canceled before the wait begins.
    /// </exception>
    public static Task AsTask(this WaitHandle waitHandle, CancellationToken token = default)
    {
        if (waitHandle.WaitOne(0))
            return Task.CompletedTask;
        token.ThrowIfCancellationRequested();
        WaitHandleState state = new(waitHandle);
        lock (state)
        {
            state.RegisterWaitHandle();
            state.RegisterToken(token);
        }

        return state.TaskCompletionSource.Task;
    }

    private class WaitHandleState(WaitHandle waitHandle)
    {
        public bool Finished { get; set; } = false;
        public TaskCompletionSource<int> TaskCompletionSource { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public RegisteredWaitHandle? WaitHandleCallback { get; private set; } = null;

        public CancellationTokenRegistration TokenRegistration { get; set; } = default;

        public WaitHandle WaitHandle { get; } = waitHandle;

        public void RegisterWaitHandle() => WaitHandleCallback = ThreadPool.RegisterWaitForSingleObject(WaitHandle,
                (state, timeout) =>
                {
                    lock (this)
                    {
                        if (Finished)
                            return;
                        Finished = true;
                        _ = WaitHandleCallback?.Unregister(null);
                        TokenRegistration.Dispose();
                        TaskCompletionSource.SetResult(0);

                    }
                },
                state: null,
                millisecondsTimeOutInterval: Timeout.Infinite,
                executeOnlyOnce: true);

        public void RegisterToken(CancellationToken token) => TokenRegistration = token.Register(() =>
                                                                       {
                                                                           lock (this)
                                                                           {
                                                                               if (Finished)
                                                                                   return;
                                                                               Finished = true;
                                                                               TaskCompletionSource.SetCanceled();
                                                                               _ = WaitHandleCallback?.Unregister(null);
                                                                               TokenRegistration.Dispose();
                                                                           }
                                                                       });
    }
}


