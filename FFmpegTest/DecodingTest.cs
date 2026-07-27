using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Codecs;
using FFmpeg.Formats;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FFmpegTest;

[TestClass]
public class DecodingTest
{
    private string jsonFile = @"Test-Files\mp4-example-video-download-full-hd-1920x1080.1min.mp4.json";

    private string bgraFile = @"Test-Files\n123.bgra";
    private string yuvFile = @"Test-Files\n123.yuv";
    private string audioFileS16le = @"Test-Files\10s-11s.s16le.pcm";
    private string audioFileF32le = @"Test-Files\10s-11s.f32le.pcm";

    DemuxerContext? demuxerContext;
    FFProbeJson? ffProbe;

    [TestInitialize]
    public void Initialize()
    {
        ffProbe = FFProbeJson.Parse(File.ReadAllText(jsonFile))!;
        string videoFile = Path.Combine(Path.GetDirectoryName(jsonFile)!, ffProbe.Format.Filename);
        demuxerContext = DemuxerContext.Open(videoFile, true);
    }

    [TestCleanup]
    public void Cleanup()
    {
        demuxerContext?.Dispose();
    }

    [TestMethod]
    public void CheckFormatProperties()
    {

        var expectedFormat = InputFormat.FindFormat(ffProbe!.Format.FormatName);
        Assert.AreEqual(expectedFormat, demuxerContext!.InputFormat);
        Assert.AreEqual(ffProbe.Format.NbStreams, demuxerContext.StreamCount);
        Assert.AreEqual(ffProbe.Format.StartTime, (double)(demuxerContext.StartTime * Rational.TIME_BASE));
        Assert.AreEqual(ffProbe.Format.Duration, (double)(demuxerContext.Duration * Rational.TIME_BASE));
        Assert.AreEqual(ffProbe.Format.BitRate, demuxerContext.BitRate);
    }

    [TestMethod]
    public void CheckStreamProperties()
    {
        Assert.HasCount(ffProbe!.Streams.Length, demuxerContext!.Streams);
        foreach (var probeStream in ffProbe.Streams)
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
            Assert.AreEqual(PixelFormat.Parse((probeStream.PixFmt)), avStream.CodecParameters.PixelFormat);
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
        foreach (var stream in context.Streams)
            stream.Discard = DiscardFlags.All;
        context.Streams[index].Discard = DiscardFlags.Default;
    }

    public static int SelectStream(DemuxerContext context, MediaType mediaType)
    {
        int index = context.FindBestStream(mediaType);
        SelectStream(context, index);
        return index;
    }

    public static AVResult32 SeekAndRead(DemuxerContext input,int streamIndex, CodecContext codec, AVFrame frame, TimeSpan seek)
    {
        using var packet = AVPacket.Allocate();
        var result = input.Seek(seek, streamIndex);
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
                if(packet.Flags.HasFlag(PacketFlags.Discard))
                {
                    result = AVResult32.TryAgain;
                    continue;
                }
                result = codec.SendPacket(packet);
                if (result.IsError)
                    return result;
                result = codec.ReceiveFrame(frame);
            } while (result.IsTryAgain);
        } while (!result.IsError && (frame.GetPresentationTimestamp()+frame.Duration) * frame.TimeBase < seek);
        return result;
    }

    [TestMethod]
    public void DecodingYUV420p()
    {
        int frameNr = 123;
        byte[] yuv = File.ReadAllBytes(yuvFile);
        int videoIndex = SelectStream(demuxerContext!,MediaType.Video);
        TimeSpan estimatedPTS = frameNr / demuxerContext!.Streams[videoIndex].RealFrameRate;
        Codec c = Codec.FindDecoder(demuxerContext.Streams[videoIndex].CodecParameters.CodecId)!.Value;
        using CodecContext decoder = CodecContext.Open(c, demuxerContext.Streams[videoIndex].CodecParameters);
        using AVFrame frame = AVFrame.Allocate();

        SeekAndRead(demuxerContext,videoIndex,decoder,frame, estimatedPTS).ThrowIfError();


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
        byte[] bgra = File.ReadAllBytes(bgraFile);
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

}
