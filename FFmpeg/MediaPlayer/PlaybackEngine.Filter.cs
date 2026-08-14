using FFmpeg.Filters;
using FFmpeg.Filters.AudioFilters;
using FFmpeg.Filters.VideoFilters;

namespace FFmpeg.MediaPlayer;

public partial class PlaybackEngine
{

    private string? videoFilter;
    private string? audioFilter;

    private void FlushFilters()
    {
        if (videoFilterGraph != null)
        {
            if (videoFilter == null)
            {
                videoFilterGraph.Flush();
                videoIn = videoFilterGraph.InputFilters.Single().As<VideoBufferSource>();
                videoOut = videoFilterGraph.OutputFilters.Single().As<VideoBufferSink>();
                _ = videoFilterGraph.Config();
            }
            else
                SetVideoFilter(videoFilter);
        }
        if (audioFilterGraph != null)
        {
            if (audioFilter == null)
            {
                audioFilterGraph.Flush();
                audioIn = audioFilterGraph.InputFilters.Single().As<AudioBufferSource>();
                audioOut = audioFilterGraph.OutputFilters.Single().As<AudioBufferSink>();
                _ = audioFilterGraph.Config();
            }
            else SetAudioFilter(audioFilter);
        }
    }

    /// <summary>
    /// Configures the video filter graph used to process decoded video frames.
    /// <br/> If possible use <see cref="SetVideoFilter(string?)"/>.
    /// </summary>
    /// <param name="in">
    /// The input filter list identifying the filter and input pad to which the
    /// internal video buffer source is connected.
    /// </param>
    /// <param name="graph">
    /// The filter graph containing the video filters to configure.
    /// Pass <see langword="null"/> together with <paramref name="in"/> and
    /// <paramref name="out"/> to remove the current video filter graph.
    /// </param>
    /// <param name="out">
    /// The output filter list identifying the filter and output pad from which
    /// the processed video is connected to the internal video buffer sink.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The media contains no video stream and a filter graph was specified;
    /// or the specified filter graph does not contain exactly one input and
    /// one output filter.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// One or more of <paramref name="in"/>, <paramref name="graph"/>, or
    /// <paramref name="out"/> is <see langword="null"/> while another is not.
    /// All three parameters must either be non-null or all be null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The player is currently playing. Filters can only be changed while
    /// the player is stopped or paused.
    /// </exception>
    /// <remarks>
    /// Passing <see langword="null"/> for all three parameters removes the
    /// currently configured video filter graph.
    /// <para>
    /// The filter graph is copied and configured internally. The supplied
    /// <paramref name="graph"/> and filter lists are disposed by this method
    /// after the graph has been configured.
    /// </para>
    /// <para>
    /// If buffered media has already advanced beyond the beginning of the
    /// stream, the source is repositioned to the current clock position
    /// before the filter graph is replaced.
    /// </para>
    /// <para>
    /// The media buffer is recreated after changing the filter graph.
    /// </para>
    /// </remarks>
    public void SetVideoFilter(FilterInOutList? @in, FilterGraph? graph, FilterInOutList? @out)
    {
        videoFilter = null;
        if (videoStreamIndex == -1)
            if (graph == null)
                return;
            else
                throw new ArgumentException("The media player has no video stream, so you can't set the filter graph");
        lock (_lock)
        {
            CheckDisposed();
            if (State == PlayerState.Playing)
                throw new InvalidOperationException("The player must be stopped before setting the filter graph.");
            if ((mediaBuffer.CanReadVideo || mediaBuffer.CanReadAudio) && Clock.Position > TimeSpan.Zero)
                source.SeekExactly(Clock.Position, videoStreamIndex).ThrowIfError();
            if (@in == null || @out == null || graph == null)
                if (@in != null || @out != null || graph != null)
                    throw new ArgumentNullException(string.Join('|', nameof(@in), nameof(graph), nameof(@out)), "null is only valid if all parameters are null");
            if (graph != null && (@in!.Count != 1 || @out!.Count != 1))
                throw new ArgumentException("@in and @out must contain exactly one filter.");

            mediaBuffer.Dispose();
            videoFilterGraph?.Dispose();
            videoIn = null;
            videoOut = null;
            if (graph != null)
            {
                VideoBufferSource videoIn = VideoBufferSource.Create("video_buffer_src", source.CodecContexts[videoStreamIndex], graph);
                VideoBufferSink videoOut = VideoBufferSink.Create("video_buffer_sink", graph);
                FilterGraph? copy = null;
                try
                {
                    graph.Link(videoIn, 0, @in![0].Filter!, @in[0].PadIdx).ThrowIfError();
                    graph.Link(@out![0].Filter!, @out[0].PadIdx, videoOut, 0).ThrowIfError();
                    copy = graph.Copy();
                    copy.Config().ThrowIfError();
                    videoIn = copy.InputFilters.Single().As<VideoBufferSource>();
                    videoOut = copy.OutputFilters.Single().As<VideoBufferSink>();
                    videoFilterGraph = copy;
                    this.videoIn = videoIn;
                    this.videoOut = videoOut;
                    copy = null;
                }
                finally
                {
                    copy?.Dispose();
                    @in!.Dispose();
                    @out!.Dispose();
                    graph.Dispose();
                    mediaBuffer = MediaBuffer.Create(PixelFormat, Width, Height, SampleFormat, Channels, SampleRate);
                }

            }
            else
                mediaBuffer = MediaBuffer.Create(PixelFormat, Width, Height, SampleFormat, Channels, SampleRate);
        }
    }

