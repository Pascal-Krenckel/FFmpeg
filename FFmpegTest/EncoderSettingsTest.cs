using FFmpeg;
using FFmpeg.Audio;
using FFmpeg.Codecs;
using FFmpeg.Codecs.Audio;
using FFmpeg.Codecs.Video;
using FFmpeg.Codecs.Video.LibSVTAV1;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpegTest;

/// <summary>
/// Tests for the typed encoder settings. Each test checks an effect in the encoded stream (key frame distance,
/// profile, size, lossless round trip, ...), so a setting that never reaches the encoder fails.
/// External encoders are skipped (inconclusive) when FFmpeg was built without them.
/// </summary>
[TestClass]
public class EncoderSettingsTest
{
    private static readonly Rational FrameRate = new(25, 1);

    #region Helpers

    private sealed record Packet(int Size, bool Key, long Pts, long Dts, long Duration);

    private static void RequireEncoder(bool supported, string name)
    {
        if (!supported)
            Assert.Inconclusive($"FFmpeg was built without {name}.");
    }

    /// <summary>A video frame with moving noise, so encoders cannot skip it.</summary>
    private static AVFrame NoiseFrame(int width, int height, PixelFormat format, long pts, int seed)
    {
        AVFrame frame = AVFrame.Allocate(format, width, height);
        Random random = new(seed);
        for (int plane = 0; plane < 3; plane++)
            random.NextBytes(frame.GetData(plane));
        frame.PresentationTimestamp = pts;
        frame.Duration = 1;
        frame.TimeBase = new Rational(FrameRate.Denominator, FrameRate.Numerator);
        return frame;
    }

    /// <summary>A smooth gradient that moves, closer to real video than noise.</summary>
    private static AVFrame GradientFrame(int width, int height, long pts)
    {
        AVFrame frame = AVFrame.Allocate(PixelFormat.YUV420P, width, height);
        Span<byte> y = frame.GetData(0);
        int stride = frame.LineSize[0];
        for (int row = 0; row < height; row++)
            for (int col = 0; col < width; col++)
                y[(row * stride) + col] = (byte)((col + row + (pts * 3)) & 0xFF);
        frame.GetData(1).Fill(100);
        frame.GetData(2).Fill(150);
        frame.PresentationTimestamp = pts;
        frame.Duration = 1;
        frame.TimeBase = new Rational(FrameRate.Denominator, FrameRate.Numerator);
        return frame;
    }

    /// <summary>Encodes the frames (disposing them) and returns the packets and their data.</summary>
    private static List<(Packet Info, byte[] Data)> Encode(CodecContext encoder, IEnumerable<AVFrame> frames)
    {
        List<(Packet, byte[])> packets = [];
        using AVPacket packet = AVPacket.Allocate();
        void Receive()
        {
            AVResult32 res;
            while (!(res = encoder.ReceivePacket(packet)).IsError)
            {
                packets.Add((new Packet(packet.Size, packet.Flags.HasFlag(PacketFlags.Key), packet.PresentationTimestamp, packet.DecompressionTimestamp, packet.Duration), packet.Data.ToArray()));
                packet.Unreference();
            }
            if (!res.IsTryAgain && res != AVResult32.EndOfFile)
                res.ThrowIfError();
        }
        foreach (AVFrame frame in frames)
        {
            using (frame)
                encoder.SendFrame(frame).ThrowIfError();
            Receive();
        }
        encoder.SendFrame(null).ThrowIfError();
        Receive();
        return packets;
    }

    private static List<(Packet Info, byte[] Data)> EncodeVideo(VideoCodec settings, int count, int width = 64, int height = 64, bool noise = true)
    {
        settings.Width = width;
        settings.Height = height;
        settings.TimeBase = new Rational(FrameRate.Denominator, FrameRate.Numerator);
        settings.Framerate = FrameRate;
        using CodecContext encoder = settings.CreateCodecContext();
        encoder.Open(null).ThrowIfError();
        PixelFormat format = encoder.PixelFormat;
        return Encode(encoder, Enumerable.Range(0, count).Select(i => noise ? NoiseFrame(width, height, format, i, i) : GradientFrame(width, height, i)));
    }

