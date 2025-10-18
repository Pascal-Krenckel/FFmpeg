namespace FFmpeg.Images;
/// <summary>
/// Represents the various pixel formats available.
/// </summary>
public enum PixelFormat : int
{
    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_NONE"/>
    None = AutoGen._AVPixelFormat.AV_PIX_FMT_NONE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P"/>
    YUV420P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUYV422"/>
    YUYV422 = AutoGen._AVPixelFormat.AV_PIX_FMT_YUYV422,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB24"/>
    RGB24 = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB24,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR24"/>
    BGR24 = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR24,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P"/>
    YUV422P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P"/>
    YUV444P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV410P"/>
    YUV410P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV410P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV411P"/>
    YUV411P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV411P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY8"/>
    Gray8 = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY8,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_MONOWHITE"/>
    MonoWhite = AutoGen._AVPixelFormat.AV_PIX_FMT_MONOWHITE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_MONOBLACK"/>
    MonoBlack = AutoGen._AVPixelFormat.AV_PIX_FMT_MONOBLACK,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_PAL8"/>
    PAL8 = AutoGen._AVPixelFormat.AV_PIX_FMT_PAL8,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ420P"/>
    YUVJ420P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ420P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ422P"/>
    YUVJ422P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ422P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ444P"/>
    YUVJ444P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ444P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_UYVY422"/>
    UYVY422 = AutoGen._AVPixelFormat.AV_PIX_FMT_UYVY422,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_UYYVYY411"/>
    UYYVYY411 = AutoGen._AVPixelFormat.AV_PIX_FMT_UYYVYY411,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR8"/>
    BGR8 = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR8,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR4"/>
    BGR4 = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR4,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR4_BYTE"/>
    BGR4Byte = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR4_BYTE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB8"/>
    RGB8 = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB8,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB4"/>
    RGB4 = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB4,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB4_BYTE"/>
    RGB4Byte = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB4_BYTE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_NV12"/>
    NV12 = AutoGen._AVPixelFormat.AV_PIX_FMT_NV12,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_NV21"/>
    NV21 = AutoGen._AVPixelFormat.AV_PIX_FMT_NV21,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_ARGB"/>
    ARGB = AutoGen._AVPixelFormat.AV_PIX_FMT_ARGB,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA"/>
    RGBA = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_ABGR"/>
    ABGR = AutoGen._AVPixelFormat.AV_PIX_FMT_ABGR,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGRA"/>
    BGRA = AutoGen._AVPixelFormat.AV_PIX_FMT_BGRA,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY16BE"/>
    Gray16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY16LE"/>
    Gray16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV440P"/>
    YUV440P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV440P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ440P"/>
    YUVJ440P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ440P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P"/>
    YUVA420P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB48BE"/>
    RGB48BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB48BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB48LE"/>
    RGB48LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB48LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB565BE"/>
    RGB565BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB565BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB565LE"/>
    RGB565LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB565LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB555BE"/>
    RGB555BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB555BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB555LE"/>
    RGB555LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB555LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR565BE"/>
    BGR565BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR565BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR565LE"/>
    BGR565LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR565LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR555BE"/>
    BGR555BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR555BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR555LE"/>
    BGR555LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR555LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_VAAPI"/>
    VAAPI = AutoGen._AVPixelFormat.AV_PIX_FMT_VAAPI,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P16LE"/>
    YUV420P16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P16BE"/>
    YUV420P16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P16LE"/>
    YUV422P16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P16BE"/>
    YUV422P16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P16LE"/>
    YUV444P16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P16BE"/>
    YUV444P16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_DXVA2_VLD"/>
    DXVA2VLD = AutoGen._AVPixelFormat.AV_PIX_FMT_DXVA2_VLD,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB444LE"/>
    RGB444LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB444LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGB444BE"/>
    RGB444BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB444BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR444LE"/>
    BGR444LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR444LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR444BE"/>
    BGR444BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR444BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YA8"/>
    YA8 = AutoGen._AVPixelFormat.AV_PIX_FMT_YA8,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_Y400A"/>
    Y400A = AutoGen._AVPixelFormat.AV_PIX_FMT_Y400A,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY8A"/>
    Gray8A = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY8A,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR48BE"/>
    BGR48BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR48BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGR48LE"/>
    BGR48LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR48LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P9BE"/>
    YUV420P9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P9BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P9LE"/>
    YUV420P9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P9LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P10BE"/>
    YUV420P10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P10BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P10LE"/>
    YUV420P10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P10LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P10BE"/>
    YUV422P10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P10BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P10LE"/>
    YUV422P10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P10LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P9BE"/>
    YUV444P9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P9BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P9LE"/>
    YUV444P9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P9LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P10BE"/>
    YUV444P10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P10BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P10LE"/>
    YUV444P10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P10LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P9BE"/>
    YUV422P9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P9BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P9LE"/>
    YUV422P9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P9LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP"/>
    GBRP = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBR24P"/>
    GBR24P = AutoGen._AVPixelFormat.AV_PIX_FMT_GBR24P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP9BE"/>
    GBRP9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP9BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP9LE"/>
    GBRP9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP9LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP10BE"/>
    GBRP10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP10BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP10LE"/>
    GBRP10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP10LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP16BE"/>
    GBRP16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP16LE"/>
    GBRP16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P"/>
    YUVA422P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P"/>
    YUVA444P = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P9BE"/>
    YUVA420P9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P9BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P9LE"/>
    YUVA420P9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P9LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P9BE"/>
    YUVA422P9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P9BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P9LE"/>
    YUVA422P9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P9LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P9BE"/>
    YUVA444P9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P9BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P9LE"/>
    YUVA444P9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P9LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P10BE"/>
    YUVA420P10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P10BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P10LE"/>
    YUVA420P10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P10LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P10BE"/>
    YUVA422P10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P10BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P10LE"/>
    YUVA422P10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P10LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P10BE"/>
    YUVA444P10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P10BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P10LE"/>
    YUVA444P10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P10LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P16BE"/>
    YUVA420P16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P16LE"/>
    YUVA420P16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA420P16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P16BE"/>
    YUVA422P16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P16LE"/>
    YUVA422P16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P16BE"/>
    YUVA444P16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P16LE"/>
    YUVA444P16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P16LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_VDPAU"/>
    VDPAU = AutoGen._AVPixelFormat.AV_PIX_FMT_VDPAU,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_XYZ12LE"/>
    XYZ12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_XYZ12LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_XYZ12BE"/>
    XYZ12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_XYZ12BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_NV16"/>
    NV16 = AutoGen._AVPixelFormat.AV_PIX_FMT_NV16,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_NV20LE"/>
    NV20LE = AutoGen._AVPixelFormat.AV_PIX_FMT_NV20LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_NV20BE"/>
    NV20BE = AutoGen._AVPixelFormat.AV_PIX_FMT_NV20BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA64BE"/>
    RGBA64BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA64BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA64LE"/>
    RGBA64LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA64LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGRA64BE"/>
    BGRA64BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGRA64BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_BGRA64LE"/>
    BGRA64LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BGRA64LE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YVYU422"/>
    YVYU422 = AutoGen._AVPixelFormat.AV_PIX_FMT_YVYU422,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YA16BE"/>
    YA16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YA16BE,

