using FFmpeg;
using FFmpeg.Audio;
using FFmpeg.Codecs;
using FFmpeg.Utils;

namespace FFmpegTest;

/// <summary>
/// End-to-end tests for <see cref="Transcoder"/>: copying, re-encoding, filtering and trimming.
/// </summary>
[TestClass]
public class TranscoderTest
{
    private static string VideoFile => TestFiles.Get("mp4-example-video-download-full-hd-1920x1080.1min.mp4");
    private const int VideoIndex = 0;
    private const int AudioIndex = 1;

    private string? output;

    [TestCleanup]
    public void DeleteOutput()
    {
        if (output != null && File.Exists(output))
            File.Delete(output);
    }

    private string NewOutput(string extension) => output = Path.Combine(Path.GetTempPath(), $"transcoder-test-{Guid.NewGuid():N}{extension}");

    /// <summary>Decodes one stream of a file and returns the number of frames (video) or samples (audio) and the first timestamp.</summary>
    private static (long Count, TimeSpan First) Decode(string file, int streamIndex)
    {
        using MediaSource source = MediaSource.Open(file);
        CodecContext decoder = source.CodecContexts[streamIndex];
        using AVPacket packet = AVPacket.Allocate();
        using AVFrame frame = AVFrame.Allocate();
        long count = 0;
        TimeSpan? first = null;

        void Receive()
        {
            while (!decoder.ReceiveFrame(frame).IsError)
            {
                first ??= frame.BestEffortTimestamp * source.Streams[streamIndex].TimeBase;
                count += decoder.CodecType == MediaType.Audio ? frame.SampleCount : 1;
            }
        }

        while (!source.ReadPacket(packet).IsError)
        {
            if (packet.StreamIndex != streamIndex)
                continue;
            decoder.SendPacket(packet).ThrowIfError();
            Receive();
        }
        _ = decoder.DrainDecoder();
        Receive();
        return (count, first ?? TimeSpan.Zero);
    }

    [TestMethod]
    public void Transcode_TrimsReEncodedStreamsExactly()
    {
        string file = NewOutput(".mkv");
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            _ = transcoder.TranscodeStream(VideoIndex, CodecID.MPEG4, "scale=320:-2");
            _ = transcoder.TranscodeStream(AudioIndex, CodecID.AAC);
            transcoder.Start = TimeSpan.FromSeconds(10);
            transcoder.Duration = TimeSpan.FromSeconds(2);
            transcoder.Run();
            Assert.IsTrue(transcoder.IsFinished);
        }

        (long frames, TimeSpan firstVideo) = Decode(file, 0);
        // 2 s of 60 fps video. The source timestamps are rounded to 1/90000, so the frame at
        // 11.999989 s is still inside the range (ffmpeg -ss 10 -t 2 keeps it as well).
        Assert.IsTrue(frames is 120 or 121, $"Expected 120 frames (121 with the rounded last timestamp), got {frames}.");
        Assert.AreEqual(TimeSpan.Zero, firstVideo);

        (long samples, _) = Decode(file, 1);
        // AAC adds encoder delay and pads the last frame; allow up to two frames.
        Assert.IsTrue(Math.Abs(samples - (2 * 48000)) <= 2048, $"Expected about 96000 samples, got {samples}.");

