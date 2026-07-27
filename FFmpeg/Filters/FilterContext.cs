using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Collections;
using FFmpeg.Images;
using FFmpeg.Unsafe;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Filters;
/// <summary>
/// Represents an instance of a filter within an FFmpeg filter graph.
/// </summary>
/// <remarks>
/// A <see cref="FilterContext"/> stores the state of a filter after it has been
/// added to a <see cref="FilterGraph"/>. It provides access to the filter's
/// options, pads, links, and methods for sending or receiving frames from
/// buffer source and sink filters.
/// </remarks>
public unsafe partial class FilterContext : Options.OptionQueryableBase, IAVPointer<_AVFilterContext>
{
    /// <summary>
    /// Gets a pointer to the filter context, used for option queries.
    /// </summary>
    protected override unsafe void* Pointer => context;
    _AVFilterContext* IAVPointer<_AVFilterContext>.Pointer => context;

    /// <summary>
    /// The underlying unmanaged FFmpeg filter context.
    /// </summary>
    internal AutoGen._AVFilterContext* context;

    /// <summary>
    /// Gets the filter associated with this context.
    /// </summary>
    public Filter Filter => new(context->filter);

    /// <summary>
    /// Gets the name of the filter context.
    /// </summary>
    public string Name => Marshal.PtrToStringUTF8((nint)context->name);

    /// <summary>
    /// Gets or sets the maximum number of worker threads used by the filter.
    /// </summary>
    /// <value>
    /// A value less than or equal to zero lets FFmpeg choose the appropriate
    /// thread count automatically.
    /// </value>
    public int Threads { get => context->nb_threads; set => context->nb_threads = value; }


    /// <summary>
    /// Initializes a new instance of the <see cref="FilterContext"/> class with the given FFmpeg filter context.
    /// </summary>
    /// <param name="context">Pointer to the FFmpeg filter context.</param>
    internal FilterContext(AutoGen._AVFilterContext* context) => this.context = context;



    /// <summary>
    /// Gets or sets the hardware device context used by the filter.
    /// </summary>
    /// <remarks>
    /// This property is primarily used by hardware-accelerated filters that require
    /// access to a GPU or other hardware device.
    /// </remarks>
    public HW.DeviceContext_ref HwDeviceContext => new(&context->hw_device_ctx, false);

    /// <summary>
    /// Gets or sets the number of additional hardware frames allocated by the filter.
    /// </summary>
    /// <remarks>
    /// This property is primarily used with hardware-accelerated filters to reduce
    /// pipeline stalls caused by frame buffering.
    /// </remarks>
    public int ExtraHWFrames
    {
        get => context->extra_hw_frames;
        set => context->extra_hw_frames = value;
    }





    /// <summary>
    /// Initializes the filter using an argument string.
    /// </summary>
    public AVResult32 Init(string? args) => ffmpeg.avfilter_init_str(context, args);

    /// <summary>
    /// Initializes the filter context without arguments.
    /// </summary>
    public AVResult32 Init() => Init(null as string);

    /// <summary>
    /// Initializes the filter using the specified dictionary of options.
    /// </summary>
    /// <param name="dictionary">
    /// The dictionary containing the filter options.
    /// Recognized options are removed from the dictionary during initialization.
    /// </param>
    /// <returns>
    /// The result of the initialization operation.
    /// </returns>
    /// <remarks>
    /// This method wraps FFmpeg's <c>avfilter_init_dict()</c>. Any options that are
    /// not recognized by the filter remain in <paramref name="dictionary"/> after
    /// the method returns.
    /// </remarks>
    public AVResult32 Init(Collections.AVDictionary dictionary)
    {
        if (dictionary == null)
            return Init(null as string);
        AutoGen._AVDictionary* dic = dictionary.dictionary;
        int res = ffmpeg.avfilter_init_dict(context, &dic);
        dictionary.dictionary = dic;        
        return res;
    }

    /// <summary>
    /// Initializes the filter using the specified dictionary of options.
    /// </summary>
    /// <param name="dictionary">
    /// The dictionary containing the filter options.
    /// Recognized options are removed from the dictionary during initialization.
    /// </param>
    /// <returns>
    /// The result of the initialization operation.
    /// </returns>
    /// <remarks>
    /// This method wraps FFmpeg's <c>avfilter_init_dict()</c>. Any options that are
    /// not recognized by the filter remain in <paramref name="dictionary"/> after
    /// the method returns.
    /// </remarks>
    public AVResult32 Init(Collections.AVMultiDictionary dictionary)
    {
        if (dictionary == null)
            return Init(null as string);
        AutoGen._AVDictionary* dic = dictionary.dictionary;
        int res = ffmpeg.avfilter_init_dict(context, &dic);
        dictionary.dictionary = dic;
        return res;
    }


    /// <summary>
    /// Initializes the filter using the specified dictionary of options.
    /// </summary>
    /// <param name="dictionary">
    /// A dictionary containing the filter options.
    /// After the method returns, the dictionary contains only the options that were
    /// not recognized by the filter.
    /// </param>
    /// <returns>
    /// The result of the initialization operation.
    /// </returns>
    /// <remarks>
    /// If <paramref name="dictionary"/> is an <see cref="AVDictionary"/>, it is
    /// passed directly to FFmpeg. Otherwise, it is copied into a temporary
    /// <see cref="AVDictionary"/>, and the remaining unrecognized options are copied
    /// back into the original dictionary after initialization.
    /// </remarks>
    public AVResult32 Init(IDictionary<string,string> dictionary)
    {        
        if (dictionary is AVDictionary dic) return Init(dic);
        using AVDictionary avDic = new(dictionary);
        var ret = Init(avDic);
        dictionary.Clear();
        foreach (var kvp in avDic)
            dictionary.Add(kvp);
        return ret;
    }

