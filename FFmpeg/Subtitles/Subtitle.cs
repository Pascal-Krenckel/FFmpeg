using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Subtitles;
/// <summary>
/// Represents a collection of subtitle rectangles and manages subtitle display timings. 
/// This class wraps an FFmpeg <see cref="AutoGen._AVSubtitle"/> structure and provides access to subtitle properties and rectangles.
/// </summary>
public unsafe class Subtitle : IReadOnlyList<SubtitleRect>, IDisposable
{
    internal AutoGen._AVSubtitle subtitle;

    /// <summary>
    /// Gets the time (in milliseconds) when the subtitle should start being displayed.
    /// </summary>
    public uint StartDisplayTime => subtitle.start_display_time;

    /// <summary>
    /// Gets the time (in milliseconds) when the subtitle should stop being displayed.
    /// </summary>
    public uint EndDisplayTime => subtitle.end_display_time;

    /// <summary>
    /// Gets the number of subtitle rectangles contained in this subtitle.
    /// </summary>
    public int Count => (int)subtitle.num_rects;

    /// <summary>
    /// Gets or sets the presentation timestamp (PTS) for the subtitle. This indicates the time when the subtitle should be presented.
    /// </summary>
    public long PresentationTime
    {
        get => subtitle.pts;
        set => subtitle.pts = value;
    }

    /// <summary>
    /// Gets the <see cref="SubtitleRect"/> at the specified index.
    /// </summary>
    /// <param name="index">The index of the subtitle rectangle to retrieve.</param>
    /// <returns>The <see cref="SubtitleRect"/> at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the index is less than 0 or greater than or equal to the number of rectangles in the subtitle.
    /// </exception>
    public SubtitleRect this[int index] => index >= 0 && index < subtitle.num_rects
        ? new(subtitle.rects[index])
        : throw new IndexOutOfRangeException();

    /// <summary>
    /// Returns an enumerator that iterates through the subtitle rectangles.
    /// </summary>
    /// <returns>An enumerator for the collection of subtitle rectangles.</returns>
    public IEnumerator<SubtitleRect> GetEnumerator() => new SubtitleRectIterator(subtitle);

    /// <summary>
    /// Returns a non-generic enumerator that iterates through the subtitle rectangles.
    /// </summary>
    /// <returns>A non-generic enumerator for the collection of subtitle rectangles.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region Dispose

    private bool disposedValue;

    /// <summary>
    /// Releases the resources used by the <see cref="Subtitle"/> instance.
    /// </summary>
    /// <param name="disposing">
    /// Indicates whether the method was called from the <see cref="Dispose()"/> method 
    /// (true) or from a finalizer (false).
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose managed resources here, if any.
            }

            // Free the underlying AVSubtitle resources.
            Free();
            disposedValue = true;
        }
    }

    //ToDo:
    public void Free()
    {
        var subtitle = this.subtitle;
        ffmpeg.avsubtitle_free(&subtitle);
        this.subtitle = subtitle;
    }

    /// <summary>
    /// Releases the resources used by the <see cref="Subtitle"/> instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Helper class that provides an enumerator for the <see cref="SubtitleRect"/> collection.
    /// </summary>
    private class SubtitleRectIterator : IEnumerator<SubtitleRect>
    {
        private int index = -1;
        private AutoGen._AVSubtitle subtitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubtitleRectIterator"/> class.
        /// </summary>
        /// <param name="subtitle">The <see cref="AutoGen._AVSubtitle"/> instance to iterate over.</param>
        public SubtitleRectIterator(AutoGen._AVSubtitle subtitle) => this.subtitle = subtitle;

        /// <summary>
        /// Gets the current <see cref="SubtitleRect"/> in the collection.
        /// </summary>
        public SubtitleRect Current => index >= 0 && index < subtitle.num_rects
            ? new(subtitle.rects[index])
            : throw new IndexOutOfRangeException();

        object IEnumerator.Current => Current;

        public void Dispose() { }

        /// <summary>
        /// Moves to the next <see cref="SubtitleRect"/> in the collection.
        /// </summary>
        /// <returns>True if there are more rectangles; otherwise, false.</returns>
        public bool MoveNext() => ++index < subtitle.num_rects;

        /// <summary>
        /// Resets the enumerator to the initial position, before the first element.
        /// </summary>
        public void Reset() => index = -1;
    }
}
