namespace FFmpeg.MediaPlayer;

public static class ClockExtension
{
    extension(IMediaClock @this)
    {
        /// <summary>
        /// Returns a task that completes when the clock reaches the specified presentation timestamp.
        /// </summary>
        /// <param name="pts">The presentation timestamp to wait for.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that completes when the clock reaches <paramref name="pts"/>.</returns>
        public async Task SleepUntil(TimeSpan pts, CancellationToken token)
        {
            if (pts <= @this.Position)
                return;

            TaskCompletionSource<int>? signal =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            void clockChanged(object sender, EventArgs e) => signal.TrySetResult(0);
            @this.ClockChanged += clockChanged;

            try
            {
                using CancellationTokenRegistration cancellation = token.Register(() => signal.TrySetCanceled(token));

                while (pts > @this.Position)
                {
                    token.ThrowIfCancellationRequested();

                    if (!@this.IsRunning)
                    {
                        _ = await signal.Task.ConfigureAwait(false);
                        signal = new(TaskCreationOptions.RunContinuationsAsynchronously);
                        continue;
                    }

                    TimeSpan delay;

                    try
                    {
                        delay = (pts - @this.Position) / @this.Rate;
                    }
                    catch (ArithmeticException)
                    {
                        // A zero rate means that the clock cannot currently advance.
                        _ = await signal.Task.ConfigureAwait(false);
                        signal = new(TaskCreationOptions.RunContinuationsAsynchronously);
                        continue;
                    }

                    if (delay <= TimeSpan.Zero)
                        continue;

                    Task delayTask = Task.Delay(delay, token);
                    Task completed = await Task.WhenAny(delayTask, signal.Task).ConfigureAwait(false);
                    await completed.ConfigureAwait(false);
                    if (completed == signal.Task)
                    {
                        signal = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    }
                }
            }
            finally
            {
                @this.ClockChanged -= clockChanged;
            }
        }
    }
}