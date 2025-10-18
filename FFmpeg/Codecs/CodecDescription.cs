using FFmpeg.AutoGen;
using FFmpeg.Utils;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace FFmpeg.Codecs;
/// <summary>
/// Represents a codec descriptor with various properties.
/// <para>
/// This struct is a readonly wrapper around an <see cref="AutoGen._AVCodecDescriptor"/> pointer, providing a way to access codec descriptor properties in a safe manner.
/// </para>
/// </summary>
public readonly unsafe struct CodecDescription : IEquatable<CodecDescription>
{
    private readonly _AVCodecDescriptor* descriptor;

    internal CodecDescription(_AVCodecDescriptor* descriptor) => this.descriptor = descriptor;

    /// <summary>
    /// Retrieves a <see cref="CodecDescription"/> for the specified codec ID.
    /// </summary>
    /// <param name="codecID">The codec ID to get the descriptor for.</param>
    /// <returns>
    /// A <see cref="CodecDescription"/> instance representing the codec descriptor.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="codecID"/> does not correspond to a valid codec descriptor.
    /// </exception>
    public static CodecDescription GetCodecDescriptor(CodecID codecID)
        => TryGetCodecDescriptor(codecID, out CodecDescription descriptor)
            ? descriptor
            : throw new ArgumentException(nameof(codecID));

    /// <summary>
    /// Retrieves a <see cref="CodecDescription"/> for the specified codec name.
    /// </summary>
    /// <param name="name">The name of the codec to get the descriptor for.</param>
    /// <returns>
    /// A <see cref="CodecDescription"/> instance representing the codec descriptor.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="name"/> does not correspond to a valid codec descriptor.
    /// </exception>
    public static CodecDescription GetCodecDescriptor(string name)
        => TryGetCodecDescriptor(name, out CodecDescription descriptor)
            ? descriptor
            : throw new ArgumentException(nameof(name));

    /// <summary>
    /// Retrieves a <see cref="CodecDescription"/> for the specified <see cref="Codec"/>.
    /// </summary>
    /// <param name="codec">The codec to get the descriptor for.</param>
    /// <returns>
    /// A <see cref="CodecDescription"/> instance representing the codec descriptor.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="codec"/> does not correspond to a valid codec descriptor.
    /// </exception>
    public static CodecDescription GetCodecDescriptor(Codec codec)
        => TryGetCodecDescriptor(codec, out CodecDescription descriptor)
            ? descriptor
            : throw new ArgumentException(nameof(codec));

    /// <summary>
    /// Attempts to retrieve a <see cref="CodecDescription"/> for the specified codec ID.
    /// </summary>
    /// <param name="codecID">The codec ID to get the descriptor for.</param>
    /// <param name="descriptor">
    /// When this method returns, contains the <see cref="CodecDescription"/> instance if the codec ID was valid; otherwise, <see cref="default"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the descriptor was successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetCodecDescriptor(CodecID codecID, out CodecDescription descriptor)
    {
        _AVCodecDescriptor* desc = ffmpeg.avcodec_descriptor_get((_AVCodecID)codecID);
        if (desc == null)
        {
            descriptor = default;
            return false;
        }
        else
        {
            descriptor = new(desc);
            return true;
        }
    }

    /// <summary>
    /// Attempts to retrieve a <see cref="CodecDescription"/> for the specified <see cref="Codec"/>.
    /// </summary>
    /// <param name="codec">The codec to get the descriptor for.</param>
    /// <param name="descriptor">
    /// When this method returns, contains the <see cref="CodecDescription"/> instance if the codec was valid; otherwise, <see cref="default"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the descriptor was successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetCodecDescriptor(Codec codec, out CodecDescription descriptor)
        => TryGetCodecDescriptor(codec.CodecID, out descriptor);

    /// <summary>
    /// Attempts to retrieve a <see cref="CodecDescription"/> for the specified codec name.
    /// </summary>
    /// <param name="name">The name of the codec to get the descriptor for.</param>
    /// <param name="descriptor">
    /// When this method returns, contains the <see cref="CodecDescription"/> instance if the name was valid; otherwise, <see cref="default"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the descriptor was successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetCodecDescriptor(string name, out CodecDescription descriptor)
    {
        _AVCodecDescriptor* desc = ffmpeg.avcodec_descriptor_get_by_name(name);
        if (desc == null)
        {
            descriptor = default;
            return false;
        }
        else
        {
            descriptor = new(desc);
            return true;
        }
    }

    /// <summary>
    /// Retrieves an array of all available codec descriptors.
    /// <para>
    /// This method iterates through all codec descriptors registered with FFmpeg and collects them into an array.
    /// </para>
    /// </summary>
    /// <returns>
    /// An array of <see cref="CodecDescription"/> instances representing all available codec descriptors.
    /// </returns>
    public static CodecDescription[] GetAllCodecDescriptors()
    {
        List<CodecDescription> descriptors = [];
        _AVCodecDescriptor* desc = null;
        while ((desc = ffmpeg.avcodec_descriptor_next(desc)) != null)
            descriptors.Add(new(desc));
        return [.. descriptors];
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="CodecDescription"/>.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with the current <see cref="CodecDescription"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current <see cref="CodecDescription"/>; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is CodecDescription descriptor && Equals(descriptor);

    /// <summary>
    /// Determines whether the specified <see cref="CodecDescription"/> is equal to the current <see cref="CodecDescription"/>.
    /// </summary>
    /// <param name="other">
    /// The <see cref="CodecDescription"/> to compare with the current instance.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="CodecDescription"/> is equal to the current instance; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(CodecDescription other)
        => EqualityComparer<nint>.Default.Equals((nint)descriptor, (nint)other.descriptor);

    /// <summary>
    /// Serves as a hash function for the <see cref="CodecDescription"/> type.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="CodecDescription"/>.
    /// </returns>
    public override int GetHashCode()
        => HashCode.Combine((nint)descriptor);

    /// <summary>
    /// Gets the codec ID, which is a value from the <see cref="CodecID"/> enumeration.
    /// </summary>
    /// <value>
    /// The codec ID for this codec descriptor.
    /// </value>
    public CodecID Id => (CodecID)descriptor->id;

    /// <summary>
    /// Gets the media type of the codec, which is a value from the <see cref="MediaType"/> enumeration.
    /// </summary>
    /// <value>
    /// The media type for this codec descriptor.
    /// </value>
    public MediaType Type => (MediaType)descriptor->type;

    /// <summary>
    /// Gets the name of the codec.
    /// <para>
    /// This name is non-empty and unique for each codec descriptor. It is intended for identification purposes.
    /// </para>
    /// </summary>
    /// <value>
    /// The name of the codec.
    /// </value>
    public string Name => Marshal.PtrToStringUTF8((nint)descriptor->name) ?? string.Empty;

    /// <summary>
    /// Gets the more descriptive name for this codec.
    /// <para>
    /// This name provides additional details about the codec and may be null if not available.
    /// </para>
    /// </summary>
    /// <value>
    /// The more descriptive name of the codec, or null if not available.
    /// </value>
    public string? LongName => Marshal.PtrToStringUTF8((nint)descriptor->long_name);

    /// <summary>
    /// Gets the codec properties, which is a combination of flags from the <see cref="CodecProperties"/> enumeration.
    /// </summary>
    /// <value>
    /// The properties of the codec, represented as a bit field combining various flags.
    /// </value>
    public CodecProperties Properties => (CodecProperties)descriptor->props;

    /// <summary>
    /// Gets the MIME types associated with the codec.
    /// <para>
    /// This property returns an array of MIME types that are associated with the codec. If no MIME types are available, it returns an empty array.
    /// The first item in the array is always the preferred MIME type if available.
    /// </para>
    /// </summary>
    /// <value>
    /// An array of MIME types associated with the codec, or an empty array if none are available.
    /// </value>
    public string[] MimeTypes
    {
        get
        {
            if (descriptor->mime_types == null) return [];
            List<string> mimeTypes = [];
            for (byte** ptr = descriptor->mime_types; *ptr != null; ptr++)
                mimeTypes.Add(Marshal.PtrToStringUTF8((nint)(*ptr)));
            return [.. mimeTypes];
        }
    }

    /// <summary>
    /// Gets the array of profiles recognized for this codec.
    /// <para>
    /// This property returns a read-only collection of profiles associated with the codec. Profiles provide additional information about codec capabilities and configurations.
    /// If no profiles are available, it returns an empty collection.
    /// </para>
    /// </summary>
    /// <value>
    /// A <see cref="ReadOnlyCollection{Profile}"/> containing the profiles recognized for this codec, or an empty collection if none are available.
    /// </value>
    public ReadOnlyCollection<Profile> Profiles
    {
        get
        {
            if (descriptor->profiles == null)
                return new ReadOnlyCollection<Profile>(Array.Empty<Profile>());

            List<Profile> profiles = [];
            for (int i = 0; descriptor->profiles[i].profile != ffmpeg.AV_PROFILE_UNKNOWN; i++)
                profiles.Add(new(descriptor->profiles[i]));
            return new ReadOnlyCollection<Profile>(profiles);
        }
    }


    /// <summary>
    /// Determines whether two <see cref="CodecDescription"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="CodecDescription"/> instance.</param>
    /// <param name="right">The second <see cref="CodecDescription"/> instance.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(CodecDescription left, CodecDescription right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="CodecDescription"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="CodecDescription"/> instance.</param>
    /// <param name="right">The second <see cref="CodecDescription"/> instance.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(CodecDescription left, CodecDescription right) => !(left == right);

    /// <summary>
    /// Returns a string representation of the <see cref="CodecDescription"/>.
    /// </summary>
    /// <returns>The long name of the codec descriptor, providing a more descriptive name for the codec.</returns>
    public override string ToString() => LongName ?? "(null)";
}