    /// <summary>
    /// Sends a frame to a buffer source filter.
    /// </summary>
    /// <param name="frame">
    /// The frame to send, or <see langword="null"/> to signal end-of-stream.
    /// </param>
    /// <param name="keepRef">
    /// <see langword="true"/> to keep a reference to the supplied frame;
    /// <see langword="false"/> to transfer ownership when possible.
    /// </param>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    public AVResult32 SendFrame(AVFrame? frame, bool keepRef = false)
    {
        AutoGen._AVFrame* f = frame != null ? frame.Frame : null;
        return keepRef ? ffmpeg.av_buffersrc_write_frame(context, f) : ffmpeg.av_buffersrc_add_frame(context, f);
    }

    /// <summary>
    /// Signals end-of-stream to the buffer source filter.
    /// </summary>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    public AVResult32 Drain() => SendFrame(null);

    /// <summary>
    /// Receives a frame from a buffer sink filter.
    /// </summary>
    /// <param name="frame">
    /// The destination frame that receives the filtered data.
    /// </param>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    public AVResult32 ReceiveFrame(AVFrame frame)
    {
        frame.Unreference();
        int res = ffmpeg.av_buffersink_get_frame(context, frame.Frame);
        frame.TimeBase = ffmpeg.av_buffersink_get_time_base(context);
        frame.BestEffortTimestamp = frame.PresentationTimestamp;
        return res;
    }

    /// <summary>
    /// Returns a string that identifies the filter context.
    /// </summary>
    /// <returns>
    /// A string containing the filter context name and filter name.
    /// </returns>
    public override string ToString() => context->filter != null
        ? $"{Marshal.PtrToStringUTF8((nint)context->name)}({Marshal.PtrToStringUTF8((nint)context->filter->name)})"
        : $"{Marshal.PtrToStringUTF8((nint)context->name)}(_unset_)";


    /// <summary>
    /// Gets the number of input pads exposed by the filter.
    /// </summary>
    public uint InputCount => context->nb_inputs;
    /// <summary>
    /// Gets the number of output pads exposed by the filter.
    /// </summary>
    public uint OutputCount => context->nb_outputs;

    /// <summary>
    /// Gets the number of output pads exposed by the filter.
    /// </summary>
    public FilterPad GetInputFilterPad(int index) => index < 0 || index >= InputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter pads.")
            : new FilterPad(context->input_pads, index);
    
    /// <summary>
    /// Gets the output pad at the specified index.
    /// </summary>
    public FilterPad GetOutputFilterPad(int index) => index < 0 || index >= OutputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter pads.")
            : new FilterPad(context->output_pads, index);

    /// <summary>
    /// Gets an enumeration of all input pads.
    /// </summary>
    public IEnumerable<FilterPad> InputFilterPads
    {
        get
        {
            for (int i = 0; i < InputCount; i++)
                yield return GetInputFilterPad(i);
        }
    }

    /// <summary>
    /// Gets an enumeration of all output pads.
    /// </summary>
    public IEnumerable<FilterPad> OutputFilterPads
    {
        get
        {
            for (int i = 0; i < OutputCount; i++)
                yield return GetOutputFilterPad(i);
        }
    }

    /// <summary>
    /// Gets all connected input links.
    /// </summary>
    /// <remarks>
    /// Only connected input pads are included in the enumeration.
    /// </remarks>
    public IEnumerable<FilterLink> InputFilterLinks
    {
        get
        {
            for (int i = 0; i < InputCount; i++)
            {
                FilterLink? link = GetInputFilterLink(i);
                if (link != null)
                    yield return link;
            }
        }
    }

    /// <summary>
    /// Gets all connected output links.
    /// </summary>
    /// <remarks>
    /// Only connected output pads are included in the enumeration.
    /// </remarks>
    public IEnumerable<FilterLink> OutputFilterLinks
    {
        get
        {
            for (int i = 0; i < OutputCount; i++)
            {
                FilterLink? link = GetOutputFilterLink(i);
                if (link != null)
                    yield return link;
            }
        }
    }

    /// <summary>
    /// Gets the link connected to the specified input pad.
    /// </summary>
    /// <param name="index">
    /// The zero-based input pad index.
    /// </param>
    /// <returns>
    /// The connected <see cref="FilterLink"/>, or <see langword="null"/> if the
    /// input pad is not connected.
    /// </returns>
    public FilterLink? GetInputFilterLink(int index) => index < 0 || index >= InputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter links.")
            : context->inputs[index] != null ? new FilterLink(context->inputs[index]) : null;

    /// <summary>
    /// Gets the link connected to the specified output pad.
    /// </summary>
    /// <param name="index">
    /// The zero-based output pad index.
    /// </param>
    /// <returns>
    /// The connected <see cref="FilterLink"/>, or <see langword="null"/> if the
    /// output pad is not connected.
    /// </returns>
    public FilterLink? GetOutputFilterLink(int index) => index < 0 || index >= OutputCount
            ? throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for filter links.")
            : context->outputs[index] != null ? new FilterLink(context->outputs[index]) : null;

}

