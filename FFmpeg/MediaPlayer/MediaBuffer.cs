using FFmpeg.Audio;
using FFmpeg.Formats;
using FFmpeg.Images;
using FFmpeg.Threading;
using FFmpeg.Utils;
using System.Diagnostics.CodeAnalysis;

namespace FFmpeg.MediaPlayer;

/// <summary>
/// Provides a synchronized buffer for streaming audio and video data.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="MediaBuffer"/> can contain an audio stream, a video stream, or both.
/// Audio and video data are buffered independently.
/// </para>
/// <para>
/// The buffer is designed for a single producer and a single consumer per stream.
/// Multiple concurrent writers or readers are not supported, as they could violate
/// the temporal ordering of the stream.
/// </para>
/// <para>
/// Synchronous read and write operations are non-blocking. Asynchronous operations
/// wait until the requested operation can be performed or the supplied cancellation
/// token is cancelled.
/// </para>
/// <para>
/// The <see cref="MaxBufferDuration"/> property specifies the target maximum duration
/// of buffered media. It is a soft limit, so an individual write may cause the
/// actual buffered duration to exceed the configured value.
/// </para>
/// </remarks>
public sealed class MediaBuffer : IDisposable
{
    private bool finishedWriting = false;
    private readonly object _lock = new();
    private readonly AudioBuffer? _audio;
    private readonly VideoBuffer? _video;
    private volatile bool _disposed = false;
    private readonly ManualResetEvent? _canReadAudioEvent, _canWriteAudioEvent, _canReadVideoEvent, _canWriteVideoEvent;

    /// <summary>
    /// Gets a value indicating whether audio data can currently be written.
    /// </summary>
    /// <remarks>
    /// Writing is allowed while the buffered audio duration is below
    /// <see cref="MaxBufferDuration"/>.
    /// </remarks>
    public bool CanWriteAudio
    {
        get
        {
            lock (_lock)
                return _audio != null &&
                            _audio.Value.Duration < MaxBufferDuration &&
                            !_disposed;
        }
    }

    /// <summary>
    /// Gets a value indicating whether video data can currently be written.
    /// </summary>
    /// <remarks>
    /// Writing is allowed while the buffered video duration is below
    /// <see cref="MaxBufferDuration"/>.
    /// </remarks>
    public bool CanWriteVideo
    {
        get
        {
            lock (_lock)
                return _video != null &&
                        _video.Value.Duration < MaxBufferDuration &&
                        !_disposed;
        }
    }

    /// <summary>
    /// Gets a value indicating whether audio data is currently available for reading.
    /// </summary>
    public bool CanReadAudio =>
        _audio != null &&
        _audio.Value.CanRead &&
        !_disposed;

    /// <summary>
    /// Gets a value indicating whether a video frame is currently available for reading.
    /// </summary>
    public bool CanReadVideo =>
        _video != null &&
        _video.Value.CanRead &&
        !_disposed;

    /// <summary>
    /// Creates a media buffer containing only a video stream.
    /// </summary>
    /// <param name="format">The pixel format of the video frames.</param>
    /// <param name="width">The width of the video frames in pixels.</param>
    /// <param name="height">The height of the video frames in pixels.</param>
    /// <returns>A new media buffer configured for the specified video format.</returns>
    public static MediaBuffer CreateVideo(
        PixelFormat format,
        int width,
        int height)
        => new(format, width, height, SampleFormat.None, 0, 0);

    /// <summary>
    /// Creates a media buffer for the specified stream.
    /// </summary>
    /// <param name="stream">
    /// The stream whose codec parameters are used to configure the buffer.
    /// </param>
    /// <returns>
    /// A new media buffer configured for the audio or video stream.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is neither an audio nor a video stream.
    /// </exception>
    public static MediaBuffer Create(AVStream stream) => stream.MediaType == MediaType.Video
            ? CreateVideo(
                stream.CodecParameters.PixelFormat,
                stream.CodecParameters.Width,
                stream.CodecParameters.Height)
            : stream.MediaType == MediaType.Audio
            ? CreateAudio(
                stream.CodecParameters.SampleFormat,
                stream.CodecParameters.Channels,
                stream.CodecParameters.SampleRate)
            : throw new ArgumentException(
            "The stream is neither audio not video",
            nameof(stream));

