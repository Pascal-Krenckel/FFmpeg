using FFmpeg.Formats;
using System.Collections.ObjectModel;

namespace FFmpeg.Devices;

/// <summary>
/// Provides access to the input and output device formats supported by FFmpeg.
/// </summary>
/// <remarks>
/// The device subsystem is initialized automatically when this class is first accessed.
/// The returned collections contain FFmpeg device formats (such as DirectShow,
/// Video4Linux2, AVFoundation, or ALSA) rather than individual hardware devices.
///
/// The device format collections are initialized lazily and cached for subsequent
/// access. If additional device formats become available after the initial
/// enumeration, the corresponding <c>ReInit*</c> method can be used to refresh
/// the cached collection.
/// </remarks>
public static unsafe class Device
{
    static readonly object _lock = new();
    static Device() => RegisterAllDevices();

    private static void RegisterAllDevices()
    {
        lock (_lock)
        {
            ffmpeg.avdevice_register_all();
        }
    }

    /// <summary>
    /// Gets the available FFmpeg audio input device formats.
    /// </summary>
    /// <remarks>
    /// This collection contains input formats capable of capturing audio,
    /// such as DirectShow on Windows, ALSA on Linux, or AVFoundation on macOS.
    /// The collection is initialized on first access and then cached.
    /// </remarks>
    public static ReadOnlyCollection<InputFormat> AudioInputDevices { get => field ??= InitAudioInputDevices(); private set; } = null;
    /// <summary>
    /// Refreshes the cached collection of available audio input device formats.
    /// </summary>
    public static void ReInitAudioInputDevices() => AudioInputDevices = InitAudioInputDevices();

    private static ReadOnlyCollection<InputFormat> InitAudioInputDevices()
    {
        lock (_lock)
        {
            List<InputFormat> devices = [];
            AutoGen._AVInputFormat* format = null;
            while ((format = ffmpeg.av_input_audio_device_next(format)) != null)
                devices.Add(new(format));
            return new(devices);
        }
    }


    /// <summary>
    /// Gets the available FFmpeg audio output device formats.
    /// </summary>
    /// <remarks>
    /// This collection contains output formats capable of rendering audio.
    /// The collection is initialized on first access and then cached.
    /// </remarks>
    public static ReadOnlyCollection<OutputFormat> AudioOutputDevices
    {
        get => field ??= InitAudioOutputDevices();
        private set;
    } = null;

    /// <summary>
    /// Refreshes the cached collection of available audio output device formats.
    /// </summary>
    /// <remarks>
    /// Call this method to re-enumerate the audio output device formats if the
    /// available formats may have changed since the collection was first initialized.
    /// </remarks>  
    public static void ReInitAudioOutputDevices() => AudioOutputDevices = InitAudioOutputDevices();
    private static ReadOnlyCollection<OutputFormat> InitAudioOutputDevices()
    {
        lock (_lock)
        {
            List<OutputFormat> devices = [];
            AutoGen._AVOutputFormat* format = null;
            while ((format = ffmpeg.av_output_audio_device_next(format)) != null)
                devices.Add(new(format));
            return new(devices);
        }
    }

    /// <summary>
    /// Gets the available FFmpeg video input device formats.
    /// </summary>
    /// <remarks>
    /// This collection contains input formats capable of capturing video.
    /// The collection is initialized on first access and then cached.
    /// </remarks>
    public static ReadOnlyCollection<InputFormat> VideoInputDevices
    {
        get => field ??= InitVideoInputDevices();
        private set;
    } = null;

    /// <summary>
    /// Refreshes the cached collection of available video input device formats.
    /// </summary>
    /// <remarks>
    /// Call this method to re-enumerate the video input device formats if the
    /// available formats may have changed since the collection was first initialized.
    /// </remarks>
    public static void ReInitVideoInputDevices() => VideoInputDevices = InitVideoInputDevices();
    private static ReadOnlyCollection<InputFormat> InitVideoInputDevices()
    {
        lock (_lock)
        {
            List<InputFormat> devices = [];
            AutoGen._AVInputFormat* format = null;
            while ((format = ffmpeg.av_input_video_device_next(format)) != null)
                devices.Add(new(format));
            return new(devices);
        }
    }

    /// <summary>
    /// Gets the available FFmpeg video output device formats.
    /// </summary>
    /// <remarks>
    /// This collection contains output formats capable of rendering video.
    /// The collection is initialized on first access and then cached.
    /// </remarks>
    public static ReadOnlyCollection<OutputFormat> VideoOutputDevices
    {
        get => field ??= InitVideoOutputDevices();
        private set;
    } = null;


    /// <summary>
    /// Refreshes the cached collection of available video output device formats.
    /// </summary>
    /// <remarks>
    /// Call this method to re-enumerate the video output device formats if the
    /// available formats may have changed since the collection was first initialized.
    /// </remarks>
    public static void ReInitVideoOutputDevices() => VideoOutputDevices = InitVideoOutputDevices();
    private static ReadOnlyCollection<OutputFormat> InitVideoOutputDevices()
    {
        lock (_lock)
        {
            List<OutputFormat> devices = [];
            AutoGen._AVOutputFormat* format = null;
            while ((format = ffmpeg.av_output_video_device_next(format)) != null)
                devices.Add(new(format));
            return new(devices);
        }
    }
}
