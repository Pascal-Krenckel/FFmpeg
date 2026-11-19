using FFmpeg.Codecs.Video;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Codecs.Image;

/// <summary>
/// Base class for typed settings of still image encoders such as <see cref="Png"/> or <see cref="Jpeg"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Encode(AVFrame)"/> and <see cref="Save(AVFrame, Stream, bool)"/> write a single frame as an image file:
/// the size and pixel format are taken from the frame when they are not set, and frames that do not match are
/// converted (the frame passed in is not changed).
/// </para>
/// <para>
/// The settings are also <see cref="VideoCodec"/> settings, so they can be used with the <see cref="Transcoder"/> or a
/// <see cref="Formats.MuxerContext"/> to write image sequences (e.g. with the <c>image2</c> muxer).
/// For AVIF use <see cref="SVTAV1.CreateAvif(Stream, IEnumerable{AVFrame}, bool, bool)"/>.
/// </para>
/// </remarks>
public abstract class ImageCodec : VideoCodec
{
    /// <summary>
    /// Initializes the settings without a size; it is taken from the frame.
    /// </summary>
    /// <param name="codec">The image encoder.</param>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is not a video encoder.</exception>
    protected ImageCodec(Codec codec) : base(codec)
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="codec">The image encoder.</param>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is not a video encoder.</exception>
    protected ImageCodec(Codec codec, int width, int height) : base(codec, width, height, Rational.Zero)
    {
    }

    /// <summary>
    /// Gets or sets the scaling algorithm for frames that have to be converted; bicubic by default.
    /// </summary>
    public SwsAlgorithm ScalingAlgorithm { get; set; } = SwsAlgorithm.Bicubic();

    /// <summary>
    /// Encodes a frame as an image file.
    /// </summary>
    /// <param name="frame">The frame; it is not changed.</param>
    /// <returns>The file content.</returns>
    /// <exception cref="ArgumentException">The encoder rejects a setting.</exception>
    public byte[] Encode(AVFrame frame)
    {
        using MemoryStream stream = new();
        Save(frame, stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Encodes a frame and writes it to a file.
    /// </summary>
    /// <param name="frame">The frame; it is not changed.</param>
    /// <param name="path">The file to create or overwrite.</param>
    /// <exception cref="ArgumentException">The encoder rejects a setting.</exception>
    public void Save(AVFrame frame, string path)
    {
        using FileStream stream = File.Create(path);
        Save(frame, stream);
    }

    /// <summary>
    /// Encodes a frame and writes the image file to a stream.
    /// </summary>
    /// <param name="frame">The frame; it is not changed.</param>
    /// <param name="output">The stream to write to.</param>
    /// <param name="closeStream"><see langword="true"/> to close <paramref name="output"/> afterwards.</param>
    /// <exception cref="ArgumentException">The encoder rejects a setting.</exception>
    public void Save(AVFrame frame, Stream output, bool closeStream = false)
    {
        try
        {
            using CodecContext encoder = CreateCodecContext();
            encoder.Width = Width > 0 ? Width : frame.Width;
            encoder.Height = Height > 0 ? Height : frame.Height;
            if (PixelFormat == PixelFormat.None)
                encoder.PixelFormat = ChoosePixelFormat(frame.PixelFormat);
            if (!encoder.TimeBase.IsValidTimeBase)
                encoder.TimeBase = frame.TimeBase.IsValidTimeBase ? frame.TimeBase : new Rational(1, 25);
            if (encoder.SampleAspectRatio.Numerator == 0 && frame.PixelAspectRatio.Numerator != 0
                && encoder.Width == frame.Width && encoder.Height == frame.Height)
                encoder.SampleAspectRatio = frame.PixelAspectRatio;
            // Like ffmpeg: convert to a color range the encoder accepts (e.g. mjpeg only takes full range).
            ReadOnlySpan<ColorRange> ranges = encoder.SupportedColorRanges;
            if (encoder.ColorRange == ColorRange.Unspecified && !ranges.IsEmpty && !Contains(ranges, frame.ColorRange))
                encoder.ColorRange = ranges[0];

            using AVFrame input = AVFrame.Allocate();
            if (frame.Width == encoder.Width && frame.Height == encoder.Height && frame.PixelFormat == encoder.PixelFormat
                && (encoder.ColorRange == ColorRange.Unspecified || frame.ColorRange == encoder.ColorRange))
            {
                input.Reference(frame);
            }
            else
            {
                ConvertFrame(frame, encoder, input);
            }

            // Like the ffmpeg tool: the color properties of the frame describe the image unless they are set.
            if (encoder.ColorSpace == ColorSpace.Unspecified)
                encoder.ColorSpace = input.ColorSpace;
            if (encoder.ColorRange == ColorRange.Unspecified)
                encoder.ColorRange = input.ColorRange;
            if (encoder.ColorPrimaries == ColorPrimaries.Unspecified)
                encoder.ColorPrimaries = input.ColorPrimaries;
            if (encoder.ColorTransferCharacteristic == ColorTransferCharacteristic.Unspecified)
                encoder.ColorTransferCharacteristic = input.ColorTransferCharacteristic;
            encoder.Open(null).ThrowIfError();

            // A single image: no timestamps of a sequence, and no picture type from a decoder. Like ffmpeg, the
            // frame carries the fixed quantizer (-q:v), which encoders such as mjpeg read from there.
            input.PresentationTimestamp = 0;
            input.PictureType = PictureType.None;
            input.Quality = new Quality(encoder.GlobalQuality);

            using AVPacket packet = AVPacket.Allocate();
            encoder.SendFrame(input).ThrowIfError();
            encoder.SendFrame(null).ThrowIfError();
            AVResult32 result;
            while (!(result = encoder.ReceivePacket(packet)).IsError)
            {
                output.Write(packet.Data);
                packet.Unreference();
            }
            if (result != AVResult32.EndOfFile)
                result.ThrowIfError();
            WriteEnd(output);
            output.Flush();
        }
        finally
        {
            if (closeStream)
                output.Dispose();
        }
    }

    /// <summary>
    /// Chooses the pixel format for a frame when <see cref="VideoCodec.PixelFormat"/> is not set.
    /// </summary>
    /// <param name="source">The pixel format of the frame.</param>
    /// <returns>The pixel format of the encoder.</returns>
    private protected virtual PixelFormat ChoosePixelFormat(PixelFormat source) => Codec.GetBestPixelFormat(source);

    /// <summary>
    /// Converts <paramref name="source"/> to the size, pixel format and (if set) color range of the encoder.
    /// </summary>
    /// <param name="source">The frame to convert.</param>
    /// <param name="encoder">The encoder context.</param>
    /// <param name="destination">An empty frame that receives the converted image.</param>
    private protected virtual void ConvertFrame(AVFrame source, CodecContext encoder, AVFrame destination)
    {
        source.CopyPropertiesTo(destination);
        // The color properties describe the source format; let the scaler set those of the new format.
        destination.ColorSpace = ColorSpace.Unspecified;
        destination.ColorRange = encoder.ColorRange;
        destination.ChromaLocation = ChromaLocation.Unspecified;
        destination.Width = encoder.Width;
        destination.Height = encoder.Height;
        destination.PixelFormat = encoder.PixelFormat;
        destination.CreateBuffer().ThrowIfError();
        using SwsContext scaler = new(ScalingAlgorithm);
        scaler.Convert(source, destination).ThrowIfError();
    }

    /// <summary>
    /// Writes what the file needs after the encoded data (e.g. the GIF trailer).
    /// </summary>
    /// <param name="output">The output stream.</param>
    private protected virtual void WriteEnd(Stream output) { }
}
