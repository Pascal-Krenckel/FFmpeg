using FFmpeg.AutoGen;
using FFmpeg.Collections;
using FFmpeg.Utils;
using System.Collections;

namespace FFmpeg.Formats;

/// <summary>
/// Represents the collection of chapters in a <see cref="FormatContext"/>.
/// </summary>
/// <remarks>
/// This collection provides access to the chapters stored in a media container.
/// Changes made through this collection directly modify the underlying FFmpeg
/// format context.
/// </remarks>
public readonly unsafe struct ChapterList(FormatContext context, bool readOnly) : IEnumerable<AVChapter>
{
    private readonly FormatContext context = context;

    private _AVChapter** Chapters => context.Context != null ? context.Context->chapters : throw new ObjectDisposedException(nameof(FormatContext));
    /// <summary>
    /// Gets the chapter at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the chapter.</param>
    /// <returns>The chapter at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is outside the bounds of the collection.
    /// </exception>
    public AVChapter this[int index] => new(context, index);
    /// <summary>
    /// Gets the number of chapters in the collection.
    /// </summary>
    public readonly int Count => context.Context != null ? (int)context.Context->nb_chapters : 0;
    /// <summary>
    /// Gets a value indicating whether the collection can be modified.
    /// </summary>
    public bool IsReadOnly { get; } = readOnly;
    /// <summary>
    /// Adds a new chapter or updates an existing chapter with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The unique chapter identifier.
    /// </param>
    /// <param name="timeBase">
    /// The time base used for <paramref name="start"/> and <paramref name="end"/>.
    /// </param>
    /// <param name="start">
    /// The chapter start timestamp expressed in units of <paramref name="timeBase"/>.
    /// </param>
    /// <param name="end">
    /// The chapter end timestamp expressed in units of <paramref name="timeBase"/>.
    /// </param>
    /// <param name="title">
    /// The chapter title.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The collection is read-only.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// FFmpeg failed to allocate or add the chapter.
    /// </exception>
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
        bool newChapter = false;
        if (chapter == null)
        {
            chapter = (_AVChapter*)ffmpeg.av_mallocz((ulong)sizeof(_AVChapter));
            newChapter = true;
        }
        chapter->id = id;
        chapter->time_base = timeBase;
        chapter->end = end;
        chapter->start = start;
        _ = ffmpeg.av_dict_set(&chapter->metadata, "title", title, 0);

        if (newChapter)
        {
            AVResult32 result = ffmpeg.av_dynarray_add_nofree(&context.Context->chapters, (int*)&context.Context->nb_chapters, chapter);
            if (result.IsError)
            {
                ffmpeg.av_dict_free(&chapter->metadata);
                ffmpeg.av_freep(&chapter);
                result.ThrowIfError();
            }
        }
    }
    /// <summary>
    /// Adds a new chapter or updates an existing chapter with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The unique chapter identifier.
    /// </param>
    /// <param name="start">
    /// The chapter start time.
    /// </param>
    /// <param name="end">
    /// The chapter end time.
    /// </param>
    /// <param name="title">
    /// The chapter title.
    /// </param>
    /// <remarks>
    /// A suitable time base is automatically chosen based on the specified
    /// start and end times.
    /// </remarks>
    public void SetChapter(long id, TimeSpan start, TimeSpan end, string title)
    {
        Rational timeBase = Rational.GreatestCommonDivisor(start, end, int.MaxValue, Rational.TIME_BASE);
        SetChapter(id, timeBase, start / timeBase, end / timeBase, title);
    }

    /// <summary>
    /// Adds a new chapter using the first available chapter identifier.
    /// </summary>
    /// <param name="timeBase">
    /// The time base used for <paramref name="start"/> and <paramref name="end"/>.
    /// </param>
    /// <param name="start">
    /// The chapter start timestamp.
    /// </param>
    /// <param name="end">
    /// The chapter end timestamp.
    /// </param>
    /// <param name="title">
    /// The chapter title.
    /// </param>
    /// <remarks>
    /// The chapter identifier is automatically selected so that it does not
    /// conflict with an existing chapter.
    /// </remarks>
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
    /// <summary>
    /// Removes the chapter at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the chapter to remove.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is outside the bounds of the collection.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The collection is read-only.
    /// </exception>
    public void RemoveAt(long index)
    {
        if (IsReadOnly)
            throw new NotSupportedException();
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (Chapters[index] != null)
        {
            if (Chapters[index]->metadata != null)
                ffmpeg.av_dict_free(&Chapters[index]->metadata);
            ffmpeg.av_freep(&Chapters[index]);
            Chapters[index] = Chapters[Count - 1];
            Chapters[Count - 1] = null;
            context.Context->nb_chapters--;
        }
    }
    /// <summary>
    /// Removes the chapter with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The chapter identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a chapter was removed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool RemoveById(long id)
    {
        for (long index = 0; index < Count; index++)
            if (Chapters[index]->id == id)
            {
                RemoveAt(index);
                return true;
            }
        return false;
    }
    /// <summary>
    /// Removes the specified chapter.
    /// </summary>
    /// <param name="chapter">
    /// The chapter to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the chapter was removed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Remove(AVChapter chapter) => RemoveById(chapter.Id);

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
    /// <summary>
    /// Returns an enumerator that iterates through the chapters in the collection.
    /// </summary>
    public IEnumerator<AVChapter> GetEnumerator() => new Enumerator(this);
    /// <summary>
    /// Returns an enumerator that iterates through the chapters in the collection.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Enumerates the chapters in a <see cref="ChapterList"/>.
    /// </summary>
    public class Enumerator(ChapterList list) : IEnumerator<AVChapter>
    {
        private int index = -1;

        /// <summary>
        /// Gets the chapter at the current position in the collection.
        /// </summary>
        public AVChapter Current => list[index];

        object IEnumerator.Current => Current;

        /// <summary>
        /// Releases the resources used by the enumerator.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Advances the enumerator to the next chapter in the collection.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the enumerator was successfully advanced to the
        /// next chapter; otherwise, <see langword="false"/> if the end of the
        /// collection has been reached.
        /// </returns>
        public bool MoveNext() => ++index < list.Count;

        /// <summary>
        /// Resets the enumerator to its initial position, which is before the first
        /// chapter in the collection.
        /// </summary>
        public void Reset() => index = -1;
    }
}
/// <summary>
/// Represents a chapter in a media container.
/// </summary>
/// <remarks>
/// An <see cref="AVChapter"/> provides access to the timing information and
/// metadata associated with a chapter in a <see cref="FormatContext"/>.
/// </remarks>
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

    /// <summary>
    /// Gets the unique identifier of the chapter.
    /// </summary>
    public long Id => Chapter->id;

    /// <summary>
    /// Gets the time base used by the chapter timestamps.
    /// </summary>
    /// <remarks>
    /// The <see cref="Start"/> and <see cref="End"/> values are expressed in
    /// units of this time base.
    /// </remarks>
    public Rational TimeBase => Chapter->time_base;

    /// <summary>
    /// Gets the start timestamp of the chapter.
    /// </summary>
    /// <remarks>
    /// The value is expressed in units of <see cref="TimeBase"/>.
    /// </remarks>
    public long Start => Chapter->start;

    /// <summary>
    /// Gets the end timestamp of the chapter.
    /// </summary>
    /// <remarks>
    /// The value is expressed in units of <see cref="TimeBase"/>.
    /// </remarks>
    public long End => Chapter->end;

    /// <summary>
    /// Gets the duration of the chapter.
    /// </summary>
    /// <remarks>
    /// The duration is calculated from <see cref="Start"/> and
    /// <see cref="End"/> using <see cref="TimeBase"/>.
    /// </remarks>
    public TimeSpan Duration => (End - Start) * TimeBase;

    /// <summary>
    /// Gets the metadata associated with the chapter.
    /// </summary>
    public AVDictionary_ref Metadata => new(&Chapter->metadata);

    /// <summary>
    /// Gets the title of the chapter, if one is available.
    /// </summary>
    /// <value>
    /// The value of the <c>title</c> metadata entry, or
    /// <see langword="null"/> if no title is present.
    /// </value>
    public string? Title => Metadata.TryGetValue("title", out string? val) ? val : null;
}
