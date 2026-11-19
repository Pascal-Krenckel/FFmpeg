using FFmpeg.AutoGen;
using FFmpeg.Unsafe;
using FFmpeg.Utils;
using System.Collections;
using System.Globalization;
using System.Text;

namespace FFmpeg.Codecs;

/// <summary>
/// Typed settings for an encoder, e.g. <see cref="Video.X264"/> or <see cref="Audio.Opus"/>.
/// </summary>
/// <remarks>
/// <para>
/// Every setting is optional: <see langword="null"/> (or 0 / <c>None</c> for sizes and formats) leaves FFmpeg's or
/// the encoder's default. The settings are translated into the same options the <c>ffmpeg</c> command line tool
/// uses; <see cref="GetOptions"/> lists them and <see cref="ToString"/> prints them as command line arguments.
/// </para>
/// <para>
/// Use <see cref="CreateCodecContext"/> to get an encoder context to open, or pass <see cref="Configure"/> as the
/// configure callback of e.g. <see cref="Transcoder.TranscodeStream(int, Codec, string?, Action{CodecContext}?, IDictionary{string, string}?)"/>
/// (or use <see cref="Transcoder.TranscodeStream(int, EncoderSettings, string?)"/>), where the properties that are not
/// set are taken from the frames.
/// </para>
/// </remarks>
public abstract class EncoderSettings
{
    /// <summary>
    /// Initializes the settings for <paramref name="codec"/>.
    /// </summary>
    /// <param name="codec">The encoder.</param>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is not an encoder.</exception>
    protected EncoderSettings(Codec codec)
    {
        if (!codec.IsEncoder)
            throw new ArgumentException($"{codec.Name} is not an encoder.", nameof(codec));
        Codec = codec;
    }

    /// <summary>
    /// Finds an encoder by name, e.g. <c>"libx264"</c>.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <returns>The encoder.</returns>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the encoder.</exception>
    protected static Codec FindEncoder(string name)
        => Codec.FindEncoder(name) ?? throw new NotSupportedException($"The encoder {name} is not available in the loaded FFmpeg libraries.");

    /// <summary>
    /// Gets the encoder.
    /// </summary>
    public Codec Codec { get; }

    /// <summary>
    /// Gets or sets the number of threads FFmpeg may use (option <c>threads</c>); 0 selects automatically.
    /// </summary>
    public int? ThreadCount { get; set; }

    /// <summary>
    /// Gets options that are applied after the typed settings, as with <c>-name value</c> on the <c>ffmpeg</c>
    /// command line: options of the codec context and private options of the encoder.
    /// </summary>
    /// <remarks>They are not changed by <see cref="Configure"/>; an option the context does not know throws.</remarks>
    public Dictionary<string, string> AdditionalParameters { get; } = [];

    /// <summary>
    /// Adds the encoder specific options. Override it in derived classes.
    /// </summary>
    /// <param name="options">The options to add to.</param>
    protected virtual void CollectOptions(EncoderOptions options) { }

    /// <summary>
    /// Adds the options of the stream format (size, pixel or sample format, ...) that are set.
    /// </summary>
    /// <param name="context">The codec context.</param>
    private protected abstract void ConfigureFormat(CodecContext context);

    /// <summary>
    /// Fills in what <see cref="CreateCodecContext"/> needs and the settings leave open (e.g. a pixel format).
    /// </summary>
    /// <param name="context">The configured codec context.</param>
    private protected abstract void ApplyDefaults(CodecContext context);

    /// <summary>
    /// Writes the stream format part of the command line (e.g. <c>-s 1920x1080 -pix_fmt yuv420p</c>).
    /// </summary>
    /// <param name="sb">The command line.</param>
    private protected abstract void AppendFormat(StringBuilder sb);

    /// <summary>
    /// Gets the options the settings translate to, in the order they are applied (without
    /// <see cref="AdditionalParameters"/>).
    /// </summary>
    /// <returns>The options.</returns>
    public EncoderOptions GetOptions()
    {
        EncoderOptions options = new();
        if (ThreadCount.HasValue)
            options.SetContext("threads", ThreadCount.Value);
        CollectOptions(options);
        return options;
    }

    /// <summary>
    /// Applies the settings that are set to an encoder context that is not open yet.
    /// </summary>
    /// <param name="context">An encoder context for <see cref="Codec"/>.</param>
    /// <exception cref="ArgumentException">The context is for another codec, or the encoder rejects a setting
    /// (e.g. an unsupported pixel format or an unknown option in <see cref="AdditionalParameters"/>).</exception>
    /// <exception cref="InvalidOperationException">The context is already open.</exception>
    public void Configure(CodecContext context)
    {
        if (context.IsOpen)
            throw new InvalidOperationException("Encoder settings can only be applied before the context is opened.");
        if (context.Codec.Name != Codec.Name)
            throw new ArgumentException($"The context is for {context.Codec.Name}, these settings are for {Codec.Name}.", nameof(context));

        ConfigureFormat(context);
        foreach (EncoderOption option in GetOptions())
            Apply(context, option);
        foreach (KeyValuePair<string, string> parameter in AdditionalParameters)
        {
            AVResult32 res = context.SetOption(parameter.Key, parameter.Value);
            if (res.IsError)
                throw new ArgumentException($"{Codec.Name}: cannot set -{parameter.Key} {parameter.Value} ({res}).", nameof(AdditionalParameters));
        }
        AddCodecOptions(context);
    }

