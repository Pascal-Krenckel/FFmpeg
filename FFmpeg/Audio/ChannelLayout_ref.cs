using FFmpeg.AutoGen;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

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
    /// Creates a copy of the current <see cref="ChannelLayout_ref"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ChannelLayout"/> that is a copy of the current instance.</returns>
    /// <exception cref="Exception">Thrown if an error occurs during copying.</exception>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_copy"/> to create a new <see cref="ChannelLayout"/> instance
    /// from the current layout.
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
        if (IsReadOnly)
            throw new NotSupportedException();
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
    /// Initializes the channel layout with a custom number of channels.
    /// </summary>
    /// <param name="nb">The number of channels to initialize with.</param>
    /// <exception cref="NotSupportedException">Thrown if the current instance is read-only.</exception>
    /// <exception cref="OutOfMemoryException">Thrown if there is insufficient memory to perform the operation.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="nb"/> parameter is invalid.</exception>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_custom_init"/> to set the number of channels for the current channel layout.
    /// </remarks>
    public readonly void Init(int nb)
    {
        if (IsReadOnly)
            throw new NotSupportedException();
        int res = ffmpeg.av_channel_layout_custom_init(ptr, nb);
        if (res == AVResult32.OutOfMemory)
            throw new OutOfMemoryException();
        else if (res == AVResult32.InvalidArgument)
            throw new ArgumentException();
    }

    /// <summary>
    /// Returns a string that describes the current channel layout.
    /// </summary>
    /// <returns>A string that represents the current channel layout, or an empty string if an error occurs.</returns>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_describe"/> to get a description of the current channel layout.
    /// </remarks>
    public override readonly string ToString()
    {
        AVResult32 res = ffmpeg.av_channel_layout_describe(ptr, null, 0);
        if (res.IsError)
            return string.Empty;
        byte* chars = stackalloc byte[res];
        res = ffmpeg.av_channel_layout_describe(ptr, chars, (ulong)(int)res);
        return res.IsError ? string.Empty : Marshal.PtrToStringUTF8((nint)chars);
    }


    /// <summary>
    /// Returns the hash code for the current instance.
    /// </summary>
    /// <returns>An <see cref="int"/> that represents the hash code for the current instance.</returns>
    /// <remarks>
    /// The hash code is computed based on the number of channels in the channel layout using <see cref="HashCode.Combine"/>.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(Channels);

    /// <summary>
    /// Retrieves a <see cref="ChannelLayout"/> object that is a copy of the referenced channel layout.
    /// </summary>
    /// <returns>A new <see cref="ChannelLayout"/> instance representing the referenced channel layout.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if there is insufficient memory to perform the operation.</exception>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_copy"/> to create a copy of the current channel layout and return it as a <see cref="ChannelLayout"/> instance.
    /// </remarks>
    public ChannelLayout? GetReferencedObject()
    {
        AutoGen._AVChannelLayout layout;
        AVResult32 res = ffmpeg.av_channel_layout_copy(&layout, ptr);
        res.ThrowIfError();
        return new(layout);
    }

    /// <summary>
    /// Sets the referenced channel layout to a new <see cref="ChannelLayout"/> object.
    /// </summary>
    /// <param name="obj">The <see cref="ChannelLayout"/> instance to set as the referenced object, or <see langword="null"/> to uninitialize the current layout.</param>
    /// <exception cref="NotSupportedException">Thrown if the current instance is read-only and cannot be modified. This occurs when <see cref="IsReadOnly"/> is <see langword="true"/>.</exception>
    /// <exception cref="OutOfMemoryException">Thrown if there is insufficient memory to perform the operation. This can occur if <see cref="ffmpeg.av_channel_layout_copy"/> fails due to memory constraints.</exception>
    /// <remarks>
    /// This method uses <see cref="ffmpeg.av_channel_layout_copy"/> to copy the layout from the specified <see cref="ChannelLayout"/> instance. If <paramref name="obj"/> is <see langword="null"/>, it uses <see cref="ffmpeg.av_channel_layout_uninit"/> to uninitialize the current layout. 
    /// </remarks>
    public void SetReferencedObject(ChannelLayout? obj)
    {
        if (IsReadOnly)
            throw new NotSupportedException("The current instance is read-only and cannot be modified.");
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
