namespace FFmpeg.Codecs;

/// <summary>
/// Flags representing various decode errors.
/// </summary>
[Flags]
public enum DecodeErrorFlags : int
{
    /// <summary>
    /// No error.
    /// </summary>
    None = 0,

    /// <summary>
    /// Error due to active concealment.
    /// </summary>
    ConcealmentActive = ffmpeg.FF_DECODE_ERROR_CONCEALMENT_ACTIVE,

    /// <summary>
    /// Error while decoding slices.
    /// </summary>
    DecodeSlices = ffmpeg.FF_DECODE_ERROR_DECODE_SLICES,

    /// <summary>
    /// Error due to an invalid bitstream.
    /// </summary>
    InvalidBitstream = ffmpeg.FF_DECODE_ERROR_INVALID_BITSTREAM,

    /// <summary>
    /// Error due to a missing reference.
    /// </summary>
    MissingReference = ffmpeg.FF_DECODE_ERROR_MISSING_REFERENCE
}


