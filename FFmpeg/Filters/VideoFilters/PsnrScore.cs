using FFmpeg.Utils;
using System.Diagnostics.CodeAnalysis;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// The MSE and PSNR of one frame, as the <see cref="Psnr"/> filter stores them in the frame metadata
/// (<c>lavfi.psnr.*</c>).
/// </summary>
public sealed class PsnrScore
{
    private const string MsePrefix = "lavfi.psnr.mse.";
    private const string PsnrPrefix = "lavfi.psnr.psnr.";

    private PsnrScore(IReadOnlyDictionary<char, double> mse, IReadOnlyDictionary<char, double> psnr, double mseAverage, double psnrAverage)
    {
        Mse = mse;
        Psnr = psnr;
        MseAverage = mseAverage;
        PsnrAverage = psnrAverage;
    }

    /// <summary>
    /// Gets the mean squared error of each component, keyed by the lowercase component letter FFmpeg uses:
    /// <c>y</c>, <c>u</c>, <c>v</c> for YUV input, <c>r</c>, <c>g</c>, <c>b</c> for RGB input (plus <c>a</c> with alpha).
    /// </summary>
    public IReadOnlyDictionary<char, double> Mse { get; }

    /// <summary>Gets the PSNR of each component in dB; infinite where the component is identical.</summary>
    public IReadOnlyDictionary<char, double> Psnr { get; }

    /// <summary>Gets the mean squared error of the frame, the components weighted by their size.</summary>
    public double MseAverage { get; }

    /// <summary>Gets the PSNR of the frame in dB; infinite for identical frames.</summary>
    public double PsnrAverage { get; }

    /// <summary>
    /// Reads the PSNR from the metadata of a frame that went through a <see cref="Psnr"/> filter.
    /// </summary>
    /// <param name="frame">A frame from the output of the filter.</param>
    /// <param name="score">The score; <see langword="null"/> if the frame has none.</param>
    /// <returns><see langword="true"/> if the frame has a PSNR score.</returns>
    public static bool TryRead(AVFrame frame, [NotNullWhen(true)] out PsnrScore? score)
    {
        score = null;
        double mseAverage = double.NaN, psnrAverage = double.NaN;
        Dictionary<char, double> mse = [];
        Dictionary<char, double> psnr = [];
        foreach (KeyValuePair<string, string> entry in frame.Metadata)
        {
            if (!entry.Key.StartsWith("lavfi.psnr.", StringComparison.Ordinal) || !FilterMetadata.TryParse(entry.Value, out double value))
                continue;
            if (entry.Key == "lavfi.psnr.mse_avg")
                mseAverage = value;
            else if (entry.Key == "lavfi.psnr.psnr_avg")
                psnrAverage = value;
            else if (entry.Key.Length == MsePrefix.Length + 1 && entry.Key.StartsWith(MsePrefix, StringComparison.Ordinal))
                mse[entry.Key[^1]] = value;
            else if (entry.Key.Length == PsnrPrefix.Length + 1 && entry.Key.StartsWith(PsnrPrefix, StringComparison.Ordinal))
                psnr[entry.Key[^1]] = value;
        }
        if (double.IsNaN(mseAverage))
            return false;
        score = new PsnrScore(mse, psnr, mseAverage, psnrAverage);
        return true;
    }
}

/// <summary>
/// Collects the PSNR of many frames and computes the values FFmpeg logs when the <see cref="Psnr"/> filter
/// is freed (<c>PSNR y:… average:… min:… max:…</c>).
/// </summary>
/// <remarks>
/// <para>
/// Like FFmpeg, the averages are computed from the mean of the MSE values, not by averaging the PSNR values
/// in dB, so frames with a much lower quality weigh more. The results match FFmpeg's log up to the precision of
/// the metadata (FFmpeg stores the per-frame values as single-precision floats).
/// </para>
/// <para>Not thread safe.</para>
/// </remarks>
public sealed class PsnrStatistics
{
    private readonly MseAccumulator average = new();
    private readonly Dictionary<char, MseAccumulator> components = [];
    private double minMse = double.NaN;
    private double maxMse = double.NaN;

    /// <summary>Gets the number of frames added.</summary>
    public int Count { get; private set; }

    /// <summary>Gets the average PSNR in dB (from the mean MSE); <see cref="double.NaN"/> if no frames were added.</summary>
    public double Average => Count == 0 ? double.NaN : average.Psnr(average.Sum / Count);

    /// <summary>Gets the mean of the per-frame MSE values; <see cref="double.NaN"/> if no frames were added.</summary>
    public double AverageMse => Count == 0 ? double.NaN : average.Sum / Count;

    /// <summary>Gets the PSNR of the worst frame in dB; <see cref="double.NaN"/> if no frames were added.</summary>
    public double Min => Count == 0 ? double.NaN : average.Psnr(maxMse);

    /// <summary>Gets the PSNR of the best frame in dB; <see cref="double.NaN"/> if no frames were added.</summary>
    public double Max => Count == 0 ? double.NaN : average.Psnr(minMse);

    /// <summary>Gets the average PSNR of each component in dB (from the mean MSE of the component).</summary>
    public IReadOnlyDictionary<char, double> ComponentAverages
        => components.ToDictionary(kv => kv.Key, kv => kv.Value.Psnr(kv.Value.Sum / Count));

    /// <summary>Adds the score of one frame.</summary>
    /// <param name="score">The score.</param>
    public void Add(PsnrScore score)
    {
        if (score is null)
            throw new ArgumentNullException(nameof(score));
        average.Add(score.MseAverage, score.PsnrAverage);
        minMse = Count == 0 ? score.MseAverage : Math.Min(minMse, score.MseAverage);
        maxMse = Count == 0 ? score.MseAverage : Math.Max(maxMse, score.MseAverage);
        foreach (KeyValuePair<char, double> mse in score.Mse)
        {
            if (!components.TryGetValue(mse.Key, out MseAccumulator? component))
                components[mse.Key] = component = new MseAccumulator();
            component.Add(mse.Value, score.Psnr.TryGetValue(mse.Key, out double psnr) ? psnr : double.NaN);
        }
        Count++;
    }

    /// <summary>Adds the score stored in a frame's metadata, if it has one.</summary>
    /// <param name="frame">A frame from the output of a <see cref="Psnr"/> filter.</param>
    /// <returns><see langword="true"/> if the frame had a score.</returns>
    public bool Add(AVFrame frame)
    {
        if (!PsnrScore.TryRead(frame, out PsnrScore? score))
            return false;
        Add(score);
        return true;
    }

    /// <summary>
    /// Sums the MSE of one component and recovers the squared peak value FFmpeg used
    /// (<c>psnr = 10·log10(max² / mse)</c>), taken from the frame with the largest MSE for the best precision.
    /// </summary>
    private sealed class MseAccumulator
    {
        private double peakSquared = double.NaN;
        private double peakSource;

        public double Sum { get; private set; }

        public void Add(double mse, double psnr)
        {
            Sum += mse;
            if (mse > peakSource && !double.IsNaN(psnr) && !double.IsInfinity(psnr))
            {
                peakSource = mse;
                peakSquared = mse * Math.Pow(10, psnr / 10);
            }
        }

        public double Psnr(double mse) => mse <= 0 ? double.PositiveInfinity : 10.0 * Math.Log10(peakSquared / mse);
    }
}
