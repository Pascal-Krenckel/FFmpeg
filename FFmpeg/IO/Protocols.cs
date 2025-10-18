using System.Collections;

namespace FFmpeg.IO;

/// <summary>
/// Provides access to the input and output protocols supported by FFmpeg for internal URL IO operations.
/// </summary>
public static unsafe class Protocols
{
    /// <summary>
    /// Gets an enumerable collection of all available input protocols supported by FFmpeg.
    /// </summary>
    /// <remarks>
    /// These protocols can be used to open input streams or files via FFmpeg's URL-based IO system.
    /// </remarks>
    public static IEnumerable<string> InputProtocols => new ProtocolEnumerator(isOutput: false);

    /// <summary>
    /// Gets an enumerable collection of all available output protocols supported by FFmpeg.
    /// </summary>
    /// <remarks>
    /// These protocols can be used to write to output streams or files via FFmpeg's URL-based IO system.
    /// </remarks>
    public static IEnumerable<string> OutputProtocols => new ProtocolEnumerator(isOutput: true);

    /// <summary>
    /// Enumerates the available FFmpeg protocols for input and output operations.
    /// </summary>
    /// <remarks>
    /// The <see cref="ProtocolEnumerator"/> class interacts with FFmpeg's protocol enumeration API
    /// to retrieve the names of supported protocols, such as "http", "https", "rtmp", etc.
    /// </remarks>
    private class ProtocolEnumerator : IEnumerable<string>, IEnumerator<string>
    {
        private void* _opaque;
        private string? _current;
        private readonly bool _isOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolEnumerator"/> class to enumerate either input or output protocols.
        /// </summary>
        /// <param name="isOutput">If <see langword="true"/>, enumerates output protocols; otherwise, enumerates input protocols.</param>
        public ProtocolEnumerator(bool isOutput)
        {
            _opaque = null;
            _current = null;
            _isOutput = isOutput;
        }

        /// <summary>
        /// Gets the current protocol name being enumerated.
        /// </summary>
        public string Current => _current!;

        object IEnumerator.Current => Current;

        /// <summary>
        /// Advances the enumerator to the next protocol in the collection.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator successfully advanced to the next protocol; otherwise, <see langword="false"/>.</returns>
        public bool MoveNext()
        {
            void* opaque = _opaque;
            _current = ffmpeg.avio_enum_protocols(&opaque, _isOutput ? 1 : 0);
            _opaque = opaque;

            return _current != null;
        }

        /// <summary>
        /// Resets the enumerator to its initial state, allowing iteration to begin again.
        /// </summary>
        public void Reset()
        {
            _opaque = null;
            _current = null;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ProtocolEnumerator"/>.
        /// </summary>
        public void Dispose()
        {
            _opaque = null;
            _current = null;
        }

        /// <summary>
        /// Returns the enumerator for the collection of protocols.
        /// </summary>
        /// <returns>The current instance as an enumerator of protocol names.</returns>
        public IEnumerator<string> GetEnumerator() => this;

        /// <summary>
        /// Returns the enumerator for the collection of protocols.
        /// </summary>
        /// <returns>The current instance as an enumerator of protocol names.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
