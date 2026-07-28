using FFmpeg.AutoGen;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace FFmpeg.Utils;

/// <summary>
/// Represents a rational number with a numerator and denominator.
/// This struct is readonly for performance reasons.
/// </summary>
public readonly unsafe struct Rational : IEquatable<Rational>, IComparable<Rational>, IConvertible, IFormattable
{
    /// <summary>
    /// Represents the time base used by the <see cref="Rational"/> structure.
    /// It is a rational number with a numerator of 1 and a denominator of <see cref="ffmpeg.AV_TIME_BASE"/>.
    /// This value is commonly used in time-based calculations in multimedia processing and represents the ffmpegs default time base.
    /// </summary>
    public static readonly Rational TIME_BASE = new(1, ffmpeg.AV_TIME_BASE);

    /// <summary>
    /// Represents the rational number zero (0/1).
    /// This value is commonly used to represent a zero rational value.
    /// </summary>
    public static readonly Rational Zero = new(0, 1);

    /// <summary>
    /// Represents the maximum possible value for a rational number, using <see cref="int.MaxValue"/> as the numerator and 1 as the denominator.
    /// </summary>
    public static readonly Rational MaxValue = new(int.MaxValue, 1);

    /// <summary>
    /// Represents the minimum possible value for a rational number, using <see cref="int.MinValue"/> as the numerator and 1 as the denominator.
    /// </summary>
    public static readonly Rational MinValue = new(int.MinValue, 1);

    /// <summary>
    /// Represents the smallest possible positive value for a rational number, defined as 1/<see cref="int.MaxValue"/>.
    /// This value is used to represent the smallest non-zero rational value.
    /// </summary>
    public static readonly Rational Epsilon = new(1, int.MaxValue);

    /// <summary>
    /// Represents positive infinity as a rational number (1/0).
    /// This is used to represent an infinitely large positive value.
    /// </summary>
    public static readonly Rational PositiveInfinity = new(1, 0);

    /// <summary>
    /// Represents negative infinity as a rational number (-1/0).
    /// This is used to represent an infinitely large negative value.
    /// </summary>
    public static readonly Rational NegativeInfinity = new(-1, 0);

    /// <summary>
    /// Represents NaN (Not-a-Number) as a rational number (0/0).
    /// This is used to represent an undefined or unrepresentable value.
    /// </summary>
    public static readonly Rational NaN = new(0, 0);




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
            Numerator = Math.Clamp(numerator, -1, 1);
            Denominator = 0;
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
        if (denominator == 0)
        {
            Numerator = (int)Math.Clamp(numerator, -1, 1);
            Denominator = 0;
        }
        else
        {
            int n;
            int d;
            _ = ffmpeg.av_reduce(&n, &d, numerator, denominator, int.MaxValue); // Let ffmpeg handle reduction
            Numerator = n;
            Denominator = d;
        }
    }



    #region Conversions

    /// <summary>
    /// Implicitly converts a tuple of int (numerator, denominator) to a <see cref="Rational"/>.
    /// </summary>
    public static implicit operator Rational((int numerator, int denominator) value)
        => new(value.numerator, value.denominator);


    /// <summary>
    /// Explicitly converts a tuple of long(numerator, denominator) to a <see cref="Rational"/>.
    /// </summary>
    public static implicit operator Rational((long numerator, long denominator) value)
        => new(value.numerator, value.denominator);

    /// <summary>
    /// Deconstructs the <see cref="Rational"/> into its numerator and denominator components.
    /// This method allows for easy extraction of the numerator and denominator 
    /// when using tuple deconstruction syntax.
    /// </summary>
    /// <param name="numerator">
    /// The numerator of the <see cref="Rational"/> instance.
    /// </param>
    /// <param name="denominator">
    /// The denominator of the <see cref="Rational"/> instance.
    /// </param>
    public void Deconstruct(out int numerator, out int denominator)
    {
        numerator = Numerator;
        denominator = Denominator;
    }

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

    /// <summary>
    /// Explicitly converts a <see cref="Rational"/> to a <see cref="long"/> by
    /// truncating its fractional part.
    /// </summary>
    /// <param name="r">The <see cref="Rational"/> value to convert.</param>
    /// <returns>
    /// The integral part of the rational value, truncated toward zero.
    /// </returns>
    public static explicit operator long(Rational r) => (long)((double)r.Numerator / r.Denominator);

    /// <summary>
    /// Explicitly converts a <see cref="Rational"/> to an <see cref="int"/> by
    /// truncating its fractional part.
    /// </summary>
    /// <param name="r">The <see cref="Rational"/> value to convert.</param>
    /// <returns>
    /// The integral part of the rational value, truncated toward zero.
    /// </returns>
    public static explicit operator int(Rational r) => (int)((double)r.Numerator / r.Denominator);

    /// <summary>
    /// Implicitly converts an <see cref="int"/> to a <see cref="Rational"/>.
    /// </summary>
    /// <param name="i">The integer value to convert.</param>
    /// <returns>
    /// A <see cref="Rational"/> representing <paramref name="i"/> with a denominator of <c>1</c>.
    /// </returns>
    public static implicit operator Rational(int i) => new(i, 1);

    /// <summary>
    /// Explicitly converts a <see cref="long"/> to a <see cref="Rational"/>.
    /// </summary>
    /// <param name="l">The integer value to convert.</param>
    /// <returns>
    /// A <see cref="Rational"/> representing <paramref name="l"/> with a denominator of <c>1</c>.
    /// </returns>
    public static explicit operator Rational(long l) => new(l, 1);

    #endregion

    #region Operators

    /// <summary>
    /// Multiplies an integer by a <see cref="Rational"/>.
    /// </summary>
    /// <param name="value">The integer to multiply with the rational number.</param>
    /// <param name="r">The <see cref="Rational"/> number to multiply.</param>
    /// <returns>A new <see cref="Rational"/> that represents the product of the integer and the rational number.</returns>
    public static Rational operator *(int value, Rational r)
        => new(value * r.Numerator, r.Denominator);

    /// <summary>
    /// Multiplies a long integer by a <see cref="Rational"/>.
    /// </summary>
    /// <param name="value">The long integer to multiply with the rational number.</param>
    /// <param name="r">The <see cref="Rational"/> number to multiply.</param>
    /// <returns>A new <see cref="Rational"/> that represents the product of the long integer and the rational number.</returns>
    public static Rational operator *(long value, Rational r)
        => new(value * r.Numerator, r.Denominator);

    /// <summary>
    /// Multiplies two <see cref="Rational"/> numbers.
    /// </summary>
    /// <param name="r1">The first <see cref="Rational"/> number to multiply.</param>
    /// <param name="r2">The second <see cref="Rational"/> number to multiply.</param>
    /// <returns>A new <see cref="Rational"/> that represents the product of the two rational numbers.</returns>
    public static Rational operator *(Rational r1, Rational r2)
        => new((long)r1.Numerator * r2.Numerator, (long)r1.Denominator * r2.Denominator);

    /// <summary>
    /// Adds two rational numbers together, returning their sum as a new <see cref="Rational"/>.
    /// The result is calculated by finding a common denominator and adding the numerators.
    /// </summary>
    /// <param name="a">The first rational number to be added.</param>
    /// <param name="b">The second rational number to be added.</param>
    /// <returns>A new <see cref="Rational"/> representing the sum of the two rational numbers.</returns>
    public static Rational operator +(Rational a, Rational b)
    {
        // Find a common denominator by multiplying the denominators.
        long commonDenominator = a.Denominator * b.Denominator;

        // Adjust the numerators to have the common denominator and add them.
        long numeratorSum = (a.Numerator * b.Denominator) + (b.Numerator * a.Denominator);

        return new Rational(numeratorSum, commonDenominator);
    }

    /// <summary>
    /// Subtracts one rational number from another, returning the result as a new <see cref="Rational"/>.
    /// The result is calculated by finding a common denominator and subtracting the numerators.
    /// </summary>
    /// <param name="a">The rational number to subtract from (minuend).</param>
    /// <param name="b">The rational number to subtract (subtrahend).</param>
    /// <returns>A new <see cref="Rational"/> representing the difference between the two rational numbers.</returns>
    public static Rational operator -(Rational a, Rational b)
    {
        // Find a common denominator by multiplying the denominators.
        long commonDenominator = a.Denominator * b.Denominator;

        // Adjust the numerators to have the common denominator and subtract them.
        long numeratorDifference = (a.Numerator * b.Denominator) - (b.Numerator * a.Denominator);

        return new Rational(numeratorDifference, commonDenominator);
    }

    /// <summary>
    /// Divides one <see cref="Rational"/> number by another, returning the result as a new <see cref="Rational"/>.
    /// </summary>
    /// <param name="r1">The <see cref="Rational"/> number to divide (numerator).</param>
    /// <param name="r2">The <see cref="Rational"/> number by which to divide (denominator).</param>
    /// <returns>A new <see cref="Rational"/> representing the quotient of the two rational numbers.</returns>
    public static Rational operator /(Rational r1, Rational r2)
        => new((long)r1.Numerator * r2.Denominator, (long)r1.Denominator * r2.Numerator);

    /// <summary>
    /// Divides a <see cref="TimeSpan"/> by a <see cref="Rational"/> number, returning the quotient as a long.
    /// </summary>
    /// <param name="t">The <see cref="TimeSpan"/> to divide.</param>
    /// <param name="r2">The <see cref="Rational"/> divisor.</param>
    /// <returns>A long representing the result of dividing the <see cref="TimeSpan"/> by the <see cref="Rational"/>.</returns>
    public static long operator /(TimeSpan t, Rational r2)
        => (long)((Rational)t / r2);

    /// <summary>
    /// Compares two <see cref="Rational"/> numbers for less than.
    /// </summary>
    /// <param name="left">The first <see cref="Rational"/> number to compare.</param>
    /// <param name="right">The second <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the first <see cref="Rational"/> is less than the second; otherwise, false.</returns>
    public static bool operator <(Rational left, Rational right)
    {
        long cmp = ffmpeg.av_cmp_q(left, right);
        return cmp is not int.MinValue and < 0;
    }

    /// <summary>
    /// Compares two <see cref="Rational"/> numbers for greater than.
    /// </summary>
    /// <param name="left">The first <see cref="Rational"/> number to compare.</param>
    /// <param name="right">The second <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the first <see cref="Rational"/> is greater than the second; otherwise, false.</returns>
    public static bool operator >(Rational left, Rational right)
    {
        long cmp = ffmpeg.av_cmp_q(left, right);
        return cmp is not int.MinValue and > 0;
    }

    /// <summary>
    /// Compares two <see cref="Rational"/> numbers for less than or equal to.
    /// </summary>
    /// <param name="left">The first <see cref="Rational"/> number to compare.</param>
    /// <param name="right">The second <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the first <see cref="Rational"/> is less than or equal to the second; otherwise, false.</returns>
    public static bool operator <=(Rational left, Rational right)
    {
        long cmp = ffmpeg.av_cmp_q(left, right);
        return cmp is not int.MinValue and <= 0;
    }

    /// <summary>
    /// Compares two <see cref="Rational"/> numbers for greater than or equal to.
    /// </summary>
    /// <param name="left">The first <see cref="Rational"/> number to compare.</param>
    /// <param name="right">The second <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the first <see cref="Rational"/> is greater than or equal to the second; otherwise, false.</returns>
    public static bool operator >=(Rational left, Rational right)
    {
        long cmp = ffmpeg.av_cmp_q(left, right);
        return cmp is not int.MinValue and >= 0;
    }

    /// <summary>
    /// Compares two <see cref="Rational"/> numbers for equality.
    /// </summary>
    /// <param name="left">The first <see cref="Rational"/> number to compare.</param>
    /// <param name="right">The second <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the two <see cref="Rational"/> numbers are equal; otherwise, false.</returns>
    public static bool operator ==(Rational left, Rational right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="Rational"/> numbers for inequality.
    /// </summary>
    /// <param name="left">The first <see cref="Rational"/> number to compare.</param>
    /// <param name="right">The second <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the two <see cref="Rational"/> numbers are not equal; otherwise, false.</returns>
    public static bool operator !=(Rational left, Rational right) =>
        // Check if the numbers are not equal by comparing numerators and denominators
        !(left == right);

    // Compare Rational < TimeSpan
    /// <summary>
    /// Compares a <see cref="Rational"/> number with a <see cref="TimeSpan"/> for less than.
    /// </summary>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <returns>True if the <see cref="Rational"/> is less than the <see cref="TimeSpan"/>; otherwise, false.</returns>
    public static bool operator <(Rational rational, TimeSpan timeSpan) =>
        // Convert TimeSpan to Rational and compare
        rational < (Rational)timeSpan;

    // Compare Rational > TimeSpan
    /// <summary>
    /// Compares a <see cref="Rational"/> number with a <see cref="TimeSpan"/> for greater than.
    /// </summary>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <returns>True if the <see cref="Rational"/> is greater than the <see cref="TimeSpan"/>; otherwise, false.</returns>
    public static bool operator >(Rational rational, TimeSpan timeSpan) =>
        // Convert TimeSpan to Rational and compare
        rational > (Rational)timeSpan;

    // Compare Rational <= TimeSpan
    /// <summary>
    /// Compares a <see cref="Rational"/> number with a <see cref="TimeSpan"/> for less than or equal to.
    /// </summary>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <returns>True if the <see cref="Rational"/> is less than or equal to the <see cref="TimeSpan"/>; otherwise, false.</returns>
    public static bool operator <=(Rational rational, TimeSpan timeSpan) =>
        // Convert TimeSpan to Rational and compare
        rational <= (Rational)timeSpan;

    // Compare Rational >= TimeSpan
    /// <summary>
    /// Compares a <see cref="Rational"/> number with a <see cref="TimeSpan"/> for greater than or equal to.
    /// </summary>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <returns>True if the <see cref="Rational"/> is greater than or equal to the <see cref="TimeSpan"/>; otherwise, false.</returns>
    public static bool operator >=(Rational rational, TimeSpan timeSpan) =>
        // Convert TimeSpan to Rational and compare
        rational >= (Rational)timeSpan;

    // Compare TimeSpan < Rational
    /// <summary>
    /// Compares a <see cref="TimeSpan"/> with a <see cref="Rational"/> for less than.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the <see cref="TimeSpan"/> is less than the <see cref="Rational"/>; otherwise, false.</returns>
    public static bool operator <(TimeSpan timeSpan, Rational rational) =>
        // Convert TimeSpan to Rational and compare
        (Rational)timeSpan < rational;

    // Compare TimeSpan > Rational
    /// <summary>
    /// Compares a <see cref="TimeSpan"/> with a <see cref="Rational"/> for greater than.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the <see cref="TimeSpan"/> is greater than the <see cref="Rational"/>; otherwise, false.</returns>
    public static bool operator >(TimeSpan timeSpan, Rational rational) =>
        // Convert TimeSpan to Rational and compare
        (Rational)timeSpan > rational;

    // Compare TimeSpan <= Rational
    /// <summary>
    /// Compares a <see cref="TimeSpan"/> with a <see cref="Rational"/> for less than or equal to.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the <see cref="TimeSpan"/> is less than or equal to the <see cref="Rational"/>; otherwise, false.</returns>
    public static bool operator <=(TimeSpan timeSpan, Rational rational) =>
        // Convert TimeSpan to Rational and compare
        (Rational)timeSpan <= rational;

    // Compare TimeSpan >= Rational
    /// <summary>
    /// Compares a <see cref="TimeSpan"/> with a <see cref="Rational"/> for greater than or equal to.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the <see cref="TimeSpan"/> is greater than or equal to the <see cref="Rational"/>; otherwise, false.</returns>
    public static bool operator >=(TimeSpan timeSpan, Rational rational) =>
        // Convert TimeSpan to Rational and compare
        (Rational)timeSpan >= rational;

    // Compare Rational == TimeSpan
    /// <summary>
    /// Compares a <see cref="Rational"/> with a <see cref="TimeSpan"/> for equality.
    /// </summary>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <returns>True if the two are equal; otherwise, false.</returns>
    public static bool operator ==(Rational rational, TimeSpan timeSpan) =>
        // Convert TimeSpan to Rational and compare
        rational == (Rational)timeSpan;

    // Compare Rational != TimeSpan
    /// <summary>
    /// Compares a <see cref="Rational"/> with a <see cref="TimeSpan"/> for inequality.
    /// </summary>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <returns>True if the two are not equal; otherwise, false.</returns>
    public static bool operator !=(Rational rational, TimeSpan timeSpan) =>
        // Convert TimeSpan to Rational and compare
        rational != (Rational)timeSpan;

    // Compare TimeSpan == Rational
    /// <summary>
    /// Compares a <see cref="TimeSpan"/> with a <see cref="Rational"/> for equality.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the two are equal; otherwise, false.</returns>
    public static bool operator ==(TimeSpan timeSpan, Rational rational) =>
        // Convert TimeSpan to Rational and compare
        (Rational)timeSpan == rational;

    // Compare TimeSpan != Rational
    /// <summary>
    /// Compares a <see cref="TimeSpan"/> with a <see cref="Rational"/> for inequality.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to compare.</param>
    /// <param name="rational">The <see cref="Rational"/> number to compare.</param>
    /// <returns>True if the two are not equal; otherwise, false.</returns>
    public static bool operator !=(TimeSpan timeSpan, Rational rational) =>
        // Convert TimeSpan to Rational and compare
        (Rational)timeSpan != rational;

    #endregion


    #region IEquatable<Rational>/Comparable Implementation

    /// <summary>
    /// Determines whether the current <see cref="Rational"/> is equal to another <see cref="Rational"/>.
    /// </summary>
    /// <param name="other">The <see cref="Rational"/> to compare with the current <see cref="Rational"/>.</param>
    /// <returns>true if the current <see cref="Rational"/> is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(Rational other)
    {
        if (Denominator == other.Denominator)
            return Numerator == other.Numerator;
        if (Denominator == 0 && other.Denominator == 0) // Inf,-Inf or Nan
        {
            // only true if both are +Inf or both are -Inf
            return (Numerator > 0 && other.Numerator > 0) || (Numerator < 0 && other.Numerator < 0);
        }
        return false;
    }

    /// <summary>
    /// Determines whether the current <see cref="Rational"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Rational"/>.</param>
    /// <returns>true if the specified object is equal to the current <see cref="Rational"/>; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Rational other && Equals(other);

    /// <inheritdoc />
    public int CompareTo(Rational other)
    {
        int comp = ffmpeg.av_cmp_q(this, other);
        return comp == int.MinValue ? 0 : comp;
    }


    #endregion

    /// <summary>
    /// Indicates whether the rational number represents a valid time base.
    /// A valid time base is defined as having both a positive numerator and denominator.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the rational number represents a valid time base (i.e., both numerator and denominator are positive), 
    /// otherwise <c>false</c>.
    /// </returns>
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

    /// <summary>
    /// Returns the greatest common divisor (GCD) of two rational numbers <paramref name="a"/> and <paramref name="b"/>.
    /// The GCD is chosen so that both numbers are multiples of it. If the resulting denominator exceeds the specified
    /// <paramref name="max_den" />, the method will return the provided <paramref name="default"/> rational value.
    /// </summary>
    /// <param name="a">
    /// The first rational number for which the GCD is to be calculated.
    /// </param>
    /// <param name="b">
    /// The second rational number for which the GCD is to be calculated.
    /// </param>
    /// <param name="max_den">
    /// The maximum allowable denominator for the resulting GCD. If the denominator of the GCD exceeds this value,
    /// the method will return <paramref name="default"/>.
    /// </param>
    /// <param name="default">
    /// A fallback rational value to return if the denominator of the GCD exceeds <paramref name="max_den"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Rational"/> representing the greatest common divisor of <paramref name="a"/> and <paramref name="b"/>.
    /// If the GCD has a denominator larger than <paramref name="max_den"/>, returns <paramref name="default"/>.
    /// </returns>
    public static Rational GreatestCommonDivisor(Rational a, Rational b, int max_den, Rational @default) => ffmpeg.av_gcd_q(a, b, max_den, @default);

    /// <summary>
    /// Returns the greatest common divisor (GCD) of two integers <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">
    /// The first integer for which the GCD is to be calculated.
    /// </param>
    /// <param name="b">
    /// The second integer for which the GCD is to be calculated.
    /// </param>
    /// <returns>
    /// The greatest common divisor of <paramref name="a"/> and <paramref name="b"/>.
    /// </returns>
    public static long GreatestCommonDivisor(long a, long b) => ffmpeg.av_gcd(a, b);

    /// <summary>
    /// Indicates whether the rational number is NaN (Not-a-Number).
    /// A rational number is considered NaN if both its numerator and denominator are zero.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the rational number is NaN, otherwise <c>false</c>.
    /// </returns>
    public bool IsNaN => Denominator == 0 && Numerator == 0;

    /// <summary>
    /// Indicates whether the rational number is positive infinity.
    /// A rational number is considered positive infinity if its numerator is greater than zero
    /// and its denominator is zero.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the rational number is positive infinity, otherwise <c>false</c>.
    /// </returns>
    public bool IsPositiveInfinity => Denominator == 0 && Numerator > 0;

    /// <summary>
    /// Indicates whether the rational number is negative infinity.
    /// A rational number is considered negative infinity if its numerator is less than zero
    /// and its denominator is zero.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the rational number is negative infinity, otherwise <c>false</c>.
    /// </returns>
    public bool IsNegativeInfinity => Denominator == 0 && Numerator < 0;



    /// <summary>
    /// Tries to parse a <see cref="Rational"/> from a string representation (in the form of a <see cref="ReadOnlySpan{T}"/>).
    /// If successful, the parsed <see cref="Rational"/> is returned in the out parameter.
    /// </summary>
    /// <param name="s">
    /// A <see cref="ReadOnlySpan{T}"/> representing the string to be parsed. The string should be in the format of either:
    /// - A rational number "numerator/denominator" (e.g., "1/2")
    /// - A decimal value (e.g., "3.14")
    /// </param>
    /// <param name="value">
    /// An output parameter that will contain the parsed <see cref="Rational"/> if parsing succeeds, or <see cref="Rational.NaN"/> if it fails.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the parsing was successful, otherwise <c>false</c>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> s, out Rational value) => TryParse(s, null, out value);

    /// <summary>
    /// Tries to parse a <see cref="Rational"/> from a string representation (in the form of a <see cref="ReadOnlySpan{T}"/>), 
    /// using a specified culture-specific formatting provider.
    /// </summary>
    /// <param name="s">
    /// A <see cref="ReadOnlySpan{T}"/> representing the string to be parsed. The string should be in the format of either:
    /// - A rational number "numerator/denominator" (e.g., "1/2")
    /// - A decimal value (e.g., "3.14")
    /// </param>
    /// <param name="provider">
    /// An optional <see cref="IFormatProvider"/> used to parse the string according to specific culture settings (e.g., decimal separators).
    /// </param>
    /// <param name="value">
    /// An output parameter that will contain the parsed <see cref="Rational"/> if parsing succeeds, or <see cref="Rational.NaN"/> if it fails.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the parsing was successful, otherwise <c>false</c>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Rational value)
    {
        provider ??= CultureInfo.InvariantCulture; // use invariant culture as default, since this video files are invariant culture
        value = Rational.NaN;
        ReadOnlySpan<char> delimiter = "/:";
        int delim = s.IndexOfAny(delimiter);

        // Try parsing as a rational number (numerator/denominator)
        if (delim >= 0)
        {
            if (!long.TryParse(s[..delim], System.Globalization.NumberStyles.Integer, provider, out long n))
                return false;
            if (!long.TryParse(s[(delim + 1)..], System.Globalization.NumberStyles.Integer, provider, out long d))
                return false;

            value = new(n, d); // If both parts are parsed successfully, return the Rational number
            return true;
        }
        else
        {
            // If no delimiter, try parsing as a double, since double can hold an integer
            if (!double.TryParse(s, System.Globalization.NumberStyles.Float, provider, out double d))
                return false;

            value = d; // If parsing succeeds, return the double as a Rational
            return true;
        }
    }

    /// <summary>
    /// Parses a <see cref="Rational"/> from a string representation (in the form of a <see cref="ReadOnlySpan{T}"/>).
    /// Throws a <see cref="FormatException"/> if parsing fails.
    /// </summary>
    /// <param name="s">
    /// A <see cref="ReadOnlySpan{T}"/> representing the string to be parsed. The string should be in the format of either:
    /// - A rational number "numerator/denominator" (e.g., "1/2")
    /// - A decimal value (e.g., "3.14")
    /// </param>
    /// <returns>
    /// A <see cref="Rational"/> representing the parsed value.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string cannot be parsed into a valid <see cref="Rational"/>.
    /// </exception>
    public static Rational Parse(ReadOnlySpan<char> s) => TryParse(s, out Rational r) ? r : throw new FormatException($"Invalid Rational format: '{s.ToString()}'");

    /// <summary>
    /// Parses a <see cref="Rational"/> from a string representation (in the form of a <see cref="ReadOnlySpan{T}"/>), 
    /// using a specified culture-specific formatting provider. Throws a <see cref="FormatException"/> if parsing fails.
    /// </summary>
    /// <param name="s">
    /// A <see cref="ReadOnlySpan{T}"/> representing the string to be parsed. The string should be in the format of either:
    /// - A rational number "numerator/denominator" (e.g., "1/2")
    /// - A decimal value (e.g., "3.14")
    /// </param>
    /// <param name="provider">
    /// An optional <see cref="IFormatProvider"/> used to parse the string according to specific culture settings (e.g., decimal separators).
    /// </param>
    /// <returns>
    /// A <see cref="Rational"/> representing the parsed value.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the string cannot be parsed into a valid <see cref="Rational"/>.
    /// </exception>
    public static Rational Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => TryParse(s, provider, out Rational r) ? r : throw new FormatException($"Invalid Rational format: '{s.ToString()}'");



    /// <summary>
    /// Returns a string representation of the <see cref="Rational"/> number.
    /// </summary>
    /// <returns>A string in the format "numerator/denominator".</returns>
    public override string ToString() => $"{Numerator}/{Denominator}";

    /// <summary>
    /// Converts this <see cref="Rational"/> instance to a <see cref="string"/> representation, 
    /// using the default format and no culture-specific formatting.
    /// </summary>
    /// <param name="format">
    /// A custom format string. If null or empty, the rational number is represented as "{numerator}/{denominator}". 
    /// </param>
    /// <returns>
    /// A string representing the rational number in the specified format.
    /// </returns>
    public string ToString(string? format) => ToString(format, null);

    /// <summary>
    /// Converts this <see cref="Rational"/> instance to a <see cref="string"/> representation, 
    /// using the default format and the specified culture-specific formatting.
    /// </summary>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information. If null, the default culture is used.
    /// </param>
    /// <returns>
    /// A string representing the rational number in the specified culture format.
    /// </returns>
    public string ToString(IFormatProvider? provider) => ToString(null, provider);

    /// <summary>
    /// Converts this <see cref="Rational"/> instance to a <see cref="string"/> representation.
    /// </summary>
    /// <param name="format">
    /// A custom format string that determines how the rational number is converted to a string.
    /// The format string can specify the following:
    /// <list type="bullet">
    ///     <item><description>If the first character is 'D' or 'd', followed by a double format (e.g., "D2"), 
    ///     the result will be a string representing the rational number as a double with the specified format. 
    ///     If the format string is just 'D' or 'd' (without any digits), the result will be a string representing 
    ///     the rational number as a double, using the default format for doubles.</description></item>
    ///     <item><description>If the first character is a delimiter (e.g., '/', '-', etc.), followed by an integer format (e.g., "F0"), 
    ///     the result will be a string formatted as "{numerator:format}{delimiter}{denominator:format}".</description></item>
    ///     <item><description>If the format string is empty or null, the method will return the rational number as a string in the format "{numerator}/{denominator}".</description></item>
    /// </list>
    /// </param>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information. 
    /// This is used to format both the numerator and denominator according to the specified format.
    /// </param>
    /// <returns>
    /// A string representing the rational number in the specified format.
    /// </returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        if (string.IsNullOrEmpty(format))
            return string.Format(formatProvider, "{0}/{1}", Numerator, Denominator);

        if (format[0] is 'D' or 'd')
            return ((double)this).ToString(format[1..], formatProvider);

        char delim = format[0];
        StringBuilder formatString = new StringBuilder("{0:").Append(format[1..])
            .Append("}")
            .Append(delim)
            .Append("{1:").Append(format[1..]).Append("}");

        return string.Format(formatProvider, formatString.ToString(), Numerator, Denominator);
    }


    #region IConvertable
    TypeCode IConvertible.GetTypeCode() => TypeCode.Object;
    bool IConvertible.ToBoolean(IFormatProvider provider) => !IsNaN && Numerator != 0;
    byte IConvertible.ToByte(IFormatProvider provider) => (byte)(Numerator / Denominator);
    char IConvertible.ToChar(IFormatProvider provider) => (char)(Numerator / Denominator);
    DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException();
    decimal IConvertible.ToDecimal(IFormatProvider provider) => Numerator / (decimal)Denominator;
    double IConvertible.ToDouble(IFormatProvider provider) => this;
    short IConvertible.ToInt16(IFormatProvider provider) => (short)(Numerator / Denominator);
    int IConvertible.ToInt32(IFormatProvider provider) => Numerator / Denominator;
    long IConvertible.ToInt64(IFormatProvider provider) => Numerator / Denominator;
    sbyte IConvertible.ToSByte(IFormatProvider provider) => (sbyte)(Numerator / Denominator);
    float IConvertible.ToSingle(IFormatProvider provider) => (float)Numerator / Denominator;
    string IConvertible.ToString(IFormatProvider provider) => ToString(null!, provider);
    object IConvertible.ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType((double)this, conversionType, provider);
    ushort IConvertible.ToUInt16(IFormatProvider provider) => (ushort)(Numerator / Denominator);
    uint IConvertible.ToUInt32(IFormatProvider provider) => (uint)(Numerator / Denominator);
    ulong IConvertible.ToUInt64(IFormatProvider provider) => (ulong)(Numerator / Denominator);
    #endregion
}


