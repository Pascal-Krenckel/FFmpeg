using FFmpeg.Audio;
using FFmpeg.Utils;
using System.Text;

namespace FFmpeg.Codecs.Audio;

/// <summary>
/// Base class for typed audio encoder settings such as <see cref="Opus"/> or <see cref="Aac"/>.
/// </summary>
/// <remarks>
/// A sample rate, sample format or channel layout that is not set (0, <see cref="SampleFormat.None"/>,
/// <see langword="null"/>) is left to the encoder context; see <see cref="EncoderSettings"/>.
/// </remarks>
public abstract class AudioCodec : EncoderSettings
{
    /// <summary>
    /// Initializes the settings for an audio encoder.
    /// </summary>
    /// <param name="codec">The audio encoder.</param>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is not an audio encoder.</exception>
    protected AudioCodec(Codec codec) : base(codec)
    {
        if (codec.MediaType != MediaType.Audio)
            throw new ArgumentException($"{codec.Name} is not an audio encoder.", nameof(codec));
    }

    /// <summary>
    /// Gets or sets the sample rate in Hz; 0 leaves it unset (<see cref="EncoderSettings.CreateCodecContext"/> then
    /// uses 48000 Hz, or the encoder's first supported rate if it does not support 48000 Hz).
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the sample format; <see cref="SampleFormat.None"/> leaves it unset
    /// (<see cref="EncoderSettings.CreateCodecContext"/> then uses the encoder's first supported format).
    /// </summary>
    public SampleFormat SampleFormat { get; set; } = SampleFormat.None;

    /// <summary>
    /// Gets or sets the channel layout; <see langword="null"/> leaves it unset
    /// (<see cref="EncoderSettings.CreateCodecContext"/> then uses stereo).
    /// </summary>
    /// <remarks>The layout is copied into the codec context; the settings do not dispose it.</remarks>
    public ChannelLayout? ChannelLayout { get; set; }

    /// <inheritdoc/>
    private protected override void ConfigureFormat(CodecContext context)
    {
        if (SampleFormat != SampleFormat.None)
        {
            ReadOnlySpan<SampleFormat> supported = Codec.SupportedSampleFormats;
            if (!supported.IsEmpty && !Contains(supported, SampleFormat))
                throw new ArgumentException($"{Codec.Name} does not support the sample format {SampleFormat}.", nameof(SampleFormat));
            context.SampleFormat = SampleFormat;
        }
        if (SampleRate > 0)
        {
            ReadOnlySpan<int> supported = Codec.SupportedSampleRates;
            if (!supported.IsEmpty && supported.IndexOf(SampleRate) < 0)
                throw new ArgumentException($"{Codec.Name} does not support the sample rate {SampleRate} Hz.", nameof(SampleRate));
            context.SampleRate = SampleRate;
            context.TimeBase = new Rational(1, SampleRate);
        }
        if (ChannelLayout != null)
            context.ChannelLayout.CopyFrom(ChannelLayout);
    }

    /// <inheritdoc/>
    private protected override void ApplyDefaults(CodecContext context)
    {
        if (context.SampleFormat == SampleFormat.None && !Codec.SupportedSampleFormats.IsEmpty)
            context.SampleFormat = Codec.SupportedSampleFormats[0];
        if (context.SampleRate <= 0)
        {
            ReadOnlySpan<int> supported = Codec.SupportedSampleRates;
            context.SampleRate = supported.IsEmpty || supported.IndexOf(48000) >= 0 ? 48000 : supported[0];
        }
        if (!context.TimeBase.IsValidTimeBase)
            context.TimeBase = new Rational(1, context.SampleRate);
        if (context.ChannelLayout.Channels == 0)
        {
            using ChannelLayout stereo = FFmpeg.Audio.ChannelLayout.CreateStereo();
            context.ChannelLayout.CopyFrom(stereo);
        }
    }

    /// <inheritdoc/>
    private protected override void AppendFormat(StringBuilder sb)
    {
        if (SampleRate > 0)
            _ = sb.Append($" -ar {SampleRate}");
        if (SampleFormat != SampleFormat.None)
            _ = sb.Append($" -sample_fmt {AutoGen.ffmpeg.av_get_sample_fmt_name((AutoGen._AVSampleFormat)SampleFormat)}");
        if (ChannelLayout != null)
            _ = sb.Append($" -ch_layout {ChannelLayout}");
    }
}
