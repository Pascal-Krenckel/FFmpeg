using FFmpeg.AutoGen;

namespace FFmpeg.Options;

/// <summary>
/// Represents the different types of options available in the FFmpeg library.
/// </summary>
public enum OptionType : int
{
    /// <summary>
    /// A set of bit flags.
    /// </summary>
    Flags = _AVOptionType.AV_OPT_TYPE_FLAGS,

    /// <summary>
    /// A 32-bit integer value.
    /// </summary>
    Int32 = _AVOptionType.AV_OPT_TYPE_INT,

    /// <summary>
    /// A 64-bit integer value.
    /// </summary>
    Int64 = _AVOptionType.AV_OPT_TYPE_INT64,

    /// <summary>
    /// A double-precision floating-point value.
    /// </summary>
    Double = _AVOptionType.AV_OPT_TYPE_DOUBLE,

    /// <summary>
    /// A single-precision floating-point value.
    /// </summary>
    Float = _AVOptionType.AV_OPT_TYPE_FLOAT,

    /// <summary>
    /// A string of characters.
    /// </summary>
    String = _AVOptionType.AV_OPT_TYPE_STRING,

    /// <summary>
    /// A rational number, represented as a pair of integers (numerator and denominator).
    /// </summary>
    Rational = _AVOptionType.AV_OPT_TYPE_RATIONAL,

    /// <summary>
    /// Binary data, with length specified by an integer following a pointer.
    /// </summary>
    Binary = _AVOptionType.AV_OPT_TYPE_BINARY,

    /// <summary>
    /// A dictionary of key-value pairs.
    /// </summary>
    Dictionary = _AVOptionType.AV_OPT_TYPE_DICT,

    /// <summary>
    /// A 64-bit unsigned integer value.
    /// </summary>
    UInt64 = _AVOptionType.AV_OPT_TYPE_UINT64,

    /// <summary>
    /// A constant value.
    /// </summary>
    Constant = _AVOptionType.AV_OPT_TYPE_CONST,

    /// <summary>
    /// An image size, where the offset points to two consecutive integers.
    /// </summary>
    ImageSize = _AVOptionType.AV_OPT_TYPE_IMAGE_SIZE,

    /// <summary>
    /// A pixel format.
    /// </summary>
    PixelFormat = _AVOptionType.AV_OPT_TYPE_PIXEL_FMT,

    /// <summary>
    /// A sample format.
    /// </summary>
    SampleFormat = _AVOptionType.AV_OPT_TYPE_SAMPLE_FMT,

    /// <summary>
    /// A video rate, where the offset points to an _AVRational structure.
    /// </summary>
    VideoRate = _AVOptionType.AV_OPT_TYPE_VIDEO_RATE,

    /// <summary>
    /// A duration value.
    /// </summary>
    Duration = _AVOptionType.AV_OPT_TYPE_DURATION,

    /// <summary>
    /// A color value.
    /// </summary>
    Color = _AVOptionType.AV_OPT_TYPE_COLOR,

    /// <summary>
    /// A boolean value (true/false).
    /// </summary>
    Bool = _AVOptionType.AV_OPT_TYPE_BOOL,

    /// <summary>
    /// A channel layout.
    /// </summary>
    ChannelLayout = _AVOptionType.AV_OPT_TYPE_CHLAYOUT,

    /// <summary>
    /// An unsigned integer value.
    /// </summary>
    UInt32 = _AVOptionType.AV_OPT_TYPE_UINT,

    /// <summary>
    /// Indicates that the option type is an array of the described type.
    /// </summary>
    ArrayFlag = _AVOptionType.AV_OPT_TYPE_FLAG_ARRAY,

    /// <summary>
    /// An array of 32-bit integers.
    /// </summary>
    Int32Array = Int32 | ArrayFlag,

    /// <summary>
    /// An array of 64-bit integers.
    /// </summary>
    Int64Array = Int64 | ArrayFlag,

    /// <summary>
    /// An array of double-precision floating-point values.
    /// </summary>
    DoubleArray = Double | ArrayFlag,

    /// <summary>
    /// An array of single-precision floating-point values.
    /// </summary>
    FloatArray = Float | ArrayFlag,

    /// <summary>
    /// An array of strings.
    /// </summary>
    StringArray = String | ArrayFlag,

    /// <summary>
    /// An array of rational numbers.
    /// </summary>
    RationalArray = Rational | ArrayFlag,

    /// <summary>
    /// An array of binary data.
    /// </summary>
    BinaryArray = Binary | ArrayFlag,

    /// <summary>
    /// An array of dictionary values.
    /// </summary>
    DictionaryArray = Dictionary | ArrayFlag,

    /// <summary>
    /// An array of 64-bit unsigned integers.
    /// </summary>
    UInt64Array = UInt64 | ArrayFlag,

    /// <summary>
    /// An array of constants.
    /// </summary>
    ConstantArray = Constant | ArrayFlag,

    /// <summary>
    /// An array of image sizes.
    /// </summary>
    ImageSizeArray = ImageSize | ArrayFlag,

    /// <summary>
    /// An array of pixel formats.
    /// </summary>
    PixelFormatArray = PixelFormat | ArrayFlag,

    /// <summary>
    /// An array of sample formats.
    /// </summary>
    SampleFormatArray = SampleFormat | ArrayFlag,

    /// <summary>
    /// An array of video rates.
    /// </summary>
    VideoRateArray = VideoRate | ArrayFlag,

    /// <summary>
    /// An array of duration values.
    /// </summary>
    DurationArray = Duration | ArrayFlag,

    /// <summary>
    /// An array of color values.
    /// </summary>
    ColorArray = Color | ArrayFlag,

    /// <summary>
    /// An array of boolean values.
    /// </summary>
    BoolArray = Bool | ArrayFlag,

    /// <summary>
    /// An array of channel layouts.
    /// </summary>
    ChannelLayoutArray = ChannelLayout | ArrayFlag,

    /// <summary>
    /// An array of unsigned integer values.
    /// </summary>
    UInt32Array = UInt32 | ArrayFlag
}


