using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Images;
/// <summary>
/// Represents different types of data loss in video compression as flags.
/// These values are defined in the FFmpeg library (ffmpeg.FF_LOSS_*).
/// </summary>
[Flags]
public enum FFLoss
{
    /// <summary>
    /// No data loss occurred.
    /// Corresponds to no loss.
    /// </summary>
    None = 0,

    /// <summary>
    /// Loss of alpha (transparency) channel information.
    /// Corresponds to ffmpeg.FF_LOSS_ALPHA.
    /// </summary>
    Alpha = ffmpeg.FF_LOSS_ALPHA,

    /// <summary>
    /// Loss of chroma (color) information, affecting color details.
    /// Corresponds to ffmpeg.FF_LOSS_CHROMA.
    /// </summary>
    Chroma = ffmpeg.FF_LOSS_CHROMA,

    /// <summary>
    /// Loss due to color quantization, reducing color precision.
    /// Corresponds to ffmpeg.FF_LOSS_COLORQUANT.
    /// </summary>
    ColorQuantization = ffmpeg.FF_LOSS_COLORQUANT,

    /// <summary>
    /// Loss of colorspace information, which can cause color distortion.
    /// Corresponds to ffmpeg.FF_LOSS_COLORSPACE.
    /// </summary>
    Colorspace = ffmpeg.FF_LOSS_COLORSPACE,

    /// <summary>
    /// Loss of bit depth, reducing the range of color shades or brightness.
    /// Corresponds to ffmpeg.FF_LOSS_DEPTH.
    /// </summary>
    Depth = ffmpeg.FF_LOSS_DEPTH,

    /// <summary>
    /// Excessive loss of bit depth beyond the necessary resolution.
    /// Corresponds to ffmpeg.FF_LOSS_EXCESS_DEPTH.
    /// </summary>
    ExcessDepth = ffmpeg.FF_LOSS_EXCESS_DEPTH,

    /// <summary>
    /// Loss due to excess resolution, where more resolution is used than needed.
    /// Corresponds to ffmpeg.FF_LOSS_EXCESS_RESOLUTION.
    /// </summary>
    ExcessResolution = ffmpeg.FF_LOSS_EXCESS_RESOLUTION,

    /// <summary>
    /// Loss of resolution, resulting in reduced image clarity or detail.
    /// Corresponds to ffmpeg.FF_LOSS_RESOLUTION.
    /// </summary>
    Resolution = ffmpeg.FF_LOSS_RESOLUTION
}
