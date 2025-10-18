using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Codecs;

/// <summary>
/// A managed wrapper around the unmanaged FFmpeg _AVCodecParameters structure, 
/// providing safe access to codec parameters.
/// </summary>
public sealed unsafe class CodecParameters : IDisposable, ICodecParameters
{
    internal AutoGen._AVCodecParameters* codecParameters;
    _AVCodecParameters* ICodecParameters.Parameters => codecParameters;


    /// <summary>
    /// Initializes a new instance of the <see cref="CodecParameters"/> class,
    /// allocating a new _AVCodecParameters structure and setting its fields to default values.
    /// </summary>
    /// <exception cref="OutOfMemoryException">Thrown if the underlying _AVCodecParameters cannot be allocated.</exception>
    public CodecParameters()
    {
        codecParameters = ffmpeg.avcodec_parameters_alloc();
        if (codecParameters == null)
        {
            throw new OutOfMemoryException("Unable to allocate _AVCodecParameters.");
        }
    }

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
    internal CodecParameters(AutoGen._AVCodecParameters* existingPtr)
    {
        if (existingPtr == null)
            throw new ArgumentNullException(nameof(existingPtr));
        else codecParameters = existingPtr;

    }

    /// <summary>
    /// Frees the _AVCodecParameters instance and everything associated with it.
    /// </summary>
    public void Dispose()
    {
        if (codecParameters != null)
        {
            AutoGen._AVCodecParameters* codecParameter = codecParameters;
            ffmpeg.avcodec_parameters_free(&codecParameter);
            codecParameters = codecParameter;
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer for the <see cref="CodecParameters"/> class.
    /// Ensures resources are freed when the object is garbage collected.
    /// </summary>
    ~CodecParameters()
    {
        Dispose();
    }

    /// <summary>
    /// Copies the contents of another <see cref="CodecParameters"/> instance to this instance.
    /// </summary>
    /// <param name="other">The other <see cref="CodecParameters"/> instance to copy from.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="other"/> is null.</exception>
    /// <exception cref="ApplicationException">Thrown if copying fails.</exception>
    public void CopyFrom(ICodecParameters other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

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
        if (other == null) throw new ArgumentNullException(nameof(other));

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
        if (other == null) throw new ArgumentNullException(nameof(other));

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
                ? ([])
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

    /// <summary>
    /// Gets or sets the average bitrate of the encoded data, in bits per second.
    /// </summary>
    public int BitRate
    {
        get => (int)codecParameters->bit_rate;
        set => codecParameters->bit_rate = value;
    }

    /// <summary>
    /// Gets or sets the number of bits per coded sample in the bitstream.
    /// </summary>
    public int BitsPerCodedSample
    {
        get => codecParameters->bits_per_coded_sample;
        set => codecParameters->bits_per_coded_sample = value;
    }

    /// <summary>
    /// Gets or sets the number of bits per raw sample in the output.
    /// </summary>
    public int BitsPerRawSample
    {
        get => codecParameters->bits_per_raw_sample;
        set => codecParameters->bits_per_raw_sample = value;
    }

    /// <summary>
    /// Gets or sets the codec-specific profile.
    /// </summary>
    public Profile Profile
    {
        get => new(CodecId, codecParameters->profile);
        set => codecParameters->profile = value.ProfileID;
    }

    /// <summary>
    /// Gets or sets the codec-specific level.
    /// </summary>
    public int Level
    {
        get => codecParameters->level;
        set => codecParameters->level = value;
    }

    /// <summary>
    /// Gets or sets the width of the video frame in pixels.
    /// </summary>
    public int Width
    {
        get => codecParameters->width;
        set => codecParameters->width = value;
    }

    /// <summary>
    /// Gets or sets the height of the video frame in pixels.
    /// </summary>
    public int Height
    {
        get => codecParameters->height;
        set => codecParameters->height = value;
    }

    /// <summary>
    /// Gets or sets the sample aspect ratio (width/height) of a single pixel.
    /// </summary>
    public Rational SampleAspectRatio
    {
        get => codecParameters->sample_aspect_ratio;
        set => codecParameters->sample_aspect_ratio = value;
    }

    /// <summary>
    /// Gets or sets the frame rate (frames per second) for video streams with constant frame durations.
    /// </summary>
    public Rational FrameRate
    {
        get => codecParameters->framerate;
        set => codecParameters->framerate = value;
    }

    /// <summary>
    /// Gets or sets the order of the fields in interlaced video.
    /// </summary>
    public FieldOrder FieldOrder
    {
        get => (FieldOrder)codecParameters->field_order;
        set => codecParameters->field_order = (_AVFieldOrder)value;
    }

    /// <summary>
    /// Gets or sets the color range of the video.
    /// </summary>
    public ColorRange ColorRange
    {
        get => (ColorRange)codecParameters->color_range;
        set => codecParameters->color_range = (_AVColorRange)value;
    }

    /// <summary>
    /// Gets or sets the color primaries of the video.
    /// </summary>
    public ColorPrimaries ColorPrimaries
    {
        get => (ColorPrimaries)codecParameters->color_primaries;
        set => codecParameters->color_primaries = (_AVColorPrimaries)value;
    }

    /// <summary>
    /// Gets or sets the color transfer characteristic of the video.
    /// </summary>
    public ColorTransferCharacteristic ColorTransferCharacteristic
    {
        get => (ColorTransferCharacteristic)codecParameters->color_trc;
        set => codecParameters->color_trc = (_AVColorTransferCharacteristic)value;
    }

    /// <summary>
    /// Gets or sets the color space of the video.
    /// </summary>
    public ColorSpace ColorSpace
    {
        get => (ColorSpace)codecParameters->color_space;
        set => codecParameters->color_space = (_AVColorSpace)value;
    }

    /// <summary>
    /// Gets or sets the chroma location in the video.
    /// </summary>
    public ChromaLocation ChromaLocation
    {
        get => (ChromaLocation)codecParameters->chroma_location;
        set => codecParameters->chroma_location = (_AVChromaLocation)value;
    }

    /// <summary>
    /// Gets or sets the number of frames to delay video frames.
    /// </summary>
    public int VideoDelay
    {
        get => codecParameters->video_delay;
        set => codecParameters->video_delay = value;
    }

    /// <summary>
    /// Gets or sets the audio sample rate (number of samples per second).
    /// </summary>
    public int SampleRate
    {
        get => codecParameters->sample_rate;
        set => codecParameters->sample_rate = value;
    }

    /// <summary>
    /// Gets or sets the number of bytes per coded audio frame.
    /// </summary>
    public int BlockAlign
    {
        get => codecParameters->block_align;
        set => codecParameters->block_align = value;
    }

    /// <summary>
    /// Gets or sets the number of audio samples per frame, if known.
    /// </summary>
    public int FrameSize
    {
        get => codecParameters->frame_size;
        set => codecParameters->frame_size = value;
    }

    /// <summary>
    /// Gets or sets the amount of padding inserted by the encoder at the beginning of the audio stream.
    /// </summary>
    public int InitialPadding
    {
        get => codecParameters->initial_padding;
        set => codecParameters->initial_padding = value;
    }

    /// <summary>
    /// Gets or sets the amount of padding appended by the encoder at the end of the audio stream.
    /// </summary>
    public int TrailingPadding
    {
        get => codecParameters->trailing_padding;
        set => codecParameters->trailing_padding = value;
    }

    /// <summary>
    /// Gets or sets the number of samples to skip after a discontinuity.
    /// </summary>
    public int SeekPreroll
    {
        get => codecParameters->seek_preroll;
        set => codecParameters->seek_preroll = value;
    }


    /// <summary>
    /// Calculates the duration of an audio frame, given the frame size in bytes.
    /// </summary>
    /// <param name="frameBytes">The size of the audio frame in bytes.</param>
    /// <returns>The duration of the audio frame in milliseconds.</returns>
    public int GetAudioFrameDuration(int frameBytes) => ffmpeg.av_get_audio_frame_duration2(codecParameters, frameBytes);

    /// <summary>
    /// Gets or sets the sample format of the codec parameters.
    /// </summary>
    /// <remarks>The sample format determines how audio samples are stored and processed.  Ensure that the
    /// assigned value is compatible with the codec being used.</remarks>
    public SampleFormat SampleFormat
    {
        get => (SampleFormat)codecParameters->format;
        set => codecParameters->format = (int)value;
    }

    /// <summary>
    /// Gets or sets the pixel format of the codec parameters.
    /// </summary>
    /// <remarks>The pixel format determines how video frames are stored and processed.  Ensure that the
    /// assigned value is compatible with the codec being used.</remarks>
    public PixelFormat PixelFormat
    {
        get => (PixelFormat)codecParameters->format;
        set => codecParameters->format = (int)value;
    }

    /// <inheritdoc />
    public int Channels => codecParameters->ch_layout.nb_channels;

    /// <inheritdoc/>
    public ChannelLayout ChannelLayout
    {
        get => new(codecParameters->ch_layout, false);
        set
        {
            var layout = value.layout;
            ffmpeg.av_channel_layout_uninit(&codecParameters->ch_layout);
            ffmpeg.av_channel_layout_copy(&codecParameters->ch_layout, &layout);
        }
    }
}
