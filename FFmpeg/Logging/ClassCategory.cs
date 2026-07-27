using FFmpeg.AutoGen;

namespace FFmpeg.Logging;

/// <summary>
/// Represents the category of an FFmpeg AVClass instance.
/// </summary>
public enum ClassCategory : int
{
    /// <summary>
    /// No specific category.
    /// </summary>
    None = _AVClassCategory.AV_CLASS_CATEGORY_NA,

    /// <summary>
    /// Input format or device category.
    /// </summary>
    Input = _AVClassCategory.AV_CLASS_CATEGORY_INPUT,

    /// <summary>
    /// Output format or device category.
    /// </summary>
    Output = _AVClassCategory.AV_CLASS_CATEGORY_OUTPUT,

    /// <summary>
    /// Muxer category.
    /// </summary>
    Muxer = _AVClassCategory.AV_CLASS_CATEGORY_MUXER,

    /// <summary>
    /// Demuxer category.
    /// </summary>
    Demuxer = _AVClassCategory.AV_CLASS_CATEGORY_DEMUXER,

    /// <summary>
    /// Encoder category.
    /// </summary>
    Encoder = _AVClassCategory.AV_CLASS_CATEGORY_ENCODER,

    /// <summary>
    /// Decoder category.
    /// </summary>
    Decoder = _AVClassCategory.AV_CLASS_CATEGORY_DECODER,

    /// <summary>
    /// Filter category.
    /// </summary>
    Filter = _AVClassCategory.AV_CLASS_CATEGORY_FILTER,

    /// <summary>
    /// Bitstream filter category.
    /// </summary>
    BitstreamFilter = _AVClassCategory.AV_CLASS_CATEGORY_BITSTREAM_FILTER,

    /// <summary>
    /// Software scaler category.
    /// </summary>
    SoftwareScaler = _AVClassCategory.AV_CLASS_CATEGORY_SWSCALER,

    /// <summary>
    /// Software resampler category.
    /// </summary>
    SoftwareResampler = _AVClassCategory.AV_CLASS_CATEGORY_SWRESAMPLER,

    /// <summary>
    /// Hardware device category.
    /// </summary>
    HardwareDevice = _AVClassCategory.AV_CLASS_CATEGORY_HWDEVICE,

    /// <summary>
    /// Video output device category.
    /// </summary>
    VideoOutputDevice = _AVClassCategory.AV_CLASS_CATEGORY_DEVICE_VIDEO_OUTPUT,

    /// <summary>
    /// Video input device category.
    /// </summary>
    VideoInputDevice = _AVClassCategory.AV_CLASS_CATEGORY_DEVICE_VIDEO_INPUT,

    /// <summary>
    /// Audio output device category.
    /// </summary>
    AudioOutputDevice = _AVClassCategory.AV_CLASS_CATEGORY_DEVICE_AUDIO_OUTPUT,

    /// <summary>
    /// Audio input device category.
    /// </summary>
    AudioInputDevice = _AVClassCategory.AV_CLASS_CATEGORY_DEVICE_AUDIO_INPUT,

    /// <summary>
    /// Generic output device category.
    /// </summary>
    DeviceOutput = _AVClassCategory.AV_CLASS_CATEGORY_DEVICE_OUTPUT,

    /// <summary>
    /// Generic input device category.
    /// </summary>
    DeviceInput = _AVClassCategory.AV_CLASS_CATEGORY_DEVICE_INPUT,

    /// <summary>
    /// Number of class categories (not part of the FFmpeg ABI/API).
    /// </summary>
    __Count__ = _AVClassCategory.AV_CLASS_CATEGORY_NB
}