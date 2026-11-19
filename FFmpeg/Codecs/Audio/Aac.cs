namespace FFmpeg.Codecs.Audio;

/// <summary>
/// Settings for FFmpeg's native AAC encoder (<c>aac</c>).
/// </summary>
/// <remarks>
/// Use <see cref="Bitrate"/> (constant bitrate, e.g. 128 kbit/s for stereo) or <see cref="Quality"/> (variable
/// bitrate, <c>-q:a</c>).
/// </remarks>
public class Aac : AudioCodec
{
    /// <summary>
    /// Initializes the settings.
    /// </summary>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the AAC encoder.</exception>
    public Aac() : base(FindEncoder("aac"))
    {
    }

    /// <summary>Gets or sets the bitrate in bit/s (<c>b</c>).</summary>
    public long? Bitrate { get; set; }

    /// <summary>Gets or sets a variable bitrate quality (<c>-q:a</c>), 0.1 to 2; higher is better.</summary>
    public double? Quality { get; set; }

    /// <summary>Gets or sets the AAC profile (<c>profile</c>).</summary>
    public AacProfile? Profile { get; set; }

    /// <summary>Gets or sets the coding algorithm (<c>aac_coder</c>).</summary>
    public AacCoder? Coder { get; set; }

    /// <summary>Gets or sets mid/side stereo coding (<c>aac_ms</c>); <see langword="null"/> decides per frame.</summary>
    public bool? MidSideStereo { get; set; }

    /// <summary>Gets or sets intensity stereo coding (<c>aac_is</c>).</summary>
    public bool? IntensityStereo { get; set; }

    /// <summary>Gets or sets perceptual noise substitution (<c>aac_pns</c>).</summary>
    public bool? PerceptualNoiseSubstitution { get; set; }

    /// <summary>Gets or sets temporal noise shaping (<c>aac_tns</c>).</summary>
    public bool? TemporalNoiseShaping { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (Bitrate.HasValue)
            options.SetContext("b", Bitrate.Value);
        if (Quality.HasValue)
            options.SetQuality(Quality.Value);
        if (Profile.HasValue)
            options.SetContext("profile", (long)Profile.Value);
        if (Coder.HasValue)
            options.SetPrivate("aac_coder", Coder.Value == AacCoder.TwoLoop ? "twoloop" : "fast");
        if (MidSideStereo.HasValue)
            options.SetPrivate("aac_ms", MidSideStereo.Value);
        if (IntensityStereo.HasValue)
            options.SetPrivate("aac_is", IntensityStereo.Value);
        if (PerceptualNoiseSubstitution.HasValue)
            options.SetPrivate("aac_pns", PerceptualNoiseSubstitution.Value);
        if (TemporalNoiseShaping.HasValue)
            options.SetPrivate("aac_tns", TemporalNoiseShaping.Value);
    }
}

/// <summary>
/// AAC profiles the native encoder supports (FFmpeg 9 only encodes LC).
/// </summary>
public enum AacProfile
{
    /// <summary>AAC LC (low complexity), the default.</summary>
    LowComplexity = AutoGen.ffmpeg.AV_PROFILE_AAC_LOW,
    /// <summary>MPEG-2 AAC LC: LC without perceptual noise substitution, which MPEG-2 AAC does not have. The opened
    /// encoder reports it as <see cref="LowComplexity"/>.</summary>
    Mpeg2LowComplexity = AutoGen.ffmpeg.AV_PROFILE_MPEG2_AAC_LOW,
}

/// <summary>
/// Coding algorithms of the native AAC encoder (<c>aac_coder</c>).
/// </summary>
public enum AacCoder
{
    /// <summary><c>twoloop</c>: two loop searching method, the default.</summary>
    TwoLoop,
    /// <summary><c>fast</c>: fast search.</summary>
    Fast,
}
