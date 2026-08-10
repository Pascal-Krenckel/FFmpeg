using FFmpeg.AutoGen;

namespace FFmpeg.Images;

/// <summary>
/// Provides extension methods for working with <see cref="PixelFormat"/> values.
/// </summary>
/// <remarks>
/// This class provides helpers for querying pixel format information and selecting
/// suitable pixel formats using FFmpeg's pixel format utilities.
/// </remarks>
public static class PixelFormatExtensions
{
    /// <summary>
    /// Gets the FFmpeg name of the specified pixel format.
    /// </summary>
    /// <param name="pixelFormat">The pixel format to convert to its FFmpeg name.</param>
    /// <returns>
    /// The FFmpeg name of the pixel format, or <see langword="null"/> if the pixel format is not recognized.
    /// </returns>
    public static string ToFFmpegString(this PixelFormat pixelFormat)
        => ffmpeg.av_get_pix_fmt_name((_AVPixelFormat)pixelFormat);

    /// <summary>
    /// Gets the number of data planes used by the pixel format.
    /// </summary>
    /// <param name="pixelFormat">
    /// The pixel format to query.
    /// </param>
    /// <returns>
    /// The number of planes used by the pixel format.
    /// </returns>
    public static int PlaneCount(this PixelFormat pixelFormat) => ffmpeg.av_pix_fmt_count_planes((AutoGen._AVPixelFormat)pixelFormat);

    /// <summary>
    /// Gets the bits per pixel.
    /// </summary>
    /// <param name="pixelFormat">
    /// The pixel format to query.
    /// </param>
    /// <returns>
    /// The bits per pixel.
    /// </returns>
    public unsafe static int BitsPerPixel(this PixelFormat pixelFormat)
    {
        return ffmpeg.av_get_bits_per_pixel(ffmpeg.av_pix_fmt_desc_get((_AVPixelFormat)pixelFormat));
    }

    /// <summary>
    /// Swaps the byte order of a pixel format.
    /// </summary>
    /// <param name="pixelFormat">
    /// The pixel format whose endianness should be swapped.
    /// </param>
    /// <returns>
    /// The pixel format with the opposite byte order, or
    /// <see cref="PixelFormat.None"/> if the operation is not supported.
    /// </returns>
    public static PixelFormat SwapEndianness(this PixelFormat pixelFormat) => (PixelFormat)ffmpeg.av_pix_fmt_swap_endianness((AutoGen._AVPixelFormat)pixelFormat);

