using FFmpeg.AutoGen;
using FFmpeg.Unsafe;
using FFmpeg.Utils;

namespace FFmpeg.HW.Vulkan;

/// <summary>
/// Represents a Vulkan hardware frames context.
/// </summary>
public readonly unsafe struct VulkanFramesContext : IAVPointer<_AVVulkanFramesContext>
{
    internal readonly _AVVulkanFramesContext* context;

    internal VulkanFramesContext(_AVVulkanFramesContext* context) => this.context = context;

    unsafe _AVVulkanFramesContext* IAVPointer<_AVVulkanFramesContext>.Pointer => context;

    /// <summary>
    /// Creates a <see cref="VulkanFramesContext"/> from the hardware frames context
    /// associated with the specified frame.
    /// </summary>
    /// <param name="frame">
    /// The frame whose hardware frames context will be used.
    /// </param>
    /// <returns>
    /// A <see cref="VulkanFramesContext"/> representing the Vulkan hardware frames
    /// context associated with the frame.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the frame does not have an associated hardware frames context, or if
    /// the associated device is not a Vulkan device.
    /// </exception>
    public static VulkanFramesContext FromAVFrame(AVFrame frame) => frame.FramesContext.IsEmpty || frame.FramesContext.DeviceContext.DeviceType != DeviceType.Vulkan
            ? throw new InvalidOperationException()
            : frame.FramesContext.AsVulkan();
}