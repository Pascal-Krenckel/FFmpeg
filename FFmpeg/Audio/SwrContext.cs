using FFmpeg.Utils;

namespace FFmpeg.Audio;
/// <summary>
/// Represents a managed wrapper for the FFmpeg SwrContext, used for audio resampling, format conversion,
/// and channel layout remapping.
/// </summary>
public unsafe class SwrContext : Options.OptionQueryBase, IDisposable
{
    private AutoGen._SwrContext* context;

    /// <summary>
    /// Gets or sets the source channel layout (e.g., stereo, mono).
    /// Setting this value will reset the internal SwrContext state.
    /// </summary>
    public ChannelLayout SourceLayout
    {
        get => !TryGetOption("in_chlayout", out ChannelLayout layout, false).IsError ? layout : new();
        set
        {
            ffmpeg.swr_close(context);
            _ = SetOption("in_chlayout", value, false);
        }
    }

    /// <summary>
    /// Gets or sets the destination channel layout.
    /// Setting this value will reset the internal SwrContext state.
    /// </summary>
    public ChannelLayout DestinationLayout
    {
        get => !TryGetOption("out_chlayout", out ChannelLayout layout, false).IsError ? layout : new();
        set
        {
            ffmpeg.swr_close(context);
            _ = SetOption("out_chlayout", value, false);
        }
    }

    /// <summary>
    /// Gets or sets the source sample format (e.g., AV_SAMPLE_FMT_FLTP).
    /// Setting this value will reset the internal SwrContext state.
    /// </summary>
    public SampleFormat SourceFormat
    {
        get => !TryGetOption("in_sample_fmt", out SampleFormat format, false).IsError ? format : SampleFormat.None;
        set
        {
            ffmpeg.swr_close(context);
            _ = SetOption("in_sample_fmt", value, false);
        }
    }

    /// <summary>
    /// Gets or sets the destination sample format.
    /// Setting this value will reset the internal SwrContext state.
    /// </summary>
    public SampleFormat DestinationFormat
    {
        get => !TryGetOption("out_sample_fmt", out SampleFormat format, false).IsError ? format : SampleFormat.None;
        set
        {
            ffmpeg.swr_close(context);
            _ = SetOption("out_sample_fmt", value, false);
        }
    }

    /// <summary>
    /// Gets or sets the source sample rate (e.g., 44100 Hz).
    /// Setting this value will reset the internal SwrContext state.
    /// </summary>
    public int SourceSampleRate
    {
        get => !TryGetOption("in_sample_rate", out int sampleRate, false).IsError ? sampleRate : -1;
        set
        {
            ffmpeg.swr_close(context);
            _ = SetOption("in_sample_rate", value, false);
        }
    }

    /// <summary>
    /// Gets or sets the destination sample rate.
    /// Setting this value will reset the internal SwrContext state.
    /// </summary>
    public int DestinationSampleRate
    {
        get => !TryGetOption("out_sample_rate", out int sampleRate, false).IsError ? sampleRate : -1;
        set
        {
            ffmpeg.swr_close(context);
            _ = SetOption("out_sample_rate", value, false);
        }
    }


