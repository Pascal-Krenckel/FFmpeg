using FFmpeg.AutoGen;
using System.Text;

namespace FFmpeg.Audio;
/// <summary>
/// Defines a common interface for working with FFmpeg audio channel layouts.
/// </summary>
/// <remarks>
/// Implementations expose the properties and operations required to inspect,
/// compare, initialize, and manipulate audio channel layouts, regardless of
/// whether they own the underlying layout or reference an existing one.
///
/// <para>
/// A custom channel layout stores an explicit channel map instead of using one
/// of FFmpeg's predefined channel masks. Methods for accessing channel names
/// and channel identifiers are only valid for custom channel layouts.
/// </para>
/// </remarks>
public interface IChannelLayout : IEquatable<IChannelLayout>
{
    /// <summary>
    /// Gets the number of channels in the current channel layout.
    /// </summary>
    /// <value>The number of channels.</value>
    int Channels { get; }

    /// <summary>
    /// Gets the underlying <see cref="_AVChannelLayout"/> structure.
    /// </summary>
    /// <value>
    /// The underlying FFmpeg channel layout.
    /// </value>
    internal _AVChannelLayout Layout { get; }

    /// <summary>
    /// Indicates whether the current channel layout is valid.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the channel layout is valid; otherwise,
    /// <see langword="false"/>.
    /// </value>
    bool Valid { get; }

    /// <summary>
    /// Creates a copy of the current channel layout.
    /// </summary>
    /// <returns>
    /// A new <see cref="ChannelLayout"/> containing a copy of the current layout.
    /// </returns>
    ChannelLayout Copy();

    /// <summary>
    /// Initializes the current channel layout as a custom layout with the specified
    /// number of channels.
    /// </summary>
    /// <param name="nb">
    /// The number of channels in the custom layout.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="nb"/> is not a valid channel count.
    /// </exception>
    /// <exception cref="OutOfMemoryException">
    /// FFmpeg could not allocate memory for the custom channel map.
    /// </exception>
    void Init(int nb);

    /// <summary>
    /// Determines whether the current channel layout contains the specified audio
    /// channel.
    /// </summary>
    /// <param name="channelId">
    /// The channel identifier to search for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified channel is present in the layout;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For custom channel layouts, this method searches the custom channel map.
    /// For native and ambisonic layouts, it checks whether the corresponding
    /// channel is present in the layout's channel mask.
    /// </remarks>
    bool HasChannel(AudioChannel channelId);


    /// <summary>
    /// Gets the UTF-8 name assigned to a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based index of the channel.
    /// </param>
    /// <returns>
    /// The UTF-8 channel name.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid channel range.
    /// </exception>
    string GetCustomChannelName(int channelNumber);

    /// <summary>
    /// Sets the UTF-8 name assigned to a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based index of the channel.
    /// </param>
    /// <param name="name">
    /// The UTF-8 channel name. The encoded name must not exceed 16 bytes.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid channel range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The UTF-8 encoded channel name exceeds the 16-byte storage limit.
    /// </exception>
    void SetCustomChannelName(int channelNumber, ReadOnlySpan<char> name);

    /// <summary>
    /// Gets the channel identifier assigned to a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based index of the channel.
    /// </param>
    /// <returns>
    /// The channel identifier assigned to the specified channel.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid channel range.
    /// </exception>
    AudioChannel GetCustomChannelId(int channelNumber);


    /// <summary>
    /// Sets the channel identifier assigned to a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based index of the channel.
    /// </param>
    /// <param name="channel">
    /// The channel identifier to assign.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid channel range.
    /// </exception>
    void SetCustomChannelId(int channelNumber, AudioChannel channel);
}