    /// <summary>Decodes the packets with a decoder opened from the encoder parameters.</summary>
    private static (List<AVFrame> Frames, Profile Profile) DecodeAll(CodecContext encoder, IEnumerable<byte[]> packets)
    {
        CodecParameters parameters = new();
        parameters.CopyFrom(encoder);
        using CodecContext decoder = CodecContext.Open(Codec.FindDecoder(encoder.CodecID)!.Value, parameters);
        List<AVFrame> frames = [];
        void Receive()
        {
            while (true)
            {
                AVFrame frame = AVFrame.Allocate();
                if (decoder.ReceiveFrame(frame).IsError)
                {
                    frame.Dispose();
                    return;
                }
                frames.Add(frame);
            }
        }
        foreach (byte[] data in packets.Where(d => d.Length > 0)) // e.g. FLAC's final packet only carries side data
        {
            using AVPacket packet = AVPacket.Allocate(data.Length);
            data.CopyTo(packet.Data);
            decoder.SendPacket(packet).ThrowIfError();
            Receive();
        }
        _ = decoder.DrainDecoder();
        Receive();
        return (frames, decoder.Profile);
    }

    private static int[] KeyFrames(List<(Packet Info, byte[] Data)> packets)
        => [.. packets.Select((p, i) => (p, i)).Where(x => x.p.Info.Key).Select(x => (int)x.p.Info.Pts)];

    private static long TotalSize(List<(Packet Info, byte[] Data)> packets) => packets.Sum(p => (long)p.Info.Size);

    /// <summary>Encodes a stereo signal of <paramref name="seconds"/> with the settings and returns the packets.</summary>
    private static (List<(Packet Info, byte[] Data)> Packets, CodecContext Encoder) EncodeAudio(AudioCodec settings, double seconds, Func<int, int, float>? signal = null)
    {
        signal ??= (sample, channel) => (float)(0.4 * Math.Sin(2 * Math.PI * (440 + (channel * 110)) * sample / 48000.0));
        CodecContext encoder = settings.CreateCodecContext();
        encoder.Open(null).ThrowIfError();
        int rate = encoder.SampleRate;
        int frameSize = encoder.FrameSize > 0 ? encoder.FrameSize : 1024;
        int total = (int)(seconds * rate);
        using ChannelLayout layout = ChannelLayout.CreateDefault(encoder.ChannelLayout.Channels);
        IEnumerable<AVFrame> Frames()
        {
            for (int start = 0; start < total; start += frameSize)
            {
                int count = Math.Min(frameSize, total - start);
                AVFrame frame = AVFrame.Allocate(encoder.SampleFormat, layout, count, rate);
                for (int channel = 0; channel < layout.Channels; channel++)
                {
                    for (int i = 0; i < count; i++)
                    {
                        float value = signal(start + i, channel);
                        SetSample(frame, encoder.SampleFormat, layout.Channels, channel, i, value);
                    }
                }
                frame.PresentationTimestamp = start;
                frame.TimeBase = new Rational(1, rate);
                yield return frame;
            }
        }
        return (Encode(encoder, Frames()), encoder);
    }

    private static void SetSample(AVFrame frame, SampleFormat format, int channels, int channel, int index, float value)
    {
        switch (format)
        {
            case SampleFormat.Float32Planar:
                MemoryMarshal.Cast<byte, float>(frame.GetData(channel))[index] = value;
                break;
            case SampleFormat.Float32:
                MemoryMarshal.Cast<byte, float>(frame.GetData(0))[(index * channels) + channel] = value;
                break;
            case SampleFormat.Int16:
                MemoryMarshal.Cast<byte, short>(frame.GetData(0))[(index * channels) + channel] = (short)(value * short.MaxValue);
                break;
            case SampleFormat.Int16Planar:
                MemoryMarshal.Cast<byte, short>(frame.GetData(channel))[index] = (short)(value * short.MaxValue);
                break;
            case SampleFormat.Int32:
                MemoryMarshal.Cast<byte, int>(frame.GetData(0))[(index * channels) + channel] = (int)(value * int.MaxValue);
                break;
            case SampleFormat.Int32Planar:
                MemoryMarshal.Cast<byte, int>(frame.GetData(channel))[index] = (int)(value * int.MaxValue);
                break;
            default:
                throw new NotSupportedException(format.ToString());
        }
    }

