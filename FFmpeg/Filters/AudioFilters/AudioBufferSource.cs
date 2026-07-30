using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Represents an audio buffer source filter that supplies audio frames to a filter graph.
/// </summary>
/// <remarks>
/// An <see cref="AudioBufferSource"/> is the entry point for audio frames into a filter graph.
/// The source can be configured using explicit audio parameters, <see cref="BufferSrcParameters"/>,
/// a <see cref="Codecs.CodecContext"/>, or an <see cref="Formats.AVStream"/>.
/// </remarks>
public unsafe class AudioBufferSource : FilterContext, IBufferSource
{
    internal AudioBufferSource(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Sets the parameters of the audio buffer source filter.
    /// </summary>
    /// <param name="parameters">
    /// The buffer source parameters to apply.
    /// </param>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    /// <remarks>
    /// This method must be called before the filter is initialized.
    /// </remarks>
    public AVResult32 SetBufferSourceParameters(BufferSrcParameters parameters) =>
        ffmpeg.av_buffersrc_parameters_set(context, parameters.parameters);

    /// <summary>
    /// Sends an audio frame to the buffer source filter.
    /// </summary>
    /// <param name="frame">
    /// The audio frame to send, or <see langword="null"/> to signal end-of-stream.
    /// </param>
    /// <param name="keepRef">
    /// <see langword="true"/> to retain a reference to the supplied frame;
    /// <see langword="false"/> to allow FFmpeg to take ownership of the frame
    /// when possible.
    /// </param>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    /// <remarks>
    /// When <paramref name="keepRef"/> is <see langword="true"/>, FFmpeg creates
    /// a new reference to the supplied frame. When it is <see langword="false"/>,
    /// ownership of the frame may be transferred to the filter.
    /// </remarks>
    public AVResult32 SendFrame(AVFrame? frame, bool keepRef = false)
    {
        AutoGen._AVFrame* f = frame != null ? frame.Frame : null;
        return keepRef
            ? ffmpeg.av_buffersrc_write_frame(context, f)
            : ffmpeg.av_buffersrc_add_frame(context, f);
    }

    /// <summary>
    /// Signals end-of-stream to the audio buffer source filter.
    /// </summary>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    /// <remarks>
    /// After end-of-stream has been signaled, no further frames can be submitted
    /// to the source filter.
    /// </remarks>
    public AVResult32 Drain() => SendFrame(null);

    /// <summary>
    /// Allocates an uninitialized audio buffer source filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The allocated, uninitialized audio buffer source filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated.
    /// </exception>
    public static AudioBufferSource Allocate(string name, FilterGraph graph) =>
        new(AllocateInternal(name, Filter.AudioBufferSource, graph));

    /// <summary>
    /// Creates and initializes an audio buffer source filter using the specified
    /// buffer source parameters.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="parameters">
    /// The parameters used to configure the audio buffer source.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="parameters"/> does not contain a valid audio sample rate.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be configured or initialized.
    /// </exception>
    public static AudioBufferSource Create(
        string name,
        BufferSrcParameters parameters,
        FilterGraph graph)
    {
        if (parameters.SampleRate <= 0)
            throw new ArgumentException(
                "The buffer source parameters must specify a positive sample rate.",
                nameof(parameters));

        AudioBufferSource context = Allocate(name, graph);

        context.SetBufferSourceParameters(parameters).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Creates and initializes an audio buffer source filter using the specified
    /// audio format.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="sampleRate">
    /// The sample rate, in samples per second.
    /// </param>
    /// <param name="sampleFormat">
    /// The sample format.
    /// </param>
    /// <param name="channels">
    /// The number of audio channels.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sampleRate"/> or <paramref name="channels"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be configured or initialized.
    /// </exception>
    public static AudioBufferSource Create(
        string name,
        int sampleRate,
        SampleFormat sampleFormat,
        int channels,
        FilterGraph graph)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                "The sample rate must be greater than zero.");

        if (channels <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(channels),
                channels,
                "The number of channels must be greater than zero.");

        AudioBufferSource context = Allocate(name, graph);

        context.SetOption("sample_rate", sampleRate).ThrowIfError();
        context.SetOption("sample_fmt", sampleFormat).ThrowIfError();
        context.SetOption("channels", channels).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Creates and initializes an audio buffer source filter using the specified
    /// audio format and channel layout.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="sampleRate">
    /// The sample rate, in samples per second.
    /// </param>
    /// <param name="sampleFormat">
    /// The sample format.
    /// </param>
    /// <param name="channelLayout">
    /// The channel layout of the audio stream.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sampleRate"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be configured or initialized.
    /// </exception>
    public static AudioBufferSource Create(
        string name,
        int sampleRate,
        SampleFormat sampleFormat,
        ChannelLayout channelLayout,
        FilterGraph graph)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                "The sample rate must be greater than zero.");

