using FFmpeg.Utils;
using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Collections;
/// <summary>
/// Represents a dictionary that wraps around the FFmpeg AVDictionary structure.
/// </summary>
public sealed unsafe class AVDictionary : IDictionary<string, string>, IDisposable
{
    private int _check = 0;


    /// <summary>
    /// Pointer to the underlying FFmpeg AVDictionary structure.
    /// </summary>
    internal AutoGen._AVDictionary* dictionary = null;

    /// <summary>
    /// Gets or sets the flags used for dictionary operations.
    /// </summary>
    internal AVDictionaryFlags Flags { get; set; } = AVDictionaryFlags.None;

    internal AVDictionary(AutoGen._AVDictionary* dictionary, bool ignoreCase = true, bool ignoreSuffix = false)
    {
        if (!ignoreCase)
            Flags |= AVDictionaryFlags.MatchCase;
        if (ignoreSuffix)
            Flags |= AVDictionaryFlags.IgnoreSuffix;
        this.dictionary = dictionary;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AVDictionary_ref"/> class with an existing FFmpeg AVDictionary.
    /// </summary>
    /// <param name="ignoreCase">If set to <c>true</c>, the dictionary will ignore case when comparing keys.</param>
    /// <param name="ignoreSuffix">If set to <c>true</c>, the dictionary will ignore suffixes when comparing keys.</param>
    public AVDictionary(bool ignoreCase = true, bool ignoreSuffix = false)
    {
        if (!ignoreCase)
            Flags |= AVDictionaryFlags.MatchCase;
        if (ignoreSuffix)
            Flags |= AVDictionaryFlags.IgnoreSuffix;
    }

    public AVDictionary(IEnumerable<KeyValuePair<string, string>> dict, bool ignoreCase = true, bool ignoreSuffix = false) : this(ignoreCase, ignoreSuffix)
    {
        foreach (KeyValuePair<string, string> pair in dict)
            Add(pair);
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    public string this[string key]
    {
        get => TryGetValue(key, out string? value) ? value : throw new KeyNotFoundException();
        set
        {
            AutoGen._AVDictionary* dictionary = this.dictionary;
            AVResult32 res = (AVResult32)AutoGen.ffmpeg.av_dict_set(&dictionary, key, value, (int)Flags);
            this.dictionary = dictionary;
            res.ThrowIfError();
            unchecked
            { _check++; }
        }
    }

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<string> Keys => this.Select(kv => kv.Key).ToList();

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public ICollection<string> Values => this.Select(kv => kv.Value).ToList();

    /// <summary>
    /// Gets the number of key-value pairs contained in the dictionary.
    /// </summary>
    public int Count => AutoGen.ffmpeg.av_dict_count(dictionary);

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
    /// <exception cref="ArgumentException">Thrown when an element with the same key already exists in the dictionary.</exception>
    public void Add(string key, string value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (ContainsKey(key))
            throw new ArgumentException("An element with the same key already exists in the Dictionary<TKey,TValue>.");
        this[key] = value;
    }

    /// <summary>
    /// Appends the specified value to the existing value under key. If the key doesn't exist. The key is added to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value to append to the existing value.</param>
    /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>    
    public void AppendText(string key, string value)
    {
        AutoGen._AVDictionary* dictionary = this.dictionary;
        AVResult32 res = AutoGen.ffmpeg.av_dict_set(&dictionary, key, value, (int)(Flags | AVDictionaryFlags.Append));
        this.dictionary = dictionary;
        res.ThrowIfError();
        unchecked
        { _check++; }
    }

    /// <summary>
    /// Adds the specified key-value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to add.</param>
    public void Add(KeyValuePair<string, string> item) => Add(item.Key, item.Value);

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    public void Clear()
    {
        _ = Keys;
        foreach (KeyValuePair<string, string> kv in this.Reverse())
            _ = Remove(kv.Key);
    }

    /// <summary>
    /// Determines whether the dictionary contains a specific key-value pair.
    /// </summary>
    /// <param name="item">The key-value pair to locate in the dictionary.</param>
    /// <returns><c>true</c> if the key-value pair is found; otherwise, <c>false</c>.</returns>
    public bool Contains(KeyValuePair<string, string> item) => TryGetValue(item.Key, out string? value) && item.Value.Equals(value);

    /// <summary>
    /// Determines whether the dictionary contains a specific key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string key) => TryGetValue(key, out _);

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        foreach (KeyValuePair<string, string> kv in this)
            array[arrayIndex++] = kv;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => new AVDictionaryIterator(this);

    /// <summary>
    /// Removes the value with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns><c>true</c> if the element is successfully found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(string key)
    {
        bool b = ContainsKey(key);
        if (!b)
            return false;
        this[key] = null!;
        return true;
    }

    /// <summary>
    /// Removes the specified key-value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to remove.</param>
    /// <returns><c>true</c> if the key-value pair is successfully found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(KeyValuePair<string, string> item)
    {
        bool b = TryGetValue(item.Key, out string? value);
        if (b && value.Equals(item.Value))
        {
            this[item.Key] = null!;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(string key, out string value)
    {
        AutoGen._AVDictionaryEntry* entry = AutoGen.ffmpeg.av_dict_get(dictionary, key, null, (int)Flags);
        if (entry != null)
        {
            value = Marshal.PtrToStringUTF8((IntPtr)entry->value)!;
            return true;
        }
        else
        {
            value = string.Empty;
            return false;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    private class AVDictionaryIterator(AVDictionary dict) : IEnumerator<KeyValuePair<string, string>>
    {
        private AutoGen._AVDictionaryEntry* prev = null;
        private readonly AVDictionary dict = dict;
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
    }

    public void Dispose()
    {
        AutoGen._AVDictionary* dict = dictionary;
        ffmpeg.av_dict_free(&dict);
        dictionary = dict;
        GC.SuppressFinalize(this);
    }

    ~AVDictionary() => Dispose();

    public override string ToString() => string.Join(':', this.Select(kv => $"{kv.Key}={kv.Value}"));
}