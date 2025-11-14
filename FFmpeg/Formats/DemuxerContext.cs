using FFmpeg.IO;
using FFmpeg.Utils;

namespace FFmpeg.Formats;

public unsafe class DemuxerContext : FormatContext
{
    #region Constructions

    /// <summary>
    /// Initializes a new instance of the <see cref="DemuxerContext"/> class with an existing <see cref="AutoGen._AVFormatContext"/>*.
    /// </summary>
    /// <param name="context">The already allocated context.</param>
    /// <param name="freeOnDispose">Indicates whether the underlying <see cref="AutoGen._AVFormatContext"/>* should be freed when this object is disposed.</param>
    protected DemuxerContext(AutoGen._AVFormatContext* context) : base(context)
    {
    }


    #endregion

    /// <summary>
    /// Gets the input format associated with this context, if available.
    /// </summary>
    public InputFormat? InputFormat => Context->iformat != null ? new(Context->iformat) : null;

    #region Properties

    #endregion

    #region Open

    /// <summary>
    /// Opens an input media file and initializes the <see cref="DemuxerContext"/>.
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
    /// An <see cref="AVResult32"/> indicating the result of the operation. If the result is successful, the <see cref="DemuxerContext"/> is initialized with the input media.
    /// </returns>
    /// <remarks>
    /// This method configures the <see cref="DemuxerContext"/> for reading from the specified input URL or file. The <see cref="Formats.InputFormat"/> can be specified if known; otherwise, it will be auto-detected. The <see cref="Collections.AVDictionary"/> allows setting additional options for the input.
    /// </remarks>
    public AVResult32 Open(string? url, InputFormat? input, Collections.AVDictionary? dictionary)
    {
        AutoGen._AVFormatContext* context = Context;
        AutoGen._AVInputFormat* format = input == null ? null : input.Value.Format;
        AutoGen._AVDictionary* dic = dictionary == null ? null : dictionary.dictionary;
        int res = ffmpeg.avformat_open_input(&context, url, format, &dic);
        Context = context;
        return res;
    }

    /// <summary>
    /// Opens an input media file and initializes the <see cref="DemuxerContext"/>.
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
    /// An <see cref="AVResult32"/> indicating the result of the operation. If the result is successful, the <see cref="DemuxerContext"/> is initialized with the input media.
    /// </returns>
    /// <remarks>
    /// This method configures the <see cref="DemuxerContext"/> for reading from the specified input URL or file. The <see cref="Formats.InputFormat"/> can be specified if known; otherwise, it will be auto-detected. The <see cref="Collections.AVMultiDictionary"/> allows setting additional options for the input.
    /// </remarks>
    public AVResult32 Open(string? url, InputFormat? input, Collections.AVMultiDictionary? dictionary)
    {
        AutoGen._AVFormatContext* context = Context;
        AutoGen._AVInputFormat* format = input == null ? null : input.Value.Format;
        AutoGen._AVDictionary* dic = dictionary == null ? null : dictionary.dictionary;
        int res = ffmpeg.avformat_open_input(&context, url, format, &dic);
        Context = context;
        return res;
    }

    /// <summary>
    /// Opens an input media file and initializes the <see cref="DemuxerContext"/>.
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
    /// An <see cref="AVResult32"/> indicating the result of the operation. If the result is successful, the <see cref="DemuxerContext"/> is initialized with the input media.
    /// </returns>
    /// <remarks>
    /// This method configures the <see cref="DemuxerContext"/> for reading from the specified input URL or file. The <see cref="Formats.InputFormat"/> can be specified if known; otherwise, it will be auto-detected. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public AVResult32 Open(string? url, InputFormat? input, IDictionary<string, string>? dic)
    {
        if (dic is Collections.AVDictionary dictionary)
        {
            return Open(url, input, dictionary);
        }
        else if (dic == null)
        {
            return Open(url, input, null as Collections.AVDictionary);
        }
        else
        {
            using Collections.AVDictionary dic2 = new(dic);
            AVResult32 result = Open(url, input, dic2);
            dic.Clear();
            foreach (KeyValuePair<string, string> kvp in dic2)
                dic[kvp.Key] = kvp.Value;
            return result;
        }
    }

