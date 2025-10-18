using FFmpeg.Utils;
using System.Security.Cryptography.X509Certificates;

namespace FFmpeg.Audio;


/// <summary>
/// Represents an audio buffer that manages audio data in various formats and provides methods for
/// manipulating and accessing the buffer.
/// </summary>
public sealed unsafe class AudioBuffer : IDisposable
{
    private AudioBufferRef buffer = default;

    /// <summary>
    /// Gets the maximum capacity of the buffer in terms of samples.
    /// </summary>
    public int Capacity => buffer.Capacity;

    /// <summary>
    /// Gets or sets the number of audio samples currently stored in the buffer.
    /// </summary>
    public int Samples
    {
        get => buffer.Samples;
        internal set => buffer.Samples = value;
    }

    /// <summary>
    /// Gets the audio buffer metadata, including the sample format, number of channels, and alignment.
    /// </summary>
    public AudioBufferInfo Info => buffer.Info;

    /// <summary>
    /// Gets a pointer to the planes in the audio buffer if the format is planar. 
    /// Each plane corresponds to a channel in the audio data.
    /// </summary>
    internal byte** Planes => buffer.Planes;

    /// <summary>
    /// Gets a pointer to the array of line sizes, which represent the size of each channel's data in the buffer.
    /// </summary>
    internal int* LineSizes => buffer.LineSizes;

    /// <summary>
    /// Gets a value indicating whether the audio format is planar.
    /// In planar format, each channel's data is stored separately.
    /// </summary>
    public bool IsPlanar => Format.IsPlanar();

    /// <summary>
    /// Gets a value indicating whether the audio format is packed.
    /// In packed format, the audio data for all channels is stored interleaved.
    /// </summary>
    public bool IsPacked => Format.IsPacked();