    #endregion

    #region Common behaviour

    [TestMethod]
    public void Configure_DoesNotConsumeAdditionalParameters()
    {
        Flac flac = new() { SampleRate = 44100, SampleFormat = SampleFormat.Int16 };
        flac.AdditionalParameters["lpc_passes"] = "3";
        using (CodecContext first = flac.CreateCodecContext())
        using (CodecContext second = flac.CreateCodecContext())
        {
            Assert.IsFalse(first.TryGetOption("lpc_passes", out long passes).IsError);
            Assert.AreEqual(3, passes);
            Assert.IsFalse(second.TryGetOption("lpc_passes", out passes).IsError);
            Assert.AreEqual(3, passes, "The second context gets the parameters too.");
        }
        Assert.AreEqual("3", flac.AdditionalParameters["lpc_passes"]);
    }

    [TestMethod]
    public void Configure_RejectsUnknownOptionsAndUnsupportedFormats()
    {
        Flac flac = new();
        flac.AdditionalParameters["no_such_option"] = "1";
        _ = Assert.Throws<ArgumentException>(() => flac.CreateCodecContext(), "Unknown options are not silently ignored.");

        _ = Assert.Throws<ArgumentException>(() => new Flac { SampleFormat = SampleFormat.Float32 }.CreateCodecContext(), "FLAC has no float samples.");

        using CodecContext open = new Flac { SampleRate = 44100 }.CreateCodecContext();
        open.Open(null).ThrowIfError();
        _ = Assert.Throws<InvalidOperationException>(() => new Flac().Configure(open));

        using CodecContext aacContext = new Aac().CreateCodecContext();
        _ = Assert.Throws<ArgumentException>(() => new Flac().Configure(aacContext), "Settings for another codec.");
    }

    [TestMethod]
    public void ToString_PrintsTheCommandLine()
    {
        Aac aac = new() { SampleRate = 48000, Bitrate = 128000, Coder = AacCoder.Fast, PerceptualNoiseSubstitution = false };
        aac.AdditionalParameters["cutoff"] = "18000";
        Assert.AreEqual("-c:a aac -ar 48000 -b 128000 -aac_coder fast -aac_pns 0 -cutoff 18000", aac.ToString());
        EncoderOptions options = aac.GetOptions();
        Assert.IsTrue(options["aac_coder"]!.Value.IsPrivate);
        Assert.IsFalse(options["b"]!.Value.IsPrivate);
    }

    #endregion

    #region Audio

    [TestMethod]
    public void Flac_IsLosslessAndHonoursTheCompressionLevel()
    {
        Flac fast = new() { SampleRate = 48000, SampleFormat = SampleFormat.Int16, CompressionLevel = 0, ChannelMode = FlacChannelMode.Independent };
        Flac best = new() { SampleRate = 48000, SampleFormat = SampleFormat.Int16, CompressionLevel = 12, LpcType = FlacLpcType.Cholesky, ExactRiceParameters = true };

        (List<(Packet Info, byte[] Data)> fastPackets, CodecContext fastEncoder) = EncodeAudio(fast, 1);
        (List<(Packet Info, byte[] Data)> bestPackets, CodecContext bestEncoder) = EncodeAudio(best, 1);
        using (fastEncoder)
        using (bestEncoder)
        {
            Assert.IsTrue(TotalSize(bestPackets) < TotalSize(fastPackets), $"level 12 ({TotalSize(bestPackets)}) < level 0 ({TotalSize(fastPackets)})");

            (List<AVFrame> frames, _) = DecodeAll(bestEncoder, bestPackets.Select(p => p.Data));
            short[] decoded = [.. frames.SelectMany(f => MemoryMarshal.Cast<byte, short>(f.GetData(0))[..(f.SampleCount * 2)].ToArray())];
            Assert.AreEqual(48000 * 2, decoded.Length);
            for (int i = 0; i < 1000; i++)
                Assert.AreEqual((short)((float)(0.4 * Math.Sin(2 * Math.PI * (440 + ((i % 2) * 110)) * (i / 2) / 48000.0)) * short.MaxValue), decoded[i], "lossless");
            frames.ForEach(f => f.Dispose());
        }
    }

