namespace FFmpeg.Images;
/// <summary>
/// Represents a scaling algorithm for use with FFmpeg's libswscale, encapsulating the algorithm flags and optional scaling parameters.
/// These parameters can tune the behavior of the scaling algorithm for finer control over image scaling quality.
/// </summary>
public readonly struct SwsAlgorithm : IEquatable<SwsAlgorithm>
{
    /// <summary>
    /// The default value for algorithm parameters when none are specified.
    /// </summary>
    public const int DEFAULT_PARAM = 123456;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwsAlgorithm"/> struct with the specified scaling algorithm flags and optional parameters.
    /// </summary>
    /// <param name="algorithmFlags">The scaling algorithm flags used to determine the interpolation or filtering method.</param>
    /// <param name="param1">An optional parameter to tune the behavior of the algorithm (default: <see cref="DEFAULT_PARAM"/>).</param>
    /// <param name="param2">An optional second parameter for additional tuning of the algorithm (default: <see cref="DEFAULT_PARAM"/>).</param>
    public SwsAlgorithm(SwsFlags algorithmFlags, double param1 = DEFAULT_PARAM, double param2 = DEFAULT_PARAM)
    {
        AlgorithmFlags = algorithmFlags;
        Param1 = param1;
        Param2 = param2;
    }

    /// <summary>
    /// Gets the scaling algorithm flags that specify the type of interpolation or filtering used.
    /// </summary>
    public SwsFlags AlgorithmFlags { get; }

    /// <summary>
    /// Gets the first parameter used for fine-tuning the scaling algorithm, specific to the algorithm.
    /// </summary>
    public double Param1 { get; }

    /// <summary>
    /// Gets the second parameter used for additional fine-tuning of the scaling algorithm, specific to the algorithm.
    /// </summary>
    public double Param2 { get; }

    /// <summary>
    /// Creates a fast bilinear scaling algorithm with no additional tuning.
    /// </summary>
    public static SwsAlgorithm FastBilinear() => new(SwsFlags.FastBilinear);

    /// <summary>
    /// Creates a bilinear scaling algorithm with no additional tuning.
    /// </summary>
    public static SwsAlgorithm Bilinear() => new(SwsFlags.Bilinear);

    /// <summary>
    /// Creates a bicubic scaling algorithm with default parameters.
    /// </summary>
    public static SwsAlgorithm Bicubic() => new(SwsFlags.Bicubic);

    /// <summary>
    /// Creates a bicubic scaling algorithm with adjustable parameters.
    /// <para>
    /// For <see cref="SwsFlags.Bicubic"/>, param1 tunes the value of f(1), and param2 tunes f'(1), which influence the shape of the basis function.
    /// </para>
    /// </summary>
    /// <param name="param1">Tuning parameter for f(1) (default: 0).</param>
    /// <param name="param2">Tuning parameter for f'(1) (default: 0.6).</param>
    public static SwsAlgorithm Bicubic(double param1 = 0, double param2 = 0.6) => new(SwsFlags.Bicubic, param1, param2);

    /// <summary>
    /// Creates a nearest neighbor scaling algorithm (point sampling).
    /// </summary>
    public static SwsAlgorithm NearestNeighbor() => new(SwsFlags.NearestNeighbor);

    /// <summary>
    /// Creates an area-based scaling algorithm with no additional tuning.
    /// </summary>
    public static SwsAlgorithm Area() => new(SwsFlags.Area);

    /// <summary>
    /// Creates an experimental scaling algorithm with a default parameter.
    /// </summary>
    public static SwsAlgorithm Experimental() => new(SwsFlags.Experimental);

    /// <summary>
    /// Creates an experimental scaling algorithm with adjustable parameters.
    /// <para>Specific to the experimental algorithm, param1 is used for custom tuning.</para>
    /// </summary>
    /// <param name="param1">Custom tuning parameter (default: 1.0).</param>
    public static SwsAlgorithm Experimental(double param1 = 1.0) => new(SwsFlags.Experimental, param1);

    /// <summary>
    /// Creates a Gaussian scaling algorithm with default parameters.
    /// </summary>
    public static SwsAlgorithm Gauss() => new(SwsFlags.Gauss);

    /// <summary>
    /// Creates a Gaussian scaling algorithm with an adjustable parameter.
    /// <para>
    /// For <see cref="SwsFlags.Gauss"/>, param1 tunes the exponent of the Gaussian function, affecting the cutoff frequency of the filter.
    /// </para>
    /// </summary>
    /// <param name="param1">Tuning parameter for the Gaussian exponent (default: 3.0).</param>
    public static SwsAlgorithm Gauss(double param1 = 3.0) => new(SwsFlags.Gauss, param1);

    /// <summary>
    /// Creates a Lanczos scaling algorithm with default parameters.
    /// </summary>
    public static SwsAlgorithm Lanczos() => new(SwsFlags.Lanczos);

    /// <summary>
    /// Creates a Lanczos scaling algorithm with an adjustable parameter.
    /// <para>
    /// For <see cref="SwsFlags.Lanczos"/>, param1 tunes the width of the window function used by the Lanczos filter.
    /// </para>
    /// </summary>
    /// <param name="param1">Tuning parameter for the width of the Lanczos window function (default: 3).</param>
    public static SwsAlgorithm Lanczos(double param1 = 3) => new(SwsFlags.Lanczos, param1);

    /// <summary>
    /// Creates a Sinc scaling algorithm with no additional tuning.
    /// </summary>
    public static SwsAlgorithm Sinc() => new(SwsFlags.Sinc);

    /// <summary>
    /// Creates a Spline scaling algorithm with no additional tuning.
    /// </summary>
    public static SwsAlgorithm Spline() => new(SwsFlags.Spline);

    /// <summary>
    /// Creates a bicublin scaling algorithm with default parameters.
    /// </summary>
    public static SwsAlgorithm Bicublin() => new(SwsFlags.Bicublin);

    /// <summary>
    /// Creates a bicublin scaling algorithm with adjustable parameters.
    /// <para>
    /// For <see cref="SwsFlags.Bicublin"/>, param1 tunes f(1) and param2 tunes f'(1), similar to the bicubic filter.
    /// </para>
    /// </summary>
    /// <param name="param1">Tuning parameter for f(1) (default: 0).</param>
    /// <param name="param2">Tuning parameter for f'(1) (default: 0.6).</param>
    public static SwsAlgorithm Bicublin(double param1 = 0, double param2 = 0.6) => new(SwsFlags.Bicublin, param1, param2);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SwsAlgorithm algorithm && Equals(algorithm);

    /// <inheritdoc />
    public bool Equals(SwsAlgorithm other) => AlgorithmFlags == other.AlgorithmFlags && Param1 == other.Param1 && Param2 == other.Param2;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(AlgorithmFlags, Param1, Param2);

    public static bool operator ==(SwsAlgorithm left, SwsAlgorithm right) => left.Equals(right);

    public static bool operator !=(SwsAlgorithm left, SwsAlgorithm right) => !(left == right);
}


