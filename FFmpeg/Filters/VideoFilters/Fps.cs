using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Converts the frame rate by duplicating or dropping frames (FFmpeg filter <c>fps</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Fps : FilterContext
{
    internal Fps(_AVFilterContext* context) : base(context, "fps")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>fps</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Fps Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.FPS, graph));

    /// <summary>
    /// Creates an fps filter for a fixed frame rate.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="fps">The output frame rate.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Fps Create(string name, Rational fps, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.FrameRate = fps.ToString();
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates an fps filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="fps">The output frame rate as an expression or a name, e.g. <c>"30000/1001"</c>, <c>"ntsc"</c> or <c>"source_fps*2"</c>.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Fps Create(string name, string fps, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.FrameRate = fps;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the output frame rate (option <c>fps</c>).
    /// </summary>
    public string? FrameRate
    {
        get => GetStringOption("fps");
        set => SetStringOption("fps", value);
    }

    /// <summary>
    /// Gets or sets how output timestamps are rounded (option <c>round</c>).
    /// </summary>
    public FpsRounding Rounding
    {
        get => (FpsRounding)GetInt64Option("round");
        set => SetInt64Option("round", (long)value);
    }

    /// <summary>
    /// Gets or sets what happens to the last frame (option <c>eof_action</c>).
    /// </summary>
    public FpsEofAction EofAction
    {
        get => (FpsEofAction)GetInt64Option("eof_action");
        set => SetInt64Option("eof_action", (long)value);
    }

    /// <summary>
    /// Gets or sets the first output timestamp in seconds (frames before it are dropped or padded) (option <c>start_time</c>).
    /// </summary>
    public double StartTime
    {
        get => GetDoubleOption("start_time");
        set => SetDoubleOption("start_time", value);
    }
}
