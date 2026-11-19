using FFmpeg.Images;
using FFmpeg.Utils;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFmpegTest;

/// <summary>
/// Tests for <see cref="SwsContext"/>: dynamic contexts (sws_scale_frame without initialization), their color
/// handling, settings and buffer overloads, and the obsolete fixed contexts.
/// </summary>
[TestClass]
public class SwsContextTest
{
    // Limited-range Y=128, U=128, V=170: the expected RGB for the two matrices (R, G, B).
    private static readonly (double R, double G, double B) Bt601 = (197.4, 96.2, 130.4);
    private static readonly (double R, double G, double B) Bt709 = (205.7, 108.0, 130.4);

    #region Helpers

    private static AVFrame Yuv(int width, int height, byte y, byte u, byte v, ColorSpace colorSpace = ColorSpace.Unspecified,
        ColorRange range = ColorRange.MPEG, PixelFormat format = PixelFormat.YUV420P)
    {
        AVFrame frame = AVFrame.Allocate(format, width, height);
        frame.GetData(0).Fill(y);
        frame.GetData(1).Fill(u);
        frame.GetData(2).Fill(v);
        frame.ColorSpace = colorSpace;
        frame.ColorRange = range;
        return frame;
    }

    /// <summary>A destination frame without a buffer.</summary>
    private static AVFrame Empty(int width, int height, PixelFormat format)
    {
        AVFrame frame = AVFrame.Allocate();
        frame.Width = width;
        frame.Height = height;
        frame.PixelFormat = format;
        return frame;
    }

    private static void AssertPixel((double R, double G, double B) expected, ReadOnlySpan<byte> rgb, string what)
    {
        Assert.IsTrue(Math.Abs(rgb[0] - expected.R) <= 3 && Math.Abs(rgb[1] - expected.G) <= 3 && Math.Abs(rgb[2] - expected.B) <= 3,
            $"{what}: expected ~{expected}, got ({rgb[0]}, {rgb[1]}, {rgb[2]})");
    }

    #endregion

    [TestMethod]
    public void Dynamic_ScalesAndConvertsWithTheFrameMatrix()
    {
        using SwsContext context = new();
        Assert.IsTrue(context.IsDynamic);

        using AVFrame bt709 = Yuv(320, 240, 128, 128, 170, ColorSpace.BT709);
        using AVFrame rgb = Empty(160, 120, PixelFormat.RGB24);
        context.Convert(bt709, rgb).ThrowIfError();
        Assert.IsTrue(rgb.HasBuffer, "FFmpeg allocates a buffer for a destination without one.");
        Assert.AreEqual(160, rgb.Width);
        AssertPixel(Bt709, rgb.GetData(0), "BT.709 frame");

        using AVFrame bt601 = Yuv(320, 240, 128, 128, 170, ColorSpace.BT470BG);
        using AVFrame rgb601 = Empty(160, 120, PixelFormat.RGB24);
        context.Convert(bt601, rgb601).ThrowIfError();
        AssertPixel(Bt601, rgb601.GetData(0), "BT.601 frame");

        using AVFrame unspecified = Yuv(320, 240, 128, 128, 170);
        using AVFrame rgbUnspecified = Empty(160, 120, PixelFormat.RGB24);
        context.Convert(unspecified, rgbUnspecified).ThrowIfError();
        AssertPixel(Bt601, rgbUnspecified.GetData(0), "An unspecified matrix is BT.601");
    }

#pragma warning disable CS0618 // fixed contexts are obsolete
    [TestMethod]
    public void Fixed_IgnoresTheFrameMatrix()
    {
        using SwsContext context = new(320, 240, PixelFormat.YUV420P, 160, 120, PixelFormat.RGB24, SwsAlgorithm.Bicubic());
        Assert.IsFalse(context.IsDynamic);
        Assert.AreEqual(320, context.SourceWidth);

        using AVFrame bt709 = Yuv(320, 240, 128, 128, 170, ColorSpace.BT709);
        using AVFrame rgb = AVFrame.Allocate(PixelFormat.RGB24, 160, 120);
        context.Convert(bt709, rgb).ThrowIfError();
        AssertPixel(Bt601, rgb.GetData(0), "The legacy API always uses BT.601");

        byte[] buffer = new byte[160 * 120 * 3];
        Assert.IsFalse(context.Convert(bt709, buffer).IsError, "The fixed overloads keep working.");
        Assert.AreEqual(rgb.GetData(0)[0], buffer[0]);

        _ = Assert.Throws<InvalidOperationException>(() => context.Threads = 2, "Fixed settings cannot change.");
        _ = Assert.Throws<InvalidOperationException>(() => context.Setup(bt709, rgb));
        using AVFrame other = Yuv(64, 48, 128, 128, 128);
        Assert.IsTrue(context.Convert(other, rgb).IsError, "A fixed context only converts its size.");
    }

