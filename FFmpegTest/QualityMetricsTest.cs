using FFmpeg.Filters;
using FFmpeg.Filters.VideoFilters;
using FFmpeg.Images;
using FFmpeg.Logging;
using FFmpeg.Utils;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FFmpegTest;

/// <summary>
/// Tests for the quality metric filters ssim, psnr and libvmaf. The averages collected from the frame metadata are
/// compared with the summary FFmpeg itself logs when the filter is freed.
/// </summary>
[TestClass]
[DoNotParallelize] // captures the process-wide FFmpeg log
public class QualityMetricsTest
{
    private const int Width = 176;
    private const int Height = 144;
    private static readonly Rational TimeBase = new(1, 25);

    private readonly StringBuilder log = new();
    private LogLevel previousLevel;

    [TestInitialize]
    public void StartLogCapture()
    {
        previousLevel = Logger.Level;
        Logger.Level = LogLevel.Info;
        Logger.LogMessageReceived += OnLog;
    }

    [TestCleanup]
    public void StopLogCapture()
    {
        Logger.LogMessageReceived -= OnLog;
        Logger.Level = previousLevel;
    }

    private void OnLog(string message, LogLevel level, ClassCategory category, string? contextName)
    {
        lock (log)
            _ = log.Append(message);
    }

    private string Log
    {
        get
        {
            lock (log)
                return log.ToString();
        }
    }

    #region Helpers

    /// <summary>Creates a YUV420P frame: noise from <paramref name="seed"/> (or a flat luma), plus an offset.</summary>
    private static AVFrame Frame(long pts, byte luma = 0, int? seed = null, int offset = 0)
    {
        AVFrame frame = AVFrame.Allocate(PixelFormat.YUV420P, Width, Height);
        Span<byte> y = frame.GetData(0);
        if (seed is int s)
        {
            Random random = new(s);
            for (int i = 0; i < y.Length; i++)
                y[i] = (byte)Math.Clamp(random.Next(40, 216) + offset, 0, 255);
        }
        else
        {
            y.Fill((byte)Math.Clamp(luma + offset, 0, 255));
        }
        frame.GetData(1).Fill(128);
        frame.GetData(2).Fill(128);
        frame.PresentationTimestamp = pts;
        frame.Duration = 1;
        frame.TimeBase = TimeBase;
        return frame;
    }

    /// <summary>
    /// Runs main/reference frame pairs through <paramref name="metric"/> (pad 0 main, pad 1 reference) and returns
    /// the output frames. The graph is disposed afterwards, which makes the filter log its summary.
    /// </summary>
    private static List<AVFrame> Compare(Func<FilterGraph, FilterContext> metric, IEnumerable<(AVFrame Main, AVFrame Reference)> pairs)
    {
        List<AVFrame> output = [];
        using (FilterGraph graph = FilterGraph.Allocate())
        {
            VideoBufferSource main = VideoBufferSource.Create("main", Width, Height, PixelFormat.YUV420P, TimeBase, graph);
            VideoBufferSource reference = VideoBufferSource.Create("reference", Width, Height, PixelFormat.YUV420P, TimeBase, graph);
            FilterContext filter = metric(graph);
            VideoBufferSink sink = VideoBufferSink.Create("out", graph);
            graph.Link(main, 0, filter, 0).ThrowIfError();
            graph.Link(reference, 0, filter, 1).ThrowIfError();
            graph.Link(filter, sink).ThrowIfError();
            graph.Config().ThrowIfError();

            void Drain()
            {
                while (true)
                {
                    AVFrame frame = AVFrame.Allocate();
                    if (sink.ReceiveFrame(frame).IsError)
                    {
                        frame.Dispose();
                        return;
                    }
                    output.Add(frame);
                }
            }

            foreach ((AVFrame m, AVFrame r) in pairs)
            {
                using (m)
                    main.SendFrame(m).ThrowIfError();
                using (r)
                    reference.SendFrame(r).ThrowIfError();
                Drain();
            }
            main.SendFrame(null).ThrowIfError();
            reference.SendFrame(null).ThrowIfError();
            Drain();
        }
        return output;
    }

