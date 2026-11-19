using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Computes the structural similarity (SSIM) between two videos (FFmpeg filter <c>ssim</c>).
/// </summary>
/// <remarks>
/// <para>
/// The filter has two inputs: pad 0 is the main (distorted) video, pad 1 the reference. Both must have the same
/// size and pixel format, so insert a <c>scale</c>/<c>format</c> filter in front of one of them if needed. The
/// main video is passed through unchanged.
/// </para>
/// <para>
/// FFmpeg stores the score of every frame in the metadata of the output frame; read it with
/// <see cref="SsimScore.TryRead(Utils.AVFrame, out SsimScore?)"/> and collect the averages with
/// <see cref="SsimStatistics"/>. FFmpeg itself only logs the averages when the filter is freed.
/// </para>
/// <para>
/// The options are read when the filter is initialized: change them on a filter from
/// <see cref="Allocate(string, FilterGraph)"/> before calling <see cref="FilterContext.Init()"/>, or use a
/// <c>Create</c> overload. Setters throw if FFmpeg rejects the value.
/// </para>
/// </remarks>
public unsafe class Ssim : FilterContext
{
    internal Ssim(_AVFilterContext* context) : base(context, "ssim")
    {
    }

    /// <summary>
    /// Allocates an uninitialized <c>ssim</c> filter in <paramref name="graph"/>.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The allocated filter; call <see cref="FilterContext.Init()"/> after setting its options.</returns>
    public static Ssim Allocate(string name, FilterGraph graph) => new(AllocateInternal(name, Filter.Ssim, graph));

    /// <summary>
    /// Creates an ssim filter.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Ssim Create(string name, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter => filter.Init().ThrowIfError());

    /// <summary>
    /// Creates an ssim filter that also writes the per-frame scores to a file.
    /// </summary>
    /// <param name="name">The name of the filter instance.</param>
    /// <param name="statsFile">The file for the per-frame scores; <c>"-"</c> writes to stdout.</param>
    /// <param name="graph">The filter graph that will own the filter.</param>
    /// <returns>The initialized filter.</returns>
    public static Ssim Create(string name, string statsFile, FilterGraph graph)
        => InitializeOrDelete(Allocate(name, graph), filter =>
        {
            filter.StatsFile = statsFile;
            filter.Init().ThrowIfError();
        });

    /// <summary>
    /// Gets or sets the file the per-frame scores are written to (option <c>stats_file</c>);
    /// <c>"-"</c> writes to stdout.
    /// </summary>
    public string? StatsFile
    {
        get => GetStringOption("stats_file");
        set => SetStringOption("stats_file", value);
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
