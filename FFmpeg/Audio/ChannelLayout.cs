using FFmpeg.AutoGen;
using FFmpeg.Exceptions;
using FFmpeg.Utils;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFmpeg.Audio;
/// <summary>
/// Represents a channel layout, which defines the arrangement of audio channels in a multi-channel audio format.
/// This class provides functionality to interact with and manipulate audio channel layouts using FFmpeg's <see cref="AutoGen._AVChannelLayout"/> structure.
/// </summary>
/// <remarks>
/// A channel layout describes the number and arrangement of audio channels (e.g., stereo, 5.1 surround sound).
/// The <see cref="ChannelLayout"/> class allows you to create, initialize, and manage channel layouts, and to retrieve standard layouts available in FFmpeg.
/// </remarks>
public unsafe class ChannelLayout : IEquatable<ChannelLayout>, IChannelLayout, IEquatable<ChannelLayout_ref>, IDisposable
{
    internal AutoGen._AVChannelLayout layout;

    /// <summary>
    /// Gets the underlying FFmpeg channel layout structure.
    /// </summary>
    AutoGen._AVChannelLayout IChannelLayout.Layout => layout;
    internal AutoGen._AVChannelLayout Layout => layout;


    /// <summary>
    /// Gets the number of channels in the current channel layout.
    /// </summary>
    public int Channels => layout.nb_channels;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelLayout"/> class using an existing FFmpeg channel layout.
    /// </summary>
    /// <param name="layout">The FFmpeg channel layout to be used.</param>
    internal ChannelLayout(AutoGen._AVChannelLayout layout) => this.layout = layout;

