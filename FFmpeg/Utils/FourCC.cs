/// <summary>
/// Represents a FourCC (four-character code), which is a sequence of four 8-bit characters
/// packed into a 32-bit unsigned integer.
/// </summary>
public readonly struct FourCC : IEquatable<FourCC>
{
    private readonly uint _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="FourCC"/> struct from a read-only span of characters.
    /// </summary>
    /// <param name="code">A span containing the four-character code.</param>
    /// <exception cref="ArgumentException">Thrown if the length of <paramref name="code"/> is not exactly 4 characters.</exception>
    public FourCC(ReadOnlySpan<char> code)
    {
        if (code.Length != 4)
            throw new ArgumentException("FourCC code must be exactly 4 characters long.", nameof(code));

        _value = (uint)(((code[3] & 0xFF) << 24) |
                        ((code[2] & 0xFF) << 16) |
                        ((code[1] & 0xFF) << 8) |
                        (code[0] & 0xFF));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FourCC"/> struct from a 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer representing the FourCC code.</param>
    public FourCC(uint value) => _value = value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is FourCC cC && Equals(cC);

    /// <inheritdoc/>
    public bool Equals(FourCC other) => _value == other._value;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_value);

    /// <summary>
    /// Converts the FourCC code to its string representation (four characters).
    /// </summary>
    /// <returns>A string containing the four-character code.</returns>
    public override string ToString()
    {
        if (_value == 0)
            return string.Empty;
        ReadOnlySpan<char> chars = stackalloc[]
        {
            (char)(_value & 0xFF),
            (char)((_value >> 8) & 0xFF),
            (char)((_value >> 16) & 0xFF),
            (char)((_value >> 24) & 0xFF),

        };
        return new string(chars);
    }

    /// <summary>
    /// Determines whether two <see cref="FourCC"/> values are equal.
    /// </summary>
    /// <param name="left">The first <see cref="FourCC"/> to compare.</param>
    /// <param name="right">The second <see cref="FourCC"/> to compare.</param>
    /// <returns><see langword="true"/> if the two values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(FourCC left, FourCC right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="FourCC"/> values are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="FourCC"/> to compare.</param>
    /// <param name="right">The second <see cref="FourCC"/> to compare.</param>
    /// <returns><see langword="true"/> if the two values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(FourCC left, FourCC right) => !(left == right);

    /// <summary>
    /// Implicitly converts a <see cref="FourCC"/> to its 32-bit unsigned integer representation.
    /// </summary>
    /// <param name="fourcc">The <see cref="FourCC"/> to convert.</param>
    public static implicit operator uint(FourCC fourcc) => fourcc._value;

    /// <summary>
    /// Implicitly converts a 32-bit unsigned integer to a <see cref="FourCC"/>.
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer representing the FourCC code.</param>
    public static implicit operator FourCC(uint value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="FourCC"/> to its string representation.
    /// </summary>
    /// <param name="fourcc">The <see cref="FourCC"/> to convert.</param>
    public static implicit operator ReadOnlySpan<char>(FourCC fourcc) => fourcc.ToString();

    /// <summary>
    /// Implicitly converts a <see cref="FourCC"/> to its string representation.
    /// </summary>
    /// <param name="fourcc">The <see cref="FourCC"/> to convert.</param>
    public static implicit operator string(FourCC fourcc) => fourcc.ToString();

    /// <summary>
    /// Implicitly converts a read-only span of characters to a <see cref="FourCC"/>.
    /// </summary>
    /// <param name="value">The read-only span containing the four-character code.</param>
    /// <exception cref="ArgumentException">Thrown if the length of <paramref name="value"/> is not exactly 4 characters.</exception>
    public static implicit operator FourCC(ReadOnlySpan<char> value) => new(value);

    /// <summary>
    /// Implicitly converts a string to a <see cref="FourCC"/>.
    /// </summary>
    /// <param name="value">The string containing the four-character code.</param>
    /// <exception cref="ArgumentException">Thrown if the length of <paramref name="value"/> is not exactly 4 characters.</exception>
    public static implicit operator FourCC(string value) => new(value);
}


