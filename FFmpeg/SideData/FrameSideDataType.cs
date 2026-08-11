using FFmpeg.AutoGen;

namespace FFmpeg.SideData;

/// <summary>Types of side data that can be associated with an <see cref="AVFrame"/>.</summary>
public enum FrameSideDataType
{
    /// <summary>The data is an <c>AVPanScan</c> structure.</summary>
    PanScan = _AVFrameSideDataType.AV_FRAME_DATA_PANSCAN,

    /// <summary>ATSC A53 Part 4 Closed Captions.</summary>
    A53CC = _AVFrameSideDataType.AV_FRAME_DATA_A53_CC,

    /// <summary>Stereoscopic 3D metadata stored as an <c>AVStereo3D</c> structure.</summary>
    Stereo3D = _AVFrameSideDataType.AV_FRAME_DATA_STEREO3D,

    /// <summary>The data is an <c>AVMatrixEncoding</c> value.</summary>
    MatrixEncoding = _AVFrameSideDataType.AV_FRAME_DATA_MATRIXENCODING,

    /// <summary>Metadata relevant to an audio downmix procedure.</summary>
    DownmixInfo = _AVFrameSideDataType.AV_FRAME_DATA_DOWNMIX_INFO,

    /// <summary>ReplayGain information stored as an <c>AVReplayGain</c> structure.</summary>
    ReplayGain = _AVFrameSideDataType.AV_FRAME_DATA_REPLAYGAIN,

    /// <summary>A 3x3 transformation matrix describing an affine transformation required for correct frame presentation.</summary>
    DisplayMatrix = _AVFrameSideDataType.AV_FRAME_DATA_DISPLAYMATRIX,

    /// <summary>Active Format Description data.</summary>
    AFD = _AVFrameSideDataType.AV_FRAME_DATA_AFD,

    /// <summary>Motion vectors exported by codecs when requested through the <c>export_mvs</c> option.</summary>
    MotionVectors = _AVFrameSideDataType.AV_FRAME_DATA_MOTION_VECTORS,

    /// <summary>Indicates the number of samples that should be skipped.</summary>
    SkipSamples = _AVFrameSideDataType.AV_FRAME_DATA_SKIP_SAMPLES,

    /// <summary>Audio service type associated with an audio frame.</summary>
    AudioServiceType = _AVFrameSideDataType.AV_FRAME_DATA_AUDIO_SERVICE_TYPE,

    /// <summary>Mastering display metadata associated with a video frame.</summary>
    MasteringDisplayMetadata = _AVFrameSideDataType.AV_FRAME_DATA_MASTERING_DISPLAY_METADATA,

    /// <summary>GOP timecode in 25-bit timecode format.</summary>
    GOPTimecode = _AVFrameSideDataType.AV_FRAME_DATA_GOP_TIMECODE,

    /// <summary>Spherical video mapping information stored as an <c>AVSphericalMapping</c> structure.</summary>
    Spherical = _AVFrameSideDataType.AV_FRAME_DATA_SPHERICAL,

    /// <summary>Content light level metadata based on CTA-861.3.</summary>
    ContentLightLevel = _AVFrameSideDataType.AV_FRAME_DATA_CONTENT_LIGHT_LEVEL,

    /// <summary>An ICC profile stored as an opaque octet buffer.</summary>
    ICCProfile = _AVFrameSideDataType.AV_FRAME_DATA_ICC_PROFILE,

    /// <summary>Timecode conforming to SMPTE ST 12-1.</summary>
    S12MTimecode = _AVFrameSideDataType.AV_FRAME_DATA_S12M_TIMECODE,

    /// <summary>HDR dynamic metadata stored as an <c>AVDynamicHDRPlus</c> structure.</summary>
    DynamicHDRPlus = _AVFrameSideDataType.AV_FRAME_DATA_DYNAMIC_HDR_PLUS,

    /// <summary>Regions of interest described by <c>AVRegionOfInterest</c> structures.</summary>
    RegionsOfInterest = _AVFrameSideDataType.AV_FRAME_DATA_REGIONS_OF_INTEREST,

    /// <summary>Video encoding parameters stored as <c>AVVideoEncParams</c>.</summary>
    VideoEncParams = _AVFrameSideDataType.AV_FRAME_DATA_VIDEO_ENC_PARAMS,

    /// <summary>User data unregistered metadata from an H.264 or H.265 SEI message.</summary>
    SEIUnregistered = _AVFrameSideDataType.AV_FRAME_DATA_SEI_UNREGISTERED,

    /// <summary>Film grain parameters stored as an <c>AVFilmGrainParams</c> structure.</summary>
    FilmGrainParams = _AVFrameSideDataType.AV_FRAME_DATA_FILM_GRAIN_PARAMS,

    /// <summary>Bounding boxes for object detection and classification.</summary>
    DetectionBBoxes = _AVFrameSideDataType.AV_FRAME_DATA_DETECTION_BBOXES,

    /// <summary>Raw Dolby Vision RPU data.</summary>
    DOVIRPUBuffer = _AVFrameSideDataType.AV_FRAME_DATA_DOVI_RPU_BUFFER,

    /// <summary>Parsed Dolby Vision metadata stored as an <c>AVDOVIMetadata</c> structure.</summary>
    DOVIMetadata = _AVFrameSideDataType.AV_FRAME_DATA_DOVI_METADATA,

    /// <summary>HDR Vivid dynamic metadata associated with a video frame.</summary>
    DynamicHDRVivid = _AVFrameSideDataType.AV_FRAME_DATA_DYNAMIC_HDR_VIVID,

    /// <summary>Ambient viewing environment metadata as defined by H.274.</summary>
    AmbientViewingEnvironment = _AVFrameSideDataType.AV_FRAME_DATA_AMBIENT_VIEWING_ENVIRONMENT,

    /// <summary>Encoder-specific hinting information about changed or unchanged portions of a frame.</summary>
    VideoHint = _AVFrameSideDataType.AV_FRAME_DATA_VIDEO_HINT,

    /// <summary>Raw LCEVC payload data.</summary>
    LCEVC = _AVFrameSideDataType.AV_FRAME_DATA_LCEVC,

    /// <summary>Identifies the view of a multi-view video frame.</summary>
    ViewID = _AVFrameSideDataType.AV_FRAME_DATA_VIEW_ID,

    /// <summary>Information about reference displays and corresponding reference stereo pairs.</summary>
    ReferenceDisplays3D = _AVFrameSideDataType.AV_FRAME_DATA_3D_REFERENCE_DISPLAYS,

    /// <summary>EXIF metadata stored in the format defined by the Exif specification.</summary>
    EXIF = _AVFrameSideDataType.AV_FRAME_DATA_EXIF,
}
