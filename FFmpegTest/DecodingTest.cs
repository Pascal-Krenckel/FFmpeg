using FFmpeg.Audio;
using FFmpeg.Codecs;
using FFmpeg.Formats;
using FFmpeg.Utils;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FFmpegTest;

[TestClass]
public class DecodingTest
{
    private string jsonFile = @"Test-Files\mp4-example-video-download-full-hd-1920x1080.1min.mp4.json";
   
    private string bgraFile = @"Test-Files\n123.bgra";
    private string yuvFile = @"Test-Files\n123.yuv";
    private string audioFileU16le = @"Test-Files\n123-1123.u16le.pcm";
    private string audioFileF32le = @"Test-Files\n123-1123.f32le.pcm";

    [TestMethod]
    public void CheckFormatAndStreams()
    {
        var ffprobe = FFProbeJson.Parse(File.ReadAllText(jsonFile))!;
        string videoFile = Path.Combine(Path.GetDirectoryName(jsonFile)!,ffprobe.Format.Filename);
        using var file = DemuxerContext.Open(videoFile, true);
        var expectedFormat = InputFormat.FindFormat(ffprobe.Format.FormatName);
        Assert.AreEqual(expectedFormat, file.InputFormat);
        Assert.AreEqual(ffprobe.Format.NbStreams, file.StreamCount);
        Assert.AreEqual(ffprobe.Format.StartTime, (double)(file.StartTime*Rational.TIME_BASE));
        Assert.AreEqual(ffprobe.Format.Duration, (double)(file.Duration*Rational.TIME_BASE));
        Assert.AreEqual(ffprobe.Format.BitRate, file.BitRate);

    }
}