    [TestMethod]
    public void Aac_BitrateQualityAndProfileReachTheEncoder()
    {
        (List<(Packet Info, byte[] Data)> low, CodecContext lowEncoder) = EncodeAudio(new Aac { Bitrate = 48000 }, 2);
        (List<(Packet Info, byte[] Data)> high, CodecContext highEncoder) = EncodeAudio(new Aac { Bitrate = 192000 }, 2);
        (List<(Packet Info, byte[] Data)> vbr, CodecContext vbrEncoder) = EncodeAudio(new Aac { Quality = 2 }, 2);
        using (lowEncoder)
        using (highEncoder)
        using (vbrEncoder)
        {
            Assert.IsTrue(TotalSize(high) > TotalSize(low) * 2, $"192k ({TotalSize(high)}) vs 48k ({TotalSize(low)})");
            Assert.IsTrue(vbrEncoder.Flags.HasFlag(CodecFlags.FixedQuantizationScale));
            Assert.AreEqual(2 * 118, vbrEncoder.GlobalQuality);
        }

        using (CodecContext configured = new Aac { Profile = AacProfile.Mpeg2LowComplexity }.CreateCodecContext())
            Assert.AreEqual((int)AacProfile.Mpeg2LowComplexity, configured.Profile.ProfileID, "The profile reaches the context.");
        (List<(Packet Info, byte[] Data)> mpeg2, CodecContext mpeg2Encoder) = EncodeAudio(new Aac { Profile = AacProfile.Mpeg2LowComplexity, Bitrate = 128000 }, 1);
        using (mpeg2Encoder)
        {
            (List<AVFrame> frames, _) = DecodeAll(mpeg2Encoder, mpeg2.Select(p => p.Data));
            Assert.AreEqual(48000, frames.Sum(f => f.SampleCount), 1024);
            frames.ForEach(f => f.Dispose());
        }
    }

    [TestMethod]
    public void Opus_OptionsReachTheEncoder()
    {
        RequireEncoder(Opus.IsSupported, "libopus");
        (List<(Packet Info, byte[] Data)> low, CodecContext lowEncoder) = EncodeAudio(new Opus { Bitrate = 24000, BitrateMode = OpusBitrateMode.Constant }, 2);
        (List<(Packet Info, byte[] Data)> high, CodecContext highEncoder) = EncodeAudio(new Opus { Bitrate = 160000, BitrateMode = OpusBitrateMode.Constant }, 2);
        using (lowEncoder)
        using (highEncoder)
        {
            Assert.IsTrue(TotalSize(high) > TotalSize(low) * 4, $"160k ({TotalSize(high)}) vs 24k ({TotalSize(low)})");
            Assert.AreEqual(1, low.Select(p => p.Info.Size).Distinct().Count(), "Constant bitrate: all packets have the same size.");
        }

        (List<(Packet Info, byte[] Data)> longFrames, CodecContext longEncoder) = EncodeAudio(new Opus { FrameDuration = 60, Application = OpusApplication.Audio, CompressionLevel = 5 }, 1);
        using (longEncoder)
        {
            Assert.AreEqual(2880, longEncoder.FrameSize, "60 ms frames at 48 kHz.");
            Assert.AreEqual(2880, longFrames[1].Info.Duration);
        }
    }

