using FFmpeg.AutoGen;
using FFmpeg.Utils;
using System.Buffers;

namespace FFmpeg.Audio;

/// <summary>
/// Represents a managed wrapper around FFmpeg's <c>AVAudioFifo</c> for audio buffering.
/// </summary>
/// <remarks>
/// <para>
/// This class provides safe, high-level methods to write and read audio data into and from an underlying <c>AVAudioFifo</c> buffer.  
/// It supports multiple input formats and channel configurations, and can handle packed ↔ planar (interleaved/unpacked) data internally.
/// </para>
/// <para>
/// Important: <see cref="AudioFifo"/> does <b>not convert between sample formats</b> (for example, float ↔ int16).  
/// The data is always stored internally according to the <see cref="Format"/> specified when the FIFO was created.
/// </para>
/// <para>
/// To avoid unnecessary copying and conversion overhead, it is recommended to provide data in the same planar/packed layout as the FIFO’s <see cref="Format"/>.  
/// If the input layout differs, the FIFO will perform planar ↔ packed conversions internally, which may allocate temporary buffers.
/// </para>
/// <para>
/// The FIFO maintains an internal buffer for audio samples, allowing for flexible processing pipelines in multi-channel or multi-format audio applications.
/// </para>
/// </remarks>
public unsafe partial class AudioFifo : IDisposable
{
    private const int BUFFER_SIZE = 81920;
    private AutoGen._AVAudioFifo* fifo;

    /// <summary>
    /// Gets the audio sample format used by this FIFO.
    /// </summary>
    public SampleFormat Format { get; }

    /// <summary>
    /// Gets the number of channels in the audio data handled by this FIFO.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioFifo"/> class with the specified sample format and channel count.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> representing the audio sample type and layout.</param>
    /// <param name="channels">The number of channels in the audio stream.</param>
    /// <remarks>
    /// Allocates a new <c>AVAudioFifo</c> with default initial capacity (0).  
    /// Use this constructor when you do not need to preallocate a specific buffer size.
    /// </remarks>
    public AudioFifo(SampleFormat format, int channels)
    {
        Format = format;
        Channels = channels;
        fifo = ffmpeg.av_audio_fifo_alloc((_AVSampleFormat)Format, Channels, 1);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioFifo"/> class with the specified sample format, channel count, and initial capacity.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> representing the audio sample type and layout.</param>
    /// <param name="channels">The number of channels in the audio stream.</param>
    /// <param name="capacity">The initial capacity of the FIFO, in number of samples per channel.</param>
    /// <remarks>
    /// Allocates a new <c>AVAudioFifo</c> with the specified initial capacity.  
    /// Use this constructor when you want to preallocate a buffer to reduce runtime allocations.
    /// </remarks>
    public AudioFifo(SampleFormat format, int channels, int capacity)
    {
        Format = format;
        Channels = channels;
        fifo = ffmpeg.av_audio_fifo_alloc((_AVSampleFormat)Format, Channels, capacity);
    }

    #region Write Helper Functions
    /// <summary>
    /// Writes audio data from an <see cref="AVFrame"/> into the <see cref="AudioFifo"/> buffer.
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> containing audio samples to write.  
    /// The frame’s number of channels must match the FIFO’s <see cref="Channels"/> property, 
    /// and the sample format must match the planar/packed layout of the FIFO's <see cref="Format"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written to the FIFO (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the frame's channel count does not match the FIFO’s <see cref="Channels"/>, 
    /// or if the frame's planar/packed layout does not match the FIFO's <see cref="Format"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The <see cref="AudioFifo"/> stores all audio data according to the <see cref="Format"/> specified when the FIFO was created.  
    /// This method does not convert between sample types (e.g., float ↔ int16); doing so must be handled by the caller.
    /// </para>
    /// <para>
    /// Planar ↔ packed conversions are handled automatically:
    /// <list type="bullet">
    /// <item>If the frame's format exactly matches the FIFO format, the data is written directly using <c>ffmpeg.av_audio_fifo_write</c>.</item>
    /// <item>If the FIFO expects planar but the frame is packed, the data is converted from packed to planar using <see cref="WritePackedToPlanar"/>.</item>
    /// <item>If the FIFO expects packed but the frame is planar, the data is converted from planar to packed using <see cref="WritePlanarToPacked"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// To avoid unnecessary copying, it is recommended to provide frames in the same planar/packed layout as the FIFO’s <see cref="Format"/>.
    /// </para>
    /// </remarks>
    public AVResult32 Write(AVFrame frame) => frame.ChannelLayout.Channels != Channels
            ? throw new ArgumentException("Frame channel count does not match the audio FIFO.", nameof(frame))
            : frame.SampleFormat.AsPlanar() != Format.AsPlanar()
            ? throw new ArgumentException("Frame planar/packed layout does not match the audio FIFO.", nameof(frame))
            : frame.SampleFormat == Format
            ? ffmpeg.av_audio_fifo_write(fifo, (void**)frame.ExtendedData, frame.SampleCount)
            : Format.IsPlanar()
            ? WritePackedToPlanar(frame.ExtendedData[0], frame.SampleCount)
            : WritePlanarToPacked(frame.ExtendedData, frame.SampleCount);
    private AVResult32 WritePackedToPacked(byte* data, int samples) => ffmpeg.av_audio_fifo_write(fifo, (void**)&data, samples);
    private AVResult32 WritePlanarToPlanar(byte** data, int samples) => ffmpeg.av_audio_fifo_write(fifo, (void**)data, samples);

    private AVResult32 WritePackedToPlanar(byte* data, int samples)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);

