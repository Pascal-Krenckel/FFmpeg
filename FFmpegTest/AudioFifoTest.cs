using FFmpeg.Audio;
using System.Runtime.InteropServices;

namespace FFmpegTest;

[TestClass]
public class AudioFifoTest
{


    [TestMethod]
    public void WriteAndReadBytes()
    {
        AudioFifo fifo = new(SampleFormat.UInt8, 2);
        byte[] buffer = new byte[1024];
        byte[] read = new byte[1024];
        Random.Shared.NextBytes(buffer);
        Assert.Throws<NotSupportedException>(() => fifo.Write(buffer, buffer, buffer));
        Assert.Throws<NotSupportedException>(() => fifo.Write(buffer.AsSpan(), buffer.AsSpan(),buffer.AsSpan()));
        Assert.AreEqual(buffer.Length, 2*(int)fifo.Write(buffer));
        Assert.AreEqual(buffer.Length, 2*fifo.Count);
        Assert.AreEqual(2, fifo.Channels);
        Assert.AreEqual(read.Length, 2*(int)fifo.Read(read));
        Assert.AreEqual(0, fifo.Count);

        for (int i = 0; i < buffer.Length; i++)
            Assert.AreEqual(buffer[i], read[i]);
    }

    [TestMethod]
    public void WriteAndReadInts()
    {
        AudioFifo fifo = new(SampleFormat.Int32Planar, 3);
        int[] buffer = new int[3*1024]; // packed to planar;
        int[] readCh1 = new int[1024], readCh2 = new int[1024], readCh3 = new int[1024];
        Random.Shared.NextBytes(MemoryMarshal.AsBytes(buffer.AsSpan()));

        Assert.AreEqual(1024, (int)fifo.Write(buffer));

        Assert.AreEqual(1024, (int)fifo.Peek(readCh1, readCh2, readCh3));

        for (int sample = 0; sample < readCh1.Length; sample++)
        {
            Assert.AreEqual(buffer[sample * 3], readCh1[sample]);
            Assert.AreEqual(buffer[sample * 3 + 1], readCh2[sample]);
            Assert.AreEqual(buffer[sample * 3+2], readCh3[sample]);
        }

        int[,] multiChannelArray = new int[3, 1024];
        Assert.AreEqual(1024, (int)fifo.Read(multiChannelArray));
        for (int sample = 0; sample < readCh1.Length; sample++)
        {
            Assert.AreEqual(buffer[sample * 3], multiChannelArray[0,sample]);
            Assert.AreEqual(buffer[sample * 3 + 1], multiChannelArray[1,sample]);
            Assert.AreEqual(buffer[sample * 3 + 2], multiChannelArray[2,sample]);
        }
    }
}
