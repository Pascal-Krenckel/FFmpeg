using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.SideData;

/// <summary>Types of side data that can be associated with an <see cref="AVPacket"/>.</summary>
public enum PacketSideDataType
{
    /// <summary>A palette containing exactly <c>AVPALETTE_SIZE</c> bytes, indicating that a new palette is present.</summary>
    Palette = _AVPacketSideDataType.AV_PKT_DATA_PALETTE,

    /// <summary>New codec or format extradata that should immediately be used for processing the current packet.</summary>
    NewExtradata = _AVPacketSideDataType.AV_PKT_DATA_NEW_EXTRADATA,

    /// <summary>Indicates that codec parameters have changed.</summary>
    ParamChange = _AVPacketSideDataType.AV_PKT_DATA_PARAM_CHANGE,

    /// <summary>H.263 macroblock information used when splitting packets at macroblock boundaries.</summary>
    H263MBInfo = _AVPacketSideDataType.AV_PKT_DATA_H263_MB_INFO,

    /// <summary>ReplayGain information associated with an audio stream.</summary>
    ReplayGain = _AVPacketSideDataType.AV_PKT_DATA_REPLAYGAIN,

    /// <summary>A 3x3 transformation matrix describing an affine transformation required for correct presentation of decoded video frames.</summary>
    DisplayMatrix = _AVPacketSideDataType.AV_PKT_DATA_DISPLAYMATRIX,

    /// <summary>Stereoscopic 3D information associated with a video stream.</summary>
    Stereo3D = _AVPacketSideDataType.AV_PKT_DATA_STEREO3D,

    /// <summary>Audio service type associated with an audio stream.</summary>
    AudioServiceType = _AVPacketSideDataType.AV_PKT_DATA_AUDIO_SERVICE_TYPE,

    /// <summary>Quality-related information provided by the encoder.</summary>
    QualityStats = _AVPacketSideDataType.AV_PKT_DATA_QUALITY_STATS,

    /// <summary>The stream index of a fallback track to use when the current track cannot be decoded.</summary>
    FallbackTrack = _AVPacketSideDataType.AV_PKT_DATA_FALLBACK_TRACK,

    /// <summary>Codec Picture Buffer properties stored as an <c>AVCPBProperties</c> structure.</summary>
    CPBProperties = _AVPacketSideDataType.AV_PKT_DATA_CPB_PROPERTIES,

    /// <summary>Indicates the number of samples that should be skipped.</summary>
    SkipSamples = _AVPacketSideDataType.AV_PKT_DATA_SKIP_SAMPLES,

    /// <summary>Japanese DTV dual-mono audio information indicating which channel should be used.</summary>
    JPDualMono = _AVPacketSideDataType.AV_PKT_DATA_JP_DUALMONO,

    /// <summary>A list of zero-terminated key/value metadata strings.</summary>
    StringsMetadata = _AVPacketSideDataType.AV_PKT_DATA_STRINGS_METADATA,

    /// <summary>Subtitle event position information.</summary>
    SubtitlePosition = _AVPacketSideDataType.AV_PKT_DATA_SUBTITLE_POSITION,

    /// <summary>Data from the Matroska <c>BlockAdditional</c> element.</summary>
    MatroskaBlockAdditional = _AVPacketSideDataType.AV_PKT_DATA_MATROSKA_BLOCKADDITIONAL,

    /// <summary>The optional identifier line of a WebVTT cue.</summary>
    WebVTTIdentifier = _AVPacketSideDataType.AV_PKT_DATA_WEBVTT_IDENTIFIER,

    /// <summary>The optional rendering settings immediately following the timestamp of a WebVTT cue.</summary>
    WebVTTSettings = _AVPacketSideDataType.AV_PKT_DATA_WEBVTT_SETTINGS,

    /// <summary>Updated metadata that appeared in the stream.</summary>
    MetadataUpdate = _AVPacketSideDataType.AV_PKT_DATA_METADATA_UPDATE,

    /// <summary>MPEG-TS stream ID.</summary>
    MPEGTSStreamID = _AVPacketSideDataType.AV_PKT_DATA_MPEGTS_STREAM_ID,

    /// <summary>Mastering display metadata associated with a video stream.</summary>
    MasteringDisplayMetadata = _AVPacketSideDataType.AV_PKT_DATA_MASTERING_DISPLAY_METADATA,

