namespace FFmpeg.Codecs;
/// <summary>
/// Identify the syntax and semantics of the bitstream. The principle is roughly:
/// Two decoders with the same ID can decode the same streams. Two encoders with the
/// same ID can encode compatible streams. There may be slight deviations from the
/// principle due to implementation details.
/// </summary>
public enum CodecID : int
{
    /// <summary>No codec specified.</summary>
    None = AutoGen._AVCodecID.AV_CODEC_ID_NONE,

    /// <summary>MPEG-1 video codec.</summary>
    MPEG1Video = AutoGen._AVCodecID.AV_CODEC_ID_MPEG1VIDEO,

    /// <summary>Preferred ID for MPEG-1/2 video decoding.</summary>
    MPEG2Video = AutoGen._AVCodecID.AV_CODEC_ID_MPEG2VIDEO,

    /// <summary>H.261 video codec.</summary>
    H261 = AutoGen._AVCodecID.AV_CODEC_ID_H261,

    /// <summary>H.263 video codec.</summary>
    H263 = AutoGen._AVCodecID.AV_CODEC_ID_H263,

    /// <summary>RealVideo 1.0 codec.</summary>
    RV10 = AutoGen._AVCodecID.AV_CODEC_ID_RV10,

    /// <summary>RealVideo 2.0 codec.</summary>
    RV20 = AutoGen._AVCodecID.AV_CODEC_ID_RV20,

    /// <summary>Motion JPEG codec.</summary>
    MJPEG = AutoGen._AVCodecID.AV_CODEC_ID_MJPEG,

    /// <summary>Motion JPEG B codec.</summary>
    MJPEGB = AutoGen._AVCodecID.AV_CODEC_ID_MJPEGB,

    /// <summary>Lossless JPEG codec.</summary>
    LJPEG = AutoGen._AVCodecID.AV_CODEC_ID_LJPEG,

    /// <summary>SP5X video codec.</summary>
    SP5X = AutoGen._AVCodecID.AV_CODEC_ID_SP5X,

    /// <summary>JPEG-LS codec.</summary>
    JPEGLS = AutoGen._AVCodecID.AV_CODEC_ID_JPEGLS,

    /// <summary>MPEG-4 part 2 video codec.</summary>
    MPEG4 = AutoGen._AVCodecID.AV_CODEC_ID_MPEG4,

    /// <summary>Raw video codec.</summary>
    RawVideo = AutoGen._AVCodecID.AV_CODEC_ID_RAWVIDEO,

    /// <summary>Microsoft MPEG-4 version 1 codec.</summary>
    MSMPEG4V1 = AutoGen._AVCodecID.AV_CODEC_ID_MSMPEG4V1,

    /// <summary>Microsoft MPEG-4 version 2 codec.</summary>
    MSMPEG4V2 = AutoGen._AVCodecID.AV_CODEC_ID_MSMPEG4V2,

    /// <summary>Microsoft MPEG-4 version 3 codec.</summary>
    MSMPEG4V3 = AutoGen._AVCodecID.AV_CODEC_ID_MSMPEG4V3,

    /// <summary>Windows Media Video 7 codec.</summary>
    WMV1 = AutoGen._AVCodecID.AV_CODEC_ID_WMV1,

    /// <summary>Windows Media Video 8 codec.</summary>
    WMV2 = AutoGen._AVCodecID.AV_CODEC_ID_WMV2,

    /// <summary>H.263+ video codec.</summary>
    H263P = AutoGen._AVCodecID.AV_CODEC_ID_H263P,

    /// <summary>H.263i video codec.</summary>
    H263I = AutoGen._AVCodecID.AV_CODEC_ID_H263I,

    /// <summary>Flash Video (FLV) codec.</summary>
    FLV1 = AutoGen._AVCodecID.AV_CODEC_ID_FLV1,

    /// <summary>Sorenson Vector Quantizer 1 codec.</summary>
    SVQ1 = AutoGen._AVCodecID.AV_CODEC_ID_SVQ1,

    /// <summary>Sorenson Vector Quantizer 3 codec.</summary>
    SVQ3 = AutoGen._AVCodecID.AV_CODEC_ID_SVQ3,

    /// <summary>DV video codec.</summary>
    DVVideo = AutoGen._AVCodecID.AV_CODEC_ID_DVVIDEO,

    /// <summary>HuffYUV video codec.</summary>
    HuffYUV = AutoGen._AVCodecID.AV_CODEC_ID_HUFFYUV,

    /// <summary>Creative YUV codec.</summary>
    CYUV = AutoGen._AVCodecID.AV_CODEC_ID_CYUV,

    /// <summary>H.264 / AVC video codec.</summary>
    H264 = AutoGen._AVCodecID.AV_CODEC_ID_H264,

    /// <summary>Intel Indeo 3 codec.</summary>
    Indeo3 = AutoGen._AVCodecID.AV_CODEC_ID_INDEO3,

    /// <summary>VP3 video codec.</summary>
    VP3 = AutoGen._AVCodecID.AV_CODEC_ID_VP3,

    /// <summary>Theora video codec.</summary>
    Theora = AutoGen._AVCodecID.AV_CODEC_ID_THEORA,

    /// <summary>ASUS Video codec version 1.</summary>
    ASV1 = AutoGen._AVCodecID.AV_CODEC_ID_ASV1,

    /// <summary>ASUS Video codec version 2.</summary>
    ASV2 = AutoGen._AVCodecID.AV_CODEC_ID_ASV2,

    /// <summary>FFmpeg's experimental lossless codec (FFV1).</summary>
    FFV1 = AutoGen._AVCodecID.AV_CODEC_ID_FFV1,

    /// <summary>4X Movie codec.</summary>
    FourXM = AutoGen._AVCodecID.AV_CODEC_ID_4XM,

    /// <summary>ATI VCR1 codec.</summary>
    VCR1 = AutoGen._AVCodecID.AV_CODEC_ID_VCR1,

    /// <summary>Cirrus Logic CLJR codec.</summary>
    CLJR = AutoGen._AVCodecID.AV_CODEC_ID_CLJR,

    /// <summary>Sony PlayStation MDEC codec.</summary>
    MDEC = AutoGen._AVCodecID.AV_CODEC_ID_MDEC,

    /// <summary>RoQ video codec.</summary>
    RoQ = AutoGen._AVCodecID.AV_CODEC_ID_ROQ,

    /// <summary>Interplay video codec.</summary>
    InterplayVideo = AutoGen._AVCodecID.AV_CODEC_ID_INTERPLAY_VIDEO,

    /// <summary>Xan WC3 video codec.</summary>
    XanWC3 = AutoGen._AVCodecID.AV_CODEC_ID_XAN_WC3,

    /// <summary>Xan WC4 video codec.</summary>
    XanWC4 = AutoGen._AVCodecID.AV_CODEC_ID_XAN_WC4,

    /// <summary>Apple RPZA video codec (QuickTime).</summary>
    RPZA = AutoGen._AVCodecID.AV_CODEC_ID_RPZA,

    /// <summary>Cinepak video codec.</summary>
    Cinepak = AutoGen._AVCodecID.AV_CODEC_ID_CINEPAK,

    /// <summary>Westwood Studios VQA video codec.</summary>
    WestwoodVQA = AutoGen._AVCodecID.AV_CODEC_ID_WS_VQA,

    /// <summary>Microsoft Run-Length Encoding (RLE) codec.</summary>
    MSRLE = AutoGen._AVCodecID.AV_CODEC_ID_MSRLE,

    /// <summary>Microsoft Video 1 codec.</summary>
    MSVideo1 = AutoGen._AVCodecID.AV_CODEC_ID_MSVIDEO1,

    /// <summary>ID Cinematic codec.</summary>
    IDCIN = AutoGen._AVCodecID.AV_CODEC_ID_IDCIN,

    /// <summary>8-Bit Planar codec.</summary>
    EightBPS = AutoGen._AVCodecID.AV_CODEC_ID_8BPS,

    /// <summary>Apple Graphics codec (SMC).</summary>
    SMC = AutoGen._AVCodecID.AV_CODEC_ID_SMC,

    /// <summary>Autodesk FLIC animation codec.</summary>
    FLIC = AutoGen._AVCodecID.AV_CODEC_ID_FLIC,

    /// <summary>Duck TrueMotion 1 codec.</summary>
    TrueMotion1 = AutoGen._AVCodecID.AV_CODEC_ID_TRUEMOTION1,

    /// <summary>VMD video codec (used in Sierra VMD format).</summary>
    VMDVideo = AutoGen._AVCodecID.AV_CODEC_ID_VMDVIDEO,

    /// <summary>AverMedia MSZH codec (Lossless video compression).</summary>
    MSZH = AutoGen._AVCodecID.AV_CODEC_ID_MSZH,

    /// <summary>Zlib compression codec.</summary>
    Zlib = AutoGen._AVCodecID.AV_CODEC_ID_ZLIB,

    /// <summary>QuickTime Animation codec (QTRLE).</summary>
    QTRLE = AutoGen._AVCodecID.AV_CODEC_ID_QTRLE,

    /// <summary>TechSmith Screen Capture Codec (TSCC).</summary>
    TSCC = AutoGen._AVCodecID.AV_CODEC_ID_TSCC,

    /// <summary>IBM Ultimotion codec.</summary>
    Ultimotion = AutoGen._AVCodecID.AV_CODEC_ID_ULTI,

    /// <summary>Apple QuickDraw codec.</summary>
    QDraw = AutoGen._AVCodecID.AV_CODEC_ID_QDRAW,

    /// <summary>Miro Video XL codec.</summary>
    VIXL = AutoGen._AVCodecID.AV_CODEC_ID_VIXL,

    /// <summary>QPEG video codec.</summary>
    QPEG = AutoGen._AVCodecID.AV_CODEC_ID_QPEG,

    /// <summary>Portable Network Graphics (PNG) codec.</summary>
    PNG = AutoGen._AVCodecID.AV_CODEC_ID_PNG,

    /// <summary>Portable Pixmap format (PPM).</summary>
    PPM = AutoGen._AVCodecID.AV_CODEC_ID_PPM,

    /// <summary>Portable Bitmap format (PBM).</summary>
    PBM = AutoGen._AVCodecID.AV_CODEC_ID_PBM,

