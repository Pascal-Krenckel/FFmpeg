namespace FFmpeg.Audio;

public readonly struct AudioBufferInfo
{
    public SampleFormat Format { get; }
    public int Channels { get; }
    public int Alignment { get; }

    public AudioBufferInfo(SampleFormat format, int channels, int alignment = 1)
    {
        Format = format;
        Alignment = alignment;
        Channels = channels;
    }
}