    /// <inheritdoc cref="AutoGen._AVPixelFormat.AV_PIX_FMT_YA16LE"/>
    YA16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YA16LE,

    /// <summary>Planar GBRA 4:4:4:4 32bpp</summary>
    GBRA4444_32 = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP,

    /// <summary>Planar GBRA 4:4:4:4 64bpp, big-endian</summary>
    GBRA4444_64BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP16BE,

    /// <summary>Planar GBRA 4:4:4:4 64bpp, little-endian</summary>
    GBRA4444_64LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP16LE,

    /// <summary>HW acceleration through QSV, data[3] contains a pointer to the mfxFrameSurface1 structure.</summary>
    QSV = AutoGen._AVPixelFormat.AV_PIX_FMT_QSV,

    /// <summary>HW acceleration through MMAL, data[3] contains a pointer to the MMAL_BUFFER_HEADER_T structure.</summary>
    MMAL = AutoGen._AVPixelFormat.AV_PIX_FMT_MMAL,

    /// <summary>HW decoding through Direct3D11 via old API, Picture.data[3] contains a ID3D11VideoDecoderOutputView pointer</summary>
    D3D11VA_VLD = AutoGen._AVPixelFormat.AV_PIX_FMT_D3D11VA_VLD,

    /// <summary>HW acceleration through CUDA. data[i] contain CUdeviceptr pointers exactly as for system memory frames.</summary>
    CUDA = AutoGen._AVPixelFormat.AV_PIX_FMT_CUDA,

