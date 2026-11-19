using System.Globalization;
using System.Xml.Linq;

namespace FFmpeg.Filters.VideoFilters;

/// <summary>
/// The XML log the <see cref="LibVmaf"/> filter writes when it is freed: the per-frame metrics and the pooled
/// scores.
/// </summary>
/// <example>
/// <code>
/// using (FilterGraph graph = FilterGraph.Allocate())
/// {
///     // ... sources, LibVmaf.Create("vmaf", "vmaf.xml", graph), sink; push all frames and flush ...
/// } // disposing the graph frees the filter, which writes the log
/// VmafLog log = VmafLog.Load("vmaf.xml");
/// Console.WriteLine($"VMAF {log.Score:F2}");
/// </code>
/// </example>
public sealed class VmafLog
{
    private VmafLog(string? version, IReadOnlyList<VmafFrameMetrics> frames, IReadOnlyDictionary<string, VmafPooledMetric> pooledMetrics)
    {
        Version = version;
        Frames = frames;
        PooledMetrics = pooledMetrics;
    }

    /// <summary>Gets the libvmaf version that wrote the log.</summary>
    public string? Version { get; }

    /// <summary>Gets the metrics of every scored frame (every <see cref="LibVmaf.Subsample"/>-th frame).</summary>
    public IReadOnlyList<VmafFrameMetrics> Frames { get; }

    /// <summary>
    /// Gets the pooled scores, keyed by metric name: the model names (<c>vmaf</c> by default) and the features
    /// libvmaf computed.
    /// </summary>
    public IReadOnlyDictionary<string, VmafPooledMetric> PooledMetrics { get; }

    /// <summary>
    /// Gets the mean VMAF score of the default model (metric <c>vmaf</c>), the value FFmpeg logs with the default
    /// <see cref="LibVmaf.Pool"/>.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The log has no metric named <c>vmaf</c> (the model was renamed).</exception>
    public double Score => PooledMetrics["vmaf"].Mean;

    /// <summary>Reads a log file.</summary>
    /// <param name="path">The path of the XML log (<see cref="LibVmaf.LogPath"/>).</param>
    /// <returns>The log.</returns>
    /// <exception cref="FormatException">The file is not a libvmaf XML log.</exception>
    public static VmafLog Load(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Load(stream);
    }

    /// <summary>Reads a log from a stream.</summary>
    /// <param name="stream">A stream with the XML log.</param>
    /// <returns>The log.</returns>
    /// <exception cref="FormatException">The stream does not contain a libvmaf XML log.</exception>
    public static VmafLog Load(Stream stream)
    {
        XElement root = XDocument.Load(stream).Root!;
        if (root.Name != "VMAF")
            throw new FormatException($"Expected a libvmaf XML log (root element VMAF), got <{root.Name}>.");

        List<VmafFrameMetrics> frames = [];
        foreach (XElement frame in root.Element("frames")?.Elements("frame") ?? [])
        {
            int frameNumber = 0;
            Dictionary<string, double> metrics = [];
            foreach (XAttribute attribute in frame.Attributes())
            {
                if (attribute.Name == "frameNum")
                    frameNumber = int.Parse(attribute.Value, CultureInfo.InvariantCulture);
                else if (FilterMetadata.TryParse(attribute.Value, out double value))
                    metrics[attribute.Name.LocalName] = value;
            }
            frames.Add(new VmafFrameMetrics(frameNumber, metrics));
        }

        Dictionary<string, VmafPooledMetric> pooled = [];
        foreach (XElement metric in root.Element("pooled_metrics")?.Elements("metric") ?? [])
        {
            string? name = (string?)metric.Attribute("name");
            if (name != null)
                pooled[name] = new VmafPooledMetric(Get(metric, "min"), Get(metric, "max"), Get(metric, "mean"), Get(metric, "harmonic_mean"));
        }

        return new VmafLog((string?)root.Attribute("version"), frames, pooled);

        static double Get(XElement element, string attribute)
            => element.Attribute(attribute) is XAttribute a && FilterMetadata.TryParse(a.Value, out double value) ? value : double.NaN;
    }
}

/// <summary>
/// The metrics libvmaf computed for one frame.
/// </summary>
public sealed class VmafFrameMetrics
{
    internal VmafFrameMetrics(int frameNumber, IReadOnlyDictionary<string, double> metrics)
    {
        FrameNumber = frameNumber;
        Metrics = metrics;
    }

    /// <summary>Gets the zero-based number of the frame.</summary>
    public int FrameNumber { get; }

    /// <summary>Gets the metrics by name, e.g. <c>vmaf</c>, <c>integer_adm2</c>, <c>integer_motion</c>.</summary>
    public IReadOnlyDictionary<string, double> Metrics { get; }

    /// <summary>Gets the score of the default model (metric <c>vmaf</c>); <see cref="double.NaN"/> if there is none.</summary>
    public double Vmaf => Metrics.TryGetValue("vmaf", out double value) ? value : double.NaN;
}

/// <summary>
/// The per-frame values of one metric, pooled over all scored frames.
/// </summary>
public readonly struct VmafPooledMetric
{
    internal VmafPooledMetric(double min, double max, double mean, double harmonicMean)
    {
        Min = min;
        Max = max;
        Mean = mean;
        HarmonicMean = harmonicMean;
    }

    /// <summary>Gets the lowest value.</summary>
    public double Min { get; }

    /// <summary>Gets the highest value.</summary>
    public double Max { get; }

    /// <summary>Gets the arithmetic mean.</summary>
    public double Mean { get; }

    /// <summary>Gets the harmonic mean, which weighs low values more.</summary>
    public double HarmonicMean { get; }
}
