using FFmpeg.AutoGen;

namespace FFmpeg.SideData;

/// <summary>Types of side data that can be associated with an <see cref="FFmpeg.Utils.AVFrame"/>.</summary>
public enum FrameSideDataType
{
    /// <summary>The data is an <c>AVPanScan</c> structure defined in libavcodec.</summary>
    PanScan = _AVFrameSideDataType.AV_FRAME_DATA_PANSCAN,

    /// <summary>
    /// ATSC A53 Part 4 Closed Captions. The caption bitstream is stored as a byte
    /// array, and the size of the side data indicates the number of bytes.
    /// </summary>
    A53CC = _AVFrameSideDataType.AV_FRAME_DATA_A53_CC,

    /// <summary>The data is an <c>AVStereo3D</c> structure containing stereoscopic 3D metadata.</summary>
    Stereo3D = _AVFrameSideDataType.AV_FRAME_DATA_STEREO3D,

    /// <summary>The data is an <c>AVMatrixEncoding</c> value.</summary>
    MatrixEncoding = _AVFrameSideDataType.AV_FRAME_DATA_MATRIXENCODING,

    /// <summary>The data is an <c>AVDownmixInfo</c> structure containing metadata relevant to an audio downmix procedure.</summary>
    DownmixInfo = _AVFrameSideDataType.AV_FRAME_DATA_DOWNMIX_INFO,

    /// <summary>The data is an <c>AVReplayGain</c> structure containing ReplayGain information.</summary>
    ReplayGain = _AVFrameSideDataType.AV_FRAME_DATA_REPLAYGAIN,

    /// <summary>
    /// A 3x3 transformation matrix describing an affine transformation that
    /// needs to be applied to the frame for correct presentation.
    /// </summary>
    DisplayMatrix = _AVFrameSideDataType.AV_FRAME_DATA_DISPLAYMATRIX,

    /// <summary>Active Format Description data consisting of a single byte as specified by ETSI TS 101 154.</summary>
    AFD = _AVFrameSideDataType.AV_FRAME_DATA_AFD,

    /// <summary>
    /// Motion vectors exported by some codecs when requested through the
    /// <c>export_mvs</c> option. The data consists of <c>AVMotionVector</c>
    /// structures.
    /// </summary>
    MotionVectors = _AVFrameSideDataType.AV_FRAME_DATA_MOTION_VECTORS,

    /// <summary>
    /// Recommends skipping the specified number of samples. This is exported
    /// only when the <c>skip_manual</c> option is enabled and uses the same
    /// format as <c>AV_PKT_DATA_SKIP_SAMPLES</c>.
    /// </summary>
    SkipSamples = _AVFrameSideDataType.AV_FRAME_DATA_SKIP_SAMPLES,

    /// <summary>The data must be associated with an audio frame and specifies its <c>AVAudioServiceType</c>.</summary>
    AudioServiceType = _AVFrameSideDataType.AV_FRAME_DATA_AUDIO_SERVICE_TYPE,

    /// <summary>
    /// Mastering display metadata associated with a video frame. The payload
    /// is an <c>AVMasteringDisplayMetadata</c> structure containing information
    /// about the mastering display color volume.
    /// </summary>
    MasteringDisplayMetadata = _AVFrameSideDataType.AV_FRAME_DATA_MASTERING_DISPLAY_METADATA,

    /// <summary>
    /// GOP timecode in 25-bit timecode format. The data is stored as a 64-bit
    /// integer and is set on the first frame of a GOP with a temporal reference of 0.
    /// </summary>
    GOPTimecode = _AVFrameSideDataType.AV_FRAME_DATA_GOP_TIMECODE,

    /// <summary>The data is an <c>AVSphericalMapping</c> structure containing spherical video mapping information.</summary>
    Spherical = _AVFrameSideDataType.AV_FRAME_DATA_SPHERICAL,

    /// <summary>
    /// Content light level metadata based on CTA-861.3. The payload is an
    /// <c>AVContentLightMetadata</c> structure.
    /// </summary>
    ContentLightLevel = _AVFrameSideDataType.AV_FRAME_DATA_CONTENT_LIGHT_LEVEL,

    /// <summary>
    /// An ICC profile stored as an opaque byte buffer following ISO 15076-1,
    /// optionally accompanied by a name in the <c>name</c> metadata entry.
    /// </summary>
    ICCProfile = _AVFrameSideDataType.AV_FRAME_DATA_ICC_PROFILE,

    /// <summary>
    /// Timecode conforming to SMPTE ST 12-1. The data is an array of four
    /// <c>uint32</c> values describing up to three timecodes.
    /// </summary>
    S12MTimecode = _AVFrameSideDataType.AV_FRAME_DATA_S12M_TIMECODE,

    /// <summary>
    /// HDR dynamic metadata associated with a video frame. The payload is an
    /// <c>AVDynamicHDRPlus</c> structure containing color volume transform
    /// information as specified by application 4 of SMPTE 2094-40:2016.
    /// </summary>
    DynamicHDRPlus = _AVFrameSideDataType.AV_FRAME_DATA_DYNAMIC_HDR_PLUS,

    /// <summary>
    /// Regions of interest represented by an array of <c>AVRegionOfInterest</c>
    /// structures.
    /// </summary>
    RegionsOfInterest = _AVFrameSideDataType.AV_FRAME_DATA_REGIONS_OF_INTEREST,

    /// <summary>Encoding parameters for a video frame, as described by <c>AVVideoEncParams</c>.</summary>
    VideoEncParams = _AVFrameSideDataType.AV_FRAME_DATA_VIDEO_ENC_PARAMS,

