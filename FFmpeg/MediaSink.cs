using FFmpeg.Codecs;
using FFmpeg.Collections;
using FFmpeg.Formats;
using FFmpeg.Utils;

namespace FFmpeg;

/// <summary>
/// Provides a high-level interface for encoding and writing media to an output
/// container.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MediaSink"/> combines one or more <see cref="CodecContext"/>
/// instances with a <see cref="MuxerContext"/> to simplify media encoding and
/// muxing.
/// </para>
/// <para>
/// Streams are added using <see cref="AddStream(CodecContext?)"/> or
/// <see cref="AddStream(Codec)"/>. Frames can then be encoded and written using
/// <see cref="WriteFrame"/>, subtitles using <see cref="WriteSubtitle"/>, or
/// pre-encoded packets using <see cref="WritePacket"/>.
/// </para>
/// <para>
/// The container header is written automatically before the first packet is written.
/// <see cref="Close"/> or <see cref="Dispose()"/> drains any remaining encoded
/// packets, writes the trailer, and releases all associated resources.
/// </para>
/// </remarks>
public class MediaSink : IDisposable
{
    private bool headerWritten = false;
    private bool trailerWritten = false;

    /// <summary>
    /// Gets the underlying muxer context.
    /// </summary>
    public MuxerContext FormatContext { get; }

    private readonly AVPacket packet = new() { StreamIndex = -1 };

    /// <summary>
    /// Initializes a new <see cref="MediaSink"/> using the specified muxer context.
    /// </summary>
    /// <param name="context">
    /// The muxer context used to write the output container.
    /// </param>
    public MediaSink(MuxerContext context) => FormatContext = context;

    /// <summary>
    /// Gets the streams contained in the output media.
    /// </summary>
    public IReadOnlyList<AVStream> Streams => FormatContext.Streams;

    private readonly List<CodecContext?> codecContexts = [];

    /// <summary>
    /// Gets the encoder contexts associated with the output streams.
    /// </summary>
    /// <remarks>
    /// The list contains one entry for each stream in <see cref="Streams"/>.
    /// Entries may be <see langword="null"/> for streams that are written using
    /// pre-encoded packets instead of an encoder.
    /// </remarks>
    public IReadOnlyList<CodecContext?> CodecContexts => codecContexts;

    /// <summary>
    /// Gets the metadata associated with the output container.
    /// </summary>
    public AVDictionary_ref Metadata => FormatContext.Metadata;

    /// <summary>
    /// Adds a new stream using an existing encoder context.
    /// </summary>
    /// <param name="encoderContext">
    /// The encoder context used to initialize the stream, or
    /// <see langword="null"/> to create a stream without an associated encoder.
    /// </param>
    /// <returns>
    /// The newly created output stream.
    /// </returns>
    /// <remarks>
    /// The codec parameters and time base are copied from the encoder context when
    /// one is supplied.
    /// </remarks>
    public AVStream AddStream(CodecContext? encoderContext)
    {
        CheckDisposed();
        if (encoderContext == null)
        {
            codecContexts.Add(null);
            return FormatContext.AddStream(default(Codec));
        }
        else
        {
            codecContexts.Add(encoderContext);
            AVStream stream = FormatContext.AddStream(encoderContext.Codec);
            CodecParameters_ref @params = stream.CodecParameters;
            @params.CodecTag = encoderContext.CodecTag;
            stream.TimeBase = encoderContext.TimeBase;
            stream.CodecParameters.CopyFrom(encoderContext);
            return stream;
        }
    }

    /// <summary>
    /// Adds a new stream for the specified codec.
    /// </summary>
    /// <param name="encoder">
    /// The codec associated with the stream.
    /// </param>
    /// <returns>
    /// The newly created output stream.
    /// </returns>
    /// <remarks>
    /// No encoder context is created automatically. Call
    /// <see cref="SetCodecContext"/> before encoding frames, or write encoded
    /// packets directly using <see cref="WritePacket"/>.
    /// </remarks>
    public AVStream AddStream(Codec encoder)
    {
        CheckDisposed();
        codecContexts.Add(null);
        return FormatContext.AddStream(encoder);
    }

