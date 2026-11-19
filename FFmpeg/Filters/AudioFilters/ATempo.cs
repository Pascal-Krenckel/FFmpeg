using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Changes the audio tempo without changing the pitch (FFmpeg filter <c>atempo</c>).
/// </summary>
/// <remarks>
/// <para>
/// The factor must be between 0.5 and 100; chain several filters for slower speeds.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class ATempo : FilterContext
{
    internal ATempo(_AVFilterContext* context) : base(context, "atempo")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>atempo</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static ATempo Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.ATempo, graph));

    /// <summary>
    /// Creates an atempo filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="tempo">The tempo factor, e.g. 2 for double speed.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static ATempo Create(string name, double tempo, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Tempo = tempo;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the tempo factor (option <c>tempo</c>).
    /// </summary>
    public double Tempo
    {
        get => GetDoubleOption("tempo");
        set => SetDoubleOption("tempo", value);
    }
}
