namespace FFmpeg.Filters.AudioFilters;

/// <summary>The sample precision <c>volume</c> computes with (option <c>precision</c>).</summary>
public enum VolumePrecision
{
    /// <summary>8-bit fixed point; limits the input to U8, S16 and S32 (<c>fixed</c>).</summary>
    Fixed = 0,

    /// <summary>32-bit floating point (<c>float</c>, default).</summary>
    Float = 1,

    /// <summary>64-bit floating point (<c>double</c>).</summary>
    Double = 2,
}

/// <summary>How <c>volume</c> uses ReplayGain side data (option <c>replaygain</c>).</summary>
public enum ReplayGainMode
{
    /// <summary>Remove the side data and ignore it (<c>drop</c>, default).</summary>
    Drop = 0,

    /// <summary>Keep the side data but ignore it (<c>ignore</c>).</summary>
    Ignore = 1,

    /// <summary>Apply the track gain (<c>track</c>).</summary>
    Track = 2,

    /// <summary>Apply the album gain (<c>album</c>).</summary>
    Album = 3,
}

/// <summary>When <c>amix</c> ends (option <c>duration</c>).</summary>
public enum AMixDuration
{
    /// <summary>With the longest input (<c>longest</c>, default).</summary>
    Longest = 0,

    /// <summary>With the shortest input (<c>shortest</c>).</summary>
    Shortest = 1,

    /// <summary>With the first input (<c>first</c>).</summary>
    First = 2,
}

/// <summary>The fade curve of <c>afade</c> (option <c>curve</c>).</summary>
public enum AudioFadeCurve
{
    /// <summary>No fade; output silence for fade-in and unchanged audio afterwards (<c>nofade</c>).</summary>
    None = -1,
    /// <summary>Triangular, linear slope (<c>tri</c>, default).</summary>
    Triangular = 0,
    /// <summary>Quarter of a sine wave (<c>qsin</c>).</summary>
    QuarterSine = 1,
    /// <summary>Exponential sine wave (<c>esin</c>).</summary>
    ExponentialSine = 2,
    /// <summary>Half of a sine wave (<c>hsin</c>).</summary>
    HalfSine = 3,
    /// <summary>Logarithmic (<c>log</c>).</summary>
    Logarithmic = 4,
    /// <summary>Inverted parabola (<c>ipar</c>).</summary>
    InvertedParabola = 5,
    /// <summary>Quadratic (<c>qua</c>).</summary>
    Quadratic = 6,
    /// <summary>Cubic (<c>cub</c>).</summary>
    Cubic = 7,
    /// <summary>Square root (<c>squ</c>).</summary>
    SquareRoot = 8,
    /// <summary>Cubic root (<c>cbr</c>).</summary>
    CubicRoot = 9,
    /// <summary>Parabola (<c>par</c>).</summary>
    Parabola = 10,
    /// <summary>Exponential (<c>exp</c>).</summary>
    Exponential = 11,
    /// <summary>Inverted quarter of a sine wave (<c>iqsin</c>).</summary>
    InvertedQuarterSine = 12,
    /// <summary>Inverted half of a sine wave (<c>ihsin</c>).</summary>
    InvertedHalfSine = 13,
    /// <summary>Double-exponential seat (<c>dese</c>).</summary>
    DoubleExponentialSeat = 14,
    /// <summary>Double-exponential sigmoid (<c>desi</c>).</summary>
    DoubleExponentialSigmoid = 15,
    /// <summary>Logistic sigmoid (<c>losi</c>).</summary>
    LogisticSigmoid = 16,
    /// <summary>Sine cardinal (<c>sinc</c>).</summary>
    SineCardinal = 17,
    /// <summary>Inverted sine cardinal (<c>isinc</c>).</summary>
    InvertedSineCardinal = 18,
    /// <summary>Quartic (<c>quat</c>).</summary>
    Quartic = 19,
    /// <summary>Quartic root (<c>quatr</c>).</summary>
    QuarticRoot = 20,
    /// <summary>Squared quarter of a sine wave (<c>qsin2</c>).</summary>
    SquaredQuarterSine = 21,
    /// <summary>Squared half of a sine wave (<c>hsin2</c>).</summary>
    SquaredHalfSine = 22,
}
