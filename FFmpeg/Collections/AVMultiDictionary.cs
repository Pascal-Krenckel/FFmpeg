using FFmpeg.Unsafe;
using FFmpeg.Utils;
using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Collections;
/// <summary>
/// Represents a managed wrapper around FFmpeg's <c>AVDictionary</c> that
/// supports multiple values for the same key.
/// </summary>
/// <remarks>
/// Unlike <see cref="AVDictionary"/>, this class allows duplicate keys by
/// storing multiple entries with the same key.
/// </remarks>
public sealed unsafe class AVMultiDictionary : ILookup<string, string>, IDisposable, IAVPointer<AutoGen._AVDictionary>
{
    /// <summary>
    /// Tracks modifications to detect changes during enumeration.
    /// </summary>
    private int _check = 0;

    /// <summary>
    /// Pointer to the underlying dictionary structure.
    /// </summary>
    internal AutoGen._AVDictionary* dictionary = null;
    AutoGen._AVDictionary* IAVPointer<AutoGen._AVDictionary>.Pointer => dictionary;

    /// <summary>
    /// Gets or sets the flags used for dictionary operations.
    /// </summary>
    internal AVDictionaryFlags Flags { get; set; } = AVDictionaryFlags.MultiKey;


    /// <summary>
    /// Initializes a new empty <see cref="AVMultiDictionary"/>.
    /// </summary>
    /// <param name="ignoreCase">If set to <see langword="true"/>, the dictionary will ignore case when comparing keys.</param>
    public AVMultiDictionary(bool ignoreCase = true)
    {
        if (!ignoreCase)
            Flags |= AVDictionaryFlags.MatchCase;
    }

    /// <summary>
    /// Initializes a new <see cref="AVMultiDictionary"/> containing the values
    /// from the specified lookup.
    /// </summary>
    public AVMultiDictionary(ILookup<string, string> values, bool ignoreCase = false) : this(ignoreCase)
    {
        foreach (IGrouping<string, string>? group in values)
        {
            foreach (string value in group)
                Add(group.Key, value);
        }
    }

    /// <summary>
    /// Initializes a new <see cref="AVMultiDictionary"/> containing the specified
    /// key/value pairs.
    /// </summary>
    public AVMultiDictionary(IDictionary<string, string> values, bool ignoreCase = false) : this(ignoreCase)
    {
        foreach (KeyValuePair<string, string> kv in values)
            Add(kv.Key, kv.Value);
    }

    internal AVMultiDictionary(AutoGen._AVDictionary* dictionary, bool ignoreCase = false) : this(ignoreCase) => this.dictionary = dictionary;


    /// <summary>
    /// Gets an enumerable sequence containing all values associated with the
    /// specified key.
    /// </summary>
    /// <param name="key">The key whose values to get.</param>
    /// <returns>An enumerable collection of values associated with the specified key.</returns>
    public IEnumerable<string> this[string key] => new AVMultiDictionaryKeyIterator(this, key);

    /// <summary>
    /// Gets the total number of key/value pairs contained in the dictionary.
    /// </summary>
    /// <remarks>
    /// Multiple entries with the same key are counted individually.
    /// </remarks>
    public int Count => AutoGen.ffmpeg.av_dict_count(dictionary);

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
    public bool Contains(string key)
    {
        if (dictionary == null)
            return false;
        AutoGen._AVDictionaryEntry* entry = AutoGen.ffmpeg.av_dict_get(dictionary, key, null, (int)Flags);
        return entry != null;
    }

    /// <summary>
    /// Removes all values associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose values to remove.</param>
    /// <returns><see langword="true"/> if any values were removed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// If multiple values exist for the specified key, only the first matching
    /// entry is removed.
    /// </remarks>
    public bool RemoveAll(string key)
    {
        bool ret = false;
        while (RemoveFirst(key))
            ret = true;
        return ret;
    }

