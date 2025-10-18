using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Images;
public static class PixelFormatExtensions
{
    public static int PlaneCount(this PixelFormat pixelFormat) => ffmpeg.av_pix_fmt_count_planes((AutoGen._AVPixelFormat)pixelFormat);

    public static PixelFormat SwapEndianness(this PixelFormat pixelFormat) => (PixelFormat)ffmpeg.av_pix_fmt_swap_endianness((AutoGen._AVPixelFormat)pixelFormat);

}