    /// <summary>
    /// Gets the number of bytes per sample for the current audio format.
    /// </summary>
    public int BytesPerSample => ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)Format);

    /// <summary>
    /// Gets the number of audio channels in the buffer.
    /// </summary>
    public int Channels => buffer.Channels;

    /// <summary>
    /// Gets the sample format of the audio buffer.
    /// </summary>
    public SampleFormat Format => buffer.Format;

    /// <summary>
    /// Gets the alignment value for the audio data in the buffer.
    /// </summary>
    public int Alignment => buffer.Alignment;

    /// <summary>
    /// Gets a value indicating whether the buffer is read-only. 
    /// If the buffer has more than one reference, <see langword="true"/>; otherwise, <see langword="false"/>.
    /// </summary>
    public bool IsReadOnly => ffmpeg.av_buffer_is_writable(buffer.Reference) == 0;

    /// <summary>
    /// Attempts to make the buffer writable by ensuring the reference count is 1.
    /// If the buffer is already writable, it does nothing.
    /// </summary>
    /// <returns><see langword="true"/> if the buffer is writable; otherwise, <see langword="false"/>.</returns>
    public bool TryMakeWriteable()
    {
        var reference = buffer.Reference;
        AVResult32 res = ffmpeg.av_buffer_make_writable(&reference);
        buffer = new AudioBufferRef(reference);
        return res >= 0;
    }

    /// <summary>
    /// Ensures that the buffer is writable. If the buffer cannot be made writable, throws an <see cref="OutOfMemoryException"/>.
    /// </summary>
    /// <exception cref="OutOfMemoryException">Thrown when the buffer cannot be made writable.</exception>
    public void MakeWriteable()
    {
        if (!TryMakeWriteable())
            throw new OutOfMemoryException();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioBuffer"/> class using the specified <see cref="AudioBufferRef"/>.
    /// </summary>
    /// <param name="buffer">The reference to the underlying audio buffer.</param>
    private AudioBuffer(AudioBufferRef buffer)
    {
        this.buffer = buffer;
    }

    /// <summary>
    /// Allocates a new <see cref="AudioBuffer"/> with the specified buffer info and sample capacity.
    /// </summary>
    /// <param name="info">The buffer info including format, channels, and alignment.</param>
    /// <param name="sampleCapacity">The number of samples the buffer should be able to hold.</param>
    /// <returns>A newly allocated <see cref="AudioBuffer"/>.</returns>
    public static AudioBuffer Allocate(AudioBufferInfo info, int sampleCapacity) => new(AudioBufferRef.Allocate(info.Format, info.Channels, info.Alignment, sampleCapacity));

    /// <summary>
    /// Allocates a new <see cref="AudioBuffer"/> with the specified format, number of channels, and sample capacity.
    /// </summary>
    /// <param name="format">The audio sample format, which defines the type of samples (e.g., integer or floating-point).</param>
    /// <param name="channels">The number of audio channels (e.g., 2 for stereo).</param>
    /// <param name="sampleCapacity">The number of audio samples the buffer should be able to hold per channel.</param>
    /// <returns>A newly allocated <see cref="AudioBuffer"/> with the specified format, channels, and sample capacity.</returns>
    public static AudioBuffer Allocate(SampleFormat format, int channels, int sampleCapacity) => new(AudioBufferRef.Allocate(format, channels, 1, sampleCapacity));

    /// <summary>
    /// Creates an <see cref="AudioBuffer"/> from a packed audio source without performing type checking.
    /// </summary>
    /// <param name="info">The audio buffer info.</param>
    /// <param name="source">The source data in packed format.</param>
    /// <returns>An <see cref="AudioBuffer"/> initialized with the specified packed audio data.</returns>
    /// <exception cref="ArgumentException">Thrown if the length of the buffer is not a multiple of BytesPerSample*Channels.</exception>
    public static AudioBuffer CreateFromPackedUncheckedType(AudioBufferInfo info, ReadOnlySpan<byte> source)
    {
        int samplesToCopy = source.Length / info.Channels / info.Format.GetBytesPerSample();
        if (source.Length != samplesToCopy * info.Channels * info.Format.GetBytesPerSample())
            throw new ArgumentException("The length of buffer must be a multiple of BytesPerSample*Channels");

        var audioBuffer = Allocate(info, samplesToCopy);
        _ = audioBuffer.CopyFromPlanarUncheckedType(source);
        return audioBuffer;
    }

    /// <summary>
    /// Creates an <see cref="AudioBuffer"/> from a packed source, performing type validation based on the sample format.
    /// </summary>
    /// <typeparam name="T">The type of the audio data, which must be unmanaged.</typeparam>
    /// <param name="info">The audio buffer info.</param>
    /// <param name="source">The source data in packed format.</param>
    /// <returns>An <see cref="AudioBuffer"/> initialized with the specified packed audio data.</returns>
    public static AudioBuffer CreateFromPacked<T>(AudioBufferInfo info, ReadOnlySpan<T> source) where T : unmanaged
    {
        info.Format.ValidateType<T>();
        fixed (void* ptr = source)
            return CreateFromPackedUncheckedType(info, new ReadOnlySpan<byte>(ptr, source.Length * sizeof(T)));
    }

    /// <summary>
    /// Clears the audio buffer, resetting the sample count to zero.
    /// </summary>
    public void Clear() => Samples = 0;

    /// <summary>
    /// Copies audio data from a planar source to a packed destination.
    /// </summary>
    /// <param name="info">The <see cref="AudioBufferInfo"/> containing format and channel information.</param>
    /// <param name="planarSrc">The source buffer in planar format.</param>
    /// <param name="srcSampleIndex">The starting sample index in the source buffer.</param>
    /// <param name="packedDst">The destination buffer in packed format.</param>
    /// <param name="dstSampleIndex">The starting sample index in the destination buffer.</param>
    /// <param name="samples">The number of samples to copy.</param>
    private static void CopyFromPlanarToPacked(AudioBufferInfo info, byte** planarSrc, int srcSampleIndex, byte* packedDst, int dstSampleIndex, int samples)
    {
        int sampleSize = info.Format.GetBytesPerSample();
        int packSize = sampleSize * info.Channels;
        for (int sample = 0; sample < samples; sample++)
        {
            for (int ch = 0; ch < info.Channels; ch++)
            {
                Buffer.MemoryCopy(planarSrc[ch] + (sample + srcSampleIndex) * sampleSize,
                                  packedDst + packSize * (sample + dstSampleIndex) + ch * sampleSize,
                                  sampleSize, sampleSize);
            }
        }
    }

    /// <summary>
    /// Copies audio data from a planar source to a planar destination.
    /// </summary>
    /// <param name="info">The <see cref="AudioBufferInfo"/> containing format and channel information.</param>
    /// <param name="planarSrc">The source buffer in planar format.</param>
    /// <param name="srcSampleIndex">The starting sample index in the source buffer.</param>
    /// <param name="planarDst">The destination buffer in planar format.</param>
    /// <param name="dstSampleIndex">The starting sample index in the destination buffer.</param>
    /// <param name="samples">The number of samples to copy.</param>
    private static void CopyFromPlanarToPlanar(AudioBufferInfo info, byte** planarSrc, int srcSampleIndex, byte** planarDst, int dstSampleIndex, int samples)
    {
        int sampleSize = info.Format.GetBytesPerSample();
        for (int ch = 0; ch < info.Channels; ch++)
        {
            Buffer.MemoryCopy(planarSrc[ch] + srcSampleIndex * sampleSize,
                              planarDst[ch] + dstSampleIndex * sampleSize,
                              samples * sampleSize, samples * sampleSize);
        }
    }

    /// <summary>
    /// Copies audio data from a packed source to a packed destination.
    /// </summary>
    /// <param name="info">The <see cref="AudioBufferInfo"/> containing format and channel information.</param>
    /// <param name="packedSrc">The source buffer in packed format.</param>
    /// <param name="srcSampleIndex">The starting sample index in the source buffer.</param>
    /// <param name="packedDst">The destination buffer in packed format.</param>
    /// <param name="dstSampleIndex">The starting sample index in the destination buffer.</param>
    /// <param name="samples">The number of samples to copy.</param>
    private static void CopyFromPackedToPacked(AudioBufferInfo info, byte* packedSrc, int srcSampleIndex, byte* packedDst, int dstSampleIndex, int samples)
    {
        int sampleSize = info.Format.GetBytesPerSample();
        Buffer.MemoryCopy(packedSrc + sampleSize * info.Channels * srcSampleIndex,
                          packedDst + sampleSize * info.Channels * dstSampleIndex,
                          samples * info.Channels * sampleSize, samples * info.Channels * sampleSize);
    }

    /// <summary>
    /// Copies audio data from a packed source to a planar destination.
    /// </summary>
    /// <param name="info">The <see cref="AudioBufferInfo"/> containing format and channel information.</param>
    /// <param name="packedSrc">The source buffer in packed format.</param>
    /// <param name="srcSampleIndex">The starting sample index in the source buffer.</param>
    /// <param name="planarDst">The destination buffer in planar format.</param>
    /// <param name="dstSampleIndex">The starting sample index in the destination buffer.</param>
    /// <param name="samples">The number of samples to copy.</param>
    private static void CopyFromPackedToPlanar(AudioBufferInfo info, byte* packedSrc, int srcSampleIndex, byte** planarDst, int dstSampleIndex, int samples)
    {
        int sampleSize = info.Format.GetBytesPerSample();
        int packSize = sampleSize * info.Channels;
        for (int sample = 0; sample < samples; sample++)
        {
            for (int ch = 0; ch < info.Channels; ch++)
            {
                Buffer.MemoryCopy(packedSrc + packSize * (sample + srcSampleIndex) + ch * sampleSize,
                                  planarDst[ch] + sampleSize * (dstSampleIndex + sample),
                                  sampleSize, sampleSize);
            }
        }
    }

    /// <summary>
    /// Copies audio data from this buffer to a packed format buffer without checking the data type.
    /// </summary>
    /// <param name="destination">The destination buffer to copy data to.</param>
    /// <param name="startSrcSampleIndex">The starting sample index in the source buffer.</param>
    /// <returns>The number of samples per channel that were copied.</returns>
    /// <exception cref="ArgumentException">Thrown when the destination buffer length is not a multiple of BytesPerSample and Channels.</exception>
    public int CopyToPackedUncheckedType(Span<byte> destination, int startSrcSampleIndex = 0)
    {
        int samplesPerChannel = destination.Length / Channels / BytesPerSample;
        if (destination.Length != samplesPerChannel * Channels * BytesPerSample)
            throw new ArgumentException("The length of buffer must be a multiple of BytesPerSample*Channels");

        int samplesToCopy = Math.Min(Samples - startSrcSampleIndex, samplesPerChannel);
        if (samplesToCopy <= 0) return 0;

        fixed (byte* ptr = destination)
        {
            if (IsPacked)
                CopyFromPackedToPacked(Info, this.buffer.Buffer, startSrcSampleIndex, ptr, 0, samplesToCopy);
            else
                CopyFromPlanarToPacked(Info, this.buffer.Planes, startSrcSampleIndex, ptr, 0, samplesToCopy);
        }

        return samplesToCopy;
    }

    /// <summary>
    /// Copies audio data from this buffer to a packed format buffer with type checking.
    /// </summary>
    /// <typeparam name="T">The type of the audio data.</typeparam>
    /// <param name="destination">The destination buffer to copy data to.</param>
    /// <param name="startSrcSampleIndex">The starting sample index in the source buffer.</param>
    /// <returns>The number of samples per channel that were copied.</returns>
    public int CopyToPacked<T>(Span<T> destination, int startSrcSampleIndex = 0) where T : unmanaged
    {
        Format.ValidateType<T>();
        fixed (void* ptr = destination)
        {
            return CopyToPackedUncheckedType(new Span<byte>(ptr, destination.Length * sizeof(T)), startSrcSampleIndex);
        }
    }

    /// <summary>
    /// Copies audio data from this buffer to another <see cref="AudioBuffer"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="AudioBuffer"/> to copy data to.</param>
    /// <param name="startSrcSampleIndex">The starting sample index in the source buffer.</param>
    /// <returns>The number of samples copied.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of channels does not match between the source and destination buffers.</exception>
    public int CopyTo(AudioBuffer destination, int startSrcSampleIndex = 0)
    {
        if (Channels != destination.Channels)
            throw new ArgumentException("Copying is only possible if the number of channels match. Please use SwrContext to convert.", nameof(destination));

        if (destination.Format.AsPlanar() == Format.AsPlanar())
        {
            int samplesToCopy = Math.Min(destination.Capacity - destination.Samples, Samples - startSrcSampleIndex);
            if (Format.IsPlanar() && destination.Format.IsPlanar())
                CopyFromPlanarToPlanar(Info, Planes, startSrcSampleIndex, destination.Planes, destination.Samples, samplesToCopy);
            else if (Format.IsPacked() && destination.Format.IsPacked())
                CopyFromPackedToPacked(Info, Planes[0], startSrcSampleIndex, destination.Planes[0], destination.Samples, samplesToCopy);
            else if (Format.IsPacked())
                CopyFromPackedToPlanar(Info, Planes[0], startSrcSampleIndex, destination.Planes, destination.Samples, samplesToCopy);
            else
                CopyFromPlanarToPlanar(Info, Planes, startSrcSampleIndex, destination.Planes, destination.Samples, samplesToCopy);

            destination.Samples += samplesToCopy;
            return samplesToCopy;
        }
        else
        {
            using var layout = new ChannelLayout();
            layout.Init(Channels);
            using SwrContext context = new(layout, Format, 1, layout, destination.Format, 1);
            var res = context.Convert(this, startSrcSampleIndex, destination, destination.Samples);
            res.ThrowIfError();
            return res;
        }
    }


    /// <summary>
    /// Creates a new copy of the current <see cref="AudioBuffer"/> with its own allocated memory.
    /// </summary>
    /// <returns>A new <see cref="AudioBuffer"/> instance with copied data.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if memory allocation fails.</exception>
    public AudioBuffer Copy()
    {
        var buffer = ffmpeg.av_buffer_alloc(this.buffer.Reference->size);
        if (buffer == null) throw new OutOfMemoryException();
        Buffer.MemoryCopy(this.buffer.Reference->data, buffer->data, buffer->size, buffer->size);
        return new AudioBuffer(new(buffer));
    }

    /// <summary>
    /// Clones the current <see cref="AudioBuffer"/> by creating a new reference to the existing buffer.
    /// </summary>
    /// <returns>A new <see cref="AudioBuffer"/> instance sharing the same memory reference.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if the buffer reference creation fails.</exception>
    public AudioBuffer Clone()
    {
        var buffer = ffmpeg.av_buffer_ref(this.buffer.Reference);
        if (buffer == null) throw new OutOfMemoryException();
        return new AudioBuffer(new(buffer));
    }

    /// <summary>
    /// Copies audio data from a planar source without checking the data type.
    /// </summary>
    /// <param name="source">The source buffer containing audio data to copy.</param>
    /// <returns>The number of samples copied.</returns>
    /// <exception cref="ArgumentException">Thrown when the source buffer length is not a multiple of BytesPerSample and Channels.</exception>
    public int CopyFromPlanarUncheckedType(ReadOnlySpan<byte> source)
    {
        if (source.Length % (BytesPerSample * Channels) != 0)
            throw new ArgumentException("The length of buffer must be a multiple of BytesPerSample*Channels");

        int samplesToCopy = Math.Min(Capacity - Samples, source.Length / Channels / BytesPerSample);
        if (samplesToCopy <= 0) return 0;

        fixed (byte* ptr = source)
        {
            if (IsPacked)
                CopyFromPackedToPacked(Info, ptr, 0, Planes[0], Samples, samplesToCopy);
            else
                CopyFromPackedToPlanar(Info, ptr, 0, Planes, Samples, samplesToCopy);
        }

        Samples += samplesToCopy;
        return samplesToCopy;
    }

    /// <summary>
    /// Copies audio data from a planar source with type checking.
    /// </summary>
    /// <typeparam name="T">The data type of the audio samples.</typeparam>
    /// <param name="source">The source buffer containing audio data to copy.</param>
    /// <returns>The number of samples copied.</returns>
    public int CopyFromPlanar<T>(ReadOnlySpan<T> source) where T : unmanaged
    {
        Format.ValidateType<T>();
        fixed (void* ptr = source)
        {
            return CopyFromPlanarUncheckedType(new(ptr, source.Length * sizeof(T)));
        }
    }

    /// <summary>
    /// Copies audio data from another <see cref="AudioBuffer"/> into this buffer.
    /// </summary>
    /// <param name="source">The source <see cref="AudioBuffer"/> to copy data from.</param>
    /// <param name="startSrcSampleIndex">The starting sample index in the source buffer.</param>
    /// <returns>The number of samples copied.</returns>
    public int CopyFrom(AudioBuffer source, int startSrcSampleIndex = 0)
    {
        return source.CopyTo(this, startSrcSampleIndex);
    }

    /// <summary>
    /// Copies audio data from an <see cref="AVFrame"/> into this buffer.
    /// </summary>
    /// <param name="source">The <see cref="AVFrame"/> containing the source audio data.</param>
    /// <param name="startSrcSampleIndex">The starting sample index in the source frame.</param>
    /// <returns>The number of samples copied.</returns>
    /// <exception cref="ArgumentException">Thrown when the channel count in the source frame does not match the destination buffer.</exception>
    public int CopyFrom(AVFrame source)
    {
        if (Channels != source.ChannelLayout.Channels)
            throw new ArgumentException(nameof(source), "Copying is only possible if the number of channels match. Please use SwrContext to convert.");

        if (source.SampleFormat.AsPlanar() == Format.AsPlanar())
        {
            int samplesToCopy = Math.Min(Capacity - Samples, source.SampleCount);
            if (Format.IsPlanar() && source.SampleFormat.IsPlanar())
                CopyFromPlanarToPlanar(Info, source.ExtendedData, 0, Planes, Samples, samplesToCopy);
            else if (Format.IsPacked() && source.SampleFormat.IsPacked())
                CopyFromPackedToPacked(Info, source.ExtendedData[0], 0, Planes[0], Samples, samplesToCopy);
            else if (Format.IsPacked())
                CopyFromPackedToPlanar(Info, source.ExtendedData[0], 0, Planes, Samples, samplesToCopy);
            else
                CopyFromPlanarToPlanar(Info, source.ExtendedData, 0, Planes, Samples, samplesToCopy);

            Samples += samplesToCopy;
            return samplesToCopy;
        }
        else
        {
            using SwrContext context = new(source.ChannelLayout, source.SampleFormat, source.SampleRate, source.ChannelLayout, Format, source.SampleRate);
            var res = context.Convert(source, this, Samples);
            res.ThrowIfError();
            return res;
        }
    }

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{T}"/> of packed audio data, where the samples are tightly packed in the buffer.
    /// </summary>
    /// <remarks>
    /// This method will only return the span if the audio buffer is packed (i.e., the data is stored without padding).
    /// If the buffer is not packed, a <see cref="NotSupportedException"/> is thrown.
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Thrown when the buffer is not packed (i.e., <see langword="false"/> for <see cref="IsPacked"/>).
    /// </exception>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> representing the packed audio data.</returns>
    public ReadOnlySpan<byte> AsReadOnlySpanPacked()
    {
        if (!IsPacked)
            throw new NotSupportedException();
        return new ReadOnlySpan<byte>(buffer.Buffer, Samples * BytesPerSample * Channels);
    }

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{T}"/> of packed audio data, where the samples are tightly packed in the buffer.
    /// </summary>
    /// <remarks>
    /// This method will only return the span if the audio buffer is packed (i.e., the data is stored without padding).
    /// If the buffer is not packed, a <see cref="NotSupportedException"/> is thrown.
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Thrown when the buffer is not packed (i.e., <see langword="false"/> for <see cref="IsPacked"/>).
    /// </exception>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> representing the packed audio data.</returns>
    public Span<byte> AsSpanPacked()
    {
        if (!IsPacked)
            throw new NotSupportedException();
        return new Span<byte>(buffer.Buffer, Samples * BytesPerSample * Channels);
    }

    #region Dispose

    private bool disposedValue;

    /// <summary>
    /// Disposes the <see cref="AudioBuffer"/> object, releasing both managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">Indicates whether to release managed resources in addition to unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: Release managed resources if needed
            }

            var b = buffer.Reference;
            ffmpeg.av_buffer_unref(&b);
            buffer = default;

            // TODO: Free unmanaged resources and override the finalizer, if necessary
            // TODO: Notify large fields to null to release memory
            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer for the <see cref="AudioBuffer"/> class. 
    /// Ensures resources are freed when the object is garbage collected.
    /// </summary>
    ~AudioBuffer()
    {
        // Do not change this code. Put cleanup logic in Dispose(bool disposing).
        Dispose(disposing: false);
    }

    /// <summary>
    /// Publicly accessible dispose method. 
    /// Disposes the current <see cref="AudioBuffer"/> and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup logic in Dispose(bool disposing).
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion


}