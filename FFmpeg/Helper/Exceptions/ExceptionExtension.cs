namespace FFmpeg.Helper.Exceptions;

/// <summary>
/// Provides compatibility extensions for common exception guard methods
/// that are unavailable on older target frameworks.
/// </summary>
public static class ExceptionExtension
{
    extension(ObjectDisposedException)
    {
        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the specified
        /// disposed condition is <see langword="true"/>.
        /// </summary>
        /// <param name="disposed">
        /// A value indicating whether the object has been disposed.
        /// </param>
        /// <param name="obj">
        /// The disposed object. Its type name is used as the object name
        /// in the exception.
        /// </param>
        /// <exception cref="ObjectDisposedException">
        /// <paramref name="disposed"/> is <see langword="true"/>.
        /// </exception>
        public static void ThrowIfTrue(bool disposed, object? obj)
        {
            if (disposed)
            {
                string? typeName = obj?.GetType().FullName;

                throw new ObjectDisposedException(
                    typeName,
                    typeName == null
                        ? "The object has been disposed."
                        : $"The object of type '{typeName}' has been disposed.");
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the specified
        /// disposed condition is <see langword="true"/>.
        /// </summary>
        /// <param name="disposed">
        /// A value indicating whether the object has been disposed.
        /// </param>
        /// <param name="objectName">
        /// The name of the disposed object.
        /// </param>
        /// <exception cref="ObjectDisposedException">
        /// <paramref name="disposed"/> is <see langword="true"/>.
        /// </exception>
        public static void ThrowIfTrue(bool disposed, string? objectName)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    objectName,
                    objectName == null
                        ? "The object has been disposed."
                        : $"The object '{objectName}' has been disposed.");
            }
        }
    }

    extension(ArgumentNullException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the specified
        /// value is <see langword="null"/>.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">
        /// The name of the parameter represented by <paramref name="value"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public static void ThrowIfNull(object? value, string? paramName = null)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }
    }

    extension(ArgumentException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the specified string
        /// is <see langword="null"/> or empty.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="paramName">
        /// The name of the parameter represented by <paramref name="value"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is <see langword="null"/> or empty.
        /// </exception>
        public static void ThrowIfNullOrEmpty(
            string? value,
            string? paramName = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(
                    "The value cannot be null or empty.",
                    paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the specified string
        /// is <see langword="null"/>, empty, or consists only of white-space
        /// characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="paramName">
        /// The name of the parameter represented by <paramref name="value"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is <see langword="null"/>, empty, or
        /// consists only of white-space characters.
        /// </exception>
        public static void ThrowIfNullOrWhiteSpace(
            string? value,
            string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "The value cannot be null, empty, or consist only of white-space characters.",
                    paramName);
            }
        }
    }

    extension(ArgumentOutOfRangeException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified
        /// value is negative.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than zero.
        /// </exception>
        public static void ThrowIfNegative(
            int value,
            string? paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    "The value cannot be negative.");
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified
        /// value is negative.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than zero.
        /// </exception>
        public static void ThrowIfNegative(
            long value,
            string? paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    "The value cannot be negative.");
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified
        /// value is zero.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is zero.
        /// </exception>
        public static void ThrowIfZero(
            int value,
            string? paramName = null)
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    "The value cannot be zero.");
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified
        /// value is zero.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is zero.
        /// </exception>
        public static void ThrowIfZero(
            long value,
            string? paramName = null)
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    "The value cannot be zero.");
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified
        /// value is less than the specified minimum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="minimum">The minimum permitted value.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than <paramref name="minimum"/>.
        /// </exception>
        public static void ThrowIfLessThan(
            int value,
            int minimum,
            string? paramName = null)
        {
            if (value < minimum)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    $"The value must be greater than or equal to {minimum}.");
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified
        /// value is less than the specified minimum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="minimum">The minimum permitted value.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than <paramref name="minimum"/>.
        /// </exception>
        public static void ThrowIfLessThan(
            long value,
            long minimum,
            string? paramName = null)
        {
            if (value < minimum)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    $"The value must be greater than or equal to {minimum}.");
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified
        /// value is outside the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the value being validated.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="start">The inclusive lower bound of the valid range.</param>
        /// <param name="endExclusive">The exclusive upper bound of the valid range.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than <paramref name="start"/> or
        /// greater than or equal to <paramref name="endExclusive"/>.
        /// </exception>
        public static void ThrowIfOutOfRange<T>(
            T value,
            T start,
            T endExclusive,
            string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(start) < 0 ||
                value.CompareTo(endExclusive) >= 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    $"The value must be greater than or equal to {start} " +
                    $"and less than {endExclusive}.");
            }
        }
    }

    extension(IndexOutOfRangeException)
    {
        /// <summary>
        /// Throws an <see cref="IndexOutOfRangeException"/> if the specified
        /// index is outside the range of a collection with the specified length.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <param name="length">The length of the collection.</param>
        /// <param name="paramName">The name of the index parameter.</param>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> is less than zero or greater than or equal
        /// to <paramref name="length"/>.
        /// </exception>
        public static void ThrowIfOutOfRange(
            int index,
            int length,
            string? paramName = null)
        {
            if ((uint)index >= (uint)length)
            {
                throw new IndexOutOfRangeException(
                    $"Index '{paramName ?? "index"}' with value {index} " +
                    $"was outside the valid range [0, {length}).");
            }
        }

        /// <summary>
        /// Throws an <see cref="IndexOutOfRangeException"/> if the specified
        /// index is outside the specified range.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <param name="start">The inclusive lower bound of the valid range.</param>
        /// <param name="endExclusive">The exclusive upper bound of the valid range.</param>
        /// <param name="paramName">The name of the index parameter.</param>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> is less than <paramref name="start"/> or
        /// greater than or equal to <paramref name="endExclusive"/>.
        /// </exception>
        public static void ThrowIfOutOfRange(
            int index,
            int start,
            int endExclusive,
            string? paramName = null)
        {
            if (index < start || index >= endExclusive)
            {
                throw new IndexOutOfRangeException(
                    $"Index '{paramName ?? "index"}' with value {index} " +
                    $"was outside the valid range [{start}, {endExclusive}).");
            }
        }
    }
}

/// <summary>
/// Provides compatibility extensions for
/// <see cref="InvalidOperationException"/> guard methods that are unavailable
/// on older target frameworks.
/// </summary>
public static class InvalidOperationExceptionExtension
{
    extension(InvalidOperationException)
    {
        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the specified
        /// condition is <see langword="true"/>.
        /// </summary>
        /// <param name="condition">
        /// The condition that indicates whether the operation is invalid.
        /// </param>
        /// <param name="message">
        /// The message to include in the exception.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="condition"/> is <see langword="true"/>.
        /// </exception>
        public static void ThrowIfTrue(
            bool condition,
            string message)
        {
            if (condition)
                throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the specified
        /// condition is <see langword="false"/>.
        /// </summary>
        /// <param name="condition">
        /// The condition that indicates whether the operation is valid.
        /// </param>
        /// <param name="message">
        /// The message to include in the exception.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="condition"/> is <see langword="false"/>.
        /// </exception>
        public static void ThrowIfFalse(
            bool condition,
            string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}