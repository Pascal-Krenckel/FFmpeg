using FFmpeg.Codecs;
using FFmpeg.Collections;
using FFmpeg.Formats;
using FFmpeg.Logging;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FFmpeg;
public class MediaSink : IDisposable
{
    public FormatContext FormatContext { get; private set; }

    private readonly AVPacket packet = new() { StreamIndex = -1 };
    
    public MediaSink(FormatContext context) => FormatContext = context;

    public IReadOnlyList<AVStream> Streams => FormatContext.Streams;

    private List<CodecContext?> codecContexts = [];

    /// <summary>
    /// A function to retrieve a codec by its ID. This can be set to provide custom codec retrieval logic.
    /// </summary>
    public static Func<CodecID, Codec>? GetCodec;

    /// <summary>
    /// Gets the list of codec contexts for the streams in this media source.
    /// </summary>
    /// <remarks>
    /// This property initializes codec contexts lazily based on the number of streams. It also sets up codec contexts
    /// for each stream, guessing the frame rate for video streams and adjusting codec parameters as needed.
    /// </remarks>
    public IReadOnlyList<CodecContext?> CodecContexts
    {
        get => codecContexts;
    }
    public AVDictionary_ref Metatdata => FormatContext.Metadata;

    public AVStream AddStream(CodecContext? encoderContext)
    {
        if (encoderContext == null)
        {
            codecContexts.Add(null);
            return FormatContext.AddStream(default);
        }
        else
        {
            codecContexts.Add(encoderContext);
            var stream = FormatContext.AddStream(encoderContext.Codec);
            var @params = stream.CodecParameters;
            @params.CodecTag = encoderContext.CodecTag;
            stream.TimeBase = encoderContext.TimeBase;
            stream.CodecParameters.CopyFrom(encoderContext);
            return stream;
        }
    }

    // You must use SetCodecContext later, or you can only send packages directly.
    public AVStream AddStream(Codec encoder)
    {
        codecContexts.Add(null);
        return FormatContext.AddStream(encoder);
    }

    public void SetCodecContext(CodecContext codec, int streamIndex)
    {
        if (!ReferenceEquals(CodecContexts[streamIndex], codec))
            CodecContexts[streamIndex]?.Dispose();
        codecContexts[streamIndex] = codec;
        if (codec.TimeBase.Numerator == 0 || codec.TimeBase.Denominator == 0)
            codec.TimeBase = Streams[streamIndex].TimeBase;
        if (Streams[streamIndex].TimeBase.Numerator == 0 || Streams[streamIndex].TimeBase.Denominator == 0)
            Streams[streamIndex].TimeBase = codec.TimeBase;
        codecContexts[streamIndex]!.PacketTimeBase = Streams[streamIndex].TimeBase;
        Streams[streamIndex].CodecParameters.CopyFrom(codec);
    }

    public void WriteHeader()
    {
        ExceptionLog.Reset();
        FormatContext.WriteHeader().ThrowIfError();
        for (int i = 0; i < Streams.Count; i++)
            if (CodecContexts[i] != null)
                CodecContexts[i]!.PacketTimeBase = Streams[i].TimeBase; // reset pkt_timebase since it might have changed.
        OpenCodecs();
    }

    public void OpenCodecs()
    {
        ExceptionLog.Reset();
        for (int i = 0; i < codecContexts.Count; i++)
            if (codecContexts[i]?.IsOpen == false) codecContexts[i]!.Open(null).ThrowIfError();
    }

    public void WriteHeader(IDictionary<string, string> dic)
    {
        ExceptionLog.Reset();
        FormatContext.WriteHeader(dic).ThrowIfError();
        for (int i = 0; i < codecContexts.Count; i++)
            if (codecContexts[i]?.IsOpen == false) codecContexts[i]!.Open(null).ThrowIfError();
    }
    public AVResult32 WritePacket(IPacket packet) => FormatContext.WriteFrameInterleaved(packet);
    public AVResult32 WriteFrame(AVFrame frame, int streamIndex)
    {
        lock (packet)
        {
            var error = CodecContexts[streamIndex]!.SendFrame(frame);
            if (error.IsError) return error;
            error = CodecContexts[streamIndex]!.ReceivePacket(packet);
            if (error.IsError) return error;
            packet.StreamIndex = streamIndex;
            return WritePacket(packet);
        }
    }
    public AVResult32 WriteSubtitle(Subtitles.Subtitle subtitle, int streamIndex)
    {
        lock (packet)
        {
            var error = CodecContexts[streamIndex]!.EncodeSubtitle(packet,subtitle);
            if (error.IsError) return error;
            packet.StreamIndex = streamIndex;
            return WritePacket(packet);
        }
    }

    public static MediaSink? Create(string? url, OutputFormat? outputFormat)
    {
        if (string.IsNullOrWhiteSpace(url) && outputFormat == null) throw new ArgumentNullException(nameof(url));
        var res  = FormatContext.OpenOutput(url, outputFormat);
        if (res == null)
            return null;
        return new(res);
    }
    public static MediaSink? Create(string url) => Create(url, null);
    public static MediaSink? Create(Stream stream, OutputFormat outputFormat)
    {
        var res  = FormatContext.OpenOutput(stream, outputFormat);
        if (res == null)
            return null;
        return new(res);
    }

    public static MediaSink? Create(IO.IOContext ioContext, OutputFormat outputFormat)
    {
        var res  = FormatContext.OpenOutput(ioContext, outputFormat);
        if (res == null)
            return null;
        return new(res);
    }

    public void Close() => Dispose();

    #region Dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {

            disposedValue = true;

            if (disposing)
            {
                for (int i = 0; i < CodecContexts.Count; i++)
                    if (CodecContexts[i] != null && CodecContexts[i]!.CodecType is MediaType.Audio or MediaType.Video)
                    {
                        var context = CodecContexts[i];
                        _ = context!.DrainEncoder();

                        AVResult32 res;
                        do
                        {
                            res = context.ReceivePacket(packet);
                            packet.StreamIndex = i;
                            if (!res.IsError)
                                _ = FormatContext.WriteFrameInterleaved(packet);
                            else break;
                        } while (true);
                    }
                FormatContext.Dispose();
                foreach (var ctx in CodecContexts)
                    ctx?.Dispose();
                packet.Dispose();
            }

        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public AVResult32 WriteTrailer() => FormatContext.WriteTrailer();
    #endregion
}