    /// <summary>Portable Graymap format (PGM).</summary>
    PGM = AutoGen._AVCodecID.AV_CODEC_ID_PGM,

    /// <summary>Portable Graymap YUV format (PGMYUV).</summary>
    PGMYUV = AutoGen._AVCodecID.AV_CODEC_ID_PGMYUV,

    /// <summary>Portable Arbitrary Map format (PAM).</summary>
    PAM = AutoGen._AVCodecID.AV_CODEC_ID_PAM,

    /// <summary>FFmpeg's Huffman codec.</summary>
    FFVHuff = AutoGen._AVCodecID.AV_CODEC_ID_FFVHUFF,

    /// <summary>RealVideo 3.0 codec.</summary>
    RV30 = AutoGen._AVCodecID.AV_CODEC_ID_RV30,

    /// <summary>RealVideo 4.0 codec.</summary>
    RV40 = AutoGen._AVCodecID.AV_CODEC_ID_RV40,

    /// <summary>VC-1 video codec.</summary>
    VC1 = AutoGen._AVCodecID.AV_CODEC_ID_VC1,

    /// <summary>Windows Media Video 9 codec.</summary>
    WMV3 = AutoGen._AVCodecID.AV_CODEC_ID_WMV3,

    /// <summary>LOCO video codec (Lossless Compression).</summary>
    LOCO = AutoGen._AVCodecID.AV_CODEC_ID_LOCO,

    /// <summary>Windows Media Video 1 codec.</summary>
    WNV1 = AutoGen._AVCodecID.AV_CODEC_ID_WNV1,

    /// <summary>Advanced Audio Coding codec.</summary>
    AASC = AutoGen._AVCodecID.AV_CODEC_ID_AASC,

    /// <summary>Intel Indeo 2 codec.</summary>
    Indeo2 = AutoGen._AVCodecID.AV_CODEC_ID_INDEO2,

    /// <summary>Fraps video codec.</summary>
    FRAPS = AutoGen._AVCodecID.AV_CODEC_ID_FRAPS,

    /// <summary>Duck TrueMotion 2 codec.</summary>
    TrueMotion2 = AutoGen._AVCodecID.AV_CODEC_ID_TRUEMOTION2,

    /// <summary>Bitmap image format codec.</summary>
    BMP = AutoGen._AVCodecID.AV_CODEC_ID_BMP,

    /// <summary>CSCD video codec (Creative Labs).</summary>
    CSCD = AutoGen._AVCodecID.AV_CODEC_ID_CSCD,

    /// <summary>MM Video codec (Multimedia Video).</summary>
    MMVideo = AutoGen._AVCodecID.AV_CODEC_ID_MMVIDEO,

    /// <summary>ZMBV video codec (Zip Motion Block View).</summary>
    ZMBV = AutoGen._AVCodecID.AV_CODEC_ID_ZMBV,

    /// <summary>AVS video codec (Advanced Video Coding).</summary>
    AVS = AutoGen._AVCodecID.AV_CODEC_ID_AVS,

    /// <summary>Smack Video codec (used in Smacker format).</summary>
    SmackVideo = AutoGen._AVCodecID.AV_CODEC_ID_SMACKVIDEO,

    /// <summary>NUV video codec (NuppelVideo format).</summary>
    NUV = AutoGen._AVCodecID.AV_CODEC_ID_NUV,

    /// <summary>KMV codec (used in KMV format).</summary>
    KMVC = AutoGen._AVCodecID.AV_CODEC_ID_KMVC,

    /// <summary>Flash SV video codec.</summary>
    FlashSV = AutoGen._AVCodecID.AV_CODEC_ID_FLASHSV,

    /// <summary>CAVS codec (Chinese AVS standard).</summary>
    CAVS = AutoGen._AVCodecID.AV_CODEC_ID_CAVS,

    /// <summary>JPEG 2000 video codec.</summary>
    JPEG2000 = AutoGen._AVCodecID.AV_CODEC_ID_JPEG2000,

    /// <summary>VMNC codec (VMware Video Codec).</summary>
    VMNC = AutoGen._AVCodecID.AV_CODEC_ID_VMNC,

    /// <summary>VP5 video codec (VP5/6 format).</summary>
    VP5 = AutoGen._AVCodecID.AV_CODEC_ID_VP5,

    /// <summary>VP6 video codec (VP6 format).</summary>
    VP6 = AutoGen._AVCodecID.AV_CODEC_ID_VP6,

    /// <summary>VP6F video codec (Flash video variant of VP6).</summary>
    VP6F = AutoGen._AVCodecID.AV_CODEC_ID_VP6F,

    /// <summary>TARGA image format codec.</summary>
    TARGA = AutoGen._AVCodecID.AV_CODEC_ID_TARGA,

    /// <summary>DSICIN Video codec (used in DSICIN format).</summary>
    DSICINVideo = AutoGen._AVCodecID.AV_CODEC_ID_DSICINVIDEO,

    /// <summary>Tiertex Sequence Video codec.</summary>
    TiertexSeqVideo = AutoGen._AVCodecID.AV_CODEC_ID_TIERTEXSEQVIDEO,

    /// <summary>Tagged Image File Format (TIFF) codec.</summary>
    TIFF = AutoGen._AVCodecID.AV_CODEC_ID_TIFF,

    /// <summary>Graphics Interchange Format (GIF) codec.</summary>
    GIF = AutoGen._AVCodecID.AV_CODEC_ID_GIF,

    /// <summary>DXA video codec (used in DXA format).</summary>
    DXA = AutoGen._AVCodecID.AV_CODEC_ID_DXA,

    /// <summary>DNxHD codec (Avid DNxHD format).</summary>
    DNxHD = AutoGen._AVCodecID.AV_CODEC_ID_DNXHD,

    /// <summary>THP video codec (used in THP format).</summary>
    THP = AutoGen._AVCodecID.AV_CODEC_ID_THP,

    /// <summary>SGI image format codec.</summary>
    SGI = AutoGen._AVCodecID.AV_CODEC_ID_SGI,

    /// <summary>C93 video codec (used in C93 format).</summary>
    C93 = AutoGen._AVCodecID.AV_CODEC_ID_C93,

    /// <summary>Bethesda Softworks video codec.</summary>
    BethsoftVid = AutoGen._AVCodecID.AV_CODEC_ID_BETHSOFTVID,

    /// <summary>PTX video codec (used in PTX format).</summary>
    PTX = AutoGen._AVCodecID.AV_CODEC_ID_PTX,

    /// <summary>TXD video codec (used in TXD format).</summary>
    TXD = AutoGen._AVCodecID.AV_CODEC_ID_TXD,

    /// <summary>VP6A video codec (variant of VP6).</summary>
    VP6A = AutoGen._AVCodecID.AV_CODEC_ID_VP6A,

    /// <summary>AMV video codec (Anime Music Video format).</summary>
    AMV = AutoGen._AVCodecID.AV_CODEC_ID_AMV,

    /// <summary>Video codec used in VB format.</summary>
    VB = AutoGen._AVCodecID.AV_CODEC_ID_VB,

    /// <summary>PCX image format codec.</summary>
    PCX = AutoGen._AVCodecID.AV_CODEC_ID_PCX,

    /// <summary>Sun Raster image format codec.</summary>
    SunRast = AutoGen._AVCodecID.AV_CODEC_ID_SUNRAST,

    /// <summary>Intel Indeo 4 codec.</summary>
    Indeo4 = AutoGen._AVCodecID.AV_CODEC_ID_INDEO4,

    /// <summary>Intel Indeo 5 codec.</summary>
    Indeo5 = AutoGen._AVCodecID.AV_CODEC_ID_INDEO5,

    /// <summary>Mimic video codec.</summary>
    Mimic = AutoGen._AVCodecID.AV_CODEC_ID_MIMIC,

    /// <summary>RL2 video codec (used in RL2 format).</summary>
    RL2 = AutoGen._AVCodecID.AV_CODEC_ID_RL2,

    /// <summary>Escape 124 video codec.</summary>
    Escape124 = AutoGen._AVCodecID.AV_CODEC_ID_ESCAPE124,

    /// <summary>Dirac video codec (Dirac format).</summary>
    Dirac = AutoGen._AVCodecID.AV_CODEC_ID_DIRAC,

    /// <summary>BFI video codec (used in BFI format).</summary>
    BFI = AutoGen._AVCodecID.AV_CODEC_ID_BFI,

    /// <summary>CMV video codec (used in CMV format).</summary>
    CMV = AutoGen._AVCodecID.AV_CODEC_ID_CMV,

    /// <summary>Motion Pixels video codec.</summary>
    MotionPixels = AutoGen._AVCodecID.AV_CODEC_ID_MOTIONPIXELS,

    /// <summary>TGV video codec (used in TGV format).</summary>
    TGV = AutoGen._AVCodecID.AV_CODEC_ID_TGV,

    /// <summary>TQ Video codec (used in TQ format).</summary>
    TGQ = AutoGen._AVCodecID.AV_CODEC_ID_TGQ,

    /// <summary>TQI Video codec (used in TQI format).</summary>
    TQI = AutoGen._AVCodecID.AV_CODEC_ID_TQI,

    /// <summary>Aura audio codec (used in Aura format).</summary>
    Aura = AutoGen._AVCodecID.AV_CODEC_ID_AURA,

    /// <summary>Aura 2 audio codec (improved version of Aura).</summary>
    Aura2 = AutoGen._AVCodecID.AV_CODEC_ID_AURA2,

    /// <summary>V210X video codec (a variant of V210 format).</summary>
    V210X = AutoGen._AVCodecID.AV_CODEC_ID_V210X,

    /// <summary>TMV Video codec (used in TMV format).</summary>
    TMV = AutoGen._AVCodecID.AV_CODEC_ID_TMV,

    /// <summary>V210 video codec (used in V210 format).</summary>
    V210 = AutoGen._AVCodecID.AV_CODEC_ID_V210,

    /// <summary>DPX image format codec (Digital Picture Exchange).</summary>
    DPX = AutoGen._AVCodecID.AV_CODEC_ID_DPX,

    /// <summary>MAD video codec (used in MAD format).</summary>
    MAD = AutoGen._AVCodecID.AV_CODEC_ID_MAD,

    /// <summary>FRWU video codec (used in FRWU format).</summary>
    FRWU = AutoGen._AVCodecID.AV_CODEC_ID_FRWU,

