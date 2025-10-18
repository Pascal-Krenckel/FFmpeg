namespace FFmpeg.Codecs;

/// <summary>
/// Represents various codec flags used to control the behavior of codecs in FFmpeg.
/// </summary>
[Flags]
public enum CodecFlags : long
{
    /// <summary>
    /// No codec flags set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Allow decoders to produce frames with data planes that are not aligned to CPU requirements (e.g., due to cropping).
    /// </summary>
    AllowUnalignedFrames = ffmpeg.AV_CODEC_FLAG_UNALIGNED,

    /// <summary>
    /// Use fixed quantization scale.
    /// </summary>
    FixedQuantizationScale = ffmpeg.AV_CODEC_FLAG_QSCALE,

    /// <summary>
    /// Allow 4 motion vectors per macroblock / advanced prediction for H.263.
    /// </summary>
    AdvancedMotionVectors = ffmpeg.AV_CODEC_FLAG_4MV,

    /// <summary>
    /// Output even frames that might be corrupted.
    /// </summary>
    OutputCorruptedFrames = ffmpeg.AV_CODEC_FLAG_OUTPUT_CORRUPT,

    /// <summary>
    /// Use quarter-pixel motion compensation.
    /// </summary>
    QuarterPixelMotionCompensation = ffmpeg.AV_CODEC_FLAG_QPEL,

    /// <summary>
    /// Request the encoder to output reconstructed frames that would be produced by decoding the encoded bitstream.
    /// </summary>
    OutputReconstructedFrames = ffmpeg.AV_CODEC_FLAG_RECON_FRAME,

    /// <summary>
    /// Propagate each packet's opaque data to its corresponding output frame or each frame's opaque data to its corresponding output packet.
    /// </summary>
    PropagateOpaqueData = ffmpeg.AV_CODEC_FLAG_COPY_OPAQUE,

    /// <summary>
    /// Signal to the encoder that the values of frame durations are valid and should be used.
    /// </summary>
    UseFrameDurations = ffmpeg.AV_CODEC_FLAG_FRAME_DURATION,

    /// <summary>
    /// Use internal 2-pass rate control in the first pass mode.
    /// </summary>
    Use2PassRateControlFirstPass = ffmpeg.AV_CODEC_FLAG_PASS1,

    /// <summary>
    /// Use internal 2-pass rate control in the second pass mode.
    /// </summary>
    Use2PassRateControlSecondPass = ffmpeg.AV_CODEC_FLAG_PASS2,

    /// <summary>
    /// Use loop filtering.
    /// </summary>
    UseLoopFilter = ffmpeg.AV_CODEC_FLAG_LOOP_FILTER,

    /// <summary>
    /// Only decode or encode grayscale images.
    /// </summary>
    GrayscaleOnly = ffmpeg.AV_CODEC_FLAG_GRAY,

    /// <summary>
    /// Notify PSNR (Peak Signal-to-Noise Ratio) variables during encoding.
    /// </summary>
    CalculatePSNR = ffmpeg.AV_CODEC_FLAG_PSNR,

    /// <summary>
    /// Use interlaced Discrete Cosine Transform (DCT).
    /// </summary>
    InterlacedDCT = ffmpeg.AV_CODEC_FLAG_INTERLACED_DCT,

    /// <summary>
    /// Force low latency encoding/decoding.
    /// </summary>
    ForceLowDelay = ffmpeg.AV_CODEC_FLAG_LOW_DELAY,

    /// <summary>
    /// Place global headers in extradata instead of every keyframe.
    /// </summary>
    GlobalHeadersInExtradata = ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER,

    /// <summary>
    /// Use only bit-exact operations (except (I)DCT).
    /// </summary>
    BitExactOperations = ffmpeg.AV_CODEC_FLAG_BITEXACT,

    /// <summary>
    /// Use advanced intra coding or MPEG-4 AC prediction.
    /// </summary>
    AdvancedIntraCoding = ffmpeg.AV_CODEC_FLAG_AC_PRED,

    /// <summary>
    /// Use interlaced motion estimation.
    /// </summary>
    InterlacedMotionEstimation = ffmpeg.AV_CODEC_FLAG_INTERLACED_ME,

    /// <summary>
    /// Use closed Group of Pictures (GOP).
    /// </summary>
    ClosedGOP = ffmpeg.AV_CODEC_FLAG_CLOSED_GOP,

    // Flag2 values (shifted by 32 bits)

    /// <summary>
    /// Allow non-spec compliant speedup tricks.
    /// </summary>
    AllowSpeedupTricks = ffmpeg.AV_CODEC_FLAG2_FAST << 32,

    /// <summary>
    /// Skip bitstream encoding.
    /// </summary>
    SkipBitstreamEncoding = ffmpeg.AV_CODEC_FLAG2_NO_OUTPUT << 32,

    /// <summary>
    /// Place global headers at every keyframe instead of in extradata.
    /// </summary>
    GlobalHeadersAtKeyframes = ffmpeg.AV_CODEC_FLAG2_LOCAL_HEADER << 32,

    /// <summary>
    /// Allow input bitstream to be truncated at packet boundaries.
    /// </summary>
    TruncateAtPacketBoundaries = ffmpeg.AV_CODEC_FLAG2_CHUNKS << 32,

    /// <summary>
    /// Discard cropping information from the Sequence Parameter Notify (SPS).
    /// </summary>
    DiscardCroppingInfo = ffmpeg.AV_CODEC_FLAG2_IGNORE_CROP << 32,

    /// <summary>
    /// Show all frames before the first keyframe.
    /// </summary>
    ShowAllFramesBeforeKeyframe = ffmpeg.AV_CODEC_FLAG2_SHOW_ALL << 32,

    /// <summary>
    /// Export motion vectors through frame side data.
    /// </summary>
    ExportMotionVectors = ffmpeg.AV_CODEC_FLAG2_EXPORT_MVS << 32,

    /// <summary>
    /// Do not skip samples and export skip information as frame side data.
    /// </summary>
    ExportSkipInformation = ffmpeg.AV_CODEC_FLAG2_SKIP_MANUAL << 32,

    /// <summary>
    /// Do not reset ASS ReadOrder field on flush for subtitle decoding.
    /// </summary>
    KeepReadOrderOnFlush = ffmpeg.AV_CODEC_FLAG2_RO_FLUSH_NOOP << 32,

    /// <summary>
    /// Generate or parse ICC profiles on encode or decode, as appropriate.
    /// </summary>
    HandleICCProfiles = ffmpeg.AV_CODEC_FLAG2_ICC_PROFILES << 32
}
