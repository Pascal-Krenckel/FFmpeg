using FFmpeg.Audio;

namespace FFmpeg.Codecs;
/// <summary>
/// Represents an interface for audio encoders, providing properties specific to audio encoding such as sample rate, format, and channel layout.
/// Inherits from <see cref="IEncoderContext"/> for general encoding options.
/// </summary>
public interface IAudioEncoder : IEncoderContext
{
    /// <summary>
    /// Gets or sets the sample rate of the audio being encoded, in samples per second (Hz).
    /// 
    /// <para>
    /// The sample rate defines how many audio samples are taken per second. Common values are 44100 Hz (CD quality) and 48000 Hz (professional audio).
    /// </para>
    /// </summary>
    int SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the sample format of the audio being encoded.
    /// 
    /// <para>
    /// The sample format defines how each audio sample is represented (e.g., 16-bit integer, floating point). This property must be set based on the audio source's format.
    /// </para>
    /// </summary>
    SampleFormat SampleFormat { get; set; }

    /// <summary>
    /// Gets or sets the channel layout of the audio being encoded.
    /// 
    /// <para>
    /// The channel layout describes how audio channels (e.g., mono, stereo, surround) are arranged. This is typically set according to the source material (e.g., stereo layout for two channels).
    /// </para>
    /// </summary>
    ChannelLayout_ref ChannelLayout { get; }

    /// <summary>
    /// Gets or sets the audio service type, which indicates the type of audio being encoded (e.g., main audio, commentary, hearing impaired audio).
    /// 
    /// <para>
    /// This is mainly used for broadcasting or streaming services that require different types of audio streams for various audiences.
    /// </para>
    /// </summary>
    AudioServiceType AudioServiceType { get; set; }
}

