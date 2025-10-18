namespace FFmpeg.Utils;

/// <summary>
/// Managed wrapper for the FFmpeg <see cref="AutoGen._AVPacket"/> structure.
/// Provides managed access to packet data, timestamps, and operations for video and audio streaming.
/// </summary>
public unsafe class AVPacket : IDisposable, IPacket
{
    internal AutoGen._AVPacket* packet;
    AutoGen._AVPacket* IPacket.Packet => packet;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVPacket"/> class.
    /// Allocates memory for the <see cref="AutoGen._AVPacket"/>.
    /// </summary>
    /// <exception cref="OutOfMemoryException">
    /// Thrown when memory allocation for the <see cref="AutoGen._AVPacket"/> fails.
    /// </exception>
    internal AVPacket()
    {
        packet = ffmpeg.av_packet_alloc();
        if (packet == null)
            throw new OutOfMemoryException("Failed to allocate memory for AVPacket.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AVPacket"/> class using an existing unmanaged <see cref="AutoGen._AVPacket"/> pointer.
    /// </summary>
    /// <param name="packet">A pointer to an unmanaged <see cref="AutoGen._AVPacket"/> structure.</param>
    internal AVPacket(AutoGen._AVPacket* packet) : this() => this.packet = packet;

    /// <summary>
    /// Initializes a new instance of the <see cref="AVPacket"/> class with a specified payload size.
    /// Allocates memory for the packet and sets its size.
    /// </summary>
    /// <param name="size">The desired size of the packet's payload, in bytes.</param>
    /// <exception cref="OutOfMemoryException">
    /// Thrown when memory allocation for the packet or its payload fails.
    /// </exception>
    private AVPacket(int size)
    {
        packet = ffmpeg.av_packet_alloc();
        if (packet == null)
            throw new OutOfMemoryException("Failed to allocate memory for the AVPacket.");

        int result = ffmpeg.av_new_packet(packet, size);
        if (result < 0)
            throw new OutOfMemoryException("Failed to allocate the packet's payload.");
    }

    /// <summary>
    /// Allocates a new instance of the <see cref="AVPacket"/> class with a specified payload size.
    /// </summary>
    /// <param name="size">The size of the packet's payload, in bytes.</param>
    /// <returns>A newly allocated <see cref="AVPacket"/> instance.</returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown when memory allocation fails for either the <see cref="AutoGen._AVPacket"/> or its payload.
    /// </exception>
    public static AVPacket Allocate(int size)
    {
        var packet = ffmpeg.av_packet_alloc();
        if (packet == null)
            throw new OutOfMemoryException("Failed to allocate memory for the AVPacket.");

        int result = ffmpeg.av_new_packet(packet, size);
        if (result < 0)
        {
            ffmpeg.av_packet_free(&packet);
            throw new OutOfMemoryException("Failed to allocate the packet's payload.");
        }
        return new(packet);
    }

    /// <summary>
    /// Allocates a new instance of the <see cref="AVPacket"/> class without specifying a payload size.
    /// </summary>
    /// <returns>A newly allocated <see cref="AVPacket"/> instance.</returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown when memory allocation for the <see cref="AutoGen._AVPacket"/> fails.
    /// </exception>
    public static AVPacket Allocate()
    {
        var packet = ffmpeg.av_packet_alloc();
        if (packet == null)
            throw new OutOfMemoryException("Failed to allocate memory for AVPacket.");
        return new(packet);
    }

    /// <summary>
    /// Gets or sets the presentation timestamp (PTS) of the packet, in stream time base units.
    /// This timestamp indicates when the packet should be presented to the user.
    /// </summary>
    public long PresentationTimestamp
    {
        get => packet->pts;
        set => packet->pts = value;
    }

    /// <summary>
    /// Gets or sets the presentation timestamp (PTS) as a <see cref="TimeSpan"/>.
    /// The <see cref="PresentationTimestamp"/> is calculated using the <see cref="TimeBase"/>.
    /// </summary>
    public TimeSpan PresentationTime
    {
        get => PresentationTimestamp * TimeBase;
        set => PresentationTimestamp = (long)((Rational)value / TimeBase);
    }

    /// <summary>
    /// Gets or sets the decompression timestamp (DTS) of the packet, in stream time base units.
    /// This timestamp indicates when the packet should be decompressed.
    /// </summary>
    public long DecompressionTimestamp
    {
        get => packet->dts;
        set => packet->dts = value;
    }

    /// <summary>
    /// Gets or sets the decompression timestamp (DTS) as a <see cref="TimeSpan"/>.
    /// The <see cref="DecompressionTimestamp"/> is calculated using the <see cref="TimeBase"/>.
    /// </summary>
    public TimeSpan DecompressionTime
    {
        get => DecompressionTimestamp * TimeBase;
        set => DecompressionTimestamp = (long)((Rational)value / TimeBase);
    }

    /// <summary>
    /// Gets the raw data contained in the packet as a <see cref="Span{T}"/> of <see cref="byte"/>.
    /// </summary>
    public Span<byte> Data => new(packet->data, packet->size);

    /// <summary>
    /// Gets or sets the size of the packet's data in bytes.
    /// </summary>
    public int Size
    {
        get => packet->size;
        set => packet->size = value;
    }

    /// <summary>
    /// Gets or sets the index of the stream this packet belongs to.
    /// </summary>
    public int StreamIndex
    {
        get => packet->stream_index;
        set => packet->stream_index = value;
    }

    /// <summary>
    /// Gets or sets the packet flags, which are a combination of <see cref="PacketFlags"/> values.
    /// These flags indicate specific characteristics of the packet (e.g., key frame, corrupted data).
    /// </summary>
    public PacketFlags Flags
    {
        get => (PacketFlags)packet->flags;
        set => packet->flags = (int)value;
    }

    /// <summary>
    /// Gets or sets the duration of the packet in stream time base units.
    /// </summary>
    public long Duration
    {
        get => packet->duration;
        set => packet->duration = value;
    }

    /// <summary>
    /// Gets or sets the byte position of the packet in the stream.
    /// </summary>
    public long Position
    {
        get => packet->pos;
        set => packet->pos = value;
    }

    /// <summary>
    /// Gets or sets the time base used for the packet's timestamps, represented as a <see cref="Rational"/> value.
    /// This value defines the unit of time for the <see cref="PresentationTimestamp"/> and <see cref="DecompressionTimestamp"/>.
    /// </summary>
    public Rational TimeBase
    {
        get => packet->time_base;
        set => packet->time_base = value;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="AVPacket"/> and frees the allocated memory.
    /// </summary>
    public void Dispose()
    {
        AutoGen._AVPacket* pkt = packet;
        ffmpeg.av_packet_free(&pkt);
        packet = pkt;
        GC.SuppressFinalize(this); // Suppress finalization to prevent the finalizer from being called.
    }

    /// <summary>
    /// Finalizer to ensure that unmanaged resources are properly released when the object is garbage collected.
    /// </summary>
    ~AVPacket()
    {
        Dispose();
    }

    /// <summary>
    /// Unreferences the packet, decrementing the reference count and releasing associated resources if no references remain.
    /// This should be called when the packet is no longer needed.
    /// </summary>
    public void Unreference() => ffmpeg.av_packet_unref(packet);


    /// <summary>
    /// Creates a new <see cref="AVPacket"/> that references the same data as this packet.
    /// This creates a shallow copy, meaning the new packet will point to the same underlying data buffer.
    /// </summary>
    /// <returns>A new <see cref="AVPacket"/> instance that references the same data.</returns>
    /// <exception cref="OutOfMemoryException">
    /// Thrown when the clone operation fails due to insufficient memory allocation.
    /// </exception>
    public AVPacket Clone()
    {
        AutoGen._AVPacket* pkt = ffmpeg.av_packet_clone(packet);
        return pkt == null
            ? throw new OutOfMemoryException("Failed to clone the AVPacket due to insufficient memory.")
            : new AVPacket(pkt);
    }

    /// <summary>
    /// Resizes the packet's data buffer to the specified size, either growing or shrinking it.
    /// </summary>
    /// <param name="size">The new size of the packet's data buffer in bytes.</param>
    /// <exception cref="OutOfMemoryException">
    /// Thrown if the packet cannot be resized due to insufficient memory during growth.
    /// </exception>
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

    /// <summary>
    /// Moves the contents of this packet to the specified <see cref="IPacket"/> destination,
    /// effectively transferring ownership of the data without copying it.
    /// </summary>
    /// <param name="dst">The destination packet to move the data to.</param>
    public void MoveTo(IPacket dst)
    {
        dst.Unreference();
        ffmpeg.av_packet_move_ref(dst.Packet, packet);
    }

    /// <summary>
    /// Clones the contents of this packet to the specified <see cref="IPacket"/> destination.
    /// This creates a deep copy of the packet, including its data and properties.
    /// </summary>
    /// <param name="dst">The destination packet to clone the data to.</param>
    public void CloneTo(IPacket dst)
    {
        dst.Unreference();
        ((AVResult32)ffmpeg.av_packet_ref(dst.Packet, packet)).ThrowIfError();
        ((AVResult32)ffmpeg.av_packet_copy_props(dst.Packet, packet)).ThrowIfError();
    }

    /// <summary>
    /// Copies the properties of this packet, such as stream index and timestamps,
    /// to the specified <see cref="IPacket"/> destination without copying the actual data.
    /// </summary>
    /// <param name="dst">The destination packet to copy the properties to.</param>
    public void CopyProperties(IPacket dst)
    {
        ((AVResult32)ffmpeg.av_packet_copy_props(dst.Packet, packet)).ThrowIfError();
    }

    /// <summary>
    /// Rescales the timing fields (timestamps and durations) of the current packet from its current time base to a new time base. <br/>
    /// This operation modifies the packet's timing fields to express them in the new time base.
    /// </summary>
    /// <param name="newTimeBase">The destination time base to which the timing fields of the packet will be converted.</param>
    /// <remarks>
    /// If any of the packet's timestamps have unknown values (such as <see cref="ffmpeg.AV_NOPTS_VALUE"/>), they will be ignored during the conversion.
    /// The method also updates the packet's time base to the new time base after rescaling.
    /// </remarks>
    public void RescaleTS(Rational newTimeBase)
    {
        ffmpeg.av_packet_rescale_ts(packet, TimeBase, newTimeBase);
        TimeBase = newTimeBase;
    }

}

