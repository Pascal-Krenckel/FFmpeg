using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Sets the sample (pixel) aspect ratio without changing the pixels (FFmpeg filter <c>setsar</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class SetSar : FilterContext
{
    internal SetSar(_AVFilterContext* context) : base(context, "setsar")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>setsar</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static SetSar Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.SetSar, graph));

    /// <summary>
    /// Creates a setsar filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="sar">The sample aspect ratio, e.g. 1/1 for square pixels.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static SetSar Create(string name, Rational sar, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Ratio = sar.ToString();
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the sample aspect ratio expression (option <c>sar</c>).
    /// </summary>
    public string? Ratio
    {
        get => GetStringOption("sar");
        set => SetStringOption("sar", value);
    }

    /// <summary>
    /// Gets or sets the maximum numerator and denominator used to reduce the ratio (option <c>max</c>).
    /// </summary>
    public int Max
    {
        get => (int)GetInt64Option("max");
        set => SetInt64Option("max", value);
    }
}
