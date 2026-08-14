using FFmpeg.Formats;
using FFmpeg.Utils;

namespace FFmpeg.MediaPlayer;

public partial class PlaybackEngine
{
    #region Create

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackEngine"/> class.
    /// </summary>
    /// <param name="source">The media source to play.</param>
    /// <param name="videoStreamIndex">
    /// The index of the selected video stream, or <c>-1</c> if no video stream
    /// is selected.
    /// </param>
    /// <param name="audioStreamIndex">
    /// The index of the selected audio stream, or <c>-1</c> if no audio stream
    /// is selected.
    /// </param>
    private PlaybackEngine(
        MediaSource source,
        int videoStreamIndex,
        int audioStreamIndex)
    {
        this.source = source;

        mediaBuffer = videoStreamIndex == -1
            ? MediaBuffer.Create(source.Streams[audioStreamIndex])
            : audioStreamIndex == -1
            ? MediaBuffer.Create(source.Streams[videoStreamIndex])
            : MediaBuffer.Create(
                source.Streams[videoStreamIndex],
                source.Streams[audioStreamIndex]);
        this.videoStreamIndex = videoStreamIndex;
        this.audioStreamIndex = audioStreamIndex;

        PlayerStateChanged += UpdateClockStateAndFinished;
    }

    private void UpdateClockStateAndFinished(object sender, PlayerState e)
    {

        if (State == PlayerState.Faulted)
            Clock.Pause();
        else if (State == PlayerState.Finished)
        {
            Clock.Pause();
            Finished?.Invoke(this, EventArgs.Empty);
        }

    }

    /// <summary>
    /// Creates a media player for the specified media source.
    /// </summary>
    /// <param name="source">The media source to play.</param>
    /// <param name="videoStreamIndex">
    /// The video stream to use. A value of <c>-1</c> selects the best available
    /// video stream automatically. A value of <c>-2</c> disables video.
    /// </param>
    /// <param name="audioStreamIndex">
    /// The audio stream to use. A value of <c>-1</c> selects the best available
    /// audio stream automatically. A value of <c>-2</c> disables audio.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to use the selected streams.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if an explicitly selected stream has an incompatible media type,
    /// or if no supported audio or video stream is selected.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if an explicitly specified stream index is outside the range of
    /// the source's streams.
    /// </exception>
    public static PlaybackEngine Create(
        MediaSource source,
        int videoStreamIndex,
        int audioStreamIndex)
    {
        if (videoStreamIndex == -1)
            videoStreamIndex = source.FindBestStream(MediaType.Video);
        else if (videoStreamIndex == -2)
            videoStreamIndex = -1;
        else if (source.Streams[videoStreamIndex].MediaType != MediaType.Video)
            throw new ArgumentException(
                $"The stream ({videoStreamIndex}) was not a video stream");

        if (audioStreamIndex == -1)
            audioStreamIndex = source.FindBestStream(MediaType.Audio);
        else if (audioStreamIndex == -2)
            audioStreamIndex = -1;
        else if (source.Streams[audioStreamIndex].MediaType != MediaType.Audio)
            throw new ArgumentException(
                $"The stream ({audioStreamIndex}) was not an audio stream");

        if (videoStreamIndex == -1 && audioStreamIndex == -1)
            throw new ArgumentException("No supported media stream found.");

        for (int i = 0; i < source.Streams.Count; i++)
            source.Streams[i].Discard = i == videoStreamIndex || i == audioStreamIndex ? DiscardFlags.Default : DiscardFlags.All;

        return new(source, videoStreamIndex, audioStreamIndex);
    }

    /// <summary>
    /// Opens a video file and creates a media player for it.
    /// </summary>
    /// <param name="file">The path of the media file to open.</param>
    /// <param name="format">
    /// The input format to use, or <see langword="null"/> to let FFmpeg
    /// determine the format automatically.
    /// </param>
    /// <param name="options">
    /// Optional format-specific options passed to FFmpeg when opening the file.
    /// </param>
    /// <param name="videoStreamIndex">
    /// The video stream to use. A value of <c>-1</c> selects the best available
    /// video stream automatically.
    /// </param>
    /// <param name="hwAccel">
    /// The hardware acceleration device type to use, or
    /// <see cref="HW.DeviceType.None"/> to disable hardware acceleration.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to play the specified file.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the selected stream is not a video stream or no supported
    /// media stream is available.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if <paramref name="videoStreamIndex"/> is outside the range of
    /// available streams.
    /// </exception>
    public static PlaybackEngine OpenVideo(
        string file,
        InputFormat? format = null,
        IDictionary<string, string>? options = null,
        int videoStreamIndex = -1,
        HW.DeviceType hwAccel = HW.DeviceType.None) => Create(
            MediaSource.Open(file, format, options, deviceType: hwAccel),
            videoStreamIndex,
            -2);

