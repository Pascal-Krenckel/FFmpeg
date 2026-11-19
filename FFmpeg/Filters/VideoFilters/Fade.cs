using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Fades video in from or out to a color (FFmpeg filter <c>fade</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Fade : FilterContext
{
    internal Fade(_AVFilterContext* context) : base(context, "fade")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>fade</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Fade Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Fade, graph));

    /// <summary>
    /// Creates a time-based fade filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="type">Fade in or out.</param>
    /// <param name="start">The timestamp the fade starts at.</param>
    /// <param name="duration">The length of the fade.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Fade Create(string name, FadeType type, TimeSpan start, TimeSpan duration, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Type = type;
            filter.StartTime = start;
            filter.Duration = duration;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the fade direction (option <c>type</c>).
    /// </summary>
    public FadeType Type
    {
        get => (FadeType)GetInt64Option("type");
        set => SetInt64Option("type", (long)value);
    }

    /// <summary>
    /// Gets or sets the number of the first frame of the fade (option <c>start_frame</c>).
    /// </summary>
    public int StartFrame
    {
        get => (int)GetInt64Option("start_frame");
        set => SetInt64Option("start_frame", value);
    }

    /// <summary>
    /// Gets or sets the length of the fade in frames (used if <see cref="Duration"/> is 0) (option <c>nb_frames</c>).
    /// </summary>
    public int FrameCount
    {
        get => (int)GetInt64Option("nb_frames");
        set => SetInt64Option("nb_frames", value);
    }

    /// <summary>
    /// Gets or sets the timestamp the fade starts at (option <c>start_time</c>).
    /// </summary>
    public TimeSpan StartTime
    {
        get => GetDurationOption("start_time");
        set => SetDurationOption("start_time", value);
    }

    /// <summary>
    /// Gets or sets the length of the fade (option <c>duration</c>).
    /// </summary>
    public TimeSpan Duration
    {
        get => GetDurationOption("duration");
        set => SetDurationOption("duration", value);
    }

    /// <summary>
    /// Gets or sets whether only the alpha channel is faded (option <c>alpha</c>).
    /// </summary>
    public bool Alpha
    {
        get => GetBoolOption("alpha");
        set => SetBoolOption("alpha", value);
    }

    /// <summary>
    /// Gets or sets the color faded from or to (FFmpeg reads it back as <c>0xRRGGBBAA</c>) (option <c>color</c>).
    /// </summary>
    public string? Color
    {
        get => GetStringOption("color");
        set => SetStringOption("color", value);
    }
}