    /// <summary>Packed RGB 8:8:8, 32bpp, XRGBXRGB... X=unused/undefined</summary>
    XRGB = AutoGen._AVPixelFormat.AV_PIX_FMT_0RGB,

    /// <summary>Packed RGB 8:8:8, 32bpp, RGBXRGBX... X=unused/undefined</summary>
    RGBX = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB0,

    /// <summary>Packed BGR 8:8:8, 32bpp, XBGRXBGR... X=unused/undefined</summary>
    XBGR = AutoGen._AVPixelFormat.AV_PIX_FMT_0BGR,

    /// <summary>Packed BGR 8:8:8, 32bpp, BGRXBGRX... X=unused/undefined</summary>
    BGRX = AutoGen._AVPixelFormat.AV_PIX_FMT_BGR0,

    /// <summary>Planar YUV 4:2:0, 18bpp, (1 Cr &amp; Cb sample per 2x2 Y samples), big-endian</summary>
    YUV420P12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P12BE,

    /// <summary>Planar YUV 4:2:0, 18bpp, (1 Cr &amp; Cb sample per 2x2 Y samples), little-endian</summary>
    YUV420P12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P12LE,

    /// <summary>Planar YUV 4:2:0, 21bpp, (1 Cr &amp; Cb sample per 2x2 Y samples), big-endian</summary>
    YUV420P14BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P14BE,

    /// <summary>Planar YUV 4:2:0, 21bpp, (1 Cr &amp; Cb sample per 2x2 Y samples), little-endian</summary>
    YUV420P14LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV420P14LE,

    /// <summary>Planar YUV 4:2:2, 24bpp, (1 Cr &amp; Cb sample per 2x1 Y samples), big-endian</summary>
    YUV422P12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P12BE,

    /// <summary>Planar YUV 4:2:2, 24bpp, (1 Cr &amp; Cb sample per 2x1 Y samples), little-endian</summary>
    YUV422P12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P12LE,

    /// <summary>Planar YUV 4:2:2, 28bpp, (1 Cr &amp; Cb sample per 2x1 Y samples), big-endian</summary>
    YUV422P14BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P14BE,

    /// <summary>Planar YUV 4:2:2, 28bpp, (1 Cr &amp; Cb sample per 2x1 Y samples), little-endian</summary>
    YUV422P14LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV422P14LE,

    /// <summary>Planar YUV 4:4:4, 36bpp, (1 Cr &amp; Cb sample per 1x1 Y samples), big-endian</summary>
    YUV444P12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P12BE,

    /// <summary>Planar YUV 4:4:4, 36bpp, (1 Cr &amp; Cb sample per 1x1 Y samples), little-endian</summary>
    YUV444P12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P12LE,

    /// <summary>Planar YUV 4:4:4, 42bpp, (1 Cr &amp; Cb sample per 1x1 Y samples), big-endian</summary>
    YUV444P14BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P14BE,

    /// <summary>Planar YUV 4:4:4, 42bpp, (1 Cr &amp; Cb sample per 1x1 Y samples), little-endian</summary>
    YUV444P14LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P14LE,

    /// <summary>Planar GBR 4:4:4, 36bpp, big-endian</summary>
    GBRP12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP12BE,

    /// <summary>Planar GBR 4:4:4, 36bpp, little-endian</summary>
    GBRP12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP12LE,

    /// <summary>Planar GBR 4:4:4, 42bpp, big-endian</summary>
    GBRP14BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP14BE,

    /// <summary>Planar GBR 4:4:4, 42bpp, little-endian</summary>
    GBRP14LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP14LE,

    /// <summary>Planar YUV 4:1:1, 12bpp, (1 Cr &amp; Cb sample per 4x1 Y samples) full scale (JPEG), deprecated in favor of AV_PIX_FMT_YUV411P and setting color_range</summary>
    YUV411P_JPEG = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVJ411P,

    /// <summary>Bayer pattern, BGBG..(odd line), GRGR..(even line), 8-bit samples</summary>
    BAYER_BGGR8 = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_BGGR8,

    /// <summary>Bayer pattern, RGRG..(odd line), GBGB..(even line), 8-bit samples</summary>
    BAYER_RGGB8 = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_RGGB8,

    /// <summary>Bayer pattern, GBGB..(odd line), RGRG..(even line), 8-bit samples</summary>
    BAYER_GBRG8 = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_GBRG8,

