using FFmpeg.Audio;
using FFmpeg.AutoGen;
using FFmpeg.Codecs;
using FFmpeg.Collections;
using FFmpeg.Images;

namespace FFmpeg.Utils;

/// <summary>
/// Represents a decoded audio or video frame in memory. This structure holds raw data for video frames or audio samples. <br/>
/// It provides methods and properties to access and manipulate this data, as well as to manage the associated resources.
/// </summary>
public sealed unsafe class AVFrame : IDisposable
{
    /// <summary>
    /// Gets the underlying unmanaged <see cref="AutoGen._AVFrame"/> structure.
    /// </summary>
    internal AutoGen._AVFrame* Frame { get; private set; } = null;

    /// <summary>
    /// Gets the capacity (in bytes) of the specified buffer index in the frame.  
    /// Returns 0 if the buffer is not set or the index is out of range.
    /// </summary>
    /// <param name="index">
    /// The index of the buffer to retrieve.  
    /// Indices 0–7 refer to <c>buf</c>, and indices 8 and above refer to <c>extended_buf</c>.
    /// </param>
    /// <returns>
    /// The size of the buffer in bytes, or 0 if not available.
    /// </returns>
    public int GetBufferCapacity(int index)
    {
        if (index < 8)
            return (int)(Frame->buf[(uint)index] != null ? Frame->buf[(uint)index]->size : 0);
        index -= 8;
        return index < Frame->nb_extended_buf
            ? (int)(Frame->extended_buf[(uint)index] != null ? Frame->extended_buf[(uint)index]->size : 0)
            : 0;

    }

    /// <summary>
    /// Allocates a new <see cref="AVFrame"/>. Throws <see cref="OutOfMemoryException"/> if allocation fails.
    /// </summary>
    /// <returns>A newly allocated <see cref="AVFrame"/> instance.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if the frame allocation fails.</exception>
    public static AVFrame Allocate()
    {
        AutoGen._AVFrame* f = ffmpeg.av_frame_alloc();
        return f == null ? throw new OutOfMemoryException() : new AVFrame(f);
    }

    /// <summary>
    /// Gets a value indicating whether this frame represents video data.
    /// </summary>
    public bool IsVideo => Width != 0 && Height != 0;

    /// <summary>
    /// Gets a value indicating whether this frame represents audio data.
    /// </summary>
    public bool IsAudio => SampleRate > 0;

    /// <summary>
    /// Gets a value indicating whether this frame has an associated buffer.
    /// </summary>
    public bool HasBuffer => Frame->buf[0] != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVFrame"/> class, wrapping an existing unmanaged <see cref="AutoGen._AVFrame"/> structure.
    /// Use <see cref="Allocate"/> to create a new instance instead of calling this constructor directly.
    /// </summary>
    /// <param name="frame">A pointer to an already allocated unmanaged AVFrame structure.</param>
    internal AVFrame(AutoGen._AVFrame* frame) => Frame = frame;

    /// <summary>
    /// Gets the data buffers for the frame, represented as a span of pointers. <br/>
    /// For video frames, the span contains pointers to pixel planes. <br/>
    /// For audio frames, it contains pointers to audio channels.
    /// </summary>
    public ReadOnlySpan<IntPtr> Data
    {
        get
        {
            if (ExtendedData == null)
                return [];
            else
            {
                return IsVideo
                ? new(Frame->extended_data, PixelFormat.PlaneCount())
                : SampleFormat.IsPacked() ? new(Frame->extended_data, 1) : new(Frame->extended_data, Frame->ch_layout.nb_channels);
            }
        }
    }

    public Span<byte> GetData(int index)
    {
        if (ExtendedData[index] == null)
            return [];
        if (IsAudio)
            return new(Frame->extended_data[index], Frame->linesize[0]);
        if (IsVideo)
        {
            uint uindex = (uint)index;
            ulong_array4 sizes = new();
            long_array4 linesize = new();
            for (uint i = 0; i < linesize.Length; i++)
                linesize[i] = LineSize[i];
            AVResult32 res = ffmpeg.av_image_fill_plane_sizes(ref sizes, (_AVPixelFormat)PixelFormat, Height, in linesize);
            res.ThrowIfError();
            int length = (int)sizes[uindex];
            byte* data = linesize[uindex] >= 0 ? ExtendedData[index] : ExtendedData[index] - length;
            return new(data, length);
        }
        throw new NotSupportedException("Only audio or video frames are supported");
    }

