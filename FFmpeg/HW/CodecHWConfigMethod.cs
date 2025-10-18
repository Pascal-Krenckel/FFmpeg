namespace FFmpeg.HW;
/// <summary>
/// Enum representing the hardware configuration methods supported by the codec.
/// </summary>
[Flags]
public enum CodecHWConfigMethod
{
    /// <summary>
    /// The codec supports this format via the hw_device_ctx interface.
    /// 
    /// When selecting this format, CodecContext.HwDeviceContext should
    /// be set to a device of the specified type before calling
    /// CodecOpen().
    /// </summary>
    HwDeviceContext = 0x01,

    /// <summary>
    /// The codec supports this format via the hw_frames_ctx interface.
    /// 
    /// For decoders: When selecting this format, CodecContext.HwFramesContext
    /// should be set to a suitable frames context within the GetFormat() callback.
    /// The frames context must have been created on a device of the specified type.
    /// 
    /// For encoders: CodecContext.HwFramesContext should be set to the context
    /// which will be used for the input frames before calling CodecOpen().
    /// </summary>
    HwFramesContext = 0x02,

    /// <summary>
    /// The codec supports this format by some internal method.
    /// 
    /// This format can be selected without any additional configuration -
    /// no device or frames context is required.
    /// </summary>
    Internal = 0x04,

    /// <summary>
    /// The codec supports this format by some ad-hoc method.
    /// 
    /// Additional settings and/or function calls are required.
    /// These methods are deprecated, and alternatives should be used.
    /// </summary>
    AdHoc = 0x08,

    /// <summary>
    /// None
    /// </summary>
    None = 0x0,
}

