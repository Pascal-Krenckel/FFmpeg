using FFmpeg.AutoGen;
using FFmpeg.SideData;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Packs two video streams into a stereoscopic video frame.
/// </summary>
/// <remarks>
/// <para>
/// The two input views must have the same dimensions and frame rate. Processing
/// stops when the shorter input stream ends.
/// </para>
/// <para>
/// The packing format can be configured through <see cref="Format"/>. The filter
/// also sets the appropriate stereoscopic 3D metadata for supported codecs.
/// </para>
/// <para>
/// The input views can conveniently be adjusted before packing by using filters
/// such as <c>scale</c> and <c>fps</c>.
/// </para>
/// </remarks>
public unsafe class FramePack : FilterContext
{
    internal FramePack(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Allocates an uninitialized <see cref="FramePack"/> filter context.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// A newly allocated <see cref="FramePack"/> filter context.
    /// </returns>
    /// <remarks>
    /// The returned filter is not initialized. Configure the filter and call
    /// <see cref="FilterContext.Init"/> before using it.
    /// </remarks>
    public static FramePack Allocate(string name, FilterGraph graph)
    {
        _AVFilterContext* ptr = AllocateInternal(name, Filter.FramePack, graph);
        return new(ptr);
    }

    /// <summary>
    /// Creates and initializes a <see cref="FramePack"/> filter using the default
    /// side-by-side packing format.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="FramePack"/> filter context.
    /// </returns>
    public static FramePack Create(string name, FilterGraph graph)
    {
        FramePack framePack = Allocate(name, graph);
        framePack.Init().ThrowIfError();
        return framePack;
    }

    /// <summary>
    /// Creates and initializes a <see cref="FramePack"/> filter using the specified
    /// stereoscopic packing format.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="format">
    /// The arrangement used to pack the two stereoscopic views.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="FramePack"/> filter context.
    /// </returns>
    public static FramePack Create(string name, Stereo3DType format, FilterGraph graph)
    {
        FramePack framePack = Allocate(name, graph);
        framePack.Format = format;
        framePack.Init().ThrowIfError();
        return framePack;
    }

    /// <summary>
    /// Creates and initializes a <see cref="FramePack"/> filter using the specified
    /// stereoscopic packing format.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="format">
    /// The packing format to use. Supported values are <c>sbs</c>, <c>tab</c>,
    /// <c>lines</c>, <c>columns</c>, and <c>frameseq</c>.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="FramePack"/> filter context.
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><c>sbs</c> - views are placed next to each other horizontally.</description></item>
    /// <item><description><c>tab</c> - views are placed above and below each other vertically.</description></item>
    /// <item><description><c>lines</c> - views are packed on alternating lines.</description></item>
    /// <item><description><c>columns</c> - views are packed on alternating columns.</description></item>
    /// <item><description><c>frameseq</c> - views are temporally interleaved.</description></item>
    /// </list>
    /// </remarks>
    public static FramePack Create(string name, string format, FilterGraph graph)
    {
        FramePack framePack = Allocate(name, graph);
        _ = framePack.SetOption("format", format);
        framePack.Init().ThrowIfError();
        return framePack;
    }

    /// <summary>
    /// Gets or sets the arrangement used to pack the two stereoscopic views.
    /// </summary>
    /// <value>
    /// The stereoscopic 3D packing format.
    /// </value>
    /// <remarks>
    /// <para>
    /// The supported formats are:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="Stereo3DType.SideBySide"/> - views are placed next to each other horizontally.</description></item>
    /// <item><description><see cref="Stereo3DType.TopBottom"/> - views are placed above and below each other vertically.</description></item>
    /// <item><description><see cref="Stereo3DType.Lines"/> - views are packed on alternating lines.</description></item>
    /// <item><description><see cref="Stereo3DType.Columns"/> - views are packed on alternating columns.</description></item>
    /// <item><description><see cref="Stereo3DType.FrameSequence"/> - views are temporally interleaved.</description></item>
    /// </list>
    /// <para>
    /// The default format is <see cref="Stereo3DType.SideBySide"/>.
    /// </para>
    /// </remarks>
    public Stereo3DType Format
    {
        get => TryGetOption("format", out int value).IsError ? Stereo3DType.Unspecified : (Stereo3DType)value;
        set => SetOption("format", (int)value);
    }
}
