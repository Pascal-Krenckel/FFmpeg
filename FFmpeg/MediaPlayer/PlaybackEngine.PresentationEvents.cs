using FFmpeg.Utils;

namespace FFmpeg.MediaPlayer;

public partial class PlaybackEngine
{
    /// <summary>
    /// Enables the <see cref="VideoFrameReady"/> event.
    /// </summary>
    /// <param name="removeExpired">If true, the presentation loop will remove all expired video frames but keep at least one frame in the queue</param>
    /// <remarks>
    /// When video events are enabled and the player is playing, a presentation
    /// task is started that raises <see cref="VideoFrameReady"/> when buffered
    /// video frames reach their presentation timestamps.
    /// <para>
    /// If video events are already enabled while the player is not playing,
    /// this method has no effect. If the player is playing, the presentation
    /// task is restarted.
    /// </para>
    /// </remarks>
    public void EnableVideoEvents(bool removeExpired = true)
    {
        removeExpiredVideo = removeExpired;
        if (videoEvents && State != PlayerState.Playing)
            return;

        lock (_lock)
        {
            CheckDisposed();

            if (videoEvents && State != PlayerState.Playing)
                return;

            videoEvents = true;

            if (State == PlayerState.Playing)
            {
                presentVideoCts.Cancel();
                presentVideoCts.Dispose();
                presentVideoCts = new();

                CancellationToken token = presentVideoCts.Token;
                presentVideoTask = taskFactory.StartNew(() => PresentVideo(token));
                _ = presentVideoTask.ContinueWith(HandleException, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }

    /// <summary>
    /// Enables the <see cref="AudioFrameReady"/> event.
    /// </summary>
    /// <param name="removeExpired">If true the presentation loop will remove all expired audio samples before firing.</param>
    /// <remarks>
    /// When audio events are enabled and the player is playing, a presentation
    /// task is started that raises <see cref="AudioFrameReady"/> when buffered
    /// audio reaches its presentation timestamp.
    /// <para>
    /// If audio events are already enabled while the player is not playing,
    /// this method has no effect. If the player is playing, the presentation
    /// task is restarted.
    /// </para>
    /// </remarks>
    public void EnableAudioEvents(bool removeExpired = false)
    {
        removeExpiredAudio = removeExpired;
        if (audioEvents && State != PlayerState.Playing)
            return;

        lock (_lock)
        {
            CheckDisposed();

            if (audioEvents && State != PlayerState.Playing)
                return;

            audioEvents = true;

            if (State == PlayerState.Playing)
            {
                presentAudioCts.Cancel();
                presentAudioCts.Dispose();
                presentAudioCts = new();

                CancellationToken token = presentAudioCts.Token;
                presentAudioTask = taskFactory.StartNew(() => PresentAudio(token));
                _ = presentAudioTask.ContinueWith(HandleException, TaskContinuationOptions.NotOnFaulted);
            }
        }
    }

    /// <summary>
    /// Disables the <see cref="VideoFrameReady"/> event.
    /// </summary>
    /// <returns>
    /// A task that completes when the video presentation task has stopped.
    /// If video events are already disabled or the player is not playing,
    /// the returned task is already completed.
    /// </returns>
    public Task DisableVideoEvents()
    {
        if (!videoEvents)
            return Task.CompletedTask;

        Task task = Task.CompletedTask;

        lock (_lock)
        {
            if (!videoEvents)
                return Task.CompletedTask;

            videoEvents = false;
            CheckDisposed();

            presentVideoCts.Cancel();

            if (State == PlayerState.Playing && !presentVideoTask.IsCompleted)
                task = presentVideoTask;
        }

        return task;
    }

    /// <summary>
    /// Disables the <see cref="AudioFrameReady"/> event.
    /// </summary>
    /// <returns>
    /// A task that completes when the audio presentation task has stopped.
    /// If audio events are already disabled or the player is not playing,
    /// the returned task is already completed.
    /// </returns>
    public Task DisableAudioEvents()
    {
        if (!audioEvents)
            return Task.CompletedTask;

        Task task = Task.CompletedTask;

        lock (_lock)
        {
            if (!audioEvents)
                return Task.CompletedTask;

            audioEvents = false;
            CheckDisposed();

            presentAudioCts.Cancel();

            if (State == PlayerState.Playing && !presentAudioTask.IsCompleted)
                task = presentAudioTask;
        }

        return task;
    }

    /// <summary>
    /// Presents buffered audio and raises <see cref="AudioFrameReady"/> when
    /// audio reaches its presentation timestamp.
    /// </summary>
    /// <param name="token">
    /// A cancellation token used to stop the presentation task.
    /// </param>
    private void PresentAudio(CancellationToken token)
    {
        TimeSpan lastAudioPTS = TimeSpan.MinValue;

        try
        {
            while (!token.IsCancellationRequested && audioEvents)
            {
                bool hasAudio = mediaBuffer!.PeekAudio(out TimeSpan nextAudioPTS);

                if (!hasAudio)
                {
                    if (finishedReading)
                        break;

                    mediaBuffer.WaitForAudio(token).Wait(token);
                    continue;
                }
                else if (removeExpiredAudio)
                {
                    mediaBuffer.RemoveExpiredAudio(Clock.Position);
                }

                Clock.SleepUntil(nextAudioPTS, token).Wait(token);

                if (lastAudioPTS >= nextAudioPTS)
                {
                    Thread.Sleep(1);
                    continue;
                }

                try
                {
                    if (!audioEvents)
                        break;

                    AudioFrameReady?.Invoke(this, EventArgs.Empty);
                    lastAudioPTS = nextAudioPTS;
                }
                catch(Exception ex) 
                {
                    Logging.Logger.Error($"[{nameof(PlaybackEngine)}.{nameof(AudioFrameReady)}] {ex}");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Presents buffered video and raises <see cref="VideoFrameReady"/> when
    /// video reaches its presentation timestamp.
    /// </summary>
    /// <param name="token">
    /// A cancellation token used to stop the presentation task.
    /// </param>
    private void PresentVideo(CancellationToken token)
    {
        TimeSpan lastVideoPTS = TimeSpan.MinValue;

        try
        {
            while (!token.IsCancellationRequested && videoEvents)
            {
                if (removeExpiredVideo)
                    mediaBuffer.RemoveExpiredVideo(Clock.Position);
                bool hasVideo = mediaBuffer!.PeekVideo(out AVFrame? frame);

                if (!hasVideo)
                {
                    if (finishedReading)
                        break;

                    mediaBuffer.WaitForVideo(token).Wait(token);
                    continue;
                }


                Rational nextVideoPTS =
                    frame!.TimeBase * frame.GetPresentationTimestamp();

                Clock.SleepUntil(nextVideoPTS, token).Wait(token);

                if (lastVideoPTS >= nextVideoPTS)
                {
                    Thread.Sleep(1);
                    continue;
                }

                try
                {
                    if (!videoEvents)
                        break;

                    VideoFrameReady?.Invoke(this, EventArgs.Empty);
                    lastVideoPTS = nextVideoPTS;
                }
                catch(Exception ex)
                {
                    Logging.Logger.Error($"[{nameof(PlaybackEngine)}.{nameof(VideoFrameReady)}] {ex}");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Occurs when a buffered video frame reaches its presentation timestamp.
    /// </summary>
    /// <remarks>
    /// The event is raised by the video's presentation task. The corresponding
    /// frame remains in the media buffer and can be obtained using
    /// <see cref="ReadVideo"/> or inspected using <see cref="PeekVideo"/>.
    /// </remarks>
    public event EventHandler? VideoFrameReady;

    /// <summary>
    /// Occurs when buffered audio reaches its presentation timestamp.
    /// </summary>
    /// <remarks>
    /// The event is raised by the audio's presentation task. The corresponding
    /// audio data remains in the media buffer and can be obtained using
    /// <see cref="ReadAudio{T}(Span{T}, out TimeSpan)"/>.
    /// </remarks>
    public event EventHandler? AudioFrameReady;

    /// <summary>
    /// Occurs when playback reaches the end of the media.
    /// </summary>
    public event EventHandler? Finished;

    /// <summary>
    /// Occurs after the playback position has been changed by a seek operation.
    /// </summary>
    public event EventHandler? Seeked;

    /// <summary>
    /// Occurs when the player's playback state changes.
    /// </summary>
    /// <remarks>
    /// The event argument specifies the new playback state.
    /// </remarks>
    public event EventHandler<PlayerState>? PlayerStateChanged;

    /// <summary>
    /// Occurs when an exception is thrown by the decoding or presentation
    /// pipeline.
    /// </summary>
    /// <remarks>
    /// When this event is raised, the player transitions to the
    /// <see cref="PlayerState.Faulted"/> state and playback is stopped.
    /// The player cannot be started again using <see cref="Play"/> while in
    /// the faulted state. Use <see cref="Restart"/> to reset the player and
    /// attempt to resume playback.
    /// </remarks>
    /// <param name="sender">The media player that encountered the fault.</param>
    /// <param name="e">The exception that caused the playback pipeline to fault.</param>
    public event EventHandler<Exception>? Faulted;
}