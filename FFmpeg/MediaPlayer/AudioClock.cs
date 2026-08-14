using System.Diagnostics;

namespace FFmpeg.MediaPlayer;

/// <summary>
/// Provides a media clock for audio playback that can advance either continuously
/// according to real time or manually according to consumed audio samples.
/// </summary>
/// <remarks>
/// When the clock is running, its position advances according to the elapsed time
/// measured by an internal <see cref="Stopwatch"/> and the current <see cref="IMediaClock.Rate"/>.
/// When the clock is stopped, its position remains fixed and can be advanced explicitly
/// using <see cref="AdvanceSamples(int)"/>.
///
/// This allows the clock to be used with audio output systems that do not expose
/// their current playback position. In that case, the clock can remain stopped and
/// be advanced whenever audio samples are consumed by the output device.
/// </remarks>
public sealed class AudioClock : IMediaClock
{
    /// <summary>
    /// Measures elapsed real time while the clock is running.
    /// </summary>
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// The sample rate used to convert consumed audio samples into media time.
    /// </summary>
    private readonly int _sampleRate;

    /// <summary>
    /// The media position corresponding to the start of the current stopwatch interval.
    /// </summary>
    private TimeSpan _clockAnchor = TimeSpan.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioClock"/> class.
    /// </summary>
    /// <param name="sampleRate">
    /// The sample rate of the audio represented by the clock, in samples per second.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="sampleRate"/> is less than or equal to zero.
    /// </exception>
    public AudioClock(int sampleRate)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRate));

        _sampleRate = sampleRate;
    }

    /// <inheritdoc/>
    public TimeSpan Position =>
        _clockAnchor + (Rate * _stopwatch.Elapsed);

    /// <inheritdoc/>
    public double Rate
    {
        get;
        set
        {
            if (field == value)
                return;

            _clockAnchor = Position;

            if (_stopwatch.IsRunning)
                _stopwatch.Restart();
            else
                _stopwatch.Reset();

            field = value;
            ClockChanged?.Invoke(this, EventArgs.Empty);
        }
    } = 1.0;

    /// <inheritdoc/>
    public bool IsRunning => _stopwatch.IsRunning;

    /// <inheritdoc/>
    public event EventHandler? ClockChanged;

    /// <summary>
    /// Starts advancing the clock according to elapsed real time. <see cref="AutoRun"/> must be true.
    /// </summary>
    public void Start()
    {
        if (IsRunning || !AutoRun)
            return;

        _stopwatch.Start();
        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Stops the clock at its current position.
    /// </summary>
    public void Pause()
    {
        if (!IsRunning)
            return;

        _clockAnchor = Position;
        _stopwatch.Stop();
        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resets the clock to position zero and stops it.
    /// </summary>
    internal void Reset()
    {
        _stopwatch.Reset();
        _clockAnchor = TimeSpan.Zero;
        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Seeked(TimeSpan position) => SetPosition(position);

    /// <summary>
    /// Sets the current position of the clock without changing its running state.
    /// </summary>
    /// <param name="position">The new position in the media timeline.</param>
    internal void SetPosition(TimeSpan position)
    {
        _clockAnchor = position;

        if (_stopwatch.IsRunning)
            _stopwatch.Restart();
        else
            _stopwatch.Reset();

        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Advances the clock by the specified number of audio samples.
    /// </summary>
    /// <param name="samples">
    /// The number of audio samples by which to advance the clock.
    /// </param>
    /// <remarks>
    /// This method can only be used while the clock is stopped. It is intended
    /// for audio output systems that do not provide their own playback position.
    /// In that case, the clock can be advanced whenever samples are consumed
    /// by the audio output device.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the clock is currently running.
    /// </exception>
    internal void AdvanceSamples(int samples)
    {
        if (IsRunning)
            throw new InvalidOperationException(
                "Samples cannot be manually advanced while the clock is running.");

        _clockAnchor += TimeSpan.FromSeconds(
            (double)samples / _sampleRate);

        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets a value indicating whether the clock behaves like a system clock. If false <see cref="Start"/> has no effect.
    /// </summary>
    public bool AutoRun { get; set; }
}