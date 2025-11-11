using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Text;

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
public unsafe class AudioFifo : IDisposable
{
    private const int BUFFER_SIZE = 81920;
    AutoGen._AVAudioFifo* fifo;

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

    #region Write
    #region Helper Functions
    private AVResult32 WritePackedToPacked(byte* data, int samples) => ffmpeg.av_audio_fifo_write(fifo, (void**)&data, samples);
    private AVResult32 WritePlanarToPlanar(byte** data, int samples) => ffmpeg.av_audio_fifo_write(fifo, (void**)data, samples);

    private AVResult32 WritePackedToPlanar(byte* data, int samples)
    {
        // Rent a buffer from ArrayPool
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        try
        {
            byte** ptr = stackalloc byte*[Channels];
            int samplesPerChannel = buffer.Length / Format.GetBytesPerSample() / Channels;
            int sampleSize = Format.GetBytesPerSample();
            AVResult32 samplesCopied = 0;

            fixed (byte* bufferPtrs = buffer)
            {
                // Set up pointers for each channel to point to their respective locations in the buffer
                for (int i = 0; i < Channels; i++)
                    ptr[i] = bufferPtrs + i * samplesPerChannel * sampleSize;

                // Process the samples until we've copied all or encounter an error
                while (samplesCopied < samples)
                {
                    // Determine how many samples we can copy in this iteration
                    int samplesToCopy = Math.Min(samplesPerChannel, samples - samplesCopied);

                    // Copy each sample from the interleaved 'data' array to the 'ptr' buffer for each channel
                    for (int sampleIndex = 0; sampleIndex < samplesToCopy; sampleIndex++)
                    {
                        // For each channel, copy the corresponding bytes for the current sample
                        for (int channel = 0; channel < Channels; channel++)
                        {
                            // Calculate the index in the interleaved input data array
                            int dataIndex = (samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize;

                            // Copy the data for this channel and sample into the corresponding buffer position
                            for (int b = 0; b < sampleSize; b++)
                            {
                                ptr[channel][sampleIndex * sampleSize + b] = data[dataIndex + b];
                            }
                        }
                    }

                    // Write the samples to the FIFO buffer
                    AVResult32 res = ffmpeg.av_audio_fifo_write(fifo, (void**)ptr, samplesToCopy);

                    // If the write failed or there were no samples copied, return the result
                    if (res <= 0)
                        return samplesCopied > 0 ? samplesCopied : res;

                    // Update the number of samples that have been successfully copied
                    samplesCopied += res;
                }
            }
            // Return the total number of samples successfully copied
            return samplesCopied;
        }
        finally
        {
            // Return the rented buffer to the pool
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    private AVResult32 WritePlanarToPacked(byte** data, int samples)
    {
        // Rent a buffer from ArrayPool
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
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
                                bufferPtr[(samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize + b] =
                                    data[channel][sampleIndex * sampleSize + b];
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
    public AVResult32 Write(AVFrame frame)
    {
        if (frame.ChannelLayout.Channels != Channels)
            throw new ArgumentException("Frame channel count does not match the audio FIFO.", nameof(frame));

        if (frame.SampleFormat.AsPlanar() != Format.AsPlanar())
            throw new ArgumentException("Frame planar/packed layout does not match the audio FIFO.", nameof(frame));

        if (frame.SampleFormat == Format) // no planar <-> packed conversion needed
            return ffmpeg.av_audio_fifo_write(fifo, (void**)frame.ExtendedData, frame.SampleCount);

        if (Format.IsPlanar()) // write from packed to planar
            return WritePackedToPlanar(frame.ExtendedData[0], frame.SampleCount);
        else // write from planar to packed 
            return WritePlanarToPacked(frame.ExtendedData, frame.SampleCount);
    }

    #region Write(ReadOnlySpan<byte> ch1, ch2....) and ReadOnlySpan<T>
    /// <summary>
    /// Writes packed multi-channel audio data from a single contiguous buffer.
    /// </summary>
    /// <param name="packedData">
    /// A read-only span containing interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each frame.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of frames successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The number of channels is determined from the current audio stream's <see cref="Channels"/> property.  
    /// The total number of frames is calculated as <c>packedData.Length / (Channels × Format.GetBytesPerSample())</c>.
    /// </remarks>
    public AVResult32 Write(ReadOnlySpan<byte> packedData)
    {
        fixed (byte* bufferPtr = packedData)
            if (Format.IsPlanar())
                return WritePackedToPlanar(bufferPtr, packedData.Length / Format.GetBytesPerSample() / Channels);
            else
                return WritePackedToPacked(bufferPtr, packedData.Length / Format.GetBytesPerSample() / Channels);
    }


    /// <summary>
    /// Writes packed multi-channel audio data from a read-only span of typed samples.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// Its size must match <see cref="Format.GetBytesPerSample()"/>.
    /// <param name="buffer">
    /// A read-only span containing interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each frame.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of frames successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The number of channels is determined from the current audio stream's <see cref="Channels"/> property.  
    /// This method simply reinterprets the <typeparamref name="T"/> span as bytes and calls 
    /// <see cref="Write(ReadOnlySpan{byte})"/>.
    /// </remarks>
    public AVResult32 Write<T>(ReadOnlySpan<T> buffer) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(buffer));

    /// <summary>
    /// Writes stereo (two-channel) audio data from separate left and right channel buffers.
    /// </summary>
    /// <param name="left">
    /// A read-only span containing audio sample data for the left channel.
    /// </param>
    /// <param name="right">
    /// A read-only span containing audio sample data for the right channel.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly two channels.  
    /// This method supports only stereo output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        if (Channels != 2)
            throw new NotSupportedException("Write(ReadOnlySpan<byte>, ReadOnlySpan<byte>) only supports stereo (2-channel) audio output.");

        byte** ptrs = stackalloc byte*[2];
        int samples = Math.Min(left.Length, right.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(left));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(right));

        if (Format.IsPlanar())
            return WritePlanarToPlanar(ptrs, samples);
        else
            return WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes stereo (two-channel) audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="left">
    /// A read-only span containing left-channel samples.
    /// </param>
    /// <param name="right">
    /// A read-only span containing right-channel samples.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly two channels.  
    /// This method supports only stereo output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(left), MemoryMarshal.AsBytes(right));

    /// <summary>
    /// Writes 3-channel audio data from separate channel buffers.
    /// </summary>
    /// <param name="ch1">
    /// A read-only span containing audio sample data for the first channel.
    /// </param>
    /// <param name="ch2">
    /// A read-only span containing audio sample data for the second channel.
    /// </param>
    /// <param name="ch3">
    /// A read-only span containing audio sample data for the third channel.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly three channels.  
    /// This method supports only 3-channel output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> ch1, ReadOnlySpan<byte> ch2, ReadOnlySpan<byte> ch3)
    {
        if (Channels != 3)
            throw new NotSupportedException("Write(3-channel) only supports 3-channel audio output.");

        byte** ptrs = stackalloc byte*[3];
        int samples = Math.Min(ch1.Length, Math.Min(ch2.Length, ch3.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes 3-channel audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="ch1">
    /// A read-only span containing samples for the first channel.
    /// </param>
    /// <param name="ch2">
    /// A read-only span containing samples for the second channel.
    /// </param>
    /// <param name="ch3">
    /// A read-only span containing samples for the third channel.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly three channels.  
    /// This method supports only 3-channel output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> ch1, ReadOnlySpan<T> ch2, ReadOnlySpan<T> ch3) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3));


    /// <summary>
    /// Writes 4-channel audio data from separate channel buffers.
    /// </summary>
    /// <param name="ch1">A read-only span containing audio sample data for the first channel.</param>
    /// <param name="ch2">A read-only span containing audio sample data for the second channel.</param>
    /// <param name="ch3">A read-only span containing audio sample data for the third channel.</param>
    /// <param name="ch4">A read-only span containing audio sample data for the fourth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly four channels.  
    /// This method supports only 4-channel output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> ch1, ReadOnlySpan<byte> ch2, ReadOnlySpan<byte> ch3, ReadOnlySpan<byte> ch4)
    {
        if (Channels != 4)
            throw new NotSupportedException("Write(4-channel) only supports 4-channel audio output.");

        byte** ptrs = stackalloc byte*[4];
        int samples = Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes 4-channel audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="ch1">A read-only span containing samples for the first channel.</param>
    /// <param name="ch2">A read-only span containing samples for the second channel.</param>
    /// <param name="ch3">A read-only span containing samples for the third channel.</param>
    /// <param name="ch4">A read-only span containing samples for the fourth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly four channels.  
    /// This method supports only 4-channel output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> ch1, ReadOnlySpan<T> ch2, ReadOnlySpan<T> ch3, ReadOnlySpan<T> ch4) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3), MemoryMarshal.AsBytes(ch4));

    /// <summary>
    /// Writes 5-channel audio data from separate channel buffers.
    /// </summary>
    /// <param name="ch1">A read-only span containing audio sample data for the first channel.</param>
    /// <param name="ch2">A read-only span containing audio sample data for the second channel.</param>
    /// <param name="ch3">A read-only span containing audio sample data for the third channel.</param>
    /// <param name="ch4">A read-only span containing audio sample data for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing audio sample data for the fifth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly five channels.  
    /// This method supports only 5-channel output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> ch1, ReadOnlySpan<byte> ch2, ReadOnlySpan<byte> ch3, ReadOnlySpan<byte> ch4, ReadOnlySpan<byte> ch5)
    {
        if (Channels != 5)
            throw new NotSupportedException("Write(5-channel) only supports 5-channel audio output.");

        byte** ptrs = stackalloc byte*[5];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)), ch5.Length)
                      / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes 5-channel audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="ch1">A read-only span containing samples for the first channel.</param>
    /// <param name="ch2">A read-only span containing samples for the second channel.</param>
    /// <param name="ch3">A read-only span containing samples for the third channel.</param>
    /// <param name="ch4">A read-only span containing samples for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing samples for the fifth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly five channels.  
    /// This method supports only 5-channel output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> ch1, ReadOnlySpan<T> ch2, ReadOnlySpan<T> ch3, ReadOnlySpan<T> ch4, ReadOnlySpan<T> ch5) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3), MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5));


    /// <summary>
    /// Writes 6-channel audio data from separate channel buffers.
    /// </summary>
    /// <param name="ch1">A read-only span containing audio sample data for the first channel.</param>
    /// <param name="ch2">A read-only span containing audio sample data for the second channel.</param>
    /// <param name="ch3">A read-only span containing audio sample data for the third channel.</param>
    /// <param name="ch4">A read-only span containing audio sample data for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing audio sample data for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing audio sample data for the sixth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly six channels.  
    /// This method supports only 6-channel output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> ch1, ReadOnlySpan<byte> ch2, ReadOnlySpan<byte> ch3, ReadOnlySpan<byte> ch4, ReadOnlySpan<byte> ch5, ReadOnlySpan<byte> ch6)
    {
        if (Channels != 6)
            throw new NotSupportedException("Write(6-channel) only supports 6-channel audio output.");

        byte** ptrs = stackalloc byte*[6];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(ch5.Length, ch6.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes 6-channel audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="ch1">A read-only span containing samples for the first channel.</param>
    /// <param name="ch2">A read-only span containing samples for the second channel.</param>
    /// <param name="ch3">A read-only span containing samples for the third channel.</param>
    /// <param name="ch4">A read-only span containing samples for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing samples for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing samples for the sixth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly six channels.  
    /// This method supports only 6-channel output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> ch1, ReadOnlySpan<T> ch2, ReadOnlySpan<T> ch3, ReadOnlySpan<T> ch4, ReadOnlySpan<T> ch5, ReadOnlySpan<T> ch6) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3), MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6));


    /// <summary>
    /// Writes 7-channel audio data from separate channel buffers.
    /// </summary>
    /// <param name="ch1">A read-only span containing audio sample data for the first channel.</param>
    /// <param name="ch2">A read-only span containing audio sample data for the second channel.</param>
    /// <param name="ch3">A read-only span containing audio sample data for the third channel.</param>
    /// <param name="ch4">A read-only span containing audio sample data for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing audio sample data for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing audio sample data for the sixth channel.</param>
    /// <param name="ch7">A read-only span containing audio sample data for the seventh channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly seven channels.  
    /// This method supports only 7-channel output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> ch1, ReadOnlySpan<byte> ch2, ReadOnlySpan<byte> ch3, ReadOnlySpan<byte> ch4,
                            ReadOnlySpan<byte> ch5, ReadOnlySpan<byte> ch6, ReadOnlySpan<byte> ch7)
    {
        if (Channels != 7)
            throw new NotSupportedException("Write(7-channel) only supports 7-channel audio output.");

        byte** ptrs = stackalloc byte*[7];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), ch7.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes 7-channel audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="ch1">A read-only span containing samples for the first channel.</param>
    /// <param name="ch2">A read-only span containing samples for the second channel.</param>
    /// <param name="ch3">A read-only span containing samples for the third channel.</param>
    /// <param name="ch4">A read-only span containing samples for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing samples for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing samples for the sixth channel.</param>
    /// <param name="ch7">A read-only span containing samples for the seventh channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly seven channels.  
    /// This method supports only 7-channel output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> ch1, ReadOnlySpan<T> ch2, ReadOnlySpan<T> ch3, ReadOnlySpan<T> ch4,
                               ReadOnlySpan<T> ch5, ReadOnlySpan<T> ch6, ReadOnlySpan<T> ch7) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                 MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6), MemoryMarshal.AsBytes(ch7));


    /// <summary>
    /// Writes 8-channel audio data from separate channel buffers.
    /// </summary>
    /// <param name="ch1">A read-only span containing audio sample data for the first channel.</param>
    /// <param name="ch2">A read-only span containing audio sample data for the second channel.</param>
    /// <param name="ch3">A read-only span containing audio sample data for the third channel.</param>
    /// <param name="ch4">A read-only span containing audio sample data for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing audio sample data for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing audio sample data for the sixth channel.</param>
    /// <param name="ch7">A read-only span containing audio sample data for the seventh channel.</param>
    /// <param name="ch8">A read-only span containing audio sample data for the eighth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly eight channels.  
    /// This method supports only 8-channel output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> ch1, ReadOnlySpan<byte> ch2, ReadOnlySpan<byte> ch3, ReadOnlySpan<byte> ch4,
                            ReadOnlySpan<byte> ch5, ReadOnlySpan<byte> ch6, ReadOnlySpan<byte> ch7, ReadOnlySpan<byte> ch8)
    {
        if (Channels != 8)
            throw new NotSupportedException("Write(8-channel) only supports 8-channel audio output.");

        byte** ptrs = stackalloc byte*[8];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), Math.Min(ch7.Length, ch8.Length)))
                               / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes 8-channel audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="ch1">A read-only span containing samples for the first channel.</param>
    /// <param name="ch2">A read-only span containing samples for the second channel.</param>
    /// <param name="ch3">A read-only span containing samples for the third channel.</param>
    /// <param name="ch4">A read-only span containing samples for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing samples for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing samples for the sixth channel.</param>
    /// <param name="ch7">A read-only span containing samples for the seventh channel.</param>
    /// <param name="ch8">A read-only span containing samples for the eighth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly eight channels.  
    /// This method supports only 8-channel output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> ch1, ReadOnlySpan<T> ch2, ReadOnlySpan<T> ch3, ReadOnlySpan<T> ch4,
                               ReadOnlySpan<T> ch5, ReadOnlySpan<T> ch6, ReadOnlySpan<T> ch7, ReadOnlySpan<T> ch8) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                 MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                 MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8));


    /// <summary>
    /// Writes 9-channel audio data from separate channel buffers.
    /// </summary>
    /// <param name="ch1">A read-only span containing audio sample data for the first channel.</param>
    /// <param name="ch2">A read-only span containing audio sample data for the second channel.</param>
    /// <param name="ch3">A read-only span containing audio sample data for the third channel.</param>
    /// <param name="ch4">A read-only span containing audio sample data for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing audio sample data for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing audio sample data for the sixth channel.</param>
    /// <param name="ch7">A read-only span containing audio sample data for the seventh channel.</param>
    /// <param name="ch8">A read-only span containing audio sample data for the eighth channel.</param>
    /// <param name="ch9">A read-only span containing audio sample data for the ninth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly nine channels.  
    /// This method supports only 9-channel output.
    /// </exception>
    public AVResult32 Write(ReadOnlySpan<byte> ch1, ReadOnlySpan<byte> ch2, ReadOnlySpan<byte> ch3, ReadOnlySpan<byte> ch4,
                            ReadOnlySpan<byte> ch5, ReadOnlySpan<byte> ch6, ReadOnlySpan<byte> ch7,
                            ReadOnlySpan<byte> ch8, ReadOnlySpan<byte> ch9)
    {
        if (Channels != 9)
            throw new NotSupportedException("Write(9-channel) only supports 9-channel audio output.");

        byte** ptrs = stackalloc byte*[9];
        int samples = Math.Min(Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length),
                                                Math.Min(ch3.Length, ch4.Length)),
                                        Math.Min(Math.Min(ch5.Length, ch6.Length),
                                                Math.Min(ch7.Length, ch8.Length))),
                               ch9.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));
        ptrs[8] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch9));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

    /// <summary>
    /// Writes 9-channel audio data from typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="ch1">A read-only span containing samples for the first channel.</param>
    /// <param name="ch2">A read-only span containing samples for the second channel.</param>
    /// <param name="ch3">A read-only span containing samples for the third channel.</param>
    /// <param name="ch4">A read-only span containing samples for the fourth channel.</param>
    /// <param name="ch5">A read-only span containing samples for the fifth channel.</param>
    /// <param name="ch6">A read-only span containing samples for the sixth channel.</param>
    /// <param name="ch7">A read-only span containing samples for the seventh channel.</param>
    /// <param name="ch8">A read-only span containing samples for the eighth channel.</param>
    /// <param name="ch9">A read-only span containing samples for the ninth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly nine channels.  
    /// This method supports only 9-channel output.
    /// </exception>
    public AVResult32 Write<T>(ReadOnlySpan<T> ch1, ReadOnlySpan<T> ch2, ReadOnlySpan<T> ch3, ReadOnlySpan<T> ch4,
                               ReadOnlySpan<T> ch5, ReadOnlySpan<T> ch6, ReadOnlySpan<T> ch7,
                               ReadOnlySpan<T> ch8, ReadOnlySpan<T> ch9) where T : unmanaged
        => Write(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                 MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                 MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8), MemoryMarshal.AsBytes(ch9));

    #endregion

    #region Write([])
    /// <summary>
    /// Writes audio data from a single byte array.
    /// </summary>
    /// <param name="data">
    /// A byte array containing interleaved audio samples to write.  
    /// The data layout depends on the audio format (<see cref="Format"/>) and the number of <see cref="Channels"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of bytes successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method simply reinterprets <paramref name="data"/> as a <see cref="ReadOnlySpan{Byte}"/> and calls 
    /// <see cref="Write(ReadOnlySpan{byte})"/>.
    /// </remarks>
    public AVResult32 Write(byte[] data) => Write(data.AsSpan());


    /// <summary>
    /// Writes multi-channel audio data from an array of channel buffers.
    /// </summary>
    /// <param name="data">
    /// An array of <see cref="byte"/> arrays, where each element represents one channel’s audio samples.  
    /// The number of elements in <paramref name="data"/> must match the number of channels in the current audio stream.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="data"/> contains a <see langword="null"/> channel buffer.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the number of provided channel buffers does not match the expected number of channels.  
    /// This method supports only configurations where <c>data.Length == Channels</c>.
    /// </exception>
    public AVResult32 Write(params byte[][] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Write(params byte[][]) only supports audio data with exactly {Channels} channels.");

        int samples = data.Min(static d => d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = handles[i].AddrOfPinnedObject().ToPointer();
            }

            return Format.IsPlanar()
                ? WritePlanarToPlanar((byte**)ptrs, samples)
                : WritePlanarToPacked((byte**)ptrs, samples);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }


    /// <summary>
    /// Writes typed audio samples from a single buffer.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="data">
    /// A buffer containing interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each frame.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of frames successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method simply reinterprets <paramref name="data"/> as a <see cref="ReadOnlySpan{Byte}"/> and calls 
    /// <see cref="Write(ReadOnlySpan{byte})"/>.
    /// </remarks>
    public AVResult32 Write<T>(T[] data) where T : unmanaged => Write<T>(data.AsSpan());


    /// <summary>
    /// Writes multi-channel audio data from an array of typed sample buffers.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="data">
    /// An array of <typeparamref name="T"/> arrays, where each element represents one channel’s samples.  
    /// The number of elements in <paramref name="data"/> must match the number of channels in the current audio stream.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="data"/> contains a <see langword="null"/> channel buffer.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the number of provided channel buffers does not match the expected number of channels.  
    /// This method supports only configurations where <c>data.Length == Channels</c>.
    /// </exception>
    /// <remarks>
    /// This method simply reinterprets each <typeparamref name="T"/> array as bytes and calls 
    /// <see cref="Write(params byte[][])"/>.
    /// </remarks>
    public AVResult32 Write<T>(params T[][] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Write(T[][]) only supports audio data with exactly {Channels} channels.");

        int bytesPerChannel = data.Min(static d => (d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) * sizeof(T));
        int samples = bytesPerChannel / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                if (data[i] is null)
                    throw new ArgumentException("One or more channel buffers are null.", nameof(data));

                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = (void*)handles[i].AddrOfPinnedObject();
            }

            return Format.IsPlanar()
                ? WritePlanarToPlanar((byte**)ptrs, samples)
                : WritePlanarToPacked((byte**)ptrs, samples);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }


    /// <summary>
    /// Writes multi-channel audio data from a two-dimensional byte array.
    /// </summary>
    /// <param name="data">
    /// A two-dimensional <see cref="byte"/> array where:
    /// <list type="bullet">
    /// <item><description>The first dimension represents the channel index (0-based).</description></item>
    /// <item><description>The second dimension represents the byte index within each channel’s buffer.</description></item>
    /// </list>
    /// The number of channels (<c>data.GetLength(0)</c>) must match the current audio stream’s <see cref="Channels"/> property.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the number of provided channels (<c>data.GetLength(0)</c>) does not match the expected channel count.  
    /// This method supports only configurations where <c>data.GetLength(0) == Channels</c>.
    /// </exception>
    public unsafe AVResult32 Write(byte[,] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int byteCountPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Write(byte[,]) only supports audio data with exactly {Channels} channels.");

        int samples = byteCountPerChannel / Format.GetBytesPerSample();

        byte** ptrs = stackalloc byte*[channelCount];

        fixed (byte* basePtr = data)
        {
            int rowStride = byteCountPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            return Format.IsPlanar()
                ? WritePlanarToPlanar(ptrs, samples)
                : WritePlanarToPacked(ptrs, samples);
        }
    }


    /// <summary>
    /// Writes multi-channel audio data from a two-dimensional typed array.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).
    /// </typeparam>
    /// <param name="data">
    /// A two-dimensional array of <typeparamref name="T"/> where:
    /// <list type="bullet">
    /// <item><description>The first dimension represents the channel index (0-based).</description></item>
    /// <item><description>The second dimension represents the sample index within each channel.</description></item>
    /// </list>
    /// The number of channels (<c>data.GetLength(0)</c>) must match the current audio stream’s <see cref="Channels"/> property.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully written (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the number of provided channels (<c>data.GetLength(0)</c>) does not match the expected channel count.  
    /// Or if the element size <c>sizeof(T)</c> does not match <see cref="Format.GetBytesPerSample()"/>.
    /// </exception>
    public unsafe AVResult32 Write<T>(T[,] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int samplesPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Write(T[,]) only supports audio data with exactly {Channels} channels.");

        if (sizeof(T) != Format.GetBytesPerSample())
            throw new NotSupportedException(
                $"Element size sizeof({typeof(T).Name}) = {sizeof(T)} does not match format sample size {Format.GetBytesPerSample()}.");

        T** ptrs = stackalloc T*[channelCount];

        fixed (T* basePtr = data)
        {
            int rowStride = samplesPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            // Cast to byte** for planar/packed methods
            return Format.IsPlanar()
                ? WritePlanarToPlanar((byte**)ptrs, samplesPerChannel)
                : WritePlanarToPacked((byte**)ptrs, samplesPerChannel);
        }
    }

    #endregion

    #endregion

    #region Read

    #region Read Helper Functions

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
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
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
                    ptr[i] = bufferPtrs + i * samplesPerChannel * sampleSize;

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
                            int dataIndex = (samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize;
                            for (int b = 0; b < sampleSize; b++)
                                data[dataIndex + b] = ptr[channel][sampleIndex * sampleSize + b];
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
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
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
                                data[channel][sampleIndex * sampleSize + b] =
                                    bufferPtr[(samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize + b];
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
    /// Reads audio data from the <see cref="AudioFifo"/> buffer into an <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to receive audio samples.  
    /// If the frame has no buffer, its channel layout, sample format, and sample count can be automatically initialized:
    /// <list type="bullet">
    /// <item><description>If <see cref="AVFrame.ChannelLayout.Channels"/> is 0, it is set to the default layout for the FIFO’s <see cref="Channels"/>.</description></item>
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
        if (frame.SampleFormat == Format)
            return ffmpeg.av_audio_fifo_read(fifo, (void**)frame.ExtendedData, frame.SampleCount);

        if (Format.IsPlanar()) // FIFO stores planar, frame expects packed
            return ReadPlanarToPacked(frame.ExtendedData[0], frame.SampleCount);
        else // FIFO stores packed, frame expects planar
            return ReadPackedToPlanar(frame.ExtendedData, frame.SampleCount);
    }

    #region Read Span

    /// <summary>
    /// Reads packed multi-channel audio data from the <see cref="AudioFifo"/> into a single contiguous buffer.
    /// </summary>
    /// <param name="buffer">
    /// A writable span to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each audio sample.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The number of channels is determined from the current audio stream's <see cref="Channels"/> property.  
    /// The total number of samples is calculated as <c>buffer.Length / Format.GetBytesPerSample()</c>.
    /// Planar ↔ packed conversions are handled automatically:
    /// <list type="bullet">
    /// <item>If the FIFO stores packed data, samples are read directly using <see cref="ReadPackedToPacked(byte*, int)"/>.</item>
    /// <item>If the FIFO stores planar data, samples are deinterleaved from planar into the packed buffer using <see cref="ReadPlanarToPacked(byte*, int)"/>.</item>
    /// </list>
    /// </remarks>
    public AVResult32 Read(Span<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            int samples = buffer.Length / Format.GetBytesPerSample();

            return Format.IsPlanar()
                ? ReadPlanarToPacked(bufferPtr, samples)
                : ReadPackedToPacked(bufferPtr, samples);
        }
    }

    /// <summary>
    /// Reads packed multi-channel audio data from the <see cref="AudioFifo"/> into a span of typed samples.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// Its size must match <see cref="Format.GetBytesPerSample()"/>.
    /// </typeparam>
    /// <param name="buffer">
    /// A writable span to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each audio sample.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The number of channels is determined from the current audio stream's <see cref="Channels"/> property.  
    /// This method simply reinterprets the <typeparamref name="T"/> span as bytes and calls <see cref="Read(Span{byte})"/>.
    /// </remarks>
    public AVResult32 Read<T>(Span<T> buffer) where T : unmanaged
    {
        return Read(MemoryMarshal.AsBytes(buffer));
    }

    /// <summary>
    /// Reads stereo (two-channel) audio data into separate left and right channel buffers.
    /// </summary>
    /// <param name="left">A writable span to receive audio samples for the left channel.</param>
    /// <param name="right">A writable span to receive audio samples for the right channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly two channels.
    /// </exception>
    public AVResult32 Read(Span<byte> left, Span<byte> right)
    {
        if (Channels != 2)
            throw new NotSupportedException("Read(Span<byte>, Span<byte>) only supports stereo (2-channel) audio output.");

        byte** ptrs = stackalloc byte*[2];
        int samples = Math.Min(left.Length, right.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(left));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(right));

        if (Format.IsPlanar())
            return ReadPlanarToPlanar(ptrs, samples);
        else
            return ReadPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Reads stereo (two-channel) audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="left">A writable span for left-channel samples.</param>
    /// <param name="right">A writable span for right-channel samples.</param>
    /// <returns>The number of samples read or an error code.</returns>
    /// <exception cref="NotSupportedException">Thrown when the current audio stream does not have exactly two channels.</exception>
    public AVResult32 Read<T>(Span<T> left, Span<T> right) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(left), MemoryMarshal.AsBytes(right));

    /// <summary>
    /// Reads 3-channel audio data into separate channel buffers.
    /// </summary>
    public AVResult32 Read(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3)
    {
        if (Channels != 3)
            throw new NotSupportedException("Read(3-channel) only supports 3-channel audio output.");

        byte** ptrs = stackalloc byte*[3];
        int samples = Math.Min(ch1.Length, Math.Min(ch2.Length, ch3.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));

        return Format.IsPlanar()
            ? ReadPlanarToPlanar(ptrs, samples)
            : ReadPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Reads 3-channel audio data into typed sample buffers.
    /// </summary>
    public AVResult32 Read<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3));

    /// <summary>
    /// Reads 4-channel audio data into separate channel buffers.
    /// </summary>
    public AVResult32 Read(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4)
    {
        if (Channels != 4)
            throw new NotSupportedException("Read(4-channel) only supports 4-channel audio output.");

        byte** ptrs = stackalloc byte*[4];
        int samples = Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));

        return Format.IsPlanar()
            ? ReadPlanarToPlanar(ptrs, samples)
            : ReadPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Reads 4-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly four channels.</exception>
    public AVResult32 Read<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3), MemoryMarshal.AsBytes(ch4));

    /// <summary>
    /// Reads 5-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly five channels.</exception>
    public AVResult32 Read(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4, Span<byte> ch5)
    {
        if (Channels != 5)
            throw new NotSupportedException("Read(5-channel) only supports 5-channel audio output.");

        byte** ptrs = stackalloc byte*[5];
        int samples = Math.Min(Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)), ch5.Length)
                               / Format.GetBytesPerSample(), int.MaxValue);

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));

        return Format.IsPlanar()
            ? ReadPlanarToPlanar(ptrs, samples)
            : ReadPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Reads 5-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly five channels.</exception>
    public AVResult32 Read<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4, Span<T> ch5) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5));

    /// <summary>
    /// Reads 6-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly six channels.</exception>
    public AVResult32 Read(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4, Span<byte> ch5, Span<byte> ch6)
    {
        if (Channels != 6)
            throw new NotSupportedException("Read(6-channel) only supports 6-channel audio output.");

        byte** ptrs = stackalloc byte*[6];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(ch5.Length, ch6.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));

        return Format.IsPlanar()
            ? ReadPlanarToPlanar(ptrs, samples)
            : ReadPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Reads 6-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly six channels.</exception>
    public AVResult32 Read<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4, Span<T> ch5, Span<T> ch6) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6));

    /// <summary>
    /// Reads 7-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly seven channels.</exception>
    public AVResult32 Read(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7)
    {
        if (Channels != 7)
            throw new NotSupportedException("Read(7-channel) only supports 7-channel audio output.");

        byte** ptrs = stackalloc byte*[7];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), ch7.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));

        return Format.IsPlanar()
            ? ReadPlanarToPlanar(ptrs, samples)
            : ReadPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Reads 7-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly seven channels.</exception>
    public AVResult32 Read<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                MemoryMarshal.AsBytes(ch7));

    /// <summary>
    /// Reads 8-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly eight channels.</exception>
    public AVResult32 Read(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7, Span<byte> ch8)
    {
        if (Channels != 8)
            throw new NotSupportedException("Read(8-channel) only supports 8-channel audio output.");

        byte** ptrs = stackalloc byte*[8];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), Math.Min(ch7.Length, ch8.Length)))
                               / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));

        return Format.IsPlanar()
            ? ReadPlanarToPlanar(ptrs, samples)
            : ReadPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Reads 8-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly eight channels.</exception>
    public AVResult32 Read<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7, Span<T> ch8) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8));

    /// <summary>
    /// Reads 9-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <param name="ch9">A span to receive samples for the ninth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly nine channels.</exception>
    public AVResult32 Read(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7, Span<byte> ch8, Span<byte> ch9)
    {
        if (Channels != 9)
            throw new NotSupportedException("Read(9-channel) only supports 9-channel audio output.");

        byte** ptrs = stackalloc byte*[9];
        int samples = Math.Min(Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length),
                                                 Math.Min(ch3.Length, ch4.Length)),
                                         Math.Min(Math.Min(ch5.Length, ch6.Length),
                                                 Math.Min(ch7.Length, ch8.Length))),
                               ch9.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));
        ptrs[8] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch9));

        return Format.IsPlanar()
            ? ReadPlanarToPlanar(ptrs, samples)
            : ReadPackedToPlanar(ptrs, samples);
    }


    /// <summary>
    /// Reads 9-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <param name="ch9">A span to receive samples for the ninth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly nine channels.</exception>
    public AVResult32 Read<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7, Span<T> ch8, Span<T> ch9) where T : unmanaged
        => Read(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8), MemoryMarshal.AsBytes(ch9));

    #endregion

    #region ReadArrays
    #region Read([])

    /// <summary>
    /// Reads audio data into a single byte array.
    /// </summary>
    /// <param name="data">
    /// A byte array to receive interleaved audio samples.  
    /// The data layout depends on the audio <see cref="Format"/> and the number of <see cref="Channels"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of bytes successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method simply wraps <see cref="Read(Span{byte})"/> for convenience.  
    /// It reads interleaved (packed) audio samples directly into <paramref name="data"/>.
    /// </remarks>
    public AVResult32 Read(byte[] data) => Read(data.AsSpan());


    /// <summary>
    /// Reads typed audio samples into a single buffer.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// The size of <typeparamref name="T"/> must match the sample size of the current audio <see cref="Format"/>.
    /// </typeparam>
    /// <param name="data">
    /// A buffer to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each frame.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of frames successfully read (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method reinterprets <paramref name="data"/> as a <see cref="Span{Byte}"/> and calls 
    /// <see cref="Read(Span{byte})"/> internally.
    /// </remarks>
    public AVResult32 Read<T>(T[] data) where T : unmanaged => Read(data.AsSpan());


    /// <summary>
    /// Reads multi-channel audio data into an array of channel buffers.
    /// </summary>
    /// <param name="data">
    /// An array of <see cref="byte"/> arrays, where each element represents one channel’s audio samples.  
    /// The number of elements must match <see cref="Channels"/>.
    /// </param>
    /// <returns>The number of samples read.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when any element of <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="data"/> length does not match <see cref="Channels"/>.</exception>
    public AVResult32 Read(params byte[][] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Read(params byte[][]) only supports audio data with exactly {Channels} channels.");

        int samples = data.Min(static d => d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = handles[i].AddrOfPinnedObject().ToPointer();
            }

            return Format.IsPlanar()
                ? ReadPlanarToPlanar((byte**)ptrs, samples)
                : ReadPackedToPlanar((byte**)ptrs, samples);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }

    /// <summary>
    /// Reads multi-channel audio data into an array of typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (e.g., <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="data">An array of <typeparamref name="T"/> arrays, one per channel.</param>
    /// <returns>The number of samples read.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when any element of <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="data"/> length does not match <see cref="Channels"/>.</exception>
    public AVResult32 Read<T>(params T[][] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Read(T[][]) only supports audio data with exactly {Channels} channels.");

        int bytesPerChannel = data.Min(static d => (d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) * sizeof(T));
        int samples = bytesPerChannel / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = (void*)handles[i].AddrOfPinnedObject();
            }

            return Format.IsPlanar()
                ? ReadPlanarToPlanar((byte**)ptrs, samples)
                : ReadPackedToPlanar((byte**)ptrs, samples);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }

    /// <summary>
    /// Reads multi-channel audio data into a two-dimensional byte array.
    /// </summary>
    /// <param name="data">
    /// A two-dimensional <see cref="byte"/> array where the first dimension is the channel index and the second is the byte index.
    /// </param>
    /// <returns>The number of samples read.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when the number of channels does not match <see cref="Channels"/>.</exception>
    public unsafe AVResult32 Read(byte[,] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int byteCountPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Read(byte[,]) only supports audio data with exactly {Channels} channels.");

        int samples = byteCountPerChannel / Format.GetBytesPerSample();

        byte** ptrs = stackalloc byte*[channelCount];

        fixed (byte* basePtr = data)
        {
            int rowStride = byteCountPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            return Format.IsPlanar()
                ? ReadPlanarToPlanar(ptrs, samples)
                : ReadPackedToPlanar(ptrs, samples);
        }
    }

    /// <summary>
    /// Reads multi-channel audio data into a two-dimensional typed array.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (e.g., <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="data">
    /// A two-dimensional <typeparamref name="T"/> array where the first dimension is the channel index and the second is the sample index.
    /// </param>
    /// <returns>The number of samples read.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the number of channels does not match <see cref="Channels"/> or
    /// when <c>sizeof(T)</c> does not match <see cref="Format.GetBytesPerSample()"/>.
    /// </exception>
    public unsafe AVResult32 Read<T>(T[,] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int samplesPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Read(T[,]) only supports audio data with exactly {Channels} channels.");

        if (sizeof(T) != Format.GetBytesPerSample())
            throw new NotSupportedException(
                $"Element size sizeof({typeof(T).Name}) = {sizeof(T)} does not match format sample size {Format.GetBytesPerSample()}.");

        T** ptrs = stackalloc T*[channelCount];

        fixed (T* basePtr = data)
        {
            int rowStride = samplesPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            return Format.IsPlanar()
                ? ReadPlanarToPlanar((byte**)ptrs, samplesPerChannel)
                : ReadPackedToPlanar((byte**)ptrs, samplesPerChannel);
        }
    }

    #endregion

    #endregion
    #endregion

    #region Peek

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
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
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
                    ptr[i] = bufferPtrs + i * samplesPerChannel * sampleSize;

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
                            int dataIndex = (samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize;
                            for (int b = 0; b < sampleSize; b++)
                                data[dataIndex + b] = ptr[channel][sampleIndex * sampleSize + b];
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
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
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
                                data[channel][sampleIndex * sampleSize + b] =
                                    bufferPtr[(samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize + b];
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
    /// Peeks audio data from the <see cref="AudioFifo"/> buffer into an <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to receive audio samples.  
    /// If the frame has no buffer, its channel layout, sample format, and sample count can be automatically initialized:
    /// <list type="bullet">
    /// <item><description>If <see cref="AVFrame.ChannelLayout.Channels"/> is 0, it is set to the default layout for the FIFO’s <see cref="Channels"/>.</description></item>
    /// <item><description>If <see cref="AVFrame.SampleFormat"/> is <see cref="SampleFormat.None"/>, it is set to the FIFO’s <see cref="Format"/>.</description></item>
    /// <item><description>If <see cref="AVFrame.SampleCount"/> is less than 1, it is set to the current <see cref="AudioFifo.Count"/> (peek all available samples).</description></item>
    /// </list>
    /// If the frame alpeeky has a buffer and any of these properties are unset, an exception is thrown.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek from the FIFO (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the frame alpeeky has a buffer allocated and some properties would need to be set,  
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
    /// <item>If the frame's format exactly matches the FIFO format, the data is peek directly using <c>ffmpeg.av_audio_fifo_peek</c>.</item>
    /// <item>If the FIFO stores planar but the frame is packed, the data is converted from planar to packed using <see cref="PeekPlanarToPacked"/>.</item>
    /// <item>If the FIFO stores packed but the frame is planar, the data is converted from packed to planar using <see cref="PeekPackedToPlanar"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// To avoid unnecessary copying, it is recommended to provide frames in the same planar/packed layout as the FIFO’s <see cref="Format"/>.
    /// </para>
    /// </remarks>
    public AVResult32 Peek(AVFrame frame)
    {
        if (frame == null)
            throw new ArgumentNullException(nameof(frame));

        bool needsInitialization = frame.ChannelLayout.Channels == 0 || frame.SampleFormat == SampleFormat.None || frame.SampleCount <= 0;

        if (frame.HasBuffer && needsInitialization)
            throw new ArgumentException("Cannot set properties on a frame that alpeeky has a buffer.", nameof(frame));

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

        // Create the buffer only if the frame did not alpeeky have one
        if (!frame.HasBuffer)
            frame.CreateBuffer().ThrowIfError();

        // Peek from FIFO
        if (frame.SampleFormat == Format)
            return ffmpeg.av_audio_fifo_peek(fifo, (void**)frame.ExtendedData, frame.SampleCount);

        if (Format.IsPlanar()) // FIFO stores planar, frame expects packed
            return PeekPlanarToPacked(frame.ExtendedData[0], frame.SampleCount);
        else // FIFO stores packed, frame expects planar
            return PeekPackedToPlanar(frame.ExtendedData, frame.SampleCount);
    }

    #region Peek Span

    /// <summary>
    /// Peeks packed multi-channel audio data from the <see cref="AudioFifo"/> into a single contiguous buffer.
    /// </summary>
    /// <param name="buffer">
    /// A writable span to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each audio sample.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The number of channels is determined from the current audio stream's <see cref="Channels"/> property.  
    /// The total number of samples is calculated as <c>buffer.Length / Format.GetBytesPerSample()</c>.
    /// Planar ↔ packed conversions are handled automatically:
    /// <list type="bullet">
    /// <item>If the FIFO stores packed data, samples are peek directly using <see cref="PeekPackedToPacked(byte*, int)"/>.</item>
    /// <item>If the FIFO stores planar data, samples are deinterleaved from planar into the packed buffer using <see cref="PeekPlanarToPacked(byte*, int)"/>.</item>
    /// </list>
    /// </remarks>
    public AVResult32 Peek(Span<byte> buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            int samples = buffer.Length / Format.GetBytesPerSample();

            return Format.IsPlanar()
                ? PeekPlanarToPacked(bufferPtr, samples)
                : PeekPackedToPacked(bufferPtr, samples);
        }
    }

    /// <summary>
    /// Peeks packed multi-channel audio data from the <see cref="AudioFifo"/> into a span of typed samples.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// Its size must match <see cref="Format.GetBytesPerSample()"/>.
    /// </typeparam>
    /// <param name="buffer">
    /// A writable span to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each audio sample.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The number of channels is determined from the current audio stream's <see cref="Channels"/> property.  
    /// This method simply reinterprets the <typeparamref name="T"/> span as bytes and calls <see cref="Peek(Span{byte})"/>.
    /// </remarks>
    public AVResult32 Peek<T>(Span<T> buffer) where T : unmanaged
    {
        return Peek(MemoryMarshal.AsBytes(buffer));
    }

    /// <summary>
    /// Peeks stereo (two-channel) audio data into separate left and right channel buffers.
    /// </summary>
    /// <param name="left">A writable span to receive audio samples for the left channel.</param>
    /// <param name="right">A writable span to receive audio samples for the right channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly two channels.
    /// </exception>
    public AVResult32 Peek(Span<byte> left, Span<byte> right)
    {
        if (Channels != 2)
            throw new NotSupportedException("Peek(Span<byte>, Span<byte>) only supports stereo (2-channel) audio output.");

        byte** ptrs = stackalloc byte*[2];
        int samples = Math.Min(left.Length, right.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(left));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(right));

        if (Format.IsPlanar())
            return PeekPlanarToPlanar(ptrs, samples);
        else
            return PeekPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Peeks stereo (two-channel) audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="left">A writable span for left-channel samples.</param>
    /// <param name="right">A writable span for right-channel samples.</param>
    /// <returns>The number of samples peek or an error code.</returns>
    /// <exception cref="NotSupportedException">Thrown when the current audio stream does not have exactly two channels.</exception>
    public AVResult32 Peek<T>(Span<T> left, Span<T> right) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(left), MemoryMarshal.AsBytes(right));

    /// <summary>
    /// Peeks 3-channel audio data into separate channel buffers.
    /// </summary>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3)
    {
        if (Channels != 3)
            throw new NotSupportedException("Peek(3-channel) only supports 3-channel audio output.");

        byte** ptrs = stackalloc byte*[3];
        int samples = Math.Min(ch1.Length, Math.Min(ch2.Length, ch3.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples)
            : PeekPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Peeks 3-channel audio data into typed sample buffers.
    /// </summary>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3));

    /// <summary>
    /// Peeks 4-channel audio data into separate channel buffers.
    /// </summary>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4)
    {
        if (Channels != 4)
            throw new NotSupportedException("Peek(4-channel) only supports 4-channel audio output.");

        byte** ptrs = stackalloc byte*[4];
        int samples = Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples)
            : PeekPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Peeks 4-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly four channels.</exception>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3), MemoryMarshal.AsBytes(ch4));

    /// <summary>
    /// Peeks 5-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly five channels.</exception>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4, Span<byte> ch5)
    {
        if (Channels != 5)
            throw new NotSupportedException("Peek(5-channel) only supports 5-channel audio output.");

        byte** ptrs = stackalloc byte*[5];
        int samples = Math.Min(Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)), ch5.Length)
                               / Format.GetBytesPerSample(), int.MaxValue);

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples)
            : PeekPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Peeks 5-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly five channels.</exception>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4, Span<T> ch5) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5));

    /// <summary>
    /// Peeks 6-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly six channels.</exception>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4, Span<byte> ch5, Span<byte> ch6)
    {
        if (Channels != 6)
            throw new NotSupportedException("Peek(6-channel) only supports 6-channel audio output.");

        byte** ptrs = stackalloc byte*[6];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(ch5.Length, ch6.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples)
            : PeekPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Peeks 6-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly six channels.</exception>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4, Span<T> ch5, Span<T> ch6) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6));

    /// <summary>
    /// Peeks 7-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly seven channels.</exception>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7)
    {
        if (Channels != 7)
            throw new NotSupportedException("Peek(7-channel) only supports 7-channel audio output.");

        byte** ptrs = stackalloc byte*[7];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), ch7.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples)
            : PeekPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Peeks 7-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly seven channels.</exception>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                MemoryMarshal.AsBytes(ch7));

    /// <summary>
    /// Peeks 8-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly eight channels.</exception>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7, Span<byte> ch8)
    {
        if (Channels != 8)
            throw new NotSupportedException("Peek(8-channel) only supports 8-channel audio output.");

        byte** ptrs = stackalloc byte*[8];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), Math.Min(ch7.Length, ch8.Length)))
                               / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples)
            : PeekPackedToPlanar(ptrs, samples);
    }

    /// <summary>
    /// Peeks 8-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly eight channels.</exception>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7, Span<T> ch8) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8));

    /// <summary>
    /// Peeks 9-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <param name="ch9">A span to receive samples for the ninth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly nine channels.</exception>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7, Span<byte> ch8, Span<byte> ch9)
    {
        if (Channels != 9)
            throw new NotSupportedException("Peek(9-channel) only supports 9-channel audio output.");

        byte** ptrs = stackalloc byte*[9];
        int samples = Math.Min(Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length),
                                                 Math.Min(ch3.Length, ch4.Length)),
                                         Math.Min(Math.Min(ch5.Length, ch6.Length),
                                                 Math.Min(ch7.Length, ch8.Length))),
                               ch9.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));
        ptrs[8] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch9));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples)
            : PeekPackedToPlanar(ptrs, samples);
    }


    /// <summary>
    /// Peeks 9-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A span to receive samples for the first channel.</param>
    /// <param name="ch2">A span to receive samples for the second channel.</param>
    /// <param name="ch3">A span to receive samples for the third channel.</param>
    /// <param name="ch4">A span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A span to receive samples for the eighth channel.</param>
    /// <param name="ch9">A span to receive samples for the ninth channel.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peek (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the audio stream does not have exactly nine channels.</exception>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7, Span<T> ch8, Span<T> ch9) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8), MemoryMarshal.AsBytes(ch9));

    #endregion

    #region PeekArrays
    #region Peek([])
    /// <summary>
    /// Peeks audio data into a single byte array without advancing the read position.
    /// </summary>
    /// <param name="data">
    /// A byte array to receive interleaved audio samples.  
    /// The data layout depends on the audio <see cref="Format"/> and the number of <see cref="Channels"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of bytes successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method allows inspecting audio data without consuming it.  
    /// It simply wraps <see cref="Peek(Span{byte})"/> for convenience and reads interleaved (packed) samples directly into <paramref name="data"/>.
    /// </remarks>
    public AVResult32 Peek(byte[] data) => Peek(data.AsSpan());


    /// <summary>
    /// Peeks typed audio samples into a single buffer without advancing the read position.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// The size of <typeparamref name="T"/> must match the sample size of the current audio <see cref="Format"/>.
    /// </typeparam>
    /// <param name="data">
    /// A buffer to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each frame.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of frames successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method reinterprets <paramref name="data"/> as a <see cref="Span{Byte}"/> and calls 
    /// <see cref="Peek(Span{byte})"/> internally.  
    /// Like all Peek methods, it does not advance the internal read position.
    /// </remarks>
    public AVResult32 Peek<T>(T[] data) where T : unmanaged => Peek(data.AsSpan());


    /// <summary>
    /// Peeks multi-channel audio data into an array of channel buffers.
    /// </summary>
    /// <param name="data">
    /// An array of <see cref="byte"/> arrays, where each element represents one channel’s audio samples.  
    /// The number of elements must match <see cref="Channels"/>.
    /// </param>
    /// <returns>The number of samples peek.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when any element of <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="data"/> length does not match <see cref="Channels"/>.</exception>
    public AVResult32 Peek(params byte[][] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Peek(params byte[][]) only supports audio data with exactly {Channels} channels.");

        int samples = data.Min(static d => d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = handles[i].AddrOfPinnedObject().ToPointer();
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar((byte**)ptrs, samples)
                : PeekPackedToPlanar((byte**)ptrs, samples);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }

    /// <summary>
    /// Peeks multi-channel audio data into an array of typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (e.g., <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="data">An array of <typeparamref name="T"/> arrays, one per channel.</param>
    /// <returns>The number of samples peek.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when any element of <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="data"/> length does not match <see cref="Channels"/>.</exception>
    public AVResult32 Peek<T>(params T[][] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Peek(T[][]) only supports audio data with exactly {Channels} channels.");

        int bytesPerChannel = data.Min(static d => (d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) * sizeof(T));
        int samples = bytesPerChannel / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = (void*)handles[i].AddrOfPinnedObject();
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar((byte**)ptrs, samples)
                : PeekPackedToPlanar((byte**)ptrs, samples);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }

    /// <summary>
    /// Peeks multi-channel audio data into a two-dimensional byte array.
    /// </summary>
    /// <param name="data">
    /// A two-dimensional <see cref="byte"/> array where the first dimension is the channel index and the second is the byte index.
    /// </param>
    /// <returns>The number of samples peek.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when the number of channels does not match <see cref="Channels"/>.</exception>
    public unsafe AVResult32 Peek(byte[,] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int byteCountPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Peek(byte[,]) only supports audio data with exactly {Channels} channels.");

        int samples = byteCountPerChannel / Format.GetBytesPerSample();

        byte** ptrs = stackalloc byte*[channelCount];

        fixed (byte* basePtr = data)
        {
            int rowStride = byteCountPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar(ptrs, samples)
                : PeekPackedToPlanar(ptrs, samples);
        }
    }

    /// <summary>
    /// Peeks multi-channel audio data into a two-dimensional typed array.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (e.g., <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="data">
    /// A two-dimensional <typeparamref name="T"/> array where the first dimension is the channel index and the second is the sample index.
    /// </param>
    /// <returns>The number of samples peek.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the number of channels does not match <see cref="Channels"/> or
    /// when <c>sizeof(T)</c> does not match <see cref="Format.GetBytesPerSample()"/>.
    /// </exception>
    public unsafe AVResult32 Peek<T>(T[,] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int samplesPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Peek(T[,]) only supports audio data with exactly {Channels} channels.");

        if (sizeof(T) != Format.GetBytesPerSample())
            throw new NotSupportedException(
                $"Element size sizeof({typeof(T).Name}) = {sizeof(T)} does not match format sample size {Format.GetBytesPerSample()}.");

        T** ptrs = stackalloc T*[channelCount];

        fixed (T* basePtr = data)
        {
            int rowStride = samplesPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar((byte**)ptrs, samplesPerChannel)
                : PeekPackedToPlanar((byte**)ptrs, samplesPerChannel);
        }
    }

    #endregion

    #endregion
    #endregion

    #region PeekAt

    #region Peek Helper Functions

    private AVResult32 PeekPackedToPacked(byte* data, int samples, int offset) =>
        ffmpeg.av_audio_fifo_peek_at(fifo, (void**)&data, samples, offset);

    private AVResult32 PeekPlanarToPlanar(byte** data, int samples, int offset) =>
        ffmpeg.av_audio_fifo_peek_at(fifo, (void**)data, samples, offset);

    private AVResult32 PeekPlanarToPacked(byte* data, int samples, int offset)
    {
        // Rent a temporary buffer from ArrayPool to hold planar data from the FIFO
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
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
                    ptr[i] = bufferPtrs + i * samplesPerChannel * sampleSize;

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
                            int dataIndex = (samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize;
                            for (int b = 0; b < sampleSize; b++)
                                data[dataIndex + b] = ptr[channel][sampleIndex * sampleSize + b];
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
    private AVResult32 PeekPackedToPlanar(byte** data, int samples, int offset)
    {
        // Rent a temporary buffer from ArrayPool to hold packed data from the FIFO
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
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
                                data[channel][sampleIndex * sampleSize + b] =
                                    bufferPtr[(samplesCopied + sampleIndex) * Channels * sampleSize + channel * sampleSize + b];
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
    /// Peeks audio data from the <see cref="AudioFifo"/> buffer into an <see cref="AVFrame"/>,
    /// starting at the specified sample <paramref name="offset"/> within the FIFO.
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to receive audio samples.  
    /// If the frame has no buffer, its channel layout, sample format, and sample count can be automatically initialized:
    /// <list type="bullet">
    /// <item><description>If <see cref="AVFrame.ChannelLayout.Channels"/> is 0, it is set to the default layout for the FIFO’s <see cref="Channels"/>.</description></item>
    /// <item><description>If <see cref="AVFrame.SampleFormat"/> is <see cref="SampleFormat.None"/>, it is set to the FIFO’s <see cref="Format"/>.</description></item>
    /// <item><description>If <see cref="AVFrame.SampleCount"/> is less than 1, it is set to the current <see cref="AudioFifo.Count"/> (peek all available samples from the offset).</description></item>
    /// </list>
    /// If the frame already has a buffer and any of these properties are unset, an exception is thrown.
    /// </param>
    /// <param name="offset">
    /// The sample index within the FIFO at which to begin peeking.  
    /// Must be non-negative and less than <see cref="AudioFifo.Count"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked from the FIFO (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="frame"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the frame already has a buffer allocated and some properties would need to be set,  
    /// or if, after initialization, the frame’s channel count or planar/packed layout does not match the FIFO.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="offset"/> is negative or exceeds the number of available samples in the FIFO.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The <see cref="AudioFifo"/> stores all audio data according to the <see cref="Format"/> specified when the FIFO was created.  
    /// This method does not convert between sample types (e.g., float ↔ int16); doing so must be handled by the caller.
    /// </para>
    /// <para>
    /// Planar ↔ packed conversions are handled automatically:
    /// <list type="bullet">
    /// <item>If the frame's format exactly matches the FIFO format, the data is peeked directly using <c>ffmpeg.av_audio_fifo_peek</c>.</item>
    /// <item>If the FIFO stores planar but the frame is packed, the data is converted from planar to packed using <see cref="PeekPlanarToPacked"/>.</item>
    /// <item>If the FIFO stores packed but the frame is planar, the data is converted from packed to planar using <see cref="PeekPackedToPlanar"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// To avoid unnecessary copying, it is recommended to provide frames in the same planar/packed layout as the FIFO’s <see cref="Format"/>.
    /// </para>
    /// </remarks>
    public AVResult32 Peek(AVFrame frame, int offset)
    {
        if (frame == null)
            throw new ArgumentNullException(nameof(frame));
        if (offset < 0 || offset >= Count)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be within the range of available samples in the FIFO.");

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
                frame.SampleCount = Count - offset; // adjust sample count to available samples from offset
        }

        // Validate after initialization
        if (frame.ChannelLayout.Channels != Channels)
            throw new ArgumentException("Frame channel count does not match the audio FIFO.", nameof(frame));

        if (frame.SampleFormat.AsPlanar() != Format.AsPlanar())
            throw new ArgumentException("Frame planar/packed layout does not match the audio FIFO.", nameof(frame));

        // Create the buffer only if the frame did not already have one
        if (!frame.HasBuffer)
            frame.CreateBuffer().ThrowIfError();

        // Peek from FIFO at specified offset
        if (frame.SampleFormat == Format)
            return ffmpeg.av_audio_fifo_peek_at(fifo, (void**)frame.ExtendedData, frame.SampleCount, offset);

        if (Format.IsPlanar()) // FIFO stores planar, frame expects packed
            return PeekPlanarToPacked(frame.ExtendedData[0], frame.SampleCount, offset);
        else // FIFO stores packed, frame expects planar
            return PeekPackedToPlanar(frame.ExtendedData, frame.SampleCount, offset);
    }


    #region Peek Span

    /// <summary>
    /// Peeks packed multi-channel audio data from the <see cref="AudioFifo"/> into a single contiguous buffer.
    /// </summary>
    /// <param name="buffer">
    /// A writable span to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each audio sample.
    /// </param>
    /// <param name="offset">
    /// The sample offset from which to start peeking.  
    /// Each unit corresponds to one multi-channel sample (i.e., one sample per channel).
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The number of channels is determined from the current audio stream's <see cref="Channels"/> property.  
    /// The total number of samples is calculated as <c>buffer.Length / Format.GetBytesPerSample()</c>.  
    /// Planar ↔ packed conversions are handled automatically:
    /// <list type="bullet">
    /// <item>If the FIFO stores packed data, samples are peeked directly using <see cref="PeekPackedToPacked(byte*, int, int)"/>.</item>
    /// <item>If the FIFO stores planar data, samples are deinterleaved from planar into the packed buffer using <see cref="PeekPlanarToPacked(byte*, int, int)"/>.</item>
    /// </list>
    /// </remarks>
    public AVResult32 Peek(Span<byte> buffer, int offset = 0)
    {
        fixed (byte* bufferPtr = buffer)
        {
            int samples = buffer.Length / Format.GetBytesPerSample();
            return Format.IsPlanar()
                ? PeekPlanarToPacked(bufferPtr, samples, offset)
                : PeekPackedToPacked(bufferPtr, samples, offset);
        }
    }

    /// <summary>
    /// Peeks packed multi-channel audio data from the <see cref="AudioFifo"/> into a span of typed samples.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// Its size must match <see cref="Format.GetBytesPerSample()"/>.
    /// </typeparam>
    /// <param name="buffer">
    /// A writable span to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each audio sample.
    /// </param>
    /// <param name="offset">
    /// The sample offset from which to start peeking.  
    /// Each unit corresponds to one multi-channel sample (i.e., one sample per channel).
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> span as bytes and calls <see cref="Peek(Span{byte}, int)"/>.
    ///</remarks>
    public AVResult32 Peek<T>(Span<T> buffer, int offset = 0) where T : unmanaged
    {
        return Peek(MemoryMarshal.AsBytes(buffer), offset);
    }

    /// <summary>
    /// Peeks stereo (two-channel) audio data into separate left and right channel buffers.
    /// </summary>
    /// <param name="left">A writable span to receive audio samples for the left channel.</param>
    /// <param name="right">A writable span to receive audio samples for the right channel.</param>
    /// <param name="offset">
    /// The sample offset from which to start peeking.  
    /// Each unit corresponds to one stereo sample.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the current audio stream does not have exactly two channels.
    /// </exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> left, Span<byte> right, int offset = 0)
    {
        if (Channels != 2)
            throw new NotSupportedException("Peek(Span<byte>, Span<byte>) only supports stereo (2-channel) audio output.");

        byte** ptrs = stackalloc byte*[2];
        int samples = Math.Min(left.Length, right.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(left));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(right));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks stereo (two-channel) audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="left">A writable span to receive left-channel samples.</param>
    /// <param name="right">A writable span to receive right-channel samples.</param>
    /// <param name="offset">
    /// The sample offset from which to start peeking.  
    /// Each unit corresponds to one stereo sample.
    /// </param>
    /// <returns>An <see cref="AVResult32"/> value representing either the number of samples peeked or an error code.</returns>
    /// <exception cref="NotSupportedException">Thrown when the current audio stream does not have exactly two channels.</exception>
    public AVResult32 Peek<T>(Span<T> left, Span<T> right, int offset = 0) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(left), MemoryMarshal.AsBytes(right), offset);

    /// <summary>
    /// Peeks 3-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A writable span for channel 1 samples.</param>
    /// <param name="ch2">A writable span for channel 2 samples.</param>
    /// <param name="ch3">A writable span for channel 3 samples.</param>
    /// <param name="offset">
    /// The sample offset from which to start peeking.  
    /// Each unit corresponds to one 3-channel sample.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the current audio stream does not have exactly three channels.</exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, int offset = 0)
    {
        if (Channels != 3)
            throw new NotSupportedException("Peek(3-channel) only supports 3-channel audio output.");

        byte** ptrs = stackalloc byte*[3];
        int samples = Math.Min(ch1.Length, Math.Min(ch2.Length, ch3.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks 3-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A writable span for channel 1 samples.</param>
    /// <param name="ch2">A writable span for channel 2 samples.</param>
    /// <param name="ch3">A writable span for channel 3 samples.</param>
    /// <param name="offset">
    /// The sample offset from which to start peeking.  
    /// Each unit corresponds to one 3-channel sample.
    /// </param>
    /// <returns>An <see cref="AVResult32"/> value representing either the number of samples peeked or an error code.</returns>
    /// <exception cref="NotSupportedException">Thrown when the current audio stream does not have exactly three channels.</exception>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, int offset = 0) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3), offset);


    /// <summary>
    /// Peeks 4-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A writable span to receive audio samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive audio samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive audio samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive audio samples for the fourth channel.</param>
    /// <param name="offset">
    /// The sample offset from which to start peeking.  
    /// Each unit corresponds to one 4-channel sample.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly four channels.</exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4, int offset = 0)
    {
        if (Channels != 4)
            throw new NotSupportedException("Peek(4-channel) only supports 4-channel audio output.");

        byte** ptrs = stackalloc byte*[4];
        int samples = Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks 4-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A writable span to receive samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive samples for the fourth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 4-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly four channels.</exception>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> spans as bytes and calls <see cref="Peek(Span{byte}, Span{byte}, Span{byte}, Span{byte}, int)"/>.
    /// </remarks>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4, int offset = 0) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3), MemoryMarshal.AsBytes(ch4), offset);

    /// <summary>
    /// Peeks 5-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A writable span to receive audio samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive audio samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive audio samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive audio samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive audio samples for the fifth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 5-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly five channels.</exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4, Span<byte> ch5, int offset = 0)
    {
        if (Channels != 5)
            throw new NotSupportedException("Peek(5-channel) only supports 5-channel audio output.");

        byte** ptrs = stackalloc byte*[5];
        int samples = Math.Min(Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)), ch5.Length)
                               / Format.GetBytesPerSample(), int.MaxValue);

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks 5-channel audio data into typed sample buffers.
    /// </summary>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4, Span<T> ch5, int offset = 0) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), offset);

    /// <summary>
    /// Peeks 6-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A writable span to receive audio samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive audio samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive audio samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive audio samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive audio samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive audio samples for the sixth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 6-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly six channels.</exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4, Span<byte> ch5, Span<byte> ch6, int offset = 0)
    {
        if (Channels != 6)
            throw new NotSupportedException("Peek(6-channel) only supports 6-channel audio output.");

        byte** ptrs = stackalloc byte*[6];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(ch5.Length, ch6.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks 6-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A writable span to receive samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive samples for the sixth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 6-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly six channels.</exception>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> spans as bytes and calls 
    /// <see cref="Peek(Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, int)"/>.
    /// </remarks>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4, Span<T> ch5, Span<T> ch6, int offset = 0) where T : unmanaged
        => Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6), offset);


    /// <summary>
    /// Peeks 7-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A writable span to receive audio samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive audio samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive audio samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive audio samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive audio samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive audio samples for the sixth channel.</param>
    /// <param name="ch7">A writable span to receive audio samples for the seventh channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 7-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly seven channels.</exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7, int offset = 0)
    {
        if (Channels != 7)
            throw new NotSupportedException("Peek(7-channel) only supports 7-channel audio output.");

        byte** ptrs = stackalloc byte*[7];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), ch7.Length)) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks 7-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A writable span to receive samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A writable span to receive samples for the seventh channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 7-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly seven channels.</exception>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> spans as bytes and calls 
    /// <see cref="Peek(Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, int)"/>.
    /// </remarks>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7, int offset = 0) where T : unmanaged
    {
        return Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                    MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                    MemoryMarshal.AsBytes(ch7), offset);
    }

    /// <summary>
    /// Peeks 8-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A writable span to receive audio samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive audio samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive audio samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive audio samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive audio samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive audio samples for the sixth channel.</param>
    /// <param name="ch7">A writable span to receive audio samples for the seventh channel.</param>
    /// <param name="ch8">A writable span to receive audio samples for the eighth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 8-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly eight channels.</exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7, Span<byte> ch8, int offset = 0)
    {
        if (Channels != 8)
            throw new NotSupportedException("Peek(8-channel) only supports 8-channel audio output.");

        byte** ptrs = stackalloc byte*[8];
        int samples = Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length), Math.Min(ch3.Length, ch4.Length)),
                               Math.Min(Math.Min(ch5.Length, ch6.Length), Math.Min(ch7.Length, ch8.Length)))
                               / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks 8-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A writable span to receive samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A writable span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A writable span to receive samples for the eighth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 8-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly eight channels.</exception>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> spans as bytes and calls 
    /// <see cref="Peek(Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, int)"/>.
    /// </remarks>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7, Span<T> ch8, int offset = 0) where T : unmanaged
    {
        return Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                    MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                    MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8), offset);
    }

    /// <summary>
    /// Peeks 9-channel audio data into separate channel buffers.
    /// </summary>
    /// <param name="ch1">A writable span to receive audio samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive audio samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive audio samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive audio samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive audio samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive audio samples for the sixth channel.</param>
    /// <param name="ch7">A writable span to receive audio samples for the seventh channel.</param>
    /// <param name="ch8">A writable span to receive audio samples for the eighth channel.</param>
    /// <param name="ch9">A writable span to receive audio samples for the ninth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 9-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly nine channels.</exception>
    /// <remarks>
    /// Planar ↔ packed conversions are handled automatically.
    /// </remarks>
    public AVResult32 Peek(Span<byte> ch1, Span<byte> ch2, Span<byte> ch3, Span<byte> ch4,
                           Span<byte> ch5, Span<byte> ch6, Span<byte> ch7, Span<byte> ch8, Span<byte> ch9, int offset = 0)
    {
        if (Channels != 9)
            throw new NotSupportedException("Peek(9-channel) only supports 9-channel audio output.");

        byte** ptrs = stackalloc byte*[9];
        int samples = Math.Min(Math.Min(Math.Min(Math.Min(ch1.Length, ch2.Length),
                                                 Math.Min(ch3.Length, ch4.Length)),
                                         Math.Min(Math.Min(ch5.Length, ch6.Length),
                                                 Math.Min(ch7.Length, ch8.Length))),
                               ch9.Length) / Format.GetBytesPerSample();

        ptrs[0] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));
        ptrs[8] = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch9));

        return Format.IsPlanar()
            ? PeekPlanarToPlanar(ptrs, samples, offset)
            : PeekPackedToPlanar(ptrs, samples, offset);
    }

    /// <summary>
    /// Peeks 9-channel audio data into typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="ch1">A writable span to receive samples for the first channel.</param>
    /// <param name="ch2">A writable span to receive samples for the second channel.</param>
    /// <param name="ch3">A writable span to receive samples for the third channel.</param>
    /// <param name="ch4">A writable span to receive samples for the fourth channel.</param>
    /// <param name="ch5">A writable span to receive samples for the fifth channel.</param>
    /// <param name="ch6">A writable span to receive samples for the sixth channel.</param>
    /// <param name="ch7">A writable span to receive samples for the seventh channel.</param>
    /// <param name="ch8">A writable span to receive samples for the eighth channel.</param>
    /// <param name="ch9">A writable span to receive samples for the ninth channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one 9-channel sample.</param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of samples successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the audio stream does not have exactly nine channels.</exception>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> spans as bytes and calls 
    /// <see cref="Peek(Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, Span{byte}, int)"/>.
    /// </remarks>
    public AVResult32 Peek<T>(Span<T> ch1, Span<T> ch2, Span<T> ch3, Span<T> ch4,
                              Span<T> ch5, Span<T> ch6, Span<T> ch7, Span<T> ch8, Span<T> ch9, int offset = 0) where T : unmanaged
    {
        return Peek(MemoryMarshal.AsBytes(ch1), MemoryMarshal.AsBytes(ch2), MemoryMarshal.AsBytes(ch3),
                    MemoryMarshal.AsBytes(ch4), MemoryMarshal.AsBytes(ch5), MemoryMarshal.AsBytes(ch6),
                    MemoryMarshal.AsBytes(ch7), MemoryMarshal.AsBytes(ch8), MemoryMarshal.AsBytes(ch9), offset);
    }


    #endregion

    #region PeekArrays
    #region Peek([])

    /// <summary>
    /// Peeks audio data into a single byte array starting at a specific sample offset,  
    /// without advancing the current read position.
    /// </summary>
    /// <param name="data">
    /// A writable byte array to receive interleaved audio samples.  
    /// The data layout depends on the audio <see cref="Format"/> and the number of <see cref="Channels"/>.
    /// </param>
    /// <param name="offset">
    /// The sample offset (in frames) from which to start peeking.  
    /// A value of <c>0</c> peeks from the beginning of the available buffered data.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of bytes successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method allows inspecting buffered audio starting at a specific position without consuming it.  
    /// It simply wraps <see cref="Peek(Span{byte}, int)"/> for convenience and reads interleaved (packed) samples directly into <paramref name="data"/>.
    /// </remarks>
    public AVResult32 Peek(byte[] data, int offset) => Peek(data.AsSpan(), offset);


    /// <summary>
    /// Peeks typed audio samples into a single buffer starting at a specific sample offset,  
    /// without advancing the current read position.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// The size of <typeparamref name="T"/> must match the sample size of the current audio <see cref="Format"/>.
    /// </typeparam>
    /// <param name="data">
    /// A writable buffer to receive interleaved audio samples for all channels.  
    /// Samples are packed: one sample per channel in sequence for each frame.
    /// </param>
    /// <param name="offset">
    /// The sample offset (in frames) from which to start peeking.  
    /// A value of <c>0</c> peeks from the beginning of the available buffered data.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> value representing either:
    /// <list type="bullet">
    /// <item><description>The number of frames successfully peeked (if the operation succeeded).</description></item>
    /// <item><description>An error code (if the operation failed).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method reinterprets <paramref name="data"/> as a <see cref="Span{Byte}"/> and calls 
    /// <see cref="Peek(Span{byte}, int)"/> internally.  
    /// Like all Peek methods, it does not advance the internal read position.
    /// </remarks>
    public AVResult32 Peek<T>(T[] data, int offset) where T : unmanaged => Peek(data.AsSpan(), offset);


    /// <summary>
    /// Peeks multi-channel audio data into an array of channel buffers.
    /// </summary>
    /// <param name="data">
    /// An array of <see cref="byte"/> arrays, where each element represents one channel’s audio samples.
    /// The number of elements must match <see cref="Channels"/>.
    /// </param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one multi-channel sample.</param>
    /// <returns>The number of samples successfully peeked.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when any element of <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="data"/> length does not match <see cref="Channels"/>.</exception>
    public AVResult32 Peek(byte[][] data, int offset)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Peek(params byte[][]) only supports audio data with exactly {Channels} channels.");

        int samples = data.Min(static d => d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = handles[i].AddrOfPinnedObject().ToPointer();
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar((byte**)ptrs, samples, offset)
                : PeekPackedToPlanar((byte**)ptrs, samples, offset);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }

    /// <summary>
    /// Peeks multi-channel audio data into an array of typed sample buffers.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (e.g., <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="data">An array of <typeparamref name="T"/> arrays, one per channel.</param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one multi-channel sample.</param>
    /// <returns>The number of samples successfully peeked.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when any element of <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="data"/> length does not match <see cref="Channels"/>.</exception>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> arrays as bytes and calls 
    /// <see cref="Peek(params byte[][], int)"/>.
    /// </remarks>
    public AVResult32 Peek<T>(T[][] data, int offset) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
            throw new NotSupportedException(
                $"Peek(T[][]) only supports audio data with exactly {Channels} channels.");

        int bytesPerChannel = data.Min(static d => (d?.Length ?? throw new ArgumentException("One or more channel buffers are null.", nameof(data))) * sizeof(T));
        int samples = bytesPerChannel / Format.GetBytesPerSample();

        Span<GCHandle> handles = stackalloc GCHandle[Channels];
        void** ptrs = stackalloc void*[Channels];

        try
        {
            for (int i = 0; i < Channels; i++)
            {
                handles[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                ptrs[i] = (void*)handles[i].AddrOfPinnedObject();
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar((byte**)ptrs, samples, offset)
                : PeekPackedToPlanar((byte**)ptrs, samples, offset);
        }
        finally
        {
            foreach (ref var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }

    /// <summary>
    /// Peeks multi-channel audio data into a two-dimensional byte array.
    /// </summary>
    /// <param name="data">
    /// A two-dimensional <see cref="byte"/> array where the first dimension is the channel index and the second is the byte index.
    /// </param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one multi-channel sample.</param>
    /// <returns>The number of samples successfully peeked.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when the number of channels does not match <see cref="Channels"/>.</exception>
    public unsafe AVResult32 Peek(byte[,] data, int offset)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int byteCountPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Peek(byte[,]) only supports audio data with exactly {Channels} channels.");

        int samples = byteCountPerChannel / Format.GetBytesPerSample();

        byte** ptrs = stackalloc byte*[channelCount];

        fixed (byte* basePtr = data)
        {
            int rowStride = byteCountPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar(ptrs, samples, offset)
                : PeekPackedToPlanar(ptrs, samples, offset);
        }
    }

    /// <summary>
    /// Peeks multi-channel audio data into a two-dimensional typed array.
    /// </summary>
    /// <typeparam name="T">The unmanaged sample type (e.g., <see cref="float"/> or <see cref="short"/>).</typeparam>
    /// <param name="data">
    /// A two-dimensional <typeparamref name="T"/> array where the first dimension is the channel index and the second is the sample index.
    /// </param>
    /// <param name="offset">The sample offset from which to start peeking. Each unit corresponds to one multi-channel sample.</param>
    /// <returns>The number of samples successfully peeked.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the number of channels does not match <see cref="Channels"/> or
    /// when <c>sizeof(T)</c> does not match <see cref="Format.GetBytesPerSample()"/>.
    /// </exception>
    /// <remarks>
    /// This method simply reinterprets the <typeparamref name="T"/> array as bytes and calls
    /// <see cref="Peek(byte[,], int)"/>.
    /// </remarks>
    public unsafe AVResult32 Peek<T>(T[,] data, int offset) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int samplesPerChannel = data.GetLength(1);

        if (channelCount != Channels)
            throw new NotSupportedException(
                $"Peek(T[,]) only supports audio data with exactly {Channels} channels.");

        if (sizeof(T) != Format.GetBytesPerSample())
            throw new NotSupportedException(
                $"Element size sizeof({typeof(T).Name}) = {sizeof(T)} does not match format sample size {Format.GetBytesPerSample()}.");

        T** ptrs = stackalloc T*[channelCount];

        fixed (T* basePtr = data)
        {
            int rowStride = samplesPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + ch * rowStride;
            }

            return Format.IsPlanar()
                ? PeekPlanarToPlanar((byte**)ptrs, samplesPerChannel, offset)
                : PeekPackedToPlanar((byte**)ptrs, samplesPerChannel, offset);
        }
    }


    #endregion

    #endregion
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
    public AVResult32 Drop(int samples)
    {
        return ffmpeg.av_audio_fifo_drain(fifo, samples);
    }

    /// <summary>
    /// Clears all samples from the FIFO buffer, resetting it to an empty state.
    /// </summary>
    /// <remarks>
    /// After calling this method, the FIFO will contain zero samples.  
    /// This does not change the capacity of the FIFO, only the number of stored samples.
    /// </remarks>
    public void Clear() => ffmpeg.av_audio_fifo_reset(fifo);


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
            if (disposing)
            {
                // TODO: Clean up managed state (managed objects) if needed
            }

            if (fifo != null)
                ffmpeg.av_audio_fifo_free(fifo);
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
    /// Always call <see cref="Dispose"/> when finished using an <see cref="AudioFifo"/> instance to avoid memory leaks.
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

}
