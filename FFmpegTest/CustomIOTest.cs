using FFmpeg.Formats;
using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpegTest;

/// <summary>
/// Tests for demuxing and muxing through .NET streams (IOStreamContext), which run
/// FFmpeg's I/O through managed read/write/seek callbacks.
/// </summary>
[TestClass]
public class CustomIOTest
{
    private static string VideoFile => TestFiles.Get("mp4-example-video-download-full-hd-1920x1080.1min.mp4");

    /// <summary>Packet count and total payload size per stream.</summary>
    private static (int Packets, long Bytes)[] ReadAllPackets(DemuxerContext demuxer)
    {
        (int Packets, long Bytes)[] result = new (int, long)[demuxer.StreamCount];
        using AVPacket packet = AVPacket.Allocate();
        AVResult32 res;
        while (!(res = demuxer.ReadPacket(packet)).IsError)
        {
            result[packet.StreamIndex].Packets++;
            result[packet.StreamIndex].Bytes += packet.Size;
        }
        Assert.AreEqual(AVResult32.EndOfFile, (int)res, $"Reading stopped with {res} instead of end of file.");
        return result;
    }

    [TestMethod]
    public void DemuxFromStream_MatchesDemuxFromFile()
    {
        (int, long)[] expected;
        long expectedDuration;
        using (DemuxerContext fromFile = DemuxerContext.Open(VideoFile, true))
        {
            expectedDuration = fromFile.Duration;
            expected = ReadAllPackets(fromFile);
        }

        // The MP4 index (moov) is at the end of the file, so this also exercises seeking.
        using DemuxerContext fromStream = DemuxerContext.Open(File.OpenRead(VideoFile), true);
        Assert.AreEqual(expected.Length, fromStream.StreamCount);
        Assert.AreEqual(expectedDuration, fromStream.Duration);
        CollectionAssert.AreEqual(expected, ReadAllPackets(fromStream));
    }

    [TestMethod]
    public void DemuxFromStream_SurvivesGarbageCollection()
    {
        // The IO callbacks must stay valid however often the GC runs while FFmpeg reads.
        using DemuxerContext demuxer = DemuxerContext.Open(File.OpenRead(VideoFile), true);
        using AVPacket packet = AVPacket.Allocate();
        for (int i = 0; i < 200 && !demuxer.ReadPacket(packet).IsError; i++)
        {
            if (i % 20 == 0)
            {
                GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
            }
        }
        Assert.AreEqual(Seekable.Byte, demuxer.Seekable);
    }

    [TestMethod]
    public void DemuxFromInvalidStream_ThrowsWithoutCrashing()
    {
        // avformat_open_input frees the format context when it fails; disposing the
        // wrapper afterwards must not touch the freed memory.
        Exception? error = null;
        try
        {
            using DemuxerContext demuxer = DemuxerContext.Open(new MemoryStream(), false);
        }
        catch (Exception e)
        {
            error = e;
        }
        Assert.IsNotNull(error, "Opening an empty stream should fail.");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    [TestMethod]
    public void RemuxToMemoryStream_RoundTrips()
    {
        (int Packets, long Bytes)[] source;
        using (DemuxerContext input = DemuxerContext.Open(VideoFile, true))
            source = ReadAllPackets(input);

        byte[] remuxed = Remux(OutputFormat.FindFormat("matroska")!.Value);
        Assert.IsTrue(remuxed.Length > 0);

        using DemuxerContext reopened = DemuxerContext.Open(new MemoryStream(remuxed), true);
        Assert.AreEqual(source.Length, reopened.StreamCount);
        (int Packets, long Bytes)[] roundTripped = ReadAllPackets(reopened);
        for (int i = 0; i < source.Length; i++)
        {
            Assert.AreEqual(source[i].Packets, roundTripped[i].Packets, $"Packet count of stream {i}");
            Assert.AreEqual(source[i].Bytes, roundTripped[i].Bytes, $"Payload size of stream {i}");
        }
    }

    [TestMethod]
    public void ReadFromNonSeekableStream()
    {
        byte[] remuxed = Remux(OutputFormat.FindFormat("matroska")!.Value);

        // Matroska can be read front to back, so a stream without seeking must work.
        using DemuxerContext demuxer = DemuxerContext.Open(new NonSeekableStream(new MemoryStream(remuxed)), true);
        Assert.AreEqual(Seekable.None, demuxer.Seekable);
        (int Packets, long Bytes)[] packets = ReadAllPackets(demuxer);
        Assert.IsTrue(packets.All(p => p.Packets > 0));
    }

    [TestMethod]
    public void DisposeTwice_IsSafe()
    {
        DemuxerContext demuxer = DemuxerContext.Open(File.OpenRead(VideoFile), false);
        demuxer.Dispose();
        demuxer.Dispose();

        MuxerContext muxer = MuxerContext.Open(new MemoryStream(), OutputFormat.FindFormat("matroska")!.Value)!;
        muxer.Dispose();
        muxer.Dispose();
    }

    /// <summary>Copies all packets of the test video into a new container in memory.</summary>
    private static byte[] Remux(OutputFormat format)
    {
        MemoryStream output = new();
        using (DemuxerContext input = DemuxerContext.Open(VideoFile, true))
        using (MuxerContext muxer = MuxerContext.Open(output, format, closeOnDispose: false)!)
        {
            foreach (AVStream stream in input.Streams)
            {
                AVStream outStream = muxer.AddStream(stream);
                // Let the muxer choose its own tag (the struct points at the native parameters).
                FFmpeg.Codecs.CodecParameters_ref parameters = outStream.CodecParameters;
                parameters.CodecTag = default;
            }
            muxer.WriteHeader().ThrowIfError();

            using AVPacket packet = AVPacket.Allocate();
            while (!input.ReadPacket(packet).IsError)
            {
                // The muxer may have changed the time base in WriteHeader.
                packet.RescaleTS(muxer.Streams[packet.StreamIndex].TimeBase);
                muxer.WritePacketInterleaved(packet).ThrowIfError();
            }
            muxer.WriteTrailer().ThrowIfError();
            muxer.Flush();
        }
        return output.ToArray();
    }

    /// <summary>A read-only stream wrapper that hides seeking.</summary>
    private sealed class NonSeekableStream(System.IO.Stream inner) : System.IO.Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
