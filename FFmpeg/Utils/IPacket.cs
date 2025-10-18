namespace FFmpeg.Utils;
/// <inheritdoc cref="AVPacket"/>
public unsafe interface IPacket
{
    /// <inheritdoc cref="AVPacket.packet"/>
    internal AutoGen._AVPacket* Packet { get; }

    /// <inheritdoc cref="AVPacket.Data"/>
    Span<byte> Data { get; }

    /// <inheritdoc cref="AVPacket.DecompressionTime"/>
    TimeSpan DecompressionTime { get; set; }

    /// <inheritdoc cref="AVPacket.DecompressionTimestamp"/>
    long DecompressionTimestamp { get; set; }

    /// <inheritdoc cref="AVPacket.Duration"/>
    long Duration { get; set; }

    /// <inheritdoc cref="AVPacket.Flags"/>
    PacketFlags Flags { get; set; }

    /// <inheritdoc cref="AVPacket.Position"/>
    long Position { get; set; }

    /// <inheritdoc cref="AVPacket.PresentationTime"/>
    TimeSpan PresentationTime { get; set; }

    /// <inheritdoc cref="AVPacket.PresentationTimestamp"/>
    long PresentationTimestamp { get; set; }

    /// <inheritdoc cref="AVPacket.Size"/>
    int Size { get; set; }

    /// <inheritdoc cref="AVPacket.StreamIndex"/>
    int StreamIndex { get; set; }

    /// <inheritdoc cref="AVPacket.TimeBase"/>
    Rational TimeBase { get; set; }

    /// <inheritdoc cref="AVPacket.Unreference"/>
    void Unreference();

    /// <inheritdoc cref="AVPacket.Clone"/>
    AVPacket Clone();

    /// <inheritdoc cref="AVPacket.Resize(int)"/>
    void Resize(int size);

    /// <inheritdoc cref="AVPacket.MoveTo(IPacket)"/>
    void MoveTo(IPacket dst);

    /// <inheritdoc cref="AVPacket.CloneTo(IPacket)"/>
    void CloneTo(IPacket dst);

    /// <inheritdoc cref="AVPacket.CopyProperties(IPacket)"/>
    void CopyProperties(IPacket dst);
}
