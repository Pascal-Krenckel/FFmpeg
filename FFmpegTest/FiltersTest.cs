using FFmpeg.Audio;
using FFmpeg.Filters;
using FFmpeg.Filters.AudioFilters;
using FFmpeg.Filters.VideoFilters;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpegTest;

/// <summary>
/// Tests for the typed filter classes: every option can be set (so the option names exist in FFmpeg) and the
/// filters do what they should when frames run through them.
/// </summary>
[TestClass]
public class FiltersTest
{
    private static readonly Rational VideoTimeBase = new(1, 25);

    #region Helpers

    /// <summary>Links the filters in order and configures the graph.</summary>
    private static void LinkChain(FilterGraph graph, params FilterContext[] filters)
    {
        for (int i = 0; i + 1 < filters.Length; i++)
            graph.Link(filters[i], filters[i + 1]).ThrowIfError();
        graph.Config().ThrowIfError();
    }

    /// <summary>Creates a YUV420P frame filled with one luma value.</summary>
    private static AVFrame GrayFrame(int width, int height, byte luma, long pts)
    {
        AVFrame frame = AVFrame.Allocate(PixelFormat.YUV420P, width, height);
        frame.GetData(0).Fill(luma);
        frame.GetData(1).Fill(128);
        frame.GetData(2).Fill(128);
        frame.PresentationTimestamp = pts;
        frame.Duration = 1;
        frame.TimeBase = VideoTimeBase;
        return frame;
    }

    /// <summary>Creates a mono float frame with a constant sample value.</summary>
    private static AVFrame ConstantAudio(float value, int samples, int sampleRate, long pts)
    {
        using ChannelLayout mono = ChannelLayout.CreateMono();
        AVFrame frame = AVFrame.Allocate(SampleFormat.Float32, mono, samples, sampleRate);
        MemoryMarshal.Cast<byte, float>(frame.GetData(0))[..samples].Fill(value);
        frame.PresentationTimestamp = pts;
        frame.Duration = samples;
        frame.TimeBase = new Rational(1, sampleRate);
        return frame;
    }

    /// <summary>
    /// Sends the frames, flushes the source and returns everything the sink outputs. Stops sending when
    /// the graph answers AVERROR_EOF (e.g. a trim has reached its end).
    /// </summary>
    private static List<AVFrame> Run(IBufferSource source, IBufferSink sink, IEnumerable<AVFrame> frames)
    {
        List<AVFrame> output = [];
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
        bool ended = false;
        foreach (AVFrame frame in frames)
        {
            using (frame)
            {
                if (ended)
                    continue;
                AVResult32 res = source.SendFrame(frame);
                ended = res == AVResult32.EndOfFile;
                if (!ended)
                    res.ThrowIfError();
            }
            Drain();
        }
        if (!ended)
        {
            AVResult32 res = source.SendFrame(null);
            if (res != AVResult32.EndOfFile)
                res.ThrowIfError();
        }
        Drain();
        return output;
    }

    private static VideoBufferSource VideoSource(FilterGraph graph, string name = "in", int width = 320, int height = 240)
        => VideoBufferSource.Create(name, width, height, PixelFormat.YUV420P, VideoTimeBase, graph);

    private static AudioBufferSource AudioSource(FilterGraph graph, int sampleRate = 48000, string name = "in")
    {
        using ChannelLayout mono = ChannelLayout.CreateMono();
        return AudioBufferSource.Create(name, sampleRate, SampleFormat.Float32, mono, graph);
    }

    private static void DisposeAll(List<AVFrame> frames) => frames.ForEach(f => f.Dispose());

    #endregion

