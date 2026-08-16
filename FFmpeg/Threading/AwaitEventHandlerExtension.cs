namespace FFmpeg.Threading;

/// <summary>
/// Provides extension methods for asynchronously invoking event handlers.
/// </summary>
public static class AwaitEventHandlerExtension
{
    /// <summary>
    /// Invokes the specified event handler asynchronously and returns a task
    /// that completes when the handler has finished executing.
    /// </summary>
    /// <param name="sender">
    /// The object that raised the event.
    /// </param>
    /// <param name="e">
    /// The event data to pass to the handler.
    /// </param>
    /// <param name="token">
    /// A cancellation token that can be used to stop waiting for the handler
    /// to complete. Cancelling the token does not interrupt the event handler
    /// if it is already executing.
    /// </param>
    /// <returns>
    /// A task that completes successfully when the event handler finishes,
    /// faults if the event handler throws an exception, or is canceled if
    /// <paramref name="token"/> is canceled before the handler completes.
    /// </returns>
    extension(EventHandler handler)
    {
        public Task InvokeAsync(object sender, EventArgs e, CancellationToken token = default)
        {
            if (token == CancellationToken.None)
                return Task.Run(() => handler.Invoke(sender, e));

            TaskCompletionSource<int> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            CancellationTokenRegistration registration = token.Register(() => tcs.TrySetCanceled(token));

            _ = Task.Run(() => handler.Invoke(sender, e)).ContinueWith(t =>
            {
                registration.Dispose();

                try
                {
                    t.GetAwaiter().GetResult();
                    _ = tcs.TrySetResult(0);
                }
                catch (Exception ex)
                {
                    _ = tcs.TrySetException(ex);
                }
            }, CancellationToken.None);

            return tcs.Task;
        }
    }

    /// <summary>
    /// Invokes the specified event handler asynchronously and returns a task
    /// that completes when the handler has finished executing.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the event data.
    /// </typeparam>
    /// <param name="sender">
    /// The object that raised the event.
    /// </param>
    /// <param name="e">
    /// The event data to pass to the handler.
    /// </param>
    /// <param name="token">
    /// A cancellation token that can be used to stop waiting for the handler
    /// to complete. Cancelling the token does not interrupt the event handler
    /// if it is already executing.
    /// </param>
    /// <returns>
    /// A task that completes successfully when the event handler finishes,
    /// faults if the event handler throws an exception, or is canceled if
    /// <paramref name="token"/> is canceled before the handler completes.
    /// </returns>
    extension<T>(EventHandler<T> handler)
    {
        public Task InvokeAsync(object sender, T e, CancellationToken token = default)
        {
            if (token == CancellationToken.None)
                return Task.Run(() => handler.Invoke(sender, e));

            TaskCompletionSource<int> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            CancellationTokenRegistration registration = token.Register(() => tcs.TrySetCanceled(token));

            _ = Task.Run(() => handler.Invoke(sender, e)).ContinueWith(t =>
            {
                registration.Dispose();

                try
                {
                    t.GetAwaiter().GetResult();
                    _ = tcs.TrySetResult(0);
                }
                catch (Exception ex)
                {
                    _ = tcs.TrySetException(ex);
                }
            }, CancellationToken.None);

            return tcs.Task;
        }
    }
}