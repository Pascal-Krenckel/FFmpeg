using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Changes the presentation timestamps of audio frames (FFmpeg filter <c>asetpts</c>).
/// </summary>
/// <remarks>
/// <para>
/// The expression can use e.g. <c>PTS</c>, <c>STARTPTS</c>, <c>NB_CONSUMED_SAMPLES</c> and <c>SR</c>;
/// <c>"N/SR/TB"</c> creates gapless timestamps from the sample count.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class ASetPts : FilterContext
{
    internal ASetPts(_AVFilterContext* context) : base(context, "asetpts")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>asetpts</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static ASetPts Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.ASetPts, graph));

    /// <summary>
    /// Creates an asetpts filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="expression">The timestamp expression.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static ASetPts Create(string name, string expression, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Expression = expression;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the timestamp expression (option <c>expr</c>).
    /// </summary>
    public string? Expression
    {
        get => GetStringOption("expr");
        set => SetStringOption("expr", value);
    }
}
