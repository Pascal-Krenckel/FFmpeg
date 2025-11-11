# Developer Guide

This document provides an overview of the most important classes and structures, and how to use them.

## Table of Contents

- [Structures](#structures)  
  - [`FFmpeg.Utils.AVResult32-63`](#ffmpegutilsavresult32-63)  
  - [`FFmpeg.Utils.Rational`](#ffmpegutilsrational)  
  - [`FFmpeg.Codec`](#ffmpegcodec)  
- [Enums](#enums)  
  - [`FFmpeg.Images.PixelFormat`](#ffmpegimagespixelformat)  
  - [`FFmpeg.Audio.SampleFormat`](#ffmpegaudiosampleformat)  
  - [`FFmpeg.Formats.DiscardFlags`](#ffmpegformatsdiscardflags)  
  - [`FFmpeg.Devices.DeviceType`](#ffmpegdevicesdevicetype)  
- [Classes](#classes)  
  - [`FFmpeg.AVFrame`](#ffmpegavframe)  
  - [`FFmpeg.AVPacket`](#ffmpegavpacket)  

---

## Structures

### `FFmpeg.Utils.AVResult32-63`

`AVResult32-63` represents the result of most FFmpeg functions that return an integer. The return value indicates **success** or **failure**:

- **Success**: `>= 0` → `IsError` returns `false`  
- **Failure**: `< 0` → `IsError` returns `true`

#### Conversion

`AVResult32-63` can be implicitly converted **from and to** both `int` and `long`.

#### Error Codes

The structure provides **static properties** for the most important FFmpeg error codes.

##### GNU Error Codes

| Code | Name         | Description |
|------|-------------|-------------|
| -11  | TryAgain    | Used during encoding/decoding or filtering when more data needs to be written or read. |
| -22  | InvalidArgument | Indicates missing or incorrect parameters. Check standard output for additional FFmpeg error information. |
| -1 to -32 | … | Other standard GNU error codes |

##### FFmpeg Error Codes [Negative FourCC]

| Code | Name      | Description |
|------|----------|-------------|
| `"EOF "` | EndOfFile | End of the file has been reached. |
| ...||

#### Exception Handling

Call `ThrowIfError()` to automatically throw an exception if the value is negative.

#### Example

```csharp
AVResult32 res = context.ReceiveFrame(frame);
do while(res == AVResult32.TryAgain)
{
    context.SendPacket(packet).ThrowIfError();
    res = context.ReceiveFrame(frame);
} 
```

---

### `FFmpeg.Utils.Rational`

The `Rational` struct represents a rational number, typically used to express **frame rates**, **timebases**, or other ratio-based values.

#### Conversions

`Rational` supports implicit conversion **from and to** `double`, and `TimeSpan`.  

#### Operators

The `Rational` struct supports several operators, including:

- Arithmetic operations (`+`, `-`, `*`, `/`)
- Comparisons (`==`, `!=`, `<`, `>`, `<=`, `>=`)
- Conversions to numeric types (`double`, `TimeSpan`)

#### Rescale

The method `Rescale` can be used to **rescale a timestamp** (usually stored as a `long` in TimeBase units) from one TimeBase into another.

#### Example

```csharp
// TimeSpan = long * Rational
// GetPresentationTimestamp returns the pts or the best effort pts if pts is not available
TimeSpan pts = frame.GetPresentationTimestamp() * frame.TimeBase;
```

```csharp
// Conversion from one timebase into another
dstFrame.PresentationTimestamp = dstFrame.TimeBase.Rescale(srcFrame.GetPresentationTimestamp(), srcFrame.TimeBase);
```

---

### `FFmpeg.Codec`

Represents a codec in FFmpeg. Provides access to core codec information, supported formats, and allows managed interaction with codec details.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Short, symbolic name of the codec (e.g., `"h264"`). |
| `LongName` | `string` | Human-readable descriptive name of the codec. |
| `MediaType` | `MediaType` | Type of media handled by the codec (video, audio, subtitle). |
| `CodecID` | `CodecID` | FFmpeg codec identifier. |
| `SupportedFramerates` | `ReadOnlySpan<Rational>` | Array of supported frame rates (video codecs). |
| `SupportedPixelFormats` | `ReadOnlySpan<PixelFormat>` | Array of supported pixel formats (video codecs). |
| `SupportedSampleRates` | `ReadOnlySpan<int>` | Array of supported audio sample rates. |
| `SupportedSampleFormats` | `ReadOnlySpan<SampleFormat>` | Array of supported audio sample formats. |

#### Methods

| Method | Description |
|--------|-------------|
| `GetBestPixelFormat(PixelFormat src, bool alphaUsed, out FFLoss loss)` | Finds the best pixel format for a source format, considering alpha usage. |
| `GetBestPixelFormat(PixelFormat src)` | Overload ignoring loss and assuming alpha used. |
| `static ReadOnlyCollection<Codec> GetAllCodecs()` | Retrieves all registered codecs (encoders and decoders). |
| `static Codec? FindDecoder(CodecID codecID)` | Finds a decoder by codec ID. |
| `static Codec? FindDecoder(string name)` | Finds a decoder by name. |
| `static Codec? FindEncoder(CodecID codecID)` | Finds an encoder by codec ID. |
| `static Codec? FindEncoder(string name)` | Finds an encoder by name. |

#### Usage Example

```csharp
// Find a decoder by codec ID
Codec? codec = Codec.FindDecoder(CodecID.H264);

if (codec is not null)
{
    Console.WriteLine($"Decoder: {codec?.Name} ({codec?.LongName})");
    Console.WriteLine($"MediaType: {codec?.MediaType}, CodecID: {codec?.CodecID}");

    // Best pixel format for a given source
    FFLoss loss = default;
    PixelFormat best = codec?.GetBestPixelFormat(PixelFormat.YUV420P, alphaUsed: false, out loss) ?? PixelFormat.None;
    if (best != PixelFormat.None)
        Console.WriteLine($"Best PixelFormat: {best}, Loss: {loss}");
}

// Enumerate all codecs
var allCodecs = Codec.GetAllCodecs();
```

---

## Enums

### `FFmpeg.Images.PixelFormat`

The `PixelFormat` enum defines all supported pixel formats within FFmpeg.  
It describes how pixel data is stored and interpreted (e.g., RGB, YUV, planar, packed, endianness, alpha channels, etc.).

#### Extensions

The `FFmpeg.Images.PixelFormatExtensions` static class provides helper methods to simplify working with pixel formats.

| Method | Description | Returns |
|---------|-------------|---------|
| `SwapEndianness()` | Swaps the byte order (endianness) of the pixel format (e.g., `RGB24BE` ↔ `RGB24LE`). | `PixelFormat` |
| `PlaneCount()` | Returns the number of image planes for the given pixel format (e.g., 1 for RGB, 3 for YUV). | `int` |
| `FindBestPixelFormat(ReadOnlySpan<PixelFormat>)` | Finds the best matching pixel format from a list of candidates. | `PixelFormat` |
| `FindBestPixelFormat(bool useAlpha, ReadOnlySpan<PixelFormat>)` | Finds the best matching format considering alpha transparency. | `PixelFormat` |
| `FindBestPixelFormat(out FFLoss loss, ReadOnlySpan<PixelFormat>)` | Finds the best matching format and reports conversion loss. | `PixelFormat` |
| `FindBestPixelFormat(bool useAlpha, out FFLoss loss, ReadOnlySpan<PixelFormat>)` | Finds the best matching format considering alpha transparency and reports conversion loss. | `PixelFormat` |

---

### `FFmpeg.Audio.SampleFormat`

The `SampleFormat` enum defines all supported audio sample formats within FFmpeg.  
It describes how audio samples are stored — integer vs. floating-point, planar vs. packed, bit depth, etc.

#### Extensions

The `FFmpeg.Audio.SampleExtensions` static class provides helper methods for inspecting, converting, and validating audio sample formats.

| Method | Description | Returns |
|---------|-------------|---------|
| `IsPlanar()` | Determines whether the sample format stores audio data in **planar** layout (one buffer per channel). | `bool` |
| `IsPacked()` | Determines whether the sample format stores audio data in **packed/interleaved** layout. | `bool` |
| `AsPlanar()` | Converts the current sample format to its planar equivalent (e.g., `Int16` → `Int16Planar`). | `SampleFormat` |
| `AsPacked()` | Converts the current sample format to its packed equivalent (e.g., `Float32Planar` → `Float32`). | `SampleFormat` |
| `GetBytesPerSample()` | Returns the number of **bytes per individual sample**. | `int` |
| `GetBitsPerSample()` | Returns the number of **bits per individual sample**. | `int` |
| `GetBitsPerSample(AutoGen._AVCodecID)` | Returns the number of bits per sample for a given FFmpeg codec ID. | `int` |
| `GetName()` | Returns the FFmpeg string name for the sample format (e.g., `"s16p"`, `"fltp"`). | `string` |
| `ValidateType<T>()` | Ensures that a given unmanaged .NET type matches the sample format (throws if not). | `void` |
| `GetSampleFormatType()` | Returns the .NET type (`byte`, `short`, `float`, etc.) corresponding to the sample format. | `Type` |

---

### `FFmpeg.Formats.DiscardFlags`

The `DiscardFlags` enum specifies which packets of an `AVStream` should be **discarded** during decoding or processing.

#### Example

```csharp
var mediaSource = MediaSource.Open(file);
int videoIndex = mediaSource.FindBestStream(MediaType.Video);

// Discard all packets from every stream
foreach (var stream in mediaSource.Streams)
    stream.Discard = DiscardFlags.All;

// Keep only the main video stream active
mediaSource.Streams[videoIndex].Discard = DiscardFlags.Default;
```

---

### `FFmpeg.Devices.DeviceType`

The `DeviceType` enum specifies the **hardware device types** available for hardware‑accelerated decoding in FFmpeg.

| DeviceType     | Description |
|----------------|-------------|
| `None`         | No hardware acceleration device. |
| `VDPAU`        | Video Decode and Presentation API for Unix — Linux hardware decoding. |
| `CUDA`         | NVIDIA GPU hardware-accelerated decoding. |
| `VAAPI`        | Video Acceleration API — Linux/Unix. |
| `DXVA2`        | Microsoft DirectX Video Acceleration 2 — Windows. |
| `QSV`          | Intel Quick Sync Video — Intel platforms. |
| `VideoToolbox` | Apple hardware-accelerated video API on macOS/iOS. |
| `D3D11VA`      | Direct3D 11 Video Acceleration — Windows. |
| `DRM`          | Direct Rendering Manager — Linux GPU acceleration. |
| `OpenCL`       | Open Computing Language — cross-platform GPU/CPU. |
| `MediaCodec`   | Android hardware-accelerated video API. |
| `Vulkan`       | Cross-platform graphics & compute API. |
| `D3D12VA`      | Direct3D 12 Video Acceleration — Windows. |
| `AMF`          | AMD Advanced Media Framework — AMD hardware acceleration. |
| `OHCODEC`      | OpenHarmony hardware acceleration. |

#### Notes

- Intended for **decoding**: choose a `DeviceType` when creating a decoder context for hardware acceleration.
- Availability depends on OS, GPU, drivers, and FFmpeg build.
- Unsupported devices may fall back to software decoding or throw an error.
- See FFmpeg Wiki: [HWAccelIntro](https://trac.ffmpeg.org/wiki/HWAccelIntro)

#### Example

```csharp
MediaSource video = MediaSource.Open(file, deviceType: DeviceType.Vulkan);
```

---

## Classes

Most classes contain unmanaged data and implement `IDisposable`.

### `FFmpeg.AVFrame`

Represents a decoded audio or video frame in memory.

#### Allocation

| Method | Description |
|--------|-------------|
| `static AVFrame.Allocate()` | Allocates a new empty frame. |
| `static AVFrame.Allocate(PixelFormat format, int width, int height)` | Allocates a video frame with initialized image buffer. |
| `static AVFrame.Allocate(SampleFormat format, ChannelLayout layout, int samples, int? sampleRate = null)` | Allocates an audio frame with initialized sample buffers. |
| `static AVFrame.Allocate(SampleFormat format, int channels, int samples, int? sampleRate = null)` | Allocates an audio frame using a default channel layout. |

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Width` | `int` | Coded width of the video frame. |
| `Height` | `int` | Coded height of the video frame. |
| `PixelFormat` | `PixelFormat` | Pixel format for video frames. |
| `SampleFormat` | `SampleFormat` | Sample format for audio frames. |
| `SampleRate` | `int` | Audio sample rate in Hz. |
| `SampleCount` | `int` | Number of audio samples per channel. |
| `ChannelLayout` | `ChannelLayout_ref` | Channel layout of the audio frame. |
| `Data` | `ReadOnlySpan<IntPtr>` | Pointers to the frame’s data buffers. |
| `LineSize` | `ref int_array8` | Stride/line sizes for each buffer. |
| `PresentationTimestamp` | `long` | Presentation timestamp (PTS) in stream time base units. |
| `TimeBase` | `Rational` | Time base for frame timestamps. |
| `Duration` | `long` | Duration of the frame in the same units as PTS. |

#### Methods

| Method | Description |
|--------|-------------|
| `GetPresentationTimestamp()` | Returns the PTS if available, otherwise best-effort timestamp. |
| `CreateBuffer(int align = 1)` | Allocates new buffers for the frame. Must set Pixel/Sample format, dimensions or channels first. |
| `GetData(int index)` | Returns a span of bytes for the specified plane/channel. |
| `GetBufferSpan(int bufferIndex)` | Returns a span of bytes for the specified buffer. |
| `Reference(AVFrame src)` | Makes the current frame reference another frame (shallow copy). |
| `Unreference()` | Releases all buffers and resets the frame for reuse. |
| `Dispose()` | Disposes the AVFrame. |

#### Usage Example

```csharp
// Allocate a video frame
using var frame = AVFrame.Allocate(PixelFormat.RGB24, 1920, 1080);

// Access pixel data
using Span<byte> plane0 = frame.GetData(0);

// Allocate an audio frame
var audioFrame = AVFrame.Allocate(SampleFormat.Float32, 2, 1024, 44100);

// Reference another frame
var clone = AVFrame.Allocate();
clone.Reference(frame);

// Reset frame for reuse
frame.Unreference();
```

---

### `FFmpeg.AVPacket`

Represents a compressed audio or video packet in memory.

#### Allocation

| Method | Description |
|--------|-------------|
| `static AVPacket.Allocate()` | Allocates a new empty packet. |
| `static AVPacket.Allocate(int size)` | Allocates a packet with a pre-allocated payload. |

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `PresentationTimestamp` | `long` | Packet PTS in stream time base units. |
| `PresentationTime` | `TimeSpan` | PTS expressed as a `TimeSpan` using the packet's `TimeBase`. |
| `DecompressionTimestamp` | `long` | Packet DTS in stream time base units. |
| `DecompressionTime` | `TimeSpan` | DTS expressed as a `TimeSpan` using the packet's `TimeBase`. |
| `Data` | `Span<byte>` | Raw packet data. |
| `Size` | `int` | Size of the packet data in bytes. |
| `StreamIndex` | `int` | Index of the stream this packet belongs to. |
| `Duration` | `long` | Duration in stream time base units. |
| `Position` | `long` | Byte position of the packet in the stream. |
| `TimeBase` | `Rational` | Time base for the packet's timestamps. |

#### Methods

| Method | Description |
|--------|-------------|
| `Unreference()` | Releases the packet and any associated data. |
| `Clone()` | Creates a new packet referencing the same data (shallow copy). |
| `Dispose()` | Disposes the AVPacket. |

#### Usage Example

```csharp
// Allocate a new packet
using var packet = AVPacket.Allocate();

// Receive a packet from the demuxer
formatContext.ReadFrame(packet);

// Send the packet to the decoder
codecContext.SendPacket(packet);

// Access raw packet data
Span<byte> buffer = packet.Data;

// Clone the packet (shallow copy)
using var clone = packet.Clone();

// Release packet resources
packet.Unreference();
```

### `FFmpeg.FormatContext`

Represents an input or output media container and manages streams, I/O, and metadata.  
Most operations wrap FFmpeg's `AVFormatContext` and unmanaged resources. Implements `IDisposable`.

---

#### Input / Output Initialization

| Method | Description |
|--------|-------------|
| `static OpenInput` | Opens a media input from a stream with optional input format and options. Handles `AVDictionary`, `AVMultiDictionary`, or `IDictionary<string,string>`. |
| `static OpenOutput` | Opens an output file, stream, or custom I/O context for writing. |

---

#### Streams

| Property | Description |
|----------|-------------|
| `Streams` | Gets a read-only collection of streams in this context. Updated automatically if the underlying stream array changes. |

| Method | Description |
|--------|-------------|
| `FindStreamInfo` | Populates stream information. Supports array/span of `AVDictionary`, `AVMultiDictionary`, or `IDictionary<string,string>`. |
| `FindBestStream` | Finds the best stream of a given media type (audio, video, subtitle). Returns stream index or negative if none found. |
| `GuessFrameRate` | Guesses frame rate of a stream by `AVStream` or stream index, optionally using a frame. |
| `AddStream` | Adds a new stream to the media file using either a `Codec` or a `CodecContext`. Returns the newly added `AVStream`. |

---

#### Frame Operations

| Method | Description |
|--------|-------------|
| `WriteHeader` | Writes the header of the output media. Supports multiple dictionary types. |
| `WriteFrame` | Writes a packet to the output, optionally interleaved. |
| `WriteTrailer` | Writes the trailer for output once. |
| `ReadFrame` | Reads a packet from input and sets packet time base if unset. |

---

#### Seeking

| Method | Description |
|--------|-------------|
| `Seek` | Seeks to a specific time or timestamp globally or per stream. |

---

#### Output Timestamps

| Method | Description |
|--------|-------------|
| `GetOutputTimestamp` | Retrieves the output timestamp for a given stream. |

---

#### IDisposable / Resource Management

| Method | Description |
|--------|-------------|
| `Dispose` | Disposes the `FormatContext` and associated resources. |
| `Free` | Alias for `Dispose()`. |
| `~FormatContext` | Finalizer ensures unmanaged resources are released. |

