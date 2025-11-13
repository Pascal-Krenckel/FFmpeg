using FFmpeg.AutoGen;
using FFmpeg.Collections;
using FFmpeg.Utils;
using System.Collections;

namespace FFmpeg.Formats;

public readonly unsafe struct ChapterList(FormatContext context, bool readOnly) : IEnumerable<AVChapter>
{
    private readonly FormatContext context = context;

    private _AVChapter** Chapters => context.Context != null ? context.Context->chapters : throw new ObjectDisposedException(nameof(FormatContext));

    public AVChapter this[int index] => new(context, index);

    public readonly int Count => context.Context != null ? (int)context.Context->nb_chapters : 0;

    public bool IsReadOnly { get; } = readOnly;

    public void SetChapter(long id, Rational timeBase, long start, long end, string title)
    {
        if (IsReadOnly)
            throw new NotSupportedException();
        _AVChapter* chapter = null;
        for (int i = 0; i < Count; i++)
        {
            if (Chapters[i]->id == id)
                chapter = Chapters[i];
        }

        if (chapter == null)
            chapter = (_AVChapter*)ffmpeg.av_mallocz((ulong)sizeof(_AVChapter));
        chapter->id = id;
        chapter->time_base = timeBase;
        chapter->end = end;
        chapter->start = start;
        _ = ffmpeg.av_dict_set(&chapter->metadata, "title", title, 0);

        AVResult32 result = ffmpeg.av_dynarray_add_nofree(&context.Context->chapters, (int*)&context.Context->nb_chapters, chapter);
        if (result.IsError)
        {
            ffmpeg.av_dict_free(&chapter->metadata);
            ffmpeg.av_freep(&chapter);
            result.ThrowIfError();
        }
    }

    public void SetChapter(long id, TimeSpan start, TimeSpan end, string title)
    {
        Rational timeBase = Rational.GreatestCommonDivisor(start, end, int.MaxValue, Rational.TIME_BASE);
        SetChapter(id, timeBase, start / timeBase, end / timeBase, title);
    }

    public void AddChapter(Rational timeBase, long start, long end, string title)
    {
        // check if Count is free as this would be usual way
        bool count_used = false;
        for (int i = 0; i < Count; i++)
        {
            if (count_used = Chapters[i]->id == Count)
                break;
        }

        long id = !count_used ? Count : FindFirstFree();
        SetChapter(id, timeBase, start, end, title);
    }

    private int FindFirstFree()
    {
        Span<bool> usedIds = stackalloc bool[Count];
        for (int i = 0; i < Count; i++)
        {
            if (Chapters[i]->id < Count && Chapters[i]->id >= 0)
                usedIds[(int)Chapters[i]->id] = true;
        }

        for (int i = 0; i < Count; i++)
        {
            if (!usedIds[i])
                return i;
        }

        return Count;
    }

    public IEnumerator<AVChapter> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class Enumerator(ChapterList list) : IEnumerator<AVChapter>
    {
        private int index = -1;
        public AVChapter Current => list[index];

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext() => ++index < list.Count;
        public void Reset() => index = -1;
    }
}

public readonly unsafe struct AVChapter

{
    internal AVChapter(FormatContext context, int index)
    {
        this.context = context;
        this.index = index;
    }

    private readonly FormatContext context;
    private readonly int index;

    private _AVChapter* Chapter => context.Context != null && index < context.Context->nb_chapters ? context.Context->chapters[index] : throw new ObjectDisposedException("The format context was disposed or the chapter removed.");

    public long Id => Chapter->id;
    public readonly Rational TimeBase => Chapter->time_base;

    public readonly long Start => Chapter->start;
    public readonly long End => Chapter->end;

    public TimeSpan Duration => (End - Start) * TimeBase;

    public AVDictionary_ref Metadata => new(&Chapter->metadata);

    public readonly string? Title => Metadata.TryGetValue("title", out string? val) ? val : null;
}
