using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Flips video frames vertically (FFmpeg filter <c>vflip</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class VFlip : FilterContext
{
    internal VFlip(_AVFilterContext* context) : base(context, "vflip")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>vflip</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static VFlip Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.VFlip, graph));

    /// <summary>
    /// Creates and initializes the filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static VFlip Create(string name, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Init().ThrowIfError();
        });
}
