using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Filters.VideoFilters;


/// <summary>
/// Represents a video buffer source filter that supplies video frames to a filter graph.
/// </summary>
/// <remarks>
/// A <see cref="VideoBufferSource"/> is the entry point for video frames into a filter graph.
/// It can be configured using explicit video parameters, <see cref="BufferSrcParameters"/>,
/// a <see cref="Codecs.CodecContext"/>, or an <see cref="Formats.AVStream"/>.
/// </remarks>
public unsafe class VideoBufferSource : FilterContext, IBufferSource
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

    internal VideoBufferSource(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Sends a frame to the video buffer source filter.
    /// </summary>
    /// <param name="frame">
    /// The frame to send, or <see langword="null"/> to signal end-of-stream.
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
        return keepRef ? ffmpeg.av_buffersrc_write_frame(context, f) : ffmpeg.av_buffersrc_add_frame(context, f);
    }

    /// <summary>
    /// Signals end-of-stream to the video buffer source filter.
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
    /// Creates and initializes a video buffer source filter using the specified video format.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="width">
    /// The width of the video frames, in pixels.
    /// </param>
    /// <param name="height">
    /// The height of the video frames, in pixels.
    /// </param>
    /// <param name="format">
    /// The pixel format of the video frames.
    /// </param>
    /// <param name="timeBase">
    /// The time base of the input video stream.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized video buffer source filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be configured or initialized.
    /// </exception>
    /// <remarks>
    /// Unlike <see cref="Allocate(string, FilterGraph)"/>, this method initializes
    /// the filter before returning it, leaving it ready to receive frames.
    /// </remarks>
    public static VideoBufferSource Create(
        string name,
        int width,
        int height,
        PixelFormat format,
        Rational timeBase,
        FilterGraph graph)
    {
        VideoBufferSource? context =
            Allocate(name, graph)
            ?? throw new ArgumentNullException();

        context.SetOption("pix_fmt", (AutoGen._AVPixelFormat)format).ThrowIfError();
        context.SetOption("video_size", width, height).ThrowIfError();
        context.SetOption("time_base", timeBase).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Allocates an uninitialized video buffer source filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The allocated, uninitialized video buffer source filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated.
    /// </exception>
    /// <remarks>
    /// The returned filter has not been initialized. Configure its options or
    /// properties as required and call <see cref="FilterContext.Init()"/> before
    /// using it in the filter graph.
    /// </remarks>
    public static VideoBufferSource Allocate(string name, FilterGraph graph)
        => new(AllocateInternal(name, Filter.VideoBufferSource, graph));

    /// <summary>
    /// Creates and initializes a video buffer source filter from the specified buffer source parameters.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="parameters">
    /// The parameters describing the video source.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized video buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="parameters"/> does not describe a video source.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be configured or initialized.
    /// </exception>
    /// <remarks>
    /// The supplied parameters must describe a video source. Audio parameters are
    /// rejected because this class represents a video buffer source.
    /// </remarks>
    public static VideoBufferSource Create(
        string name,
        BufferSrcParameters parameters,
        FilterGraph graph)
    {
        bool isVideo = !(parameters.Width == 0 && parameters.Height == 0);
        if (!isVideo)
            throw new ArgumentException("The bufferSrcParameters are not video parameters");

        VideoBufferSource context = Allocate(name, graph);
        context.SetBufferSourceParameters(parameters).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Creates and initializes a video buffer source filter from a codec context.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="ctx">
    /// The codec context containing the video format parameters.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized video buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="ctx"/> does not represent a video codec.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated, configured, or initialized.
    /// </exception>
    /// <remarks>
    /// The source is configured from the codec context's video properties,
    /// including its dimensions, pixel format, time base, frame rate, color
    /// information, alpha mode, and pixel aspect ratio.
    /// </remarks>
    public static VideoBufferSource Create(
        string name,
        Codecs.CodecContext ctx,
        FilterGraph graph)
    {
        if (ctx.CodecType != MediaType.Video)
            throw new ArgumentException("The codec context does not represent a video stream.", nameof(ctx));

        _AVFilterContext* ptr = AllocateInternal(name, Filter.VideoBufferSource, graph);
        if (ptr == null)
            throw new NullReferenceException();

        var context = new VideoBufferSource(ptr)
        {
            Width = ctx.Width,
            Height = ctx.Height,
            PixelFormat = ctx.PixelFormat,
            PixelAspectRation = ctx.SampleAspectRatio,
            TimeBase = ctx.TimeBase,
            FrameRate = ctx.FrameRate,
            ColorSpace = ctx.ColorSpace,
            ColorRange = ctx.ColorRange,
            AlphaMode = ctx.AlphaMode
        };

        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates and initializes a video buffer source filter from an input stream.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="stream">
    /// The input stream containing the video format parameters.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized video buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="stream"/> does not represent a video stream.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated, configured, or initialized.
    /// </exception>
    /// <remarks>
    /// The source is configured from the stream's codec parameters and timing
    /// information.
    /// </remarks>
    public static VideoBufferSource Create(
        string name,
        Formats.AVStream stream,
        FilterGraph graph)
    {
        if (stream.MediaType != MediaType.Video)
            throw new ArgumentException("The stream does not represent a video stream.", nameof(stream));

        _AVFilterContext* ptr = AllocateInternal(name, Filter.VideoBufferSource, graph);
        if (ptr == null)
            throw new NullReferenceException();

        var parameters = stream.CodecParameters;
        var context = new VideoBufferSource(ptr)
        {
            Width = parameters.Width,
            Height = parameters.Height,
            PixelFormat = parameters.PixelFormat,
            PixelAspectRation = parameters.SampleAspectRatio,
            TimeBase = stream.TimeBase,
            FrameRate = parameters.FrameRate,
            ColorSpace = parameters.ColorSpace,
            ColorRange = parameters.ColorRange,
            AlphaMode = parameters.AlphaMode,
        };

        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Gets or sets the pixel format of the video frames accepted by the source filter.
    /// </summary>
    /// <value>
    /// The pixel format, or <see cref="PixelFormat.None"/> if the option could not be read.
    /// </value>
    public PixelFormat PixelFormat
    {
        get => TryGetOption("pix_fmt", out PixelFormat format).IsError ? PixelFormat.None : format;
        set => SetOption("pix_fmt", value);
    }

    /// <summary>
    /// Gets or sets the height of the video frames in pixels.
    /// </summary>
    /// <value>
    /// The frame height in pixels, or <c>0</c> if the option could not be read.
    /// </value>
    public int Height
    {
        get => TryGetOption("height", out int h).IsError ? 0 : h;
        set => SetOption("height", value);
    }

    /// <summary>
    /// Gets or sets the width of the video frames in pixels.
    /// </summary>
    /// <value>
    /// The frame width in pixels, or <c>0</c> if the option could not be read.
    /// </value>
    public int Width
    {
        get => TryGetOption("width", out int w).IsError ? 0 : w;
        set => SetOption("width", value);
    }

    /// <summary>
    /// Gets or sets the dimensions of the video frames.
    /// </summary>
    /// <value>
    /// A tuple containing the width and height of the video frames.
    /// </value>
    public (int Width, int Height) Size
    {
        get => TryGetOption("video_size", out (int, int) s).IsError ? (0, 0) : s;
        set => SetOption("video_size", value);
    }

    /// <summary>
    /// Gets or sets the time base of the video frames.
    /// </summary>
    /// <value>
    /// The time base used for timestamps of frames submitted to the source.
    /// </value>
    public Rational TimeBase
    {
        get => TryGetOption("time_base", out Rational h).IsError ? 0 : h;
        set => SetOption("time_base", value);
    }

    /// <summary>
    /// Gets or sets the frame rate of the video source.
    /// </summary>
    /// <value>
    /// The frame rate of the video source.
    /// </value>
    public Rational FrameRate
    {
        get => TryGetOption("frame_rate", out Rational h).IsError ? 0 : h;
        set => SetOption("frame_rate", value);
    }

    /// <summary>
    /// Gets or sets the color space of the video frames.
    /// </summary>
    /// <value>
    /// The color space, or <see cref="ColorSpace.Unspecified"/> if the option
    /// could not be read.
    /// </value>
    public ColorSpace ColorSpace
    {
        get => TryGetOption("colorspace", out int v).IsError ? ColorSpace.Unspecified : (ColorSpace)v;
        set => SetOption("colorspace", (int)value);
    }

    /// <summary>
    /// Gets or sets the color range of the video frames.
    /// </summary>
    /// <value>
    /// The color range, or <see cref="ColorRange.Unspecified"/> if the option
    /// could not be read.
    /// </value>
    public ColorRange ColorRange
    {
        get => TryGetOption("range", out int v).IsError ? ColorRange.Unspecified : (ColorRange)v;
        set => SetOption("range", (int)value);
    }

    /// <summary>
    /// Gets or sets the alpha mode of the video frames.
    /// </summary>
    /// <value>
    /// The alpha mode, or <see cref="AlphaMode.Unspecified"/> if the option
    /// could not be read.
    /// </value>
    public AlphaMode AlphaMode
    {
        get => TryGetOption("alpha_mode", out int v).IsError ? AlphaMode.Unspecified : (AlphaMode)v;
        set => SetOption("alpha_mode", (int)value);
    }

    /// <summary>
    /// Gets or sets the sample aspect ratio of the video frames.
    /// </summary>
    /// <value>
    /// The sample aspect ratio of the video frames.
    /// </value>
    public Rational PixelAspectRation
    {
        get => TryGetOption("sar", out Rational h).IsError ? 0 : h;
        set => SetOption("sar", value);
    }

    /// <summary>
    /// Returns a string representation of the video format.
    /// </summary>
    /// <returns>
    /// A string containing the video dimensions and pixel format.
    /// </returns>
    public override string ToString() => $"{Width}:{Height}@{PixelFormat}";
}