    /// <summary>
    /// Opens a video stream and creates a media player for it.
    /// </summary>
    /// <param name="stream">The stream containing the media data.</param>
    /// <param name="format">
    /// The input format to use, or <see langword="null"/> to let FFmpeg
    /// determine the format automatically.
    /// </param>
    /// <param name="options">
    /// Optional format-specific options passed to FFmpeg when opening the stream.
    /// </param>
    /// <param name="videoStreamIndex">
    /// The video stream to use. A value of <c>-1</c> selects the best available
    /// video stream automatically.
    /// </param>
    /// <param name="hwAccel">
    /// The hardware acceleration device type to use, or
    /// <see cref="HW.DeviceType.None"/> to disable hardware acceleration.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to play the specified stream.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the selected stream is not a video stream or no supported
    /// media stream is available.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if <paramref name="videoStreamIndex"/> is outside the range of
    /// available streams.
    /// </exception>
    public static PlaybackEngine OpenVideo(
        Stream stream,
        InputFormat? format = null,
        IDictionary<string, string>? options = null,
        int videoStreamIndex = -1,
        HW.DeviceType hwAccel = HW.DeviceType.None) => Create(
            MediaSource.Open(stream, format, options, deviceType: hwAccel),
            videoStreamIndex,
            -2);

    /// <summary>
    /// Opens an audio file and creates a media player for it.
    /// </summary>
    /// <param name="file">The path of the media file to open.</param>
    /// <param name="format">
    /// The input format to use, or <see langword="null"/> to let FFmpeg
    /// determine the format automatically.
    /// </param>
    /// <param name="options">
    /// Optional format-specific options passed to FFmpeg when opening the file.
    /// </param>
    /// <param name="audioStreamIndex">
    /// The audio stream to use. A value of <c>-1</c> selects the best available
    /// audio stream automatically.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to play the specified file.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the selected stream is not an audio stream or no supported
    /// media stream is available.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if <paramref name="audioStreamIndex"/> is outside the range of
    /// available streams.
    /// </exception>
    public static PlaybackEngine OpenAudio(
        string file,
        InputFormat? format = null,
        IDictionary<string, string>? options = null,
        int audioStreamIndex = -1) => Create(
            MediaSource.Open(file, format, options),
            -2,
            audioStreamIndex);

    /// <summary>
    /// Opens an audio stream and creates a media player for it.
    /// </summary>
    /// <param name="stream">The stream containing the media data.</param>
    /// <param name="format">
    /// The input format to use, or <see langword="null"/> to let FFmpeg
    /// determine the format automatically.
    /// </param>
    /// <param name="options">
    /// Optional format-specific options passed to FFmpeg when opening the stream.
    /// </param>
    /// <param name="audioStreamIndex">
    /// The audio stream to use. A value of <c>-1</c> selects the best available
    /// audio stream automatically.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to play the specified stream.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the selected stream is not an audio stream or no supported
    /// media stream is available.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if <paramref name="audioStreamIndex"/> is outside the range of
    /// available streams.
    /// </exception>
    public static PlaybackEngine OpenAudio(
        Stream stream,
        InputFormat? format = null,
        IDictionary<string, string>? options = null,
        int audioStreamIndex = -1) => Create(
            MediaSource.Open(stream, format, options),
            -2,
            audioStreamIndex);

