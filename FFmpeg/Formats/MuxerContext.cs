using FFmpeg.AutoGen;
using FFmpeg.Codecs;
using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpeg.Formats;

public unsafe class MuxerContext : FormatContext
{

    /// <summary>
    /// Gets the output format associated with this context, if available.
    /// </summary>
    public OutputFormat? OutputFormat => field ??= Context->oformat != null ? new(Context->oformat) : null;

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
        context.InitContext(output, IOOptions.Write);
        return output;
    }

    #endregion

    /// <summary>
    /// Adds a new stream to the media file.
    /// </summary>
    /// <param name="codec">The codec to be used for the new stream.</param>
    /// <returns>
    /// An instance of <see cref="AVStream"/> representing the newly added stream.
    /// </returns>
    /// <remarks>
    /// When demuxing, this method is called by the demuxer in <see langword="read_header"/>. If the <see langword="AVFMTCTX_NOHEADER"/> flag is set in <see cref="AutoGen._AVFormatContext.ctx_flags"/>, it may also be called in <see langword="read_packet"/>.
    /// When muxing, this method should be called by the user before <see cref="ffmpeg.avformat_write_header"/>.
    /// The user is required to call <see cref="ffmpeg.avformat_free_context"/> to clean up the allocation made by <see cref="ffmpeg.avformat_new_stream"/>.
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
    public AVResult32 WriteFrame(IPacket? packet)
        => packet == null ? (AVResult32)ffmpeg.av_write_frame(Context, null) : (AVResult32)ffmpeg.av_write_frame(Context, packet.Packet);

    /// <summary>
    /// Writes an interleaved frame to the output media file.
    /// </summary>
    /// <param name="packet">The packet to write. If <see langword="null"/>, writes a null packet.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 WriteFrameInterleaved(IPacket? packet)
        => packet == null ? (AVResult32)ffmpeg.av_interleaved_write_frame(Context, null) : (AVResult32)ffmpeg.av_interleaved_write_frame(Context, packet.Packet);

    #endregion

    public AVResult32 WriteTrailer()
    {
        AVResult32 res = ffmpeg.av_write_trailer(Context);
        return res;
    }

    public void Flush() => ioContext?.Flush();

    public override string ToString() => OutputFormat?.LongName ?? "Unkown";

}
