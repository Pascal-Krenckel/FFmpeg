using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Changes the audio volume (FFmpeg filter <c>volume</c>).
/// </summary>
/// <remarks>
/// <para>
/// The volume is an expression: a factor (<c>"0.5"</c>), a value in dB (<c>"-6dB"</c>) or a formula that can
/// use e.g. <c>t</c> (time in seconds) when <see cref="Evaluation"/> is <see cref="ExpressionEvaluation.PerFrame"/>.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Volume : FilterContext
{
    internal Volume(_AVFilterContext* context) : base(context, "volume")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>volume</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Volume Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Volume, graph));

    /// <summary>
    /// Creates a volume filter with a factor.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="volume">The volume factor (1 keeps the volume).</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Volume Create(string name, double volume, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Expression = volume.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Creates a volume filter with an expression.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="volume">The volume expression, e.g. <c>"-6dB"</c>.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Volume Create(string name, string volume, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.Expression = volume;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the volume expression (option <c>volume</c>).
    /// </summary>
    public string? Expression
    {
        get => GetStringOption("volume");
        set => SetStringOption("volume", value);
    }

    /// <summary>
    /// Gets or sets the precision used for the computation (option <c>precision</c>).
    /// </summary>
    public VolumePrecision Precision
    {
        get => (VolumePrecision)GetInt64Option("precision");
        set => SetInt64Option("precision", (long)value);
    }

    /// <summary>
    /// Gets or sets when the volume expression is evaluated (option <c>eval</c>).
    /// </summary>
    public ExpressionEvaluation Evaluation
    {
        get => (ExpressionEvaluation)GetInt64Option("eval");
        set => SetInt64Option("eval", (long)value);
    }

    /// <summary>
    /// Gets or sets how ReplayGain side data is used (option <c>replaygain</c>).
    /// </summary>
    public ReplayGainMode ReplayGain
    {
        get => (ReplayGainMode)GetInt64Option("replaygain");
        set => SetInt64Option("replaygain", (long)value);
    }

    /// <summary>
    /// Gets or sets the pre-amplification in dB applied with ReplayGain (option <c>replaygain_preamp</c>).
    /// </summary>
    public double ReplayGainPreamp
    {
        get => GetDoubleOption("replaygain_preamp");
        set => SetDoubleOption("replaygain_preamp", value);
    }

    /// <summary>
    /// Gets or sets whether ReplayGain avoids clipping (option <c>replaygain_noclip</c>).
    /// </summary>
    public bool ReplayGainNoClip
    {
        get => GetBoolOption("replaygain_noclip");
        set => SetBoolOption("replaygain_noclip", value);
    }
}