    /// <summary>Flash SV2 video codec (second version of Flash SV).</summary>
    FlashSV2 = AutoGen._AVCodecID.AV_CODEC_ID_FLASHSV2,

    /// <summary>CD Graphics video codec (used in CD Graphics format).</summary>
    CDGraphics = AutoGen._AVCodecID.AV_CODEC_ID_CDGRAPHICS,

    /// <summary>R210 video codec (used in R210 format).</summary>
    R210 = AutoGen._AVCodecID.AV_CODEC_ID_R210,

    /// <summary>ANM video codec (used in ANM format).</summary>
    ANM = AutoGen._AVCodecID.AV_CODEC_ID_ANM,

    /// <summary>Bink Video codec (used in Bink format).</summary>
    BinkVideo = AutoGen._AVCodecID.AV_CODEC_ID_BINKVIDEO,

    /// <summary>IFF ILBM image format codec (used in IFF ILBM format).</summary>
    IFFILBM = AutoGen._AVCodecID.AV_CODEC_ID_IFF_ILBM,

    /// <summary>KGV1 video codec (used in KGV1 format).</summary>
    KGV1 = AutoGen._AVCodecID.AV_CODEC_ID_KGV1,

    /// <summary>YOP video codec (used in YOP format).</summary>
    YOP = AutoGen._AVCodecID.AV_CODEC_ID_YOP,

    /// <summary>VP8 video codec (used in VP8 format).</summary>
    VP8 = AutoGen._AVCodecID.AV_CODEC_ID_VP8,

    /// <summary>PICTOR video codec (used in PICTOR format).</summary>
    PICTOR = AutoGen._AVCodecID.AV_CODEC_ID_PICTOR,

    /// <summary>ANSI video codec (used in ANSI format).</summary>
    ANSI = AutoGen._AVCodecID.AV_CODEC_ID_ANSI,

    /// <summary>A64 Multi video codec (used in A64 Multi format).</summary>
    A64Multi = AutoGen._AVCodecID.AV_CODEC_ID_A64_MULTI,

    /// <summary>A64 Multi5 video codec (used in A64 Multi5 format).</summary>
    A64Multi5 = AutoGen._AVCodecID.AV_CODEC_ID_A64_MULTI5,

    /// <summary>R10K video codec (used in R10K format).</summary>
    R10K = AutoGen._AVCodecID.AV_CODEC_ID_R10K,

    /// <summary>MXPEG video codec (used in MXPEG format).</summary>
    MXPEG = AutoGen._AVCodecID.AV_CODEC_ID_MXPEG,

    /// <summary>Lagarith video codec (lossless video compression).</summary>
    Lagarith = AutoGen._AVCodecID.AV_CODEC_ID_LAGARITH,

    /// <summary>Apple ProRes video codec.</summary>
    ProRes = AutoGen._AVCodecID.AV_CODEC_ID_PRORES,

    /// <summary>JV video codec (used in JV format).</summary>
    JV = AutoGen._AVCodecID.AV_CODEC_ID_JV,

    /// <summary>DFA video codec (used in DFA format).</summary>
    DFA = AutoGen._AVCodecID.AV_CODEC_ID_DFA,

    /// <summary>WMV3 Image codec (used for WMV3 still images).</summary>
    WMV3Image = AutoGen._AVCodecID.AV_CODEC_ID_WMV3IMAGE,

    /// <summary>VC-1 Image codec (used for VC-1 still images).</summary>
    VC1Image = AutoGen._AVCodecID.AV_CODEC_ID_VC1IMAGE,

    /// <summary>UT Video codec (used in UT Video format).</summary>
    UTVideo = AutoGen._AVCodecID.AV_CODEC_ID_UTVIDEO,

    /// <summary>BMV Video codec (used in BMV format).</summary>
    BMVVideo = AutoGen._AVCodecID.AV_CODEC_ID_BMV_VIDEO,

    /// <summary>VBLE video codec (used in VBLE format).</summary>
    VBLE = AutoGen._AVCodecID.AV_CODEC_ID_VBLE,

    /// <summary>DXTory video codec (used in DXTory format).</summary>
    DXTory = AutoGen._AVCodecID.AV_CODEC_ID_DXTORY,

    /// <summary>V410 video codec (used in V410 format).</summary>
    V410 = AutoGen._AVCodecID.AV_CODEC_ID_V410,

    /// <summary>XWD image format codec (used in XWD format).</summary>
    XWD = AutoGen._AVCodecID.AV_CODEC_ID_XWD,

    /// <summary>CDXL video codec (used in CDXL format).</summary>
    CDXL = AutoGen._AVCodecID.AV_CODEC_ID_CDXL,

    /// <summary>XBM image format codec (used in XBM format).</summary>
    XBM = AutoGen._AVCodecID.AV_CODEC_ID_XBM,

    /// <summary>ZeroCodec video codec (used in ZeroCodec format).</summary>
    ZeroCodec = AutoGen._AVCodecID.AV_CODEC_ID_ZEROCODEC,

    /// <summary>Microsoft Screen Video 1 codec.</summary>
    MSS1 = AutoGen._AVCodecID.AV_CODEC_ID_MSS1,

    /// <summary>Microsoft Screen Video 1 (A) codec.</summary>
    MSA1 = AutoGen._AVCodecID.AV_CODEC_ID_MSA1,

    /// <summary>TechSmith Screen Capture Codec 2 (TSCC2).</summary>
    TSCC2 = AutoGen._AVCodecID.AV_CODEC_ID_TSCC2,

    /// <summary>MTS2 video codec (used in MTS2 format).</summary>
    MTS2 = AutoGen._AVCodecID.AV_CODEC_ID_MTS2,

    /// <summary>CLLC video codec (used in CLLC format).</summary>
    CLLC = AutoGen._AVCodecID.AV_CODEC_ID_CLLC,

    /// <summary>Microsoft Screen Video 2 codec.</summary>
    MSS2 = AutoGen._AVCodecID.AV_CODEC_ID_MSS2,

    /// <summary>VP9 video codec (used in VP9 format).</summary>
    VP9 = AutoGen._AVCodecID.AV_CODEC_ID_VP9,

    /// <summary>Apple Intermediate Codec (AIC).</summary>
    AIC = AutoGen._AVCodecID.AV_CODEC_ID_AIC,

    /// <summary>Escape 130 video codec.</summary>
    Escape130 = AutoGen._AVCodecID.AV_CODEC_ID_ESCAPE130,

    /// <summary>G2M video codec (used in G2M format).</summary>
    G2M = AutoGen._AVCodecID.AV_CODEC_ID_G2M,

    /// <summary>WebP image format codec.</summary>
    WebP = AutoGen._AVCodecID.AV_CODEC_ID_WEBP,

    /// <summary>HNM4 video codec (used in HNM4 format).</summary>
    HNM4Video = AutoGen._AVCodecID.AV_CODEC_ID_HNM4_VIDEO,

    /// <summary>High Efficiency Video Coding (HEVC), also known as H.265.</summary>
    HEVC = AutoGen._AVCodecID.AV_CODEC_ID_HEVC,

    /// <summary>FIC video codec (used in FIC format).</summary>
    FIC = AutoGen._AVCodecID.AV_CODEC_ID_FIC,

    /// <summary>Alias Pix video codec (used in Alias Pix format).</summary>
    AliasPix = AutoGen._AVCodecID.AV_CODEC_ID_ALIAS_PIX,

    /// <summary>BRender Pix video codec (used in BRender Pix format).</summary>
    BRenderPix = AutoGen._AVCodecID.AV_CODEC_ID_BRENDER_PIX,

    /// <summary>PAF video codec (used in PAF format).</summary>
    PAFVideo = AutoGen._AVCodecID.AV_CODEC_ID_PAF_VIDEO,

    /// <summary>OpenEXR image format codec.</summary>
    EXR = AutoGen._AVCodecID.AV_CODEC_ID_EXR,

    /// <summary>VP7 video codec (used in VP7 format).</summary>
    VP7 = AutoGen._AVCodecID.AV_CODEC_ID_VP7,

    /// <summary>SANM video codec (used in SANM format).</summary>
    SANM = AutoGen._AVCodecID.AV_CODEC_ID_SANM,

    /// <summary>SGI RLE image format codec (used in SGI RLE format).</summary>
    SGIRLE = AutoGen._AVCodecID.AV_CODEC_ID_SGIRLE,

    /// <summary>MVC1 video codec (used in MVC1 format).</summary>
    MVC1 = AutoGen._AVCodecID.AV_CODEC_ID_MVC1,

    /// <summary>MVC2 video codec (used in MVC2 format).</summary>
    MVC2 = AutoGen._AVCodecID.AV_CODEC_ID_MVC2,

    /// <summary>HQX video codec (used in HQX format).</summary>
    HQX = AutoGen._AVCodecID.AV_CODEC_ID_HQX,

    /// <summary>TDSC video codec (used in TDSC format).</summary>
    TDSC = AutoGen._AVCodecID.AV_CODEC_ID_TDSC,

    /// <summary>HQ/HQA video codec (used in HQ/HQA format).</summary>
    HQ_HQA = AutoGen._AVCodecID.AV_CODEC_ID_HQ_HQA,

    /// <summary>HAP video codec (used in HAP format).</summary>
    HAP = AutoGen._AVCodecID.AV_CODEC_ID_HAP,

    /// <summary>DirectDraw Surface (DDS) image format codec.</summary>
    DDS = AutoGen._AVCodecID.AV_CODEC_ID_DDS,

    /// <summary>DXV video codec (used in DXV format).</summary>
    DXV = AutoGen._AVCodecID.AV_CODEC_ID_DXV,

    /// <summary>ScreenPresso video codec (used in ScreenPresso format).</summary>
    ScreenPresso = AutoGen._AVCodecID.AV_CODEC_ID_SCREENPRESSO,

    /// <summary>RSCC video codec (used in RSCC format).</summary>
    RSCC = AutoGen._AVCodecID.AV_CODEC_ID_RSCC,

    /// <summary>AVS2 video codec (used in AVS2 format).</summary>
    AVS2 = AutoGen._AVCodecID.AV_CODEC_ID_AVS2,

