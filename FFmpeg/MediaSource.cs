using FFmpeg.Codecs;
using FFmpeg.Collections;
using FFmpeg.Formats;
using FFmpeg.HW;
using FFmpeg.Utils;

namespace FFmpeg;
/// <summary>
/// Represents a media source that handles reading and decoding of media data from various input sources.
/// </summary>
public class MediaSource : IDisposable
{
    /// <summary>
    /// Gets the format context associated with this media source.
    /// </summary>
    public FormatContext FormatContext { get; private set; }
    private readonly AVPacket packet = new() { StreamIndex = -1 };
    private readonly DeviceType hwType = DeviceType.None;


    /// <summary>
    /// Gets the list of streams available in this media source.
    /// </summary>
    public IReadOnlyList<AVStream> Streams => FormatContext.Streams;

    private CodecContext[] codecContexts = [];

    /// <summary>
    /// A function to retrieve a codec by its ID. This can be set to provide custom codec retrieval logic.
    /// </summary>
    public Func<AVStream, Codec?>? GetCodec;

    /// <summary>
    /// Gets the list of codec contexts for the streams in this media source.
    /// </summary>
    /// <remarks>
    /// This property initializes codec contexts lazily based on the number of streams. It also sets up codec contexts
    /// for each stream, guessing the frame rate for video streams and adjusting codec parameters as needed.
    /// </remarks>
    public IReadOnlyList<CodecContext> CodecContexts
    {
        get
        {
            if (codecContexts.Length != Streams.Count)
            {
                if (this.codecContexts.Length == Streams.Count)
                    return this.codecContexts;
                CodecContext[] codecContexts = new CodecContext[Streams.Count];
                int old = this.codecContexts.Length;
                Array.Resize(ref codecContexts, Streams.Count);
                for (int i = old; i < Streams.Count; i++)
                {
                    Codec hwCodec = GetCodec?.Invoke(Streams[i]) ?? Codec.FindDecoder(Streams[i].CodecParameters.CodecId)!.Value;
                    codecContexts[i] = CodecContext.Allocate(hwCodec);
                    codecContexts[i].SetCodecParameters(Streams[i].CodecParameters);
                    _ = codecContexts[i].SetHWDeviceType(hwType);
                    codecContexts[i].PacketTimeBase = Streams[i].TimeBase;
                    if (codecContexts[i].CodecType == MediaType.Video)
                        codecContexts[i].FrameRate = FormatContext.GuessFrameRate(Streams[i], null);
                }
                this.codecContexts = codecContexts;
            }
            return codecContexts;
        }
    }

    public CodecContext SetCodec(Codec codec, int streamIndex)
    {
        if (codecContexts.Length != Streams.Count)
            _ = CodecContexts;
        codecContexts[streamIndex].Dispose();
        codecContexts[streamIndex] = CodecContext.Allocate(codec);
        codecContexts[streamIndex].SetCodecParameters(Streams[streamIndex].CodecParameters);
        _ = codecContexts[streamIndex].SetHWDeviceType(hwType);
        codecContexts[streamIndex].PacketTimeBase = Streams[streamIndex].TimeBase;
        if (codecContexts[streamIndex].CodecType == MediaType.Video)
            codecContexts[streamIndex].FrameRate = FormatContext.GuessFrameRate(Streams[streamIndex], null);
        return codecContexts[streamIndex];
    }

