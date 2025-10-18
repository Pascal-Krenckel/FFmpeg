using FFmpeg.Options;
using FFmpeg.Utils;

namespace FFmpeg.Codecs;

/// <summary>
/// Represents the context for encoding operations, including codec information and encoding settings.
/// This interface manages encoding state and allows querying and setting options for the encoder.
/// </summary>
public interface IEncoderContext : IDisposable, IOptionQuery
{
    /// <summary>
    /// Gets the codec that is used for encoding.
    /// </summary>
    Codec Codec { get; }

    /// <summary>
    /// Gets the type of media (video, audio, etc.) handled by the codec.
    /// </summary>
    MediaType CodecType { get; }

    /// <summary>
    /// Gets the specific codec identifier, which uniquely identifies the codec being used.
    /// </summary>
    CodecID CodecID { get; }

    /// <summary>
    /// Gets or sets the FourCC (four-character code) tag for the codec.
    /// 
    /// <para>
    /// The codec tag is used to identify the codec in a container file. 
    /// For encoding, this is set by the user. If not set, the default based on <see cref="CodecID"/> will be used.
    /// </para>
    /// <para>
    /// The FourCC tag is stored with the least significant byte (LSB) first, so the string "ABCD" is represented as
    /// <code>('D'&lt;&lt;24) + ('C'&lt;&lt;16) + ('B'&lt;&lt;8) + 'A'</code>.
    /// </para>
    /// </summary>
    FourCC CodecTag { get; set; }


    /// <summary>
    /// Gets or sets the average bit rate for encoding.
    /// 
    /// <para>
    /// The bit rate controls the amount of data processed per second during encoding. 
    /// This must be set by the user unless a constant quantizer encoding mode is being used.
    /// </para>
    /// </summary>
    long BitRate { get; set; }

    /// <summary>
    /// Gets or sets the codec flags, which provide additional control over codec behavior.
    /// 
    /// <para>
    /// These flags can adjust how the encoder behaves during the encoding process.
    /// </para>
    /// </summary>
    CodecFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the time base, which is the fundamental unit of time (in seconds) for frame timestamps.
    /// 
    /// <para>
    /// For fixed-frame-rate content, the time base should be set to 1/framerate, and timestamp increments should be exactly 1.
    /// This often represents the inverse of the frame rate for video codecs.
    /// </para>
    /// <para>
    /// The time base must be set by the user during encoding.
    /// </para>
    /// </summary>
    Rational TimeBase { get; set; }
}
