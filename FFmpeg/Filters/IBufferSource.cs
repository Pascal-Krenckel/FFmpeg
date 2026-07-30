using FFmpeg.AutoGen;
using FFmpeg.IO;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.Filters;

/// <summary>
/// Interface to access buffer source specific functions.
/// </summary>
public unsafe interface IBufferSource
{
    /// <summary>
    /// Pointer to the unmanaged _AVFilterContext
    /// </summary>
    protected _AVFilterContext* Context { get; }

    /// <summary>
    /// Sets the parameters of a buffer source filter.
    /// </summary>
    /// <param name="parameters">
    /// The buffer source parameters to apply.
    /// </param>
    /// <returns>
    /// The result of the operation.
    /// </returns>
    public AVResult32 SetBufferSourceParameters(BufferSrcParameters parameters) =>
        ffmpeg.av_buffersrc_parameters_set(Context, parameters.parameters);

    /// <summary>
    /// Sends a frame to a buffer source filter.
    /// </summary>
    /// <param name="frame">
    /// The frame to send, or <see langword="null"/> to signal end-of-stream.
    /// </param>
    /// <param name="keepRef">
    /// <see langword="true"/> to keep a reference to the supplied frame;
    /// <see langword="false"/> to transfer ownership when possible.
    /// </param>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    public AVResult32 SendFrame(AVFrame? frame, bool keepRef = false)
    {
        AutoGen._AVFrame* f = frame != null ? frame.Frame : null;
        return keepRef ? ffmpeg.av_buffersrc_write_frame(Context, f) : ffmpeg.av_buffersrc_add_frame(Context, f);
    }

    /// <summary>
    /// Signals end-of-stream to the buffer source filter.
    /// </summary>
    /// <returns>
    /// The result returned by FFmpeg.
    /// </returns>
    public AVResult32 Drain() => SendFrame(null);

}