    /// <summary>Bayer pattern, GRGR..(odd line), BGBG..(even line), 8-bit samples</summary>
    BAYER_GRBG8 = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_GRBG8,

    /// <summary>Bayer pattern, BGBG..(odd line), GRGR..(even line), 16-bit samples, little-endian</summary>
    BAYER_BGGR16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_BGGR16LE,

    /// <summary>Bayer pattern, BGBG..(odd line), GRGR..(even line), 16-bit samples, big-endian</summary>
    BAYER_BGGR16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_BGGR16BE,

    /// <summary>Bayer pattern, RGRG..(odd line), GBGB..(even line), 16-bit samples, little-endian</summary>
    BAYER_RGGB16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_RGGB16LE,

    /// <summary>Bayer pattern, RGRG..(odd line), GBGB..(even line), 16-bit samples, big-endian</summary>
    BAYER_RGGB16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_RGGB16BE,

    /// <summary>Bayer pattern, GBGB..(odd line), RGRG..(even line), 16-bit samples, little-endian</summary>
    BAYER_GBRG16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_GBRG16LE,

    /// <summary>Bayer pattern, GBGB..(odd line), RGRG..(even line), 16-bit samples, big-endian</summary>
    BAYER_GBRG16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_GBRG16BE,

    /// <summary>Bayer pattern, GRGR..(odd line), BGBG..(even line), 16-bit samples, little-endian</summary>
    BAYER_GRBG16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_GRBG16LE,

    /// <summary>Bayer pattern, GRGR..(odd line), BGBG..(even line), 16-bit samples, big-endian</summary>
    BAYER_GRBG16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_BAYER_GRBG16BE,

    /// <summary>Planar YUV 4:4:0, 20bpp, (1 Cr &amp; Cb sample per 1x2 Y samples), little-endian</summary>
    YUV440P10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV440P10LE,

    /// <summary>Planar YUV 4:4:0, 20bpp, (1 Cr &amp; Cb sample per 1x2 Y samples), big-endian</summary>
    YUV440P10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV440P10BE,

    /// <summary>Planar YUV 4:4:0, 24bpp, (1 Cr &amp; Cb sample per 1x2 Y samples), little-endian</summary>
    YUV440P12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV440P12LE,

    /// <summary>Planar YUV 4:4:0, 24bpp, (1 Cr &amp; Cb sample per 1x2 Y samples), big-endian</summary>
    YUV440P12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV440P12BE,

    /// <summary>Packed AYUV 4:4:4, 64bpp (1 Cr &amp; Cb sample per 1x1 Y &amp; A samples), little-endian</summary>
    AYUV64LE = AutoGen._AVPixelFormat.AV_PIX_FMT_AYUV64LE,

    /// <summary>Packed AYUV 4:4:4, 64bpp (1 Cr &amp; Cb sample per 1x1 Y &amp; A samples), big-endian</summary>
    AYUV64BE = AutoGen._AVPixelFormat.AV_PIX_FMT_AYUV64BE,

    /// <summary>Hardware decoding through Videotoolbox</summary>
    VIDEOTOOLBOX = AutoGen._AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,

    /// <summary>Like NV12, with 10bpp per component, data in the high bits, zeros in the low bits, little-endian</summary>
    P010LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P010LE,

    /// <summary>Like NV12, with 10bpp per component, data in the high bits, zeros in the low bits, big-endian</summary>
    P010BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P010BE,

    /// <summary>Planar GBR 4:4:4:4, 48bpp, big-endian</summary>
    GBRAP12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP12BE,

    /// <summary>Planar GBR 4:4:4:4, 48bpp, little-endian</summary>
    GBRAP12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP12LE,

    /// <summary>Planar GBR 4:4:4:4, 40bpp, big-endian</summary>
    GBRAP10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP10BE,

    /// <summary>Planar GBR 4:4:4:4, 40bpp, little-endian</summary>
    GBRAP10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP10LE,

    /// <summary>Hardware decoding through MediaCodec</summary>
    MEDIACODEC = AutoGen._AVPixelFormat.AV_PIX_FMT_MEDIACODEC,

    /// <summary>Y, 12bpp, big-endian</summary>
    GRAY12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY12BE,

    /// <summary>Y, 12bpp, little-endian</summary>
    GRAY12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY12LE,

    /// <summary>Y, 10bpp, big-endian</summary>
    GRAY10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY10BE,

