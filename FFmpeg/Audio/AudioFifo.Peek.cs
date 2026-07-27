using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Audio;

public unsafe partial class AudioFifo
{
    /// <summary>
    /// Peeks audio data from the <see cref="AudioFifo"/> buffer into an <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to receive audio samples.  
    /// If the frame has no buffer, its channel layout, sample format, and sample count can be automatically initialized:
    /// <list type="bullet">
    /// <item><description>If <see cref="AVFrame.ChannelLayout" href=".Channels"/> is 0, it is set to the default layout for the FIFO’s <see cref="Channels"/>.</description></item>
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
        return frame.SampleFormat == Format
            ? (AVResult32)ffmpeg.av_audio_fifo_peek(fifo, (void**)frame.ExtendedData, frame.SampleCount)
            : Format.IsPlanar()
            ? PeekPlanarToPacked(frame.ExtendedData[0], frame.SampleCount)
            : PeekPackedToPlanar(frame.ExtendedData, frame.SampleCount);
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
    /// Its size must match <see cref="SampleExtensions.GetBytesPerSample(SampleFormat)"/>.
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
    public AVResult32 Peek<T>(Span<T> buffer) where T : unmanaged => Peek(MemoryMarshal.AsBytes(buffer));

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(left));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(right));

        return Format.IsPlanar() ? PeekPlanarToPlanar(ptrs, samples) : PeekPackedToPlanar(ptrs, samples);
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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));

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

        ptrs[0] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch1));
        ptrs[1] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch2));
        ptrs[2] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch3));
        ptrs[3] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch4));
        ptrs[4] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch5));
        ptrs[5] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch6));
        ptrs[6] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch7));
        ptrs[7] = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(ch8));

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
        {
            throw new NotSupportedException(
                $"Peek(params byte[][]) only supports audio data with exactly {Channels} channels.");
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
                ? PeekPlanarToPlanar((byte**)ptrs, samples)
                : PeekPackedToPlanar((byte**)ptrs, samples);
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
        {
            throw new NotSupportedException(
                $"Peek(T[][]) only supports audio data with exactly {Channels} channels.");
        }

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
            foreach (ref GCHandle handle in handles)
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
        {
            throw new NotSupportedException(
                $"Peek(byte[,]) only supports audio data with exactly {Channels} channels.");
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
    /// when <c>sizeof(T)</c> does not match <c>Format.GetBytesPerSample()</c>.
    /// </exception>
    public unsafe AVResult32 Peek<T>(T[,] data) where T : unmanaged
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        int channelCount = data.GetLength(0);
        int samplesPerChannel = data.GetLength(1);

        if (channelCount != Channels)
        {
            throw new NotSupportedException(
                $"Peek(T[,]) only supports audio data with exactly {Channels} channels.");
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

            return Format.IsPlanar()
                ? PeekPlanarToPlanar((byte**)ptrs, samplesPerChannel)
                : PeekPackedToPlanar((byte**)ptrs, samplesPerChannel);
        }
    }

    #endregion

    #endregion
}
