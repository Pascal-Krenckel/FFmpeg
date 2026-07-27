using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFmpeg.Filters;

public unsafe partial class FilterContext
{
    /// <summary>
    /// Sets the parameters of a buffer source filter.
    /// </summary>
    /// <param name="parameters">
    /// The buffer source parameters to apply.
    /// </param>
    /// <returns>
    /// The result of the operation.
    /// </returns>
    public AVResult32 SetBufferSourceParameters(BufferSrcParameters parameters) =>
        ffmpeg.av_buffersrc_parameters_set(context, parameters.parameters);

    /// <summary>
    /// Gets the sample format produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for audio buffer sink filters.
    /// </remarks>
    public SampleFormat BufferSinkSampleFormat =>
        (SampleFormat)ffmpeg.av_buffersink_get_format(context);

    /// <summary>
    /// Gets the pixel format produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public PixelFormat BufferSinkPixelFormat =>
        (PixelFormat)ffmpeg.av_buffersink_get_format(context);

    /// <summary>
    /// Gets the media type produced by the buffer sink filter.
    /// </summary>
    public MediaType BufferSinkType =>
        (MediaType)ffmpeg.av_buffersink_get_type(context);

    /// <summary>
    /// Gets the time base of frames produced by the buffer sink filter.
    /// </summary>
    public Rational BufferSinkTimeBase =>
        ffmpeg.av_buffersink_get_time_base(context);

    /// <summary>
    /// Gets the frame rate of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public Rational BufferSinkFrameRate =>
        ffmpeg.av_buffersink_get_frame_rate(context);

    /// <summary>
    /// Gets the width of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public int BufferSinkWidth =>
        ffmpeg.av_buffersink_get_w(context);

    /// <summary>
    /// Gets the height of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public int BufferSinkHeight =>
        ffmpeg.av_buffersink_get_h(context);

    /// <summary>
    /// Gets the sample aspect ratio of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public Rational BufferSinkSampleAspectRatio =>
        ffmpeg.av_buffersink_get_sample_aspect_ratio(context);

    /// <summary>
    /// Gets the color space of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public ColorSpace BufferSinkColorSpace =>
        (ColorSpace)ffmpeg.av_buffersink_get_colorspace(context);

    /// <summary>
    /// Gets the color range of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public ColorRange BufferSinkColorRange =>
        (ColorRange)ffmpeg.av_buffersink_get_color_range(context);

    /// <summary>
    /// Gets the number of channels in audio frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for audio buffer sink filters.
    /// </remarks>
    public int BufferSinkChannels =>
        ffmpeg.av_buffersink_get_channels(context);



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
    /// <remarks>
    /// This method is only valid for audio buffer sink filters.
    /// </remarks>
    public bool TryGetBufferSinkChannelLayout([NotNullWhen(true)] out Audio.ChannelLayout? layout)
    {
        _AVChannelLayout l;
        if (ffmpeg.av_buffersink_get_ch_layout(context,&l) < 0)
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
    /// <remarks>
    /// This property is only valid for audio buffer sink filters.
    /// </remarks>
    public int BufferSinkSampleRate =>
        ffmpeg.av_buffersink_get_sample_rate(context);
}