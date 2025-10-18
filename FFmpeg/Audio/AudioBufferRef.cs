using FFmpeg.Utils;

namespace FFmpeg.Audio;

/// <summary>
/// Represents a reference to an audio buffer with various properties such as format, channels, alignment, and capacity.
/// Provides methods for managing audio data buffers in a low-level, unsafe context.
/// </summary>
internal readonly unsafe struct AudioBufferRef
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioBufferRef"/> struct.
    /// </summary>
    /// <param name="reference">A pointer to an <see cref="AutoGen._AVBufferRef"/> structure representing the buffer reference.</param>
    public AudioBufferRef(AutoGen._AVBufferRef* reference) => Reference = reference;

    /// <summary>
    /// Gets the underlying pointer to the <see cref="AutoGen._AVBufferRef"/> structure.
    /// This structure contains the buffer reference for the audio data.
    /// </summary>
    public AutoGen._AVBufferRef* Reference { get; }

    // Internal constants representing memory offsets for different audio buffer fields.
    private const int SAMPLE_FORMAT_OFFSET = 0;
    private const int CHANNELS_OFFSET = sizeof(SampleFormat);
    private const int ALIGNMENT_OFFSET = CHANNELS_OFFSET + sizeof(int);
    private const int SAMPLES_OFFSET = ALIGNMENT_OFFSET + sizeof(int);
    private const int CAPACITY_OFFSET = SAMPLES_OFFSET + sizeof(int);
    private const int LINES_SIZE_OFFSET = CAPACITY_OFFSET + sizeof(int);

    /// <summary>
    /// Gets the offset for the planes in the buffer, depending on whether the format is planar.
    /// </summary>
    private int PLANES_OFFSET => LINES_SIZE_OFFSET + (Format.IsPlanar() ? Channels * sizeof(int) : sizeof(int));

    /// <summary>
    /// Gets the offset for the actual audio data in the buffer, depending on the format and channels.
    /// </summary>
    private int DATA_OFFSET => LINES_SIZE_OFFSET + ((Format.IsPlanar() ? Channels : 1) * (sizeof(int) + sizeof(byte*)));

    /// <summary>
    /// Computes the data offset for the buffer based on the sample format and the number of channels.
    /// </summary>
    /// <param name="format">The sample format of the audio data.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <returns>The computed data offset in the buffer.</returns>
    public static int GET_DATA_OFFSET(SampleFormat format, int channels) => LINES_SIZE_OFFSET + ((format.IsPlanar() ? channels : 1) * (sizeof(int) + sizeof(byte*)));

    /// <summary>
    /// Gets or sets the sample format of the audio buffer.
    /// </summary>
    public SampleFormat Format
    {
        get => *(SampleFormat*)(Reference->data + SAMPLE_FORMAT_OFFSET);
        set => *(SampleFormat*)(Reference->data + SAMPLE_FORMAT_OFFSET) = value;
    }

    /// <summary>
    /// Gets or sets the number of audio channels in the buffer.
    /// </summary>
    public int Channels
    {
        get => *(int*)(Reference->data + CHANNELS_OFFSET);
        set => *(int*)(Reference->data + CHANNELS_OFFSET) = value;
    }

    /// <summary>
    /// Gets or sets the alignment value of the audio buffer.
    /// </summary>
    public int Alignment
    {
        get => *(int*)(Reference->data + ALIGNMENT_OFFSET);
        set => *(int*)(Reference->data + ALIGNMENT_OFFSET) = value;
    }

    /// <summary>
    /// Gets or sets an <see cref="AudioBufferInfo"/> object containing metadata for the audio buffer, 
    /// including sample format, channels, and alignment.
    /// </summary>
    public AudioBufferInfo Info
    {
        get => new(Format, Channels, Alignment);
        set
        {
            Format = value.Format;
            Channels = value.Channels;
            Alignment = value.Alignment;
        }
    }

    /// <summary>
    /// Gets or sets the number of audio samples currently stored in the buffer.
    /// </summary>
    public int Samples
    {
        get => *(int*)(Reference->data + SAMPLES_OFFSET);
        set => *(int*)(Reference->data + SAMPLES_OFFSET) = value;
    }

    /// <summary>
    /// Gets or sets the maximum capacity of the buffer in terms of samples.
    /// </summary>
    public int Capacity
    {
        get => *(int*)(Reference->data + CAPACITY_OFFSET);
        set => *(int*)(Reference->data + CAPACITY_OFFSET) = value;
    }

    /// <summary>
    /// Gets a pointer to the array of line sizes, which represent the size of each channel's data in the buffer.
    /// </summary>
    public int* LineSizes => (int*)(Reference->data + LINES_SIZE_OFFSET);

    /// <summary>
    /// Gets a pointer to the planes in the audio buffer if the format is planar.
    /// Each plane corresponds to a channel in the audio data.
    /// </summary>
    public byte** Planes => (byte**)(Reference->data + PLANES_OFFSET);

    /// <summary>
    /// Gets a pointer to the raw audio data buffer.
    /// </summary>
    public byte* Buffer => Reference->data + DATA_OFFSET;

    /// <summary>
    /// Allocates a new <see cref="AudioBufferRef"/> for the specified format, channels, alignment, and capacity.
    /// </summary>
    /// <param name="format">The sample format of the audio buffer.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <param name="alignment">The alignment requirement for the buffer.</param>
    /// <param name="capacity">The capacity of the buffer in samples.</param>
    /// <returns>A newly allocated <see cref="AudioBufferRef"/>.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if the buffer could not be allocated.</exception>
    public static AudioBufferRef Allocate(SampleFormat format, int channels, int alignment, int capacity)
    {
        int dataOffset = GET_DATA_OFFSET(format, channels);
        AVResult32 size = ffmpeg.av_samples_get_buffer_size(null, channels, capacity, (AutoGen._AVSampleFormat)format, alignment);
        size.ThrowIfError();

        AutoGen._AVBufferRef* buffer = ffmpeg.av_buffer_alloc((ulong)(size + dataOffset));
        if (buffer == null)
            throw new OutOfMemoryException("The buffer could not be allocated");

        AudioBufferRef abuffer = new(buffer)
        {
            Format = format,
            Channels = channels,
            Alignment = alignment,
            Capacity = capacity,
            Samples = 0,
        };

        var res = ffmpeg.av_samples_fill_arrays(abuffer.Planes, abuffer.LineSizes, abuffer.Buffer, channels, capacity, (AutoGen._AVSampleFormat)format, alignment);
        if (res < 0)
        {
            ffmpeg.av_buffer_unref(&buffer);
            FFmpeg.Exceptions.FFmpegException.ThrowIfError(res);
        }

        return abuffer;
    }
}