    [TestMethod]
    public void AllOptions_CanBeSetAndReadBack()
    {
        using FilterGraph graph = FilterGraph.Allocate();

        Scale scale = Scale.Allocate("scale", graph);
        scale.Width = "iw/2";
        scale.Height = "-2";
        scale.Flags = "lanczos";
        scale.Interlaced = true;
        scale.AspectRatioMode = ScaleAspectRatioMode.Decrease;
        scale.DivisibleBy = 4;
        scale.ResetSar = true;
        scale.Evaluation = ExpressionEvaluation.PerFrame;
        Assert.AreEqual("iw/2", scale.Width);
        Assert.AreEqual("lanczos", scale.Flags);
        Assert.AreEqual(ScaleAspectRatioMode.Decrease, scale.AspectRatioMode);
        Assert.AreEqual(4, scale.DivisibleBy);
        Assert.IsTrue(scale.ResetSar);
        Assert.AreEqual(ExpressionEvaluation.PerFrame, scale.Evaluation);

        Crop crop = Crop.Allocate("crop", graph);
        crop.Width = "100"; crop.Height = "50"; crop.X = "10"; crop.Y = "20"; crop.KeepAspect = true; crop.Exact = true;
        Assert.AreEqual("20", crop.Y);
        Assert.IsTrue(crop.Exact);

        Pad pad = Pad.Allocate("pad", graph);
        pad.Width = "ow"; pad.Height = "ih+20"; pad.X = "0"; pad.Y = "10"; pad.Color = "red"; pad.Aspect = new Rational(16, 9); pad.Evaluation = ExpressionEvaluation.PerFrame;
        Assert.AreEqual("0xff0000ff", pad.Color);
        Assert.AreEqual(new Rational(16, 9), pad.Aspect);

        Fps fps = Fps.Allocate("fps", graph);
        fps.FrameRate = "ntsc"; fps.Rounding = FpsRounding.Up; fps.EofAction = FpsEofAction.Pass; fps.StartTime = 1.5;
        Assert.AreEqual(FpsRounding.Up, fps.Rounding);
        Assert.AreEqual(FpsEofAction.Pass, fps.EofAction);
        Assert.AreEqual(1.5, fps.StartTime);

        Transpose transpose = Transpose.Allocate("transpose", graph);
        transpose.Direction = TransposeDirection.ClockwiseFlip; transpose.Passthrough = TransposePassthrough.Portrait;
        Assert.AreEqual(TransposeDirection.ClockwiseFlip, transpose.Direction);
        Assert.AreEqual(TransposePassthrough.Portrait, transpose.Passthrough);

        SetSar setSar = SetSar.Allocate("setsar", graph);
        setSar.Ratio = "4/3"; setSar.Max = 1000;
        Assert.AreEqual(1000, setSar.Max);
        SetDar setDar = SetDar.Allocate("setdar", graph);
        setDar.Ratio = "16/9"; setDar.Max = 1000;
        Assert.AreEqual("16/9", setDar.Ratio);

        SetPts setPts = SetPts.Allocate("setpts", graph);
        setPts.Expression = "PTS-STARTPTS"; setPts.StripFps = true;
        Assert.AreEqual("PTS-STARTPTS", setPts.Expression);

        Trim trim = Trim.Allocate("trim", graph);
        trim.Start = TimeSpan.FromSeconds(1); trim.End = TimeSpan.FromSeconds(2); trim.StartPts = 10; trim.EndPts = 20;
        trim.Duration = TimeSpan.FromMilliseconds(500); trim.StartFrame = 3; trim.EndFrame = 9;
        Assert.AreEqual(TimeSpan.FromSeconds(2), trim.End);
        Assert.AreEqual(9, trim.EndFrame);

        Yadif yadif = Yadif.Allocate("yadif", graph);
        yadif.Mode = YadifMode.SendField; yadif.Parity = YadifParity.BottomFieldFirst; yadif.Deinterlace = YadifDeinterlace.Interlaced;
        Assert.AreEqual(YadifParity.BottomFieldFirst, yadif.Parity);

        Fade fade = Fade.Allocate("fade", graph);
        fade.Type = FadeType.Out; fade.StartFrame = 5; fade.FrameCount = 10; fade.StartTime = TimeSpan.FromSeconds(3);
        fade.Duration = TimeSpan.FromSeconds(1); fade.Alpha = true; fade.Color = "white";
        Assert.AreEqual(FadeType.Out, fade.Type);
        Assert.AreEqual(TimeSpan.FromSeconds(3), fade.StartTime);

        Overlay overlay = Overlay.Allocate("overlay", graph);
        overlay.X = "W-w"; overlay.Y = "H-h"; overlay.EofAction = OverlayEofAction.Pass; overlay.Evaluation = ExpressionEvaluation.Once;
        overlay.Shortest = true; overlay.Format = OverlayFormat.Yuv444; overlay.RepeatLast = false; overlay.AlphaMode = AlphaMode.Premultiplied;
        Assert.AreEqual(OverlayFormat.Yuv444, overlay.Format);
        Assert.AreEqual(AlphaMode.Premultiplied, overlay.AlphaMode);

        Volume volume = Volume.Allocate("volume", graph);
        volume.Expression = "-6dB"; volume.Precision = VolumePrecision.Double; volume.Evaluation = ExpressionEvaluation.PerFrame;
        volume.ReplayGain = ReplayGainMode.Track; volume.ReplayGainPreamp = 2; volume.ReplayGainNoClip = false;
        Assert.AreEqual(ReplayGainMode.Track, volume.ReplayGain);
        Assert.AreEqual(VolumePrecision.Double, volume.Precision);

        AResample resample = AResample.Allocate("aresample", graph);
        resample.SampleRate = 44100; resample.Async = 1000;
        Assert.AreEqual(1000, resample.Async);

        ATempo tempo = ATempo.Allocate("atempo", graph);
        tempo.Tempo = 1.5;
        Assert.AreEqual(1.5, tempo.Tempo);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => tempo.Tempo = 0.1, "Values outside 0.5-100 must be rejected.");

