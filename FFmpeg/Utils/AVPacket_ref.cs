namespace FFmpeg.Utils;

/// <inheritdoc cref="AVPacket"/>
public readonly unsafe struct AVPacket_ref : IPacket, IReference<AVPacket>
{
    /// <inheritdoc />
    internal readonly AutoGen._AVPacket* packet;
    /// <inheritdoc />
    AutoGen._AVPacket* IPacket.Packet => packet;

    /// <inheritdoc />
    internal AVPacket_ref(AutoGen._AVPacket* packet) => this.packet = packet;

    /// <inheritdoc />
    public long PresentationTimestamp
    {
        get => packet->pts;
        set => packet->pts = value;
    }

    /// <inheritdoc />
    public TimeSpan PresentationTime
    {
        get => PresentationTimestamp * TimeBase;
        set => PresentationTimestamp = (long)((Rational)value / TimeBase);
    }

    /// <inheritdoc />
    public long DecompressionTimestamp
    {
        get => packet->dts;
        set => packet->dts = value;
    }

    /// <inheritdoc />
    public TimeSpan DecompressionTime
    {
        get => DecompressionTimestamp * TimeBase;
        set => DecompressionTimestamp = (long)((Rational)value / TimeBase);
    }

    /// <inheritdoc />
    public Span<byte> Data => new(packet->data, packet->size);

    /// <inheritdoc />
    public int Size
    {
        get => packet->size;
        set => packet->size = value;
    }

    /// <inheritdoc />
    public int StreamIndex
    {
        get => packet->stream_index;
        set => packet->stream_index = value;
    }

    /// <inheritdoc />
    public PacketFlags Flags
    {
        get => (PacketFlags)packet->flags;
        set => packet->flags = (int)value;
    }

    /// <inheritdoc />
    public long Duration
    {
        get => packet->duration;
        set => packet->duration = value;
    }

    /// <inheritdoc />
    public long Position
    {
        get => packet->pos;
        set => packet->pos = value;
    }


    /// <inheritdoc />
    public Rational TimeBase
    {
        get => packet->time_base;
        set => packet->time_base = value;
    }

    /// <inheritdoc />
    public AVPacket Clone()
    {
        AutoGen._AVPacket* pkt = ffmpeg.av_packet_clone(packet);
        return pkt == null ? throw new OutOfMemoryException("Failed to clone the AVPacket due to insufficient memory.") : new AVPacket(pkt);
    }

    /// <inheritdoc />
    public void Unreference() => ffmpeg.av_packet_unref(packet);



    /// <inheritdoc />
    public void Resize(int size)
    {
        int sizeDifference = size - packet->size;

        if (sizeDifference > 0)
        {
            // Increase packet size
            if (ffmpeg.av_grow_packet(packet, sizeDifference) < 0)
            {
                throw new OutOfMemoryException("Failed to grow the packet due to insufficient memory.");
            }
        }
        else if (sizeDifference < 0)
        {
            // Reduce packet size
            ffmpeg.av_shrink_packet(packet, size);
        }
        // If sizeDifference is 0, no resizing is needed.

    }

    ///<inheritdoc />
    public AVPacket? GetReferencedObject()
    {
        AutoGen._AVPacket* packet = ffmpeg.av_packet_alloc();
        if (packet == null)
            throw new OutOfMemoryException();
        AVResult32 res = ffmpeg.av_packet_ref(packet, this.packet);
        if (res.IsError)
        {
            ffmpeg.av_packet_free(&packet);
            res.ThrowIfError();
        }
        return new(packet);
    }

    ///<inheritdoc />
    public void SetReferencedObject(AVPacket? packet)
    {
        Unreference();
        if (packet != null)
        {
            AVResult32 res = ffmpeg.av_packet_ref(this.packet, packet.packet);
            res.ThrowIfError();
            res = ffmpeg.av_packet_copy_props(this.packet, packet.packet);
            res.ThrowIfError();
        }
    }
    /// <inheritdoc />
    public void MoveTo(IPacket dst)
    {
        dst.Unreference();
        ffmpeg.av_packet_move_ref(dst.Packet, packet);
    }
    /// <inheritdoc />
    public void CloneTo(IPacket dst)
    {
        dst.Unreference();
        ((AVResult32)ffmpeg.av_packet_ref(dst.Packet, packet)).ThrowIfError();
        ((AVResult32)ffmpeg.av_packet_copy_props(dst.Packet, packet)).ThrowIfError();
    }
    /// <inheritdoc />
    public void CopyProperties(IPacket dst) => ((AVResult32)ffmpeg.av_packet_copy_props(dst.Packet, packet)).ThrowIfError();
}
