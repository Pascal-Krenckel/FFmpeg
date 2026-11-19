using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Deinterlaces video frames (yet another deinterlacing filter) (FFmpeg filter <c>yadif</c>).
/// </summary>
/// <remarks>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Yadif : FilterContext
{
    internal Yadif(_AVFilterContext* context) : base(context, "yadif")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>yadif</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Yadif Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Yadif, graph));

    /// <summary>
    /// Creates and initializes the filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Yadif Create(string name, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates a yadif filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="mode">What to output.</param>
    /// <param name="parity">The assumed field order.</param>
    /// <param name="deinterlace">Which frames to deinterlace.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Yadif Create(string name, YadifMode mode, YadifParity parity, YadifDeinterlace deinterlace, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Mode = mode;
            filter.Parity = parity;
            filter.Deinterlace = deinterlace;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets what the filter outputs (option <c>mode</c>).
    /// </summary>
    public YadifMode Mode
    {
        get => (YadifMode)GetInt64Option("mode");
        set => SetInt64Option("mode", (long)value);
    }

    /// <summary>
    /// Gets or sets the assumed field order (option <c>parity</c>).
    /// </summary>
    public YadifParity Parity
    {
        get => (YadifParity)GetInt64Option("parity");
        set => SetInt64Option("parity", (long)value);
    }

    /// <summary>
    /// Gets or sets which frames are deinterlaced (option <c>deint</c>).
    /// </summary>
    public YadifDeinterlace Deinterlace
    {
        get => (YadifDeinterlace)GetInt64Option("deint");
        set => SetInt64Option("deint", (long)value);
    }
}
