namespace FFmpeg.Audio;

/// <summary>
/// Describes the format and layout of an audio buffer.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AudioBufferInfo"/> struct.
/// </remarks>
/// <param name="format">
/// The sample format of the audio buffer.
/// </param>
/// <param name="channels">
/// The number of audio channels.
/// </param>
/// <param name="alignment">
/// The required alignment, in bytes, for each audio data plane.
/// Specify <c>1</c> to disable additional alignment.
/// </param>
public readonly struct AudioBufferInfo(SampleFormat format, int channels, int alignment = 1)
{
    /// <summary>
    /// Gets the sample format of the audio buffer.
    /// </summary>
    public SampleFormat Format { get; } = format;

    /// <summary>
    /// Gets the number of audio channels in the buffer.
    /// </summary>
    public int Channels { get; } = channels;

    /// <summary>
    /// Gets the required alignment, in bytes, for each audio data plane.
    /// </summary>
    /// <remarks>
    /// This value is passed to FFmpeg when allocating audio buffers. A value of
    /// <c>1</c> disables additional alignment.
    /// </remarks>
    public int Alignment { get; } = alignment;
}
