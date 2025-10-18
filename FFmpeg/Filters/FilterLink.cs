using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Filters;
public unsafe class FilterLink
{
    internal readonly AutoGen._AVFilterLink* link;

    internal FilterLink(AutoGen._AVFilterLink* link)
    {
        this.link = link;
    }

    public FilterContext SourceContext => new(link->src);
    public FilterContext DestinationContext => new(link->dst);

    public int SourcePadIndex => Find(link->src->output_pads,link->srcpad);
    public int DestinationPadIndex => Find(link->dst->input_pads,link->dstpad);


    private int Find(_AVFilterPad* pads, _AVFilterPad* pad)
    {
        return (int)(pads - pad);
    }



}
