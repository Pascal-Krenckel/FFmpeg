using FFmpeg.Audio;
using FFmpeg.Codecs;
using FFmpeg.Formats;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpegTest;

[TestClass]
public class DecodingTest
{

    private DemuxerContext? demuxerContext;
    private FFProbeJson? ffProbe;

    [TestInitialize]
    public void Initialize()
    {
        ffProbe = FFProbeJson.Parse(File.ReadAllText(TestFiles.VideoProbeJson))!;
        string videoFile = TestFiles.Get(ffProbe.Format.Filename);
        demuxerContext = DemuxerContext.Open(videoFile, true);
    }

    [TestCleanup]
    public void Cleanup() => demuxerContext?.Dispose();

    [TestMethod]
    public void CheckFormatProperties()
    {

        InputFormat? expectedFormat = InputFormat.FindFormat(ffProbe!.Format.FormatName);
        Assert.AreEqual(expectedFormat, demuxerContext!.InputFormat);
        Assert.AreEqual(ffProbe.Format.NbStreams, demuxerContext.StreamCount);
        Assert.AreEqual(ffProbe.Format.StartTime, (double)(demuxerContext.StartTime * Rational.TIME_BASE));
        Assert.AreEqual(ffProbe.Format.Duration, (double)(demuxerContext.Duration * Rational.TIME_BASE));
        Assert.AreEqual(ffProbe.Format.BitRate, demuxerContext.BitRate);
    }

    [TestMethod]
    public void CheckSeekableBitAlignment()
    {
        Assert.AreEqual(FFmpeg.IO.Seekable.Byte, demuxerContext!.Seekable);
    }

    [TestMethod]
    public void CheckStreamProperties()
    {
        Assert.HasCount(ffProbe!.Streams.Length, demuxerContext!.Streams);
        foreach (Stream probeStream in ffProbe.Streams)
            CheckStreamProperties(probeStream, demuxerContext.Streams[probeStream.Index]);
    }

    private void CheckStreamProperties(Stream probeStream, AVStream avStream)
    {
        Assert.AreEqual(probeStream.Index, avStream.Index);
        Assert.AreEqual(int.Parse(probeStream.Id[2..], System.Globalization.NumberStyles.HexNumber), avStream.Id);
        CheckCodec(probeStream, avStream);
    }

    private void CheckCodec(Stream probeStream, AVStream avStream)
    {
        Codec codec = Codec.FindDecoder(probeStream.CodecName)!.Value;
        Assert.AreEqual(probeStream.CodecLongName, codec.LongName);
        if (avStream.MediaType == MediaType.Video)
        {
            Assert.AreEqual(probeStream.Profile, avStream.CodecParameters.Profile.Name);
            Assert.AreEqual(probeStream.Width, avStream.CodecParameters.Width);
            Assert.AreEqual(probeStream.Height, avStream.CodecParameters.Height);
            Assert.AreEqual(PixelFormat.Parse(probeStream.PixFmt), avStream.CodecParameters.PixelFormat);
            Assert.AreEqual(Rational.Parse(probeStream.SampleAspectRatio), avStream.SampleAspectRatio);
            Assert.AreEqual(probeStream.Level, avStream.CodecParameters.Level);

        }
        else if (avStream.MediaType == MediaType.Audio)
        {
            Assert.AreEqual(probeStream.SampleRate, avStream.CodecParameters.SampleRate);
            Assert.AreEqual(ChannelLayout.Parse(probeStream.ChannelLayout), avStream.CodecParameters.ChannelLayout.GetReferencedObject());
            Assert.AreEqual(probeStream.Channels, avStream.CodecParameters.Channels);
            Assert.AreEqual(SampleFormat.Parse(probeStream.SampleFmt), avStream.CodecParameters.SampleFormat);
        }
        Assert.AreEqual(probeStream.CodecType, codec.MediaType.ToString(), StringComparer.OrdinalIgnoreCase);
        Assert.AreEqual(probeStream.CodecTag, avStream.CodecParameters.CodecTag);

        Assert.AreEqual(probeStream.Id, "0x" + avStream.Id.ToString("X"), StringComparer.OrdinalIgnoreCase);
        Assert.AreEqual(Rational.Parse(probeStream.RFrameRate), avStream.RealFrameRate);
        Assert.AreEqual(Rational.Parse(probeStream.AvgFrameRate), avStream.AverageFrameRate);
        Assert.AreEqual(Rational.Parse(probeStream.TimeBase), avStream.TimeBase);
        Assert.AreEqual(probeStream.StartPts, avStream.StartTime);
        Assert.AreEqual(probeStream.Duration, (double)(avStream.Duration * avStream.TimeBase));
        Assert.AreEqual(probeStream.DurationTs, avStream.Duration);
        Assert.AreEqual(probeStream.BitRate, avStream.CodecParameters.BitRate);
        Assert.AreEqual(probeStream.NbFrames, avStream.NumberOfFrames);
    }
    public static void SelectStream(DemuxerContext context, int index)
    {
        foreach (AVStream stream in context.Streams)
            stream.Discard = DiscardFlags.All;
        context.Streams[index].Discard = DiscardFlags.Default;
    }

