using FFmpeg.AutoGen;
using FFmpeg.Unsafe;

namespace FFmpeg.Filters;

/// <summary>
/// Represents a connection between two filters in a filter graph.
/// </summary>
/// <remarks>
/// A <see cref="FilterLink"/> describes the media path between the output pad of a source
/// <see cref="FilterContext"/> and the input pad of a destination <see cref="FilterContext"/>.
/// It provides access to both connected filters and the corresponding pad indices.
/// </remarks>
public unsafe class FilterLink : IAVPointer<_AVFilterLink>
{
    internal readonly AutoGen._AVFilterLink* link;

    unsafe _AVFilterLink* IAVPointer<_AVFilterLink>.Pointer => link;

    internal FilterLink(AutoGen._AVFilterLink* link) => this.link = link;

    /// <summary>
    /// Gets the filter that produces frames for this link.
    /// </summary>
    public FilterContext SourceContext => new(link->src);

    /// <summary>
    /// Gets the filter that consumes frames from this link.
    /// </summary>
    public FilterContext DestinationContext => new(link->dst);

    /// <summary>
    /// Gets the index of the output pad on the source filter associated with this link.
    /// </summary>
    public int SourcePadIndex => Find(link->src->output_pads, link->srcpad);

    /// <summary>
    /// Gets the index of the input pad on the destination filter associated with this link.
    /// </summary>
    public int DestinationPadIndex => Find(link->dst->input_pads, link->dstpad);

    /// <summary>
    /// Finds the index of a filter pad within a pad array.
    /// </summary>
    /// <param name="pads">The beginning of the filter pad array.</param>
    /// <param name="pad">The filter pad whose index should be determined.</param>
    /// <returns>The zero-based index of <paramref name="pad"/> within <paramref name="pads"/>.</returns>
    private int Find(_AVFilterPad* pads, _AVFilterPad* pad) => (int)(pads - pad);
}
