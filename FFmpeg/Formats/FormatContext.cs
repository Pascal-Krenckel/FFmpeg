using FFmpeg.Collections;
using FFmpeg.IO;
using FFmpeg.Unsafe;
using FFmpeg.Utils;
using System.Runtime.InteropServices;

namespace FFmpeg.Formats;

/// <remarks>
/// <see cref="FormatContext"/> provides the common functionality shared by
/// demuxing and muxing format contexts. It wraps FFmpeg's native
/// <see cref="AutoGen._AVFormatContext"/> structure and exposes a managed,
/// object-oriented API for working with media containers, streams, metadata,
/// and format-specific options.
///
/// <para>
/// As a wrapper around unmanaged resources, this class implements
/// <see cref="IDisposable"/>. Instances should be disposed when no longer
/// needed to release the underlying FFmpeg resources.
/// </para>
///
/// <para>
/// Because this class derives from
/// <see cref="Options.OptionQueryableBase"/>, any FFmpeg options supported by
/// <see cref="AutoGen._AVFormatContext"/> can be queried and modified through
/// the managed options API.
/// </para>
/// </remarks>
public abstract unsafe class FormatContext : Options.OptionQueryableBase, IDisposable, IAVPointer<AutoGen._AVFormatContext>
{
    internal AutoGen._AVFormatContext* Context { get; set; }
    AutoGen._AVFormatContext* IAVPointer<AutoGen._AVFormatContext>.Pointer => Context;

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
    /// Initializes a new <see cref="FormatContext"/> from an existing native
    /// <see cref="AutoGen._AVFormatContext"/>.
    /// </summary>
    /// <param name="context">
    /// The native format context to wrap.
    /// </param>
    protected FormatContext(AutoGen._AVFormatContext* context) => Context = context;

    #endregion

    #region Streams

    /// <summary>
    /// Cached managed wrappers for the native streams.
    /// </summary>
    private AVStream[] streams = [];

    /// <summary>
    /// Cached managed wrappers for the native streams.
    /// </summary>
    /// <remarks>
    /// The collection is synchronized with the underlying native stream array on
    /// demand. If FFmpeg adds or replaces streams, the cached managed wrappers are
    /// recreated automatically.
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
    /// Synchronizes the cached managed stream wrappers with the native stream array.
    /// </summary>
    /// <remarks>
    /// Existing wrapper instances are reused whenever possible. New wrappers are
    /// created only when the underlying native stream pointers change or the number
    /// of streams differs.
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
    /// Determines whether the cached stream wrappers still match the current native
    /// stream array.
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
    /// <summary>
    /// Gets the chapters contained in the media.
    /// </summary>
    /// <remarks>
    /// The returned collection provides access to the chapter entries stored in the
    /// underlying format context.
    /// </remarks>
    public virtual ChapterList Chapters => new(this, false);

    #region SetIOContext
    /// <summary>
    /// Associates a custom <see cref="IOContext"/> with this format context.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IOContext"/> to associate with the current format context.
    /// </param>
    /// <param name="options">
    /// The <see cref="IOOptions"/> specifying the operations (e.g., read or write) to be used with the context.
    /// </param>
    /// <param name="bufferSize">
    /// The size of the buffer to use for the I/O operations. Default is 32 KB.
    /// </param>
    /// <remarks>
    /// Calling this method replaces any previously associated
    /// <see cref="IOContext"/>. The supplied context is initialized and attached to
    /// this format context.
    /// </remarks>
    public void SetContext(IOContext context, IOOptions options, int bufferSize = 32768) => context.InitContext(this, options, bufferSize);

    /// <summary>
    /// Gets the types of seeking supported by the underlying I/O context.
    /// </summary>
    /// <remarks>
    /// The value indicates which seeking operations are supported by the underlying
    /// <see cref="AVIOContext"/>. Derived classes can modify this value when creating
    /// or configuring a custom I/O context.
    /// </remarks>
    public Seekable Seekable
    {
        get
        {
#if NET6_0_OR_GREATER
            return (Seekable)Context->pb->seekable;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return (Seekable)((FFmpeg.AutoGen._AVIOContext_win*)Context->pb)->seekable;
            else
                return (Seekable)Context->pb->seekable;
#endif
        }
    }
    #endregion

    /// <summary>
    /// Gets the URL or filename associated with the format context.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> if no URL has been assigned.
    /// </remarks>
    public string? Url => Context->url != null ? Marshal.PtrToStringAnsi((IntPtr)Context->url) : null;

    /// <summary>
    /// Gets the metadata associated with the format context.
    /// </summary>
    public AVDictionary_ref Metadata => new(&Context->metadata, true, false);
    /// <summary>
    /// Gets the wall-clock time at which the media stream started.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> if the information is unavailable.
    /// </remarks>
    public DateTime? StartTimeRealTime => Context->start_time_realtime != ffmpeg.AV_NOPTS_VALUE ? DateTime.UnixEpoch.AddTicks(10 * Context->start_time_realtime) : null;

    #region IDisposable

    /// <summary>
    /// Indicates whether the instance is disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Releases the resources used by the current format context.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is being called from the <see cref="Dispose()"/> method or a finalizer.</param>
    protected virtual void Dispose(bool disposing) => IsDisposed = true;


    /// <summary>
    /// Finalizes the <see cref="FormatContext"/>.
    /// </summary>
    ~FormatContext()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Releases the resources associated with this format context.
    /// </summary>
    /// <remarks>
    /// Equivalent to calling <see cref="Dispose()"/>.
    /// </remarks>
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


    /// <summary>
    /// Retrieves the most recently output timestamp for a stream.
    /// </summary>
    /// <param name="streamIndex">
    /// The index of the output stream.
    /// </param>
    /// <param name="timestamp">
    /// Receives the output timestamp converted to the stream's time base.
    /// </param>
    /// <param name="wallTime">
    /// Receives the corresponding wall-clock timestamp.
    /// </param>
    /// <returns>
    /// The result returned by <c>av_get_output_timestamp()</c>.
    /// </returns>
    /// <remarks>
    /// This method is primarily intended for monitoring the progress of output
    /// devices or muxers that support timestamp reporting.
    /// </remarks>
    public AVResult32 GetOutputTimestamp(int streamIndex, out TimeSpan timestamp, out TimeSpan wallTime)
    {
        Rational timeBase = Streams[streamIndex].TimeBase;
        long dts, wall;
        int res = ffmpeg.av_get_output_timestamp(Context, streamIndex, &dts, &wall);
        timestamp = dts * timeBase;
        wallTime = TimeSpan.FromTicks(10L * wall);
        return res;
    }

    #region FindBestStream


    /// <summary>
    /// Finds the most appropriate stream of the specified media type.
    /// </summary>
    /// <param name="type">
    /// The media type to search for.
    /// </param>
    /// <returns>
    /// The zero-based stream index if a suitable stream is found; otherwise a
    /// negative FFmpeg error code.
    /// </returns>
    /// <remarks>
    /// This method calls FFmpeg's <c>av_find_best_stream()</c> using the default
    /// stream selection behavior.
    /// </remarks>
    public int FindBestStream(MediaType type)
    => ffmpeg.av_find_best_stream(Context, (AutoGen._AVMediaType)type, -1, -1, null, 0);

    #endregion
}
