using FFmpeg.Audio;
using FFmpeg.Collections;
using FFmpeg.Utils;

namespace FFmpeg.Filters;

/// <summary>
/// Provides factory methods for creating commonly used audio filters.
/// </summary>
/// <remarks>
/// This class contains convenience methods for creating and initializing
/// audio filter contexts. All methods automatically allocate and initialize
/// the created filter.
/// </remarks>
public static class AudioFilters
{
    /// <summary>
    /// Creates an buffer source filter using the specified buffer source parameters.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="parameters">
    /// The parameters used to configure the buffer source.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized buffer source filter.
    /// </returns>
    public static FilterContext CreateSource(string name, BufferSrcParameters parameters, FilterGraph graph)
    {
        bool isVideo = !(parameters.Width == 0 && parameters.Height == 0);
        FilterContext? context = FilterContext.Allocate(name, isVideo ? Filter.VideoBufferSource : Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        context.SetBufferSourceParameters(parameters).ThrowIfError();
        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates an audio buffer source filter using the specified audio format.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="sampleRate">
    /// The sample rate, in samples per second.
    /// </param>
    /// <param name="sampleFormat">
    /// The sample format.
    /// </param>
    /// <param name="channels">
    /// The number of audio channels.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized buffer source filter.
    /// </returns>
    public static FilterContext CreateSource(string name, int sampleRate, SampleFormat sampleFormat, int channels, FilterGraph graph)
    {
        FilterContext? context = FilterContext.Allocate(name, Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        context.SetOption("sample_rate", sampleRate).ThrowIfError();
        context.SetOption("sample_fmt", sampleFormat).ThrowIfError();
        context.SetOption("channels", channels).ThrowIfError();
        context.Init().ThrowIfError();
        return context;

    }

    /// <summary>
    /// Creates an audio buffer source filter using the specified audio format.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="sampleRate">
    /// The sample rate, in samples per second.
    /// </param>
    /// <param name="sampleFormat">
    /// The sample format.
    /// </param>
    /// <param name="channelLayout">
    /// The channel layout of the audio stream.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized buffer source filter.
    /// </returns>
    public static FilterContext CreateSource(string name, int sampleRate, SampleFormat sampleFormat, ChannelLayout channelLayout, FilterGraph graph)
    {
        FilterContext? context = FilterContext.Allocate(name, Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        context.SetOption("sample_rate", sampleRate).ThrowIfError();
        context.SetOption("sample_fmt", sampleFormat).ThrowIfError();
        context.SetOption("channel_layout", channelLayout).ThrowIfError();
        context.Init().ThrowIfError();
        return context;

    }

    /// <summary>
    /// Creates an buffer source filter using the settings from a codec context.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="ctx">
    /// The codec context whose parameters are used to configure the filter.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="ctx"/> does not describe an audio stream.
    /// </exception>
    public static FilterContext CreateSource(string name, Codecs.CodecContext ctx, FilterGraph graph)
    {
        FilterContext? context = FilterContext.Allocate(name, ctx.CodecType == MediaType.Video ? Filter.VideoBufferSource : Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        using BufferSrcParameters @params = BufferSrcParameters.Allocate();

        @params.ChannelLayout.CopyFrom(ctx.ChannelLayout);
        @params.Width = ctx.Width;
        @params.Height = ctx.Height;
        @params.ColorRange = ctx.ColorRange;
        @params.ColorSpace = ctx.ColorSpace;
        @params.FrameRate = ctx.FrameRate;
        if (ctx.CodecType == MediaType.Video)
            @params.PixelFormat = ctx.PixelFormat;
        else
            @params.SampleFormat = ctx.SampleFormat;
        @params.SampleAspectRatio = ctx.SampleAspectRatio;
        //@params.SampleFormat = stream.SampleFormat; already set by PixelFormat
        @params.SampleRate = ctx.SampleRate;
        @params.TimeBase = ctx.TimeBase;

        context.SetBufferSourceParameters(@params).ThrowIfError();

        context.Init().ThrowIfError();
        return context;
    }


    /// <summary>
    /// Creates an buffer source filter using the settings from a stream.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="stream">
    /// The stream whose parameters are used to configure the filter.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized buffer source filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="stream"/> is not an audio stream.
    /// </exception>
    public static FilterContext CreateSource(string name, Formats.AVStream stream, FilterGraph graph)
    {
        FilterContext? context = FilterContext.Allocate(name, stream.MediaType == MediaType.Video ? Filter.VideoBufferSource : Filter.AudioBufferSource, graph) ?? throw new ArgumentNullException();
        using BufferSrcParameters @params = BufferSrcParameters.Allocate();

        @params.ChannelLayout.CopyFrom(stream.CodecParameters.ChannelLayout);
        @params.Width = stream.CodecParameters.Width;
        @params.Height = stream.CodecParameters.Height;
        @params.ColorRange = stream.CodecParameters.ColorRange;
        @params.ColorSpace = stream.CodecParameters.ColorSpace;
        @params.FrameRate = stream.CodecParameters.FrameRate;
        if (stream.MediaType == MediaType.Video)
            @params.PixelFormat = stream.CodecParameters.PixelFormat;
        else
            @params.SampleFormat = stream.CodecParameters.SampleFormat;
        @params.SampleAspectRatio = stream.SampleAspectRatio;
        //@params.SampleFormat = stream.SampleFormat; already set by PixelFormat
        @params.SampleRate = stream.CodecParameters.SampleRate;
        @params.TimeBase = stream.TimeBase;

        context.SetBufferSourceParameters(@params).ThrowIfError();

        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates an audio buffer sink filter context.
    /// </summary>
    /// <param name="name">The name to assign to the filter context.</param>
    /// <param name="graph">The <see cref="FilterGraph"/> to which the filter context belongs.</param>
    /// <returns>A new <see cref="FilterContext"/> configured as an audio buffer sink.</returns>
    public static FilterContext CreateSink(string name, FilterGraph graph)
        => FilterContext.Create(name, Filter.AudioBufferSink, graph);

    /// <summary>
    /// Creates an audio format filter context configured for a specific sample format,
    /// sample rate, and channel layout.
    /// </summary>
    /// <param name="name">The name to assign to the filter context.</param>
    /// <param name="sampleFormat">The sample format to convert the input audio to.</param>
    /// <param name="sampleRate">The sample rate to convert the input audio to, in samples per second.</param>
    /// <param name="channelLayout">The channel layout to use for the output audio.</param>
    /// <param name="graph">The <see cref="FilterGraph"/> to which the filter context belongs.</param>
    /// <returns>
    /// A new <see cref="FilterContext"/> configured to convert audio to the specified format,
    /// sample rate, and channel layout.
    /// </returns>
    public static FilterContext CreateFormat(
        string name,
        SampleFormat sampleFormat,
        int sampleRate,
        ChannelLayout channelLayout,
        FilterGraph graph)
    {
        var context = FilterContext.Allocate(name, Filter.AudioFormat, graph)!;
        context.SetOption("f", sampleFormat.ToFFmpegString()).ThrowIfError();
        context.SetOption("r", sampleRate.ToString()).ThrowIfError();
        context.SetOption("cl", channelLayout.ToString()).ThrowIfError();
        context.Init().ThrowIfError();
        return context;
    }

    /// <summary>
    /// Creates an audio format filter context configured with one or more sample formats,
    /// sample rates, and channel layouts.
    /// </summary>
    /// <param name="name">The name to assign to the filter context.</param>
    /// <param name="sampleFormats">
    /// The sample formats to which the input audio may be converted.
    /// </param>
    /// <param name="sampleRates">
    /// The sample rates to which the input audio may be converted, in samples per second.
    /// </param>
    /// <param name="channelLayouts">
    /// The channel layouts to use for the output audio.
    /// </param>
    /// <param name="graph">The <see cref="FilterGraph"/> to which the filter context belongs.</param>
    /// <returns>
    /// A new <see cref="FilterContext"/> configured with the specified audio format constraints.
    /// </returns>
    public static FilterContext CreateFormat(
        string name,
        ReadOnlySpan<SampleFormat> sampleFormats,
        ReadOnlySpan<int> sampleRates,
        ReadOnlySpan<ChannelLayout> channelLayouts,
        FilterGraph graph)
    {
        var context = FilterContext.Allocate(name, Filter.AudioFormat, graph)!;
        if (!sampleFormats.IsEmpty)
            context.SetOption("f", string.Join('|', sampleFormats, SampleExtensions.ToFFmpegString)).ThrowIfError();
        if (!sampleRates.IsEmpty)
            context.SetOption("r", string.Join('|', sampleRates)).ThrowIfError();
        if (!channelLayouts.IsEmpty)
            context.SetOption("cl", string.Join('|', channelLayouts)).ThrowIfError();
        context.Init().ThrowIfError();
        return context;
    }

}