    /// <summary>
    /// Creates an encoder context with these settings; open it with <see cref="CodecContext.Open(Codec?, IDictionary{string, string}?)"/>.
    /// </summary>
    /// <returns>A new, configured, not yet opened encoder context.</returns>
    /// <exception cref="ArgumentException">The encoder rejects a setting.</exception>
    public CodecContext CreateCodecContext()
    {
        CodecContext context = CodecContext.Allocate(Codec);
        try
        {
            Configure(context);
            ApplyDefaults(context);
            return context;
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    /// <summary>Gets whether <paramref name="values"/> contains <paramref name="value"/> (enums are not <see cref="IEquatable{T}"/>).</summary>
    private protected static bool Contains<T>(ReadOnlySpan<T> values, T value) where T : struct, Enum
    {
        foreach (T item in values)
        {
            if (EqualityComparer<T>.Default.Equals(item, value))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Adds codec options directly to the context, after all other settings.
    /// </summary>
    /// <remarks>Kept for derived classes written before <see cref="CollectOptions"/>; prefer that.</remarks>
    /// <param name="context">The codec context.</param>
    protected virtual void AddCodecOptions(CodecContext context) { }

    private unsafe void Apply(CodecContext context, EncoderOption option)
    {
        _AVCodecContext* ctx = ((IAVPointer<_AVCodecContext>)context).Pointer;
        // Private options are set on the encoder's own object: a recursive search would find options of the codec
        // context with the same name first (e.g. the integer "profile" instead of libx264's "profile").
        void* target = option.IsPrivate ? ctx->priv_data : ctx;
        AVResult32 res;
        if (target == null)
        {
            res = AVResult32.OptionNotFound;
        }
        else if (option.Values != null)
        {
            _AVDictionary* dictionary = null;
            try
            {
                foreach (KeyValuePair<string, string> pair in option.Values)
                    _ = ffmpeg.av_dict_set(&dictionary, pair.Key, pair.Value, 0);
                res = ffmpeg.av_opt_set_dict_val(target, option.Name, dictionary, 0);
            }
            finally
            {
                ffmpeg.av_dict_free(&dictionary);
            }
        }
        else
        {
            res = ffmpeg.av_opt_set(target, option.Name, option.Value, 0);
        }
        if (res.IsError)
            throw new ArgumentException($"{Codec.Name}: cannot set {option} ({res}).");
    }

    /// <summary>
    /// Returns the settings as <c>ffmpeg</c> command line arguments, e.g.
    /// <c>-c:v libx264 -pix_fmt yuv420p -preset slow -crf 20</c>.
    /// </summary>
    /// <returns>The command line arguments.</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        string stream = Codec.MediaType switch
        {
            MediaType.Video => ":v",
            MediaType.Audio => ":a",
            MediaType.Subtitle => ":s",
            _ => string.Empty,
        };
        _ = sb.Append($"-c{stream} {Codec.Name}");
        AppendFormat(sb);
        foreach (EncoderOption option in GetOptions())
            _ = sb.Append(' ').Append(option);
        foreach (KeyValuePair<string, string> parameter in AdditionalParameters)
            _ = sb.Append(" -").Append(parameter.Key).Append(' ').Append(EncoderOption.Quote(parameter.Value));
        return sb.ToString();
    }
}

/// <summary>
/// One option an <see cref="EncoderSettings"/> object sets.
/// </summary>
public readonly struct EncoderOption
{
    internal EncoderOption(string name, string? value, IReadOnlyDictionary<string, string>? values, bool isPrivate)
    {
        Name = name;
        Value = value;
        Values = values;
        IsPrivate = isPrivate;
    }

    /// <summary>Gets the option name, e.g. <c>crf</c>.</summary>
    public string Name { get; }

    /// <summary>Gets the value; <see langword="null"/> for dictionary options.</summary>
    public string? Value { get; }

    /// <summary>Gets the entries of a dictionary option (e.g. <c>x264-params</c>); otherwise <see langword="null"/>.</summary>
    public IReadOnlyDictionary<string, string>? Values { get; }

    /// <summary>Gets whether it is a private option of the encoder; otherwise it is an option of the codec context.</summary>
    public bool IsPrivate { get; }

    /// <summary>Returns the option as command line argument, e.g. <c>-crf 20</c> or <c>-x264-params "keyint=50:bframes=3"</c>.</summary>
    /// <returns>The argument.</returns>
    public override string ToString()
        => Values == null
            ? $"-{Name} {Quote(Value ?? string.Empty)}"
            : $"-{Name} {Quote(string.Join(":", Values.Select(kv => $"{Escape(kv.Key)}={Escape(kv.Value)}")))}";

    // FFmpeg parses dictionary options with av_get_token: ':' and '=' must be escaped with a backslash.
    private static string Escape(string text) => text.Replace("\\", "\\\\").Replace(":", "\\:").Replace("=", "\\=");

    internal static string Quote(string text)
        => text.Length > 0 && text.IndexOfAny([' ', '"', '\t']) < 0 ? text : $"\"{text.Replace("\"", "\\\"")}\"";
}

/// <summary>
/// The ordered options an <see cref="EncoderSettings"/> object sets; setting an option again replaces it.
/// </summary>
public sealed class EncoderOptions : IEnumerable<EncoderOption>
{
    private readonly List<EncoderOption> options = [];

