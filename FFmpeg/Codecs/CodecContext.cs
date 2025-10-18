using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Images;
using FFmpeg.Logging;
using FFmpeg.Utils;
using FFmpeg.Helper;
using System.Runtime.InteropServices;
using AVFrame = FFmpeg.Utils.AVFrame;
using AVPacket = FFmpeg.Utils.AVPacket;

namespace FFmpeg.Codecs;

/// <summary>
/// Represents a codec context used for encoding or decoding video and audio streams using FFmpeg.
/// <para>
/// This class wraps the <see cref="AutoGen._AVCodecContext"/> structure from the FFmpeg library,
/// providing an interface for configuring and interacting with codec parameters for both encoding
/// and decoding operations.
/// </para>
/// </summary>
public sealed unsafe class CodecContext : Options.OptionQueryBase, IDisposable, IAudioDecoder, IAudioEncoder, IVideoDecoder, IVideoEncoder
{
    internal AutoGen._AVCodecContext* context;
    private readonly GCHandle handle;
    private readonly AVFrame hardwareFrame = AVFrame.Allocate()!;
    private HW.CodecHWConfig hardwareConfig = default;
    private bool releaseExtraData = false;

    /// <summary>
    /// Gets the pointer to the underlying <see cref="AutoGen._AVCodecContext"/> structure.
    /// </summary>
    protected override unsafe void* Pointer => context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodecContext"/> class with the specified codec.
    /// </summary>
    /// <param name="codec">The codec to initialize the context with. This should be an instance of <see cref="Codec"/> representing the codec to be used.</param>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if the codec context cannot be allocated due to insufficient memory.
    /// </exception>
    internal CodecContext(Codec codec)
    {
        context = ffmpeg.avcodec_alloc_context3(codec.codec);
        if (context == null)
        {
            throw new OutOfMemoryException("Failed to allocate codec context. Ensure sufficient memory is available.");
        }
        handle = GCHandle.Alloc(this);
        context->opaque = (void*)(nint)handle;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodecContext"/> class with an existing <see cref="AutoGen._AVCodecContext"/> pointer.
    /// </summary>
    /// <param name="context">The pointer to the existing <see cref="AutoGen._AVCodecContext"/> structure.</param>
    internal CodecContext(AutoGen._AVCodecContext* context)
    {
        this.context = context;
        handle = GCHandle.Alloc(this);
        context->opaque = (void*)(nint)handle;
    }

    /// <summary>
    /// Allocates a new <see cref="CodecContext"/> instance for the specified codec.
    /// </summary>
    /// <param name="codec">The codec for which to allocate the context. If <see langword="null"/>, a context is allocated without a specific codec.</param>
    /// <returns>A new <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if the codec context cannot be allocated due to insufficient memory.
    /// </exception>
    public static CodecContext Allocate(Codec? codec)
    {
        _AVCodec* codecPtr = codec != null ? codec.Value.codec : null;
        _AVCodecContext* context = ffmpeg.avcodec_alloc_context3(codecPtr);
        return context == null ? throw new OutOfMemoryException("Failed to allocate codec context.") : new CodecContext(context);
    }

    /// <summary>
    /// Opens a codec and initializes a <see cref="CodecContext"/> for it.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec.
    /// </exception>
    public static CodecContext Open(Codec codec)
    {
        CodecContext context = new(codec);       
        AVResult32 result = ffmpeg.avcodec_open2(context.context, codec.codec, null);
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    /// <summary>
    /// Opens a codec and initializes a <see cref="CodecContext"/> for it with additional options from a dictionary.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="dictionary">A dictionary of additional codec options. Can be <see langword="null"/>.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec.
    /// </exception>
    public static CodecContext Open(Codec codec, Collections.AVDictionary? dictionary)
    {
        if (dictionary == null) return Open(codec);

        CodecContext context = new(codec);
        _AVDictionary* dic = dictionary.dictionary;
        AVResult32 result = ffmpeg.avcodec_open2(context.context, codec.codec, &dic);
        dictionary.dictionary = dic;
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    /// <summary>
    /// Opens a codec and initializes a <see cref="CodecContext"/> for it with additional options from a multi-dictionary.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="dictionary">A multi-dictionary of additional codec options. Can be <see langword="null"/>.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec.
    /// </exception>
    public static CodecContext Open(Codec codec, Collections.AVMultiDictionary? dictionary)
    {
        if (dictionary == null) return Open(codec);

        CodecContext context = new(codec);
        _AVDictionary* dic = dictionary.dictionary;
        AVResult32 result = ffmpeg.avcodec_open2(context.context, codec.codec, &dic);
        dictionary.dictionary = dic;
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    /// <summary>
    /// Opens a codec and initializes a <see cref="CodecContext"/> for it with additional options from a dictionary of string key-value pairs.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="dictionary">A dictionary of additional codec options. Can be <see langword="null"/>.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec.
    /// </exception>
    public static CodecContext Open(Codec codec, IDictionary<string, string>? dictionary)
    {
        if (dictionary == null) return Open(codec);

        if (dictionary is Collections.AVDictionary avDict) return Open(codec, avDict);

        using Collections.AVDictionary dic = new(dictionary);
        CodecContext result = Open(codec, dic);
        dictionary.Clear();
        foreach (KeyValuePair<string, string> kvp in dic)
            dictionary[kvp.Key] = kvp.Value;
        return result;
    }

    /// <summary>
    /// Opens a codec, sets codec parameters, and initializes a <see cref="CodecContext"/> for it.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="codecParams">The codec parameters to set. Can be <see langword="null"/>.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec or setting parameters.
    /// </exception>
    public static CodecContext Open(Codec codec, ICodecParameters? codecParams)
    {
        CodecContext context = Allocate(codec);
        if (codecParams != null)
            context.SetCodecParameters(codecParams);

        AVResult32 result = context.Open(codec);
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    /// <summary>
    /// Opens a codec and initializes a <see cref="CodecContext"/> for it with hardware acceleration support.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="deviceType">The type of hardware device to use for acceleration.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec or setting hardware device type.
    /// </exception>
    public static CodecContext Open(Codec codec, HW.DeviceType deviceType)
    {
        CodecContext context = new(codec);
        _ = context.SetHWDeviceType(deviceType);

        AVResult32 result = ffmpeg.avcodec_open2(context.context, codec.codec, null);
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    /// <summary>
    /// Opens a codec, sets hardware device type, and initializes a <see cref="CodecContext"/> for it with additional options from a dictionary.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="dictionary">A dictionary of additional codec options. Can be <see langword="null"/>.</param>
    /// <param name="deviceType">The type of hardware device to use for acceleration.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec or setting hardware device type.
    /// </exception>
    public static CodecContext Open(Codec codec, Collections.AVDictionary? dictionary, HW.DeviceType deviceType)
    {
        if (dictionary == null) return Open(codec, deviceType);

        CodecContext context = new(codec);
        _ = context.SetHWDeviceType(deviceType);
        _AVDictionary* dic = dictionary.dictionary;
        AVResult32 result = ffmpeg.avcodec_open2(context.context, codec.codec, &dic);
        dictionary.dictionary = dic;
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    /// <summary>
    /// Opens a codec, sets hardware device type, and initializes a <see cref="CodecContext"/> for it with additional options from a multi-dictionary.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="dictionary">A multi-dictionary of additional codec options. Can be <see langword="null"/>.</param>
    /// <param name="deviceType">The type of hardware device to use for acceleration.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec or setting hardware device type.
    /// </exception>
    public static CodecContext Open(Codec codec, Collections.AVMultiDictionary? dictionary, HW.DeviceType deviceType)
    {
        if (dictionary == null) return Open(codec, deviceType);

        CodecContext context = new(codec);
        _ = context.SetHWDeviceType(deviceType);
        _AVDictionary* dic = dictionary.dictionary;
        AVResult32 result = ffmpeg.avcodec_open2(context.context, codec.codec, &dic);
        dictionary.dictionary = dic;
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    /// <summary>
    /// Opens a codec, sets hardware device type, and initializes a <see cref="CodecContext"/> for it with additional options from a dictionary of string key-value pairs.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="dictionary">A dictionary of additional codec options. Can be <see langword="null"/>.</param>
    /// <param name="deviceType">The type of hardware device to use for acceleration.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec or setting hardware device type.
    /// </exception>
    public static CodecContext Open(Codec codec, IDictionary<string, string>? dictionary, HW.DeviceType deviceType)
    {
        if (dictionary == null) return Open(codec, deviceType);

        if (dictionary is Collections.AVDictionary avDict) return Open(codec, avDict, deviceType);

        using Collections.AVDictionary dic = new(dictionary);
        CodecContext result = Open(codec, dic, deviceType);
        dictionary.Clear();
        foreach (KeyValuePair<string, string> kvp in dic)
            dictionary[kvp.Key] = kvp.Value;
        return result;
    }

    /// <summary>
    /// Opens a codec, sets codec parameters, and initializes a <see cref="CodecContext"/> for it with hardware acceleration support.
    /// </summary>
    /// <param name="codec">The codec to open.</param>
    /// <param name="codecParams">The codec parameters to set. Can be <see langword="null"/>.</param>
    /// <param name="deviceType">The type of hardware device to use for acceleration.</param>
    /// <returns>An open <see cref="CodecContext"/> instance.</returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while opening the codec, setting parameters, or configuring hardware acceleration.
    /// </exception>
    public static CodecContext Open(Codec codec, ICodecParameters? codecParams, HW.DeviceType deviceType)
    {
        ExceptionLog.Reset();
        CodecContext context = Allocate(codec);
        if (codecParams != null)
            context.SetCodecParameters(codecParams);
        _ = context.SetHWDeviceType(deviceType);
        AVResult32 result = context.Open(codec);
        if (result.IsError) context.Dispose();
        result.ThrowIfError();
        return context;
    }

    // ToDo:
    public static CodecContext OpenEncoder(Formats.AVStream stream)
    {
        Logger.LogLevel = LogLevel.Debug;
        var ctx = CodecContext.Allocate(Codec.FindEncoder(stream.CodecId));
        ctx.SetCodecParameters(stream.CodecParameters);
        ctx.TimeBase = ctx.PacketTimeBase = stream.TimeBase;
        ctx.FrameRate = stream.RealFrameRate;
        ctx.Open(null).ThrowIfError();
        return ctx;
    }


    /// <summary>
    /// Sets the hardware device type for the codec context.
    /// <para>
    /// This method configures the hardware acceleration device type to be used by the codec context. It must be set
    /// before opening the codec. If the codec context is already open, an exception will be thrown.
    /// </para>
    /// </summary>
    /// <param name="deviceType">The type of hardware device to use for acceleration.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if the codec context is already open when attempting to set the hardware device type.
    /// </exception>
    public AVResult32 SetHWDeviceType(HW.DeviceType deviceType)
    {
        if (IsOpen)
        {
            throw new NotSupportedException("The hardware device type should be set before opening the codec.");
        }

        // Find the hardware configuration that matches the specified device type and supports hardware device context.
        hardwareConfig = HW.CodecHWConfig.CodecHWConfigs(Codec)
            .FirstOrDefault(c => c.DeviceType == deviceType && c.Methods.HasFlag(HW.CodecHWConfigMethod.HwDeviceContext));

        InitHW();
        return hardwareConfig == default ? AVResult32.OptionNotFound : (AVResult32)0;
    }

    /// <summary>
    /// Initializes hardware acceleration for the codec context based on the configured hardware settings.
    /// <para>
    /// This method configures the codec context to use hardware acceleration if the hardware configuration supports it.
    /// </para>
    /// </summary>
    private void InitHW()
    {
        if (!hardwareConfig.Methods.HasFlag(HW.CodecHWConfigMethod.HwDeviceContext))
        {
            return;
        }

        // Initialize hardware decoder with the specified device type.
        AVResult32 res = HW.HWInternalFunctions.HWDecoderInit(this, hardwareConfig.DeviceType);

        if (res.IsError)
        {
            return;
        }

        // Notify the function pointer to handle format selection based on hardware configuration.
        context->get_format = (_AVCodecContext_get_format_func)GetFormatHW;
    }

    /// <summary>
    /// Callback function to get the pixel format supported by the hardware device.
    /// <para>
    /// This function is called by FFmpeg to determine the pixel format that is supported by the hardware device.
    /// </para>
    /// </summary>
    /// <param name="context">A pointer to the <see cref="AutoGen._AVCodecContext"/> structure.</param>
    /// <param name="formats">A pointer to an array of pixel formats.</param>
    /// <returns>The pixel format supported by the hardware device, or <see cref="_AVPixelFormat.AV_PIX_FMT_NONE"/> if none are supported.</returns>
    internal static AutoGen._AVPixelFormat GetFormatHW(AutoGen._AVCodecContext* context, AutoGen._AVPixelFormat* formats)
    {
        // Retrieve the CodecContext from the opaque pointer.
        CodecContext ctx = (CodecContext)GCHandle.FromIntPtr((nint)context->opaque).Target;

        // Check the list of formats and return the one that matches the hardware configuration.
        for (int i = 0; formats[i] != _AVPixelFormat.AV_PIX_FMT_NONE; i++)
        {
            if (formats[i] == (_AVPixelFormat)ctx.hardwareConfig.PixelFormat)
            {
                return formats[i];
            }
        }

        return _AVPixelFormat.AV_PIX_FMT_NONE;
    }


    #region Properties
    /// <summary>
    /// Gets a value indicating whether the codec context is open.
    /// </summary>
    public bool IsOpen => Convert.ToBoolean(ffmpeg.avcodec_is_open(context));

    /// <summary>
    /// Gets or sets the general type of the codec (e.g., video, audio).
    /// This property indicates whether the codec is handling audio, video, or other media types.
    /// </summary>
    public MediaType CodecType => (MediaType)context->codec_type;

    /// <summary>
    /// Gets the codec associated with the context.
    /// Represents the specific codec being used for encoding or decoding.
    /// </summary>
    public Codec Codec => new(context->codec);

    /// <summary>
    /// Gets or sets the codec ID, which represents the specific codec used.
    /// This identifies the codec being used, such as H.264 or AAC.
    /// </summary>
    public CodecID CodecID => (CodecID)context->codec_id;

    /// <summary>
    /// Gets or sets the codec tag, an additional codec identifier used to work around some encoder bugs.
    /// The codec tag is a 32-bit identifier used to map codec information, especially when containers store this data in formats
    /// that exceed 32 bits. It is commonly represented as a FourCC code.
    /// <para>
    /// <b>Encoding:</b> Notify by the user; if not set, a default based on <see cref="CodecID"/> will be used.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by the user; during initialization, libavcodec will convert the value to uppercase.
    /// </para>
    /// </summary>
    public FourCC CodecTag
    {
        get => context->codec_tag;
        set => context->codec_tag = value;
    }

    /// <summary>
    /// Gets or sets the target bit rate in bits per second, which influences the compression level.
    /// The bit rate directly affects the quality and size of the encoded media.
    /// <para>
    /// <b>Encoding:</b> Notify by the user. Not used in constant quantizer encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by the user. May be overwritten by libavcodec if this information is available in the stream.
    /// </para>
    /// </summary>
    public long BitRate
    {
        get => context->bit_rate;
        set => context->bit_rate = value;
    }

    /// <summary>
    /// Gets or sets codec flags that determine various encoding or decoding settings.
    /// The flags are split between general codec flags (<see cref="AV_CODEC_FLAG_*"/>) and extended flags (<see cref="AV_CODEC_FLAG2_*"/>).
    /// <para>
    /// <b>Encoding:</b> Notify by the user.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by the user.
    /// </para>
    /// </summary>
    public CodecFlags Flags
    {
        get => (CodecFlags)context->flags | (CodecFlags)((ulong)context->flags2 << 32);
        set => (context->flags, context->flags2) = ((int)((ulong)value & uint.MaxValue), (int)(((ulong)value >> 32) & uint.MaxValue));
    }

    /// <summary>
    /// Gets or sets the extra data, typically codec-specific initialization data required for decoding or encoding.
    /// Certain codecs use extradata to store auxiliary information, such as Huffman tables for MJPEG, additional flags for RV10,
    /// or global headers for MPEG-4.
    /// <para>
    /// <b>Encoding:</b> The data is set, allocated, and freed by libavcodec.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> The data must be set, allocated, and freed by the user. The allocated memory should be 
    /// AV_INPUT_BUFFER_PADDING_SIZE bytes larger than the actual extradata to prevent buffer overflows when reading the bitstream.
    /// </para>
    /// </summary>
    public ReadOnlySpan<byte> ExtraData
    {
        get => (context->extradata_size > 0 && context->extradata != null) ? new ReadOnlySpan<byte>(context->extradata, context->extradata_size) : [];
        set
        {
            if (releaseExtraData && context->extradata != null)
            {
                ffmpeg.av_free(context->extradata);
            }

            if (value.IsEmpty)
            {
                releaseExtraData = false;
                context->extradata = null;
                context->extradata_size = 0;
            }
            else
            {
                context->extradata = (byte*)ffmpeg.av_malloc((ulong)value.Length);
                if (context->extradata == null)
                {
                    throw new OutOfMemoryException("Failed to allocate memory for extra data.");
                }

                context->extradata_size = value.Length;
                releaseExtraData = true;
                fixed (byte* src = value)
                {
                    Buffer.MemoryCopy(src, context->extradata, value.Length, value.Length);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the time base, representing the unit of time in terms of frame duration.
    /// This is the fundamental unit used to express timestamps in the media stream (e.g., 1/25 for 25 fps).
    /// <para>
    /// <b>Encoding:</b> The time base is unused.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by the user to define the base time unit for timestamps.
    /// </para>
    /// </summary>
    public Rational TimeBase
    {
        get => context->time_base;
        set => context->time_base = value;
    }

    /// <summary>
    /// Gets or sets the packet time base, which is used for expressing packet timestamps (pkt_dts/pts) and AVPacket.dts/pts.
    /// This defines the time base in which the packet timestamps are calculated.
    /// <para>
    /// <b>Encoding:</b> Unused.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by the user.
    /// </para>
    /// </summary>
    public Rational PacketTimeBase
    {
        get => context->pkt_timebase;
        set => context->pkt_timebase = value;
    }

    /// <summary>
    /// Gets or sets the frame rate of the video stream, in frames per second.
    /// <para>
    /// <b>Encoding:</b> Used to signal the constant frame rate (CFR) content to the encoder.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the decoder for codecs that store a frame rate value in the bitstream.
    /// If unknown, this value will be { 0, 1 }.
    /// </para>
    /// </summary>
    public Rational FrameRate
    {
        get => context->framerate;
        set => context->framerate = value;
    }

    /// <summary>
    /// Gets the delay in frames between the input to the encoder and the output of the decoder.
    /// For video, this represents the number of frames delayed between the encoded input and the decoded output.
    /// For audio decoding, this is the number of samples that the decoder needs to output before its output is valid.
    /// <para>
    /// <b>Encoding:</b> Notify by libavcodec.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec.
    /// </para>
    /// </summary>
    public int Delay => context->delay;

    /// <summary>
    /// Gets or sets the width of the video frame in pixels.
    /// This field must be set by the user for encoding and may be set for decoding if known.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the user before opening the decoder if known, e.g., from the container.
    /// The decoder may overwrite this value during parsing.
    /// </para>
    /// </summary>
    public int Width
    {
        get => context->width;
        set => context->width = value;
    }

    /// <summary>
    /// Gets or sets the height of the video frame in pixels.
    /// This field must be set by the user for encoding and may be set for decoding if known.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the user before opening the decoder if known, e.g., from the container.
    /// The decoder may overwrite this value during parsing.
    /// </para>
    /// </summary>
    public int Height
    {
        get => context->height;
        set => context->height = value;
    }

    /// <summary>
    /// Gets or sets the coded width of the video frame, which may be larger than the actual display width.
    /// This value represents the width of the frame in the bitstream, which may differ from the decoded output width due to cropping or other optimizations.
    /// <para>
    /// <b>Encoding:</b> Unused.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the user before opening the decoder if known.
    /// The decoder may overwrite this value during parsing.
    /// </para>
    /// </summary>
    public int CodedWidth
    {
        get => context->coded_width;
        set => context->coded_width = value;
    }

    /// <summary>
    /// Gets or sets the coded height of the video frame, which may be larger than the actual display height.
    /// This value represents the height of the frame in the bitstream, which may differ from the decoded output height due to cropping or other optimizations.
    /// <para>
    /// <b>Encoding:</b> Unused.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the user before opening the decoder if known.
    /// The decoder may overwrite this value during parsing.
    /// </para>
    /// </summary>
    public int CodedHeight
    {
        get => context->coded_height;
        set => context->coded_height = value;
    }

    /// <summary>
    /// Gets or sets the minimum size of the Group of Pictures (GOP).
    /// <para>
    /// <b>Encoding:</b> Notify by the user to specify the minimum number of frames between keyframes (i.e., I-frames).
    /// </para>
    /// <para>
    /// <b>Decoding:</b> This field is unused.
    /// </para>
    /// </summary>
    public int MinGOPSize
    {
        get => context->keyint_min;
        set => context->keyint_min = value;
    }

    /// <summary>
    /// Gets or sets the number of frames in a Group of Pictures (GOP).
    /// <para>
    /// <b>Encoding:</b> Notify by the user to specify the number of frames in each GOP. If set to 0, the encoding will be intra-only (I-frames only).
    /// </para>
    /// <para>
    /// <b>Decoding:</b> This field is unused.
    /// </para>
    /// </summary>
    public int GOPSize
    {
        get => context->gop_size;
        set => context->gop_size = value;
    }



    /// <summary>
    /// Gets the hardware device context pointer, which represents the device context used for hardware acceleration.
    /// <para>
    /// <b>Encoding:</b> Not used.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Access this property to interact with the hardware device context when using hardware acceleration.
    /// </para>
    /// </summary>
    public HW.DeviceContext_ref HardwareDeviceContext => new(&context->hw_device_ctx, false);

    /// <summary>
    /// Gets or sets the sample aspect ratio, which represents the width of a pixel divided by its height.
    /// A sample aspect ratio of 0 means the value is unknown. For some video standards, the numerator and denominator must be relatively prime and less than 256.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to specify the sample aspect ratio for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec to provide the sample aspect ratio for decoding.
    /// </para>
    /// </summary>
    public Rational SampleAspectRatio
    {
        get => context->sample_aspect_ratio;
        set => context->sample_aspect_ratio = value;
    }

    /// <summary>
    /// Gets or sets the pixel format of the video, which defines the format of the pixels (e.g., YUV, RGB).
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to define the pixel format for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the user if known from the headers. However, it can be overridden by the decoder during parsing.
    /// </para>
    /// </summary>
    public PixelFormat PixelFormat
    {
        get => (PixelFormat)context->pix_fmt;
        set => context->pix_fmt = (_AVPixelFormat)value;
    }

    /// <summary>
    /// Gets the software pixel format, which represents the nominal unaccelerated pixel format used during decoding.
    /// <para>
    /// <b>Encoding:</b> Unused.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec before calling the format selection function (get_format).
    /// </para>
    /// </summary>
    public PixelFormat SoftwarePixelFormat => (PixelFormat)context->sw_pix_fmt;

    /// <summary>
    /// Gets or sets the chromaticity coordinates of the source primaries, which define the red, green, and blue components of the video.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to define the chromaticity coordinates for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec during decoding to interpret the color information correctly.
    /// </para>
    /// </summary>
    public ColorPrimaries ColorPrimaries
    {
        get => (ColorPrimaries)context->color_primaries;
        set => context->color_primaries = (_AVColorPrimaries)value;
    }

    /// <summary>
    /// Gets or sets the color transfer characteristic, which is used for gamma correction and defines how colors are transformed.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to specify the color transfer characteristics for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec during decoding to handle color transformation properly.
    /// </para>
    /// </summary>
    public ColorTransferCharacteristic ColorTransferCharacteristic
    {
        get => (ColorTransferCharacteristic)context->color_trc;
        set => context->color_trc = (_AVColorTransferCharacteristic)value;
    }

    /// <summary>
    /// Gets or sets the YUV colorspace type of the video, which defines how the color components are organized.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to specify the YUV colorspace for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec during decoding to interpret the colorspace of the video stream.
    /// </para>
    /// </summary>
    public ColorSpace ColorSpace
    {
        get => (ColorSpace)context->colorspace;
        set => context->colorspace = (_AVColorSpace)value;
    }

    /// <summary>
    /// Gets or sets the color range of the video, which defines the range of values for the color components (MPEG vs JPEG YUV range).
    /// <para>
    /// <b>Encoding:</b> Can be set by the user to override the default output color range. If not specified, libavcodec sets the color range based on the output format.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec, but may be set by the user to propagate the color range to other components reading from the decoder context.
    /// </para>
    /// </summary>
    public ColorRange ColorRange
    {
        get => (ColorRange)context->color_range;
        set => context->color_range = (_AVColorRange)value;
    }

    /// <summary>
    /// Gets or sets the chroma sample location, which defines the position of the chroma samples relative to the luma samples in the video.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to specify the chroma sample location for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec during decoding to correctly align chroma samples.
    /// </para>
    /// </summary>
    public ChromaLocation ChromaSampleLocation
    {
        get => (ChromaLocation)context->chroma_sample_location;
        set => context->chroma_sample_location = (_AVChromaLocation)value;
    }

    /// <summary>
    /// Gets or sets the field order for interlaced video, which defines whether the top or bottom field is displayed first.
    /// <para>
    /// <b>Encoding:</b> Notify by libavcodec during encoding to ensure proper field order in the output stream.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by the user if known, to correctly interpret the field order of the input stream.
    /// </para>
    /// </summary>
    public FieldOrder FieldOrder
    {
        get => (FieldOrder)context->field_order;
        set => context->field_order = (_AVFieldOrder)value;
    }

    /// <summary>
    /// Gets or sets the number of reference frames used for inter-frame compression.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to specify the number of reference frames for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec during decoding to manage reference frame usage.
    /// </para>
    /// </summary>
    public int ReferenceFrames
    {
        get => context->refs;
        set => context->refs = value;
    }

    /// <summary>
    /// Gets or sets the audio sample rate in samples per second (Hz).
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to specify the sample rate of the audio stream being encoded.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the user if known, but could be overridden by libavcodec based on the audio stream format.
    /// </para>
    /// </summary>
    public int SampleRate
    {
        get => context->sample_rate;
        set => context->sample_rate = value;
    }

    /// <summary>
    /// Gets or sets the audio sample format, which defines how audio samples are stored (e.g., signed 16-bit, float).
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to define the sample format for the audio stream being encoded.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec based on the audio stream format during decoding.
    /// </para>
    /// </summary>
    public SampleFormat SampleFormat
    {
        get => (SampleFormat)context->sample_fmt;
        set => context->sample_fmt = (_AVSampleFormat)value;
    }

    /// <summary>
    /// Gets or sets the channel layout for the audio stream, such as stereo or surround sound.
    /// <para>
    /// <b>Encoding:</b> Must be set by the user to specify the desired channel layout for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> May be set by the user if known, but the decoder may override it based on the actual stream data.
    /// </para>
    /// </summary>
    public ChannelLayout_ref ChannelLayout => new(&context->ch_layout, true);

    /// <summary>
    /// Gets or sets the frame size for audio encoding/decoding, which represents the number of samples per channel in each frame.
    /// <para>
    /// <b>Encoding:</b> Notify by libavcodec during encoding to define the number of samples per frame. Each frame submitted must have exactly this number of samples, except the last frame.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Some decoders may set this field to indicate a constant frame size.
    /// </para>
    /// </summary>
    public int FrameSize
    {
        get => context->frame_size;
        set => context->frame_size = value;
    }

    /// <summary>
    /// Gets or sets the audio service type, which indicates the intended usage of the audio stream (e.g., main audio, commentary, or karaoke).
    /// <para>
    /// <b>Encoding:</b> Notify by the user to specify the type of audio service for encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> This field is set by libavcodec during decoding and cannot be set by the user.
    /// </para>
    /// </summary>
    public AudioServiceType AudioServiceType
    {
        get => (AudioServiceType)context->audio_service_type;
        set => context->audio_service_type = (_AVAudioServiceType)value; // Notify only for encoding
    }


    /// <summary>
    /// Gets or sets the codec profile, which defines the level of compression and quality for the codec.
    /// <para>
    /// <b>Encoding:</b> Notify by the user to specify the desired codec profile (e.g., main, high, baseline for H.264).
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by libavcodec during decoding to reflect the profile used in the encoded stream.
    /// </para>
    /// </summary>
    public Profile Profile
    {
        get => new(CodecID, context->profile);
        set => context->profile = value.ProfileID; // Notify only for encoding
    }

    /// <summary>
    /// Gets or sets the tolerance for bit rate deviations during encoding.
    /// <para>
    /// <b>Encoding:</b> Defines how much the bitstream is allowed to deviate from the target bit rate. This applies to both constant bit rate (CBR) and variable bit rate (VBR) encoding.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> This field is not used during decoding.
    /// </para>
    /// </summary>
    public int BitRateTolerance
    {
        get => context->bit_rate_tolerance;
        set => context->bit_rate_tolerance = value; // Notify only for encoding
    }

    /// <summary>
    /// Gets or sets the global quality factor for codecs that do not adjust quality on a per-frame basis.
    /// <para>
    /// <b>Encoding:</b> This value is used to set the global quality for encoding, which is proportional to the MPEG quantization scale (qscale).
    /// </para>
    /// <para>
    /// <b>Decoding:</b> This field is not used during decoding.
    /// </para>
    /// </summary>
    public int GlobalQuality
    {
        get => context->global_quality;
        set => context->global_quality = value; // Notify only for encoding
    }

    /// <summary>
    /// Gets or sets the compression level for encoding, affecting the tradeoff between speed and quality.
    /// <para>
    /// <b>Encoding:</b> Notify by the user to control the level of compression. Higher values may result in slower encoding but potentially better quality.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> This field is not used during decoding.
    /// </para>
    /// </summary>
    public int CompressionLevel
    {
        get => context->compression_level;
        set => context->compression_level = value; // Notify only for encoding
    }

    /// <summary>
    /// Gets or sets the number of threads used for encoding or decoding operations.
    /// <para>
    /// <b>Encoding:</b> Notify by the user to determine the number of threads for parallel encoding tasks.
    /// </para>
    /// <para>
    /// <b>Decoding:</b> Notify by the user to determine the number of threads for parallel decoding tasks.
    /// </para>
    /// </summary>
    public int ThreadCount
    {
        get => context->thread_count;
        set => context->thread_count = value; // Notify for both encoding and decoding
    }

    /// <summary>
    /// Gets the pass1 encoding statistics output buffer as a read-only span of bytes.
    /// <para>
    /// This buffer contains encoding statistics collected during the first pass of encoding.
    /// </para>
    /// <para>
    /// This property is set by libavcodec and cannot be modified by the user.
    /// </para>
    /// </summary>
    public ReadOnlySpan<byte> Pass1StatsOutput => context->stats_out != null
            ? new ReadOnlySpan<byte>(context->stats_out, Helper.StringHelper.StrLen(context->stats_out))
            : [];

    /// <summary>
    /// Gets or sets the pass2 encoding statistics input buffer as a read-only span of bytes.
    /// <para>
    /// This buffer should contain the concatenated statistics from the pass1 output, used for the second pass of encoding.
    /// </para>
    /// <para>
    /// This property can be set by the user for pass2 encoding. If the buffer is being replaced, the old buffer is freed.
    /// </para>
    /// </summary>
    public ReadOnlySpan<byte> Pass2StatsInput
    {
        get => context->stats_in != null
            ? new ReadOnlySpan<byte>(context->stats_in, Helper.StringHelper.StrLen(context->stats_in) + 1)
            : [];
        set
        {
            if (context->stats_in != null)
            {
                ffmpeg.av_freep(&context->stats_in);
            }
            if (value.Length > 0)
            {
                context->stats_in = (byte*)ffmpeg.av_malloc((ulong)(value.Length + 1)); // Ensure null termination
                if (context->stats_in == null)
                {
                    throw new OutOfMemoryException();
                }
                fixed (byte* src = value)
                {
                    Buffer.MemoryCopy(src, context->stats_in, value.Length, value.Length);
                }
                context->stats_in[value.Length] = 0; // Null-terminate the buffer
            }
            else
            {
                context->stats_in = null;
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves the codec parameters from the current codec context.
    /// </summary>
    /// <returns>
    /// An <see cref="ICodecParameters"/> object containing the codec parameters retrieved from the context.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if there is insufficient memory to allocate the <see cref="ICodecParameters"/> object.
    /// </exception>
    /// <exception cref="Exceptions.FFmpegException">
    /// Thrown if the FFmpeg function to retrieve parameters returns an error.
    /// </exception>
    public CodecParameters GetCodecParameters()
    {
        CodecParameters parameters = new(); // Throws OutOfMemoryException if insufficient memory
        AVResult32 result = ffmpeg.avcodec_parameters_from_context(parameters.codecParameters, context);
        result.ThrowIfError();
        return parameters;
    }

    /// <summary>
    /// Sets the codec parameters for the current codec context using the specified parameters.
    /// </summary>
    /// <param name="parameters">
    /// An <see cref="ICodecParameters"/> object containing the codec parameters to be set in the context.
    /// </param>
    /// <exception cref="Exceptions.FFmpegException">
    /// Thrown if the FFmpeg function to set parameters returns an error.
    /// </exception>
    public void SetCodecParameters(ICodecParameters parameters)
    {
        AVResult32 result = ffmpeg.avcodec_parameters_to_context(context, parameters.Parameters);
        result.ThrowIfError();
    }

    /// <summary>
    /// Copies codec parameters from the current codec context to another <see cref="CodecContext"/>.
    /// </summary>
    /// <param name="dst">
    /// The destination <see cref="CodecContext"/> to which the codec parameters will be copied.
    /// </param>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if there is insufficient memory to create the temporary <see cref="ICodecParameters"/> object.
    /// </exception>
    /// <exception cref="Exceptions.FFmpegException">
    /// Thrown if the FFmpeg function to copy parameters returns an error.
    /// </exception>
    public void CopyParameters(CodecContext dst)
    {
        using CodecParameters parameters = GetCodecParameters();
        dst.SetCodecParameters(parameters);
    }

    /// <summary>
    /// Opens the codec context with the specified codec.
    /// </summary>
    /// <param name="codec">
    /// The codec to be used to open the codec context. If <see langword="null"/>, the context is opened with the currently set codec.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 Open(Codec? codec)
    {
        _AVCodec* c = codec != null ? codec.Value.codec : null;
        return ffmpeg.avcodec_open2(context, c, null);
    }

    /// <summary>
    /// Opens the codec context with the specified codec and optional dictionary of codec options.
    /// </summary>
    /// <param name="codec">
    /// The codec to be used to open the codec context. If <see langword="null"/>, the context is opened with the currently set codec.
    /// </param>
    /// <param name="dictionary">
    /// An optional <see cref="Collections.AVDictionary"/> containing codec-specific options. If <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 Open(Codec? codec, Collections.AVDictionary? dictionary)
    {
        if (dictionary == null) return Open(codec);
        _AVCodec* c = codec != null ? codec.Value.codec : null;
        _AVDictionary* dic = dictionary.dictionary;
        AVResult32 result = ffmpeg.avcodec_open2(context, c, &dic);
        dictionary.dictionary = dic;
        return result;
    }

    /// <summary>
    /// Opens the codec context with the specified codec and optional multi-dictionary of codec options.
    /// </summary>
    /// <param name="codec">
    /// The codec to be used to open the codec context. If <see langword="null"/>, the context is opened with the currently set codec.
    /// </param>
    /// <param name="dictionary">
    /// An optional <see cref="Collections.AVMultiDictionary"/> containing codec-specific options. If <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 Open(Codec? codec, Collections.AVMultiDictionary? dictionary)
    {
        if (dictionary == null) return Open(codec);
        _AVCodec* c = codec != null ? codec.Value.codec : null;
        _AVDictionary* dic = dictionary.dictionary;
        AVResult32 result = ffmpeg.avcodec_open2(context, c, &dic);
        dictionary.dictionary = dic;
        return result;
    }

    /// <summary>
    /// Opens the codec context with the specified codec and an <see cref="IDictionary{TKey, TValue}"/> of codec options.
    /// </summary>
    /// <param name="codec">
    /// The codec to be used to open the codec context. If <see langword="null"/>, the context is opened with the currently set codec.
    /// </param>
    /// <param name="dictionary">
    /// An optional <see cref="IDictionary{string, string}"/> containing codec-specific options. If <see langword="null"/>, defaults are used.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 Open(Codec? codec, IDictionary<string, string>? dictionary)
    {
        if (dictionary == null) return Open(codec);
        if (dictionary is Collections.AVDictionary avDict) return Open(codec, avDict);
        using Collections.AVDictionary dic = new(dictionary);
        AVResult32 result = Open(codec, dic);
        dictionary.Clear();
        foreach (KeyValuePair<string, string> kvp in dic)
            dictionary[kvp.Key] = kvp.Value;
        return result;
    }

    /// <summary>
    /// Opens the codec context with the specified codec and codec parameters.
    /// </summary>
    /// <param name="codec">
    /// The codec to be used to open the codec context. If <see langword="null"/>, the context is opened with the currently set codec.
    /// </param>
    /// <param name="ICodecParameters">
    /// Optional <see cref="ICodecParameters"/> to set before opening the codec. If <see langword="null"/>, only the codec is used.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 Open(Codec? codec, ICodecParameters? codecParameters)
    {
        if (codecParameters == null) return Open(codec);
        SetCodecParameters(codecParameters);
        return Open(codec);
    }

    /// <summary>
    /// Decodes a subtitle from the given <see cref="AVPacket"/> and outputs the result as a <see cref="Subtitles.Subtitle"/>.
    /// </summary>
    /// <param name="packet">The <see cref="AVPacket"/> containing the encoded subtitle data.</param>
    /// <param name="subtitle">The decoded subtitle output as a <see cref="Subtitles.Subtitle"/>.</param>
    /// <returns>
    /// The result of the decoding operation as <see cref="AVResult32"/>. This result indicates whether the operation was
    /// successful or if an error occurred.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the codec is not of type subtitle.</exception>
    /// <remarks>
    /// This method is specific to subtitle codecs. If the codec type is not set to <see cref="MediaType.Subtitle"/>, 
    /// the operation will throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    public AVResult32 DecodeSubtitle(AVPacket packet, Subtitles.Subtitle subtitle)
    {
        subtitle?.Free();
        if (!IsOpen) Open(null).ThrowIfError();
        if (CodecType != MediaType.Subtitle)
            throw new NotSupportedException($"{nameof(DecodeSubtitle)} can only be used with subtitle codecs.");

        _AVSubtitle sub;
        int b;

        // Decoding the subtitle from the given packet
        var res = ffmpeg.avcodec_decode_subtitle2(context, &sub, &b, packet.packet);

        // Creating the subtitle object from the decoded AVSubtitle
        subtitle ??= new();
        subtitle.subtitle = sub;

        return res;
    }

    /// <summary>
    /// Encodes a subtitle into the provided <see cref="AVPacket"/> and returns the result.
    /// </summary>
    /// <param name="packet">
    /// The <see cref="AVPacket"/> that will contain the encoded subtitle data. This method will
    /// modify and return a fresh packet with the encoded data.
    /// </param>
    /// <param name="subtitle">The <see cref="Subtitles.Subtitle"/> to encode into the packet.</param>
    /// <returns>
    /// The result of the encoding operation as <see cref="AVResult32"/>. A successful result indicates
    /// that the subtitle was encoded into the packet successfully.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the codec is not of type subtitle.</exception>
    /// <remarks>
    /// After encoding the subtitle, the returned <see cref="AVPacket"/> will be freshly created. 
    /// Important settings, such as <see cref="AVPacket.StreamIndex"/>, must be set accordingly after the encoding process completes.
    /// The <see cref="AVPacket"/> will have a maximum size of 1MB (1024 * 1024 bytes).
    /// </remarks>
    public AVResult32 EncodeSubtitle(AVPacket packet, Subtitles.Subtitle subtitle)
    {
        if (!IsOpen) Open(null).ThrowIfError();
        if (CodecType != MediaType.Subtitle)
            throw new NotSupportedException($"{nameof(EncodeSubtitle)} can only be used with subtitle codecs.");

        // Getting the underlying AVSubtitle structure from the Subtitle object
        _AVSubtitle sub = subtitle.subtitle;

        // Prepare the packet for encoding
        packet.Unreference();  // Ensure the packet is unreferenced before use
        packet.TimeBase = ffmpeg.AV_TIME_BASE;
        packet.DecompressionTimestamp = packet.PresentationTimestamp = subtitle.PresentationTime;

        // Allocate space for the encoded subtitle (maximum 1MB)
        packet.Resize(1024 * 1024);

        // Encoding the subtitle
        var res = ffmpeg.avcodec_encode_subtitle(context, packet.packet->data, packet.Size, &sub);

        // Notify the size of the packet to the result if encoding was successful
        if (res >= 0)
            packet.Size = res;

        return res;
    }



    /// <summary>
    /// Sends a packet to the codec for encoding or decoding.
    /// </summary>
    /// <param name="packet">
    /// The <see cref="AVPacket"/> to send to the codec.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 SendPacket(AVPacket? packet)
    {
        if (!IsOpen) Open(null).ThrowIfError();
        if (CodecType is MediaType.Audio or MediaType.Video)
            return ffmpeg.avcodec_send_packet(context,packet!= null? packet.packet : null);
        throw new NotSupportedException($"{nameof(SendPacket)} can only be used with audio or video codecs.");
    }

    /// <summary>
    /// Receives a packet from the codec after encoding or decoding.
    /// </summary>
    /// <param name="packet">
    /// The <see cref="AVPacket"/> to receive from the codec.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 ReceivePacket(AVPacket packet)
    {
        if (!IsOpen) Open(null).ThrowIfError();
        
        var result = ffmpeg.avcodec_receive_packet(context, packet.packet);
        if(TimeBase.IsValidTimeBase)
            packet.TimeBase = TimeBase;
        if (PacketTimeBase.IsValidTimeBase)
            packet.RescaleTS(PacketTimeBase);
        return result;
    }

    /// <summary>
    /// Sends a frame to the codec for encoding or decoding.
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to send to the codec.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 SendFrame(AVFrame? frame)
    {
        if (!IsOpen) this.Open(null).ThrowIfError();
        if(frame?.TimeBase.IsValidTimeBase == true)
            TimeBase = frame.TimeBase; // set this to frames time base, so receive package uses the right timebase.
        return ffmpeg.avcodec_send_frame(context, frame != null ? frame.Frame : null);
    }

    /// <summary>
    /// Receives a hardware-accelerated frame from the codec.
    /// <para>
    /// If the frame is in RAM, this method behaves the same as <see cref="ReceiveFrame"/>. 
    /// If the frame is in GPU memory, this method will return the frame without transferring it to RAM first.
    /// </para>
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to receive from the codec.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 ReceiveHWFrame(AVFrame frame)
    {
        if(!IsOpen) Open(null).ThrowIfError();
        int res = ffmpeg.avcodec_receive_frame(context, frame.Frame);
        if (frame.TimeBase.Numerator == 0)
            frame.TimeBase = context->pkt_timebase;
        return res;
    }

    /// <summary>
    /// Receives a frame from the codec and transfers it to the provided <see cref="AVFrame"/>.
    /// <para>
    /// If the codec context uses hardware acceleration, the frame will be transferred from hardware memory (GPU) to system memory (RAM). 
    /// </para>
    /// </summary>
    /// <param name="frame">
    /// The <see cref="AVFrame"/> to receive from the codec. This frame will be populated with the decoded data.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. Check for errors using <c>ThrowIfError()</c>.
    /// </returns>
    public AVResult32 ReceiveFrame(AVFrame frame)
    {
        if(!IsOpen) Open(null).ThrowIfError();
        if (context->hw_device_ctx == null)
        {
            int res = ffmpeg.avcodec_receive_frame(context, frame.Frame);
            if (frame.TimeBase.Numerator == 0)
                frame.TimeBase = context->pkt_timebase;
            return res;
        }
        else
        {
            AVResult32 res = ffmpeg.avcodec_receive_frame(context, hardwareFrame.Frame);
            if (res.IsError || res == AVResult32.TryAgain) return res;
            res = ffmpeg.av_hwframe_transfer_data(frame.Frame, hardwareFrame.Frame, 0);
            if (res.IsError) return res;
            _ = ffmpeg.av_frame_copy_props(frame.Frame, hardwareFrame.Frame);
            hardwareFrame.Unreference();
            if (frame.TimeBase.Numerator == 0)
                frame.TimeBase = context->pkt_timebase;
            return res;
        }
    }

    // ToDo: Docu, include info about TryAgain
    public AVResult32 Decode(AVPacket? packet, AVFrame frame)
    {        
        var res = SendPacket(packet);
        if(res == AVResult32.TryAgain) res.ThrowIfError(); //  should never happen, since ReceiveFrame is always called afterwards, but if it does happen, you need to call ReceiveFrame until TryAgain 
        // in draining mode SendPacket might return EOF
        if(res.IsError &&  res != AVResult32.EndOfFile) return res;       
        return ReceiveFrame(frame);
    }


    public AVResult32 Encode(AVFrame frame,AVPacket packet)
    {
        var res = SendFrame(frame);
        if (res == AVResult32.TryAgain)
            res.ThrowIfError(); //  should never happen, since ReceivePacket is always called afterwards, but if it does, you need to call ReveicePacket until TryAgain
        // in draining mode SendPacket might return EOF
        if (res.IsError && res != AVResult32.EndOfFile)
            return res;
        return ReceivePacket(packet);
    }

    /// <summary>
    /// Resets the internal codec state and flushes internal buffers.
    /// <para>
    /// This method should be called, for example, when seeking or when switching to a different stream.
    /// It clears any cached data and prepares the codec for new data.
    /// </para>
    /// </summary>
    public void FlushBuffers()
    {
        if (!IsOpen) return;
        ffmpeg.avcodec_flush_buffers(context);
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Finalizer for <see cref="CodecContext"/> to ensure resources are released.
    /// <para>
    /// This finalizer calls the <see cref="Dispose(bool)"/> method with <c>disposing</c> set to <c>false</c>.
    /// It is called by the garbage collector if the object is not explicitly disposed.
    /// </para>
    /// </summary>
    ~CodecContext()
    {
        // Do not change this code. Put cleanup code in the Dispose(bool disposing) method.
        Dispose(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by the <see cref="CodecContext"/> class.
    /// <para>
    /// This method is called by both the public <see cref="Dispose"/> method and the finalizer. 
    /// If <c>disposing</c> is <c>true</c>, it indicates that the method was called directly or indirectly by a user's code.
    /// If <c>disposing</c> is <c>false</c>, it indicates that the method was called by the runtime from inside the finalizer.
    /// </para>
    /// <para>
    /// In the <c>disposing</c> parameter being <c>true</c>, it releases managed resources. 
    /// In both cases, it releases unmanaged resources.
    /// </para>
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called from the <see cref="Dispose"/> method (<c>true</c>) 
    /// or from the finalizer (<c>false</c>).
    /// </param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            hardwareFrame.Dispose();
        }

        if (handle.IsAllocated)
        {
            handle.Free();
        }

        // Release extra data if needed
        if (releaseExtraData)
        {
            ffmpeg.av_freep(&context->extradata);
            context->extradata_size = 0;
        }

        // Free the codec context
        _AVCodecContext* ctx = context;
        ffmpeg.av_freep(&context->stats_in);
        ffmpeg.avcodec_free_context(&ctx);
        context = ctx;
    }

    #endregion

    /// <inheritdoc />
    public override string ToString() => $"{Codec.MediaType}/{CodecID}";

    /// <summary>
    /// Enters draining mode for the encoder at the end of a stream, ensuring that any internally buffered packets are processed.
    /// This is required when no more input frames are available for encoding, but the codec may still have frames to output.
    /// </summary>
    /// <remarks>
    /// The draining process for encoding works as follows:
    /// <list type="bullet">
    /// <item>Instead of passing a valid frame, call <see cref="SendFrame"/> with a <see langword="null"/> frame, which enters draining mode.</item>
    /// <item>Then, call <see cref="ReceivePacket"/> repeatedly to retrieve any remaining encoded packets until <c>AVERROR_EOF</c> is returned, 
    /// indicating that all buffered packets have been processed.</item>
    /// <item>Before encoding can be resumed, call <see cref="FlushBuffers"/> to clear the codec's internal buffers and reset its state.</item>
    /// </list>
    /// <para>During draining mode, <see cref="ReceivePacket"/> will not return <c>AVERROR(EAGAIN)</c> unless the draining mode was not properly initiated.</para>
    /// </remarks>
    public AVResult32 DrainEncoder() => SendFrame(null);  // Equivalent to sending a null frame for draining

    /// <summary>
    /// Enters draining mode for the decoder at the end of a stream, ensuring that any internally buffered frames are processed.
    /// This is required when no more input packets are available for decoding, but the codec may still have frames to output.
    /// </summary>
    /// <remarks>
    /// The draining process for decoding works as follows:
    /// <list type="bullet">
    /// <item>Instead of passing a valid packet, call <see cref="SendPacket"/> with a <see langword="null"/> packet, which enters draining mode.</item>
    /// <item>Then, call <see cref="ReceiveFrame"/> repeatedly to retrieve any remaining decoded frames until <c>AVERROR_EOF</c> is returned, 
    /// indicating that all buffered frames have been processed.</item>
    /// <item>Before decoding can be resumed, call <see cref="FlushBuffers"/> to clear the codec's internal buffers and reset its state.</item>
    /// </list>
    /// <para>During draining mode, <see cref="ReceiveFrame"/> will not return <c>AVERROR(EAGAIN)</c> unless the draining mode was not properly initiated.</para>
    /// </remarks>
    public AVResult32 DrainDecoder() => SendPacket(null);  // Equivalent to sending a null packet for draining

}
