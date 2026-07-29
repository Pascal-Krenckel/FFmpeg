using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Images;

/// <summary>
/// Specifies how an alpha channel is represented in relation to the color components.
/// </summary>
public enum AlphaMode
{
    /// <summary>
    /// The alpha handling is unspecified, or the pixel format does not contain
    /// an alpha channel.
    /// </summary>
    Unspecified = _AVAlphaMode.AVALPHA_MODE_UNSPECIFIED,

    /// <summary>
    /// The color components have already been multiplied by the alpha value.
    /// </summary>
    Premultiplied = _AVAlphaMode.AVALPHA_MODE_PREMULTIPLIED,

    /// <summary>
    /// The alpha channel is stored independently of the color components.
    /// </summary>
    Straight = _AVAlphaMode.AVALPHA_MODE_STRAIGHT,

    /// <summary>
    /// Number of defined alpha modes.
    /// This value is not part of the FFmpeg ABI and should not be used as an alpha mode.
    /// </summary>
    __COUNT__ = _AVAlphaMode.AVALPHA_MODE_NB
}
