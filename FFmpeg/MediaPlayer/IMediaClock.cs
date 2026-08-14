namespace FFmpeg.MediaPlayer;

/// <summary>
/// Represents a clock that tracks the current position in a media timeline.
/// </summary>
public interface IMediaClock
{
    /// <summary>
    /// Gets the current position in the media timeline.
    /// </summary>
    TimeSpan Position { get; }

    /// <summary>
    /// Notifies the clock, that the position has changed.
    /// </summary>
    /// <param name="timespan">The pts that was seeked to.</param>
    void Seeked(TimeSpan timespan);

    /// <summary>
    /// Gets or sets the rate at which media time progresses relative to real time.
    /// A rate of <c>1.0</c> represents normal playback speed.
    /// </summary>
    double Rate { get; }

    /// <summary>
    /// Gets a value indicating whether the clock is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Called by <see cref="PlaybackEngine"/> when the engine starts decoding.
    /// </summary>
    void Start();

    /// <summary>
    /// Called by <see cref="PlaybackEngine"/> when the engine stops decoding.
    /// </summary>
    void Pause();


    /// <summary>
    /// Occurs when the clock state changes in a way that may affect the timing
    /// of waiting operations, such as when the clock is started, stopped,
    /// repositioned, or its rate changes.
    /// </summary>
    event EventHandler? ClockChanged;

}