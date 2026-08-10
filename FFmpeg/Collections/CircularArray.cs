using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace FFmpeg.Collections;

/// <summary>
/// A resizable, array-backed list that stores its elements in a ring buffer
/// (a "deque"-like structure) rather than always starting at index 0.
/// This makes both ends of the collection cheap to grow/shrink from, and
/// <see cref="Insert(int, T)"/> / <see cref="RemoveAt(int)"/> only need to
/// shift whichever side of the target index is smaller, instead of always
/// shifting everything after it as a plain array-backed list would.
/// </summary>
/// <typeparam name="T">The type of elements stored in the collection.</typeparam>
public class CircularArray<T> : IEnumerable<T>, IList<T>, IReadOnlyList<T>
{
    T[] data;

    /// <summary>
    /// The physical index of the first logical element, i.e. logical index 0.
    /// </summary>
    private int Head { get; set; }

    /// <summary>
    /// The physical index one past the last logical element (where the next
    /// appended item would be written).
    /// </summary>
    private int Tail => (Head + Count) % Capacity;

    /// <summary>
    /// Gets a reference to the element at the given logical index, allowing
    /// both reading and in-place mutation (e.g. <c>array[i]++</c>).
    /// </summary>
    /// <param name="index">A zero-based logical index in the range <c>[0, Count)</c>.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="index"/> is negative or not less than <see cref="Count"/>.
    /// </exception>
    public ref T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            return ref data[(Head + index) % Capacity];
        }
    }

    /// <summary>
    /// Creates an empty <see cref="CircularArray{T}"/> with a small default capacity.
    /// </summary>
    public CircularArray()
    {
        data = new T[16];
    }

    /// <summary>
    /// Creates an empty <see cref="CircularArray{T}"/> with the given initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements the backing array can hold before it needs to grow.</param>
    public CircularArray(int capacity)
    {
        data = new T[capacity];
    }

    /// <summary>
    /// Creates a <see cref="CircularArray{T}"/> containing a copy of <paramref name="list"/>'s elements.
    /// </summary>
    public CircularArray(IReadOnlyList<T> list)
    {
        data = new T[list.Count];
        for (int i = 0; i < list.Count; i++)
            data[i] = list[i];
        Count = list.Count;
    }

    /// <summary>
    /// Creates a <see cref="CircularArray{T}"/> containing a copy of <paramref name="list"/>'s elements.
    /// </summary>
    public CircularArray(IList<T> list)
    {
        data = new T[list.Count];
        list.CopyTo(data, 0);
        Count = list.Count;
    }

    /// <summary>
    /// Creates a <see cref="CircularArray{T}"/> containing a copy of <paramref name="list"/>'s elements.
    /// </summary>
    public CircularArray(List<T> list)
    {
        data = new T[list.Count];
        list.CopyTo(data);
        Count = list.Count;
    }

    /// <summary>
    /// Creates a <see cref="CircularArray{T}"/> containing a copy of <paramref name="list"/>'s elements.
    /// </summary>
    public CircularArray(ReadOnlySpan<T> list)
    {
        data = new T[list.Length];
        for (int i = 0; i < list.Length; i++)
            data[i] = list[i];
        Count = list.Length;
    }

    /// <summary>
    /// Creates a <see cref="CircularArray{T}"/> containing a copy of <paramref name="list"/>'s elements.
    /// </summary>
    public CircularArray(ICollection<T> list)
    {
        data = new T[list.Count];
        Count = list.Count;
        int index = 0;
        foreach (var item in list)
            data[index++] = item;
    }

    /// <summary>
    /// Creates a <see cref="CircularArray{T}"/> containing a copy of <paramref name="list"/>'s elements.
    /// The sequence is consumed lazily via repeated <see cref="Add(T)"/> calls, since its length isn't
    /// known up front.
    /// </summary>
    public CircularArray(IEnumerable<T> list)
    {
        data = new T[25];
        foreach (var item in list)
            Add(item);
    }

    /// <summary>
    /// Reallocates the backing array to <paramref name="newCapacity"/> and repacks the current
    /// logical elements to start at physical index 0 (i.e. <see cref="Head"/> becomes 0).
    /// </summary>
    /// <param name="newCapacity">The new capacity. Must be greater than or equal to <see cref="Count"/>.</param>
    private void Resize(int newCapacity)
    {
        T[] newData = new T[newCapacity];
        GetSpans(out var first, out var second);
        first.CopyTo(newData);
        second.CopyTo(newData.AsSpan(first.Length));
        Head = 0;
        data = newData;
    }

    /// <summary>The number of elements currently stored.</summary>
    public int Count { get; private set; }

    /// <summary>The number of elements the backing array can hold before it needs to grow.</summary>
    public int Capacity => data.Length;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    T IList<T>.this[int index] { get => this[index]; set => this[index] = value; }
    T IReadOnlyList<T>.this[int index] { get => this[index]; }

    /// <summary>
    /// Returns the live elements as one or two contiguous <see cref="Span{T}"/> slices over the
    /// backing array, in logical order. Two spans are returned when the logical range wraps past
    /// the end of the backing array; <paramref name="second"/> is empty otherwise.
    /// </summary>
    /// <param name="first">The first (and possibly only) contiguous slice of live elements.</param>
    /// <param name="second">The remaining live elements after the wrap point, or empty if there is no wrap.</param>
    public void GetSpans(out Span<T> first, out Span<T> second)
    {
        // Head == Tail is ambiguous on its own: it means either "empty"
        // (Count == 0) or "completely full" (Count == Capacity). Only
        // Count can tell those apart, so check it explicitly first.
        if (Count == 0)
        {
            first = default;
            second = default;
        }
        else if (Head < Tail)
        {
            first = data.AsSpan(Head, Count);
            second = default;
        }
        else
        {
            first = data.AsSpan(Head, Capacity - Head);
            second = data.AsSpan(0, Tail);
        }
    }

    /// <summary>
    /// Returns the free (unused) region of the backing array as one or two contiguous
    /// <see cref="Span{T}"/> slices, i.e. the complement of <see cref="GetSpans"/>.
    /// </summary>
    /// <param name="first">The first (and possibly only) contiguous slice of free space.</param>
    /// <param name="second">The remaining free space after the wrap point, or empty if there is no wrap.</param>
    private void GetSpansToWrite(out Span<T> first, out Span<T> second)
    {
        // Same Head == Tail ambiguity as GetSpans, mirrored: it means
        // either "no free space" (Count == Capacity) or "entirely free"
        // (Count == 0).
        if (Count == Capacity)
        {
            first = default;
            second = default;
        }
        else if (Head < Tail)
        {
            first = data.AsSpan(Tail, Capacity - Tail);
            second = data.AsSpan(0, Head);
        }
        else
        {
            first = data.AsSpan(Tail, Head - Tail);
            second = default;
        }
    }

    /// <summary>
    /// Returns a struct enumerator over the elements in logical order. Prefer iterating via
    /// <c>foreach</c> directly (rather than through <see cref="IEnumerable{T}"/>) to avoid boxing.
    /// </summary>
    // Returning the concrete struct (rather than IEnumerator<T>) lets `foreach`
    // bind to it structurally with no boxing. Only IEnumerable<T>/IEnumerable
    // consumers (LINQ, non-generic foreach, etc.) pay the boxing cost, via the
    // explicit interface implementations below.
    public CircularArrayEnumerator GetEnumerator() => new CircularArrayEnumerator(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Searches for <paramref name="item"/> and returns its logical index, or -1 if not found.
    /// </summary>
    /// <param name="item">The value to locate, compared using the default equality comparer for <typeparamref name="T"/>.</param>
    /// <returns>The zero-based logical index of the first matching element, or -1 if none is found.</returns>
    public int IndexOf(T item)
    {
        if (Head < Tail)
        {
            int index = Array.IndexOf(data, item, Head, Count);
            if (index < 0)
                return -1;
            return index - Head;
        }
        else
        {
            int index = Array.IndexOf(data, item, Head, data.Length - Head);
            if (index >= 0)
                return index - Head;
            index = Array.IndexOf(data, item, 0, Tail);
            if (index >= 0)
                return index + (Capacity - Head);
            return -1;
        }
    }

    // ---- InsertRange ----
    //
    // All InsertRange overloads funnel into InsertRange, which:
    //   1. grows the backing array if there isn't enough free room,
    //   2. opens a gap of `insertCount` slots at `index` by shifting
    //      whichever side (the part before `index` or the part at/after it)
    //      is smaller - mirroring the two RemoveAt branches below but in
    //      reverse - and
    //   3. copies the new items into the freed gap.

    /// <summary>
    /// Inserts <paramref name="item"/> at the given logical index, shifting later elements
    /// (or earlier elements, whichever is fewer) to make room.
    /// </summary>
    /// <param name="index">The logical index to insert at. Valid range is <c>[0, Count]</c>; <c>Count</c> appends.</param>
    /// <param name="item">The value to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    public void Insert(int index, T item) =>
        // MemoryMarshal.CreateReadOnlySpan gives a 1-element span over the
        // local parameter directly - no `new T[1]` allocation for the (by
        // far) most common InsertRange call.
        InsertRange(index, MemoryMarshal.CreateReadOnlySpan(ref item, 1));


    /// <summary>
    /// Inserts the elements of <paramref name="data"/>, in order, starting at the given logical index.
    /// </summary>
    /// <param name="index">The logical index to insert at. Valid range is <c>[0, Count]</c>; <c>Count</c> appends.</param>
    /// <param name="data">
    /// The elements to insert. If this isn't a <see cref="T:T[]"/>, <see cref="List{T}"/>, or
    /// <see cref="ICollection{T}"/>, it is fully enumerated and buffered first, since the length
    /// must be known before any shifting happens.
    /// </param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    public void InsertRange(int index, IEnumerable<T> data)
    {
        switch (data)
        {
            case T[] arr:
                InsertRange(index, arr.AsSpan());
                return;
            case IList<T> list:
                InsertRange(index, list);
                return;
            case IReadOnlyList<T> rlist:
                InsertRange(index, rlist);
                return;
            case ICollection<T> clist:
                InsertRange(index, clist);
                return;
            case IReadOnlyCollection<T> crlist:
                InsertRange(index, crlist);
                return;
            default:
                var tmp = data.ToList();
                InsertRange(index, (IList<T>)tmp);
                return;
        }
    }

    /// <summary>
    /// Inserts the elements of <paramref name="data"/>, in order, starting at the given logical index.
    /// </summary>
    /// <param name="index">The logical index to insert at. Valid range is <c>[0, Count]</c>; <c>Count</c> appends.</param>
    /// <param name="data">The elements to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    public void InsertRange(int index, ReadOnlySpan<T> items)
    {
        if (index < 0 || index > Count)
            throw new IndexOutOfRangeException();

        int insertCount = items.Length;
        if (insertCount == 0)
            return;

        if (Count + insertCount > Capacity)
            Resize(Math.Max(Count + insertCount, (Capacity + 1) * 2));

        int leftCount = index;
        int rightCount = Count - index;

        if (leftCount <= rightCount)
        {
            // Fewer elements before `index`: pull Head back by insertCount
            // slots (opening room at the front) and slide that left part
            // into the newly freed physical slots.
            Head = (Head - insertCount + Capacity) % Capacity;
            CopyRange(insertCount, 0, leftCount);
        }
        else
        {
            // Fewer elements from `index` onward: slide that right part
            // forward by insertCount slots to open the gap.
            CopyRange(index, index + insertCount, rightCount);
        }

        Count += insertCount;
        CopyIntoLogicalRange(index, items);
    }


    /// <summary>
    /// Inserts the elements of <paramref name="data"/>, in order, starting at the given logical index.
    /// </summary>
    /// <param name="index">The logical index to insert at. Valid range is <c>[0, Count]</c>; <c>Count</c> appends.</param>
    /// <param name="data">The elements to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    private void InsertRange(int index, IList<T> items)
    {
        if (index < 0 || index > Count)
            throw new IndexOutOfRangeException();

        int insertCount = items.Count;
        if (insertCount == 0)
            return;

        if (Count + insertCount > Capacity)
            Resize(Math.Max(Count + insertCount, (Capacity + 1) * 2));

        int leftCount = index;
        int rightCount = Count - index;

        if (leftCount <= rightCount)
        {
            // Fewer elements before `index`: pull Head back by insertCount
            // slots (opening room at the front) and slide that left part
            // into the newly freed physical slots.
            Head = (Head - insertCount + Capacity) % Capacity;
            CopyRange(insertCount, 0, leftCount);
        }
        else
        {
            // Fewer elements from `index` onward: slide that right part
            // forward by insertCount slots to open the gap.
            CopyRange(index, index + insertCount, rightCount);
        }

        Count += insertCount;
        CopyIntoLogicalRange(index, items);
    }

    /// <summary>
    /// Inserts the elements of <paramref name="data"/>, in order, starting at the given logical index.
    /// </summary>
    /// <param name="index">The logical index to insert at. Valid range is <c>[0, Count]</c>; <c>Count</c> appends.</param>
    /// <param name="data">The elements to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    private void InsertRange(int index, IReadOnlyList<T> items)
    {
        if (index < 0 || index > Count)
            throw new IndexOutOfRangeException();

        int insertCount = items.Count;
        if (insertCount == 0)
            return;

        if (Count + insertCount > Capacity)
            Resize(Math.Max(Count + insertCount, (Capacity + 1) * 2));

        int leftCount = index;
        int rightCount = Count - index;

        if (leftCount <= rightCount)
        {
            // Fewer elements before `index`: pull Head back by insertCount
            // slots (opening room at the front) and slide that left part
            // into the newly freed physical slots.
            Head = (Head - insertCount + Capacity) % Capacity;
            CopyRange(insertCount, 0, leftCount);
        }
        else
        {
            // Fewer elements from `index` onward: slide that right part
            // forward by insertCount slots to open the gap.
            CopyRange(index, index + insertCount, rightCount);
        }

        Count += insertCount;
        CopyIntoLogicalRange(index, items);
    }


    /// <summary>
    /// Inserts the elements of <paramref name="data"/>, in order, starting at the given logical index.
    /// </summary>
    /// <param name="index">The logical index to insert at. Valid range is <c>[0, Count]</c>; <c>Count</c> appends.</param>
    /// <param name="data">The elements to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    private void InsertRange(int index, ICollection<T> items)
    {
        if (index < 0 || index > Count)
            throw new IndexOutOfRangeException();

        int insertCount = items.Count;
        if (insertCount == 0)
            return;

        if (Count + insertCount > Capacity)
            Resize(Math.Max(Count + insertCount, (Capacity + 1) * 2));

        int leftCount = index;
        int rightCount = Count - index;

        if (leftCount <= rightCount)
        {
            // Fewer elements before `index`: pull Head back by insertCount
            // slots (opening room at the front) and slide that left part
            // into the newly freed physical slots.
            Head = (Head - insertCount + Capacity) % Capacity;
            CopyRange(insertCount, 0, leftCount);
        }
        else
        {
            // Fewer elements from `index` onward: slide that right part
            // forward by insertCount slots to open the gap.
            CopyRange(index, index + insertCount, rightCount);
        }

        Count += insertCount;
        CopyIntoLogicalRange(index, items);
    }

    /// <summary>
    /// Inserts the elements of <paramref name="data"/>, in order, starting at the given logical index.
    /// </summary>
    /// <param name="index">The logical index to insert at. Valid range is <c>[0, Count]</c>; <c>Count</c> appends.</param>
    /// <param name="data">The elements to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    private void InsertRange(int index, IReadOnlyCollection<T> items)
    {
        if (index < 0 || index > Count)
            throw new IndexOutOfRangeException();

        int insertCount = items.Count;
        if (insertCount == 0)
            return;

        if (Count + insertCount > Capacity)
            Resize(Math.Max(Count + insertCount, (Capacity + 1) * 2));

        int leftCount = index;
        int rightCount = Count - index;

        if (leftCount <= rightCount)
        {
            // Fewer elements before `index`: pull Head back by insertCount
            // slots (opening room at the front) and slide that left part
            // into the newly freed physical slots.
            Head = (Head - insertCount + Capacity) % Capacity;
            CopyRange(insertCount, 0, leftCount);
        }
        else
        {
            // Fewer elements from `index` onward: slide that right part
            // forward by insertCount slots to open the gap.
            CopyRange(index, index + insertCount, rightCount);
        }

        Count += insertCount;
        CopyIntoLogicalRange(index, items);
    }

    /// <summary>
    /// Writes <paramref name="src"/> into the logical range starting at <paramref name="start"/>,
    /// using at most two <see cref="Span{T}.CopyTo(Span{T})"/> calls (the range wraps the physical
    /// buffer at most once) instead of writing element-by-element through the <c>%Capacity</c> indexer.
    /// </summary>
    /// <param name="start">The logical index of the first element to write.</param>
    /// <param name="src">The values to write, in order.</param>
    private void CopyIntoLogicalRange(int start, ReadOnlySpan<T> src)
    {
        if (src.Length == 0)
            return;

        int physicalStart = (Head + start) % Capacity;
        int firstLength = Math.Min(src.Length, Capacity - physicalStart);

        src.Slice(0, firstLength).CopyTo(data.AsSpan(physicalStart, firstLength));

        int remaining = src.Length - firstLength;
        if (remaining > 0)
            src.Slice(firstLength).CopyTo(data.AsSpan(0, remaining));
    }

    /// <summary>
    /// Writes <paramref name="src"/> into the logical range starting at <paramref name="start"/>,
    /// using at most two <see cref="Span{T}.CopyTo(Span{T})"/> calls (the range wraps the physical
    /// buffer at most once) instead of writing element-by-element through the <c>%Capacity</c> indexer.
    /// </summary>
    /// <param name="start">The logical index of the first element to write.</param>
    /// <param name="src">The values to write, in order.</param>
    private void CopyIntoLogicalRange(int start, IList<T> src)
    {
        if (src.Count == 0)
            return;

        int physicalStart = (Head + start) % Capacity;
        int firstLength = Math.Min(src.Count, Capacity - physicalStart);

        var copySpan = data.AsSpan(physicalStart, firstLength);
        for (int i = 0; i < firstLength; i++)
            copySpan[i] = src[i];

        int remaining = src.Count - firstLength;
        if (remaining <= 0)
            return;

        copySpan = data.AsSpan(0, remaining);
        for (int i = firstLength; i < src.Count; i++)
            copySpan[i - firstLength] = src[i];
    }

    /// <summary>
    /// Writes <paramref name="src"/> into the logical range starting at <paramref name="start"/>,
    /// using at most two <see cref="Span{T}.CopyTo(Span{T})"/> calls (the range wraps the physical
    /// buffer at most once) instead of writing element-by-element through the <c>%Capacity</c> indexer.
    /// </summary>
    /// <param name="start">The logical index of the first element to write.</param>
    /// <param name="src">The values to write, in order.</param>
    private void CopyIntoLogicalRange(int start, IReadOnlyList<T> src)
    {
        if (src.Count == 0)
            return;

        int physicalStart = (Head + start) % Capacity;
        int firstLength = Math.Min(src.Count, Capacity - physicalStart);

        var copySpan = data.AsSpan(physicalStart, firstLength);
        for (int i = 0; i < firstLength; i++)
            copySpan[i] = src[i];

        int remaining = src.Count - firstLength;
        if (remaining <= 0)
            return;

        copySpan = data.AsSpan(0, remaining);
        for (int i = firstLength; i < src.Count; i++)
            copySpan[i - firstLength] = src[i];
    }

    /// <summary>
    /// Writes <paramref name="src"/> into the logical range starting at <paramref name="start"/>,
    /// using at most two <see cref="Span{T}.CopyTo(Span{T})"/> calls (the range wraps the physical
    /// buffer at most once) instead of writing element-by-element through the <c>%Capacity</c> indexer.
    /// </summary>
    /// <param name="start">The logical index of the first element to write.</param>
    /// <param name="src">The values to write, in order.</param>
    private void CopyIntoLogicalRange(int start, ICollection<T> src)
    {
        if (src.Count == 0)
            return;

        int physicalStart = (Head + start) % Capacity;
        int firstLength = Math.Min(src.Count, Capacity - physicalStart);

        var copySpan = data.AsSpan(physicalStart, firstLength);
        int count = 0;
        var iterator = src.GetEnumerator();

        for (; count < firstLength && iterator.MoveNext(); count++)
            copySpan[count] = iterator.Current;

        int remaining = src.Count - firstLength;
        if (remaining <= 0)
            return;

        for (int i = 0; i < remaining && iterator.MoveNext(); i++)
            data[i] = iterator.Current;
    }

    /// <summary>
    /// Writes <paramref name="src"/> into the logical range starting at <paramref name="start"/>,
    /// using at most two <see cref="Span{T}.CopyTo(Span{T})"/> calls (the range wraps the physical
    /// buffer at most once) instead of writing element-by-element through the <c>%Capacity</c> indexer.
    /// </summary>
    /// <param name="start">The logical index of the first element to write.</param>
    /// <param name="src">The values to write, in order.</param>
    private void CopyIntoLogicalRange(int start, IReadOnlyCollection<T> src)
    {
        if (src.Count == 0)
            return;

        int physicalStart = (Head + start) % Capacity;
        int firstLength = Math.Min(src.Count, Capacity - physicalStart);

        var copySpan = data.AsSpan(physicalStart, firstLength);
        int count = 0;
        var iterator = src.GetEnumerator();

        for (; count < firstLength && iterator.MoveNext(); count++)
            copySpan[count] = iterator.Current;

        int remaining = src.Count - firstLength;
        if (remaining <= 0)
            return;

        for (int i = 0; i < remaining && iterator.MoveNext(); i++)
            data[i] = iterator.Current;
    }

    /// <summary>
    /// Removes the element at the given logical index, shifting whichever side of the split
    /// (elements before or after <paramref name="index"/>) is smaller to close the gap. If the
    /// collection becomes sufficiently sparse, the backing array is also shrunk to reclaim memory.
    /// </summary>
    /// <param name="index">The logical index of the element to remove. Valid range is <c>[0, Count)</c>.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new IndexOutOfRangeException();
        if (index > Count / 2)
        {
            CopyRange(index + 1, index, Count - index - 1);
            data[Tail] = default!;
            Count--;
        }
        else
        {
            CopyRange(0, 1, index);
            Count--;
            data[Head] = default!;
            Head = (Head + 1) % Capacity;
        }

        // Shrink once the array gets sufficiently sparse, mirroring the
        // growth policy in AddRange(). Resize() repacks the live elements
        // starting at index 0, so this is always safe.
        if (Capacity > 10 && Count < Capacity / 2)
        {
            Resize(Math.Max(10, Capacity / 2));
        }
    }


    /// <summary>
    /// Copies <paramref name="count"/> logical elements from logical index <paramref name="src"/>
    /// to logical index <paramref name="dst"/> within the current backing array, correctly handling
    /// overlap between the source and destination ranges.
    /// </summary>
    /// <param name="src">The logical index to copy from.</param>
    /// <param name="dst">The logical index to copy to.</param>
    /// <param name="count">The number of elements to copy.</param>
    private void CopyRange(int src, int dst, int count)
    {
        if (count <= 0 || src == dst)
            return;

        if (dst > src && dst < src + count)
            CopyRangeBackward(src, dst, count);
        else
            CopyRangeForward(src, dst, count);
    }

    /// <summary>
    /// Copies <paramref name="count"/> logical elements from <paramref name="src"/> to <paramref name="dst"/>
    /// starting at the lowest index and moving upward. Safe when <paramref name="dst"/> does not lie
    /// strictly within the source range ahead of <paramref name="src"/>.
    /// </summary>
    private void CopyRangeForward(int src, int dst, int count)
    {
        int srcIndex = (Head + src) % Capacity;
        int dstIndex = (Head + dst) % Capacity;

        while (count > 0)
        {
            int length = Math.Min(
                count,
                Math.Min(Capacity - srcIndex, Capacity - dstIndex));

            Array.Copy(data, srcIndex, data, dstIndex, length);

            srcIndex = (srcIndex + length) % Capacity;
            dstIndex = (dstIndex + length) % Capacity;
            count -= length;
        }
    }

    /// <summary>
    /// Copies <paramref name="count"/> logical elements from <paramref name="src"/> to <paramref name="dst"/>
    /// starting at the highest index and moving downward. Required when <paramref name="dst"/> overlaps
    /// <paramref name="src"/>'s range ahead of it, to avoid overwriting source data before it's read.
    /// </summary>
    private void CopyRangeBackward(int src, int dst, int count)
    {
        while (count > 0)
        {
            // Treat 0 as Capacity because these are exclusive end positions.
            int srcEnd = (Head + src + count) % Capacity;
            int dstEnd = (Head + dst + count) % Capacity;

            if (srcEnd == 0)
                srcEnd = Capacity;

            if (dstEnd == 0)
                dstEnd = Capacity;

            int length = Math.Min(count, Math.Min(srcEnd, dstEnd));

            Array.Copy(
                data,
                srcEnd - length,
                data,
                dstEnd - length,
                length);

            count -= length;
        }
    }

    /// <summary>
    /// Appends <paramref name="item"/> to the end of the collection, growing the backing array
    /// first if it's already full.
    /// </summary>
    /// <param name="item">The value to append.</param>
    public void Add(T item)
    {
        if (Count == Capacity)
            Resize((Capacity + 1) * 2);
        data[Tail] = item;
        Count++;
    }

    /// <summary>
    /// Adds the elements of <paramref name="data"/>, in order at the end of the list.
    /// </summary>
    /// <param name="data">The elements to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    public void AddRange(ReadOnlySpan<T> item) => InsertRange(Count, item);

    /// <summary>
    /// Adds the elements of <paramref name="data"/>, in order at the end of the list.
    /// </summary>
    /// <param name="data">The elements to insert.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or greater than <see cref="Count"/>.</exception>
    public void AddRange(IEnumerable<T> item) => InsertRange(Count, item);


    /// <summary>
    /// Removes all elements. This does not shrink the backing array.
    /// </summary>
    public void Clear()
    {
        Count = 0;
        Array.Clear(data, 0, Count);
    }


    public void ShrinkToFit()
    {
        if (data.Length == Count)
            return;
        Resize(Count);

    }

    /// <summary>Determines whether <paramref name="item"/> is present in the collection.</summary>
    /// <param name="item">The value to locate, compared using the default equality comparer for <typeparamref name="T"/>.</param>
    public bool Contains(T item) => IndexOf(item) >= 0;

    /// <summary>
    /// Copies all elements, in logical order, into <paramref name="array"/> starting at <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array">The destination array. Must have room for <see cref="Count"/> elements starting at <paramref name="arrayIndex"/>.</param>
    /// <param name="arrayIndex">The index in <paramref name="array"/> to start writing at.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        GetSpans(out var first, out var second);
        first.CopyTo(array.AsSpan(arrayIndex));
        second.CopyTo(array.AsSpan(arrayIndex + first.Length));
    }

    /// <summary>
    /// Removes the first occurrence of <paramref name="item"/>, if present.
    /// </summary>
    /// <param name="item">The value to remove, compared using the default equality comparer for <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if a matching element was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
            return false;
        RemoveAt(index);
        return true;
    }


    /// <summary>
    /// A struct enumerator over a <see cref="CircularArray{T}"/>'s elements in logical order.
    /// Obtained via <see cref="CircularArray{T}.GetEnumerator"/>; using it through <c>foreach</c>
    /// directly avoids the heap allocation that boxing to <see cref="IEnumerator{T}"/> would incur.
    /// </summary>
    public struct CircularArrayEnumerator(CircularArray<T> array) : IEnumerator<T>
    {
        private int _index = -1;

        /// <inheritdoc/>
        public readonly T Current => array[_index];
        readonly object? IEnumerator.Current => Current;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_index < array.Count - 1)
            {
                _index++;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _index = -1;
        }

        /// <inheritdoc/>
        public readonly void Dispose() { }
    }
}