    /// <summary>Y, 10bpp, little-endian</summary>
    GRAY10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY10LE,

    /// <summary>Like NV12, with 16bpp per component, little-endian</summary>
    P016LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P016LE,

    /// <summary>Like NV12, with 16bpp per component, big-endian</summary>
    P016BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P016BE,

    /// <summary>Hardware surfaces for Direct3D11.</summary>
    D3D11 = AutoGen._AVPixelFormat.AV_PIX_FMT_D3D11,

    /// <summary>Y, 9bpp, big-endian</summary>
    GRAY9BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY9BE,

    /// <summary>Y, 9bpp, little-endian</summary>
    GRAY9LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY9LE,

    /// <summary>IEEE-754 single precision planar GBR 4:4:4, 96bpp, big-endian</summary>
    GBRPF32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRPF32BE,

    /// <summary>IEEE-754 single precision planar GBR 4:4:4, 96bpp, little-endian</summary>
    GBRPF32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRPF32LE,

    /// <summary>IEEE-754 single precision planar GBRA 4:4:4:4, 128bpp, big-endian</summary>
    GBRAPF32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAPF32BE,

    /// <summary>IEEE-754 single precision planar GBRA 4:4:4:4, 128bpp, little-endian</summary>
    GBRAPF32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAPF32LE,

    /// <summary>DRM-managed buffers exposed through PRIME buffer sharing.</summary>
    DRM_PRIME = AutoGen._AVPixelFormat.AV_PIX_FMT_DRM_PRIME,

    /// <summary>Hardware surfaces for OpenCL.</summary>
    OPENCL = AutoGen._AVPixelFormat.AV_PIX_FMT_OPENCL,

    /// <summary>Y, 14bpp, big-endian</summary>
    GRAY14BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY14BE,

    /// <summary>Y, 14bpp, little-endian</summary>
    GRAY14LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY14LE,

    /// <summary>IEEE-754 single precision Y, 32bpp, big-endian</summary>
    GRAYF32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAYF32BE,

    /// <summary>IEEE-754 single precision Y, 32bpp, little-endian</summary>
    GRAYF32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAYF32LE,

    /// <summary>Planar YUV 4:2:2, 24bpp, (1 Cr &amp; Cb sample per 2x1 Y samples), 12b alpha, big-endian</summary>
    YUVA422P12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P12BE,

    /// <summary>Planar YUV 4:2:2, 24bpp, (1 Cr &amp; Cb sample per 2x1 Y samples), 12b alpha, little-endian</summary>
    YUVA422P12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA422P12LE,

    /// <summary>Planar YUV 4:4:4, 36bpp, (1 Cr &amp; Cb sample per 1x1 Y samples), 12b alpha, big-endian</summary>
    YUVA444P12BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P12BE,

    /// <summary>Planar YUV 4:4:4, 36bpp, (1 Cr &amp; Cb sample per 1x1 Y samples), 12b alpha, little-endian</summary>
    YUVA444P12LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUVA444P12LE,

    /// <summary>Planar YUV 4:4:4, 24bpp, 1 plane for Y and 1 plane for the UV components, which are interleaved (first byte U and the following byte V)</summary>
    NV24 = AutoGen._AVPixelFormat.AV_PIX_FMT_NV24,

    /// <summary>As above, but U and V bytes are swapped</summary>
    NV42 = AutoGen._AVPixelFormat.AV_PIX_FMT_NV42,

    /// <summary>Vulkan hardware images.</summary>
    VULKAN = AutoGen._AVPixelFormat.AV_PIX_FMT_VULKAN,

    /// <summary>Packed YUV 4:2:2 like YUYV422, 20bpp, data in the high bits, big-endian</summary>
    Y210BE = AutoGen._AVPixelFormat.AV_PIX_FMT_Y210BE,

    /// <summary>Packed YUV 4:2:2 like YUYV422, 20bpp, data in the high bits, little-endian</summary>
    Y210LE = AutoGen._AVPixelFormat.AV_PIX_FMT_Y210LE,

    /// <summary>Packed RGB 10:10:10, 30bpp, (msb)2X 10R 10G 10B(lsb), little-endian, X=unused/undefined</summary>
    X2RGB10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_X2RGB10LE,

    /// <summary>Packed RGB 10:10:10, 30bpp, (msb)2X 10R 10G 10B(lsb), big-endian, X=unused/undefined</summary>
    X2RGB10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_X2RGB10BE,

