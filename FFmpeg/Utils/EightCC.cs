using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Utils;
/// <summary>
/// Represents an EightCC (eight-character code), which is a sequence of eight 8-bit characters
/// packed into a 64-bit unsigned integer.
/// </summary>
public readonly struct EightCC : IEquatable<EightCC>
{
    private readonly ulong _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="EightCC"/> struct from a read-only span of characters.
    /// </summary>
    /// <param name="code">A span containing the eight-character code.</param>
    /// <exception cref="ArgumentException">Thrown if the length of <paramref name="code"/> is not exactly 8 characters.</exception>
    public EightCC(ReadOnlySpan<char> code)
    {
        if (code.Length != 8)
            throw new ArgumentException("EightCC code must be exactly 8 characters long.", nameof(code));

        _value = (ulong)(((code[7] & 0xFF) << 56) |
                         ((code[6] & 0xFF) << 48) |
                         ((code[5] & 0xFF) << 40) |
                         ((code[4] & 0xFF) << 32) |
                         ((code[3] & 0xFF) << 24) |
                         ((code[2] & 0xFF) << 16) |
                         ((code[1] & 0xFF) << 8) |
                         (code[0] & 0xFF));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EightCC"/> struct from a 64-bit unsigned integer.
    /// </summary>
    /// <param name="value">The 64-bit unsigned integer representing the EightCC code.</param>
    public EightCC(ulong value) => _value = value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EightCC eightCC && Equals(eightCC);

    /// <inheritdoc/>
    public bool Equals(EightCC other) => _value == other._value;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_value);

    /// <summary>
    /// Converts the EightCC code to its string representation (eight characters).
    /// </summary>
    /// <returns>A string containing the eight-character code.</returns>
    public override string ToString()
    {
        if (_value == 0) return string.Empty;
        ReadOnlySpan<char> chars = stackalloc[]
        {
            (char)(_value & 0xFF),
            (char)((_value >> 8) & 0xFF),
            (char)((_value >> 16) & 0xFF),
            (char)((_value >> 24) & 0xFF),
            (char)((_value >> 32) & 0xFF),
            (char)((_value >> 40) & 0xFF),
            (char)((_value >> 48) & 0xFF),
            (char)((_value >> 56) & 0xFF),
        };
        return new string(chars);
    }

    /// <summary>
    /// Determines whether two <see cref="EightCC"/> values are equal.
    /// </summary>
    /// <param name="left">The first <see cref="EightCC"/> to compare.</param>
    /// <param name="right">The second <see cref="EightCC"/> to compare.</param>
    /// <returns><see langword="true"/> if the two values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(EightCC left, EightCC right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="EightCC"/> values are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="EightCC"/> to compare.</param>
    /// <param name="right">The second <see cref="EightCC"/> to compare.</param>
    /// <returns><see langword="true"/> if the two values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(EightCC left, EightCC right) => !(left == right);

    /// <summary>
    /// Implicitly converts an <see cref="EightCC"/> to its 64-bit unsigned integer representation.
    /// </summary>
    /// <param name="eightCC">The <see cref="EightCC"/> to convert.</param>
    public static implicit operator ulong(EightCC eightCC) => eightCC._value;

    /// <summary>
    /// Implicitly converts a 64-bit unsigned integer to an <see cref="EightCC"/>.
    /// </summary>
    /// <param name="value">The 64-bit unsigned integer representing the EightCC code.</param>
    public static implicit operator EightCC(ulong value) => new(value);

    /// <summary>
    /// Implicitly converts an <see cref="EightCC"/> to its string representation.
    /// </summary>
    /// <param name="eightCC">The <see cref="EightCC"/> to convert.</param>
    public static implicit operator ReadOnlySpan<char>(EightCC eightCC) => eightCC.ToString();

    /// <summary>
    /// Implicitly converts an <see cref="EightCC"/> to its string representation.
    /// </summary>
    /// <param name="eightCC">The <see cref="EightCC"/> to convert.</param>
    public static implicit operator string(EightCC eightCC) => eightCC.ToString();

    /// <summary>
    /// Implicitly converts a read-only span of characters to an <see cref="EightCC"/>.
    /// </summary>
    /// <param name="value">The read-only span containing the eight-character code.</param>
    /// <exception cref="ArgumentException">Thrown if the length of <paramref name="value"/> is not exactly 8 characters.</exception>
    public static implicit operator EightCC(ReadOnlySpan<char> value) => new(value);

    /// <summary>
    /// Implicitly converts a string to an <see cref="EightCC"/>.
    /// </summary>
    /// <param name="value">The string containing the eight-character code.</param>
    /// <exception cref="ArgumentException">Thrown if the length of <paramref name="value"/> is not exactly 8 characters.</exception>
    public static implicit operator EightCC(string value) => new(value);
}

