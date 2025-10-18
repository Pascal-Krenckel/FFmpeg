namespace FFmpeg.Utils;
/// <summary>
/// Specifies different types of picture frames used in video coding standards.
/// </summary>
public enum PictureType : int
{
    /// <summary>
    /// Undefined picture type.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_NONE.
    /// </summary>
    None = AutoGen._AVPictureType.AV_PICTURE_TYPE_NONE,

    /// <summary>
    /// Intra-coded picture, also known as an I-frame.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_I.
    /// </summary>
    Intra = AutoGen._AVPictureType.AV_PICTURE_TYPE_I,

    /// <summary>
    /// Predicted picture, also known as a P-frame.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_P.
    /// </summary>
    Predicted = AutoGen._AVPictureType.AV_PICTURE_TYPE_P,

    /// <summary>
    /// Bi-directionally predicted picture, also known as a B-frame.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_B.
    /// </summary>
    BiDirectional = AutoGen._AVPictureType.AV_PICTURE_TYPE_B,

    /// <summary>
    /// S(GMC)-VOP (Global Motion Compensation) picture, used in MPEG-4.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_S.
    /// </summary>
    GlobalMotionCompensated = AutoGen._AVPictureType.AV_PICTURE_TYPE_S,

    /// <summary>
    /// Switching Intra picture, also known as an SI-frame.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_SI.
    /// </summary>
    SwitchingIntra = AutoGen._AVPictureType.AV_PICTURE_TYPE_SI,

    /// <summary>
    /// Switching Predicted picture, also known as an SP-frame.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_SP.
    /// </summary>
    SwitchingPredicted = AutoGen._AVPictureType.AV_PICTURE_TYPE_SP,

    /// <summary>
    /// BI type picture, combining intra and inter-frame characteristics.
    /// Corresponds to AutoGen._AVPictureType.AV_PICTURE_TYPE_BI.
    /// </summary>
    BiType = AutoGen._AVPictureType.AV_PICTURE_TYPE_BI
}

