using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Codecs;
/// <summary>
/// Represents a codec tag, combining a codec ID and a FourCC (Four Character Code) tag.
/// </summary>
public readonly struct CodecTag : IEquatable<CodecTag>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodecTag"/> struct with the specified codec ID and FourCC tag.
    /// </summary>
    /// <param name="id">The codec identifier.</param>
    /// <param name="tag">The FourCC tag associated with the codec.</param>
    public CodecTag(CodecID id, FourCC tag)
    {
        ID = id;
        Tag = tag;
    }

    /// <summary>
    /// Gets the codec identifier.
    /// </summary>
    public CodecID ID { get; init; }

    /// <summary>
    /// Gets the FourCC tag associated with the codec.
    /// </summary>
    public FourCC Tag { get; init; }

    /// <summary>
    /// Implicitly converts an <see cref="AutoGen._AVCodecTag"/> to a <see cref="CodecTag"/>.
    /// </summary>
    /// <param name="tag">The <see cref="AutoGen._AVCodecTag"/> to convert.</param>
    public static implicit operator CodecTag(AutoGen._AVCodecTag tag)
        => new((CodecID)tag.id, tag.tag);

    /// <summary>
    /// Implicitly converts a <see cref="CodecTag"/> to an <see cref="AutoGen._AVCodecTag"/>.
    /// </summary>
    /// <param name="tag">The <see cref="CodecTag"/> to convert.</param>
    public static implicit operator AutoGen._AVCodecTag(CodecTag tag)
        => new() { id = (AutoGen._AVCodecID)tag.ID, tag = tag.Tag };

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is CodecTag tag && Equals(tag);

    /// <summary>
    /// Determines whether the specified <see cref="CodecTag"/> is equal to the current <see cref="CodecTag"/>.
    /// </summary>
    /// <param name="other">The <see cref="CodecTag"/> to compare with the current <see cref="CodecTag"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="CodecTag"/> is equal to the current <see cref="CodecTag"/>; otherwise, <see langword="false"/>.</returns>
    public bool Equals(CodecTag other)
        => ID == other.ID && Tag.Equals(other.Tag);

    /// <summary>
    /// Returns a hash code for the current <see cref="CodecTag"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="CodecTag"/>.</returns>
    public override int GetHashCode()
        => HashCode.Combine(ID, Tag);

    /// <summary>
    /// Determines whether two specified <see cref="CodecTag"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="CodecTag"/> to compare.</param>
    /// <param name="right">The second <see cref="CodecTag"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="CodecTag"/> instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(CodecTag left, CodecTag right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two specified <see cref="CodecTag"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="CodecTag"/> to compare.</param>
    /// <param name="right">The second <see cref="CodecTag"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="CodecTag"/> instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(CodecTag left, CodecTag right)
        => !(left == right);

    /// <summary>
    /// Returns a string that represents the current <see cref="CodecTag"/>.
    /// </summary>
    /// <returns>A string that represents the current <see cref="CodecTag"/>, in the format "ID/Tag".</returns>
    public override string ToString()
        => $"{ID}/{Tag}";
}
