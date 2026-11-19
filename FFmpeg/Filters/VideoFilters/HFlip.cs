using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Mirrors video frames horizontally (FFmpeg filter <c>hflip</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class HFlip : FilterContext
{
    internal HFlip(_AVFilterContext* context) : base(context, "hflip")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>hflip</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static HFlip Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.HFlip, graph));

    /// <summary>
    /// Creates and initializes the filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static HFlip Create(string name, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Init().ThrowIfError();
        });
}
