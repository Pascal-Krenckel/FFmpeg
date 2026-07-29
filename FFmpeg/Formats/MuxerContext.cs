using FFmpeg.AutoGen;
using FFmpeg.Codecs;
using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpeg.Formats;

/// <summary>
/// Represents an output media container used for muxing encoded packets into
/// a media file or stream.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MuxerContext"/> is the output counterpart to
/// <see cref="DemuxerContext"/>. It provides functionality for creating media
/// containers, adding streams, writing container headers, writing encoded
/// packets, and finalizing the output.
/// </para>
/// <para>
/// A muxer can write to a file, a <see cref="Stream"/>, or a custom
/// <see cref="IOContext"/>. After opening a muxer, one or more streams should
/// be added and configured before calling <see cref="WriteHeader()"/>. Encoded
/// packets can then be written using <see cref="WritePacket(IPacket?)"/> or
/// <see cref="WritePacketInterleaved(IPacket?)"/>. Once all packets have been
/// written, <see cref="WriteTrailer"/> should be called to finalize the
/// container.
/// </para>
/// <para>
/// This class wraps FFmpeg's <c>AVFormatContext</c> for output operations and
/// corresponds to the FFmpeg muxing API.
/// </para>
/// </remarks>
public unsafe class MuxerContext : FormatContext
{

    /// <summary>
    /// Gets the output format associated with this context, if available.
    /// </summary>
    public OutputFormat? OutputFormat => field ??= Context->oformat != null ? new(Context->oformat) : null;

    /// <summary>
    /// Creates a muxer context that wraps around the provided ffmpeg ibject
    /// </summary>
    /// <param name="context">The pointer to the _AVFormatContext</param>
    protected MuxerContext(_AVFormatContext* context) : base(context)
    {
    }

    #region Allocate

    /// <summary>
    /// Allocates a new <see cref="MuxerContext"/> for output operations using the specified filename and output format.
    /// This function wraps the <see cref="ffmpeg.avformat_alloc_output_context2"/> function.
    /// </summary>
    /// <param name="filename">
    /// The name of the file to use for output. Can be <see langword="null"/> if the output format does not require a filename.
    /// </param>
    /// <param name="format">
    /// The output format for the context, or <see langword="null"/> to determine the format from the filename.
    /// </param>
    /// <returns>
    /// A new <see cref="MuxerContext"/> instance for output, or <see langword="null"/> if allocation fails.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if the allocation fails and the context could not be created.
    /// </exception>
    /// <remarks>
    /// This function corresponds to the FFmpeg sequence: <c>avformat_alloc_output_context2</c>.
    /// </remarks>
    private static MuxerContext? AllocateOutput(string? filename, OutputFormat? format)
    {
        AutoGen._AVFormatContext* context;
        AutoGen._AVOutputFormat* oFormat = format != null ? format.Value.Format : null;
        AVResult32 res = ffmpeg.avformat_alloc_output_context2(&context, oFormat, null, filename);
        return res == AVResult32.OutOfMemory ? throw new OutOfMemoryException() : context == null ? null : new(context);
    }

    #endregion

    #region Open

    /// <summary>
    /// Opens an output media file for writing.
    /// </summary>
    /// <param name="filename">The filename for the output media.</param>
    /// <returns>An instance of <see cref="MuxerContext"/> or <see langword="null"/> if the operation fails.</returns>
    public static MuxerContext? Open(string filename) => Open(filename, null);

    /// <summary>
    /// Opens an output media file for writing. For Output fomats of the no file type, filename may be null.
    /// </summary>
    /// <param name="filename">The filename for the output media.</param>
    /// <param name="format">The output format.</param>
    /// <returns>An instance of <see cref="MuxerContext"/> or <see langword="null"/> if the operation fails.</returns>
    public static MuxerContext? Open(string? filename, OutputFormat? format)
    {
        MuxerContext? output = AllocateOutput(filename, format);
        if (output == null)
            return null;
        if (filename != null && !(format?.Flags == FormatFlags.NoFile))
        {
            AVResult32 result = AVIOContext.Open(&output.Context->pb, filename, ffmpeg.AVIO_FLAG_WRITE, out output.ioContext);
            if (result.IsError)
            {
                output.Dispose();
                return result == AVResult32.OutOfMemory ? throw new OutOfMemoryException() : null;
            }
        }
        return output;
    }

    /// <summary>
    /// Opens an output stream for writing.
    /// </summary>
    /// <param name="stream">The stream for output media.</param>
    /// <param name="format">The output format.</param>
    /// <param name="closeOnDispose">If true, when the MuxerContext is closed the stream will be too.</param>
    /// <returns>An instance of <see cref="MuxerContext"/> or <see langword="null"/> if the operation fails.</returns>
    public static MuxerContext? Open(Stream stream, OutputFormat format, bool closeOnDispose = true)
        => Open(new IOStreamContext(stream, closeOnDispose), format);

