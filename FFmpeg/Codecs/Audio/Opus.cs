namespace FFmpeg.Codecs.Audio;

/// <summary>
/// Settings for the Opus encoder (<c>libopus</c>).
/// </summary>
/// <remarks>Opus encodes at 48, 24, 16, 12 or 8 kHz; resample other rates first.</remarks>
public class Opus : AudioCodec
{
    /// <summary>
    /// Initializes the settings.
    /// </summary>
    /// <exception cref="NotSupportedException">FFmpeg was built without libopus.</exception>
    public Opus() : base(FindEncoder("libopus"))
    {
    }

    /// <summary>Gets whether the loaded FFmpeg libraries contain libopus.</summary>
    public static bool IsSupported => Codec.FindEncoder("libopus") != null;

    /// <summary>Gets or sets the bitrate in bit/s (<c>b</c>), e.g. 96000 for stereo music.</summary>
    public long? Bitrate { get; set; }

    /// <summary>Gets or sets the bitrate mode (<c>vbr</c>).</summary>
    public OpusBitrateMode? BitrateMode { get; set; }

    /// <summary>Gets or sets what the encoder optimizes for (<c>application</c>).</summary>
    public OpusApplication? Application { get; set; }

    /// <summary>Gets or sets the frame duration in milliseconds (<c>frame_duration</c>): 2.5, 5, 10, 20 (default), 40, 60, 80, 100 or 120.</summary>
    public double? FrameDuration { get; set; }

    /// <summary>Gets or sets the encoding complexity (<c>compression_level</c>), 0 to 10 (default, slowest and best).</summary>
    public int? CompressionLevel { get; set; }

    /// <summary>Gets or sets the expected packet loss in percent (<c>packet_loss</c>).</summary>
    public int? PacketLoss { get; set; }

    /// <summary>Gets or sets in-band forward error correction (<c>fec</c>); needs <see cref="PacketLoss"/>.</summary>
    public bool? ForwardErrorCorrection { get; set; }

    /// <summary>Gets or sets discontinuous transmission, which reduces the bitrate during silence (<c>dtx</c>).</summary>
    public bool? DiscontinuousTransmission { get; set; }

    /// <summary>Gets or sets the channel mapping family (<c>mapping_family</c>), e.g. 1 for surround.</summary>
    public int? MappingFamily { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Bitrate.HasValue)
            options.SetContext("b", Bitrate.Value);
        if (BitrateMode.HasValue)
            options.SetPrivate("vbr", BitrateMode.Value switch
            {
                OpusBitrateMode.Constant => "off",
                OpusBitrateMode.Variable => "on",
                _ => "constrained",
            });
        if (Application.HasValue)
            options.SetPrivate("application", Application.Value switch
            {
                OpusApplication.Voip => "voip",
                OpusApplication.Audio => "audio",
                _ => "lowdelay",
            });
        if (FrameDuration.HasValue)
            options.SetPrivate("frame_duration", FrameDuration.Value);
        if (CompressionLevel.HasValue)
            options.SetContext("compression_level", CompressionLevel.Value);
        if (PacketLoss.HasValue)
            options.SetPrivate("packet_loss", PacketLoss.Value);
        if (ForwardErrorCorrection.HasValue)
            options.SetPrivate("fec", ForwardErrorCorrection.Value);
        if (DiscontinuousTransmission.HasValue)
            options.SetPrivate("dtx", DiscontinuousTransmission.Value);
        if (MappingFamily.HasValue)
            options.SetPrivate("mapping_family", MappingFamily.Value);
    }
}

/// <summary>
/// Opus bitrate modes (<c>vbr</c>).
/// </summary>
public enum OpusBitrateMode
{
    /// <summary><c>off</c>: constant bitrate.</summary>
    Constant,
    /// <summary><c>on</c>: variable bitrate, the default.</summary>
    Variable,
    /// <summary><c>constrained</c>: variable bitrate with a limited deviation.</summary>
    ConstrainedVariable,
}

/// <summary>
/// What Opus optimizes for (<c>application</c>).
/// </summary>
public enum OpusApplication
{
    /// <summary><c>voip</c>: speech intelligibility.</summary>
    Voip,
    /// <summary><c>audio</c>: fidelity, the default.</summary>
    Audio,
    /// <summary><c>lowdelay</c>: restricted to the lowest delay modes.</summary>
    LowDelay,
}
