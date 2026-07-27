namespace FFmpeg.Filters;

/// <summary>
/// Specifies how two video streams are packed into a stereoscopic frame.
/// </summary>
public enum FramePackFormat
{
    /// <summary>
    /// Packs the left and right views side by side.
    /// This is the default frame packing mode.
    /// </summary>
    SideBySide,

    /// <summary>
    /// Packs the left and right views one above the other.
    /// Also known as top-and-bottom packing.
    /// </summary>
    TopAndBottom,

    /// <summary>
    /// Interleaves the views by alternating horizontal lines.
    /// </summary>
    LineInterleaved,

    /// <summary>
    /// Interleaves the views by alternating vertical columns.
    /// </summary>
    ColumnInterleaved,

    /// <summary>
    /// Interleaves the views temporally by alternating complete frames.
    /// </summary>
    FrameSequential,
}

/// <summary>
/// Provides extension methods for <see cref="FramePackFormat"/>.
/// </summary>
public static class FramePackFormatExtensions
{
    /// <summary>
    /// Converts a <see cref="FramePackFormat"/> to the corresponding FFmpeg filter option value.
    /// </summary>
    /// <param name="format">The frame packing format.</param>
    /// <returns>The FFmpeg string representation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="format"/> is not a valid <see cref="FramePackFormat"/> value.
    /// </exception>
    public static string ToFFmpegString(this FramePackFormat format) => format switch
    {
        FramePackFormat.SideBySide => "sbs",
        FramePackFormat.TopAndBottom => "tab",
        FramePackFormat.LineInterleaved => "lines",
        FramePackFormat.ColumnInterleaved => "columns",
        FramePackFormat.FrameSequential => "frameseq",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown frame pack format."),
    };
}