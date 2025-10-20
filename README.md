# FFmpeg (.NET Wrapper)

A **.NET wrapper library for FFmpeg** built using [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen).  
This library provides a **managed, object-oriented interface** to FFmpeg’s C API — making it easier, safer, and more intuitive to use FFmpeg in .NET applications.

---

## 📖 Overview

[FFmpeg](https://ffmpeg.org/) is a powerful open-source multimedia framework for handling video, audio, and streams.  
While [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen) exposes FFmpeg’s native C API to .NET, it requires working directly with pointers and unsafe memory operations.

The **FFmpeg (.NET Wrapper)** library provides a **high-level, managed abstraction** over FFmpeg.AutoGen — letting you access FFmpeg’s encoding, decoding, transcoding, and streaming capabilities safely from C#.

---

## 🚀 Getting Started



### 1. Prerequisites

- [.NET Standard 2.1](https://learn.microsoft.com/en-us/dotnet/standard/net-standard) compatible runtime (e.g., .NET Core 3.1+, .NET 5+, .NET 6+, Mono, etc.)  
- [FFmpeg binaries](https://ffmpeg.org/download.html) available in your system path  

#### Supported FFmpeg Versions

| Library       | Version |
|----------------|----------|
| avcodec        | 62       |
| avdevice       | 62       |
| avfilter       | 11       |
| avformat       | 62       |
| avutil         | 60       |
| swresample     | 6        |
| swscale        | 9        |
 
### 2. Loading FFmpeg Libraries

The **`FFmpeg`** namespace includes a static helper class named `FFmpegLoader`, which is responsible for locating and initializing the native FFmpeg libraries before use.  
If `Initialize()` is not called manually, the library will automatically attempt to locate the FFmpeg binaries using a predefined search order.

#### Default Search Order

When `FFmpegLoader.Initialize()` is called with no parameters, the following directories are searched in order:

1. Current directory (`./`)  
2. `./ffmpeg/`  
3. Platform-specific subdirectories (e.g. `./ffmpeg/win-x64`, `./ffmpeg/linux-arm64`, etc.)  
4. System `%PATH%` environment variable  

If a suitable library is not found, initialization will fail and FFmpeg functions will be unavailable.

## 🧩 Important Classes

The `FFmpeg` namespace provides a set of managed wrapper classes around FFmpeg’s core C structs and APIs.  
These classes simplify the process of interacting with FFmpeg by handling resource management, initialization, and data conversion automatically.

---

### 🎞️ Core Low-Level Wrappers

These classes map directly to FFmpeg’s internal structures.  
They are typically used when building advanced or custom media pipelines.

| Class | Description |
|-------|--------------|
| **`FFmpeg.Formats.FormatContext`** | Wraps `AVFormatContext`. Handles input/output container formats, stream information, and demuxing/muxing. |
| **`FFmpeg.Codecs.CodecContext`** | Wraps `AVCodecContext`. Manages codecs for encoding and decoding audio/video streams. |
| **`FFmpeg.Utils.AVPacket`** | Wraps `AVPacket`. Represents encoded data (compressed frames or packets). |
| **`FFmpeg.Utils.AVFrame`** | Wraps `AVFrame`. Represents decoded, uncompressed media frames in memory. |
| **`FFmpeg.Filters.FilterGraph`** | Wraps `AVFilterGraph`. Provides access to FFmpeg’s powerful filter system (for resizing, color correction, etc.). |
| **`FFmpeg.Images.SwsContext`** | Wraps `SwsContext`. Used for image scaling and pixel format conversion via libswscale. |
| **`FFmpeg.Audio.SwrContext`** | Wraps `SwrContext`. Handles audio resampling, format conversion, and channel layout adjustments via libswresample. |

> 🧠 **Note:**  
> These classes closely follow FFmpeg’s internal lifecycle patterns and should be disposed (`IDisposable`) when no longer needed.

---

### ⚡ High-Level Abstractions

If your goal is to read, decode, encode, or transcode media streams,  
you can use the high-level abstractions instead of managing `FormatContext` and `CodecContext` directly.

| Class | Description |
|-------|--------------|
| **`MediaSource`** | Simplifies reading and decoding frames from a file or stream. Handles demuxing and decoding internally. |
| **`MediaSink`** | Simplifies encoding and writing frames to a file or stream. Handles muxing and encoding internally. |
| **`Transcoder`** | Provides a convenient way to read from one source and write to another, performing full media transcoding. |

These abstractions are ideal for typical scenarios such as:
- Extracting frames from a video  
- Re-encoding media to a different format  
- Streaming or piping decoded frames to other components  