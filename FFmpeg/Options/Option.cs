using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Options;

/// <summary>
/// Represents an option in the FFmpeg library's configuration system.
/// </summary>
/// <remarks>
/// This class provides access to the properties and metadata of an option, including its name, type, help text, and related flags.
/// </remarks>
public sealed unsafe class Option
{
    private readonly AutoGen._AVOption* option;

    /// <summary>
    /// Gets the name of the option.
    /// </summary>
    /// <remarks>
    /// The name is used as an identifier for the option within the FFmpeg library.
    /// </remarks>
    public string Name => Marshal.PtrToStringUTF8((IntPtr)option->name);

    /// <summary>
    /// Gets the help text associated with the option.
    /// </summary>
    /// <remarks>
    /// The help text provides a description of the option's purpose and usage.
    /// </remarks>
    public string HelpText => Marshal.PtrToStringUTF8((IntPtr)option->help);

    /// <summary>
    /// Gets the type of the option.
    /// </summary>
    /// <remarks>
    /// The type indicates the kind of data the option accepts (e.g., integer, string, boolean).
    /// </remarks>
    public OptionType Type => (OptionType)option->type;

    /// <summary>
    /// Gets the media type associated with the option.
    /// </summary>
    /// <remarks>
    /// This property determines whether the option is related to video, audio, subtitle, or an unknown media type based on its flags.
    /// </remarks>
    public MediaType OptionMediaType => (option->flags & ffmpeg.AV_OPT_FLAG_VIDEO_PARAM) != 0
                ? MediaType.Video
                : (option->flags & ffmpeg.AV_OPT_FLAG_AUDIO_PARAM) != 0
                ? MediaType.Audio
                : (option->flags & ffmpeg.AV_OPT_FLAG_SUBTITLE_PARAM) != 0
                ? MediaType.Subtitle
                : MediaType.Unknown;

    /// <summary>
    /// Gets the flags associated with the option.
    /// </summary>
    /// <remarks>
    /// Flags provide additional information about the option, such as whether it is read-only or affects specific media types.
    /// </remarks>
    public OptionFlags Flags => (OptionFlags)option->flags;

    /// <summary>
    /// Gets a value indicating whether the option is read-only.
    /// </summary>
    /// <remarks>
    /// If this property returns <c>true</c>, the option cannot be modified after it is set.
    /// </remarks>
    public bool IsReadOnly => (option->flags & ffmpeg.AV_OPT_FLAG_READONLY) != 0;

    /// <summary>
    /// Creates an instance of <see cref="Option"/> from a pointer to a native <see cref="AutoGen._AVOption"/> structure.
    /// </summary>
    /// <param name="v">Pointer to the native <see cref="AutoGen._AVOption"/> structure.</param>
    /// <returns>
    /// An instance of <see cref="Option"/> if the pointer is not null; otherwise, <c>null</c>.
    /// </returns>
    internal static Option? Create(AutoGen._AVOption* v) => v != null ? new Option(v) : null;

    /// <summary>
    /// Returns a string representation of the option.
    /// </summary>
    /// <returns>
    /// The name of the option.
    /// </returns>
    public override string ToString() => Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="Option"/> class.
    /// </summary>
    /// <param name="option">Pointer to the native <see cref="AutoGen._AVOption"/> structure.</param>
    internal Option(AutoGen._AVOption* option) => this.option = option;
}
