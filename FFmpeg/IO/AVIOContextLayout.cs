using System.Runtime.InteropServices;

namespace FFmpeg.IO;

/// <summary>
/// Reads and writes <c>AVIOContext.seekable</c> with the layout of the running platform.
/// </summary>
/// <remarks>
/// <c>AVIOContext.checksum</c> is a C <c>unsigned long</c>, which is 32 bit on Windows and 64 bit
/// on other 64-bit platforms. The AutoGen bindings used by this version declare it as <see cref="ulong"/>,
/// so on Windows every field after <c>checksum</c> would be read 4 bytes too far (<c>seekable</c> comes
/// back as part of the following pointer or <c>direct</c>). On Windows the field is therefore
/// accessed through a mirror of the native layout up to <c>seekable</c>.
/// </remarks>
internal static unsafe class AVIOContextLayout
{
    private static readonly bool useWindowsLayout = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>The native <c>AVIOContext</c> up to <c>seekable</c>, with a 32-bit <c>unsigned long</c>.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowsLayout
    {
        public void* av_class;
        public byte* buffer;
        public int buffer_size;
        public byte* buf_ptr;
        public byte* buf_end;
        public void* opaque;
        public void* read_packet;
        public void* write_packet;
        public void* seek;
        public long pos;
        public int eof_reached;
        public int error;
        public int write_flag;
        public int max_packet_size;
        public int min_packet_size;
        public uint checksum;
        public byte* checksum_ptr;
        public void* update_checksum;
        public void* read_pause;
        public void* read_seek;
        public int seekable;
    }

    public static int GetSeekable(AutoGen._AVIOContext* pb)
        => useWindowsLayout ? ((WindowsLayout*)pb)->seekable : pb->seekable;

    public static void SetSeekable(AutoGen._AVIOContext* pb, int value)
    {
        if (useWindowsLayout)
            ((WindowsLayout*)pb)->seekable = value;
        else
            pb->seekable = value;
    }
}