    public Span<byte> GetBufferSpan(int bufferIndex)
    {
        if (bufferIndex < 8)
        {
            return Frame->buf[(uint)bufferIndex] == null
                ? []
                : new(Frame->buf[(uint)bufferIndex]->data, (int)Frame->buf[(uint)bufferIndex]->size);
        }
        bufferIndex -= 8;
        return bufferIndex >= Frame->nb_extended_buf
            ? []
            : Frame->extended_buf[(uint)bufferIndex] == null
            ? []
            : new(Frame->extended_buf[(uint)bufferIndex]->data, (int)Frame->extended_buf[(uint)bufferIndex]->size);
    }

    /// <summary>
    /// Gets a reference to the array of line sizes for each data buffer in the frame. <br/>
    /// For video frames, this usually indicates the size in bytes of each picture line (stride). <br/>
    /// For audio frames, this may indicate the size of each channel buffer.
    /// </summary>
    public ref int_array8 LineSize => ref Frame->linesize;

    /// <summary>
    /// Gets or sets pointers to the extended data buffers for the frame. <br/>
    /// This is typically used when the data exceeds the size of the <see cref="Data"/> array.
    /// </summary>
    internal byte** ExtendedData { get => Frame->extended_data; set => Frame->extended_data = value; }

    /// <summary>
    /// Gets or sets the coded width (in pixels) of the video frame. <br/>
    /// This represents the width of the rectangle that contains meaningful pixel data.
    /// </summary>
    public int Width { get => Frame->width; set => Frame->width = value; }

    /// <summary>
    /// Gets the width of the cropped frame in pixels.
    /// </summary>
    /// <remarks>The cropped width is calculated by subtracting the left and right crop offsets from the
    /// original frame width.</remarks>
    public int CroppedWidth => Width - (int)Frame->crop_left - (int)Frame->crop_right;

    /// <summary>
    /// Gets or sets the coded height (in pixels) of the video frame. <br/>
    /// This represents the height of the rectangle that contains meaningful pixel data.
    /// </summary>
    public int Height { get => Frame->height; set => Frame->height = value; }

    /// <summary>
    /// Gets the height of the video frame after applying the top and bottom cropping offsets.
    /// </summary>
    /// <remarks>The cropped height is calculated by subtracting the top and bottom crop offsets from the
    /// original frame height.</remarks>
    public int CroppedHeight => Height - (int)Frame->crop_top - (int)Frame->crop_bottom;

    /// <summary>
    /// Gets or sets the number of audio samples per channel described by this frame.
    /// </summary>
    public int SampleCount { get => Frame->nb_samples; set => Frame->nb_samples = value; }

    /// <summary>
    /// Gets or sets the format of the frame. <br/>
    /// Values correspond to <see cref="_AVPixelFormat"/> for video frames or <see cref="_AVSampleFormat"/> for audio frames.
    /// </summary>
    public int Format { get => Frame->format; set => Frame->format = value; }

    /// <summary>
    /// Gets or sets the pixel format of the video frame. <br/>
    /// This is a strongly-typed version of <see cref="Format"/> when working with video frames.
    /// </summary>
    public PixelFormat PixelFormat
    {
        get => (PixelFormat)Frame->format;
        set => Frame->format = (int)value;
    }

    /// <summary>
    /// Gets or sets the sample format of the audio frame. <br/>
    /// This is a strongly-typed version of <see cref="Format"/> when working with audio frames.
    /// </summary>
    public SampleFormat SampleFormat
    {
        get => (SampleFormat)Frame->format;
        set => Frame->format = (int)value;
    }

    /// <summary>
    /// Gets or sets the type of the picture in the video frame, such as I-frame, P-frame, or B-frame.
    /// </summary>
    public PictureType PictureType
    {
        get => (PictureType)Frame->pict_type;
        set => Frame->pict_type = (_AVPictureType)value;
    }



    /// <summary>
    /// Gets or sets the sample aspect ratio for the video frame. A value of 0/1 indicates an unspecified aspect ratio.
    /// </summary>
    public Rational PixelAspectRatio
    {
        get => Frame->sample_aspect_ratio;
        set => Frame->sample_aspect_ratio = value;
    }

