using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Codecs;
/// <summary>
/// Represents an interface for video encoders, providing properties specific to video encoding such as frame rate, resolution, and pixel format.
/// Inherits from <see cref="IEncoderContext"/> for general encoding options.
/// </summary>
public interface IVideoEncoder : IEncoderContext
{
    /// <summary>
    /// Gets or sets the frame rate (in frames per second) of the video being encoded.
    /// 
    /// <para>
    /// This property signals the constant frame rate (CFR) content to the encoder.
    /// </para>
    /// </summary>
    Rational FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the width of the video frame in pixels.
    /// 
    /// <para>
    /// The width (and height) must be set by the user before encoding.
    /// </para>
    /// </summary>
    int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the video frame in pixels.
    /// 
    /// <para>
    /// The height (and width) must be set by the user before encoding.
    /// </para>
    /// </summary>
    int Height { get; set; }

    /// <summary>
    /// Gets or sets the pixel format of the video being encoded.
    /// 
    /// <para>
    /// The pixel format must be set by the user to define how pixels are represented (e.g., YUV, RGB).
    /// </para>
    /// </summary>
    PixelFormat PixelFormat { get; set; }

    /// <summary>
    /// Gets or sets the group of pictures (GOP) size, which defines the number of pictures between keyframes.
    /// 
    /// <para>
    /// A value of 0 indicates intra-only encoding (every frame is a keyframe).
    /// This property must be set by the user for encoding.
    /// </para>
    /// </summary>
    int GOPSize { get; set; }

    /// <summary>
    /// Gets the pass1 encoding statistics output buffer as a read-only span of bytes.
    /// This buffer contains encoding statistics collected during the first pass of encoding.
    /// <para>
    /// This property is set by libavcodec and cannot be modified by the user.
    /// </para>
    /// </summary>
    public ReadOnlySpan<byte> Pass1StatsOutput { get; }

    /// <summary>
    /// Gets or sets the pass2 encoding statistics input buffer as a read-only span of bytes.
    /// This buffer should contain the concatenated statistics from the pass1 output, used for the second pass of encoding.
    /// <para>
    /// This property can be set by the user for pass2 encoding.
    /// </para>
    /// </summary>
    public ReadOnlySpan<byte> Pass2StatsInput { get; set; }

    /// <summary>
    /// Gets or sets the number of threads to be used during encoding.
    /// 
    /// <para>
    /// This property defines how many independent tasks should be passed to the encoder to execute in parallel.
    /// It must be set by the user for encoding.
    /// </para>
    /// </summary>
    int ThreadCount { get; set; }
}

