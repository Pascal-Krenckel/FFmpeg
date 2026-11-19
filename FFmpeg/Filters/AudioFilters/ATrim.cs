using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Keeps only a time range of the audio, cutting frames sample-exactly (FFmpeg filter <c>atrim</c>).
/// </summary>
/// <remarks>
/// <para>
/// Times are compared with the frame timestamps (they are not reset to 0; add <see cref="ASetPts"/> with
/// <c>"PTS-STARTPTS"</c> for that).
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class ATrim : FilterContext
{
    internal ATrim(_AVFilterContext* context) : base(context, "atrim")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>atrim</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static ATrim Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.ATrim, graph));

    /// <summary>
    /// Creates an atrim filter for a time range.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="start">The time of the first sample to keep.</param>
    /// <param name="end">The time of the first sample to drop.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static ATrim Create(string name, TimeSpan start, TimeSpan end, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Start = start;
            filter.End = end;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the time of the first sample to keep (option <c>start</c>).
    /// </summary>
    public TimeSpan Start
    {
        get => GetDurationOption("start");
        set => SetDurationOption("start", value);
    }

    /// <summary>
    /// Gets or sets the time of the first sample to drop (option <c>end</c>).
    /// </summary>
    public TimeSpan End
    {
        get => GetDurationOption("end");
        set => SetDurationOption("end", value);
    }

    /// <summary>
    /// Gets or sets the first timestamp to keep in the input time base (option <c>start_pts</c>).
    /// </summary>
    public long StartPts
    {
        get => GetInt64Option("start_pts");
        set => SetInt64Option("start_pts", value);
    }

    /// <summary>
    /// Gets or sets the first timestamp to drop in the input time base (option <c>end_pts</c>).
    /// </summary>
    public long EndPts
    {
        get => GetInt64Option("end_pts");
        set => SetInt64Option("end_pts", value);
    }

    /// <summary>
    /// Gets or sets the maximum duration of the output (option <c>duration</c>).
    /// </summary>
    public TimeSpan Duration
    {
        get => GetDurationOption("duration");
        set => SetDurationOption("duration", value);
    }

    /// <summary>
    /// Gets or sets the number of the first sample to keep (option <c>start_sample</c>).
    /// </summary>
    public long StartSample
    {
        get => GetInt64Option("start_sample");
        set => SetInt64Option("start_sample", value);
    }

    /// <summary>
    /// Gets or sets the number of the first sample to drop (option <c>end_sample</c>).
    /// </summary>
    public long EndSample
    {
        get => GetInt64Option("end_sample");
        set => SetInt64Option("end_sample", value);
    }
}