    /// <summary>
    /// Removes the first value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose first value to remove.</param>
    /// <returns><see langword="true"/> if a value was removed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// If multiple values exist for the specified key, only the first matching
    /// entry is removed.
    /// </remarks>
    public bool RemoveFirst(string key)
    {
        bool b = Contains(key);
        if (!b)
            return false;
        AutoGen._AVDictionary* dictionary = this.dictionary;
        AVResult32 res = AutoGen.ffmpeg.av_dict_set(&dictionary, key, null, (int)(Flags & ~AVDictionaryFlags.MultiKey));
        if (res.IsError)
            return false;
        unchecked
        { _check++; }
        this.dictionary = dictionary;
        return true;
    }

    /// <summary>
    /// Adds a key/value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <remarks>
    /// This method does not replace existing values. If the key already exists,
    /// an additional value is added.
    /// </remarks>
    public void Add(string key, string value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        AutoGen._AVDictionary* dictionary = this.dictionary;
        AVResult32 res = AutoGen.ffmpeg.av_dict_set(&dictionary, key, value, (int)Flags);
        this.dictionary = dictionary;
        res.ThrowIfError();
        unchecked
        { _check++; }
    }

    /// <summary>
    /// Removes all key/value pairs from the dictionary.
    /// </summary>
    public void Clear()
    {
        List<string> keys = this.Select(g => g.Key).ToList();
        foreach (string? k in keys)
            _ = RemoveAll(k);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
    public IEnumerator<IGrouping<string, string>> GetEnumerator() =>
        new AVMultiDictionaryIterator(this)
        .GroupBy(kv => kv.Key, kv => kv.Value, GetComparer())
        .GetEnumerator();

    /// <summary>
    /// Gets the comparer used for comparing keys.
    /// </summary>
    /// <returns>An equality comparer for the dictionary keys.</returns>
    private IEqualityComparer<string> GetComparer() =>
        Flags.HasFlag(AVDictionaryFlags.MatchCase) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class AVMultiDictionaryIterator(AVMultiDictionary dict) : IEnumerator<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>
    {
        private AutoGen._AVDictionaryEntry* prev = null;
        private readonly AVMultiDictionary dict = dict;
        private int _version = dict._check;
        private int _count = dict.Count;
        private bool Check() => _version == dict._check && _count == dict.Count;
        public KeyValuePair<string, string> Current => prev == null
                    ? throw new InvalidOperationException()
                    : new KeyValuePair<string, string>(Marshal.PtrToStringUTF8((IntPtr)prev->key), Marshal.PtrToStringUTF8((IntPtr)prev->value));

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext()
        {
            if (!Check())
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
            prev = AutoGen.ffmpeg.av_dict_iterate(dict.dictionary, prev);
            return prev != null;
        }

        public void Reset()
        {
            prev = null;
            _version = dict._check;
            _count = dict.Count;
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    private class AVMultiDictionaryKeyIterator(AVMultiDictionary dict, string key) : IEnumerator<string>, IEnumerable<string>
    {
        private AutoGen._AVDictionaryEntry* prev = null;
        private readonly string key = key;
        private readonly AVMultiDictionary dict = dict;
        private int _version = dict._check;
        private int _count = dict.Count;
        private bool Check() => _version == dict._check && _count == dict.Count;
        public string Current => prev == null ? throw new InvalidOperationException() : Marshal.PtrToStringUTF8((IntPtr)prev->value);

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext()
        {
            if (!Check())
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
            prev = AutoGen.ffmpeg.av_dict_get(dict.dictionary, key, prev, (int)dict.Flags);
            return prev != null;
        }

        public void Reset()
        {
            prev = null;
            _version = dict._check;
            _count = dict.Count;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }


    /// <summary>
    /// Releases the unmanaged FFmpeg dictionary associated with this instance.
    /// </summary>
    public void Dispose()
    {
        AutoGen._AVDictionary* dictionary = this.dictionary;
        AutoGen.ffmpeg.av_dict_free(&dictionary);
        this.dictionary = dictionary;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    ~AVMultiDictionary() => Dispose();

    /// <summary>
    /// Returns a string representation of the dictionary.
    /// </summary>
    /// <returns>
    /// A colon-separated list of key/value groups in the form
    /// <c>key=value1,value2:key2=value3</c>.
    /// </returns>
    public override string ToString() => string.Join(':', this.Select(kv => $"{kv.Key}={string.Join(',', kv)}"));

}