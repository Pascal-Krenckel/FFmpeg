namespace FFmpeg.Audio;

/// <summary>
/// Provides extension methods for working with <see cref="SampleFormat"/> values.
/// </summary>
public static class SampleExtensions
{
    /// <summary>
    /// Determines if the specified <see cref="SampleFormat"/> is planar.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> to check.</param>
    /// <returns><see langword="true"/> if the format is planar; otherwise, <see langword="false"/>.</returns>
    public static bool IsPlanar(this SampleFormat format)
        => ffmpeg.av_sample_fmt_is_planar((AutoGen._AVSampleFormat)format) == 1;

    /// <summary>
    /// Determines if the specified <see cref="SampleFormat"/> is packed.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> to check.</param>
    /// <returns><see langword="true"/> if the format is packed; otherwise, <see langword="false"/>.</returns>
    public static bool IsPacked(this SampleFormat format)
        => ffmpeg.av_sample_fmt_is_planar((AutoGen._AVSampleFormat)format) == 0;

    /// <summary>
    /// Converts the specified <see cref="SampleFormat"/> to its planar equivalent.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> to convert.</param>
    /// <returns>The planar <see cref="SampleFormat"/> equivalent of the specified format.</returns>
    public static SampleFormat AsPlanar(this SampleFormat format)
        => (SampleFormat)ffmpeg.av_get_planar_sample_fmt((AutoGen._AVSampleFormat)format);

    /// <summary>
    /// Converts the specified <see cref="SampleFormat"/> to its packed equivalent.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> to convert.</param>
    /// <returns>The packed <see cref="SampleFormat"/> equivalent of the specified format.</returns>
    public static SampleFormat AsPacked(this SampleFormat format)
        => (SampleFormat)ffmpeg.av_get_packed_sample_fmt((AutoGen._AVSampleFormat)format);

    /// <summary>
    /// Gets the number of bytes per sample for the specified <see cref="SampleFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> to get the byte size for.</param>
    /// <returns>The number of bytes per sample.</returns>
    public static int GetBytesPerSample(this SampleFormat format)
        => ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)format);

    /// <summary>
    /// Gets the number of bits per sample for the specified <see cref="SampleFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> to get the bit size for.</param>
    /// <returns>The number of bits per sample.</returns>
    public static int GetBitsPerSample(this SampleFormat format)
        => ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)format) * 8;

    /// <summary>
    /// Gets the number of bits per sample for the specified <see cref="AutoGen._AVCodecID"/>.
    /// </summary>
    /// <param name="codec">The <see cref="AutoGen._AVCodecID"/> to get the bit size for.</param>
    /// <returns>The number of bits per sample.</returns>
    public static int GetBitsPerSample(this AutoGen._AVCodecID codec)
        => ffmpeg.av_get_bits_per_sample(codec);

    /// <summary>
    /// Gets the name of the specified <see cref="SampleFormat"/> as a string.
    /// </summary>
    /// <param name="format">The <see cref="SampleFormat"/> to get the name for.</param>
    /// <returns>The name of the sample format.</returns>
    public static string GetName(this SampleFormat format)
        => ffmpeg.av_get_sample_fmt_name((AutoGen._AVSampleFormat)format);

    /// <summary>
    /// Validates that the generic type <typeparamref name="T"/> matches the specified <see cref="SampleFormat"/>.
    /// </summary>
    /// <typeparam name="T">The unmanaged type to validate against the sample format.</typeparam>
    /// <param name="sampleFormat">The <see cref="SampleFormat"/> to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if <typeparamref name="T"/> does not match the <see cref="SampleFormat"/> or if the format is unsupported.
    /// </exception>
    public static void ValidateType<T>(this SampleFormat sampleFormat) where T : unmanaged
    {
        switch (sampleFormat)
        {
            case SampleFormat.UInt8:
            case SampleFormat.UInt8Planar:
                if (typeof(T) != typeof(byte))
                    throw new ArgumentException($"The type {typeof(T)} does not match the sample format {sampleFormat}");
                break;

            case SampleFormat.Int16:
            case SampleFormat.Int16Planar:
                if (typeof(T) != typeof(short))
                    throw new ArgumentException($"The type {typeof(T)} does not match the sample format {sampleFormat}");
                break;

            case SampleFormat.Int32:
            case SampleFormat.Int32Planar:
                if (typeof(T) != typeof(int))
                    throw new ArgumentException($"The type {typeof(T)} does not match the sample format {sampleFormat}");
                break;

            case SampleFormat.Int64:
            case SampleFormat.Int64Planar:
                if (typeof(T) != typeof(long))
                    throw new ArgumentException($"The type {typeof(T)} does not match the sample format {sampleFormat}");
                break;

            case SampleFormat.Float32:
            case SampleFormat.Float32Planar:
                if (typeof(T) != typeof(float))
                    throw new ArgumentException($"The type {typeof(T)} does not match the sample format {sampleFormat}");
                break;

            case SampleFormat.Float64:
            case SampleFormat.Float64Planar:
                if (typeof(T) != typeof(double))
                    throw new ArgumentException($"The type {typeof(T)} does not match the sample format {sampleFormat}");
                break;

            case SampleFormat.None:
            case SampleFormat.__COUNT__:
            default:
                throw new ArgumentException($"Unsupported sample format: {sampleFormat}");
        }
    }

    /// <summary>
    /// Returns the .NET type that corresponds to the specified <see cref="SampleFormat"/>.
    /// </summary>
    /// <param name="sampleFormat">The <see cref="SampleFormat"/> to get the corresponding .NET type for.</param>
    /// <returns>The .NET type that matches the given sample format.</returns>
    /// <exception cref="ArgumentException">Thrown if the <see cref="SampleFormat"/> is not recognized or is unsupported.</exception>
    public static Type GetSampleFormatType(this SampleFormat sampleFormat) => sampleFormat switch
    {
        // Unsigned 8-bit integer format
        SampleFormat.UInt8 => typeof(byte),
        SampleFormat.UInt8Planar => typeof(byte),

        // Signed 16-bit integer format
        SampleFormat.Int16 => typeof(short),
        SampleFormat.Int16Planar => typeof(short),

        // Signed 32-bit integer format
        SampleFormat.Int32 => typeof(int),
        SampleFormat.Int32Planar => typeof(int),

        // Signed 64-bit integer format
        SampleFormat.Int64 => typeof(long),
        SampleFormat.Int64Planar => typeof(long),

        // 32-bit floating-point format
        SampleFormat.Float32 => typeof(float),
        SampleFormat.Float32Planar => typeof(float),

        // 64-bit floating-point (double) format
        SampleFormat.Float64 => typeof(double),
        SampleFormat.Float64Planar => typeof(double),

        // If the format is unsupported or invalid
        SampleFormat.None or SampleFormat.__COUNT__ or _ => throw new ArgumentException($"Unsupported sample format: {sampleFormat}")
    };
}






