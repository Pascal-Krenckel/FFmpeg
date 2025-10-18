using FFmpeg.Audio;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Filters;
/// <summary>
/// Represents the parameters used for configuring a buffer source in FFmpeg. 
/// Provides mechanisms to allocate, manage, and dispose of buffer source parameters.
/// </summary>
public sealed unsafe class BufferSrcParameters : IDisposable
{
    /// <summary>
    /// Pointer to the unmanaged <see cref="AutoGen._AVBufferSrcParameters"/> structure used by FFmpeg.
    /// </summary>
    internal AutoGen._AVBufferSrcParameters* parameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferSrcParameters"/> class with the given FFmpeg buffer source parameters.
    /// </summary>
    /// <param name="parameters">Pointer to the FFmpeg buffer source parameters.</param>
    internal BufferSrcParameters(AutoGen._AVBufferSrcParameters* parameters) => this.parameters = parameters;

    /// <summary>
    /// Allocates memory for the buffer source parameters using FFmpeg's <c>av_buffersrc_parameters_alloc</c> function.
    /// </summary>
    /// <returns>A new instance of <see cref="BufferSrcParameters"/> if memory allocation succeeds.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if memory allocation fails.</exception>
    public static BufferSrcParameters Allocate()
    {
        AutoGen._AVBufferSrcParameters* p = ffmpeg.av_buffersrc_parameters_alloc();
        return p == null ? throw new OutOfMemoryException() : new(p);
    }

    /// <summary>
    /// Gets or sets the pixel format used for the buffer source.
    /// </summary>
    public PixelFormat PixelFormat { get => (PixelFormat)parameters->format; set => parameters->format = (int)value; }

    /// <summary>
    /// Gets or sets the sample format used for the buffer source.
    /// </summary>
    public SampleFormat SampleFormat { get => (SampleFormat)parameters->format; set => parameters->format = (int)value; }

    /// <summary>
    /// Gets or sets the time base for the buffer source.
    /// </summary>
    public Rational TimeBase { get => parameters->time_base; set => parameters->time_base = value; }

    /// <summary>
    /// Gets or sets the width of the buffer source.
    /// </summary>
    public int Width { get => parameters->width; set => parameters->width = value; }

    /// <summary>
    /// Gets or sets the height of the buffer source.
    /// </summary>
    public int Height { get => parameters->height; set => parameters->height = value; }

    /// <summary>
    /// Gets or sets the sample aspect ratio of the buffer source.
    /// </summary>
    public Rational SampleAspectRatio { get => parameters->sample_aspect_ratio; set => parameters->sample_aspect_ratio = value; }

    /// <summary>
    /// Gets or sets the frame rate of the buffer source.
    /// </summary>
    public Rational FrameRate { get => parameters->frame_rate; set => parameters->frame_rate = value; }

    /// <summary>
    /// Gets or sets the sample rate of the buffer source.
    /// </summary>
    public int SampleRate { get => parameters->sample_rate; set => parameters->sample_rate = value; }

    /// <summary>
    /// Gets the channel layout for the buffer source.
    /// </summary>
    public ChannelLayout_ref ChannelLayout => new(&parameters->ch_layout, false);

    private bool disposedValue;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="BufferSrcParameters"/> class and optionally releases managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: Cleanup managed objects if needed
            }

            // Free unmanaged memory
            ffmpeg.av_free(parameters);
            parameters = null;

            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer for the <see cref="BufferSrcParameters"/> class.
    /// Ensures that unmanaged resources are freed if <see cref="Dispose"/> is not called.
    /// </summary>
    ~BufferSrcParameters()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes of the resources (other than memory) used by the <see cref="BufferSrcParameters"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets or sets the color space of the buffer source.
    /// </summary>
    public ColorSpace ColorSpace { get => (ColorSpace)parameters->color_space; set => parameters->color_space = (AutoGen._AVColorSpace)value; }

    /// <summary>
    /// Gets or sets the color range of the buffer source.
    /// </summary>
    public ColorRange ColorRange { get => (ColorRange)parameters->color_range; set => parameters->color_range = (AutoGen._AVColorRange)value; }
}