    #region Open with string

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input specified by the <paramref name="url"/> or device, using the provided <paramref name="input"/> format if specified. If <paramref name="url"/> is <see langword="null"/> and the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag, it is treated as a special device or input format that does not require a file path.
    /// </remarks>
    public static DemuxerContext Open(string? url, InputFormat? input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
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

        return new DemuxerContext(context);
    }

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input specified by the <paramref name="url"/> or device, using the provided <paramref name="input"/> format if specified. If <paramref name="url"/> is <see langword="null"/> and the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag, it is treated as a special device or input format that does not require a file path.
    /// </remarks>
    public static DemuxerContext Open(string? url, InputFormat? input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
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

        return new DemuxerContext(context);
    }

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input specified by the <paramref name="url"/> or device, using the provided <paramref name="input"/> format if specified. If <paramref name="url"/> is <see langword="null"/> and the <paramref name="input"/> has the <see cref="FormatFlags.NoFile"/> flag, it is treated as a special device or input format that does not require a file path. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public static DemuxerContext Open(string? url, InputFormat? input, IDictionary<string, string>? dic, bool findStreamInfo = false)
    {
        if (dic is Collections.AVDictionary dictionary)
        {
            return Open(url, input, dictionary);
        }
        else if (dic == null)
        {
            return Open(url, input, null as Collections.AVDictionary, findStreamInfo);
        }
        else
        {
            using Collections.AVDictionary dic2 = new(dic);
            DemuxerContext? result = Open(url, input, dic2);
            dic.Clear();
            foreach (KeyValuePair<string, string> kvp in dic2)
                dic[kvp.Key] = kvp.Value;
            return result;
        }
    }

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using only the URL.
    /// </summary>
    /// <param name="url">
    /// The URL or path to the input media file or device. This can be <see langword="null"/> if the input format has the <see cref="FormatFlags.NoFile"/> flag set.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="Open(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for both the input format and the dictionary.
    /// </remarks>
    public static DemuxerContext Open(string? url, bool findStreamInfo = false)
        => Open(url, null, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using the URL and input format.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="Open(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for the dictionary.
    /// </remarks>
    public static DemuxerContext Open(string? url, InputFormat? input, bool findStreamInfo = false)
        => Open(url, input, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using the URL and a multi-dictionary.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="Open(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for the input format.
    /// </remarks>
    public static DemuxerContext Open(string? url, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => Open(url, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using the URL and a dictionary.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="Open(string?, InputFormat?, Collections.AVDictionary?, bool)"/> method with <see langword="null"/> for the input format.
    /// </remarks>
    public static DemuxerContext Open(string? url, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => Open(url, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using the URL and a dictionary of options.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This overload calls the main <see cref="Open(string?, InputFormat?, IDictionary<string, string>?, bool)"/> method. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public static DemuxerContext Open(string? url, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => Open(url, null, dic, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using only the input format.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </remarks>
    public static DemuxerContext Open(InputFormat input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => Open(null as string, input, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using only the input format and a multi-dictionary.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </remarks>
    public static DemuxerContext Open(InputFormat input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => Open(null as string, input, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using only the input format and a dictionary of options.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
    /// </remarks>
    public static DemuxerContext Open(InputFormat input, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => Open(null as string, input, dic, findStreamInfo);

    /// <summary>
    /// Opens an input media file or device and initializes the <see cref="DemuxerContext"/> using only the input format.
    /// </summary>
    /// <param name="input">
    /// The <see cref="Formats.InputFormat"/> to use for parsing the input. This format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="DemuxerContext"/> initialized with the input media or device, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </remarks>
    public static DemuxerContext Open(InputFormat input, bool findStreamInfo = false)
        => Open(null as string, input, findStreamInfo);

    #endregion

    #region Open Overloads With Stream

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified input format and a dictionary of options.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, InputFormat? input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        DemuxerContext formatContext = new(context);

        _ = IOStreamContext.OpenRead(formatContext, stream);

        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);
        formatContext.Context = context;
        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            formatContext.Dispose();
            result.ThrowIfError();
        }
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified input format and a multi-dictionary of options.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, InputFormat? input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        DemuxerContext formatContext = new(context);
        _ = IOStreamContext.OpenRead(formatContext, stream);

        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);
        formatContext.Context = context;
        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            formatContext.Dispose();
            result.ThrowIfError();
        }
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified input format and a dictionary of options.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, InputFormat? input, IDictionary<string, string>? dic, bool findStreamInfo = false)
    {
        if (dic is null)
            return Open(stream, input, findStreamInfo);
        if (dic is Collections.AVDictionary avDic)
            return Open(stream, input, avDic, findStreamInfo);
        using Collections.AVDictionary dicCopy = new(dic);
        DemuxerContext res = Open(stream, input, dicCopy, findStreamInfo);
        dic.Clear();
        foreach (KeyValuePair<string, string> kp in dicCopy)
            dic[kp.Key] = kp.Value;
        return res;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified input format.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format with <paramref name="url"/> and <paramref name="dictionary"/> set to <see langword="null"/>. 
    /// The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating that no file path is required. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, InputFormat? input, bool findStreamInfo = false)
        => Open(stream, input, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified multi-dictionary of options.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => Open(stream, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified dictionary of options.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="dictionary"/> is used to pass additional options to the input. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => Open(stream, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified dictionary of options, which can be any <see cref="IDictionary{TKey, TValue}"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dic"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// If <paramref name="dic"/> is an <see cref="IDictionary{TKey, TValue}"/> other than <see cref="Collections.AVDictionary"/> or <see cref="Collections.AVMultiDictionary"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => Open(stream, null, dic, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> with default settings (no input format, no dictionary of options).
    /// </summary>
    /// <param name="stream">
    /// The <see cref="Stream"/> to read the input media data from. It must support reading and seeking.
    /// </param>
    /// <param name="findStreamInfo">
    /// Optional. If <see langword="true"/>, will call <see cref="ffmpeg.avformat_find_stream_info"/> after opening the input to find stream information.
    /// </param>
    /// <returns>
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="stream"/> with <paramref name="url"/>, <paramref name="input"/>, and <paramref name="dictionary"/> all set to <see langword="null"/>. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, bool findStreamInfo = false)
        => Open(stream, null, null as Collections.AVDictionary, findStreamInfo);

    #endregion


    #region Open Overloads With IOContext

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified input format and <see cref="IOContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="input"/> format with <paramref name="url"/> and <paramref name="dictionary"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, InputFormat? input, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        DemuxerContext formatContext = new(context);
        ioContext.InitContext(formatContext, IOOptions.Read);
        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);
        formatContext.Context = context;

        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            formatContext.Dispose();
            result.ThrowIfError();
        }
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified multi-dictionary of options and <see cref="IOContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, InputFormat? input, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
    {
        AutoGen._AVInputFormat* iFormat = input != null ? input.Value.Format : null;
        AutoGen._AVFormatContext* context = ffmpeg.avformat_alloc_context();
        if (context == null)
            throw new OutOfMemoryException();
        DemuxerContext formatContext = new(context);
        if (ioContext.CanSeek)
            ioContext.InitContext(formatContext, IOOptions.Read | IOOptions.Seek);
        else
            ioContext.InitContext(formatContext, IOOptions.Read);

        AutoGen._AVDictionary* dic = dictionary != null ? dictionary.dictionary : null;
        AVResult32 result = ffmpeg.avformat_open_input(&context, null, iFormat, &dic);
        formatContext.Context = context;

        dictionary?.dictionary = dic;
        if (result.IsError)
        {
            formatContext.Dispose();
            result.ThrowIfError();
        }
        if (findStreamInfo)
            _ = ffmpeg.avformat_find_stream_info(context, null);

        return formatContext;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified dictionary of options and <see cref="IOContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dic"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// If <paramref name="dic"/> is an <see cref="IDictionary{TKey, TValue}"/> other than <see cref="Collections.AVDictionary"/> or <see cref="Collections.AVMultiDictionary"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, InputFormat? input, IDictionary<string, string>? dic, bool findStreamInfo = false)
    {
        if (dic is null)
            return Open(ioContext, input, findStreamInfo);
        if (dic is Collections.AVDictionary avDict)
            return Open(ioContext, input, avDict, findStreamInfo);
        using Collections.AVDictionary dicCopy = new(dic);
        DemuxerContext res = Open(ioContext, input, dicCopy, findStreamInfo);
        dic.Clear();
        foreach (KeyValuePair<string, string> kp in dicCopy)
            dic[kp.Key] = kp.Value;
        return res;
    }

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified input format and <see cref="IOContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="input"/> format with <paramref name="url"/> and <paramref name="dictionary"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, InputFormat? input, bool findStreamInfo = false)
        => Open(ioContext, input, null as Collections.AVDictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified multi-dictionary of options and <see cref="IOContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, Collections.AVMultiDictionary? dictionary, bool findStreamInfo = false)
        => Open(ioContext, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified dictionary of options and <see cref="IOContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => Open(ioContext, null, dictionary, findStreamInfo);

    /// <summary>
    /// Opens an input media stream and initializes the <see cref="DemuxerContext"/> using the specified dictionary of options and <see cref="IOContext"/>.
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
    /// An <see cref="DemuxerContext"/> initialized with the input media stream, or <see langword="null"/> if the operation fails. Throws an <see cref="OutOfMemoryException"/> if memory allocation fails.
    /// </returns>
    /// <remarks>
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dic"/> with <paramref name="url"/> and <paramref name="input"/> set to <see langword="null"/>. 
    /// If <paramref name="dic"/> is an <see cref="IDictionary{TKey, TValue}"/> other than <see cref="Collections.AVDictionary"/> or <see cref="Collections.AVMultiDictionary"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => Open(ioContext, null, dic, findStreamInfo);

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

    #region ReadFrame

    /// <summary>
    /// Reads a frame from the input media file.
    /// </summary>
    /// <param name="packet">The packet to read into.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
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

    /// <summary>
    /// Reads a frame from the input media file.
    /// </summary>
    /// <param name="packet">The packet to read into.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the result of the operation.</returns>
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

    public long StartTime => Context->start_time;
    public long Duration => Context->duration;
    public long BitRate => Context->bit_rate;


    public override ChapterList Chapters => new(this, true);

    // ToDo: Find Program, Add Program, FindBestStream overloads

    #region Dispose
    protected override void Dispose(bool disposing) => base.Dispose(disposing);
    #endregion

    public override string ToString() => InputFormat?.LongName ?? "Unknown";
}
