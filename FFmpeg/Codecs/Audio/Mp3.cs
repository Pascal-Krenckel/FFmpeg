namespace FFmpeg.Codecs.Audio;

/// <summary>
/// Settings for the LAME MP3 encoder (<c>libmp3lame</c>).
/// </summary>
/// <remarks>
/// Use <see cref="Bitrate"/> (constant bitrate, or average bitrate with <see cref="AverageBitrate"/>) or
/// <see cref="Quality"/> (variable bitrate, LAME's <c>-V</c>).
/// </remarks>
public class Mp3 : AudioCodec
{
    /// <summary>
    /// Initializes the settings.
    /// </summary>
    /// <exception cref="NotSupportedException">FFmpeg was built without libmp3lame.</exception>
    public Mp3() : base(FindEncoder("libmp3lame"))
    {
    }

    /// <summary>Gets whether the loaded FFmpeg libraries contain libmp3lame.</summary>
    public static bool IsSupported => Codec.FindEncoder("libmp3lame") != null;

    /// <summary>Gets or sets the bitrate in bit/s (<c>b</c>), e.g. 192000.</summary>
    public long? Bitrate { get; set; }

    /// <summary>Gets or sets average bitrate encoding with <see cref="Bitrate"/> as average (<c>abr</c>).</summary>
    public bool? AverageBitrate { get; set; }

    /// <summary>Gets or sets a variable bitrate quality (<c>-q:a</c>, LAME's <c>-V</c>): 0 (best, about 245 kbit/s) to 9.</summary>
    public double? Quality { get; set; }

    /// <summary>Gets or sets the algorithm quality (<c>compression_level</c>), 0 (best, slowest) to 9.</summary>
    public int? CompressionLevel { get; set; }

    /// <summary>Gets or sets joint stereo coding (<c>joint_stereo</c>), on by default.</summary>
    public bool? JointStereo { get; set; }

    /// <summary>Gets or sets the use of the bit reservoir (<c>reservoir</c>), on by default.</summary>
    public bool? BitReservoir { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Bitrate.HasValue)
            options.SetContext("b", Bitrate.Value);
        if (AverageBitrate.HasValue)
            options.SetPrivate("abr", AverageBitrate.Value);
        if (Quality.HasValue)
            options.SetQuality(Quality.Value);
        if (CompressionLevel.HasValue)
            options.SetContext("compression_level", CompressionLevel.Value);
        if (JointStereo.HasValue)
            options.SetPrivate("joint_stereo", JointStereo.Value);
        if (BitReservoir.HasValue)
            options.SetPrivate("reservoir", BitReservoir.Value);
    }
}
