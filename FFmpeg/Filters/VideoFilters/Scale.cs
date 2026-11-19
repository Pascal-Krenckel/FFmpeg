using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Resizes video frames and converts their pixel format (FFmpeg filter <c>scale</c>).
/// </summary>
/// <remarks>
/// <para>
/// Width and height are expressions: <c>-1</c> keeps the aspect ratio, <c>-2</c> does the same but keeps the
/// value divisible by 2, and <c>iw</c>/<c>ih</c> refer to the input size.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Scale : FilterContext
{
    internal Scale(_AVFilterContext* context) : base(context, "scale")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>scale</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Scale Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Scale, graph));

    /// <summary>
    /// Creates a scale filter with width and height expressions.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="width">The output width: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="height">The output height: an expression (e.g. <c>"iw/2"</c>) or a number.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Scale Create(string name, string width, string height, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Width = width;
            filter.Height = height;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates a scale filter with a fixed output size.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="width">The output width in pixels, or -1/-2 to derive it from the height.</param>
    /// <param name="height">The output height in pixels, or -1/-2 to derive it from the width.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Scale Create(string name, int width, int height, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Width = width.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Height = height.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates a scale filter that fits the input into (or around) a box while keeping its aspect ratio.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="width">The maximum (or minimum) width in pixels.</param>
    /// <param name="height">The maximum (or minimum) height in pixels.</param>
    /// <param name="aspectRatioMode">How to keep the input aspect ratio.</param>
    /// <param name="divisibleBy">The output size is made divisible by this value (e.g. 2 for most encoders).</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Scale Create(string name, int width, int height, ScaleAspectRatioMode aspectRatioMode, int divisibleBy, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Width = width.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.Height = height.ToString(System.Globalization.CultureInfo.InvariantCulture);
            filter.AspectRatioMode = aspectRatioMode;
            filter.DivisibleBy = divisibleBy;
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
    /// Gets or sets the libswscale flags, e.g. <c>"bicubic"</c> or <c>"lanczos+accurate_rnd"</c> (option <c>flags</c>).
    /// </summary>
    public string? Flags
    {
        get => GetStringOption("flags");
        set => SetStringOption("flags", value);
    }

    /// <summary>
    /// Gets or sets whether the input is scaled as interlaced fields (option <c>interl</c>).
    /// </summary>
    public bool Interlaced
    {
        get => GetBoolOption("interl");
        set => SetBoolOption("interl", value);
    }

    /// <summary>
    /// Gets or sets how the input aspect ratio is kept (option <c>force_original_aspect_ratio</c>).
    /// </summary>
    public ScaleAspectRatioMode AspectRatioMode
    {
        get => (ScaleAspectRatioMode)GetInt64Option("force_original_aspect_ratio");
        set => SetInt64Option("force_original_aspect_ratio", (long)value);
    }

    /// <summary>
    /// Gets or sets the value the output size is made divisible by when the aspect ratio is kept (option <c>force_divisible_by</c>).
    /// </summary>
    public int DivisibleBy
    {
        get => (int)GetInt64Option("force_divisible_by");
        set => SetInt64Option("force_divisible_by", value);
    }

    /// <summary>
    /// Gets or sets whether the output gets square pixels when scaling proportionally (option <c>reset_sar</c>).
    /// </summary>
    public bool ResetSar
    {
        get => GetBoolOption("reset_sar");
        set => SetBoolOption("reset_sar", value);
    }

    /// <summary>
    /// Gets or sets when the size expressions are evaluated (option <c>eval</c>).
    /// </summary>
    public ExpressionEvaluation Evaluation
    {
        get => (ExpressionEvaluation)GetInt64Option("eval");
        set => SetInt64Option("eval", (long)value);
    }
}
