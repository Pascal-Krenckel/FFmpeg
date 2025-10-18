namespace FFmpeg.Codecs.Video.LibSVTAV1;

/// <summary>
/// Specifies the mode for handling screen content in the SVT-AV1 encoder.
/// Screen content refers to video content that primarily consists of static images, text, or graphical elements (such as computer-generated content).
/// </summary>
public enum ScreenContentMode
{
    /// <summary>
    /// Screen content optimizations are disabled.
    /// </summary>
    Off = 0,

    /// <summary>
    /// Screen content optimizations are enabled, improving encoding efficiency for static and graphical content.
    /// </summary>
    On = 1,

    /// <summary>
    /// Enables content-adaptive screen mode, where the encoder automatically decides whether to apply screen content optimizations based on the input.
    /// </summary>
    ContentAdaptive = 2
};

