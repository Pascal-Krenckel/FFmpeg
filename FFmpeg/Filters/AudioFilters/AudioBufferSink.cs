using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Filters.VideoFilters;
using FFmpeg.IO;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Represents an audio buffer sink filter that receives audio frames from a filter graph.
/// </summary>
/// <remarks>
/// An <see cref="AudioBufferSink"/> is the exit point for audio frames from a filter graph.
/// Frames can be retrieved using <see cref="ReceiveFrame(AVFrame)"/> after the filter graph
/// has been configured and initialized.
/// </remarks>
public unsafe class AudioBufferSink : FilterContext, IBufferSink
{
    internal AudioBufferSink(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Allocates an uninitialized audio buffer sink filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The allocated, uninitialized audio buffer sink filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated.
    /// </exception>
    public static AudioBufferSink Allocate(string name, FilterGraph graph) =>
        new(AllocateInternal(name, Filter.AudioBufferSink, graph));

    /// <summary>
    /// Creates and initializes an audio buffer sink filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio buffer sink filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated or initialized.
    /// </exception>
    public static AudioBufferSink Create(string name, FilterGraph graph)
    {
        AudioBufferSink context = Allocate(name, graph);
        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Receives the next available audio frame from the buffer sink filter.
    /// </summary>
    /// <param name="frame">
    /// The destination frame that receives the filtered audio data.
    /// </param>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    /// <remarks>
    /// The supplied frame is unreferenced before receiving the new frame.
    /// The frame's <see cref="AVFrame.TimeBase"/> is updated to the sink's time base
    /// and its <see cref="AVFrame.BestEffortTimestamp"/> is set to its presentation
    /// timestamp.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="frame"/> is <see langword="null"/>.
    /// </exception>
    public AVResult32 ReceiveFrame(AVFrame frame)
    {
        if(frame == null) throw new ArgumentNullException(nameof(frame),"´frame was null");

        frame.Unreference();

        int res = ffmpeg.av_buffersink_get_frame(context, frame.Frame);

        frame.TimeBase = ffmpeg.av_buffersink_get_time_base(context);
        frame.BestEffortTimestamp = frame.PresentationTimestamp;

        return res;
    }

    /// <summary>
    /// Gets the time base of frames produced by the audio buffer sink filter.
    /// </summary>
    /// <value>
    /// The time base used for timestamps of frames received from the sink.
    /// </value>
    public Rational TimeBase =>
        ffmpeg.av_buffersink_get_time_base(context);

    /// <summary>
    /// Gets the number of channels in audio frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The number of audio channels in the frames received from the sink.
    /// </value>
    public int Channels =>
        ffmpeg.av_buffersink_get_channels(context);

    /// <summary>
    /// Gets the sample format of audio frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The sample format of the frames received from the sink.
    /// </value>
    public SampleFormat SampleFormat =>
        (SampleFormat)ffmpeg.av_buffersink_get_format(context);

    /// <summary>
    /// Attempts to retrieve the channel layout of audio frames produced by the buffer sink filter.
    /// </summary>
    /// <param name="layout">
    /// When this method returns <see langword="true"/>, contains the channel layout;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the channel layout was successfully retrieved;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetChannelLayout([NotNullWhen(true)] out Audio.ChannelLayout? layout)
    {
        _AVChannelLayout l;

        if (ffmpeg.av_buffersink_get_ch_layout(context, &l) < 0)
        {
            layout = null;
            return false;
        }

        layout = new(l);
        return true;
    }

    /// <summary>
    /// Gets the sample rate of audio frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The sample rate of the frames received from the sink, in samples per second.
    /// </value>
    public int SampleRate =>
        ffmpeg.av_buffersink_get_sample_rate(context);
}
