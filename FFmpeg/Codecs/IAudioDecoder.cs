using FFmpeg.Audio;

namespace FFmpeg.Codecs;
/// <summary>
/// Represents an audio decoder providing access to various properties of the audio stream being decoded.
/// </summary>
public interface IAudioDecoder : IDecoderContext
{
    /// <summary>
    /// Gets the audio sample format, which is set by the decoder during the decoding process.
    /// </summary>
    SampleFormat SampleFormat { get; }

    /// <summary>
    /// Gets the sample rate, representing the number of audio samples per second.
    /// </summary>
    int SampleRate { get; }

    /// <summary>
    /// Gets the audio channel layout, which may be set by the caller if known, such as from the container. 
    /// The decoder can override this during decoding if necessary.
    /// </summary>
    ChannelLayout_ref ChannelLayout { get; }

    /// <summary>
    /// Gets the type of service that the audio stream conveys, as determined by the decoder during decoding.
    /// </summary>
    AudioServiceType AudioServiceType { get; }
}