    /// <summary>PGX image format codec (used in PGX format).</summary>
    PGX = AutoGen._AVCodecID.AV_CODEC_ID_PGX,

    /// <summary>AVS3 video codec (used in AVS3 format).</summary>
    AVS3 = AutoGen._AVCodecID.AV_CODEC_ID_AVS3,

    /// <summary>MSP2 video codec (used in MSP2 format).</summary>
    MSP2 = AutoGen._AVCodecID.AV_CODEC_ID_MSP2,

    /// <summary>Versatile Video Coding (VVC), also known as H.266.</summary>
    VVC = AutoGen._AVCodecID.AV_CODEC_ID_VVC,

    /// <summary>Y41P video codec (used in Y41P format).</summary>
    Y41P = AutoGen._AVCodecID.AV_CODEC_ID_Y41P,

    /// <summary>AVRP video codec (used in AVRP format).</summary>
    AVRP = AutoGen._AVCodecID.AV_CODEC_ID_AVRP,

    /// <summary>012V video codec (used in 012V format).</summary>
    _012V = AutoGen._AVCodecID.AV_CODEC_ID_012V,

    /// <summary>AVUI video codec (used in AVUI format).</summary>
    AVUI = AutoGen._AVCodecID.AV_CODEC_ID_AVUI,

    /// <summary>TARGA Y216 video codec (used in TARGA Y216 format).</summary>
    TARGAY216 = AutoGen._AVCodecID.AV_CODEC_ID_TARGA_Y216,

    /// <summary>V308 video codec (used in V308 format).</summary>
    V308 = AutoGen._AVCodecID.AV_CODEC_ID_V308,

    /// <summary>V408 video codec (used in V408 format).</summary>
    V408 = AutoGen._AVCodecID.AV_CODEC_ID_V408,

    /// <summary>YUV4 video codec (used in YUV4 format).</summary>
    YUV4 = AutoGen._AVCodecID.AV_CODEC_ID_YUV4,

    /// <summary>AVRN video codec (used in AVRN format).</summary>
    AVRN = AutoGen._AVCodecID.AV_CODEC_ID_AVRN,

    /// <summary>CPIA video codec (used in CPIA format).</summary>
    CPIA = AutoGen._AVCodecID.AV_CODEC_ID_CPIA,

    /// <summary>XFace video codec (used in XFace format).</summary>
    XFace = AutoGen._AVCodecID.AV_CODEC_ID_XFACE,

    /// <summary>Snow video codec (used in Snow format).</summary>
    Snow = AutoGen._AVCodecID.AV_CODEC_ID_SNOW,

    /// <summary>SMVJPEG video codec (used in SMVJPEG format).</summary>
    SMVJPEG = AutoGen._AVCodecID.AV_CODEC_ID_SMVJPEG,

    /// <summary>APNG image format codec (used in APNG format).</summary>
    APNG = AutoGen._AVCodecID.AV_CODEC_ID_APNG,

    /// <summary>Daala video codec (used in Daala format).</summary>
    Daala = AutoGen._AVCodecID.AV_CODEC_ID_DAALA,

    /// <summary>CFHD video codec (used in CFHD format).</summary>
    CFHD = AutoGen._AVCodecID.AV_CODEC_ID_CFHD,

    /// <summary>TrueMotion 2RT video codec (used in TrueMotion 2RT format).</summary>
    TrueMotion2RT = AutoGen._AVCodecID.AV_CODEC_ID_TRUEMOTION2RT,

    /// <summary>M101 video codec (used in M101 format).</summary>
    M101 = AutoGen._AVCodecID.AV_CODEC_ID_M101,

    /// <summary>MagicYUV video codec (used in MagicYUV format).</summary>
    MagicYUV = AutoGen._AVCodecID.AV_CODEC_ID_MAGICYUV,

    /// <summary>SheerVideo video codec (used in SheerVideo format).</summary>
    SheerVideo = AutoGen._AVCodecID.AV_CODEC_ID_SHEERVIDEO,

    /// <summary>YLC video codec (used in YLC format).</summary>
    YLC = AutoGen._AVCodecID.AV_CODEC_ID_YLC,

    /// <summary>Photoshop (PSD) image format codec.</summary>
    PSD = AutoGen._AVCodecID.AV_CODEC_ID_PSD,

    /// <summary>Pixlet video codec (used in Pixlet format).</summary>
    Pixlet = AutoGen._AVCodecID.AV_CODEC_ID_PIXLET,

    /// <summary>SpeedHQ video codec.</summary>
    SpeedHQ = AutoGen._AVCodecID.AV_CODEC_ID_SPEEDHQ,

    /// <summary>FMVC video codec (used in FMVC format).</summary>
    FMVC = AutoGen._AVCodecID.AV_CODEC_ID_FMVC,

    /// <summary>SCPR video codec (used in SCPR format).</summary>
    SCPR = AutoGen._AVCodecID.AV_CODEC_ID_SCPR,

    /// <summary>ClearVideo codec (used in ClearVideo format).</summary>
    ClearVideo = AutoGen._AVCodecID.AV_CODEC_ID_CLEARVIDEO,

    /// <summary>XPM image format codec (used in XPM format).</summary>
    XPM = AutoGen._AVCodecID.AV_CODEC_ID_XPM,

    /// <summary>AV1 video codec (used in AV1 format).</summary>
    AV1 = AutoGen._AVCodecID.AV_CODEC_ID_AV1,

    /// <summary>Bitpacked video codec (used in Bitpacked format).</summary>
    Bitpacked = AutoGen._AVCodecID.AV_CODEC_ID_BITPACKED,

    /// <summary>MSCC video codec (used in MSCC format).</summary>
    MSCC = AutoGen._AVCodecID.AV_CODEC_ID_MSCC,

    /// <summary>SRGC video codec (used in SRGC format).</summary>
    SRGC = AutoGen._AVCodecID.AV_CODEC_ID_SRGC,

    /// <summary>Scalable Vector Graphics (SVG) image format codec.</summary>
    SVG = AutoGen._AVCodecID.AV_CODEC_ID_SVG,

    /// <summary>GDV video codec (used in GDV format).</summary>
    GDV = AutoGen._AVCodecID.AV_CODEC_ID_GDV,

    /// <summary>FITS image format codec (used in FITS format).</summary>
    FITS = AutoGen._AVCodecID.AV_CODEC_ID_FITS,

    /// <summary>IMM4 video codec (used in IMM4 format).</summary>
    IMM4 = AutoGen._AVCodecID.AV_CODEC_ID_IMM4,

    /// <summary>ProSumer video codec (used in ProSumer format).</summary>
    ProSumer = AutoGen._AVCodecID.AV_CODEC_ID_PROSUMER,

    /// <summary>MWSC video codec (used in MWSC format).</summary>
    MWSC = AutoGen._AVCodecID.AV_CODEC_ID_MWSC,

    /// <summary>WCMV video codec (used in WCMV format).</summary>
    WCMV = AutoGen._AVCodecID.AV_CODEC_ID_WCMV,

    /// <summary>RASC video codec (used in RASC format).</summary>
    RASC = AutoGen._AVCodecID.AV_CODEC_ID_RASC,

    /// <summary>HYMT video codec (used in HYMT format).</summary>
    HYMT = AutoGen._AVCodecID.AV_CODEC_ID_HYMT,

    /// <summary>ARBC video codec (used in ARBC format).</summary>
    ARBC = AutoGen._AVCodecID.AV_CODEC_ID_ARBC,

    /// <summary>AGM video codec (used in AGM format).</summary>
    AGM = AutoGen._AVCodecID.AV_CODEC_ID_AGM,

    /// <summary>LSCR video codec (used in LSCR format).</summary>
    LSCR = AutoGen._AVCodecID.AV_CODEC_ID_LSCR,

    /// <summary>VP4 video codec (used in VP4 format).</summary>
    VP4 = AutoGen._AVCodecID.AV_CODEC_ID_VP4,

    /// <summary>IMM5 video codec (used in IMM5 format).</summary>
    IMM5 = AutoGen._AVCodecID.AV_CODEC_ID_IMM5,

    /// <summary>MVDV video codec (used in MVDV format).</summary>
    MVDV = AutoGen._AVCodecID.AV_CODEC_ID_MVDV,

    /// <summary>MVHA video codec (used in MVHA format).</summary>
    MVHA = AutoGen._AVCodecID.AV_CODEC_ID_MVHA,

    /// <summary>CDToons video codec (used in CDToons format).</summary>
    CDToons = AutoGen._AVCodecID.AV_CODEC_ID_CDTOONS,

    /// <summary>MV30 video codec (used in MV30 format).</summary>
    MV30 = AutoGen._AVCodecID.AV_CODEC_ID_MV30,

    /// <summary>NotchLC video codec (used in NotchLC format).</summary>
    NotchLC = AutoGen._AVCodecID.AV_CODEC_ID_NOTCHLC,

    /// <summary>PFM image format codec (used in PFM format).</summary>
    PFM = AutoGen._AVCodecID.AV_CODEC_ID_PFM,

    /// <summary>Mobiclip video codec (used in Mobiclip format).</summary>
    Mobiclip = AutoGen._AVCodecID.AV_CODEC_ID_MOBICLIP,

    /// <summary>PhotoCD image format codec (used in PhotoCD format).</summary>
    PhotoCD = AutoGen._AVCodecID.AV_CODEC_ID_PHOTOCD,

    /// <summary>IPU video codec (used in IPU format).</summary>
    IPU = AutoGen._AVCodecID.AV_CODEC_ID_IPU,

    /// <summary>Argo video codec (used in Argo format).</summary>
    Argo = AutoGen._AVCodecID.AV_CODEC_ID_ARGO,

    /// <summary>CRI video codec (used in CRI format).</summary>
    CRI = AutoGen._AVCodecID.AV_CODEC_ID_CRI,

    /// <summary>Simbiosis IMX video codec (used in Simbiosis IMX format).</summary>
    SimbiosisIMX = AutoGen._AVCodecID.AV_CODEC_ID_SIMBIOSIS_IMX,

    /// <summary>SGA video codec (used in SGA format).</summary>
    SGAVideo = AutoGen._AVCodecID.AV_CODEC_ID_SGA_VIDEO,

    /// <summary>GEM video codec (used in GEM format).</summary>
    GEM = AutoGen._AVCodecID.AV_CODEC_ID_GEM,