    /// <summary>
    /// Creates a media buffer containing an audio and a video stream.
    /// </summary>
    /// <param name="videoStream">
    /// The video stream whose codec parameters are used to configure the video buffer.
    /// </param>
    /// <param name="audioStream">
    /// The audio stream whose codec parameters are used to configure the audio buffer.
    /// </param>
    /// <returns>
    /// A new media buffer configured for the specified audio and video streams.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="videoStream"/> is not a video stream or
    /// <paramref name="audioStream"/> is not an audio stream.
    /// </exception>
    public static MediaBuffer Create(
        AVStream videoStream,
        AVStream audioStream) => videoStream.MediaType != MediaType.Video
            ? throw new ArgumentException(
                "The stream is not a video stream",
                nameof(videoStream))
            : audioStream.MediaType != MediaType.Audio
            ? throw new ArgumentException(
                "The stream is not an audio stream",
                nameof(audioStream))
            : Create(
            videoStream.CodecParameters.PixelFormat,
            videoStream.CodecParameters.Width,
            videoStream.CodecParameters.Height,
            audioStream.CodecParameters.SampleFormat,
            audioStream.CodecParameters.Channels,
            audioStream.CodecParameters.SampleRate);

    /// <summary>
    /// Creates a media buffer containing only an audio stream.
    /// </summary>
    /// <param name="format">The sample format of the audio data.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <param name="sampleRate">
    /// The audio sample rate in samples per second.
    /// </param>
    /// <returns>
    /// A new media buffer configured for the specified audio format.
    /// </returns>
    public static MediaBuffer CreateAudio(
        SampleFormat format,
        int channels,
        int sampleRate)
        => new(
            PixelFormat.None,
            0,
            0,
            format,
            channels,
            sampleRate);

    /// <summary>
    /// Creates a media buffer containing the specified audio and video formats.
    /// </summary>
    /// <param name="pixFmt">
    /// The pixel format of the video frames, or
    /// <see cref="PixelFormat.None"/> to omit video.
    /// </param>
    /// <param name="width">The width of the video frames in pixels.</param>
    /// <param name="height">The height of the video frames in pixels.</param>
    /// <param name="sampleFmt">
    /// The sample format of the audio data, or
    /// <see cref="SampleFormat.None"/> to omit audio.
    /// </param>
    /// <param name="channels">The number of audio channels.</param>
    /// <param name="sampleRate">
    /// The audio sample rate in samples per second.
    /// </param>
    /// <returns>
    /// A new media buffer configured for the specified formats.
    /// </returns>
    public static MediaBuffer Create(
        PixelFormat pixFmt,
        int width,
        int height,
        SampleFormat sampleFmt,
        int channels,
        int sampleRate)
        => new(
            pixFmt,
            width,
            height,
            sampleFmt,
            channels,
            sampleRate);

    private MediaBuffer(
        PixelFormat pixFmt,
        int width,
        int height,
        SampleFormat smpFmt,
        int channels,
        int sampleRate)
    {
        if (pixFmt != PixelFormat.None)
        {
            _video = new(pixFmt, width, height);
            _canWriteVideoEvent = new(true);
            _canReadVideoEvent = new(false);
        }

        if (smpFmt != SampleFormat.None)
        {
            _audio = new(smpFmt, channels, sampleRate);
            _canWriteAudioEvent = new(true);
            _canReadAudioEvent = new(false);
        }
    }

    /// <summary>
    /// Attempts to write an audio or video frame without blocking.
    /// </summary>
    /// <param name="frame">The frame to write.</param>
    /// <returns>
    /// The number of samples written for an audio frame, or <c>1</c> for a
    /// successfully written video frame. Returns <c>0</c> if the corresponding
    /// buffer cannot currently accept the frame.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the frame is neither an audio nor a video frame.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public int Write(AVFrame frame)
    {
        CheckDisposed();

        if (frame.IsAudio)
        {
            if (CanWriteAudio)
                lock (_lock)
                {
                    CheckDisposed();

                    if (!CanWriteAudio)
                        return 0;
                    if (finishedWriting)
                        throw new InvalidOperationException("The media buffer is in read only mode.");
                    int ret = _audio!.Value.Write(frame);
                    UpdateEvents();
                    return ret;
                }
            else
                return 0;
        }

        if (frame.IsVideo)
        {
            if (CanWriteVideo)
                lock (_lock)
                {
                    CheckDisposed();

                    if (!CanWriteVideo)
                        return 0;
                    if (finishedWriting)
                        throw new InvalidOperationException("The media buffer is in read only mode.");
                    _video!.Value.Write(frame);
                    UpdateEvents();
                    return 1;
                }
            else
                return 0;
        }

        throw new NotSupportedException(
            "Only audio or video frames are supported");
    }