    /// <summary>
    /// Opens an output media file using an I/O context for writing.
    /// </summary>
    /// <param name="context">The I/O context for output media.</param>
    /// <param name="format">The output format.</param>
    /// <returns>An instance of <see cref="MuxerContext"/> or <see langword="null"/> if the operation fails.</returns>
    public static MuxerContext? Open(IOContext context, OutputFormat format)
    {
        MuxerContext? output = AllocateOutput(null, format);
        if (output == null)
            return null;
        context.InitContext(output, IOOptions.Write | (context.CanSeek ? IOOptions.Seek : 0));
        return output;
    }

    #endregion

    /// <summary>
    /// Adds a new stream to the output container.
    /// </summary>
    /// <param name="codec">
    /// The codec associated with the new stream.
    /// </param>
    /// <returns>
    /// An <see cref="AVStream"/> representing the newly created stream.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if FFmpeg fails to allocate the new stream.
    /// </exception>
    /// <remarks>
    /// This method creates a new <see cref="AVStream"/> using
    /// <see cref="ffmpeg.avformat_new_stream"/> and initializes its codec
    /// identifier and media type.
    ///
    /// The returned stream should be configured before calling
    /// <see cref="WriteHeader()"/>.
    /// </remarks>
    public AVStream AddStream(Codecs.Codec codec)
    {
        AutoGen._AVStream* res = ffmpeg.avformat_new_stream(Context, codec.codec);
        if (res == null)
            throw new OutOfMemoryException();
        res->id = res->index;
        res->codecpar->codec_id = (AutoGen._AVCodecID)codec.CodecID;
        res->codecpar->codec_type = (AutoGen._AVMediaType)codec.MediaType;
        return new AVStream(res);
    }

    /// <summary>
    /// Adds a stream based on the codec parameters of the 
    /// </summary>
    /// <param name="copyStream">The stream that contains the codec parameters we want to copy</param>
    /// <returns>The added stream.</returns>
    public AVStream AddStream(AVStream copyStream)
    {
        AutoGen._AVStream* res = ffmpeg.avformat_new_stream(Context, null);
        if (res == null)
            throw new OutOfMemoryException();
        res->id = res->index;

        var newStream = new AVStream(res);
        newStream.CodecParameters.CopyFrom(copyStream.CodecParameters);
        newStream.TimeBase = copyStream.TimeBase;
        return newStream;
    }


    /// <summary>
    /// Adds a new stream to the output container and copies codec parameters
    /// from an existing source.
    /// </summary>
    /// <param name="codec">
    /// The codec associated with the new stream.
    /// </param>
    /// <param name="codecParameters">
    /// The codec parameters to copy into the stream.
    /// </param>
    /// <returns>
    /// An <see cref="AVStream"/> representing the newly created stream.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if FFmpeg fails to allocate the new stream.
    /// </exception>
    /// <remarks>
    /// The codec parameters are copied using
    /// <see cref="ffmpeg.avcodec_parameters_copy"/>. This overload is useful
    /// when remuxing existing streams or when codec parameters are already
    /// available.
    /// </remarks>
    public AVStream AddStream(Codec codec, ICodecParameters codecParameters)
    {
        AutoGen._AVStream* res = ffmpeg.avformat_new_stream(Context, codec.codec);
        if (res == null)
            throw new OutOfMemoryException();
        res->id = res->index;
        res->codecpar->codec_id = (AutoGen._AVCodecID)codec.CodecID;
        res->codecpar->codec_type = (AutoGen._AVMediaType)codec.MediaType;
        ((AVResult32)ffmpeg.avcodec_parameters_copy(res->codecpar, codecParameters.Parameters)).ThrowIfError();
        return new AVStream(res);
    }

    /// <summary>
    /// Adds a new stream to the output container using an existing codec context.
    /// </summary>
    /// <param name="codec">
    /// The codec context whose parameters are copied to the new stream.
    /// </param>
    /// <returns>
    /// An <see cref="AVStream"/> representing the newly created stream.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if FFmpeg fails to allocate the new stream.
    /// </exception>
    /// <remarks>
    /// The codec parameters and time base are copied from the supplied
    /// <see cref="CodecContext"/>.
    ///
    /// If the selected output format requires global headers, the
    /// <see cref="CodecFlags.GlobalHeader"/> flag is automatically enabled on
    /// the codec context before encoding begins.
    /// </remarks>
    public AVStream AddStream(CodecContext codec)
    {
        AutoGen._AVStream* res = ffmpeg.avformat_new_stream(Context, codec.Codec.codec);
        if (res == null)
            throw new OutOfMemoryException();
        res->id = res->index;
        res->codecpar->codec_id = (AutoGen._AVCodecID)codec.CodecID;
        res->codecpar->codec_type = (AutoGen._AVMediaType)codec.Codec.MediaType;
        using CodecParameters codecParams = codec.GetCodecParameters();
        ((AVResult32)ffmpeg.avcodec_parameters_copy(res->codecpar, codecParams.codecParameters)).ThrowIfError();
        res->time_base = codec.TimeBase;
        if (OutputFormat.GetValueOrDefault().Flags.HasFlag(FormatFlags.GlobalHeader))
            codec.Flags |= CodecFlags.GlobalHeader;
        return new AVStream(res);
    }

