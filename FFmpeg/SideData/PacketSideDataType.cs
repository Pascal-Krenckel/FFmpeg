using FFmpeg.AutoGen;

namespace FFmpeg.SideData;

/// <summary>Types of side data that can be associated with an <see cref="FFmpeg.Utils.AVPacket"/>.</summary>
public enum PacketSideDataType
{
    /// <summary>
    /// A palette containing exactly <c>AVPALETTE_SIZE</c> bytes. The presence
    /// of this side data indicates that a new palette is present.
    /// </summary>
    Palette = _AVPacketSideDataType.AV_PKT_DATA_PALETTE,

    /// <summary>
    /// Indicates that the codec or format extradata has changed. The new
    /// extradata is embedded in the side data and should be immediately used
    /// for processing the current packet.
    /// </summary>
    NewExtradata = _AVPacketSideDataType.AV_PKT_DATA_NEW_EXTRADATA,

    /// <summary>
    /// Indicates that codec parameters have changed. The side data contains
    /// the updated parameter information.
    /// </summary>
    ParamChange = _AVPacketSideDataType.AV_PKT_DATA_PARAM_CHANGE,

    /// <summary>
    /// H.263 macroblock information used when splitting packets at macroblock
    /// boundaries, such as when generating RFC 2190 packets.
    /// </summary>
    H263MBInfo = _AVPacketSideDataType.AV_PKT_DATA_H263_MB_INFO,

    /// <summary>
    /// ReplayGain information associated with an audio stream. The payload is
    /// an <c>AVReplayGain</c> structure.
    /// </summary>
    ReplayGain = _AVPacketSideDataType.AV_PKT_DATA_REPLAYGAIN,

    /// <summary>
    /// A 3x3 transformation matrix describing an affine transformation that
    /// needs to be applied to decoded video frames for correct presentation.
    /// </summary>
    DisplayMatrix = _AVPacketSideDataType.AV_PKT_DATA_DISPLAYMATRIX,

    /// <summary>
    /// Stereoscopic 3D information associated with a video stream. The payload
    /// is an <c>AVStereo3D</c> structure.
    /// </summary>
    Stereo3D = _AVPacketSideDataType.AV_PKT_DATA_STEREO3D,

    /// <summary>
    /// Audio service type associated with an audio stream, corresponding to
    /// <c>AVAudioServiceType</c>.
    /// </summary>
    AudioServiceType = _AVPacketSideDataType.AV_PKT_DATA_AUDIO_SERVICE_TYPE,

    /// <summary>Quality-related information provided by the encoder.</summary>
    QualityStats = _AVPacketSideDataType.AV_PKT_DATA_QUALITY_STATS,

    /// <summary>
    /// The stream index of a fallback track. A fallback track is an alternate
    /// track to use when the current track cannot be decoded, for example when
    /// no decoder is available for its codec.
    /// </summary>
    FallbackTrack = _AVPacketSideDataType.AV_PKT_DATA_FALLBACK_TRACK,

    /// <summary>
    /// Codec Picture Buffer properties stored as an <c>AVCPBProperties</c>
    /// structure.
    /// </summary>
    CPBProperties = _AVPacketSideDataType.AV_PKT_DATA_CPB_PROPERTIES,

    /// <summary>
    /// Recommends skipping the specified number of samples.
    /// </summary>
    SkipSamples = _AVPacketSideDataType.AV_PKT_DATA_SKIP_SAMPLES,

    /// <summary>
    /// Japanese DTV dual-mono audio information indicating that only the
    /// selected channel should be used.
    /// </summary>
    JPDualMono = _AVPacketSideDataType.AV_PKT_DATA_JP_DUALMONO,

    /// <summary>
    /// A list of zero-terminated key/value metadata strings. The side data
    /// size determines the end of the list because there is no end marker.
    /// </summary>
    StringsMetadata = _AVPacketSideDataType.AV_PKT_DATA_STRINGS_METADATA,

    /// <summary>Subtitle event position information.</summary>
    SubtitlePosition = _AVPacketSideDataType.AV_PKT_DATA_SUBTITLE_POSITION,

    /// <summary>
    /// Data found in the Matroska <c>BlockAdditional</c> element. The data
    /// consists of an 8-byte identifier followed by the additional data.
    /// </summary>
    MatroskaBlockAdditional = _AVPacketSideDataType.AV_PKT_DATA_MATROSKA_BLOCKADDITIONAL,

