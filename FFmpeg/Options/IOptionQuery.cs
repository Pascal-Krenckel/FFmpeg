using FFmpeg.Audio;
using FFmpeg.Collections;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Options;

/// <summary>
/// Defines methods for querying and setting options in a system. Provides functionality to find,
/// retrieve, and modify various types of options using either <see cref="Option"/> or option names.
/// </summary>
public interface IOptionQuery
{
    /// <inheritdoc cref="OptionQueryBase.FindOption(string, bool)"/>
    Option? FindOption(string name, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.ResetOptions()"/>
    void ResetOptions();

    /// <inheritdoc cref="OptionQueryBase.GetOptions(bool)"/>
    IReadOnlyList<Option> GetOptions(bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, (int, int), bool)"/>
    AVResult32 SetOption(Option option, (int Width, int Height) size, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, _AVPixelFormat, bool)"/>
    AVResult32 SetOption(Option option, PixelFormat value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, _AVSampleFormat, bool)"/>
    AVResult32 SetOption(Option option, SampleFormat value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, bool, bool)"/>
    AVResult32 SetOption(Option option, bool value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, double, bool)"/>
    AVResult32 SetOption(Option option, double value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, IDictionary{string, string}, bool)"/>
    AVResult32 SetOption(Option option, IDictionary<string, string> values, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, ILookup{string, string}, bool)"/>
    AVResult32 SetOption(Option option, ILookup<string, string> values, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, int, int, bool)"/>
    AVResult32 SetOption(Option option, int width, int height, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, long, bool)"/>
    AVResult32 SetOption(Option option, long value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, object, bool)"/>
    AVResult32 SetOption(Option option, object value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, Rational, bool)"/>
    AVResult32 SetOption(Option option, Rational value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, ReadOnlyMemory{byte}, bool)"/>
    AVResult32 SetOption(Option option, ReadOnlyMemory<byte> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, ReadOnlySpan{byte}, bool)"/>
    AVResult32 SetOption(Option option, ReadOnlySpan<byte> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(Option, string, bool)"/>
    AVResult32 SetOption(Option option, string value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(IDictionary{string, string}, bool)"/>
    AVResult32 SetOption(IDictionary<string, string> values, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, (int, int), bool)"/>
    AVResult32 SetOption(string name, (int Width, int Height) size, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, _AVPixelFormat, bool)"/>
    AVResult32 SetOption(string name, PixelFormat format, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, _AVSampleFormat, bool)"/>
    AVResult32 SetOption(string name, SampleFormat format, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, bool, bool)"/>
    AVResult32 SetOption(string name, bool value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, double, bool)"/>
    AVResult32 SetOption(string name, double value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, IDictionary{string, string}, bool)"/>
    AVResult32 SetOption(string name, IDictionary<string, string> values, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, ILookup{string, string}, bool)"/>
    AVResult32 SetOption(string name, ILookup<string, string> values, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, int, int, bool)"/>
    AVResult32 SetOption(string name, int width, int height, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, long, bool)"/>
    AVResult32 SetOption(string name, long value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, object, bool)"/>
    AVResult32 SetOption(string name, object value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, Rational, bool)"/>
    AVResult32 SetOption(string name, Rational value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, ReadOnlyMemory{byte}, bool)"/>
    AVResult32 SetOption(string name, ReadOnlyMemory<byte> memory, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, ReadOnlySpan{byte}, bool)"/>
    AVResult32 SetOption(string name, ReadOnlySpan<byte> span, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, ref ILookup{string, string}, bool)"/>
    AVResult32 SetOption(string name, ref ILookup<string, string> values, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.SetOption(string, string, bool)"/>
    AVResult32 SetOption(string name, string value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out ValueTuple{int, int}, bool)"/>
    AVResult32 TryGetOption(Option option, out (int Width, int Height) size, bool recursive = true);


    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out Collections.AVDictionary, bool)"/>
    AVResult32 TryGetOption(Option option, out Collections.AVDictionary value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out AVMultiDictionary, bool)"/>
    AVResult32 TryGetOption(Option option, out AVMultiDictionary value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out PixelFormat, bool)"/>
    AVResult32 TryGetOption(Option option, out PixelFormat value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out SampleFormat, bool)"/>
    AVResult32 TryGetOption(Option option, out SampleFormat value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out bool, bool)"/>
    AVResult32 TryGetOption(Option option, out bool value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out byte[], bool)"/>
    AVResult32 TryGetOption(Option option, out byte[] value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out Dictionary{string, string}, bool)"/>
    AVResult32 TryGetOption(Option option, out Dictionary<string, string> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out double, bool)"/>
    AVResult32 TryGetOption(Option option, out double value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out IDictionary{string, string}, bool)"/>
    AVResult32 TryGetOption(Option option, out IDictionary<string, string> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out ILookup{string, string}, bool)"/>
    AVResult32 TryGetOption(Option option, out ILookup<string, string> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out int, bool)"/>
    AVResult32 TryGetOption(Option option, out int value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out int, out int, bool)"/>
    AVResult32 TryGetOption(Option option, out int width, out int height, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out long, bool)"/>
    AVResult32 TryGetOption(Option option, out long value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out Memory{byte}, bool)"/>
    AVResult32 TryGetOption(Option option, out Memory<byte> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out object?, bool)"/>
    AVResult32 TryGetOption(Option option, out object? obj, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out Rational, bool)"/>
    AVResult32 TryGetOption(Option option, out Rational value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out ReadOnlyMemory{byte}, bool)"/>
    AVResult32 TryGetOption(Option option, out ReadOnlyMemory<byte> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out string?, bool)"/>
    AVResult32 TryGetOption(Option option, out string? value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out uint, bool)"/>
    AVResult32 TryGetOption(Option option, out uint value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, out ulong, bool)"/>
    AVResult32 TryGetOption(Option option, out ulong value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(Option, Span{byte}, bool)"/>
    AVResult32 TryGetOption(Option option, Span<byte> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out (int, int), bool)"/>
    AVResult32 TryGetOption(string name, out (int Width, int Height) size, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out Collections.AVDictionary, bool)"/>
    AVResult32 TryGetOption(string name, out Collections.AVDictionary value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out AVMultiDictionary, bool)"/>
    AVResult32 TryGetOption(string name, out AVMultiDictionary value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out _AVPixelFormat, bool)"/>
    AVResult32 TryGetOption(string name, out PixelFormat value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out _AVSampleFormat, bool)"/>
    AVResult32 TryGetOption(string name, out SampleFormat value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out bool, bool)"/>
    AVResult32 TryGetOption(string name, out bool value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out byte[], bool)"/>
    AVResult32 TryGetOption(string name, out byte[] value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out Dictionary{string, string}, bool)"/>
    AVResult32 TryGetOption(string name, out Dictionary<string, string> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out double, bool)"/>
    AVResult32 TryGetOption(string name, out double value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out IDictionary{string, string}, bool)"/>
    AVResult32 TryGetOption(string name, out IDictionary<string, string> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out ILookup{string, string}, bool)"/>
    AVResult32 TryGetOption(string name, out ILookup<string, string> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out int, bool)"/>
    AVResult32 TryGetOption(string name, out int value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out int, out int, bool)"/>
    AVResult32 TryGetOption(string name, out int width, out int height, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out long, bool)"/>
    AVResult32 TryGetOption(string name, out long value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out Memory{byte}, bool)"/>
    AVResult32 TryGetOption(string name, out Memory<byte> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out object?, bool)"/>
    AVResult32 TryGetOption(string name, out object? value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out Rational, bool)"/>
    AVResult32 TryGetOption(string name, out Rational value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out ReadOnlyMemory{byte}, bool)"/>
    AVResult32 TryGetOption(string name, out ReadOnlyMemory<byte> value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out string?, bool)"/>
    AVResult32 TryGetOption(string name, out string? value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out uint, bool)"/>
    AVResult32 TryGetOption(string name, out uint value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, out ulong, bool)"/>
    AVResult32 TryGetOption(string name, out ulong value, bool recursive = true);

    /// <inheritdoc cref="OptionQueryBase.TryGetOption(string, Span{byte}, bool)"/>
    AVResult32 TryGetOption(string name, Span<byte> value, bool recursive = true);


}
