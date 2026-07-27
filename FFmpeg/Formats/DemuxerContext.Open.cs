using FFmpeg.IO;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Formats;

public unsafe partial class DemuxerContext : FormatContext
{
    #region Open

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
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format. 
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
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format. 
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
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format. 
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
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dic"/>. 
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
    #endregion

    #endregion


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
    /// This overload calls the main <see cref="Open(string?, Formats.InputFormat?, IDictionary{string, string}?, bool)"/> method. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
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
    /// This method opens the input using the specified <paramref name="input"/> format with url set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
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
    /// This method opens the input using the specified <paramref name="input"/> format with url set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
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
    /// This method opens the input using the specified <paramref name="input"/> format with url set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path. If a dictionary is provided as an <see cref="IDictionary{TKey, TValue}"/>, it will be converted to a <see cref="Collections.AVDictionary"/> internally.
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
    /// This method opens the input using the specified <paramref name="input"/> format with url set to <see langword="null"/>. The input format must have the <see cref="FormatFlags.NoFile"/> flag set, indicating it does not require a file path.
    /// </remarks>
    public static DemuxerContext Open(InputFormat input, bool findStreamInfo = false)
        => Open(null as string, input, findStreamInfo);

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
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="input"/> format. 
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
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dictionary"/>. 
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
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dictionary"/>. 
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
    /// This method opens the input using the specified <paramref name="stream"/> and <paramref name="dic"/>. 
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
    /// This method opens the input using the specified <paramref name="stream"/>. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(Stream stream, bool findStreamInfo = false)
        => Open(stream, null, null as Collections.AVDictionary, findStreamInfo);

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
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="input"/> format. 
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
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with url and input set to <see langword="null"/>. 
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
    /// This method opens the input using the specified <paramref name="ioContext"/> and <paramref name="dictionary"/> with url and input set to <see langword="null"/>. 
    /// The <paramref name="ioContext"/> is used to initialize the <see cref="DemuxerContext"/> with read and write capabilities. 
    /// After opening the input, the <paramref name="findStreamInfo"/> flag determines if <see cref="ffmpeg.avformat_find_stream_info"/> will be called to find stream information.
    /// </remarks>
    public static DemuxerContext Open(IOContext ioContext, Collections.AVDictionary? dictionary, bool findStreamInfo = false)
        => Open(ioContext, null, dictionary, findStreamInfo);

    /// <inheritdoc cref="Open(IOContext, Formats.InputFormat?, IDictionary{string, string}?, bool)"/>
    public static DemuxerContext Open(IOContext ioContext, IDictionary<string, string>? dic, bool findStreamInfo = false)
        => Open(ioContext, null, dic, findStreamInfo);
}
