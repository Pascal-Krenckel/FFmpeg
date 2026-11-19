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
    /// Gets the index of the output pad of <see cref="SourceContext"/> this link starts at.
    /// </summary>
    public int SourcePadIndex => IndexOf(link->src->outputs, link->src->nb_outputs);

    /// <summary>
    /// Gets the index of the input pad of <see cref="DestinationContext"/> this link ends at.
    /// </summary>
    public int DestinationPadIndex => IndexOf(link->dst->inputs, link->dst->nb_inputs);

    // AVFilterPad is opaque in FFmpeg 9 (its size is unknown here), so the pad pointer cannot be used
    // to compute the index. The filter's links array is indexed by pad, so look the link up there.
    private int IndexOf(_AVFilterLink** links, uint count)
    {
        for (int i = 0; i < count; i++)
        {
            if (links[i] == link)
                return i;
        }
        return -1;
    }
}
