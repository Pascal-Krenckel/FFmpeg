namespace FFmpeg.Codecs;
/// <summary>
/// Flags representing the capabilities of codecs in FFmpeg. These capabilities describe various features or behaviors
/// that a codec can support, such as multi-threading, handling frame sizes, and experimental features.
/// </summary>
[Flags]
public enum CodecCapabilities : int
{
    /// <summary>
    /// Indicates that the codec can avoid probing. This capability is used to skip the probing phase when initializing the codec,
    /// which might be useful in certain scenarios where probing is unnecessary or costly.
    /// </summary>
    AvoidProbing = AutoGen.ffmpeg.AV_CODEC_CAP_AVOID_PROBING,

    /// <summary>
    /// Indicates that the codec supports channel configuration. This capability allows the codec to handle different channel layouts
    /// and audio configurations, making it versatile for various audio formats.
    /// </summary>
    ChannelConfiguration = AutoGen.ffmpeg.AV_CODEC_CAP_CHANNEL_CONF,

    /// <summary>
    /// Indicates that the codec has delay handling capabilities. This allows the codec to manage or report delays in processing,
    /// which can be important for synchronization and buffering.
    /// </summary>
    Delay = AutoGen.ffmpeg.AV_CODEC_CAP_DELAY,

    /// <summary>
    /// Indicates that the codec supports delayed reference frame handling. This capability allows the codec to handle delayed
    /// reference frames in video encoding or decoding, which is important for some types of video compression.
    /// </summary>
    DelayedReference = AutoGen.ffmpeg.AV_CODEC_CAP_DR1,

    /// <summary>
    /// Indicates that the codec can draw horizontal bands. This capability is relevant for codecs that support progressive
    /// scanning or frame-by-frame processing where horizontal bands are used for rendering.
    /// </summary>
    DrawHorizontalBand = AutoGen.ffmpeg.AV_CODEC_CAP_DRAW_HORIZ_BAND,

    /// <summary>
    /// Indicates that the codec supports encoder flushing. This capability allows the codec to flush its internal buffers
    /// and ensure that all encoded data is output, which can be useful for finalizing encoded streams.
    /// </summary>
    EncoderFlush = AutoGen.ffmpeg.AV_CODEC_CAP_ENCODER_FLUSH,

    /// <summary>
    /// Indicates that the codec supports encoder reconstruction of frames. This capability allows the codec to reconstruct
    /// frames after encoding, which can be useful for some advanced encoding techniques.
    /// </summary>
    EncoderReconstructFrame = AutoGen.ffmpeg.AV_CODEC_CAP_ENCODER_RECON_FRAME,

    /// <summary>
    /// Indicates that the codec supports reordered opaque encoder formats. This capability allows the codec to handle
    /// formats where the encoded data might be reordered or opaque, which can be important for certain encoding scenarios.
    /// </summary>
    EncoderReorderedOpaque = AutoGen.ffmpeg.AV_CODEC_CAP_ENCODER_REORDERED_OPAQUE,

    /// <summary>
    /// Indicates that the codec is experimental. This capability means that the codec is not yet fully supported or may
    /// have experimental features that are still under development.
    /// </summary>
    Experimental = AutoGen.ffmpeg.AV_CODEC_CAP_EXPERIMENTAL,

    /// <summary>
    /// Indicates that the codec supports frame-level multi-threading. This capability allows the codec to use multiple
    /// threads for processing frames, improving performance for codecs that benefit from parallelism.
    /// </summary>
    FrameThreads = AutoGen.ffmpeg.AV_CODEC_CAP_FRAME_THREADS,

    /// <summary>
    /// Indicates that the codec supports hardware acceleration. This capability means that the codec can leverage hardware
    /// resources for processing, which can improve performance and efficiency.
    /// </summary>
    Hardware = AutoGen.ffmpeg.AV_CODEC_CAP_HARDWARE,

    /// <summary>
    /// Indicates that the codec supports hybrid processing. This capability allows the codec to use a combination of
    /// different processing methods or techniques, which can be useful for certain types of video or audio encoding.
    /// </summary>
    Hybrid = AutoGen.ffmpeg.AV_CODEC_CAP_HYBRID,

    /// <summary>
    /// Indicates that the codec supports other types of threading beyond frame-level threading. This capability allows
    /// for additional threading options or techniques that may be specific to the codec.
    /// </summary>
    OtherThreads = AutoGen.ffmpeg.AV_CODEC_CAP_OTHER_THREADS,

    /// <summary>
    /// Indicates that the codec supports parameter changes during encoding or decoding. This capability allows the codec
    /// to adapt its parameters dynamically, which can be important for certain encoding or decoding scenarios.
    /// </summary>
    ParameterChange = AutoGen.ffmpeg.AV_CODEC_CAP_PARAM_CHANGE,

    /// <summary>
    /// Indicates that the codec supports slice-level multi-threading. This capability allows the codec to use multiple
    /// threads for processing slices of frames, improving performance for codecs that benefit from parallelism at the slice level.
    /// </summary>
    SliceThreads = AutoGen.ffmpeg.AV_CODEC_CAP_SLICE_THREADS,

    /// <summary>
    /// Indicates that the codec supports handling small last frames. This capability allows the codec to efficiently
    /// process small or incomplete frames, which can be useful for certain types of video streams.
    /// </summary>
    SmallLastFrame = AutoGen.ffmpeg.AV_CODEC_CAP_SMALL_LAST_FRAME,

    /// <summary>
    /// Indicates that the codec supports variable frame sizes. This capability allows the codec to handle frames of varying
    /// sizes, which can be important for adaptive streaming or other scenarios where frame sizes are not fixed.
    /// </summary>
    VariableFrameSize = AutoGen.ffmpeg.AV_CODEC_CAP_VARIABLE_FRAME_SIZE,
}
