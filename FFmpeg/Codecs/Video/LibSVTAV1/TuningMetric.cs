using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Codecs.Video.LibSVTAV1;
/// <summary>
/// Specifies the metric used for tuning the encoding process to optimize quality or performance.
/// </summary>
/// <remarks>
/// The tuning metric determines how the encoder adjusts its settings to balance between different aspects of quality and performance.
/// Valid values are:
/// <list type="bullet">
/// <item><see cref="VQ"/> (0) - Visual Quality: Optimized for subjective visual quality.</item>
/// <item><see cref="PSNR"/> (1) - Peak Signal-to-Noise Ratio: Optimized for objective image quality measurement. This is the default metric.</item>
/// <item><see cref="SSIM"/> (2) - Structural Similarity Index: Optimized for perceived visual quality, measuring structural similarity between images.</item>
/// </list>
/// </remarks>
public enum TuningMetric : int
{
    /// <summary>
    /// Visual Quality (VQ): Optimized for subjective visual quality.
    /// Use this metric if the goal is to achieve the best visual appearance according to human perception.
    /// </summary>
    VQ = 0,

    /// <summary>
    /// Peak Signal-to-Noise Ratio (PSNR): Optimized for objective image quality measurement.
    /// This is the default metric. It measures the ratio between the maximum possible value of a signal and the power of distorting noise.
    /// </summary>
    PSNR = 1,

    /// <summary>
    /// Structural Similarity Index (SSIM): Optimized for measuring perceived visual quality.
    /// This metric assesses image quality based on structural similarity and perceived changes in the image.
    /// </summary>
    SSIM = 2,
}

