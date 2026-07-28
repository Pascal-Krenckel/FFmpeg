using FFmpeg.Collections;
using FFmpeg.Utils;

namespace FFmpeg.Filters;

public unsafe partial class FilterContext
{

    /// <summary>
    /// Allocates a filter context without initializing it.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="filter">
    /// The filter to allocate.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// A newly allocated <see cref="FilterContext"/>, or <see langword="null"/> if
    /// the allocation failed.
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="Create(string, Filter, string?, FilterGraph)"/>, this
    /// method only allocates the filter context. Before the filter can be used,
    /// it must be initialized by calling one of the <see cref="Init()"/> overloads.
    /// This overload is useful when filter options must be configured before
    /// initialization.
    /// </remarks>
    public static FilterContext? Allocate(string name, Filter filter, FilterGraph graph)
    {
        AutoGen._AVFilterContext* context = ffmpeg.avfilter_graph_alloc_filter(graph.graph, filter.filter, name);
        return context == null ? null : new(context);
    }

    /// <summary>
    /// Creates and initializes a filter within the specified filter graph.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="filter">
    /// The filter to create.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized <see cref="FilterContext"/>.
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="Allocate"/>, this method both allocates and initializes
    /// the filter. After the method returns successfully, the filter is ready to
    /// be linked into a filter graph and used.
    /// </remarks>
    public static FilterContext Create(string name, Filter filter, FilterGraph graph)
    {
        AutoGen._AVFilterContext* context;
        ((AVResult32)ffmpeg.avfilter_graph_create_filter(&context, filter.filter, name, default(string), null, graph.graph)).ThrowIfError();
        return new(context);
    }

    /// <summary>
    /// Creates and initializes a filter within the specified filter graph.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="filter">
    /// The filter to create.
    /// </param>
    /// <param name="args">
    /// An optional filter argument string, or <see langword="null"/> to use the
    /// filter's default settings.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized <see cref="FilterContext"/>.
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="Allocate"/>, this method both allocates and initializes
    /// the filter. After the method returns successfully, the filter is ready to
    /// be linked into a filter graph and used.
    /// </remarks>
    public static FilterContext Create(string name, Filter filter, string? args, FilterGraph graph)
    {
        AutoGen._AVFilterContext* context;
        ((AVResult32)ffmpeg.avfilter_graph_create_filter(&context, filter.filter, name, args, null, graph.graph)).ThrowIfError();
        return new(context);
    }

    /// <summary>
    /// Creates and initializes a filter using the specified dictionary of options.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="filter">
    /// The filter to create.
    /// </param>
    /// <param name="dictionary">
    /// The dictionary containing the filter options.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized <see cref="FilterContext"/>.
    /// </returns>
    /// <remarks>
    /// This method allocates the filter, applies the specified options during
    /// initialization, and returns the initialized filter context.
    /// </remarks>
    public static FilterContext Create(string name, Filter filter, AVDictionary dictionary, FilterGraph graph)
    {
        FilterContext? filterContext = Allocate(name, filter, graph);
        filterContext!.Init(dictionary).ThrowIfError();
        return filterContext;
    }

    /// <summary>
    /// Creates and initializes a filter using the specified dictionary of options.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="filter">
    /// The filter to create.
    /// </param>
    /// <param name="dictionary">
    /// A dictionary containing the filter options.
    /// After the method returns, the dictionary contains only the options that
    /// were not recognized by the filter.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized <see cref="FilterContext"/>.
    /// </returns>
    /// <remarks>
    /// This method allocates the filter, initializes it using the specified
    /// options, and returns the initialized filter context.
    /// </remarks>
    public static FilterContext Create(string name, Filter filter, IDictionary<string, string> dictionary, FilterGraph graph)
    {
        FilterContext? filterContext = Allocate(name, filter, graph);
        filterContext!.Init(dictionary).ThrowIfError();
        return filterContext;
    }

    /// <summary>
    /// Creates and initializes a filter using the specified multi-value dictionary
    /// of options.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="filter">
    /// The filter to create.
    /// </param>
    /// <param name="dictionary">
    /// The multi-value dictionary containing the filter options.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized <see cref="FilterContext"/>.
    /// </returns>
    /// <remarks>
    /// This method allocates the filter, initializes it using the specified
    /// options, and returns the initialized filter context.
    /// </remarks>
    public static FilterContext Create(string name, Filter filter, AVMultiDictionary dictionary, FilterGraph graph)
    {
        FilterContext? filterContext = Allocate(name, filter, graph);
        filterContext!.Init(dictionary).ThrowIfError();
        return filterContext;
    }

}
