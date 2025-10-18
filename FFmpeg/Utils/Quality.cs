namespace FFmpeg.Utils;

/// <summary>
/// Represents quality from 1 (BEST) to FF_LAMBDA_MAX (WORST).
/// </summary>
public readonly struct Quality : IEquatable<Quality>, IEquatable<int>
{
    private readonly int Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Quality"/> struct with the default value of 1 (BEST).
    /// </summary>
    public Quality() => Value = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Quality"/> struct with the specified value.
    /// </summary>
    /// <param name="value">The quality value.</param>
    public Quality(int value) => Value = value;

    /// <summary>
    /// The best quality value.
    /// </summary>
    public const int BEST = 1;

    /// <summary>
    /// The worst quality value.
    /// </summary>
    public const int WORST = ffmpeg.FF_LAMBDA_MAX;

    /// <summary>
    /// Implicitly converts a <see cref="Quality"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="quality">The <see cref="Quality"/> to convert.</param>
    public static implicit operator int(Quality quality) => quality.Value;

    /// <summary>
    /// Implicitly converts an <see cref="int"/> to a <see cref="Quality"/>.
    /// </summary>
    /// <param name="quality">The <see cref="int"/> to convert.</param>
    public static implicit operator Quality(int quality) => new(quality);

    /// <summary>
    /// Determines whether two <see cref="Quality"/> instances are equal.
    /// </summary>
    /// <param name="left">The left <see cref="Quality"/>.</param>
    /// <param name="right">The right <see cref="Quality"/>.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Quality left, Quality right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Quality"/> instances are not equal.
    /// </summary>
    /// <param name="left">The left <see cref="Quality"/>.</param>
    /// <param name="right">The right <see cref="Quality"/>.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Quality left, Quality right) => !(left == right);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Quality quality && Equals(quality);

    /// <summary>
    /// Determines whether the specified <see cref="Quality"/> is equal to the current <see cref="Quality"/>.
    /// </summary>
    /// <param name="other">The <see cref="Quality"/> to compare with the current <see cref="Quality"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="Quality"/> is equal to the current <see cref="Quality"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(Quality other) => Value == other.Value;

    /// <summary>
    /// Determines whether the specified <see cref="int"/> is equal to the current <see cref="Quality"/>.
    /// </summary>
    /// <param name="other">The <see cref="int"/> to compare with the current <see cref="Quality"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="int"/> is equal to the current <see cref="Quality"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(int other) => Value == other;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Value);

    /// <inheritdoc cref="int.ToString()"/>
    public override string ToString() => Value.ToString();
}
