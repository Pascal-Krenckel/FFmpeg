using FFmpeg.Audio;
using FFmpeg.Collections;
using FFmpeg.IO;
using FFmpeg.MediaPlayer;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpegTest;

/// <summary>
/// Regression tests for bugs found in code review.
/// </summary>
[TestClass]
public class RegressionTest
{
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
    public unsafe void SubtitleRect_SetText_ReplacesAndClears()
    {
        // SubtitleRect has no public constructor; wrap a zeroed native rect directly.
        FFmpeg.AutoGen._AVSubtitleRect* native = (FFmpeg.AutoGen._AVSubtitleRect*)FFmpeg.AutoGen.ffmpeg.av_mallocz((ulong)sizeof(FFmpeg.AutoGen._AVSubtitleRect));
        try
        {
            System.Reflection.ConstructorInfo ctor = typeof(FFmpeg.Subtitles.SubtitleRect).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                [typeof(FFmpeg.AutoGen._AVSubtitleRect*)])!;
            FFmpeg.Subtitles.SubtitleRect rect = (FFmpeg.Subtitles.SubtitleRect)ctor.Invoke([System.Reflection.Pointer.Box(native, typeof(FFmpeg.AutoGen._AVSubtitleRect*))]);

            rect.Text = "first";
            rect.Text = "second, replacing the first";
            Assert.AreEqual("second, replacing the first", rect.Text);
            rect.ASS = "Dialogue: 0,0:00:00.00,0:00:01.00,Default,,0,0,0,,hi";
            rect.ASS = "replaced";
            Assert.AreEqual("replaced", rect.ASS);

            rect.Text = null!;
            rect.ASS = null!;
            Assert.IsTrue(native->text == null && native->ass == null);
        }
        finally
        {
            FFmpeg.AutoGen.ffmpeg.av_freep(&native->text);
            FFmpeg.AutoGen.ffmpeg.av_freep(&native->ass);
            FFmpeg.AutoGen.ffmpeg.av_free(native);
        }
    }

    [TestMethod]
    public unsafe void HardwareContextRef_Empty_ReturnsNull()
    {
        // An empty hw_device_ctx / hw_frames_ctx field, as on any software frame or codec.
        FFmpeg.AutoGen._AVBufferRef* field = null;
        foreach (Type type in new[] { typeof(FFmpeg.HW.DeviceContext_ref), typeof(FFmpeg.HW.FramesContext_ref) })
        {
            System.Reflection.ConstructorInfo ctor = type.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                [typeof(FFmpeg.AutoGen._AVBufferRef**), typeof(bool)])!;
            object reference = ctor.Invoke([System.Reflection.Pointer.Box(&field, typeof(FFmpeg.AutoGen._AVBufferRef**)), true]);
            Assert.IsNull(type.GetMethod("GetReferencedObject")!.Invoke(reference, null), type.Name);

            // Read-only references must not release the owner's context either.
            System.Reflection.TargetInvocationException e = Assert.Throws<System.Reflection.TargetInvocationException>(
                () => type.GetMethod("SetReferencedObject")!.Invoke(reference, [null]));
            _ = Assert.IsInstanceOfType<NotSupportedException>(e.InnerException);
        }
    }

    [TestMethod]
    public void AVBuffer_ReplaceWithIBuffer_UsesNewReference()
    {
        using FFmpeg.Collections.AVBuffer target = FFmpeg.Collections.AVBuffer.Allocate(16);
        using FFmpeg.Collections.AVBuffer source = FFmpeg.Collections.AVBuffer.Allocate(32);
        source.AsSpan().Fill(0x5A);

        target.ReplaceWith((FFmpeg.Collections.IBuffer)source);
        Assert.AreEqual(32, target.Length);
        Assert.AreEqual((byte)0x5A, target[0]);
        Assert.AreEqual(2, source.ReferenceCount);

        using FFmpeg.Collections.AVBuffer other = FFmpeg.Collections.AVBuffer.Allocate(8);
        Assert.IsTrue(other.TryReplaceWith((FFmpeg.Collections.IBuffer)source));
        Assert.AreEqual(32, other.Length);
        Assert.AreEqual(3, source.ReferenceCount);
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
        CircularArray<string> array = [with(3), "a", "b"];
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
        Assert.AreEqual("EndOfFile", ((AVResult64)FFmpeg.AutoGen.ffmpeg.AVERROR_EOF).ToString());
        Assert.AreEqual("OutOfMemory", ((AVResult64)(-12L)).ToString());
    }

    [TestMethod]
    public void IOStreamContext_StreamConstructor_DoesNotThrow()
    {
        using IOStreamContext context = new(new MemoryStream(), false);
        Assert.IsTrue(context.CanSeek);
        Assert.AreEqual(Seekable.Byte, context.Seekable);
        _ = Assert.Throws<ArgumentNullException>(() => new IOStreamContext(null!, false));
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

    [TestMethod]
    public void ChannelLayout_FailedInit_DoesNotKeepFreedMap()
    {
        ChannelLayout layout = new();
        layout.Init(2);
        _ = Assert.Throws<ArgumentException>(() => layout.Init(0));
        Assert.AreEqual(0, layout.Channels);
        layout.Dispose(); // would free the old map a second time
    }

    [TestMethod]
    public void ChannelLayout_CustomName_MustLeaveRoomForTerminator()
    {
        using ChannelLayout layout = new();
        layout.Init(1);
        layout.SetCustomChannelName(0, "fifteen-chars!!");
        _ = Assert.Throws<ArgumentException>(() => layout.SetCustomChannelName(0, "sixteen-chars!!!"));
        Assert.AreEqual("fifteen-chars!!", layout.GetCustomChannelName(0));
    }

    private static float[] ReadPacked(FFmpeg.Audio.AudioBuffer buffer)
    {
        float[] data = new float[buffer.Samples * buffer.Channels];
        _ = buffer.CopyToPacked<float>(data);
        return data;
    }

    [TestMethod]
    public void AudioBuffer_CopyAndMakeWriteable_DoNotShareData()
    {
        float[] samples = [1, 2, 3, 4, 5, 6, 7, 8];
        using FFmpeg.Audio.AudioBuffer source = FFmpeg.Audio.AudioBuffer.Allocate(new AudioBufferInfo(SampleFormat.Float32Planar, 2), 4);
        _ = source.CopyFromPlanar<float>(samples);

        using (FFmpeg.Audio.AudioBuffer copy = source.Copy())
        {
            copy.Clear();
            _ = copy.CopyFromPlanar<float>(new float[8]);
        }
        CollectionAssert.AreEqual(samples, ReadPacked(source));

        using (FFmpeg.Audio.AudioBuffer clone = source.Clone())
        {
            clone.MakeWriteable();
            clone.Clear();
            _ = clone.CopyFromPlanar<float>(new float[8]);
        }
        CollectionAssert.AreEqual(samples, ReadPacked(source));
    }

    [TestMethod]
    public void AudioBuffer_CopyTo_PlanarToPacked()
    {
        float[] samples = [1, 2, 3, 4, 5, 6, 7, 8];
        using FFmpeg.Audio.AudioBuffer planar = FFmpeg.Audio.AudioBuffer.Allocate(new AudioBufferInfo(SampleFormat.Float32Planar, 2), 4);
        _ = planar.CopyFromPlanar<float>(samples);
        using FFmpeg.Audio.AudioBuffer packed = FFmpeg.Audio.AudioBuffer.Allocate(new AudioBufferInfo(SampleFormat.Float32, 2), 4);

        Assert.AreEqual(4, planar.CopyTo(packed));
        CollectionAssert.AreEqual(samples, MemoryMarshal.Cast<byte, float>(packed.AsReadOnlySpanPacked()).ToArray());

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => planar.CopyTo(packed, 5));
        Assert.AreEqual(0, planar.CopyTo(packed, 4));
    }

    [TestMethod]
    public void AudioClock_Pause_DoesNotCountElapsedTimeTwice()
    {
        AudioClock clock = new(48000) { AutoRun = true };
        System.Diagnostics.Stopwatch wall = System.Diagnostics.Stopwatch.StartNew();
        clock.Start();
        Thread.Sleep(100);
        clock.Pause();
        TimeSpan paused = clock.Position;
        wall.Stop();

        Assert.IsTrue(paused <= wall.Elapsed, $"Clock reports {paused} after {wall.Elapsed} of real time.");
        clock.Start();
        Assert.IsTrue(clock.Position - paused < TimeSpan.FromMilliseconds(50));
        clock.Pause();
    }
}
