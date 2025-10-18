using FFmpeg.Formats;
using System.Collections.ObjectModel;

namespace FFmpeg.Devices;
public static unsafe class Device
{
    static Device() => ffmpeg.avdevice_register_all();

    private static ReadOnlyCollection<InputFormat>? audioInputDevices = null;
    public static ReadOnlyCollection<InputFormat> AudioInputDevices => audioInputDevices ??= InitAudioInputDevices();

    private static ReadOnlyCollection<InputFormat> InitAudioInputDevices()
    {
        List<InputFormat> devices = [];
        AutoGen._AVInputFormat* format = null;
        while ((format = ffmpeg.av_input_audio_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }

    private static ReadOnlyCollection<OutputFormat>? audioOutputDevices = null;
    public static ReadOnlyCollection<OutputFormat> AudioOutputDevices => audioOutputDevices ??= InitAudioOutputDevices();
    private static ReadOnlyCollection<OutputFormat> InitAudioOutputDevices()
    {
        List<OutputFormat> devices = [];
        AutoGen._AVOutputFormat* format = null;
        while ((format = ffmpeg.av_output_audio_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }


    private static ReadOnlyCollection<InputFormat>? videoInputDevices = null;
    public static ReadOnlyCollection<InputFormat> VideoInputDevices => videoInputDevices ??= InitVideoInputDevices();

    private static ReadOnlyCollection<InputFormat> InitVideoInputDevices()
    {
        List<InputFormat> devices = [];
        AutoGen._AVInputFormat* format = null;
        while ((format = ffmpeg.av_input_video_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }

    private static ReadOnlyCollection<OutputFormat>? videoOutputDevices = null;
    public static ReadOnlyCollection<OutputFormat> VideoOutputDevices => videoOutputDevices ??= InitVideoOutputDevices();

    private static ReadOnlyCollection<OutputFormat> InitVideoOutputDevices()
    {
        List<OutputFormat> devices = [];
        AutoGen._AVOutputFormat* format = null;
        while ((format = ffmpeg.av_output_video_device_next(format)) != null)
            devices.Add(new(format));
        return new(devices);
    }
}
