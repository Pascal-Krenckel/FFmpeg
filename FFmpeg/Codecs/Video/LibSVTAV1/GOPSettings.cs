using System.Text;

namespace FFmpeg.Codecs.Video.LibSVTAV1;

/// <summary>
/// Represents the Group of Pictures (GOP) settings, controlling the interval between keyframes in video encoding.
/// </summary>
public struct GOPSettings : IEquatable<GOPSettings>
{
    private int? frames;

    /// <summary>
    /// Gets or sets the GOP size in frames. 
    /// Accepts values from [-2 to (2^31)-1].
    /// <br/>
    /// Special values:
    /// <list type="bullet">
    /// <item><description>-2: Approximately 5 seconds (default)</description></item>
    /// <item><description>-1: Infinite GOP size, only applicable in CRF mode</description></item>
    /// <item><description>0: Equivalent to -1</description></item>
    /// </list>
    /// <br/>
    /// In SvtAv1EncApp, you can append the 's' suffix to use seconds instead of frames.
    /// To set GOP size in seconds directly, use <see cref="AsTimeSpan"/>.
    /// </summary>
    public int? Frames
    {
        readonly get => frames;
        set
        {
            frames = value;
            IsTimeSpan = false; // Indicates GOP size is specified in frames.
        }
    }

    /// <summary>
    /// Indicates whether the GOP size is in seconds or frames.
    /// <br/>
    /// True if <see cref="AsTimeSpan"/> was used to set the value; false if <see cref="Frames"/> was used.
    /// </summary>
    public bool IsTimeSpan { get; private set; }

    /// <summary>
    /// Gets or sets the GOP size as a <see cref="TimeSpan"/>, representing the duration between keyframes in seconds.
    /// <br/>
    /// When set, the <see cref="Frames"/> property is automatically adjusted to reflect the equivalent number of seconds.
    /// </summary>
    public TimeSpan? AsTimeSpan
    {
        readonly get => frames.HasValue ? TimeSpan.FromSeconds(frames.Value) : null;
        set
        {
            frames = value.HasValue ? (int)Math.Floor(value.Value.TotalSeconds) : null;
            IsTimeSpan = true; // Indicates GOP size is specified in seconds.
        }
    }

    /// <summary>
    /// Controls whether scene change detection is enabled.
    /// <br/>
    /// When enabled, the encoder inserts keyframes at scene changes to enhance visual quality.
    /// <br/>
    /// Default is false.
    /// </summary>
    public bool? SceneChangeDetection { get; set; }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => obj is GOPSettings settings && Equals(settings);

    /// <inheritdoc/>
    public readonly bool Equals(GOPSettings other) => frames == other.frames && IsTimeSpan == other.IsTimeSpan && SceneChangeDetection == other.SceneChangeDetection;

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(frames, IsTimeSpan, SceneChangeDetection);

    /// <summary>
    /// Adds the current GOP settings to the provided dictionary in a key=value format.
    /// <br/>
    /// <paramref name="dic"/> is the dictionary where the GOP settings will be added.
    /// </summary>
    /// <param name="dic">The dictionary to store the settings.</param>
    public readonly void AddToDictionary(IDictionary<string, string> dic)
    {
        if (Frames.HasValue)
            dic["keyint"] = IsTimeSpan ? $"{Frames.Value}s" : Frames.Value.ToString();
        if (SceneChangeDetection.HasValue)
            dic["scd"] = SceneChangeDetection.Value ? "1" : "0";
    }

    /// <summary>
    /// Returns a string representation of the GOP settings in a key=value format.
    /// <br/>
    /// For example, it might return "keyint=48:scd=1" for a frame-based GOP with scene change detection enabled.
    /// </summary>
    public override readonly string ToString()
    {
        StringBuilder stringBuilder = new();
        if (Frames.HasValue)
        {
            _ = IsTimeSpan ? stringBuilder.Append($"keyint={Frames.Value}s") : stringBuilder.Append($"keyint={Frames.Value}");
        }
        if (SceneChangeDetection.HasValue)
        {
            if (stringBuilder.Length > 0)
                _ = stringBuilder.Append(':');
            _ = stringBuilder.Append($"scd={(SceneChangeDetection.Value ? 1 : 0)}");
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Determines whether two specified <see cref="GOPSettings"/> instances are equal.
    /// </summary>
    public static bool operator ==(GOPSettings left, GOPSettings right) => left.Equals(right);

    /// <summary>
    /// Determines whether two specified <see cref="GOPSettings"/> instances are not equal.
    /// </summary>
    public static bool operator !=(GOPSettings left, GOPSettings right) => !(left == right);
}
