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
/// <item><see cref="IQ"/> (3), <see cref="MSSSIM"/> (4), <see cref="VMAF"/> (5) - added in SVT-AV1 4.x; older versions reject them.</item>
/// <item>SVT-AV1 only allows CRF rate control with <see cref="SSIM"/>, <see cref="IQ"/>, <see cref="MSSSIM"/> and <see cref="VMAF"/>.</item>
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

    /// <summary>
    /// Image Quality (IQ): optimized for still images only (e.g. AVIF). SVT-AV1 4.0 and later.
    /// </summary>
    IQ = 3,

    /// <summary>
    /// Multi-scale SSIM, for video and still images. SVT-AV1 4.x.
    /// </summary>
    MSSSIM = 4,

    /// <summary>
    /// Optimized for the VMAF metric, video only. SVT-AV1 4.2 and later.
    /// </summary>
    VMAF = 5,
}

