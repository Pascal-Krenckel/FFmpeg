namespace FFmpeg.Images;
/// <summary>
/// Specifies the field order of interlaced video. These values match the ones defined by various video standards.
/// </summary>
public enum FieldOrder : int
{
    /// <summary>
    /// Field order is unknown or not specified.
    /// </summary>
    Unknown = AutoGen._AVFieldOrder.AV_FIELD_UNKNOWN,

    /// <summary>
    /// The video is progressive, meaning it is not interlaced and each frame is a complete image.
    /// </summary>
    Progressive = AutoGen._AVFieldOrder.AV_FIELD_PROGRESSIVE,

    /// <summary>
    /// Top field is coded first and also displayed first. This is used for top-first interlacing.
    /// </summary>
    TopFirst = AutoGen._AVFieldOrder.AV_FIELD_TT,

    /// <summary>
    /// Bottom field is coded first and also displayed first. This is used for bottom-first interlacing.
    /// </summary>
    BottomFirst = AutoGen._AVFieldOrder.AV_FIELD_BB,

    /// <summary>
    /// Top field is coded first, but the bottom field is displayed first. This is used in certain interlacing schemes.
    /// </summary>
    TopCodedBottomDisplayed = AutoGen._AVFieldOrder.AV_FIELD_TB,

    /// <summary>
    /// Bottom field is coded first, but the top field is displayed first. This is used in certain interlacing schemes.
    /// </summary>
    BottomCodedTopDisplayed = AutoGen._AVFieldOrder.AV_FIELD_BT
}

