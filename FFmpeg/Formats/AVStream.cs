using FFmpeg.AutoGen;
using FFmpeg.Codecs;
using FFmpeg.Unsafe;
using FFmpeg.Utils;

namespace FFmpeg.Formats;

/// <summary>
/// Represents a single media stream within a container.
/// A stream typically contains encoded audio, video, subtitles, or other media data,
/// together with its timing information, codec parameters, and metadata.
/// </summary>
public unsafe class AVStream : Options.OptionQueryableBase, IEquatable<AVStream?>, IAVPointer<_AVStream>
{
    internal readonly AutoGen._AVStream* stream;
    unsafe _AVStream* IAVPointer<_AVStream>.Pointer => stream;


    /// <summary>
    /// Initializes a new instance of the <see cref="AVStream"/> class that wraps an existing unmanaged
    /// <see cref="AutoGen._AVStream"/> structure.
    /// </summary>
    /// <param name="stream">A pointer to the unmanaged FFmpeg stream.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public AVStream(AutoGen._AVStream* stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        this.stream = stream;
    }


    /// <summary>
    /// Gets the zero-based index of this stream within its containing format context.
    /// </summary>
    public int Index => stream->index;

    /// <summary>
    /// Gets or sets the container-specific stream identifier.
    /// The meaning of this value depends on the container format.
    /// </summary>
    public int Id
    {
        get => stream->id;
        set => stream->id = value;
    }

    /// <summary>
    /// Gets a reference to the codec parameters describing the encoded data in this stream.
    /// These parameters include the codec, media type, dimensions, sample rate, channel layout,
    /// and other codec-specific information.
    /// </summary>
    public Codecs.CodecParameters_ref CodecParameters => new(stream->codecpar);

    /// <summary>
    /// Gets or sets the stream time base.
    /// All timestamps stored in this stream, such as <see cref="StartTime"/> and
    /// <see cref="Duration"/>, are expressed in this time base.
    /// </summary>
    public Rational TimeBase { get => stream->time_base; set => stream->time_base = value; }


    /// <summary>
    /// Gets or sets the presentation timestamp of the first frame in the stream,
    /// expressed in <see cref="TimeBase"/> units.
    /// A value of <c>AV_NOPTS_VALUE</c> indicates that the start time is unknown.
    /// </summary>
    public long StartTime { get => stream->start_time; set => stream->start_time = value; }


    /// <summary>
    /// Gets or sets the duration of the stream,
    /// expressed in <see cref="TimeBase"/> units.
    /// A value of 0 or <c>AV_NOPTS_VALUE</c> may indicate that the duration is unknown.
    /// </summary>
    public long Duration { get => stream->duration; set => stream->duration = value; }


    /// <summary>
    /// Gets or sets the number of frames contained in the stream, if known.
    /// Some demuxers may leave this value unset.
    /// </summary>
    public long NumberOfFrames
    {
        get => stream->nb_frames;
        set => stream->nb_frames = value;
    }

    /// <summary>
    /// Gets or sets the stream disposition flags.
    /// These flags describe special characteristics of the stream,
    /// such as whether it is the default stream, forced, attached picture, or hearing impaired.
    /// </summary>
    public StreamDisposition Disposition
    {
        get => (StreamDisposition)stream->disposition;
        set => stream->disposition = (int)value;
    }

    /// <summary>
    /// Gets or sets which packets from this stream may be discarded during decoding.
    /// </summary>
    public DiscardFlags Discard
    {
        get => (DiscardFlags)stream->discard;
        set => stream->discard = (_AVDiscard)value;
    }

    /// <summary>
    /// Gets or sets the sample aspect ratio of the stream.
    /// This describes the aspect ratio of individual pixels rather than the displayed image.
    /// </summary>
    public Rational SampleAspectRatio
    {
        get => stream->sample_aspect_ratio;
        set => stream->sample_aspect_ratio = value;
    }

    /// <summary>
    /// Gets the metadata associated with this stream.
    /// </summary>
    public Collections.AVDictionary_ref Metadata => new(&stream->metadata, ignoreCase: true, ignoreSuffix: false);

    /// <summary>
    /// Gets or sets the average frame rate of the stream.
    /// For variable frame rate content this value represents the average rather than the instantaneous frame rate.
    /// </summary>
    public Rational AverageFrameRate
    {
        get => stream->avg_frame_rate;
        set => stream->avg_frame_rate = value;
    }

    /// <summary>
    /// Gets a reference to the attached picture associated with this stream.
    /// This is primarily used by formats that store album artwork or thumbnails.
    /// </summary>
    public AVPacket_ref AttachedPicture => new(&stream->attached_pic);

    /// <summary>
    /// Gets or sets event flags reported by FFmpeg for this stream.
    /// These flags are used internally to notify the application of stream changes.
    /// </summary>
    public int EventFlags
    {
        get => stream->event_flags;
        set => stream->event_flags = value;
    }

    /// <summary>
    /// Gets the estimated real base frame rate of the stream.
    /// This value is primarily intended for timing calculations and may differ from
    /// <see cref="AverageFrameRate"/>.
    /// </summary>
    public Rational RealFrameRate => stream->r_frame_rate;

    /// <summary>
    /// Gets the number of bits used for presentation timestamp wrapping.
    /// This is primarily relevant for container formats with limited timestamp ranges.
    /// </summary>
    public int PtsWrapBits => stream->pts_wrap_bits;

    /// <inheritdoc cref="FFmpeg.Codecs.CodecParameters.MediaType"/>
    public MediaType MediaType
    {
        get => CodecParameters.MediaType; set
        {
            CodecParameters_ref codecParameters = CodecParameters;
            codecParameters.MediaType = value;
        }
    }

    /// <summary>
    /// Gets the codec identifier describing the encoded data in this stream.
    /// </summary>
    public Codecs.CodecID CodecId => CodecParameters.CodecId;

    /// <summary>
    /// The pointer to the unmanaged _AVStream object
    /// </summary>
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
