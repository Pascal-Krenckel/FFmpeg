using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Codecs;
/// <summary>
/// Defines the interface for a video decoder, which is responsible for decoding video streams.
/// </summary>
public interface IVideoDecoder : IDecoderContext
{
    /// <summary>
    /// Gets the frame rate of the video stream, expressed as a <see cref="Rational"/> value.
    /// </summary>
    /// <value>
    /// The frame rate of the video stream (frames per second), as a <see cref="Rational"/> struct representing the frame rate as a ratio.
    /// </value>
    Rational FrameRate { get; }

    /// <summary>
    /// Gets or sets the width of the video frames in pixels.
    /// </summary>
    /// <value>
    /// The width of the video frames in pixels.
    /// </value>
    int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the video frames in pixels.
    /// </summary>
    /// <value>
    /// The height of the video frames in pixels.
    /// </value>
    int Height { get; set; }

    /// <summary>
    /// Gets or sets the pixel format used by the video decoder.
    /// </summary>
    /// <value>
    /// The <see cref="PixelFormat"/> value representing the format of the pixels in the video frames.
    /// </value>
    PixelFormat PixelFormat { get; set; }
}

