using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.Filters;

public sealed unsafe partial class FilterGraph
{
    /// <summary>
    /// Creates a filter graph from the specified filter graph description.
    /// </summary>
    /// <param name="filter">
    /// The FFmpeg filter graph description to parse.
    /// </param>
    /// <returns>
    /// The newly created and parsed <see cref="FilterGraph"/>.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// Thrown if the filter graph could not be parsed.
    /// </exception>
    /// <remarks>
    /// This method parses the supplied filter graph description using
    /// <c>avfilter_graph_parse_ptr()</c>. Any unlinked inputs or outputs are
    /// discarded. Use <see cref="Create(out FilterInOutList?, string, out FilterInOutList?)"/>
    /// if access to unlinked endpoints is required.
    /// </remarks>
    public static FilterGraph Create(string filter) => TryCreate(filter, out FilterGraph? graph) switch
    {
        AVResult32 res when res.IsError => throw new FFmpeg.Exceptions.FFmpegException(res),
        _ => graph!,
    };


    /// <summary>
    /// Attempts to create a filter graph from the specified filter graph description.
    /// </summary>
    /// <param name="inputs">
    /// When this method returns successfully, contains any unlinked input endpoints
    /// remaining after parsing; otherwise, <see langword="null"/>.
    /// </param>
    /// <param name="filter">
    /// The FFmpeg filter graph description to parse.
    /// </param>
    /// <param name="outputs">
    /// When this method returns successfully, contains any unlinked output endpoints
    /// remaining after parsing; otherwise, <see langword="null"/>.
    /// </param>
    /// <param name="filterGraph">
    /// When this method returns successfully, contains the created
    /// <see cref="FilterGraph"/>; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    public static AVResult32 TryCreate(out FilterInOutList? inputs, string filter, out FilterInOutList? outputs, out FilterGraph? filterGraph)
    {
        filterGraph = Allocate();
        _AVFilterInOut* @in;
        _AVFilterInOut* @out;
        AVResult32 res = ffmpeg.avfilter_graph_parse_ptr(filterGraph.graph, filter, &@in, &@out, null);
        if (res.IsError)
        {
            ffmpeg.avfilter_inout_free(&@in);
            ffmpeg.avfilter_inout_free(&@out);
            filterGraph.Dispose();
            inputs = null;
            outputs = null;
        }
        else
        {
            inputs = new FilterInOutList(@in);
            outputs = new FilterInOutList(@out);
        }
        return res;

    }

    /// <summary>
    /// Creates a filter graph from the specified filter graph description.
    /// </summary>
    /// <param name="inputs">
    /// When this method returns, contains any unlinked input endpoints remaining
    /// after parsing.
    /// </param>
    /// <param name="filter">
    /// The FFmpeg filter graph description to parse.
    /// </param>
    /// <param name="outputs">
    /// When this method returns, contains any unlinked output endpoints remaining
    /// after parsing.
    /// </param>
    /// <returns>
    /// The newly created <see cref="FilterGraph"/>.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// Thrown if the filter graph could not be parsed.
    /// </exception>
    /// <remarks>
    /// This overload preserves any unlinked input and output endpoints returned by
    /// FFmpeg, allowing them to be connected later.
    /// </remarks>
    public static FilterGraph Create(out FilterInOutList? inputs, string filter, out FilterInOutList? outputs)
    {
        AVResult32 res = TryCreate(out inputs, filter, out outputs, out FilterGraph? graph);
        return res.IsError ? throw new FFmpeg.Exceptions.FFmpegException(res) : graph!;
    }

    /// <summary>
    /// Allocates a new, empty filter graph.
    /// </summary>
    /// <returns>
    /// A new unconfigured <see cref="FilterGraph"/> instance.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// FFmpeg was unable to allocate the filter graph.
    /// </exception>
    /// <remarks>
    /// Unlike the <c>Create</c> methods, this method only allocates the filter graph.
    /// No filters are created or parsed.
    /// </remarks>
    public static FilterGraph Allocate()
    {
        AutoGen._AVFilterGraph* graph = ffmpeg.avfilter_graph_alloc();
        return graph == null ? throw new OutOfMemoryException() : new(graph);

    }

    /// <summary>
    /// Attempts to create a filter graph from the specified filter graph description.
    /// </summary>
    /// <param name="filter">
    /// The FFmpeg filter graph description to parse.
    /// </param>
    /// <param name="graph">
    /// When this method returns successfully, contains the created
    /// <see cref="FilterGraph"/>; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The result of the parse operation.
    /// </returns>
    /// <remarks>
    /// This overload parses the graph description but discards any unlinked input
    /// or output endpoints. Use
    /// <see cref="TryCreate(out FilterInOutList?, string, out FilterInOutList?, out FilterGraph?)"/>
    /// if access to those endpoints is required.
    /// </remarks>
    public static AVResult32 TryCreate(string filter, out FilterGraph? graph)
    {
        graph = Allocate();
        AVResult32 res = ffmpeg.avfilter_graph_parse_ptr(graph.graph, filter, null, null, null);
        if (res.IsError)
        {
            graph.Dispose();
            graph = null;
            return res;
        }
        return res;
    }
}
