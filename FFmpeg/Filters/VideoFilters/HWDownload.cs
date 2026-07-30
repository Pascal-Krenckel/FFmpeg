using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Downloads hardware frames to system memory.
/// </summary>
/// <remarks>
/// <para>
/// The input frames must be hardware frames, while the output frames are stored in
/// system memory using a non-hardware pixel format.
/// </para>
/// <para>
/// Not all pixel formats are supported as output. If the required output format is
/// not supported by the hardware download operation, an additional <c>format</c>
/// filter may need to be inserted immediately after this filter to convert the
/// frames to a supported format.
/// </para>
/// </remarks>
public unsafe class HWDownload : FilterContext
{
    internal HWDownload(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Allocates an uninitialized <see cref="HWDownload"/> filter context.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// A newly allocated <see cref="HWDownload"/> filter context.
    /// </returns>
    /// <remarks>
    /// The returned filter is not initialized. Call <see cref="FilterContext.Init"/>
    /// before using the filter.
    /// </remarks>
    public static HWDownload Allocate(string name, FilterGraph graph)
    {
        var ptr = AllocateInternal(name, Filter.HWDownload, graph);
        return new(ptr);
    }

    /// <summary>
    /// Creates and initializes an <see cref="HWDownload"/> filter.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="HWDownload"/> filter context.
    /// </returns>
    public static HWDownload Create(string name, FilterGraph graph)
    {
        var download = Allocate(name, graph);
        download.Init().ThrowIfError();
        return download;
    }
}