        AMix mix = AMix.Allocate("amix", graph);
        mix.Inputs = 3; mix.Duration = AMixDuration.First; mix.DropoutTransition = 0.5; mix.Weights = "1 2 3"; mix.Normalize = false;
        Assert.AreEqual(3, mix.Inputs);
        Assert.AreEqual(0.5, mix.DropoutTransition, 1e-6);

        ASetPts aSetPts = ASetPts.Allocate("asetpts", graph);
        aSetPts.Expression = "N/SR/TB";
        Assert.AreEqual("N/SR/TB", aSetPts.Expression);

        ATrim aTrim = ATrim.Allocate("atrim", graph);
        aTrim.Start = TimeSpan.FromSeconds(1); aTrim.End = TimeSpan.FromSeconds(2); aTrim.StartPts = 1; aTrim.EndPts = 2;
        aTrim.Duration = TimeSpan.FromSeconds(1); aTrim.StartSample = 100; aTrim.EndSample = 200;
        Assert.AreEqual(200, aTrim.EndSample);

        AFade aFade = AFade.Allocate("afade", graph);
        aFade.Type = FadeType.Out; aFade.StartSample = 10; aFade.SampleCount = 100; aFade.StartTime = TimeSpan.FromSeconds(1);
        aFade.Duration = TimeSpan.FromSeconds(2); aFade.Curve = AudioFadeCurve.None; aFade.Silence = 0.1; aFade.Unity = 0.9;
        Assert.AreEqual(AudioFadeCurve.None, aFade.Curve);
        Assert.AreEqual(100, aFade.SampleCount);

        LoudNorm loudNorm = LoudNorm.Allocate("loudnorm", graph);
        loudNorm.IntegratedLoudness = -16; loudNorm.LoudnessRange = 11; loudNorm.TruePeak = -1.5;
        loudNorm.MeasuredIntegratedLoudness = -20; loudNorm.MeasuredLoudnessRange = 5; loudNorm.MeasuredTruePeak = -3;
        loudNorm.MeasuredThreshold = -30; loudNorm.Offset = 0.5; loudNorm.Linear = false; loudNorm.DualMono = true;
        Assert.AreEqual(-16, loudNorm.IntegratedLoudness);
        Assert.IsTrue(loudNorm.DualMono);

