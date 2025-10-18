using FFmpeg.Collections;
using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpeg.Formats;

/// <summary>
/// Provides a managed wrapper around the native <see cref="AutoGen._AVFormatContext"/> structure from the FFmpeg library.
/// </summary>
/// <remarks>
/// The <see cref="FormatContext"/> class is a critical component in handling media streams with FFmpeg. It encapsulates the context used for handling input and output formats, including parsing and managing media files or streams.
///
/// This class provides a managed interface to the native <see cref="AutoGen._AVFormatContext"/> pointer, offering functionalities for opening media streams, querying and modifying format options, and interacting with other FFmpeg components.
///
/// <para>
/// As a wrapper around a native pointer, this class implements <see cref="IDisposable"/> to ensure proper cleanup of unmanaged resources. You should always dispose of instances of this class when they are no longer needed to avoid memory leaks and potential crashes.
/// </para>
///
/// <para>
/// The class also inherits from <see cref="Options.OptionQueryBase"/>, enabling access to FFmpeg's options querying and setting capabilities. This allows for configuring various aspects of media handling by setting or querying options on the format context.
/// </para>
/// </remarks>
public sealed unsafe class FormatContext : Options.OptionQueryBase, IDisposable
{
    private readonly object _lock = new();
    private readonly bool freeOnDispose = true;
    private bool isInput = false;
    internal AutoGen._AVFormatContext* Context { get; private set; }

    internal AVIOContext? ioContext;

    /// <summary>
    /// Gets the input format associated with this context, if available.
    /// </summary>
    public InputFormat? InputFormat => field ??= Context->iformat != null ? new(Context->iformat) : null;

    /// <summary>
    /// Gets the output format associated with this context, if available.
    /// </summary>
    public OutputFormat? OutputFormat => field ??= Context->oformat != null ? new(Context->oformat) : null;

    /// <summary>
    /// Gets the flags associated with this format context.
    /// </summary>
    public FormatContextFlags Flags => (FormatContextFlags)Context->flags;

    /// <summary>
    /// Gets the number of streams in this format context.
    /// </summary>
    public int StreamCount => (int)Context->nb_streams;

    /// <inheritdoc/>
    protected override unsafe void* Pointer => Context;

    #region Constructions

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatContext"/> class with an existing <see cref="AutoGen._AVFormatContext"/>*.
    /// </summary>
    /// <param name="context">The already allocated context.</param>
    /// <param name="freeOnDispose">Indicates whether the underlying <see cref="AutoGen._AVFormatContext"/>* should be freed when this object is disposed.</param>
    private FormatContext(AutoGen._AVFormatContext* context, bool freeOnDispose)
    {
        Context = context;
        this.freeOnDispose = freeOnDispose;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatContext"/> class with an existing <see cref="AutoGen._AVFormatContext"/>*, specifying whether it is an input context.
    /// </summary>
    /// <param name="context">The allocated context.</param>
    /// <param name="freeOnDispose">Indicates whether the context should be freed when disposed.</param>
    /// <param name="isInput">Indicates whether the context is for input.</param>
    private FormatContext(AutoGen._AVFormatContext* context, bool freeOnDispose, bool isInput)
    {
        Context = context;
        this.freeOnDispose = freeOnDispose;
        this.isInput = isInput;
    }

    #endregion

    #region Streams

    /// <summary>
    /// The array of streams associated with this format context.
    /// </summary>
    private AVStream[] streams = [];

    /// <summary>
    /// Gets the collection of streams in this format context.
    /// </summary>
    /// <remarks>
    /// This property returns a read-only list of streams. The list is updated if the underlying stream array does not match the current number of streams in the context.
    /// </remarks>
    public IReadOnlyList<AVStream> Streams
    {
        get
        {
            if (!CompareStreams())
                UpdateStreamArray();
            return streams;
        }
    }

    public AVDictionary_ref Metadata => new(&Context->metadata, true, false);

    /// <summary>
    /// Updates the internal stream array to reflect the current streams in the context.
    /// </summary>
    /// <remarks>
    /// This method initializes or refreshes the array of streams to match the number of streams in the format context. It should be called when the stream count has changed.
    /// </remarks>
    private void UpdateStreamArray()
    {
        lock (_lock)
        {
            AVStream[] streams = this.streams;
            if (streams.Length != StreamCount)
                streams = new AVStream[StreamCount];
            for (int i = 0; i < streams.Length; i++)
                streams[i] = new AVStream(Context->streams[i]);
            this.streams = streams;
        }
    }

    /// <summary>
    /// Compares the internal stream array with the current streams in the format context.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the internal array matches the current streams, otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks if the internal stream array is in sync with the actual streams in the format context by comparing the pointers.
    /// </remarks>
    private bool CompareStreams()
    {
        if (streams.Length != StreamCount)
            return false;
        for (int i = 0; i < streams.Length; i++)
        {
            if (streams[i].stream != Context->streams[i])
                return false;
        }

        return true;
    }

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
        res->codecpar->codec_id = (AutoGen._AVCodecID)codec.CodecID;
        res->codecpar->codec_type = (AutoGen._AVMediaType)codec.MediaType;
        return res != null ? new AVStream(res) : throw new OutOfMemoryException();
    }

