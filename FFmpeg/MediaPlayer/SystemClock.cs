using System.Diagnostics;

namespace FFmpeg.MediaPlayer;

/// <summary>
/// Provides a media clock based on a monotonic <see cref="Stopwatch"/>.
/// </summary>
/// <remarks>
/// The clock maintains an anchor position and advances it according to the
/// elapsed time measured by the stopwatch and the current <see cref="IMediaClock.Rate"/>.
/// Changing the rate re-anchors the clock at its current position to ensure
/// that the position remains continuous.
/// </remarks>
public class SystemClock : IMediaClock
{
    /// <summary>
    /// Measures the elapsed real time since the current clock anchor was established.
    /// </summary>
    private readonly Stopwatch _stopwatch = new();

    /// <inheritdoc/>
    public TimeSpan Position { get => field + (Rate * _stopwatch.Elapsed); private set; } = TimeSpan.Zero;

    /// <inheritdoc/>
    public double Rate
    {
        get;
        set
        {
            Position = Position;
            if (_stopwatch.IsRunning)
                _stopwatch.Restart();
            else
                _stopwatch.Reset();
            field = value;
            ClockChanged?.Invoke(this, EventArgs.Empty);
        }
    } = 1;

    /// <inheritdoc/>
    public bool IsRunning => _stopwatch.IsRunning;

    /// <inheritdoc/>
    public event EventHandler? ClockChanged;

    /// <summary>
    /// Starts advancing the clock from its current position.
    /// </summary>
    public void Start()
    {
        if (IsRunning)
            return;
        _stopwatch.Start();
        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Stops advancing the clock while preserving its current position.
    /// </summary>
    public void Pause()
    {
        if (!IsRunning)
            return;
        _stopwatch.Stop();
        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resets the clock to the beginning of the media timeline and stops it.
    /// </summary>
    internal void Reset()
    {
        _stopwatch.Reset();
        Position = TimeSpan.Zero;
        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the current position of the clock.
    /// </summary>
    /// <param name="position">The new position in the media timeline.</param>
    internal void SetPosition(TimeSpan position)
    {
        Position = position;
        if (_stopwatch.IsRunning)
            _stopwatch.Restart();
        else
            _stopwatch.Reset();
        ClockChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Seeked(TimeSpan position) => SetPosition(position);
}

