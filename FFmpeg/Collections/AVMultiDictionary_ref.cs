using FFmpeg.Exceptions;
using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Collections;

/// <summary>
/// Represents a multi-dictionary that allows multiple values for a single key.
/// </summary>
public unsafe struct AVMultiDictionary_ref : ILookup<string, string>, Utils.IReference<AVMultiDictionary>
{


    /// <summary>
    /// Pointer to the underlying dictionary structure.
    /// </summary>
    internal readonly AutoGen._AVDictionary** dictionary = null;

    /// <summary>
    /// Gets or sets the flags used for dictionary operations.
    /// </summary>
    private AVDictionaryFlags Flags { get; set; } = AVDictionaryFlags.MultiKey;


    /// <summary>
    /// Initializes a new instance of the <see cref="AVMultiDictionary_ref"/> class with an existing dictionary.
    /// </summary>
    /// <param name="dictionary">Pointer to an existing dictionary.</param>
    /// <param name="ignoreCase">If set to <c>true</c>, the dictionary will ignore case when comparing keys.</param>
    internal AVMultiDictionary_ref(AutoGen._AVDictionary** dictionary, bool ignoreCase = true)
    {
        this.dictionary = dictionary;
        if (!ignoreCase)
            Flags |= AVDictionaryFlags.MatchCase;
    }

    /// <summary>
    /// Gets the values associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose values to get.</param>
    /// <returns>An enumerable collection of values associated with the specified key.</returns>
    public IEnumerable<string> this[string key] => new AVMultiDictionaryKeyIterator(this, key);

    /// <summary>
    /// Gets the number of key/value pairs contained in the dictionary.
    /// </summary>
    public int Count => AutoGen.ffmpeg.av_dict_count(*dictionary);

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
    public bool Contains(string key)
    {
        if (dictionary == null) return false;
        AutoGen._AVDictionaryEntry* entry = AutoGen.ffmpeg.av_dict_get(*dictionary, key, null, (int)Flags);
        return entry != null;
    }

    /// <summary>
    /// Removes all values associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose values to remove.</param>
    /// <returns><c>true</c> if any values were removed; otherwise, <c>false</c>.</returns>
    public bool RemoveAll(string key)
    {
        bool ret = false;
        while (RemoveFirst(key)) ret = true;
        return ret;
    }

    /// <summary>
    /// Removes the first value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose first value to remove.</param>
    /// <returns><c>true</c> if a value was removed; otherwise, <c>false</c>.</returns>
    public bool RemoveFirst(string key)
    {
        bool b = Contains(key);
        if (!b) return false;
        return AutoGen.ffmpeg.av_dict_set(dictionary, key, null, (int)(Flags & ~AVDictionaryFlags.MultiKey)) >= 0;
    }

    /// <summary>
    /// Adds a key/value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void Add(string key, string value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        FFmpegException.ThrowIfError(AutoGen.ffmpeg.av_dict_set(dictionary, key, value, (int)Flags));
    }

    /// <summary>
    /// Clears all key/value pairs from the dictionary.
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

    private class AVMultiDictionaryIterator(AVMultiDictionary_ref dict) : IEnumerator<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>
    {
        private AutoGen._AVDictionaryEntry* prev = null;
        private readonly AVMultiDictionary_ref dict = dict;
        private int _count = dict.Count;
        public KeyValuePair<string, string> Current => prev == null
                    ? throw new InvalidOperationException()
                    : new KeyValuePair<string, string>(Marshal.PtrToStringUTF8((IntPtr)prev->key), Marshal.PtrToStringUTF8((IntPtr)prev->value));

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext()
        {
            if (_count != dict.Count)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
            prev = AutoGen.ffmpeg.av_dict_iterate(*dict.dictionary, prev);
            return prev != null;
        }

        public void Reset()
        {
            prev = null;
            _count = dict.Count;
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    private class AVMultiDictionaryKeyIterator(AVMultiDictionary_ref dict, string key) : IEnumerator<string>, IEnumerable<string>
    {
        private AutoGen._AVDictionaryEntry* prev = null;
        private readonly string key = key;
        private readonly AVMultiDictionary_ref dict = dict;
        private int _count = dict.Count;
        private bool Check() => _count == dict.Count;
        public string Current => prev == null ? throw new InvalidOperationException() : Marshal.PtrToStringUTF8((IntPtr)prev->value);

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext()
        {
            if (!Check())
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
            prev = AutoGen.ffmpeg.av_dict_get(*dict.dictionary, key, prev, (int)dict.Flags);
            return prev != null;
        }

        public void Reset()
        {
            prev = null;
            _count = dict.Count;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    /// <summary>
    /// Converts the <see cref="AVMultiDictionary_ref"/> into an <see cref="AVDictionary_ref"/>
    /// </summary>
    /// <returns>The <see cref="AVDictionary_ref"/> without the <see cref="AVDictionaryFlags.IgnoreSuffix"/> flag set.</returns>
    public AVDictionary_ref AsAVDictionary() => new(dictionary, ignoreCase: !Flags.HasFlag(AVDictionaryFlags.MatchCase));

    public AVMultiDictionary? GetReferencedObject()
    {
        if (*dictionary == null) return null;
        AutoGen._AVDictionary* copy;
        FFmpeg.Exceptions.FFmpegException.ThrowIfError(ffmpeg.av_dict_copy(&copy, *dictionary, (int)Flags)); // OutOfMemoryException
        return new AVMultiDictionary(copy, !Flags.HasFlag(AVDictionaryFlags.MatchCase));
    }
    public void SetReferencedObject(AVMultiDictionary? obj)
    {
        if (obj == null) ffmpeg.av_dict_free(dictionary);
        else
        {
            FFmpeg.Exceptions.FFmpegException.ThrowIfError(ffmpeg.av_dict_copy(dictionary, obj.dictionary, (int)obj.Flags)); // OutOfMemoryException            
            Flags = obj.Flags;
        }
    }

    public override string ToString() => string.Join(':', this.Select(kv => $"{kv.Key}={string.Join(',', kv)}"));

}