    /// <summary>Packed BGR 10:10:10, 30bpp, (msb)2X 10B 10G 10R(lsb), little-endian, X=unused/undefined</summary>
    X2BGR10LE = AutoGen._AVPixelFormat.AV_PIX_FMT_X2BGR10LE,

    /// <summary>Packed BGR 10:10:10, 30bpp, (msb)2X 10B 10G 10R(lsb), big-endian, X=unused/undefined</summary>
    X2BGR10BE = AutoGen._AVPixelFormat.AV_PIX_FMT_X2BGR10BE,

    /// <summary>Interleaved chroma YUV 4:2:2, 20bpp, data in the high bits, big-endian</summary>
    P210BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P210BE,

    /// <summary>Interleaved chroma YUV 4:2:2, 20bpp, data in the high bits, little-endian</summary>
    P210LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P210LE,

    /// <summary>Interleaved chroma YUV 4:4:4, 30bpp, data in the high bits, big-endian</summary>
    P410BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P410BE,

    /// <summary>Interleaved chroma YUV 4:4:4, 30bpp, data in the high bits, little-endian</summary>
    P410LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P410LE,

    /// <summary>Interleaved chroma YUV 4:2:2, 32bpp, big-endian</summary>
    P216BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P216BE,

    /// <summary>Interleaved chroma YUV 4:2:2, 32bpp, little-endian</summary>
    P216LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P216LE,

    /// <summary>Interleaved chroma YUV 4:4:4, 48bpp, big-endian</summary>
    P416BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P416BE,

    /// <summary>Interleaved chroma YUV 4:4:4, 48bpp, little-endian</summary>
    P416LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P416LE,

    /// <summary>Packed VUYA 4:4:4, 32bpp, VUYAVUYA...</summary>
    VUYA = AutoGen._AVPixelFormat.AV_PIX_FMT_VUYA,

    /// <summary>IEEE-754 half precision packed RGBA 16:16:16:16, 64bpp, RGBARGBA..., big-endian</summary>
    RGBAF16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBAF16BE,

    /// <summary>IEEE-754 half precision packed RGBA 16:16:16:16, 64bpp, RGBARGBA..., little-endian</summary>
    RGBAF16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBAF16LE,

    /// <summary>Packed VUYX 4:4:4, 32bpp, Variant of VUYA where alpha channel is left undefined</summary>
    VUYX = AutoGen._AVPixelFormat.AV_PIX_FMT_VUYX,

    /// <summary>Like NV12, with 12bpp per component, data in the high bits, zeros in the low bits, little-endian</summary>
    P012LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P012LE,

    /// <summary>Like NV12, with 12bpp per component, data in the high bits, zeros in the low bits, big-endian</summary>
    P012BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P012BE,

    /// <summary>Packed YUV 4:2:2 like YUYV422, 24bpp, data in the high bits, zeros in the low bits, big-endian</summary>
    Y212BE = AutoGen._AVPixelFormat.AV_PIX_FMT_Y212BE,

    /// <summary>Packed YUV 4:2:2 like YUYV422, 24bpp, data in the high bits, zeros in the low bits, little-endian</summary>
    Y212LE = AutoGen._AVPixelFormat.AV_PIX_FMT_Y212LE,

    /// <summary>Packed XVYU 4:4:4, 32bpp, (msb)2X 10V 10Y 10U(lsb), big-endian, variant of Y410 where alpha channel is left undefined</summary>
    XV30BE = AutoGen._AVPixelFormat.AV_PIX_FMT_XV30BE,

    /// <summary>Packed XVYU 4:4:4, 32bpp, (msb)2X 10V 10Y 10U(lsb), little-endian, variant of Y410 where alpha channel is left undefined</summary>
    XV30LE = AutoGen._AVPixelFormat.AV_PIX_FMT_XV30LE,

    /// <summary>Packed XVYU 4:4:4, 48bpp, data in the high bits, zeros in the low bits, big-endian, variant of Y412 where alpha channel is left undefined</summary>
    XV36BE = AutoGen._AVPixelFormat.AV_PIX_FMT_XV36BE,

    /// <summary>Packed XVYU 4:4:4, 48bpp, data in the high bits, zeros in the low bits, little-endian, variant of Y412 where alpha channel is left undefined</summary>
    XV36LE = AutoGen._AVPixelFormat.AV_PIX_FMT_XV36LE,

