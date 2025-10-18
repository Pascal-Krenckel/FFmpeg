using FFmpeg.AutoGen;

namespace FFmpeg.Audio;
/// <summary>
/// Defines an interface for working with channel layouts in audio processing.
/// </summary>
/// <remarks>
/// Implementations of this interface should provide the ability to access channel layout properties, 
/// validate the layout, and copy or initialize the layout as needed.
/// </remarks>
public interface IChannelLayout : IEquatable<IChannelLayout>
{
    /// <summary>
    /// Gets the number of channels in the current channel layout.
    /// </summary>
    /// <value>The number of channels.</value>
    int Channels { get; }

    /// <summary>
    /// Gets the underlying <see cref="_AVChannelLayout"/> structure without copying.
    /// </summary>
    /// <value>The <see cref="_AVChannelLayout"/> representing the layout.</value>
    internal _AVChannelLayout Layout { get; }

    /// <summary>
    /// Indicates whether the current channel layout is valid.
    /// </summary>
    /// <value><see langword="true"/> if the layout is valid; otherwise, <see langword="false"/>.</value>
    bool Valid { get; }

    /// <summary>
    /// Creates a copy of the current channel layout.
    /// </summary>
    /// <returns>A new <see cref="ChannelLayout"/> that is a copy of the current layout.</returns>
    ChannelLayout Copy();

    /// <summary>
    /// Initializes the current channel layout with a specified number of channels.
    /// </summary>
    /// <param name="nb">The number of channels to initialize the layout with.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="nb"/> is invalid.</exception>
    /// <exception cref="OutOfMemoryException">Thrown if there is insufficient memory to perform the operation.</exception>
    void Init(int nb);
}
