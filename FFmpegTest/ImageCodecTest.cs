using FFmpeg;
using FFmpeg.Codecs;
using FFmpeg.Codecs.Image;
using FFmpeg.Formats;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpegTest;

/// <summary>
/// Tests for the image encoder settings: every image is read back with FFmpeg's demuxer and decoder, so the files are
/// complete, and each test checks an effect of a setting in the file. libwebp and libjxl are skipped (inconclusive)
/// when FFmpeg was built without them.
/// </summary>
[TestClass]
public class ImageCodecTest
{
    #region Helpers

    private static void RequireEncoder(bool supported, string name)
    {
        if (!supported)
            Assert.Inconclusive($"FFmpeg was built without {name}.");
    }

    /// <summary>A packed RGB/BGR frame with noise, which only a lossless encoder keeps.</summary>
    private static AVFrame NoiseFrame(PixelFormat format, int width, int height, int seed = 1, bool opaque = true)
    {
        AVFrame frame = AVFrame.Allocate(format, width, height);
        new Random(seed).NextBytes(frame.GetData(0));
        if (opaque && format is PixelFormat.BGRA or PixelFormat.RGBA)
        {
            Span<byte> data = frame.GetData(0);
            for (int row = 0; row < height; row++)
                for (int col = 0; col < width; col++)
                    data[(row * frame.LineSize[0]) + (col * 4) + 3] = 255; // alpha is the 4th byte of BGRA and RGBA
        }
        return frame;
    }

