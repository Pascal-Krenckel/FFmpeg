namespace FFmpeg.Codecs.Audio;

/// <summary>
/// Settings for FFmpeg's native FLAC encoder (<c>flac</c>), lossless.
/// </summary>
public class Flac : AudioCodec
{
    /// <summary>
    /// Initializes the settings.
    /// </summary>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the FLAC encoder.</exception>
    public Flac() : base(FindEncoder("flac"))
    {
    }

    /// <summary>Gets or sets the compression level (<c>compression_level</c>), 0 (fastest) to 12 (smallest), 5 by default.</summary>
    public int? CompressionLevel { get; set; }

    /// <summary>Gets or sets the LPC algorithm (<c>lpc_type</c>).</summary>
    public FlacLpcType? LpcType { get; set; }

    /// <summary>Gets or sets the stereo decorrelation mode (<c>ch_mode</c>).</summary>
    public FlacChannelMode? ChannelMode { get; set; }

    /// <summary>Gets or sets the minimum prediction order (<c>min_prediction_order</c>).</summary>
    public int? MinPredictionOrder { get; set; }

    /// <summary>Gets or sets the maximum prediction order (<c>max_prediction_order</c>).</summary>
    public int? MaxPredictionOrder { get; set; }

    /// <summary>Gets or sets exact rice parameters (<c>exact_rice_parameters</c>): slightly smaller, slower.</summary>
    public bool? ExactRiceParameters { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (CompressionLevel.HasValue)
            options.SetContext("compression_level", CompressionLevel.Value);
        if (LpcType.HasValue)
            options.SetPrivate("lpc_type", LpcType.Value.ToString().ToLowerInvariant());
        if (ChannelMode.HasValue)
            options.SetPrivate("ch_mode", ChannelMode.Value switch
            {
                FlacChannelMode.Auto => "auto",
                FlacChannelMode.Independent => "indep",
                FlacChannelMode.LeftSide => "left_side",
                FlacChannelMode.RightSide => "right_side",
                _ => "mid_side",
            });
        if (MinPredictionOrder.HasValue)
            options.SetPrivate("min_prediction_order", MinPredictionOrder.Value);
        if (MaxPredictionOrder.HasValue)
            options.SetPrivate("max_prediction_order", MaxPredictionOrder.Value);
        if (ExactRiceParameters.HasValue)
            options.SetPrivate("exact_rice_parameters", ExactRiceParameters.Value);
    }
}

/// <summary>
/// FLAC LPC algorithms (<c>lpc_type</c>).
/// </summary>
public enum FlacLpcType
{
    /// <summary><c>none</c>: verbatim and constant subframes only.</summary>
    None,
    /// <summary><c>fixed</c>: fixed predictors only.</summary>
    Fixed,
    /// <summary><c>levinson</c>: Levinson-Durbin recursion.</summary>
    Levinson,
    /// <summary><c>cholesky</c>: Cholesky factorization, slower and slightly better.</summary>
    Cholesky,
}

/// <summary>
/// FLAC stereo decorrelation modes (<c>ch_mode</c>).
/// </summary>
public enum FlacChannelMode
{
    /// <summary><c>auto</c>: chosen per frame, the default.</summary>
    Auto,
    /// <summary><c>indep</c>: independent channels.</summary>
    Independent,
    /// <summary><c>left_side</c></summary>
    LeftSide,
    /// <summary><c>right_side</c></summary>
    RightSide,
    /// <summary><c>mid_side</c></summary>
    MidSide,
}
