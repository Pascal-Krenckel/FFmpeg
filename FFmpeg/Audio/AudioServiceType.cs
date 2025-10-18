namespace FFmpeg.Audio;
/// <summary>
/// Specifies the type of audio service or channel. These values are used to indicate the purpose of the audio track in various multimedia applications.
/// </summary>
public enum AudioServiceType : int
{
    /// <summary>
    /// Main audio service, typically used for the primary audio content such as dialogue and main sound.
    /// </summary>
    Main = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_MAIN,

    /// <summary>
    /// Audio service used for effects, such as sound effects or environmental sounds.
    /// </summary>
    Effects = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_EFFECTS,

    /// <summary>
    /// Audio service designed for visually impaired users, often including audio descriptions of visual content.
    /// </summary>
    VisuallyImpaired = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_VISUALLY_IMPAIRED,

    /// <summary>
    /// Audio service aimed at hearing-impaired users, which may include enhanced dialogue or other audio adjustments.
    /// </summary>
    HearingImpaired = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_HEARING_IMPAIRED,

    /// <summary>
    /// Audio service for dialogue, focusing on the clarity and prominence of spoken words.
    /// </summary>
    Dialogue = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_DIALOGUE,

    /// <summary>
    /// Audio service for commentary, which might be used for adding additional spoken content such as in sports or educational videos.
    /// </summary>
    Commentary = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_COMMENTARY,

    /// <summary>
    /// Audio service for emergency messages or alerts, often used in safety or alert systems.
    /// </summary>
    Emergency = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_EMERGENCY,

    /// <summary>
    /// Audio service for voice-over, which could be used for narrations or overdubs.
    /// </summary>
    VoiceOver = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_VOICE_OVER,

    /// <summary>
    /// Audio service for karaoke, typically used for music tracks with lyrics displayed for sing-along.
    /// </summary>
    Karaoke = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_KARAOKE,

    /// <summary>
    /// Reserved value indicating that the audio service type is not part of the defined ABI (Application Binary Interface).
    /// </summary>
    __COUNT__ = AutoGen._AVAudioServiceType.AV_AUDIO_SERVICE_TYPE_NB
}
