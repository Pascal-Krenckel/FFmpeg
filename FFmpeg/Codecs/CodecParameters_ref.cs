using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Codecs;
/// <summary>
/// A managed wrapper around the unmanaged FFmpeg _AVCodecParameters structure, 
/// providing safe access to codec parameters.
/// </summary>
public unsafe struct CodecParameters_ref : ICodecParameters
{
    internal AutoGen._AVCodecParameters* codecParameters;
    AutoGen._AVCodecParameters* ICodecParameters.Parameters => codecParameters;



    /// <summary>
    /// Initializes a new instance of the <see cref="CodecParameters"/> class by wrapping 
    /// an existing unmanaged _AVCodecParameters pointer.
    /// </summary>
    /// <param name="existingPtr">Pointer to an existing _AVCodecParameters structure.</param>
    /// <param name="takeOwnership">
    /// If true, the wrapper will take ownership of the existing pointer and manage its lifetime.
    /// If false, a new _AVCodecParameters structure is allocated and its fields are copied from the existing pointer.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="existingPtr"/> is null.</exception>
    /// <exception cref="OutOfMemoryException">Thrown if memory allocation fails during copying.</exception>
    internal CodecParameters_ref(AutoGen._AVCodecParameters* existingPtr) => codecParameters = existingPtr == null ? throw new ArgumentNullException(nameof(existingPtr)) : existingPtr;

    /// <summary>
    /// Copies the contents of another <see cref="CodecParameters"/> instance to this instance.
    /// </summary>
    /// <param name="other">The other <see cref="CodecParameters"/> instance to copy from.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="other"/> is null.</exception>
    /// <exception cref="ApplicationException">Thrown if copying fails.</exception>
    public void CopyFrom(ICodecParameters other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        int result = ffmpeg.avcodec_parameters_copy(codecParameters, other.Parameters);
        if (result < 0)
        {
            throw new ApplicationException($"Error copying _AVCodecParameters: {result}");
        }
    }

    /// <summary>
    /// Copies the contents of a <see cref="CodecContext"/> instance to this instance.
    /// </summary>
    /// <param name="other">The <see cref="CodecContext"/> instance to copy from.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="other"/> is null.</exception>
    /// <exception cref="ApplicationException">Thrown if copying fails.</exception>
    public void CopyFrom(CodecContext other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        int result = ffmpeg.avcodec_parameters_from_context(codecParameters, other.context);
        if (result < 0)
        {
            throw new ApplicationException($"Error copying _AVCodecParameters: {result}");
        }
    }

    /// <summary>
    /// Copies the contents of this instance to a specified <see cref="CodecContext"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="CodecContext"/> instance to copy to.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="other"/> is null.</exception>
    /// <exception cref="ApplicationException">Thrown if copying fails.</exception>
    public void CopyTo(CodecContext other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        int result = ffmpeg.avcodec_parameters_to_context(other.context, codecParameters);
        if (result < 0)
        {
            throw new ApplicationException($"Error copying to _AVCodecContext: {result}");
        }
    }


    /// <summary>
    /// Gets or sets the general type of the encoded data (e.g., video, audio).
    /// </summary>
    public MediaType MediaType
    {
        get => (MediaType)codecParameters->codec_type;
        set => codecParameters->codec_type = (_AVMediaType)value;
    }

    /// <summary>
    /// Gets or sets the specific codec ID used for the encoded data.
    /// </summary>
    public CodecID CodecId
    {
        get => (CodecID)codecParameters->codec_id;
        set => codecParameters->codec_id = (_AVCodecID)value;
    }

    /// <summary>
    /// Gets or sets the codec tag, which is additional information about the codec (corresponding to the AVI FOURCC).
    /// </summary>
    public FourCC CodecTag
    {
        get => codecParameters->codec_tag;
        set => codecParameters->codec_tag = value;
    }

    /// <summary>
    /// Gets the extra binary data needed for initializing the decoder.
    /// </summary>
    public Span<byte> ExtraData => codecParameters->extradata_size == 0 || codecParameters->extradata == null
                ? []
                : new Span<byte>(codecParameters->extradata, codecParameters->extradata_size);

    /// <summary>
    /// Sets the extra binary data needed for initializing the decoder.
    /// </summary>
    /// <param name="data">The extra binary data to set.</param>
    /// <exception cref="OutOfMemoryException">Thrown if memory allocation fails.</exception>
    public void SetExtraData(ReadOnlySpan<byte> data)
    {
        ffmpeg.av_freep(&codecParameters->extradata);
        codecParameters->extradata_size = data.Length;
        if (!data.IsEmpty)
        {
            codecParameters->extradata = (byte*)ffmpeg.av_malloc((ulong)data.Length);
            if (codecParameters->extradata == null)
            {
                codecParameters->extradata_size = 0;
                throw new OutOfMemoryException("Unable to allocate memory for extra data.");
            }
            data.CopyTo(ExtraData);
        }
    }

