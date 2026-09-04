# FFmpeg (.NET Wrapper)

[![NuGet](https://img.shields.io/nuget/v/FFmpegDotNet.svg)](https://www.nuget.org/packages/FFmpegDotNet/)
[![License](https://img.shields.io/badge/license-LGPL--2.1-blue.svg)](LICENSE)

A **.NET wrapper library for FFmpeg**, built on top of [FFmpeg.AutoGen](https://github.com/Pascal-Krenckel/FFmpeg.AutoGen).
It provides a managed, object-oriented interface to FFmpeg's C API, making it easier and safer to use FFmpeg from .NET applications, without giving up direct access to the underlying API when you need it.

📚 **[API Documentation](https://pascal-krenckel.github.io/FFmpeg/api/FFmpeg.html)**
🧪 **[Sample Projects](https://github.com/Pascal-Krenckel/FFmpeg.Examples)**

---

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Supported FFmpeg Versions](#supported-ffmpeg-versions)
  - [Installation](#installation)
  - [Loading FFmpeg Libraries](#loading-ffmpeg-libraries)
- [Quick Start](#quick-start)
- [Important Classes](#important-classes)
  - [Core Low-Level Wrappers](#core-low-level-wrappers)
  - [High-Level Abstractions](#high-level-abstractions)
- [Examples](#examples)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

[FFmpeg](https://ffmpeg.org/) is a powerful open-source multimedia framework for handling video, audio, and streams.
[FFmpeg.AutoGen](https://github.com/Pascal-Krenckel/FFmpeg.AutoGen) exposes FFmpeg's native C API to .NET, but using it directly means working with raw pointers and unsafe memory operations.

**FFmpeg (.NET Wrapper)** builds a **high-level, managed abstraction** on top of FFmpeg.AutoGen, so you can access FFmpeg's encoding, decoding, transcoding, and streaming capabilities safely from C#, with automatic resource cleanup and idiomatic .NET types.

---

## Getting Started

### Prerequisites

- A [.NET Standard 2.1](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)–compatible runtime (.NET Core 3.1+, .NET 5+, .NET 6+, Mono, etc.)
- [FFmpeg binaries](https://ffmpeg.org/download.html) available on your system path (or bundled via NuGet — see [Installation](#installation))

### Supported FFmpeg Versions

FFmpeg 9.0+ ("Lei")

| Library | Version |
|---------|---------|
| avcodec | 63 |
| avdevice | 63 |
| avfilter | 12 |
| avformat | 63 |
| avutil | 61 |
| swresample | 7 |
| swscale | 10 |

### Installation

Install the wrapper from NuGet:

```bash
dotnet add package FFmpegDotNet
```

If you don't already have FFmpeg installed on your system, the [FFmpegDotNet.bin.winx64](https://www.nuget.org/packages/FFmpegDotNet.bin.winx64/) package bundles the LGPL FFmpeg binaries for Windows x64:

```bash
dotnet add package FFmpegDotNet.bin.winx64
```

### Loading FFmpeg Libraries

The `FFmpeg` namespace includes a static helper class, `FFmpegLoader`, responsible for locating and initializing the native FFmpeg libraries before use. If `Initialize()` is not called manually, the library automatically attempts to locate the FFmpeg binaries using the default search order below.

**Default search order:**

1. Current directory (`./`)
2. `./ffmpeg/`
3. Platform-specific subdirectories (e.g. `./ffmpeg/win-x64`, `./ffmpeg/linux-arm64`)
4. The system `PATH` environment variable

If no suitable library is found, initialization fails and FFmpeg functions become unavailable. You can also point `Initialize()` at a specific directory if your binaries live somewhere non-standard:

```csharp
FFmpegLoader.Initialize(@"C:\tools\ffmpeg\bin");
```

---

## Quick Start

A minimal example using the high-level API to remux a file (copy streams into a new container without re-encoding):

```csharp
using FFmpeg;
using FFmpeg.Formats;
using FFmpeg.Utils;

FFmpegLoader.Initialize();

using MediaSource src = MediaSource.Open("input.mp4");
using MediaSink dst = MediaSink.Create("output.mkv")!;

// Copy all streams as-is (no decoding/encoding involved).
foreach (var stream in src.Streams)
    dst.AddStream(stream);

using AVPacket packet = AVPacket.Allocate();
AVResult32 result;
while (!(result = src.ReadPacket(packet)).IsError)
{
    dst.WritePacket(packet).ThrowIfError();
}
if (result != AVResult32.EndOfFile)
    result.ThrowIfError();

dst.WriteTrailer().ThrowIfError();
```

> **Note:** Real transcoding — decoding, re-encoding with a specific codec, and optionally running frames through a `FilterGraph` — is significantly more involved than a stream copy, since you're responsible for driving the decode/filter/encode loop yourself (see the [Filters example](https://github.com/Pascal-Krenckel/FFmpeg.Examples) for a full walkthrough using `CodecContext`, `FilterGraph`, and `MediaSink`). If you just need a straightforward file-to-file transcode with no custom filtering or frame-level access, it's often simpler and more robust to shell out to `ffmpeg.exe` directly (e.g. via `System.Diagnostics.Process`) and reserve `MediaSource`/`MediaSink` for scenarios where you need in-process control.

For finer control over decoding, filtering, or encoding, see the [high-level abstractions](#high-level-abstractions) below or browse the [example projects](https://github.com/Pascal-Krenckel/FFmpeg.Examples).

---

## Important Classes

The `FFmpeg` namespace provides managed wrapper classes around FFmpeg's core C structs and APIs. These classes simplify interacting with FFmpeg by handling resource management, initialization, and data conversion automatically.

### Core Low-Level Wrappers

These classes map directly to FFmpeg's internal structures and are typically used when building advanced or custom media pipelines.

| Class | Description |
|-------|-------------|
| `FFmpeg.Formats.FormatContext` | Wraps `AVFormatContext`. Handles input/output container formats, stream information, and demuxing/muxing. |
| `FFmpeg.Codecs.CodecContext` | Wraps `AVCodecContext`. Manages codecs for encoding and decoding audio/video streams. |
| `FFmpeg.Utils.AVPacket` | Wraps `AVPacket`. Represents encoded data (compressed frames or packets). |
| `FFmpeg.Utils.AVFrame` | Wraps `AVFrame`. Represents decoded, uncompressed media frames in memory. |
| `FFmpeg.Filters.FilterGraph` | Wraps `AVFilterGraph`. Provides access to FFmpeg's filter system (resizing, color correction, etc.). |
| `FFmpeg.Images.SwsContext` | Wraps `SwsContext`. Handles image scaling and pixel format conversion via libswscale. |
| `FFmpeg.Audio.SwrContext` | Wraps `SwrContext`. Handles audio resampling, format conversion, and channel layout adjustments via libswresample. |

> **Note:** These classes closely follow FFmpeg's internal lifecycle patterns and implement `IDisposable`. Always dispose them (or wrap them in a `using` statement) when no longer needed to avoid leaking native resources.

### High-Level Abstractions

If your goal is simply to read, decode, encode, or transcode media, use these instead of managing `FormatContext` and `CodecContext` directly.

| Class | Description |
|-------|-------------|
| `MediaSource` | Simplifies reading and decoding frames from a file or stream. Handles demuxing and decoding internally. |
| `MediaSink` | Simplifies encoding and writing frames to a file or stream. Handles muxing and encoding internally. |
| `Transcoder` | Reads from one source and writes to another, performing full media transcoding in a few lines of code. For simple file-to-file transcodes, consider calling `ffmpeg.exe` directly instead — see the note in [Quick Start](#quick-start). |

These abstractions are ideal for typical scenarios such as:

- Extracting frames from a video
- Re-encoding media to a different format
- Streaming or piping decoded frames to other components

---

## Examples

Complete sample applications are available in the [**FFmpeg.Examples**](https://github.com/Pascal-Krenckel/FFmpeg.Examples) repository, ranging from low-level API usage to the higher-level abstractions provided by FFmpegDotNet. Each project is intentionally small and focused, demonstrating a single feature or workflow:

- Opening media files with `DemuxerContext`
- Decoding audio and video streams
- Reading and writing media containers
- Audio resampling with `SwrContext`
- Image scaling and pixel format conversion with `SwsContext`
- Using `MediaSource` for simplified decoding
- Encoding
- Filters
- A simple video player

---

## Contributing

Contributions, bug reports, and feature requests are welcome. Please open an issue or pull request on GitHub.

## License

This project is licensed under the [LGPL-2.1 License](LICENSE). FFmpeg itself is licensed separately under the LGPL/GPL — see the [FFmpeg legal page](https://ffmpeg.org/legal.html) for details on which license applies to your build.