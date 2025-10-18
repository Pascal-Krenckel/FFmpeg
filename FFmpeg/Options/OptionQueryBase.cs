using FFmpeg.Collections;
using FFmpeg.Audio;
using FFmpeg.Utils;
using System.Runtime.InteropServices;
using FFmpeg.Images;

namespace FFmpeg.Options;

/// <summary>
/// An abstract base class for querying options from AV-option-enabled classes.
/// </summary>
/// <remarks>
/// This class provides methods to retrieve or set various types of options from AV-option-enabled classes.
/// </remarks>
public abstract unsafe class OptionQueryBase : IOptionQuery
{
    /// <summary>
    /// Abstract base class that provides access to a pointer for AVOptions retrieval.
    /// </summary>
    protected abstract void* Pointer { get; }

    /// <summary>
    /// Recursively retrieves AVOptions for a given pointer and adds them to a provided list.
    /// Deprecated options are not included.
    /// </summary>
    /// <param name="ptr">The pointer to the object from which options are retrieved.</param>
    /// <param name="options">The list of <see cref="Option"/> to which valid options are added.</param>
    /// <param name="recursive">Whether to retrieve options from child objects recursively.</param>
    private void GetOptions(void* ptr, List<Option> options, bool recursive)
    {
        AutoGen._AVOption* option = null;
        while ((option = AutoGen.ffmpeg.av_opt_next(ptr, option)) != null)
            if ((option->flags & ffmpeg.AV_OPT_FLAG_DEPRECATED) == 0)
                options.Add(new(option)); // Do not add deprecated options, as they should not be used.

        if (recursive)
        {
            void* child = null;
            while ((child = AutoGen.ffmpeg.av_opt_child_next(ptr, child)) != null)
                GetOptions(child, options, recursive);
        }
    }

    /// <summary>
    /// Reset all options to their default value.
    /// </summary>
    public void ResetOptions() => ffmpeg.av_opt_set_defaults(Pointer);

    /// <summary>
    /// Retrieves a list of available AVOptions, optionally including those from child objects recursively.
    /// </summary>
    /// <param name="recursive">Specifies whether to include options from child objects recursively.</param>
    /// <returns>A read-only list of <see cref="Option"/> objects.</returns>
    public IReadOnlyList<Option> GetOptions(bool recursive = true)
    {
        List<Option> options = [];
        GetOptions(Pointer, options, recursive);
        return options;
    }

    /// <summary>
    /// Finds and returns an AVOption by name, optionally searching recursively through child objects.
    /// </summary>
    /// <param name="name">The name of the option to find.</param>
    /// <param name="recursive">Specifies whether to search recursively through child objects.</param>
    /// <returns>The found <see cref="Option"/>, or <c>null</c> if not found.</returns>
    public Option? FindOption(string name, bool recursive = true)
        => Option.Create(ffmpeg.av_opt_find(Pointer, name, null, 0, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0));