    /// <summary>
    /// Gets the number of bytes per audio sample for the current source format.
    /// </summary>
    public int BytesPerSampleSource => ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)SourceFormat);

    /// <summary>
    /// Gets the number of bytes per audio sample for the current destination format.
    /// </summary>
    public int BytesPerSampleDestination => ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)DestinationFormat);

    /// <summary>
    /// Gets the native pointer to the internal SwrContext structure.
    /// </summary>
    protected override unsafe void* Pointer => context;

    /// <summary>
    /// Gets the number of bytes per sample for a specified audio sample format.
    /// </summary///>
    /// <param name="format">The sample format to query.</param>
    /// <returns>The number of bytes per sample.</returns>
    public static int BytesPerSample(SampleFormat format) =>
        ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)format);

    /// <summary>
    /// Initializes a new instance of the <see cref="SwrContext"/> class with full configuration parameters.
    /// Automatically initializes the underlying SwrContext.
    /// </summary>
    /// <param name="srcLayout">Source channel layout.</param>
    /// <param name="srcFormat">Source sample format.</param>
    /// <param name="srcSampleRate">Source sample rate.</param>
    /// <param name="dstLayout">Destination channel layout.</param>
    /// <param name="dstFormat">Destination sample format.</param>
    /// <param name="dstSampleRate">Destination sample rate.</param>
    public SwrContext(IChannelLayout srcLayout, SampleFormat srcFormat, int srcSampleRate,
                      IChannelLayout dstLayout, SampleFormat dstFormat, int dstSampleRate)
    {
        AutoGen._SwrContext* context;
        AutoGen._AVChannelLayout srcL = srcLayout.Layout;
        AutoGen._AVChannelLayout dstL = dstLayout.Layout;

        AVResult32 res = ffmpeg.swr_alloc_set_opts2(&context, &dstL, (AutoGen._AVSampleFormat)dstFormat, dstSampleRate,
                                                    &srcL, (AutoGen._AVSampleFormat)srcFormat, srcSampleRate, 0, null);
        this.context = context;
        res.ThrowIfError();

        res = ffmpeg.swr_init(context);
        res.ThrowIfError();
    }

    /// <summary>
    /// Initializes a new empty <see cref="SwrContext"/> instance.
    /// Configuration should be done manually via property setters or <see cref="Config"/>.
    /// </summary>
    public SwrContext()
    {
        context = ffmpeg.swr_alloc();
        SourceFormat = SampleFormat.None;
        DestinationFormat = SampleFormat.None;
        SourceLayout = new ChannelLayout();
        DestinationLayout = new ChannelLayout();
        SourceSampleRate = 0;
        DestinationSampleRate = 0;
    }

    /// <summary>
    /// Initializes the SwrContext after configuration. Must be called before conversion if using the default constructor.
    /// </summary>
    /// <returns>An <see cref="AVResult32"/> indicating success or error.</returns>
    public AVResult32 Init() => ffmpeg.swr_init(context);

    /// <summary>
    /// Gets a value indicating whether the SwrContext is initialized and ready for conversion.
    /// </summary>
    public bool IsInitialized => System.Convert.ToBoolean(ffmpeg.swr_is_initialized(context));

    /// <summary>
    /// Initializes a new SwrContext based on two AVFrame formats.
    /// </summary>
    /// <param name="srcFormat">The source AVFrame for format detection.</param>
    /// <param name="dstFormat">The destination AVFrame for format detection.</param>
    public SwrContext(AVFrame srcFormat, AVFrame dstFormat) : this() => Config(srcFormat, dstFormat);

    /// <summary>
    /// Configures the <see cref="SwrContext"/> using source and destination <see cref="AVFrame"/> formats.
    /// </summary>
    /// <remarks>
    /// This method updates the internal configuration using frame metadata. 
    /// <br/>
    /// <strong>Note:</strong> You do <b>not</b> need to call <see cref="Init"/> manually if you use <see cref="Convert"/> afterwards,
    /// as it will initialize the context if necessary.
    /// </remarks>
    /// <param name="src">Source AVFrame.</param>
    /// <param name="dst">Destination AVFrame.</param>
    /// <returns>An <see cref="AVResult32"/> indicating success or failure.</returns>
    public AVResult32 Config(AVFrame src, AVFrame dst) =>
        ffmpeg.swr_config_frame(context, dst.Frame, src.Frame);

    /// <summary>
    /// Configures the <see cref="SwrContext"/> with explicit channel layouts, sample formats, and sample rates.
    /// </summary>
    /// <remarks>
    /// This method closes any existing context configuration, reallocates the context, and sets new parameters.
    /// <br/>
    /// <strong>Note:</strong> You do <b>not</b> need to call <see cref="Init"/> manually if you use <see cref="Convert"/> afterwards,
    /// as it will initialize the context if necessary.
    /// </remarks>
    /// <param name="srcLayout">Source channel layout.</param>
    /// <param name="srcFormat">Source sample format.</param>
    /// <param name="srcSampleRate">Source sample rate.</param>
    /// <param name="dstLayout">Destination channel layout.</param>
    /// <param name="dstFormat">Destination sample format.</param>
    /// <param name="dstSampleRate">Destination sample rate.</param>
    /// <returns>An <see cref="AVResult32"/> indicating success or failure.</returns>
    public AVResult32 Config(IChannelLayout srcLayout, SampleFormat srcFormat, int srcSampleRate,
                             IChannelLayout dstLayout, SampleFormat dstFormat, int dstSampleRate)
    {
        AutoGen._SwrContext* context = this.context;
        ffmpeg.swr_close(context);

        AutoGen._AVChannelLayout srcL = srcLayout.Layout;
        AutoGen._AVChannelLayout dstL = dstLayout.Layout;

        AVResult32 res = ffmpeg.swr_alloc_set_opts2(&context, &dstL, (AutoGen._AVSampleFormat)dstFormat, dstSampleRate,
                                                    &srcL, (AutoGen._AVSampleFormat)srcFormat, srcSampleRate, 0, null);
        this.context = context;
        return res;
    }



    /// <summary>
    /// Converts audio data from a source <see cref="AVFrame"/> to a destination <see cref="AVFrame"/>.
    /// </summary>
    /// <remarks>
    /// This method will automatically call <see cref="Init"/> if the context is not yet initialized.
    /// If conversion fails, it is recommended to verify whether the context was initialized correctly.
    /// </remarks>
    /// <param name="src">The source frame. Can be <c>null</c>.</param>
    /// <param name="dst">The destination frame. Can be <c>null</c>.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="src"/> and <paramref name="dst"/> refer to the same instance.</exception>
    public AVResult32 Convert(AVFrame? src, AVFrame? dst) => src == dst
            ? throw new ArgumentException("Source and destination AVFrame must not reference the same instance.", nameof(dst))
            : dst == null
            ? (AVResult32)ffmpeg.swr_convert_frame(context, null, src!.Frame)
            : src != null
                ? (AVResult32)ffmpeg.swr_convert_frame(context, dst.Frame, src.Frame)
                : (AVResult32)ffmpeg.swr_convert_frame(context, dst.Frame, null);

    /// <summary>
    /// Converts audio from an <see cref="AVFrame"/> source to an <see cref="AudioBuffer"/> destination, starting at the specified sample index.
    /// </summary>
    /// <remarks>
    /// This method will automatically call <see cref="Init"/> if the context is not initialized.
    /// If conversion fails, check both the return value and initialization state.
    /// </remarks>
    /// <param name="src">The source frame, or <c>null</c> to flush the resampler.</param>
    /// <param name="dst">The destination buffer.</param>
    /// <param name="dstSampleIndex">The starting index in the destination buffer to write to.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    public AVResult32 Convert(AVFrame? src, AudioBuffer dst, int dstSampleIndex)
    {
        if (!EnsureInitialize(out AVResult32 res))
            return res;

        byte** dstBuffer = stackalloc byte*[dst.IsPlanar ? dst.Channels : 1];
        if (dst.IsPlanar)
        {
            for (int j = 0; j < dst.Channels; j++)
                dstBuffer[j] = dst.Planes[j] + (dst.BytesPerSample * dstSampleIndex);
        }
        else
        {
            dstBuffer[0] = dst.Planes[0] + (dst.BytesPerSample * dstSampleIndex * dst.Channels);
        }

        res = src == null
            ? ffmpeg.swr_convert(context, dstBuffer, dst.Capacity - dstSampleIndex, null, 0)
            : ffmpeg.swr_convert(context, dstBuffer, dst.Capacity - dst.Samples, src.ExtendedData, src.SampleCount);

        if (res.IsError)
            return res;

        dst.Samples = Math.Max(dstSampleIndex + res, dst.Samples);
        return res;
    }

    /// <summary>
    /// Converts audio from an <see cref="AVFrame"/> to an <see cref="AudioBuffer"/>, writing to the end of the buffer.
    /// </summary>
    /// <param name="src">Source frame. Can be <c>null</c> to flush the resampler.</param>
    /// <param name="dst">Destination audio buffer.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    public AVResult32 Convert(AVFrame? src, AudioBuffer dst) =>
        Convert(src, dst, dst.Samples);

    /// <summary>
    /// Converts audio from an <see cref="AudioBuffer"/> source to an <see cref="AVFrame"/> destination.
    /// </summary>
    /// <remarks>
    /// This method will automatically call <see cref="Init"/> if needed.
    /// </remarks>
    /// <param name="src">Source buffer.</param>
    /// <param name="srcSampleIndex">Sample index to begin reading from in the source buffer.</param>
    /// <param name="dst">Destination frame. Can be <c>null</c>.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    public AVResult32 Convert(AudioBuffer src, int srcSampleIndex, AVFrame? dst)
    {
        if (!EnsureInitialize(out AVResult32 res))
            return res;

        byte** srcBuffer = stackalloc byte*[src.IsPlanar ? src.Channels : 1];
        if (src.IsPlanar)
        {
            for (int j = 0; j < src.Channels; j++)
                srcBuffer[j] = src.Planes[j] + (src.BytesPerSample * srcSampleIndex);
        }
        else
        {
            srcBuffer[0] = src.Planes[0] + (src.BytesPerSample * srcSampleIndex * src.Channels);
        }

        res = dst == null
            ? ffmpeg.swr_convert(context, null, 0, srcBuffer, src.Samples - srcSampleIndex)
            : ffmpeg.swr_convert(context, dst.ExtendedData, dst.SampleCount, srcBuffer, src.Samples - srcSampleIndex);

        return res;
    }

    /// <summary>
    /// Converts audio from an <see cref="AudioBuffer"/> source to an <see cref="AVFrame"/> destination.
    /// </summary>
    /// <param name="src">Source buffer.</param>
    /// <param name="dst">Destination frame. Can be <c>null</c>.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    public AVResult32 Convert(AudioBuffer src, AVFrame? dst) =>
        Convert(src, 0, dst);

    /// <summary>
    /// Converts audio from a source <see cref="AudioBuffer"/> to a destination <see cref="AudioBuffer"/>, starting at specified sample indices.
    /// </summary>
    /// <remarks>
    /// This method will automatically call <see cref="Init"/> if the context is not initialized.
    /// If both buffers are <c>null</c>, the method throws.
    /// </remarks>
    /// <param name="src">Source buffer. Can be <c>null</c>.</param>
    /// <param name="srcSampleIndex">Sample index to begin reading from in the source buffer.</param>
    /// <param name="dst">Destination buffer. Can be <c>null</c>.</param>
    /// <param name="dstSampleIndex">Sample index to begin writing to in the destination buffer.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if both <paramref name="src"/> and <paramref name="dst"/> are <c>null</c>.</exception>
    public AVResult32 Convert(AudioBuffer src, int srcSampleIndex, AudioBuffer dst, int dstSampleIndex)
    {
        if (src == null && dst == null)
            throw new ArgumentNullException("At least one of src or dst must be non-null.");

        if (src == null)
            return Convert(null, dst, dstSampleIndex);

        if (dst == null)
            return Convert(src, srcSampleIndex, null);

        if (!EnsureInitialize(out AVResult32 res))
            return res;

        byte** srcBuffer = stackalloc byte*[src.IsPlanar ? src.Channels : 1];
        if (src.IsPlanar)
        {
            for (int j = 0; j < src.Channels; j++)
                srcBuffer[j] = src.Planes[j] + (src.BytesPerSample * srcSampleIndex);
        }
        else
        {
            srcBuffer[0] = src.Planes[0] + (src.BytesPerSample * srcSampleIndex * src.Channels);
        }

        byte** dstBuffer = stackalloc byte*[dst.IsPlanar ? dst.Channels : 1];
        if (dst.IsPlanar)
        {
            for (int j = 0; j < dst.Channels; j++)
                dstBuffer[j] = dst.Planes[j] + (dst.BytesPerSample * dstSampleIndex);
        }
        else
        {
            dstBuffer[0] = dst.Planes[0] + (dst.BytesPerSample * dstSampleIndex * dst.Channels);
        }

        res = ffmpeg.swr_convert(context, dstBuffer, dst.Capacity - dst.Samples, srcBuffer, src.Samples - srcSampleIndex);
        if (res < 0)
            return res;

        dst.Samples = Math.Max(dstSampleIndex + res, dst.Samples);
        return res;
    }



    /// <summary>
    /// Converts an <see cref="AVFrame"/> to a raw audio buffer represented by a pointer.
    /// </summary>
    /// <remarks>
    /// This method supports packed or planar mono output only. If the output format is planar,
    /// the destination layout must be mono (1 channel).
    /// Automatically calls <see cref="Init"/> if not yet initialized.
    /// </remarks>
    /// <param name="src">The source <see cref="AVFrame"/>. Can be <c>null</c> to flush the resampler.</param>
    /// <param name="dst">Pointer to the destination buffer.</param>
    /// <param name="outSamplesPerChannel">The number of output samples per channel that <paramref name="dst"/> can hold.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the source layout, format, or sample rate does not match expectations, or
    /// if planar output is requested with non-mono layout.
    /// </exception>
    public AVResult32 Convert(AVFrame? src, IntPtr dst, int outSamplesPerChannel)
    {
        if (!EnsureInitialize(out AVResult32 res))
            return res;

        if (src != null)
        {
            using ChannelLayout chlLayout = SourceLayout;
            if (src.ChannelLayout != chlLayout)
                throw new ArgumentException("The source channel layout does not match the configured layout.");
            if (src.SampleRate != SourceSampleRate)
                throw new ArgumentException("The source sample rate does not match the configured sample rate.");
            if (src.SampleFormat != SourceFormat)
                throw new ArgumentException("The source sample format does not match the configured format.");
        }

        if (DestinationFormat.IsPlanar())
        {
            using ChannelLayout chlLayout = DestinationLayout;
            if (chlLayout.Channels != 1)
                throw new ArgumentException("Only mono is accepted for planar output. Use packed output or perform channel mixing before conversion.");
        }

        byte** dstData = (byte**)&dst;
        byte** srcData = src != null ? src.ExtendedData : null;
        int srcSamples = src?.SampleCount ?? 0;

        return ffmpeg.swr_convert(context, dstData, outSamplesPerChannel, srcData, srcSamples);
    }

    /// <summary>
    /// Converts an <see cref="AVFrame"/> to a raw audio buffer provided as a <see cref="Span{Byte}"/>.
    /// </summary>
    /// <remarks>
    /// Supports packed or planar mono output. Automatically calculates how many output samples fit into <paramref name="dst"/>.
    /// Automatically calls <see cref="Init"/> if not yet initialized.
    /// </remarks>
    /// <param name="src">The source <see cref="AVFrame"/>. Can be <c>null</c>.</param>
    /// <param name="dst">A span representing the output buffer.</param>
    /// <returns>
    /// On success, returns the number of output samples produced. On error, returns an <see cref="AVResult32"/> error code.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the source layout, format, or sample rate does not match the configuration,
    /// or if planar output is used with more than one channel.
    /// </exception>
    public AVResult32 Convert(AVFrame? src, Span<byte> dst)
    {
        if (!EnsureInitialize(out AVResult32 res))
            return res;

        if (src != null)
        {
            using ChannelLayout chlLayout = SourceLayout;
            if (src.ChannelLayout != chlLayout)
                throw new ArgumentException("The source channel layout does not match the configured layout.");
            if (src.SampleRate != SourceSampleRate)
                throw new ArgumentException("The source sample rate does not match the configured sample rate.");
            if (src.SampleFormat != SourceFormat)
                throw new ArgumentException("The source sample format does not match the configured format.");
        }

        using ChannelLayout outChlLayout = DestinationLayout;

        if (DestinationFormat.IsPlanar() && outChlLayout.Channels != 1)
            throw new ArgumentException("Only mono is accepted for planar output. Use packed output or convert channels beforehand.");

        int bytesPerSample = DestinationFormat.GetBytesPerSample();
        int channels = outChlLayout.Channels;
        int outSamplesPerChannel = dst.Length / (channels * bytesPerSample);

        fixed (byte* dstData = dst)
        {
            byte** dstBuffer = &dstData;
            byte** srcData = src != null ? src.ExtendedData : null;
            int srcSamples = src?.SampleCount ?? 0;

            return ffmpeg.swr_convert(context, dstBuffer, outSamplesPerChannel, srcData, srcSamples);
        }
    }

    /// <summary>
    /// Ensures that the context has been initialized before conversion.
    /// </summary>
    /// <param name="error">Notify to the result of <see cref="Init"/> if initialization fails.</param>
    /// <returns><c>true</c> if the context is already initialized or successfully initialized now; otherwise <c>false</c>.</returns>
    private bool EnsureInitialize(out AVResult32 error)
    {
        error = 0;
        if (!IsInitialized)
        {
            error = Init();
            if (error.IsError)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the number of output samples that can be produced from the currently queued input samples.
    /// </summary>
    /// <returns>The number of available output samples, or an error code.</returns>
    public AVResult32 GetOutputSampleCount() =>
        ffmpeg.swr_get_out_samples(context, 0);

    /// <summary>
    /// Gets the number of output samples that will be generated from a given number of input samples.
    /// </summary>
    /// <param name="inSamples">The number of input samples per channel.</param>
    /// <returns>The corresponding number of output samples, or an error code.</returns>
    public AVResult32 GetOutputSampleCount(int inSamples) =>
        ffmpeg.swr_get_out_samples(context, inSamples);

    /// <summary>
    /// Releases unmanaged resources associated with the <see cref="SwrContext"/>.
    /// </summary>
    public void Dispose()
    {
        AutoGen._SwrContext* c = context;
        ffmpeg.swr_free(&c);
        context = null;
    }


    /// <summary>
    /// Drops all output samples currently stored in the internal SwrContext.
    /// </summary>
    /// <remarks>
    /// This method clears any buffered output samples that have not yet been retrieved or converted,
    /// effectively resetting the output state of the resampler.
    /// </remarks>
    public void Clear() => _ = ffmpeg.swr_drop_output(context, GetOutputSampleCount());

    /// <summary>
    /// Finalizer to ensure unmanaged resources are freed if <see cref="Dispose"/> is not called.
    /// </summary>
    ~SwrContext() => Dispose();
}