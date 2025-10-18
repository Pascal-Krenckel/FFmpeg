using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFmpeg.Codecs.Video.LibSVTAV1;

/// <summary>
/// Represents the bitrate control settings for the SVT-AV1 encoder.
/// Provides configuration options for different bitrate control modes and adaptive quantization.
/// </summary>
public struct BitrateControl : IEquatable<BitrateControl>
{
    /// <summary>
    /// Defines the available bitrate control modes.
    /// </summary>
    public enum BitrateControlMode
    {
        /// <summary>
        /// Constant Rate Factor (CRF) or Constant Quantization Parameter (CQP).
        /// </summary>
        CRF = 0,

        /// <summary>
        /// Variable Bitrate (VBR).
        /// </summary>
        VBR = 1,

        /// <summary>
        /// Constant Bitrate (CBR).
        /// </summary>
        CBR = 2
    }

    /// <summary>
    /// Defines the available adaptive quantization modes.
    /// </summary>
    public enum AdaptiveQuantization
    {
        /// <summary>
        /// Disables adaptive quantization.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Variance-based adaptive quantization.
        /// </summary>
        VarianceBased = 1,

        /// <summary>
        /// Delta Q prediction efficiency mode.
        /// </summary>
        DeltaQPredEfficiency = 2
    }

    /// <summary>
    /// Gets or sets the rate control mode.
    /// Determines the mode used for bitrate control, such as CRF, VBR, or CBR.
    /// </summary>
    public BitrateControlMode? Mode { get; set; }

    /// <summary>
    /// Gets or sets the adaptive quantization mode.
    /// Specifies the mode of adaptive quantization used, if any.
    /// </summary>
    public AdaptiveQuantization? AQMode { get; set; }

    /// <summary>
    /// Gets or sets the initial quantization parameter (QP) value.
    /// Applicable for CRF and CQP modes.
    /// </summary>
    public int? QP { get; set; }

    /// <summary>
    /// Gets or sets the maximum quantizer value.
    /// Valid range is 1-63 and is only applicable for VBR and CBR modes. Default is 63.
    /// </summary>
    public int? MaxQP { get; set; }

    /// <summary>
    /// Gets or sets the minimum quantizer value.
    /// Valid range is 1-63, with the maximum value being MaxQP - 1. Only applicable for VBR and CBR modes. Default is 1.
    /// </summary>
    public int? MinQP { get; set; }

    /// <summary>
    /// Gets or sets the Constant Rate Factor (CRF) value.
    /// Configures the rate control to CRF mode with adaptive quantization set to DeltaQPredEfficiency.
    /// Valid range is 1-63.
    /// </summary>
    [DisallowNull]
    public int? CRF
    {
        readonly get => (Mode == BitrateControlMode.CRF && AQMode == AdaptiveQuantization.DeltaQPredEfficiency) ? QP : null;
        set
        {
            Mode = BitrateControlMode.CRF;
            AQMode = AdaptiveQuantization.DeltaQPredEfficiency;
            QP = value;
        }
    }

    /// <summary>
    /// Gets or sets the target bitrate in bits per second (bps).
    /// Applicable for VBR and CBR modes.
    /// </summary>
    public long? Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the maximum bitrate in bits per second (bps).
    /// Applies only to CRF mode. Accepts suffixes: b, k, and m.
    /// </summary>
    public long? MaxBitrate { get; set; }

    /// <summary>
    /// Gets or sets the bias for CBR/VBR encoding.
    /// A value of 0 is CBR-like and 100 is VBR-like.
    /// </summary>
    public int? VBRBiasPct { get; set; }

    /// <summary>
    /// Adds the bitrate control settings to the given dictionary as string key-value pairs.
    /// </summary>
    /// <param name="dictionary">The dictionary to which the settings will be added.</param>
    public readonly void AddToDictionary(IDictionary<string, string> dictionary)
    {
        if (Mode.HasValue)
            dictionary["rc"] = ((int)Mode.Value).ToString();
        if (AQMode.HasValue)
            dictionary["aq-mode"] = ((int)AQMode.Value).ToString();
        if (QP.HasValue)
            dictionary["qp"] = QP.Value.ToString();
        if (MaxQP.HasValue)
            dictionary["max-qp"] = MaxQP.Value.ToString();
        if (MinQP.HasValue)
            dictionary["min-qp"] = MinQP.Value.ToString();
        if (Bitrate.HasValue)
            dictionary["tbr"] = $"{Bitrate.Value}b";
        if (MaxBitrate.HasValue)
            dictionary["mbr"] = $"{MaxBitrate.Value}b";
        if (VBRBiasPct.HasValue)
            dictionary["bias-pct"] = VBRBiasPct.Value.ToString();
    }

