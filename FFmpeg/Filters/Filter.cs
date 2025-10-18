using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;
/// <summary>
/// Represents a filter used in FFmpeg's filter graph. Provides information about filters, including their names, descriptions, inputs, outputs, and flags.
/// </summary>
public readonly unsafe struct Filter : IEquatable<Filter>
{
    /// <summary>
    /// Pointer to the unmanaged <see cref="AutoGen._AVFilter"/> structure used by FFmpeg.
    /// </summary>
    internal readonly AutoGen._AVFilter* filter;

    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> struct with the given FFmpeg filter.
    /// </summary>
    /// <param name="filter">Pointer to the FFmpeg filter.</param>
    internal Filter(AutoGen._AVFilter* filter) => this.filter = filter;

    private static ReadOnlyCollection<Filter>? allFilters;

    /// <summary>
    /// Gets a read-only collection of all available filters in FFmpeg.
    /// The filters are initialized lazily and cached for future access.
    /// </summary>
    public static ReadOnlyCollection<Filter> AllFilters => allFilters ??= InitAllFilters();

    /// <summary>
    /// Initializes and retrieves all available FFmpeg filters.
    /// </summary>
    /// <returns>A read-only collection of all available filters.</returns>
    private static ReadOnlyCollection<Filter> InitAllFilters()
    {
        List<Filter> filters = [];
        void* iterState = null;
        AutoGen._AVFilter* filter;
        while ((filter = ffmpeg.av_filter_iterate(&iterState)) != null)
            filters.Add(new(filter));
        return new(filters);
    }

    /// <summary>
    /// Gets the name of the filter.
    /// </summary>
    public string Name => Marshal.PtrToStringUTF8((nint)filter->name);

    /// <summary>
    /// Gets the description of the filter.
    /// </summary>
    public string Description => Marshal.PtrToStringUTF8((nint)filter->description);

    /// <summary>
    /// Gets the filter's flags, indicating its properties and behavior.
    /// </summary>
    public FilterFlags Flags => (FilterFlags)filter->flags;

    /// <summary>
    /// Gets the list of input pads for this filter.
    /// Handles dynamic input pads if specified by the filter flags.
    /// </summary>
    public FilterPadList Inputs => new(filter->inputs, (int)ffmpeg.avfilter_filter_pad_count(filter, 0));

    /// <summary>
    /// Gets the list of output pads for this filter.
    /// Handles dynamic output pads if specified by the filter flags.
    /// </summary>
    public FilterPadList Outputs => new(filter->outputs, (int)ffmpeg.avfilter_filter_pad_count(filter, 1));

    /// <summary>
    /// Attempts to retrieve a filter by its name.
    /// </summary>
    /// <param name="name">The name of the filter to retrieve.</param>
    /// <param name="filter">When this method returns, contains the filter if found; otherwise, contains the default value.</param>
    /// <returns><c>true</c> if the filter is found; otherwise, <c>false</c>.</returns>
    public static bool TryGetFilterByName(string name, out Filter filter)
    {
        AutoGen._AVFilter* f = ffmpeg.avfilter_get_by_name(name);
        if (f == null)
        {
            filter = default;
            return false;
        }
        filter = new(f);
        return true;
    }

    /// <summary>
    /// Retrieves a filter by its name.
    /// </summary>
    /// <param name="name">The name of the filter to retrieve.</param>
    /// <returns>The filter associated with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if the filter with the specified name is not found.</exception>
    public static Filter GetFilterByName(string name) => TryGetFilterByName(name, out Filter filter) ? filter : throw new ArgumentException(nameof(name));

    /// <summary>
    /// Gets the video buffer source filter, used to create frames in filter graphs.
    /// </summary>
    public static Filter VideoBufferSource => GetFilterByName("buffer");

    /// <summary>
    /// Gets the video buffer sink filter, used to receive frames in filter graphs.
    /// </summary>
    public static Filter VideoBufferSink => GetFilterByName("buffersink");

    /// <summary>
    /// Gets the scale filter, used to resize video frames.
    /// </summary>
    public static Filter Scale => GetFilterByName("scale");

    /// <summary>
    /// Gets the FPS filter, used to change the frame rate of video streams.
    /// </summary>
    public static Filter FPS => GetFilterByName("fps");

    /// <summary>
    /// Gets the audio buffer source filter, used to create audio frames in filter graphs.
    /// </summary>
    public static Filter AudioBufferSource => GetFilterByName("abuffer");

    /// <summary>
    /// Gets the audio buffer sink filter, used to receive audio frames in filter graphs.
    /// </summary>
    public static Filter AudioBufferSink => GetFilterByName("abuffersink");

    public bool IsSourceFilter => (ffmpeg.avfilter_filter_pad_count(filter, 0) | (uint)(filter->flags & (AutoGen.ffmpeg.AVFILTER_FLAG_DYNAMIC_INPUTS))) == 0;

    public bool IsSinkFilter => (ffmpeg.avfilter_filter_pad_count(filter, 1) | (uint)(filter->flags & (AutoGen.ffmpeg.AVFILTER_FLAG_DYNAMIC_OUTPUTS))) == 0;

    public static Filter VideoNullSink => GetFilterByName("nullsink");
    public static Filter AudioNullSink => GetFilterByName("anullsink");

    public static Filter VideoNullSource => GetFilterByName("nullsrc");
    public static Filter AudioNullSource => GetFilterByName("anullsrc");

    /// <summary>
    /// Returns the name of the filter as a string.
    /// </summary>
    /// <returns>The name of the filter.</returns>
    public override string ToString() => Name;
    public override bool Equals(object? obj) => obj is Filter filter && Equals(filter);
    public bool Equals(Filter other) => Name == other.Name;
    public override int GetHashCode() => HashCode.Combine(Name);

    public static bool operator ==(Filter left, Filter right) => left.Equals(right);
    public static bool operator !=(Filter left, Filter right) => !(left == right);
}
