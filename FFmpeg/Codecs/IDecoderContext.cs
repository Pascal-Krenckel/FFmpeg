using FFmpeg.Utils;

namespace FFmpeg.Codecs;

/// <summary>
/// Defines the common functionality exposed by decoder contexts.
/// </summary>
/// <remarks>
/// This interface provides access to codec information, configuration options,
/// and common properties shared by all decoder context implementations.
/// </remarks>
public interface IDecoderContext : IDisposable, Options.IOptionQueryable
{
    /// <summary>
    /// Gets the codec associated with this decoder.
    /// </summary>
    Codec Codec { get; }

    /// <summary>
    /// Gets the type of media handled by the decoder, such as video, audio, or subtitles.
    /// </summary>
    MediaType CodecType { get; }

    /// <summary>
    /// Gets the identifier of the codec associated with this decoder.
    /// </summary>
    CodecID CodecID { get; }

    /// <summary>
    /// Gets or sets the FourCC (four-character code) associated with the codec.
    ///
    /// <para>
    /// The codec tag identifies the codec within a media container. During
    /// decoding, it is typically populated from the input stream. During
    /// encoding, it may be specified explicitly; otherwise, a default value
    /// appropriate for the selected <see cref="CodecID"/> is used.
    /// </para>
    ///
    /// <para>
    /// The FourCC value is stored with the least significant byte (LSB) first.
    /// For example, the string <c>"ABCD"</c> is represented as:
    /// <code>('D' &lt;&lt; 24) + ('C' &lt;&lt; 16) + ('B' &lt;&lt; 8) + 'A'</code>.
    /// </para>
    /// </summary>
    FourCC CodecTag { get; set; }

    /// <summary>
    /// Gets or sets the average bit rate of the media stream.
    /// </summary>
    /// <remarks>
    /// During decoding, this value is typically read from the input stream.
    /// During encoding, it controls the target average bit rate unless another
    /// rate control mode is used.
    /// </remarks>
    long BitRate { get; set; }

    /// <summary>
    /// Gets or sets codec-specific flags that control codec behavior.
    /// </summary>
    CodecFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the time base in which packet timestamps
    /// (<see cref="AVPacket.PresentationTimestamp"/> and
    /// <see cref="AVPacket.DecompressionTimestamp"/>) are expressed.
    /// </summary>
    Rational PacketTimeBase { get; set; }
}
