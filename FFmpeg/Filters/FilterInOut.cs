using FFmpeg.AutoGen;
using FFmpeg.Unsafe;
using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;

/// <summary>
/// Represents an entry in a list of filter graph inputs or outputs.
/// </summary>
/// <remarks>
/// A <see cref="FilterInOutEntry"/> corresponds to FFmpeg's
/// <c>AVFilterInOut</c> structure and is primarily used when parsing
/// filter graph descriptions with <c>avfilter_graph_parse_ptr()</c>.
/// </remarks>
public unsafe struct FilterInOutEntry : IAVPointer<_AVFilterInOut>
{
    internal AutoGen._AVFilterInOut* filterInOut;
    readonly _AVFilterInOut* IAVPointer<_AVFilterInOut>.Pointer => filterInOut;
    internal FilterInOutEntry(AutoGen._AVFilterInOut* filterInOut) => this.filterInOut = filterInOut;


    /// <summary>
    /// Gets or sets the name of the link associated with this entry.
    /// </summary>
    /// <remarks>
    /// The name is used to match unconnected filter graph inputs and outputs
    /// when parsing a filter graph description.
    /// </remarks>
    public string? Name
    {
        get => filterInOut->name != null ? Marshal.PtrToStringUTF8((nint)filterInOut->name) : null;
        set
        {
            ffmpeg.av_freep(&filterInOut->name);
            filterInOut->name = ffmpeg.av_strdup(value);
        }
    }

    /// <summary>
    /// Gets or sets the filter associated with this entry.
    /// </summary>
    /// <value>
    /// The corresponding <see cref="FilterContext"/>, or <see langword="null"/>
    /// if no filter has been assigned.
    /// </value>
    public FilterContext? Filter
    {
        get => filterInOut->filter_ctx != null ? new FilterContext(filterInOut->filter_ctx) : null; set => filterInOut->filter_ctx = value == null ? (AutoGen._AVFilterContext*)null : value.context;
    }

    /// <summary>
    /// Gets or sets the index of the filter pad associated with this entry.
    /// </summary>
    public int PadIdx
    {
        get => filterInOut->pad_idx;
        set => filterInOut->pad_idx = value;
    }
}

/// <summary>
/// Represents a collection of filter graph input and output entries.
/// </summary>
/// <remarks>
/// <see cref="FilterInOutList"/> wraps FFmpeg's linked list of
/// <c>AVFilterInOut</c> structures. It is typically used to specify
/// named inputs and outputs when parsing filter graph descriptions.
/// </remarks>
public unsafe class FilterInOutList : IDisposable, IEnumerable<FilterInOutEntry>, IReadOnlyCollection<FilterInOutEntry>, IReadOnlyList<FilterInOutEntry>
{

    internal AutoGen._AVFilterInOut* Head
    {
        get => head;
        set
        {
            if (value == null)
                head = null;
            head = value;
            Count = GetTail(head, out tail);
        }
    }

    private AutoGen._AVFilterInOut* head;
    private AutoGen._AVFilterInOut* tail;
    private bool disposedValue;

    /// <summary>
    /// Gets the number of entries in the list.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the entry at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the entry to retrieve.</param>
    /// <returns>The entry at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="index"/> is less than zero or greater than or equal to
    /// <see cref="Count"/>.
    /// </exception>
    public FilterInOutEntry this[int index]
    {
        get
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index must be larger than or equal than 0.");
            AutoGen._AVFilterInOut* node = head;
            while (node != null && index-- > 0)
                node = node->next;
            return node == null ? throw new IndexOutOfRangeException("Index is out of range.") : new FilterInOutEntry(node);
        }
    }

    /// <summary>
    /// Initializes an empty <see cref="FilterInOutList"/>.
    /// </summary>
    public FilterInOutList()
    {
        head = null;
        tail = null;
        Count = 0;
    }

    internal FilterInOutList(AutoGen._AVFilterInOut* filterInOut)
    {
        head = filterInOut;
        Count = GetTail(head, out tail);

    }

    private int GetTail(AutoGen._AVFilterInOut* head, out AutoGen._AVFilterInOut* tail)
    {
        int count = 0;
        tail = null;
        while (head != null)
        {
            tail = head;
            count++;
            head = head->next;
        }
        return count;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {

            AutoGen._AVFilterInOut* head = this.head;
            ffmpeg.avfilter_inout_free(&head);
            _ = tail = null;
            disposedValue = true;
        }
    }

    ~FilterInOutList()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Releases the unmanaged resources used by this list.
    /// </summary>
    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the entries in the list.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator<FilterInOutEntry> GetEnumerator() => new FilterInOutEnumerator(this);
    /// <summary>
    /// Returns an enumerator that iterates through the entries in the list.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class FilterInOutEnumerator : IEnumerator<FilterInOutEntry>
    {
        private AutoGen._AVFilterInOut* current;
        private readonly FilterInOutList filterInOutList;
        public FilterInOutEnumerator(FilterInOutList filterInOutList)
        {
            this.filterInOutList = filterInOutList;
            current = null;
        }
        public FilterInOutEntry Current => new(current);
        object IEnumerator.Current => Current;
        public void Dispose() { }
        public bool MoveNext()
        {
            if (current == null)
            {
                if (filterInOutList.head == null)
                {
                    return false;
                }
                else
                {
                    current = filterInOutList.head;
                    return true;
                }
            }

            if (current->next == null)
                return false;
            current = current->next;
            return true;
        }
        public void Reset() => current = null;
    }

    /// <summary>
    /// Adds a new filter input or output entry to the list.
    /// </summary>
    /// <param name="linkName">
    /// The name of the filter graph link.
    /// </param>
    /// <param name="filter">
    /// The filter associated with the link.
    /// </param>
    /// <param name="filterPadIndex">
    /// The zero-based index of the filter pad.
    /// </param>
    /// <returns>
    /// The newly created <see cref="FilterInOutEntry"/>.
    /// </returns>
    public FilterInOutEntry Add(string linkName, FilterContext filter, int filterPadIndex)
    {
        AutoGen._AVFilterInOut* inout = ffmpeg.avfilter_inout_alloc();
        inout->name = ffmpeg.av_strdup(linkName);
        inout->filter_ctx = filter.context;
        inout->pad_idx = filterPadIndex;
        return Add(new(inout));
    }

    /// <summary>
    /// Adds a new filter input or output entry to the list.
    /// </summary>
    /// <param name="entry">
    /// The FilterInOutEntry contain the filter informations.
    /// </param>
    /// <returns>
    /// The newly created <see cref="FilterInOutEntry"/>.
    /// </returns>
    private FilterInOutEntry Add(FilterInOutEntry entry)
    {
        int count = GetTail(entry.filterInOut, out AutoGen._AVFilterInOut* tail);
        Count += count;
        if (head == null)
            head = entry.filterInOut;
        else
            this.tail->next = entry.filterInOut;
        this.tail = tail;
        return entry;
    }

    /// <summary>
    /// Removes all entries from the list and releases the associated unmanaged
    /// resources.
    /// </summary>
    public void Clear()
    {
        AutoGen._AVFilterInOut* head = this.head;
        ffmpeg.avfilter_inout_free(&head);
        this.head = null;
        tail = null;
        Count = 0;
    }
}