    #region WriteHeader

    /// <summary>
    /// Writes the header of the output media file.
    /// </summary>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 WriteHeader() => ffmpeg.avformat_write_header(Context, null);

    /// <summary>
    /// Writes the header of the output media file using a specified dictionary.
    /// </summary>
    /// <param name="dictionary">A dictionary of options.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 WriteHeader(Collections.AVDictionary dictionary)
    {
        AutoGen._AVDictionary* dic = dictionary.dictionary;
        int res = ffmpeg.avformat_write_header(Context, &dic);
        dictionary.dictionary = dic;
        return res;
    }

    /// <summary>
    /// Writes the header of the output media file using a multi-dictionary.
    /// </summary>
    /// <param name="dictionary">A multi-dictionary of options.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 WriteHeader(Collections.AVMultiDictionary dictionary)
    {
        AutoGen._AVDictionary* dic = dictionary.dictionary;
        int res = ffmpeg.avformat_write_header(Context, &dic);
        dictionary.dictionary = dic;
        return res;
    }

    /// <summary>
    /// Writes the header of the output media file using a dictionary represented as <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="dictionary">A dictionary of options.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 WriteHeader(IDictionary<string, string> dictionary)
    {
        if (dictionary is Collections.AVDictionary avDict)
        {
            return WriteHeader(avDict);
        }
        else
        {
            using Collections.AVDictionary dicCopy = new(dictionary);
            AVResult32 res = WriteHeader(dicCopy);
            dictionary.Clear();
            foreach (KeyValuePair<string, string> kvp in dicCopy)
                dictionary[kvp.Key] = kvp.Value;
            return res;
        }
    }

    #endregion


    #region WriteFrame

    /// <summary>
    /// Writes a frame to the output media file.
    /// </summary>
    /// <param name="packet">The packet to write. If <see langword="null"/>, writes a null packet.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 WritePacket(IPacket? packet)
    {
        if (packet == null)
            return (AVResult32)ffmpeg.av_write_frame(Context, null);
        else
        {
            if (packet.TimeBase != Streams[packet.StreamIndex].TimeBase)
                ffmpeg.av_packet_rescale_ts(packet.Packet, packet.TimeBase, Streams[packet.StreamIndex].TimeBase);
            return (AVResult32)ffmpeg.av_write_frame(Context, packet.Packet);
        }
    }

    /// <summary>
    /// Writes a frame to the output media file.
    /// </summary>
    /// <param name="packet">The packet to write. If <see langword="null"/>, writes a null packet.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    [Obsolete("Renamed to WritePacket")]
    public AVResult32 WriteFrame(IPacket? packet) => WritePacket(packet);

    /// <summary>
    /// Writes an interleaved frame to the output media file.
    /// </summary>
    /// <param name="packet">The packet to write. If <see langword="null"/>, writes a null packet.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 WritePacketInterleaved(IPacket? packet)
    {
        if (packet == null)
            return (AVResult32)ffmpeg.av_interleaved_write_frame(Context, null);
        else
        {
            if (packet.TimeBase != Streams[packet.StreamIndex].TimeBase)
                ffmpeg.av_packet_rescale_ts(packet.Packet, packet.TimeBase, Streams[packet.StreamIndex].TimeBase);
            return (AVResult32)ffmpeg.av_interleaved_write_frame(Context, packet.Packet);
        }
    }

    /// <summary>
    /// Writes an interleaved frame to the output media file.
    /// </summary>
    /// <param name="packet">The packet to write. If <see langword="null"/>, writes a null packet.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    [Obsolete("Renamed to WritePacketInterleaved")]
    public AVResult32 WriteFrameInterleaved(IPacket? packet) => WritePacketInterleaved(packet);
    #endregion

    /// <summary>
    /// Writes the trailer of the output media file and finalizes the container.
    /// </summary>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method must be called after all packets have been written. It writes
    /// any remaining buffered data, updates container metadata such as indexes,
    /// and releases internal muxing state maintained by FFmpeg.
    /// </remarks>
    public AVResult32 WriteTrailer()
    {
        AVResult32 res = ffmpeg.av_write_trailer(Context);
        return res;
    }

    /// <summary>
    /// Flushes the underlying output I/O context, if one exists.
    /// </summary>
    /// <remarks>
    /// This forces any buffered data to be written to the underlying stream or
    /// file. It does not finalize the media container; call
    /// <see cref="WriteTrailer"/> to properly finish the output.
    /// </remarks>
    public void Flush() => ioContext?.Flush();

    /// <summary>
    /// Returns the long name of the output format.
    /// </summary>
    /// <returns>
    /// The long name of the output format, or <c>"Unknown"</c> if no output
    /// format is associated with this context.
    /// </returns>
    public override string ToString() => OutputFormat?.LongName ?? "Unknown";

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {        
        if (disposing)
        {
            ioContext?.Dispose();
        }
        ffmpeg.avio_close(Context->pb);
        ffmpeg.avformat_free_context(Context);
        Context = null;
        base.Dispose(disposing);
    }

}