    /// <summary>
    /// Associates an encoder context with an existing stream.
    /// </summary>
    /// <param name="codec">
    /// The encoder context.
    /// </param>
    /// <param name="streamIndex">
    /// The zero-based index of the stream.
    /// </param>
    /// <remarks>
    /// The codec parameters are copied to the corresponding stream. If either the
    /// stream or codec context does not define a valid time base, it is initialized
    /// from the other.
    /// </remarks>
    public void SetCodecContext(CodecContext codec, int streamIndex)
    {
        CheckDisposed();
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

    /// <summary>
    /// Writes the container header.
    /// </summary>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// After the header has been written, the packet time base of each associated
    /// encoder is synchronized with the corresponding output stream, since some
    /// muxers may adjust stream parameters while writing the header.
    /// </remarks>
    public AVResult32 WriteHeader()
    {
        AVResult32 result = FormatContext.WriteHeader();
        if (result.IsError)
            return result;
        for (int i = 0; i < Streams.Count; i++)
        {
            if (CodecContexts[i] != null)
                CodecContexts[i]!.PacketTimeBase = Streams[i].TimeBase; // reset pkt_timebase since it might have changed.
        }
        headerWritten = true;
        return 0;
    }

    /// <summary>
    /// Writes the container header using the specified muxer options.
    /// </summary>
    /// <param name="dic">
    /// A dictionary containing muxer options.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// Any options that are not recognized by the muxer remain in the supplied
    /// dictionary after the call returns.
    /// </remarks>
    public AVResult32 WriteHeader(IDictionary<string, string> dic)
    {
        AVResult32 result = FormatContext.WriteHeader(dic);
        if (result.IsError)
            return result;
        for (int i = 0; i < Streams.Count; i++)
        {
            if (CodecContexts[i] != null)
                CodecContexts[i]!.PacketTimeBase = Streams[i].TimeBase; // reset pkt_timebase since it might have changed.
        }
        headerWritten = true;
        return 0;
    }

    /// <summary>
    /// Opens all encoder contexts that have not already been opened.
    /// </summary>
    /// <remarks>
    /// Calling this method is optional. If an encoder has not been opened manually,
    /// it will be opened automatically when the first frame is encoded.
    /// </remarks>
    public void OpenCodecs()
    {
        CheckDisposed();
        for (int i = 0; i < codecContexts.Count; i++)
        {
            if (codecContexts[i]?.IsOpen == false)
                codecContexts[i]!.Open(null).ThrowIfError();
        }
    }


    /// <summary>
    /// Writes an encoded packet to the output container.
    /// </summary>
    /// <param name="packet">
    /// The packet to write.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// If the container header has not yet been written, it is written
    /// automatically before the packet is muxed.
    /// </remarks>
    public AVResult32 WritePacket(IPacket packet)
    {
        CheckDisposed();
        if (!headerWritten)
            WriteHeader().ThrowIfError();
        return FormatContext.WritePacketInterleaved(packet);
    }

    /// <summary>
    /// Encodes a frame and writes the resulting packets to the output container.
    /// </summary>
    /// <param name="frame">
    /// The frame to encode.
    /// </param>
    /// <param name="streamIndex">
    /// The index of the destination stream.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// The frame is sent to the encoder and all immediately available packets are
    /// written to the output container.
    /// </remarks>
    public AVResult32 WriteFrame(AVFrame frame, int streamIndex)
    {
        CheckDisposed();
        AVResult32 error = CodecContexts[streamIndex]!.SendFrame(frame);
        if (error.IsError)
            return error;
        while (!(error = CodecContexts[streamIndex]!.ReceivePacket(packet)).IsError)
        {
            packet.StreamIndex = streamIndex;
            error = WritePacket(packet);
            if (error.IsError)
                return error;
        }
        if (error.IsTryAgain)
            return 0;
        return error;

    }

    /// <summary>
    /// Encodes and writes a subtitle to the output container.
    /// </summary>
    /// <param name="subtitle">
    /// The subtitle to encode.
    /// </param>
    /// <param name="streamIndex">
    /// The destination stream.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    public AVResult32 WriteSubtitle(Subtitles.Subtitle subtitle, int streamIndex)
    {
        CheckDisposed();
        AVResult32 error = CodecContexts[streamIndex]!.EncodeSubtitle(packet, subtitle);
        if (error.IsError)
            return error;
        packet.StreamIndex = streamIndex;
        return WritePacket(packet);

    }

    /// <summary>
    /// Creates a new <see cref="MediaSink"/> for writing media to a file.
    /// </summary>
    /// <param name="url">
    /// The output filename or URL.
    /// </param>
    /// <param name="outputFormat">
    /// The output format, or <see langword="null"/> to determine the format from
    /// the filename.
    /// </param>
    /// <returns>
    /// A new <see cref="MediaSink"/>, or <see langword="null"/> if the output
    /// could not be created.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when both <paramref name="url"/> and
    /// <paramref name="outputFormat"/> are <see langword="null"/>.
    /// </exception>
    public static MediaSink? Create(string? url, OutputFormat? outputFormat)
    {
        if (string.IsNullOrWhiteSpace(url) && outputFormat == null)
            throw new ArgumentNullException(nameof(url));
        MuxerContext? res = MuxerContext.Open(url, outputFormat);
        return res == null ? null : new(res);
    }

    /// <summary>
    /// Creates a new <see cref="MediaSink"/> for writing media to a file.
    /// </summary>
    /// <param name="url">
    /// The output filename or URL.
    /// </param>
    /// <returns>
    /// A new <see cref="MediaSink"/>, or <see langword="null"/> if the output
    /// could not be created.
    /// </returns>
    public static MediaSink? Create(string url) => Create(url, null);

    /// <summary>
    /// Creates a new <see cref="MediaSink"/> for writing media to a managed stream.
    /// </summary>
    /// <param name="stream">
    /// The destination stream.
    /// </param>
    /// <param name="outputFormat">
    /// The output format.
    /// </param>
    /// <returns>
    /// A new <see cref="MediaSink"/>, or <see langword="null"/> if the output
    /// could not be created.
    /// </returns>
    public static MediaSink? Create(Stream stream, OutputFormat outputFormat)
    {
        MuxerContext? res = MuxerContext.Open(stream, outputFormat);
        return res == null ? null : new(res);
    }

    /// <summary>
    /// Creates a new <see cref="MediaSink"/> using a custom I/O context.
    /// </summary>
    /// <param name="ioContext">
    /// The custom I/O context.
    /// </param>
    /// <param name="outputFormat">
    /// The output format.
    /// </param>
    /// <returns>
    /// A new <see cref="MediaSink"/>, or <see langword="null"/> if the output
    /// could not be created.
    /// </returns>
    public static MediaSink? Create(IO.IOContext ioContext, OutputFormat outputFormat)
    {
        MuxerContext? res = MuxerContext.Open(ioContext, outputFormat);
        return res == null ? null : new(res);
    }

    /// <summary>
    /// Flushes all encoders and writes the trailer of the output container.
    /// </summary>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// All audio and video encoders are drained before the trailer is written.
    /// Subsequent calls after a successful write return immediately without writing the trailer again.
    /// </remarks>
    public AVResult32 WriteTrailer()
    {
        if (trailerWritten)
            return 0;
        foreach (var context in CodecContexts.Where(c => c != null && c.CodecType is MediaType.Video or MediaType.Audio))
        {
            AVResult32 error = context!.DrainEncoder();
            if (error == AVResult32.EndOfFile)
                continue;
            if (error.IsError)
                return error;
            while (!(error = context.ReceivePacket(packet)).IsError)
            {
                error = FormatContext.WritePacket(packet);
                if (error.IsError)
                    return error;
            }
            if (error != AVResult32.EndOfFile)
                return error;
        }
        AVResult32 result = FormatContext.WriteTrailer();
        if (!result.IsError)
            trailerWritten = true;
        return result;
    }

    /// <summary>
    /// Finalizes the output container and releases all associated resources.
    /// </summary>
    /// <remarks>
    /// This method writes the trailer if it has not already been written and then
    /// disposes the <see cref="MediaSink"/>.
    /// </remarks>
    public void Close()
    {
        if (!trailerWritten)
            WriteTrailer().ThrowIfError();
        Dispose();
    }

    #region Dispose
    private bool disposedValue;

    /// <summary>
    /// Releases the resources used by the current instance.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources; otherwise,
    /// <see langword="false"/>.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {

            if (disposing)
            {
                _ = WriteTrailer();
                FormatContext.Dispose();
                foreach (CodecContext? ctx in CodecContexts)
                    ctx?.Dispose();
                packet.Dispose();
            }
            disposedValue = true;
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance.
    /// </summary>
    /// <remarks>
    /// If the trailer has not yet been written, this method attempts to write it
    /// before releasing the underlying resources.
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    // Drains the encoders and finilizes the file
    #endregion

    private void CheckDisposed()
    {
        if (disposedValue)
            throw new ObjectDisposedException(GetType().FullName);
    }
}

