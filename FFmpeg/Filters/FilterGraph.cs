using FFmpeg.AutoGen;
using FFmpeg.Options;
using FFmpeg.Utils;
using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;
/// <summary>
/// Represents an FFmpeg filter graph, which contains multiple filter contexts and manages their connections.
/// </summary>
public sealed unsafe class FilterGraph : IDisposable
{
    internal AutoGen._AVFilterGraph* graph;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterGraph"/> class using the given FFmpeg filter graph.
    /// </summary>
    internal FilterGraph(AutoGen._AVFilterGraph* filterGraph) => graph = filterGraph;

    /// <summary>
    /// Allocates and adds a filter to the graph with the given name and filter. The filter is not yet initialized.
    /// </summary>
    public FilterContext? AllocateFilter(string name, Filter filter) => FilterContext.Allocate(name, filter, this);

    /// <summary>
    /// Gets or sets the number of threads used in the filter graph.
    /// </summary>
    public int Threads { get => graph->nb_threads; set => graph->nb_threads = value; }

    /// <summary>
    /// Gets the number of filters currently in the graph.
    /// </summary>
    public int Count => (int)graph->nb_filters;

    /// <summary>
    /// Gets the filter context at the specified index.
    /// </summary>
    /// <param name="index">The index of the filter context.</param>
    public FilterContext this[int index] => index < 0 || index >= Count ? throw new ArgumentOutOfRangeException(nameof(index)) : new(graph->filters[index]);

    /// <summary>
    /// Allocates a new filter graph.
    /// </summary>
    public static FilterGraph Allocate()
    {
        AutoGen._AVFilterGraph* graph = ffmpeg.avfilter_graph_alloc();
        return graph == null ? throw new OutOfMemoryException() : new(graph);

    }

    #region Dispose

    private bool disposedValue;

    /// <summary>
    /// Releases the unmanaged resources used by the filter graph and optionally releases the managed resources.
    /// </summary>
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
    /// Links the source filter context to the destination filter context using the specified pads.
    /// </summary>
    /// <param name="src">The source filter context.</param>
    /// <param name="srcpad">The index of the output pad on the source filter context.</param>
    /// <param name="dst">The destination filter context.</param>
    /// <param name="dstpad">The index of the input pad on the destination filter context.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the link operation.</returns>
    public AVResult32 Link(FilterContext src, int srcpad, FilterContext dst, int dstpad) => ffmpeg.avfilter_link(src.context, (uint)srcpad, dst.context, (uint)dstpad);

    /// <summary>
    /// Links the source filter context to the destination filter context using the first pads.
    /// </summary>
    /// <param name="src">The source filter context.</param>
    /// <param name="dst">The destination filter context.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the link operation.</returns>
    public AVResult32 Link(FilterContext src, FilterContext dst) => ffmpeg.avfilter_link(src.context, 0, dst.context, 0);


    /// <summary>
    /// Parses the given filter graph description and links input and output filter contexts.
    /// </summary>
    /// <param name="input">The input filters.</param>
    /// <param name="filters">The filter graph description.</param>
    /// <param name="output">The output filters.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the parse operation.</returns>
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
    /// Parses the given filter graph description and links input and output filter contexts.
    /// </summary>
    /// <param name="input">The input filters.</param>
    /// <param name="filters">The filter graph description.</param>
    /// <param name="output">The output filters.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the parse operation.</returns>
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
    /// Parses the given filter graph description but doesn't link input and output filter contexts.
    /// </summary>
    /// <param name="inputs">The input filters.</param>
    /// <param name="filters">The filter graph description.</param>
    /// <param name="outputs">The output filters.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the parse operation.</returns>
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

    public static FilterGraph Create(string filter) => TryCreate(filter, out FilterGraph? graph) switch
    {
        AVResult32 res when res.IsError => throw new FFmpeg.Exceptions.FFmpegException(res),
        _ => graph!,
    };

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

    public static FilterGraph Create(out FilterInOutList? inputs, string filter, out FilterInOutList? outputs)
    {
        AVResult32 res = TryCreate(out inputs, filter, out outputs, out FilterGraph? graph);
        return res.IsError ? throw new FFmpeg.Exceptions.FFmpegException(res) : graph!;
    }

    // Init and Init, string may be null if no arguments are needed
    // If you want to that arguments later use CreateFilter(name,filter) instead
    public FilterContext CreateFilter(string name, Filter filter, string? args)
    {
        _AVFilterContext* ctx = null;
        AVResult32 res = ffmpeg.avfilter_graph_create_filter(&ctx, filter.filter, name, args, null, graph);
        res.ThrowIfError();
        return new(ctx);
    }

    // Init and Init
    public FilterContext CreateFilter(string name, Filter filter, Collections.AVDictionary? args)
    {
        _AVFilterContext* ctx = ffmpeg.avfilter_graph_alloc_filter(graph, filter.filter, name);
        if (args == null)
        {
            ((AVResult32)ffmpeg.avfilter_init_dict(ctx, null)).ThrowIfError();
        }
        else
        {
            _AVDictionary* dictionary = args.dictionary;
            AVResult32 res = ffmpeg.avfilter_init_dict(ctx, &dictionary);
            args.dictionary = dictionary; // Update the dictionary in case it was modified
            res.ThrowIfError();
        }
        return new(ctx);

    }

