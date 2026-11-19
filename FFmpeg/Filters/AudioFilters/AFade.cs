using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Fades audio in or out (FFmpeg filter <c>afade</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class AFade : FilterContext
{
    internal AFade(_AVFilterContext* context) : base(context, "afade")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>afade</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static AFade Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.AFade, graph));

    /// <summary>
    /// Creates a time-based afade filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="type">Fade in or out.</param>
    /// <param name="start">The time the fade starts at.</param>
    /// <param name="duration">The length of the fade.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static AFade Create(string name, FadeType type, TimeSpan start, TimeSpan duration, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Type = type;
            filter.StartTime = start;
            filter.Duration = duration;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates a time-based afade filter with a curve.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="type">Fade in or out.</param>
    /// <param name="start">The time the fade starts at.</param>
    /// <param name="duration">The length of the fade.</param>
    /// <param name="curve">The fade curve.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static AFade Create(string name, FadeType type, TimeSpan start, TimeSpan duration, AudioFadeCurve curve, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Type = type;
            filter.StartTime = start;
            filter.Duration = duration;
            filter.Curve = curve;
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
    /// Gets or sets the number of the first sample of the fade (option <c>start_sample</c>).
    /// </summary>
    public long StartSample
    {
        get => GetInt64Option("start_sample");
        set => SetInt64Option("start_sample", value);
    }

    /// <summary>
    /// Gets or sets the length of the fade in samples (used if <see cref="Duration"/> is 0) (option <c>nb_samples</c>).
    /// </summary>
    public long SampleCount
    {
        get => GetInt64Option("nb_samples");
        set => SetInt64Option("nb_samples", value);
    }

    /// <summary>
    /// Gets or sets the time the fade starts at (option <c>start_time</c>).
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
    /// Gets or sets the fade curve (option <c>curve</c>).
    /// </summary>
    public AudioFadeCurve Curve
    {
        get => (AudioFadeCurve)GetInt64Option("curve");
        set => SetInt64Option("curve", (long)value);
    }

    /// <summary>
    /// Gets or sets the gain at the silent end of the fade (option <c>silence</c>).
    /// </summary>
    public double Silence
    {
        get => GetDoubleOption("silence");
        set => SetDoubleOption("silence", value);
    }

    /// <summary>
    /// Gets or sets the gain at the full end of the fade (option <c>unity</c>).
    /// </summary>
    public double Unity
    {
        get => GetDoubleOption("unity");
        set => SetDoubleOption("unity", value);
    }
}
