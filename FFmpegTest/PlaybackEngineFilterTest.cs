using FFmpeg;
using FFmpeg.Filters;
using FFmpeg.MediaPlayer;
using FFmpeg.Utils;

namespace FFmpegTest;

/// <summary>
/// Tests for the filters of <see cref="PlaybackEngine"/>: strings, factories and removal.
/// </summary>
[TestClass]
public class PlaybackEngineFilterTest
{
    private static string VideoFile => TestFiles.Get("mp4-example-video-download-full-hd-1920x1080.1min.mp4");

    [TestMethod]
    public void FilterFactories_AreCalledAgainAfterSeeking()
    {
        List<FilterGraphContext> videoCalls = [];
        List<FilterGraphContext> audioCalls = [];
        using PlaybackEngine player = PlaybackEngine.Open(VideoFile);
        player.SetVideoFilterFactory(ctx =>
        {
            videoCalls.Add(ctx);
            return $"scale={ctx.Width / 4}:-2";
        });
        player.SetAudioFilterFactory(ctx =>
        {
            audioCalls.Add(ctx);
            FilterGraph graph = FilterGraph.Create(out FilterInOutList? inputs, "aresample=24000", out FilterInOutList? outputs);
            return (inputs!, graph, outputs!);
        });

        Assert.AreEqual(480, player.Width);
        Assert.AreEqual(24000, player.SampleRate);
        Assert.AreEqual(1, videoCalls.Count);
        Assert.AreEqual(1920, videoCalls[0].Width);
        Assert.IsFalse(videoCalls[0].IsRebuild);
        Assert.IsNull(videoCalls[0].Encoder);
        Assert.AreEqual(-1, videoCalls[0].OutputStreamIndex);
        Assert.AreEqual(48000, audioCalls[0].SampleRate);

        Assert.IsTrue(player.Seek(TimeSpan.FromSeconds(20)));
        Assert.IsTrue(videoCalls.Count >= 2 && videoCalls[^1].IsRebuild, "The video factory must be called again after seeking.");
        Assert.IsTrue(audioCalls.Count >= 2 && audioCalls[^1].IsRebuild, "The audio factory must be called again after seeking.");
        Assert.AreEqual(480, player.Width);
    }

    [TestMethod]
    public void RemovingAFilter_RestoresTheStreamFormat()
    {
        using PlaybackEngine player = PlaybackEngine.Open(VideoFile);
        player.SetVideoFilter("scale=320:-2");
        player.SetAudioFilter("aresample=24000");
        Assert.AreEqual(320, player.Width);
        Assert.AreEqual(24000, player.SampleRate);

        player.SetVideoFilter((string?)null);
        player.SetAudioFilter((string?)null);
        Assert.AreEqual(1920, player.Width);
        Assert.AreEqual(48000, player.SampleRate);
    }

    [TestMethod]
    public void FilterGraphCopy_SkipsOptionsThatWereNotSet()
    {
        // aresample's unset channel layouts read back as "0 channels", which cannot be set again.
        using FilterGraph graph = FilterGraph.Create(out FilterInOutList? inputs, "aresample=24000", out FilterInOutList? outputs);
        inputs!.Dispose();
        outputs!.Dispose();
        using FilterGraph copy = graph.Copy();
        FilterContext resampler = copy.FindFilter("Parsed_aresample_0")!;
        Assert.IsFalse(resampler.IsSetToDefault("sample_rate"));
        Assert.IsFalse(resampler.TryGetOption("sample_rate", out long rate).IsError);
        Assert.AreEqual(24000L, rate);
        Assert.IsTrue(resampler.IsSetToDefault("ichl"));
    }
}
