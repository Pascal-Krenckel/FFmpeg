namespace FFmpeg.HW;

/// <summary>
/// Specifies the hardware device types available for multimedia acceleration in FFmpeg.
/// </summary>
public enum DeviceType : int
{
    /// <summary>
    /// No hardware acceleration device.
    /// </summary>
    None = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_NONE,

    /// <summary>
    /// VDPAU (Video Decode and Presentation API for Unix), used for hardware-accelerated video decoding on Linux.
    /// </summary>
    VDPAU = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU,

    /// <summary>
    /// CUDA (Compute Unified Device Architecture), NVIDIA's GPU computing platform for hardware acceleration.
    /// </summary>
    CUDA = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA,

    /// <summary>
    /// VAAPI (Video Acceleration API), a Linux/Unix API for hardware-accelerated video processing.
    /// </summary>
    VAAPI = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI,

    /// <summary>
    /// DXVA2 (DirectX Video Acceleration 2), Microsoft's API for hardware-accelerated video on Windows.
    /// </summary>
    DXVA2 = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2,

    /// <summary>
    /// QSV (Quick Sync Video), Intel's technology for hardware-accelerated video encoding and decoding.
    /// </summary>
    QSV = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_QSV,

    /// <summary>
    /// VideoToolbox, Apple's API for hardware-accelerated video encoding and decoding on macOS/iOS.
    /// </summary>
    VideoToolbox = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX,

    /// <summary>
    /// D3D11VA (Direct3D 11 Video Acceleration), Microsoft's API for hardware-accelerated video decoding using Direct3D 11.
    /// </summary>
    D3D11VA = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,

    /// <summary>
    /// DRM (Direct Rendering Manager), Linux kernel subsystem for GPU management and hardware acceleration.
    /// </summary>
    DRM = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_DRM,

    /// <summary>
    /// OpenCL (Open Computing Language), a framework for cross-platform GPU and CPU acceleration.
    /// </summary>
    OpenCL = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL,

    /// <summary>
    /// MediaCodec, Android's API for hardware-accelerated video encoding and decoding.
    /// </summary>
    MediaCodec = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC,

    /// <summary>
    /// Vulkan, a cross-platform API for high-efficiency graphics and compute on modern GPUs.
    /// </summary>
    Vulkan = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_VULKAN,

    /// <summary>
    /// D3D12VA (Direct3D 12 Video Acceleration), Microsoft's API for hardware-accelerated video processing using Direct3D 12.
    /// </summary>
    D3D12VA = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_D3D12VA,

    /// <summary>
    /// AMF (Advanced Media Framework), AMD's hardware acceleration API.
    /// </summary>
    AMF = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_AMF,

    /// <summary>
    /// OHCodec, hardware acceleration device type for OpenHarmony codec.
    /// </summary>
    OHCODEC = AutoGen._AVHWDeviceType.AV_HWDEVICE_TYPE_OHCODEC,
}

