using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Mixes several audio inputs into one (FFmpeg filter <c>amix</c>).
/// </summary>
/// <remarks>
/// <para>
/// The filter has <see cref="Inputs"/> input pads. By default the inputs are scaled so that their sum does not clip
/// (<see cref="Normalize"/>).
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class AMix : FilterContext
{
    internal AMix(_AVFilterContext* context) : base(context, "amix")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>amix</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static AMix Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.AMix, graph));

    /// <summary>
    /// Creates an amix filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="inputs">The number of inputs.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static AMix Create(string name, int inputs, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Inputs = inputs;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates an amix filter with an end mode.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="inputs">The number of inputs.</param>
    /// <param name="duration">When the output ends.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static AMix Create(string name, int inputs, AMixDuration duration, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Inputs = inputs;
            filter.Duration = duration;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the number of inputs (option <c>inputs</c>).
    /// </summary>
    public int Inputs
    {
        get => (int)GetInt64Option("inputs");
        set => SetInt64Option("inputs", value);
    }

    /// <summary>
    /// Gets or sets when the output ends (option <c>duration</c>).
    /// </summary>
    public AMixDuration Duration
    {
        get => (AMixDuration)GetInt64Option("duration");
        set => SetInt64Option("duration", (long)value);
    }

    /// <summary>
    /// Gets or sets the time in seconds over which the volume is readjusted when an input ends (option <c>dropout_transition</c>).
    /// </summary>
    public double DropoutTransition
    {
        get => GetDoubleOption("dropout_transition");
        set => SetDoubleOption("dropout_transition", value);
    }

    /// <summary>
    /// Gets or sets the weight of each input, separated by spaces (e.g. <c>"1 0.5"</c>) (option <c>weights</c>).
    /// </summary>
    public string? Weights
    {
        get => GetStringOption("weights");
        set => SetStringOption("weights", value);
    }

    /// <summary>
    /// Gets or sets whether the inputs are scaled to avoid clipping (option <c>normalize</c>).
    /// </summary>
    public bool Normalize
    {
        get => GetBoolOption("normalize");
        set => SetBoolOption("normalize", value);
    }
}