        try
        {
            int sampleSize = Format.GetBytesPerSample();

            // Number of samples per channel that fit into one conversion block.
            int sampelsPerChannel = buffer.Length / Channels / sampleSize;

            byte** planes = stackalloc byte*[Channels];

            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtr = buffer)
            {
                for (int channel = 0; channel < Channels; channel++)
                    planes[channel] = bufferPtr + (channel * sampelsPerChannel * sampleSize);


                while (samplesCopied < samples)
                {
                    int samplesToCopy = Math.Min(
                        sampelsPerChannel,
                        samples - samplesCopied);


                    for (int sample = 0; sample < samplesToCopy; sample++)
                    {
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            byte* destination =
                                planes[channel] + (sample * sampleSize);

                            byte* source =
                                data +
                                ((((samplesCopied + sample) * Channels) + channel) * sampleSize);


                            Buffer.MemoryCopy(
                                source,
                                destination,
                                sampleSize,
                                sampleSize);
                        }
                    }


                    AVResult32 res =
                        ffmpeg.av_audio_fifo_write(
                            fifo,
                            (void**)planes,
                            samplesToCopy);


                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;


                    samplesCopied += res;
                }
            }

            return samplesCopied;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    private AVResult32 WritePlanarToPacked(byte** data, int samples)
    {
        // Rent a buffer from ArrayPool
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            fixed (byte* bufferPtr = buffer)  // Pin the rented buffer to use a byte* pointer
            {
                int samplesPerChannel = buffer.Length / Format.GetBytesPerSample() / Channels;
                int sampleSize = Format.GetBytesPerSample(); // Size of each sample in bytes
                AVResult32 samplesCopied = 0;

                // Process the samples until we've copied all or encounter an error
                while (samplesCopied < samples)
                {
                    // Determine how many samples we can copy in this iteration
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);

                    // For each sample to copy
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        // For each channel, copy the corresponding bytes for the current sample
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            // Copy the data for this channel and sample into the interleaved buffer
                            for (int b = 0; b < sampleSize; b++)
                            {
                                // Write data from the unpacked input to the interleaved buffer
                                bufferPtr[((samplesCopied + sampleIndex) * Channels * sampleSize) + (channel * sampleSize) + b] =
                                    data[channel][(sampleIndex * sampleSize) + b];
                            }
                        }
                    }

                    // Write the packed samples to the FIFO buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_write(fifo, (void**)&bufferPtr, samplesToCopy);

                    // If the write failed or there were no samples copied, return the result
                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    // Update the number of samples that have been successfully copied
                    samplesCopied += res;
                }

                // Return the total number of samples successfully copied
                return samplesCopied;
            }
        }
        finally
        {
            // Return the rented buffer to the pool
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    #endregion

    #region Read Helper Functions
    /// <summary>
    /// Reads audio data from the <see cref="AudioFifo"/> buffer into an <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to receive audio samples.  
    /// If the frame has no buffer, its channel layout, sample format, and sample count can be automatically initialized:
    /// <list type="bullet">
    /// <item><description>If <see cref="AVFrame.ChannelLayout"/>.Channels is 0, it is set to the default layout for the FIFO’s <see cref="Channels"/>.</description></item>
    /// <item><description>If <see cref="AVFrame.SampleFormat"/> is <see cref="SampleFormat.None"/>, it is set to the FIFO’s <see cref="Format"/>.</description></item>
    /// <item><description>If <see cref="AVFrame.SampleCount"/> is less than 1, it is set to the current <see cref="AudioFifo.Count"/> (read all available samples).</description></item>
    /// </list>
    /// If the frame already has a buffer and any of these properties are unset, an exception is thrown.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read from the FIFO (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the frame already has a buffer allocated and some properties would need to be set,  
    /// or if, after initialization, the frame’s channel count or planar/packed layout does not match the FIFO.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The <see cref="AudioFifo"/> stores all audio data according to the <see cref="Format"/> specified when the FIFO was created.  
    /// This method does not convert between sample types (e.g., float ↔ int16); doing so must be handled by the caller.
    /// </para>
    /// <para>
    /// Planar ↔ packed conversions are handled automatically:
    /// <list type="bullet">
    /// <item>If the frame's format exactly matches the FIFO format, the data is read directly using <c>ffmpeg.av_audio_fifo_read</c>.</item>
    /// <item>If the FIFO stores planar but the frame is packed, the data is converted from planar to packed using <see cref="ReadPlanarToPacked"/>.</item>
    /// <item>If the FIFO stores packed but the frame is planar, the data is converted from packed to planar using <see cref="ReadPackedToPlanar"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// To avoid unnecessary copying, it is recommended to provide frames in the same planar/packed layout as the FIFO’s <see cref="Format"/>.
    /// </para>
    /// </remarks>
    public AVResult32 Read(AVFrame frame)
    {
        if (frame == null)
            throw new ArgumentNullException(nameof(frame));

        bool needsInitialization = frame.ChannelLayout.Channels == 0 || frame.SampleFormat == SampleFormat.None || frame.SampleCount <= 0;

        if (frame.HasBuffer && needsInitialization)
            throw new ArgumentException("Cannot set properties on a frame that already has a buffer.", nameof(frame));

        // Initialize properties only if the frame does not have a buffer
        if (!frame.HasBuffer)
        {
            if (frame.ChannelLayout.Channels == 0)
                frame.ChannelLayout.SetReferencedObject(ChannelLayout.CreateDefault(Channels));

            if (frame.SampleFormat == SampleFormat.None)
                frame.SampleFormat = Format;

            if (frame.SampleCount <= 0)
                frame.SampleCount = Count;
        }

        // Validate after initialization
        if (frame.ChannelLayout.Channels != Channels)
            throw new ArgumentException("Frame channel count does not match the audio FIFO.", nameof(frame));

        if (frame.SampleFormat.AsPlanar() != Format.AsPlanar())
            throw new ArgumentException("Frame planar/packed layout does not match the audio FIFO.", nameof(frame));

        // Create the buffer only if the frame did not already have one
        if (!frame.HasBuffer)
            frame.CreateBuffer().ThrowIfError();

        // Read from FIFO
        return frame.SampleFormat == Format
            ? ffmpeg.av_audio_fifo_read(fifo, (void**)frame.ExtendedData, frame.SampleCount)
            : Format.IsPlanar()
            ? ReadPlanarToPacked(frame.ExtendedData[0], frame.SampleCount)
            : ReadPackedToPlanar(frame.ExtendedData, frame.SampleCount);
    }

    private AVResult32 ReadPackedToPacked(byte* data, int samples) =>
        ffmpeg.av_audio_fifo_read(fifo, (void**)&data, samples);

    private AVResult32 ReadPlanarToPlanar(byte** data, int samples) =>
        ffmpeg.av_audio_fifo_read(fifo, (void**)data, samples);

    /// <summary>
    /// Reads audio data from a planar FIFO buffer and interleaves it into a packed byte array.
    /// </summary>
    /// <param name="data">
    /// A pointer to the target packed buffer where the interleaved audio samples will be stored.
    /// </param>
    /// <param name="samples">
    /// The number of audio samples to read per channel.
    /// </param>
    /// <returns>
    /// The number of samples successfully read, or an error code if the operation failed.
    /// </returns>
    private AVResult32 ReadPlanarToPacked(byte* data, int samples)
    {
        // Rent a temporary buffer from ArrayPool to hold planar data from the FIFO
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            byte** ptr = stackalloc byte*[Channels];
            int samplesPerChannel = buffer.Length / Format.GetBytesPerSample() / Channels;
            int sampleSize = Format.GetBytesPerSample();
            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtrs = buffer)
            {
                // Set up pointers for each channel to point into the temporary buffer
                for (int i = 0; i < Channels; i++)
                    ptr[i] = bufferPtrs + (i * samplesPerChannel * sampleSize);

                while (samplesCopied < samples)
                {
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);

                    // Read planar samples from the FIFO into the temporary buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_read(fifo, (void**)ptr, samplesToCopy);

                    // Copy and interleave the planar data into the user’s packed buffer
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            int dataIndex = ((samplesCopied + sampleIndex) * Channels * sampleSize) + (channel * sampleSize);
                            for (int b = 0; b < sampleSize; b++)
                                data[dataIndex + b] = ptr[channel][(sampleIndex * sampleSize) + b];
                        }
                    }

                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    samplesCopied += res;
                }
            }

            return samplesCopied;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Reads audio data from a packed FIFO buffer and deinterleaves it into planar channel buffers.
    /// </summary>
    /// <param name="data">
    /// An array of pointers, one per channel, where the deinterleaved samples will be stored.
    /// </param>
    /// <param name="samples">
    /// The number of audio samples to read per channel.
    /// </param>
    /// <returns>
    /// The number of samples successfully read, or an error code if the operation failed.
    /// </returns>
    private AVResult32 ReadPackedToPlanar(byte** data, int samples)
    {
        // Rent a temporary buffer from ArrayPool to hold packed data from the FIFO
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            int sampleSize = Format.GetBytesPerSample();
            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtr = buffer)
            {
                int samplesPerChannel = buffer.Length / sampleSize / Channels;

                while (samplesCopied < samples)
                {
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);
                    // Read packed samples from the FIFO into the temporary buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_read(fifo, (void**)&bufferPtr, samplesToCopy);

                    // Copy and deinterleave the packed data into the user’s planar buffers
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            for (int b = 0; b < sampleSize; b++)
                            {
                                data[channel][(sampleIndex * sampleSize) + b] =
                                    bufferPtr[((samplesCopied + sampleIndex) * Channels * sampleSize) + (channel * sampleSize) + b];
                            }
                        }
                    }

                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    samplesCopied += res;
                }

                return samplesCopied;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    #endregion


    #region Peek Helper Functions

    private AVResult32 PeekPackedToPacked(byte* data, int samples) =>
        ffmpeg.av_audio_fifo_peek(fifo, (void**)&data, samples);

    private AVResult32 PeekPlanarToPlanar(byte** data, int samples) =>
        ffmpeg.av_audio_fifo_peek(fifo, (void**)data, samples);

    /// <summary>
    /// Peeks audio data from a planar FIFO buffer and interleaves it into a packed byte array.
    /// </summary>
    /// <param name="data">
    /// A pointer to the target packed buffer where the interleaved audio samples will be stored.
    /// </param>
    /// <param name="samples">
    /// The number of audio samples to peek per channel.
    /// </param>
    /// <returns>
    /// The number of samples successfully peek, or an error code if the operation failed.
    /// </returns>
    private AVResult32 PeekPlanarToPacked(byte* data, int samples)
    {
        // Rent a temporary buffer from ArrayPool to hold planar data from the FIFO
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            byte** ptr = stackalloc byte*[Channels];
            int samplesPerChannel = buffer.Length / Format.GetBytesPerSample() / Channels;
            int sampleSize = Format.GetBytesPerSample();
            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtrs = buffer)
            {
                // Set up pointers for each channel to point into the temporary buffer
                for (int i = 0; i < Channels; i++)
                    ptr[i] = bufferPtrs + (i * samplesPerChannel * sampleSize);

                while (samplesCopied < samples)
                {
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);

                    // Peek planar samples from the FIFO into the temporary buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_peek(fifo, (void**)ptr, samplesToCopy);

                    // Copy and interleave the planar data into the user’s packed buffer
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            int dataIndex = ((samplesCopied + sampleIndex) * Channels * sampleSize) + (channel * sampleSize);
                            for (int b = 0; b < sampleSize; b++)
                                data[dataIndex + b] = ptr[channel][(sampleIndex * sampleSize) + b];
                        }
                    }

                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    samplesCopied += res;
                }
            }

            return samplesCopied;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Peeks audio data from a packed FIFO buffer and deinterleaves it into planar channel buffers.
    /// </summary>
    /// <param name="data">
    /// An array of pointers, one per channel, where the deinterleaved samples will be stored.
    /// </param>
    /// <param name="samples">
    /// The number of audio samples to peek per channel.
    /// </param>
    /// <returns>
    /// The number of samples successfully peek, or an error code if the operation failed.
    /// </returns>
    private AVResult32 PeekPackedToPlanar(byte** data, int samples)
    {
        // Rent a temporary buffer from ArrayPool to hold packed data from the FIFO
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            int sampleSize = Format.GetBytesPerSample();
            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtr = buffer)
            {
                int samplesPerChannel = buffer.Length / sampleSize / Channels;

                while (samplesCopied < samples)
                {
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);

                    // Peek packed samples from the FIFO into the temporary buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_peek(fifo, (void**)&bufferPtr, samplesToCopy);

                    // Copy and deinterleave the packed data into the user’s planar buffers
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            for (int b = 0; b < sampleSize; b++)
                            {
                                data[channel][(sampleIndex * sampleSize) + b] =
                                    bufferPtr[((samplesCopied + sampleIndex) * Channels * sampleSize) + (channel * sampleSize) + b];
                            }
                        }
                    }

                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    samplesCopied += res;
                }

                return samplesCopied;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    #endregion

    #region PeekAt Helper Functions

    private AVResult32 PeekPackedToPacked(byte* data, int samples, int offset) =>
        ffmpeg.av_audio_fifo_peek_at(fifo, (void**)&data, samples, offset);

    private AVResult32 PeekPlanarToPlanar(byte** data, int samples, int offset) =>
        ffmpeg.av_audio_fifo_peek_at(fifo, (void**)data, samples, offset);

    private AVResult32 PeekPlanarToPacked(byte* data, int samples, int offset)
    {
        // Rent a temporary buffer from ArrayPool to hold planar data from the FIFO
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            byte** ptr = stackalloc byte*[Channels];
            int samplesPerChannel = buffer.Length / Format.GetBytesPerSample() / Channels;
            int sampleSize = Format.GetBytesPerSample();
            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtrs = buffer)
            {
                // Set up pointers for each channel to point into the temporary buffer
                for (int i = 0; i < Channels; i++)
                    ptr[i] = bufferPtrs + (i * samplesPerChannel * sampleSize);

                while (samplesCopied < samples)
                {
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);

                    // Peek planar samples from the FIFO into the temporary buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_peek_at(fifo, (void**)ptr, samplesToCopy, offset);

                    // Copy and interleave the planar data into the user’s packed buffer
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            int dataIndex = ((samplesCopied + sampleIndex) * Channels * sampleSize) + (channel * sampleSize);
                            for (int b = 0; b < sampleSize; b++)
                                data[dataIndex + b] = ptr[channel][(sampleIndex * sampleSize) + b];
                        }
                    }

                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    samplesCopied += res;
                }
            }

            return samplesCopied;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Peeks audio samples from a packed FIFO buffer and deinterleaves them into
    /// separate planar channel buffers.
    /// </summary>
    /// <param name="data">
    /// An array of pointers, one for each channel, that receives the deinterleaved
    /// audio samples.
    /// </param>
    /// <param name="samples">
    /// The number of samples to peek for each channel.
    /// </param>
    /// <param name="offset">
    /// The sample offset, relative to the beginning of the FIFO, at which to start peeking.
    /// </param>
    /// <returns>
    /// The number of samples successfully peeked per channel, or an error code if the
    /// operation failed.
    /// </returns>
    private AVResult32 PeekPackedToPlanar(byte** data, int samples, int offset)
    {
        // Rent a temporary buffer from ArrayPool to hold packed data from the FIFO
        byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            int sampleSize = Format.GetBytesPerSample();
            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtr = buffer)
            {
                int samplesPerChannel = buffer.Length / sampleSize / Channels;

                while (samplesCopied < samples)
                {
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);

                    // Peek packed samples from the FIFO into the temporary buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_peek_at(fifo, (void**)&bufferPtr, samplesToCopy, offset);

                    // Copy and deinterleave the packed data into the user’s planar buffers
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            for (int b = 0; b < sampleSize; b++)
                            {
                                data[channel][(sampleIndex * sampleSize) + b] =
                                    bufferPtr[((samplesCopied + sampleIndex) * Channels * sampleSize) + (channel * sampleSize) + b];
                            }
                        }
                    }

                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    samplesCopied += res;
                }

                return samplesCopied;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    #endregion



    /// <summary>
    /// Drops (removes) a specified number of samples from the start of the FIFO buffer.
    /// </summary>
    /// <param name="samples">The number of samples to drop from the FIFO.</param>
    /// <returns>
    /// Returns 0 on success, or a negative error code if the operation fails.
    /// </returns>
    /// <remarks>
    /// This method reduces the number of available samples in the FIFO by <paramref name="samples"/>.  
    /// Dropping more samples than are currently available in the FIFO will result in an error.
    /// </remarks>
    public AVResult32 Drop(int samples) => ffmpeg.av_audio_fifo_drain(fifo, samples);

    /// <summary>
    /// Drops (removes) audio samples corresponding to the specified duration from
    /// the beginning of the FIFO buffer.
    /// </summary>
    /// <param name="duration">
    /// The duration of audio to remove from the FIFO.
    /// </param>
    /// <param name="sampleRate">
    /// The sample rate, in samples per second, used to convert the duration into
    /// a sample count.
    /// </param>
    /// <returns>
    /// <c>0</c> if the samples were successfully removed; otherwise, a negative
    /// error code.
    /// </returns>
    /// <remarks>
    /// The specified <paramref name="duration"/> is converted to a sample count
    /// using <paramref name="sampleRate"/>. Attempting to remove more samples than
    /// are currently stored in the FIFO results in an error.
    /// </remarks>
    public AVResult32 Drop(TimeSpan duration, int sampleRate)
    {
        int samples = (int)(duration.TotalSeconds * sampleRate);
        return ffmpeg.av_audio_fifo_drain(fifo, samples);
    }

    /// <summary>
    /// Clears all samples from the FIFO buffer, resetting it to an empty state.
    /// </summary>
    /// <remarks>
    /// After calling this method, the FIFO will contain zero samples.  
    /// This does not change the capacity of the FIFO, only the number of stored samples.
    /// </remarks>
    public void Clear()
    {
        ffmpeg.av_audio_fifo_reset(fifo);
    }


    /// <summary>
    /// Gets the total capacity of the audio FIFO in samples, including both used and available space.
    /// </summary>
    /// <remarks>
    /// Calculated as the sum of the currently available space (<see cref="ffmpeg.av_audio_fifo_space"/>) and the number of samples currently in the FIFO (<see cref="ffmpeg.av_audio_fifo_size"/>).
    /// </remarks>
    public int Capacity => ffmpeg.av_audio_fifo_space(fifo) + ffmpeg.av_audio_fifo_size(fifo);

    /// <summary>
    /// Gets the number of audio samples currently stored in the FIFO.
    /// </summary>
    /// <remarks>
    /// This corresponds to the value returned by <see cref="ffmpeg.av_audio_fifo_size"/> and represents the number of samples available for reading.
    /// </remarks>
    public int Count => ffmpeg.av_audio_fifo_size(fifo);

    #region Dispose
    private bool disposedValue;

    /// <summary>
    /// Releases the resources used by the <see cref="AudioFifo"/>.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (fifo != null)
            {
                ffmpeg.av_audio_fifo_free(fifo);
            }
            fifo = null;
            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure unmanaged resources are freed if <see cref="Dispose()"/> was not called.
    /// </summary>
    ~AudioFifo()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="AudioFifo"/>.
    /// </summary>
    /// <remarks>
    /// This method frees the underlying <see cref="ffmpeg.av_audio_fifo_free"/> and suppresses finalization.  
    /// Always call <see cref="Dispose()"/> when finishedReading using an <see cref="AudioFifo"/> instance to free unmanaged memory.
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

}
