namespace FFmpeg.Images;
/// <summary>
/// Represents the visual content value range.
/// </summary>
public enum ColorRange : int
{
    /// <inheritdoc cref="AutoGen._AVColorRange.AVCOL_RANGE_UNSPECIFIED"/>
    Unspecified = AutoGen._AVColorRange.AVCOL_RANGE_UNSPECIFIED,

    /// <summary>
    /// Narrow or limited range content.
    /// </summary>
    /// <inheritdoc cref="AutoGen._AVColorRange.AVCOL_RANGE_MPEG"/>
    MPEG = AutoGen._AVColorRange.AVCOL_RANGE_MPEG,

    /// <summary>
    /// Full range content.
    /// </summary>
    /// <inheritdoc cref="AutoGen._AVColorRange.AVCOL_RANGE_JPEG"/>
    JPEG = AutoGen._AVColorRange.AVCOL_RANGE_JPEG,

    /// <inheritdoc cref="AutoGen._AVColorRange.AVCOL_RANGE_NB"/>
    __COUNT__ = AutoGen._AVColorRange.AVCOL_RANGE_NB
}

