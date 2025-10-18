using FFmpeg.Exceptions;

namespace FFmpeg.Utils;
/// <summary>
/// Represents a 32-bit integer result from an AV operation.
/// </summary>
public readonly struct AVResult32 : IEquatable<AVResult32>, IEquatable<int>
{
    private readonly int value;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVResult32"/> class with a value of 0.
    /// </summary>
    public AVResult32() => value = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVResult32"/> class with the specified value.
    /// </summary>
    /// <param name="value">The 32-bit integer value to initialize with.</param>
    public AVResult32(int value) => this.value = value;

    /// <summary>
    /// Implicitly converts an <see cref="AVResult32"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="result">The <see cref="AVResult32"/> instance to convert.</param>
    /// <returns>The underlying integer value of the <see cref="AVResult32"/> instance.</returns>
    public static implicit operator int(AVResult32 result) => result.value;

    /// <summary>
    /// Implicitly converts an <see cref="AVResult32"/> to an <see cref="long"/>.
    /// </summary>
    /// <param name="result">The <see cref="AVResult32"/> instance to convert.</param>
    /// <returns>The underlying int value of the <see cref="AVResult32"/> instance.</returns>
    public static implicit operator long(AVResult32 result) => result.value;

    /// <summary>
    /// Explicitly converts an <see cref="AVResult32"/> to an <see cref="uint"/>.
    /// </summary>
    /// <param name="result">The <see cref="AVResult32"/> instance to convert.</param>
    /// <returns>The underlying integer value of the <see cref="AVResult32"/> instance, cast to uint.</returns>
    public static explicit operator uint(AVResult32 result) => (uint)result.value;

    /// <summary>
    /// Explicitly converts an <see cref="AVResult32"/> to an <see cref="ulong"/>.
    /// </summary>
    /// <param name="result">The <see cref="AVResult32"/> instance to convert.</param>
    /// <returns>The underlying integer value of the <see cref="AVResult32"/> instance, cast to ulong.</returns>
    public static explicit operator ulong(AVResult32 result) => (ulong)result.value;

    /// <summary>
    /// Implicitly converts an <see cref="int"/> to an <see cref="AVResult32"/>.
    /// </summary>
    /// <param name="result">The integer value to convert.</param>
    /// <returns>A new <see cref="AVResult32"/> instance initialized with the specified value.</returns>
    public static implicit operator AVResult32(int result) => new(result);

    /// <summary>
    /// Explicitly converts an <see cref="uint"/> to an <see cref="AVResult32"/>.
    /// </summary>
    /// <param name="result">The unsigned integer value to convert.</param>
    /// <returns>A new <see cref="AVResult32"/> instance initialized with the specified value.</returns>
    public static implicit operator AVResult32(uint result) => new((int)result);

    /// <summary>
    /// Explicitly converts an <see cref="long"/> to an <see cref="AVResult32"/>.
    /// </summary>
    /// <param name="result">The long value to convert.</param>
    /// <returns>A new <see cref="AVResult32"/> instance initialized with the specified value.</returns>
    public static implicit operator AVResult32(long result) => new((int)result);


    /// <summary>
    /// Explicitly converts an <see cref="ulong"/> to an <see cref="AVResult32"/>.
    /// </summary>
    /// <param name="result">The unsigned long value to convert.</param>
    /// <returns>A new <see cref="AVResult32"/> instance initialized with the specified value.</returns>
    public static implicit operator AVResult32(ulong result) => new((int)result);

    /// <summary>
    /// Determines whether two <see cref="AVResult32"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="AVResult32"/> instance to compare.</param>
    /// <param name="right">The second <see cref="AVResult32"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(AVResult32 left, AVResult32 right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="AVResult32"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="AVResult32"/> instance to compare.</param>
    /// <param name="right">The second <see cref="AVResult32"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(AVResult32 left, AVResult32 right) => !(left == right);

    /// <summary>
    /// Gets a value indicating whether the result represents an error.
    /// </summary>
    /// <value><c>true</c> if the result is less than 0 or (EAGAIN); otherwise, <c>false</c>.</value>
    public bool IsError => value < 0;

    /// <summary>
    /// Throws an exception if the result represents an error.
    /// </summary>
    /// <exception cref="FFmpegException">Thrown when the result represents an error.</exception>
    public void ThrowIfError() => FFmpegException.ThrowIfError(value);


    #region GNU Error Codes

    /// <summary>Operation not permitted (EPERM)</summary>
    public static int OperationNotPermitted => -1;

    /// <summary>No such file or directory (ENOENT)</summary>
    public static int NoSuchFileOrDirectory => -2;

    /// <summary>No such process (ESRCH)</summary>
    public static int NoSuchProcess => -3;

    /// <summary>Interrupted system call (EINTR)</summary>
    public static int InterruptedSystemCall => -4;

    /// <summary>I/O error (EIO)</summary>
    public static int IOError => -5;

    /// <summary>No such device or address (ENXIO)</summary>
    public static int NoSuchDeviceOrAddress => -6;

    /// <summary>Argument list too long (E2BIG)</summary>
    public static int ArgumentListTooLong => -7;

    /// <summary>Exec format error (ENOEXEC)</summary>
    public static int ExecFormatError => -8;

    /// <summary>Bad file descriptor (EBADF)</summary>
    public static int BadFileDescriptor => -9;

    /// <summary>No child processes (ECHILD)</summary>
    public static int NoChildProcesses => -10;

    /// <summary>Try again (EAGAIN)</summary>
    public static int TryAgain => -11;

    /// <summary>Out of memory (ENOMEM)</summary>
    public static int OutOfMemory => -12;

    /// <summary>Permission denied (EACCES)</summary>
    public static int PermissionDenied => -13;

    /// <summary>Bad address (EFAULT)</summary>
    public static int BadAddress => -14;

    /// <summary>Block device required (ENOTBLK)</summary>
    public static int BlockDeviceRequired => -15;

    /// <summary>Device or resource busy (EBUSY)</summary>
    public static int DeviceOrResourceBusy => -16;

    /// <summary>File exists (EEXIST)</summary>
    public static int FileExists => -17;

    /// <summary>Cross-device link (EXDEV)</summary>
    public static int CrossDeviceLink => -18;

    /// <summary>No such device (ENODEV)</summary>
    public static int NoSuchDevice => -19;

    /// <summary>Not a directory (ENOTDIR)</summary>
    public static int NotADirectory => -20;

    /// <summary>Is a directory (EISDIR)</summary>
    public static int IsADirectory => -21;

    /// <summary>Invalid argument (EINVAL)</summary>
    public static int InvalidArgument => -22;

    /// <summary>Too many open files in system (ENFILE)</summary>
    public static int TooManyOpenFilesInSystem => -23;

    /// <summary>Too many open files (EMFILE)</summary>
    public static int TooManyOpenFiles => -24;

    /// <summary>Not a typewriter (ENOTTY)</summary>
    public static int NotATypewriter => -25;

    /// <summary>Text file busy (ETXTBSY)</summary>
    public static int TextFileBusy => -26;

    /// <summary>File too large (EFBIG)</summary>
    public static int FileTooLarge => -27;

    /// <summary>No space left on device (ENOSPC)</summary>
    public static int NoSpaceLeftOnDevice => -28;

    /// <summary>Illegal seek (ESPIPE)</summary>
    public static int IllegalSeek => -29;

    /// <summary>Read-only file system (EROFS)</summary>
    public static int ReadOnlyFileSystem => -30;

    /// <summary>Too many links (EMLINK)</summary>
    public static int TooManyLinks => -31;

    /// <summary>Broken pipe (EPIPE)</summary>
    public static int BrokenPipe => -32;


    /// <summary>The value was out of range (ERANGE)</summary>
    public static int OutOfRange => -34;
    #endregion

    #region FFmpeg Error Codes
    /// <summary>
    /// Bitstream filter not found. 
    /// </summary>
    public static int BitstreamFilterNotFound => ffmpeg.AVERROR_BSF_NOT_FOUND;

    /// <summary>
    /// Internal bug, also see <see cref="Bug2"/> 
    /// </summary>
    public static int Bug => ffmpeg.AVERROR_BUG;

    /// <summary>
    /// This is semantically identical to <see cref="Bug"/> it has been introduced in Libav after our AVERROR_BUG and with a modified value. 
    /// </summary>
    public static int Bug2 => ffmpeg.AVERROR_BUG2;

    /// <summary>
    /// Buffer too small. 
    /// </summary>
    public static int BufferTooSmall => ffmpeg.AVERROR_BUFFER_TOO_SMALL;

    /// <summary>
    /// Decoder not found. 
    /// </summary>
    public static int DecoderNotFound => ffmpeg.AVERROR_DECODER_NOT_FOUND;

    /// <summary>
    /// Demuxer not found. 
    /// </summary>
    public static int DemuxerNotFound => ffmpeg.AVERROR_DEMUXER_NOT_FOUND;

    /// <summary>
    /// Encoder not found. 
    /// </summary>
    public static int EncoderNotFound => ffmpeg.AVERROR_ENCODER_NOT_FOUND;

    /// <summary>
    /// End of file. 
    /// </summary>
    public static int EndOfFile => ffmpeg.AVERROR_EOF;

    /// <summary>
    /// Immediate exit was requested; the called function should not be restarted. 
    /// </summary>
    public static int Exit => ffmpeg.AVERROR_EXIT;

    /// <summary>
    /// Generic error in an external library. 
    /// </summary>
    public static int ExternalError => ffmpeg.AVERROR_EXTERNAL;

    /// <summary>
    /// Filter not found. 
    /// </summary>
    public static int FilterNotFound => ffmpeg.AVERROR_FILTER_NOT_FOUND;

    /// <summary>
    /// Invalid data found when processing input. 
    /// </summary>
    public static int InvalidData => ffmpeg.AVERROR_INVALIDDATA;

    /// <summary>
    /// Muxer not found. 
    /// </summary>
    public static int MuxerNotFound => ffmpeg.AVERROR_MUXER_NOT_FOUND;

    /// <summary>
    /// Option not found. 
    /// </summary>
    public static int OptionNotFound => ffmpeg.AVERROR_OPTION_NOT_FOUND;

    /// <summary>
    /// Not yet implemented in FFmpeg, patches welcome. 
    /// </summary>
    public static int PatchWelcome => ffmpeg.AVERROR_PATCHWELCOME;

    /// <summary>
    /// Protocol not found. 
    /// </summary>
    public static int ProtocolNotFound => ffmpeg.AVERROR_PROTOCOL_NOT_FOUND;

    /// <summary>
    /// Stream not found. 
    /// </summary>
    public static int StreamNotFound => ffmpeg.AVERROR_STREAM_NOT_FOUND;

    /// <summary>
    /// Unknown error, typically from an external library. 
    /// </summary>
    public static int UnknownError => ffmpeg.AVERROR_UNKNOWN;

    /// <summary>
    /// Requested feature is flagged experimental. Notify strict_std_compliance if you really want to use it. 
    /// </summary>
    public static int ExperimentalFeature => ffmpeg.AVERROR_EXPERIMENTAL;

    /// <summary>
    /// Input changed between calls. Reconfiguration is required. (can be OR-ed with <see cref="OutputChanged"/>) 
    /// </summary>
    public static int InputChanged => ffmpeg.AVERROR_INPUT_CHANGED;

    /// <summary>
    /// Output changed between calls. Reconfiguration is required. (can be OR-ed with <see cref="InputChanged"/>) 
    /// </summary>
    public static int OutputChanged => ffmpeg.AVERROR_OUTPUT_CHANGED;

    /// <summary>
    /// HTTP 400 Bad Request. 
    /// </summary>
    public static int HttpBadRequest => ffmpeg.AVERROR_HTTP_BAD_REQUEST;

    /// <summary>
    /// HTTP 401 Unauthorized. 
    /// </summary>
    public static int HttpUnauthorized => ffmpeg.AVERROR_HTTP_UNAUTHORIZED;

    /// <summary>
    /// HTTP 403 Forbidden. 
    /// </summary>
    public static int HttpForbidden => ffmpeg.AVERROR_HTTP_FORBIDDEN;

    /// <summary>
    /// HTTP 404 Not Found. 
    /// </summary>
    public static int HttpNotFound => ffmpeg.AVERROR_HTTP_NOT_FOUND;

    /// <summary>
    /// HTTP 429 Too Many Requests. 
    /// </summary>
    public static int HttpTooManyRequests => ffmpeg.AVERROR_HTTP_TOO_MANY_REQUESTS;

    /// <summary>
    /// HTTP other 4xx errors. 
    /// </summary>
    public static int HttpOther4xx => ffmpeg.AVERROR_HTTP_OTHER_4XX;

    /// <summary>
    /// HTTP server error (5xx). 
    /// </summary>
    public static int HttpServerError => ffmpeg.AVERROR_HTTP_SERVER_ERROR;

    /// <summary>
    /// True if, <c><see langword="this"/> == <see cref="AVResult32.TryAgain"/></c>
    /// </summary>
    public bool IsTryAgain => this == AVResult32.TryAgain;

    #endregion

    #region Methods
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AVResult32 result && Equals(result);

    /// <inheritdoc />
    public bool Equals(AVResult32 other) => value == other.value;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(value);

    /// <inheritdoc />
    bool IEquatable<int>.Equals(int other) => value.Equals(other);

    /// <inheritdoc cref="int.ToString()"/>
    public override string ToString()
    {
        if (!IsError)
            return value.ToString();
        int v = value;
        string str =
        typeof(AVResult32).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .Where(p => p.CanRead && !p.IsSpecialName && (p.PropertyType == typeof(int) || p.PropertyType == typeof(AVResult32)))
            .Where(p => (int)p.GetValue(null) == v)
            .Select(p => p.Name).FirstOrDefault();
        return string.IsNullOrEmpty(str) ? value.ToString() : str;
    }
    #endregion
}

