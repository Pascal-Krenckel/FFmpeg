using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Sets the display aspect ratio by changing the sample aspect ratio, without changing the pixels (FFmpeg filter <c>setdar</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class SetDar : FilterContext
{
    internal SetDar(_AVFilterContext* context) : base(context, "setdar")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>setdar</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static SetDar Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.SetDar, graph));

    /// <summary>
    /// Creates a setdar filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="dar">The display aspect ratio, e.g. 16/9.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static SetDar Create(string name, Rational dar, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Ratio = dar.ToString();
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the display aspect ratio expression (option <c>dar</c>).
    /// </summary>
    public string? Ratio
    {
        get => GetStringOption("dar");
        set => SetStringOption("dar", value);
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
