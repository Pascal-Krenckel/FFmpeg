using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Computes the VMAF score between two videos with Netflix's libvmaf (FFmpeg filter <c>libvmaf</c>).
/// </summary>
/// <remarks>
/// <para>
/// The filter is only available if FFmpeg was built with <c>--enable-libvmaf</c>; check
/// <see cref="IsAvailable"/>. The filter has two inputs: pad 0 is the main (distorted) video, pad 1 the
/// reference. Both must have the same size and the same pixel format, which must be 8, 10, 12 or 16-bit planar
/// YUV 4:2:0, 4:2:2 or 4:4:4. VMAF models are trained for a viewing size (1080p for the default model), so scale
/// both videos to it, ideally with <c>bicubic</c>. The main video is passed through unchanged.
/// </para>
/// <para>
/// Unlike <see cref="Ssim"/> and <see cref="Psnr"/>, libvmaf does not store per-frame scores in the frame
/// metadata. The scores are computed when the filter is freed, i.e. when the <see cref="FilterGraph"/> is
/// disposed: FFmpeg logs the pooled score (<c>VMAF score: …</c>) and writes the log file set by
/// <see cref="LogPath"/>. Use <see cref="Create(string, string, FilterGraph)"/> to write an XML log and read it
/// with <see cref="VmafLog.Load(string)"/> after disposing the graph.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class LibVmaf : FilterContext
{
    internal LibVmaf(_AVFilterContext* context) : base(context, "libvmaf")
    {
    }

    /// <summary>
    /// Gets whether the loaded FFmpeg libraries contain the <c>libvmaf</c> filter.
    /// </summary>
    public static bool IsAvailable => Filter.TryGetFilterByName("libvmaf", out _);

    /// <summary>
    /// Allocates an uninitialized <c>libvmaf</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    /// <exception cref="ArgumentException">FFmpeg was built without libvmaf.</exception>
    public static LibVmaf Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.LibVmaf, graph));

    /// <summary>
    /// Creates a libvmaf filter with the default model (<c>vmaf_v0.6.1</c>) and no log file.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    /// <exception cref="ArgumentException">FFmpeg was built without libvmaf.</exception>
    public static LibVmaf Create(string name, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter => filter.Init().ThrowIfError());

    /// <summary>
    /// Creates a libvmaf filter that writes an XML log, which <see cref="VmafLog.Load(string)"/> reads once the
    /// graph has been disposed.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="logPath">The path of the XML log file.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    /// <exception cref="ArgumentException">FFmpeg was built without libvmaf.</exception>
    public static LibVmaf Create(string name, string logPath, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.LogPath = logPath;
            filter.LogFormat = VmafLogFormat.Xml;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the file the scores are written to when the filter is freed (option <c>log_path</c>).
    /// </summary>
    public string? LogPath
    {
        get => GetStringOption("log_path");
        set => SetStringOption("log_path", value);
    }

    /// <summary>
    /// Gets or sets the format of the log file (option <c>log_fmt</c>); <see cref="VmafLog"/> reads
    /// <see cref="VmafLogFormat.Xml"/>, the default.
    /// </summary>
    public VmafLogFormat LogFormat
    {
        get => GetStringOption("log_fmt") switch
        {
            "json" => VmafLogFormat.Json,
            "csv" => VmafLogFormat.Csv,
            "sub" => VmafLogFormat.Sub,
            _ => VmafLogFormat.Xml,
        };
        set => SetStringOption("log_fmt", value switch
        {
            VmafLogFormat.Xml => "xml",
            VmafLogFormat.Json => "json",
            VmafLogFormat.Csv => "csv",
            VmafLogFormat.Sub => "sub",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        });
    }

    /// <summary>
    /// Gets or sets how the per-frame scores are pooled into the score FFmpeg logs (option <c>pool</c>).
    /// The log file always contains all pooling methods.
    /// </summary>
    public VmafPoolMethod Pool
    {
        get => GetStringOption("pool") switch
        {
            "min" => VmafPoolMethod.Min,
            "harmonic_mean" => VmafPoolMethod.HarmonicMean,
            _ => VmafPoolMethod.Mean,
        };
        set => SetStringOption("pool", value switch
        {
            VmafPoolMethod.Mean => "mean",
            VmafPoolMethod.Min => "min",
            VmafPoolMethod.HarmonicMean => "harmonic_mean",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        });
    }

    /// <summary>
    /// Gets or sets the number of threads libvmaf uses; 0 computes on the filter thread (option <c>n_threads</c>).
    /// This is libvmaf's own thread pool, unlike <see cref="FilterContext.Threads"/>.
    /// </summary>
    public int VmafThreads
    {
        get => (int)GetInt64Option("n_threads");
        set => SetInt64Option("n_threads", value);
    }

    /// <summary>
    /// Gets or sets the frame subsampling interval: only every n-th frame is scored (option <c>n_subsample</c>).
    /// </summary>
    public int Subsample
    {
        get => (int)GetInt64Option("n_subsample");
        set => SetInt64Option("n_subsample", value);
    }

    /// <summary>
    /// Gets or sets the models to use (option <c>model</c>): <c>key=value</c> pairs separated by <c>:</c>,
    /// models separated by <c>|</c>, e.g. <c>"version=vmaf_4k_v0.6.1"</c>,
    /// <c>"path=/models/custom.json:name=custom"</c> or <c>"version=vmaf_v0.6.1:name=vmaf|version=vmaf_v0.6.1neg:name=vmaf_neg"</c>.
    /// The <c>name</c> is the metric name in <see cref="VmafLog"/> (default <c>vmaf</c>).
    /// </summary>
    public string? Model
    {
        get => GetStringOption("model");
        set => SetStringOption("model", value);
    }

    /// <summary>
    /// Gets or sets additional features to compute (option <c>feature</c>), in the same syntax as
    /// <see cref="Model"/>, e.g. <c>"name=psnr|name=float_ssim"</c>. Their per-frame values appear in the log.
    /// </summary>
    public string? Feature
    {
        get => GetStringOption("feature");
        set => SetStringOption("feature", value);
    }

    /// <summary>
    /// Gets or sets whether the output ends with the shorter input (option <c>shortest</c>).
    /// </summary>
    public bool Shortest
    {
        get => GetBoolOption("shortest");
        set => SetBoolOption("shortest", value);
    }

    /// <summary>
    /// Gets or sets whether the last reference frame is compared with the remaining main frames when the reference
    /// ends first (option <c>repeatlast</c>).
    /// </summary>
    public bool RepeatLast
    {
        get => GetBoolOption("repeatlast");
        set => SetBoolOption("repeatlast", value);
    }
}
