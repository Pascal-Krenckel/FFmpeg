using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Images;

public partial class SwsContext
{
    /// <summary>
    /// Ensures that a scaling context matches the specified source and destination
    /// codec contexts.
    /// </summary>
    /// <param name="context">
    /// The existing scaling context, or <see langword="null"/>.
    /// </param>
    /// <param name="src">
    /// The source codec context.
    /// </param>
    /// <param name="dst">
    /// The destination codec context.
    /// </param>
    /// <returns>
    /// A compatible <see cref="SwsContext"/>. If the supplied context is not
    /// compatible, it is disposed and replaced with a newly created instance.
    /// </returns>
    /// <remarks>
    /// This helper is intended for applications that repeatedly convert frames
    /// whose dimensions or pixel formats may change during playback or encoding.
    /// </remarks>
    public static SwsContext CheckContext(SwsContext? context, Codecs.CodecContext src, Codecs.CodecContext dst)
    {
        if (context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }

    /// <summary>
    /// Ensures that a scaling context matches the specified source and destination
    /// codec contexts.
    /// </summary>
    /// <param name="context">
    /// The existing scaling context, or <see langword="null"/>.
    /// </param>
    /// <param name="src">
    /// The source codec context.
    /// </param>
    /// <param name="dst">
    /// The destination codec context.
    /// </param>
    /// <returns>
    /// A compatible <see cref="SwsContext"/>. If the supplied context is not
    /// compatible, it is disposed and replaced with a newly created instance.
    /// </returns>
    /// <remarks>
    /// This helper is intended for applications that repeatedly convert frames
    /// whose dimensions or pixel formats may change during playback or encoding.
    /// </remarks>
    public static SwsContext CheckContext(SwsContext? context, AVFrame src, Codecs.CodecContext dst)
    {
        if (context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }
    /// <summary>
    /// Ensures that a scaling context matches the specified source codec context and destination frame.
    /// </summary>
    /// <param name="context">
    /// The existing scaling context, or <see langword="null"/>.
    /// </param>
    /// <param name="src">
    /// The source codec context.
    /// </param>
    /// <param name="dst">
    /// The destination codec context.
    /// </param>
    /// <returns>
    /// A compatible <see cref="SwsContext"/>. If the supplied context is not
    /// compatible, it is disposed and replaced with a newly created instance.
    /// </returns>
    /// <remarks>
    /// This helper is intended for applications that repeatedly convert frames
    /// whose dimensions or pixel formats may change during playback or encoding.
    /// </remarks>
    public static SwsContext CheckContext(SwsContext? context, Codecs.CodecContext src, AVFrame dst)
    {
        if (context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }
    /// <summary>
    /// Ensures that a scaling context matches the specified source and destination frames.
    /// </summary>
    /// <param name="context">
    /// The existing scaling context, or <see langword="null"/>.
    /// </param>
    /// <param name="src">
    /// The source codec context.
    /// </param>
    /// <param name="dst">
    /// The destination codec context.
    /// </param>
    /// <returns>
    /// A compatible <see cref="SwsContext"/>. If the supplied context is not
    /// compatible, it is disposed and replaced with a newly created instance.
    /// </returns>
    /// <remarks>
    /// This helper is intended for applications that repeatedly convert frames
    /// whose dimensions or pixel formats may change during playback or encoding.
    /// </remarks>
    public static SwsContext CheckContext(SwsContext? context, AVFrame src, AVFrame dst)
    {
        if (context == null || context.SourceFormat != src.PixelFormat || context.SourceHeight != src.Height || context.SourceWidth != src.Width
            || context.DestinationFormat != dst.PixelFormat || context.DestinationHeight != dst.Height || context.DestinationWidth != dst.Width)
        {
            context?.Dispose();
            return new(src.Width, src.Height, src.PixelFormat, dst.Width, dst.Height, dst.PixelFormat, context?.Algorithm ?? SwsAlgorithm.Bicubic());
        }
        return context;
    }

}