    /// <summary>
    /// Gets or sets the presentation timestamp (PTS) of the frame, in time base units. 
    /// This timestamp indicates when the frame should be displayed to the user.
    /// </summary>
    public long PresentationTimestamp
    {
        get => Frame->pts;
        set => Frame->pts = value;
    }

    /// <summary>
    /// Gets the presentation timestamp (PTS) of the frame, if available. 
    /// If the PTS is unavailable, returns the best-effort timestamp as a fallback.
    /// </summary>
    /// <returns>
    /// A timestamp (in stream time base units) representing when the frame should be presented. 
    /// This is typically the PTS; if the PTS is not set, the best-effort timestamp is returned instead.
    /// </returns>
    public long GetPresentationTimestamp() => PresentationTimestamp != ffmpeg.AV_NOPTS_VALUE ? PresentationTimestamp : BestEffortTimestamp;

    /// <summary>
    /// Gets or sets the decoding timestamp (DTS) of the packet that triggered the return of this frame.
    /// The DTS is used to order frames before they are presented.
    /// </summary>
    public long PacketDecoderTimestamp
    {
        get => Frame->pkt_dts;
        set => Frame->pkt_dts = value;
    }

    /// <summary>
    /// Gets or sets the time base for the timestamps in this frame. The time base defines the unit of time for
    /// <see cref="PresentationTimestamp"/> and <see cref="PacketDecoderTimestamp"/> and is typically set by decoders or filters.
    /// </summary>
    public Rational TimeBase
    {
        get => Frame->time_base;
        set => Frame->time_base = value;
    }

    /// <summary>
    /// Gets or sets the quality of the frame. Valid values range from 1 (high quality) to <c>FF_LAMBDA_MAX</c> (low quality).
    /// </summary>
    public Quality Quality
    {
        get => Frame->quality;
        set => Frame->quality = value;
    }

    /// <summary>
    /// Gets or sets the number of fields in this frame that should be repeated.
    /// The total duration of the frame should be the normal duration plus the number of repeated fields.
    /// </summary>
    public int RepeatPicture
    {
        get => Frame->repeat_pict;
        set => Frame->repeat_pict = value;
    }


    /// <summary>
    /// Gets or sets the sample rate of the audio data in this frame, in Hz.
    /// </summary>
    public int SampleRate
    {
        get => Frame->sample_rate;
        set => Frame->sample_rate = value;
    }

    /// <summary>
    /// Gets or sets the flags associated with this frame, represented as a combination of <see cref="FrameFlags"/> values.
    /// These flags provide additional metadata for the frame.
    /// </summary>
    public FrameFlags Flags
    {
        get => (FrameFlags)Frame->flags;
        set => Frame->flags = (int)value;
    }

    /// <summary>
    /// Gets or sets the YUV range (color range) for the frame. This specifies the range of color values used in YUV encoding.
    /// For video frames, this can differentiate between MPEG (full range) and JPEG (limited range) YUV encoding.
    /// - Encoding: Notify by user.
    /// - Decoding: Notify by <see cref="ffmpeg"/>.
    /// </summary>
    public ColorRange ColorRange
    {
        get => (ColorRange)Frame->color_range;
        set => Frame->color_range = (_AVColorRange)value;
    }

    /// <summary>
    /// Gets or sets the color primaries for the frame, describing the chromaticity coordinates of the source's red, green, and blue components.
    /// </summary>
    public ColorPrimaries ColorPrimaries
    {
        get => (ColorPrimaries)Frame->color_primaries;
        set => Frame->color_primaries = (_AVColorPrimaries)value;
    }

    /// <summary>
    /// Gets or sets the transfer characteristic of the frame, which describes the non-linear transfer function 
    /// used to encode and decode color values. This can affect how brightness and contrast are handled in the frame.
    /// </summary>
    public ColorTransferCharacteristic ColorTransferCharacteristic
    {
        get => (ColorTransferCharacteristic)Frame->color_trc;
        set => Frame->color_trc = (_AVColorTransferCharacteristic)value;
    }

    /// <summary>
    /// Gets or sets the YUV colorspace type of the frame. The colorspace determines how the YUV color model is applied to the frame.
    /// - Encoding: Notify by user.
    /// - Decoding: Notify by <see cref="ffmpeg"/>.
    /// </summary>
    public ColorSpace ColorSpace
    {
        get => (ColorSpace)Frame->colorspace;
        set => Frame->colorspace = (_AVColorSpace)value;
    }