    #endregion

    #region Allocate
    /// <summary>
    /// Allocates a new <see cref="FormatContext"/> for input or output operations. 
    /// This function wraps the <see cref="ffmpeg.avformat_alloc_context"/> function.
    /// </summary>
    /// <param name="freeOnDispose">
    /// Specifies whether the allocated context should be automatically freed when the <see cref="FormatContext"/> object is disposed.
    /// </param>
    /// <returns>
    /// A new <see cref="FormatContext"/> instance, or throws an <see cref="OutOfMemoryException"/> if allocation fails.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if the allocation fails and the context could not be created.
    /// </exception>
    /// <remarks>
    /// This function corresponds to the FFmpeg sequence: <c>avformat_alloc_context</c>.
    /// </remarks>
    public static FormatContext Allocate(bool freeOnDispose = true)
    {
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        return context == null ? throw new OutOfMemoryException() : new(context, freeOnDispose);
    }

    /// <summary>
    /// Allocates a new <see cref="FormatContext"/> for output operations using the specified filename and output format.
    /// This function wraps the <see cref="ffmpeg.avformat_alloc_output_context2"/> function.
    /// </summary>
    /// <param name="filename">
    /// The name of the file to use for output. Can be <see langword="null"/> if the output format does not require a filename.
    /// </param>
    /// <param name="format">
    /// The output format for the context, or <see langword="null"/> to determine the format from the filename.
    /// </param>
    /// <param name="freeOnDispose">
    /// Specifies whether the allocated context should be automatically freed when the <see cref="FormatContext"/> object is disposed.
    /// </param>
    /// <returns>
    /// A new <see cref="FormatContext"/> instance for output, or <see langword="null"/> if allocation fails.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if the allocation fails and the context could not be created.
    /// </exception>
    /// <remarks>
    /// This function corresponds to the FFmpeg sequence: <c>avformat_alloc_output_context2</c>.
    /// </remarks>
    public static FormatContext? AllocateOutput(string? filename, OutputFormat? format, bool freeOnDispose = true)
    {
        AutoGen._AVFormatContext* context;
        AutoGen._AVOutputFormat* oFormat = format != null ? format.Value.Format : null;
        AVResult32 res = ffmpeg.avformat_alloc_output_context2(&context, oFormat, null, filename);
        return res == AVResult32.OutOfMemory ? throw new OutOfMemoryException() : context == null ? null : new(context, freeOnDispose);
    }

    #endregion

    #region SetIOContext
    /// <summary>
    /// Initializes the current <see cref="FormatContext"/> with the provided <see cref="IOContext"/>.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IOContext"/> to associate with the current format context.
    /// </param>
    /// <param name="options">
    /// The <see cref="IOOptions"/> specifying the operations (e.g., read or write) to be used with the context.
    /// </param>
    /// <param name="buffer_size">
    /// The size of the buffer to use for the I/O operations. Default is 32 KB.
    /// </param>
    /// <remarks>
    /// This method is typically used to set up a custom I/O context for input or output operations in FFmpeg.
    /// It wraps the initialization of the I/O context by calling <see cref="IOContext.InitContext"/> internally.
    /// </remarks>
    public void SetContext(IOContext context, IOOptions options, int buffer_size = 32768) => context.InitContext(this, options, buffer_size);
    #endregion


