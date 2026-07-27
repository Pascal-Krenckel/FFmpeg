using FFmpeg.AutoGen;

namespace FFmpeg.Audio;

/// <summary>
/// Represents the position or purpose of an audio channel within a channel layout.
/// </summary>
public enum AudioChannel
{
    /// <summary>
    /// No channel / invalid channel.
    /// </summary>
    None = _AVChannel.AV_CHAN_NONE,

    /// <summary>
    /// Front left channel.
    /// </summary>
    FrontLeft = _AVChannel.AV_CHAN_FRONT_LEFT,

    /// <summary>
    /// Front right channel.
    /// </summary>
    FrontRight = _AVChannel.AV_CHAN_FRONT_RIGHT,

    /// <summary>
    /// Front center channel.
    /// </summary>
    FrontCenter = _AVChannel.AV_CHAN_FRONT_CENTER,

    /// <summary>
    /// Low-frequency effects (LFE) channel.
    /// </summary>
    LowFrequency = _AVChannel.AV_CHAN_LOW_FREQUENCY,

    /// <summary>
    /// Back left (rear left) channel.
    /// </summary>
    BackLeft = _AVChannel.AV_CHAN_BACK_LEFT,

    /// <summary>
    /// Back right (rear right) channel.
    /// </summary>
    BackRight = _AVChannel.AV_CHAN_BACK_RIGHT,

    /// <summary>
    /// Front left-of-center channel.
    /// </summary>
    FrontLeftOfCenter = _AVChannel.AV_CHAN_FRONT_LEFT_OF_CENTER,

    /// <summary>
    /// Front right-of-center channel.
    /// </summary>
    FrontRightOfCenter = _AVChannel.AV_CHAN_FRONT_RIGHT_OF_CENTER,

    /// <summary>
    /// Back center (rear center) channel.
    /// </summary>
    BackCenter = _AVChannel.AV_CHAN_BACK_CENTER,

    /// <summary>
    /// Side left channel.
    /// </summary>
    SideLeft = _AVChannel.AV_CHAN_SIDE_LEFT,

    /// <summary>
    /// Side right channel.
    /// </summary>
    SideRight = _AVChannel.AV_CHAN_SIDE_RIGHT,

    /// <summary>
    /// Top center channel.
    /// </summary>
    TopCenter = _AVChannel.AV_CHAN_TOP_CENTER,

    /// <summary>
    /// Top front left channel.
    /// </summary>
    TopFrontLeft = _AVChannel.AV_CHAN_TOP_FRONT_LEFT,

    /// <summary>
    /// Top front center channel.
    /// </summary>
    TopFrontCenter = _AVChannel.AV_CHAN_TOP_FRONT_CENTER,

    /// <summary>
    /// Top front right channel.
    /// </summary>
    TopFrontRight = _AVChannel.AV_CHAN_TOP_FRONT_RIGHT,

    /// <summary>
    /// Top back left channel.
    /// </summary>
    TopBackLeft = _AVChannel.AV_CHAN_TOP_BACK_LEFT,

    /// <summary>
    /// Top back center channel.
    /// </summary>
    TopBackCenter = _AVChannel.AV_CHAN_TOP_BACK_CENTER,

    /// <summary>
    /// Top back right channel.
    /// </summary>
    TopBackRight = _AVChannel.AV_CHAN_TOP_BACK_RIGHT,

    /// <summary>
    /// Left channel of a stereo downmix.
    /// </summary>
    StereoLeft = _AVChannel.AV_CHAN_STEREO_LEFT,

    /// <summary>
    /// Right channel of a stereo downmix.
    /// </summary>
    StereoRight = _AVChannel.AV_CHAN_STEREO_RIGHT,

    /// <summary>
    /// Wide left channel.
    /// </summary>
    WideLeft = _AVChannel.AV_CHAN_WIDE_LEFT,

    /// <summary>
    /// Wide right channel.
    /// </summary>
    WideRight = _AVChannel.AV_CHAN_WIDE_RIGHT,

    /// <summary>
    /// Surround direct left channel.
    /// </summary>
    SurroundDirectLeft = _AVChannel.AV_CHAN_SURROUND_DIRECT_LEFT,

    /// <summary>
    /// Surround direct right channel.
    /// </summary>
    SurroundDirectRight = _AVChannel.AV_CHAN_SURROUND_DIRECT_RIGHT,

    /// <summary>
    /// Second low-frequency effects (LFE2) channel.
    /// </summary>
    LowFrequency2 = _AVChannel.AV_CHAN_LOW_FREQUENCY_2,

    /// <summary>
    /// Top side left channel.
    /// </summary>
    TopSideLeft = _AVChannel.AV_CHAN_TOP_SIDE_LEFT,

    /// <summary>
    /// Top side right channel.
    /// </summary>
    TopSideRight = _AVChannel.AV_CHAN_TOP_SIDE_RIGHT,

    /// <summary>
    /// Bottom front center channel.
    /// </summary>
    BottomFrontCenter = _AVChannel.AV_CHAN_BOTTOM_FRONT_CENTER,

    /// <summary>
    /// Bottom front left channel.
    /// </summary>
    BottomFrontLeft = _AVChannel.AV_CHAN_BOTTOM_FRONT_LEFT,

    /// <summary>
    /// Bottom front right channel.
    /// </summary>
    BottomFrontRight = _AVChannel.AV_CHAN_BOTTOM_FRONT_RIGHT,

    /// <summary>
    /// Side surround left channel (+90°, also known as Lss or SiL).
    /// </summary>
    SideSurroundLeft = _AVChannel.AV_CHAN_SIDE_SURROUND_LEFT,

    /// <summary>
    /// Side surround right channel (-90°, also known as Rss or SiR).
    /// </summary>
    SideSurroundRight = _AVChannel.AV_CHAN_SIDE_SURROUND_RIGHT,

    /// <summary>
    /// Top surround left channel (+110°, also known as Lvs or TpLS).
    /// </summary>
    TopSurroundLeft = _AVChannel.AV_CHAN_TOP_SURROUND_LEFT,

    /// <summary>
    /// Top surround right channel (-110°, also known as Rvs or TpRS).
    /// </summary>
    TopSurroundRight = _AVChannel.AV_CHAN_TOP_SURROUND_RIGHT,

    /// <summary>
    /// Left binaural channel.
    /// </summary>
    BinauralLeft = _AVChannel.AV_CHAN_BINAURAL_LEFT,

    /// <summary>
    /// Right binaural channel.
    /// </summary>
    BinauralRight = _AVChannel.AV_CHAN_BINAURAL_RIGHT,

    /// <summary>
    /// Channel is intentionally unused and may be safely ignored.
    /// </summary>
    Unused = _AVChannel.AV_CHAN_UNUSED,

    /// <summary>
    /// Channel contains audio data, but its physical position is unknown.
    /// </summary>
    Unknown = _AVChannel.AV_CHAN_UNKNOWN,

    /// <summary>
    /// First Ambisonic channel using the ACN (Ambisonic Channel Number) system.
    /// Values from <see cref="AmbisonicBase"/> to <see cref="AmbisonicEnd"/> represent
    /// Ambisonic components.
    /// </summary>
    AmbisonicBase = _AVChannel.AV_CHAN_AMBISONIC_BASE,

    /// <summary>
    /// Last Ambisonic channel using the ACN (Ambisonic Channel Number) system.
    /// </summary>
    AmbisonicEnd = _AVChannel.AV_CHAN_AMBISONIC_END,
}