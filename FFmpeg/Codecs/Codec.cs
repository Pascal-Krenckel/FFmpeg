using FFmpeg.AutoGen;
using FFmpeg.Audio;
using FFmpeg.Utils;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using FFmpeg.Images;
using FFmpeg.Logging;

namespace FFmpeg.Codecs;

/// <summary>
/// Represents a codec in FFmpeg, providing access to its core information such as name, capabilities, 
/// supported formats, and additional metadata. This struct allows interaction with codec details 
/// in a managed, type-safe manner.
/// </summary>
public readonly unsafe struct Codec : IEquatable<Codec>
{

    /// <summary>
    /// Pointer to the native FFmpeg <see cref="AutoGen._AVCodec"/> structure that holds the codec information.
    /// </summary>
    internal readonly AutoGen._AVCodec* codec;


    /// <summary>
    /// Initializes a new instance of the <see cref="Codec"/> struct, wrapping a native 
    /// FFmpeg <see cref="AutoGen._AVCodec"/> pointer.
    /// </summary>
    /// <param name="codec">Pointer to the native <see cref="AutoGen._AVCodec"/> structure representing the codec.</param>
    internal Codec(AutoGen._AVCodec* codec) => this.codec = codec;

    /// <summary>
    /// Gets the short, symbolic name of the codec implementation (e.g., "h264", "aac"). 
    /// This name is typically used in code or configuration files to reference the codec.
    /// </summary>
    public string Name => codec != null ? Marshal.PtrToStringUTF8((nint)codec->name) : "UNKNOWN";

    /// <summary>
    /// Gets the human-readable, descriptive name of the codec (e.g., "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10").
    /// This name is more verbose than <see cref="Name"/> and provides better clarity for end users.
    /// </summary>
    public string LongName => codec != null ? Marshal.PtrToStringUTF8((nint)codec->long_name) : "UNKNOWN";

    /// <summary>
    /// Gets the capabilities of the codec, represented by a set of flags indicating what the codec supports.
    /// These capabilities can include features such as hardware acceleration, variable frame rates, 
    /// or support for certain compression techniques.
    /// </summary>
    public CodecCapabilities Capabilities => codec != null ? (CodecCapabilities)codec->capabilities : default;

    /// <summary>
    /// Gets the maximum value for low-resolution scaling supported by the codec. 
    /// This is used to determine how much a video can be downscaled for low-resolution playback, 
    /// particularly in situations where lower resolutions are required to improve performance.
    /// </summary>
    public byte MaxLowResolution => codec != null ? codec->max_lowres : (byte)0;

    /// <summary>
    /// Gets the media type of the codec (e.g., video, audio, subtitle). 
    /// This value indicates the kind of media data that the codec handles, allowing it to be 
    /// categorized appropriately in multimedia processing workflows.
    /// </summary>
    public MediaType MediaType => codec != null ? (MediaType)codec->type : MediaType.Unknown;

    /// <summary>
    /// Gets the codec ID, which uniquely identifies the codec within the FFmpeg framework. 
    /// The codec ID is used to match the codec with the corresponding media stream format or 
    /// container, ensuring correct decoding or encoding.
    /// </summary>
    public CodecID CodecID => codec != null ? (CodecID)codec->id : CodecID.None;

    /// <summary>
    /// Gets the array of supported framerates for the codec.
    /// The framerates represent the possible frame-per-second (FPS) values that the codec can handle. 
    /// These values are returned as an array of rational numbers, allowing precise representation of fractional framerates.
    /// </summary>    
    public ReadOnlySpan<Rational> SupportedFramerates
    {
        get
        {
            if (codec == null) return [];
            void* output = null;
            System.Diagnostics.Debug.Assert(sizeof(ulong) == sizeof(Rational));
            AVResult32 res = ffmpeg.avcodec_get_supported_config(null, codec, _AVCodecConfig.AV_CODEC_CONFIG_FRAME_RATE, 0, &output, null);
            if(res.IsError || output == null)
            {
                return [];
            }
           
            return new(output, res);
        }
    }

    /// <summary>
    /// Gets the array of supported pixel formats for the codec.
    /// This array defines the pixel formats the codec can encode or decode, 
    /// which control how the image data is represented (e.g., RGB, YUV, etc.).
    /// </summary>
    public ReadOnlySpan<PixelFormat> SupportedPixelFormats
    {
        get
        {
            if (codec == null) return null;
            System.Diagnostics.Debug.Assert(sizeof(_AVPixelFormat) == sizeof(PixelFormat));

            void* output = null;
            AVResult32 res = ffmpeg.avcodec_get_supported_config(null, codec, _AVCodecConfig.AV_CODEC_CONFIG_PIX_FORMAT, 0, &output, null);
            if (output == null || res.IsError)
                return [];
            return new(output, res);
        }
    }

    // ToDo:
    public PixelFormat GetBestPixelFormat(PixelFormat src, bool alphaUsed, out FFLoss loss)
    {
        _AVPixelFormat* output = null;
        AVResult32 res = ffmpeg.avcodec_get_supported_config(null, codec, _AVCodecConfig.AV_CODEC_CONFIG_PIX_FORMAT, 0, (void**)&output, null);
        if (output == null)
        {
            loss = FFLoss.None;
            return PixelFormat.None;
        }
        int _loss;
        int alpha = Convert.ToInt32(alphaUsed);
        var pixFmt = (PixelFormat)ffmpeg.avcodec_find_best_pix_fmt_of_list(output, (_AVPixelFormat)src, alpha, &_loss);
        loss = (FFLoss)_loss;
        return pixFmt;
    }

    public PixelFormat GetBestPixelFormat(PixelFormat src, bool alphaUsed) => GetBestPixelFormat(src, alphaUsed, out _);
    public PixelFormat GetBestPixelFormat(PixelFormat src, out FFLoss loss) => GetBestPixelFormat(src, true, out loss);

    public PixelFormat GetBestPixelFormat(PixelFormat src) => GetBestPixelFormat(src, true, out _);


    /// <summary>
    /// Gets the array of supported audio sample rates for the codec.
    /// These are the sample rates (in Hz) that the codec supports for audio processing, 
    /// which determine the frequency at which audio data is captured or played back.
    /// </summary>
    public ReadOnlySpan<int> SupportedSampleRates
    {
        get
        {
            if (codec == null) return null;
            void* output = null;
            AVResult32 res = ffmpeg.avcodec_get_supported_config(null, codec, _AVCodecConfig.AV_CODEC_CONFIG_SAMPLE_RATE, 0, &output, null);
            if(res.IsError || output == null)
            {
                return [];
            }
            return new ReadOnlySpan<int>(output, res);
        }
    }

    /// <summary>
    /// Gets the array of supported audio sample formats for the codec.
    /// The sample formats specify how audio data is represented in memory (e.g., signed 16-bit, 
    /// floating point, planar, etc.), and this array lists all formats that the codec can process.
    /// </summary>
    public ReadOnlySpan<SampleFormat> SupportedSampleFormats
    {
        get
        {
            if (codec == null) return null;
            System.Diagnostics.Debug.Assert(sizeof(SampleFormat) == sizeof(_AVSampleFormat));
            _AVSampleFormat* ptr = null;
            AVResult32 length = ffmpeg.avcodec_get_supported_config(null, codec, _AVCodecConfig.AV_CODEC_CONFIG_SAMPLE_FORMAT, 0, (void**)&ptr, null);

            if (ptr == null || length.IsError)
                return [];

            return new(ptr, length);
        }
    }


    /// <summary>
    /// Gets the array of recognized profiles for the codec.
    /// Each profile defines a set of features or capabilities that the codec can use during encoding or decoding.
    /// This array lists the profiles that the codec supports, allowing you to choose the appropriate one for your use case.
    /// </summary>
    public ReadOnlyCollection<Profile> Profiles
    {
        get
        {  
          
            _AVProfile* ptr;
            if (codec == null || (ptr = codec->profiles) == null)
                return new([]);
            List<Profile> profiles = [];
            // Calculate the length of the array
            int length = 0;
            while (ptr[length] != Profile.Unknown)
                profiles.Add(new(ptr[length++]));

            return new(profiles);
        }
    }

    /// <summary>
    /// Gets the group name of the codec implementation.
    /// This is a short, symbolic name that identifies the codec's wrapper or implementation, 
    /// such as a specific library or framework backing the codec. 
    /// If the field is null, the codec is native to FFmpeg.
    /// </summary>
    public string? WrapperName => codec != null ? Marshal.PtrToStringUTF8((nint)codec->wrapper_name) : null;

    /// <summary>
    /// Gets the array of supported channel layouts for the codec.
    /// A channel layout describes how audio channels are positioned and ordered, such as stereo (left, right), 5.1 surround sound, etc.
    /// This property returns the set of channel layouts that the codec can handle, which is useful for ensuring correct audio processing.
    /// </summary>
    public ChannelLayoutConstList ChannelLayouts
    {
        get
        {
            _AVChannelLayout* ptr;
            AVResult32 length = ffmpeg.avcodec_get_supported_config(null, codec, _AVCodecConfig.AV_CODEC_CONFIG_SAMPLE_FORMAT, 0, (void**)&ptr, null);

            if (ptr == null || length.IsError)
                return [];
            return new ChannelLayoutConstList(ptr, length);
        }
    }


    /// <summary>
    /// Retrieves a list of all available codecs, including both encoders and decoders.
    /// This method iterates through all registered codecs in FFmpeg and returns them as <see cref="Codec"/> instances.
    /// </summary>
    /// <returns>A read-only collection of <see cref="Codec"/> instances representing all registered codecs.</returns>
    public static ReadOnlyCollection<Codec> GetAllCodecs()
    {
        List<Codec> codecs = [];
        void* opaque = null;
        AutoGen._AVCodec* codec;
        while ((codec = ffmpeg.av_codec_iterate(&opaque)) != null)
            codecs.Add(new(codec));
        return new(codecs);
    }

    /// <summary>
    /// Finds a decoder by its codec ID.
    /// This method searches for a decoder codec that matches the provided <paramref name="codecID"/>.
    /// </summary>
    /// <param name="codecID">The codec ID that uniquely identifies the desired decoder.</param>
    /// <returns>An instance of <see cref="Codec"/> representing the decoder if found; otherwise, null.</returns>
    public static Codec? FindDecoder(CodecID codecID)
    {
        AutoGen._AVCodec* codecPtr = ffmpeg.avcodec_find_decoder((_AVCodecID)codecID);
        return codecPtr == null ? null : new Codec(codecPtr);
    }

    /// <summary>
    /// Finds a decoder by its name.
    /// This method searches for a decoder codec by its symbolic name (e.g., "h264" for an H.264 decoder).
    /// </summary>
    /// <param name="name">The symbolic name of the decoder codec to search for.</param>
    /// <returns>An instance of <see cref="Codec"/> representing the decoder if found; otherwise, null.</returns>
    public static Codec? FindDecoder(string name)
    {
        AutoGen._AVCodec* codecPtr = ffmpeg.avcodec_find_decoder_by_name(name);
        return codecPtr == null ? null : new Codec(codecPtr);
    }

    /// <summary>
    /// Finds an encoder by its codec ID.
    /// This method searches for an encoder codec that matches the provided <paramref name="codecID"/>.
    /// </summary>
    /// <param name="codecID">The codec ID that uniquely identifies the desired encoder.</param>
    /// <returns>An instance of <see cref="Codec"/> representing the encoder if found; otherwise, null.</returns>
    public static Codec? FindEncoder(CodecID codecID)
    {
        AutoGen._AVCodec* codecPtr = ffmpeg.avcodec_find_encoder((_AVCodecID)codecID);
        return codecPtr == null ? null : new Codec(codecPtr);
    }

    /// <summary>
    /// Finds an encoder by its name.
    /// This method searches for an encoder codec by its symbolic name (e.g., "aac" for an AAC encoder).
    /// </summary>
    /// <param name="name">The symbolic name of the encoder codec to search for.</param>
    /// <returns>An instance of <see cref="Codec"/> representing the encoder if found; otherwise, null.</returns>
    public static Codec? FindEncoder(string name)
    {
        AutoGen._AVCodec* codecPtr = ffmpeg.avcodec_find_encoder_by_name(name);
        return codecPtr == null ? null : new Codec(codecPtr);
    }

    /// <summary>
    /// Indicates whether the codec is a decoder, meaning it can be used to decode media streams.
    /// </summary>
    public bool IsDecoder => Convert.ToBoolean(ffmpeg.av_codec_is_decoder(codec));

    /// <summary>
    /// Indicates whether the codec is an encoder, meaning it can be used to encode media streams.
    /// </summary>
    public bool IsEncoder => Convert.ToBoolean(ffmpeg.av_codec_is_encoder(codec));

    /// <summary>
    /// Returns a string representation of the current <see cref="Codec"/> instance, 
    /// including the codec's symbolic name, descriptive name, and codec ID.
    /// </summary>
    /// <returns>A string representation of the codec with its name, long name, and codec ID.</returns>
    public override string ToString() => $"Codec: {Name} ({LongName}), ID: {CodecID}";

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Codec"/> instance.
    /// Two <see cref="Codec"/> instances are considered equal if they reference the same underlying FFmpeg codec.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Codec"/>.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Codec codec && Equals(codec);

    /// <summary>
    /// Determines whether the specified <see cref="Codec"/> is equal to the current instance.
    /// Two <see cref="Codec"/> instances are considered equal if they reference the same underlying FFmpeg codec.
    /// </summary>
    /// <param name="other">The <see cref="Codec"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="Codec"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Codec other) => EqualityComparer<nint>.Default.Equals((nint)codec, (nint)other.codec);

    /// <summary>
    /// Returns a hash code for the current <see cref="Codec"/> instance, 
    /// based on the memory pointer of the underlying FFmpeg codec structure.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Codec"/> instance.</returns>
    public override int GetHashCode() => HashCode.Combine((nint)codec);

    /// <summary>
    /// Determines whether two <see cref="Codec"/> instances are equal by comparing their underlying codec structures.
    /// </summary>
    /// <param name="left">The first <see cref="Codec"/> to compare.</param>
    /// <param name="right">The second <see cref="Codec"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="Codec"/> instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Codec left, Codec right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Codec"/> instances are not equal by comparing their underlying codec structures.
    /// </summary>
    /// <param name="left">The first <see cref="Codec"/> to compare.</param>
    /// <param name="right">The second <see cref="Codec"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="Codec"/> instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Codec left, Codec right) => !(left == right);

}
