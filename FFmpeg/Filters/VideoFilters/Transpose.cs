using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Rotates video frames by 90° and optionally flips them (FFmpeg filter <c>transpose</c>).
/// </summary>
/// <remarks>
/// <para>
/// For 180° use <see cref="HFlip"/> followed by <see cref="VFlip"/>.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Transpose : FilterContext
{
    internal Transpose(_AVFilterContext* context) : base(context, "transpose")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>transpose</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Transpose Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Transpose, graph));

    /// <summary>
    /// Creates a transpose filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="direction">The rotation and flip.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Transpose Create(string name, TransposeDirection direction, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Direction = direction;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the rotation and flip (option <c>dir</c>).
    /// </summary>
    public TransposeDirection Direction
    {
        get => (TransposeDirection)GetInt64Option("dir");
        set => SetInt64Option("dir", (long)value);
    }

    /// <summary>
    /// Gets or sets for which input geometry the frames are left unchanged (option <c>passthrough</c>).
    /// </summary>
    public TransposePassthrough Passthrough
    {
        get => (TransposePassthrough)GetInt64Option("passthrough");
        set => SetInt64Option("passthrough", (long)value);
    }
}
