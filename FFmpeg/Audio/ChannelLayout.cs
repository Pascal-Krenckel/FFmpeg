using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Audio;
/// <summary>
/// Represents a channel layout, which defines the arrangement of audio channels in a multi-channel audio format.
/// This class provides functionality to interact with and manipulate audio channel layouts using FFmpeg's <see cref="AutoGen._AVChannelLayout"/> structure.
/// </summary>
/// <remarks>
/// A channel layout describes the number and arrangement of audio channels (e.g., stereo, 5.1 surround sound).
/// The <see cref="ChannelLayout"/> class allows you to create, initialize, and manage channel layouts, and to retrieve standard layouts available in FFmpeg.
/// It also supports copying and cloning of layouts, as well as conversion between planar and packed formats.
/// </remarks>
public unsafe class ChannelLayout : IEquatable<ChannelLayout>, IChannelLayout, IEquatable<ChannelLayout_ref>, IDisposable
{
    internal AutoGen._AVChannelLayout layout;
    private readonly bool disposed = false;

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
    /// Initializes a new instance of the <see cref="ChannelLayout"/> class using an existing FFmpeg channel layout,
    /// with control over whether the instance should be disposed.
    /// </summary>
    /// <param name="layout">The FFmpeg channel layout to be used.</param>
    /// <param name="dispose">If <see langword="true"/>, the object will manage its own disposal; otherwise, it will not.</param>
    internal ChannelLayout(AutoGen._AVChannelLayout layout, bool dispose)
    {
        this.layout = layout;
        disposed = !dispose;
    }

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
        int res = ffmpeg.av_channel_layout_custom_init(&l, nb);
        if (res == AVResult32.OutOfMemory)
            throw new OutOfMemoryException("Insufficient memory to initialize the channel layout.");
        else if (res == AVResult32.InvalidArgument)
            throw new ArgumentException("Invalid number of channels specified for the channel layout.");
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="ChannelLayout"/>, uninitializing the layout.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            AutoGen._AVChannelLayout c = layout;
            ffmpeg.av_channel_layout_uninit(&c); // Uninitialize the layout to release resources.
            layout = c;
        }
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

        // Allocate a buffer for the description and retrieve it.
        byte* chars = stackalloc byte[res];
        res = ffmpeg.av_channel_layout_describe(&layout, chars, (ulong)(int)res);

        return res.IsError ? string.Empty : Marshal.PtrToStringUTF8((nint)chars);
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
    public static bool TryParse(string str, out ChannelLayout layout)
    {
        AutoGen._AVChannelLayout l;
        int res = ffmpeg.av_channel_layout_from_string(&l, str);
        layout = new ChannelLayout(l);
        return res >= 0;
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
        _ = ffmpeg.av_channel_layout_copy(&l, &layout);
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

    public static ChannelLayout CreateDefault(int channels)
    {
        ChannelLayout layout = new();
        AutoGen._AVChannelLayout p = layout.layout;
        ffmpeg.av_channel_layout_default(&p, channels);
        layout.layout = p;
        return layout;
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