    /// <summary>Spherical video mapping information associated with a video stream.</summary>
    Spherical = _AVPacketSideDataType.AV_PKT_DATA_SPHERICAL,

    /// <summary>Content light level metadata associated with a video stream.</summary>
    ContentLightLevel = _AVPacketSideDataType.AV_PKT_DATA_CONTENT_LIGHT_LEVEL,

    /// <summary>ATSC A53 Part 4 Closed Captions associated with a video stream.</summary>
    A53CC = _AVPacketSideDataType.AV_PKT_DATA_A53_CC,

    /// <summary>Encryption initialization data.</summary>
    EncryptionInitInfo = _AVPacketSideDataType.AV_PKT_DATA_ENCRYPTION_INIT_INFO,

    /// <summary>Encryption information describing how to decrypt the packet.</summary>
    EncryptionInfo = _AVPacketSideDataType.AV_PKT_DATA_ENCRYPTION_INFO,

    /// <summary>Active Format Description data.</summary>
    AFD = _AVPacketSideDataType.AV_PKT_DATA_AFD,

    /// <summary>Producer Reference Time information stored as an <c>AVProducerReferenceTime</c> structure.</summary>
    PRFT = _AVPacketSideDataType.AV_PKT_DATA_PRFT,

    /// <summary>An ICC profile stored as an opaque octet buffer.</summary>
    ICCProfile = _AVPacketSideDataType.AV_PKT_DATA_ICC_PROFILE,

    /// <summary>Dolby Vision decoder configuration stored as an <c>AVDOVIDecoderConfigurationRecord</c> structure.</summary>
    DOVIConf = _AVPacketSideDataType.AV_PKT_DATA_DOVI_CONF,

    /// <summary>Timecode conforming to SMPTE ST 12-1:2014.</summary>
    S12MTimecode = _AVPacketSideDataType.AV_PKT_DATA_S12M_TIMECODE,

    /// <summary>HDR10+ dynamic metadata associated with a video frame.</summary>
    DynamicHDR10Plus = _AVPacketSideDataType.AV_PKT_DATA_DYNAMIC_HDR10_PLUS,

    /// <summary>IAMF Mix Gain Parameter Data associated with an audio frame.</summary>
    IAMFMixGainParam = _AVPacketSideDataType.AV_PKT_DATA_IAMF_MIX_GAIN_PARAM,

    /// <summary>IAMF Demixing Info Parameter Data associated with an audio frame.</summary>
    IAMFDemixingInfoParam = _AVPacketSideDataType.AV_PKT_DATA_IAMF_DEMIXING_INFO_PARAM,

    /// <summary>IAMF Recon Gain Info Parameter Data associated with an audio frame.</summary>
    IAMFReconGainInfoParam = _AVPacketSideDataType.AV_PKT_DATA_IAMF_RECON_GAIN_INFO_PARAM,

    /// <summary>Ambient viewing environment metadata associated with a video stream.</summary>
    AmbientViewingEnvironment = _AVPacketSideDataType.AV_PKT_DATA_AMBIENT_VIEWING_ENVIRONMENT,

    /// <summary>The number of pixels to discard from each border of the decoded frame to obtain the intended presentation rectangle.</summary>
    FrameCropping = _AVPacketSideDataType.AV_PKT_DATA_FRAME_CROPPING,

    /// <summary>Raw LCEVC payload data.</summary>
    LCEVC = _AVPacketSideDataType.AV_PKT_DATA_LCEVC,

    /// <summary>Information about reference displays and corresponding reference stereo pairs.</summary>
    ReferenceDisplays3D = _AVPacketSideDataType.AV_PKT_DATA_3D_REFERENCE_DISPLAYS,

    /// <summary>The last received RTCP Sender Report information.</summary>
    RTCPSenderReport = _AVPacketSideDataType.AV_PKT_DATA_RTCP_SR,

    /// <summary>EXIF metadata stored in the format defined by the Exif specification.</summary>
    EXIF = _AVPacketSideDataType.AV_PKT_DATA_EXIF,

    /// <summary>
    /// Not part of the public ABI
    /// </summary>
    __COUNT__ = _AVPacketSideDataType.AV_PKT_DATA_NB,
}