    [TestMethod]
    public void Dynamic_RejectsTheFixedOverloads()
    {
        using SwsContext context = new();
        using AVFrame frame = Yuv(64, 48, 128, 128, 128);
        byte[] buffer = new byte[64 * 48 * 3];
        _ = Assert.Throws<InvalidOperationException>(() => context.Convert(frame, buffer));
        _ = Assert.Throws<InvalidOperationException>(() => context.Convert(IntPtr.Zero, IntPtr.Zero));
        Assert.AreEqual(0, context.SourceWidth);
        Assert.AreEqual(PixelFormat.None, context.DestinationFormat);
    }

    [TestMethod]
    public void CheckContext_KeepsDynamicContexts()
    {
        using SwsContext context = new();
        using AVFrame src = Yuv(64, 48, 128, 128, 128);
        using AVFrame dst = AVFrame.Allocate(PixelFormat.RGB24, 32, 24);
        Assert.AreSame(context, SwsContext.CheckContext(context, src, dst));
    }
#pragma warning restore CS0618

    [TestMethod]
    public void Dynamic_KeepsTheColorsWhenOnlyResizing()
    {
        // The destination leaves its color properties unspecified, so they are taken from the source (like
        // -vf scale): the chroma must stay the same instead of being converted from BT.709 to BT.601.
        using SwsContext context = new();
        using AVFrame src = Yuv(320, 240, 128, 128, 170, ColorSpace.BT709);
        src.ColorPrimaries = ColorPrimaries.BT709;
        src.ColorTransferCharacteristic = ColorTransferCharacteristic.BT709;
        src.ChromaLocation = ChromaLocation.Left;
        using AVFrame dst = Empty(160, 120, PixelFormat.YUV420P);
        context.Convert(src, dst).ThrowIfError();

        Assert.AreEqual(ColorSpace.BT709, dst.ColorSpace);
        Assert.AreEqual(ColorRange.MPEG, dst.ColorRange);
        Assert.AreEqual(ColorPrimaries.BT709, dst.ColorPrimaries);
        Assert.AreEqual(ColorTransferCharacteristic.BT709, dst.ColorTransferCharacteristic);
        Assert.AreEqual(ChromaLocation.Left, dst.ChromaLocation);
        Assert.IsTrue(Math.Abs(dst.GetData(0)[0] - 128) <= 1);
        Assert.IsTrue(Math.Abs(dst.GetData(2)[0] - 170) <= 1, $"V changed to {dst.GetData(2)[0]}.");

        // A destination that asks for another matrix gets converted.
        using AVFrame bt601 = Empty(160, 120, PixelFormat.YUV420P);
        bt601.ColorSpace = ColorSpace.BT470BG;
        context.Convert(src, bt601).ThrowIfError();
        Assert.AreEqual(ColorSpace.BT470BG, bt601.ColorSpace);
        // BT.709 (128, 128, 170) is RGB (205.7, 108.0, 130.4), which is BT.601 Y 136, U 123.5, V 169.
        Assert.IsTrue(Math.Abs(bt601.GetData(0)[0] - 136) <= 1, $"BT.709 to BT.601 changes Y to 136, got {bt601.GetData(0)[0]}.");
        Assert.IsTrue(Math.Abs(bt601.GetData(1)[0] - 123.5) <= 1, $"BT.709 to BT.601 changes U to 123.5, got {bt601.GetData(1)[0]}.");
    }

    [TestMethod]
    public void Dynamic_ConvertsFullToLimitedRange()
    {
        using SwsContext context = new();
        using AVFrame full = Yuv(64, 48, 255, 128, 128, ColorSpace.BT709, ColorRange.JPEG);
        using AVFrame limited = Empty(64, 48, PixelFormat.YUV420P);
        limited.ColorRange = ColorRange.MPEG;
        context.Convert(full, limited).ThrowIfError();
        Assert.AreEqual(235, limited.GetData(0)[0], "Full-range white is 235 in limited range.");
    }