    /// <summary>
    /// Gets or sets the chroma subsampling location, which defines the position of chroma samples in relation to the luma samples.
    /// </summary>
    public ChromaLocation ChromaLocation
    {
        get => (ChromaLocation)Frame->chroma_location;
        set => Frame->chroma_location = (_AVChromaLocation)value;
    }
    /// <summary>
    /// Gets or sets the timestamp of the frame, estimated using various heuristics, in the stream's time base.
    /// - <langword>Encoding</langword>: Unused.
    /// - <langword>Decoding</langword>: Notify by libavcodec, read by the user.
    /// </summary>
    public long BestEffortTimestamp
    {
        get => Frame->best_effort_timestamp;
        set => Frame->best_effort_timestamp = value;
    }

    /// <summary>
    /// Gets the metadata associated with the frame.
    /// - <langword>Encoding</langword>: Notify by the user.
    /// - <langword>Decoding</langword>: Notify by libavcodec.
    /// </summary>
    public AVDictionary_ref Metadata => new(&Frame->metadata);

    /// <summary>
    /// Gets or sets the decode error flags for the frame, indicating errors that occurred during decoding.
    /// This is set to a combination of <c>FF_DECODE_ERROR_xxx</c> flags if the decoder produced a frame but encountered errors.
    /// - <langword>Encoding</langword>: Unused.
    /// - <langword>Decoding</langword>: Notify by libavcodec, read by the user.
    /// </summary>
    public DecodeErrorFlags DecodeErrorFlags
    {
        get => (DecodeErrorFlags)Frame->decode_error_flags;
        set => Frame->decode_error_flags = (int)value;
    }

    /// <summary>
    /// Gets a reference to the <see cref="HW.FramesContext"/> for hardware-accelerated frames.
    /// This context describes the frame when using hardware acceleration.
    /// </summary>
    public HW.FramesContext_ref FramesContext => new(&Frame->hw_frames_ctx, false);

    /// <summary>
    /// Gets or sets the number of pixels to crop from the top border of the video frame. 
    /// This is used to discard pixels from the frame and obtain a sub-rectangle intended for presentation.
    /// </summary>
    public ulong CropTop
    {
        get => Frame->crop_top;
        set => Frame->crop_top = value;
    }

    /// <summary>
    /// Gets or sets the number of pixels to crop from the bottom border of the video frame. 
    /// This is used to discard pixels from the frame and obtain a sub-rectangle intended for presentation.
    /// </summary>
    public ulong CropBottom
    {
        get => Frame->crop_bottom;
        set => Frame->crop_bottom = value;
    }

    /// <summary>
    /// Gets or sets the number of pixels to crop from the left border of the video frame. 
    /// This is used to discard pixels from the frame and obtain a sub-rectangle intended for presentation.
    /// </summary>
    public ulong CropLeft
    {
        get => Frame->crop_left;
        set => Frame->crop_left = value;
    }

    /// <summary>
    /// Gets or sets the number of pixels to crop from the right border of the video frame. 
    /// This is used to discard pixels from the frame and obtain a sub-rectangle intended for presentation.
    /// </summary>
    public ulong CropRight
    {
        get => Frame->crop_right;
        set => Frame->crop_right = value;
    }

    /// <summary>
    /// Gets the channel layout of the audio data in the frame.
    /// The channel layout defines the spatial positioning of audio channels (e.g., mono, stereo).
    /// </summary>
    public ChannelLayout_ref ChannelLayout => new(&Frame->ch_layout, false);

    /// <summary>
    /// Gets or sets the duration of the frame, in the same units as <see cref="PresentationTimestamp"/>.
    /// A value of 0 indicates an unknown duration.
    /// </summary>
    public long Duration
    {
        get => Frame->duration;
        set => Frame->duration = value;
    }

    /// <summary>
    /// Makes the frame writeable, ensuring that it can be modified. 
    /// If the frame is shared, this will create a writable copy of the frame.
    /// </summary>
    /// <returns>An <see cref="AVResult32"/> indicating success or failure of the operation.</returns>
    public AVResult32 MakeWriteable() => ffmpeg.av_frame_make_writable(Frame);

    /// <summary>
    /// Gets a value indicating whether the frame is read-only. 
    /// If <see langword="true"/>, the frame cannot be modified.
    /// </summary>
    public bool IsReadOnly => !Convert.ToBoolean(ffmpeg.av_frame_is_writable(Frame));

