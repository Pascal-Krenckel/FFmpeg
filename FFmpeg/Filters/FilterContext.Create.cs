using FFmpeg.AutoGen;
using FFmpeg.Collections;
using FFmpeg.Utils;
using System.Reflection;

namespace FFmpeg.Filters;

public unsafe partial class FilterContext
{

    internal static _AVFilterContext* AllocateInternal(string name, Filter filter, FilterGraph graph)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (filter.filter == null)
            throw new ArgumentException("The filter is not set.", nameof(filter));
        _AVFilterContext* context = ffmpeg.avfilter_graph_alloc_filter(graph.graph, filter.filter, name);
        return context != null ? context : throw new OutOfMemoryException($"Could not allocate the filter '{name}'.");
    }

    /// <summary>
    /// Runs <paramref name="initialize"/> (setting options and calling <see cref="Init()"/>) on a newly
    /// allocated filter. If it throws, the filter is removed from its graph again, like
    /// <c>avfilter_graph_create_filter</c> does; otherwise the uninitialized filter would remain in the
    /// graph and make <see cref="FilterGraph.Config"/> fail.
    /// </summary>
    internal static T InitializeOrDelete<T>(T context, Action<T> initialize) where T : FilterContext
    {
        try
        {
            initialize(context);
            return context;
        }
        catch
        {
            context.Delete();
            throw;
        }
    }

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
        => new(AllocateInternal(name, filter, graph));

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
    public static FilterContext Create(string name, Filter filter, FilterGraph graph) => Create(name, filter, default(string), graph);

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
        => InitializeOrDelete(new FilterContext(AllocateInternal(name, filter, graph)), c => c.Init(dictionary).ThrowIfError());

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
        => InitializeOrDelete(new FilterContext(AllocateInternal(name, filter, graph)), c => c.Init(dictionary).ThrowIfError());

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
        => InitializeOrDelete(new FilterContext(AllocateInternal(name, filter, graph)), c => c.Init(dictionary).ThrowIfError());

    /// <summary>
    /// Creates a managed wrapper of the specified filter context type for the current native filter context.
    /// </summary>
    /// <typeparam name="T">
    /// The managed filter context type to create.
    /// </typeparam>
    /// <returns>
    /// A managed wrapper of type <typeparamref name="T"/> referencing the current native filter context.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// <typeparamref name="T"/> does not provide a constructor accepting an <c>_AVFilterContext*</c>, or the
    /// native filter is not the filter <typeparamref name="T"/> wraps (e.g. <c>As&lt;AudioFormat&gt;()</c> on a
    /// <c>scale</c> filter).
    /// </exception>
    /// <remarks>
    /// The typed filter classes of this library check the native filter. A custom subclass of
    /// <see cref="FilterContext"/> is only checked if its constructor does so.
    /// </remarks>
    public unsafe T As<T>() where T : FilterContext
    {
        if (this is T typed)
            return typed;
        ConstructorInfo[] constructors = typeof(T).GetConstructors(
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        ConstructorInfo constructor = constructors.FirstOrDefault(c =>
            c.GetParameters() is [{ ParameterType: Type type }] && type == typeof(_AVFilterContext*))
            ?? throw new InvalidCastException($"{typeof(T).Name} cannot wrap a filter context.");
        try
        {
            return (T)constructor.Invoke([System.Reflection.Pointer.Box(context, typeof(_AVFilterContext*))]);
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            // The typed constructor throws InvalidCastException if the filter does not match T.
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

}
