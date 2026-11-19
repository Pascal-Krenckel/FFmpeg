namespace FFmpeg.Filters.VideoFilters;

/// <summary>How <c>scale</c> keeps the input aspect ratio (option <c>force_original_aspect_ratio</c>).</summary>
public enum ScaleAspectRatioMode
{
    /// <summary>Use the requested size as is (<c>disable</c>).</summary>
    Disable = 0,

    /// <summary>Decrease width or height so the result fits into the requested size (<c>decrease</c>).</summary>
    Decrease = 1,

    /// <summary>Increase width or height so the result covers the requested size (<c>increase</c>).</summary>
    Increase = 2,
}

/// <summary>Rounding of output timestamps in <c>fps</c> (option <c>round</c>).</summary>
public enum FpsRounding
{
    /// <summary>Toward zero (<c>zero</c>).</summary>
    Zero = 0,

    /// <summary>Away from zero (<c>inf</c>).</summary>
    Infinity = 1,

    /// <summary>Toward negative infinity (<c>down</c>).</summary>
    Down = 2,

    /// <summary>Toward positive infinity (<c>up</c>).</summary>
    Up = 3,

    /// <summary>To the nearest value (<c>near</c>, default).</summary>
    Near = 5,
}

/// <summary>What <c>fps</c> does with the last frame (option <c>eof_action</c>).</summary>
public enum FpsEofAction
{
    /// <summary>Round the end timestamp like the other frames (<c>round</c>, default).</summary>
    Round = 0,

    /// <summary>Pass the last frame through if the end is not reached yet (<c>pass</c>).</summary>
    Pass = 1,
}

/// <summary>The rotation and flip of <c>transpose</c> (option <c>dir</c>).</summary>
public enum TransposeDirection
{
    /// <summary>Rotate 90° counterclockwise and flip vertically (<c>cclock_flip</c>, default).</summary>
    CounterClockwiseFlip = 0,

    /// <summary>Rotate 90° clockwise (<c>clock</c>).</summary>
    Clockwise = 1,

    /// <summary>Rotate 90° counterclockwise (<c>cclock</c>).</summary>
    CounterClockwise = 2,

    /// <summary>Rotate 90° clockwise and flip vertically (<c>clock_flip</c>).</summary>
    ClockwiseFlip = 3,
}

/// <summary>When <c>transpose</c> leaves the input unchanged (option <c>passthrough</c>).</summary>
public enum TransposePassthrough
{
    /// <summary>Always transpose (<c>none</c>, default).</summary>
    None = 0,

    /// <summary>Do not transpose landscape input (<c>landscape</c>).</summary>
    Landscape = 1,

    /// <summary>Do not transpose portrait input (<c>portrait</c>).</summary>
    Portrait = 2,
}

/// <summary>What <c>yadif</c> outputs (option <c>mode</c>).</summary>
public enum YadifMode
{
    /// <summary>One frame per frame (<c>send_frame</c>, default).</summary>
    SendFrame = 0,

    /// <summary>One frame per field, doubling the frame rate (<c>send_field</c>).</summary>
    SendField = 1,

    /// <summary>Like <see cref="SendFrame"/>, without the spatial interlacing check (<c>send_frame_nospatial</c>).</summary>
    SendFrameNoSpatial = 2,

    /// <summary>Like <see cref="SendField"/>, without the spatial interlacing check (<c>send_field_nospatial</c>).</summary>
    SendFieldNoSpatial = 3,
}

/// <summary>The assumed field order for <c>yadif</c> (option <c>parity</c>).</summary>
public enum YadifParity
{
    /// <summary>Detect it from the frames (<c>auto</c>, default).</summary>
    Auto = -1,

    /// <summary>Top field first (<c>tff</c>).</summary>
    TopFieldFirst = 0,

    /// <summary>Bottom field first (<c>bff</c>).</summary>
    BottomFieldFirst = 1,
}

/// <summary>Which frames <c>yadif</c> deinterlaces (option <c>deint</c>).</summary>
public enum YadifDeinterlace
{
    /// <summary>All frames (<c>all</c>, default).</summary>
    All = 0,

    /// <summary>Only frames marked as interlaced (<c>interlaced</c>).</summary>
    Interlaced = 1,
}

/// <summary>What <c>overlay</c> does when the overlay input ends (option <c>eof_action</c>).</summary>
public enum OverlayEofAction
{
    /// <summary>Keep showing its last frame (<c>repeat</c>, default).</summary>
    Repeat = 0,

    /// <summary>End both streams (<c>endall</c>).</summary>
    EndAll = 1,

    /// <summary>Pass the main input through (<c>pass</c>).</summary>
    Pass = 2,
}

/// <summary>The format <c>overlay</c> blends in (option <c>format</c>).</summary>
public enum OverlayFormat
{
    /// <summary>YUV 4:2:0 (<c>yuv420</c>, default).</summary>
    Yuv420 = 0,

    /// <summary>10-bit YUV 4:2:0 (<c>yuv420p10</c>).</summary>
    Yuv420P10 = 1,

    /// <summary>YUV 4:2:2 (<c>yuv422</c>).</summary>
    Yuv422 = 2,

    /// <summary>10-bit YUV 4:2:2 (<c>yuv422p10</c>).</summary>
    Yuv422P10 = 3,

    /// <summary>YUV 4:4:4 (<c>yuv444</c>).</summary>
    Yuv444 = 4,

    /// <summary>10-bit YUV 4:4:4 (<c>yuv444p10</c>).</summary>
    Yuv444P10 = 5,

    /// <summary>Packed RGB (<c>rgb</c>).</summary>
    Rgb = 6,

    /// <summary>Planar RGB (<c>gbrp</c>).</summary>
    Gbrp = 7,

    /// <summary>Choose automatically (<c>auto</c>).</summary>
    Auto = 8,
}

/// <summary>
/// The format of the <see cref="LibVmaf"/> log file (option <c>log_fmt</c>).
/// </summary>
public enum VmafLogFormat
{
    /// <summary>XML (<c>xml</c>), the default; read it with <see cref="VmafLog"/>.</summary>
    Xml,

    /// <summary>JSON (<c>json</c>).</summary>
    Json,

    /// <summary>Comma-separated values (<c>csv</c>).</summary>
    Csv,

    /// <summary>MicroDVD subtitles that show the score of each frame (<c>sub</c>).</summary>
    Sub,
}

/// <summary>
/// How <see cref="LibVmaf"/> pools the per-frame scores into the score FFmpeg logs (option <c>pool</c>).
/// </summary>
public enum VmafPoolMethod
{
    /// <summary>The arithmetic mean (<c>mean</c>), the default.</summary>
    Mean,

    /// <summary>The lowest score (<c>min</c>).</summary>
    Min,

    /// <summary>The harmonic mean (<c>harmonic_mean</c>), which weighs bad frames more.</summary>
    HarmonicMean,
}
