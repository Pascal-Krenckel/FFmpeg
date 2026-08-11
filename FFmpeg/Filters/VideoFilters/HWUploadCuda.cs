using FFmpeg.AutoGen;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Uploads frames from system memory to a CUDA device for hardware-accelerated processing.
/// </summary>
/// <remarks>
/// <para>
/// The CUDA device used by the filter can be selected through the
/// <see cref="Device"/> property. If no device is specified, FFmpeg uses its
/// default CUDA device.
/// </para>
/// </remarks>
public unsafe class HWUploadCuda : FilterContext
{
    internal HWUploadCuda(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Allocates an uninitialized <see cref="HWUploadCuda"/> filter context.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// A newly allocated <see cref="HWUploadCuda"/> filter context.
    /// </returns>
    /// <remarks>
    /// The returned filter is not initialized. Call <see cref="FilterContext.Init"/>
    /// after configuring any desired options.
    /// </remarks>
    public static HWUploadCuda Allocate(string name, FilterGraph graph)
    {
        _AVFilterContext* ptr = AllocateInternal(name, Filter.HWUploadCuda, graph);
        return new(ptr);
    }

    /// <summary>
    /// Creates and initializes an <see cref="HWUploadCuda"/> filter using the specified
    /// CUDA device.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="device">
    /// The zero-based index of the CUDA device to use.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="HWUploadCuda"/> filter context.
    /// </returns>
    public static HWUploadCuda Create(string name, int device, FilterGraph graph)
    {
        HWUploadCuda upload = Allocate(name, graph);
        upload.Device = device;
        upload.Init().ThrowIfError();
        return upload;
    }

    /// <summary>
    /// Gets or sets the index of the CUDA device used by the filter.
    /// </summary>
    /// <value>
    /// The zero-based index of the CUDA device to use.
    /// </value>
    /// <remarks>
    /// If not explicitly set, FFmpeg uses its default CUDA device.
    /// </remarks>
    public int Device
    {
        get => TryGetOption("device", out int device).IsError ? 0 : device;
        set => SetOption("device", value);
    }
}