    #region OpenInput

    /// <summary>
    /// Opens an input media file and initializes the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. If <see langword="null"/>, the format will be detected automatically.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. If the result is successful, the <see cref="FormatContext"/> is initialized with the input media.
    /// </returns>
    /// <remarks>
    /// This method configures the <see cref="FormatContext"/> for reading from the specified input URL or file. The <see cref="Formats.InputFormat"/> can be specified if known; otherwise, it will be auto-detected. The <see cref="Collections.AVDictionary"/> allows setting additional options for the input.
    /// </remarks>
    public AVResult32 OpenInput(string? url, InputFormat? input, Collections.AVDictionary? dictionary)
    {
        isInput = true;
        AutoGen._AVFormatContext* context = Context;
        AutoGen._AVInputFormat* format = input == null ? null : input.Value.Format;
        AutoGen._AVDictionary* dic = dictionary == null ? null : dictionary.dictionary;
        int res = ffmpeg.avformat_open_input(&context, url, format, &dic);
        Context = context;
        return res;
    }

    /// <summary>
    /// Opens an input media file and initializes the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. If <see langword="null"/>, the format will be detected automatically.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. If the result is successful, the <see cref="FormatContext"/> is initialized with the input media.
    /// </returns>
    /// <remarks>
    /// This method configures the <see cref="FormatContext"/> for reading from the specified input URL or file. The <see cref="Formats.InputFormat"/> can be specified if known; otherwise, it will be auto-detected. The <see cref="Collections.AVMultiDictionary"/> allows setting additional options for the input.
    /// </remarks>
    public AVResult32 OpenInput(string? url, InputFormat? input, Collections.AVMultiDictionary? dictionary)
    {
        isInput = true;
        AutoGen._AVFormatContext* context = Context;
        AutoGen._AVInputFormat* format = input == null ? null : input.Value.Format;
        AutoGen._AVDictionary* dic = dictionary == null ? null : dictionary.dictionary;
        int res = ffmpeg.avformat_open_input(&context, url, format, &dic);
        Context = context;
        return res;
    }

    /// <summary>
    /// Opens an input media file and initializes the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. If <see langword="null"/>, the format will be detected automatically.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>.
    /// </param>
    /// <returns>
    /// An <see cref="AVResult32"/> indicating the result of the operation. If the result is successful, the <see cref="FormatContext"/> is initialized with the input media.
    /// </returns>
    /// <remarks>
    /// This method configures the <see cref="FormatContext"/> for reading from the specified input URL or file. The <see cref="Formats.InputFormat"/> can be specified if known; otherwise, it will be auto-detected. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public AVResult32 OpenInput(string? url, InputFormat? input, IDictionary<string, string>? dic)
    {
        if (dic is Collections.AVDictionary dictionary)
        {
            return OpenInput(url, input, dictionary);
        }
        else if (dic == null)
        {
            return OpenInput(url, input, null as Collections.AVDictionary);
        }
        else
        {
            using Collections.AVDictionary dic2 = new(dic);
            AVResult32 result = OpenInput(url, input, dic2);
            dic.Clear();
            foreach (KeyValuePair<string, string> kvp in dic2)
                dic[kvp.Key] = kvp.Value;
            return result;
        }
    }

