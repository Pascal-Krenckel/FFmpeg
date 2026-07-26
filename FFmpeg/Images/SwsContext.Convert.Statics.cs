using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Images;

public unsafe partial class SwsContext
{
    /// <summary>
    /// Converts the source <see cref="AVFrame"/> to the destination <see cref="AVFrame"/> using the current scaling context.
    /// </summary>
    /// <param name="src">The source frame to be converted.</param>
    /// <param name="dst">The destination frame where the converted data will be stored.</param>
    /// <returns>An <see cref="AVResult32"/> value indicating success or failure of the conversion operation.</returns>
    /// <seealso cref="Convert(AVFrame, Image)"/>
    public static AVResult32 Convert(AVFrame src, AVFrame dst, SwsAlgorithm algorithm)
    {
        using SwsContext context = new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, algorithm);
        return context.Convert(src, dst);
    }
}
