using FFmpeg.Images;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Filters;

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
    /// Creates a video buffer source filter using the specified video format.
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
    public static FilterContext CreateSource(string name, int width, int height, PixelFormat format, Rational timeBase, FilterGraph graph)
    {
        FilterContext? context = FilterContext.Allocate(name, Filter.VideoBufferSource, graph) ?? throw new ArgumentNullException();
        context.SetOption("pix_fmt", (AutoGen._AVPixelFormat)format).ThrowIfError();
        context.SetOption("video_size", width, height).ThrowIfError();
        context.SetOption("time_base", timeBase).ThrowIfError();
        context.Init().ThrowIfError();
        return context;
    }


    /// <inheritdoc cref="AudioFilters.CreateSource(string, BufferSrcParameters, FilterGraph)"/>
    public static FilterContext CreateSource(string name, BufferSrcParameters parameters, FilterGraph graph) => AudioFilters.CreateSource(name, parameters, graph);




    /// <inheritdoc cref="AudioFilters.CreateSource(string, Codecs.CodecContext, FilterGraph)"/>
    public static FilterContext CreateSource(string name, Codecs.CodecContext ctx, FilterGraph graph) => AudioFilters.CreateSource(name,ctx, graph);


    /// <inheritdoc cref="AudioFilters.CreateSource(string, Formats.AVStream, FilterGraph)" />
    public static FilterContext CreateSource(string name, Formats.AVStream stream, FilterGraph graph) => AudioFilters.CreateSource(name,stream, graph);

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
    /// Creates a <c>framepack</c> filter.
    /// </summary>
    /// <param name="name">The filter instance name.</param>
    /// <param name="format">The stereoscopic frame packing format.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter context.</returns>
    public static FilterContext CreateFramePack(string name, FramePackFormat format, FilterGraph graph)
        => CreateFramePack(name, format.ToFFmpegString(), graph);

    /// <summary>
    /// Creates a <c>framepack</c> filter.
    /// </summary>
    /// <param name="name">The filter instance name.</param>
    /// <param name="format">The FFmpeg frame packing format string.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter context.</returns>
    public static FilterContext CreateFramePack(string name, string format, FilterGraph graph)
    {
        FilterContext context = FilterContext.Allocate(name, Filter.FramePack, graph)
            ?? throw new ArgumentNullException(nameof(Filter.FramePack));

        context.Init(format).ThrowIfError();
        return context;
    }
}
