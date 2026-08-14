using FFmpeg.Audio;
using FFmpeg.Collections;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.MediaPlayer;

/// <summary>
/// Buffers decoded audio samples together with their presentation timestamps.
/// </summary>
/// <remarks>
/// <para>
/// The buffer stores audio samples in packed format using an <see cref="AudioFifo"/>
/// and maintains a separate presentation timeline for the buffered samples.
/// </para>
/// <para>
/// Presentation timestamp discontinuities are preserved. Read operations never
/// cross a discontinuity and therefore only return samples belonging to one
/// continuous presentation range.
/// </para>
/// <para>
/// The buffer is intended to have at most one concurrent writer and one concurrent
/// reader. Asynchronous read and write operations can wait for data to become
/// available or for buffer space to become available.
/// </para>
/// </remarks>
internal struct AudioBuffer(SampleFormat format, int channels, int sampleRate) : IDisposable
{
    private bool _disposed = false;

    private readonly AudioFifo fifo = new(format.AsPacked(), channels);
    private readonly CircularArray<AudioPresentationInfo> audioPresentationInfos = [];


    /// <summary>
    /// Gets the approximate size of the currently buffered audio data in bytes.
    /// </summary>
    public readonly long BufferSize => (long)Samples * Format.GetBytesPerSample() * Channels;

    /// <summary>
    /// Gets the number of currently buffered audio samples per channel.
    /// </summary>
    public readonly int Samples => fifo.Count;

    /// <summary>
    /// Gets the sample rate of the buffered audio.
    /// </summary>
    public int SampleRate { get; } = sampleRate;

    /// <summary>
    /// Gets the packed sample format used by the buffer.
    /// </summary>
    public readonly SampleFormat Format => fifo.Format;

    /// <summary>
    /// Gets the number of audio channels in the buffer.
    /// </summary>
    public readonly int Channels => fifo.Channels;

    /// <summary>
    /// Gets the duration of the timeline currently covered by the buffer.
    /// </summary>
    /// <remarks>
    /// The duration is calculated from the PTS of the first buffered sample to
    /// the end of the last buffered presentation range. Consequently, the
    /// duration includes gaps between presentation ranges and can be greater
    /// than the duration of the actual audio samples stored in the buffer.
    /// </remarks>
    public readonly TimeSpan Duration => !CanRead
                ? default
                : audioPresentationInfos[^1].GetNextPts(SampleRate) -
                   audioPresentationInfos[0].PTS;