    /// <summary>The optional identifier line of a WebVTT cue.</summary>
    WebVTTIdentifier = _AVPacketSideDataType.AV_PKT_DATA_WEBVTT_IDENTIFIER,

    /// <summary>
    /// The optional rendering settings that immediately follow the timestamp
    /// specifier of a WebVTT cue.
    /// </summary>
    WebVTTSettings = _AVPacketSideDataType.AV_PKT_DATA_WEBVTT_SETTINGS,

    /// <summary>
    /// A list of zero-terminated key/value metadata strings containing
    /// metadata updates that appeared in the stream.
    /// </summary>
    MetadataUpdate = _AVPacketSideDataType.AV_PKT_DATA_METADATA_UPDATE,

    /// <summary>
    /// MPEG-TS stream ID stored as a byte. This is used to pass stream ID
    /// information from the demuxer to the corresponding muxer.
    /// </summary>
    MPEGTSStreamID = _AVPacketSideDataType.AV_PKT_DATA_MPEGTS_STREAM_ID,

    /// <summary>
    /// Mastering display metadata based on SMPTE 2086:2014. The payload is an
    /// <c>AVMasteringDisplayMetadata</c> structure associated with a video stream.
    /// </summary>
    MasteringDisplayMetadata = _AVPacketSideDataType.AV_PKT_DATA_MASTERING_DISPLAY_METADATA,

    /// <summary>
    /// Spherical video mapping information associated with a video stream.
    /// The payload is an <c>AVSphericalMapping</c> structure.
    /// </summary>
    Spherical = _AVPacketSideDataType.AV_PKT_DATA_SPHERICAL,

    /// <summary>
    /// Content light level metadata based on CTA-861.3. The payload is an
    /// <c>AVContentLightMetadata</c> structure associated with a video stream.
    /// </summary>
    ContentLightLevel = _AVPacketSideDataType.AV_PKT_DATA_CONTENT_LIGHT_LEVEL,

    /// <summary>
    /// ATSC A53 Part 4 Closed Captions associated with a video stream. The
    /// caption bitstream is stored as a byte array, and the side data size
    /// indicates the number of bytes.
    /// </summary>
    A53CC = _AVPacketSideDataType.AV_PKT_DATA_A53_CC,

    /// <summary>
    /// Encryption initialization data. The data format is not part of the
    /// ABI and should be accessed through the corresponding FFmpeg encryption
    /// initialization information functions.
    /// </summary>
    EncryptionInitInfo = _AVPacketSideDataType.AV_PKT_DATA_ENCRYPTION_INIT_INFO,

    /// <summary>
    /// Encryption information describing how to decrypt the packet. The data
    /// format is not part of the ABI and should be accessed through the
    /// corresponding FFmpeg encryption information functions.
    /// </summary>
    EncryptionInfo = _AVPacketSideDataType.AV_PKT_DATA_ENCRYPTION_INFO,

    /// <summary>
    /// Active Format Description data consisting of a single byte as specified
    /// by ETSI TS 101 154.
    /// </summary>
    AFD = _AVPacketSideDataType.AV_PKT_DATA_AFD,

    /// <summary>
    /// Producer Reference Time information stored as an
    /// <c>AVProducerReferenceTime</c> structure.
    /// </summary>
    PRFT = _AVPacketSideDataType.AV_PKT_DATA_PRFT,

    /// <summary>
    /// An ICC profile stored as an opaque byte buffer following the format
    /// described by ISO 15076-1.
    /// </summary>
    ICCProfile = _AVPacketSideDataType.AV_PKT_DATA_ICC_PROFILE,

    /// <summary>
    /// Dolby Vision decoder configuration stored as an
    /// <c>AVDOVIDecoderConfigurationRecord</c> structure.
    /// </summary>
    DOVIConf = _AVPacketSideDataType.AV_PKT_DATA_DOVI_CONF,

    /// <summary>
    /// Timecode conforming to SMPTE ST 12-1:2014. The data contains up to
    /// three timecodes in the format described by FFmpeg's timecode utilities.
    /// </summary>
    S12MTimecode = _AVPacketSideDataType.AV_PKT_DATA_S12M_TIMECODE,

    /// <summary>
    /// HDR10+ dynamic metadata associated with a video frame. The payload is
    /// an <c>AVDynamicHDRPlus</c> structure containing color volume transform
    /// information as specified by application 4 of SMPTE 2094-40:2016.
    /// </summary>
    DynamicHDR10Plus = _AVPacketSideDataType.AV_PKT_DATA_DYNAMIC_HDR10_PLUS,