    /// <summary>
    /// Configures the video filter graph from a filter description string.
    /// </summary>
    /// <param name="videoFilter">
    /// The filter description to parse and apply to the video stream,
    /// or <see langword="null"/> to remove the current video filter graph.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The media contains no video stream, or the filter description does not
    /// describe a valid video filter graph.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The player is currently playing. Filters can only be changed while
    /// the player is stopped or paused.
    /// </exception>
    /// <remarks>
    /// The filter description is parsed into a <see cref="FilterGraph"/>,
    /// which is then passed to <see cref="SetVideoFilter(FilterInOutList?, FilterGraph?, FilterInOutList?)"/>.
    /// </remarks>
    public void SetVideoFilter(string? videoFilter)
    {
        if (videoFilter == null)
            SetVideoFilter(null, null, null);
        else
        {
            using FilterGraph graph = FilterGraph.Create(out FilterInOutList? inputs, videoFilter, out FilterInOutList? outputs);
            SetVideoFilter(inputs!, graph, outputs!);
        }
        this.videoFilter = videoFilter;
    }

    /// <summary>
    /// Configures the audio filter graph used to process decoded audio frames.
    /// <br/> If possible use <see cref="SetAudioFilter(string?)"/>.
    /// </summary>
    /// <param name="in">
    /// The input filter list identifying the filter and input pad to which the
    /// internal audio buffer source is connected.
    /// </param>
    /// <param name="graph">
    /// The filter graph containing the audio filters to configure.
    /// Pass <see langword="null"/> together with <paramref name="in"/> and
    /// <paramref name="out"/> to remove the current audio filter graph.
    /// </param>
    /// <param name="out">
    /// The output filter list identifying the filter and output pad from which
    /// the processed audio is connected to the internal audio buffer sink.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The media contains no audio stream and a filter graph was specified;
    /// or the specified filter graph does not contain exactly one input and
    /// one output filter.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// One or more of <paramref name="in"/>, <paramref name="graph"/>, or
    /// <paramref name="out"/> is <see langword="null"/> while another is not.
    /// All three parameters must either be non-null or all be null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The player is currently playing. Filters can only be changed while
    /// the player is stopped or paused.
    /// </exception>
    /// <remarks>
    /// Passing <see langword="null"/> for all three parameters removes the
    /// currently configured audio filter graph.
    /// <para>
    /// The filter graph is copied and configured internally. The supplied
    /// <paramref name="graph"/> and filter lists are disposed by this method
    /// after the graph has been configured.
    /// </para>
    /// <para>
    /// If buffered media has already advanced beyond the beginning of the
    /// stream, the source is repositioned to the current clock position
    /// before the filter graph is replaced.
    /// </para>
    /// <para>
    /// The media buffer is recreated after changing the filter graph.
    /// </para>
    /// </remarks>
    public void SetAudioFilter(FilterInOutList? @in, FilterGraph? graph, FilterInOutList? @out)
    {
        audioFilter = null;
        if (audioStreamIndex == -1)
            if (graph == null)
                return;
            else
                throw new ArgumentException("The media player has no audio stream, so you can't set the filter graph");
        lock (_lock)
        {
            CheckDisposed();
            if (State == PlayerState.Playing)
                throw new InvalidOperationException("The player must be stopped before setting the filter graph.");
            if ((mediaBuffer.CanReadVideo || mediaBuffer.CanReadAudio) && Clock.Position > TimeSpan.Zero)
                source.SeekExactly(Clock.Position, audioStreamIndex).ThrowIfError();
            if (@in == null || @out == null || graph == null)
                if (@in != null || @out != null || graph != null)
                    throw new ArgumentNullException(string.Join('|', nameof(@in), nameof(graph), nameof(@out)), "null is only valid if all parameters are null");
            if (graph != null && (@in!.Count != 1 || @out!.Count != 1))
                throw new ArgumentException("@in and @out must contain exactly one filter.");
            mediaBuffer.Dispose();
            audioFilterGraph?.Dispose();
            audioIn = null;
            audioOut = null;
            if (graph != null)
            {
                AudioBufferSource audioIn = AudioBufferSource.Create("audio_buffer_src", source.CodecContexts[audioStreamIndex], graph);
                AudioBufferSink audioOut = AudioBufferSink.Create("audio_buffer_sink", graph);
                FilterGraph? copy = null;
                try
                {
                    graph.Link(audioIn, 0, @in![0].Filter!, @in[0].PadIdx).ThrowIfError();
                    graph.Link(@out![0].Filter!, @out[0].PadIdx, audioOut, 0).ThrowIfError();
                    copy = graph.Copy();
                    copy.Config().ThrowIfError();
                    audioIn = copy.InputFilters.Single().As<AudioBufferSource>();
                    audioOut = copy.OutputFilters.Single().As<AudioBufferSink>();
                    audioFilterGraph = copy;
                    this.audioIn = audioIn;
                    this.audioOut = audioOut;
                    copy = null;
                }
                finally
                {
                    copy?.Dispose();
                    @in!.Dispose();
                    @out!.Dispose();
                    graph.Dispose();
                    mediaBuffer = MediaBuffer.Create(PixelFormat, Width, Height, SampleFormat, Channels, SampleRate);
                }
            }
            else
                mediaBuffer = MediaBuffer.Create(PixelFormat, Width, Height, SampleFormat, Channels, SampleRate);
        }
    }

    /// <summary>
    /// Configures the audio filter graph from a filter description string.
    /// </summary>
    /// <param name="audioFilter">
    /// The filter description to parse and apply to the audio stream,
    /// or <see langword="null"/> to remove the current audio filter graph.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The media contains no audio stream, or the filter description does not
    /// describe a valid audio filter graph.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The player is currently playing. Filters can only be changed while
    /// the player is stopped or paused.
    /// </exception>
    /// <remarks>
    /// The filter description is parsed into a <see cref="FilterGraph"/>,
    /// which is then passed to <see cref="SetAudioFilter(FilterInOutList?, FilterGraph?, FilterInOutList?)"/>.
    /// </remarks>
    public void SetAudioFilter(string? audioFilter)
    {
        if (audioFilter == null)
            SetAudioFilter(null, null, null);
        else
        {
            using FilterGraph graph = FilterGraph.Create(out FilterInOutList? inputs, audioFilter, out FilterInOutList? outputs);
            SetAudioFilter(inputs!, graph, outputs!);
        }
        this.audioFilter = audioFilter;
    }
}