    /// <summary>
    /// User data unregistered metadata associated with a video frame. This is
    /// the H.264/H.265 user data unregistered SEI message.
    /// </summary>
    SEIUnregistered = _AVFrameSideDataType.AV_FRAME_DATA_SEI_UNREGISTERED,

    /// <summary>
    /// Film grain parameters for a frame, described by an <c>AVFilmGrainParams</c>
    /// structure. This side data must be present for every frame that should
    /// have film grain applied.
    /// </summary>
    FilmGrainParams = _AVFrameSideDataType.AV_FRAME_DATA_FILM_GRAIN_PARAMS,

    /// <summary>
    /// Bounding boxes for object detection and classification, as described by
    /// <c>AVDetectionBBoxHeader</c>.
    /// </summary>
    DetectionBBoxes = _AVFrameSideDataType.AV_FRAME_DATA_DETECTION_BBOXES,

    /// <summary>
    /// Raw Dolby Vision RPU data suitable for passing to x265 or other libraries.
    /// The data is a byte array with NAL emulation bytes intact.
    /// </summary>
    DOVIRPUBuffer = _AVFrameSideDataType.AV_FRAME_DATA_DOVI_RPU_BUFFER,

    /// <summary>
    /// Parsed Dolby Vision metadata suitable for passing to a software
    /// implementation. The payload is an <c>AVDOVIMetadata</c> structure.
    /// </summary>
    DOVIMetadata = _AVFrameSideDataType.AV_FRAME_DATA_DOVI_METADATA,

    /// <summary>
    /// HDR Vivid dynamic metadata associated with a video frame. The payload
    /// is an <c>AVDynamicHDRVivid</c> structure containing color volume
    /// transform information as specified by CUVA 005.1-2021.
    /// </summary>
    DynamicHDRVivid = _AVFrameSideDataType.AV_FRAME_DATA_DYNAMIC_HDR_VIVID,

    /// <summary>Ambient viewing environment metadata as defined by H.274.</summary>
    AmbientViewingEnvironment = _AVFrameSideDataType.AV_FRAME_DATA_AMBIENT_VIEWING_ENVIRONMENT,

    /// <summary>
    /// Encoder-specific hinting information about changed or unchanged portions
    /// of a frame. This can be used to identify portions that can be skipped
    /// because they have not changed from the corresponding portions of the
    /// previous frame.
    /// </summary>
    VideoHint = _AVFrameSideDataType.AV_FRAME_DATA_VIDEO_HINT,

    /// <summary>
    /// Raw LCEVC payload data stored as a byte array with NAL emulation bytes intact.
    /// </summary>
    LCEVC = _AVFrameSideDataType.AV_FRAME_DATA_LCEVC,

    /// <summary>
    /// Identifies the view of a multi-view video frame. The data is an integer
    /// containing the view ID.
    /// </summary>
    ViewID = _AVFrameSideDataType.AV_FRAME_DATA_VIEW_ID,

    /// <summary>
    /// Information about reference display widths and viewing distances, as well
    /// as the corresponding reference stereo pairs. The payload is an
    /// <c>AV3DReferenceDisplaysInfo</c> structure.
    /// </summary>
    ReferenceDisplays3D = _AVFrameSideDataType.AV_FRAME_DATA_3D_REFERENCE_DISPLAYS,

    /// <summary>
    /// Exchangeable Image File Format (EXIF) metadata. The payload is a buffer
    /// containing EXIF metadata beginning with a TIFF header that specifies
    /// the byte order.
    /// </summary>
    EXIF = _AVFrameSideDataType.AV_FRAME_DATA_EXIF,

    /// <summary>
    /// HDR dynamic metadata associated with a video frame. The payload is an
    /// <c>AVDynamicHDRSmpte2094App5</c> structure containing color volume
    /// transform information as specified by SMPTE 2094-50.
    /// </summary>
    DynamicHDRSmpte2094App5 = _AVFrameSideDataType.@AV_FRAME_DATA_DYNAMIC_HDR_SMPTE_2094_APP5,

    /// <summary>
    /// IAMF Mix Gain parameter data associated with an audio frame. The payload
    /// is an <c>AVIAMFParamDefinition</c> structure containing information
    /// defined in sections 3.6.1 and 3.8.1 of the Immersive Audio Model and
    /// Formats standard.
    /// </summary>
    IAMFMixGainParam = _AVFrameSideDataType.@AV_FRAME_DATA_IAMF_MIX_GAIN_PARAM,

    /// <summary>
    /// IAMF Demixing Info parameter data associated with an audio frame. The
    /// payload is an <c>AVIAMFParamDefinition</c> structure containing
    /// information defined in sections 3.6.1 and 3.8.2 of the Immersive Audio
    /// Model and Formats standard.
    /// </summary>
    IAMFDemixingInfoParam = _AVFrameSideDataType.@AV_FRAME_DATA_IAMF_DEMIXING_INFO_PARAM,

    /// <summary>
    /// IAMF Recon Gain Info parameter data associated with an audio frame. The
    /// payload is an <c>AVIAMFParamDefinition</c> structure containing
    /// information defined in sections 3.6.1 and 3.8.3 of the Immersive Audio
    /// Model and Formats standard.
    /// </summary>
    IAMFReconGainInfoParam = _AVFrameSideDataType.@AV_FRAME_DATA_IAMF_RECON_GAIN_INFO_PARAM,

    /// <summary>
    /// Color information from a RAW camera codec, needed to correctly process
    /// the video data. The payload is an <c>AVRawColorParams</c> structure
    /// defined in <c>libavutil/raw_color_params.h</c>.
    /// </summary>
    RawColorParams = _AVFrameSideDataType.@AV_FRAME_DATA_RAW_COLOR_PARAMS,


}