        AudioBufferSource context = Allocate(name, graph);

        context.SetOption("sample_rate", sampleRate).ThrowIfError();
        context.SetOption("sample_fmt", sampleFormat).ThrowIfError();
        context.SetOption("channel_layout", channelLayout).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Creates and initializes an audio buffer source filter using the settings
    /// from a codec context.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="ctx">
    /// The codec context whose audio parameters are used to configure the filter.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="ctx"/> does not describe an audio codec.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated, configured, or initialized.
    /// </exception>
    public static AudioBufferSource Create(
        string name,
        Codecs.CodecContext ctx,
        FilterGraph graph)
    {
        if (ctx.CodecType != MediaType.Audio)
            throw new ArgumentException(
                "The codec context does not describe an audio codec.",
                nameof(ctx));

        AudioBufferSource context = Allocate(name, graph);

        using BufferSrcParameters parameters = BufferSrcParameters.Allocate();

        parameters.ChannelLayout.CopyFrom(ctx.ChannelLayout);
        parameters.SampleFormat = ctx.SampleFormat;
        parameters.SampleAspectRatio = ctx.SampleAspectRatio;
        parameters.SampleRate = ctx.SampleRate;
        parameters.TimeBase = ctx.TimeBase;

        context.SetBufferSourceParameters(parameters).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Creates and initializes an audio buffer source filter using the settings
    /// from an audio stream.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="stream">
    /// The audio stream whose parameters are used to configure the filter.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="stream"/> does not describe an audio stream.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated, configured, or initialized.
    /// </exception>
    public static AudioBufferSource Create(
        string name,
        Formats.AVStream stream,
        FilterGraph graph)
    {
        if (stream.MediaType != MediaType.Audio)
            throw new ArgumentException(
                "The stream does not describe an audio stream.",
                nameof(stream));

        AudioBufferSource context = Allocate(name, graph);

        using BufferSrcParameters parameters = BufferSrcParameters.Allocate();

        parameters.ChannelLayout.CopyFrom(stream.CodecParameters.ChannelLayout);
        parameters.SampleFormat = stream.CodecParameters.SampleFormat;
        parameters.SampleRate = stream.CodecParameters.SampleRate;
        parameters.TimeBase = stream.TimeBase;

        context.SetBufferSourceParameters(parameters).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Gets or sets the time base of the audio source.
    /// </summary>
    /// <value>
    /// The time base used for timestamps of audio frames submitted to the source,
    /// or <see cref="Rational.NaN"/> if the option could not be read.
    /// </value>
    public Rational TimeBase
    {
        get => TryGetOption("time_base", out Rational tb).IsError
            ? Rational.NaN
            : tb;
        set => SetOption("time_base", value);
    }

    /// <summary>
    /// Gets or sets the sample rate of the audio source.
    /// </summary>
    /// <value>
    /// The sample rate in samples per second, or <c>0</c> if the option could
    /// not be read.
    /// </value>
    public int SampleRate
    {
        get => TryGetOption("sample_rate", out int value).IsError ? 0 : value;
        set => SetOption("sample_rate", value);
    }

    /// <summary>
    /// Gets or sets the sample format of the audio source.
    /// </summary>
    /// <value>
    /// The sample format, or <see cref="SampleFormat.None"/> if the option could
    /// not be read.
    /// </value>
    public SampleFormat SampleFormat
    {
        get => TryGetOption("sample_fmt", out SampleFormat value).IsError
            ? SampleFormat.None
            : value;
        set => SetOption("sample_fmt", value);
    }

    /// <summary>
    /// Gets or sets the number of audio channels of the source.
    /// </summary>
    /// <value>
    /// The number of audio channels, or <c>0</c> if the option could not be read.
    /// </value>
    public int Channels
    {
        get => TryGetOption("channels", out int value).IsError ? 0 : value;
        set => SetOption("channels", value);
    }

    /// <summary>
    /// Gets or sets the channel layout of the audio source.
    /// </summary>
    /// <value>
    /// The channel layout of the source.
    /// </value>
    [DisallowNull]
    public ChannelLayout? ChannelLayout
    {
        get => TryGetOption("channel_layout", out ChannelLayout value).IsError
            ? default
            : value;
        set => SetOption("channel_layout", value);
    }

    /// <summary>
    /// Returns a string representation of the audio format.
    /// </summary>
    /// <returns>
    /// A string containing the sample rate, channel count, and sample format.
    /// </returns>
    public override string ToString() =>
        $"{SampleRate}Hz:{Channels}ch@{SampleFormat}";
}

