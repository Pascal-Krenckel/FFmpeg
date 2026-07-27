using FFmpeg.AutoGen;
using FFmpeg.Utils;
using System.Buffers;
using System.Text;

namespace FFmpeg.Audio;

/// <summary>
/// Represents a reference to an FFmpeg <see cref="AutoGen._AVChannelLayout"/> structure.
/// </summary>
/// <remarks>
/// This struct provides a read-only or read-write reference to an FFmpeg channel layout. 
/// It implements <see cref="IChannelLayout"/> for accessing channel layout properties and 
/// supports copying and comparison operations.
/// </remarks>
public readonly unsafe struct ChannelLayout_ref : IChannelLayout, IEquatable<ChannelLayout_ref>, IEquatable<ChannelLayout>, IReference<ChannelLayout>
{
    /// <summary>
    /// Pointer to the underlying FFmpeg <see cref="AutoGen._AVChannelLayout"/> structure.
    /// </summary>
    internal readonly AutoGen._AVChannelLayout* ptr;

    /// <summary>
    /// Indicates whether this instance is read-only.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelLayout_ref"/> struct with the specified pointer.
    /// </summary>
    /// <param name="ptr">Pointer to the <see cref="AutoGen._AVChannelLayout"/> structure.</param>
    private ChannelLayout_ref(AutoGen._AVChannelLayout* ptr) => this.ptr = ptr;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelLayout_ref"/> struct with the specified pointer and read-only flag.
    /// </summary>
    /// <param name="ptr">Pointer to the <see cref="AutoGen._AVChannelLayout"/> structure.</param>
    /// <param name="readOnly">Indicates whether this instance is read-only.</param>
    internal ChannelLayout_ref(_AVChannelLayout* ptr, bool readOnly) : this(ptr) => IsReadOnly = readOnly;

    /// <summary>
    /// Gets the number of channels in the layout.
    /// </summary>
    /// <value>The number of channels.</value>
    public int Channels => ptr->nb_channels;

    /// <summary>
    /// Gets the underlying <see cref="AutoGen._AVChannelLayout"/> structure.
    /// </summary>
    /// <value>The channel layout structure.</value>
    _AVChannelLayout IChannelLayout.Layout => *ptr;

    /// <summary>
    /// Gets the underlying <see cref="AutoGen._AVChannelLayout"/> structure.
    /// </summary>
    /// <value>The channel layout structure.</value>
    internal _AVChannelLayout Layout => *ptr;

    /// <summary>
    /// Determines whether the current <see cref="ChannelLayout_ref"/> instance is valid.
    /// </summary>
    /// <value><see langword="true"/> if the channel layout is valid; otherwise, <see langword="false"/>.</value>
    public bool Valid => ffmpeg.av_channel_layout_check(ptr) != 0;

    /// <summary>
    /// Creates a deep copy of the referenced channel layout.
    /// </summary>
    /// <returns>
    /// A new <see cref="ChannelLayout"/> containing a copy of the referenced
    /// channel layout.
    /// </returns>
    /// <remarks>
    /// The returned <see cref="ChannelLayout"/> owns its copied layout
    /// independently of this reference.
    /// </remarks>
    public ChannelLayout Copy()
    {
        AutoGen._AVChannelLayout l;
        AVResult32 res = ffmpeg.av_channel_layout_copy(&l, ptr);
        res.ThrowIfError();
        return new(l);
    }

    /// <summary>
    /// Copies the layout from a <see cref="ChannelLayout"/> instance into the current <see cref="ChannelLayout_ref"/> instance.
    /// </summary>
    /// <param name="layout">The <see cref="ChannelLayout"/> instance to copy from.</param>
    /// <exception cref="NotSupportedException">Thrown if the current instance is read-only.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during copying.</exception>
    /// <remarks>
    /// This method replaces the layout of the current instance with the layout from the specified <see cref="ChannelLayout"/> 
    /// instance, provided the current instance is not read-only.
    /// </remarks>
    public void CopyFrom(ChannelLayout layout)
    {
        CheckReadOnly();
        AutoGen._AVChannelLayout l = layout.Layout;
        ((AVResult32)ffmpeg.av_channel_layout_copy(ptr, &l)).ThrowIfError();
    }

    /// <summary>
    /// Copies the layout from another <see cref="ChannelLayout_ref"/> instance into the current instance.
    /// </summary>
    /// <param name="layout">The <see cref="ChannelLayout_ref"/> instance to copy from.</param>
    /// <exception cref="NotSupportedException">Thrown if the current instance is read-only.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during copying.</exception>
    /// <remarks>
    /// This method replaces the layout of the current instance with the layout from the specified <see cref="ChannelLayout_ref"/> 
    /// instance, provided the current instance is not read-only.
    /// </remarks>
    public void CopyFrom(ChannelLayout_ref layout)
    {
        if (IsReadOnly)
            throw new NotSupportedException();
        AutoGen._AVChannelLayout l = layout.Layout;
        ((AVResult32)ffmpeg.av_channel_layout_copy(ptr, &l)).ThrowIfError();
    }


    /// <summary>
    /// Determines whether the current instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method checks if the given <paramref name="obj"/> is of type <see cref="ChannelLayout_ref"/> or <see cref="ChannelLayout"/> or <see cref="IChannelLayout"/> 
    /// and uses the appropriate equality comparison method based on the type.
    /// </remarks>
    public override bool Equals(object? obj) => obj is ChannelLayout_ref ptr
            ? Equals(ptr)
            : obj is ChannelLayout layout ? Equals(layout) : obj is IChannelLayout iLayout && Equals(iLayout);

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="IChannelLayout"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="IChannelLayout"/> instance to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the specified <see cref="IChannelLayout"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_compare"/> to compare the layout of the current instance with the layout of the specified <see cref="IChannelLayout"/>.
    /// </remarks>
    public readonly bool Equals(IChannelLayout? other)
    {
        if (other == null)
            return false;
        _AVChannelLayout right = other.Layout;
        return ffmpeg.av_channel_layout_compare(ptr, &right) == 0;
    }

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="ChannelLayout_ref"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="ChannelLayout_ref"/> instance to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the specified <see cref="ChannelLayout_ref"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_compare"/> to compare the layout of the current instance with the layout of the specified <see cref="ChannelLayout_ref"/> instance.
    /// </remarks>
    public readonly bool Equals(ChannelLayout_ref other) => ffmpeg.av_channel_layout_compare(ptr, other.ptr) == 0;

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="ChannelLayout"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="ChannelLayout"/> instance to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the specified <see cref="ChannelLayout"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_compare"/> to compare the layout of the current instance with the layout of the specified <see cref="ChannelLayout"/> instance.
    /// </remarks>
    public readonly bool Equals(ChannelLayout? other)
    {
        if (other == null)
            return false;
        _AVChannelLayout right = other.Layout;
        return ffmpeg.av_channel_layout_compare(ptr, &right) == 0;
    }

    /// <summary>
    /// Initializes the current channel layout as a custom layout with the specified number of channels.
    /// </summary>
    /// <param name="nb">
    /// The number of channels in the custom layout.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The current instance is read-only.
    /// </exception>
    /// <exception cref="OutOfMemoryException">
    /// FFmpeg could not allocate memory for the custom channel map.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="nb"/> is not a valid channel count.
    /// </exception>
    /// <remarks>
    /// This method initializes the referenced channel layout as a custom layout by
    /// calling <see cref="ffmpeg.av_channel_layout_custom_init"/>.
    /// Any previous layout is released before the new layout is created.
    /// </remarks>
    public readonly void Init(int nb)
    {
        if (IsReadOnly)
            throw new NotSupportedException();
        ffmpeg.av_channel_layout_uninit(ptr);
        int res = ffmpeg.av_channel_layout_custom_init(ptr, nb);
        if (res == AVResult32.OutOfMemory)
            throw new OutOfMemoryException();
        else if (res == AVResult32.InvalidArgument)
            throw new ArgumentException();
    }

    /// <summary>
    /// Returns a human-readable description of the current channel layout.
    /// </summary>
    /// <returns>
    /// A string describing the channel layout, or an empty string if the description
    /// could not be generated.
    /// </returns>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_describe"/> to format the
    /// channel layout using FFmpeg's standard textual representation, such as
    /// <c>"stereo"</c>, <c>"5.1"</c>, or a custom channel list.
    /// </remarks>
    public override string ToString()
    {

        // Get the required size for the layout description.
        AVResult32 res = ffmpeg.av_channel_layout_describe(ptr, null, 0);


        if (res.IsError)
            return string.Empty;

        byte[]? buffer = null;
        if (res > 256)
            buffer = ArrayPool<byte>.Shared.Rent(res);
        Span<byte> data = buffer ?? (stackalloc byte[res]);

        // Allocate a buffer for the description and retrieve it.
        fixed (byte* chars = data)
        {
            res = ffmpeg.av_channel_layout_describe(ptr, chars, (ulong)(int)res);
            string ret = res.IsError ? string.Empty : Encoding.UTF8.GetString(chars, res - 1);
            if (buffer != null)
                ArrayPool<byte>.Shared.Return(buffer);
            return ret;
        }
    }


    /// <summary>
    /// Returns the hash code for the current instance.
    /// </summary>
    /// <returns>An <see cref="int"/> that represents the hash code for the current instance.</returns>
    /// <remarks>
    /// The hash code is computed based on the number of channels in the channel layout.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(Channels);

    /// <summary>
    /// Retrieves a copy of the referenced channel layout.
    /// </summary>
    /// <returns>
    /// A new <see cref="ChannelLayout"/> containing a copy of the referenced layout.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// FFmpeg could not allocate memory while copying the channel layout.
    /// </exception>
    /// <remarks>
    /// The returned <see cref="ChannelLayout"/> is independent of the referenced
    /// layout. Modifying either instance does not affect the other.
    /// </remarks>
    public readonly ChannelLayout GetReferencedObject()
    {
        AutoGen._AVChannelLayout layout;
        AVResult32 res = ffmpeg.av_channel_layout_copy(&layout, ptr);
        res.ThrowIfError();
        return new(layout);
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
    /// For native and ambisonic layouts, it checks whether the corresponding channel
    /// bit is set in the layout's channel mask.
    /// </remarks>
    public bool HasChannel(AudioChannel channelId)
    {
        if (Channels <= 0)
            return false;

        if (ptr->order == _AVChannelOrder.AV_CHANNEL_ORDER_CUSTOM)
        {
            if (ptr->u.map == null)
                return false;

            for (int i = 0; i < Channels; i++)
            {
                if (ptr->u.map[i].id == (_AVChannel)channelId)
                    return true;
            }

            return false;
        }

        return ((ptr->u.mask >> (int)channelId) & 1) != 0;
    }

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
    /// <remarks>
    /// Custom channel names are stored in a fixed-size 16-byte UTF-8 buffer.
    /// </remarks>
    public string GetCustomChannelName(int channelNumber)
    {
        ValidateCustomChannel(channelNumber);

        Span<byte> bytes = new((byte*)&ptr->u.map[channelNumber].name, 16);
        int length = bytes.IndexOf((byte)0);
        if (length < 0)
            length = bytes.Length;

        return Encoding.UTF8.GetString(bytes[..length]);
    }

    /// <summary>
    /// Sets the UTF-8 name assigned to a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based index of the channel.
    /// </param>
    /// <param name="name">
    /// The UTF-8 channel name. The encoded name must not exceed 16 bytes.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The current instance is read-only.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid channel range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The UTF-8 encoded channel name exceeds the 16-byte storage limit.
    /// </exception>
    /// <remarks>
    /// Any unused bytes in the fixed-size channel name buffer are cleared.
    /// </remarks>
    public void SetCustomChannelName(int channelNumber, ReadOnlySpan<char> name)
    {
        ValidateCustomChannel(channelNumber);
        CheckReadOnly();

        Span<byte> bytes = new((byte*)&ptr->u.map[channelNumber].name, 16);
        bytes.Clear();

        if (Encoding.UTF8.GetByteCount(name) > bytes.Length)
            throw new ArgumentException("The UTF-8 encoded channel name must not exceed 16 bytes.", nameof(name));

        _ = Encoding.UTF8.GetBytes(name, bytes);
    }

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
    public AudioChannel GetCustomChannelId(int channelNumber)
    {
        ValidateCustomChannel(channelNumber);
        return (AudioChannel)ptr->u.map[channelNumber].id;
    }

    /// <summary>
    /// Sets the channel identifier assigned to a channel in a custom channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based index of the channel.
    /// </param>
    /// <param name="channel">
    /// The channel identifier to assign.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The current instance is read-only.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid channel range.
    /// </exception>
    public void SetCustomChannelId(int channelNumber, AudioChannel channel)
    {
        ValidateCustomChannel(channelNumber);
        CheckReadOnly();
        ptr->u.map[channelNumber].id = (_AVChannel)channel;
    }

    /// <summary>
    /// Validates that the specified channel index refers to a channel in a custom
    /// channel layout.
    /// </summary>
    /// <param name="channelNumber">
    /// The zero-based channel index.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// The channel layout is not a custom channel layout.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="channelNumber"/> is outside the valid channel range.
    /// </exception>
    private void ValidateCustomChannel(int channelNumber)
    {

        if (ptr->order != _AVChannelOrder.AV_CHANNEL_ORDER_CUSTOM)
            throw new InvalidOperationException("The channel layout is not a custom channel layout.");

        if ((uint)channelNumber >= (uint)Channels)
            throw new ArgumentOutOfRangeException(nameof(channelNumber));
    }

    /// <summary>
    /// Sets the referenced channel layout.
    /// </summary>
    /// <param name="obj">
    /// The channel layout to copy into the referenced layout, or
    /// <see langword="null"/> to uninitialize the current layout.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The current instance is read-only.
    /// </exception>
    /// <exception cref="OutOfMemoryException">
    /// FFmpeg could not allocate memory while copying the channel layout.
    /// </exception>
    /// <remarks>
    /// If <paramref name="obj"/> is <see langword="null"/>, the referenced layout
    /// is uninitialized using <see cref="ffmpeg.av_channel_layout_uninit"/>.
    /// Otherwise, the layout is replaced with a copy of
    /// <paramref name="obj"/> using
    /// <see cref="ffmpeg.av_channel_layout_copy"/>.
    /// </remarks>
    public void SetReferencedObject(ChannelLayout? obj)
    {
        CheckReadOnly();
        if (obj == null)
        {
            ffmpeg.av_channel_layout_uninit(ptr);
        }
        else
        {
            _AVChannelLayout layout = obj.Layout;
            AVResult32 res = ffmpeg.av_channel_layout_copy(ptr, &layout);
            res.ThrowIfError(); // Throws OutOfMemoryException or other exceptions based on the result
        }
    }

    private void CheckReadOnly()
    {
        if (IsReadOnly)
            throw new NotSupportedException("The current instance is read-only and cannot be modified.");
    }


    /// <summary>
    /// Determines whether two <see cref="ChannelLayout_ref"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <param name="right">The second <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This operator uses <see cref="Equals(ChannelLayout_ref)"/> to determine equality between two <see cref="ChannelLayout_ref"/> instances.
    /// </remarks>
    public static bool operator ==(ChannelLayout_ref left, ChannelLayout_ref right) => EqualityComparer<ChannelLayout_ref>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="ChannelLayout_ref"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <param name="right">The second <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the two instances are not equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This operator uses <see cref="Equals(ChannelLayout_ref)"/> to determine if two <see cref="ChannelLayout_ref"/> instances are not equal.
    /// </remarks>
    public static bool operator !=(ChannelLayout_ref left, ChannelLayout_ref right) => !(left == right);

    /// <summary>
    /// Determines whether a <see cref="ChannelLayout_ref"/> instance is equal to a <see cref="ChannelLayout"/> instance.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <param name="right">The <see cref="ChannelLayout"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="ChannelLayout_ref"/> instance is equal to the <see cref="ChannelLayout"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This operator uses <see cref="Equals(ChannelLayout?)"/> to determine equality between a <see cref="ChannelLayout_ref"/> instance and a <see cref="ChannelLayout"/> instance.
    /// </remarks>
    public static bool operator ==(ChannelLayout_ref left, ChannelLayout? right) => left.Equals(right);

    /// <summary>
    /// Determines whether a <see cref="ChannelLayout_ref"/> instance is not equal to a <see cref="ChannelLayout"/> instance.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <param name="right">The <see cref="ChannelLayout"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="ChannelLayout_ref"/> instance is not equal to the <see cref="ChannelLayout"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This operator uses <see cref="Equals(ChannelLayout?)"/> to determine if a <see cref="ChannelLayout_ref"/> instance is not equal to a <see cref="ChannelLayout"/> instance.
    /// </remarks>
    public static bool operator !=(ChannelLayout_ref left, ChannelLayout? right) => !(left == right);

    /// <summary>
    /// Determines whether a <see cref="ChannelLayout_ref"/> instance is equal to an <see cref="IChannelLayout"/> instance.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <param name="right">The <see cref="IChannelLayout"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="ChannelLayout_ref"/> instance is equal to the <see cref="IChannelLayout"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This operator uses <see cref="Equals(IChannelLayout?)"/> to determine equality between a <see cref="ChannelLayout_ref"/> instance and an <see cref="IChannelLayout"/> instance.
    /// </remarks>
    public static bool operator ==(ChannelLayout_ref left, IChannelLayout? right) => left.Equals(right);

    /// <summary>
    /// Determines whether a <see cref="ChannelLayout_ref"/> instance is not equal to an <see cref="IChannelLayout"/> instance.
    /// </summary>
    /// <param name="left">The <see cref="ChannelLayout_ref"/> instance to compare.</param>
    /// <param name="right">The <see cref="IChannelLayout"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="ChannelLayout_ref"/> instance is not equal to the <see cref="IChannelLayout"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This operator uses <see cref="Equals(IChannelLayout?)"/> to determine if a <see cref="ChannelLayout_ref"/> instance is not equal to an <see cref="IChannelLayout"/> instance.
    /// </remarks>
    public static bool operator !=(ChannelLayout_ref left, IChannelLayout? right) => !(left == right);

}