    [TestMethod]
    public void Mp3_ConstantAndVariableBitrate()
    {
        RequireEncoder(Mp3.IsSupported, "libmp3lame");
        (List<(Packet Info, byte[] Data)> cbr, CodecContext cbrEncoder) = EncodeAudio(new Mp3 { Bitrate = 128000, SampleRate = 48000 }, 2);
        (List<(Packet Info, byte[] Data)> best, CodecContext bestEncoder) = EncodeAudio(new Mp3 { Quality = 0, SampleRate = 48000 }, 2);
        (List<(Packet Info, byte[] Data)> worst, CodecContext worstEncoder) = EncodeAudio(new Mp3 { Quality = 9, SampleRate = 48000 }, 2);
        using (cbrEncoder)
        using (bestEncoder)
        using (worstEncoder)
        {
            // 128 kbit/s at 48 kHz: 1152 samples per frame are 384 bytes.
            Assert.IsTrue(cbr.Skip(2).All(p => p.Info.Size == 384), string.Join(",", cbr.Take(6).Select(p => p.Info.Size)));
            Assert.IsTrue(TotalSize(best) > TotalSize(worst), $"V0 ({TotalSize(best)}) > V9 ({TotalSize(worst)})");
        }
    }

    #endregion

    #region Video

    [TestMethod]
    public void X264_RateControlGopAndProfileReachTheEncoder()
    {
        RequireEncoder(X264.IsSupported, "libx264");
        List<(Packet Info, byte[] Data)> gop = EncodeVideo(new X264 { Preset = SpeedPreset.UltraFast, KeyFrameInterval = 5, MinKeyFrameInterval = 5, Crf = 30 }, 12, noise: false);
        CollectionAssert.AreEqual(new[] { 0, 5, 10 }, KeyFrames(gop));

        long Size(double crf) => TotalSize(EncodeVideo(new X264 { Preset = SpeedPreset.UltraFast, Crf = crf }, 10));
        Assert.IsTrue(Size(10) > Size(45) * 2, "crf changes the size");

        X264 baseline = new() { Profile = X264Profile.Baseline, Tune = X264Tune.ZeroLatency | X264Tune.Film, Preset = SpeedPreset.Fast };
        baseline.Width = 64;
        baseline.Height = 64;
        baseline.TimeBase = new Rational(1, 25);
        using CodecContext encoder = baseline.CreateCodecContext();
        encoder.Open(null).ThrowIfError();
        List<(Packet Info, byte[] Data)> packets = Encode(encoder, Enumerable.Range(0, 8).Select(i => GradientFrame(64, 64, i)));
        Assert.IsTrue(packets.All(p => p.Info.Pts == p.Info.Dts), "Baseline has no B-frames.");
        (List<AVFrame> frames, Profile profile) = DecodeAll(encoder, packets.Select(p => p.Data));
        Assert.AreEqual(66, profile.ProfileID & 0xFF, $"Decoder sees profile {profile.ProfileID}.");
        Assert.AreEqual(8, frames.Count);
        frames.ForEach(f => f.Dispose());

        _ = Assert.Throws<ArgumentException>(() => new X264 { Tune = X264Tune.Film | X264Tune.Grain }.GetOptions(), "Only one content tune.");
    }

    [TestMethod]
    public void X264_ParametersOverrideTheTypedSettings()
    {
        RequireEncoder(X264.IsSupported, "libx264");
        X264 settings = new() { Preset = SpeedPreset.UltraFast, KeyFrameInterval = 100 };
        settings.X264Parameters["keyint"] = "4";
        settings.X264Parameters["min-keyint"] = "4";
        settings.X264Parameters["scenecut"] = "0";
        Assert.AreEqual("-c:v libx264 -preset ultrafast -g 100 -x264-params keyint=4:min-keyint=4:scenecut=0", settings.ToString());
        CollectionAssert.AreEqual(new[] { 0, 4, 8 }, KeyFrames(EncodeVideo(settings, 10, noise: false)));
    }