    /// <summary>
    /// Returns a string representation of the bitrate control settings.
    /// </summary>
    /// <returns>A formatted string representing the bitrate control configuration.</returns>
    public override readonly string ToString()
    {
        StringBuilder sb = new();
        if (Mode.HasValue)
            _ = sb.Append($"rc={(int)Mode.Value}:");
        if (AQMode.HasValue)
            _ = sb.Append($"aq-mode={(int)AQMode.Value}:");
        if (QP.HasValue)
            _ = sb.Append($"qp={QP.Value}:");
        if (MaxQP.HasValue)
            _ = sb.Append($"max-qp={MaxQP.Value}:");
        if (MinQP.HasValue)
            _ = sb.Append($"min-qp={MinQP.Value}:");
        if (Bitrate.HasValue)
            _ = sb.Append($"tbr={Bitrate.Value}b:");
        if (MaxBitrate.HasValue)
            _ = sb.Append($"mbr={MaxBitrate.Value}b:");
        if (VBRBiasPct.HasValue)
            _ = sb.AppendLine($"bias-pct={VBRBiasPct.Value}:");

        if (sb.Length > 0 && sb[^1] == ':')
            sb.Length--; // Remove trailing colon

        return sb.ToString();
    }

    /// <summary>
    /// Creates a new instance of <see cref="BitrateControl"/> configured for CRF mode.
    /// </summary>
    /// <param name="crf">The Constant Rate Factor value.</param>
    /// <returns>A <see cref="BitrateControl"/> instance configured for CRF mode.</returns>
    public static BitrateControl CreateCRF(int crf) => new()
    {
        CRF = crf
    };

    /// <summary>
    /// Creates a new instance of <see cref="BitrateControl"/> configured for CBR mode with the specified bitrate.
    /// </summary>
    /// <param name="bitrate">The target bitrate in bits per second.</param>
    /// <returns>A <see cref="BitrateControl"/> instance configured for CBR mode.</returns>
    public static BitrateControl CreateCBR(long bitrate) => new()
    {
        Bitrate = bitrate,
        Mode = BitrateControlMode.CBR
    };

    /// <summary>
    /// Creates a new instance of <see cref="BitrateControl"/> configured for VBR mode with the specified bitrate.
    /// </summary>
    /// <param name="bitrate">The target bitrate in bits per second.</param>
    /// <returns>A <see cref="BitrateControl"/> instance configured for VBR mode.</returns>
    public static BitrateControl CreateVBR(long bitrate) => new()
    {
        Bitrate = bitrate,
        Mode = BitrateControlMode.VBR
    };

    /// <summary>
    /// Creates a new instance of <see cref="BitrateControl"/> configured for CQP mode with the specified quantization parameter.
    /// </summary>
    /// <param name="qp">The quantization parameter (QP) value.</param>
    /// <returns>A <see cref="BitrateControl"/> instance configured for CQP mode.</returns>
    public static BitrateControl CreateCQP(int qp) => new()
    {
        QP = qp,
        Mode = BitrateControlMode.CRF,
        AQMode = AdaptiveQuantization.Off
    };

    /// <inheritdoc />
    public override readonly bool Equals(object? obj) => obj is BitrateControl control && Equals(control);

    /// <inheritdoc />
    public readonly bool Equals(BitrateControl other) =>
        Mode == other.Mode &&
        AQMode == other.AQMode &&
        QP == other.QP &&
        MaxQP == other.MaxQP &&
        MinQP == other.MinQP &&
        Bitrate == other.Bitrate &&
        MaxBitrate == other.MaxBitrate &&
        VBRBiasPct == other.VBRBiasPct;

    /// <inheritdoc />
    public override readonly int GetHashCode() =>
        HashCode.Combine(Mode, AQMode, QP, MaxQP, MinQP, Bitrate, MaxBitrate, VBRBiasPct);

    /// <summary>
    /// Compares two <see cref="BitrateControl"/> objects for equality.
    /// </summary>
    /// <param name="left">The left-hand <see cref="BitrateControl"/>.</param>
    /// <param name="right">The right-hand <see cref="BitrateControl"/>.</param>
    /// <returns><c>true</c> if both objects are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(BitrateControl left, BitrateControl right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="BitrateControl"/> objects for inequality.
    /// </summary>
    /// <param name="left">The left-hand <see cref="BitrateControl"/>.</param>
    /// <param name="right">The right-hand <see cref="BitrateControl"/>.</param>
    /// <returns><c>true</c> if both objects are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(BitrateControl left, BitrateControl right) => !(left == right);
}