    [TestMethod]
    public void Dynamic_ToneMapsHdrWhenTheDestinationAsksForSdr()
    {
        using SwsContext context = new();
        using AVFrame hdr = AVFrame.Allocate(PixelFormat.YUV420P10LE, 64, 48);
        MemoryMarshal.Cast<byte, ushort>(hdr.GetData(0)).Fill(700);
        MemoryMarshal.Cast<byte, ushort>(hdr.GetData(1)).Fill(512);
        MemoryMarshal.Cast<byte, ushort>(hdr.GetData(2)).Fill(560);
        hdr.ColorSpace = ColorSpace.BT2020_NCL;
        hdr.ColorRange = ColorRange.MPEG;
        hdr.ColorPrimaries = ColorPrimaries.BT2020;
        hdr.ColorTransferCharacteristic = ColorTransferCharacteristic.SMPTE2084;

        using AVFrame pq = Empty(64, 48, PixelFormat.RGB24);
        context.Convert(hdr, pq).ThrowIfError();
        Assert.AreEqual(ColorTransferCharacteristic.SMPTE2084, pq.ColorTransferCharacteristic, "Without a request the transfer is kept.");

        using AVFrame sdr = Empty(64, 48, PixelFormat.RGB24);
        sdr.ColorPrimaries = ColorPrimaries.BT709;
        sdr.ColorTransferCharacteristic = ColorTransferCharacteristic.BT709;
        context.Convert(hdr, sdr).ThrowIfError();
        Span<byte> a = pq.GetData(0), b = sdr.GetData(0);
        Assert.IsTrue(Math.Abs(a[0] - b[0]) + Math.Abs(a[1] - b[1]) + Math.Abs(a[2] - b[2]) > 10,
            $"PQ ({a[0]},{a[1]},{a[2]}) and tone-mapped SDR ({b[0]},{b[1]},{b[2]}) must differ.");
    }

    [TestMethod]
    public void Dynamic_NoOpReferencesTheSource()
    {
        using SwsContext context = new();
        using AVFrame src = Yuv(64, 48, 100, 128, 128);
        using AVFrame dst = Empty(64, 48, PixelFormat.YUV420P);
        Assert.IsTrue(SwsContext.IsNoOp(src, dst), "Unspecified destination colors count as the source's.");
        Assert.AreEqual(ColorRange.Unspecified, dst.ColorRange, "IsNoOp does not change the frame.");
        using AVFrame rgb = Empty(64, 48, PixelFormat.RGB24);
        Assert.IsFalse(SwsContext.IsNoOp(src, rgb));
        context.Convert(src, dst).ThrowIfError();
        Assert.IsTrue(Unsafe.AreSame(ref MemoryMarshal.GetReference(src.GetData(0)), ref MemoryMarshal.GetReference(dst.GetData(0))),
            "A no-op into a frame without buffer references the source.");

        using AVFrame copy = AVFrame.Allocate(PixelFormat.YUV420P, 64, 48);
        context.Convert(src, copy).ThrowIfError();
        Assert.IsFalse(Unsafe.AreSame(ref MemoryMarshal.GetReference(src.GetData(0)), ref MemoryMarshal.GetReference(copy.GetData(0))));
        Assert.AreEqual(100, copy.GetData(0)[0], "A no-op into a provided buffer copies.");
    }

    [TestMethod]
    public void Dynamic_OneContextServesChangingFormats()
    {
        using SwsContext context = new(SwsAlgorithm.Bilinear());
        (int W, int H, PixelFormat Src, int DW, int DH, PixelFormat Dst)[] cases =
        [
            (320, 240, PixelFormat.YUV420P, 160, 120, PixelFormat.RGB24),
            (640, 480, PixelFormat.YUV422P, 100, 100, PixelFormat.BGRA),
            (64, 48, PixelFormat.YUV444P, 128, 96, PixelFormat.YUV420P),
            (320, 240, PixelFormat.YUV420P, 160, 120, PixelFormat.RGB24),
        ];
        foreach ((int w, int h, PixelFormat src, int dw, int dh, PixelFormat dst) in cases)
        {
            using AVFrame input = Yuv(w, h, 128, 128, 128, format: src);
            using AVFrame output = Empty(dw, dh, dst);
            context.Convert(input, output).ThrowIfError();
            Assert.AreEqual(dw, output.Width);
            Assert.AreEqual(dst, output.PixelFormat);
        }
    }

