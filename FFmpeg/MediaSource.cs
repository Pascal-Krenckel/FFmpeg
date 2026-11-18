using FFmpeg.Codecs;
using FFmpeg.Collections;
using FFmpeg.Formats;
using FFmpeg.HW;
using FFmpeg.IO;
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
    public DemuxerContext FormatContext { get; private set; }

    /// <inheritdoc cref="FormatContext.Seekable"/>
    public Seekable Seekable => FormatContext.Seekable;
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
                // Grow the existing array so the decoders of earlier streams are kept
                // (streams can appear mid-file, e.g. in MPEG-TS).
                CodecContext[] codecContexts = this.codecContexts;
                int old = codecContexts.Length;
                Array.Resize(ref codecContexts, Streams.Count);
                for (int i = old; i < Streams.Count; i++)
                {
                    // Streams without a decoder (data, attachments, unknown codecs) get a context
                    // without a codec instead of throwing for every caller of CodecContexts.
                    Codec? decoder = GetCodec?.Invoke(Streams[i]) ?? Codec.FindDecoder(Streams[i].CodecParameters.CodecId);
                    codecContexts[i] = CodecContext.Allocate(decoder);
                    codecContexts[i].SetCodecParameters(Streams[i].CodecParameters);
                    _ = codecContexts[i].SetHWDeviceType(hwType);
                    codecContexts[i].PacketTimeBase = Streams[i].TimeBase;
                    codecContexts[i].TimeBase = Streams[i].TimeBase;
                    if (codecContexts[i].CodecType == MediaType.Video)
                        codecContexts[i].FrameRate = FormatContext.GuessFrameRate(Streams[i], null);
                }
                this.codecContexts = codecContexts;
            }
            return codecContexts;
        }
    }

    /// <summary>
    /// Replaces the codec context for the specified stream with a new context using
    /// the specified codec.
    /// </summary>
    /// <param name="codec">
    /// The codec to use for the new codec context.
    /// </param>
    /// <param name="streamIndex">
    /// The zero-based index of the stream whose codec context should be replaced.
    /// </param>
    /// <returns>
    /// The newly created <see cref="CodecContext"/>.
    /// </returns>
    /// <remarks>
    /// The existing codec context for the stream, if any, is disposed before the
    /// new context is created.
    ///
    /// <para>
    /// The new codec context is initialized from the stream's
    /// <see cref="CodecParameters"/>, configured with the current hardware device
    /// type, and assigned the stream's packet time base.
    /// </para>
    ///
    /// <para>
    /// For video streams, the frame rate is initialized using
    /// <see cref="DemuxerContext.GuessFrameRate(AVStream, AVFrame?)"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="streamIndex"/> is outside the range of available streams.
    /// </exception>
    public CodecContext SetCodec(Codec codec, int streamIndex)
    {
        if (codecContexts.Length != Streams.Count)
            _ = CodecContexts;
        codecContexts[streamIndex].Dispose();
        codecContexts[streamIndex] = CodecContext.Allocate(codec);
        codecContexts[streamIndex].SetCodecParameters(Streams[streamIndex].CodecParameters);
        _ = codecContexts[streamIndex].SetHWDeviceType(hwType);
        codecContexts[streamIndex].PacketTimeBase = Streams[streamIndex].TimeBase;
        codecContexts[streamIndex].TimeBase = Streams[streamIndex].TimeBase;
        if (codecContexts[streamIndex].CodecType == MediaType.Video)
            codecContexts[streamIndex].FrameRate = FormatContext.GuessFrameRate(Streams[streamIndex], null);
        return codecContexts[streamIndex];
    }

    /// <inheritdoc cref="FormatContext.Metadata" />
    public AVDictionary_ref Metadata => FormatContext.Metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSource"/> class with a specified format context and device type.
    /// </summary>
    /// <param name="context">The format context to associate with this media source.</param>
    /// <param name="deviceType">The device type for hardware-accelerated decoding.</param>
    public MediaSource(DemuxerContext context, DeviceType deviceType)
    {
        FormatContext = context;
        hwType = deviceType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSource"/> class with a specified format context.
    /// </summary>
    /// <param name="context">The format context to associate with this media source.</param>
    public MediaSource(DemuxerContext context)
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
        DemuxerContext input = DemuxerContext.Open(url, format, options, true);
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
        DemuxerContext? input = DemuxerContext.Open(stream, format, options, true);
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
        DemuxerContext input = DemuxerContext.Open(io, format, options, true);
        return new MediaSource(input, deviceType);
    }

    /// <summary>
    /// Creates a new <see cref="MediaSource"/> instance from an existing format context.
    /// </summary>
    /// <param name="file">The format context representing the media source.</param>
    /// <param name="deviceType">The device type for hardware-accelerated decoding (default is <see cref="DeviceType.None"/>).</param>
    /// <returns>A new <see cref="MediaSource"/> instance.</returns>
    public static MediaSource Open(DemuxerContext file, DeviceType deviceType = DeviceType.None) => new(file, deviceType);

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
    public AVResult32 ReadPacket(AVPacket packet) => FormatContext.ReadPacket(packet);

    /// <summary>
    /// Sends a packet to the codec context and attempts to receive a frame.
    /// </summary>
    /// <param name="srcPacket">The <see cref="AVPacket"/> to send to the codec context.</param>
    /// <param name="dstFrame">The <see cref="AVFrame"/> to receive the decoded frame.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    [Obsolete("Use Decode")]
    public AVResult32 ReadFrame(AVPacket srcPacket, AVFrame dstFrame)
    {
        AVResult32 res = CodecContexts[srcPacket.StreamIndex].SendPacket(srcPacket);
        return res.IsError ? res : CodecContexts[srcPacket.StreamIndex].ReceiveFrame(dstFrame);
    }

    /// <inheritdoc cref="CodecContext.DecodeSubtitle(AVPacket, Subtitles.Subtitle)" />
    public AVResult32 DecodeSubtitle(AVPacket srcPacket, Subtitles.Subtitle subtitle) => codecContexts[srcPacket.StreamIndex].DecodeSubtitle(srcPacket, subtitle);



    /// <summary>
    /// Reads a packet and decodes it into a frame. Continues until a frame is successfully decoded or no more frames can be read.
    /// </summary>
    /// <param name="frame">The <see cref="AVFrame"/> to populate with the decoded frame.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation. Returns the stream index if successful; otherwise, the error result.</returns>
    public AVResult32 ReadAndDecodeAVFrame(AVFrame frame)
        => ReadAndDecode(frame, static (codec, f) => codec.ReceiveFrame(f));

    /// <summary>
    /// Reads a packet and decodes it into a frame. Continues until a frame is successfully decoded or no more frames can be read. <br/>
    /// This methods does not transfer a HwFrame to Ram.
    /// </summary>
    /// <param name="frame">The <see cref="AVFrame"/> to populate with the decoded frame.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation. Returns the stream index if successful; otherwise, the error result.</returns>
    public AVResult32 ReadAndDecodeHWAVFrame(AVFrame frame)
        => ReadAndDecode(frame, static (codec, f) => codec.ReceiveHWFrame(f));

    /// <summary>
    /// <see langword="true"/> once the end of the input was reached and the decoders were
    /// told to drain; reset by <see cref="FlushBuffers"/> (and therefore by every seek).
    /// </summary>
    private bool draining;

    private static bool IsAudioOrVideo(CodecContext codec) => codec.CodecType is MediaType.Audio or MediaType.Video;

    private AVResult32 ReadAndDecode(AVFrame frame, Func<CodecContext, AVFrame, AVResult32> receive)
    {
        if (draining)
            return ReceiveDrained(frame, receive);

        AVResult32 res = 0;
        do
        {
            if (res == AVResult32.TryAgain || packet.StreamIndex == -1)
            {
                res = FormatContext.ReadPacket(packet);
                if (res == AVResult32.EndOfFile)
                {
                    // Flush every decoder once, then hand out their remaining frames,
                    // one per call, until all of them report end of file.
                    foreach (CodecContext ctx in CodecContexts.Where(IsAudioOrVideo))
                        _ = ctx.DrainDecoder();
                    draining = true;
                    return ReceiveDrained(frame, receive);
                }
                res.ThrowIfError();
                // ToDo: decode other media types
                if (Streams[packet.StreamIndex].Discard.HasFlag(DiscardFlags.All) ||
                    !IsAudioOrVideo(CodecContexts[packet.StreamIndex]))
                {
                    res = AVResult32.TryAgain;
                    continue;
                }
                CodecContexts[packet.StreamIndex].SendPacket(packet).ThrowIfError();
            }
            res = receive(CodecContexts[packet.StreamIndex], frame);
        } while (res == AVResult32.TryAgain);
        return res.IsError ? res : (AVResult32)packet.StreamIndex;
    }

    private AVResult32 ReceiveDrained(AVFrame frame, Func<CodecContext, AVFrame, AVResult32> receive)
    {
        for (int i = 0; i < codecContexts.Length; i++)
        {
            if (!IsAudioOrVideo(codecContexts[i]))
                continue;
            AVResult32 res = receive(codecContexts[i], frame);
            if (!res.IsError)
                return i;
            if (res != AVResult32.EndOfFile)
                return res;
        }
        return AVResult32.EndOfFile;
    }

    /// <summary>
    /// Flushes the buffers of all codec contexts, clearing any buffered data.
    /// </summary>
    public void FlushBuffers()
    {
        // Flushed decoders accept packets again.
        draining = false;
        for (int i = 0; i < CodecContexts.Count; i++)
            CodecContexts[i].FlushBuffers();
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

    /// <summary>
    /// Seeks exactly to the specified time position within the media stream for a specific stream index.
    /// </summary>
    /// <param name="timeSpan">The position to seek to.</param>
    /// <param name="streamIndex">The stream index.</param>
    /// <returns>Negative if error.</returns>
    public AVResult32 SeekExactly(TimeSpan timeSpan, int streamIndex)
    {
        AVResult32 result = Seek(timeSpan, streamIndex);
        if (result.IsError)
            return result;
        using AVFrame frame = AVFrame.Allocate();
        for (; ; )
        {
            result = ReadPacket(packet);
            if (result.IsError)
                return result;
            if (packet.StreamIndex != streamIndex)
            {
                // Keep the other audio/video decoders fed, but only the requested stream
                // decides when the target has been reached.
                if (IsAudioOrVideo(CodecContexts[packet.StreamIndex]) && !Streams[packet.StreamIndex].Discard.HasFlag(DiscardFlags.All))
                {
                    result = Decode(packet, frame);
                    if (result.IsError && !result.IsTryAgain)
                        return result;
                }
                continue;
            }
            if ((packet.PresentationTimestamp + packet.Duration) * packet.TimeBase < timeSpan)
            {
                result = Decode(packet, frame);
                if (result.IsTryAgain)
                    continue;
                if (result.IsError)
                    return result;
            }
            else
                return CodecContexts[packet.StreamIndex].SendPacket(packet);
        }
    }

    /// <inheritdoc cref="DemuxerContext.GuessFrameRate(int)"/>
    public Rational GuessFramerate(int streamIndex) => FormatContext.GuessFrameRate(streamIndex);

    /// <inheritdoc cref="DemuxerContext.GuessFrameRate(AVStream)"/>
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

    /// <summary>
    /// Decodes a packet and writes the next available decoded frame into the specified
    /// <see cref="AVFrame"/>.
    /// </summary>
    /// <param name="packet">
    /// The encoded packet to submit to the decoder.
    /// </param>
    /// <param name="frame">
    /// Receives the decoded frame if one is available.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the decoding operation.
    /// </returns>
    /// <remarks>
    /// This method forwards the packet to the decoder associated with
    /// <see cref="AVPacket.StreamIndex"/>.
    ///
    /// <para>
    /// A successful return value indicates that a decoded frame was written to
    /// <paramref name="frame"/>.
    /// </para>
    ///
    /// <para>
    /// <see cref="AVResult32.TryAgain"/> indicates that the decoder requires additional
    /// input packets before it can produce another frame.
    /// </para>
    ///
    /// <para>
    /// <see cref="AVResult32.EndOfFile"/> indicates that the decoder has been fully
    /// drained and no more frames are available.
    /// </para>
    /// </remarks>
    public AVResult32 Decode(AVPacket packet, AVFrame frame)
        => CodecContexts[packet.StreamIndex].Decode(packet, frame);

    #endregion
}


