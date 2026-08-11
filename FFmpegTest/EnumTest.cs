using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Codecs;
using FFmpeg.Images;
using FFmpeg.Logging;
using System.Diagnostics;

namespace FFmpegTest;

[TestClass]
public class EnumTest
{
    [TestInitialize]
    public void FFmpegInit() => FFmpeg.FFmpegLoader.Initialize();

    public static Dictionary<string, string> Match(string[] left, string[] right)
    {
        Dictionary<string, string> dic = [];
        for (int i = 0; i < left.Length; i++)
            dic[left[i]] = BestMatch(left[i], i, right, EqualityComparer<char>.Create((cLeft, cRight) => char.ToUpper(cLeft) == char.ToUpper(cRight)));
        return dic;
    }

    public static int[] MissingValues<TTest, TSource>() where TTest : struct where TSource : struct
    {
        int[] valuesTest = Array.ConvertAll((TTest[])Enum.GetValues(typeof(TTest)), new Converter<TTest, int>((obj) => Convert.ToInt32(obj)));
        int[] valuesSource = Array.ConvertAll((TSource[])Enum.GetValues(typeof(TSource)), new Converter<TSource, int>((obj) => Convert.ToInt32(obj)));

        List<int> values = [];
        foreach (int value in valuesTest)
        {
            if (!valuesSource.Contains(value))
                values.Add(value);
        }

        foreach (int value in valuesSource)
        {
            if (!valuesTest.Contains(value))
                values.Add(value);
        }

        Debug.WriteLineIf(values.Count != 0, $"Missing values for {typeof(TTest).Name} vs {typeof(TSource).Name}: {string.Join(", ", values)}");

        return [.. values];
    }

    public static unsafe void TestEnum<TTest, TSource>() where TTest : unmanaged where TSource : unmanaged
    {
        Assert.AreEqual(sizeof(TSource), sizeof(TTest));
        int[] ms = MissingValues<TTest, TSource>();
        Assert.HasCount(0, ms);
        string[] namesTest = Enum.GetNames(typeof(TTest));
        string[] namesSource = Enum.GetNames(typeof(TSource));
        Assert.HasCount(namesSource.Length, namesTest);

        Array valuesTest = Enum.GetValues(typeof(TTest));
        Array valuesSource = Enum.GetValues(typeof(TSource));

        Assert.HasCount(valuesTest.Length, valuesSource);
        for (int i = 0; i < valuesTest.Length; i++)
            Assert.AreEqual((int)valuesTest.GetValue(i)!, (int)valuesSource.GetValue(i)!);

    }

    [TestMethod]
    public void TestCodecID() => TestEnum<CodecID, _AVCodecID>();

    [TestMethod]
    public void TestAudioChannel() => TestEnum<AudioChannel, _AVChannel>();

    [TestMethod]
    public void TestClassCategory() => TestEnum<ClassCategory, _AVClassCategory>();

    [TestMethod]
    public void TestPixelFormat() => TestEnum<PixelFormat, _AVPixelFormat>();

    [TestMethod]
    public void TestMediaType() => TestEnum<FFmpeg.Utils.MediaType, _AVMediaType>();

    [TestMethod]
    public void TestSampleFormat() => TestEnum<FFmpeg.Audio.SampleFormat, _AVSampleFormat>();

    [TestMethod]
    public void TestHWDeviceType() => TestEnum<FFmpeg.HW.DeviceType, _AVHWDeviceType>();
    [TestMethod]
    public void TestAlphaMode() => TestEnum<FFmpeg.Images.AlphaMode, _AVAlphaMode>();

    [TestMethod]
    public void TestColorPrimaries() => TestEnum<FFmpeg.Images.ColorPrimaries, _AVColorPrimaries>();

    [TestMethod]
    public void TestColorSpace() => TestEnum<FFmpeg.Images.ColorSpace, _AVColorSpace>();

    [TestMethod]
    public void TestChromaLocation() => TestEnum<FFmpeg.Images.ChromaLocation, _AVChromaLocation>();

    [TestMethod]
    public void TestStereo3DType() => TestEnum<FFmpeg.SideData.Stereo3DType, _AVStereo3DType>();

    [TestMethod]
    public void TestFrameSideDataType() => TestEnum<FFmpeg.SideData.FrameSideDataType, _AVFrameSideDataType>();

    [TestMethod]
    public void TestPacketSideDataType() => TestEnum<FFmpeg.SideData.PacketSideDataType, _AVPacketSideDataType>();

    [TestMethod]
    public void TestColorTransferCharacteristic() => TestEnum<FFmpeg.Images.ColorTransferCharacteristic, _AVColorTransferCharacteristic>();

    [TestMethod]
    public void TestFieldOrder() => TestEnum<FFmpeg.Images.FieldOrder, _AVFieldOrder>();

    [TestMethod]
    public void TestAudioServiceType() => TestEnum<FFmpeg.Audio.AudioServiceType, _AVAudioServiceType>();

    private static string BestMatch(string left, int lIndex, string[] array, IEqualityComparer<char>? comparer = null)
    {
        int bestMatch = 0;
        int minDistance = LevenshteinDistance.Compute(left.AsSpan(), array[bestMatch].AsSpan(), comparer) - Math.Abs(left.Length - array[bestMatch].Length);
        for (int i = 1; i < array.Length; i++)
        {
            int dist = LevenshteinDistance.Compute(left.AsSpan(), array[i].AsSpan(), comparer) - Math.Abs(left.Length - array[i].Length);
            if (dist <= minDistance)
            {
                if (dist < minDistance || (Math.Abs(i - lIndex) < Math.Abs(bestMatch - lIndex)))
                {
                    minDistance = dist;
                    bestMatch = i;
                }

            }
        }
        return array[bestMatch];
    }
}
