using FFmpeg.Formats;
using System.Collections.ObjectModel;

namespace FFmpeg.Devices;

public static unsafe class Device
{
    static Device() => ffmpeg.avdevice_register_all();

    public static ReadOnlyCollection<InputFormat> AudioInputDevices { get => field ??= InitAudioInputDevices(); } = null;

    private static ReadOnlyCollection<InputFormat> InitAudioInputDevices()
    {
        List<InputFormat> devices = [];
        AutoGen._AVInputFormat* format = null;
        while ((format = ffmpeg.av_input_audio_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }

    public static ReadOnlyCollection<OutputFormat> AudioOutputDevices { get => field ??= InitAudioOutputDevices(); } = null;
    private static ReadOnlyCollection<OutputFormat> InitAudioOutputDevices()
    {
        List<OutputFormat> devices = [];
        AutoGen._AVOutputFormat* format = null;
        while ((format = ffmpeg.av_output_audio_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }

    public static ReadOnlyCollection<InputFormat> VideoInputDevices { get => field ??= InitVideoInputDevices(); } = null;

    private static ReadOnlyCollection<InputFormat> InitVideoInputDevices()
    {
        List<InputFormat> devices = [];
        AutoGen._AVInputFormat* format = null;
        while ((format = ffmpeg.av_input_video_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }

    public static ReadOnlyCollection<OutputFormat> VideoOutputDevices { get => field ??= InitVideoOutputDevices(); } = null;

    private static ReadOnlyCollection<OutputFormat> InitVideoOutputDevices()
    {
        List<OutputFormat> devices = [];
        AutoGen._AVOutputFormat* format = null;
        while ((format = ffmpeg.av_output_video_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }
}
