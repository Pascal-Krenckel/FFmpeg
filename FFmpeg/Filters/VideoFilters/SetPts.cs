using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Changes the presentation timestamps of video frames (FFmpeg filter <c>setpts</c>).
/// </summary>
/// <remarks>
/// <para>
/// The expression can use e.g. <c>PTS</c>, <c>STARTPTS</c>, <c>N</c> (frame number), <c>TB</c> and <c>T</c>:
/// <c>"PTS-STARTPTS"</c> starts at 0, <c>"0.5*PTS"</c> doubles the speed.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class SetPts : FilterContext
{
    internal SetPts(_AVFilterContext* context) : base(context, "setpts")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>setpts</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static SetPts Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.SetPts, graph));

    /// <summary>
    /// Creates a setpts filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="expression">The timestamp expression.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static SetPts Create(string name, string expression, FilterGraph graph)
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

    /// <summary>
    /// Gets or sets whether the frame rate of the output is unset (option <c>strip_fps</c>).
    /// </summary>
    public bool StripFps
    {
        get => GetBoolOption("strip_fps");
        set => SetBoolOption("strip_fps", value);
    }
}
