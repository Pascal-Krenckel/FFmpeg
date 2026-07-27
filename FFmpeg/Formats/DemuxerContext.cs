using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpeg.Formats;

/// <summary>
/// Represents an FFmpeg input format context used for reading and demultiplexing
/// media containers.
/// </summary>
/// <remarks>
/// <see cref="DemuxerContext"/> provides access to media streams, metadata,
/// chapters, and container information from an input source. It also exposes
/// methods for reading packets, seeking within the media, retrieving stream
/// information, and estimating stream properties.
/// </remarks>
public unsafe partial class DemuxerContext : FormatContext
{
    #region Constructions

    /// <summary>
    /// Initializes a new <see cref="DemuxerContext"/> from an existing native
    /// <see cref="AutoGen._AVFormatContext"/>.
    /// </summary>
    /// <param name="context">
    /// The native input format context to wrap.
    /// </param>
    protected DemuxerContext(AutoGen._AVFormatContext* context) : base(context)
    {
    }


    #endregion

    /// <summary>
    /// Gets the input format associated with this demuxer.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> if the input format has not yet been
    /// determined.
    /// </remarks>
    public InputFormat? InputFormat => Context->iformat != null ? new(Context->iformat) : null;
 
    #region FindStreamInfo

    /// <summary>
    /// Reads packets from the input to gather stream information.
    /// </summary>
    /// <returns>
    /// The result returned by <c>avformat_find_stream_info()</c>.
    /// </returns>
    /// <remarks>
    /// This method analyzes the input streams and populates codec parameters,
    /// durations, frame rates, and other stream metadata. It should typically be
    /// called immediately after opening an input.
    /// </remarks>
    public AVResult32 FindStreamInfo() => ffmpeg.avformat_find_stream_info(Context, null);

    /// <summary>
    /// Reads packets from the input to gather stream information using per-stream
    /// option dictionaries.
    /// </summary>
    /// <param name="options">
    /// An array containing one option dictionary for each stream, or
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The result returned by <c>avformat_find_stream_info()</c>.
    /// </returns>
    public AVResult32 FindStreamInfo(Collections.AVDictionary[]? options) => FindStreamInfo(options != null ? options.AsSpan() : []);

    /// <summary>
    /// Reads packets from the input to gather stream information using per-stream
    /// option dictionaries.
    /// </summary>
    /// <param name="options">
    /// A span containing one option dictionary for each stream.
    /// </param>
    /// <returns>
    /// The result returned by <c>avformat_find_stream_info()</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the number of supplied option dictionaries does not equal
    /// <see cref="FormatContext.StreamCount"/>.
    /// </exception>
    public AVResult32 FindStreamInfo(Span<Collections.AVDictionary> options)
    {
        if (options == null || options.Length == 0)
            return ffmpeg.avformat_find_stream_info(Context, null);
        if (options.Length != StreamCount)
            throw new ArgumentOutOfRangeException(nameof(options));
        AutoGen._AVDictionary** dics = stackalloc AutoGen._AVDictionary*[StreamCount];
        for (int i = 0; i < StreamCount; i++)
            dics[i] = options[i].dictionary;
        int res = ffmpeg.avformat_find_stream_info(Context, dics);
        for (int i = 0; i < StreamCount; i++)
            options[i].dictionary = dics[i];
        return res;
    }

    /// <summary>
    /// Reads packets from the input to gather stream information using per-stream
    /// option dictionaries.
    /// </summary>
    /// <param name="options">
    /// An array containing one option dictionary for each stream, or
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The result returned by <c>avformat_find_stream_info()</c>.
    /// </returns>
    public AVResult32 FindStreamInfo(Collections.AVMultiDictionary[]? options)
        => FindStreamInfo(options != null ? options.AsSpan() : []);

    /// <summary>
    /// Reads packets from the input to gather stream information using per-stream
    /// option dictionaries.
    /// </summary>
    /// <param name="options">
    /// A span containing one option dictionary for each stream.
    /// </param>
    /// <returns>
    /// The result returned by <c>avformat_find_stream_info()</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the number of supplied option dictionaries does not equal
    /// <see cref="FormatContext.StreamCount"/>.
    /// </exception>
    public AVResult32 FindStreamInfo(Span<Collections.AVMultiDictionary> options)
    {
        if (options == null || options.Length == 0)
            return ffmpeg.avformat_find_stream_info(Context, null);
        if (options.Length != StreamCount)
            throw new ArgumentOutOfRangeException(nameof(options));
        AutoGen._AVDictionary** dics = stackalloc AutoGen._AVDictionary*[StreamCount];
        for (int i = 0; i < StreamCount; i++)
            dics[i] = options[i].dictionary;
        int res = ffmpeg.avformat_find_stream_info(Context, dics);
        for (int i = 0; i < StreamCount; i++)
            options[i].dictionary = dics[i];
        return res;
    }