    /// <summary>
    /// Releases all resources used by the audio buffer.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        fifo.Dispose();
        audioPresentationInfos.Clear();
        audioPresentationInfos.ShrinkToFit();



    }

    /// <summary>
    /// Gets a value indicating whether the buffer currently contains audio samples.
    /// </summary>
    public readonly bool CanRead => audioPresentationInfos.Count > 0 && !_disposed;

    /// <summary>
    /// Writes an audio frame to the buffer without waiting for available space.
    /// </summary>
    /// <param name="frame">The decoded audio frame to append to the buffer.</param>
    /// <returns>The number of samples per channel written to the buffer.</returns>
    public readonly int Write(AVFrame frame)
    {

        CheckDisposed();

        TimeSpan pts = frame.GetPresentationTimestamp() * frame.TimeBase;
        int samples = fifo.Write(frame);

        if (audioPresentationInfos.Count == 0)
            audioPresentationInfos.Add(new(pts, samples));
        else if (audioPresentationInfos[^1].GetNextPts(SampleRate) == pts)
            audioPresentationInfos[^1].Samples += samples;
        else
            audioPresentationInfos.Add(new(pts, samples));

        return samples;

    }

    /// <summary>
    /// Copies audio samples from the beginning of the buffer without consuming them.
    /// </summary>
    /// <typeparam name="T">
    /// The managed type corresponding to the buffer's <see cref="Format"/>.
    /// </typeparam>
    /// <param name="array">The destination span for the samples.</param>
    /// <param name="pts">
    /// When this method returns, contains the PTS of the first sample in the buffer.
    /// </param>
    /// <returns>
    /// The number of samples per channel copied to <paramref name="array"/>.
    /// </returns>
    /// <remarks>
    /// Only samples from the first continuous presentation range are returned.
    /// Passing an empty span is valid and can be used to obtain the PTS without
    /// copying any audio data.
    /// </remarks>
    public readonly int Peek<T>(Span<T> array, out TimeSpan pts) where T : unmanaged
    {
        CheckDisposed();

        if (typeof(T) != typeof(byte))
            Format.ValidateType<T>();

        if (audioPresentationInfos.Count == 0)
        {
            pts = default;
            return 0;
        }

        Span<byte> byteArray = MemoryMarshal.AsBytes(array);
        int maxCountSamples = audioPresentationInfos[0].Samples;
        int maxSizeInBytes = maxCountSamples * Channels * Format.GetBytesPerSample();

        byteArray = byteArray[..Math.Min(byteArray.Length, maxSizeInBytes)];
        pts = audioPresentationInfos[0].PTS;

        return fifo.Peek(byteArray);
    }
    public readonly bool Peek(out TimeSpan pts)
    {
        CheckDisposed();
        if (audioPresentationInfos.Count == 0)
        {
            pts = default;
            return false;
        }
        pts = audioPresentationInfos[0].PTS;
        return true;
    }

    /// <summary>
    /// Reads audio samples from the beginning of the buffer without waiting.
    /// </summary>
    /// <typeparam name="T">
    /// The managed type corresponding to the buffer's <see cref="Format"/>.
    /// </typeparam>
    /// <param name="array">The destination span for the samples.</param>
    /// <param name="pts">
    /// When this method returns, contains the PTS of the first sample read.
    /// </param>
    /// <returns>
    /// The number of samples per channel read from the buffer, or zero if no
    /// samples are currently available.
    /// </returns>
    /// <remarks>
    /// Reading never crosses a presentation timestamp discontinuity.
    /// </remarks>
    public readonly int Read<T>(Span<T> array, out TimeSpan pts) where T : unmanaged
    {
        CheckDisposed();

        if (typeof(T) != typeof(byte))
            Format.ValidateType<T>();

        if (audioPresentationInfos.Count == 0)
        {
            pts = default;
            return 0;
        }

        Span<byte> byteArray = MemoryMarshal.AsBytes(array);
        int maxCountSamples = audioPresentationInfos[0].Samples;
        int maxSizeInBytes = maxCountSamples * Channels * Format.GetBytesPerSample();

        byteArray = byteArray[..Math.Min(byteArray.Length, maxSizeInBytes)];

        pts = audioPresentationInfos[0].PTS;
        int samplesRead = fifo.Read(byteArray);

        if (samplesRead == maxCountSamples)
            audioPresentationInfos.RemoveAt(0);
        else
            audioPresentationInfos[0].Remove(samplesRead, SampleRate);


        return samplesRead;

    }

    /// <summary>
    /// Determines whether the specified presentation timestamp can be reached
    /// using the currently buffered audio.
    /// </summary>
    /// <param name="pts">The presentation timestamp to test.</param>
    /// <returns>
    /// <see langword="true"/> if the timestamp lies within the timeline covered
    /// by the buffer; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method considers the complete timeline range between the first and
    /// last buffered presentation ranges. The requested timestamp may therefore
    /// lie within a discontinuity between buffered audio ranges.
    /// </remarks>
    public readonly bool CanSeek(TimeSpan pts)
    {
        if (_disposed || audioPresentationInfos.Count == 0)
            return false;

        TimeSpan first = audioPresentationInfos[0].PTS;
        TimeSpan last = audioPresentationInfos[^1].GetNextPts(SampleRate);

        return pts >= first && pts <= last;
    }

    /// <summary>
    /// Discards buffered audio before the specified presentation timestamp.
    /// </summary>
    /// <param name="pts">The presentation timestamp to seek to.</param>
    /// <remarks>
    /// <para>
    /// If <paramref name="pts"/> lies within a buffered presentation range,
    /// samples before that position are discarded.
    /// </para>
    /// <para>
    /// If <paramref name="pts"/> lies within a discontinuity, all samples before
    /// the discontinuity are discarded and the buffer is positioned at the first
    /// available sample following the gap.
    /// </para>
    /// <para>
    /// If the requested timestamp is after all buffered audio, the buffer is cleared.
    /// If it is before the first buffered timestamp, no samples are discarded.
    /// </para>
    /// </remarks>
    public readonly void Seek(TimeSpan pts)
    {
        CheckDisposed();

        if (audioPresentationInfos.Count > 0)
        {
            int index = AudioPresentationInfo.BinarySearch(
                audioPresentationInfos, pts, SampleRate);

            if (index >= audioPresentationInfos.Count)
            {
                Clear();
                return;
            }

            int samplesToDelete = 0;

            for (int j = 0; j < index; j++)
            {
                samplesToDelete += audioPresentationInfos[0].Samples;
                audioPresentationInfos.RemoveAt(0);
            }

            int samplesToDeleteUntilPts = Math.Max(
                0,
                (int)(SampleRate * (pts - audioPresentationInfos[0].PTS).TotalSeconds));

            samplesToDelete += samplesToDeleteUntilPts;
            audioPresentationInfos[0].Remove(samplesToDeleteUntilPts, SampleRate);

            if (audioPresentationInfos[0].Samples == 0)
                audioPresentationInfos.RemoveAt(0);

            fifo.Drop(samplesToDelete).ThrowIfError();

        }
    }

    /// <summary>
    /// Removes all audio samples and presentation information from the buffer.
    /// </summary>
    public readonly void Clear()
    {
        CheckDisposed();
        audioPresentationInfos.Clear();
        fifo.Clear();
    }

    private readonly void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    private struct AudioPresentationInfo(TimeSpan pts, int samples)
    {
        public TimeSpan PTS { get; set; } = pts;

        public int Samples { get; set; } = samples;

        public readonly TimeSpan GetNextPts(int sampleRate) =>
            PTS + TimeSpan.FromSeconds((double)Samples / sampleRate);

        public void Remove(int samples, int sampleRate)
        {
            samples = Math.Clamp(samples, 0, Samples);
            Samples -= samples;
            PTS += TimeSpan.FromSeconds((double)samples / sampleRate);
        }

        public static int BinarySearch(
            IList<AudioPresentationInfo> orderedList,
            TimeSpan pts,
            int sampleRate)
        {
            int start = 0;
            int end = orderedList.Count;

            while (start < end)
            {
                int m = (end - start) / 2;
                AudioPresentationInfo info = orderedList[m];

                if (pts < info.PTS)
                    end = m;
                else if (info.GetNextPts(sampleRate) <= pts)
                    start = m + 1;
                else
                    return m;
            }

            return start;
        }
    }
}