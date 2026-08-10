using FFmpeg.AutoGen;
using FFmpeg.Logging;
using FFmpeg.Options;
using FFmpeg.Unsafe;
using FFmpeg.Utils;
using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;
/// <summary>
/// Represents a filter graph that contains a collection of connected filters.
/// </summary>
/// <remarks>
/// A <see cref="FilterGraph"/> owns the <see cref="FilterContext"/> instances
/// created within it and manages the links between them. Once configured, the
/// graph can be used to process audio or video frames.
/// </remarks>
public sealed unsafe partial class FilterGraph : ILoggingContext, IDisposable, IAVPointer<_AVFilterGraph>
{
    internal AutoGen._AVFilterGraph* graph;
    unsafe void* ILoggingContext.AVClassPointer => graph;

    _AVFilterGraph* IAVPointer<_AVFilterGraph>.Pointer => graph;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterGraph"/> class from an
    /// existing FFmpeg filter graph.
    /// </summary>
    /// <param name="filterGraph">
    /// A pointer to the underlying FFmpeg filter graph.
    /// </param>
    internal FilterGraph(AutoGen._AVFilterGraph* filterGraph) => graph = filterGraph;

    /// <summary>
    /// Gets or sets the maximum number of threads used when processing the filter graph.
    /// </summary>
    /// <remarks>
    /// A value of <c>0</c> allows FFmpeg to choose an appropriate number of threads.
    /// </remarks>
    public int Threads
    {
        get => graph->nb_threads;
        set => graph->nb_threads = value;
    }

    /// <summary>
    /// Gets the number of filters contained in the graph.
    /// </summary>
    public int Count => (int)graph->nb_filters;

    /// <summary>
    /// Gets the filter at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the filter.
    /// </param>
    /// <returns>
    /// The <see cref="FilterContext"/> at the specified index.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is outside the bounds of the graph.
    /// </exception>
    public FilterContext this[int index] =>
        index < 0 || index >= Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : new(graph->filters[index]);

    #region Dispose

    private bool disposedValue;

