using FFmpeg.Utils;

namespace FFmpeg.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an option is not found in an FFmpeg operation.
/// </summary>
public class OptionNotFoundException : FFmpegException
{
    /// <summary>
    /// Gets the name of the option that was not found.
    /// </summary>
    public string OptionName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionNotFoundException"/> class with an empty option name.
    /// </summary>
    public OptionNotFoundException() : this(string.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionNotFoundException"/> class with the specified option name.
    /// </summary>
    /// <param name="optionName">The name of the option that was not found.</param>
    public OptionNotFoundException(string optionName) : this(optionName, "The option was not found.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionNotFoundException"/> class with the specified option name and error message.
    /// </summary>
    /// <param name="optionName">The name of the option that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    public OptionNotFoundException(string optionName, string message)
        : base(AVResult32.OptionNotFound, string.Format(message, optionName)) => OptionName = optionName;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionNotFoundException"/> class with the specified option name, error message, and inner exception.
    /// </summary>
    /// <param name="optionName">The name of the option that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public OptionNotFoundException(string optionName, string message, Exception innerException)
        : base(AVResult32.OptionNotFound, string.Format(message, optionName), innerException) => OptionName = optionName;
}