    /// <summary>
    /// Writes an audio or video frame asynchronously.
    /// </summary>
    /// <param name="frame">The frame to write.</param>
    /// <param name="token">
    /// A token used to cancel the operation while waiting for buffer space.
    /// </param>
    /// <returns>
    /// A task containing the number of samples written for audio or
    /// <c>1</c> for a successfully written video frame.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the frame is neither an audio nor a video frame, or when
    /// the buffer does not support the frame's media type.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="token"/> is cancelled while waiting.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public ValueTask<int> WriteAsync(
        AVFrame frame,
        CancellationToken token = default)
    {
        CheckDisposed();

        return frame.IsAudio
            ? _audio.HasValue
                ? WriteAudioAsync(frame, token)
                : throw new NotSupportedException(
                "The buffer does not support audio data.")
            : frame.IsVideo
            ? _video.HasValue
                ? WriteVideoAsync(frame, token)
                : throw new NotSupportedException(
                "The buffer does not support video data.")
            : throw new NotSupportedException(
            "Only audio or video frames are supported");
    }

    /// <summary>
    /// Writes an audio frame asynchronously.
    /// </summary>
    /// <param name="frame">The audio frame to write.</param>
    /// <param name="token">
    /// A token used to cancel the operation while waiting for buffer space.
    /// </param>
    /// <returns>
    /// A task containing the number of samples written.
    /// </returns>
    private async ValueTask<int> WriteAudioAsync(
        AVFrame frame,
        CancellationToken token = default)
    {
        if (!CanWriteAudio)
            await _canWriteAudioEvent!
                .AsTask(token)
                .ConfigureAwait(false);

        lock (_lock)
        {
            CheckDisposed();
            if (finishedWriting)
                throw new InvalidOperationException("The media buffer is in read only mode.");
            int i = _audio!.Value.Write(frame);
            UpdateEvents();
            return i;
        }
    }

    /// <summary>
    /// Writes a video frame asynchronously.
    /// </summary>
    /// <param name="frame">The video frame to write.</param>
    /// <param name="token">
    /// A token used to cancel the operation while waiting for buffer space.
    /// </param>
    /// <returns>
    /// A task containing <c>1</c> when the frame was written.
    /// </returns>
    private async ValueTask<int> WriteVideoAsync(
        AVFrame frame,
        CancellationToken token = default)
    {
        if (!CanWriteVideo)
            await _canWriteVideoEvent!
                .AsTask(token)
                .ConfigureAwait(false);

        lock (_lock)
        {
            CheckDisposed();
            if (finishedWriting)
                throw new InvalidOperationException("The media buffer is in read only mode.");
            _video!.Value.Write(frame);
            UpdateEvents();
            return 1;
        }
    }

    /// <summary>
    /// Attempts to read the next video frame without blocking.
    /// </summary>
    /// <returns>
    /// The next video frame, or <see langword="null"/> if no frame is currently available.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public AVFrame? ReadVideo()
    {
        CheckDisposed();

        if (!CanReadVideo)
            return null;

        lock (_lock)
        {
            CheckDisposed();

            AVFrame? frame = _video!.Value.Read();
            UpdateEvents();
            return frame;
        }
    }

    /// <summary>
    /// Reads the next video frame asynchronously.
    /// </summary>
    /// <param name="token">
    /// A token used to cancel the operation while waiting for a frame.
    /// </param>
    /// <returns>
    /// A task containing the next video frame.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="token"/> is cancelled while waiting.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public async ValueTask<AVFrame?> ReadVideoAsync(
        CancellationToken token = default)
    {
        CheckDisposed();

        if (!CanReadVideo)
            await _canReadVideoEvent!
                .AsTask(token)
                .ConfigureAwait(false);

        return ReadVideo();
    }

    /// <summary>
    /// Attempts to read audio samples without blocking.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type used to receive the audio samples.
    /// </typeparam>
    /// <param name="data">The destination buffer for the samples.</param>
    /// <param name="pts">
    /// Receives the presentation timestamp of the first sample read.
    /// </param>
    /// <returns>
    /// The number of samples read, or <c>0</c> if no audio data is available.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public int ReadAudio<T>(
        Span<T> data,
        out TimeSpan pts)
        where T : unmanaged
    {
        CheckDisposed();

        if (!CanReadAudio)
        {
            pts = default;
            return 0;
        }

        lock (_lock)
        {
            CheckDisposed();

            int i = _audio!.Value.Read(data, out pts);
            UpdateEvents();
            return i;
        }
    }

    /// <summary>
    /// Reads audio samples asynchronously.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type used to receive the audio samples.
    /// </typeparam>
    /// <param name="data">The destination buffer for the samples.</param>
    /// <param name="token">
    /// A token used to cancel the operation while waiting for audio data.
    /// </param>
    /// <returns>
    /// A task containing the number of samples read and the presentation timestamp
    /// of the first sample.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="token"/> is cancelled while waiting.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public async ValueTask<(int Samples, TimeSpan PTS)> ReadAudioAsync<T>(
        Memory<T> data,
        CancellationToken token = default)
        where T : unmanaged
    {
        CheckDisposed();

        if (!CanReadAudio)
            await _canReadAudioEvent!
                .AsTask(token)
                .ConfigureAwait(false);

        int samples = ReadAudio(data.Span, out TimeSpan pts);
        return (samples, pts);
    }

    /// <summary>
    /// Peeks at the next video frame without removing it from the buffer.
    /// </summary>
    /// <param name="frame">
    /// Receives the next video frame when one is available.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a video frame is available; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public bool PeekVideo(
        [NotNullWhen(true)] out AVFrame? frame)
    {
        if (!_video.HasValue)
        {
            frame = null;
            return false;
        }

        lock (_lock)
        {
            CheckDisposed();
            return _video!.Value.Peek(out frame);
        }
    }

    /// <summary>
    /// Peeks at the presentation timestamp of the next audio data without
    /// removing it from the buffer.
    /// </summary>
    /// <param name="pts">
    /// Receives the presentation timestamp of the next audio data.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if audio data is available; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public bool PeekAudio(out TimeSpan pts)
    {
        pts = default;

        lock (_lock)
        {
            CheckDisposed();
            return _audio.HasValue &&
                   _audio.Value.Peek(out pts);
        }
    }

    /// <summary>
    /// Peeks at the next audio samples without removing them from the buffer.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type used to receive the audio samples.
    /// </typeparam>
    /// <param name="data">The destination buffer for the samples.</param>
    /// <param name="pts">
    /// Receives the presentation timestamp of the first available sample.
    /// </param>
    /// <returns>
    /// The number of samples copied, or <c>0</c> if no audio data is available.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public int PeekAudio<T>(
        Span<T> data,
        out TimeSpan pts)
        where T : unmanaged
    {
        if (!_audio.HasValue)
        {
            pts = default;
            return 0;
        }

        lock (_lock)
        {
            CheckDisposed();
            return _audio.Value.Peek(data, out pts);
        }
    }

    /// <summary>
    /// Waits asynchronously until audio data becomes available for reading.
    /// </summary>
    /// <param name="token">A token used to cancel the wait.</param>
    /// <returns>
    /// A task that completes when audio data is available.
    /// If the buffer does not contain an audio stream, the returned task is already completed.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="token"/> is cancelled while waiting.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public Task WaitForAudio(CancellationToken token = default)
    {
        CheckDisposed();

        return !_audio.HasValue ? Task.CompletedTask : _canReadAudioEvent!.AsTask(token);
    }

    /// <summary>
    /// Waits asynchronously until a video frame becomes available for reading.
    /// </summary>
    /// <param name="token">A token used to cancel the wait.</param>
    /// <returns>
    /// A task that completes when a video frame is available.
    /// If the buffer does not contain a video stream, the returned task is already completed.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="token"/> is cancelled while waiting.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public Task WaitForVideo(CancellationToken token = default)
    {
        CheckDisposed();

        return !_video.HasValue ? Task.CompletedTask : _canReadVideoEvent!.AsTask(token);
    }

    /// <summary>
    /// Seeks the buffered audio and video streams to the specified presentation timestamp.
    /// </summary>
    /// <param name="pts">The presentation timestamp to seek to.</param>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public void Seek(TimeSpan pts)
    {
        lock (_lock)
        {
            CheckDisposed();

            _audio?.Seek(pts);
            _video?.Seek(pts);

            UpdateEvents();
        }
    }

    /// <summary>
    /// Determines whether the buffered streams can seek to the specified timestamp.
    /// </summary>
    /// <param name="pts">The presentation timestamp to test.</param>
    /// <returns>
    /// <see langword="true"/> if every stream contained in the buffer can seek
    /// to the specified timestamp; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanSeek(TimeSpan pts) =>
        _audio?.CanSeek(pts) != false &&
        _video?.CanSeek(pts) != false;

    /// <summary>
    /// Gets or sets the target maximum duration of buffered media.
    /// </summary>
    /// <value>
    /// The maximum target duration of buffered media.
    /// The value is a soft limit, so an individual write may cause the actual
    /// buffered duration to exceed this value.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the assigned value is less than or equal to
    /// <see cref="TimeSpan.Zero"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public TimeSpan MaxBufferDuration
    {
        get;
        set
        {
            lock (_lock)
            {
                CheckDisposed();

                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "The timespan must be greater than 0.");

                field = value;

                _ = CanWriteAudio ? (_canWriteAudioEvent?.Set()) : (_canWriteAudioEvent?.Reset());

                _ = CanWriteVideo ? (_canWriteVideoEvent?.Set()) : (_canWriteVideoEvent?.Reset());
            }
        }
    } = TimeSpan.FromSeconds(0.1);

    /// <summary>
    /// Gets the current duration of the buffered video data.
    /// </summary>
    public TimeSpan VideoBufferDuration
    {
        get
        {
            lock (_lock)
                return _video?.Duration ?? TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Gets the current duration of the buffered audio data.
    /// </summary>
    public TimeSpan AudioBufferDuration
    {
        get
        {
            lock (_lock)
                return _audio?.Duration ?? TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Removes all buffered audio and video data.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public void Clear()
    {
        lock (_lock)
        {
            CheckDisposed();

            _audio?.Clear();
            _video?.Clear();

            UpdateEvents();
        }
    }

    /// <summary>
    /// Releases all resources used by the media buffer.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;

            _audio?.Dispose();
            _video?.Dispose();

            _ = _canReadAudioEvent?.Set();
            _ = _canWriteAudioEvent?.Set();
            _ = _canReadVideoEvent?.Set();
            _ = _canWriteVideoEvent?.Set();

            _canReadAudioEvent?.Dispose();
            _canWriteAudioEvent?.Dispose();
            _canReadVideoEvent?.Dispose();
            _canWriteVideoEvent?.Dispose();
        }
    }

    /// <summary>
    /// Throws an exception if this buffer has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    /// <summary>
    /// Updates the synchronization events to reflect the current buffer state.
    /// </summary>
    /// <remarks>
    /// This method must be called while holding <see cref="_lock"/> after an operation
    /// that can change the availability of buffered data or the available buffer space.
    /// </remarks>
    private void UpdateEvents()
    {
        if (finishedWriting)
            return;
        _ = CanReadAudio ? (_canReadAudioEvent?.Set()) : (_canReadAudioEvent?.Reset());

        _ = CanWriteAudio ? (_canWriteAudioEvent?.Set()) : (_canWriteAudioEvent?.Reset());

        _ = CanReadVideo ? (_canReadVideoEvent?.Set()) : (_canReadVideoEvent?.Reset());

        _ = CanWriteVideo ? (_canWriteVideoEvent?.Set()) : (_canWriteVideoEvent?.Reset());
    }

    /// <summary>
    /// Marks the buffer as finished writing and switches it to read-only mode.
    /// </summary>
    /// <remarks>
    /// Once writing has finished, no additional frames can be written until
    /// <see cref="MakeWriteable"/> is called. Any pending read or write operations
    /// are released so they can observe the changed state.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public void FinishWriting()
    {
        CheckDisposed();
        finishedWriting = true;
        _ = _canReadAudioEvent?.Set();
        _ = _canReadVideoEvent?.Set();
        _ = _canWriteAudioEvent?.Set();
        _ = _canWriteVideoEvent?.Set();
    }

    /// <summary>
    /// Re-enables writing to the buffer.
    /// </summary>
    /// <remarks>
    /// This method transitions the buffer from read-only mode back to writable
    /// mode and updates its synchronization state.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the buffer has been disposed.
    /// </exception>
    public void MakeWriteable()
    {
        CheckDisposed();
        finishedWriting = false;
        UpdateEvents();
    }

    /// <summary>
    /// Removes all expired audio data. Equal to seek but does not seek video.
    /// </summary>
    /// <param name="position">The presentation time stamp.</param>
    public void RemoveExpiredAudio(TimeSpan position)
    {
        if (!_audio.HasValue)
            return;
        lock (_lock)
        {
            CheckDisposed();
            _audio.Value.Seek(position);
        }
    }

    /// <summary>
    /// Removes all expired video data. Similar to seek but keeps at least one frame and does not seek audio.
    /// </summary>
    /// <param name="position">The presentation time stamp.</param>
    public void RemoveExpiredVideo(TimeSpan position)
    {
        if (!_video.HasValue)
            return;
        lock (_lock)
        {
            CheckDisposed();
            _video.Value.RemoveExpired(position);
        }
    }
}