    [TestMethod]
    public void Dynamic_ScalesInterlacedFramesIntoProgressiveOnes()
    {
        // sws_scale_frame refuses interlaced -> progressive; like the scale filter, both are scaled as progressive.
        using SwsContext context = new();
        using AVFrame src = Yuv(64, 48, 128, 128, 128);
        src.Flags |= FrameFlags.Interlaced | FrameFlags.TopFieldFirst;
        using AVFrame dst = Empty(32, 24, PixelFormat.RGB24);
        context.Convert(src, dst).ThrowIfError();
        Assert.IsTrue(src.Flags.HasFlag(FrameFlags.Interlaced), "The source flags are restored.");
        Assert.IsFalse(dst.Flags.HasFlag(FrameFlags.Interlaced));
    }

    [TestMethod]
    public void Settings_CanBeChangedAndAreUsed()
    {
        using SwsContext context = new();
        Assert.AreEqual(SwsFlags.Bicubic, context.Flags);
        Assert.AreEqual(SwsScaler.Auto, context.Scaler);
        Assert.AreEqual(1, context.Threads);
        Assert.AreEqual(SwsIntent.RelativeColorimetric, context.Intent);

        context.Algorithm = SwsAlgorithm.Lanczos(5);
        Assert.AreEqual(SwsFlags.Lanczos, context.Flags);
        Assert.AreEqual(5, context.ScalerParam1);
        context.Flags |= SwsFlags.AccurateRounding;
        context.Algorithm = SwsAlgorithm.NearestNeighbor();
        Assert.AreEqual(SwsFlags.NearestNeighbor | SwsFlags.AccurateRounding, context.Flags, "Algorithm only replaces the scaler bits.");
        Assert.AreEqual(SwsAlgorithm.NearestNeighbor(), context.Algorithm);

        context.Scaler = SwsScaler.Lanczos;
        context.ChromaScaler = SwsScaler.Bilinear;
        context.Threads = 0;
        context.Dither = SwsDither.ErrorDiffusion;
        context.AlphaBlend = SwsAlphaBlend.Checkerboard;
        context.GammaCorrect = true;
        context.Intent = SwsIntent.Perceptual;
        context.Backends = SwsBackend.Legacy;
        Assert.AreEqual(SwsScaler.Lanczos, context.Scaler);
        Assert.AreEqual(SwsScaler.Bilinear, context.ChromaScaler);
        Assert.AreEqual(SwsDither.ErrorDiffusion, context.Dither);
        Assert.AreEqual(SwsAlphaBlend.Checkerboard, context.AlphaBlend);
        Assert.IsTrue(context.GammaCorrect);
        Assert.AreEqual(SwsIntent.Perceptual, context.Intent);
        Assert.AreEqual(SwsBackend.Legacy, context.Backends);
        Assert.IsFalse(context.TryGetOption("scaler", out long scaler).IsError);
        Assert.AreEqual((long)SwsScaler.Lanczos, scaler, "The properties are the AVOptions.");

        using AVFrame src = Yuv(64, 48, 128, 128, 128);
        using AVFrame dst = Empty(32, 24, PixelFormat.RGB24);
        context.Convert(src, dst).ThrowIfError();

        context.Dither = (SwsDither)99;
        using AVFrame dst2 = Empty(32, 24, PixelFormat.RGB24);
        Assert.AreEqual<AVResult32>(AVResult32.InvalidArgument, context.Convert(src, dst2), "FFmpeg validates the settings.");
    }

    [TestMethod]
    public void Scaler_OverridesTheAlgorithm()
    {
        static byte[] Upscale(SwsScaler scaler)
        {
            using SwsContext context = new(SwsAlgorithm.Bicubic());
            context.Scaler = scaler;
            using AVFrame src = AVFrame.Allocate(PixelFormat.Gray8, 4, 4);
            Span<byte> data = src.GetData(0);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    data[(y * src.LineSize[0]) + x] = (byte)((x + y) % 2 == 0 ? 0 : 255);
            using AVFrame dst = AVFrame.Allocate(PixelFormat.Gray8, 16, 16);
            context.Convert(src, dst).ThrowIfError();
            byte[] values = new byte[16 * 16];
            for (int y = 0; y < 16; y++)
                dst.GetData(0).Slice(y * dst.LineSize[0], 16).CopyTo(values.AsSpan(y * 16));
            return values;
        }

        Assert.IsTrue(Upscale(SwsScaler.Point).All(v => v is 0 or 255), "Point sampling only repeats pixels.");
        Assert.IsTrue(Upscale(SwsScaler.Bilinear).Any(v => v is > 10 and < 245), "Bilinear interpolates.");
    }

