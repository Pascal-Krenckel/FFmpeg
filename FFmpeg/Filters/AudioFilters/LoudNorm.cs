using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Normalizes loudness according to EBU R128 (FFmpeg filter <c>loudnorm</c>).
/// </summary>
/// <remarks>
/// <para>
/// In dynamic mode the filter works in one pass. For linear normalization run it once to measure, then set the
/// <c>Measured*</c> properties from that run (see the FFmpeg documentation). The filter resamples to 192 kHz
/// internally; add <see cref="AResample"/> afterwards to get the original rate back.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class LoudNorm : FilterContext
{
    internal LoudNorm(_AVFilterContext* context) : base(context, "loudnorm")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>loudnorm</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static LoudNorm Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.LoudNorm, graph));

    /// <summary>
    /// Creates a loudnorm filter with targets.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="integrated">The integrated loudness target in LUFS (-70 to -5).</param>
    /// <param name="loudnessRange">The loudness range target in LU (1 to 50).</param>
    /// <param name="truePeak">The maximum true peak in dBTP (-9 to 0).</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static LoudNorm Create(string name, double integrated, double loudnessRange, double truePeak, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.IntegratedLoudness = integrated;
            filter.LoudnessRange = loudnessRange;
            filter.TruePeak = truePeak;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the integrated loudness target in LUFS (option <c>I</c>).
    /// </summary>
    public double IntegratedLoudness
    {
        get => GetDoubleOption("I");
        set => SetDoubleOption("I", value);
    }

    /// <summary>
    /// Gets or sets the loudness range target in LU (option <c>LRA</c>).
    /// </summary>
    public double LoudnessRange
    {
        get => GetDoubleOption("LRA");
        set => SetDoubleOption("LRA", value);
    }

    /// <summary>
    /// Gets or sets the maximum true peak in dBTP (option <c>TP</c>).
    /// </summary>
    public double TruePeak
    {
        get => GetDoubleOption("TP");
        set => SetDoubleOption("TP", value);
    }

    /// <summary>
    /// Gets or sets the measured integrated loudness of a first pass (option <c>measured_I</c>).
    /// </summary>
    public double MeasuredIntegratedLoudness
    {
        get => GetDoubleOption("measured_I");
        set => SetDoubleOption("measured_I", value);
    }

    /// <summary>
    /// Gets or sets the measured loudness range of a first pass (option <c>measured_LRA</c>).
    /// </summary>
    public double MeasuredLoudnessRange
    {
        get => GetDoubleOption("measured_LRA");
        set => SetDoubleOption("measured_LRA", value);
    }

    /// <summary>
    /// Gets or sets the measured true peak of a first pass (option <c>measured_TP</c>).
    /// </summary>
    public double MeasuredTruePeak
    {
        get => GetDoubleOption("measured_TP");
        set => SetDoubleOption("measured_TP", value);
    }

    /// <summary>
    /// Gets or sets the measured threshold of a first pass (option <c>measured_thresh</c>).
    /// </summary>
    public double MeasuredThreshold
    {
        get => GetDoubleOption("measured_thresh");
        set => SetDoubleOption("measured_thresh", value);
    }

    /// <summary>
    /// Gets or sets the offset gain in LU (option <c>offset</c>).
    /// </summary>
    public double Offset
    {
        get => GetDoubleOption("offset");
        set => SetDoubleOption("offset", value);
    }

    /// <summary>
    /// Gets or sets whether linear normalization is used when the measured values allow it (option <c>linear</c>).
    /// </summary>
    public bool Linear
    {
        get => GetBoolOption("linear");
        set => SetBoolOption("linear", value);
    }

    /// <summary>
    /// Gets or sets whether mono input is treated as dual mono (option <c>dual_mono</c>).
    /// </summary>
    public bool DualMono
    {
        get => GetBoolOption("dual_mono");
        set => SetBoolOption("dual_mono", value);
    }
}
