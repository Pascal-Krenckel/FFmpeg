using FFmpeg.Collections;
using FFmpeg.IO;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Formats;

/// <summary>
/// Provides a managed wrapper around the native <see cref="AutoGen._AVFormatContext"/> structure from the FFmpeg library.
/// </summary>
/// <remarks>
/// The <see cref="FormatContext"/> class is a critical component in handling media streams with FFmpeg. It encapsulates the context used for handling input and output formats, including parsing and managing media files or streams.
///
/// This class provides a managed interface to the native <see cref="AutoGen._AVFormatContext"/> pointer, offering functionalities for opening media streams, querying and modifying format options, and interacting with other FFmpeg components.
///
/// <para>
/// As a wrapper around a native pointer, this class implements <see cref="IDisposable"/> to ensure proper cleanup of unmanaged resources. You should always dispose of instances of this class when they are no longer needed to avoid memory leaks and potential crashes.
/// </para>
///
/// <para>
/// The class also inherits from <see cref="Options.OptionQueryBase"/>, enabling access to FFmpeg's options querying and setting capabilities. This allows for configuring various aspects of media handling by setting or querying options on the format context.
/// </para>
/// </remarks>
public unsafe class FormatContext : Options.OptionQueryBase, IDisposable
{
    public AutoGen._AVFormatContext* Context { get; protected set; }

    internal AVIOContext? ioContext;


    /// <summary>
    /// Gets the flags associated with this format context.
    /// </summary>
    public FormatContextFlags Flags => (FormatContextFlags)Context->flags;

    /// <summary>
    /// Gets the number of streams in this format context.
    /// </summary>
    public int StreamCount => (int)Context->nb_streams;

    /// <inheritdoc/>
    protected override unsafe void* Pointer => Context;

    #region Constructions

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatContext"/> class with an existing <see cref="AutoGen._AVFormatContext"/>*.
    /// </summary>
    /// <param name="context">The already allocated context.</param>
    /// <param name="freeOnDispose">Indicates whether the underlying <see cref="AutoGen._AVFormatContext"/>* should be freed when this object is disposed.</param>
    protected FormatContext(AutoGen._AVFormatContext* context)
    {
        Context = context;
    }

    #endregion

    #region Streams

    /// <summary>
    /// The array of streams associated with this format context.
    /// </summary>
    private AVStream[] streams = [];

    /// <summary>
    /// Gets the collection of streams in this format context.
    /// </summary>
    /// <remarks>
    /// This property returns a read-only list of streams. The list is updated if the underlying stream array does not match the current number of streams in the context.
    /// </remarks>
    public IReadOnlyList<AVStream> Streams
    {
        get
        {
            if (!CompareStreams())
                UpdateStreamArray();
            return streams;
        }
    }

    /// <summary>
    /// Updates the internal stream array to reflect the current streams in the context.
    /// </summary>
    /// <remarks>
    /// This method initializes or refreshes the array of streams to match the number of streams in the format context. It should be called when the stream count has changed.
    /// </remarks>
    private void UpdateStreamArray()
    {
            AVStream[] streams = this.streams;
            if (streams.Length != StreamCount)
                streams = new AVStream[StreamCount];
            for (int i = 0; i < streams.Length; i++)
                streams[i] = new AVStream(Context->streams[i]);
            this.streams = streams;

        }

    /// <summary>
    /// Compares the internal stream array with the current streams in the format context.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the internal array matches the current streams, otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks if the internal stream array is in sync with the actual streams in the format context by comparing the pointers.
    /// </remarks>
    private bool CompareStreams()
    {
        if (streams.Length != StreamCount)
            return false;
        for (int i = 0; i < streams.Length; i++)
        {
            if (streams[i].stream != Context->streams[i])
                return false;
        }

        return true;
    }



    #endregion

    public virtual ChapterList Chapters => new ChapterList(this, false);

    #region SetIOContext
    /// <summary>
    /// Initializes the current <see cref="FormatContext"/> with the provided <see cref="IOContext"/>.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IOContext"/> to associate with the current format context.
    /// </param>
    /// <param name="options">
    /// The <see cref="IOOptions"/> specifying the operations (e.g., read or write) to be used with the context.
    /// </param>
    /// <param name="buffer_size">
    /// The size of the buffer to use for the I/O operations. Default is 32 KB.
    /// </param>
    /// <remarks>
    /// This method is typically used to set up a custom I/O context for input or output operations in FFmpeg.
    /// It wraps the initialization of the I/O context by calling <see cref="IOContext.InitContext"/> internally.
    /// </remarks>
    public void SetContext(IOContext context, IOOptions options, int buffer_size = 32768) => context.InitContext(this, options, buffer_size);
    #endregion


    public string? Url => Context->url != null ? Marshal.PtrToStringAnsi((IntPtr)Context->url) : null;

    public AVDictionary_ref Metadata => new(&Context->metadata, true, false);

    public DateTime? StartTimeRealTime => Context->start_time_realtime != ffmpeg.AV_NOPTS_VALUE ? DateTime.UnixEpoch.AddMilliseconds(Context->start_time_realtime) : null;

    #region IDisposable

    private bool disposedValue;
    public bool IsDisposed => disposedValue;

    /// <summary>
    /// Disposes of the resources used by the <see cref="FormatContext"/>.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is being called from the <see cref="Dispose"/> method or a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
                ioContext?.Dispose();
           if(Context != null)
            {
                var context = Context;
                ffmpeg.avformat_close_input(&context);
                Context = context;
            }
            disposedValue = true;
        }
    }



    /// <summary>
    /// Finalizes the <see cref="FormatContext"/>.
    /// </summary>
    ~FormatContext()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="FormatContext"/>.
    /// </summary>
    public void Free()
        => Dispose();

    /// <summary>
    /// Disposes of the resources used by the <see cref="FormatContext"/> and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion


    // ToDo: -40 if not supported
    public AVResult32 GetOutputTimestamp(int streamIndex, out TimeSpan timestamp, out TimeSpan wallTime)
    {
        Rational timeBase = Streams[streamIndex].TimeBase;
        long dts, wall;
        int res = ffmpeg.av_get_output_timestamp(Context, streamIndex, &dts, &wall);
        timestamp = dts * timeBase;
        wallTime = TimeSpan.FromMilliseconds(wall);
        return res;
    }

    #region FindBestStream

    /// <summary>
    /// Finds the best stream of a given media type in the media file.
    /// </summary>
    /// <param name="type">The media type to search for.</param>
    /// <returns>The index of the best stream, or a negative value if no suitable stream is found.</returns>
    public int FindBestStream(MediaType type)
        => ffmpeg.av_find_best_stream(Context, (AutoGen._AVMediaType)type, -1, -1, null, 0);

    #endregion
}
