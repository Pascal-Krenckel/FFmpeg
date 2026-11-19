using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Computes the peak signal-to-noise ratio (PSNR) between two videos (FFmpeg filter <c>psnr</c>).
/// </summary>
/// <remarks>
/// <para>
/// The filter has two inputs: pad 0 is the main (distorted) video, pad 1 the reference. Both must have the same
/// size and pixel format, so insert a <c>scale</c>/<c>format</c> filter in front of one of them if needed. The
/// main video is passed through unchanged.
/// </para>
/// <para>
/// FFmpeg stores the MSE and PSNR of every frame in the metadata of the output frame; read them with
/// <see cref="PsnrScore.TryRead(Utils.AVFrame, out PsnrScore?)"/> and collect the averages with
/// <see cref="PsnrStatistics"/>. FFmpeg itself only logs the averages when the filter is freed.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Psnr : FilterContext
{
    internal Psnr(_AVFilterContext* context) : base(context, "psnr")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>psnr</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Psnr Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Psnr, graph));

    /// <summary>
    /// Creates a psnr filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Psnr Create(string name, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter => filter.Init().ThrowIfError());

    /// <summary>
    /// Creates a psnr filter that also writes the per-frame values to a file.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="statsFile">The file for the per-frame values; <c>"-"</c> writes to stdout.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Psnr Create(string name, string statsFile, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.StatsFile = statsFile;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the file the per-frame values are written to (option <c>stats_file</c>);
    /// <c>"-"</c> writes to stdout.
    /// </summary>
    public string? StatsFile
    {
        get => GetStringOption("stats_file");
        set => SetStringOption("stats_file", value);
    }

    /// <summary>
    /// Gets or sets the format version of the stats file, 1 or 2 (option <c>stats_version</c>).
    /// Version 2 writes a header line naming the fields.
    /// </summary>
    public int StatsVersion
    {
        get => (int)GetInt64Option("stats_version");
        set => SetInt64Option("stats_version", value);
    }

    /// <summary>
    /// Gets or sets whether the stats file (version 2) also contains the maximum value of each component
    /// (option <c>output_max</c>).
    /// </summary>
    public bool OutputMax
    {
        get => GetBoolOption("output_max");
        set => SetBoolOption("output_max", value);
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
