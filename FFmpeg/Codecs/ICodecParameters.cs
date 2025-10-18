using FFmpeg.Audio;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Codecs;
/// <summary>
/// Defines an interface for accessing and modifying codec parameters, including audio, video, and general stream properties.
/// </summary>
public unsafe interface ICodecParameters
{
    /// <summary>
    /// Gets the pointer to the underlying FFmpeg _AVCodecParameters structure.
    /// </summary>
    internal AutoGen._AVCodecParameters* Parameters { get; }

    /// <summary>
    /// Gets or sets the target bit rate in bits per second for the codec.
    /// </summary>
    int BitRate { get; set; }

    /// <summary>
    /// Gets or sets the number of bits per coded sample.
    /// </summary>
    int BitsPerCodedSample { get; set; }

    /// <summary>
    /// Gets or sets the number of bits per raw (uncoded) sample.
    /// </summary>
    int BitsPerRawSample { get; set; }

    /// <summary>
    /// Gets or sets the block alignment (in bytes) for audio data.
    /// </summary>
    int BlockAlign { get; set; }

    /// <summary>
    /// Gets or sets the chroma sample location for video data.
    /// </summary>
    ChromaLocation ChromaLocation { get; set; }

    /// <summary>
    /// Gets or sets the codec identifier specifying the codec type (e.g., H264, AAC).
    /// </summary>
    CodecID CodecId { get; set; }

    /// <summary>
    /// Gets or sets the codec tag, a four-character code or integer identifying the codec.
    /// </summary>
    FourCC CodecTag { get; set; }

    /// <summary>
    /// Gets or sets the color primaries used for color interpretation in video streams.
    /// </summary>
    ColorPrimaries ColorPrimaries { get; set; }

    /// <summary>
    /// Gets or sets the color range (e.g., limited or full) for video data.
    /// </summary>
    ColorRange ColorRange { get; set; }

    /// <summary>
    /// Gets or sets the color space for video data.
    /// </summary>
    ColorSpace ColorSpace { get; set; }

    /// <summary>
    /// Gets or sets the color transfer characteristic for video data.
    /// </summary>
    ColorTransferCharacteristic ColorTransferCharacteristic { get; set; }

    /// <summary>
    /// Gets a span containing codec-specific extra data, such as headers or tables.
    /// </summary>
    Span<byte> ExtraData { get; }

    /// <summary>
    /// Gets or sets the field order for interlaced video (e.g., top field first).
    /// </summary>
    FieldOrder FieldOrder { get; set; }

    /// <summary>
    /// Gets or sets the frame rate for video streams.
    /// </summary>
    Rational FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the frame size, such as the number of audio samples per frame.
    /// </summary>
    int FrameSize { get; set; }

    /// <summary>
    /// Gets or sets the height of the video frame in pixels.
    /// </summary>
    int Height { get; set; }

    /// <summary>
    /// Gets or sets the initial padding for audio data, in samples.
    /// </summary>
    int InitialPadding { get; set; }

    /// <summary>
    /// Gets or sets the codec level, which may indicate the complexity or feature set.
    /// </summary>
    int Level { get; set; }

    /// <summary>
    /// Gets or sets the media type (e.g., audio, video, subtitle) for the stream.
    /// </summary>
    MediaType MediaType { get; set; }

    /// <summary>
    /// Gets or sets the codec profile, which defines the feature set and restrictions for the codec.
    /// </summary>
    Profile Profile { get; set; }

    /// <summary>
    /// Gets or sets the sample aspect ratio for video frames.
    /// </summary>
    Rational SampleAspectRatio { get; set; }

    /// <summary>
    /// Gets or sets the audio sample rate in Hz.
    /// </summary>
    int SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the seek preroll, which is the number of samples to discard before playback starts.
    /// </summary>
    int SeekPreroll { get; set; }

    /// <summary>
    /// Gets or sets the trailing padding for audio data, in samples.
    /// </summary>
    int TrailingPadding { get; set; }

    /// <summary>
    /// Gets or sets the video delay, which is the number of frames of delay for video decoding.
    /// </summary>
    int VideoDelay { get; set; }

    /// <summary>
    /// Gets or sets the width of the video frame in pixels.
    /// </summary>
    int Width { get; set; }

    /// <summary>
    /// Copies all codec parameters from another ICodecParameters instance.
    /// </summary>
    /// <param name="other">The source codec parameters to copy from.</param>
    void CopyFrom(ICodecParameters other);

    /// <summary>
    /// Copies all codec parameters from a CodecContext instance.
    /// </summary>
    /// <param name="context">The source codec context to copy from.</param>
    void CopyFrom(CodecContext context);

    /// <summary>
    /// Copies all codec parameters to a CodecContext instance.
    /// </summary>
    /// <param name="context">The destination codec context to copy to.</param>
    void CopyTo(CodecContext context);

    /// <summary>
    /// Calculates the duration of an audio frame based on the number of bytes in the frame.
    /// </summary>
    /// <param name="frameBytes">The number of bytes in the audio frame.</param>
    /// <returns>The duration of the frame in samples.</returns>
    int GetAudioFrameDuration(int frameBytes);

    /// <summary>
    /// Sets the codec-specific extra data from the provided byte span.
    /// </summary>
    /// <param name="data">A read-only span containing the extra data to set.</param>
    void SetExtraData(ReadOnlySpan<byte> data);

    /// <summary>
    /// Gets the number of audio channels for the current codec parameters.
    /// </summary>
    /// <remarks>
    /// This value indicates the number of channels in the audio stream, such as 1 for mono, 2 for stereo, or higher for surround formats.
    /// The meaning is related to the <see cref="ChannelLayout"/> property.
    /// </remarks>
    public int Channels { get; }

    /// <summary>
    /// Gets or sets the channel layout, which defines the spatial arrangement of audio channels (e.g., mono, stereo, 5.1 surround).
    /// </summary>
    /// <remarks>
    /// The channel layout determines how audio channels are positioned and should match the number of channels and the codec in use.
    /// </remarks>
    public ChannelLayout ChannelLayout
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the sample format, which specifies how audio samples are stored and processed.
    /// </summary>
    /// <remarks>
    /// Ensure the assigned value is compatible with the codec and audio stream.
    /// </remarks>
    public SampleFormat SampleFormat
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the pixel format, which specifies how video frames are stored and processed.
    /// </summary>
    /// <remarks>
    /// Ensure the assigned value is compatible with the codec and video stream.
    /// </remarks>
    public PixelFormat PixelFormat
    {
        get;
        set;
    }
}
