using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;

public unsafe struct FilterInOutEntry
{
    internal AutoGen._AVFilterInOut* filterInOut;

    internal FilterInOutEntry(AutoGen._AVFilterInOut* filterInOut) => this.filterInOut = filterInOut;


    public string? Name
    {
        get => filterInOut->name != null ? Marshal.PtrToStringUTF8((nint)filterInOut->name) : null;
        set
        {
            ffmpeg.av_freep(&filterInOut->name);
            filterInOut->name = ffmpeg.av_strdup(value);
        }
    }

    public FilterContext? Filter
    {
        get => filterInOut->filter_ctx != null ? new FilterContext(filterInOut->filter_ctx) : null; set => filterInOut->filter_ctx = value == null ? (AutoGen._AVFilterContext*)null : value.context;
    }

    public int PadIdx
    {
        get => filterInOut->pad_idx;
        set => filterInOut->pad_idx = value;
    }
}

/// <summary>
/// 
/// </summary>
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

    public int Count { get; private set; }

    public FilterInOutEntry this[int index]
    {
        get
        {
            AutoGen._AVFilterInOut* node = head;
            while (node != null && index-- >= 0)
                node = node->next;
            return node == null ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.") : new FilterInOutEntry(node);
        }
    }

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
            if (disposing)
            {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            AutoGen._AVFilterInOut* head = this.head;
            ffmpeg.avfilter_inout_free(&head);
            _ = tail = null;
            disposedValue = true;
        }
    }

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~FilterInOutList()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public IEnumerator<FilterInOutEntry> GetEnumerator() => new FilterInOutEnumerator(this);
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

    public FilterInOutEntry Add(string linkName, FilterContext filter, int filterPadIndex)
    {
        AutoGen._AVFilterInOut* inout = ffmpeg.avfilter_inout_alloc();
        inout->name = ffmpeg.av_strdup(linkName);
        inout->filter_ctx = filter.context;
        inout->pad_idx = filterPadIndex;
        return Add(new(inout));
    }

    private FilterInOutEntry Add(FilterInOutEntry entry)
    {
        int count = GetTail(entry.filterInOut, out AutoGen._AVFilterInOut* tail);
        Count += count;
        if (head == null)
            head = entry.filterInOut;
        this.tail = tail;
        return entry;
    }

    public void Clear()
    {
        AutoGen._AVFilterInOut* head = this.head;
        ffmpeg.avfilter_inout_free(&head);
        this.head = null;
        tail = null;
        Count = 0;
    }
}