    /// <summary>
    /// Allocates new buffers for the frame. This method is required before encoding or decoding if the frame does not have any buffer.
    /// If the frame already has a buffer, an exception will be thrown to prevent memory leakage.
    /// </summary>
    /// <param name="align">Specifies the alignment requirement for the buffer. Defaults to 1.</param>
    /// <returns>An <see cref="AVResult32"/> indicating the success or failure of the buffer allocation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the frame already has a buffer.</exception>
    public AVResult32 GetBuffer(int align = 1) => HasBuffer
            ? throw new InvalidOperationException("The frame already has a buffer, to avoid memory leakage, this operation is not permitted.")
            : (AVResult32)ffmpeg.av_frame_get_buffer(Frame, align);

    /// <summary>
    /// Unreferences the frame, releasing any associated resources but keeping the frame structure allocated.
    /// This effectively resets the frame for reuse.
    /// </summary>
    public void Unreference() => ffmpeg.av_frame_unref(Frame);

    public void Reference(AVFrame src)
    {
        Unreference();
        ((AVResult32)ffmpeg.av_frame_ref(Frame, src.Frame)).ThrowIfError();
    }

    /// <summary>
    /// Frees the frame by disposing of any allocated resources.
    /// </summary>
    public void Free() => Dispose();

    /// <summary>
    /// Gets the media type of the frame based on its dimensions and sample count.
    /// - If the frame has width and height set and no samples, it's video.
    /// - If the frame has samples and no dimensions, it's audio.
    /// - Otherwise, the type is unknown.
    /// </summary>
    public _AVMediaType MediaType => Width == 0 && Height == 0 && SampleCount > 0
                    ? _AVMediaType.AVMEDIA_TYPE_AUDIO
                    : Width > 0 && Height > 0 && SampleCount == 0
                    ? _AVMediaType.AVMEDIA_TYPE_VIDEO
                    : _AVMediaType.AVMEDIA_TYPE_UNKNOWN;

    /// <summary>
    /// Creates a clone of the current frame. The cloned frame is an exact duplicate but with separate memory allocation.
    /// </summary>
    /// <returns>A new <see cref="AVFrame"/> that is a clone of the current frame.</returns>
    public AVFrame Clone()
    {
        AutoGen._AVFrame* frame = ffmpeg.av_frame_clone(Frame);
        return new(frame);
    }

    /// <summary>
    /// Copies the current frame's data and properties to a new frame. 
    /// This includes copying the frame's format, dimensions, and any other associated properties.
    /// </summary>
    /// <returns>A new <see cref="AVFrame"/> with the same properties and data as the current frame.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an error occurs during the copy process.</exception>
    public AVFrame Copy()
    {
        AVFrame frame = Allocate();
        frame.Format = Format;
        frame.Width = Width;
        frame.Height = Height;
        frame.SampleCount = SampleCount;
        Exceptions.FFmpegException.ThrowIfError(ffmpeg.av_frame_copy_props(frame.Frame, Frame));
        frame.GetBuffer().ThrowIfError();
        Exceptions.FFmpegException.ThrowIfError(ffmpeg.av_frame_copy(frame.Frame, Frame));
        return frame;
    }

    public void CopyPropertiesTo(AVFrame dst) => ffmpeg.av_frame_copy_props(dst.Frame, Frame);

    #region IDisposable
    private bool disposedValue;

    /// <summary>
    /// Disposes of the frame, releasing both managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// A boolean indicating whether the method is called from <see cref="Dispose()"/> (true) or the finalizer (false).
    /// </param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Clean up managed resources here if necessary.
            }

            // Clean up unmanaged resources.
            AutoGen._AVFrame* frame = Frame;
            ffmpeg.av_frame_free(&frame);
            Frame = null;

            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer for the frame, ensures that unmanaged resources are cleaned up if <see cref="Dispose"/> was not called.
    /// </summary>
    ~AVFrame()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Releases the resources associated with the frame. 
    /// This method should be called when the frame is no longer needed to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees any side data associated with the frame, releasing the allocated memory.
    /// </summary>
    public void FreeSideData() => ffmpeg.av_frame_side_data_free(&Frame->side_data, &Frame->nb_side_data);
    #endregion

}
