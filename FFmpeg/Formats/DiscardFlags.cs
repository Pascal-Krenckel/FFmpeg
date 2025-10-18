using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Formats;
/// <summary>
/// Flags to specify the discard behavior for frames during processing.
/// </summary>
[Flags]
public enum DiscardFlags : int
{
    /// <summary>
    /// Discard nothing, keep all frames.
    /// </summary>
    None = AutoGen._AVDiscard.AVDISCARD_NONE,

    /// <summary>
    /// Discard useless packets like zero-size packets in AVI files.
    /// </summary>
    Default = AutoGen._AVDiscard.AVDISCARD_DEFAULT,

    /// <summary>
    /// Discard all non-reference frames.
    /// </summary>
    NonReference = AutoGen._AVDiscard.AVDISCARD_NONREF,

    /// <summary>
    /// Discard all bi-directional frames (B-frames).
    /// </summary>
    BiDirectional = AutoGen._AVDiscard.AVDISCARD_BIDIR,

    /// <summary>
    /// Discard all non-intra frames.
    /// </summary>
    NonIntra = AutoGen._AVDiscard.AVDISCARD_NONINTRA,

    /// <summary>
    /// Discard all frames except keyframes.
    /// </summary>
    NonKey = AutoGen._AVDiscard.AVDISCARD_NONKEY,

    /// <summary>
    /// Discard all frames.
    /// </summary>
    All = AutoGen._AVDiscard.AVDISCARD_ALL,
}