/// <summary>
/// Represents a 64-bit integer result from an AV operation.
/// </summary>
public readonly struct AVResult64 : IEquatable<AVResult64>, IEquatable<long>
{
    private readonly long value;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVResult64"/> struct with a value of 0.
    /// </summary>
    public AVResult64() => value = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVResult64"/> struct with the specified value.
    /// </summary>
    /// <param name="value">The 64-bit integer value to initialize with.</param>
    public AVResult64(long value) => this.value = value;

    /// <summary>
    /// Implicitly converts an <see cref="AVResult64"/> to a <see cref="long"/>.
    /// </summary>
    /// <param name="result">The <see cref="AVResult64"/> instance to convert.</param>
    /// <returns>The underlying long value of the <see cref="AVResult64"/> instance.</returns>
    public static implicit operator long(AVResult64 result) => result.value;

    /// <summary>
    /// Implicitly converts a <see cref="long"/> to an <see cref="AVResult64"/>.
    /// </summary>
    /// <param name="result">The long value to convert.</param>
    /// <returns>A new <see cref="AVResult64"/> instance initialized with the specified value.</returns>
    public static implicit operator AVResult64(long result) => new(result);

    /// <summary>
    /// Explicitly converts an <see cref="AVResult64"/> to a <see cref="ulong"/>.
    /// </summary>
    /// <param name="result">The <see cref="AVResult64"/> instance to convert.</param>
    /// <returns>The underlying <see cref="ulong"/> value of the <see cref="AVResult64"/> instance.</returns>
    public static explicit operator ulong(AVResult64 result) => (ulong)result.value;

    /// <summary>
    /// Explicitly converts a <see cref="ulong"/> to an <see cref="AVResult64"/>.
    /// </summary>
    /// <param name="result">The <see cref="ulong"/> value to convert.</param>
    /// <returns>A new <see cref="AVResult64"/> instance initialized with the specified value.</returns>
    public static explicit operator AVResult64(ulong result) => new((long)result);


    /// <summary>
    /// Determines whether two <see cref="AVResult64"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="AVResult64"/> instance to compare.</param>
    /// <param name="right">The second <see cref="AVResult64"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(AVResult64 left, AVResult64 right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="AVResult64"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="AVResult64"/> instance to compare.</param>
    /// <param name="right">The second <see cref="AVResult64"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(AVResult64 left, AVResult64 right) => !(left == right);

    /// <summary>
    /// Determines whether an <see cref="AVResult64"/> instance is equal to a long value.
    /// </summary>
    /// <param name="left">The <see cref="AVResult64"/> instance to compare.</param>
    /// <param name="right">The long value to compare.</param>
    /// <returns><c>true</c> if the instance's value is equal to the long value; otherwise, <c>false</c>.</returns>
    public static bool operator ==(AVResult64 left, long right) => left.Equals(right);

    /// <summary>
    /// Determines whether an <see cref="AVResult64"/> instance is not equal to a long value.
    /// </summary>
    /// <param name="left">The <see cref="AVResult64"/> instance to compare.</param>
    /// <param name="right">The long value to compare.</param>
    /// <returns><c>true</c> if the instance's value is not equal to the long value; otherwise, <c>false</c>.</returns>
    public static bool operator !=(AVResult64 left, long right) => !(left == right);

    /// <summary>
    /// Determines whether a long value is equal to an <see cref="AVResult64"/> instance.
    /// </summary>
    /// <param name="left">The long value to compare.</param>
    /// <param name="right">The <see cref="AVResult64"/> instance to compare.</param>
    /// <returns><c>true</c> if the long value is equal to the instance's value; otherwise, <c>false</c>.</returns>
    public static bool operator ==(long left, AVResult64 right) => right.Equals(left);

    /// <summary>
    /// Determines whether a long value is not equal to an <see cref="AVResult64"/> instance.
    /// </summary>
    /// <param name="left">The long value to compare.</param>
    /// <param name="right">The <see cref="AVResult64"/> instance to compare.</param>
    /// <returns><c>true</c> if the long value is not equal to the instance's value; otherwise, <c>false</c>.</returns>
    public static bool operator !=(long left, AVResult64 right) => !(left == right);

    /// <summary>
    /// Gets a value indicating whether the result represents an error.
    /// </summary>
    /// <value><c>true</c> if the result is less than 0; otherwise, <c>false</c>.</value>
    public bool IsError => value < 0;

    /// <summary>
    /// Throws an exception if the result represents an error.
    /// </summary>
    /// <exception cref="FFmpegException">Thrown when the result represents an error.</exception>
    public void ThrowIfError()
    {
        if (IsError)
            FFmpegException.ThrowIfError((int)value);
    }

    #region GNU Error Codes

    /// <summary>Operation not permitted (EPERM)</summary>
    public static long OperationNotPermitted => -1;

    /// <summary>No such file or directory (ENOENT)</summary>
    public static long NoSuchFileOrDirectory => -2;

    /// <summary>No such process (ESRCH)</summary>
    public static long NoSuchProcess => -3;

    /// <summary>Interrupted system call (EINTR)</summary>
    public static long InterruptedSystemCall => -4;

    /// <summary>I/O error (EIO)</summary>
    public static long IOError => -5;

    /// <summary>No such device or address (ENXIO)</summary>
    public static long NoSuchDeviceOrAddress => -6;

    /// <summary>Argument list too long (E2BIG)</summary>
    public static long ArgumentListTooLong => -7;

    /// <summary>Exec format error (ENOEXEC)</summary>
    public static long ExecFormatError => -8;

    /// <summary>Bad file descriptor (EBADF)</summary>
    public static long BadFileDescriptor => -9;

    /// <summary>No child processes (ECHILD)</summary>
    public static long NoChildProcesses => -10;

    /// <summary>Try again (EAGAIN)</summary>
    public static long TryAgain => -11;

    /// <summary>Out of memory (ENOMEM)</summary>
    public static long OutOfMemory => -12;

    /// <summary>Permission denied (EACCES)</summary>
    public static long PermissionDenied => -13;

    /// <summary>Bad address (EFAULT)</summary>
    public static long BadAddress => -14;

    /// <summary>Block device required (ENOTBLK)</summary>
    public static long BlockDeviceRequired => -15;

    /// <summary>Device or resource busy (EBUSY)</summary>
    public static long DeviceOrResourceBusy => -16;

    /// <summary>File exists (EEXIST)</summary>
    public static long FileExists => -17;

    /// <summary>Cross-device link (EXDEV)</summary>
    public static long CrossDeviceLink => -18;

    /// <summary>No such device (ENODEV)</summary>
    public static long NoSuchDevice => -19;

    /// <summary>Not a directory (ENOTDIR)</summary>
    public static long NotADirectory => -20;

    /// <summary>Is a directory (EISDIR)</summary>
    public static long IsADirectory => -21;

    /// <summary>Invalid argument (EINVAL)</summary>
    public static long InvalidArgument => -22;

    /// <summary>Too many open files in system (ENFILE)</summary>
    public static long TooManyOpenFilesInSystem => -23;

    /// <summary>Too many open files (EMFILE)</summary>
    public static long TooManyOpenFiles => -24;

    /// <summary>Not a typewriter (ENOTTY)</summary>
    public static long NotATypewriter => -25;

    /// <summary>Text file busy (ETXTBSY)</summary>
    public static long TextFileBusy => -26;

    /// <summary>File too large (EFBIG)</summary>
    public static long FileTooLarge => -27;

    /// <summary>No space left on device (ENOSPC)</summary>
    public static long NoSpaceLeftOnDevice => -28;

    /// <summary>Illegal seek (ESPIPE)</summary>
    public static long IllegalSeek => -29;

    /// <summary>Read-only file system (EROFS)</summary>
    public static long ReadOnlyFileSystem => -30;

    /// <summary>Too many links (EMLINK)</summary>
    public static long TooManyLinks => -31;

    /// <summary>Broken pipe (EPIPE)</summary>
    public static long BrokenPipe => -32;

    #endregion

    #region FFmpeg Error Codes

    /// <summary>
    /// Bitstream filter not found. 
    /// </summary>
    public static long BitstreamFilterNotFound => ffmpeg.AVERROR_BSF_NOT_FOUND;

    /// <summary>
    /// Internal bug, also see <see cref="Bug2"/> 
    /// </summary>
    public static long Bug => ffmpeg.AVERROR_BUG;

    /// <summary>
    /// This is semantically identical to <see cref="Bug"/> it has been introduced in Libav after our AVERROR_BUG and with a modified value. 
    /// </summary>
    public static long Bug2 => ffmpeg.AVERROR_BUG2;

    /// <summary>
    /// Buffer too small. 
    /// </summary>
    public static long BufferTooSmall => ffmpeg.AVERROR_BUFFER_TOO_SMALL;

    /// <summary>
    /// Decoder not found. 
    /// </summary>
    public static long DecoderNotFound => ffmpeg.AVERROR_DECODER_NOT_FOUND;

    /// <summary>
    /// Demuxer not found. 
    /// </summary>
    public static long DemuxerNotFound => ffmpeg.AVERROR_DEMUXER_NOT_FOUND;

    /// <summary>
    /// Encoder not found. 
    /// </summary>
    public static long EncoderNotFound => ffmpeg.AVERROR_ENCODER_NOT_FOUND;

    /// <summary>
    /// End of file. 
    /// </summary>
    public static long EndOfFile => ffmpeg.AVERROR_EOF;

    /// <summary>
    /// Immediate exit was requested; the called function should not be restarted. 
    /// </summary>
    public static long Exit => ffmpeg.AVERROR_EXIT;

    /// <summary>
    /// Generic error in an external library. 
    /// </summary>
    public static long ExternalError => ffmpeg.AVERROR_EXTERNAL;

    /// <summary>
    /// Filter not found. 
    /// </summary>
    public static long FilterNotFound => ffmpeg.AVERROR_FILTER_NOT_FOUND;

    /// <summary>
    /// Invalid data found when processing input. 
    /// </summary>
    public static long InvalidData => ffmpeg.AVERROR_INVALIDDATA;

    /// <summary>
    /// Muxer not found. 
    /// </summary>
    public static long MuxerNotFound => ffmpeg.AVERROR_MUXER_NOT_FOUND;

    /// <summary>
    /// Option not found. 
    /// </summary>
    public static long OptionNotFound => ffmpeg.AVERROR_OPTION_NOT_FOUND;

    /// <summary>
    /// Not yet implemented in FFmpeg, patches welcome. 
    /// </summary>
    public static long PatchWelcome => ffmpeg.AVERROR_PATCHWELCOME;

    /// <summary>
    /// Protocol not found. 
    /// </summary>
    public static long ProtocolNotFound => ffmpeg.AVERROR_PROTOCOL_NOT_FOUND;

    /// <summary>
    /// Stream not found. 
    /// </summary>
    public static long StreamNotFound => ffmpeg.AVERROR_STREAM_NOT_FOUND;

    /// <summary>
    /// Unknown error, typically from an external library. 
    /// </summary>
    public static long UnknownError => ffmpeg.AVERROR_UNKNOWN;

    /// <summary>
    /// Requested feature is flagged experimental. Notify strict_std_compliance if you really want to use it. 
    /// </summary>
    public static long ExperimentalFeature => ffmpeg.AVERROR_EXPERIMENTAL;

    /// <summary>
    /// Input changed between calls. Reconfiguration is required. (can be OR-ed with <see cref="OutputChanged"/>) 
    /// </summary>
    public static long InputChanged => ffmpeg.AVERROR_INPUT_CHANGED;

    /// <summary>
    /// Output changed between calls. Reconfiguration is required. (can be OR-ed with <see cref="InputChanged"/>) 
    /// </summary>
    public static long OutputChanged => ffmpeg.AVERROR_OUTPUT_CHANGED;

    /// <summary>
    /// HTTP 400 Bad Request. 
    /// </summary>
    public static long HttpBadRequest => ffmpeg.AVERROR_HTTP_BAD_REQUEST;

    /// <summary>
    /// HTTP 401 Unauthorized. 
    /// </summary>
    public static long HttpUnauthorized => ffmpeg.AVERROR_HTTP_UNAUTHORIZED;

    /// <summary>
    /// HTTP 403 Forbidden. 
    /// </summary>
    public static long HttpForbidden => ffmpeg.AVERROR_HTTP_FORBIDDEN;

    /// <summary>
    /// HTTP 404 Not Found. 
    /// </summary>
    public static long HttpNotFound => ffmpeg.AVERROR_HTTP_NOT_FOUND;

    /// <summary>
    /// HTTP 429 Too Many Requests. 
    /// </summary>
    public static long HttpTooManyRequests => ffmpeg.AVERROR_HTTP_TOO_MANY_REQUESTS;

    /// <summary>
    /// HTTP other 4xx errors. 
    /// </summary>
    public static long HttpOther4xx => ffmpeg.AVERROR_HTTP_OTHER_4XX;

    /// <summary>
    /// HTTP server error (5xx). 
    /// </summary>
    public static long HttpServerError => ffmpeg.AVERROR_HTTP_SERVER_ERROR;

    #endregion

    #region Methods

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AVResult64 result && Equals(result);

    /// <inheritdoc />
    public bool Equals(AVResult64 other) => value == other.value;

    /// <inheritdoc />
    public override int GetHashCode() => value.GetHashCode();

    /// <inheritdoc />
    bool IEquatable<long>.Equals(long other) => value.Equals(other);

    /// <inheritdoc cref="long.ToString()"/>
    public override string ToString()
    {
        if (!IsError)
            return value.ToString();
        string str =
        typeof(AVResult64).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .Where(p => p.CanRead && !p.IsSpecialName && (p.PropertyType == typeof(long) || p.PropertyType == typeof(AVResult64)))
            .Select(p => p.Name).FirstOrDefault();
        return string.IsNullOrEmpty(str) ? value.ToString() : str;
    }

    #endregion
}