    private static double Number(string text) => text switch
    {
        "inf" => double.PositiveInfinity,
        _ => double.Parse(text, CultureInfo.InvariantCulture),
    };

    private static void AreClose(double expected, double actual, double tolerance, string what)
    {
        if (double.IsInfinity(expected) || double.IsInfinity(actual))
            Assert.AreEqual(expected, actual, what);
        else
            Assert.IsTrue(Math.Abs(expected - actual) <= tolerance, $"{what}: expected {expected}, got {actual}");
    }

    #endregion

    [TestMethod]
    public void Psnr_MatchesTheFormulaAndTheFFmpegLog()
    {
        // Main luma 100, reference 110 and then 120: MSE(y) = 100 and 400, chroma identical.
        List<AVFrame> output = Compare(g => Psnr.Create("psnr", g),
            [(Frame(0, 100), Frame(0, 110)), (Frame(1, 100), Frame(1, 120))]);
        Assert.AreEqual(2, output.Count);

        Assert.IsTrue(PsnrScore.TryRead(output[0], out PsnrScore? first));
        AreClose(100, first.Mse['y'], 1e-3, "mse y");
        AreClose(0, first.Mse['u'], 1e-9, "mse u");
        AreClose(10 * Math.Log10(255.0 * 255 / 100), first.Psnr['y'], 1e-4, "psnr y");
        Assert.AreEqual(double.PositiveInfinity, first.Psnr['u'], "Identical planes have an infinite PSNR ('inf' in the metadata).");
        AreClose(100 * 2.0 / 3, first.MseAverage, 1e-3, "mse_avg (Y weighs 4/6 in 4:2:0)");

        PsnrStatistics statistics = new();
        output.ForEach(f => Assert.IsTrue(statistics.Add(f)));
        AreClose(10 * Math.Log10(255.0 * 255 / ((100 + 400) * 2.0 / 3 / 2)), statistics.Average, 1e-4, "average from the mean MSE");
        AreClose(10 * Math.Log10(255.0 * 255 / (400 * 2.0 / 3)), statistics.Min, 1e-4, "min");
        AreClose(10 * Math.Log10(255.0 * 255 / (100 * 2.0 / 3)), statistics.Max, 1e-4, "max");
        AreClose(10 * Math.Log10(255.0 * 255 / 250), statistics.ComponentAverages['y'], 1e-4, "y average");
        Assert.AreEqual(double.PositiveInfinity, statistics.ComponentAverages['u']);

        Match logged = Regex.Match(Log, @"PSNR y:(\S+) u:(\S+) v:(\S+) average:(\S+) min:(\S+) max:(\S+)");
        Assert.IsTrue(logged.Success, "FFmpeg logs the PSNR summary when the filter is freed:\n" + Log);
        AreClose(Number(logged.Groups[1].Value), statistics.ComponentAverages['y'], 1e-4, "y vs log");
        AreClose(Number(logged.Groups[4].Value), statistics.Average, 1e-4, "average vs log");
        AreClose(Number(logged.Groups[5].Value), statistics.Min, 1e-4, "min vs log");
        AreClose(Number(logged.Groups[6].Value), statistics.Max, 1e-4, "max vs log");
        output.ForEach(f => f.Dispose());
    }

    [TestMethod]
    public void Psnr_OfNoise_MatchesTheFFmpegLog()
    {
        List<AVFrame> output = Compare(g => Psnr.Create("psnr", g),
            Enumerable.Range(0, 5).Select(i => (Frame(i, seed: i, offset: i * 3), Frame(i, seed: i))));
        PsnrStatistics statistics = new();
        output.ForEach(f => statistics.Add(f));
        Assert.AreEqual(5, statistics.Count);

        Match logged = Regex.Match(Log, @"PSNR y:(\S+) u:\S+ v:\S+ average:(\S+) min:(\S+) max:(\S+)");
        Assert.IsTrue(logged.Success, Log);
        AreClose(Number(logged.Groups[1].Value), statistics.ComponentAverages['y'], 1e-4, "y vs log");
        AreClose(Number(logged.Groups[2].Value), statistics.Average, 1e-4, "average vs log");
        AreClose(Number(logged.Groups[3].Value), statistics.Min, 1e-4, "min vs log");
        AreClose(Number(logged.Groups[4].Value), statistics.Max, 1e-4, "max vs log");
        output.ForEach(f => f.Dispose());
    }

