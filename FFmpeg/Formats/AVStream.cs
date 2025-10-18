using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Formats;

/// <summary>
/// Managed wrapper for the FFmpeg AVStream structure.
/// </summary>
public unsafe class AVStream : Options.OptionQueryBase, IEquatable<AVStream?>
{
    internal readonly AutoGen._AVStream* stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVStream"/> class.
    /// </summary>
    /// <param name="stream">Pointer to the unmanaged AVStream structure.</param>
    public AVStream(AutoGen._AVStream* stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        this.stream = stream;
    }


    /// <summary>
    /// Stream index in AVFormatContext.
    /// </summary>
    public int Index => stream->index;

    /// <summary>
    /// Format-specific stream ID.
    /// </summary>
    public int Id
    {
        get => stream->id;
        set => stream->id = value;
    }

    /// <summary>
    /// Codec parameters associated with this stream.
    /// </summary>
    public Codecs.CodecParameters_ref CodecParameters => new(stream->codecpar);

    /// <summary>
    /// Fundamental unit of time (in seconds) in which timestamps are represented.
    /// </summary>
    public Rational TimeBase { get => stream->time_base; set => stream->time_base = value; }


    /// <summary>
    /// Presentation timestamp (PTS) of the first frame in the stream, in stream time base.
    /// </summary>
    public long StartTime { get => stream->start_time; set => stream->start_time = value; }


    /// <summary>
    /// Duration of the stream, in stream time base units.
    /// </summary>
    public long Duration { get => stream->duration; set => stream->duration = value; }


    /// <summary>
    /// Number of frames in this stream, if known.
    /// </summary>
    public long NumberOfFrames
    {
        get => stream->nb_frames;
        set => stream->nb_frames = value;
    }

    /// <summary>
    /// Stream disposition flags (a combination of AV_DISPOSITION_* flags).
    /// </summary>
    public StreamDisposition Disposition
    {
        get => (StreamDisposition)stream->disposition;
        set => stream->disposition = (int)value;
    }

    /// <summary>
    /// Specifies which packets can be discarded at will.
    /// </summary>
    public DiscardFlags Discard
    {
        get => (DiscardFlags)stream->discard;
        set => stream->discard = (_AVDiscard)value;
    }

    /// <summary>
    /// Sample aspect ratio of the stream.
    /// </summary>
    public Rational SampleAspectRatio
    {
        get => stream->sample_aspect_ratio;
        set => stream->sample_aspect_ratio = value;
    }

    /// <summary>
    /// Metadata associated with this stream.
    /// </summary>
    public Collections.AVDictionary_ref Metadata => new(&stream->metadata, ignoreCase: true, ignoreSuffix: false);

    /// <summary>
    /// Average frame rate of the stream.
    /// </summary>
    public Rational AverageFrameRate
    {
        get => stream->avg_frame_rate;
        set => stream->avg_frame_rate = value;
    }

    /// <summary>
    /// Attached picture for streams with AV_DISPOSITION_ATTACHED_PIC disposition.
    /// </summary>
    public AVPacket_ref AttachedPicture => new(&stream->attached_pic);

    /// <summary>
    /// Flags indicating events happening on the stream.
    /// </summary>
    public int EventFlags
    {
        get => stream->event_flags;
        set => stream->event_flags = value;
    }

    /// <summary>
    /// Real base framerate of the stream.
    /// </summary>
    public Rational RealFrameRate => stream->r_frame_rate;

    /// <summary>
    /// Number of bits in timestamps used for wrapping control.
    /// </summary>
    public int PtsWrapBits => stream->pts_wrap_bits;

    /// <inheritdoc cref="FFmpeg.Codecs.CodecParameters.MediaType"/>
    public MediaType MediaType
    {
        get => CodecParameters.MediaType; set
        {
            var codecParameters = CodecParameters;
            codecParameters.MediaType = value;
        }
    }

    /// <summary>
    /// Gets the specific codec ID used for the encoded data.
    /// </summary>
    public Codecs.CodecID CodecId
    {
        get => CodecParameters.CodecId;
    }

    protected override unsafe void* Pointer => stream;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AVStream);
    /// <inheritdoc />
    public bool Equals(AVStream? other) => other is not null && EqualityComparer<nint>.Default.Equals((nint)stream, (nint)other.stream);
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine((nint)stream);

    /// <inheritdoc />
    public static bool operator ==(AVStream? left, AVStream? right) => EqualityComparer<AVStream?>.Default.Equals(left, right);

    /// <inheritdoc />
    public static bool operator !=(AVStream? left, AVStream? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => $"{CodecParameters.MediaType}/{CodecParameters.CodecId}";
}
