using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Draws one video on top of another (FFmpeg filter <c>overlay</c>).
/// </summary>
/// <remarks>
/// <para>
/// The filter has two inputs: pad 0 is the main video, pad 1 the video drawn on top. <see cref="X"/> and
/// <see cref="Y"/> are expressions; <c>main_w</c>/<c>main_h</c> and <c>overlay_w</c>/<c>overlay_h</c> refer to the
/// input sizes, e.g. <c>X = "main_w-overlay_w-10"</c> places it 10 pixels from the right edge.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Overlay : FilterContext
{
    internal Overlay(_AVFilterContext* context) : base(context, "overlay")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>overlay</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Overlay Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Overlay, graph));

    /// <summary>
    /// Creates an overlay filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="x">The left edge of the overlay: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="y">The top edge of the overlay: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Overlay Create(string name, string x, string y, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.X = x;
            filter.Y = y;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates an overlay filter at a fixed position.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="x">The left edge of the overlay in pixels.</param>
    /// <param name="y">The top edge of the overlay in pixels.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Overlay Create(string name, int x, int y, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.X = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Y = y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the expression for the left edge of the overlay (option <c>x</c>).
    /// </summary>
    public string? X
    {
        get => GetStringOption("x");
        set => SetStringOption("x", value);
    }

    /// <summary>
    /// Gets or sets the expression for the top edge of the overlay (option <c>y</c>).
    /// </summary>
    public string? Y
    {
        get => GetStringOption("y");
        set => SetStringOption("y", value);
    }

    /// <summary>
    /// Gets or sets what happens when the overlay input ends (option <c>eof_action</c>).
    /// </summary>
    public OverlayEofAction EofAction
    {
        get => (OverlayEofAction)GetInt64Option("eof_action");
        set => SetInt64Option("eof_action", (long)value);
    }

    /// <summary>
    /// Gets or sets when the position expressions are evaluated (option <c>eval</c>).
    /// </summary>
    public ExpressionEvaluation Evaluation
    {
        get => (ExpressionEvaluation)GetInt64Option("eval");
        set => SetInt64Option("eval", (long)value);
    }

    /// <summary>
    /// Gets or sets whether the output ends with the shortest input (option <c>shortest</c>).
    /// </summary>
    public bool Shortest
    {
        get => GetBoolOption("shortest");
        set => SetBoolOption("shortest", value);
    }

    /// <summary>
    /// Gets or sets the format the inputs are blended in (option <c>format</c>).
    /// </summary>
    public OverlayFormat Format
    {
        get => (OverlayFormat)GetInt64Option("format");
        set => SetInt64Option("format", (long)value);
    }

    /// <summary>
    /// Gets or sets whether the last overlay frame is repeated until the main input ends (option <c>repeatlast</c>).
    /// </summary>
    public bool RepeatLast
    {
        get => GetBoolOption("repeatlast");
        set => SetBoolOption("repeatlast", value);
    }

    /// <summary>
    /// Gets or sets whether the overlay alpha is straight or premultiplied (option <c>alpha</c>).
    /// </summary>
    public FFmpeg.Images.AlphaMode AlphaMode
    {
        get => (FFmpeg.Images.AlphaMode)GetInt64Option("alpha");
        set => SetInt64Option("alpha", (long)value);
    }
}