    /// <summary>
    /// Opens a media file and creates a media player using the specified
    /// audio and video streams.
    /// </summary>
    /// <param name="file">The path of the media file to open.</param>
    /// <param name="format">
    /// The input format to use, or <see langword="null"/> to let FFmpeg
    /// determine the format automatically.
    /// </param>
    /// <param name="options">
    /// Optional format-specific options passed to FFmpeg when opening the file.
    /// </param>
    /// <param name="videoStreamIndex">
    /// The video stream to use. A value of <c>-1</c> selects the best available
    /// video stream automatically. A value of <c>-2</c> disables video.
    /// </param>
    /// <param name="audioStreamIndex">
    /// The audio stream to use. A value of <c>-1</c> selects the best available
    /// audio stream automatically. A value of <c>-2</c> disables audio.
    /// </param>
    /// <param name="hwAccel">
    /// The hardware acceleration device type to use, or
    /// <see cref="HW.DeviceType.None"/> to disable hardware acceleration.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to play the selected streams.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if a selected stream has an incompatible media type or if no
    /// supported audio or video stream is selected.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if a stream index is outside the range of available streams.
    /// </exception>
    public static PlaybackEngine OpenVideo(
        string file,
        InputFormat? format = null,
        IDictionary<string, string>? options = null,
        int videoStreamIndex = -1,
        int audioStreamIndex = -1,
        HW.DeviceType hwAccel = HW.DeviceType.None) => Create(
            MediaSource.Open(file, format, options, deviceType: hwAccel),
            videoStreamIndex,
            audioStreamIndex);

    /// <summary>
    /// Opens a media file and creates a media player using the specified
    /// audio and video streams.
    /// </summary>
    /// <param name="file">The path of the media file to open.</param>
    /// <param name="format">
    /// The input format to use, or <see langword="null"/> to let FFmpeg
    /// determine the format automatically.
    /// </param>
    /// <param name="options">
    /// Optional format-specific options passed to FFmpeg when opening the file.
    /// </param>
    /// <param name="videoStreamIndex">
    /// The video stream to use. A value of <c>-1</c> selects the best available
    /// video stream automatically. A value of <c>-2</c> disables video.
    /// </param>
    /// <param name="audioStreamIndex">
    /// The audio stream to use. A value of <c>-1</c> selects the best available
    /// audio stream automatically. A value of <c>-2</c> disables audio.
    /// </param>
    /// <param name="hwAccel">
    /// The hardware acceleration device type to use, or
    /// <see cref="HW.DeviceType.None"/> to disable hardware acceleration.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to play the selected streams.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if a selected stream has an incompatible media type or if no
    /// supported audio or video stream is selected.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if a stream index is outside the range of available streams.
    /// </exception>
    public static PlaybackEngine Open(
        string file,
        InputFormat? format = null,
        IDictionary<string, string>? options = null,
        int videoStreamIndex = -1,
        int audioStreamIndex = -1,
        HW.DeviceType hwAccel = HW.DeviceType.None) => Create(
            MediaSource.Open(file, format, options, deviceType: hwAccel),
            videoStreamIndex,
            audioStreamIndex);

    /// <summary>
    /// Opens a media stream and creates a media player using the specified
    /// audio and video streams.
    /// </summary>
    /// <param name="stream">The stream containing the media data.</param>
    /// <param name="format">
    /// The input format to use, or <see langword="null"/> to let FFmpeg
    /// determine the format automatically.
    /// </param>
    /// <param name="options">
    /// Optional format-specific options passed to FFmpeg when opening the stream.
    /// </param>
    /// <param name="videoStreamIndex">
    /// The video stream to use. A value of <c>-1</c> selects the best available
    /// video stream automatically. A value of <c>-2</c> disables video.
    /// </param>
    /// <param name="audioStreamIndex">
    /// The audio stream to use. A value of <c>-1</c> selects the best available
    /// audio stream automatically. A value of <c>-2</c> disables audio.
    /// </param>
    /// <param name="hwAccel">
    /// The hardware acceleration device type to use, or
    /// <see cref="HW.DeviceType.None"/> to disable hardware acceleration.
    /// </param>
    /// <returns>
    /// A new <see cref="PlaybackEngine"/> configured to play the selected streams.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if a selected stream has an incompatible media type or if no
    /// supported audio or video stream is selected.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if a stream index is outside the range of available streams.
    /// </exception>
    public static PlaybackEngine Open(
        Stream stream,
        InputFormat? format = null,
        IDictionary<string, string>? options = null,
        int videoStreamIndex = -1,
        int audioStreamIndex = -1,
        HW.DeviceType hwAccel = HW.DeviceType.None) => Create(
            MediaSource.Open(stream, format, options, deviceType: hwAccel),
            videoStreamIndex,
            audioStreamIndex);


    #endregion
}