    public FilterContext CreateFilter(string name, Filter filter, Collections.AVMultiDictionary? args)
    {
        _AVFilterContext* ctx = ffmpeg.avfilter_graph_alloc_filter(graph, filter.filter, name);
        if (args == null)
        {
            ((AVResult32)ffmpeg.avfilter_init_dict(ctx, null)).ThrowIfError();
        }
        else
        {
            _AVDictionary* dictionary = args.dictionary;
            AVResult32 res = ffmpeg.avfilter_init_dict(ctx, &dictionary);
            args.dictionary = dictionary; // Update the dictionary in case it was modified
            res.ThrowIfError();
        }
        return new(ctx);

    }
    public FilterContext CreateFilter(string name, Filter filter, IDictionary<string, string> args)
    {
        if (args == null)
            return CreateFilter(name, filter, default(Collections.AVDictionary)!);
        using Collections.AVDictionary dictionary = new(args);
        try
        {
            return CreateFilter(name, filter, dictionary);
        }
        finally
        {
            args.Clear();
            foreach (KeyValuePair<string, string> item in dictionary)
                args.Add(item);
        }
    }

    /// <summary>
    ///  Create a filter, but doesn't initialize it.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public FilterContext CreateFilter(string name, Filter filter)
    {
        _AVFilterContext* ctx = ffmpeg.avfilter_graph_alloc_filter(graph, filter.filter, name);
        return ctx == null ? throw new OutOfMemoryException("Failed to allocate filter context.") : new(ctx);
    }

    /// <summary>
    /// Configures all the links in the filter graph.
    /// </summary>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the configuration.</returns>
    public AVResult32 Config() => ffmpeg.avfilter_graph_config(graph, null);

    /// <summary>
    /// Dumps the current filter graph as a string representation.
    /// </summary>
    public string Dump()
    {
        byte* buff = ffmpeg.avfilter_graph_dump(graph, null);
        if (buff == null)
            return "Error";
        string dump = Marshal.PtrToStringUTF8((nint)buff);
        ffmpeg.av_free(buff);
        return dump;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the filter contexts in the graph.
    /// </summary>
    public IEnumerator<FilterContext> GetEnumerator() => new FilterEnumerator(this);

    public ReadOnlySpan<FilterContext> AsSpan() => new(graph->filters, Count);

    public IReadOnlyList<FilterContext> Filters => new FilterGraphList(this);

    public IEnumerable<FilterContext> InputFilters => Filters.Where(f => f.Filter == FFmpeg.Filters.Filter.VideoBufferSource || f.Filter == FFmpeg.Filters.Filter.AudioBufferSource);
    public IEnumerable<FilterContext> OutputFilters => Filters.Where(f => f.Filter == FFmpeg.Filters.Filter.VideoBufferSink || f.Filter == FFmpeg.Filters.Filter.AudioBufferSink);

    public IEnumerable<FilterContext> SourceFilters => Filters.Where(f => f.Filter.IsSourceFilter);

    public IEnumerable<FilterContext> SinkFilters => Filters.Where(f => f.Filter.IsSinkFilter);

    public IEnumerable<FilterContext> UnlinkedOutFilters
    {
        get
        {
            foreach (FilterContext filter in Filters)
            {
                for (int i = 0; i < filter.OutputCount; i++)
                {
                    if (filter.OutputFilterLinks == null)
                    {
                        yield return filter;
                        break;
                    }
                }
            }
        }
    }

    public IEnumerable<FilterContext> UnlinkedInFilters
    {
        get
        {
            foreach (FilterContext filter in Filters)
            {
                for (int i = 0; i < filter.InputCount; i++)
                {
                    if (filter.InputFilterLinks == null)
                    {
                        yield return filter;
                        break;
                    }
                }
            }
        }
    }


    public AVResult32 Insert(FilterLink link, FilterContext filter, int srcFilterPad, int destFilterPad) => ffmpeg.avfilter_insert_filter(link.link, filter.context, (uint)srcFilterPad, (uint)destFilterPad);

    public FilterContext? FindFilter(string name)
    {
        _AVFilterContext* ptr = ffmpeg.avfilter_graph_get_filter(graph, name);
        return ptr == null ? null : new(ptr);
    }

    private class FilterEnumerator : IEnumerator<FilterContext>
    {
        private readonly FilterGraph filterGraph;
        private int index = -1;
        public FilterEnumerator(FilterGraph filterGraph) => this.filterGraph = filterGraph;

        public FilterContext Current => new(filterGraph.graph->filters[index]);

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext() => ++index < filterGraph.Count;

        public void Reset() => index = -1;
    }

    private class FilterGraphList : IReadOnlyList<FilterContext>
    {
        private readonly FilterGraph filterGraph;
        public FilterGraphList(FilterGraph filterGraph) => this.filterGraph = filterGraph;
        public FilterContext this[int index] => filterGraph[index];
        public int Count => filterGraph.Count;
        public IEnumerator<FilterContext> GetEnumerator() => filterGraph.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public FilterGraph Copy()
    {
        FilterGraph copy = FilterGraph.Allocate();
        foreach (FilterContext context in this)
        {
            using Collections.AVMultiDictionary dictionary = [];
            foreach (Option o in context.GetOptions(true))
            {
                if (!context.TryGetOption(o, out string? value, true).IsError && !string.IsNullOrEmpty(value))
                    dictionary.Add(o.Name, value!);
            }

            _ = copy.CreateFilter(context.Name, context.Filter, dictionary);
        }
        foreach (FilterContext context in this)
        {
            foreach (FilterLink l in context.OutputFilterLinks)
            {
                copy.Link(copy.FindFilter(l.SourceContext.Name!), l.SourcePadIndex, copy.FindFilter(l.DestinationContext.Name!), l.DestinationPadIndex).ThrowIfError();
            }
        }
        return copy;

    }
}
