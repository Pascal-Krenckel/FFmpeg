using FFmpeg.AutoGen;
using FFmpeg.Unsafe;
using FFmpeg.Utils;

namespace FFmpeg.HW.Vulkan;

/// <summary>
/// Represents a Vulkan device context associated with an FFmpeg hardware device.
/// </summary>
public readonly unsafe struct VulkanDeviceContext : IAVPointer<_AVVulkanDeviceContext>
{
    private readonly _AVVulkanDeviceContext* context;

    internal VulkanDeviceContext(_AVVulkanDeviceContext* context) => this.context = context;

    unsafe _AVVulkanDeviceContext* IAVPointer<_AVVulkanDeviceContext>.Pointer => context;

    /// <summary>
    /// Creates a <see cref="VulkanDeviceContext"/> from the hardware device associated with the specified frame.
    /// </summary>
    /// <param name="frame">
    /// The frame whose hardware device context will be used.
    /// </param>
    /// <returns>
    /// A <see cref="VulkanDeviceContext"/> representing the Vulkan device associated with the frame.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the frame does not have an associated hardware device context, or if the
    /// associated device is not a Vulkan device.
    /// </exception>
    public static VulkanDeviceContext FromAVFrame(AVFrame frame) => frame.FramesContext.IsEmpty || frame.FramesContext.DeviceContext.DeviceType != DeviceType.Vulkan
            ? throw new InvalidOperationException()
            : frame.FramesContext.DeviceContext.AsVulkan();
}
