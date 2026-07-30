using FFmpeg.AutoGen;
using FFmpeg.HW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// Uploads frames from system memory to hardware surfaces for hardware-accelerated processing.
/// </summary>
/// <remarks>
/// <para>
/// The hardware device used by the filter can be specified explicitly through a
/// <see cref="HW.DeviceContext"/> or derived from the device associated with the input
/// frames by specifying a <see cref="DeviceType"/>.
/// </para>
/// <para>
/// When using an explicit hardware device, the device can be supplied through
/// <see cref="Create(string, HW.DeviceContext, FilterGraph)"/> or
/// <see cref="Create(string, HW.DeviceContext_ref, FilterGraph)"/>.
/// Alternatively, <see cref="Create(string, DeviceType, FilterGraph)"/> can be used
/// to derive a new device of the specified type from the device on which the input
/// frames reside.
/// </para>
/// <para>
/// The input and output devices must be compatible. Typically, this means that they
/// refer to the same underlying hardware context, such as the same graphics card.
/// </para>
/// </remarks>
public unsafe class HWUpload : FilterContext
{
    internal HWUpload(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Allocates an uninitialized <see cref="HWUpload"/> filter context.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// A newly allocated <see cref="HWUpload"/> filter context.
    /// </returns>
    /// <remarks>
    /// The returned filter is not initialized. A hardware device must be assigned
    /// through <see cref="FilterContext.HwDeviceContext"/> or
    /// <see cref="DeriveDevice"/> before calling <see cref="FilterContext.Init"/>.
    /// </remarks>
    public static HWUpload Allocate(string name, FilterGraph graph)
    {
        var ptr = AllocateInternal(name, Filter.HWUpload, graph);
        return new(ptr);
    }

    /// <summary>
    /// Creates and initializes an <see cref="HWUpload"/> filter using the specified
    /// hardware device.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="hwContext">
    /// The hardware device context to which frames are uploaded.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="HWUpload"/> filter context.
    /// </returns>
    public static HWUpload Create(string name, HW.DeviceContext hwContext, FilterGraph graph)
    {
        var upload = Allocate(name, graph);
        upload.HwDeviceContext.SetReferencedObject(hwContext);
        upload.Init().ThrowIfError();
        return upload;
    }

    /// <summary>
    /// Creates and initializes an <see cref="HWUpload"/> filter using the specified
    /// hardware device context reference.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="hwContext">
    /// A reference to the hardware device context to which frames are uploaded.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="HWUpload"/> filter context.
    /// </returns>
    /// <inheritdoc cref="Create(string, HW.DeviceContext, FilterGraph)"/>
    public static HWUpload Create(string name, HW.DeviceContext_ref hwContext, FilterGraph graph)
    {
        var upload = Allocate(name, graph);
        using var hwC = hwContext.GetReferencedObject();
        upload.HwDeviceContext.SetReferencedObject(hwC);
        upload.Init().ThrowIfError();
        return upload;
    }

    /// <summary>
    /// Creates and initializes an <see cref="HWUpload"/> filter by deriving a hardware
    /// device of the specified type from the device associated with the input frames.
    /// </summary>
    /// <param name="name">
    /// The name assigned to the filter context.
    /// </param>
    /// <param name="deviceType">
    /// The type of hardware device to derive from the device associated with the input frames.
    /// </param>
    /// <param name="graph">
    /// The filter graph to which the filter belongs.
    /// </param>
    /// <returns>
    /// An initialized <see cref="HWUpload"/> filter context.
    /// </returns>
    /// <remarks>
    /// This is equivalent to configuring the <c>derive_device</c> option before
    /// initializing the filter.
    /// </remarks>
    public static HWUpload Create(string name, DeviceType deviceType, FilterGraph graph)
    {
        var upload = Allocate(name, graph);
        upload.DeriveDevice = deviceType;
        upload.Init().ThrowIfError();
        return upload;
    }

    /// <summary>
    /// Gets or sets the type of hardware device to derive from the device associated
    /// with the input frames.
    /// </summary>
    /// <value>
    /// The hardware device type to derive, or <see cref="DeviceType.None"/> when no
    /// derived device is configured.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set, the filter derives a new hardware device of the specified type from
    /// the device on which the input frames reside, rather than using the device
    /// supplied through <see cref="FilterContext.HwDeviceContext"/>.
    /// </para>
    /// <para>
    /// The derived device must be compatible with the input device. Typically, both
    /// devices must refer to the same underlying hardware context.
    /// </para>
    /// </remarks>
    public DeviceType DeriveDevice
    {
        get => TryGetOption("derive_device", out string? deviceName).IsError ? DeviceType.None : DeviceType.Parse(deviceName!);
        set => SetOption("derive_device", value.ToFFmpegString());
    }
}
