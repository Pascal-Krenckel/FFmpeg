using FFmpeg.Audio;
using FFmpeg.AutoGen;

namespace FFmpeg.Filters.AudioFilters;

/// <summary>
/// Represents an audio format filter that constrains the sample format, sample rate,
/// and channel layout of audio passing through a filter graph.
/// </summary>
/// <remarks>
/// The audio format filter does not perform a standalone conversion itself. Instead,
/// it specifies the audio formats that are acceptable for the output of the filter,
/// allowing FFmpeg's filter graph negotiation to select or insert the appropriate
/// conversion when necessary.
/// </remarks>
public unsafe class AudioFormat : FilterContext
{
    internal AudioFormat(_AVFilterContext* context) : base(context)
    {
    }

    /// <summary>
    /// Allocates an uninitialized audio format filter.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The allocated, uninitialized audio format filter.
    /// </returns>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be allocated.
    /// </exception>
    public static AudioFormat Allocate(string name, FilterGraph graph) =>
        new(AllocateInternal(name, Filter.AudioFormat, graph));

    /// <summary>
    /// Creates and initializes an audio format filter configured for a specific
    /// sample format, sample rate, and channel layout.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="sampleFormat">
    /// The sample format accepted by the filter.
    /// </param>
    /// <param name="sampleRate">
    /// The sample rate accepted by the filter, in samples per second.
    /// </param>
    /// <param name="channelLayout">
    /// The channel layout accepted by the filter.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio format filter.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sampleRate"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be configured or initialized.
    /// </exception>
    public static AudioFormat Create(
        string name,
        SampleFormat sampleFormat,
        int sampleRate,
        ChannelLayout channelLayout,
        FilterGraph graph)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                "The sample rate must be greater than zero.");

        AudioFormat context = Allocate(name, graph);

        context.SetOption("f", sampleFormat.ToFFmpegString()).ThrowIfError();
        context.SetOption("r", sampleRate.ToString()).ThrowIfError();
        context.SetOption("cl", channelLayout.ToString()).ThrowIfError();
        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Creates and initializes an audio format filter configured with one or more
    /// acceptable sample formats, sample rates, and channel layouts.
    /// </summary>
    /// <param name="name">
    /// The name of the filter instance.
    /// </param>
    /// <param name="sampleFormats">
    /// The sample formats accepted by the filter. An empty span leaves the
    /// sample format unconstrained.
    /// </param>
    /// <param name="sampleRates">
    /// The sample rates accepted by the filter, in samples per second.
    /// An empty span leaves the sample rate unconstrained.
    /// </param>
    /// <param name="channelLayouts">
    /// The channel layouts accepted by the filter. An empty span leaves the
    /// channel layout unconstrained.
    /// </param>
    /// <param name="graph">
    /// The filter graph that will own the filter.
    /// </param>
    /// <returns>
    /// The initialized audio format filter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// One of the specified sample rates is less than or equal to zero.
    /// </exception>
    /// <exception cref="FFmpeg.Exceptions.FFmpegException">
    /// The filter could not be configured or initialized.
    /// </exception>
    public static AudioFormat Create(
        string name,
        ReadOnlySpan<SampleFormat> sampleFormats,
        ReadOnlySpan<int> sampleRates,
        ReadOnlySpan<ChannelLayout> channelLayouts,
        FilterGraph graph)
    {
        AudioFormat context = Allocate(name, graph);

        if (!sampleFormats.IsEmpty)
            context.SetOption(
                "f",
                string.Join('|', sampleFormats, SampleExtensions.ToFFmpegString))
                .ThrowIfError();

        if (!sampleRates.IsEmpty)
            context.SetOption(
                "r",
                string.Join('|', sampleRates))
                .ThrowIfError();

        if (!channelLayouts.IsEmpty)
            context.SetOption(
                "cl",
                string.Join('|', channelLayouts))
                .ThrowIfError();

        context.Init().ThrowIfError();

        return context;
    }

    /// <summary>
    /// Gets or sets the sample formats accepted by the filter.
    /// </summary>
    /// <value>
    /// A collection containing the accepted sample formats. An empty collection
    /// indicates that no sample format constraint is configured.
    /// </value>
    public IReadOnlyList<SampleFormat> SampleFormats
    {
        get =>
            TryGetOption("f", out string? formats).IsError ||
            string.IsNullOrWhiteSpace(formats)
                ? []
                : [.. formats
                    .Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => SampleFormat.Parse(f.Trim()))];

        set => SetOption(
            "f",
            Helper.StringHelper.Join('|', value, SampleExtensions.ToFFmpegString));
    }

    /// <summary>
    /// Gets or sets the sample rates accepted by the filter.
    /// </summary>
    /// <value>
    /// A collection containing the accepted sample rates in samples per second.
    /// An empty collection indicates that no sample rate constraint is configured.
    /// </value>
    public IReadOnlyList<int> SampleRates
    {
        get =>
            TryGetOption("r", out string? rates).IsError ||
            string.IsNullOrWhiteSpace(rates)
                ? []
                : [.. rates
                    .Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => int.Parse(r.Trim()))];

        set => SetOption("r", string.Join('|', value));
    }

    /// <summary>
    /// Gets or sets the channel layouts accepted by the filter.
    /// </summary>
    /// <value>
    /// A collection containing the accepted channel layouts. An empty collection
    /// indicates that no channel layout constraint is configured.
    /// </value>
    public IReadOnlyList<ChannelLayout> ChannelLayouts
    {
        get =>
            TryGetOption("cl", out string? layouts).IsError ||
            string.IsNullOrWhiteSpace(layouts)
                ? []
                : [.. layouts
                    .Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => ChannelLayout.Parse(l.Trim()))];

        set => SetOption("cl", string.Join('|', value));
    }
}