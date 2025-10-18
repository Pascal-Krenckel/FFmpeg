using System.Runtime.InteropServices;

namespace FFmpeg.Codecs;

/// <summary>
/// Represents a profile in FFmpeg, providing information about the profile ID and its associated name.
/// </summary>
public readonly unsafe struct Profile : IEquatable<Profile>, IEquatable<AutoGen._AVProfile>
{
    private readonly int profile;
    private readonly string? name;

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> struct using a profile ID and a name pointer.
    /// </summary>
    /// <param name="profile">The profile ID, an integer that uniquely identifies the profile.</param>
    /// <param name="name">A pointer to the profile name in memory.</param>
    public Profile(int profile, byte* name)
    {
        this.profile = profile;
        this.name = name != null ? Marshal.PtrToStringUTF8((nint)name) : null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> struct using a profile ID and a profile name.
    /// </summary>
    /// <param name="profile">The profile ID.</param>
    /// <param name="name">The name of the profile, or <c>null</c> if not available.</param>
    public Profile(int profile, string? name)
    {
        this.profile = profile;
        this.name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> struct using an <see cref="AutoGen._AVProfile"/> object.
    /// </summary>
    /// <param name="profile">The <see cref="AutoGen._AVProfile"/> object containing profile information.</param>
    internal Profile(AutoGen._AVProfile profile) : this(profile.profile, profile.name) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> struct using a codec and a profile ID.
    /// </summary>
    /// <param name="codec">The codec associated with this profile.</param>
    /// <param name="profile">The profile ID.</param>
    public Profile(Codec codec, int profile)
    {
        this.profile = profile;
        name = ffmpeg.av_get_profile_name(codec.codec, profile);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> struct using a codec ID and a profile ID.
    /// </summary>
    /// <param name="codec">The codec ID associated with this profile.</param>
    /// <param name="profile">The profile ID.</param>
    public Profile(CodecID codec, int profile)
    {
        this.profile = profile;
        name = ffmpeg.avcodec_profile_name((AutoGen._AVCodecID)codec, profile);
    }

    /// <summary>
    /// Gets the profile ID.
    /// </summary>
    /// <value>The ID of the profile.</value>
    public int ProfileID => profile;

    /// <summary>
    /// Gets the name of the profile. Returns <c>null</c> if the profile name is not available.
    /// </summary>
    /// <value>The name of the profile, or <c>null</c> if not available.</value>
    public string? Name => name;

    /// <summary>
    /// Represents an unknown profile with an ID of <c>AV_PROFILE_UNKNOWN</c> and no associated name.
    /// </summary>
    /// <value>A <see cref="Profile"/> instance representing an unknown profile.</value>
    public static Profile Unknown => new(ffmpeg.AV_PROFILE_UNKNOWN, null as string);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Profile profile && Equals(profile);

    /// <inheritdoc />
    public bool Equals(Profile other) => profile == other.profile;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(profile);

    /// <inheritdoc />
    public bool Equals(AutoGen._AVProfile other) => other.profile == profile && Marshal.PtrToStringUTF8((nint)other.name) == name;

    /// <summary>
    /// Determines whether two <see cref="Profile"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Profile"/> instance.</param>
    /// <param name="right">The second <see cref="Profile"/> instance.</param>
    /// <returns><c>true</c> if the two profiles are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Profile left, Profile right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Profile"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Profile"/> instance.</param>
    /// <param name="right">The second <see cref="Profile"/> instance.</param>
    /// <returns><c>true</c> if the two profiles are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Profile left, Profile right) => !(left == right);

    /// <summary>
    /// Determines whether a <see cref="Profile"/> instance and an <see cref="AutoGen._AVProfile"/> instance are equal.
    /// </summary>
    /// <param name="left">The <see cref="AutoGen._AVProfile"/> instance.</param>
    /// <param name="right">The <see cref="Profile"/> instance.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(AutoGen._AVProfile left, Profile right) => right.Equals(left);

    /// <summary>
    /// Determines whether a <see cref="Profile"/> instance and an <see cref="AutoGen._AVProfile"/> instance are not equal.
    /// </summary>
    /// <param name="left">The <see cref="AutoGen._AVProfile"/> instance.</param>
    /// <param name="right">The <see cref="Profile"/> instance.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(AutoGen._AVProfile left, Profile right) => !(left == right);

    /// <summary>
    /// Returns the profile name or "UNKNOWN" if the name is not available.
    /// </summary>
    /// <returns>A string representing the profile name, or "UNKNOWN" if not available.</returns>
    public override string ToString() => name ?? "UNKNOWN";
}