        using MediaSource result = MediaSource.Open(file);
        Assert.AreEqual(320, result.Streams[0].CodecParameters.Width);
        Assert.AreEqual(180, result.Streams[0].CodecParameters.Height);
    }

    [TestMethod]
    public void CopyStream_StartsAtTheKeyframeBeforeStart()
    {
        string file = NewOutput(".mkv");
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            _ = transcoder.CopyStream(VideoIndex);
            transcoder.Start = TimeSpan.FromSeconds(10);
            transcoder.Duration = TimeSpan.FromSeconds(2);
            transcoder.Run();
        }

        using MediaSource result = MediaSource.Open(file);
        Assert.AreEqual(1, result.Streams.Count);
        Assert.AreEqual(CodecID.H264, result.Streams[0].CodecParameters.CodecId);
        using AVPacket packet = AVPacket.Allocate();
        result.ReadPacket(packet).ThrowIfError();
        Assert.IsTrue(packet.Flags.HasFlag(PacketFlags.Key), "A copied stream must start with a keyframe.");

        // The keyframe before 10 s is at ~8.17 s, so the copy covers ~3.8 s, not more.
        TimeSpan duration = TimeSpan.FromTicks(result.FormatContext.Duration * 10);
        Assert.IsTrue(duration > TimeSpan.FromSeconds(2) && duration < TimeSpan.FromSeconds(4.5), $"Unexpected duration {duration}.");
    }

    [TestMethod]
    public void FilterStream_ReEncodesWithTheInputCodec()
    {
        string file = NewOutput(".mkv");
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            _ = transcoder.FilterStream(AudioIndex, "volume=0.5");
            transcoder.Start = TimeSpan.FromSeconds(10);
            transcoder.Duration = TimeSpan.FromSeconds(1);
            transcoder.Run();
        }

        using MediaSource result = MediaSource.Open(file);
        Assert.AreEqual(CodecID.AC3, result.Streams[0].CodecParameters.CodecId);
        Assert.AreEqual(6, result.Streams[0].CodecParameters.ChannelLayout.Channels);
    }

    [TestMethod]
    public void EncoderSettings_AreEnforcedByTheFilterGraph()
    {
        string file = NewOutput(".mkv");
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            _ = transcoder.TranscodeStream(VideoIndex, CodecID.MPEG4, configure: encoder =>
            {
                encoder.Width = 320;
                encoder.Height = 180;
                encoder.FrameRate = new Rational(25, 1);
            });
            _ = transcoder.TranscodeStream(AudioIndex, CodecID.MP2, configure: encoder =>
            {
                using ChannelLayout stereo = ChannelLayout.CreateStereo();
                encoder.ChannelLayout.SetReferencedObject(stereo);
            });
            transcoder.Duration = TimeSpan.FromSeconds(2);
            transcoder.Run();
        }

        using (MediaSource result = MediaSource.Open(file))
        {
            Assert.AreEqual(320, result.Streams[0].CodecParameters.Width);
            Assert.AreEqual(180, result.Streams[0].CodecParameters.Height);
            Assert.AreEqual(2, result.Streams[1].CodecParameters.ChannelLayout.Channels);
            // MP2 only supports s16 samples; the 6-channel float input is converted by the graph.
            Assert.AreEqual(CodecID.MP2, result.Streams[1].CodecParameters.CodecId);
        }
        Assert.AreEqual(50, Decode(file, 0).Count, "2 s at 25 fps");
    }

    [TestMethod]
    public void Run_ReportsProgressAndPreview()
    {
        string file = NewOutput(".mkv");
        int progressEvents = 0;
        TranscodingProgress last = default;
        using Transcoder transcoder = Transcoder.Create(VideoFile, file);
        _ = transcoder.TranscodeStream(VideoIndex, CodecID.MPEG4, "scale=320:-2");
        transcoder.Duration = TimeSpan.FromSeconds(1);
        transcoder.ProgressInterval = TimeSpan.Zero;
        transcoder.ProgressChanged += (_, progress) => { progressEvents++; last = progress; };
        transcoder.Run();

        Assert.IsTrue(progressEvents > 0);
        Assert.AreEqual(TimeSpan.FromSeconds(1), last.TotalDuration);
        Assert.AreEqual(100, last.Percent!.Value, 0.5);

        using AVFrame preview = AVFrame.Allocate();
        Assert.IsTrue(transcoder.TryGetPreviewFrame(preview));
        Assert.AreEqual(320, preview.Width);
    }

    [TestMethod]
    public void Cancel_StillWritesAPlayableFile()
    {
        string file = NewOutput(".mkv");
        using CancellationTokenSource cancel = new();
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            _ = transcoder.TranscodeStream(VideoIndex, CodecID.MPEG4, "scale=320:-2");
            _ = transcoder.CopyStream(AudioIndex);
            transcoder.ProgressInterval = TimeSpan.Zero;
            transcoder.ProgressChanged += (_, progress) =>
            {
                if (progress.Position > TimeSpan.FromSeconds(1))
                    cancel.Cancel();
            };
            _ = Assert.ThrowsExactly<OperationCanceledException>(() => transcoder.Run(cancel.Token));
            Assert.IsTrue(transcoder.IsFinished);
        }

        long frames = Decode(file, 0).Count;
        Assert.IsTrue(frames is >= 60 and < 600, $"Unexpected frame count {frames}.");
    }

    [TestMethod]
    public void Mapping_IsValidated()
    {
        string file = NewOutput(".mkv");
        using Transcoder transcoder = Transcoder.Create(VideoFile, file);
        _ = Assert.Throws<ArgumentException>(() => transcoder.TranscodeStream(VideoIndex, CodecID.AAC));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => transcoder.CopyStream(42));
        _ = Assert.Throws<InvalidOperationException>(() => transcoder.Run());

        _ = transcoder.TranscodeStream(VideoIndex, CodecID.MPEG4, "scale=160:-2");
        transcoder.Duration = TimeSpan.FromMilliseconds(100);
        _ = transcoder.ProcessNext();
        _ = Assert.Throws<InvalidOperationException>(() => transcoder.CopyStream(AudioIndex));
        _ = Assert.Throws<InvalidOperationException>(() => transcoder.Start = TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void Filter_WithTwoOutputs_IsRejected()
    {
        string file = NewOutput(".mkv");
        using Transcoder transcoder = Transcoder.Create(VideoFile, file);
        _ = transcoder.TranscodeStream(VideoIndex, CodecID.MPEG4, "split[a][b]");
        transcoder.Duration = TimeSpan.FromMilliseconds(100);
        _ = Assert.Throws<ArgumentException>(() => transcoder.Run());
    }

    [TestMethod]
    public void TranscodeStream_WithFilterGraphObject()
    {
        string file = NewOutput(".mkv");
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            FFmpeg.Filters.FilterGraph video = FFmpeg.Filters.FilterGraph.Create(out FFmpeg.Filters.FilterInOutList? videoIn, "scale=320:-2,hflip", out FFmpeg.Filters.FilterInOutList? videoOut);
            _ = transcoder.TranscodeStream(VideoIndex, Codec.FindEncoder(CodecID.MPEG4)!.Value, videoIn!, video, videoOut!);
            FFmpeg.Filters.FilterGraph audio = FFmpeg.Filters.FilterGraph.Create(out FFmpeg.Filters.FilterInOutList? audioIn, "volume=0.5", out FFmpeg.Filters.FilterInOutList? audioOut);
            _ = transcoder.FilterStream(AudioIndex, audioIn!, audio, audioOut!);
            transcoder.Duration = TimeSpan.FromSeconds(1);
            transcoder.Run();
        }

        using MediaSource result = MediaSource.Open(file);
        Assert.AreEqual(320, result.Streams[0].CodecParameters.Width);
        Assert.AreEqual(CodecID.AC3, result.Streams[1].CodecParameters.CodecId);
    }

    [TestMethod]
    public void TranscodeStream_WithFilterGraphObject_RejectsTwoOutputs()
    {
        string file = NewOutput(".mkv");
        using Transcoder transcoder = Transcoder.Create(VideoFile, file);
        FFmpeg.Filters.FilterGraph graph = FFmpeg.Filters.FilterGraph.Create(out FFmpeg.Filters.FilterInOutList? inputs, "split[a][b]", out FFmpeg.Filters.FilterInOutList? outputs);
        _ = Assert.Throws<ArgumentException>(() => transcoder.TranscodeStream(VideoIndex, Codec.FindEncoder(CodecID.MPEG4)!.Value, inputs!, graph, outputs!));
    }

    [TestMethod]
    public void TranscodeStream_WithFilterFactories()
    {
        string file = NewOutput(".mkv");
        List<FilterGraphContext> videoCalls = [];
        List<FilterGraphContext> audioCalls = [];
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            _ = transcoder.TranscodeStream(VideoIndex, Codec.FindEncoder(CodecID.MPEG4)!.Value, ctx =>
            {
                videoCalls.Add(ctx);
                return $"scale={ctx.Width / 4}:-2";
            });
            _ = transcoder.FilterStream(AudioIndex, ctx =>
            {
                audioCalls.Add(ctx);
                FFmpeg.Filters.FilterGraph graph = FFmpeg.Filters.FilterGraph.Create(out FFmpeg.Filters.FilterInOutList? inputs, "volume=0.5", out FFmpeg.Filters.FilterInOutList? outputs);
                return (inputs!, graph, outputs!);
            });
            transcoder.Duration = TimeSpan.FromSeconds(1);
            transcoder.Run();
        }

        Assert.AreEqual(1, videoCalls.Count);
        Assert.AreEqual(1920, videoCalls[0].Width);
        Assert.AreEqual(1080, videoCalls[0].Height);
        Assert.IsFalse(videoCalls[0].IsRebuild);
        Assert.AreEqual(1, audioCalls.Count);
        Assert.AreEqual(48000, audioCalls[0].SampleRate);
        Assert.AreEqual(6, audioCalls[0].Channels);

        using MediaSource result = MediaSource.Open(file);
        Assert.AreEqual(480, result.Streams[0].CodecParameters.Width);
        Assert.AreEqual(CodecID.AC3, result.Streams[1].CodecParameters.CodecId);
    }

    [TestMethod]
    public void UserFilterThatEndsEarly_EndsTheOutput()
    {
        // trim closes the graph after 5 frames; the buffer source then answers AVERROR_EOF to every
        // further frame, which must end the output instead of failing the transcode.
        string file = NewOutput(".mkv");
        using (Transcoder transcoder = Transcoder.Create(VideoFile, file))
        {
            _ = transcoder.TranscodeStream(VideoIndex, CodecID.MPEG4, "trim=end_frame=5,scale=160:-2");
            transcoder.Duration = TimeSpan.FromSeconds(1);
            transcoder.Run();
        }
        Assert.AreEqual(5, Decode(file, 0).Count);
    }
}
