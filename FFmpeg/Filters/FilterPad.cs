using FFmpeg.Utils;
using System.Collections;

namespace FFmpeg.Filters;
/// <summary>
/// Represents a single filter pad, which is used for connecting filter inputs and outputs in an FFmpeg filter graph.
/// </summary>
public readonly unsafe struct FilterPad
{
    /// <summary>
    /// The pointer to the array of _AVFilterPad structures.
    /// </summary>
    internal readonly AutoGen._AVFilterPad* pads;

    /// <summary>
    /// The index of the filter pad in the array.
    /// </summary>
    internal readonly int index;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterPad"/> struct.
    /// </summary>
    /// <param name="pads">Pointer to the array of filter pads (<see cref="AutoGen._AVFilterPad"/>).</param>
    /// <param name="index">Index of the specific pad within the array.</param>
    internal FilterPad(AutoGen._AVFilterPad* pads, int index)
    {
        this.pads = pads;
        this.index = index;
    }

    /// <summary>
    /// Gets the media type handled by this filter pad (e.g., video, audio).
    /// </summary>
    public MediaType MediaType => (MediaType)ffmpeg.avfilter_pad_get_type(pads, index);

    /// <summary>
    /// Gets the name of this filter pad.
    /// </summary>
    public string Name => ffmpeg.avfilter_pad_get_name(pads, index);

    /// <summary>
    /// Returns the name of the filter pad as a string.
    /// </summary>
    /// <returns>The name of the filter pad.</returns>
    public override string ToString() => Name;
}



/// <summary>
/// Represents a read-only list of <see cref="FilterPad"/> objects, providing access to the pads in a filter.
/// </summary>
public readonly unsafe struct FilterPadList : IReadOnlyList<FilterPad>
{
    /// <summary>
    /// A pointer to the array of <see cref="AutoGen._AVFilterPad"/> structures.
    /// </summary>
    internal readonly AutoGen._AVFilterPad* pads;

    /// <summary>
    /// The number of filter pads in the list.
    /// </summary>
    private readonly int count;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterPadList"/> struct.
    /// </summary>
    /// <param name="pads">A pointer to the array of filter pads (<see cref="AutoGen._AVFilterPad"/>).</param>
    /// <param name="count">The number of filter pads in the array.</param>
    internal FilterPadList(AutoGen._AVFilterPad* pads, int count)
    {
        this.pads = pads;
        this.count = count;
    }

    /// <summary>
    /// Gets the <see cref="FilterPad"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the pad to retrieve.</param>
    /// <returns>The <see cref="FilterPad"/> at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the index is outside the range of available pads.
    /// </exception>
    public FilterPad this[int index] => index >= 0 && index < Count ? new(pads, index) : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Gets the number of filter pads in the list.
    /// </summary>
    public int Count => count;

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="FilterPadList"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="FilterPadList"/>.</returns>
    public IEnumerator<FilterPad> GetEnumerator() => new FilterPadEnumerator(this);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Provides an enumerator for the <see cref="FilterPadList"/> that supports simple iteration over the collection.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FilterPadEnumerator"/> class.
    /// </remarks>
    /// <param name="filterPadList">The <see cref="FilterPadList"/> to enumerate.</param>
    private class FilterPadEnumerator(FilterPadList filterPadList) : IEnumerator<FilterPad>
    {
        /// <summary>
        /// The current index of the enumerator.
        /// </summary>
        private int currentIndex = -1;

        /// <summary>
        /// The <see cref="FilterPadList"/> being enumerated.
        /// </summary>
        private readonly FilterPadList filterPadList = filterPadList;

        /// <summary>
        /// Gets the current <see cref="FilterPad"/> in the enumeration.
        /// </summary>
        public FilterPad Current => filterPadList[currentIndex];

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Disposes of any resources used by the enumerator.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Advances the enumerator to the next element in the collection.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced; otherwise, <see langword="false"/>.</returns>
        public bool MoveNext() => ++currentIndex < filterPadList.Count;

        /// <summary>
        /// Resets the enumerator to its initial position, before the first element.
        /// </summary>
        public void Reset() => currentIndex = -1;
    }
}