    /// <summary>
    /// Releases the unmanaged resources used by the filter graph and optionally releases any managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources as well; otherwise, <see langword="false"/>.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Optionally clean up managed resources here.
            }

            AutoGen._AVFilterGraph* graph = this.graph;
            ffmpeg.avfilter_graph_free(&graph);
            this.graph = null;
            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer for the <see cref="FilterGraph"/> class.
    /// </summary>
    ~FilterGraph()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Releases all resources used by this <see cref="FilterGraph"/> instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Connects an output pad of one filter to an input pad of another filter.
    /// </summary>
    /// <param name="src">
    /// The source filter.
    /// </param>
    /// <param name="srcpad">
    /// The zero-based output pad index on <paramref name="src"/>.
    /// </param>
    /// <param name="dst">
    /// The destination filter.
    /// </param>
    /// <param name="dstpad">
    /// The zero-based input pad index on <paramref name="dst"/>.
    /// </param>
    /// <returns>
    /// The result of the link operation.
    /// </returns>
    public AVResult32 Link(FilterContext src, int srcpad, FilterContext dst, int dstpad) =>
        ffmpeg.avfilter_link(src.context, (uint)srcpad, dst.context, (uint)dstpad);

    /// <summary>
    /// Connects the first output pad of one filter to the first input pad of another filter.
    /// </summary>
    /// <param name="src">
    /// The source filter.
    /// </param>
    /// <param name="dst">
    /// The destination filter.
    /// </param>
    /// <returns>
    /// The result of the link operation.
    /// </returns>
    public AVResult32 Link(FilterContext src, FilterContext dst) =>
        ffmpeg.avfilter_link(src.context, 0, dst.context, 0);

    /// <summary>
    /// Parses a filter graph description and connects it to the specified input and output filters.
    /// </summary>
    /// <param name="input">
    /// The input filter list, or <see langword="null"/> if no explicit input list should be supplied.
    /// </param>
    /// <param name="filters">
    /// The filter graph description.
    /// </param>
    /// <param name="output">
    /// The output filter list, or <see langword="null"/> if no explicit output list should be supplied.
    /// </param>
    /// <returns>
    /// The result of the parse operation.
    /// </returns>
    /// <remarks>
    /// This method wraps FFmpeg's <c>avfilter_graph_parse_ptr()</c>. Any unconnected
    /// inputs or outputs remaining after parsing are written back into the supplied
    /// <see cref="FilterInOutList"/> instances.
    /// </remarks>
    public AVResult32 ParseAndLink(FilterInOutList? input, string filters, FilterInOutList? output)
    {
        AutoGen._AVFilterInOut* @in = input != null ? input.Head : null;
        AutoGen._AVFilterInOut* @out = output != null ? output.Head : null;

        AutoGen._AVFilterInOut** inPtrs = input != null ? &@in : null;
        AutoGen._AVFilterInOut** outPtrs = output != null ? &@out : null;
        int res = ffmpeg.avfilter_graph_parse_ptr(graph, filters, inPtrs, outPtrs, null);
        input?.Head = @in;
        output?.Head = @out;

        return res;
    }

    /// <summary>
    /// Parses a filter graph description and returns any unconnected inputs and outputs.
    /// </summary>
    /// <param name="input">
    /// When this method returns, contains the unconnected input filters.
    /// </param>
    /// <param name="filters">
    /// The filter graph description.
    /// </param>
    /// <param name="output">
    /// When this method returns, contains the unconnected output filters.
    /// </param>
    /// <returns>
    /// The result of the parse operation.
    /// </returns>
    /// <remarks>
    /// This method wraps FFmpeg's <c>avfilter_graph_parse_ptr()</c>.
    /// </remarks>
    public AVResult32 ParseAndLink(out FilterInOutList input, string filters, out FilterInOutList output)
    {
        input = [];
        output = [];
        AutoGen._AVFilterInOut* @in = input.Head;
        AutoGen._AVFilterInOut* @out = output.Head;

        int res = ffmpeg.avfilter_graph_parse_ptr(graph, filters, &@in, &@out, null);
        input.Head = @in;
        output.Head = @out;
        return res;
    }

    /// <summary>
    /// Parses a filter graph description without linking it to existing filters.
    /// </summary>
    /// <param name="inputs">
    /// When this method returns, contains the unconnected input filters.
    /// </param>
    /// <param name="filters">
    /// The filter graph description.
    /// </param>
    /// <param name="outputs">
    /// When this method returns, contains the unconnected output filters.
    /// </param>
    /// <returns>
    /// The result of the parse operation.
    /// </returns>
    /// <remarks>
    /// This method wraps FFmpeg's <c>avfilter_graph_parse2()</c>.
    /// </remarks>
    public AVResult32 Parse(out FilterInOutList inputs, string filters, out FilterInOutList outputs)
    {
        inputs = [];
        outputs = [];
        AutoGen._AVFilterInOut* @in = inputs.Head;
        AutoGen._AVFilterInOut* @out = outputs.Head;

        int res = ffmpeg.avfilter_graph_parse2(graph, filters, &@in, &@out);
        inputs.Head = @in;
        outputs.Head = @out;
        return res;
    }

    /// <summary>
    /// Validates and configures all links in the filter graph.
    /// </summary>
    /// <returns>
    /// The result of the configuration operation.
    /// </returns>
    /// <remarks>
    /// This method should be called after all filters have been created and linked,
    /// and before frames are processed through the graph.
    /// </remarks>
    public AVResult32 Config() => ffmpeg.avfilter_graph_config(graph, null);

    /// <summary>
    /// Returns a textual representation of the current filter graph.
    /// </summary>
    /// <returns>
    /// A human-readable description of the graph.
    /// </returns>
    /// <remarks>
    /// This method is primarily intended for debugging and diagnostics.
    /// </remarks>
    public string Dump()
    {
        byte* buff = ffmpeg.avfilter_graph_dump(graph, (byte*)null);
        if (buff == null)
            return "Error";
        string dump = Marshal.PtrToStringUTF8((nint)buff);
        ffmpeg.av_free(buff);
        return dump;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the filters in the graph.
    /// </summary>
    /// <returns>An enumerator over the graph's filters.</returns>
    public IEnumerator<FilterContext> GetEnumerator() => new FilterEnumerator(this);


    /// <summary>
    /// Gets a read-only list view over all filters contained in the graph.
    /// </summary>
    public IReadOnlyList<FilterContext> Filters => new FilterGraphList(this);

    /// <summary>
    /// Gets all buffer source filters contained in the graph.
    /// </summary>
    /// <remarks>
    /// These are the filters that provide input frames to the graph.
    /// </remarks>
    public IEnumerable<FilterContext> InputFilters =>
        Filters.Where(f => f.Filter == FFmpeg.Filters.Filter.VideoBufferSource || f.Filter == FFmpeg.Filters.Filter.AudioBufferSource);

    /// <summary>
    /// Gets all buffer sink filters contained in the graph.
    /// </summary>
    /// <remarks>
    /// These are the filters that receive processed output frames from the graph.
    /// </remarks>
    public IEnumerable<FilterContext> OutputFilters =>
        Filters.Where(f => f.Filter == FFmpeg.Filters.Filter.VideoBufferSink || f.Filter == FFmpeg.Filters.Filter.AudioBufferSink);

    /// <summary>
    /// Gets all source filters contained in the graph.
    /// </summary>
    /// <remarks>
    /// Source filters have no input pads and generate media for the graph.
    /// </remarks>
    public IEnumerable<FilterContext> SourceFilters =>
        Filters.Where(f => f.Filter.IsSourceFilter);

    /// <summary>
    /// Gets all sink filters contained in the graph.
    /// </summary>
    /// <remarks>
    /// Sink filters have no output pads and consume media from the graph.
    /// </remarks>
    public IEnumerable<FilterContext> SinkFilters =>
        Filters.Where(f => f.Filter.IsSinkFilter);

    /// <summary>
    /// Gets all filters that have at least one unconnected output pad.
    /// </summary>
    public IEnumerable<FilterContext> UnlinkedOutFilters
    {
        get
        {
            foreach (FilterContext filter in Filters)
            {
                for (int i = 0; i < filter.OutputCount; i++)
                {
                    if (filter.GetOutputFilterLink(i) == null)
                    {
                        yield return filter;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets all filters that have at least one unconnected input pad.
    /// </summary>
    public IEnumerable<FilterContext> UnlinkedInFilters
    {
        get
        {
            foreach (FilterContext filter in Filters)
            {
                for (int i = 0; i < filter.InputCount; i++)
                {
                    if (filter.GetInputFilterLink(i) == null)
                    {
                        yield return filter;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Inserts a filter into an existing link between two filters.
    /// </summary>
    /// <param name="link">
    /// The link to split.
    /// </param>
    /// <param name="filter">
    /// The filter to insert.
    /// </param>
    /// <param name="srcFilterPad">
    /// The output pad index on the inserted filter.
    /// </param>
    /// <param name="destFilterPad">
    /// The input pad index on the inserted filter.
    /// </param>
    /// <returns>
    /// The result of the insertion operation.
    /// </returns>
    public AVResult32 Insert(FilterLink link, FilterContext filter, int srcFilterPad, int destFilterPad) =>
        ffmpeg.avfilter_insert_filter(link.link, filter.context, (uint)srcFilterPad, (uint)destFilterPad);

    /// <summary>
    /// Finds a filter by its instance name.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <returns>
    /// The matching filter, or <see langword="null"/> if no filter with the specified name exists.
    /// </returns>
    public FilterContext? FindFilter(string name)
    {
        _AVFilterContext* ptr = ffmpeg.avfilter_graph_get_filter(graph, name);
        return ptr == null ? null : new(ptr);
    }

    /// <summary>
    /// Creates a unconfigured copy of the filter graph.
    /// </summary>
    /// <returns>
    /// A new <see cref="FilterGraph"/> containing equivalent filters and connections.
    /// </returns>
    /// <remarks>
    /// The copied graph contains newly allocated filter contexts configured with the
    /// same options and topology as the original graph.
    ///  There is not ffmpeg function. 
    ///  This function tries to recreate the filter graph with the same paramters.
    ///  Buffer sinks and filter contexts named auto_* are not configured.
    /// </remarks>
    public FilterGraph Copy()
    {
        FilterGraph copy = FilterGraph.Allocate();
        foreach (FilterContext context in this)
        {
            using Collections.AVMultiDictionary dictionary = [];
            if(!context.Name.StartsWith("auto_") && (context.Filter != Filter.VideoBufferSink && (context.Filter != Filter.AudioBufferSink)))
            foreach (Option o in context.GetOptions(true))
            {
                if (!context.TryGetOption(o, out string? value, true).IsError && !string.IsNullOrEmpty(value))
                    dictionary.Add(o.Name, value!);
            }

            _ = FilterContext.Create(context.Name, context.Filter, dictionary, copy);
        }
        foreach (FilterContext context in this)
        {
            foreach (FilterLink l in context.OutputFilterLinks)
            {
                copy.Link(
                        copy.FindFilter(l.SourceContext.Name!)!,
                        l.SourcePadIndex,
                        copy.FindFilter(l.DestinationContext.Name!)!,
                        l.DestinationPadIndex)
                    .ThrowIfError();
            }
        }
        return copy;
    }

    /// <summary>
    /// Creates a copy of the filter graph and disposed the old one.
    /// </summary>
    public void Flush()
    {
        using var copy = Copy();
        var ptr = graph;
        graph = copy.graph;
        copy.graph = ptr;
    }

    private class FilterEnumerator(FilterGraph filterGraph) : IEnumerator<FilterContext>
    {
        private readonly FilterGraph filterGraph = filterGraph;
        private int index = -1;

        /// <summary>
        /// Gets the current filter context.
        /// </summary>
        public FilterContext Current => new(filterGraph.graph->filters[index]);

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() { }

        /// <summary>
        /// Advances the enumerator to the next filter.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was advanced; otherwise, <see langword="false"/>.</returns>
        public bool MoveNext() => ++index < filterGraph.Count;

        /// <summary>
        /// Resets the enumerator to the initial position.
        /// </summary>
        public void Reset() => index = -1;
    }

    private class FilterGraphList(FilterGraph filterGraph) : IReadOnlyList<FilterContext>
    {
        private readonly FilterGraph filterGraph = filterGraph;

        /// <summary>
        /// Gets the filter at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the filter.</param>
        public FilterContext this[int index] => filterGraph[index];

        /// <summary>
        /// Gets the number of filters in the graph.
        /// </summary>
        public int Count => filterGraph.Count;

        /// <summary>
        /// Returns an enumerator that iterates through the filters in the graph.
        /// </summary>
        public IEnumerator<FilterContext> GetEnumerator() => filterGraph.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