    /// <summary>
    /// Reads packets from the input to gather stream information using managed
    /// dictionaries.
    /// </summary>
    /// <param name="options">
    /// A span containing one dictionary for each stream.
    /// </param>
    /// <returns>
    /// The result returned by <c>avformat_find_stream_info()</c>.
    /// </returns>
    /// <remarks>
    /// FFmpeg may consume or modify option entries. The supplied dictionaries are
    /// updated to reflect any changes made during the call.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the number of supplied dictionaries does not equal
    /// <see cref="FormatContext.StreamCount"/>.
    /// </exception>
    public AVResult32 FindStreamInfo(Span<IDictionary<string, string>> options)
    {
        if (options == null || options.Length == 0)
            return ffmpeg.avformat_find_stream_info(Context, null);
        if (options.Length != StreamCount)
            throw new ArgumentOutOfRangeException(nameof(options));
        Collections.AVDictionary[] dics = new Collections.AVDictionary[StreamCount];
        for (int i = 0; i < StreamCount; i++)
            dics[i] = new(options[i]);
        AVResult32 res = FindStreamInfo(dics);
        for (int i = 0; i < StreamCount; i++)
        {
            options[i].Clear();
            foreach (KeyValuePair<string, string> kv in dics[i])
                options[i][kv.Key] = kv.Value;
            dics[i].Dispose();
        }
        return res;
    }

    /// <summary>
    /// Reads packets from the input to gather stream information using managed
    /// dictionaries.
    /// </summary>
    /// <param name="options">
    /// A span containing one dictionary for each stream.
    /// </param>
    /// <returns>
    /// The result returned by <c>avformat_find_stream_info()</c>.
    /// </returns>
    /// <remarks>
    /// FFmpeg may consume or modify option entries. The supplied dictionaries are
    /// updated to reflect any changes made during the call.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the number of supplied dictionaries does not equal
    /// <see cref="FormatContext.StreamCount"/>.
    /// </exception>
    public AVResult32 FindStreamInfo(IDictionary<string, string>[]? options)
        => FindStreamInfo(options != null ? options.AsSpan() : []);

    #endregion

    #region ReadFrame

    /// <summary>
    /// Reads the next packet from the input.
    /// </summary>
    /// <param name="packet">
    /// The packet that receives the demultiplexed data.
    /// </param>
    /// <returns>
    /// The result returned by <c>av_read_frame()</c>.
    /// </returns>
    /// <remarks>
    /// The packet is automatically unreferenced before reading. If the packet does
    /// not already have a time base assigned, it is initialized from the
    /// corresponding stream.
    /// </remarks>
    public AVResult32 ReadPacket(AVPacket packet)
    {
        packet.Unreference(); // av_read_frame in contrast to receive frame/packet (codec context) does not unreference the packet
        AVResult32 result = ffmpeg.av_read_frame(Context, packet.packet);
        if (result.IsError)
            return result;
        if (packet.TimeBase.Numerator == 0) // set packet time base if not set
            packet.TimeBase = Context->streams[packet.StreamIndex]->time_base;
        return result;
    }

    /// <inheritdoc cref="ReadPacket(AVPacket)"/>
    [Obsolete("Renamed to ReadPacket")]
    public AVResult32 ReadFrame(AVPacket packet)
    {
        packet.Unreference(); // av_read_frame in contrast to receive frame/packet (codec context) does not unreference the packet
        AVResult32 result = ffmpeg.av_read_frame(Context, packet.packet);
        if (result.IsError)
            return result;
        if (packet.TimeBase.Numerator == 0) // set packet time base if not set
            packet.TimeBase = Context->streams[packet.StreamIndex]->time_base;
        return result;
    }


    #endregion

    #region GuessFrameRate

    /// <summary>
    /// Estimates the frame rate of the specified stream.
    /// </summary>
    /// <param name="avStream">
    /// The stream whose frame rate should be estimated.
    /// </param>
    /// <param name="frame">
    /// An optional decoded frame used to improve the estimation.
    /// </param>
    /// <returns>
    /// The estimated frame rate.
    /// </returns>
    public Rational GuessFrameRate(AVStream avStream, AVFrame? frame)
            => ffmpeg.av_guess_frame_rate(Context, avStream.stream, frame != null ? frame.Frame : null);

