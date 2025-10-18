using FFmpeg.AutoGen;
using System.Collections;

namespace FFmpeg.Audio;
/// <summary>
/// Represents a read-only list of <see cref="ChannelLayout_ref"/> instances.
/// </summary>
/// <remarks>
/// This class provides an enumerator to iterate over a collection of <see cref="ChannelLayout_ref"/> instances.
/// The list is backed by a pointer to an array of <see cref="_AVChannelLayout"/> and its length.
/// </remarks>
public readonly unsafe struct ChannelLayoutConstList : IReadOnlyList<ChannelLayout_ref>
{
    private readonly _AVChannelLayout* first;
    private readonly int length;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelLayoutConstList"/> class.
    /// </summary>
    /// <param name="first">A pointer to the first <see cref="_AVChannelLayout"/> in the list.</param>
    /// <param name="length">The number of <see cref="_AVChannelLayout"/> items in the list.</param>
    internal ChannelLayoutConstList(_AVChannelLayout* first, int length)
    {
        this.first = first;
        this.length = length;
    }

    /// <summary>
    /// Gets the <see cref="ChannelLayout_ref"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the <see cref="ChannelLayout_ref"/> to get.</param>
    /// <returns>The <see cref="ChannelLayout_ref"/> at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is less than zero or greater than or equal to <see cref="Count"/>.</exception>
    public ChannelLayout_ref this[int index] => index >= 0 && index < length ? new(first + index, true) : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Gets the number of <see cref="ChannelLayout_ref"/> instances in the list.
    /// </summary>
    public int Count => length;

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ChannelLayoutConstList"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="ChannelLayout_ref"/> instances in the list.</returns>
    public IEnumerator<ChannelLayout_ref> GetEnumerator() => new ChannelLayoutConstListIterator(this);

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ChannelLayoutConstList"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="ChannelLayout_ref"/> instances in the list.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelLayoutConstListIterator"/> class.
    /// </summary>
    /// <param name="channelLayoutConstList">The <see cref="ChannelLayoutConstList"/> to iterate over.</param>
    private class ChannelLayoutConstListIterator(ChannelLayoutConstList channelLayoutConstList) : IEnumerator<ChannelLayout_ref>
    {
        private readonly ChannelLayoutConstList channelLayoutConstList = channelLayoutConstList;
        private int index = -1;

        /// <summary>
        /// Gets the current <see cref="ChannelLayout_ref"/> in the iteration.
        /// </summary>
        public ChannelLayout_ref Current => channelLayoutConstList[index];

        /// <summary>
        /// Gets the current <see cref="ChannelLayout_ref"/> as an <see cref="object"/>.
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Releases the resources used by the <see cref="ChannelLayoutConstListIterator"/>.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Advances the enumerator to the next <see cref="ChannelLayout_ref"/> in the list.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator successfully advanced to the next element; otherwise, <see langword="false"/>.</returns>
        public bool MoveNext() => ++index < channelLayoutConstList.Count;

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the list.
        /// </summary>
        public void Reset() => index = -1;
    }
}


