using FFmpeg.AutoGen;

namespace FFmpeg.Audio;

/// <summary>
/// Provides extension methods for working with <see cref="SampleFormat"/> values.
/// </summary>
public static class SampleExtensions
{


    extension(SampleFormat format)
    {
        /// <summary>
        /// Determines if the specified <see cref="SampleFormat"/> is planar.
        /// </summary>
        /// <returns><see langword="true"/> if the format is planar; otherwise, <see langword="false"/>.</returns>
        public bool IsPlanar()
            => ffmpeg.av_sample_fmt_is_planar((AutoGen._AVSampleFormat)format) == 1;

        /// <summary>
        /// Determines if the specified <see cref="SampleFormat"/> is packed.
        /// </summary>
        /// <returns><see langword="true"/> if the format is packed; otherwise, <see langword="false"/>.</returns>
        public bool IsPacked()
            => ffmpeg.av_sample_fmt_is_planar((AutoGen._AVSampleFormat)format) == 0;

        /// <summary>
        /// Converts the specified <see cref="SampleFormat"/> to its planar equivalent.
        /// </summary>
        /// <returns>The planar <see cref="SampleFormat"/> equivalent of the specified format.</returns>
        public SampleFormat AsPlanar()
            => (SampleFormat)ffmpeg.av_get_planar_sample_fmt((AutoGen._AVSampleFormat)format);

        /// <summary>
        /// Converts the specified <see cref="SampleFormat"/> to its packed equivalent.
        /// </summary>
        /// <returns>The packed <see cref="SampleFormat"/> equivalent of the specified format.</returns>
        public SampleFormat AsPacked()
            => (SampleFormat)ffmpeg.av_get_packed_sample_fmt((AutoGen._AVSampleFormat)format);

        /// <summary>
        /// Gets the number of bytes per sample for the specified <see cref="SampleFormat"/>.
        /// </summary>
        /// <returns>The number of bytes per sample.</returns>
        public int GetBytesPerSample()
            => ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)format);

        /// <summary>
        /// Gets the number of bits per sample for the specified <see cref="SampleFormat"/>.
        /// </summary>
        /// <returns>The number of bits per sample.</returns>
        public int GetBitsPerSample()
            => ffmpeg.av_get_bytes_per_sample((AutoGen._AVSampleFormat)format) * 8;

        /// <summary>
        /// Gets the name of the specified <see cref="SampleFormat"/> as a string.
        /// </summary>
        /// <returns>The name of the sample format.</returns>
        public string GetName()
            => ffmpeg.av_get_sample_fmt_name((AutoGen._AVSampleFormat)format);
    }

    extension(AutoGen._AVCodecID codec)
    {
        /// <summary>
        /// Gets the number of bits per sample for the specified <see cref="AutoGen._AVCodecID"/>.
        /// </summary>
        /// <returns>The number of bits per sample.</returns>
        public int GetBitsPerSample()
            => ffmpeg.av_get_bits_per_sample(codec);
    }

    extension(SampleFormat sampleFormat)
    {
        /// <summary>
        /// Validates that the generic type <typeparamref name="T"/> matches the specified <see cref="SampleFormat"/>.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to validate against the sample format.</typeparam>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="T"/> does not match the <see cref="SampleFormat"/> or if the format is unsupported.
        /// </exception>
        public void ValidateType<T>() where T : unmanaged
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
        /// <returns>The .NET type that matches the given sample format.</returns>
        /// <exception cref="ArgumentException">Thrown if the <see cref="SampleFormat"/> is not recognized or is unsupported.</exception>
        public Type GetSampleFormatType() => sampleFormat switch
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

    extension(SampleFormat format)
    {
        /// <summary>
        /// Gets the <see cref="SampleFormat"/> from its name.
        /// </summary>
        /// <param name="name">The name of the sample format.</param>
        /// <returns>The corresponding <see cref="SampleFormat"/>.</returns>
        public static SampleFormat Parse(string name) => (SampleFormat)ffmpeg.av_get_sample_fmt(name);

        /// <summary>
        /// Gets the FFmpeg name of this sample format.
        /// </summary>
        /// <returns>
        /// The FFmpeg name of the sample format, or <see langword="null"/> if the sample format is not recognized.
        /// </returns>
        public string ToFFmpegString() => ffmpeg.av_get_sample_fmt_name((_AVSampleFormat)format);
    }
}






