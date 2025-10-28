namespace FFmpeg.Images;

public static class PixelFormatExtensions
{
    public static int PlaneCount(this PixelFormat pixelFormat) => ffmpeg.av_pix_fmt_count_planes((AutoGen._AVPixelFormat)pixelFormat);

    public static PixelFormat SwapEndianness(this PixelFormat pixelFormat) => (PixelFormat)ffmpeg.av_pix_fmt_swap_endianness((AutoGen._AVPixelFormat)pixelFormat);

    public unsafe static PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat, params ReadOnlySpan<PixelFormat> formats)
    {
        AutoGen._AVPixelFormat best = AutoGen._AVPixelFormat.AV_PIX_FMT_NONE;
        for(int i = 0; i < formats.Length; i++)
        {
            best = ffmpeg.av_find_best_pix_fmt_of_2(best, (AutoGen._AVPixelFormat)formats[i], (AutoGen._AVPixelFormat)pixelFormat, 1, null);
        }
        return (PixelFormat)best;        
    }

    public unsafe static PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat,out FFLoss loss, params ReadOnlySpan<PixelFormat> formats)
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

    public unsafe static PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat, bool useAlpha, params ReadOnlySpan<PixelFormat> formats)
    {
        int alpha = Convert.ToInt32(useAlpha);
        AutoGen._AVPixelFormat best = AutoGen._AVPixelFormat.AV_PIX_FMT_NONE;
        for (int i = 0; i < formats.Length; i++)
        {
            best = ffmpeg.av_find_best_pix_fmt_of_2(best, (AutoGen._AVPixelFormat)formats[i], (AutoGen._AVPixelFormat)pixelFormat, alpha, null);
        }
        return (PixelFormat)best;
    }

    public unsafe static PixelFormat FindBestPixelFormat(this PixelFormat pixelFormat, bool useAlpha, out FFLoss loss, params ReadOnlySpan<PixelFormat> formats)
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

}