    /// <inheritdoc/>
    public int BitRate
    {
        get => (int)codecParameters->bit_rate;
        set => codecParameters->bit_rate = value;
    }

    /// <inheritdoc/>
    public int BitsPerCodedSample
    {
        get => codecParameters->bits_per_coded_sample;
        set => codecParameters->bits_per_coded_sample = value;
    }

    /// <inheritdoc/>
    public int BitsPerRawSample
    {
        get => codecParameters->bits_per_raw_sample;
        set => codecParameters->bits_per_raw_sample = value;
    }

    /// <inheritdoc/>
    public Profile Profile
    {
        get => new(CodecId, codecParameters->profile);
        set => codecParameters->profile = value.ProfileID;
    }

    /// <inheritdoc/>
    public int Level
    {
        get => codecParameters->level;
        set => codecParameters->level = value;
    }

    /// <inheritdoc/>
    public int Width
    {
        get => codecParameters->width;
        set => codecParameters->width = value;
    }

    /// <inheritdoc/>
    public int Height
    {
        get => codecParameters->height;
        set => codecParameters->height = value;
    }

    /// <inheritdoc/>
    public Rational SampleAspectRatio
    {
        get => codecParameters->sample_aspect_ratio;
        set => codecParameters->sample_aspect_ratio = value;
    }

    /// <inheritdoc/>
    public SampleFormat SampleFormat
    {
        get => (SampleFormat)codecParameters->format;
        set => codecParameters->format = (int)value;
    }

    /// <inheritdoc/>
    public PixelFormat PixelFormat
    {
        get => (PixelFormat)codecParameters->format;
        set => codecParameters->format = (int)value;
    }

    /// <inheritdoc/>
    public Rational FrameRate
    {
        get => codecParameters->framerate;
        set => codecParameters->framerate = value;
    }

    /// <inheritdoc/>
    public FieldOrder FieldOrder
    {
        get => (FieldOrder)codecParameters->field_order;
        set => codecParameters->field_order = (_AVFieldOrder)value;
    }

    /// <inheritdoc/>
    public ColorRange ColorRange
    {
        get => (ColorRange)codecParameters->color_range;
        set => codecParameters->color_range = (_AVColorRange)value;
    }

    /// <inheritdoc/>
    public ColorPrimaries ColorPrimaries
    {
        get => (ColorPrimaries)codecParameters->color_primaries;
        set => codecParameters->color_primaries = (_AVColorPrimaries)value;
    }

    /// <inheritdoc/>
    public ColorTransferCharacteristic ColorTransferCharacteristic
    {
        get => (ColorTransferCharacteristic)codecParameters->color_trc;
        set => codecParameters->color_trc = (_AVColorTransferCharacteristic)value;
    }

    /// <inheritdoc/>
    public ColorSpace ColorSpace
    {
        get => (ColorSpace)codecParameters->color_space;
        set => codecParameters->color_space = (_AVColorSpace)value;
    }
    /// <inheritdoc/>
    public ChromaLocation ChromaLocation
    {
        get => (ChromaLocation)codecParameters->chroma_location;
        set => codecParameters->chroma_location = (_AVChromaLocation)value;
    }

    /// <inheritdoc/>
    public int VideoDelay
    {
        get => codecParameters->video_delay;
        set => codecParameters->video_delay = value;
    }

    /// <inheritdoc/>
    public int SampleRate
    {
        get => codecParameters->sample_rate;
        set => codecParameters->sample_rate = value;
    }

    /// <inheritdoc/>
    public int BlockAlign
    {
        get => codecParameters->block_align;
        set => codecParameters->block_align = value;
    }

    /// <inheritdoc/>
    public int FrameSize
    {
        get => codecParameters->frame_size;
        set => codecParameters->frame_size = value;
    }

    /// <inheritdoc/>
    public int InitialPadding
    {
        get => codecParameters->initial_padding;
        set => codecParameters->initial_padding = value;
    }

    /// <inheritdoc/>
    public int TrailingPadding
    {
        get => codecParameters->trailing_padding;
        set => codecParameters->trailing_padding = value;
    }

    /// <inheritdoc/>
    public int SeekPreroll
    {
        get => codecParameters->seek_preroll;
        set => codecParameters->seek_preroll = value;
    }

    /// <inheritdoc />
    public int Channels => codecParameters->ch_layout.nb_channels;

    /// <inheritdoc/>
    public ChannelLayout ChannelLayout
    {
        get => new(codecParameters->ch_layout, false);
        set
        {
            _AVChannelLayout layout = value.layout;
            ffmpeg.av_channel_layout_uninit(&codecParameters->ch_layout);
            _ = ffmpeg.av_channel_layout_copy(&codecParameters->ch_layout, &layout);
        }
    }


    /// <inheritdoc/>
    public readonly int GetAudioFrameDuration(int frameBytes) => ffmpeg.av_get_audio_frame_duration2(codecParameters, frameBytes);
}
