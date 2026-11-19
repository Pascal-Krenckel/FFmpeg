using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Resamples audio with libswresample (FFmpeg filter <c>aresample</c>).
/// </summary>
/// <remarks>
/// <para>
/// Besides the sample rate, libswresample options such as <c>async</c> (stretch or squeeze to match timestamps),
/// <c>ochl</c> (output channel layout) or <c>osf</c> (output sample format) can be set with
/// <see cref="Options.OptionQueryableBase.SetOption(string, string, bool)"/> before initialization.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class AResample : FilterContext
{
    internal AResample(_AVFilterContext* context) : base(context, "aresample")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>aresample</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static AResample Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.AResample, graph));

    /// <summary>
    /// Creates an aresample filter for a sample rate.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="sampleRate">The output sample rate.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static AResample Create(string name, int sampleRate, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.SampleRate = sampleRate;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the output sample rate (0 keeps the input rate) (option <c>sample_rate</c>).
    /// </summary>
    public int SampleRate
    {
        get => (int)GetInt64Option("sample_rate");
        set => SetInt64Option("sample_rate", value);
    }

    /// <summary>
    /// Gets or sets the maximum compensation in samples per second to match the timestamps (0 disables it, 1 only pads and trims) (option <c>async</c>).
    /// </summary>
    public int Async
    {
        get => (int)GetInt64Option("async");
        set => SetInt64Option("async", value);
    }
}
