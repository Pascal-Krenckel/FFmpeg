using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpeg.MediaPlayer;

public partial class PlaybackEngine
{
    /// <summary>
    /// Gets a value indicating whether the media source supports seeking.
    /// </summary>
    public bool CanSeek => source.Seekable != Seekable.None;

    /// <summary>
    /// Seeks the media source to the specified presentation timestamp.
    /// </summary>
    /// <param name="pts">
    /// The presentation timestamp to seek to, expressed in the stream's time base.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the media source is seekable and the seek was initiated;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private bool SeekInternally(long pts)
    {
        if (source.Seekable == Seekable.None)
            return false;

        lock (_lock)
        {
            bool running = State == PlayerState.Playing;

            _ = PauseInternally();
            source.Seek(pts).ThrowIfError();
            mediaBuffer.Clear();
            FlushFilters();
            Clock.Seeked(pts * Rational.TIME_BASE);

            if (running)
                _ = PlayInternally();
        }

        return true;
    }

    /// <summary>
    /// Seeks the player to the specified presentation timestamp.
    /// </summary>
    /// <param name="pts">The presentation timestamp to seek to.</param>
    /// <returns>
    /// <see langword="true"/> if the seek was performed; otherwise,
    /// <see langword="false"/> if the media source is not seekable.
    /// </returns>
    /// <remarks>
    /// If the requested position is currently contained in the media buffer
    /// and the player is not faulted, the buffer is used to perform the seek
    /// without accessing the underlying media source.
    /// <para>
    /// If the requested position is not available in the buffer, or if the
    /// player is faulted, the underlying media source is seeked and the buffer
    /// is cleared.
    /// </para>
    /// <para>
    /// If the player was playing before the seek, playback is resumed
    /// automatically after the seek.
    /// </para>
    /// <para>
    /// The <see cref="Seeked"/> event is raised after the seek has completed.
    /// </para>
    /// </remarks>
    public bool Seek(TimeSpan pts)
    {
        if (source.Seekable == Seekable.None)
            return false;
        try
        {
            lock (_lock)
            {
                if (mediaBuffer!.CanSeek(pts) && State != PlayerState.Faulted)
                {
                    mediaBuffer.Seek(pts);
                    Clock.Seeked(pts);
                }
                else
                {
                    int index = audioStreamIndex == -1 ? videoStreamIndex : audioStreamIndex;
                    bool running = State == PlayerState.Playing;

                    _ = PauseInternally();
                    AVResult32 result = source.SeekExactly(pts, index);
                    mediaBuffer.Clear();
                    mediaBuffer.FinishWriting();
                    FlushFilters();
                    if (result != AVResult32.EndOfFile)
                    {
                        if (result.IsError)
                        {
                            State = PlayerState.Faulted;
                            result.ThrowIfError();
                        }
                        Clock.Seeked(pts);
                        if (running)
                            _ = PlayInternally();
                    }
                    else
                    {
                        Clock.Seeked(Duration);
                        State = PlayerState.Finished;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Faulted?.Invoke(this, ex);
            PlayerStateChanged?.Invoke(this, PlayerState.Faulted);
            throw;
        }
        Seeked?.Invoke(this, EventArgs.Empty);
        if (State == PlayerState.Finished)
            PlayerStateChanged?.Invoke(this, PlayerState.Finished);
        return true;
    }

    /// <summary>
    /// Starts or resumes playback.
    /// </summary>
    /// <remarks>
    /// If the player is already playing, this method has no effect.
    /// <para>
    /// A faulted player must be reset using <see cref="Restart"/> before
    /// playback can be started again.
    /// </para>
    /// <para>
    /// The <see cref="PlayerStateChanged"/> event is raised when the player
    /// successfully transitions to the <see cref="PlayerState.Playing"/> state.
    /// </para>
    /// </remarks>
    public void Play()
    {
        if (State == PlayerState.Playing)
            return;
        if (State == PlayerState.Faulted)
            throw new InvalidOperationException("The player is in a faulted state, please restart the player.");
        if (PlayInternally())
            PlayerStateChanged?.Invoke(this, PlayerState.Playing);
    }

    /// <summary>
    /// Starts playback internally.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the player transitioned to the playing state;
    /// otherwise, <see langword="false"/> if it was already playing.
    /// </returns>
    private bool PlayInternally()
    {
        if (State == PlayerState.Playing)
            return false;

        lock (_lock)
        {
            if (State == PlayerState.Playing)
                return false;
            if (State == PlayerState.Faulted)
                throw new InvalidOperationException("The player is in a faulted state, please restart the player.");
            if (State == PlayerState.Finished)
                if (!SeekInternally(0))
                    return false;
            decodingCts.Cancel();
            decodingCts.Dispose();
            decodingCts = new();

            CancellationToken token = decodingCts.Token;

            try
            {
                decodingTask.Wait();
            }
            catch { }

            State = PlayerState.Playing;
            finishedReading = false;
            mediaBuffer.MakeWriteable();
            decodingTask = taskFactory.StartNew(() => Decoding(token).ConfigureAwait(false).GetAwaiter().GetResult());
            _ = decodingTask.ContinueWith(HandleException, TaskContinuationOptions.OnlyOnFaulted);
            if (audioEvents)
                EnableAudioEvents(removeExpiredAudio);

            if (videoEvents)
                EnableVideoEvents(removeExpiredVideo);

            Clock.Start();
        }

        return true;
    }

    /// <summary>
    /// Stops playback.
    /// </summary>
    /// <remarks>
    /// If the media source is seekable, stopping also resets the playback
    /// position to the beginning of the media.
    /// <para>
    /// If the media source is not seekable, a playing player transitions to
    /// <see cref="PlayerState.Paused"/>, while a paused player remains paused.
    /// A faulted player remains in the <see cref="PlayerState.Faulted"/> state
    /// because its position cannot be reset.
    /// </para>
    /// <para>
    /// The <see cref="PlayerStateChanged"/> event is raised after the state
    /// transition has completed.
    /// </para>
    /// </remarks>
    public void Stop()
    {
        if (State == PlayerState.Stopped)
            return;

        PlayerState state = StopInternally();
        PlayerStateChanged?.Invoke(this, state);
    }

    /// <summary>
    /// Stops playback and resets the player position when possible.
    /// </summary>
    /// <returns>
    /// The resulting <see cref="PlayerState"/>.
    /// </returns>
    /// <remarks>
    /// A faulted player is reset to <see cref="PlayerState.Stopped"/> only if
    /// the media source can be successfully seeked to the beginning.
    /// </remarks>
    private PlayerState StopInternally()
    {
        lock (_lock)
        {
            CheckDisposed();

            switch (State)
            {
                case PlayerState.Stopped:
                    return PlayerState.Stopped;
                case PlayerState.Faulted:
                    if (!SeekInternally(0))
                        return PlayerState.Faulted;
                    return State = PlayerState.Stopped;
                case PlayerState.Finished:
                case PlayerState.Paused:
                    if (!SeekInternally(0))
                        return State = PlayerState.Paused;
                    return PlayerState.Stopped;
                case PlayerState.Playing:
                    _ = PauseInternally();
                    if (SeekInternally(0))
                        return State = PlayerState.Stopped;
                    return State = PlayerState.Paused;
                default:
                    throw new NotImplementedException("The player state is not implemented yet.");
            }
        }
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    /// <remarks>
    /// If the player is not currently playing, this method has no effect.
    /// <para>
    /// The <see cref="PlayerStateChanged"/> event is raised when the player
    /// successfully transitions to the <see cref="PlayerState.Paused"/> state.
    /// </para>
    /// </remarks>
    public void Pause()
    {
        if (State != PlayerState.Playing)
            return;

        if (PauseInternally())
            PlayerStateChanged?.Invoke(this, State);
    }

    /// <summary>
    /// Pauses playback and stops all active processing tasks.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the player transitioned to the paused state;
    /// otherwise, <see langword="false"/> if the player was not playing.
    /// </returns>
    private bool PauseInternally()
    {
        if (State != PlayerState.Playing)
            return false;

        lock (_lock)
        {
            if (State != PlayerState.Playing)
                return false;

            CheckDisposed();

            decodingCts.Cancel();
            presentAudioCts.Cancel();
            presentVideoCts.Cancel();
            try
            {
                if (!Task.WaitAll([decodingTask, presentVideoTask, presentAudioTask], 1000))
                    decodingTask.Wait();
            }
            catch { }
            Clock.Pause();
            State = PlayerState.Paused;
        }

        return true;
    }

    /// <summary>
    /// Resets the player to the beginning of the media and starts playback.
    /// </summary>
    /// <remarks>
    /// The player must be able to seek in order to restart.
    /// <para>
    /// Restarting stops the current playback pipeline, resets the playback
    /// position to the beginning, clears the media buffer, resets the clock,
    /// and starts playback again.
    /// </para>
    /// <para>
    /// A faulted player can be recovered using this method if the media source
    /// supports seeking.
    /// </para>
    /// <para>
    /// The <see cref="Seeked"/> and <see cref="PlayerStateChanged"/> events
    /// are raised after the player has been successfully restarted.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// The player has been disposed.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The player could not be reset to the beginning of the media. The player
    /// remains in its current state and playback is not restarted.
    /// </exception>
    public void Restart()
    {
        lock (_lock)
        {
            if (StopInternally() != PlayerState.Stopped)
                throw new NotSupportedException("Failed to reset the media player.");
            _ = PlayInternally();
        }
        Seeked?.Invoke(this, EventArgs.Empty);
        PlayerStateChanged?.Invoke(this, PlayerState.Playing);
    }
}