    /// <summary>
    /// Attempts to convert the source object to the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to which to convert the object.</typeparam>
    /// <param name="source">The source object to be converted.</param>
    /// <param name="value">The converted value, or default if conversion fails.</param>
    /// <returns><c>true</c> if conversion succeeds, otherwise <c>false</c>.</returns>
    private static bool TryConvert<T>(object? source, out T value)
    {
        if (source is T t)
        {
            value = t;
            return true;
        }
        else if (source == null)
        {
            value = default!;
            return false;
        }
        else if (typeof(T).IsAssignableFrom(source.GetType()))
        {
            value = (T)source;
            return true;
        }
        else if (typeof(T) == typeof((int, int)))
        {
            if (TryCheckIsSize<int>(source, out (int, int) size))
            {
                value = (T)(object)size;
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }
        else if (typeof(T) == typeof(long))
        {
            switch (Type.GetTypeCode(source.GetType()))
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    value = (T)Convert.ChangeType(source, typeof(T));
                    return true;
                default:
                    if (source.GetType() == typeof(AVResult32))
                    {
                        AVResult32 avResult = (AVResult32)source;
                        long l = avResult;
                        value = (T)Convert.ChangeType(l, typeof(T));
                        return true;
                    }
                    if (source.GetType() == typeof(AVResult64))
                    {
                        AVResult64 avResult = (AVResult64)source;
                        long l = avResult;
                        value = (T)Convert.ChangeType(l, typeof(T));
                        return true;
                    }
                    break;
            }
        }
        else if (typeof(T) == typeof(double) && source.GetType() == typeof(float))
        {
            value = (T)Convert.ChangeType(source, typeof(T));
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Represents a structure for size with width and height properties.
    /// </summary>
    private struct Size
    {
        /// <summary>
        /// Gets or sets the width of the size.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the size.
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// Tries to retrieve the value of a field or property from the specified source object by name.
    /// </summary>
    /// <typeparam name="T">The expected type of the member value.</typeparam>
    /// <param name="source">The source object to retrieve the member value from.</param>
    /// <param name="memberName">The name of the member to retrieve.</param>
    /// <param name="value">The retrieved value if successful, otherwise default.</param>
    /// <returns><c>true</c> if the member value is found and matches the expected type; otherwise, <c>false</c>.</returns>
    private static bool TryGetMember<T>(object source, string memberName, out T value)
    {
        System.Reflection.MemberInfo[] members = source.GetType().GetMember(memberName, System.Reflection.MemberTypes.Property | System.Reflection.MemberTypes.Field,
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetField |
        System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.IgnoreCase |
        System.Reflection.BindingFlags.Instance);

        if (members.Length == 0)
        {
            value = default!;
            return false;
        }

        for (int i = 0; i < members.Length; i++)
        {
            System.Reflection.MemberInfo member = members[i];
            if (member is System.Reflection.FieldInfo fi && fi.FieldType == typeof(T))
            {
                value = (T)fi.GetValue(source);
                return true;
            }
            else if (member is System.Reflection.PropertyInfo pi && pi.PropertyType == typeof(T))
            {
                value = (T)pi.GetValue(source);
                return true;
            }
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to check whether the source object has "Width" and "Height" members and retrieves them as a tuple of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the width and height values.</typeparam>
    /// <param name="source">The source object to inspect.</param>
    /// <param name="value">A tuple containing the width and height if successful, otherwise default.</param>
    /// <returns><c>true</c> if both width and height are found; otherwise, <c>false</c>.</returns>
    private static bool TryCheckIsSize<T>(object source, out (T, T) value)
    {
        if (TryGetMember(source, "Width", out T width) && TryGetMember(source, "Height", out T height))
        {
            value = (width, height);
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Sets an option by name with a string value.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="value">The string value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, string value, bool recursive = true)
        => ffmpeg.av_opt_set(Pointer, name, value, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a long integer value.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="value">The long integer value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, long value, bool recursive = true)
        => ffmpeg.av_opt_set_int(Pointer, name, value, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a boolean value.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="value">The boolean value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, bool value, bool recursive = true)
        => ffmpeg.av_opt_set_int(Pointer, name, Convert.ToInt32(value), recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a double value.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="value">The double value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, double value, bool recursive = true)
        => ffmpeg.av_opt_set_double(Pointer, name, value, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a Rational value.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="value">The rational value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, Rational value, bool recursive = true)
        => ffmpeg.av_opt_set_q(Pointer, name, value, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a ReadOnlySpan of bytes.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="span">The span of bytes to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, ReadOnlySpan<byte> span, bool recursive = true)
    {
        fixed (byte* b = span)
            return ffmpeg.av_opt_set_bin(Pointer, name, b, span.Length, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
    }

    /// <summary>
    /// Sets an option by name with a ReadOnlyMemory of bytes.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="memory">The memory of bytes to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, ReadOnlyMemory<byte> memory, bool recursive = true)
    {
        using System.Buffers.MemoryHandle pin = memory.Pin();
        return ffmpeg.av_opt_set_bin(Pointer, name, (byte*)pin.Pointer, memory.Length, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
    }

    /// <summary>
    /// Sets an option by name with image size (width and height).
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="width">The image width to set.</param>
    /// <param name="height">The image height to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, int width, int height, bool recursive = true)
        => ffmpeg.av_opt_set_image_size(Pointer, name, width, height, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a tuple representing image size (Width, Height).
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="size">A tuple containing width and height values.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, (int Width, int Height) size, bool recursive = true)
        => ffmpeg.av_opt_set_image_size(Pointer, name, size.Width, size.Height, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a pixel format.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="format">The pixel format to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, PixelFormat format, bool recursive = true)
        => ffmpeg.av_opt_set_pixel_fmt(Pointer, name, (AutoGen._AVPixelFormat)format, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a sample format.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="format">The sample format to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, SampleFormat format, bool recursive = true)
        => ffmpeg.av_opt_set_sample_fmt(Pointer, name, (AutoGen._AVSampleFormat)format, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);

    /// <summary>
    /// Sets an option by name with a dictionary of key-value pairs.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="values">The dictionary of key-value pairs to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, IDictionary<string, string> values, bool recursive = true)
    {
        if (values is AVDictionary_ref pDic)
            return ffmpeg.av_opt_set_dict_val(Pointer, name, *pDic.Pointer, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        else if (values is Collections.AVDictionary dic)
            return ffmpeg.av_opt_set_dict_val(Pointer, name, dic.dictionary, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        else
        {
            using Collections.AVDictionary dictionary = new(values);
            return ffmpeg.av_opt_set_dict_val(Pointer, name, dictionary.dictionary, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        }
    }

    /// <summary>
    /// Sets an option by name with a lookup collection of key-value pairs.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="values">The lookup collection of key-value pairs to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, ILookup<string, string> values, bool recursive = true)
    {
        if (values is AVMultiDictionary_ref pDic)
            return ffmpeg.av_opt_set_dict_val(Pointer, name, *pDic.dictionary, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        else if (values is Collections.AVMultiDictionary dic)
            return ffmpeg.av_opt_set_dict_val(Pointer, name, dic.dictionary, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        else
        {
            using Collections.AVMultiDictionary dictionary = new(values);
            return ffmpeg.av_opt_set_dict_val(Pointer, name, dictionary.dictionary, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        }
    }

    /// <summary>
    /// Sets multiple options using a dictionary of key-value pairs.
    /// </summary>
    /// <param name="values">The dictionary of key-value pairs to set as options.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(IDictionary<string, string> values, bool recursive = true)
    {
        if (values is AVDictionary_ref pDic)
            return ffmpeg.av_opt_set_dict2(Pointer, pDic.Pointer, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        else if (values is Collections.AVDictionary dic)
        {
            AutoGen._AVDictionary* dicPointer = dic.dictionary;
            int result = ffmpeg.av_opt_set_dict2(Pointer, &dicPointer, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
            dic.dictionary = dicPointer;
            return result;
        }
        else
        {
            using Collections.AVDictionary dictionary = new(values);
            AutoGen._AVDictionary* dicPointer = dictionary.dictionary;
            int result = ffmpeg.av_opt_set_dict2(Pointer, &dicPointer, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
            dictionary.dictionary = dicPointer;
            values.Clear();
            foreach (KeyValuePair<string, string> kv in dictionary)
                values.Add(kv.Key, kv.Value);
            return result;
        }
    }

    /// <summary>
    /// Sets multiple options using a reference to a lookup collection of key-value pairs.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="values">The lookup collection of key-value pairs to set as options.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, ref ILookup<string, string> values, bool recursive = true)
    {
        if (values is AVMultiDictionary_ref pDic)
            return ffmpeg.av_opt_set_dict2(Pointer, pDic.dictionary, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        else if (values is Collections.AVMultiDictionary dic)
        {
            AutoGen._AVDictionary* p = dic.dictionary;
            int res = ffmpeg.av_opt_set_dict2(Pointer, &p, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
            dic.dictionary = p;
            return res;
        }
        else
        {
            using Collections.AVMultiDictionary dictionary = new(values);
            int res = ffmpeg.av_opt_set_dict_val(Pointer, name, dictionary.dictionary, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
            values = dictionary.SelectMany(group => group, (g, Value) => (g.Key, Value)).ToLookup(kv => kv.Key, kv => kv.Value);
            return res;
        }
    }

    /// <summary>
    /// Sets an option by name with a generic object value.
    /// </summary>
    /// <param name="name">The option name to set.</param>
    /// <param name="value">The value to set (can be string, long, double, etc.).</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(string name, object value, bool recursive = true)
    {
        if (TryConvert(value, out string str)) return SetOption(name, str, recursive);
        if (TryConvert(value, out long l)) return SetOption(name, l, recursive);
        if (TryConvert(value, out double d)) return SetOption(name, d, recursive);
        if (TryConvert(value, out Rational r)) return SetOption(name, r, recursive);
        if (value is byte[] bytes) return SetOption(name, bytes.AsSpan(), recursive);
        if (value is Memory<byte> memory) return SetOption(name, memory, recursive);
        if (TryConvert(value, out (int, int) size)) return SetOption(name, size.Item1, size.Item2, recursive);
        if (TryConvert(value, out PixelFormat pixFmt)) return SetOption(name, pixFmt, recursive);
        if (TryConvert(value, out SampleFormat sampleFmt)) return SetOption(name, sampleFmt, recursive);
        if (TryConvert(value, out IDictionary<string, string> dict)) return SetOption(name, dict, recursive);
        return TryConvert(value, out ILookup<string, string> lookup)
            ? SetOption(name, lookup, recursive)
            : TryConvert(value, out ChannelLayout layout) ? SetOption(name, layout, recursive) : (AVResult32)AVResult32.InvalidData;
    }



    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a string value.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The string value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, string value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a boolean value.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The boolean value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, bool value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a long value.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The long value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, long value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a double value.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The double value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, double value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a Rational value.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The Rational value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, Rational value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a span of bytes.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The span of bytes to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, ReadOnlySpan<byte> value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a memory of bytes.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The memory of bytes to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, ReadOnlyMemory<byte> value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and image dimensions (width and height).
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="width">The width value to set.</param>
    /// <param name="height">The height value to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, int width, int height, bool recursive = true)
        => SetOption(option.Name, width, height, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a tuple representing image size.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="size">The tuple containing width and height values.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, (int Width, int Height) size, bool recursive = true)
        => SetOption(option.Name, size, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a pixel format.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The pixel format to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, PixelFormat value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a sample format.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The sample format to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, SampleFormat value, bool recursive = true)
        => SetOption(option.Name, value, recursive);


    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a dictionary of key-value pairs.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="values">The dictionary of key-value pairs to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, IDictionary<string, string> values, bool recursive = true)
        => SetOption(option.Name, values, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a lookup collection of key-value pairs.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="values">The lookup collection of key-value pairs to set.</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, ILookup<string, string> values, bool recursive = true)
        => SetOption(option.Name, values, recursive);

    /// <summary>
    /// Sets an option using an <see cref="Option"/> and a generic object value.
    /// </summary>
    /// <param name="option">The AVOption to set.</param>
    /// <param name="value">The object value to set (can be string, long, double, etc.).</param>
    /// <param name="recursive">Whether to search child objects recursively.</param>
    /// <returns>An AVResult32 indicating the success of the operation.</returns>
    public AVResult32 SetOption(Option option, object value, bool recursive = true)
        => SetOption(option.Name, value, recursive);

    /// <summary>
    /// Attempts to retrieve an option as a string from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the string representation of the option value, or null if the option is not found.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation.</returns>
    /// <remarks>
    /// If the option is found and is of type binary, it will be converted to a hexadecimal string representation.
    /// If the option is not found or is invalid, appropriate error codes will be returned.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out string? value, bool recursive = true)
    {
        value = default;
        void* ptr;
        AutoGen._AVOption* ptrOption = ffmpeg.av_opt_find2(Pointer, name, null, 0, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &ptr);
        if (ptrOption == null || ptr == null || (ptrOption->offset <= 0 && !ptrOption->type.HasFlag(OptionType.Constant)))
            return AVResult32.OptionNotFound;
        void* dst = (byte*)ptr + ptrOption->offset;
        if (ptrOption->type == AutoGen._AVOptionType.AV_OPT_TYPE_BINARY)
        {
            byte* src = *(byte**)ptr;
            if (src == null)
            {
                value = string.Empty;
                return 0;
            }
            long size = *((int*)ptr+1);
            if ((size * 2) + 1 > int.MaxValue)
                return AVResult32.InvalidArgument;
            byte[] buffer = new byte[size];
            fixed (byte* by = buffer)
                Buffer.MemoryCopy(src, by, size, size);
            value = BitConverter.ToString(buffer);
            return 0;
        }
        byte* b;
        AVResult32 res = ffmpeg.av_opt_get(ptr, name, ffmpeg.AV_OPT_ALLOW_NULL, &b);
        if (res < 0) return res;
        value = b == null ? string.Empty : Marshal.PtrToStringUTF8((nint)b);
        ffmpeg.av_free(b);
        return res;

    }

    /// <summary>
    /// Attempts to retrieve an option as a 64-bit signed integer from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the 64-bit signed integer value of the option, or 0 if the option is not found.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 TryGetOption(string name, out long value, bool recursive = true)
    {
        long v;
        AVResult32 res = ffmpeg.av_opt_get_int(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = v;
        return res;
    }


    /// <summary>
    /// Attempts to retrieve an option as a 64-bit unsigned integer from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the 64-bit unsigned integer value of the option, or 0 if the option is not found.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 TryGetOption(string name, out ulong value, bool recursive = true)
    {
        long v;
        AVResult32 res = ffmpeg.av_opt_get_int(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = (ulong)v;
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a 32-bit signed integer from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the 32-bit signed integer value of the option, or 0 if the option is not found.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 TryGetOption(string name, out int value, bool recursive = true)
    {
        long v;
        AVResult32 res = ffmpeg.av_opt_get_int(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        unchecked { value = (int)v; }
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a boolean value from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the boolean value of the option, or false if the option is not found.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation.</returns>
    public AVResult32 TryGetOption(string name, out bool value, bool recursive = true)
    {
        long v;
        AVResult32 res = ffmpeg.av_opt_get_int(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = Convert.ToBoolean(v);
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a 32-bit unsigned integer from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the 32-bit unsigned integer value of the option, or 0 if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as a 64-bit signed integer, and then converted to a 32-bit unsigned integer.
    /// Ensure that the value fits within the range of a 32-bit unsigned integer; otherwise, unexpected results may occur.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out uint value, bool recursive = true)
    {
        long v;
        AVResult32 res = ffmpeg.av_opt_get_int(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        unchecked { value = (uint)v; }
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a double-precision floating point number from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the double-precision floating point value of the option, or 0.0 if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as a double-precision floating point number directly.
    /// This method is useful for retrieving options that are represented as floating point values in the underlying AV system.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out double value, bool recursive = true)
    {
        double v;
        AVResult32 res = ffmpeg.av_opt_get_double(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = v;
        return res;
    }


    /// <summary>
    /// Attempts to retrieve an option as a rational number from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the <see cref="Rational"/> value of the option, or a default value if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="_AVRational"/> structure and assigned to the <see cref="Rational"/> output parameter.
    /// This method is useful for options that represent rational values (e.g., frame rates or timebases).
    /// </remarks>
    public AVResult32 TryGetOption(string name, out Rational value, bool recursive = true)
    {
        AutoGen._AVRational v;
        AVResult32 res = ffmpeg.av_opt_get_q(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = v;
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as an <see cref="PixelFormat"/> from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the <see cref="PixelFormat"/> value of the option, or a default value if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="PixelFormat"/> enumeration value, which represents pixel formats used in video processing.
    /// This method is useful for retrieving options related to image formats or color spaces.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out PixelFormat value, bool recursive = true)
    {
        AutoGen._AVPixelFormat v;
        AVResult32 res = ffmpeg.av_opt_get_pixel_fmt(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = (PixelFormat)v;
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as an <see cref="_AVSampleFormat"/> from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the <see cref="_AVSampleFormat"/> value of the option, or a default value if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="_AVSampleFormat"/> enumeration value, which represents audio sample formats.
    /// This method is useful for retrieving options related to audio data formats.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out SampleFormat value, bool recursive = true)
    {
        AutoGen._AVSampleFormat v;
        AVResult32 res = ffmpeg.av_opt_get_sample_fmt(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = (SampleFormat)v;
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a dictionary of key-value pairs from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains a dictionary where keys and values are strings representing the option's key-value pairs, or an empty dictionary if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="AutoGen._AVDictionary"/> and converted into a .NET <see cref="IDictionary{String, String}"/>.
    /// This method is useful for options that are represented as key-value pairs in the underlying AV system.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out IDictionary<string, string> value, bool recursive = true)
    {
        AutoGen._AVDictionary* v = null;
        AVResult32 res = ffmpeg.av_opt_get_dict_val(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        using Collections.AVDictionary dict = new(v, false, false);
        value = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> kv in dict)
            value.Add(kv.Key, kv.Value);
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a dictionary of key-value pairs from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains a dictionary where keys and values are strings representing the option's key-value pairs, or an empty dictionary if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="AutoGen._AVDictionary"/> and converted into a .NET <see cref="Dictionary{String, String}"/>.
    /// This method is useful for options that are represented as key-value pairs in the underlying AV system.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out Dictionary<string, string> value, bool recursive = true)
    {
        AutoGen._AVDictionary* v = null;
        AVResult32 res = ffmpeg.av_opt_get_dict_val(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        using Collections.AVDictionary dict = new(v, false, false);
        value = [];
        foreach (KeyValuePair<string, string> kv in dict)
            value.Add(kv.Key, kv.Value);
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a collection of key-value pairs from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains an instance of <see cref="Collections.AVDictionary"/> representing the option's key-value pairs, or a new instance if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="AutoGen._AVDictionary"/> and wrapped in a .NET <see cref="Collections.AVDictionary"/>.
    /// This method is useful when you need to work directly with the AV dictionary object within the FFmpeg library.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out Collections.AVDictionary value, bool recursive = true)
    {
        AutoGen._AVDictionary* v = null;
        AVResult32 res = ffmpeg.av_opt_get_dict_val(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = new FFmpeg.Collections.AVDictionary(v, false, false);
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a lookup of key-value pairs from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains an <see cref="ILookup{String, String}"/> of key-value pairs representing the option's key-value pairs, or an empty lookup if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="AutoGen._AVDictionary"/> and converted into an <see cref="ILookup{String, String}"/>.
    /// This method is useful for options that may contain multiple values per key and need to be represented as a lookup collection.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out ILookup<string, string> value, bool recursive = true)
    {
        AutoGen._AVDictionary* v = null;
        AVResult32 res = ffmpeg.av_opt_get_dict_val(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        using AVMultiDictionary dict = new(v, false);
        value = dict.SelectMany(group => group.Select(Value => (group.Key, Value))).ToLookup(kv => kv.Key, kv => kv.Value);
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option as a multi-dictionary of key-value pairs from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains an instance of <see cref="Collections.AVMultiDictionary"/> representing the option's key-value pairs, or a new instance if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The option is retrieved as an <see cref="AutoGen._AVDictionary"/> and wrapped in a .NET <see cref="Collections.AVMultiDictionary"/>.
    /// This method is useful for options that may have multiple values for each key and need to be represented in a multi-dictionary format.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out Collections.AVMultiDictionary value, bool recursive = true)
    {
        AutoGen._AVDictionary* v = null;
        AVResult32 res = ffmpeg.av_opt_get_dict_val(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &v);
        value = new FFmpeg.Collections.AVMultiDictionary(v, false);
        return res;
    }

    /// <summary>
    /// Attempts to retrieve the width and height of an image option from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="width">When this method returns, contains the width of the image option in pixels, or 0 if the option is not found or if an error occurs.</param>
    /// <param name="height">When this method returns, contains the height of the image option in pixels, or 0 if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The width and height are retrieved as separate integers using the <see cref="ffmpeg.av_opt_get_image_size"/> function.
    /// This method is useful for options that specify image dimensions, such as video frame sizes or image resolutions.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out int width, out int height, bool recursive = true)
    {
        int w, h;
        AVResult32 res = ffmpeg.av_opt_get_image_size(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &w, &h);
        width = w;
        height = h;
        return res;
    }

    /// <summary>
    /// Attempts to retrieve the width and height of an image option as a tuple from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="size">When this method returns, contains a tuple with the width and height of the image option in pixels, or (0, 0) if the option is not found or if an error occurs.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>Returns an <see cref="AVResult32"/> indicating the result of the operation. Possible values include success or various error codes.</returns>
    /// <remarks>
    /// The width and height are retrieved as a tuple using the <see cref="ffmpeg.av_opt_get_image_size"/> function.
    /// This method is useful for quickly obtaining image dimensions as a single tuple value, which can simplify code that requires both width and height together.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out (int Width, int Height) size, bool recursive = true)
    {
        int w, h;
        AVResult32 res = ffmpeg.av_opt_get_image_size(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &w, &h);
        size.Width = w;
        size.Height = h;
        return res;
    }

    /// <summary>
    /// Attempts to retrieve an option and copy its binary data into a <see cref="Span{Byte}"/> from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the data of the option copied into the provided <see cref="Span{Byte}"/>. If the span is not large enough to hold the data, the method returns a negative size indicating the required size.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>
    /// Returns the size of the data successfully copied into the <see cref="Span{Byte}"/> if the operation was successful.
    /// If the provided span is too small to hold the data, returns a negative value indicating the required size of the span.
    /// If the option is not found, the data type is incorrect, or any other error occurs, returns <see cref="int.MinValue"/>.
    /// </returns>
    /// <remarks>
    /// The method retrieves the option data as binary using the <see cref="ffmpeg.av_opt_find2"/> and <see cref="ffmpeg.av_opt_get_binary"/> functions.
    /// The binary data is copied into the provided <see cref="Span{Byte}"/>. If the span is too small to accommodate the data, a negative value representing the required size is returned.
    /// In case of errors such as invalid option names, incorrect data types, or other failures, <see cref="int.MinValue"/> is returned.
    /// To ensure the data is copied correctly, make sure the span is sufficiently sized based on the expected data size.
    /// </remarks>
    public AVResult32 TryGetOption(string name, Span<byte> value, bool recursive = true)
    {
        void* ptr;
        AutoGen._AVOption* ptrOption = ffmpeg.av_opt_find2(Pointer, name, null, 0, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &ptr);
        if (ptrOption == null || ptr == null || (ptrOption->offset <= 0 && ptrOption->type != AutoGen._AVOptionType.AV_OPT_TYPE_CONST))
            return int.MinValue;
        void* dst = (byte*)ptr + ptrOption->offset;
        if (ptrOption->type == AutoGen._AVOptionType.AV_OPT_TYPE_BINARY)
        {
            byte* src = *(byte**)ptr;
            if (src == null)
                return 0;
            long size = *((int*)ptr + 1);
            if ((size * 2) + 1 > int.MaxValue)
                return int.MinValue;
            if (size > value.Length)
                return (int)-size;
            fixed (byte* b = value)
                Buffer.MemoryCopy(src, b, size, size);
            return (int)size;
        }
        return int.MinValue;
    }


    /// <summary>
    /// Attempts to retrieve an option and return its binary data as a <see cref="byte"/> array from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the binary data of the option as a <see cref="byte"/> array. If the option is not found or if an error occurs, the array will be empty.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation. 
    /// Possible values include:
    /// <list type="bullet">
    /// <item>
    /// <description>Zero (0) if the operation was successful.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.OptionNotFound"/> if the option could not be found or if the data type is incorrect.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the data size is invalid or if any other argument-related issue occurs.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The method retrieves the option data as binary using the <see cref="ffmpeg.av_opt_find2"/> function. 
    /// If the option contains binary data, it is copied into a new <see cref="byte"/> array. 
    /// If the option is not found or does not contain binary data, or if there is an issue with the size, appropriate error codes are returned:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="AVResult32.OptionNotFound"/> if the option is not found or the data type is not binary.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the binary data size is too large or other issues occur.</description>
    /// </item>
    /// </list>
    /// To ensure correct operation, handle errors based on the returned <see cref="AVResult32"/> code and validate the size of binary data if necessary.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out byte[] value, bool recursive = true)
    {
        value = [];
        void* ptr;
        AutoGen._AVOption* ptrOption = ffmpeg.av_opt_find2(Pointer, name, null, 0, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &ptr);
        if (ptrOption == null || ptr == null || (ptrOption->offset <= 0 && ptrOption->type != AutoGen._AVOptionType.AV_OPT_TYPE_CONST))
            return AVResult32.OptionNotFound;
        void* dst = (byte*)ptr + ptrOption->offset;
        if (ptrOption->type == AutoGen._AVOptionType.AV_OPT_TYPE_BINARY)
        {
            byte* src = *(byte**)ptr;
            if (src == null)
            {
                value = [];
                return 0;
            }
            long size = *((int*)ptr + 1);
            if ((size * 2) + 1 > int.MaxValue)
                return AVResult32.InvalidArgument;
            value = new byte[size];
            fixed (byte* b = value)
                Buffer.MemoryCopy(src, b, size, size);
            return 0;
        }
        return AVResult32.InvalidArgument;
    }

    /// <summary>
    /// Attempts to retrieve an option and return its binary data as a <see cref="Memory{Byte}"/> from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the binary data of the option as a <see cref="Memory{Byte}"/>. If the option is not found or if an error occurs, the memory will be empty.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation. 
    /// Possible values include:
    /// <list type="bullet">
    /// <item>
    /// <description>Zero (0) if the operation was successful and the binary data has been successfully copied into the <see cref="Memory{Byte}"/>.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.OptionNotFound"/> if the option could not be found or if the data type is incorrect.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the data size is invalid or if any other argument-related issue occurs.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The method retrieves the option data as binary using the <see cref="ffmpeg.av_opt_find2"/> function. 
    /// If the option contains binary data, it is copied into a new <see cref="byte"/> array, which is then wrapped in a <see cref="Memory{Byte}"/> object.
    /// If the option is not found, or the data type is not binary, or there are issues with the data size, appropriate error codes are returned:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="AVResult32.OptionNotFound"/> if the option is not found or if the data type is not binary.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the binary data size is too large or other issues occur.</description>
    /// </item>
    /// </list>
    /// To ensure correct operation, handle errors based on the returned <see cref="AVResult32"/> code and validate the size of binary data if necessary.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out Memory<byte> value, bool recursive = true)
    {
        value = Memory<byte>.Empty;
        void* ptr;
        AutoGen._AVOption* ptrOption = ffmpeg.av_opt_find2(Pointer, name, null, 0, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &ptr);
        if (ptrOption == null || ptr == null || (ptrOption->offset <= 0 && ptrOption->type != AutoGen._AVOptionType.AV_OPT_TYPE_CONST))
            return AVResult32.OptionNotFound;
        void* dst = (byte*)ptr + ptrOption->offset;
        if (ptrOption->type == AutoGen._AVOptionType.AV_OPT_TYPE_BINARY)
        {
            byte* src = *(byte**)ptr;
            if (src == null)
                return 0;

            long size = *((int*)ptr + 1);
            if ((size * 2) + 1 > int.MaxValue)
                return AVResult32.InvalidArgument;
            byte[] bytes = new byte[size];
            fixed (byte* b = bytes)
                Buffer.MemoryCopy(src, b, size, size);
            value = new Memory<byte>(bytes);
            return 0;
        }
        return AVResult32.InvalidArgument;
    }

    /// <summary>
    /// Attempts to retrieve an option and return its binary data as a <see cref="ReadOnlyMemory{Byte}"/> from the AV-option-enabled class.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the binary data of the option as a <see cref="ReadOnlyMemory{Byte}"/>. If the option is not found or if an error occurs, the memory will be empty.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation. 
    /// Possible values include:
    /// <list type="bullet">
    /// <item>
    /// <description>Zero (0) if the operation was successful and the binary data has been successfully copied into the <see cref="ReadOnlyMemory{Byte}"/>.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.OptionNotFound"/> if the option could not be found or if the data type is incorrect.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the data size is invalid or if any other argument-related issue occurs.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The method retrieves the option data as binary using the <see cref="ffmpeg.av_opt_find2"/> function. 
    /// If the option contains binary data, it is copied into a new <see cref="byte"/> array, which is then wrapped in a <see cref="ReadOnlyMemory{Byte}"/> object.
    /// If the option is not found, or the data type is not binary, or there are issues with the data size, appropriate error codes are returned:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="AVResult32.OptionNotFound"/> if the option is not found or if the data type is not binary.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the binary data size is too large or other issues occur.</description>
    /// </item>
    /// </list>
    /// To ensure correct operation, handle errors based on the returned <see cref="AVResult32"/> code and validate the size of binary data if necessary.
    /// </remarks>
    public AVResult32 TryGetOption(string name, out ReadOnlyMemory<byte> value, bool recursive = true)
    {
        value = Memory<byte>.Empty;
        void* ptr;
        AutoGen._AVOption* ptrOption = ffmpeg.av_opt_find2(Pointer, name, null, 0, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &ptr);
        if (ptrOption == null || ptr == null || (ptrOption->offset <= 0 && ptrOption->type != AutoGen._AVOptionType.AV_OPT_TYPE_CONST))
            return AVResult32.OptionNotFound;
        void* dst = (byte*)ptr + ptrOption->offset;
        if (ptrOption->type == AutoGen._AVOptionType.AV_OPT_TYPE_BINARY)
        {
            byte* src = *(byte**)ptr;
            if (src == null)
                return 0;

            long size = *((int*)ptr + 1);
            if ((size * 2) + 1 > int.MaxValue)
                return AVResult32.InvalidArgument;
            byte[] bytes = new byte[size];
            fixed (byte* b = bytes)
                Buffer.MemoryCopy(src, b, size, size);
            value = new ReadOnlyMemory<byte>(bytes);
            return 0;
        }
        return AVResult32.InvalidArgument;
    }

    /// <inheritdoc cref="TryGetOption(Option, out object?, bool)"/>
    /// <param name="name">The name of the option to retrieve.</param>
    public AVResult32 TryGetOption(string name, out object? value, bool recursive = true)
    {
        value = null;
        AutoGen._AVOption* ptr = ffmpeg.av_opt_find(Pointer, name, null, 0, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
        return ptr == null ? (AVResult32)AVResult32.InvalidArgument : TryGetOption(new Option(ptr), out value, recursive);
    }

    /// <summary>
    /// Attempts to retrieve an option using an <see cref="Option"/> object and return its value as a <see cref="string"/>.
    /// </summary>
    /// <param name="option">The <see cref="Option"/> object representing the option to retrieve.</param>
    /// <param name="value">When this method returns, contains the string representation of the option's value. If the option is of type binary, this method will call another overload to handle binary data. If the option is not found or if an error occurs, this will be null.</param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation. 
    /// Possible values include:
    /// <list type="bullet">
    /// <item>
    /// <description>Zero (0) if the operation was successful and the value has been successfully retrieved as a <see cref="string"/>.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the option is of type binary and the binary retrieval fails, or if any other argument-related issue occurs.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method handles options of type binary separately by calling the <see cref="TryGetOption(string, out ReadOnlyMemory{byte}, bool)"/> overload. 
    /// For other types, it retrieves the option's value as a UTF-8 string using the <see cref="ffmpeg.av_opt_get"/> function.
    /// If the option is not found or if there are issues with retrieving its value, appropriate error codes are returned:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the option cannot be found or other issues occur.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public AVResult32 TryGetOption(Option option, out string? value, bool recursive = true)
    {
        value = null;
        if (option.Type == OptionType.Binary)
            return TryGetOption(option.Name, out value, recursive);
        byte* b;
        AVResult32 res = ffmpeg.av_opt_get(Pointer, option.Name, ffmpeg.AV_OPT_ALLOW_NULL | (recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0), &b);
        if (res < 0) return res;
        value = b == null ? string.Empty : Marshal.PtrToStringUTF8((nint)b);
        ffmpeg.av_free(b);
        return res;
    }

    /// <inheritdoc cref="TryGetOption(string, out long, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out long value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out ulong, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out ulong value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out int, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out int value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out bool, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out bool value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out uint, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out uint value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out double, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out double value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out Rational, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out Rational value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out PixelFormat, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out PixelFormat value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out SampleFormat, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out SampleFormat value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out IDictionary{string, string}, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out IDictionary<string, string> value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out Dictionary{string, string}, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out Dictionary<string, string> value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out Collections.AVDictionary, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out Collections.AVDictionary value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out ILookup{string,string}, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out ILookup<string, string> value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out Collections.AVMultiDictionary, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out Collections.AVMultiDictionary value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out int,out int, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out int width, out int height, bool recursive = true) => TryGetOption(option.Name, out width, out height, recursive);

    /// <inheritdoc cref="TryGetOption(string, out ValueTuple{int, int}, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out (int Width, int Height) size, bool recursive = true) => TryGetOption(option.Name, out size, recursive);

    /// <inheritdoc cref="TryGetOption(string, out byte[], bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out byte[] value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out Memory{byte}, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out Memory<byte> value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, out ReadOnlyMemory{byte}, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, out ReadOnlyMemory<byte> value, bool recursive = true) => TryGetOption(option.Name, out value, recursive);

    /// <inheritdoc cref="TryGetOption(string, Span{byte}, bool)"/>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    public AVResult32 TryGetOption(Option option, Span<byte> value, bool recursive = true) => TryGetOption(option.Name, value, recursive);

    /// <summary>
    /// Attempts to retrieve the value of the specified <see cref="Option"/> and convert it to an <see cref="object"/> of the appropriate type.
    /// </summary>
    /// <param name="option">The <see cref="Option"/> object representing the option whose value you want to retrieve.</param>
    /// <param name="obj">
    /// When this method returns, contains the value of the option as an <see cref="object"/> of the appropriate type. 
    /// The type of the object depends on the option's type, as described in the remarks. 
    /// If the option is not found or if an error occurs, this will be <see langword="null"/>.
    /// </param>
    /// <param name="recursive">Specifies whether to search for the option recursively in child options.</param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation. 
    /// Possible values include:
    /// <list type="bullet">
    /// <item>
    /// <description><see langword="0"/> if the operation was successful and the value has been retrieved and converted to the appropriate type.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the option type is not supported or if there is an issue with the arguments.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method attempts to retrieve the value of the <see cref="Option"/> and convert it to a type corresponding to the option’s type:
    /// <list type="table">
    /// <listheader>
    /// <term>Option Type</term>
    /// <description>Returned Value Type</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="OptionType.Color"/>, <see cref="OptionType.Flags"/>, <see cref="OptionType.Int32"/></term>
    /// <description><see cref="int"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.Int64"/></term>
    /// <description><see cref="long"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.Constant"/>, <see cref="OptionType.Double"/>, <see cref="OptionType.Float"/></term>
    /// <description><see cref="double"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.String"/></term>
    /// <description><see cref="string"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.VideoRate"/>, <see cref="OptionType.Rational"/></term>
    /// <description><see cref="Rational"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.Binary"/></term>
    /// <description><see cref="byte[]"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.Dictionary"/></term>
    /// <description><see cref="Dictionary{string, string}"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.Duration"/>, <see cref="OptionType.UInt64"/></term>
    /// <description><see cref="ulong"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.ImageSize"/></term>
    /// <description>A tuple of <see cref="int"/> representing width and height.</description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.PixelFormat"/></term>
    /// <description><see cref="PixelFormat"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.SampleFormat"/></term>
    /// <description><see cref="SampleFormat"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.Bool"/></term>
    /// <description><see cref="bool"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.ChannelLayout"/></term>
    /// <description><see cref="Audio.ChannelLayout"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="OptionType.UInt32"/></term>
    /// <description><see cref="uint"/></description>
    /// </item>
    /// </list>
    /// <para>
    /// If the option type is not implemented or not supported, such as <see cref="OptionType.ChannelLayout"/>, the method may return <see cref="AVResult32.InvalidArgument"/>.
    /// </para>
    /// </remarks>
    public AVResult32 TryGetOption(Option option, out object? obj, bool recursive = true)
    {
        AVResult32 res;
        bool array = option.Type.HasFlag(OptionType.ArrayFlag);
        OptionType type = option.Type & ~OptionType.ArrayFlag;

        if (array && type != OptionType.String && type != OptionType.Binary)
        {
            obj = null;
            return AVResult32.InvalidArgument;
        }

        switch (type)
        {
            case OptionType.Color:
            case OptionType.Flags:
            case OptionType.Int32:
                res = TryGetOption(option, out int i, recursive);
                obj = i;
                return res;
            case OptionType.Int64:
                res = TryGetOption(option, out long l, recursive);
                obj = l;
                return res;
            case OptionType.Constant:
            case OptionType.Double:
            case OptionType.Float:
                res = TryGetOption(option, out double d, recursive);
                obj = d;
                return res;
            case OptionType.String:
                res = TryGetOption(option, out string? str, recursive);
                obj = str;
                return res;
            case OptionType.VideoRate:
            case OptionType.Rational:
                res = TryGetOption(option, out Rational q, recursive);
                obj = q;
                return res;
            case OptionType.Binary:
                res = TryGetOption(option, out byte[] b, recursive);
                obj = b;
                return res;
            case OptionType.Dictionary:
                res = TryGetOption(option, out Dictionary<string, string> dict, recursive);
                obj = dict;
                return res;
            case OptionType.Duration:
            case OptionType.UInt64:
                res = TryGetOption(option, out ulong u, recursive);
                obj = u;
                return res;
            case OptionType.ImageSize:
                res = TryGetOption(option, out (int Width, int Height) size, recursive);
                obj = size;
                return res;
            case OptionType.PixelFormat:
                res = TryGetOption(option, out PixelFormat pix_fmt, recursive);
                obj = pix_fmt;
                return res;
            case OptionType.SampleFormat:
                res = TryGetOption(option, out SampleFormat sample_fmt);
                obj = sample_fmt;
                return res;
            case OptionType.Bool:
                res = TryGetOption(option, out bool @bool, recursive);
                obj = @bool;
                return res;
            case OptionType.ChannelLayout:
                res = TryGetOption(option, out Audio.ChannelLayout channelLayout, recursive);
                obj = channelLayout;
                return res;
            case OptionType.UInt32:
                res = TryGetOption(option, out uint @uint, recursive);
                obj = @uint;
                return res;
        }

        obj = null;
        return AVResult32.InvalidArgument;
    }




    /// <summary>
    /// Sets the value of an option specified by name to the given <see cref="Audio.ChannelLayout"/>.
    /// </summary>
    /// <param name="name">The name of the option to set.</param>
    /// <param name="layout">The <see cref="Audio.ChannelLayout"/> value to assign to the option.</param>
    /// <param name="recursive">
    /// Specifies whether the search for the option should be recursive in child options.
    /// If <see langword="true"/>, the method will search child options recursively.
    /// </param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation. 
    /// Possible values include:
    /// <list type="bullet">
    /// <item>
    /// <description>Zero (0) if the operation was successful.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the option type or the arguments are invalid.</description>
    /// </item>
    /// </list>
    /// </returns>
    public AVResult32 SetOption(string name, Audio.ChannelLayout layout, bool recursive = true)
    {
        AutoGen._AVChannelLayout l = layout.Layout;
        return ffmpeg.av_opt_set_chlayout(Pointer, name, &l, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0);
    }

    /// <summary>
    /// Sets the value of the specified <see cref="Option"/> to the given <see cref="Audio.ChannelLayout"/>.
    /// </summary>
    /// <param name="option">The <see cref="Option"/> to set.</param>
    /// <param name="layout">The <see cref="Audio.ChannelLayout"/> value to assign to the option.</param>
    /// <param name="recursive">
    /// Specifies whether the search for the option should be recursive in child options.
    /// If <see langword="true"/>, the method will search child options recursively.
    /// </param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    public AVResult32 SetOption(Option option, Audio.ChannelLayout layout, bool recursive = true) => SetOption(option.Name, layout, recursive);

    /// <summary>
    /// Attempts to retrieve the value of the option specified by name and convert it to an <see cref="Audio.ChannelLayout"/>.
    /// </summary>
    /// <param name="name">The name of the option to retrieve.</param>
    /// <param name="layout">
    /// When this method returns, contains the <see cref="Audio.ChannelLayout"/> value of the specified option.
    /// If the option is not found or if an error occurs, this will be set to the default <see cref="Audio.ChannelLayout"/> value.
    /// </param>
    /// <param name="recursive">
    /// Specifies whether the search for the option should be recursive in child options.
    /// If <see langword="true"/>, the method will search child options recursively.
    /// </param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation.
    /// Possible values include:
    /// <list type="bullet">
    /// <item>
    /// <description>Zero (0) if the operation was successful and the value has been successfully retrieved and converted.</description>
    /// </item>
    /// <item>
    /// <description><see cref="AVResult32.InvalidArgument"/> if the option type is not supported or if there is an issue with the arguments.</description>
    /// </item>
    /// </list>
    /// </returns>
    public AVResult32 TryGetOption(string name, out Audio.ChannelLayout layout, bool recursive = true)
    {
        AutoGen._AVChannelLayout l;
        AVResult32 res = ffmpeg.av_opt_get_chlayout(Pointer, name, recursive ? ffmpeg.AV_OPT_SEARCH_CHILDREN : 0, &l);
        layout = new Audio.ChannelLayout(l);
        return res;
    }

    /// <summary>
    /// Attempts to retrieve the value of the specified <see cref="Option"/> and convert it to an <see cref="Audio.ChannelLayout"/>.
    /// </summary>
    /// <param name="option">The <see cref="Option"/> whose value you want to retrieve.</param>
    /// <param name="layout">
    /// When this method returns, contains the <see cref="Audio.ChannelLayout"/> value of the specified option.
    /// If the option is not found or if an error occurs, this will be set to the default <see cref="Audio.ChannelLayout"/> value.
    /// </param>
    /// <param name="recursive">
    /// Specifies whether the search for the option should be recursive in child options.
    /// If <see langword="true"/>, the method will search child options recursively.
    /// </param>
    /// <returns>
    /// Returns an <see cref="AVResult32"/> indicating the result of the operation.
    /// </returns>
    public AVResult32 TryGetOption(Option option, out Audio.ChannelLayout layout, bool recursive = true) => TryGetOption(option.Name, out layout, recursive);


}
