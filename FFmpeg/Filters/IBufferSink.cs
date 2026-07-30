using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.IO;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFmpeg.Filters;

/// <summary>
/// Interface to access ReceiveFrame
/// </summary>
public unsafe interface IBufferSink
{
    /// <summary>
    /// Pointer to the internal filter context
    /// </summary>
    protected _AVFilterContext* Context { get; }

    /// <summary>
    /// Receives a frame from a buffer sink filter.
    /// </summary>
    /// <param name="frame">
    /// The destination frame that receives the filtered data.
    /// </param>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    public AVResult32 ReceiveFrame(AVFrame frame)
    {
        frame.Unreference();
        int res = ffmpeg.av_buffersink_get_frame(Context, frame.Frame);
        frame.TimeBase = ffmpeg.av_buffersink_get_time_base(Context);
        frame.BestEffortTimestamp = frame.PresentationTimestamp;
        return res;
    }

    /// <summary>
    /// Gets the media type produced by the buffer sink filter.
    /// </summary>
    public MediaType MediaType =>
        (MediaType)ffmpeg.av_buffersink_get_type(Context);

    /// <summary>
    /// Gets the time base of frames produced by the buffer sink filter.
    /// </summary>
    public Rational TimeBase =>
        ffmpeg.av_buffersink_get_time_base(Context);

    /// <summary>
    /// Gets the pixel format produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public PixelFormat PixelFormat =>
        (PixelFormat)ffmpeg.av_buffersink_get_format(Context);

    /// <summary>
    /// Gets the frame rate of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public Rational FrameRate =>
        ffmpeg.av_buffersink_get_frame_rate(Context);

    /// <summary>
    /// Gets the width of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public int Width =>
        ffmpeg.av_buffersink_get_w(Context);

    /// <summary>
    /// Gets the height of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public int Height =>
        ffmpeg.av_buffersink_get_h(Context);

    /// <summary>
    /// Gets the color space of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public ColorSpace ColorSpace =>
        (ColorSpace)ffmpeg.av_buffersink_get_colorspace(Context);

    /// <summary>
    /// Gets the sample aspect ratio of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public Rational PixelAspectRatio =>
        ffmpeg.av_buffersink_get_sample_aspect_ratio(Context);

    /// <summary>
    /// Gets the alpha mode of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public AlphaMode AlphaMode => (AlphaMode)ffmpeg.av_buffersink_get_alpha_mode(Context);
    

    /// <summary>
    /// Gets the color range of frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for video buffer sink filters.
    /// </remarks>
    public ColorRange ColorRange =>
        (ColorRange)ffmpeg.av_buffersink_get_color_range(Context);

    /// <summary>
    /// Gets the number of channels in audio frames produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for audio buffer sink filters.
    /// </remarks>
    public int Channels =>
        ffmpeg.av_buffersink_get_channels(Context);

    /// <summary>
    /// Gets the sample format produced by the buffer sink filter.
    /// </summary>
    /// <remarks>
    /// This property is only valid for audio buffer sink filters.
    /// </remarks>
    public SampleFormat SampleFormat =>
        (SampleFormat)ffmpeg.av_buffersink_get_format(Context);

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
    public bool TryGetChannelLayout([NotNullWhen(true)] out Audio.ChannelLayout? layout)
    {
        _AVChannelLayout l;
        if (ffmpeg.av_buffersink_get_ch_layout(Context, &l) < 0)
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
    public int SampleRate =>
        ffmpeg.av_buffersink_get_sample_rate(Context);
}
