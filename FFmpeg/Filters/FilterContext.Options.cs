using FFmpeg.Utils;

namespace FFmpeg.Filters;

public unsafe partial class FilterContext
{
    // Helpers for the typed filter classes. Getters return the default value of the type if the option
    // cannot be read; setters throw if FFmpeg rejects the value (e.g. out of range).

    private protected string? GetStringOption(string name) => TryGetOption(name, out string? value).IsError ? null : value;

    private protected void SetStringOption(string name, string? value) => SetOption(name, value!).ThrowIfError();

    private protected long GetInt64Option(string name) => TryGetOption(name, out long value).IsError ? 0 : value;

    private protected void SetInt64Option(string name, long value) => SetOption(name, value).ThrowIfError();

    private protected double GetDoubleOption(string name) => TryGetOption(name, out double value).IsError ? 0 : value;

    private protected void SetDoubleOption(string name, double value) => SetOption(name, value).ThrowIfError();

    private protected bool GetBoolOption(string name) => !TryGetOption(name, out bool value).IsError && value;

    private protected void SetBoolOption(string name, bool value) => SetOption(name, value).ThrowIfError();

    private protected Rational GetRationalOption(string name) => TryGetOption(name, out Rational value).IsError ? default : value;

    private protected void SetRationalOption(string name, Rational value) => SetOption(name, value).ThrowIfError();

    /// <summary>Reads an <c>AV_OPT_TYPE_DURATION</c> option (microseconds).</summary>
    private protected TimeSpan GetDurationOption(string name) => TimeSpan.FromTicks(GetInt64Option(name) * 10);

    /// <summary>Writes an <c>AV_OPT_TYPE_DURATION</c> option (microseconds).</summary>
    private protected void SetDurationOption(string name, TimeSpan value) => SetInt64Option(name, value.Ticks / 10);
}
