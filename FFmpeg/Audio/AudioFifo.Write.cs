using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Audio;

public unsafe partial class AudioFifo
{
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
            ? (AVResult32)ffmpeg.av_audio_fifo_write(fifo, (void**)frame.ExtendedData, frame.SampleCount)
            : Format.IsPlanar()
            ? WritePackedToPlanar(frame.ExtendedData[0], frame.SampleCount)
            : WritePlanarToPacked(frame.ExtendedData, frame.SampleCount);

    #region Packed/Mono
    /// <summary>
    /// Writes packed multi-channel audio data from a single contiguous buffer.
    /// </summary>
    /// <param name="packedData">
    /// A read-only span containing interleaved audio samples for all channels.
    /// Samples are packed: one sample per channel in sequence for each frame.
    /// </param>
    /// <returns>
    /// The number of frames successfully written, or an error code.
    /// </returns>
    public AVResult32 Write(ReadOnlySpan<byte> packedData)
    {
        if (packedData.IsEmpty)
            return 0;


        int blockSize = Format.GetBytesPerSample() * Channels;

        if (packedData.Length % blockSize != 0)
            return AVResult32.InvalidArgument;


        int samples = packedData.Length / blockSize;


        fixed (byte* bufferPtr = packedData)
        {
            return Format.IsPlanar()
                ? WritePackedToPlanar(bufferPtr, samples)
                : WritePackedToPacked(bufferPtr, samples);
        }
    }
    #endregion

    #region Write(ReadOnlySpan<byte> ch1, ch2....)
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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(left));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(right));

        return Format.IsPlanar() ? WritePlanarToPlanar(ptrs, samples) : WritePlanarToPacked(ptrs, samples);
    }


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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }



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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }


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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }




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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }



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
        ;
        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));
        ptrs[8] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch9));

        return Format.IsPlanar()
            ? WritePlanarToPlanar(ptrs, samples)
            : WritePlanarToPacked(ptrs, samples);
    }



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
        {
            throw new NotSupportedException(
                $"Write(params byte[][]) only supports audio data with exactly {Channels} channels.");
        }

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
            foreach (ref GCHandle handle in handles)
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
    /// <see cref="Write(byte[][])"/>.
    /// </remarks>
    public AVResult32 Write<T>(params T[][] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != Channels)
        {
            throw new NotSupportedException(
                $"Write(T[][]) only supports audio data with exactly {Channels} channels.");
        }

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
            foreach (ref GCHandle handle in handles)
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
        {
            throw new NotSupportedException(
                $"Write(byte[,]) only supports audio data with exactly {Channels} channels.");
        }

        int samples = byteCountPerChannel / Format.GetBytesPerSample();

        byte** ptrs = stackalloc byte*[channelCount];

        fixed (byte* basePtr = data)
        {
            int rowStride = byteCountPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + (ch * rowStride);
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
    /// Or if the element size <c>sizeof(T)</c> does not match <see cref="SampleExtensions.GetBytesPerSample(SampleFormat)"/>.
    /// </exception>
    public unsafe AVResult32 Write<T>(T[,] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int samplesPerChannel = data.GetLength(1);

        if (channelCount != Channels)
        {
            throw new NotSupportedException(
                $"Write(T[,]) only supports audio data with exactly {Channels} channels.");
        }

        if (sizeof(T) != Format.GetBytesPerSample())
        {
            throw new NotSupportedException(
                $"Element size sizeof({typeof(T).Name}) = {sizeof(T)} does not match format sample size {Format.GetBytesPerSample()}.");
        }

        T** ptrs = stackalloc T*[channelCount];

        fixed (T* basePtr = data)
        {
            int rowStride = samplesPerChannel;

            for (int ch = 0; ch < channelCount; ch++)
            {
                ptrs[ch] = basePtr + (ch * rowStride);
            }

            // Cast to byte** for planar/packed methods
            return Format.IsPlanar()
                ? WritePlanarToPlanar((byte**)ptrs, samplesPerChannel)
                : WritePlanarToPacked((byte**)ptrs, samplesPerChannel);
        }
    }

    #endregion

    #region Marshals ReadOnlySpan<T>
    /// <summary>
    /// Writes packed multi-channel audio data from a read-only span of typed samples.
    /// </summary>
    /// <typeparam name="T">
    /// The unmanaged sample type (for example, <see cref="float"/> or <see cref="short"/>).  
    /// Its size must match <see cref="SampleExtensions.GetBytesPerSample(SampleFormat)"/>.
    /// </typeparam>
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

}
