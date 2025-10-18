using FFmpeg.Exceptions;
using FFmpeg.Utils;
using System.Collections;
using System.Runtime.InteropServices;

namespace FFmpeg.Collections;
/// <summary>
/// Represents a dictionary that wraps around the FFmpeg AVDictionary structure, allowing for 
/// key-value pair management with additional features such as case-insensitive comparisons.
/// </summary>
public unsafe struct AVDictionary_ref : IDictionary<string, string>, Utils.IReference<AVDictionary>
{
    /// <summary>
    /// Gets the pointer to the underlying AVDictionary structure.
    /// </summary>
    internal AutoGen._AVDictionary** Pointer { get; } = null;

    /// <summary>
    /// Gets the flags used for dictionary operations, which can include matching case or ignoring suffixes.
    /// </summary>
    private AVDictionaryFlags Flags { get; set; } = AVDictionaryFlags.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVDictionary_ref"/> class using an existing FFmpeg AVDictionary structure.
    /// </summary>
    /// <param name="dictionary">A pointer to an existing FFmpeg AVDictionary structure.</param>
    /// <param name="ignoreCase">If set to <see langword="true"/>, the dictionary will ignore case when comparing keys.</param>
    /// <param name="ignoreSuffix">If set to <see langword="true"/>, the dictionary will ignore suffixes when comparing keys.</param>
    internal AVDictionary_ref(AutoGen._AVDictionary** dictionary, bool ignoreCase = true, bool ignoreSuffix = false)
    {
        Pointer = dictionary;
        if (!ignoreCase)
            Flags |= AVDictionaryFlags.MatchCase;
        if (ignoreSuffix)
            Flags |= AVDictionaryFlags.IgnoreSuffix;
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the dictionary.</exception>
    public string this[string key]
    {
        readonly get => TryGetValue(key, out string? value) ? value : throw new KeyNotFoundException();
        set => FFmpegException.ThrowIfError(AutoGen.ffmpeg.av_dict_set(Pointer, key, value, (int)Flags));
    }

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public readonly ICollection<string> Keys => this.Select(kv => kv.Key).ToList();

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public readonly ICollection<string> Values => this.Select(kv => kv.Value).ToList();

    /// <summary>
    /// Gets the number of key-value pairs contained in the dictionary.
    /// </summary>
    public readonly int Count => AutoGen.ffmpeg.av_dict_count(*Pointer);

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only. This implementation always returns <see langword="false"/>.
    /// </summary>
    public readonly bool IsReadOnly => false;

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the key is <see langword="null"/>.</exception>
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
    /// Appends the specified value to the existing value under the specified key. If the key doesn't exist, it is added.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value to append to the existing value.</param>
    /// <exception cref="ArgumentNullException">Thrown when the key is <see langword="null"/>.</exception>
    public void AppendText(string key, string value) => FFmpegException.ThrowIfError(AutoGen.ffmpeg.av_dict_set(Pointer, key, value, (int)(Flags | AVDictionaryFlags.Append)));

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
    /// <returns><see langword="true"/> if the key-value pair is found; otherwise, <see langword="false"/>.</returns>
    public readonly bool Contains(KeyValuePair<string, string> item) => TryGetValue(item.Key, out string? value) && item.Value.Equals(value);

    /// <summary>
    /// Determines whether the dictionary contains a specific key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
    public readonly bool ContainsKey(string key) => TryGetValue(key, out _);

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public readonly void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        foreach (KeyValuePair<string, string> kv in this)
            array[arrayIndex++] = kv;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    public readonly IEnumerator<KeyValuePair<string, string>> GetEnumerator() => new AVDictionaryIterator(this);

    /// <summary>
    /// Removes the value with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the key-value pair is successfully found and removed; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the value type.</param>
    /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
    public readonly bool TryGetValue(string key, out string value)
    {
        AutoGen._AVDictionaryEntry* entry = AutoGen.ffmpeg.av_dict_get(*Pointer, key, null, (int)Flags);
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
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns the dictionary as a multi dictionary. <see cref="AVDictionaryFlags.IgnoreSuffix"/> is not supported for <see cref="AVMultiDictionary_ref"/>.
    /// </summary>
    /// <returns>An <see cref="AVMultiDictionary_ref"/> object representing the dictionary.</returns>
    public readonly AVMultiDictionary_ref AsMultiDictionary() => new(Pointer, !Flags.HasFlag(AVDictionaryFlags.MatchCase));

    /// <summary>
    /// Returns a copy of the referenced AVDictionary object, not a direct reference, since AVDictionary is not reference-counted.
    /// </summary>
    /// <returns>A copy of the referenced <see cref="AVDictionary"/> object, or <see langword="null"/> if the pointer is <see langword="null"/>.</returns>
    public readonly AVDictionary? GetReferencedObject()
    {
        if (*Pointer == null)
            return null;
        AutoGen._AVDictionary* copy;
        FFmpeg.Exceptions.FFmpegException.ThrowIfError(AutoGen.ffmpeg.av_dict_copy(&copy, *Pointer, (int)Flags)); // May throw OutOfMemoryException if allocation fails.
        return new AVDictionary(copy, !Flags.HasFlag(AVDictionaryFlags.MatchCase), Flags.HasFlag(AVDictionaryFlags.IgnoreSuffix));
    }

    /// <summary>
    /// Sets the referenced object to a copy of the provided AVDictionary. If <see langword="null"/>, the current dictionary is freed.
    /// </summary>
    /// <param name="dict">The dictionary to set as the referenced object, or <see langword="null"/> to free the current dictionary.</param>
    public void SetReferencedObject(AVDictionary? dict)
    {
        if (dict == null)
        {
            Clear();
        }
        else
        {
            FFmpeg.Exceptions.FFmpegException.ThrowIfError(AutoGen.ffmpeg.av_dict_copy(Pointer, dict.dictionary, (int)dict.Flags)); // May throw OutOfMemoryException if allocation fails.
            dict.Flags = Flags;
        }
    }

    public void Init(IDictionary<string, string> metadata)
    {
        Clear();
        if (metadata is AVDictionary avDict)
            ((AVResult32)ffmpeg.av_dict_copy(Pointer, avDict.dictionary, (int)Flags)).ThrowIfError();
        else if (metadata is AVDictionary_ref avDictRef)
            ((AVResult32)ffmpeg.av_dict_copy(Pointer, *avDictRef.Pointer, (int)Flags)).ThrowIfError();
        else
        {
            foreach (KeyValuePair<string, string> kv in metadata)
                this[kv.Key] = kv.Value;
        }
    }
    public override string ToString() => string.Join(':', this.Select(kv => $"{kv.Key}={kv.Value}"));

    // ToDo: comments, Extract Interface
    public void CopyFrom(IDictionary<string, string> dic)
    {
        foreach (KeyValuePair<string, string> kv in dic)
            this[kv.Key] = kv.Value;
    }

    private class AVDictionaryIterator(AVDictionary_ref dict) : IEnumerator<KeyValuePair<string, string>>
    {
        private AutoGen._AVDictionaryEntry* prev = null;
        private readonly AVDictionary_ref dict = dict;
        private int _count = dict.Count;

        /// <summary>
        /// Gets the current key-value pair.
        /// </summary>
        public KeyValuePair<string, string> Current => prev == null
            ? throw new InvalidOperationException()
            : new KeyValuePair<string, string>(Marshal.PtrToStringUTF8((IntPtr)prev->key), Marshal.PtrToStringUTF8((IntPtr)prev->value));

        object IEnumerator.Current => Current;

        public void Dispose() { }

        /// <summary>
        /// Advances the enumerator to the next element in the dictionary.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator successfully advances; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the dictionary is modified during enumeration.</exception>
        public bool MoveNext()
        {
            if (_count != dict.Count)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
            prev = AutoGen.ffmpeg.av_dict_iterate(*dict.Pointer, prev);
            return prev != null;
        }

        public void Reset()
        {
            prev = null;
            _count = dict.Count;
        }

    }
}




