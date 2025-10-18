using FFmpeg.Codecs.Video.LibSVTAV1;
using FFmpeg.Images;
using FFmpeg.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FFmpeg.Codecs.Video;

/// <summary>
/// Represents the SVT-AV1 video codec.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SVTAV1"/> class.
/// </remarks>
/// <param name="width">The width of the video.</param>
/// <param name="height">The height of the video.</param>
/// <param name="timeBase">The time base for the video.</param>
/// <exception cref="NotSupportedException">Thrown when the SVT-AV1 encoder is not found.</exception>
public unsafe class SVTAV1(int width, int height, Rational timeBase) : VideoCodec(Codec.FindEncoder("libsvtav1") ?? throw new NotSupportedException(), width, height, timeBase)
{

    /// <summary>
    /// Gets a value indicating whether the SVT-AV1 encoder is supported.
    /// </summary>
    public static bool IsSupported => Codec.FindEncoder("libsvtav1") != null;

    /// <summary>
    /// Gets or sets the profile to be used for encoding.
    /// </summary>
    public Profile? Profile { get; set; }

    /// <summary>
    /// Gets or sets the encoding preset.
    /// </summary>
    public int? Preset { get; set; }

    /// <summary>
    /// Gets or sets the bitrate control settings.
    /// </summary>
    public BitrateControl? BitrateControl { get; set; }

    /// <summary>
    /// Gets or sets the Constant Rate Factor (CRF) value.
    /// Setting this value is equivalent to `--rc 0 --aq-mode 2 --qp x [1-63]`.
    /// </summary>
    [DisallowNull]
    public int? CRF
    {
        get => BitrateControl?.CRF;
        set
        {
            BitrateControl bitrateControl = BitrateControl ?? new BitrateControl();
            bitrateControl.CRF = value;
            BitrateControl = bitrateControl;
        }
    }

    /// <summary>
    /// Specifies whether to use PSNR or VQ or SSIM as the tuning metric.
    /// </summary>
    public TuningMetric? Tune { get; set; }

    /// <summary>
    /// Gets or sets the GOP (Group of Pictures) settings.
    /// </summary>
    public GOPSettings? GOPSettings { get; set; }

    /// <summary>
    /// Gets or sets whether scene change detection is enabled.
    /// </summary>
    public bool? SceneChangeDetection
    {
        get => GOPSettings?.SceneChangeDetection;
        set
        {
            GOPSettings settings = GOPSettings ?? new GOPSettings();
            settings.SceneChangeDetection = value;
            GOPSettings = settings;
        }
    }

    /// <summary>
    /// Gets or sets the GOP size in frames.
    /// Use <see cref="KeyFrameIntervalAsTimeSpan"/> for seconds.
    /// </summary>
    public int? KeyFrameInterval
    {
        get => GOPSettings?.Frames;
        set
        {
            GOPSettings settings = GOPSettings ?? new GOPSettings();
            settings.Frames = value;
            GOPSettings = settings;
        }
    }

    /// <summary>
    /// Gets or sets the GOP size as a <see cref="TimeSpan"/>.
    /// Use <see cref="KeyFrameInterval"/> for frames.
    /// </summary>
    public TimeSpan? KeyFrameIntervalAsTimeSpan
    {
        get => GOPSettings?.AsTimeSpan;
        set
        {
            GOPSettings settings = GOPSettings ?? new GOPSettings();
            settings.AsTimeSpan = value;
            GOPSettings = settings;
        }
    }

    /// <summary>
    /// Gets or sets whether overlay pictures are enabled.
    /// </summary>
    public bool? EnableOverlays { get; set; }

    /// <summary>
    /// Gets or sets the screen content detection level.
    /// </summary>
    public ScreenContentMode? ScreenContentMode { get; set; }

    /// <summary>
    /// Gets or sets the level of film grain.
    /// </summary>
    public int? FilmGrain { get; set; }

    /// <summary>
    /// Gets or sets whether denoising is applied when film grain is enabled.
    /// </summary>
    public bool? FilmGrainDenoise { get; set; }

