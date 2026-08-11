using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Represents a video buffer sink filter that receives video frames from a filter graph.
/// </summary>
/// <remarks>
/// A <see cref="VideoBufferSink"/> is the exit point for video frames from a filter graph.
/// Frames can be retrieved using <see cref="ReceiveFrame(AVFrame)"/> after the filter graph
/// has been configured and initialized.
/// </remarks>
public unsafe class VideoBufferSink : FilterContext, IBufferSink
{
    internal VideoBufferSink(AutoGen._AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Receives the next available video frame from the buffer sink filter.
    /// </summary>
    /// <param name="frame">
    /// The destination frame that receives the filtered video data.
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
        if (frame == null)
            throw new ArgumentNullException(nameof(frame), "The provided argument was null");

        frame.Unreference();

        int res = ffmpeg.av_buffersink_get_frame(context, frame.Frame);

        frame.TimeBase = ffmpeg.av_buffersink_get_time_base(context);
        frame.BestEffortTimestamp = frame.PresentationTimestamp;

        return res;
    }

    /// <summary>
    /// Allocates an uninitialized video buffer sink filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The allocated, uninitialized video buffer sink filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated.
    /// </exception>
    public static VideoBufferSink Allocate(string name, FilterGraph graph) =>
        new(AllocateInternal(name, Filter.VideoBufferSink, graph));

    /// <summary>
    /// Creates and initializes a video buffer sink filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized video buffer sink filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated or initialized.
    /// </exception>
    public static VideoBufferSink Create(string name, FilterGraph graph)
    {
        VideoBufferSink context = Allocate(name, graph);
        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Gets the time base of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The time base used for timestamps of frames received from the sink.
    /// </value>
    public Rational TimeBase =>
        ffmpeg.av_buffersink_get_time_base(context);

    /// <summary>
    /// Gets the pixel format of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The pixel format of the frames received from the sink.
    /// </value>
    public PixelFormat PixelFormat =>
        (PixelFormat)ffmpeg.av_buffersink_get_format(context);

    /// <summary>
    /// Gets the frame rate of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The frame rate of the frames received from the sink.
    /// </value>
    public Rational FrameRate =>
        ffmpeg.av_buffersink_get_frame_rate(context);

    /// <summary>
    /// Gets the width of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The frame width in pixels.
    /// </value>
    public int Width =>
        ffmpeg.av_buffersink_get_w(context);

    /// <summary>
    /// Gets the height of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The frame height in pixels.
    /// </value>
    public int Height =>
        ffmpeg.av_buffersink_get_h(context);

    /// <summary>
    /// Gets the color space of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The color space of the frames received from the sink.
    /// </value>
    public ColorSpace ColorSpace =>
        (ColorSpace)ffmpeg.av_buffersink_get_colorspace(context);

    /// <summary>
    /// Gets the sample aspect ratio of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The sample aspect ratio of the frames received from the sink.
    /// </value>
    public Rational PixelAspectRatio =>
        ffmpeg.av_buffersink_get_sample_aspect_ratio(context);

    /// <summary>
    /// Gets the alpha mode of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The alpha mode of the frames received from the sink.
    /// </value>
    public AlphaMode AlphaMode =>
        (AlphaMode)ffmpeg.av_buffersink_get_alpha_mode(context);

    /// <summary>
    /// Gets the color range of frames produced by the buffer sink filter.
    /// </summary>
    /// <value>
    /// The color range of the frames received from the sink.
    /// </value>
    public ColorRange ColorRange =>
        (ColorRange)ffmpeg.av_buffersink_get_color_range(context);
}