    /// <summary>Gets the number of options.</summary>
    public int Count => options.Count;

    /// <summary>Gets the option with the given name, or <see langword="null"/>.</summary>
    /// <param name="name">The option name.</param>
    /// <returns>The option.</returns>
    public EncoderOption? this[string name]
    {
        get
        {
            int index = options.FindIndex(o => o.Name == name);
            return index < 0 ? null : options[index];
        }
    }

    private void Add(EncoderOption option)
    {
        int index = options.FindIndex(o => o.Name == option.Name && o.IsPrivate == option.IsPrivate);
        if (index < 0)
            options.Add(option);
        else
            options[index] = option;
    }

    /// <summary>Sets a private option of the encoder.</summary>
    /// <param name="name">The option name.</param>
    /// <param name="value">The value as FFmpeg parses it.</param>
    public void SetPrivate(string name, string value) => Add(new(name, value, null, true));

    /// <summary>Sets a private option of the encoder.</summary>
    /// <param name="name">The option name.</param>
    /// <param name="value">The value.</param>
    public void SetPrivate(string name, long value) => SetPrivate(name, value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Sets a private option of the encoder.</summary>
    /// <param name="name">The option name.</param>
    /// <param name="value">The value.</param>
    public void SetPrivate(string name, double value) => SetPrivate(name, value.ToString("R", CultureInfo.InvariantCulture));

    /// <summary>Sets a private boolean option of the encoder.</summary>
    /// <param name="name">The option name.</param>
    /// <param name="value">The value.</param>
    public void SetPrivate(string name, bool value) => SetPrivate(name, value ? "1" : "0");

    /// <summary>Sets a private dictionary option of the encoder (e.g. <c>x264-params</c>); empty dictionaries are skipped.</summary>
    /// <param name="name">The option name.</param>
    /// <param name="values">The entries; they are copied.</param>
    public void SetPrivate(string name, IEnumerable<KeyValuePair<string, string>> values)
    {
        Dictionary<string, string> copy = [];
        foreach (KeyValuePair<string, string> pair in values)
            copy[pair.Key] = pair.Value;
        if (copy.Count > 0)
            Add(new(name, null, copy, true));
    }

    /// <summary>Sets an option of the codec context (e.g. <c>b</c>, <c>g</c>, <c>bf</c>).</summary>
    /// <param name="name">The option name.</param>
    /// <param name="value">The value as FFmpeg parses it.</param>
    public void SetContext(string name, string value) => Add(new(name, value, null, false));

    /// <summary>Sets an option of the codec context.</summary>
    /// <param name="name">The option name.</param>
    /// <param name="value">The value.</param>
    public void SetContext(string name, long value) => SetContext(name, value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Sets an option of the codec context.</summary>
    /// <param name="name">The option name.</param>
    /// <param name="value">The value.</param>
    public void SetContext(string name, double value) => SetContext(name, value.ToString("R", CultureInfo.InvariantCulture));

    /// <summary>
    /// Selects variable bitrate encoding with a quality value (<c>-q:a</c> / <c>-q:v</c>): sets the <c>qscale</c> flag and
    /// <c>global_quality</c> to <paramref name="quality"/> × FF_QP2LAMBDA.
    /// </summary>
    /// <param name="quality">The quality in the encoder's scale.</param>
    public void SetQuality(double quality)
    {
        AddContextFlag("flags", "qscale");
        SetContext("global_quality", (long)Math.Round(quality * ffmpeg.FF_QP2LAMBDA));
    }

    /// <summary>Adds a flag to a flags option of the codec context (e.g. <c>flags</c>), keeping the flags added before.</summary>
    /// <param name="name">The flags option, e.g. <c>flags</c> or <c>flags2</c>.</param>
    /// <param name="flag">The flag name, e.g. <c>qscale</c>.</param>
    public void AddContextFlag(string name, string flag)
    {
        int index = options.FindIndex(o => o.Name == name && !o.IsPrivate);
        string value = index < 0 ? $"+{flag}" : $"{options[index].Value}+{flag}";
        SetContext(name, value);
    }

    /// <inheritdoc/>
    public IEnumerator<EncoderOption> GetEnumerator() => options.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