    public static int SelectStream(DemuxerContext context, MediaType mediaType)
    {
        int index = context.FindBestStream(mediaType);
        SelectStream(context, index);
        return index;
    }

    public static AVResult32 SeekAndRead(DemuxerContext input, int streamIndex, CodecContext codec, AVFrame frame, TimeSpan seek)
    {
        using AVPacket packet = AVPacket.Allocate();
        AVResult32 result = input.Seek(seek, streamIndex);
        if (result.IsError)
            return result;
        codec.FlushBuffers(); // we seeked so flush the codecs internal buffers
        codec.TimeBase = input.Streams[streamIndex].TimeBase; // set codec TimeBase just to be sure
        do
        {
            do
            {
                result = input.ReadPacket(packet);
                if (result.IsError)
                    return result;
                if (packet.Flags.HasFlag(PacketFlags.Discard))
                {
                    result = AVResult32.TryAgain;
                    continue;
                }
                result = codec.SendPacket(packet);
                if (result.IsError)
                    return result;
                result = codec.ReceiveFrame(frame);
            } while (result.IsTryAgain);
        } while (!result.IsError && (frame.GetPresentationTimestamp() + frame.Duration) * frame.TimeBase < seek);
        return result;
    }

    [TestMethod]
    public void DecodingYUV420p()
    {
        int frameNr = 123;
        byte[] yuv = File.ReadAllBytes(TestFiles.Yuv);
        int videoIndex = SelectStream(demuxerContext!, MediaType.Video);
        TimeSpan estimatedPTS = frameNr / demuxerContext!.Streams[videoIndex].RealFrameRate;
        Codec c = Codec.FindDecoder(demuxerContext.Streams[videoIndex].CodecParameters.CodecId)!.Value;
        using CodecContext decoder = CodecContext.Open(c, demuxerContext.Streams[videoIndex].CodecParameters);
        using AVFrame frame = AVFrame.Allocate();

        SeekAndRead(demuxerContext, videoIndex, decoder, frame, estimatedPTS).ThrowIfError();


        using Image image = Image.FromPixelCopy(new ImageInfo(frame.Width, frame.Height, PixelFormat.YUV420P), yuv);
        Assert.AreEqual(yuv.Length, image.Info.BufferSize);

        for (int plane = 0; plane < image.Info.Planes; plane++)
        {
            Span<byte> frameData = frame.GetData(plane);
            Span<byte> imageData = image.GetPlane(plane);
            Assert.IsTrue(frameData.SequenceEqual(imageData));
        }

    }

    [TestMethod]
    public void DecodingAndConversionBGRA()
    {
        int frameNr = 123;
        byte[] bgra = File.ReadAllBytes(TestFiles.Bgra);
        int videoIndex = SelectStream(demuxerContext!, MediaType.Video);
        TimeSpan estimatedPTS = frameNr / demuxerContext!.Streams[videoIndex].RealFrameRate;
        Codec c = Codec.FindDecoder(demuxerContext.Streams[videoIndex].CodecParameters.CodecId)!.Value;
        using CodecContext decoder = CodecContext.Open(c, demuxerContext.Streams[videoIndex].CodecParameters);
        using AVFrame frame = AVFrame.Allocate();

        SeekAndRead(demuxerContext, videoIndex, decoder, frame, estimatedPTS).ThrowIfError();

        using Image image = Image.Create(new ImageInfo(frame.Width, frame.Height, PixelFormat.BGRA));
        SwsContext.Convert(frame, image, SwsAlgorithm.FastBilinear()).ThrowIfError();
        Assert.IsTrue(image.GetPlane(0).SequenceEqual(bgra));
    }