    [TestMethod]
    public void X265_GopAndCrfReachTheEncoder()
    {
        RequireEncoder(X265.IsSupported, "libx265");
        X265 settings = new() { Preset = SpeedPreset.UltraFast, KeyFrameInterval = 6, MinKeyFrameInterval = 6, Crf = 30, Profile = "main" };
        settings.X265Parameters["scenecut"] = "0";
        settings.X265Parameters["log-level"] = "error";
        CollectionAssert.AreEqual(new[] { 0, 6 }, KeyFrames(EncodeVideo(settings, 12, noise: false)));

        long Size(double crf)
        {
            X265 s = new() { Preset = SpeedPreset.UltraFast, Crf = crf };
            s.X265Parameters["log-level"] = "error";
            return TotalSize(EncodeVideo(s, 8));
        }
        Assert.IsTrue(Size(5) > Size(45) * 2, "crf changes the size");
    }

    [TestMethod]
    public void VP9_LosslessRoundTripAndConstantQuality()
    {
        RequireEncoder(VP9.IsSupported, "libvpx-vp9");
        VP9 lossless = new() { Lossless = true, Deadline = VpxDeadline.Realtime, CpuUsed = 8, RowMultithreading = true, PixelFormat = PixelFormat.YUV420P };
        lossless.Width = 64;
        lossless.Height = 64;
        lossless.TimeBase = new Rational(1, 25);
        using CodecContext encoder = lossless.CreateCodecContext();
        encoder.Open(null).ThrowIfError();
        using AVFrame original = NoiseFrame(64, 64, PixelFormat.YUV420P, 0, 7);
        byte[] luma = original.GetData(0).ToArray();
        List<(Packet Info, byte[] Data)> packets = Encode(encoder, [original.Clone()]);
        (List<AVFrame> frames, _) = DecodeAll(encoder, packets.Select(p => p.Data));
        Assert.AreEqual(1, frames.Count);
        for (int row = 0; row < 64; row++)
            CollectionAssert.AreEqual(luma.AsSpan(row * original.LineSize[0], 64).ToArray(), frames[0].GetData(0).Slice(row * frames[0].LineSize[0], 64).ToArray(), "lossless");
        frames.ForEach(f => f.Dispose());

        VP9 quality = new() { Crf = 30 };
        Assert.AreEqual("0", quality.GetOptions()["b"]!.Value.Value, "crf alone selects constant quality (b=0).");
        long Size(int crf) => TotalSize(EncodeVideo(new VP9 { Crf = crf, Deadline = VpxDeadline.Realtime, CpuUsed = 8 }, 6, noise: false));
        Assert.IsTrue(Size(4) > Size(60) * 2, "crf changes the size");
    }

    [TestMethod]
    public void SVTAV1_ParametersReachTheEncoder()
    {
        // svtav1-params used to be applied as separate top-level options, which libsvtav1 does not have, so
        // FFmpeg left them unused: keyint, tune, lp, ... never reached SVT-AV1.
        RequireEncoder(SVTAV1.IsSupported, "libsvtav1");
        SVTAV1 settings = new() { Preset = 12, KeyFrameInterval = 5, CRF = 40, Tune = TuningMetric.PSNR, LevelOfParallelism = 1 };
        settings.AdditionalSVTParameters["scd"] = "0";
        Assert.AreEqual("-c:v libsvtav1 -svtav1-params preset=12:rc=0:aq-mode=2:qp=40:tune=1:keyint=5:lp=1:scd=0", settings.ToString());
        CollectionAssert.AreEqual(new[] { 0, 5, 10 }, KeyFrames(EncodeVideo(settings, 12)));

        long Size(double crf) => TotalSize(EncodeVideo(new SVTAV1 { Preset = 12, Quality = crf }, 6, noise: false));
        Assert.IsTrue(Size(5) > Size(60) * 2, "crf changes the size");
    }

