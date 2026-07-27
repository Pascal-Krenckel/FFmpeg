using FFmpeg.AutoGen;
using FFmpeg.Unsafe;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;
/// <summary>
/// Represents a filter used in FFmpeg's filter graph. Provides information about filters, including their names, descriptions, inputs, outputs, and flags.
/// </summary>
public readonly unsafe struct Filter : IEquatable<Filter>, IAVPointer<_AVFilter>
{
    /// <summary>
    /// Pointer to the unmanaged <see cref="AutoGen._AVFilter"/> structure used by FFmpeg.
    /// </summary>
    internal readonly AutoGen._AVFilter* filter;

    readonly _AVFilter* IAVPointer<_AVFilter>.Pointer => filter;

    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> struct with the given FFmpeg filter.
    /// </summary>
    /// <param name="filter">Pointer to the FFmpeg filter.</param>
    internal Filter(AutoGen._AVFilter* filter) => this.filter = filter;

    /// <summary>
    /// Gets a read-only collection of all available filters in FFmpeg.
    /// The filters are initialized lazily and cached for future access.
    /// </summary>
    public static ReadOnlyCollection<Filter> AllFilters => field ??= InitAllFilters();

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
    public string? Name => Marshal.PtrToStringUTF8((nint)filter->name);

    /// <summary>
    /// Gets a human-readable description of the filter.
    /// </summary>
    /// <value>
    /// A description of the filter, or <see langword="null"/> if the filter does
    /// not provide one.
    /// </value>>
    public string? Description => Marshal.PtrToStringUTF8((nint)filter->description);

    /// <summary>
    /// Gets the filter's flags, indicating its properties and behavior.
    /// </summary>
    public FilterFlags Flags => (FilterFlags)filter->flags;

    /// <summary>
    /// Gets the input pads supported by the filter.
    /// </summary>
    public FilterPadList Inputs => new(filter->inputs, (int)ffmpeg.avfilter_filter_pad_count(filter, 0));

    /// <summary>
    /// Gets the output pads supported by the filter.
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
    public static Filter GetFilterByName(string name) => TryGetFilterByName(name, out Filter filter) ? filter : throw new ArgumentException(
    $"No filter named '{name}' exists.",
    nameof(name));

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

    /// <summary>
    /// Gets a value indicating whether the filter acts as a source filter.
    /// </summary>
    /// <remarks>
    /// Source filters have no input pads and generate media for a filter graph.
    /// </remarks>
    public bool IsSourceFilter => (ffmpeg.avfilter_filter_pad_count(filter, 0) | (uint)(filter->flags & AutoGen.ffmpeg.AVFILTER_FLAG_DYNAMIC_INPUTS)) == 0;

    /// <summary>
    /// Gets a value indicating whether the filter acts as a sink filter.
    /// </summary>
    /// <remarks>
    /// Sink filters have no output pads and consume media from a filter graph.
    /// </remarks>
    public bool IsSinkFilter => (ffmpeg.avfilter_filter_pad_count(filter, 1) | (uint)(filter->flags & AutoGen.ffmpeg.AVFILTER_FLAG_DYNAMIC_OUTPUTS)) == 0;

    /// <summary>
    /// Gets the built-in video null sink filter.
    /// </summary>
    /// <remarks>
    /// The null sink filter consumes video frames without producing any output.
    /// It is useful when the output of a filter graph does not need to be retrieved.
    /// </remarks>
    public static Filter VideoNullSink => GetFilterByName("nullsink");

    /// <summary>
    /// Gets the built-in audio null sink filter.
    /// </summary>
    /// <remarks>
    /// The null sink filter consumes audio frames without producing any output.
    /// It is useful when the output of a filter graph does not need to be retrieved.
    /// </remarks>
    public static Filter AudioNullSink => GetFilterByName("anullsink");

    /// <summary>
    /// Gets the built-in video null source filter.
    /// </summary>
    /// <remarks>
    /// The null source filter generates synthetic video frames and is commonly used
    /// as the starting point of a filter graph when no external video input is available.
    /// </remarks>
    public static Filter VideoNullSource => GetFilterByName("nullsrc");

    /// <summary>
    /// Gets the built-in audio null source filter.
    /// </summary>
    /// <remarks>
    /// The null source filter generates synthetic audio samples and is commonly used
    /// as the starting point of a filter graph when no external audio input is available.
    /// </remarks>
    public static Filter AudioNullSource => GetFilterByName("anullsrc");

    /// <summary>
    /// Packs two video streams into a stereoscopic video using one of the supported frame packing layouts.
    /// </summary>
    /// <remarks>
    /// The filter combines two input video streams into a single stereoscopic output stream and sets
    /// the appropriate stereo metadata when supported by the output codec.
    ///
    /// Both input streams must have the same frame size and frame rate. Processing stops when the
    /// shorter input stream reaches the end.
    ///
    /// Common packing formats include side-by-side, top-and-bottom, line-interleaved,
    /// column-interleaved, and frame-sequential.
    /// </remarks>
    public static Filter FramePack => GetFilterByName("framepack");

    /// <summary>
    /// Returns the name of the filter as a string.
    /// </summary>
    /// <returns>The name of the filter.</returns>
    public override string? ToString() => Name;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Filter filter && Equals(filter);
    /// <inheritdoc />
    public bool Equals(Filter other) => filter == other.filter;
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine((nint)filter);
    /// <inheritdoc />
    public static bool operator ==(Filter left, Filter right) => left.Equals(right);
    /// <inheritdoc />
    public static bool operator !=(Filter left, Filter right) => !(left == right);
}
