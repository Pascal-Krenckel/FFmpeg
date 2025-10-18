using FFmpeg.AutoGen;
using System.Runtime.CompilerServices;

namespace FFmpeg.Utils;

/// <summary>
/// Represents a rational number with a numerator and denominator.
/// This struct is readonly for performance reasons.
/// </summary>
public readonly unsafe struct Rational : IEquatable<Rational>
{
    public static Rational TIME_BASE => new(1, ffmpeg.AV_TIME_BASE);


    /// <summary>
    /// Numerator of the rational number.
    /// </summary>
    public int Numerator { get; }

    /// <summary>
    /// Width represented by the numerator.
    /// </summary>
    public int Width => Numerator;

    /// <summary>
    /// Height represented by the denominator.
    /// </summary>
    public int Height => Denominator;

    /// <summary>
    /// Denominator of the rational number.
    /// </summary>
    public int Denominator { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rational"/> struct with a value of 0/1.
    /// </summary>
    public Rational() : this(0, 1) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rational"/> struct with the specified numerator and denominator.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <exception cref="DivideByZeroException">Thrown when the denominator is zero.</exception>
    public Rational(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
        else
        {
            int gcd = GCD(numerator, denominator);

            // Normalize to avoid negative denominator
            Numerator = numerator / gcd;
            Denominator = denominator / gcd;
            if (Denominator < 0)
            {
                Numerator = -Numerator;
                Denominator = -Denominator;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rational"/> struct using a long numerator and denominator, reducing the result.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    public Rational(long numerator, long denominator)
    {
        int n;
        int d;
        _ = ffmpeg.av_reduce(&n, &d, numerator, denominator, int.MaxValue); // Let ffmpeg handle reduction
        Numerator = n;
        Denominator = d;
    }

    #region Implicit Conversions

    /// <summary>
    /// Implicitly converts a tuple of (numerator, denominator) to a <see cref="Rational"/>.
    /// </summary>
    public static implicit operator Rational((int numerator, int denominator) value)
        => new(value.numerator, value.denominator);

    /// <summary>
    /// Implicitly converts a <see cref="Rational"/> to an <see cref="_AVRational"/>.
    /// </summary>
    public static implicit operator _AVRational(Rational r)
        => new()
        { num = r.Numerator, den = r.Denominator };

    /// <summary>
    /// Implicitly converts an <see cref="_AVRational"/> to a <see cref="Rational"/>.
    /// </summary>
    public static implicit operator Rational(_AVRational r)
        => new(r.num, r.den);

    /// <summary>
    /// Implicitly converts a <see cref="Rational"/> to a <see cref="TimeSpan"/>.
    /// </summary>
    public static implicit operator TimeSpan(Rational r)
        => ffmpeg.av_q2TimeSpan(r);

    /// <summary>
    /// Implicitly converts a <see cref="TimeSpan"/> to a <see cref="Rational"/>.
    /// </summary>
    public static implicit operator Rational(TimeSpan timeSpan)
        => timeSpan.TotalSeconds;

    /// <summary>
    /// Implicitly converts a <see cref="double"/> to a <see cref="Rational"/>.
    /// </summary>
    public static implicit operator Rational(double value)
        => ffmpeg.av_d2q(value, int.MaxValue);

    /// <summary>
    /// Implicitly converts a <see cref="Rational"/> to a <see cref="double"/>.
    /// </summary>
    public static implicit operator double(Rational r)
        => (double)r.Numerator / r.Denominator;

    #endregion

    #region Operators

    /// <summary>
    /// Multiplies an integer by a <see cref="Rational"/>.
    /// </summary>
    public static Rational operator *(int value, Rational r)
        => new(value * r.Numerator, r.Denominator);

    /// <summary>
    /// Multiplies a long integer by a <see cref="Rational"/>.
    /// </summary>
    public static Rational operator *(long value, Rational r)
        => new((int)(value * r.Numerator), r.Denominator);

    /// <summary>
    /// Multiplies two <see cref="Rational"/> numbers.
    /// </summary>
    public static Rational operator *(Rational r1, Rational r2)
        => new((long)r1.Numerator * r2.Numerator, (long)r1.Denominator * r2.Denominator);

    /// <summary>
    /// Divides one <see cref="Rational"/> number by another.
    /// </summary>
    public static Rational operator /(Rational r1, Rational r2)
        => new((long)r1.Numerator * r2.Denominator, (long)r1.Denominator * r2.Numerator);

    /// <summary>
    /// Divides one <see cref="Rational"/> number by another.
    /// </summary>
    public static long operator /(TimeSpan t, Rational r2)
        => (long)((Rational)t / r2);

    ///<inheritdoc />
    public static bool operator <(Rational left, Rational right) => (long)left.Numerator * right.Denominator < (long)right.Numerator * left.Denominator;

    ///<inheritdoc />
    public static bool operator >(Rational left, Rational right) => (long)left.Numerator * right.Denominator > (long)right.Numerator * left.Denominator;

    ///<inheritdoc />
    public static bool operator <=(Rational left, Rational right) => (long)left.Numerator * right.Denominator <= (long)right.Numerator * left.Denominator;

    ///<inheritdoc />
    public static bool operator >=(Rational left, Rational right) => (long)left.Numerator * right.Denominator >= (long)right.Numerator * left.Denominator;
    #endregion

    #region IEquatable<Rational> Implementation

    /// <summary>
    /// Determines whether the current <see cref="Rational"/> is equal to another <see cref="Rational"/>.
    /// </summary>
    /// <param name="other">The <see cref="Rational"/> to compare with the current <see cref="Rational"/>.</param>
    /// <returns>true if the current <see cref="Rational"/> is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(Rational other) => Numerator == other.Numerator && Denominator == other.Denominator;

    /// <summary>
    /// Determines whether the current <see cref="Rational"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Rational"/>.</param>
    /// <returns>true if the specified object is equal to the current <see cref="Rational"/>; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Rational other && Equals(other);

    #endregion

    public bool IsValidTimeBase => Numerator > 0 && Denominator > 0;

    #region HashCode

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Rational"/>.</returns>
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    #endregion

    /// <summary>
    /// Rescales a 64-bit integer according to two rational time bases. <br/>
    /// This operation is mathematically equivalent to <c>value * timeBaseSrc / timeBaseDst</c>.
    /// </summary>
    /// <param name="value">The 64-bit integer value to be rescaled.</param>
    /// <param name="timeBaseSrc">The source time base (rational number).</param>
    /// <param name="timeBaseDst">The destination time base (rational number).</param>
    /// <returns>The rescaled value in the destination time base units.</returns>
    public static long Rescale(long value, Rational timeBaseSrc, Rational timeBaseDst) => ffmpeg.av_rescale_q(value, timeBaseSrc, timeBaseDst);

    /// <summary>
    /// Rescales a 64-bit integer from a specific source time base to the current instance's time base. <br/>
    /// This operation is mathematically equivalent to <c>value * timeBaseSrc / <see langword="this"/></c>, 
    /// where <see langword="this"/> refers to the current time base.
    /// </summary>
    /// <param name="value">The 64-bit integer value to be rescaled.</param>
    /// <param name="timeBaseSrc">The source time base (rational number).</param>
    /// <returns>The rescaled value in the current instance's time base units.</returns>
    public long Rescale(long value, Rational timeBaseSrc) => Rescale(value, timeBaseSrc, this);



    /// <summary>
    /// Reduces the <see cref="Rational"/> number to its simplest form, considering a maximum value for the numerator and denominator.
    /// </summary>
    /// <param name="max">The maximum allowed value for the numerator and denominator.</param>
    /// <returns>A reduced <see cref="Rational"/> number.</returns>
    public Rational Reduce(int max)
    {
        int d;
        int n;
        _ = ffmpeg.av_reduce(&d, &n, Numerator, Denominator, max);
        return new(n, d);
    }

    /// <summary>
    /// Returns a string representation of the <see cref="Rational"/> number.
    /// </summary>
    /// <returns>A string in the format "numerator/denominator".</returns>
    public override string ToString() => $"{Numerator}/{Denominator}";

    #region Helper Methods

    /// <summary>
    /// Computes the greatest common divisor (GCD) of two integers.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The GCD of the two integers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    #endregion
}
