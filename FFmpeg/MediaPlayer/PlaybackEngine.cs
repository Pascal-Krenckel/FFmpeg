using FFmpeg.Audio;
using FFmpeg.Filters;
using FFmpeg.Filters.AudioFilters;
using FFmpeg.Filters.VideoFilters;
using FFmpeg.Formats;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.MediaPlayer;

/// <summary>
/// Represents a media player that decodes audio and video from a <see cref="MediaSource"/>
/// and manages playback, buffering, presentation, and optional filtering.
/// </summary>
/// <remarks>
/// <para>
/// The media player supports at most one audio stream and one video stream.
/// Decoded frames are stored in an internal <see cref="MediaBuffer"/> before
/// being presented.
/// </para>
/// <para>
/// Playback is controlled through the player's <see cref="Clock"/>. The clock
/// determines the current playback position and playback rate.
/// </para>
/// <para>
/// The player is thread-safe for its public control operations. Playback and
/// decoding are performed asynchronously on background tasks.
/// </para>
/// </remarks>
public sealed partial class PlaybackEngine : IDisposable
{
    private bool disposed = false;
    private volatile bool finishedReading = false;
    private MediaBuffer mediaBuffer;
    private readonly MediaSource source;
    private readonly int videoStreamIndex = -1;
    private readonly int audioStreamIndex = -1;

    private readonly object _lock = new();
    private volatile bool audioEvents = false, videoEvents = false;
    private volatile bool removeExpiredVideo = false;
    private volatile bool removeExpiredAudio = false;
    private CancellationTokenSource decodingCts = new();
    private CancellationTokenSource presentVideoCts = new();
    private CancellationTokenSource presentAudioCts = new();
    private readonly TaskFactory taskFactory = new(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
    private Task decodingTask = Task.CompletedTask;
    private Task presentVideoTask = Task.CompletedTask;
    private Task presentAudioTask = Task.CompletedTask;


    private VideoBufferSource? videoIn;
    private VideoBufferSink? videoOut;
    private FilterGraph? videoFilterGraph;
    private AudioBufferSource? audioIn;
    private AudioBufferSink? audioOut;
    private FilterGraph? audioFilterGraph;
    private volatile PlayerState state;

    /// <summary>
    /// Gets the current state of the media player.
    /// </summary>
    /// <value>
    /// The current <see cref="PlayerState"/>.
    /// </value>
    public PlayerState State { get => state; private set => state = value; }

    /// <summary>
    /// Gets or sets the clock used to control the media player's playback position
    /// and playback rate.
    /// </summary>
    /// <value>
    /// The media clock used by the player.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// Thrown when an attempt is made to assign <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an attempt is made to change the clock while the player is
    /// in the <see cref="PlayerState.Playing"/> state.
    /// </exception>
    /// <remarks>
    /// The clock can only be changed while the media player is not playing.
    /// Changing the clock while playback is active would make the current
    /// playback position ambiguous.
    /// </remarks>
    public IMediaClock Clock
    {
        get; set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            lock (_lock)
            {
                if (State == PlayerState.Playing)
                    throw new InvalidOperationException("You cannot change the clock, while the media player is running");
                field = value;
            }
        }
    } = new SystemClock();

    /// <summary>
    /// Returns the audio stream or null if none.
    /// </summary>
    public AVStream? VideoStream => videoStreamIndex >= 0 ? source.Streams[videoStreamIndex] : null;

    /// <summary>
    /// Returns the audio stream or null if none.
    /// </summary>
    public AVStream? AudioStream => audioStreamIndex >= 0 ? source.Streams[audioStreamIndex] : null;

