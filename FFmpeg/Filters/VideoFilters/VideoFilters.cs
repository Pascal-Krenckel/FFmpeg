using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Provides factory methods for creating commonly used video filters.
/// </summary>
/// <remarks>
/// This class contains convenience methods for creating and initializing
/// video filter contexts. All methods automatically allocate and initialize
/// the created filter.
/// </remarks>
public static class VideoFilters
{

    /// <summary>
    /// Creates a video scale filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="width">
    /// The output width, in pixels.
    /// </param>
    /// <param name="height">
    /// The output height, in pixels.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized scale filter.
    /// </returns>
    /// <remarks>
    /// The scale filter resizes video frames to the specified dimensions.
    /// </remarks>
    public static FilterContext CreateScale(string name, int width, int height, FilterGraph graph)
    {
        FilterContext? context = FilterContext.Allocate(name, Filter.Scale, graph) ?? throw new ArgumentNullException();
        context.Init($"{width}:{height}").ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates a video frame rate filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="fps">
    /// The desired output frame rate.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized frame rate filter.
    /// </returns>
    /// <remarks>
    /// The FPS filter converts the input stream to the specified output frame
    /// rate by dropping or duplicating frames as needed.
    /// </remarks>
    public static FilterContext CreateFPS(string name, Rational fps, FilterGraph graph)
    {
        FilterContext? context = FilterContext.Allocate(name, Filter.FPS, graph) ?? throw new ArgumentNullException();
        context.Init(fps.ToString()).ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates a video format filter context that converts input video frames to the specified pixel format.
    /// </summary>
    /// <param name="name">The name to assign to the filter context.</param>
    /// <param name="format">The pixel format to convert the input video to.</param>
    /// <param name="graph">The <see cref="FilterGraph"/> to which the filter context belongs.</param>
    /// <returns>
    /// A new <see cref="FilterContext"/> configured to convert video frames to the specified pixel format.
    /// </returns>
    public static FilterContext CreateFormat(string name, PixelFormat format, FilterGraph graph)
    {
        FilterContext context = FilterContext.Allocate(name, Filter.VideoFormat, graph)!;
        context.SetOption("pix_fmts", format.ToFFmpegString()).ThrowIfError();
        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates a video format filter context that converts input video frames to one of the specified
    /// pixel formats. When multiple formats are specified, libavfilter selects a format suitable for
    /// the next filter in the filter graph.
    /// </summary>
    /// <param name="name">The name to assign to the filter context.</param>
    /// <param name="graph">The <see cref="FilterGraph"/> to which the filter context belongs.</param>
    /// <param name="formats">
    /// The pixel formats to which the input video may be converted.
    /// </param>
    /// <returns>
    /// A new <see cref="FilterContext"/> configured with the specified pixel formats.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="formats"/> is empty.
    /// </exception>
    public static FilterContext CreateFormat(string name, FilterGraph graph, params ReadOnlySpan<PixelFormat> formats)
    {
        FilterContext context = FilterContext.Allocate(name, Filter.VideoFormat, graph)!;
        if (formats.IsEmpty)
            throw new ArgumentException(nameof(formats));
        context.Init($"pix_fmts={string.Join('|', formats, PixelFormatExtensions.ToFFmpegString)}").ThrowIfError();
        return context;
    }


}
