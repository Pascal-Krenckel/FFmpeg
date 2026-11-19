using FFmpeg.Utils;
using System.Diagnostics.CodeAnalysis;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// The SSIM of one frame, as the <see cref="Ssim"/> filter stores it in the frame metadata
/// (<c>lavfi.ssim.*</c>).
/// </summary>
public sealed class SsimScore
{
    private const string Prefix = "lavfi.ssim.";

    private SsimScore(IReadOnlyDictionary<char, double> components, double all, double decibels)
    {
        Components = components;
        All = all;
        Decibels = decibels;
    }

    /// <summary>
    /// Gets the SSIM of each component, keyed by the component letter: <c>Y</c>, <c>U</c>, <c>V</c> for YUV
    /// input, <c>R</c>, <c>G</c>, <c>B</c> for RGB input (plus <c>A</c> with alpha).
    /// </summary>
    public IReadOnlyDictionary<char, double> Components { get; }

    /// <summary>
    /// Gets the SSIM of the whole frame, the components weighted by their size (1 means identical).
    /// </summary>
    public double All { get; }

    /// <summary>
    /// Gets <see cref="All"/> in decibels, <c>10·log10(1 / (1 - All))</c>; infinite for identical frames.
    /// </summary>
    public double Decibels { get; }

    /// <summary>
    /// Reads the SSIM from the metadata of a frame that went through an <see cref="Ssim"/> filter.
    /// </summary>
    /// <param name="frame">A frame from the output of the filter.</param>
    /// <param name="score">The score; <see langword="null"/> if the frame has none.</param>
    /// <returns><see langword="true"/> if the frame has an SSIM score.</returns>
    public static bool TryRead(AVFrame frame, [NotNullWhen(true)] out SsimScore? score)
    {
        score = null;
        double all = double.NaN, decibels = double.NaN;
        Dictionary<char, double> components = [];
        foreach (KeyValuePair<string, string> entry in frame.Metadata)
        {
            if (!entry.Key.StartsWith(Prefix, StringComparison.Ordinal))
                continue;
            string name = entry.Key[Prefix.Length..];
            if (!FilterMetadata.TryParse(entry.Value, out double value))
                continue;
            if (name == "All")
                all = value;
            else if (name == "dB")
                decibels = value;
            else if (name.Length == 1)
                components[name[0]] = value;
        }
        if (double.IsNaN(all))
            return false;
        score = new SsimScore(components, all, decibels);
        return true;
    }
}

/// <summary>
/// Collects the SSIM of many frames and computes the averages the way FFmpeg logs them when the
/// <see cref="Ssim"/> filter is freed (<c>SSIM Y:… All:… (dB)</c>).
/// </summary>
/// <remarks>Not thread safe.</remarks>
public sealed class SsimStatistics
{
    private readonly Dictionary<char, double> componentTotals = [];
    private double total;

    /// <summary>Gets the number of frames added.</summary>
    public int Count { get; private set; }

    /// <summary>Gets the average SSIM of all frames; <see cref="double.NaN"/> if none were added.</summary>
    public double Average => Count == 0 ? double.NaN : total / Count;

    /// <summary>Gets <see cref="Average"/> in decibels.</summary>
    public double AverageDecibels => Count == 0 ? double.NaN : ToDecibels(Average);

    /// <summary>Gets the lowest SSIM of a frame; <see cref="double.NaN"/> if none were added.</summary>
    public double Min { get; private set; } = double.NaN;

    /// <summary>Gets the highest SSIM of a frame; <see cref="double.NaN"/> if none were added.</summary>
    public double Max { get; private set; } = double.NaN;

    /// <summary>Gets the average SSIM of each component.</summary>
    public IReadOnlyDictionary<char, double> ComponentAverages
        => componentTotals.ToDictionary(kv => kv.Key, kv => kv.Value / Count);

    /// <summary>Adds the score of one frame.</summary>
    /// <param name="score">The score.</param>
    public void Add(SsimScore score)
    {
        if (score is null)
            throw new ArgumentNullException(nameof(score));
        total += score.All;
        Min = Count == 0 ? score.All : Math.Min(Min, score.All);
        Max = Count == 0 ? score.All : Math.Max(Max, score.All);
        foreach (KeyValuePair<char, double> component in score.Components)
            componentTotals[component.Key] = (componentTotals.TryGetValue(component.Key, out double sum) ? sum : 0) + component.Value;
        Count++;
    }

    /// <summary>Adds the score stored in a frame's metadata, if it has one.</summary>
    /// <param name="frame">A frame from the output of an <see cref="Ssim"/> filter.</param>
    /// <returns><see langword="true"/> if the frame had a score.</returns>
    public bool Add(AVFrame frame)
    {
        if (!SsimScore.TryRead(frame, out SsimScore? score))
            return false;
        Add(score);
        return true;
    }

    /// <summary>Converts an SSIM to decibels like FFmpeg: <c>10·log10(1 / (1 - ssim))</c>.</summary>
    /// <param name="ssim">The SSIM.</param>
    /// <returns>The value in decibels; infinite for 1.</returns>
    public static double ToDecibels(double ssim)
        => Math.Abs(1 - ssim) > 1e-9 ? 10.0 * Math.Log10(1 / (1 - ssim)) : double.PositiveInfinity;
}