    public AVDictionary_ref Metadata => FormatContext.Metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSource"/> class with a specified format context and device type.
    /// </summary>
    /// <param name="context">The format context to associate with this media source.</param>
    /// <param name="deviceType">The device type for hardware-accelerated decoding.</param>
    public MediaSource(FormatContext context, DeviceType deviceType)
    {
        FormatContext = context;
        hwType = deviceType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSource"/> class with a specified format context.
    /// </summary>
    /// <param name="context">The format context to associate with this media source.</param>
    public MediaSource(FormatContext context)
    {
        FormatContext = context;
        hwType = DeviceType.None;
    }

    /// <summary>
    /// Opens a media source from a URL.
    /// </summary>
    /// <param name="url">The URL of the media source.</param>
    /// <param name="format">Optional input format.</param>
    /// <param name="options">Optional options for opening the media source.</param>
    /// <param name="deviceType">Optional device type for hardware-accelerated decoding.</param>
    /// <returns>A new <see cref="MediaSource"/> instance if successful; otherwise, <see langword="null"/>.</returns>
    public static MediaSource Open(string url, InputFormat? format = null, IDictionary<string, string>? options = null, DeviceType deviceType = DeviceType.None)
    {
        FormatContext input = FormatContext.OpenInput(url, format, options, true);
        return new MediaSource(input, deviceType);
    }

    /// <summary>
    /// Opens a media source from a stream.
    /// </summary>
    /// <param name="stream">The stream representing the media source.</param>
    /// <param name="format">Optional input format.</param>
    /// <param name="options">Optional options for opening the media source.</param>
    /// <param name="deviceType">Optional device type for hardware-accelerated decoding.</param>
    /// <returns>A new <see cref="MediaSource"/> instance if successful; otherwise, <see langword="null"/>.</returns>
    public static MediaSource Open(Stream stream, InputFormat? format = null, IDictionary<string, string>? options = null, DeviceType deviceType = DeviceType.None)
    {
        FormatContext? input = FormatContext.OpenInput(stream, format, options, true);
        return new MediaSource(input, deviceType);
    }

    /// <summary>
    /// Opens a media source from an I/O context.
    /// </summary>
    /// <param name="io">The I/O context representing the media source.</param>
    /// <param name="format">Optional input format.</param>
    /// <param name="options">Optional options for opening the media source.</param>
    /// <param name="deviceType">Optional device type for hardware-accelerated decoding.</param>
    /// <returns>A new <see cref="MediaSource"/> instance if successful; otherwise, <see langword="null"/>.</returns>
    public static MediaSource Open(IO.IOContext io, InputFormat? format = null, IDictionary<string, string>? options = null, DeviceType deviceType = DeviceType.None)
    {
        FormatContext input = FormatContext.OpenInput(io, format, options, true);
        return new MediaSource(input, deviceType);
    }

    /// <summary>
    /// Creates a new <see cref="MediaSource"/> instance from an existing format context.
    /// </summary>
    /// <param name="file">The format context representing the media source.</param>
    /// <param name="deviceType">The device type for hardware-accelerated decoding (default is <see cref="DeviceType.None"/>).</param>
    /// <returns>A new <see cref="MediaSource"/> instance.</returns>
    public static MediaSource Open(FormatContext file, DeviceType deviceType = DeviceType.None) => new(file, deviceType);

    /// <summary>
    /// Finds the best stream of the specified media type within this media source.
    /// </summary>
    /// <param name="type">The media type to search for (e.g., audio or video).</param>
    /// <returns>The index of the best stream of the specified type, or -1 if no suitable stream is found.</returns>
    public int FindBestStream(MediaType type) => FormatContext.FindBestStream(type);

    /// <summary>
    /// Reads a packet of data from the media source into the provided <see cref="AVPacket"/> instance.
    /// </summary>
    /// <param name="packet">The <see cref="AVPacket"/> instance to populate with data.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 ReadPacket(AVPacket packet) => FormatContext.ReadFrame(packet);

    /// <summary>
    /// Sends a packet to the codec context and attempts to receive a frame.
    /// </summary>
    /// <param name="srcPacket">The <see cref="AVPacket"/> to send to the codec context.</param>
    /// <param name="dstFrame">The <see cref="AVFrame"/> to receive the decoded frame.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 ReadFrame(AVPacket srcPacket, AVFrame dstFrame)
    {
        AVResult32 res = CodecContexts[srcPacket.StreamIndex].SendPacket(srcPacket);
        return res.IsError ? res : CodecContexts[srcPacket.StreamIndex].ReceiveFrame(dstFrame);
    }

    /// <inheritdoc cref=CodecContext.DecodeSubtitle(AVPacket, out Subtitles.Subtitle) />
    public AVResult32 DecodeSubtitle(AVPacket srcPacket, Subtitles.Subtitle subtitle)
    {
        return codecContexts[srcPacket.StreamIndex].DecodeSubtitle(srcPacket, subtitle);
    }



    /// <summary>
    /// Reads a packet and decodes it into a frame. Continues until a frame is successfully decoded or no more frames can be read.
    /// </summary>
    /// <param name="frame">The <see cref="AVFrame"/> to populate with the decoded frame.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation. Returns the stream index if successful; otherwise, the error result.</returns>
    public AVResult32 ReadAndDecodeAVFrame(AVFrame frame)
    {

        AVResult32 res = 0;
        do
        {
            if (res == AVResult32.TryAgain || packet.StreamIndex == -1)
            {
                res = FormatContext.ReadFrame(packet);

                if (res == AVResult32.EndOfFile)
                {
                    foreach (CodecContext? ctx in codecContexts.Where(ctx => ctx.CodecType is MediaType.Audio or MediaType.Video))
                        _ = ctx.DrainDecoder();
                    for (int i = 0; i < codecContexts.Length; i++)
                    {
                        if (codecContexts[i].CodecType is MediaType.Audio or MediaType.Video)
                        {
                            res = codecContexts[i].ReceiveFrame(frame);
                            if (!res.IsError)
                                return i;
                            else if (res != AVResult32.EndOfFile)
                                return res;

                        }
                    }

                    return AVResult32.EndOfFile;
                }
                res.ThrowIfError();
                // ToDo: decode other media types
                if (Streams[packet.StreamIndex].Discard.HasFlag(DiscardFlags.All) ||
                    (CodecContexts[packet.StreamIndex].CodecType != MediaType.Audio &&
                    CodecContexts[packet.StreamIndex].CodecType != MediaType.Video))
                {
                    res = AVResult32.TryAgain;
                    continue;
                }

                if (packet.Flags.HasFlag(PacketFlags.Discard))
                {
                    res = AVResult32.TryAgain;
                    continue;
                }
                _ = CodecContexts[packet.StreamIndex].SendPacket(packet);
            }
            res = CodecContexts[packet.StreamIndex].ReceiveFrame(frame);
        } while (res == AVResult32.TryAgain);

        return res.IsError ? res : (AVResult32)packet.StreamIndex;
    }


    /// <summary>
    /// Flushes the buffers of all codec contexts, clearing any buffered data.
    /// </summary>
    public void FlushBuffers()
    {
        for (int i = 0; i < CodecContexts.Count; i++)
        {
            CodecContexts[i].FlushBuffers();

            if (CodecContexts[i].HardwareDeviceContext != null)
            { // if hw accel then recreate the context, it seek seems to fail otherwise
                CodecContext @new = CodecContext.Allocate(CodecContexts[i].Codec);
                CodecContexts[i].CopyParameters(@new);
                _ = @new.SetHWDeviceType(hwType);
                @new.PacketTimeBase = CodecContexts[i].PacketTimeBase;
                _ = @new.Open(@new.Codec);
                CodecContexts[i].Dispose();
                codecContexts[i] = @new;

            }
        }

    }


    /// <summary>
    /// Seeks to the specified time position within the media stream.
    /// </summary>
    /// <param name="time">The time position to seek to.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the seek operation.</returns>
    public AVResult32 Seek(TimeSpan time)
    {
        FlushBuffers();
        return FormatContext.Seek(time);
    }

    /// <summary>
    /// Seeks to the specified time position within the media stream for a specific stream index.
    /// </summary>
    /// <param name="time">The time position to seek to.</param>
    /// <param name="streamIndex">The index of the stream to seek within.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the seek operation.</returns>
    public AVResult32 Seek(TimeSpan time, int streamIndex)
    {
        FlushBuffers();
        return FormatContext.Seek(time, streamIndex);
    }

    /// <summary>
    /// Seeks to the specified frame number within the media stream.
    /// </summary>
    /// <param name="frame">The frame number to seek to.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the seek operation.</returns>
    public AVResult32 Seek(long frame)
    {
        FlushBuffers();
        return FormatContext.Seek(frame);
    }

    /// <summary>
    /// Seeks to the specified frame number within the media stream for a specific stream index.
    /// </summary>
    /// <param name="frame">The frame number to seek to.</param>
    /// <param name="streamIndex">The index of the stream to seek within.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the seek operation.</returns>
    public AVResult32 Seek(long frame, int streamIndex)
    {
        FlushBuffers();
        return FormatContext.Seek(frame, streamIndex);
    }

    /// <inheritdoc cref="FormatContext.GuessFrameRate(int)"/>
    public Rational GuessFramerate(int streamIndex) => FormatContext.GuessFrameRate(streamIndex);

    /// <inheritdoc cref="FormatContext.GuessFrameRate(AVStream)"/>
    public Rational GuessFramerate(AVStream stream) => FormatContext.GuessFrameRate(stream);

    #region Dispose

    private bool disposedValue;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="MediaSource"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">A boolean indicating whether to release both managed and unmanaged resources (<see langword="true"/>), or only unmanaged resources (<see langword="false"/>).</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                FormatContext.Dispose();
                packet.Dispose();
                foreach (CodecContext cctx in codecContexts)
                    cctx.Dispose();
            }
            disposedValue = true;
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="MediaSource"/> object.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // ToDo: Beschriftung, TryAgain
    public AVResult32 Decode(AVPacket packet, AVFrame frame) => CodecContexts[packet.StreamIndex].Decode(packet, frame);

    #endregion
}