    /// <summary>A colorful, smooth RGB24 image, closer to a photo than noise.</summary>
    private static AVFrame GradientFrame(int width, int height)
    {
        AVFrame frame = AVFrame.Allocate(PixelFormat.RGB24, width, height);
        Span<byte> data = frame.GetData(0);
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int i = (row * frame.LineSize[0]) + (col * 3);
                data[i] = (byte)(col * 255 / (width - 1));
                data[i + 1] = (byte)(row * 255 / (height - 1));
                data[i + 2] = (byte)((col + row) * 255 / (width + height - 2));
            }
        }
        return frame;
    }

    /// <summary>An RGB24 image of flat colored blocks, which lossless compressors shrink well.</summary>
    private static AVFrame BlocksFrame(int width, int height)
    {
        AVFrame frame = AVFrame.Allocate(PixelFormat.RGB24, width, height);
        Span<byte> data = frame.GetData(0);
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int i = (row * frame.LineSize[0]) + (col * 3);
                int block = ((row / 16) * 7) + (col / 16);
                data[i] = (byte)(block * 40);
                data[i + 1] = (byte)(block * 90);
                data[i + 2] = (byte)(block * 150);
            }
        }
        return frame;
    }

    /// <summary>A limited range YUV420P frame like decoded video: luma ramps from 16 to 235.</summary>
    private static AVFrame VideoFrame(int width, int height)
    {
        AVFrame frame = AVFrame.Allocate(PixelFormat.YUV420P, width, height);
        Span<byte> y = frame.GetData(0);
        for (int row = 0; row < height; row++)
            for (int col = 0; col < width; col++)
                y[(row * frame.LineSize[0]) + col] = (byte)(16 + (col * 219 / (width - 1)));
        frame.GetData(1).Fill(128);
        frame.GetData(2).Fill(128);
        frame.ColorRange = ColorRange.MPEG;
        frame.ColorSpace = ColorSpace.BT709;
        return frame;
    }

    /// <summary>Writes the image to a file, opens it with the demuxer and decodes the first frame.</summary>
    private static (AVFrame Frame, string Format) Decode(byte[] image, string extension)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(path, image);
        try
        {
            using DemuxerContext demuxer = DemuxerContext.Open(path, true);
            string format = string.Join(",", demuxer.InputFormat!.Value.ShortNames);
            int index = demuxer.FindBestStream(MediaType.Video);
            Assert.IsTrue(index >= 0, "The file has no image stream.");
            Codec codec = Codec.FindDecoder(demuxer.Streams[index].CodecParameters.CodecId)!.Value;
            using CodecContext decoder = CodecContext.Open(codec, demuxer.Streams[index].CodecParameters);
            using AVPacket packet = AVPacket.Allocate();
            AVFrame frame = AVFrame.Allocate();
            while (!demuxer.ReadPacket(packet).IsError)
            {
                if (packet.StreamIndex == index)
                {
                    decoder.SendPacket(packet).ThrowIfError();
                    if (!decoder.ReceiveFrame(frame).IsError)
                        return (frame, format);
                }
                packet.Unreference();
            }
            _ = decoder.DrainDecoder();
            decoder.ReceiveFrame(frame).ThrowIfError();
            return (frame, format);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Converts a frame to packed RGB24 rows without padding.</summary>
    private static byte[] ToRgb24(AVFrame frame)
    {
        using AVFrame converted = AVFrame.Allocate(PixelFormat.RGB24, frame.Width, frame.Height);
        AVFrame rgb = frame.PixelFormat == PixelFormat.RGB24 ? frame : converted;
        if (rgb == converted)
            SwsContext.Convert(frame, converted, SwsAlgorithm.NearestNeighbor()).ThrowIfError();
        byte[] pixels = new byte[frame.Width * frame.Height * 3];
        for (int row = 0; row < frame.Height; row++)
            rgb.GetData(0).Slice(row * rgb.LineSize[0], frame.Width * 3).CopyTo(pixels.AsSpan(row * frame.Width * 3));
        return pixels;
    }

    private static double MeanAbsoluteDifference(byte[] a, byte[] b)
    {
        Assert.HasCount(a.Length, b);
        long sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += Math.Abs(a[i] - b[i]);
        return (double)sum / a.Length;
    }

    private static void AssertStartsWith(byte[] data, params byte[] magic)
        => CollectionAssert.AreEqual(magic, data.Take(magic.Length).ToArray(), $"The file starts with {Convert.ToHexString(data, 0, Math.Min(8, data.Length))}.");

    #endregion

    [TestMethod]
    public void Png_IsLosslessAndKeepsTheSize()
    {
        using AVFrame source = NoiseFrame(PixelFormat.RGB24, 37, 23);
        byte[] png = new Png().Encode(source);

        AssertStartsWith(png, 0x89, (byte)'P', (byte)'N', (byte)'G');
        (AVFrame decoded, string format) = Decode(png, ".png");
        using (decoded)
        {
            StringAssert.Contains(format, "png");
            Assert.AreEqual(PixelFormat.RGB24, decoded.PixelFormat);
            Assert.AreEqual(37, decoded.Width);
            Assert.AreEqual(23, decoded.Height);
            CollectionAssert.AreEqual(ToRgb24(source), ToRgb24(decoded));
        }
    }

    [TestMethod]
    public void Png_OptionsReachTheEncoder()
    {
        using AVFrame source = GradientFrame(128, 96);
        int stored = new Png { CompressionLevel = 0 }.Encode(source).Length;
        int compressed = new Png { CompressionLevel = 9, Prediction = PngPrediction.Mixed }.Encode(source).Length;
        Assert.IsTrue(compressed < stored / 4, $"level 0: {stored} bytes, level 9: {compressed} bytes");
        Assert.IsTrue(stored > 128 * 96 * 3);

        // 300 dpi = 11811 pixels per meter in the pHYs chunk.
        byte[] png = new Png { Dpi = 300 }.Encode(source);
        int phys = png.AsSpan().IndexOf("pHYs"u8);
        Assert.IsTrue(phys > 0);
        Assert.AreEqual(11811, System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(phys + 4)));
    }

    [TestMethod]
    public void Png_ScalesAndKeepsAlpha()
    {
        using AVFrame source = NoiseFrame(PixelFormat.BGRA, 64, 48, opaque: false);
        (AVFrame decoded, _) = Decode(new Png(32, 24).Encode(source), ".png");
        using (decoded)
        {
            Assert.AreEqual(32, decoded.Width);
            Assert.AreEqual(24, decoded.Height);
            Assert.AreEqual(PixelFormat.RGBA, decoded.PixelFormat);
        }
        // The frame passed in is not changed.
        Assert.AreEqual(PixelFormat.BGRA, source.PixelFormat);
        Assert.AreEqual(64, source.Width);
    }

    [TestMethod]
    public void Jpeg_QScaleAndHuffmanTableReachTheEncoder()
    {
        using AVFrame source = GradientFrame(160, 120);
        byte[] best = new Jpeg { QScale = 2 }.Encode(source);
        byte[] worst = new Jpeg { QScale = 31 }.Encode(source);
        byte[] defaultTables = new Jpeg { QScale = 2, HuffmanTable = JpegHuffmanTable.Default }.Encode(source);

        AssertStartsWith(best, 0xFF, 0xD8);
        CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD9 }, best[^2..]);
        Assert.IsTrue(worst.Length < best.Length / 2, $"q 2: {best.Length} bytes, q 31: {worst.Length} bytes");
        Assert.IsTrue(defaultTables.Length > best.Length);

        (AVFrame decoded, string format) = Decode(best, ".jpg");
        using (decoded)
        {
            StringAssert.Contains(format, "jpeg");
            Assert.AreEqual(160, decoded.Width);
            Assert.IsTrue(MeanAbsoluteDifference(ToRgb24(source), ToRgb24(decoded)) < 3.0);
        }
    }

    [TestMethod]
    public void Jpeg_ConvertsLimitedRangeVideoToFullRange()
    {
        using AVFrame source = VideoFrame(64, 48);
        Jpeg jpeg = new() { QScale = 2, PixelFormat = PixelFormat.YUV420P };
        (AVFrame decoded, _) = Decode(jpeg.Encode(source), ".jpg");
        using (decoded)
        {
            Assert.AreEqual(ColorRange.JPEG, decoded.ColorRange);
            // Luma 16 (black) becomes 0 and 235 (white) becomes 255.
            Span<byte> y = decoded.GetData(0);
            Assert.IsTrue(y[0] < 4);
            Assert.IsTrue(y[63] > 251);
        }
        Assert.AreEqual(ColorRange.MPEG, source.ColorRange);
    }

    [TestMethod]
    public void Tiff_CompressionReachesTheEncoder()
    {
        using AVFrame source = BlocksFrame(128, 96);
        byte[] raw = new Tiff { Compression = TiffCompression.Raw }.Encode(source);
        byte[] deflate = new Tiff { Compression = TiffCompression.Deflate }.Encode(source);

        AssertStartsWith(raw, (byte)'I', (byte)'I', 42, 0);
        Assert.IsTrue(raw.Length > 128 * 96 * 3);
        Assert.IsTrue(deflate.Length < raw.Length / 2, $"raw: {raw.Length} bytes, deflate: {deflate.Length} bytes");
        foreach (byte[] tiff in new[] { raw, deflate })
        {
            (AVFrame decoded, _) = Decode(tiff, ".tif");
            using (decoded)
                CollectionAssert.AreEqual(ToRgb24(source), ToRgb24(decoded));
        }
    }

    [TestMethod]
    public void Tiff_UsesRgbForYuvFrames()
    {
        using AVFrame source = VideoFrame(64, 48);
        (AVFrame decoded, _) = Decode(new Tiff().Encode(source), ".tif");
        using (decoded)
            Assert.AreEqual(PixelFormat.RGB24, decoded.PixelFormat);
    }

    [TestMethod]
    public void Bmp_IsLossless()
    {
        using AVFrame source = NoiseFrame(PixelFormat.BGR24, 33, 17);
        byte[] bmp = new Bmp().Encode(source);
        AssertStartsWith(bmp, (byte)'B', (byte)'M');
        (AVFrame decoded, string format) = Decode(bmp, ".bmp");
        using (decoded)
        {
            StringAssert.Contains(format, "bmp");
            CollectionAssert.AreEqual(ToRgb24(source), ToRgb24(decoded));
        }
    }

    [TestMethod]
    public void Gif_GeneratesAPaletteWithMaxColors()
    {
        using AVFrame source = GradientFrame(96, 64);
        byte[] gif = new Gif().Encode(source);
        AssertStartsWith(gif, "GIF89a"u8.ToArray());
        Assert.AreEqual((byte)0x3B, gif[^1]);

        (int Colors, double Difference) Read(byte[] file)
        {
            (AVFrame decoded, string format) = Decode(file, ".gif");
            using (decoded)
            {
                StringAssert.Contains(format, "gif");
                byte[] rgb = ToRgb24(decoded);
                int colors = Enumerable.Range(0, rgb.Length / 3).Select(i => (rgb[i * 3] << 16) | (rgb[(i * 3) + 1] << 8) | rgb[(i * 3) + 2]).Distinct().Count();
                return (colors, MeanAbsoluteDifference(ToRgb24(source), rgb));
            }
        }

        (int colors, double generated) = Read(gif);
        (int colors16, _) = Read(new Gif { MaxColors = 16, Dither = GifDither.None }.Encode(source));
        (_, double fixedPalette) = Read(new Gif { PixelFormat = PixelFormat.RGB8 }.Encode(source));

        Assert.IsTrue(colors is > 128 and <= 256, $"{colors} colors");
        Assert.IsTrue(colors16 is > 8 and <= 16, $"{colors16} colors with MaxColors = 16");
        Assert.IsTrue(generated < fixedPalette / 2, $"mean difference {generated} with the generated palette, {fixedPalette} with RGB8");
    }

    [TestMethod]
    public void WebP_LosslessAndQualityReachTheEncoder()
    {
        RequireEncoder(WebP.IsSupported, "libwebp");
        using AVFrame source = NoiseFrame(PixelFormat.BGRA, 40, 30);
        byte[] lossless = new WebP { Lossless = true }.Encode(source);
        AssertStartsWith(lossless, (byte)'R', (byte)'I', (byte)'F', (byte)'F');
        CollectionAssert.AreEqual("WEBP"u8.ToArray(), lossless[8..12]);
        (AVFrame decoded, string format) = Decode(lossless, ".webp");
        using (decoded)
        {
            StringAssert.Contains(format, "webp");
            CollectionAssert.AreEqual(ToRgb24(source), ToRgb24(decoded));
        }

        using AVFrame gradient = GradientFrame(128, 96);
        int high = new WebP { Quality = 95, CompressionLevel = 6 }.Encode(gradient).Length;
        int low = new WebP { Quality = 5, Preset = WebPPreset.Photo }.Encode(gradient).Length;
        Assert.IsTrue(low < high, $"quality 95: {high} bytes, quality 5: {low} bytes");
    }

    [TestMethod]
    public void JpegXL_DistanceAndEffortReachTheEncoder()
    {
        RequireEncoder(JpegXL.IsSupported, "libjxl");
        using AVFrame source = NoiseFrame(PixelFormat.RGB24, 40, 30);
        byte[] lossless = new JpegXL { Distance = 0, Effort = 3 }.Encode(source);
        (AVFrame decoded, string format) = Decode(lossless, ".jxl");
        using (decoded)
        {
            StringAssert.Contains(format, "jpegxl");
            CollectionAssert.AreEqual(ToRgb24(source), ToRgb24(decoded));
        }

        using AVFrame gradient = GradientFrame(128, 96);
        int fine = new JpegXL { Distance = 0.5f }.Encode(gradient).Length;
        int coarse = new JpegXL { Distance = 8f }.Encode(gradient).Length;
        Assert.IsTrue(coarse < fine, $"distance 0.5: {fine} bytes, distance 8: {coarse} bytes");
    }

    [TestMethod]
    public void Save_WritesFilesAndStreams()
    {
        using AVFrame source = GradientFrame(32, 32);
        Png png = new() { CompressionLevel = 9 };
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        try
        {
            png.Save(source, path);
            CollectionAssert.AreEqual(png.Encode(source), File.ReadAllBytes(path));
        }
        finally
        {
            File.Delete(path);
        }

        MemoryStream stream = new();
        png.Save(source, stream, closeStream: true);
        Assert.IsFalse(stream.CanWrite);
        CollectionAssert.AreEqual(png.Encode(source), stream.ToArray());
    }

    [TestMethod]
    public void Transcoder_PassesTheQScaleToTheFrames()
    {
        // The Transcoder writes a Motion JPEG video with the settings; mjpeg reads -q:v from the frames.
        long VideoBytes(int qscale)
        {
            string file = Path.Combine(Path.GetTempPath(), $"image-settings-{Guid.NewGuid():N}.mkv");
            try
            {
                using (Transcoder transcoder = Transcoder.Create(TestFiles.Get("mp4-example-video-download-full-hd-1920x1080.1min.mp4"), file))
                {
                    _ = transcoder.TranscodeStream(0, new Jpeg { QScale = qscale }, "scale=160:-2");
                    transcoder.Duration = TimeSpan.FromSeconds(1);
                    transcoder.Run();
                }
                using MediaSource source = MediaSource.Open(file);
                Assert.AreEqual(CodecID.MJPEG, source.Streams[0].CodecParameters.CodecId);
                using AVPacket packet = AVPacket.Allocate();
                long size = 0;
                while (!source.ReadPacket(packet).IsError)
                {
                    if (packet.StreamIndex == 0)
                        size += packet.Size;
                    packet.Unreference();
                }
                return size;
            }
            finally
            {
                File.Delete(file);
            }
        }

        long best = VideoBytes(2);
        long worst = VideoBytes(31);
        Assert.IsTrue(worst < best / 2, $"q 2: {best} bytes, q 31: {worst} bytes");
    }

    [TestMethod]
    public void ToString_PrintsTheOptions()
    {
        Assert.AreEqual("-c:v png -compression_level 9 -pred mixed -dpi 300", new Png { CompressionLevel = 9, Prediction = PngPrediction.Mixed, Dpi = 300 }.ToString());
        Assert.AreEqual("-c:v tiff -compression_algo lzw", new Tiff { Compression = TiffCompression.Lzw }.ToString());
        Assert.AreEqual("-c:v mjpeg -flags +qscale -global_quality 354 -huffman default", new Jpeg { QScale = 3, HuffmanTable = JpegHuffmanTable.Default }.ToString());
        Assert.AreEqual("-c:v gif -global_palette 0", new Gif { GlobalPalette = false }.ToString());
    }
}
