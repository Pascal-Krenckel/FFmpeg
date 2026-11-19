using FFmpeg.Formats;

namespace FFmpegTest;

/// <summary>
/// SetOption(name, dictionary) sets one option of type dictionary; SetOptions(dictionary) sets one option per entry.
/// </summary>
[TestClass]
public class OptionDictionaryTest
{
    // The image2 muxer has a dictionary option (protocol_opts) and does not open a file before writing the header.
    private static MuxerContext OpenImage2() => MuxerContext.Open("frame%03d.png", OutputFormat.FindFormat("image2"))!;

    [TestMethod]
    public void SetOption_SetsTheDictionaryOption()
    {
        using MuxerContext muxer = OpenImage2();
        Dictionary<string, string> values = new() { ["user_agent"] = "test", ["timeout"] = "5" };
        muxer.SetOption("protocol_opts", values).ThrowIfError();

        Assert.AreEqual(2, values.Count, "The caller's dictionary is not changed.");
        muxer.TryGetOption("protocol_opts", out IDictionary<string, string> stored).ThrowIfError();
        Assert.AreEqual("test", stored["user_agent"]);
        Assert.AreEqual("5", stored["timeout"]);
        Assert.IsTrue(muxer.TryGetOption("user_agent", out string _).IsError, "The entries are not set as options of their own.");

        using FFmpeg.Collections.AVDictionary native = new(new Dictionary<string, string> { ["a"] = "1" });
        muxer.SetOption("protocol_opts", native).ThrowIfError();
        muxer.TryGetOption("protocol_opts", out stored).ThrowIfError();
        Assert.AreEqual(1, stored.Count);
        Assert.AreEqual(1, native.Count);

        ILookup<string, string> lookup = new[] { ("b", "2") }.ToLookup(kv => kv.Item1, kv => kv.Item2);
        muxer.SetOption("protocol_opts", lookup).ThrowIfError();
        muxer.TryGetOption("protocol_opts", out stored).ThrowIfError();
        Assert.AreEqual("2", stored["b"]);
    }

    [TestMethod]
    public void SetOptions_SetsTopLevelOptionsAndKeepsTheUnusedOnes()
    {
        using MuxerContext muxer = OpenImage2();
        Dictionary<string, string> values = new() { ["start_number"] = "5", ["update"] = "1", ["no_such_option"] = "x" };
        muxer.SetOptions(values).ThrowIfError();

        CollectionAssert.AreEqual(new[] { "no_such_option" }, values.Keys.ToArray(), "Options that were set are removed, like FFmpeg does.");
        muxer.TryGetOption("start_number", out long start).ThrowIfError();
        Assert.AreEqual(5, start);
        muxer.TryGetOption("update", out bool update).ThrowIfError();
        Assert.IsTrue(update);

        ILookup<string, string> lookup = new[] { ("start_number", "7"), ("unknown", "y") }.ToLookup(kv => kv.Item1, kv => kv.Item2);
        muxer.SetOptions(ref lookup).ThrowIfError();
        muxer.TryGetOption("start_number", out start).ThrowIfError();
        Assert.AreEqual(7, start);
        CollectionAssert.AreEqual(new[] { "unknown" }, lookup.Select(g => g.Key).ToArray());
    }

#pragma warning disable CS0618 // the old names still work
    [TestMethod]
    public void ObsoleteOverloads_ForwardToSetOptions()
    {
        using MuxerContext muxer = OpenImage2();
        Dictionary<string, string> values = new() { ["start_number"] = "9" };
        muxer.SetOption(values).ThrowIfError();
        muxer.TryGetOption("start_number", out long start).ThrowIfError();
        Assert.AreEqual(9, start);
        Assert.AreEqual(0, values.Count);
    }
#pragma warning restore CS0618
}