    /// <summary>
    /// Gets the duration of the media without applying filters.
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            TimeSpan endVideo = TimeSpan.Zero;
            TimeSpan endAudio = TimeSpan.Zero;
            if (VideoStream != null)
                endVideo = (VideoStream.Duration + VideoStream.StartTime) * VideoStream.TimeBase;
            if (AudioStream != null)
                endAudio = (AudioStream.Duration + AudioStream.StartTime) * AudioStream.TimeBase;
            return endAudio > endVideo ? endAudio : endVideo;
        }
    }

    /// <summary>
    /// Gets the audio sample format produced by the media player.
    /// </summary>
    /// <value>
    /// The output audio sample format, or <see cref="SampleFormat.None"/> if
    /// the media source does not contain an audio stream.
    /// </value>
    /// <remarks>
    /// If an audio filter graph is configured, the format of the filter output
    /// is returned. Otherwise, the format of the audio decoder is returned.
    /// </remarks>
    public SampleFormat SampleFormat =>
        audioStreamIndex >= 0
            ? audioFilterGraph != null
                ? audioOut!.SampleFormat
                : source.CodecContexts[audioStreamIndex].SampleFormat
            : SampleFormat.None;

    /// <summary>
    /// Gets the number of audio channels produced by the media player.
    /// </summary>
    /// <value>
    /// The number of output audio channels, or <c>0</c> if the media source
    /// does not contain an audio stream.
    /// </value>
    /// <remarks>
    /// If an audio filter graph is configured, the number of channels of the
    /// filter output is returned. Otherwise, the number of channels configured
    /// by the audio decoder is returned.
    /// </remarks>
    public int Channels =>
        audioStreamIndex >= 0
            ? audioFilterGraph != null
                ? audioOut!.Channels
                : source.CodecContexts[audioStreamIndex].ChannelLayout.Channels
            : 0;

    /// <summary>
    /// Gets the channel layout of the audio produced by the media player.
    /// </summary>
    /// <value>
    /// The output audio channel layout, or <see langword="null"/> if the media
    /// source does not contain an audio stream or the output channel layout
    /// cannot be determined.
    /// </value>
    /// <remarks>
    /// If an audio filter graph is configured, the channel layout of the filter
    /// output is returned. Otherwise, the channel layout configured by the
    /// audio decoder is returned.
    /// </remarks>
    public ChannelLayout? ChannelLayout => audioStreamIndex < 0
                ? null
                : audioFilterGraph != null
                ? audioOut!.TryGetChannelLayout(out ChannelLayout? layout) ? layout : null
                : source.CodecContexts[audioStreamIndex].ChannelLayout.GetReferencedObject();

    /// <summary>
    /// Gets the audio sample rate produced by the media player.
    /// </summary>
    /// <value>
    /// The output audio sample rate in samples per second, or <c>0</c> if the
    /// media source does not contain an audio stream.
    /// </value>
    /// <remarks>
    /// If an audio filter graph is configured, the sample rate of the filter
    /// output is returned. Otherwise, the sample rate configured by the audio
    /// decoder is returned.
    /// </remarks>
    public int SampleRate =>
        audioStreamIndex >= 0
            ? audioFilterGraph != null
                ? audioOut!.SampleRate
                : source.CodecContexts[audioStreamIndex].SampleRate
            : 0;

    /// <summary>
    /// Gets the video pixel format produced by the media player.
    /// </summary>
    /// <value>
    /// The output video pixel format, or <see cref="PixelFormat.None"/> if the
    /// media source does not contain a video stream.
    /// </value>
    /// <remarks>
    /// If a video filter graph is configured, the pixel format of the filter
    /// output is returned. Otherwise, the software pixel format configured by
    /// the video decoder is returned.
    /// </remarks>
    public PixelFormat PixelFormat =>
        videoStreamIndex >= 0
            ? videoFilterGraph != null
                ? videoOut!.PixelFormat
                : (source.CodecContexts[videoStreamIndex].SoftwarePixelFormat != PixelFormat.None ?
                        source.CodecContexts[videoStreamIndex].SoftwarePixelFormat :
                        source.CodecContexts[videoStreamIndex].PixelFormat)
            : PixelFormat.None;

    /// <summary>
    /// Gets the width of the video produced by the media player.
    /// </summary>
    /// <value>
    /// The output video width in pixels, or <c>0</c> if the media source does
    /// not contain a video stream.
    /// </value>
    /// <remarks>
    /// If a video filter graph is configured, the width of the filter output
    /// is returned. Otherwise, the width configured by the video decoder is
    /// returned.
    /// </remarks>
    public int Width =>
        videoStreamIndex >= 0
            ? videoFilterGraph != null
                ? videoOut!.Width
                : source.CodecContexts[videoStreamIndex].Width
            : 0;

    /// <summary>
    /// Gets the height of the video produced by the media player.
    /// </summary>
    /// <value>
    /// The output video height in pixels, or <c>0</c> if the media source does
    /// not contain a video stream.
    /// </value>
    /// <remarks>
    /// If a video filter graph is configured, the height of the filter output
    /// is returned. Otherwise, the height configured by the video decoder is
    /// returned.
    /// </remarks>
    public int Height =>
        videoStreamIndex >= 0
            ? videoFilterGraph != null
                ? videoOut!.Height
                : source.CodecContexts[videoStreamIndex].Height
            : 0;


    private async Task Decoding(CancellationToken token)
    {
        using AVFrame frame = AVFrame.Allocate();
        AVResult32 result;
        try
        {
            mediaBuffer.MakeWriteable();
            while (!(result = source.ReadAndDecodeAVFrame(frame)).IsError)
            {
                await FilterFrame(frame, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }
            if (result != AVResult32.EndOfFile)
                result.ThrowIfError();
            await DrainFilter(token);
            HandleFinished(token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Logging.Logger.Error(ex.ToString());
        }
        finally { frame.Dispose(); }
    }

    private async Task DrainFilter(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        AVResult32 resultVideo = 0;
        AVResult32 resultAudio = 0;
        _ = videoIn?.Drain();
        _ = audioIn?.Drain();
        AVFrame? audioFrame = null;
        AVFrame? videoFrame = null;
        if (audioFilterGraph == null)
            resultAudio = AVResult32.EndOfFile;
        if (videoFilterGraph == null)
            resultVideo = AVResult32.EndOfFile;
        try
        {
            while (resultAudio != AVResult32.EndOfFile || resultVideo != AVResult32.EndOfFile)
            {
                bool writeVideo;
                token.ThrowIfCancellationRequested();
                if (resultVideo != AVResult32.EndOfFile && videoFrame == null)
                {
                    videoFrame = AVFrame.Allocate();
                    resultVideo = GetFilteredFrame(videoFrame, videoOut);
                    if (resultVideo == AVResult32.EndOfFile)
                    {
                        videoFrame.Dispose();
                        videoFrame = null;
                    }
                    else
                        resultVideo.ThrowIfError();
                }
                if (resultAudio != AVResult32.EndOfFile && audioFrame == null)
                {
                    audioFrame = AVFrame.Allocate();
                    resultAudio = GetFilteredFrame(audioFrame, audioOut);
                    if (resultAudio == AVResult32.EndOfFile)
                    {
                        audioFrame.Dispose();
                        audioFrame = null;
                    }
                    else
                        resultAudio.ThrowIfError();
                }
                if (audioFrame == null && videoFrame == null)
                    break;
                writeVideo = audioFrame == null || (videoFrame != null && (videoFrame.GetPresentationTimestamp() * videoFrame.TimeBase) < (audioFrame.GetPresentationTimestamp() * audioFrame.TimeBase));
                if (writeVideo)
                {
                    _ = await mediaBuffer!.WriteAsync(videoFrame!, token).ConfigureAwait(false);
                    videoFrame = null;
                }
                else
                {
                    _ = await mediaBuffer!.WriteAsync(audioFrame!, token).ConfigureAwait(false);
                    audioFrame = null;
                }
            }
        }
        finally
        {
            videoFrame?.Dispose();
            audioFrame?.Dispose();
        }
    }

    private AVResult32 GetFilteredFrame(AVFrame frame, IBufferSink? filterOut)
    {
        if (filterOut == null)
            return AVResult32.EndOfFile;
        AVResult32 result = filterOut.ReceiveFrame(frame);
        return result;
    }

    private async Task FilterFrame(AVFrame frame, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        AVFrame tmpFrame = AVFrame.Allocate();
        tmpFrame.Reference(frame);
        try
        {
            if (tmpFrame.IsVideo)
                if (videoFilterGraph != null)
                {
                    AVResult32 result32;
                    videoIn!.SendFrame(tmpFrame).ThrowIfError();
                    while (!(result32 = videoOut!.ReceiveFrame(tmpFrame)).IsError)
                    {
                        _ = await mediaBuffer!.WriteAsync(tmpFrame, token).ConfigureAwait(false);
                        tmpFrame = AVFrame.Allocate();
                    }
                    tmpFrame.Dispose();
                    if (!result32.IsTryAgain)
                        result32.ThrowIfError();
                }
                else
                    _ = await mediaBuffer!.WriteAsync(tmpFrame, token).ConfigureAwait(false);
            else if (tmpFrame.IsAudio)
                if (audioFilterGraph != null)
                {
                    AVResult32 result32;
                    audioIn!.SendFrame(tmpFrame).ThrowIfError();
                    while (!(result32 = audioOut!.ReceiveFrame(tmpFrame)).IsError)
                    {
                        _ = await mediaBuffer!.WriteAsync(tmpFrame, token).ConfigureAwait(false);
                        tmpFrame = AVFrame.Allocate();
                    }
                    tmpFrame.Dispose();
                    if (!result32.IsTryAgain)
                        result32.ThrowIfError();
                }
                else
                    _ = await mediaBuffer!.WriteAsync(tmpFrame, token).ConfigureAwait(false);
            else
                Logging.Logger.Warning($"[{GetType().Name}] Frame is neither audio nor video.");
        }
        catch
        {
            tmpFrame.Dispose();
            throw;
        }
    }
    private void HandleFinished(CancellationToken token)
    {
        Task? audioPresenter = null, videoPresenter = null;
        while (!Monitor.TryEnter(_lock))
        {
            if (token.IsCancellationRequested)
                return;
            Thread.Sleep(1); // sleep for 1 ms, and try too get the lock again, this is needed to avoid deadlock
            // this should normally not happen as the lock is only taken in Pause/Play/Pause/Seek.
        }
        try
        {
            if (token.IsCancellationRequested)
                return;
            finishedReading = true;
            mediaBuffer.FinishWriting();
            if (audioEvents)
            {
                audioPresenter = presentAudioTask;

            }
            if (videoEvents)
            {
                videoPresenter = presentVideoTask;
            }
        }
        finally
        {
            Monitor.Exit(_lock); // release lock, too await presentation tasks
        }
        try
        {
            // awaiting the decoding tasks internally only happens if the decoding task was canceled, so this should never dead lock.
            if (audioPresenter != null && videoPresenter != null)
                Task.WaitAll([audioPresenter, videoPresenter], token);
            else
            {
                audioPresenter?.Wait(token); // at most one can be not null
                videoPresenter?.Wait(token);
            }
        }
        catch (OperationCanceledException)
        {

        }
        catch
        {
            // a presenter faulted, we will just return
            return;
        }
        while (!Monitor.TryEnter(_lock))
        {
            if (token.IsCancellationRequested)
                return;
            Thread.Sleep(1); // sleep for 1 ms, and try too get the lock again, this is needed to avoid deadlock
        }
        try
        {
            if (!token.IsCancellationRequested && State != PlayerState.Faulted)
            {
                State = PlayerState.Finished;
                mediaBuffer.Clear();
                _ = PlayerStateChanged?.BeginInvoke(this, PlayerState.Finished, null, null);
            }
        }
        finally { Monitor.Exit(_lock); }
    }

    private void HandleException(Task task)
    {
        if (!task.IsFaulted)
            return;

        Exception exception = task.Exception!.GetBaseException();
        if (exception is OperationCanceledException)
            return;

        lock (_lock)
        {
            if (disposed || State == PlayerState.Faulted)
                return;
            decodingCts.Cancel();
            presentVideoCts.Cancel();
            presentAudioCts.Cancel();
            State = PlayerState.Faulted;
        }
        try
        { Faulted?.Invoke(this, exception); }
        catch { }
        try
        { PlayerStateChanged?.Invoke(this, PlayerState.Faulted); }
        catch { }
    }

    /// <summary>
    /// Releases all resources used by the media player.
    /// </summary>
    /// <remarks>
    /// Disposing the media player stops playback, releases the internal media
    /// buffer, disposes configured filter graphs, and releases the resources
    /// associated with the player's asynchronous operations.
    /// </remarks>
    public void Dispose()
    {
        if (disposed)
            return;
        lock (_lock)
        {
            if (disposed)
                return;
            _ = PauseInternally();
            disposed = true;
            mediaBuffer!.Dispose();
            videoFilterGraph?.Dispose();
            audioFilterGraph?.Dispose();
            decodingCts.Dispose();
            presentAudioCts.Dispose();
            presentVideoCts.Dispose();
        }

    }

    private void CheckDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }


}

/// <summary>
/// Specifies the current operating state of a <see cref="PlaybackEngine"/>.
/// </summary>
public enum PlayerState
{
    /// <summary>
    /// The media player is stopped and is not currently presenting media.
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// The media player is actively playing and presenting media.
    /// </summary>
    Playing = 1,

    /// <summary>
    /// The media player is paused.
    /// </summary>
    Paused = 2,

    /// <summary>
    /// The media player finished playing.
    /// </summary>
    Finished = 3,

    /// <summary>
    /// The media player encountered an unrecoverable error during playback.
    /// </summary>
    /// <remarks>
    /// When the player enters this state, the <c>Faulted</c> event is raised
    /// with the exception that caused the failure.
    /// </remarks>
    Faulted = 4,
}