    /// <summary>
    /// Guesses the frame rate of the given stream.
    /// </summary>
    /// <param name="avStream">The stream to analyze.</param>
    /// <returns>A <see cref="Rational"/> representing the guessed frame rate.</returns>
    public Rational GuessFrameRate(AVStream avStream)
        => ffmpeg.av_guess_frame_rate(Context, avStream.stream, null);

    /// <inheritdoc cref = "GuessFrameRate(AVStream, AVFrame?)" /
    public Rational GuessFrameRate(int streamIndex, AVFrame? frame)
        => streamIndex < 0 || streamIndex >= StreamCount
            ? throw new ArgumentOutOfRangeException(nameof(streamIndex))
            : (Rational)ffmpeg.av_guess_frame_rate(Context, Context->streams[streamIndex], frame != null ? frame.Frame : null);

    /// <inheritdoc cref = "GuessFrameRate(AVStream, AVFrame?)" /
    public Rational GuessFrameRate(int streamIndex)
        => GuessFrameRate(streamIndex, null);

    #endregion

    #region Seek

    /// <summary>
    /// Seeks to the specified presentation time.
    /// </summary>
    /// <param name="time">
    /// The target presentation time.
    /// </param>
    /// <returns>
    /// The result returned by <c>av_seek_frame()</c>.
    /// </returns>
    /// <remarks>
    /// The seek is performed using FFmpeg's default stream selection and the
    /// <c>AVSEEK_FLAG_BACKWARD</c> flag.
    /// </remarks>
    public AVResult32 Seek(Rational time)
    {
        Rational timeBase = new(1, ffmpeg.AV_TIME_BASE);
        long l = (long)(time / timeBase);
        return ffmpeg.av_seek_frame(Context, -1, l, ffmpeg.AVSEEK_FLAG_BACKWARD);
    }

    /// <summary>
    /// Seeks to the specified presentation time within a stream.
    /// </summary>
    /// <param name="time">
    /// The target presentation time.
    /// </param>
    /// <param name="streamIndex">
    /// The zero-based stream index.
    /// </param>
    /// <returns>
    /// The result returned by <c>av_seek_frame()</c>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="streamIndex"/> is outside the valid range.
    /// </exception>
    public AVResult32 Seek(Rational time, int streamIndex)
    {
        if (streamIndex < 0 || streamIndex >= StreamCount)
            throw new ArgumentOutOfRangeException(nameof(streamIndex));
        Rational timeBase = Context->streams[streamIndex]->time_base;
        long l = (long)(time / timeBase);
        return ffmpeg.av_seek_frame(Context, streamIndex, l, ffmpeg.AVSEEK_FLAG_BACKWARD);
    }

    /// <summary>
    /// Seeks to the specified timestamp.
    /// </summary>
    /// <param name="timestamp">
    /// The target timestamp.
    /// </param>
    /// <returns>
    /// The result returned by <c>av_seek_frame()</c>.
    /// </returns>
    public AVResult32 Seek(long timestamp)
            => ffmpeg.av_seek_frame(Context, -1, timestamp, ffmpeg.AVSEEK_FLAG_FRAME | ffmpeg.AVSEEK_FLAG_BACKWARD);

    /// <summary>
    /// Seeks to the specified timestamp within a stream.
    /// </summary>
    /// <param name="timestamp">
    /// The target timestamp.
    /// </param>
    /// <param name="streamIndex">
    /// The zero-based stream index.
    /// </param>
    /// <returns>
    /// The result returned by <c>av_seek_frame()</c>.
    /// </returns>
    public AVResult32 Seek(long timestamp, int streamIndex)
            => ffmpeg.av_seek_frame(Context, streamIndex, timestamp, ffmpeg.AVSEEK_FLAG_FRAME | ffmpeg.AVSEEK_FLAG_BACKWARD);

    #endregion

    /// <summary>
    /// Gets the start timestamp of the input.
    /// </summary>
    /// <remarks>
    /// The value is expressed in <c>AV_TIME_BASE</c> units.
    /// </remarks>
    public long StartTime => Context->start_time; public long Duration => Context->duration;
    /// <summary>
    /// Gets the overall bit rate of the input, in bits per second.
    /// </summary>
    public long BitRate => Context->bit_rate;

    /// <inheritdoc/>
    public override ChapterList Chapters => new(this, true);
    // ToDo: Find Program, Add Program, FindBestStream overloads

    #region Dispose
    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
                ioContext?.Dispose();
            if (Context != null)
            {
                AutoGen._AVFormatContext* context = Context;
                ffmpeg.avformat_close_input(&context);
                Context = context;
            }
        }
        base.Dispose(disposing);

    }
    #endregion

    /// <inheritdoc />
    public override string ToString() => InputFormat?.LongName ?? "Unknown";
}
