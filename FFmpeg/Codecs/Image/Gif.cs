using FFmpeg.Filters;
using FFmpeg.Filters.VideoFilters;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Globalization;

namespace FFmpeg.Codecs.Image;

/// <summary>
/// Settings for FFmpeg's GIF encoder (<c>gif</c>).
/// </summary>
/// <remarks>
/// <para>
/// GIF stores at most 256 colors. <see cref="ImageCodec.Encode(AVFrame)"/> and <see cref="ImageCodec.Save(AVFrame, Stream, bool)"/>
/// use PAL8 unless another pixel format is set, and compute an optimal palette for the image with the <c>palettegen</c> and
/// <c>paletteuse</c> filters (like <c>ffmpeg -vf "split[a][b];[a]palettegen[p];[b][p]paletteuse"</c>);
/// see <see cref="MaxColors"/> and <see cref="Dither"/>.
/// </para>
/// <para>
/// Other supported pixel formats: RGB8, BGR8, RGB4_BYTE, BGR4_BYTE (fixed palettes) and Gray8. When the settings are
/// used to encode a sequence (e.g. with the <see cref="Transcoder"/>), the frames are converted with the scaler and
/// a fixed palette instead; use the palette filters in the filter graph for better quality.
/// </para>
/// </remarks>
public class Gif : ImageCodec
{
    /// <summary>
    /// Initializes the settings; the size is taken from the frame.
    /// </summary>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the GIF encoder.</exception>
    public Gif() : base(FindEncoder("gif"))
    {
    }

    /// <summary>
    /// Initializes the settings with a size; frames of another size are scaled.
    /// </summary>
    /// <param name="width">The width in pixels; 0 to take it from the frame.</param>
    /// <param name="height">The height in pixels; 0 to take it from the frame.</param>
    /// <exception cref="NotSupportedException">The loaded FFmpeg libraries do not contain the GIF encoder.</exception>
    public Gif(int width, int height) : base(FindEncoder("gif"), width, height)
    {
    }

    /// <summary>
    /// Gets or sets the maximum number of colors of the generated palette (<c>palettegen</c> option <c>max_colors</c>),
    /// 2 to 256 (the default). One entry is kept for transparency.
    /// </summary>
    public int? MaxColors { get; set; }

    /// <summary>
    /// Gets or sets the dithering when the palette is applied (<c>paletteuse</c> option <c>dither</c>);
    /// <see cref="GifDither.Sierra2_4A"/> by default.
    /// </summary>
    public GifDither? Dither { get; set; }

    /// <summary>
    /// Gets or sets whether a palette is written to the global header where possible (<c>global_palette</c>); on by default.
    /// </summary>
    public bool? GlobalPalette { get; set; }

    /// <inheritdoc/>
    protected override void CollectOptions(EncoderOptions options)
    {
        if (GlobalPalette.HasValue)
            options.SetPrivate("global_palette", GlobalPalette.Value);
    }

    /// <inheritdoc/>
    private protected override PixelFormat ChoosePixelFormat(PixelFormat source)
        => source is PixelFormat.Gray8 or PixelFormat.PAL8 ? source : PixelFormat.PAL8;

    /// <inheritdoc/>
    private protected override void ConvertFrame(AVFrame source, CodecContext encoder, AVFrame destination)
    {
        if (encoder.PixelFormat != PixelFormat.PAL8)
        {
            base.ConvertFrame(source, encoder, destination);
            return;
        }

        using FilterGraph graph = FilterGraph.Allocate();
        Rational timeBase = source.TimeBase.IsValidTimeBase ? source.TimeBase : new Rational(1, 25);
        VideoBufferSource input = VideoBufferSource.Create("in", source.Width, source.Height, source.PixelFormat, timeBase, graph);
        VideoBufferSink output = VideoBufferSink.Create("out", graph, PixelFormat.PAL8);
        FilterContext scale = FilterContext.Create("scale", Filter.Scale,
            string.Format(CultureInfo.InvariantCulture, "w={0}:h={1}", encoder.Width, encoder.Height), graph);
        FilterContext split = FilterContext.Create("split", Filter.GetFilterByName("split"), "2", graph);
        FilterContext paletteGen = FilterContext.Create("palettegen", Filter.GetFilterByName("palettegen"),
            string.Format(CultureInfo.InvariantCulture, "max_colors={0}:stats_mode=single", MaxColors ?? 256), graph);
        FilterContext paletteUse = FilterContext.Create("paletteuse", Filter.GetFilterByName("paletteuse"),
            Dither.HasValue ? $"dither={Dither.Value.Name()}" : string.Empty, graph);
        graph.Link(input, 0, scale, 0).ThrowIfError();
        graph.Link(scale, 0, split, 0).ThrowIfError();
        graph.Link(split, 0, paletteGen, 0).ThrowIfError();
        graph.Link(split, 1, paletteUse, 0).ThrowIfError();
        graph.Link(paletteGen, 0, paletteUse, 1).ThrowIfError();
        graph.Link(paletteUse, 0, output, 0).ThrowIfError();
        graph.Config().ThrowIfError();

        // palettegen needs the end of the input before it outputs the palette.
        input.SendFrame(source, keepRef: true).ThrowIfError();
        input.Drain().ThrowIfError();
        output.ReceiveFrame(destination).ThrowIfError();
    }

    /// <inheritdoc/>
    private protected override void WriteEnd(Stream output)
        // The encoder writes the header and the image; the GIF trailer (';') is written by the muxer otherwise.
        => output.WriteByte(0x3B);
}