        foreach (FilterContext filter in new FilterContext[] { scale, crop, pad, fps, transpose, setSar, setDar, setPts, trim, yadif, fade, overlay, volume, resample, tempo, mix, aSetPts, aTrim, aFade, loudNorm })
            filter.Init().ThrowIfError();
    }

    [TestMethod]
    public void VideoChain_ChangesTheGeometry()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource source = VideoSource(graph);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        LinkChain(graph,
            source,
            Crop.Create("crop", 160, 120, 0, 0, graph),         // 160x120
            Scale.Create("scale", 80, -2, graph),               // 80x60
            Pad.Create("pad", 100, 60, graph),                  // 100x60
            Transpose.Create("transpose", TransposeDirection.Clockwise, graph), // 60x100
            HFlip.Create("hflip", graph),
            VFlip.Create("vflip", graph),
            SetSar.Create("setsar", new Rational(2, 1), graph),
            sink);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 3).Select(i => GrayFrame(320, 240, 100, i)));
        Assert.AreEqual(3, output.Count);
        Assert.AreEqual(60, output[0].Width);
        Assert.AreEqual(100, output[0].Height);
        Assert.AreEqual(new Rational(2, 1), sink.PixelAspectRatio);
        DisposeAll(output);
    }

    [TestMethod]
    public void Scale_KeepsTheAspectRatio()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource source = VideoSource(graph, width: 1920, height: 1080);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        LinkChain(graph, source, Scale.Create("scale", 500, 500, ScaleAspectRatioMode.Decrease, 2, graph), sink);
        Assert.AreEqual(500, sink.Width);
        Assert.AreEqual(282, sink.Height); // 500 * 1080 / 1920 = 281.25, made divisible by 2
    }

    [TestMethod]
    public void Fps_DoublesTheFrameRate()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource source = VideoSource(graph);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        LinkChain(graph, source, Fps.Create("fps", new Rational(50, 1), graph), sink);
        Assert.AreEqual(new Rational(50, 1), sink.FrameRate);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 10).Select(i => GrayFrame(320, 240, 100, i)));
        Assert.AreEqual(20, output.Count);
        DisposeAll(output);
    }

    [TestMethod]
    public void TrimAndSetPts_KeepARangeStartingAtZero()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource source = VideoSource(graph);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        LinkChain(graph, source,
            Trim.Create("trim", TimeSpan.FromSeconds(0.4), TimeSpan.FromSeconds(0.8), graph), // frames 10..19
            SetPts.Create("setpts", "PTS-STARTPTS", graph),
            sink);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 25).Select(i => GrayFrame(320, 240, 100, i)));
        Assert.AreEqual(10, output.Count);
        Assert.AreEqual(0, output[0].PresentationTimestamp);
        Assert.AreEqual(9, output[^1].PresentationTimestamp);
        DisposeAll(output);
    }

    [TestMethod]
    public void Fade_StartsFromBlack()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource source = VideoSource(graph);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        LinkChain(graph, source, Fade.Create("fade", FadeType.In, TimeSpan.Zero, TimeSpan.FromSeconds(1), graph), sink);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 30).Select(i => GrayFrame(64, 48, 200, i)));
        Assert.AreEqual(30, output.Count);
        Assert.IsTrue(output[0].GetData(0)[0] <= 20, "The first frame must be (nearly) black.");
        Assert.AreEqual(200, output[^1].GetData(0)[0], "After the fade the input must be unchanged.");
        DisposeAll(output);
    }

    [TestMethod]
    public void Yadif_OutputsOneFramePerFrame()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource source = VideoSource(graph);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        LinkChain(graph, source, Yadif.Create("yadif", graph), sink);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 5).Select(i => GrayFrame(320, 240, 100, i)));
        Assert.AreEqual(5, output.Count);
        DisposeAll(output);
    }

    [TestMethod]
    public void Overlay_DrawsTheSecondInputOnTop()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource main = VideoSource(graph, "main", 320, 240);
        VideoBufferSource logo = VideoSource(graph, "logo", 64, 64);
        Overlay overlay = Overlay.Create("overlay", 10, 20, graph);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        graph.Link(main, 0, overlay, 0).ThrowIfError();
        graph.Link(logo, 0, overlay, 1).ThrowIfError();
        graph.Link(overlay, sink).ThrowIfError();
        graph.Config().ThrowIfError();

        using (AVFrame logoFrame = GrayFrame(64, 64, 235, 0))
            logo.SendFrame(logoFrame).ThrowIfError();
        logo.SendFrame(null).ThrowIfError();
        using (AVFrame mainFrame = GrayFrame(320, 240, 16, 0))
            main.SendFrame(mainFrame).ThrowIfError();
        main.SendFrame(null).ThrowIfError();

        using AVFrame output = AVFrame.Allocate();
        sink.ReceiveFrame(output).ThrowIfError();
        Assert.AreEqual(320, output.Width);
        Span<byte> luma = output.GetData(0);
        int stride = output.LineSize[0];
        Assert.AreEqual(235, luma[(30 * stride) + 30], "Inside the overlay.");
        Assert.AreEqual(16, luma[(200 * stride) + 200], "Outside the overlay.");
    }

    private static float[] Samples(List<AVFrame> frames)
        => [.. frames.SelectMany(f => MemoryMarshal.Cast<byte, float>(f.GetData(0))[..f.SampleCount].ToArray())];

    [TestMethod]
    public void Volume_ScalesTheSamples()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        AudioBufferSource source = AudioSource(graph);
        AudioBufferSink sink = AudioBufferSink.Create("out", graph, [SampleFormat.Float32], [], []);
        LinkChain(graph, source, Volume.Create("volume", 0.5, graph), sink);

        List<AVFrame> output = Run(source, sink, [ConstantAudio(0.8f, 1024, 48000, 0)]);
        float[] samples = Samples(output);
        Assert.AreEqual(1024, samples.Length);
        Assert.AreEqual(0.4f, samples[512], 1e-6f);
        DisposeAll(output);
    }

    [TestMethod]
    public void AResample_ChangesTheSampleRate()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        AudioBufferSource source = AudioSource(graph);
        AudioBufferSink sink = AudioBufferSink.Create("out", graph);
        LinkChain(graph, source, AResample.Create("aresample", 24000, graph), sink);
        Assert.AreEqual(24000, sink.SampleRate);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 10).Select(i => ConstantAudio(0.5f, 4800, 48000, i * 4800L)));
        int count = output.Sum(f => f.SampleCount);
        Assert.IsTrue(Math.Abs(count - 24000) <= 64, $"1 s at 24 kHz, got {count} samples.");
        DisposeAll(output);
    }

    [TestMethod]
    public void ATempo_HalvesTheDuration()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        AudioBufferSource source = AudioSource(graph);
        AudioBufferSink sink = AudioBufferSink.Create("out", graph);
        LinkChain(graph, source, ATempo.Create("atempo", 2.0, graph), sink);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 10).Select(i => ConstantAudio(0.5f, 4800, 48000, i * 4800L)));
        int count = output.Sum(f => f.SampleCount);
        Assert.IsTrue(Math.Abs(count - 24000) <= 2400, $"1 s at double tempo is ~0.5 s, got {count} samples.");
        DisposeAll(output);
    }

    [TestMethod]
    public void ATrimAndAFade_CutAndFade()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        AudioBufferSource source = AudioSource(graph);
        AudioBufferSink sink = AudioBufferSink.Create("out", graph, [SampleFormat.Float32], [], []);
        LinkChain(graph, source,
            ATrim.Create("atrim", TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.75), graph),
            ASetPts.Create("asetpts", "PTS-STARTPTS", graph),
            AFade.Create("afade", FadeType.In, TimeSpan.Zero, TimeSpan.FromSeconds(0.5), AudioFadeCurve.Triangular, graph),
            sink);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 10).Select(i => ConstantAudio(1f, 4800, 48000, i * 4800L)));
        float[] samples = Samples(output);
        Assert.AreEqual(24000, samples.Length, "atrim cuts sample-exactly.");
        Assert.AreEqual(0, output[0].PresentationTimestamp);
        Assert.AreEqual(0f, samples[0], 1e-3f);
        Assert.AreEqual(0.5f, samples[12000], 1e-2f, "A linear fade is halfway in the middle.");
        DisposeAll(output);
    }

    [TestMethod]
    public void AMix_AddsTheInputs()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        AudioBufferSource first = AudioSource(graph, name: "a");
        AudioBufferSource second = AudioSource(graph, name: "b");
        AMix mix = AMix.Allocate("amix", graph);
        mix.Inputs = 2;
        mix.Normalize = false;
        mix.Init().ThrowIfError();
        AudioBufferSink sink = AudioBufferSink.Create("out", graph, [SampleFormat.Float32], [], []);
        graph.Link(first, 0, mix, 0).ThrowIfError();
        graph.Link(second, 0, mix, 1).ThrowIfError();
        graph.Link(mix, sink).ThrowIfError();
        graph.Config().ThrowIfError();

        using (AVFrame a = ConstantAudio(0.25f, 1024, 48000, 0))
            first.SendFrame(a).ThrowIfError();
        using (AVFrame b = ConstantAudio(0.5f, 1024, 48000, 0))
            second.SendFrame(b).ThrowIfError();
        first.SendFrame(null).ThrowIfError();
        second.SendFrame(null).ThrowIfError();

        using AVFrame output = AVFrame.Allocate();
        sink.ReceiveFrame(output).ThrowIfError();
        Assert.AreEqual(0.75f, MemoryMarshal.Cast<byte, float>(output.GetData(0))[10], 1e-6f);
    }

    [TestMethod]
    public void LoudNorm_Runs()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        AudioBufferSource source = AudioSource(graph);
        AudioBufferSink sink = AudioBufferSink.Create("out", graph);
        LinkChain(graph, source, LoudNorm.Create("loudnorm", -16, 11, -1.5, graph), AResample.Create("aresample", 48000, graph), sink);
        Assert.AreEqual(48000, sink.SampleRate);

        List<AVFrame> output = Run(source, sink, Enumerable.Range(0, 20).Select(i => ConstantAudio(0.1f, 4800, 48000, i * 4800L)));
        Assert.IsTrue(output.Sum(f => f.SampleCount) > 0);
        DisposeAll(output);
    }

    [TestMethod]
    public void TypedFilters_SurviveFilterGraphCopy()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        VideoBufferSource source = VideoSource(graph);
        VideoBufferSink sink = VideoBufferSink.Create("out", graph);
        LinkChain(graph, source, Scale.Create("scale", 160, 90, graph), Pad.Create("pad", 200, 100, graph), sink);

        using FilterGraph copy = graph.Copy();
        copy.Config().ThrowIfError();
        VideoBufferSink copiedSink = copy.FindFilter("out")!.As<VideoBufferSink>();
        Assert.AreEqual(200, copiedSink.Width);
        Assert.AreEqual(100, copiedSink.Height);
        Assert.AreEqual("160", copy.FindFilter("scale")!.As<Scale>().Width);
    }

    [TestMethod]
    public void As_RejectsTheWrongFilterType()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        FilterContext scale = Scale.Create("scale", 10, 10, graph);
        _ = Assert.Throws<InvalidCastException>(() => scale.As<Crop>());
        Assert.AreEqual("scale", scale.As<Scale>().Name);
    }

    [TestMethod]
    public void FailedCreate_LeavesNoFilterBehind()
    {
        using FilterGraph graph = FilterGraph.Allocate();
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => ATempo.Create("atempo", 0.1, graph));
        Assert.AreEqual(0, graph.Count);
    }

    [TestMethod]
    public void FilterGraphCopy_KeepsLinksOfMultiPadFilters()
    {
        using FilterGraph graph = FilterGraph.Create(out FilterInOutList? inputs, "split=2[a][b];[b]hflip[c];[a][c]hstack", out FilterInOutList? outputs);
        inputs!.Dispose();
        outputs!.Dispose();
        FilterContext split = graph.Filters.First(f => f.Filter.Name == "split");
        CollectionAssert.AreEqual(new[] { 0, 1 }, split.OutputFilterLinks.Select(l => l.SourcePadIndex).ToArray());
        using FilterGraph copy = graph.Copy();
        FilterContext stack = copy.Filters.First(f => f.Filter.Name == "hstack");
        CollectionAssert.AreEqual(new[] { 0, 1 }, stack.InputFilterLinks.Select(l => l.DestinationPadIndex).ToArray());
    }
}
