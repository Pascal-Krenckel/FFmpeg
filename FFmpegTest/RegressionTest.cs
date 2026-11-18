using FFmpeg.Collections;
using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpegTest;

/// <summary>
/// Regression tests for bugs found in code review.
/// </summary>
[TestClass]
public class RegressionTest
{
    [TestInitialize]
    public void FFmpegInit() => FFmpeg.FFmpegLoader.Initialize();

    [TestMethod]
    public void Rational_AddSubtract_LargeDenominators_DoNotOverflow()
    {
        Rational tb = new(1, 90000);
        Assert.AreEqual(new Rational(1, 45000), tb + tb);
        Assert.AreEqual(new Rational(-7, 720000), tb - new Rational(1, 48000));
    }

    [TestMethod]
    public void Rational_IntTimesRational_DoesNotWrap()
    {
        Rational result = 50000 * new Rational(50000, 1);
        Assert.IsTrue(result > Rational.Zero, "int * Rational wrapped around to a negative value.");
    }

    [TestMethod]
    public void TimeSpanDivRational_ExceedsIntRange()
    {
        // 10 h at 1/90000 = 3,240,000,000 ticks, more than int.MaxValue.
        Assert.AreEqual(3_240_000_000L, TimeSpan.FromHours(10) / new Rational(1, 90000));
        Assert.AreEqual(45L, TimeSpan.FromMilliseconds(1.5) / new Rational(1, 30000));
    }

    [TestMethod]
    public void AVPacket_PresentationTime_LongDurations()
    {
        using AVPacket packet = AVPacket.Allocate();
        packet.TimeBase = new Rational(1, 90000);
        packet.PresentationTime = TimeSpan.FromHours(10);
        Assert.AreEqual(3_240_000_000L, packet.PresentationTimestamp);
        packet.DecompressionTime = TimeSpan.FromHours(10);
        Assert.AreEqual(3_240_000_000L, packet.DecompressionTimestamp);
    }

    [TestMethod]
    public void AudioFifo_LayoutConversion_LargerThanOneChunk()
    {
        // More samples than one internal conversion chunk, in both FIFO layouts.
        const int n = 40000;
        float[] left = Enumerable.Range(0, n).Select(i => (float)i).ToArray();
        float[] right = Enumerable.Range(0, n).Select(i => -(float)i - 1).ToArray();
        float[] packed = new float[2 * n];
        for (int i = 0; i < n; i++)
        {
            packed[2 * i] = left[i];
            packed[(2 * i) + 1] = right[i];
        }

        foreach (FFmpeg.Audio.SampleFormat format in new[] { FFmpeg.Audio.SampleFormat.Float32, FFmpeg.Audio.SampleFormat.Float32Planar })
        {
            using FFmpeg.Audio.AudioFifo fifo = new(format, 2);
            Assert.AreEqual(n, (int)fifo.Write<float>(left, right));

            float[] peekedPacked = new float[2 * n];
            Assert.AreEqual(n, (int)fifo.Peek(System.Runtime.InteropServices.MemoryMarshal.AsBytes(peekedPacked.AsSpan())));
            CollectionAssert.AreEqual(packed, peekedPacked);

            const int offset = 5000;
            float[] l = new float[n - offset];
            float[] r = new float[n - offset];
            Assert.AreEqual(n - offset, (int)fifo.Peek<float>(l, r, offset));
            CollectionAssert.AreEqual(left.Skip(offset).ToArray(), l);
            CollectionAssert.AreEqual(right.Skip(offset).ToArray(), r);

            float[] readPacked = new float[2 * n];
            Assert.AreEqual(n, (int)fifo.Read<float>(readPacked.AsSpan()));
            CollectionAssert.AreEqual(packed, readPacked);
        }
    }

    [TestMethod]
    public void Image_GetPlane_UsesImageAlignment()
    {
        // Width 33 is not a multiple of the alignment, so aligned rows are padded.
        FFmpeg.Images.ImageInfo info = new(33, 10, FFmpeg.Images.PixelFormat.YUV420P, 32);
        using FFmpeg.Images.Image image = FFmpeg.Images.Image.Create(info);
        Span<IntPtr> planes = stackalloc IntPtr[4];
        Span<int> lineSizes = stackalloc int[4];
        Assert.AreEqual(3, (int)image.GetPlanes(planes, lineSizes));

        int[] rows = [10, 5, 5];
        for (int p = 0; p < 3; p++)
        {
            Span<byte> plane = image.GetPlane(p);
            unsafe
            {
                fixed (byte* start = plane)
                    Assert.AreEqual(planes[p], (IntPtr)start, $"Start of plane {p}");
            }
            Assert.AreEqual(lineSizes[p] * rows[p], plane.Length, $"Size of plane {p}");
        }
    }

    [TestMethod]
    public void CircularArray_Empty_ContainsAndRemove()
    {
        CircularArray<int> array = [];
        Assert.IsFalse(array.Contains(0));
        Assert.AreEqual(-1, array.IndexOf(0));
        Assert.IsFalse(array.Remove(0));

        array.Add(1);
        array.RemoveAt(0);
        Assert.IsFalse(array.Contains(1));
        Assert.IsFalse(array.Remove(1));
    }

    [TestMethod]
    public void CircularArray_Clear_ReleasesItems()
    {
        CircularArray<string> array = new(3);
        array.Add("a");
        array.Add("b");
        array.RemoveAt(0);
        array.Add("c");
        array.Add("d"); // wraps around
        array.Clear();

        Assert.AreEqual(0, array.Count);
        Assert.IsFalse(array.Contains("c"));
        Assert.IsFalse(array.Contains(null!));

        array.Add("x");
        CollectionAssert.AreEqual(new[] { "x" }, array.ToArray());
    }

    [TestMethod]
    public void AVResult64_ToString_NamesTheActualError()
    {
        Assert.AreEqual("EndOfFile", ((AVResult64)(long)FFmpeg.AutoGen.ffmpeg.AVERROR_EOF).ToString());
        Assert.AreEqual("OutOfMemory", ((AVResult64)(-12L)).ToString());
    }

    [TestMethod]
    public void IOStreamContext_StreamConstructor_DoesNotThrow()
    {
        using IOStreamContext context = new(new MemoryStream(), false);
        Assert.IsTrue(context.CanSeek);
        Assert.AreEqual(Seekable.Byte, context.Seekable);
        _ = Assert.Throws<ArgumentNullException>(() => new IOStreamContext((System.IO.Stream)null!, false));
    }

    [TestMethod]
    public void AVPacket_Size_CannotExceedBuffer()
    {
        using AVPacket packet = AVPacket.Allocate(16);
        packet.Size = 8;
        Assert.AreEqual(8, packet.Data.Length);
        packet.Size = 16;
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => packet.Size = 1 << 20);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => packet.Size = -1);
    }
}