    /// <summary>VBN video codec (used in VBN format).</summary>
    VBN = AutoGen._AVCodecID.AV_CODEC_ID_VBN,

    /// <summary>JPEG XL video codec (used in JPEG XL format).</summary>
    JPEGXL = AutoGen._AVCodecID.AV_CODEC_ID_JPEGXL,

    /// <summary>QOI image format codec (used in QOI format).</summary>
    QOI = AutoGen._AVCodecID.AV_CODEC_ID_QOI,

    /// <summary>PHM video codec (used in PHM format).</summary>
    PHM = AutoGen._AVCodecID.AV_CODEC_ID_PHM,

    /// <summary>Radiance HDR image format codec (used in Radiance HDR format).</summary>
    RadianceHDR = AutoGen._AVCodecID.AV_CODEC_ID_RADIANCE_HDR,

    /// <summary>WBMP image format codec (used in WBMP format).</summary>
    WBMP = AutoGen._AVCodecID.AV_CODEC_ID_WBMP,

    /// <summary>Media 100 video codec (used in Media 100 format).</summary>
    Media100 = AutoGen._AVCodecID.AV_CODEC_ID_MEDIA100,

    /// <summary>VQC video codec (used in VQC format).</summary>
    VQC = AutoGen._AVCodecID.AV_CODEC_ID_VQC,

    /// <summary>PDV video codec (used in PDV format).</summary>
    PDV = AutoGen._AVCodecID.AV_CODEC_ID_PDV,

    /// <summary>EVC video codec (used in EVC format).</summary>
    EVC = AutoGen._AVCodecID.AV_CODEC_ID_EVC,

    /// <summary>RTV1 video codec (used in RTV1 format).</summary>
    RTV1 = AutoGen._AVCodecID.AV_CODEC_ID_RTV1,

    /// <summary>VMIX video codec (used in VMIX format).</summary>
    VMIX = AutoGen._AVCodecID.AV_CODEC_ID_VMIX,

    /// <summary>LEAD video codec (used in LEAD format).</summary>
    LEAD = AutoGen._AVCodecID.AV_CODEC_ID_LEAD,

    /// <summary>A dummy ID pointing at the start of audio codecs.</summary>
    FirstAudio = AutoGen._AVCodecID.AV_CODEC_ID_FIRST_AUDIO,

    /// <summary>Pulse Code Modulation (PCM) 16-bit little-endian audio format.</summary>
    PCM_S16LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S16LE,

    /// <summary>Pulse Code Modulation (PCM) 16-bit big-endian audio format.</summary>
    PCM_S16BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S16BE,

    /// <summary>Pulse Code Modulation (PCM) 16-bit unsigned little-endian audio format.</summary>
    PCM_U16LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_U16LE,

    /// <summary>Pulse Code Modulation (PCM) 16-bit unsigned big-endian audio format.</summary>
    PCM_U16BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_U16BE,

    /// <summary>Pulse Code Modulation (PCM) 8-bit signed audio format.</summary>
    PCM_S8 = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S8,

    /// <summary>Pulse Code Modulation (PCM) 8-bit unsigned audio format.</summary>
    PCM_U8 = AutoGen._AVCodecID.AV_CODEC_ID_PCM_U8,

    /// <summary>Pulse Code Modulation (PCM) μ-law audio format.</summary>
    PCM_MULAW = AutoGen._AVCodecID.AV_CODEC_ID_PCM_MULAW,

    /// <summary>Pulse Code Modulation (PCM) A-law audio format.</summary>
    PCM_ALAW = AutoGen._AVCodecID.AV_CODEC_ID_PCM_ALAW,

    /// <summary>Pulse Code Modulation (PCM) 32-bit little-endian audio format.</summary>
    PCM_S32LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S32LE,

    /// <summary>Pulse Code Modulation (PCM) 32-bit big-endian audio format.</summary>
    PCM_S32BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S32BE,

    /// <summary>Pulse Code Modulation (PCM) 32-bit unsigned little-endian audio format.</summary>
    PCM_U32LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_U32LE,

    /// <summary>Pulse Code Modulation (PCM) 32-bit unsigned big-endian audio format.</summary>
    PCM_U32BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_U32BE,

    /// <summary>Pulse Code Modulation (PCM) 24-bit little-endian audio format.</summary>
    PCM_S24LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S24LE,

    /// <summary>Pulse Code Modulation (PCM) 24-bit big-endian audio format.</summary>
    PCM_S24BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S24BE,

    /// <summary>Pulse Code Modulation (PCM) 24-bit unsigned little-endian audio format.</summary>
    PCM_U24LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_U24LE,

    /// <summary>Pulse Code Modulation (PCM) 24-bit unsigned big-endian audio format.</summary>
    PCM_U24BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_U24BE,

    /// <summary>Pulse Code Modulation (PCM) 24-bit DAUD audio format.</summary>
    PCM_S24DAUD = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S24DAUD,

    /// <summary>Pulse Code Modulation (PCM) Zork audio format.</summary>
    PCM_ZORK = AutoGen._AVCodecID.AV_CODEC_ID_PCM_ZORK,

    /// <summary>Pulse Code Modulation (PCM) 16-bit little-endian planar audio format.</summary>
    PCM_S16LE_PLANAR = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S16LE_PLANAR,

    /// <summary>Pulse Code Modulation (PCM) DVD audio format.</summary>
    PCM_DVD = AutoGen._AVCodecID.AV_CODEC_ID_PCM_DVD,

    /// <summary>Pulse Code Modulation (PCM) 32-bit big-endian floating-point audio format.</summary>
    PCM_F32BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_F32BE,

    /// <summary>Pulse Code Modulation (PCM) 32-bit little-endian floating-point audio format.</summary>
    PCM_F32LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_F32LE,

    /// <summary>Pulse Code Modulation (PCM) 64-bit big-endian floating-point audio format.</summary>
    PCM_F64BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_F64BE,

    /// <summary>Pulse Code Modulation (PCM) 64-bit little-endian floating-point audio format.</summary>
    PCM_F64LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_F64LE,

    /// <summary>Pulse Code Modulation (PCM) Blu-ray audio format.</summary>
    PCM_BLURAY = AutoGen._AVCodecID.AV_CODEC_ID_PCM_BLURAY,

    /// <summary>Pulse Code Modulation (PCM) LXF audio format.</summary>
    PCM_LXF = AutoGen._AVCodecID.AV_CODEC_ID_PCM_LXF,

    /// <summary>S302M audio format (used in certain professional audio equipment).</summary>
    S302M = AutoGen._AVCodecID.AV_CODEC_ID_S302M,

    /// <summary>Pulse Code Modulation (PCM) 8-bit signed planar audio format.</summary>
    PCM_S8_PLANAR = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S8_PLANAR,

    /// <summary>Pulse Code Modulation (PCM) 24-bit little-endian planar audio format.</summary>
    PCM_S24LE_PLANAR = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S24LE_PLANAR,

    /// <summary>Pulse Code Modulation (PCM) 32-bit little-endian planar audio format.</summary>
    PCM_S32LE_PLANAR = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S32LE_PLANAR,

    /// <summary>Pulse Code Modulation (PCM) 16-bit big-endian planar audio format.</summary>
    PCM_S16BE_PLANAR = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S16BE_PLANAR,

    /// <summary>Pulse Code Modulation (PCM) 64-bit little-endian audio format.</summary>
    PCM_S64LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S64LE,

    /// <summary>Pulse Code Modulation (PCM) 64-bit big-endian audio format.</summary>
    PCM_S64BE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_S64BE,

    /// <summary>Pulse Code Modulation (PCM) 16-bit little-endian floating-point audio format.</summary>
    PCM_F16LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_F16LE,

    /// <summary>Pulse Code Modulation (PCM) 24-bit little-endian floating-point audio format.</summary>
    PCM_F24LE = AutoGen._AVCodecID.AV_CODEC_ID_PCM_F24LE,

    /// <summary>Pulse Code Modulation (PCM) video codec used in VIDC format.</summary>
    PCM_VIDC = AutoGen._AVCodecID.AV_CODEC_ID_PCM_VIDC,