    #region OpenInput with string

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. If <see langword="null"/>, the format will be detected automatically.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input specified by the <paramref name="url"/> or device, using the provided <paramref name="input"/> format if specified. If <paramref name="url"/> is <see langword="null"/> and the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag, it is treated as a special device or input format that does not require a file path.
    /// </remarks>
    public static FormatContext OpenInput(string? url, InputFormat? input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context;
        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, url, iFormat, &dic);

        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            ffmpeg.avformat_close_input(&context);
            result.ThrowIfError();
        }
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return new FormatContext(context, true)
        {
            isInput = true,
        };
    }

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. If <see langword="null"/>, the format will be detected automatically.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input specified by the <paramref name="url"/> or device, using the provided <paramref name="input"/> format if specified. If <paramref name="url"/> is <see langword="null"/> and the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag, it is treated as a special device or input format that does not require a file path.
    /// </remarks>
    public static FormatContext OpenInput(string? url, InputFormat? input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context;
        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, url, iFormat, &dic);

        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            ffmpeg.avformat_close_input(&context);
            result.ThrowIfError();
        }
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return new FormatContext(context, true)
        {
            isInput = true,
        };
    }

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. If <see langword="null"/>, the format will be detected automatically.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input specified by the <paramref name="url"/> or device, using the provided <paramref name="input"/> format if specified. If <paramref name="url"/> is <see langword="null"/> and the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag, it is treated as a special device or input format that does not require a file path. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public static FormatContext OpenInput(string? url, InputFormat? input, IDictionary<string, string>? dic, bool findStreamInfo = false)
    {
        if (dic is Collections.AVDictionary dictionary)
        {
            return OpenInput(url, input, dictionary);
        }
        else if (dic == null)
        {
            return OpenInput(url, input, null as Collections.AVDictionary, findStreamInfo);
        }
        else
        {
            using Collections.AVDictionary dic2 = new(dic);
            FormatContext? result = OpenInput(url, input, dic2);
            dic.Clear();
            foreach (KeyValuePair<string, string> kvp in dic2)
                dic[kvp.Key] = kvp.Value;
            return result;
        }
    }

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using only the URL.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the input format has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="OpenInput(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for both the input format and the dictionary.
    /// </remarks>
    public static FormatContext OpenInput(string? url, bool findStreamInfo = false)
        => OpenInput(url, null, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using the URL and input format.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the input format has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. If <see langword="null"/>, the format will be detected automatically.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="OpenInput(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for the dictionary.
    /// </remarks>
    public static FormatContext OpenInput(string? url, InputFormat? input, bool findStreamInfo = false)
        => OpenInput(url, input, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using the URL and a multi-dictionary.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the input format has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="OpenInput(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for the input format.
    /// </remarks>
    public static FormatContext OpenInput(string? url, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(url, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using the URL and a dictionary.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the input format has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="OpenInput(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for the input format.
    /// </remarks>
    public static FormatContext OpenInput(string? url, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(url, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using the URL and a dictionary of options.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the input format has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="OpenInput(string?, InputFormat?, IDictionary<string, string>?, bool)"/> method. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public static FormatContext OpenInput(string? url, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => OpenInput(url, null, dic, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using only the input format.
    /// </summary>
    /// <param name="input">
    /// The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </remarks>
    public static FormatContext OpenInput(InputFormat input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(null as string, input, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using only the input format and a multi-dictionary.
    /// </summary>
    /// <param name="input">
    /// The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </remarks>
    public static FormatContext OpenInput(InputFormat input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(null as string, input, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using only the input format and a dictionary of options.
    /// </summary>
    /// <param name="input">
    /// The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public static FormatContext OpenInput(InputFormat input, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => OpenInput(null as string, input, dic, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="FormatContext"/> using only the input format.
    /// </summary>
    /// <param name="input">
    /// The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </remarks>
    public static FormatContext OpenInput(InputFormat input, bool findStreamInfo = false)
        => OpenInput(null as string, input, findStreamInfo);

    #endregion

    #region OpenInput Overloads With Stream

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified input format and a dictionary of options.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, InputFormat? input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        FormatContext formatContext = new(context, true)
        { isInput = true };
        _ = new IOStreamContext(formatContext, stream, IOOptions.Read | IOOptions.Seek);

        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);
        formatContext.Context = context;
        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            ffmpeg.avformat_close_input(&context);
            formatContext.Context = null;
            formatContext.Dispose();
            result.ThrowIfError();
        }
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified input format and a multi-dictionary of options.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, InputFormat? input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        FormatContext formatContext = new(context, true, true);
        _ = new IOStreamContext(formatContext, stream, IOOptions.Read | IOOptions.Seek);

        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);

        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            ffmpeg.avformat_close_input(&context);
            formatContext.Context = null;
            formatContext.Dispose();
            result.ThrowIfError();
        }
        formatContext.Context = context;
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified input format and a dictionary of options.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>. 
    /// May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, InputFormat? input, IDictionary<string, string>? dic, bool findStreamInfo = false)
    {
        if (dic is null)
            return OpenInput(stream, input, findStreamInfo);
        if (dic is Collections.AVDictionary avDic)
            return OpenInput(stream, input, avDic, findStreamInfo);
        using Collections.AVDictionary dicCopy = new(dic);
        FormatContext res = OpenInput(stream, input, dicCopy, findStreamInfo);
        dic.Clear();
        foreach (KeyValuePair<string, string> kp in dicCopy)
            dic[kp.Key] = kp.Value;
        return res;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified input format.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> and <paramref name="dictionary"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, InputFormat? input, bool findStreamInfo = false)
        => OpenInput(stream, input, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified multi-dictionary of options.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(stream, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified dictionary of options.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(stream, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified dictionary of options, which can be any <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>. 
    /// May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dic"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// If <paramref name="dic"/> is an <see cref="IDictionary{TKey, TValue}"/> other than <see cref="Collections.AVDictionary"/> or <see cref="Collections.AVMultiDictionary"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => OpenInput(stream, null, dic, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> with default settings (no input format, no dictionary of options).
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> with <paramref name="url"/>, <paramref name="input"/>, and <paramref name="dictionary"/> all set to <see langword="null"/>. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(Stream stream, bool findStreamInfo = false)
        => OpenInput(stream, null, null as Collections.AVDictionary, findStreamInfo);

    #endregion


    #region OpenInput Overloads With IOContext

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified input format and <see cref="IOContext"/>.
    /// </summary>
    /// <param name="ioContext">
    /// The <see cref="IOContext"/> to handle I/O operations for the media stream. It is responsible for setting up reading and writing capabilities.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="input"/> format with <paramref name="url"/> and <paramref name="dictionary"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="FormatContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(IOContext ioContext, InputFormat? input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        FormatContext formatContext = new(context, true, true);
        ioContext.InitContext(formatContext, IOOptions.Read | IOOptions.Write);
        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);

        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            ffmpeg.avformat_close_input(&context);
            formatContext.Context = null;
            formatContext.Dispose();
            result.ThrowIfError();
        }
        formatContext.Context = context;
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified multi-dictionary of options and <see cref="IOContext"/>.
    /// </summary>
    /// <param name="ioContext">
    /// The <see cref="IOContext"/> to handle I/O operations for the media stream. It is responsible for setting up reading and writing capabilities.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="FormatContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(IOContext ioContext, InputFormat? input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        FormatContext formatContext = new(context, true, true);
        ioContext.InitContext(formatContext, IOOptions.Read | IOOptions.Write);

        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);

        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            ffmpeg.avformat_close_input(&context);
            formatContext.Context = null;
            formatContext.Dispose();
            result.ThrowIfError();
        }
        formatContext.Context = context;
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified dictionary of options and <see cref="IOContext"/>.
    /// </summary>
    /// <param name="ioContext">
    /// The <see cref="IOContext"/> to handle I/O operations for the media stream. It is responsible for setting up reading and writing capabilities.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>. 
    /// May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dic"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// If <paramref name="dic"/> is an <see cref="IDictionary{TKey, TValue}"/> other than <see cref="Collections.AVDictionary"/> or <see cref="Collections.AVMultiDictionary"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="FormatContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(IOContext ioContext, InputFormat? input, IDictionary<string, string>? dic, bool findStreamInfo = false)
    {
        if (dic is null)
            return OpenInput(ioContext, input, findStreamInfo);
        if (dic is Collections.AVDictionary avDict)
            return OpenInput(ioContext, input, avDict, findStreamInfo);
        using Collections.AVDictionary dicCopy = new(dic);
        FormatContext res = OpenInput(ioContext, input, dicCopy, findStreamInfo);
        dic.Clear();
        foreach (KeyValuePair<string, string> kp in dicCopy)
            dic[kp.Key] = kp.Value;
        return res;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified input format and <see cref="IOContext"/>.
    /// </summary>
    /// <param name="ioContext">
    /// The <see cref="IOContext"/> to handle I/O operations for the media stream. It is responsible for setting up reading and writing capabilities.
    /// </param>
    /// <param name="input">
    /// Optional. The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="input"/> format with <paramref name="url"/> and <paramref name="dictionary"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="FormatContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(IOContext ioContext, InputFormat? input, bool findStreamInfo = false)
        => OpenInput(ioContext, input, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified multi-dictionary of options and <see cref="IOContext"/>.
    /// </summary>
    /// <param name="ioContext">
    /// The <see cref="IOContext"/> to handle I/O operations for the media stream. It is responsible for setting up reading and writing capabilities.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVMultiDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="FormatContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(IOContext ioContext, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(ioContext, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified dictionary of options and <see cref="IOContext"/>.
    /// </summary>
    /// <param name="ioContext">
    /// The <see cref="IOContext"/> to handle I/O operations for the media stream. It is responsible for setting up reading and writing capabilities.
    /// </param>
    /// <param name="dictionary">
    /// Optional. A <see cref="Collections.AVDictionary"/> containing additional options for the input. May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="FormatContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(IOContext ioContext, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => OpenInput(ioContext, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="FormatContext"/> using the specified dictionary of options and <see cref="IOContext"/>.
    /// </summary>
    /// <param name="ioContext">
    /// The <see cref="IOContext"/> to handle I/O operations for the media stream. It is responsible for setting up reading and writing capabilities.
    /// </param>
    /// <param name="dic">
    /// Optional. A dictionary of additional options for the input. This can be a <see cref="Collections.AVDictionary"/>, <see cref="Collections.AVMultiDictionary"/>, or a simple <see cref="IDictionary{TKey, TValue}"/>. 
    /// May be <see langword="null"/>.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="FormatContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dic"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// If <paramref name="dic"/> is an <see cref="IDictionary{TKey, TValue}"/> other than <see cref="Collections.AVDictionary"/> or <see cref="Collections.AVMultiDictionary"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="FormatContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static FormatContext OpenInput(IOContext ioContext, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => OpenInput(ioContext, null, dic, findStreamInfo);

    #endregion

    #endregion

    #region FindStreamInfo

    /// <summary>
    /// Finds the stream information from the media file.
    /// </summary>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 FindStreamInfo() => ffmpeg.avformat_find_stream_info(Context, null);

    /// <summary>
    /// Finds the stream information using a specified set of dictionary options.
    /// </summary>
    /// <param name="options">An array of <see cref="Collections.AVDictionary"/> for each stream.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 FindStreamInfo(Collections.AVDictionary[]? options)
        => FindStreamInfo(options != null ? options.AsSpan() : []);

    /// <summary>
    /// Finds the stream information using a span of dictionary options.
    /// </summary>
    /// <param name="options">A span of <see cref="Collections.AVDictionary"/> for each stream.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of options does not match the stream count.</exception>
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
    /// Finds the stream information using a specified set of multi-dictionary options.
    /// </summary>
    /// <param name="options">An array of <see cref="Collections.AVMultiDictionary"/> for each stream.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 FindStreamInfo(Collections.AVMultiDictionary[]? options)
        => FindStreamInfo(options != null ? options.AsSpan() : []);

    /// <summary>
    /// Finds the stream information using a span of multi-dictionary options.
    /// </summary>
    /// <param name="options">A span of <see cref="Collections.AVMultiDictionary"/> for each stream.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of options does not match the stream count.</exception>
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
    /// Finds the stream information using a span of dictionaries represented as <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="options">A span of dictionaries for each stream.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of options does not match the stream count.</exception>
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
    /// Finds the stream information using an array of dictionaries represented as <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="options">An array of dictionaries for each stream.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 FindStreamInfo(IDictionary<string, string>[]? options)
        => FindStreamInfo(options != null ? options.AsSpan() : []);

    #endregion

    #region OpenOutput

    /// <summary>
    /// Opens an output media file for writing.
    /// </summary>
    /// <param name="filename">The filename for the output media.</param>
    /// <param name="format">The output format.</param>
    /// <returns>An instance of <see cref="FormatContext"/> or <see langword="null"/> if the operation fails.</returns>
    public static FormatContext? OpenOutput(string? filename, OutputFormat? format)
    {
        FormatContext? output = AllocateOutput(filename, format);
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
    /// <returns>An instance of <see cref="FormatContext"/> or <see langword="null"/> if the operation fails.</returns>
    public static FormatContext? OpenOutput(Stream stream, OutputFormat format)
        => OpenOutput(new IOStreamContext(stream), format);

    /// <summary>
    /// Opens an output media file using an I/O context for writing.
    /// </summary>
    /// <param name="context">The I/O context for output media.</param>
    /// <param name="format">The output format.</param>
    /// <returns>An instance of <see cref="FormatContext"/> or <see langword="null"/> if the operation fails.</returns>
    public static FormatContext? OpenOutput(IOContext context, OutputFormat format)
    {
        FormatContext? output = AllocateOutput(null, format);
        if (output == null)
            return null;
        context.InitContext(output, IOOptions.Write);
        return output;
    }

    #endregion

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

    #region ReadFrame

    /// <summary>
    /// Reads a frame from the input media file.
    /// </summary>
    /// <param name="packet">The packet to read into.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
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

    #region Seek

    /// <summary>
    /// Seeks to a specific time in the media file.
    /// </summary>
    /// <param name="time">The time to seek to.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 Seek(Rational time)
    {
        Rational timeBase = new(1, ffmpeg.AV_TIME_BASE);
        long l = (long)(time / timeBase);
        return ffmpeg.av_seek_frame(Context, -1, l, ffmpeg.AVSEEK_FLAG_BACKWARD);
    }

    /// <summary>
    /// Seeks to a specific time in the media file for a given stream.
    /// </summary>
    /// <param name="time">The time to seek to.</param>
    /// <param name="streamIndex">The index of the stream to seek within.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the stream index is out of range.</exception>
    public AVResult32 Seek(Rational time, int streamIndex)
    {
        if (streamIndex < 0 || streamIndex >= StreamCount)
            throw new ArgumentOutOfRangeException(nameof(streamIndex));
        Rational timeBase = Context->streams[streamIndex]->time_base;
        long l = (long)(time / timeBase);
        return ffmpeg.av_seek_frame(Context, streamIndex, l, ffmpeg.AVSEEK_FLAG_BACKWARD);
    }

    /// <summary>
    /// Seeks to a specific timestamp in the media file.
    /// </summary>
    /// <param name="frame">The timestamp in to seek to.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 Seek(long timestamp)
        => ffmpeg.av_seek_frame(Context, -1, timestamp, ffmpeg.AVSEEK_FLAG_FRAME | ffmpeg.AVSEEK_FLAG_BACKWARD);

    /// <summary>
    /// Seeks to a specific timestamp in the media file for a given stream.
    /// </summary>
    /// <param name="frame">The timestamp number to seek to.</param>
    /// <param name="streamIndex">The index of the stream to seek within.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 Seek(long timestamp, int streamIndex)
        => ffmpeg.av_seek_frame(Context, streamIndex, timestamp, ffmpeg.AVSEEK_FLAG_FRAME | ffmpeg.AVSEEK_FLAG_BACKWARD);

    #endregion

    #region FindBestStream

    /// <summary>
    /// Finds the best stream of a given media type in the media file.
    /// </summary>
    /// <param name="type">The media type to search for.</param>
    /// <returns>The index of the best stream, or a negative value if no suitable stream is found.</returns>
    public int FindBestStream(MediaType type)
        => ffmpeg.av_find_best_stream(Context, (AutoGen._AVMediaType)type, -1, -1, null, 0);

    #endregion

    #region IDisposable

    private bool disposedValue;

    /// <summary>
    /// Disposes of the resources used by the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is being called from the <see cref="Dispose"/> method or a finalizer.</param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (!isInput && !trailerWritten)
                    _ = WriteTrailer();
                ioContext?.Dispose();
            }
            if (freeOnDispose)
            {
                if (isInput)
                {
                    AutoGen._AVFormatContext* ctx = Context;
                    ffmpeg.avformat_close_input(&ctx);
                }
                else
                {
                    ffmpeg.avformat_free_context(Context);
                }
            }
            disposedValue = true;
        }
    }



    /// <summary>
    /// Finalizes the <see cref="FormatContext"/>.
    /// </summary>
    ~FormatContext()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="FormatContext"/>.
    /// </summary>
    public void Free()
        => Dispose();

    /// <summary>
    /// Disposes of the resources used by the <see cref="FormatContext"/> and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
    // ToDo:
    private bool trailerWritten;
    public AVResult32 WriteTrailer()
    {
        if (trailerWritten)
            return 0;
        AVResult32 res = ffmpeg.av_write_trailer(Context);
        trailerWritten = !res.IsError;
        return res;
    }

    // ToDo: -40 if not supported
    public AVResult32 GetOutputTimestamp(int streamIndex, out TimeSpan timestamp, out TimeSpan wallTime)
    {
        Rational timeBase = Streams[streamIndex].TimeBase;
        long dts, wall;
        int res = ffmpeg.av_get_output_timestamp(Context, streamIndex, &dts, &wall);
        timestamp = dts * timeBase;
        wallTime = TimeSpan.FromMilliseconds(wall);
        return res;
    }

    #region GuessFrameRate

    /// <summary>
    /// Guesses the frame rate of the given stream.
    /// </summary>
    /// <param name="avStream">The stream to analyze.</param>
    /// <param name="frame">The frame, if available, to use for analysis.</param>
    /// <returns>A <see cref="Rational"/> representing the guessed frame rate.</returns>
    public Rational GuessFrameRate(AVStream avStream, AVFrame? frame)
        => ffmpeg.av_guess_frame_rate(Context, avStream.stream, frame != null ? frame.Frame : null);

    /// <summary>
    /// Guesses the frame rate of the given stream.
    /// </summary>
    /// <param name="avStream">The stream to analyze.</param>
    /// <returns>A <see cref="Rational"/> representing the guessed frame rate.</returns>
    public Rational GuessFrameRate(AVStream avStream)
        => ffmpeg.av_guess_frame_rate(Context, avStream.stream, null);

    /// <summary>
    /// Guesses the frame rate of the stream at the specified index.
    /// </summary>
    /// <param name="streamIndex">The index of the stream to analyze.</param>
    /// <param name="frame">The frame, if available, to use for analysis.</param>
    /// <returns>A <see cref="Rational"/> representing the guessed frame rate.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the stream index is out of range.</exception>
    public Rational GuessFrameRate(int streamIndex, AVFrame? frame)
        => streamIndex < 0 || streamIndex >= StreamCount
            ? throw new ArgumentOutOfRangeException(nameof(streamIndex))
            : (Rational)ffmpeg.av_guess_frame_rate(Context, Context->streams[streamIndex], frame != null ? frame.Frame : null);

    /// <summary>
    /// Guesses the frame rate of the stream at the specified index.
    /// </summary>
    /// <param name="streamIndex">The index of the stream to analyze.</param>
    /// <returns>A <see cref="Rational"/> representing the guessed frame rate.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the stream index is out of range.</exception>
    public Rational GuessFrameRate(int streamIndex)
        => GuessFrameRate(streamIndex, null);

    #endregion


    public override string ToString() => $"{(InputFormat.HasValue ? InputFormat.Value.ToString() : OutputFormat.HasValue ? OutputFormat.Value : "Unknown")}";
}
