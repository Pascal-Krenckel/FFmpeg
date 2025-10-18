using FFmpeg.AutoGen;
using FFmpeg.Utils;

namespace FFmpeg.HW;
internal static unsafe class HWInternalFunctions
{
    public static AVResult32 HWDecoderInit(Codecs.CodecContext context, HW.DeviceType type) => ffmpeg.av_hwdevice_ctx_create(&context.context->hw_device_ctx, (_AVHWDeviceType)type, null, null, 0);
}