    /// <summary>
    /// Finds the best matching pixel format from a list of candidates.
    /// </summary>
    /// <param name="pixelFormat">
    /// The source pixel format to convert from.
    /// </param>
    /// <param name="formats">
    /// The candidate pixel formats to evaluate.
    /// </param>
    /// <returns>
    /// The best matching pixel format, or
    /// <see cref="PixelFormat.None"/> if no suitable format was found.
    /// </returns>
    /// <remarks>
    /// This method selects the candidate format with the lowest conversion loss
    /// according to FFmpeg's pixel format selection rules.
    /// </remarks>
    public static unsafe PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat, params ReadOnlySpan<PixelFormat> formats)
    {
        AutoGen._AVPixelFormat best = AutoGen._AVPixelFormat.AV_PIX_FMT_NONE;
        for (int i = 0; i < formats.Length; i++)
        {
            best = ffmpeg.av_find_best_pix_fmt_of_2(best, (AutoGen._AVPixelFormat)formats[i], (AutoGen._AVPixelFormat)pixelFormat, 1, null);
        }
        return (PixelFormat)best;
    }
    /// <summary>
    /// Finds the best matching pixel format from a list of candidates and returns
    /// the conversion loss information.
    /// </summary>
    /// <param name="pixelFormat">
    /// The source pixel format to convert from.
    /// </param>
    /// <param name="loss">
    /// Receives the reported conversion loss.
    /// </param>
    /// <param name="formats">
    /// The candidate pixel formats to evaluate.
    /// </param>
    /// <returns>
    /// The best matching pixel format, or
    /// <see cref="PixelFormat.None"/> if no suitable format was found.
    /// </returns>
    /// <remarks>
    /// The loss flags describe quality differences introduced when converting
    /// from the source format to the selected format.
    /// </remarks>
    public static unsafe PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat, out FFLoss loss, params ReadOnlySpan<PixelFormat> formats)
    {
        int l = 0;
        AutoGen._AVPixelFormat best = AutoGen._AVPixelFormat.AV_PIX_FMT_NONE;
        for (int i = 0; i < formats.Length; i++)
        {
            best = ffmpeg.av_find_best_pix_fmt_of_2(best, (AutoGen._AVPixelFormat)formats[i], (AutoGen._AVPixelFormat)pixelFormat, 1, &l);
        }
        loss = (FFLoss)l;
        return (PixelFormat)best;
    }
    /// <summary>
    /// Finds the best matching pixel format from a list of candidates while
    /// considering alpha channel preservation.
    /// </summary>
    /// <param name="pixelFormat">
    /// The source pixel format to convert from.
    /// </param>
    /// <param name="useAlpha">
    /// <see langword="true"/> to prefer formats that preserve an alpha channel;
    /// otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="formats">
    /// The candidate pixel formats to evaluate.
    /// </param>
    /// <returns>
    /// The best matching pixel format, or
    /// <see cref="PixelFormat.None"/> if no suitable format was found.
    /// </returns>
    public static unsafe PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat, bool useAlpha, params ReadOnlySpan<PixelFormat> formats)
    {
        int alpha = Convert.ToInt32(useAlpha);
        AutoGen._AVPixelFormat best = AutoGen._AVPixelFormat.AV_PIX_FMT_NONE;
        for (int i = 0; i < formats.Length; i++)
        {
            best = ffmpeg.av_find_best_pix_fmt_of_2(best, (AutoGen._AVPixelFormat)formats[i], (AutoGen._AVPixelFormat)pixelFormat, alpha, null);
        }
        return (PixelFormat)best;
    }
    /// <summary>
    /// Finds the best matching pixel format from a list of candidates while
    /// considering alpha channel preservation and returns the conversion loss.
    /// </summary>
    /// <param name="pixelFormat">
    /// The source pixel format to convert from.
    /// </param>
    /// <param name="useAlpha">
    /// <see langword="true"/> to prefer formats that preserve an alpha channel;
    /// otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="loss">
    /// Receives the reported conversion loss.
    /// </param>
    /// <param name="formats">
    /// The candidate pixel formats to evaluate.
    /// </param>
    /// <returns>
    /// The best matching pixel format, or
    /// <see cref="PixelFormat.None"/> if no suitable format was found.
    /// </returns>
    public static unsafe PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat, bool useAlpha, out FFLoss loss, params ReadOnlySpan<PixelFormat> formats)
    {
        int l = 0;
        int alpha = Convert.ToInt32(useAlpha);
        AutoGen._AVPixelFormat best = AutoGen._AVPixelFormat.AV_PIX_FMT_NONE;
        for (int i = 0; i < formats.Length; i++)
        {
            best = ffmpeg.av_find_best_pix_fmt_of_2(best, (AutoGen._AVPixelFormat)formats[i], (AutoGen._AVPixelFormat)pixelFormat, alpha, &l);
        }
        loss = (FFLoss)l;
        return (PixelFormat)best;
    }

    extension(PixelFormat)
    {
        /// <summary>
        /// Parses a pixel format name into a <see cref="PixelFormat"/> value.
        /// </summary>
        /// <param name="name">
        /// The FFmpeg pixel format name.
        /// </param>
        /// <returns>
        /// The corresponding <see cref="PixelFormat"/> value, or
        /// <see cref="PixelFormat.None"/> if the name is not recognized.
        /// </returns>
        public static PixelFormat Parse(string name) => (PixelFormat)ffmpeg.av_get_pix_fmt(name);
    }
}

