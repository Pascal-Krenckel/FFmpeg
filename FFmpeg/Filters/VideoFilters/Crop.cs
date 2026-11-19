using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Crops video frames to a rectangle (FFmpeg filter <c>crop</c>).
/// </summary>
/// <remarks>
/// <para>
/// All values are expressions; <c>iw</c>/<c>ih</c> refer to the input size and <c>ow</c>/<c>oh</c> to the output size.
/// The defaults crop the center.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Crop : FilterContext
{
    internal Crop(_AVFilterContext* context) : base(context, "crop")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>crop</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Crop Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Crop, graph));

    /// <summary>
    /// Creates a crop filter that keeps the centered area of the given size.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="width">The output width: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="height">The output height: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Crop Create(string name, string width, string height, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Width = width;
            filter.Height = height;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates a crop filter for a fixed rectangle.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="width">The output width in pixels.</param>
    /// <param name="height">The output height in pixels.</param>
    /// <param name="x">The left edge of the area in pixels.</param>
    /// <param name="y">The top edge of the area in pixels.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Crop Create(string name, int width, int height, int x, int y, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Width = width.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Height = height.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.X = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Y = y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the output width expression (option <c>w</c>).
    /// </summary>
    public string? Width
    {
        get => GetStringOption("w");
        set => SetStringOption("w", value);
    }

    /// <summary>
    /// Gets or sets the output height expression (option <c>h</c>).
    /// </summary>
    public string? Height
    {
        get => GetStringOption("h");
        set => SetStringOption("h", value);
    }

    /// <summary>
    /// Gets or sets the expression for the left edge (option <c>x</c>).
    /// </summary>
    public string? X
    {
        get => GetStringOption("x");
        set => SetStringOption("x", value);
    }

    /// <summary>
    /// Gets or sets the expression for the top edge (option <c>y</c>).
    /// </summary>
    public string? Y
    {
        get => GetStringOption("y");
        set => SetStringOption("y", value);
    }

    /// <summary>
    /// Gets or sets whether the display aspect ratio of the input is kept (option <c>keep_aspect</c>).
    /// </summary>
    public bool KeepAspect
    {
        get => GetBoolOption("keep_aspect");
        set => SetBoolOption("keep_aspect", value);
    }

    /// <summary>
    /// Gets or sets whether the position is used exactly instead of being rounded for chroma subsampling (option <c>exact</c>).
    /// </summary>
    public bool Exact
    {
        get => GetBoolOption("exact");
        set => SetBoolOption("exact", value);
    }
}