    /// <summary>
    /// IAMF Mix Gain parameter data associated with an audio frame. The payload
    /// is an <c>AVIAMFParamDefinition</c> structure containing information
    /// defined in sections 3.6.1 and 3.8.1 of the Immersive Audio Model and
    /// Formats standard.
    /// </summary>
    IAMFMixGainParam = _AVPacketSideDataType.AV_PKT_DATA_IAMF_MIX_GAIN_PARAM,

    /// <summary>
    /// IAMF Demixing Info parameter data associated with an audio frame. The
    /// payload is an <c>AVIAMFParamDefinition</c> structure containing
    /// information defined in sections 3.6.1 and 3.8.2 of the Immersive Audio
    /// Model and Formats standard.
    /// </summary>
    IAMFDemixingInfoParam = _AVPacketSideDataType.AV_PKT_DATA_IAMF_DEMIXING_INFO_PARAM,

    /// <summary>
    /// IAMF Recon Gain Info parameter data associated with an audio frame. The
    /// payload is an <c>AVIAMFParamDefinition</c> structure containing
    /// information defined in sections 3.6.1 and 3.8.3 of the Immersive Audio
    /// Model and Formats standard.
    /// </summary>
    IAMFReconGainInfoParam = _AVPacketSideDataType.AV_PKT_DATA_IAMF_RECON_GAIN_INFO_PARAM,

    /// <summary>
    /// Ambient viewing environment metadata defined by H.274. The payload is
    /// an <c>AVAmbientViewingEnvironment</c> structure associated with a video
    /// stream.
    /// </summary>
    AmbientViewingEnvironment = _AVPacketSideDataType.AV_PKT_DATA_AMBIENT_VIEWING_ENVIRONMENT,

    /// <summary>
    /// The number of pixels to discard from the top, bottom, left, and right
    /// borders of a decoded frame to obtain the sub-rectangle intended for
    /// presentation.
    /// </summary>
    FrameCropping = _AVPacketSideDataType.AV_PKT_DATA_FRAME_CROPPING,

    /// <summary>
    /// Raw LCEVC payload data stored as a byte array with NAL emulation bytes
    /// intact.
    /// </summary>
    LCEVC = _AVPacketSideDataType.AV_PKT_DATA_LCEVC,

    /// <summary>
    /// Information about reference display widths and viewing distances, as
    /// well as the corresponding reference stereo pairs. The payload is an
    /// <c>AV3DReferenceDisplaysInfo</c> structure.
    /// </summary>
    ReferenceDisplays3D = _AVPacketSideDataType.AV_PKT_DATA_3D_REFERENCE_DISPLAYS,

    /// <summary>
    /// The last received RTCP Sender Report (SR) information. The payload is
    /// an <c>AVRTCPSenderReport</c> structure.
    /// </summary>
    RTCPSenderReport = _AVPacketSideDataType.AV_PKT_DATA_RTCP_SR,

    /// <summary>
    /// Exchangeable Image File Format (EXIF) metadata. The payload is a buffer
    /// containing EXIF metadata beginning with a TIFF header.
    /// </summary>
    EXIF = _AVPacketSideDataType.AV_PKT_DATA_EXIF,

    /// <summary>
    /// HDR dynamic metadata associated with a video frame. The payload is an
    /// <c>AVDynamicHDRSmpte2094App5</c> structure containing color volume
    /// transform information as specified by SMPTE 2094-50.
    /// </summary>
    DynamicHDRSmpte2094App5 = _AVPacketSideDataType.@AV_PKT_DATA_DYNAMIC_HDR_SMPTE_2094_APP5,

    /// <summary>
    /// Dolby Vision enhancement-layer HEVC decoder configuration. The data is
    /// a raw <c>HEVCDecoderConfigurationRecord</c> as defined in ISO 14496-15.
    /// </summary>
    HEVCConf = _AVPacketSideDataType.AV_PKT_DATA_HEVC_CONF,

    /// <summary>
    /// The number of packet side data types. This value is not part of the
    /// public API or ABI and may change when new side data types are added.
    /// It must remain the last enum value.
    /// </summary>
    __COUNT__ = _AVPacketSideDataType.AV_PKT_DATA_NB,


}