    [TestMethod]
    public void Ssim_OfIdenticalFrames_IsOne()
    {
        List<AVFrame> output = Compare(g => Ssim.Create("ssim", g),
            Enumerable.Range(0, 3).Select(i => (Frame(i, seed: i), Frame(i, seed: i))));
        Assert.AreEqual(3, output.Count);
        Assert.IsTrue(SsimScore.TryRead(output[0], out SsimScore? score));
        AreClose(1, score.All, 1e-6, "All");
        Assert.AreEqual(double.PositiveInfinity, score.Decibels, "Identical frames are 'inf' dB.");
        CollectionAssert.AreEqual(new[] { 'U', 'V', 'Y' }, score.Components.Keys.OrderBy(c => c).ToArray());
        Assert.IsFalse(PsnrScore.TryRead(output[0], out _), "No psnr metadata on an ssim frame.");
        output.ForEach(f => f.Dispose());
    }

    [TestMethod]
    public void Ssim_OfNoise_MatchesTheFFmpegLog()
    {
        List<AVFrame> output = Compare(g => Ssim.Create("ssim", g),
            Enumerable.Range(0, 5).Select(i => (Frame(i, seed: i, offset: i * 4), Frame(i, seed: i))));
        SsimStatistics statistics = new();
        output.ForEach(f => statistics.Add(f));
        Assert.AreEqual(5, statistics.Count);
        Assert.IsTrue(statistics.Average < 1 && statistics.Min < statistics.Max);

        Match logged = Regex.Match(Log, @"SSIM Y:(\S+) \(\S+\) U:\S+ \(\S+\) V:\S+ \(\S+\) All:(\S+) \((\S+)\)");
        Assert.IsTrue(logged.Success, "FFmpeg logs the SSIM summary when the filter is freed:\n" + Log);
        AreClose(Number(logged.Groups[1].Value), statistics.ComponentAverages['Y'], 1e-5, "Y vs log");
        AreClose(Number(logged.Groups[2].Value), statistics.Average, 1e-5, "All vs log");
        AreClose(Number(logged.Groups[3].Value), statistics.AverageDecibels, 1e-3, "dB vs log");
        output.ForEach(f => f.Dispose());
    }

    [TestMethod]
    public void Options_CanBeSetAndReadBack()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        Ssim ssim = Ssim.Allocate("ssim", graph);
        ssim.StatsFile = "ssim.log"; ssim.Shortest = true; ssim.RepeatLast = false;
        Assert.AreEqual("ssim.log", ssim.StatsFile);
        Assert.IsTrue(ssim.Shortest);
        Assert.IsFalse(ssim.RepeatLast);