    /// <summary>IEEE-754 single precision packed RGB 32:32:32, 96bpp, RGBRGB..., big-endian</summary>
    RGBF32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBF32BE,

    /// <summary>IEEE-754 single precision packed RGB 32:32:32, 96bpp, RGBRGB..., little-endian</summary>
    RGBF32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBF32LE,

    /// <summary>IEEE-754 single precision packed RGBA 32:32:32:32, 128bpp, RGBARGBA..., big-endian</summary>
    RGBAF32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBAF32BE,

    /// <summary>IEEE-754 single precision packed RGBA 32:32:32:32, 128bpp, RGBARGBA..., little-endian</summary>
    RGBAF32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBAF32LE,

    /// <summary>Interleaved chroma YUV 4:2:2, 24bpp, data in the high bits, big-endian</summary>
    P212BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P212BE,

    /// <summary>Interleaved chroma YUV 4:2:2, 24bpp, data in the high bits, little-endian</summary>
    P212LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P212LE,

    /// <summary>Interleaved chroma YUV 4:4:4, 36bpp, data in the high bits, big-endian</summary>
    P412BE = AutoGen._AVPixelFormat.AV_PIX_FMT_P412BE,

    /// <summary>Interleaved chroma YUV 4:4:4, 36bpp, data in the high bits, little-endian</summary>
    P412LE = AutoGen._AVPixelFormat.AV_PIX_FMT_P412LE,

    /// <summary>Planar GBR 4:4:4:4 56bpp, big-endian</summary>
    GBRAP14BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP14BE,

    /// <summary>Planar GBR 4:4:4:4 56bpp, little-endian</summary>
    GBRAP14LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP14LE,

    /// <summary>Hardware surfaces for Direct3D 12.</summary>
    D3D12 = AutoGen._AVPixelFormat.AV_PIX_FMT_D3D12,

    /// <summary>Packed AYUV 4:4:4:4, 32bpp, AYUVAYUV...</summary>
    AYUV = AutoGen._AVPixelFormat.AV_PIX_FMT_AYUV,

    /// <summary>Packed UYVA 4:4:4:4, 32bpp, UYVAUYVA...</summary>
    UYVA = AutoGen._AVPixelFormat.AV_PIX_FMT_UYVA,

    /// <summary>Packed VYU 4:4:4, 24bpp, VYUVYU...</summary>
    VYU444 = AutoGen._AVPixelFormat.AV_PIX_FMT_VYU444,

    /// <summary>Packed VYUX 4:4:4, 32bpp, big-endian</summary>
    V30XBE = AutoGen._AVPixelFormat.AV_PIX_FMT_V30XBE,

    /// <summary>Packed VYUX 4:4:4, 32bpp, little-endian</summary>
    V30XLE = AutoGen._AVPixelFormat.AV_PIX_FMT_V30XLE,

    /// <summary>IEEE-754 half precision packed RGB 16:16:16, 48bpp, big-endian</summary>
    RGBF16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBF16BE,

    /// <summary>IEEE-754 half precision packed RGB 16:16:16, 48bpp, little-endian</summary>
    RGBF16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBF16LE,

    /// <summary>Packed RGBA 32:32:32:32, 128bpp, big-endian</summary>
    RGBA128BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA128BE,

    /// <summary>Packed RGBA 32:32:32:32, 128bpp, little-endian</summary>
    RGBA128LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGBA128LE,

    /// <summary>Packed RGBA 32:32:32, 96bpp, big-endian</summary>
    RGB96BE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB96BE,

    /// <summary>Packed RGBA 32:32:32, 96bpp, little-endian</summary>
    RGB96LE = AutoGen._AVPixelFormat.AV_PIX_FMT_RGB96LE,

    /// <summary>Packed YUV 4:2:2 like YUYV422, 32bpp, big-endian</summary>
    Y216BE = AutoGen._AVPixelFormat.AV_PIX_FMT_Y216BE,

    /// <summary>Packed YUV 4:2:2 like YUYV422, 32bpp, little-endian</summary>
    Y216LE = AutoGen._AVPixelFormat.AV_PIX_FMT_Y216LE,

    /// <summary>Packed XVYU 4:4:4, 64bpp, big-endian</summary>
    XV48BE = AutoGen._AVPixelFormat.AV_PIX_FMT_XV48BE,