    [TestMethod]
    public void DecodingAudio_MatchesReferencePcm()
    {
        // Test-Files/10s-11s.*.pcm hold the decoded 5.1 AC-3 track from 10 s to 11 s
        // as packed float and as packed 16-bit samples.
        TimeSpan start = TimeSpan.FromSeconds(10);
        int audioIndex = SelectStream(demuxerContext!, MediaType.Audio);
        AVStream stream = demuxerContext!.Streams[audioIndex];
        int sampleRate = stream.CodecParameters.SampleRate;
        int channels = stream.CodecParameters.Channels;

        Codec c = Codec.FindDecoder(stream.CodecParameters.CodecId)!.Value;
        using CodecContext decoder = CodecContext.Open(c, stream.CodecParameters);
        using AVFrame frame = AVFrame.Allocate();
        using AVPacket packet = AVPacket.Allocate();

        SeekAndRead(demuxerContext, audioIndex, decoder, frame, start).ThrowIfError();

        // The decoder delivers planar float; the FIFO converts to packed, as in the reference.
        using AudioFifo fifo = new(SampleFormat.Float32, channels);
        TimeSpan firstFrameStart = frame.GetPresentationTimestamp() * frame.TimeBase;
        int samplesBeforeStart = (int)Math.Round((start - firstFrameStart).TotalSeconds * sampleRate);
        Assert.IsTrue(samplesBeforeStart >= 0 && samplesBeforeStart < frame.SampleCount, $"Seek landed {samplesBeforeStart} samples away from the start.");
        Assert.AreEqual(frame.SampleCount, (int)fifo.Write(frame));
        fifo.Drop(samplesBeforeStart).ThrowIfError();

        // Decode until one second of audio is buffered.
        while (fifo.Count < sampleRate)
        {
            AVResult32 result = decoder.ReceiveFrame(frame);
            if (result.IsTryAgain)
            {
                demuxerContext.ReadPacket(packet).ThrowIfError();
                decoder.SendPacket(packet).ThrowIfError();
                continue;
            }
            result.ThrowIfError();
            Assert.AreEqual(frame.SampleCount, (int)fifo.Write(frame));
        }

        float[] decoded = new float[sampleRate * channels];
        Assert.AreEqual(sampleRate, (int)fifo.Read<float>(decoded.AsSpan()));

        float[] expected = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(File.ReadAllBytes(TestFiles.AudioF32le)).ToArray();
        short[] expected16 = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, short>(File.ReadAllBytes(TestFiles.AudioS16le)).ToArray();
        Assert.HasCount(decoded.Length, expected);
        Assert.HasCount(decoded.Length, expected16);

        // AC-3 frames overlap, so the first frame decoded after a seek can differ slightly
        // from a continuous decode; compare from the second frame (1536 samples) on.
        const int skipSamples = 1536;
        float maxDiff = 0;
        int maxDiff16 = 0;
        for (int i = skipSamples * channels; i < decoded.Length; i++)
        {
            maxDiff = Math.Max(maxDiff, Math.Abs(decoded[i] - expected[i]));
            int asInt16 = (int)Math.Clamp(MathF.Round(decoded[i] * 32768f), short.MinValue, short.MaxValue);
            maxDiff16 = Math.Max(maxDiff16, Math.Abs(asInt16 - expected16[i]));
        }
        Assert.IsTrue(maxDiff < 1e-4f, $"Decoded float samples differ from the reference by up to {maxDiff}.");
        Assert.IsTrue(maxDiff16 <= 2, $"Decoded samples differ from the 16-bit reference by up to {maxDiff16} LSB.");
    }
}
