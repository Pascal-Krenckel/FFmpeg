namespace FFmpeg.Audio;
/// <summary>
/// Defines the audio sample formats used for encoding and decoding audio data.
/// </summary>
public enum SampleFormat : int
{
    /// <summary>
    /// Invalid or unsupported sample format.
    /// </summary>
    None = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_NONE,

    /// <summary>
    /// Unsigned 8-bit audio sample format.
    /// Each sample is represented as an 8-bit unsigned integer.
    /// </summary>
    UInt8 = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_U8,

    /// <summary>
    /// Signed 16-bit audio sample format.
    /// Each sample is represented as a 16-bit signed integer.
    /// </summary>
    Int16 = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_S16,

    /// <summary>
    /// Signed 32-bit audio sample format.
    /// Each sample is represented as a 32-bit signed integer.
    /// </summary>
    Int32 = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_S32,

    /// <summary>
    /// 32-bit floating-point audio sample format.
    /// Each sample is represented as a 32-bit floating-point number.
    /// </summary>
    Float32 = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_FLT,

    /// <summary>
    /// 64-bit double precision floating-point audio sample format.
    /// Each sample is represented as a 64-bit floating-point number.
    /// </summary>
    Float64 = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_DBL,

    /// <summary>
    /// Unsigned 8-bit audio sample format in planar layout.
    /// Each channel is represented in a separate plane, with each sample as an 8-bit unsigned integer.
    /// </summary>
    UInt8Planar = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_U8P,

    /// <summary>
    /// Signed 16-bit audio sample format in planar layout.
    /// Each channel is represented in a separate plane, with each sample as a 16-bit signed integer.
    /// </summary>
    Int16Planar = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_S16P,

    /// <summary>
    /// Signed 32-bit audio sample format in planar layout.
    /// Each channel is represented in a separate plane, with each sample as a 32-bit signed integer.
    /// </summary>
    Int32Planar = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_S32P,

    /// <summary>
    /// 32-bit floating-point audio sample format in planar layout.
    /// Each channel is represented in a separate plane, with each sample as a 32-bit floating-point number.
    /// </summary>
    Float32Planar = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_FLTP,

    /// <summary>
    /// 64-bit double precision floating-point audio sample format in planar layout.
    /// Each channel is represented in a separate plane, with each sample as a 64-bit floating-point number.
    /// </summary>
    Float64Planar = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_DBLP,

    /// <summary>
    /// Signed 64-bit audio sample format.
    /// Each sample is represented as a 64-bit signed integer.
    /// </summary>
    Int64 = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_S64,

    /// <summary>
    /// Signed 64-bit audio sample format in planar layout.
    /// Each channel is represented in a separate plane, with each sample as a 64-bit signed integer.
    /// </summary>
    Int64Planar = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_S64P,

    /// <summary>
    /// Total number of audio sample formats supported.
    /// This is not a valid sample format but an enumeration count. Do not use this value for actual sample format operations.
    /// </summary>
    __COUNT__ = AutoGen._AVSampleFormat.AV_SAMPLE_FMT_NB
}