        Psnr psnr = Psnr.Allocate("psnr", graph);
        psnr.StatsFile = "psnr.log"; psnr.StatsVersion = 2; psnr.OutputMax = true; psnr.Shortest = true; psnr.RepeatLast = false;
        Assert.AreEqual(2, psnr.StatsVersion);
        Assert.IsTrue(psnr.OutputMax);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => psnr.StatsVersion = 3);

        if (!LibVmaf.IsAvailable)
        {
            _ = Assert.Throws<ArgumentException>(() => LibVmaf.Allocate("vmaf", graph));
            return;
        }
        LibVmaf vmaf = LibVmaf.Allocate("vmaf", graph);
        vmaf.LogPath = "vmaf.json"; vmaf.LogFormat = VmafLogFormat.Json; vmaf.Pool = VmafPoolMethod.HarmonicMean;
        vmaf.VmafThreads = 2; vmaf.Subsample = 3; vmaf.Model = "version=vmaf_v0.6.1neg"; vmaf.Feature = "name=psnr";
        vmaf.Shortest = true;
        Assert.AreEqual("vmaf.json", vmaf.LogPath);
        Assert.AreEqual(VmafLogFormat.Json, vmaf.LogFormat);
        Assert.AreEqual(VmafPoolMethod.HarmonicMean, vmaf.Pool);
        Assert.AreEqual(2, vmaf.VmafThreads);
        Assert.AreEqual(3, vmaf.Subsample);
        Assert.AreEqual("name=psnr", vmaf.Feature);
    }

    [TestMethod]
    public void Psnr_WritesTheStatsFile()
    {
        string file = Path.Combine(Path.GetTempPath(), $"psnr-{Guid.NewGuid():N}.log");
        try
        {
            List<AVFrame> output = Compare(g => Psnr.Create("psnr", file, g),
                Enumerable.Range(0, 3).Select(i => (Frame(i, 100), Frame(i, 110))));
            output.ForEach(f => f.Dispose());
            string[] lines = File.ReadAllLines(file);
            Assert.AreEqual(3, lines.Length);
            StringAssert.StartsWith(lines[0], "n:1 mse_avg:66.67");
        }
        finally
        {
            File.Delete(file);
        }
    }

    [TestMethod]
    public void LibVmaf_ScoresAndWritesTheLog()
    {
        if (!LibVmaf.IsAvailable)
            Assert.Inconclusive("FFmpeg was built without libvmaf.");

        string identicalLog = Path.Combine(Path.GetTempPath(), $"vmaf-{Guid.NewGuid():N}.xml");
        string degradedLog = Path.Combine(Path.GetTempPath(), $"vmaf-{Guid.NewGuid():N}.xml");
        try
        {
            Compare(g => LibVmaf.Create("vmaf", identicalLog, g),
                Enumerable.Range(0, 10).Select(i => (Frame(i, seed: i), Frame(i, seed: i)))).ForEach(f => f.Dispose());
            Match loggedIdentical = Regex.Match(Log, @"VMAF score: (\S+)");
            Assert.IsTrue(loggedIdentical.Success, Log);

            VmafLog identical = VmafLog.Load(identicalLog);
            Assert.AreEqual(10, identical.Frames.Count);
            Assert.AreEqual(9, identical.Frames[^1].FrameNumber);
            Assert.IsTrue(identical.Frames.All(f => !double.IsNaN(f.Vmaf)));
            Assert.IsTrue(identical.Frames[0].Metrics.ContainsKey("integer_adm2"), string.Join(",", identical.Frames[0].Metrics.Keys));
            AreClose(Number(loggedIdentical.Groups[1].Value), identical.Score, 1e-4, "VMAF vs log");
            AreClose(identical.Frames.Average(f => f.Vmaf), identical.Score, 1e-4, "pooled mean");
            Assert.IsNotNull(identical.Version);

            // Every main frame has an offset and more noise: the score must drop.
            Compare(g => LibVmaf.Create("vmaf", degradedLog, g),
                Enumerable.Range(0, 10).Select(i => (Frame(i, seed: i + 100, offset: 10), Frame(i, seed: i)))).ForEach(f => f.Dispose());
            VmafLog degraded = VmafLog.Load(degradedLog);
            Assert.IsTrue(degraded.Score < identical.Score - 10, $"identical {identical.Score}, degraded {degraded.Score}");
            VmafPooledMetric pooled = degraded.PooledMetrics["vmaf"];
            Assert.IsTrue(pooled.Min <= pooled.HarmonicMean && pooled.HarmonicMean <= pooled.Mean && pooled.Mean <= pooled.Max);
        }
        finally
        {
            File.Delete(identicalLog);
            File.Delete(degradedLog);
        }
    }

    [TestMethod]
    public void VmafLog_RejectsOtherXml()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("<root/>"));
        _ = Assert.Throws<FormatException>(() => VmafLog.Load(stream));
    }
}