    /// <summary>
    /// Determines whether the current channel layout is valid.
    /// </summary>
    /// <returns><see langword="true"/> if the layout is valid, otherwise <see langword="false"/>.</returns>
    public bool Valid
    {
        get
        {
            AutoGen._AVChannelLayout l = layout;
            return ffmpeg.av_channel_layout_check(&l) != 0;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelLayout"/> class with no predefined layout.
    /// </summary>
    public ChannelLayout() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelLayout"/> class using a channel mask.
    /// </summary>
    /// <param name="mask">The channel mask used to initialize the layout.</param>
    /// <exception cref="ArgumentException">Thrown when the channel layout could not be initialized from the mask.</exception>
    public ChannelLayout(ulong mask)
    {
        AutoGen._AVChannelLayout l;
        if (ffmpeg.av_channel_layout_from_mask(&l, mask) < 0)
            throw new ArgumentException("Failed to initialize the channel layout from the provided mask.");
        layout = l;
    }

    /// <summary>
    /// Retrieves all standard channel layouts available in FFmpeg.
    /// </summary>
    /// <returns>A read-only list of <see cref="ChannelLayout"/> objects representing the available layouts.</returns>
    /// <exception cref="OutOfMemoryException">Thrown when there is insufficient memory to copy a channel layout.</exception>
    public static IReadOnlyList<ChannelLayout> GetAllChannelLayouts()
    {
        List<ChannelLayout> layouts = [];
        void* opaque = null;
        AutoGen._AVChannelLayout* layout;

        // Loop through and retrieve all standard channel layouts from FFmpeg.
        while ((layout = ffmpeg.av_channel_layout_standard(&opaque)) != null)
        {
            AutoGen._AVChannelLayout l;

            // Copy the layout and throw if memory allocation fails.
            if (ffmpeg.av_channel_layout_copy(&l, layout) == AVResult32.OutOfMemory)
                throw new OutOfMemoryException("Failed to allocate memory for a channel layout.");

            layouts.Add(new ChannelLayout(l));
        }

        return layouts;
    }

    /// <summary>
    /// Creates a deep copy of the current <see cref="ChannelLayout"/> object.
    /// </summary>
    /// <returns>A new <see cref="ChannelLayout"/> instance that is a deep copy of the current one.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if there is insufficient memory to copy the channel layout.</exception>
    public ChannelLayout Copy()
    {

        AutoGen._AVChannelLayout l;
        AutoGen._AVChannelLayout src = layout;

        // Copy the current layout and throw if an error occurs.
        AVResult32 res = ffmpeg.av_channel_layout_copy(&l, &src);
        res.ThrowIfError();

        return new ChannelLayout(l);

    }

    /// <summary>
    /// Initializes the current <see cref="ChannelLayout"/> with a custom number of channels.
    /// </summary>
    /// <param name="nb">The number of channels to initialize the layout with.</param>
    /// <exception cref="OutOfMemoryException">Thrown if memory allocation fails during initialization.</exception>
    /// <exception cref="ArgumentException">Thrown if the number of channels provided is invalid.</exception>
    public void Init(int nb)
    {

        AutoGen._AVChannelLayout l = layout;
        // Initialize the layout with a custom number of channels and handle errors.
        ffmpeg.av_channel_layout_uninit(&l);
        int res = ffmpeg.av_channel_layout_custom_init(&l, nb);
        if (res == AVResult32.OutOfMemory)
            throw new OutOfMemoryException("Insufficient memory to initialize the channel layout.");
        else if (res == AVResult32.InvalidArgument)
            throw new ArgumentException("Invalid number of channels specified for the channel layout.");
        layout = l;
    }

    /// <summary>
    /// Determines whether the current channel layout contains the specified audio channel.
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
    /// For native channel layouts, it checks whether the corresponding channel bit
    /// is set in the channel mask.
    /// </remarks>
    public bool HasChannel(AudioChannel channelId)
    {
        if (Channels <= 0)
            return false;

        if (layout.order == _AVChannelOrder.AV_CHANNEL_ORDER_CUSTOM)
        {
            if (layout.u.map == null)
                return false;

            for (int i = 0; i < Channels; i++)
            {
                if (layout.u.map[i].id == (_AVChannel)channelId)
                    return true;
            }

            return false;
        }

        return ((layout.u.mask >> (int)channelId) & 1) != 0;
    }

    /// <summary>
    /// Gets the UTF-8 name of a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based channel index.
    /// </param>
    /// <returns>
    /// The channel name.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid range.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    public string GetCustomChannelName(int channelNumber)
    {
        ValidateCustomChannel(channelNumber);

        Span<byte> bytes = new((byte*)&layout.u.map[channelNumber].name, 16);
        int length = bytes.IndexOf((byte)0);
        if (length < 0)
            length = bytes.Length;

        return Encoding.UTF8.GetString(bytes[..length]);
    }

    /// <summary>
    /// Sets the UTF-8 name of a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based channel index.
    /// </param>
    /// <param name="name">
    /// The channel name. The encoded UTF-8 name must not exceed 16 bytes.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is too long to fit into the fixed-size channel name.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    public void SetCustomChannelName(int channelNumber, ReadOnlySpan<char> name)
    {
        ValidateCustomChannel(channelNumber);

        Span<byte> bytes = new((byte*)&layout.u.map[channelNumber].name, 16);
        bytes.Clear();

        if (Encoding.UTF8.GetByteCount(name) > bytes.Length)
            throw new ArgumentException("The UTF-8 encoded channel name must not exceed 16 bytes.", nameof(name));

        _ = Encoding.UTF8.GetBytes(name, bytes);
    }

    /// <summary>
    /// Gets the channel identifier for a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based channel index.
    /// </param>
    /// <returns>
    /// The channel identifier.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid range.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    public AudioChannel GetCustomChannelId(int channelNumber)
    {
        ValidateCustomChannel(channelNumber);
        return (AudioChannel)layout.u.map[channelNumber].id;
    }

    /// <summary>
    /// Sets the channel identifier for a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based channel index.
    /// </param>
    /// <param name="channel">
    /// The channel identifier.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid range.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    public void SetCustomChannelId(int channelNumber, AudioChannel channel)
    {
        ValidateCustomChannel(channelNumber);
        layout.u.map[channelNumber].id = (_AVChannel)channel;
    }

    private void ValidateCustomChannel(int channelNumber)
    {

        if (layout.order != _AVChannelOrder.AV_CHANNEL_ORDER_CUSTOM)
            throw new InvalidOperationException("The channel layout is not a custom channel layout.");

        if ((uint)channelNumber >= (uint)Channels)
            throw new ArgumentOutOfRangeException(nameof(channelNumber));
    }


    /// <summary>
    /// Disposes of the resources used by the <see cref="ChannelLayout"/>, uninitializing the layout.
    /// </summary>
    public void Dispose()
    {

        _AVChannelLayout layout = this.layout;
        ffmpeg.av_channel_layout_uninit(&layout);
        this.layout = layout;
    }

    /// <inheritdoc />
    ~ChannelLayout()
    {
        Dispose();
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="ChannelLayout"/>.
    /// </summary>
    /// <returns>A description of the channel layout as a string, or an empty string if an error occurs.</returns>
    public override string ToString()
    {

        AutoGen._AVChannelLayout layout = this.layout;

        // Get the required size for the layout description.
        AVResult32 res = ffmpeg.av_channel_layout_describe(&layout, null, 0);


        if (res.IsError)
            return string.Empty;

        byte[]? buffer = null;
        if (res > 256)
            buffer = ArrayPool<byte>.Shared.Rent(res);
        Span<byte> data = buffer ?? (stackalloc byte[res]);

        // Allocate a buffer for the description and retrieve it.
        fixed (byte* chars = data)
        {
            res = ffmpeg.av_channel_layout_describe(&layout, chars, (ulong)(int)res);
            string ret = res.IsError ? string.Empty : Encoding.UTF8.GetString(chars, res - 1);
            if (buffer != null)
                ArrayPool<byte>.Shared.Return(buffer);
            return ret;
        }
    }



    /// <summary>
    /// Tries to parse a <see cref="ChannelLayout"/> from a string representation.
    /// </summary>
    /// <param name="str">The string representation of the channel layout.</param>
    /// <param name="layout">When this method returns, contains the parsed <see cref="ChannelLayout"/> if the parse operation succeeded; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the parse operation succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method attempts to create a <see cref="ChannelLayout"/> from the given string. 
    /// If parsing is successful, the <paramref name="layout"/> will contain the result; otherwise, it will be <see langword="null"/>.
    /// </remarks>
    public static bool TryParse(string str, [NotNullWhen(true)] out ChannelLayout? layout)
    {
        AutoGen._AVChannelLayout l;
        int res = ffmpeg.av_channel_layout_from_string(&l, str);
        if (res < 0)
        {
            layout = null;
            return false;
        }
        layout = new ChannelLayout(l);
        return true;
    }

    /// <summary>
    /// Creates a new <see cref="ChannelLayout"/> by copying an existing FFmpeg channel layout.
    /// </summary>
    /// <param name="layout">The FFmpeg channel layout to copy from.</param>
    /// <returns>A new <see cref="ChannelLayout"/> instance that is a copy of the provided layout.</returns>
    /// <remarks>
    /// This method creates a new <see cref="ChannelLayout"/> by copying the provided FFmpeg channel layout structure.
    /// </remarks>
    internal static ChannelLayout CopyFrom(AutoGen._AVChannelLayout layout)
    {
        AutoGen._AVChannelLayout l;
        FFmpegException.ThrowIfError(ffmpeg.av_channel_layout_copy(&l, &layout));
        return new ChannelLayout(l);
    }

    /// <summary>
    /// Parses a <see cref="ChannelLayout"/> from a string representation.
    /// </summary>
    /// <param name="str">The string representation of the channel layout.</param>
    /// <returns>The parsed <see cref="ChannelLayout"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the string cannot be parsed into a valid channel layout.</exception>
    /// <remarks>
    /// This method is similar to <see cref="TryParse(string, out ChannelLayout)"/> but throws an exception if parsing fails.
    /// </remarks>
    public static ChannelLayout Parse(string str) => TryParse(str, out ChannelLayout? layout) ? layout : throw new ArgumentException("Invalid channel layout string.");

    /// <summary>
    /// Creates a <see cref="ChannelLayout"/> instance representing a mono layout.
    /// </summary>
    /// <returns>A <see cref="ChannelLayout"/> instance for a mono channel layout.</returns>
    public static ChannelLayout CreateMono() => new(ffmpeg.AV_CH_LAYOUT_MONO);

    /// <summary>
    /// Creates a <see cref="ChannelLayout"/> instance representing a stereo layout.
    /// </summary>
    /// <returns>A <see cref="ChannelLayout"/> instance for a stereo channel layout.</returns>
    public static ChannelLayout CreateStereo() => new(ffmpeg.AV_CH_LAYOUT_STEREO);

    /// <summary>
    /// Creates a <see cref="ChannelLayout"/> instance representing a stereo downmix layout.
    /// </summary>
    /// <returns>A <see cref="ChannelLayout"/> instance for a stereo downmix channel layout.</returns>
    public static ChannelLayout CreateStereoDownMix() => new(ffmpeg.AV_CH_LAYOUT_STEREO_DOWNMIX);

    /// <summary>
    /// Creates a <see cref="ChannelLayout"/> instance representing a surround sound layout.
    /// </summary>
    /// <returns>A <see cref="ChannelLayout"/> instance for a surround sound channel layout.</returns>
    public static ChannelLayout CreateSurround() => new(ffmpeg.AV_CH_LAYOUT_SURROUND);

    /// <summary>
    /// Determines whether the current <see cref="ChannelLayout"/> is equal to another <see cref="ChannelLayout"/>.
    /// </summary>
    /// <param name="other">The other <see cref="ChannelLayout"/> to compare with.</param>
    /// <returns><see langword="true"/> if the current layout is equal to the other layout; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ChannelLayout? other)
    {
        if (ReferenceEquals(other, this))
            return true;
        if (null == other)
            return false;
        AutoGen._AVChannelLayout left = layout;
        AutoGen._AVChannelLayout right = other.layout;
        return ffmpeg.av_channel_layout_compare(&left, &right) == 0;
    }

    /// <summary>
    /// Determines whether the current <see cref="ChannelLayout"/> is equal to another <see cref="IChannelLayout"/>.
    /// </summary>
    /// <param name="other">The other <see cref="IChannelLayout"/> to compare with.</param>
    /// <returns><see langword="true"/> if the current layout is equal to the other layout; otherwise, <see langword="false"/>.</returns>
    public bool Equals(IChannelLayout? other)
    {
        if (null == other)
            return false;
        AutoGen._AVChannelLayout left = layout;
        AutoGen._AVChannelLayout right = other.Layout;
        return ffmpeg.av_channel_layout_compare(&left, &right) == 0;
    }

    /// <summary>
    /// Determines whether the current <see cref="ChannelLayout"/> is equal to another <see cref="ChannelLayout_ref"/>.
    /// </summary>
    /// <param name="other">The other <see cref="ChannelLayout_ref"/> to compare with.</param>
    /// <returns><see langword="true"/> if the current layout is equal to the other layout; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ChannelLayout_ref other)
    {
        AutoGen._AVChannelLayout left = layout;
        return ffmpeg.av_channel_layout_compare(&left, other.ptr) == 0;
    }

    /// <summary>
    /// Gets the hash code for the current <see cref="ChannelLayout"/> instance.
    /// </summary>
    /// <returns>The hash code for the current instance.</returns>
    public override int GetHashCode() => HashCode.Combine(Channels);

    /// <summary>
    /// Determines whether the current <see cref="ChannelLayout"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the other object; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is ChannelLayout_ref ptr
            ? Equals(ptr)
            : obj is ChannelLayout layout ? Equals(layout) : obj is IChannelLayout iLayout && Equals(iLayout);

    /// <summary>
    /// Creates a <see cref="ChannelLayout"/> instance using FFmpeg's default channel layout
    /// for the specified number of channels.
    /// </summary>
    /// <param name="channels">
    /// The number of channels in the default layout.
    /// </param>
    /// <returns>
    /// A <see cref="ChannelLayout"/> representing FFmpeg's default layout for the specified
    /// channel count.
    /// </returns>
    /// <remarks>
    /// FFmpeg selects the default layout associated with the specified number of channels.
    /// For example, a channel count of 1 creates a mono layout, while a channel count of 2
    /// creates a stereo layout.
    /// </remarks>
    public static ChannelLayout CreateDefault(int channels)
    {
        AutoGen._AVChannelLayout p = new();
        ffmpeg.av_channel_layout_default(&p, channels);
        return new(p);
    }

    /// <summary>
    /// Defines the equality operator for <see cref="ChannelLayout"/> instances.
    /// </summary>
    /// <param name="left">The left <see cref="ChannelLayout"/> instance.</param>
    /// <param name="right">The right <see cref="ChannelLayout"/> instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ChannelLayout? left, ChannelLayout? right) => EqualityComparer<ChannelLayout?>.Default.Equals(left, right);

    /// <summary>
    /// Defines the inequality operator for <see cref="ChannelLayout"/> instances.
    /// </summary>
    /// <param name="left">The left <see cref="ChannelLayout"/> instance.</param>
    /// <param name="right">The right <see cref="ChannelLayout"/> instance.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ChannelLayout? left, ChannelLayout? right) => !(left == right);

    /// <summary>
    /// Defines the equality operator for <see cref="ChannelLayout"/> and <see cref="IChannelLayout"/> instances.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout"/> instance.</param>
    /// <param name="right">The <see cref="IChannelLayout"/> instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ChannelLayout? left, IChannelLayout? right) => ReferenceEquals(left, right) || left?.Equals(right) == true;

    /// <summary>
    /// Defines the inequality operator for <see cref="ChannelLayout"/> and <see cref="IChannelLayout"/> instances.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout"/> instance.</param>
    /// <param name="right">The <see cref="IChannelLayout"/> instance.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ChannelLayout? left, IChannelLayout? right) => !(left == right);

    /// <summary>
    /// Defines the equality operator for <see cref="ChannelLayout"/> and <see cref="ChannelLayout_ref"/> instances.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout"/> instance.</param>
    /// <param name="right">The <see cref="ChannelLayout_ref"/> instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ChannelLayout? left, ChannelLayout_ref right) => left?.Equals(right) == true;

    /// <summary>
    /// Defines the inequality operator for <see cref="ChannelLayout"/> and <see cref="ChannelLayout_ref"/> instances.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout"/> instance.</param>
    /// <param name="right">The <see cref="ChannelLayout_ref"/> instance.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ChannelLayout? left, ChannelLayout_ref right) => !(left == right);
}