    [TestMethod]
    public void SVTAV1_CreateAvif()
    {
        RequireEncoder(SVTAV1.IsSupported, "libsvtav1");
        List<AVFrame> frames = [.. Enumerable.Range(0, 4).Select(i => NoiseFrame(80, 64, PixelFormat.RGB24, i * 40, i))];
        frames.ForEach(f => { f.TimeBase = new Rational(1, 1000); f.Duration = 40; });
        try
        {
            SVTAV1 settings = new() { Preset = 12 };
            using MemoryStream timed = new();
            settings.CreateAvif(timed, frames, closeStream: false);
            Assert.AreEqual(0, settings.Width, "The settings are not changed.");
            Assert.AreEqual(40, frames[1].PresentationTimestamp, "The frames are not changed.");

            using MemoryStream fixedRate = new();
            settings.CreateAvif(fixedRate, frames, TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(40, frames[1].PresentationTimestamp, "The frames are not changed.");

            foreach ((MemoryStream stream, double expected) in new[] { (timed, 0.04), (fixedRate, 0.1) })
            {
                stream.Position = 0;
                // An animated AVIF has the still image item (stream 0) and the sequence track.
                using MediaSource source = MediaSource.Open(stream);
                int sequence = source.Streams.Count - 1;
                Assert.AreEqual(80, source.Streams[sequence].CodecParameters.Width);
                using AVPacket packet = AVPacket.Allocate();
                List<double> times = [];
                while (!source.ReadPacket(packet).IsError)
                {
                    if (packet.StreamIndex != sequence)
                        continue;
                    Assert.IsFalse(packet.Flags.HasFlag(PacketFlags.Discard), "No frame is cut off by the edit list.");
                    Assert.AreEqual(expected, (double)(packet.Duration * source.Streams[sequence].TimeBase), 1e-3, "duration");
                    times.Add((double)(packet.PresentationTimestamp * source.Streams[sequence].TimeBase));
                }
                Assert.AreEqual(4, times.Count);
                Assert.AreEqual(expected * 3, times[3] - times[0], 1e-3, "timestamps");
            }
        }
        finally
        {
            frames.ForEach(f => f.Dispose());
        }
        _ = Assert.Throws<ArgumentException>(() => new SVTAV1().CreateAvif(new MemoryStream(), [], disposeFrames: true), "An empty sequence throws ArgumentException, not NullReferenceException.");
    }

    [TestMethod]
    public void Transcoder_UsesTheEncoderSettings()
    {
        RequireEncoder(X264.IsSupported, "libx264");
        string file = Path.Combine(Path.GetTempPath(), $"encoder-settings-{Guid.NewGuid():N}.mkv");
        try
        {
            using (Transcoder transcoder = Transcoder.Create(TestFiles.Get("mp4-example-video-download-full-hd-1920x1080.1min.mp4"), file))
            {
                X264 x264 = new() { Preset = SpeedPreset.UltraFast, Crf = 35, KeyFrameInterval = 10 };
                x264.X264Parameters["scenecut"] = "0"; // no extra key frames at the scene cuts of the test video
                _ = transcoder.TranscodeStream(0, x264, "scale=160:-2");
                _ = transcoder.TranscodeStream(1, new Aac { Bitrate = 64000 });
                transcoder.Duration = TimeSpan.FromSeconds(2);
                transcoder.Run();
            }
            using MediaSource source = MediaSource.Open(file);
            Assert.AreEqual(160, source.Streams[0].CodecParameters.Width);
            Assert.AreEqual(CodecID.H264, source.Streams[0].CodecParameters.CodecId);
            using AVPacket packet = AVPacket.Allocate();
            List<long> keys = [];
            long index = 0;
            while (!source.ReadPacket(packet).IsError)
            {
                if (packet.StreamIndex != 0)
                    continue;
                if (packet.Flags.HasFlag(PacketFlags.Key))
                    keys.Add(index);
                index++;
            }
            Assert.IsTrue(keys.Count >= 4 && keys.Take(4).SequenceEqual([0L, 10, 20, 30]), $"key frames at {string.Join(",", keys)}");
        }
        finally
        {
            File.Delete(file);
        }
    }

    #endregion
}
