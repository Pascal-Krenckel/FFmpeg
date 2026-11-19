using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Adds borders around video frames (FFmpeg filter <c>pad</c>).
/// </summary>
/// <remarks>
/// <para>
/// All sizes and positions are expressions; <c>iw</c>/<c>ih</c> refer to the input size and <c>ow</c>/<c>oh</c> to
/// the output size, e.g. <c>x = "(ow-iw)/2"</c> centers the input.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Pad : FilterContext
{
    internal Pad(_AVFilterContext* context) : base(context, "pad")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>pad</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Pad Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Pad, graph));

    /// <summary>
    /// Creates a pad filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="width">The output width: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="height">The output height: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="x">The left edge of the input: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="y">The top edge of the input: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="color">The border color, e.g. <c>"black"</c> or <c>"0x00000000"</c>.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Pad Create(string name, string width, string height, string x, string y, string color, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Width = width;
            filter.Height = height;
            filter.X = x;
            filter.Y = y;
            filter.Color = color;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates a pad filter that centers the input in black borders (letter- or pillarboxing).
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="width">The output width in pixels.</param>
    /// <param name="height">The output height in pixels.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Pad Create(string name, int width, int height, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Width = width.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Height = height.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.X = "(ow-iw)/2";
            filter.Y = "(oh-ih)/2";
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
    /// Gets or sets the expression for the left edge of the input (option <c>x</c>).
    /// </summary>
    public string? X
    {
        get => GetStringOption("x");
        set => SetStringOption("x", value);
    }

    /// <summary>
    /// Gets or sets the expression for the top edge of the input (option <c>y</c>).
    /// </summary>
    public string? Y
    {
        get => GetStringOption("y");
        set => SetStringOption("y", value);
    }

    /// <summary>
    /// Gets or sets the border color (FFmpeg reads it back as <c>0xRRGGBBAA</c>) (option <c>color</c>).
    /// </summary>
    public string? Color
    {
        get => GetStringOption("color");
        set => SetStringOption("color", value);
    }

    /// <summary>
    /// Gets or sets the display aspect ratio to pad to (0 to use the size) (option <c>aspect</c>).
    /// </summary>
    public Rational Aspect
    {
        get => GetRationalOption("aspect");
        set => SetRationalOption("aspect", value);
    }

    /// <summary>
    /// Gets or sets when the expressions are evaluated (option <c>eval</c>).
    /// </summary>
    public ExpressionEvaluation Evaluation
    {
        get => (ExpressionEvaluation)GetInt64Option("eval");
        set => SetInt64Option("eval", (long)value);
    }
}
