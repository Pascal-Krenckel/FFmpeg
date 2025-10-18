namespace FFmpeg.Filters;

/// <summary>
/// Represents various filter flags used in FFmpeg filters.
/// These flags indicate different capabilities or behaviors of the filters.
/// </summary>
[Flags]
public enum FilterFlags : int
{
    /// <summary>
    /// The number of the filter inputs is not determined just by _AVFilter.inputs.
    /// The filter might add additional inputs during initialization depending on the options supplied to it.
    /// </summary>
    DynamicInputs = ffmpeg.AVFILTER_FLAG_DYNAMIC_INPUTS,

    /// <summary>
    /// The number of the filter outputs is not determined just by _AVFilter.outputs.
    /// The filter might add additional outputs during initialization depending on the options supplied to it.
    /// </summary>
    DynamicOutputs = ffmpeg.AVFILTER_FLAG_DYNAMIC_OUTPUTS,

    /// <summary>
    /// The filter supports multithreading by splitting frames into multiple parts and processing them concurrently.
    /// </summary>
    SliceThreads = ffmpeg.AVFILTER_FLAG_SLICE_THREADS,

    /// <summary>
    /// The filter is a "metadata" filter - it does not modify the frame data in any way.
    /// It may only affect the metadata (fields copied by av_frame_copy_props).
    /// </summary>
    MetadataOnly = ffmpeg.AVFILTER_FLAG_METADATA_ONLY,

    /// <summary>
    /// The filter can create hardware frames using _AVFilterContext.hw_device_ctx.
    /// </summary>
    HWDevice = ffmpeg.AVFILTER_FLAG_HWDEVICE,

    /// <summary>
    /// The filter supports a generic "enable" expression option that can be used to enable or disable
    /// the filter in the timeline.
    /// </summary>
    SupportTimelineGeneric = ffmpeg.AVFILTER_FLAG_SUPPORT_TIMELINE_GENERIC,

    /// <summary>
    /// Same as SupportTimelineGeneric, except the filter will have its filter_frame() callback(s)
    /// called as usual, even when the enable expression is false. The filter itself will disable 
    /// filtering within the callback.
    /// </summary>
    SupportTimelineInternal = ffmpeg.AVFILTER_FLAG_SUPPORT_TIMELINE_INTERNAL,

    /// <summary>
    /// Handy mask to test whether the filter supports timeline features (either generically or internally).
    /// </summary>
    SupportTimeline = ffmpeg.AVFILTER_FLAG_SUPPORT_TIMELINE
}
