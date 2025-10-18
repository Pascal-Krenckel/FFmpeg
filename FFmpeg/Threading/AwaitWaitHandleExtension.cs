using System.Runtime.CompilerServices;

namespace FFmpeg.Threading;

public static class AwaitWaitHandleExtension
{
    public static TaskAwaiter GetAwaiter(this WaitHandle waitHandle) => AsTask(waitHandle).GetAwaiter();
    public static ConfiguredTaskAwaitable ConfigureAwait(this WaitHandle waitHandle, bool continueOnCapturedContext) => AsTask(waitHandle).ConfigureAwait(continueOnCapturedContext);


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
        public TaskCompletionSource<int> TaskCompletionSource { get; } = new();

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
                        TaskCompletionSource.SetResult(0);
                        _ = WaitHandleCallback?.Unregister(null);
                        TokenRegistration.Dispose();
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