    /// <summary>Pulse Code Modulation (PCM) SGA audio format.</summary>
    PCM_SGA = AutoGen._AVCodecID.AV_CODEC_ID_PCM_SGA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA QuickTime format.</summary>
    ADPCM_IMA_QT = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_QT,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA WAV format.</summary>
    ADPCM_IMA_WAV = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_WAV,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA DK3 format.</summary>
    ADPCM_IMA_DK3 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_DK3,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA DK4 format.</summary>
    ADPCM_IMA_DK4 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_DK4,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA WS format.</summary>
    ADPCM_IMA_WS = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_WS,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA SMJPEG format.</summary>
    ADPCM_IMA_SMJPEG = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_SMJPEG,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) MS format.</summary>
    ADPCM_MS = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_MS,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) 4XM format.</summary>
    ADPCM_4XM = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_4XM,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) XA format.</summary>
    ADPCM_XA = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_XA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) ADX format.</summary>
    ADPCM_ADX = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_ADX,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) EA format.</summary>
    ADPCM_EA = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_EA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) G726 format.</summary>
    ADPCM_G726 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_G726,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) CT format.</summary>
    ADPCM_CT = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_CT,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) SWF format.</summary>
    ADPCM_SWF = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_SWF,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) Yamaha format.</summary>
    ADPCM_YAMAHA = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_YAMAHA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) SBPRO 4 format.</summary>
    ADPCM_SBPRO_4 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_SBPRO_4,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) SBPRO 3 format.</summary>
    ADPCM_SBPRO_3 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_SBPRO_3,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) SBPRO 2 format.</summary>
    ADPCM_SBPRO_2 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_SBPRO_2,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) THP format.</summary>
    ADPCM_THP = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_THP,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA AMV format.</summary>
    ADPCM_IMA_AMV = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_AMV,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) EA R1 format.</summary>
    ADPCM_EA_R1 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_EA_R1,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) EA R3 format.</summary>
    ADPCM_EA_R3 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_EA_R3,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) EA R2 format.</summary>
    ADPCM_EA_R2 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_EA_R2,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA EA SEAD format.</summary>
    ADPCM_IMA_EA_SEAD = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_EA_SEAD,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA EA EACS format.</summary>
    ADPCM_IMA_EA_EACS = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_EA_EACS,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) EA XAS format.</summary>
    ADPCM_EA_XAS = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_EA_XAS,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) EA Maxis XA format.</summary>
    ADPCM_EA_MAXIS_XA = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_EA_MAXIS_XA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA ISS format.</summary>
    ADPCM_IMA_ISS = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_ISS,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) G722 format.</summary>
    ADPCM_G722 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_G722,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA APC format.</summary>
    ADPCM_IMA_APC = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_APC,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) VIMA format.</summary>
    ADPCM_VIMA = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_VIMA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) AFC format.</summary>
    ADPCM_AFC = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_AFC,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA OKI format.</summary>
    ADPCM_IMA_OKI = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_OKI,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) DTK format.</summary>
    ADPCM_DTK = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_DTK,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA RAD format.</summary>
    ADPCM_IMA_RAD = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_RAD,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) G726 little-endian format.</summary>
    ADPCM_G726LE = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_G726LE,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) THP little-endian format.</summary>
    ADPCM_THP_LE = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_THP_LE,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) PSX format.</summary>
    ADPCM_PSX = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_PSX,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) AICA format.</summary>
    ADPCM_AICA = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_AICA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA DAT4 format.</summary>
    ADPCM_IMA_DAT4 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_DAT4,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) MTAF format.</summary>
    ADPCM_MTAF = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_MTAF,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) AGM format.</summary>
    ADPCM_AGM = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_AGM,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) ARGO format.</summary>
    ADPCM_ARGO = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_ARGO,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA SSI format.</summary>
    ADPCM_IMA_SSI = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_SSI,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) ZORK format.</summary>
    ADPCM_ZORK = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_ZORK,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA APM format.</summary>
    ADPCM_IMA_APM = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_APM,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA ALP format.</summary>
    ADPCM_IMA_ALP = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_ALP,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA MTF format.</summary>
    ADPCM_IMA_MTF = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_MTF,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA CUNNING format.</summary>
    ADPCM_IMA_CUNNING = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_CUNNING,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA MOFLEX format.</summary>
    ADPCM_IMA_MOFLEX = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_MOFLEX,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA ACORN format.</summary>
    ADPCM_IMA_ACORN = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_ACORN,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) XMD format.</summary>
    ADPCM_XMD = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_XMD,

    /// <summary>Adaptive Multi-Rate Narrowband (AMR-NB) audio format.</summary>
    AMR_NB = AutoGen._AVCodecID.AV_CODEC_ID_AMR_NB,

    /// <summary>Adaptive Multi-Rate Wideband (AMR-WB) audio format.</summary>
    AMR_WB = AutoGen._AVCodecID.AV_CODEC_ID_AMR_WB,

    /// <summary>RealAudio 1.0 (RA-144) format.</summary>
    RA_144 = AutoGen._AVCodecID.AV_CODEC_ID_RA_144,

    /// <summary>RealAudio 2.0 (RA-288) format.</summary>
    RA_288 = AutoGen._AVCodecID.AV_CODEC_ID_RA_288,

    /// <summary>Razorback (ROQ) DPCM audio format.</summary>
    ROQ_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_ROQ_DPCM,

    /// <summary>Interplay DPCM audio format.</summary>
    INTERPLAY_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_INTERPLAY_DPCM,

    /// <summary>Xan DPCM audio format.</summary>
    XAN_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_XAN_DPCM,

    /// <summary>Sol DPCM audio format.</summary>
    SOL_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_SOL_DPCM,

    /// <summary>SDX2 DPCM audio format.</summary>
    SDX2_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_SDX2_DPCM,

    /// <summary>Gremlin DPCM audio format.</summary>
    GREMLIN_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_GREMLIN_DPCM,

    /// <summary>Derrf DPCM audio format.</summary>
    DERF_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_DERF_DPCM,

    /// <summary>WADY DPCM audio format.</summary>
    WADY_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_WADY_DPCM,

    /// <summary>CBD2 DPCM audio format.</summary>
    CBD2_DPCM = AutoGen._AVCodecID.AV_CODEC_ID_CBD2_DPCM,

    /// <summary>MPEG-1/2 Audio Layer II format.</summary>
    MP2 = AutoGen._AVCodecID.AV_CODEC_ID_MP2,

    /// <summary>Preferred ID for decoding MPEG Audio Layer 1, 2, or 3 formats.</summary>
    MP3 = AutoGen._AVCodecID.AV_CODEC_ID_MP3,

    /// <summary>Advanced Audio Coding (AAC) format.</summary>
    AAC = AutoGen._AVCodecID.AV_CODEC_ID_AAC,

    /// <summary>Audio Codec 3 (AC-3) format.</summary>
    AC3 = AutoGen._AVCodecID.AV_CODEC_ID_AC3,

    /// <summary>Digital Theater Systems (DTS) audio format.</summary>
    DTS = AutoGen._AVCodecID.AV_CODEC_ID_DTS,

    /// <summary>Vorbis audio format.</summary>
    VORBIS = AutoGen._AVCodecID.AV_CODEC_ID_VORBIS,

    /// <summary>DVAUDIO format.</summary>
    DVAUDIO = AutoGen._AVCodecID.AV_CODEC_ID_DVAUDIO,

    /// <summary>Windows Media Audio Version 1 (WMAV1) format.</summary>
    WMAV1 = AutoGen._AVCodecID.AV_CODEC_ID_WMAV1,

    /// <summary>Windows Media Audio Version 2 (WMAV2) format.</summary>
    WMAV2 = AutoGen._AVCodecID.AV_CODEC_ID_WMAV2,

    /// <summary>Apple MACE 3: Apple audio codec.</summary>
    MACE3 = AutoGen._AVCodecID.AV_CODEC_ID_MACE3,

    /// <summary>Apple MACE 6: Apple audio codec.</summary>
    MACE6 = AutoGen._AVCodecID.AV_CODEC_ID_MACE6,

    /// <summary>VMDAUDIO format used by the VMD system.</summary>
    VMDAUDIO = AutoGen._AVCodecID.AV_CODEC_ID_VMDAUDIO,

    /// <summary>Free Lossless Audio Codec</summary>
    FLAC = AutoGen._AVCodecID.AV_CODEC_ID_FLAC,

    /// <summary>MP3 Audio Data Unit</summary>
    MP3ADU = AutoGen._AVCodecID.AV_CODEC_ID_MP3ADU,

    /// <summary>MP3 on 4</summary>
    MP3ON4 = AutoGen._AVCodecID.AV_CODEC_ID_MP3ON4,

    /// <summary>Shorten Audio Codec</summary>
    Shorten = AutoGen._AVCodecID.AV_CODEC_ID_SHORTEN,

    /// <summary>Apple Lossless Audio Codec</summary>
    ALAC = AutoGen._AVCodecID.AV_CODEC_ID_ALAC,

    /// <summary>Westwood Sound 1</summary>
    WestwoodSnd1 = AutoGen._AVCodecID.AV_CODEC_ID_WESTWOOD_SND1,

    /// <summary>GSM codec as in Berlin toast format</summary>
    GSM = AutoGen._AVCodecID.AV_CODEC_ID_GSM,

    /// <summary>QDM2 Audio Codec</summary>
    QDM2 = AutoGen._AVCodecID.AV_CODEC_ID_QDM2,

    /// <summary>Cook Audio Codec</summary>
    Cook = AutoGen._AVCodecID.AV_CODEC_ID_COOK,

    /// <summary>TrueSpeech Codec</summary>
    TrueSpeech = AutoGen._AVCodecID.AV_CODEC_ID_TRUESPEECH,

    /// <summary>True Audio Codec</summary>
    TTA = AutoGen._AVCodecID.AV_CODEC_ID_TTA,

    /// <summary>Smack Audio Codec</summary>
    SmackAudio = AutoGen._AVCodecID.AV_CODEC_ID_SMACKAUDIO,

    /// <summary>Qualcomm's CELP codec</summary>
    QCELP = AutoGen._AVCodecID.AV_CODEC_ID_QCELP,

    /// <summary>WavPack Codec</summary>
    WavPack = AutoGen._AVCodecID.AV_CODEC_ID_WAVPACK,

    /// <summary>DSICIN Audio Codec</summary>
    DSICINAUDIO = AutoGen._AVCodecID.AV_CODEC_ID_DSICINAUDIO,

    /// <summary>IMC Audio Codec</summary>
    IMC = AutoGen._AVCodecID.AV_CODEC_ID_IMC,

    /// <summary>MusePack 7 Codec</summary>
    MusePack7 = AutoGen._AVCodecID.AV_CODEC_ID_MUSEPACK7,

    /// <summary>Meridian Lossless Packing</summary>
    MLP = AutoGen._AVCodecID.AV_CODEC_ID_MLP,

    /// <summary>GSM MS Codec</summary>
    GSM_MS = AutoGen._AVCodecID.AV_CODEC_ID_GSM_MS,

    /// <summary>ATRAC3 Codec</summary>
    ATRAC3 = AutoGen._AVCodecID.AV_CODEC_ID_ATRAC3,

    /// <summary>Monkey's Audio Codec</summary>
    APE = AutoGen._AVCodecID.AV_CODEC_ID_APE,

    /// <summary>Nellymoser Audio Codec</summary>
    Nellymoser = AutoGen._AVCodecID.AV_CODEC_ID_NELLYMOSER,

    /// <summary>MusePack 8 Codec</summary>
    MusePack8 = AutoGen._AVCodecID.AV_CODEC_ID_MUSEPACK8,

    /// <summary>Speex Codec</summary>
    Speex = AutoGen._AVCodecID.AV_CODEC_ID_SPEEX,

    /// <summary>Windows Media Audio Voice Codec</summary>
    WMAVoice = AutoGen._AVCodecID.AV_CODEC_ID_WMAVOICE,

    /// <summary>Windows Media Audio Professional Codec</summary>
    WMAPRO = AutoGen._AVCodecID.AV_CODEC_ID_WMAPRO,

    /// <summary>Windows Media Audio Lossless Codec</summary>
    WMALossless = AutoGen._AVCodecID.AV_CODEC_ID_WMALOSSLESS,

    /// <summary>ATRAC3 Plus Codec</summary>
    ATRAC3P = AutoGen._AVCodecID.AV_CODEC_ID_ATRAC3P,

    /// <summary>Enhanced AC-3 Codec</summary>
    EAC3 = AutoGen._AVCodecID.AV_CODEC_ID_EAC3,

    /// <summary>SIPR Codec</summary>
    SIPR = AutoGen._AVCodecID.AV_CODEC_ID_SIPR,

    /// <summary>MP1 Codec</summary>
    MP1 = AutoGen._AVCodecID.AV_CODEC_ID_MP1,

    /// <summary>TwinVQ Codec</summary>
    TwinVQ = AutoGen._AVCodecID.AV_CODEC_ID_TWINVQ,

    /// <summary>TrueHD Codec</summary>
    TrueHD = AutoGen._AVCodecID.AV_CODEC_ID_TRUEHD,

    /// <summary>MP4 Audio Lossless Codec</summary>
    MP4ALS = AutoGen._AVCodecID.AV_CODEC_ID_MP4ALS,

    /// <summary>ATRAC1 Codec</summary>
    ATRAC1 = AutoGen._AVCodecID.AV_CODEC_ID_ATRAC1,

    /// <summary>Binkaudio RDFT Codec</summary>
    BinkaudioRDFT = AutoGen._AVCodecID.AV_CODEC_ID_BINKAUDIO_RDFT,

    /// <summary>Binkaudio DCT Codec</summary>
    BinkaudioDCT = AutoGen._AVCodecID.AV_CODEC_ID_BINKAUDIO_DCT,

    /// <summary>AAC LATM Codec</summary>
    AAC_LATM = AutoGen._AVCodecID.AV_CODEC_ID_AAC_LATM,

    /// <summary>QDMC Codec</summary>
    QDMC = AutoGen._AVCodecID.AV_CODEC_ID_QDMC,

    /// <summary>Celt Codec</summary>
    Celt = AutoGen._AVCodecID.AV_CODEC_ID_CELT,

    /// <summary>G.723.1 Codec</summary>
    G723_1 = AutoGen._AVCodecID.AV_CODEC_ID_G723_1,

    /// <summary>G.729 Codec</summary>
    G729 = AutoGen._AVCodecID.AV_CODEC_ID_G729,

    /// <summary>8SVX Exp Codec</summary>
    _8SVX_EXP = AutoGen._AVCodecID.AV_CODEC_ID_8SVX_EXP,

    /// <summary>8SVX Fib Codec</summary>
    _8SVX_FIB = AutoGen._AVCodecID.AV_CODEC_ID_8SVX_FIB,

    /// <summary>BMV Audio Codec</summary>
    BMV_AUDIO = AutoGen._AVCodecID.AV_CODEC_ID_BMV_AUDIO,

    /// <summary>RALF Codec</summary>
    RALF = AutoGen._AVCodecID.AV_CODEC_ID_RALF,

    /// <summary>IAC Codec</summary>
    IAC = AutoGen._AVCodecID.AV_CODEC_ID_IAC,

    /// <summary>ILBC Codec</summary>
    ILBC = AutoGen._AVCodecID.AV_CODEC_ID_ILBC,

    /// <summary>Opus Codec</summary>
    Opus = AutoGen._AVCodecID.AV_CODEC_ID_OPUS,

    /// <summary>Comfort Noise Codec</summary>
    ComfortNoise = AutoGen._AVCodecID.AV_CODEC_ID_COMFORT_NOISE,

    /// <summary>TAK Codec</summary>
    TAK = AutoGen._AVCodecID.AV_CODEC_ID_TAK,

    /// <summary>Metasound Codec</summary>
    Metasound = AutoGen._AVCodecID.AV_CODEC_ID_METASOUND,

    /// <summary>PAF Audio Codec</summary>
    PAF_AUDIO = AutoGen._AVCodecID.AV_CODEC_ID_PAF_AUDIO,

    /// <summary>On2 AVC Codec</summary>
    On2AVC = AutoGen._AVCodecID.AV_CODEC_ID_ON2AVC,

    /// <summary>DSS Speech Codec</summary>
    DSS_SP = AutoGen._AVCodecID.AV_CODEC_ID_DSS_SP,

    /// <summary>Codec2 Codec</summary>
    Codec2 = AutoGen._AVCodecID.AV_CODEC_ID_CODEC2,

    /// <summary>FFWaveSynth Codec</summary>
    FFWaveSynth = AutoGen._AVCodecID.AV_CODEC_ID_FFWAVESYNTH,

    /// <summary>Sonic Audio Codec</summary>
    Sonic = AutoGen._AVCodecID.AV_CODEC_ID_SONIC,

    /// <summary>Sonic Lossless Codec</summary>
    SonicLS = AutoGen._AVCodecID.AV_CODEC_ID_SONIC_LS,

    /// <summary>Enhanced Variable Rate Codec</summary>
    EVRC = AutoGen._AVCodecID.AV_CODEC_ID_EVRC,

    /// <summary>SMV Codec</summary>
    SMV = AutoGen._AVCodecID.AV_CODEC_ID_SMV,

    /// <summary>DSD Least Significant Bit First Codec</summary>
    DSD_LSBF = AutoGen._AVCodecID.AV_CODEC_ID_DSD_LSBF,

    /// <summary>DSD Most Significant Bit First Codec</summary>
    DSD_MSBF = AutoGen._AVCodecID.AV_CODEC_ID_DSD_MSBF,

    /// <summary>DSD LSBF Planar Codec</summary>
    DSD_LSBF_PLANAR = AutoGen._AVCodecID.AV_CODEC_ID_DSD_LSBF_PLANAR,

    /// <summary>DSD MSBF Planar Codec</summary>
    DSD_MSBF_PLANAR = AutoGen._AVCodecID.AV_CODEC_ID_DSD_MSBF_PLANAR,

    /// <summary>4GV Codec</summary>
    _4GV = AutoGen._AVCodecID.AV_CODEC_ID_4GV,

    /// <summary>Interplay ACM Codec</summary>
    InterplayACM = AutoGen._AVCodecID.AV_CODEC_ID_INTERPLAY_ACM,

    /// <summary>XMA1 Codec</summary>
    XMA1 = AutoGen._AVCodecID.AV_CODEC_ID_XMA1,

    /// <summary>XMA2 Codec</summary>
    XMA2 = AutoGen._AVCodecID.AV_CODEC_ID_XMA2,

    /// <summary>Destiny Codec</summary>
    DST = AutoGen._AVCodecID.AV_CODEC_ID_DST,

    /// <summary>ATRAC3 Advanced Lossless Codec</summary>
    ATRAC3AL = AutoGen._AVCodecID.AV_CODEC_ID_ATRAC3AL,

    /// <summary>ATRAC3 Personal Audio Lossless Codec</summary>
    ATRAC3PAL = AutoGen._AVCodecID.AV_CODEC_ID_ATRAC3PAL,

    /// <summary>Dolby E Codec</summary>
    DolbyE = AutoGen._AVCodecID.AV_CODEC_ID_DOLBY_E,

    /// <summary>APT-X Codec</summary>
    APTX = AutoGen._AVCodecID.AV_CODEC_ID_APTX,

    /// <summary>APT-X HD Codec</summary>
    APTX_HD = AutoGen._AVCodecID.AV_CODEC_ID_APTX_HD,

    /// <summary>Sub-band Coding Codec</summary>
    SBC = AutoGen._AVCodecID.AV_CODEC_ID_SBC,

    /// <summary>ATRAC9 Codec</summary>
    ATRAC9 = AutoGen._AVCodecID.AV_CODEC_ID_ATRAC9,

    /// <summary>HCOM Codec</summary>
    HCOM = AutoGen._AVCodecID.AV_CODEC_ID_HCOM,

    /// <summary>ACELP Kelvin Codec</summary>
    ACELP_KELVIN = AutoGen._AVCodecID.AV_CODEC_ID_ACELP_KELVIN,

    /// <summary>MPEG-H 3D Audio Codec</summary>
    MPEGH_3D_AUDIO = AutoGen._AVCodecID.AV_CODEC_ID_MPEGH_3D_AUDIO,

    /// <summary>Siren Codec</summary>
    Siren = AutoGen._AVCodecID.AV_CODEC_ID_SIREN,

    /// <summary>HCA Codec</summary>
    HCA = AutoGen._AVCodecID.AV_CODEC_ID_HCA,

    /// <summary>FastAudio Codec</summary>
    FastAudio = AutoGen._AVCodecID.AV_CODEC_ID_FASTAUDIO,

    /// <summary>MSN Siren Codec</summary>
    MSNSIREN = AutoGen._AVCodecID.AV_CODEC_ID_MSNSIREN,

    /// <summary>DFPWM Codec</summary>
    DFPWM = AutoGen._AVCodecID.AV_CODEC_ID_DFPWM,

    /// <summary>Bonk Codec</summary>
    Bonk = AutoGen._AVCodecID.AV_CODEC_ID_BONK,

    /// <summary>Miscellaneous Codec 4</summary>
    Misc4 = AutoGen._AVCodecID.AV_CODEC_ID_MISC4,

    /// <summary>APAC Codec</summary>
    APAC = AutoGen._AVCodecID.AV_CODEC_ID_APAC,

    /// <summary>FTR Codec</summary>
    FTR = AutoGen._AVCodecID.AV_CODEC_ID_FTR,

    /// <summary>WAVARC Codec</summary>
    WAVARC = AutoGen._AVCodecID.AV_CODEC_ID_WAVARC,

    /// <summary>RKA Codec</summary>
    RKA = AutoGen._AVCodecID.AV_CODEC_ID_RKA,

    /// <summary>AC-4 Codec</summary>
    AC4 = AutoGen._AVCodecID.AV_CODEC_ID_AC4,

    /// <summary>OSQ Codec</summary>
    OSQ = AutoGen._AVCodecID.AV_CODEC_ID_OSQ,

    /// <summary>QOA Codec</summary>
    QOA = AutoGen._AVCodecID.AV_CODEC_ID_QOA,

    /// <summary>LC3 Codec</summary>
    LC3 = AutoGen._AVCodecID.AV_CODEC_ID_LC3,

    /// <summary>A dummy ID pointing at the start of subtitle codecs.</summary>
    FIRST_SUBTITLE = AutoGen._AVCodecID.AV_CODEC_ID_FIRST_SUBTITLE,

    /// <summary>DVD Subtitle Codec</summary>
    DVD_SUBTITLE = AutoGen._AVCodecID.AV_CODEC_ID_DVD_SUBTITLE,

    /// <summary>DVB Subtitle Codec</summary>
    DVB_SUBTITLE = AutoGen._AVCodecID.AV_CODEC_ID_DVB_SUBTITLE,

    /// <summary>Raw UTF-8 Text</summary>
    Text = AutoGen._AVCodecID.AV_CODEC_ID_TEXT,

    /// <summary>SubRip Subtitle Codec</summary>
    XSUB = AutoGen._AVCodecID.AV_CODEC_ID_XSUB,

    /// <summary>SubRip Subtitle Codec</summary>
    SSA = AutoGen._AVCodecID.AV_CODEC_ID_SSA,

    /// <summary>QuickTime MOV Text Codec</summary>
    MovText = AutoGen._AVCodecID.AV_CODEC_ID_MOV_TEXT,

    /// <summary>HDMV PGS Subtitle Codec</summary>
    HDMV_PGS_SUBTITLE = AutoGen._AVCodecID.AV_CODEC_ID_HDMV_PGS_SUBTITLE,

    /// <summary>DVB Teletext Codec</summary>
    DVB_TELETEXT = AutoGen._AVCodecID.AV_CODEC_ID_DVB_TELETEXT,

    /// <summary>SubRip Subtitle Codec</summary>
    SRT = AutoGen._AVCodecID.AV_CODEC_ID_SRT,

    /// <summary>MicroDVD Subtitle Codec</summary>
    MicroDVD = AutoGen._AVCodecID.AV_CODEC_ID_MICRODVD,

    /// <summary>EIA-608 Caption Codec</summary>
    EIA_608 = AutoGen._AVCodecID.AV_CODEC_ID_EIA_608,

    /// <summary>Jacosoft Subtitle Codec</summary>
    Jacosub = AutoGen._AVCodecID.AV_CODEC_ID_JACOSUB,

    /// <summary>SAMI Subtitle Codec</summary>
    SAMI = AutoGen._AVCodecID.AV_CODEC_ID_SAMI,

    /// <summary>RealText Subtitle Codec</summary>
    RealText = AutoGen._AVCodecID.AV_CODEC_ID_REALTEXT,

    /// <summary>Spruce STL Subtitle Codec</summary>
    STL = AutoGen._AVCodecID.AV_CODEC_ID_STL,

    /// <summary>SubViewer 1 Subtitle Codec</summary>
    SubViewer1 = AutoGen._AVCodecID.AV_CODEC_ID_SUBVIEWER1,

    /// <summary>SubViewer Subtitle Codec</summary>
    SubViewer = AutoGen._AVCodecID.AV_CODEC_ID_SUBVIEWER,

    /// <summary>SubRip Subtitle Codec</summary>
    SubRip = AutoGen._AVCodecID.AV_CODEC_ID_SUBRIP,

    /// <summary>WebVTT Subtitle Codec</summary>
    WebVTT = AutoGen._AVCodecID.AV_CODEC_ID_WEBVTT,

    /// <summary>MPL2 Subtitle Codec</summary>
    MPL2 = AutoGen._AVCodecID.AV_CODEC_ID_MPL2,

    /// <summary>VPlayer Subtitle Codec</summary>
    VPlayer = AutoGen._AVCodecID.AV_CODEC_ID_VPLAYER,

    /// <summary>PJS Subtitle Codec</summary>
    PJS = AutoGen._AVCodecID.AV_CODEC_ID_PJS,

    /// <summary>Advanced SubStation Alpha Codec</summary>
    ASS = AutoGen._AVCodecID.AV_CODEC_ID_ASS,

    /// <summary>HDMV Text Subtitle Codec</summary>
    HDMV_TEXT_SUBTITLE = AutoGen._AVCodecID.AV_CODEC_ID_HDMV_TEXT_SUBTITLE,

    /// <summary>Timed Text Markup Language Codec</summary>
    TTML = AutoGen._AVCodecID.AV_CODEC_ID_TTML,

    /// <summary>ARIB Caption Codec</summary>
    ARIB_CAPTION = AutoGen._AVCodecID.AV_CODEC_ID_ARIB_CAPTION,

    /// <summary>A dummy ID pointing at the start of various fake codecs.</summary>
    FIRST_UNKNOWN = AutoGen._AVCodecID.AV_CODEC_ID_FIRST_UNKNOWN,

    /// <summary>TrueType Font Codec</summary>
    TTF = AutoGen._AVCodecID.AV_CODEC_ID_TTF,

    /// <summary>Contains timestamp estimated through PCR of program stream.</summary>
    SCTE_35 = AutoGen._AVCodecID.AV_CODEC_ID_SCTE_35,

    /// <summary>Electronic Program Guide Codec</summary>
    EPG = AutoGen._AVCodecID.AV_CODEC_ID_EPG,

    /// <summary>Binary Text Codec</summary>
    BINTEXT = AutoGen._AVCodecID.AV_CODEC_ID_BINTEXT,

    /// <summary>Binary Codec</summary>
    XBIN = AutoGen._AVCodecID.AV_CODEC_ID_XBIN,

    /// <summary>IDF Codec</summary>
    IDF = AutoGen._AVCodecID.AV_CODEC_ID_IDF,

    /// <summary>OpenType Font Codec</summary>
    OTF = AutoGen._AVCodecID.AV_CODEC_ID_OTF,

    /// <summary>SMPTE KLV Codec</summary>
    SMPTE_KLV = AutoGen._AVCodecID.AV_CODEC_ID_SMPTE_KLV,

    /// <summary>DVD Navigation Codec</summary>
    DVD_NAV = AutoGen._AVCodecID.AV_CODEC_ID_DVD_NAV,

    /// <summary>Timed ID3 Codec</summary>
    TIMED_ID3 = AutoGen._AVCodecID.AV_CODEC_ID_TIMED_ID3,

    /// <summary>Binary Data Codec</summary>
    BIN_DATA = AutoGen._AVCodecID.AV_CODEC_ID_BIN_DATA,

    /// <summary>SMPTE 2038 Codec</summary>
    SMPTE_2038 = AutoGen._AVCodecID.AV_CODEC_ID_SMPTE_2038,

    /// <summary>Codec ID is not known (like AV_CODEC_ID_NONE) but lavf should attempt to identify it</summary>
    PROBE = AutoGen._AVCodecID.AV_CODEC_ID_PROBE,

    /// <summary>Fake codec to indicate a raw MPEG-2 TS stream (only used by libavformat)</summary>
    MPEG2TS = AutoGen._AVCodecID.AV_CODEC_ID_MPEG2TS,

    /// <summary>Fake codec to indicate a MPEG-4 Systems stream (only used by libavformat)</summary>
    MPEG4SYSTEMS = AutoGen._AVCodecID.AV_CODEC_ID_MPEG4SYSTEMS,

    /// <summary>Dummy codec for streams containing only metadata information.</summary>
    FFMETADATA = AutoGen._AVCodecID.AV_CODEC_ID_FFMETADATA,

    /// <summary>Passthrough codec, AVFrames wrapped in AVPacket</summary>
    WRAPPED_AVFRAME = AutoGen._AVCodecID.AV_CODEC_ID_WRAPPED_AVFRAME,

    /// <summary>Dummy null video codec, useful mainly for development and debugging. Null encoder/decoder discard all input and never return any output.</summary>
    VNULL = AutoGen._AVCodecID.AV_CODEC_ID_VNULL,

    /// <summary>Dummy null audio codec, useful mainly for development and debugging. Null encoder/decoder discard all input and never return any output.</summary>
    ANULL = AutoGen._AVCodecID.AV_CODEC_ID_ANULL,

    /// <summary>DNxUncompressed video codec.</summary>
    DNXUC = AutoGen._AVCodecID.AV_CODEC_ID_DNXUC,

    /// <summary>RealVideo 6.0 codec.</summary>
    RV60 = AutoGen._AVCodecID.AV_CODEC_ID_RV60,

    /// <summary>JPEG XL Animation codec.</summary>
    JPEGXLAnim = AutoGen._AVCodecID.AV_CODEC_ID_JPEGXL_ANIM,

    /// <summary>APV video codec.</summary>
    APV = AutoGen._AVCodecID.AV_CODEC_ID_APV,

    /// <summary>ProRes RAW video codec.</summary>
    ProResRaw = AutoGen._AVCodecID.AV_CODEC_ID_PRORES_RAW,

    /// <summary>IVTV VBI subtitle codec.</summary>
    IVTV_VBI = AutoGen._AVCodecID.AV_CODEC_ID_IVTV_VBI,

    /// <summary>LCEVC video codec.</summary>
    LCEVC = AutoGen._AVCodecID.AV_CODEC_ID_LCEVC,

    /// <summary>SMPTE 436M ANC codec.</summary>
    SMPTE_436M_ANC = AutoGen._AVCodecID.AV_CODEC_ID_SMPTE_436M_ANC,


    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA Xbox format.</summary>
    ADPCM_IMA_XBOX = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_XBOX,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) Sanyo format.</summary>
    ADPCM_SANYO = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_SANYO,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA HVQM4 format.</summary>
    ADPCM_IMA_HVQM4 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_HVQM4,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA PDA format.</summary>
    ADPCM_IMA_PDA = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_PDA,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) N64 format.</summary>
    ADPCM_N64 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_N64,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA HVQM2 format.</summary>
    ADPCM_IMA_HVQM2 = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_HVQM2,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) IMA MAGIX format.</summary>
    ADPCM_IMA_MAGIX = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_MAGIX,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) PSX format.</summary>
    ADPCM_PSXC = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_PSXC,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) Circus format.</summary>
    ADPCM_CIRCUS = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_CIRCUS,

    /// <summary>Adaptive Differential Pulse Code Modulation (ADPCM) Escape format.</summary>
    ADPCM_IMA_ESCAPE = AutoGen._AVCodecID.AV_CODEC_ID_ADPCM_IMA_ESCAPE,

    /// <summary>G.728 audio codec.</summary>
    G728 = AutoGen._AVCodecID.AV_CODEC_ID_G728,

    /// <summary>AHX audio codec.</summary>
    AHX = AutoGen._AVCodecID.AV_CODEC_ID_AHX,
}

