using System.Runtime.InteropServices;

namespace FFmpeg.Formats;

/// <summary>
/// Represents an output format in the FFmpeg library.
/// </summary>
/// <remarks>
/// This struct provides access to various properties and methods related to output formats,
/// including supported codecs, default codecs, and format flags.
/// </remarks>
public readonly unsafe struct OutputFormat
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFormat"/> struct.
    /// </summary>
    /// <param name="format">Pointer to the native <see cref="AutoGen._AVOutputFormat"/> structure.</param>
    internal OutputFormat(AutoGen._AVOutputFormat* format) => Format = format;

    /// <summary>
    /// Gets the native <see cref="AutoGen._AVOutputFormat"/> structure.
    /// </summary>
    internal AutoGen._AVOutputFormat* Format { get; }

    /// <summary>
    /// Gets the short name of the output format.
    /// </summary>
    /// <remarks>
    /// The short name is a concise identifier for the format, often used for quick reference.
    /// </remarks>
    public string ShortName => Marshal.PtrToStringUTF8((nint)Format->name);

    /// <summary>
    /// Gets the long name of the output format.
    /// </summary>
    /// <remarks>
    /// The long name provides a more descriptive name for the format compared to the short name.
    /// </remarks>
    public string LongName => Marshal.PtrToStringUTF8((nint)Format->long_name);

    /// <summary>
    /// Gets a read-only list of MIME types associated with the output format.
    /// </summary>
    /// <remarks>
    /// MIME types represent the media type of the format and are useful for content negotiation.
    /// This property retrieves and splits the MIME types from a comma-separated string.
    /// </remarks>
    public IReadOnlyList<string> MimeTypes => Marshal.PtrToStringUTF8((nint)Format->mime_type)?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

    /// <summary>
    /// Gets a read-only list of file extensions associated with the output format.
    /// </summary>
    /// <remarks>
    /// File extensions are used to identify files of this format. This property retrieves and splits
    /// the extensions from a comma-separated string. Returns an empty list if no extensions are available.
    /// </remarks>
    public IReadOnlyList<string> Extensions => Marshal.PtrToStringUTF8((nint)Format->extensions)?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

    /// <summary>
    /// Gets the list of supported codecs for this output format.
    /// </summary>
    /// <remarks>
    /// This property iterates through the codec tags associated with the format to collect the supported
    /// codec IDs.
    /// </remarks>
    public IReadOnlyList<Codecs.CodecTag> SupportedCodecs
    {
        get
        {
            List<Codecs.CodecTag> ids = [];
            for (int i = 0; Format->codec_tag != null && Format->codec_tag[i] != null; i++)
                for (int j = 0; Format->codec_tag[i][j].id != AutoGen._AVCodecID.AV_CODEC_ID_NONE; j++)
                    ids.Add(Format->codec_tag[i][j]);
            return ids;
        }
    }

    /// <summary>
    /// Gets the default audio codec for this output format.
    /// </summary>
    /// <remarks>
    /// This represents the codec used for audio streams by default when the format is selected.
    /// </remarks>
    public AutoGen._AVCodecID DefaultAudioCodec => Format->audio_codec;

    /// <summary>
    /// Gets the default video codec for this output format.
    /// </summary>
    /// <remarks>
    /// This represents the codec used for video streams by default when the format is selected.
    /// </remarks>
    public AutoGen._AVCodecID DefaultVideoCodec => Format->video_codec;

    /// <summary>
    /// Gets the default subtitle codec for this output format.
    /// </summary>
    /// <remarks>
    /// This represents the codec used for subtitle streams by default when the format is selected.
    /// </remarks>
    public AutoGen._AVCodecID DefaultSubtitleCodec => Format->subtitle_codec;

    /// <summary>
    /// Gets the flags associated with this output format.
    /// </summary>
    /// <remarks>
    /// Format flags provide information about the capabilities and properties of the format, such as
    /// whether it supports certain features or has specific characteristics.
    /// </remarks>
    public FormatFlags Flags => (FormatFlags)Format->flags;

    /// <summary>
    /// Finds an output format by its short name.
    /// </summary>
    /// <param name="short_name">The short name of the output format.</param>
    /// <returns>
    /// An <see cref="OutputFormat"/> instance if found; otherwise, <c>null</c>.
    /// </returns>
    public static OutputFormat? FindFormat(string short_name) => Create(ffmpeg.av_guess_format(short_name, null, null));

    /// <summary>
    /// Guesses an output format based on the provided parameters.
    /// </summary>
    /// <param name="short_name">The short name of the format.</param>
    /// <param name="filename">The filename to guess the format from.</param>
    /// <param name="mime_type">The MIME type to guess the format from.</param>
    /// <returns>
    /// An <see cref="OutputFormat"/> instance if found; otherwise, <c>null</c>.
    /// </returns>
    public static OutputFormat? GuessOutputFormat(string? short_name = null, string? filename = null, string? mime_type = null) => Create(ffmpeg.av_guess_format(short_name, filename, mime_type));

    /// <summary>
    /// Creates an instance of <see cref="OutputFormat"/> from a pointer.
    /// </summary>
    /// <param name="v">Pointer to the native <see cref="AutoGen._AVOutputFormat"/> structure.</param>
    /// <returns>
    /// An instance of <see cref="OutputFormat"/> if the pointer is not null; otherwise, <c>null</c>.
    /// </returns>
    private static OutputFormat? Create(AutoGen._AVOutputFormat* v) => v != null ? new OutputFormat(v) : null;

    /// <summary>
    /// Retrieves a collection of available AV output formats.
    /// </summary>
    /// <returns>
    /// An <see cref="IReadOnlyList{AVOutputFormat}"/> containing all available output formats.
    /// </returns>
    /// <remarks>
    /// This method uses the FFmpeg library to iterate through all available muxers and collects
    /// them into a list of <see cref="OutputFormat"/> objects.
    /// </remarks>
    public static IReadOnlyList<OutputFormat> GetOutputFormats()
    {
        List<OutputFormat> formats = [];
        void* opaque = null;
        AutoGen._AVOutputFormat* format;
        while ((format = ffmpeg.av_muxer_iterate(&opaque)) != null)
            formats.Add(new(format));
        return formats;
    }

    /// <summary>
    /// Returns a string representation of the output format.
    /// </summary>
    /// <returns>
    /// The <see cref="LongName"/> of the output format.
    /// </returns>
    public override string ToString() => LongName;

    // Predefined output formats

    /// <summary>
    /// Gets the AVI output format.
    /// </summary>
    public static OutputFormat? AVI => FindFormat("avi");                    // AVI

    /// <summary>
    /// Gets the MP4 output format (MPEG-4 Part 14).
    /// </summary>
    public static OutputFormat? MP4 => FindFormat("mp4");                    // MP4

    /// <summary>
    /// Gets the MOV output format (QuickTime / MOV).
    /// </summary>
    public static OutputFormat? MOV => FindFormat("mov");                    // QuickTime / MOV

    /// <summary>
    /// Gets the Matroska output format.
    /// </summary>
    public static OutputFormat? MKV => FindFormat("matroska");               // Matroska

    /// <summary>
    /// Gets the WebM output format.
    /// </summary>
    public static OutputFormat? WEBM => FindFormat("webm");                  // WebM

    /// <summary>
    /// Gets the Flash Video output format.
    /// </summary>
    public static OutputFormat? FLV => FindFormat("flv");                    // Flash Video

    /// <summary>
    /// Gets the MPEG output format (MPEG-1 Systems / MPEG program stream).
    /// </summary>
    public static OutputFormat? MPEG => FindFormat("mpeg");                  // MPEG-1 Systems

    /// <summary>
    /// Gets the MPEG-TS output format (MPEG-2 Transport Stream).
    /// </summary>
    public static OutputFormat? MPEGTS => FindFormat("mpegts");              // MPEG-TS

    /// <summary>
    /// Gets the Ogg output format.
    /// </summary>
    public static OutputFormat? OGG => FindFormat("ogg");                    // Ogg

    /// <summary>
    /// Gets the Oga output format.
    /// </summary>
    public static OutputFormat? OGA => FindFormat("oga");                    // Oga

    /// <summary>
    /// Gets the Ogv output format.
    /// </summary>
    public static OutputFormat? OGV => FindFormat("ogv");                    // Ogv

    // Audio Formats

    /// <summary>
    /// Gets the MP3 output format (MPEG audio layer 3).
    /// </summary>
    public static OutputFormat? MP3 => FindFormat("mp3");                    // MP3

    /// <summary>
    /// Gets the WAV output format (Waveform Audio).
    /// </summary>
    public static OutputFormat? WAV => FindFormat("wav");                    // WAV

    /// <summary>
    /// Gets the ADTS AAC output format (Advanced Audio Coding).
    /// </summary>
    public static OutputFormat? AAC => FindFormat("adts");                   // ADTS AAC

    /// <summary>
    /// Gets the FLAC output format (Free Lossless Audio Codec).
    /// </summary>
    public static OutputFormat? FLAC => FindFormat("flac");                  // FLAC

    /// <summary>
    /// Gets the Apple Lossless Audio Codec (ALAC) output format.
    /// </summary>
    public static OutputFormat? ALAC => FindFormat("caf");                   // ALAC

    // Image Formats

    /// <summary>
    /// Gets the PNG output format (Portable Network Graphics).
    /// </summary>
    public static OutputFormat? PNG => FindFormat("image2");                 // PNG

    /// <summary>
    /// Gets the JPEG output format (Joint Photographic Experts Group).
    /// </summary>
    public static OutputFormat? JPEG => FindFormat("image2");                // JPEG

    /// <summary>
    /// Gets the BMP output format (Bitmap).
    /// </summary>
    public static OutputFormat? BMP => FindFormat("image2");                 // BMP

    /// <summary>
    /// Gets the TIFF output format (Tagged Image File Format).
    /// </summary>
    public static OutputFormat? TIFF => FindFormat("image2");                // TIFF

    /// <summary>
    /// Gets the GIF output format (CompuServe Graphics Interchange Format).
    /// </summary>
    public static OutputFormat? GIF => FindFormat("gif");                    // GIF

    /// <summary>
    /// Gets the AVIF output format (AV1 Image File Format).
    /// </summary>
    public static OutputFormat? AVIF => FindFormat("avif");                  // AVIF

    /// <summary>
    /// Gets the WebP output format.
    /// </summary>
    public static OutputFormat? WebP => FindFormat("webp");                  // WebP

    /// <summary>
    /// Gets the JPEG XL output format.
    /// </summary>
    public static OutputFormat? JPEGXL => FindFormat("image2");              // JPEG XL

    /// <summary>
    /// Gets the APNG output format.
    /// </summary>
    public static OutputFormat? APNG => FindFormat("apng");

    // Pipe Formats

    /// <summary>
    /// Gets the piped PNG output format.
    /// </summary>
    public static OutputFormat? PNG_Pipe => FindFormat("image2pipe");         // PNG (Piped)

    /// <summary>
    /// Gets the piped JPEG output format.
    /// </summary>
    public static OutputFormat? JPEG_Pipe => FindFormat("image2pipe");        // JPEG (Piped)

    /// <summary>
    /// Gets the piped BMP output format.
    /// </summary>
    public static OutputFormat? BMP_Pipe => FindFormat("image2pipe");         // BMP (Piped)

    /// <summary>
    /// Gets the piped TIFF output format.
    /// </summary>
    public static OutputFormat? TIFF_Pipe => FindFormat("image2pipe");        // TIFF (Piped)

    // Subtitle and Streaming Formats

    /// <summary>
    /// Gets the SSA (SubStation Alpha) subtitle output format.
    /// </summary>
    public static OutputFormat? ASS => FindFormat("ass");                    // SSA/ASS

    /// <summary>
    /// Gets the SubRip subtitle output format.
    /// </summary>
    public static OutputFormat? SRT => FindFormat("srt");                    // SubRip Subtitle

    /// <summary>
    /// Gets the HTTP Live Streaming (HLS) output format.
    /// </summary>
    public static OutputFormat? HLS => FindFormat("hls");                    // HTTP Live Streaming

    /// <summary>
    /// Gets the DASH (Dynamic Adaptive Streaming over HTTP) output format.
    /// </summary>
    public static OutputFormat? DASH => FindFormat("dash");                  // DASH
}