    /// <summary>Packed XVYU 4:4:4, 64bpp, little-endian</summary>
    XV48LE = AutoGen._AVPixelFormat.AV_PIX_FMT_XV48LE,

    /// <summary>IEEE-754 half precision planer GBR 4:4:4, 48bpp, big-endian</summary>
    GBRPF16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRPF16BE,

    /// <summary>IEEE-754 half precision planer GBR 4:4:4, 48bpp, little-endian</summary>
    GBRPF16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRPF16LE,

    /// <summary>IEEE-754 half precision planar GBRA 4:4:4:4, 64bpp, big-endian</summary>
    GBRAPF16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAPF16BE,

    /// <summary>IEEE-754 half precision planar GBRA 4:4:4:4, 64bpp, little-endian</summary>
    GBRAPF16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAPF16LE,

    /// <summary>IEEE-754 half precision Y, 16bpp, big-endian</summary>
    GRAYF16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAYF16BE,

    /// <summary>IEEE-754 half precision Y, 16bpp, little-endian</summary>
    GRAYF16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAYF16LE,

    /// <summary>HW acceleration through AMF. data[0] contain AMFSurface pointer</summary>
    AMF_SURFACE = AutoGen._AVPixelFormat.AV_PIX_FMT_AMF_SURFACE,

    /// <summary>Y, 32bpp, big-endian</summary>
    GRAY32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY32BE,

    /// <summary>Y, 32bpp, little-endian</summary>
    GRAY32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GRAY32LE,

    /// <summary>IEEE-754 single precision packed YA, 32 bits gray, 32 bits alpha, 64bpp, big-endian</summary>
    YAF32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YAF32BE,

    /// <summary>IEEE-754 single precision packed YA, 32 bits gray, 32 bits alpha, 64bpp, little-endian</summary>
    YAF32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YAF32LE,

    /// <summary>IEEE-754 half precision packed YA, 16 bits gray, 16 bits alpha, 32bpp, big-endian</summary>
    YAF16BE = AutoGen._AVPixelFormat.AV_PIX_FMT_YAF16BE,

    /// <summary>IEEE-754 half precision packed YA, 16 bits gray, 16 bits alpha, 32bpp, little-endian</summary>
    YAF16LE = AutoGen._AVPixelFormat.AV_PIX_FMT_YAF16LE,

    /// <summary>Planar GBRA 4:4:4:4 128bpp, big-endian</summary>
    GBRAP32BE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP32BE,

    /// <summary>Planar GBRA 4:4:4:4 128bpp, little-endian</summary>
    GBRAP32LE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRAP32LE,

    /// <summary>Planar YUV 4:4:4, 30bpp, lowest bits zero, big-endian</summary>
    YUV444P10MSBBE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P10MSBBE,

    /// <summary>Planar YUV 4:4:4, 30bpp, lowest bits zero, little-endian</summary>
    YUV444P10MSBLE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P10MSBLE,

    /// <summary>Planar YUV 4:4:4, 30bpp, lowest bits zero, big-endian</summary>
    YUV444P12MSBBE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P12MSBBE,

    /// <summary>Planar YUV 4:4:4, 30bpp, lowest bits zero, little-endian</summary>
    YUV444P12MSBLE = AutoGen._AVPixelFormat.AV_PIX_FMT_YUV444P12MSBLE,

    /// <summary>Planar GBR 4:4:4 30bpp, lowest bits zero, big-endian</summary>
    GBRP10MSBBE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP10MSBBE,

    /// <summary>Planar GBR 4:4:4 30bpp, lowest bits zero, little-endian</summary>
    GBRP10MSBLE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP10MSBLE,

    /// <summary>Planar GBR 4:4:4 36bpp, lowest bits zero, big-endian</summary>
    GBRP12MSBBE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP12MSBBE,

    /// <summary>Planar GBR 4:4:4 36bpp, lowest bits zero, little-endian</summary>
    GBRP12MSBLE = AutoGen._AVPixelFormat.AV_PIX_FMT_GBRP12MSBLE,

    /// <summary>Special codec pixel format</summary>
    OHCODEC = AutoGen._AVPixelFormat.AV_PIX_FMT_OHCODEC,

    /// <summary>Number of pixel formats, DO NOT USE THIS if you want to link with shared libav* because the number of formats might differ between versions</summary>
    __COUNT__ = AutoGen._AVPixelFormat.AV_PIX_FMT_NB,
}