    /// <summary>
    /// Gets or sets whether fast decoding is enabled.
    /// </summary>
    public bool? FastDecode { get; set; }

    /// <summary>
    /// Gets or sets the number of logical processors used for encoding.
    /// A value of 0 means all processors are used.
    /// </summary>
    public int? Threads { get; set; }

    /// <summary>
    /// Gets additional SVT parameters that are not directly exposed through properties.
    /// </summary>
    public Dictionary<string, string> AdditionalSVTParameters { get; } = [];

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();

        if (PixelFormat != PixelFormat.None)
            _ = sb.Append($" -pix_fmt {AutoGen.ffmpeg.av_get_pix_fmt_name((AutoGen._AVPixelFormat)PixelFormat)}");

        _ = sb.Append($" {string.Join(' ', AdditionalParameters.Select(kv => $"--{kv.Key}={kv.Value}"))}");

        StringBuilder svtParams = new();

        if (Profile.HasValue)
            _ = svtParams.Append($"profile={Profile.Value.ProfileID}:");
        if (Preset.HasValue)
            _ = svtParams.Append($"preset={Preset.Value}:");
        if (BitrateControl.HasValue)
            _ = svtParams.Append($"{BitrateControl.Value}:");
        if (Tune.HasValue)
            _ = svtParams.Append($"tune={(int)Tune.Value}:");
        if (GOPSettings.HasValue)
            _ = svtParams.Append($"{GOPSettings.Value}:");
        if (EnableOverlays.HasValue)
            _ = svtParams.Append($"enable-overlays={(EnableOverlays.Value ? 1 : 0)}:");
        if (ScreenContentMode.HasValue)
            _ = svtParams.Append($"scm={(int)ScreenContentMode.Value}:");
        if (FilmGrain.HasValue)
            _ = svtParams.Append($"film-grain={FilmGrain.Value}:");
        if (FilmGrainDenoise.HasValue)
            _ = svtParams.Append($"film-grain-denoise={(FilmGrainDenoise.Value ? 1 : 0)}:");
        if (FastDecode.HasValue)
            _ = svtParams.Append($"fast-decode={(FastDecode.Value ? 1 : 0)}:");
        if (Threads.HasValue)
            _ = svtParams.Append($"lp={Threads}:");
        foreach (KeyValuePair<string, string> pair in AdditionalSVTParameters)
            _ = svtParams.Append($"{pair.Key}={pair.Value}:");

        if (svtParams.Length > 0)
        {
            if (svtParams[^1] == ':')
                svtParams.Length--;
            _ = sb.Append($" -svtav1-params \"{svtParams}\"");
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    protected override void AddCodecOptions(CodecContext context)
    {
        Dictionary<string, string> dic = new(AdditionalSVTParameters);

        if (Profile.HasValue)
            context.Profile = Profile.Value;
        if (Preset.HasValue)
            dic["preset"] = Preset.Value.ToString();
        if (BitrateControl.HasValue)
            BitrateControl.Value.AddToDictionary(dic);
        if (Tune.HasValue)
            dic["tune"] = ((int)Tune.Value).ToString();
        if (GOPSettings.HasValue)
            GOPSettings.Value.AddToDictionary(dic);
        if (EnableOverlays.HasValue)
            dic["enable-overlays"] = EnableOverlays.Value ? "1" : "0";
        if (ScreenContentMode.HasValue)
            dic["scm"] = ((int)ScreenContentMode.Value).ToString();
        if (FilmGrain.HasValue)
            dic["film-grain"] = FilmGrain.Value.ToString();
        if (FilmGrainDenoise.HasValue)
            dic["film-grain-denoise"] = FilmGrainDenoise.Value ? "1" : "0";
        if (FastDecode.HasValue)
            dic["fast-decode"] = FastDecode.Value ? "1" : "0";
        if (Threads.HasValue)
            dic["lp"] = Threads.Value.ToString();

        context.SetOption("svtav1-params", dic).ThrowIfError();
    }
}
