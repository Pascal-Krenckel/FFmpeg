using FFmpeg.Audio;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Diagnostics.CodeAnalysis;

namespace FFmpeg.MediaPlayer;

public partial class PlaybackEngine
{
    /// <inheritdoc cref="MediaBuffer.ReadVideo"/>
    public AVFrame? ReadVideo() => mediaBuffer.ReadVideo();

    /// <inheritdoc cref="MediaBuffer.ReadVideoAsync"/>
    public ValueTask<AVFrame?> ReadVideoAsync(CancellationToken token = default) => mediaBuffer.ReadVideoAsync(token);

    /// <inheritdoc cref="MediaBuffer.CanReadVideo"/>
    public bool HasVideoBuffered => mediaBuffer.CanReadVideo;

    /// <inheritdoc cref="MediaBuffer.PeekVideo(out AVFrame?)"/>
    public bool PeekVideo([NotNullWhen(true)] out AVFrame? video) => mediaBuffer.PeekVideo(out video);

    /// <inheritdoc cref="MediaBuffer.WaitForVideo(CancellationToken)"/>
    public Task WaitForVideo(CancellationToken token = default) => mediaBuffer.WaitForVideo(token);

    /// <inheritdoc cref="MediaBuffer.ReadAudio{T}(Span{T}, out TimeSpan)"/>
    public int ReadAudio<T>(Span<T> data, out TimeSpan pts) where T : unmanaged => mediaBuffer.ReadAudio(data, out pts);


    /// <inheritdoc cref="MediaBuffer.ReadAudioAsync{T}(Memory{T}, CancellationToken)"/>
    public ValueTask<(int Samples, TimeSpan PTS)> ReadAudioAsync<T>(Memory<T> data, CancellationToken token = default) where T : unmanaged => mediaBuffer.ReadAudioAsync(data, token);


    /// <inheritdoc cref="MediaBuffer.CanReadAudio"/>
    public bool HasAudioBuffered => mediaBuffer.CanReadAudio;

    /// <inheritdoc cref="MediaBuffer.PeekAudio{T}(Span{T}, out TimeSpan)"/>
    public int PeekAudio<T>(Span<T> data, out TimeSpan pts) where T : unmanaged => mediaBuffer.PeekAudio(data, out pts);

    /// <inheritdoc cref="MediaBuffer.PeekAudio(out TimeSpan)"/>
    public bool PeekAudio(out TimeSpan pts) => mediaBuffer.PeekAudio(out pts);

    /// <inheritdoc cref="MediaBuffer.WaitForAudio(CancellationToken)"/>
    public Task WaitForAudio(CancellationToken token = default) => mediaBuffer.WaitForAudio(token);

    /// <inheritdoc cref="MediaBuffer.AudioBufferDuration"/>
    public TimeSpan BufferedAudioDuration => mediaBuffer.AudioBufferDuration;

    /// <inheritdoc cref="MediaBuffer.VideoBufferDuration"/>
    public TimeSpan BufferedVideoDuration => mediaBuffer.VideoBufferDuration;

    /// <inheritdoc cref="MediaBuffer.MaxBufferDuration"/>
    public TimeSpan MaxBufferDuration { get => mediaBuffer.MaxBufferDuration; set => mediaBuffer.MaxBufferDuration = value; }

    /// <summary>
    /// Sets the maximum buffer duration based on an approximate buffer size
    /// in bytes.
    /// </summary>
    /// <param name="bufferSizeInBytes">
    /// The approximate maximum amount of memory, in bytes, that should be
    /// used by the media buffer.
    /// </param>
    /// <remarks>
    /// <para>
    /// The specified size is converted to a duration using an estimated
    /// uncompressed data rate for the selected audio and video streams.
    /// The resulting duration is assigned to <see cref="MaxBufferDuration"/>.
    /// </para>
    /// <para>
    /// For video, the data rate is estimated from the frame dimensions,
    /// pixel format, and frame rate when necessary. For audio, it is
    /// estimated from the sample rate, channel count, and sample format.
    /// </para>
    /// <para>
    /// When a filter graph is active, the output format and frame rate are
    /// used to estimate the data rate.
    /// </para>
    /// <para>
    /// The calculated value is an approximation. The actual memory usage of
    /// buffered media may differ due to frame allocation overhead, padding,
    /// alignment, and other data associated with <see cref="AVFrame"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSizeInBytes"/> is not positive.</exception>
    /// <exception cref="InvalidOperationException">No data rate could be estimated for the selected streams.</exception>
    public void SetBufferSize(long bufferSizeInBytes)
    {
        if (bufferSizeInBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSizeInBytes));
        long bpsVideo = 0;
        long bpsAudio = 0;

        #region B/s video

        if (videoStreamIndex >= 0)
        {
            if (videoFilterGraph == null)
            {
                // Buffered video consists of decoded frames, so the data rate is
                // frame size * frame rate (bits_per_raw_sample is per component, not per frame).
                Codecs.CodecContext codec = source.CodecContexts[videoStreamIndex];

                Rational frameRate = codec.FrameRate;
                if (!frameRate.IsValidTimeBase)
                    frameRate = source.GuessFramerate(videoStreamIndex);
                if (!frameRate.IsValidTimeBase)
                    frameRate = new(60, 1);

                // sw_pix_fmt is only known once decoding has started.
                PixelFormat pixelFormat = codec.SoftwarePixelFormat != PixelFormat.None
                    ? codec.SoftwarePixelFormat
                    : codec.PixelFormat;

                long frameSize = (long)codec.Width * codec.Height * pixelFormat.BitsPerPixel() / 8;

                bpsVideo = (long)(frameSize * (double)frameRate);
            }
            else
            {
                Rational frameRate = videoOut!.FrameRate;

                long frameSize =
                    (long)videoOut.Width *
                     videoOut.Height *
                     videoOut.PixelFormat.BitsPerPixel() / 8;

                if (!frameRate.IsValidTimeBase)
                    frameRate = new(60, 1);

                bpsVideo = (long)(frameSize * (double)frameRate);
            }
        }

        #endregion

        #region B/s audio

        if (audioStreamIndex >= 0)
        {
            if (audioFilterGraph == null)
            {
                long sampleRate =
                    source.CodecContexts[audioStreamIndex].SampleRate;

                long bitsPerSample =
                    source.CodecContexts[audioStreamIndex]
                        .ChannelLayout.Channels *
                    source.CodecContexts[audioStreamIndex]
                        .SampleFormat.GetBitsPerSample();

                bpsAudio = sampleRate * bitsPerSample / 8;
            }
            else
            {
                long sampleRate = audioOut!.SampleRate;

                long bitsPerSample =
                    audioOut.Channels *
                    audioOut.SampleFormat.GetBitsPerSample();

                bpsAudio = sampleRate * bitsPerSample / 8;
            }
        }

        #endregion

        long bps = bpsVideo + bpsAudio;
        if (bps <= 0)
            throw new InvalidOperationException("The data rate of the selected streams could not be determined.");

        mediaBuffer.MaxBufferDuration =
            TimeSpan.FromSeconds((double)bufferSizeInBytes / bps);
    }
}
