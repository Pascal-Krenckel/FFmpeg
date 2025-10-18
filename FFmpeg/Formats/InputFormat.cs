using System.Runtime.InteropServices;

namespace FFmpeg.Formats;

/// <summary>
/// Represents an AV input format, wrapping around an <see cref="AutoGen._AVInputFormat"/> pointer.
/// </summary>
/// <remarks>
/// This struct provides access to the properties and methods associated with an AV input format.
/// </remarks>
public readonly unsafe struct InputFormat
{
    /// <summary>
    /// Gets the underlying <see cref="AutoGen._AVInputFormat"/> pointer.
    /// </summary>
    internal AutoGen._AVInputFormat* Format { get; }

    /// <summary>
    /// Gets the short names associated with the input format.
    /// </summary>
    /// <remarks>
    /// Short names are often used as identifiers or abbreviations for the format. This property retrieves
    /// and splits the short names from a comma-separated string.
    /// </remarks>
    public IEnumerable<string> ShortNames => Marshal.PtrToStringUTF8((nint)Format->name).Split(',', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Gets the long name of the input format.
    /// </summary>
    /// <remarks>
    /// The long name provides a more descriptive name for the format compared to the short names.
    /// </remarks>
    public string LongName => Marshal.PtrToStringUTF8((nint)Format->long_name);

    /// <summary>
    /// Gets the file extensions associated with the input format.
    /// </summary>
    /// <remarks>
    /// File extensions are used to identify files of this format. This property retrieves and splits
    /// the extensions from a comma-separated string. Returns an empty list if no extensions are available.
    /// </remarks>
    public IReadOnlyList<string> Extensions => Marshal.PtrToStringUTF8((nint)Format->extensions)?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

    /// <summary>
    /// Gets the flags associated with the input format.
    /// </summary>
    /// <remarks>
    /// Flags represent various properties and capabilities of the format, such as whether it supports
    /// seeking or demuxing.
    /// </remarks>
    public FormatFlags Flags => (FormatFlags)Format->flags;

    /// <summary>
    /// Gets the MIME types associated with the input format.
    /// </summary>
    /// <remarks>
    /// MIME types are used to indicate the media type of the format. This property retrieves and splits
    /// the MIME types from a comma-separated string. Returns an empty list if no MIME types are available.
    /// </remarks>
    public IReadOnlyList<string> MimeTypes => Marshal.PtrToStringUTF8((nint)Format->mime_type)?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

    /// <summary>
    /// Gets the list of supported codecs for this input format.
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
            {
                for (int j = 0; Format->codec_tag[i][j].id != AutoGen._AVCodecID.AV_CODEC_ID_NONE; j++)
                    ids.Add(Format->codec_tag[i][j]);
            }

            return ids;
        }
    }


    /// <summary>
    /// Finds an <see cref="InputFormat"/> by its short name.
    /// </summary>
    /// <param name="short_name">The short name of the input format to find.</param>
    /// <returns>
    /// An instance of <see cref="InputFormat"/> if found; otherwise, null.
    /// </returns>
    public static InputFormat? FindFormat(string short_name) => Create(ffmpeg.av_find_input_format(short_name));

    /// <summary>
    /// Creates an instance of <see cref="InputFormat"/> from a pointer.
    /// </summary>
    /// <param name="v">Pointer to an <see cref="AutoGen._AVInputFormat"/> instance.</param>
    /// <returns>
    /// An instance of <see cref="InputFormat"/> if the pointer is not null; otherwise, null.
    /// </returns>
    private static InputFormat? Create(AutoGen._AVInputFormat* v) => v == null ? null : new InputFormat(v);

    /// <summary>
    /// Retrieves a collection of all available AV input formats.
    /// </summary>
    /// <returns>
    /// An <see cref="IReadOnlyList{AVInputFormat}"/> containing all available input formats.
    /// </returns>
    /// <remarks>
    /// This method uses the FFmpeg library to iterate through all available demuxers and collects
    /// them into a list of <see cref="InputFormat"/> objects.
    /// </remarks>
    public static IReadOnlyList<InputFormat> GetInputFormats()
    {
        List<InputFormat> formats = [];
        void* opaque = null;
        AutoGen._AVInputFormat* format;
        while ((format = ffmpeg.av_demuxer_iterate(&opaque)) != null)
            formats.Add(new(format));
        return formats;
    }

    /// <summary>
    /// Returns a string representation of the input format.
    /// </summary>
    /// <returns>
    /// The <see cref="LongName"/> of the input format.
    /// </returns>
    public override string ToString() => LongName;

    // Predefined AV input formats

    /// <summary>
    /// Gets the Matroska/WebM input format.
    /// </summary>
    public static InputFormat? MKV => FindFormat("matroska");   // Matroska/WebM

    /// <summary>
    /// Gets the MP4 input format (QuickTime / MOV / MP4 / 3GP / M4A / 3G2 / MJ2).
    /// </summary>
    public static InputFormat? MP4 => FindFormat("mov");        // QuickTime / MOV / MP4 / 3GP / M4A / 3G2 / MJ2

    /// <summary>
    /// Gets the AVI input format.
    /// </summary>
    public static InputFormat? AVI => FindFormat("avi");        // AVI

    /// <summary>
    /// Gets the Flash Video input format.
    /// </summary>
    public static InputFormat? FLV => FindFormat("flv");        // Flash Video

    /// <summary>
    /// Gets the MOV input format (alias for MP4).
    /// </summary>
    public static InputFormat? MOV => FindFormat("mov");        // Alias for MP4

    /// <summary>
    /// Gets the WebM input format (alias for Matroska).
    /// </summary>
    public static InputFormat? WEBM => FindFormat("webm");      // Alias for MKV

    /// <summary>
    /// Gets the MPEG-PS input format.
    /// </summary>
    public static InputFormat? MPEG => FindFormat("mpeg");      // MPEG-PS

    /// <summary>
    /// Gets the MPEG-TS input format.
    /// </summary>
    public static InputFormat? MPEGTS => FindFormat("mpegts");  // MPEG-TS

    /// <summary>
    /// Gets the ASF input format.
    /// </summary>
    public static InputFormat? ASF => FindFormat("asf");        // ASF

    /// <summary>
    /// Gets the raw video input format.
    /// </summary>
    public static InputFormat? RAWVIDEO => FindFormat("rawvideo"); // Raw Video

    // Audio input formats

    /// <summary>
    /// Gets the MP3 input format (MP2/MP3).
    /// </summary>
    public static InputFormat? MP3 => FindFormat("mp3");        // MP2/MP3

    /// <summary>
    /// Gets the WAV input format.
    /// </summary>
    public static InputFormat? WAV => FindFormat("wav");        // WAV/WAVE

    /// <summary>
    /// Gets the FLAC input format.
    /// </summary>
    public static InputFormat? FLAC => FindFormat("flac");      // Raw FLAC

    /// <summary>
    /// Gets the AAC input format.
    /// </summary>
    public static InputFormat? AAC => FindFormat("aac");        // Raw AAC

    /// <summary>
    /// Gets the Ogg input format.
    /// </summary>
    public static InputFormat? OGG => FindFormat("ogg");        // Ogg

    /// <summary>
    /// Gets the Ogg input format (alias for Ogg).
    /// </summary>
    public static InputFormat? OGA => FindFormat("ogg");        // Ogg

    /// <summary>
    /// Gets the Ogg input format (alias for Ogg).
    /// </summary>
    public static InputFormat? OGV => FindFormat("ogg");        // Ogg

    /// <summary>
    /// Gets the raw AC-3 input format.
    /// </summary>
    public static InputFormat? AC3 => FindFormat("ac3");        // Raw AC-3

    /// <summary>
    /// Gets the M4A input format (alias for MP4/MOV).
    /// </summary>
    public static InputFormat? M4A => FindFormat("mov");        // Alias for MP4/MOV

    /// <summary>
    /// Gets the CRI AAX input format.
    /// </summary>
    public static InputFormat? AAX => FindFormat("aax");        // CRI AAX

    /// <summary>
    /// Gets the WMA input format (inside ASF).
    /// </summary>
    public static InputFormat? WMA => FindFormat("asf");        // WMA (inside ASF)

    // Streaming input formats

    /// <summary>
    /// Gets the HTTP Live Streaming input format.
    /// </summary>
    public static InputFormat? HLS => FindFormat("hls");        // HTTP Live Streaming

    /// <summary>
    /// Gets the RTSP input format.
    /// </summary>
    public static InputFormat? RTSP => FindFormat("rtsp");      // RTSP input

    /// <summary>
    /// Gets the DASH input format.
    /// </summary>
    public static InputFormat? DASH => FindFormat("dash");      // DASH

    /// <summary>
    /// Gets the RTP input format.
    /// </summary>
    public static InputFormat? RTP => FindFormat("rtp");        // RTP input

    // Subtitle input formats

    /// <summary>
    /// Gets the SubRip subtitle input format.
    /// </summary>
    public static InputFormat? SRT => FindFormat("srt");        // SubRip

    /// <summary>
    /// Gets the SSA/ASS subtitle input format.
    /// </summary>
    public static InputFormat? ASS => FindFormat("ass");        // SSA/ASS subtitles

    /// <summary>
    /// Gets the SSA subtitle input format (alias for ASS).
    /// </summary>
    public static InputFormat? SSA => FindFormat("ass");        // Alias for ASS

    // Piped image formats (handled as codecs and piped sequences)

    /// <summary>
    /// Gets the piped BMP sequence input format.
    /// </summary>
    public static InputFormat? BMP_PIPE => FindFormat("bmp_pipe");      // Piped BMP sequence

    /// <summary>
    /// Gets the piped PNG sequence input format.
    /// </summary>
    public static InputFormat? PNG_PIPE => FindFormat("png_pipe");      // Piped PNG sequence

    /// <summary>
    /// Gets the piped JPEG sequence input format.
    /// </summary>
    public static InputFormat? JPEG_PIPE => FindFormat("jpeg_pipe");    // Piped JPEG sequence

    /// <summary>
    /// Gets the piped TIFF sequence input format.
    /// </summary>
    public static InputFormat? TIFF_PIPE => FindFormat("tiff_pipe");    // Piped TIFF sequence

    /// <summary>
    /// Gets the piped WEBP sequence input format.
    /// </summary>
    public static InputFormat? WEBP_PIPE => FindFormat("webp_pipe");    // Piped WEBP sequence

    /// <summary>
    /// Gets the piped JPEG XL sequence input format.
    /// </summary>
    public static InputFormat? JPEGXL_PIPE => FindFormat("jpegxl_pipe"); // Piped JPEG XL sequence

    // Specialized input formats

    /// <summary>
    /// Gets the YUV4MPEG input format.
    /// </summary>
    public static InputFormat? YUV4MPEGPIPE => FindFormat("yuv4mpegpipe"); // YUV4MPEG

    /// <summary>
    /// Gets the Matroska/WebM input format (alias for MKV/WebM).
    /// </summary>
    public static InputFormat? MATROSKA => FindFormat("matroska");  // Alias for MKV/WebM

    /// <param name="format">Pointer to an <see cref="AutoGen._AVInputFormat"/> instance that this struct will wrap.</param>
    internal InputFormat(AutoGen._AVInputFormat* format) => Format = format;
}


