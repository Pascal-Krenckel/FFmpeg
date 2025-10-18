using FFmpeg.AutoGen;
using FFmpeg.Images;

namespace FFmpeg.HW;

/// <summary>
/// Represents a hardware configuration for a codec, wrapping around the FFmpeg _AVCodecHWConfig structure.
/// </summary>
public readonly unsafe struct CodecHWConfig : IEquatable<CodecHWConfig>
{
    /// <summary>
    /// Pointer to the FFmpeg _AVCodecHWConfig structure.
    /// </summary>
    internal readonly AutoGen._AVCodecHWConfig* config;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodecHWConfig"/> struct with the given _AVCodecHWConfig pointer.
    /// </summary>
    /// <param name="config">Pointer to the _AVCodecHWConfig structure.</param>
    internal CodecHWConfig(AutoGen._AVCodecHWConfig* config) => this.config = config;

    /// <summary>
    /// Gets the available hardware configurations for the specified codec.
    /// </summary>
    /// <param name="codec">The codec to retrieve hardware configurations for.</param>
    /// <returns>An array of <see cref="CodecHWConfig"/> representing the available hardware configurations.</returns>
    public static CodecHWConfig[] CodecHWConfigs(Codecs.Codec codec)
    {
        List<CodecHWConfig> configs = []; // Correct initialization
        AutoGen._AVCodecHWConfig* ptr;
        for (int i = 0; (ptr = ffmpeg.avcodec_get_hw_config(codec.codec, i)) != null; i++)
        {
            configs.Add(new(ptr));
        }
        return [.. configs]; // Correct array creation
    }

    /// <summary>
    /// Gets the pixel format of the hardware configuration.
    /// </summary>
    public PixelFormat PixelFormat => (PixelFormat)(config == null ? _AVPixelFormat.AV_PIX_FMT_NONE : config->pix_fmt);

    /// <summary>
    /// Gets the device type required for the hardware configuration.
    /// </summary>
    public DeviceType DeviceType => (DeviceType)(config == null ? _AVHWDeviceType.AV_HWDEVICE_TYPE_NONE : config->device_type);

    /// <summary>
    /// Gets the methods supported by the hardware configuration.
    /// </summary>
    public CodecHWConfigMethod Methods => config == null ? CodecHWConfigMethod.None : (CodecHWConfigMethod)config->methods;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CodecHWConfig config && Equals(config);

    /// <inheritdoc/>
    public bool Equals(CodecHWConfig other) => PixelFormat == other.PixelFormat && DeviceType == other.DeviceType && Methods == other.Methods;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(PixelFormat, DeviceType, Methods);

    /// <summary>
    /// Determines whether two <see cref="CodecHWConfig"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="CodecHWConfig"/> to compare.</param>
    /// <param name="right">The second <see cref="CodecHWConfig"/> to compare.</param>
    /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(CodecHWConfig left, CodecHWConfig right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="CodecHWConfig"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="CodecHWConfig"/> to compare.</param>
    /// <param name="right">The second <see cref="CodecHWConfig"/> to compare.</param>
    /// <returns><see langword="true"/> if the two instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(CodecHWConfig left, CodecHWConfig right) => !(left == right);
}

