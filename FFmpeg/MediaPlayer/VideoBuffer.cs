using FFmpeg.Collections;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Diagnostics.CodeAnalysis;

namespace FFmpeg.MediaPlayer;

internal struct VideoBuffer(PixelFormat format, int width, int height) : IDisposable
{
    private bool _disposed;
    private readonly CircularArray<AVFrame> _buffer = [];

    /// <summary>
    /// Gets the pixel format of the video frames stored in the buffer.
    /// </summary>
    public PixelFormat Format { get; } = format;

    /// <summary>
    /// Gets the width of the video frames stored in the buffer, in pixels.
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// Gets the height of the video frames stored in the buffer, in pixels.
    /// </summary>
    public int Height { get; } = height;


    /// <summary>
    /// Gets the approximate size of the currently buffered video frames in bytes.
    /// </summary>
    public long BufferSize
    {
        get; private set;
    }

    /// <summary>
    /// Gets the number of video frames currently buffered.
    /// </summary>
    public readonly int Count => _buffer.Count;

    /// <summary>
    /// Gets the timeline duration covered by the buffered frames.
    /// </summary>
    public readonly TimeSpan Duration
    {
        get
        {

            if (_buffer.Count == 0)
                return default;
            TimeSpan first = _buffer[0].GetPresentationTimestamp() * _buffer[0].TimeBase;
            TimeSpan end = (_buffer[^1].GetPresentationTimestamp() + _buffer[^1].Duration) * _buffer[^1].TimeBase;
            return end - first;
        }
    }

    /// <summary>
    /// Writes a video frame to the buffer and transfers ownership of the frame
    /// to the buffer.
    /// </summary>
    public void Write(AVFrame frame)
    {
        CheckDisposed();
        _buffer.Add(frame);
        BufferSize += frame.Size;
    }


    /// <summary>
    /// Gets the next frame without removing it from the buffer or transferring
    /// ownership to the caller.
    /// </summary>
    public readonly bool Peek([NotNullWhen(true)] out AVFrame? frame)
    {
        if (Count == 0)
        {
            frame = null;
            return false;
        }
        frame = _buffer[0];
        return true;
    }

    /// <summary>
    /// Removes and returns the next frame, transferring ownership of the frame
    /// to the caller.
    /// </summary>
    public readonly AVFrame? Read()
    {
        if (Count == 0)
            return null;
        AVFrame frame = _buffer[0];
        _buffer.RemoveAt(0);
        return frame;
    }



    /// <summary>
    /// Determines whether the specified presentation timestamp is covered by
    /// the buffered frames.
    /// </summary>
    public readonly bool CanSeek(TimeSpan pts)
    {
        if (!Peek(out AVFrame? frame))
            return false;
        TimeSpan first = frame.GetPresentationTimestamp() * frame.TimeBase;
        TimeSpan last = (_buffer[^1].GetPresentationTimestamp() + _buffer[^1].Duration) * _buffer[^1].TimeBase;
        return first >= pts && pts < last;
    }

    /// <summary>
    /// Removes all frames preceding the specified presentation timestamp.
    /// </summary>
    /// <returns>
    /// The presentation timestamp of the first frame remaining in the buffer.
    /// </returns>
    public readonly void Seek(TimeSpan pts)
    {
        while (_buffer.Count > 0 && pts > (_buffer[0].GetPresentationTimestamp() + _buffer[0].Duration) * _buffer[0].TimeBase)
        {
            _buffer[0].Dispose();
            _buffer.RemoveAt(0);
        }
    }

    /// <summary>
    /// Removes all frames preceding the specified presentation timestamp but keeps at least one.
    /// </summary>
    /// <returns>
    /// The presentation timestamp of the first frame remaining in the buffer.
    /// </returns>
    public readonly void RemoveExpired(TimeSpan pts)
    {
        while (_buffer.Count > 1 && pts > (_buffer[0].GetPresentationTimestamp() + _buffer[0].Duration) * _buffer[0].TimeBase)
        {
            _buffer[0].Dispose();
            _buffer.RemoveAt(0);
        }
    }

    /// <summary>
    /// Removes and disposes all frames currently stored in the buffer.
    /// </summary>
    public readonly void Clear()
    {
        for (int i = 0; i < _buffer.Count; i++)
            _buffer[i].Dispose();
        _buffer.Clear();
    }

    public void Dispose()
    {
        Clear();
        _disposed = true;
    }

    public readonly bool CanRead => _buffer.Count > 0 && !_disposed;

    private readonly void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}