    [TestMethod]
    public void Buffers_MatchTheFramePath()
    {
        using SwsContext context = new();
        using AVFrame src = Yuv(64, 48, 128, 128, 170);
        ImageInfo rgbInfo = new(32, 24, PixelFormat.RGB24);

        using AVFrame expected = Empty(32, 24, PixelFormat.RGB24);
        context.Convert(src, expected).ThrowIfError();

        byte[] buffer = new byte[rgbInfo.BufferSize];
        context.Convert(src, buffer, rgbInfo).ThrowIfError();
        AssertPixel(Bt601, buffer, "frame -> span");
        Assert.AreEqual(expected.GetData(0)[30], buffer[30]);

        using Image image = Image.Create(rgbInfo);
        context.Convert(src, image).ThrowIfError();
        Assert.AreEqual(buffer[30], image.GetPlane(0)[30], "frame -> image");

        using Image yuvImage = Image.Create(new ImageInfo(32, 24, PixelFormat.YUV420P));
        context.Convert(image, yuvImage).ThrowIfError();
        Assert.IsTrue(Math.Abs(yuvImage.GetPlane(2)[0] - 170) <= 2, $"image -> image round trip V = {yuvImage.GetPlane(2)[0]}");

        using AVFrame back = Empty(32, 24, PixelFormat.YUV420P);
        context.Convert(buffer, rgbInfo, back).ThrowIfError();
        Assert.IsTrue(Math.Abs(back.GetData(2)[0] - 170) <= 2, "span -> frame");

        Assert.AreEqual<AVResult32>(AVResult32.InvalidArgument, context.Convert(src, new byte[10], rgbInfo), "A span that is too small.");
        Assert.IsFalse(SwsContext.Convert(src, image, SwsAlgorithm.Bicubic()).IsError, "The static helpers use dynamic contexts.");
    }

    [TestMethod]
    public void InvalidFrames_ReturnAnErrorInsteadOfCrashing()
    {
        using SwsContext context = new();
        using AVFrame src = Yuv(64, 48, 128, 128, 128);
        using AVFrame noFormat = AVFrame.Allocate();
        noFormat.Width = 32;
        noFormat.Height = 24;
        Assert.AreEqual<AVResult32>(AVResult32.InvalidArgument, context.Convert(src, noFormat));
        Assert.AreEqual<AVResult32>(AVResult32.InvalidArgument, context.Convert(noFormat, src));
    }

    [TestMethod]
    public void Capabilities_AreReported()
    {
        Assert.IsTrue(SwsContext.IsSupportedInput(PixelFormat.YUV420P));
        Assert.IsTrue(SwsContext.IsSupportedOutput(PixelFormat.RGB24));
        Assert.IsTrue(SwsContext.IsSupportedInput(PixelFormat.BAYER_BGGR8));
        Assert.IsFalse(SwsContext.IsSupportedOutput(PixelFormat.BAYER_BGGR8), "Bayer is input only.");
        Assert.IsTrue(SwsContext.IsSupported(ColorSpace.BT709, output: true));
        Assert.IsTrue(SwsContext.IsSupported(ColorPrimaries.BT2020, output: false));
        Assert.IsTrue(SwsContext.IsSupported(ColorTransferCharacteristic.SMPTE2084, output: false));

        using AVFrame src = Yuv(64, 48, 128, 128, 128);
        Assert.IsTrue(SwsContext.IsSupported(src, output: false));

        using SwsContext context = new();
        using AVFrame rgb = Empty(32, 24, PixelFormat.RGB24);
        Assert.IsFalse(context.Setup(src, rgb).IsError);
        Assert.IsFalse(rgb.HasBuffer, "Setup does not convert.");
        using AVFrame bayer = Empty(32, 24, PixelFormat.BAYER_BGGR8);
        Assert.IsTrue(context.Setup(src, bayer).IsError, "Setup reports an unsupported conversion.");
    }

    [TestMethod]
    public void Dispose_CanBeCalledTwice()
    {
        SwsContext context = new();
        using AVFrame src = Yuv(64, 48, 128, 128, 128);
        context.Convert(src, new byte[32 * 24 * 3], new ImageInfo(32, 24, PixelFormat.RGB24)).ThrowIfError();
        context.Dispose();
        context.Dispose();
        _ = Assert.Throws<ObjectDisposedException>(() => context.Threads);
    }
}
