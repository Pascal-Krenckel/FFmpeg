using FFmpeg.Audio;
using FFmpeg.Codecs;
using FFmpeg.Filters;
using FFmpeg.Formats;
using FFmpeg.Images;
using FFmpeg.Logging;
using FFmpeg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace FFmpeg;
// a one to one transcoder supporting -ss
public sealed class Transcoder : IDisposable
{
    private MediaSource Source { get; }
    private MediaSink Sink { get; }


    private TimeSpan duration;
    private TimeSpan start;
    private AVFrame readFrame = AVFrame.Allocate();
    private AVFrame filteredFrame = AVFrame.Allocate();
    private AVFrame convertedFrame = AVFrame.Allocate();
    private AVPacket readPacket = AVPacket.Allocate();
    private AVPacket writePacket = AVPacket.Allocate();

    // PreviewOutputStreamIndex must point to a valid audio/video stream. If the stream is just copied, the frame gets decoded every 10s, can be set see UpdateInterval
    // Only makes sense for video streams
    public AVFrame PreviewFrame { get; } = AVFrame.Allocate();

    // last time stamp if PreviewOutputStreamIndex points to a valid audio or video stream index.
    public TimeSpan CurrentTimestamp { get; private set; }
    // Index of the stream that should be used for previews. if -1 automatically set to best video then best audio in WriteHeader
    public int PreviewOutputStreamIndex { get; set; } = -1;
    private DateTime lastUpdate = DateTime.MinValue;
    // if < Zero no preview frame will additionally get decoded 
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan Duration => duration > TimeSpan.Zero ?  duration : OutputStream.Max(s => (s.StartTime + s.Duration) * s.TimeBase)-start;



    private Subtitles.Subtitle subtitle = new();


    private Transcoder(MediaSource source, MediaSink sink)
    {
        Source = source;
        Sink = sink;
        Sink.Metatdata.Init(source.Metadata);
    }

    public Transcoder(FormatContext source, FormatContext sink) : this(new MediaSource(source), new(sink))
    { }

    public static Transcoder Create(FormatContext source, HW.DeviceType hwSourceType, FormatContext sink) => new(new MediaSource(source, hwSourceType), new(sink));
    public static Transcoder Create(FormatContext source, FormatContext sink) => new(new MediaSource(source), new(sink));


    public IReadOnlyList<AVStream> InputStreams => Source.Streams;
    public IReadOnlyList<CodecContext> InputCodecs => Source.CodecContexts;
    public IReadOnlyList<AVStream> OutputStream => Sink.Streams;
    public IReadOnlyList<CodecContext?> OutputCodecs => Sink.CodecContexts;


    Dictionary<int, List<TranscodingMap>> transCoderMap = [];


    // filter must have 1 ctx called in and 1 ctx called out
    public int MapStream(int sourceIndex, CodecContext? sinkCodec, FilterGraph? filter)
    {
        int sinkIndex = Sink.Streams.Count;
        var stream = Sink.AddStream(sinkCodec);
        stream.Metadata.Init(InputStreams[sourceIndex].Metadata);

        if (transCoderMap.Any(kv => kv.Value.Any(map => map.OutputIndex == sinkIndex)))
            throw new ArgumentException(nameof(sinkIndex), $"The output stream #{sinkIndex} is already linked.");
        if (!transCoderMap.TryGetValue(sourceIndex, out var list))
            transCoderMap[sourceIndex] = list = [];
        FilterContext? bfSink = null;
        if (filter != null)
            bfSink = filter.Filters.Single(f => f.Name == "out");
        int width = bfSink?.BufferSinkWidth() ?? InputCodecs[sourceIndex].Width;
        int height = bfSink?.BufferSinkHeight() ?? InputCodecs[sourceIndex].Height;
        Rational timeBase = bfSink?.BufferSinkTimeBase() ?? InputStreams[sourceIndex].TimeBase;
        PixelFormat pixFmt = bfSink?.BufferSinkPixelFormat() ?? InputCodecs[sourceIndex].PixelFormat;
        ChannelLayout l = null!;
        if (!(bfSink?.BufferSinkChannelLayout(out l) >= 0))
            l = InputCodecs[sourceIndex].ChannelLayout.GetReferencedObject()!;
        using var layout = l;
        var colorRange = bfSink?.BufferSinkColorRange() ?? InputCodecs[sourceIndex].ColorRange;
        var colorSpace = bfSink?.BufferSinkColorSpace() ?? InputCodecs[sourceIndex].ColorSpace;

        var frameRate =  InputCodecs[sourceIndex].FrameRate;
        if (bfSink?.BufferSinkFrameRate().IsValidTimeBase == true)
            frameRate = bfSink.BufferSinkFrameRate();


        var sampleAspectRatio = bfSink?.BufferSinkSampleAspectRatio() ?? InputCodecs[sourceIndex].SampleAspectRatio;
        var sampleFmt = bfSink?.BufferSinkSampleFormat() ?? InputCodecs[sourceIndex].SampleFormat;
        var sampleRate = bfSink?.BufferSinkSampleRate() ?? InputCodecs[sourceIndex].SampleRate;

        if (sinkCodec != null)
        {
            if (sinkCodec.Width == 0)
                sinkCodec.Width = width;
            if (sinkCodec.Height == 0)
                sinkCodec.Height = height;
            if (!sinkCodec.TimeBase.IsValidTimeBase)
                sinkCodec.TimeBase = timeBase;
            if (sinkCodec.PixelFormat == PixelFormat.None)
                sinkCodec.PixelFormat = sinkCodec.Codec.GetBestPixelFormat(pixFmt);
            if (sinkCodec.SampleFormat == SampleFormat.None)
            {
                if (sinkCodec.Codec.SupportedSampleFormats.Contains(sampleFmt))
                    sinkCodec.SampleFormat = sampleFmt;
                else sinkCodec.SampleFormat = sinkCodec.Codec.SupportedSampleFormats.FirstOrDefault(SampleFormat.None);
            }
            if (sinkCodec.CodecType == MediaType.Audio && (!sinkCodec.ChannelLayout.Valid || sinkCodec.ChannelLayout.Channels == 0))
                sinkCodec.ChannelLayout.SetReferencedObject(l);
            if (sinkCodec.ColorRange == ColorRange.Unspecified)
                sinkCodec.ColorRange = colorRange;
            if (sinkCodec.ColorSpace == ColorSpace.Unspecified)
                sinkCodec.ColorSpace = colorSpace;
            if (!sinkCodec.FrameRate.IsValidTimeBase)
                sinkCodec.FrameRate = frameRate;
            if (sinkCodec.SampleAspectRatio.IsValidTimeBase)
                sinkCodec.SampleAspectRatio = sampleAspectRatio;
            if (sinkCodec.SampleRate == 0)
                sinkCodec.SampleRate = sampleRate;
            Sink.SetCodecContext(sinkCodec, sinkIndex);
        }
        else
            OutputStream[sinkIndex].CodecParameters.CopyFrom(InputStreams[sourceIndex].CodecParameters);
        if (filter != null)
        {
            if (sinkCodec == null)
            {
                CodecContext encoder = CodecContext.Allocate(Codec.FindEncoder(InputCodecs[sourceIndex].CodecID));
                if (encoder.CodecType == Utils.MediaType.Audio)
                {
                    var fctx = filter.Filters.First(f => f.Name=="out");
                    encoder.SampleRate = fctx.BufferSinkSampleRate();
                    fctx.BufferSinkChannelLayout(out var ch).ThrowIfError();
                    encoder.ChannelLayout.CopyFrom(ch);
                    ch.Dispose();
                    encoder.SampleFormat = fctx.BufferSinkSampleFormat();
                    encoder.TimeBase = fctx.BufferSinkTimeBase();
                }
                else if (encoder.CodecType == Utils.MediaType.Video)
                {
                    var fctx = filter.Filters.First(f => f.Name=="out");
                    encoder.TimeBase = fctx.BufferSinkTimeBase();
                    encoder.PixelFormat = fctx.BufferSinkPixelFormat();
                    encoder.FrameRate = fctx.BufferSinkFrameRate();
                    encoder.Width = fctx.BufferSinkWidth();
                    encoder.Height = fctx.BufferSinkHeight();
                    encoder.SampleAspectRatio = fctx.BufferSinkSampleAspectRatio();
                }
                Sink.SetCodecContext(encoder, sinkIndex);
            }
            list.Add(TranscodingMap.Create(sourceIndex, sinkIndex, filter));
        }
        else
            list.Add(TranscodingMap.Create(sourceIndex, sinkIndex));
        return sinkIndex;
    }
    //Copy
    public int MapStream(int sourceIndex) => MapStream(sourceIndex, null, null);
    public int MapStream(int sourceIndex, CodecContext sinkCodec) => MapStream(sourceIndex, sinkCodec, null);
    public int MapStream(int sourceIndex, FilterGraph filter) => MapStream(sourceIndex,null, filter);


    public AVResult32 WriteTrailer() => Sink.WriteTrailer();
    public void WriteHeader(TimeSpan startTime = default, TimeSpan duration = default)
    {

        ExceptionLog.Reset();
        for (int i = 0; i < Source.Streams.Count; i++)
        {
            int sourceIndex = i;
            if (!transCoderMap.TryGetValue(i, out var maps))
                Source.Streams[i].Discard = DiscardFlags.All;
            else foreach (var map in maps)
                {
                    var sourceStream = Source.Streams[sourceIndex];
                    var sinkStream = Sink.Streams[map.OutputIndex];
                    var sinkContext = Sink.CodecContexts[map.OutputIndex];

                    if (sinkContext != null)
                        sinkStream.CodecParameters.CopyFrom(sinkContext);
                    if (!sinkStream.TimeBase.IsValidTimeBase)
                        if (sinkContext?.PacketTimeBase.IsValidTimeBase == true)
                            sinkStream.TimeBase = sinkContext!.PacketTimeBase;
                        else if (sinkContext?.TimeBase.IsValidTimeBase == true)
                            sinkStream.TimeBase = sinkContext!.TimeBase;
                        else sinkStream.TimeBase = sourceStream.TimeBase;

                    long startTB  = (long)((Rational)startTime/sinkStream.TimeBase);
                    long durationTB = (long)((Rational)duration/sinkStream.TimeBase);

                    sinkStream.StartTime = sinkStream.TimeBase.Rescale(sourceStream.StartTime, sourceStream.TimeBase); // rescale start time
                    sinkStream.Duration = sinkStream.TimeBase.Rescale(sourceStream.Duration, sourceStream.TimeBase); // rescale duration
                    if (startTB > 0)
                    {
                        long shift = sinkStream.StartTime - startTB;
                        sinkStream.StartTime = Math.Max(shift, 0);
                        sinkStream.Duration += Math.Min(shift, 0); // if shift negative, remove the cut parts from duration. 
                    }
                    if (durationTB > 0 && startTB + durationTB < sinkStream.StartTime + sinkStream.Duration)
                    { // duration is specified and the new end is before the calculated end
                        sinkStream.Duration = durationTB;
                    }
                }
        }
        this.start = startTime;
        this.duration = duration;
        Sink.OpenCodecs();
        Sink.WriteHeader();
        ExceptionLog.Reset();
        Source.Seek(start).ThrowIfError();
        if (PreviewOutputStreamIndex < 0)
            PreviewOutputStreamIndex = Sink.FormatContext.FindBestStream(MediaType.Video);
        if (PreviewOutputStreamIndex < 0)
            PreviewOutputStreamIndex = Sink.FormatContext.FindBestStream(MediaType.Audio);
    }
    private DrainState drainState = DrainState.None;
    [Flags]
    private enum DrainState { None, DecoderDraining = 0b1, DecoderDrained = 0b11, FilterDraining = 0b100, FilterDrained = 0b1100, EncoderDraining = 0b10000, EncoderDrained = 0b110000 };
    private void DrainDecoders()
    {
        if (drainState.HasFlag(DrainState.DecoderDraining)) return;
        for (int i = 0; i < Source.CodecContexts.Count; i++)
            if (Source.CodecContexts[i].CodecType is MediaType.Video or MediaType.Audio)
                Source.CodecContexts[i]!.DrainDecoder().ThrowIfError();
        drainState = DrainState.DecoderDraining;
    }

    public AVResult32 ProcessNextPacket()
    {
        AVResult32 res;
        do
        {
            res = ReadPacket();

            if (res != AVResult32.EndOfFile)
                res = ProcessPacket();
            else res = HandleEndOfFile();
        } while (res.IsTryAgain);
        return res;
    }

    private AVResult32 ProcessPacket()
    {
        if (Source.Streams[readPacket.StreamIndex].MediaType is MediaType.Video or MediaType.Audio)
            return ProcessPacketAV();
        else if (Source.Streams[readPacket.StreamIndex].MediaType is MediaType.Subtitle)
            return ProcessPacketSubtitle();
        else return ProcessJustCopy();
    }

    public void Close() => Dispose();
    private AVResult32 HandleEndOfFile()
    {
        DrainDecoders();
        AVResult32 res = ReadFrameDraining();
        if (res != AVResult32.EndOfFile)
            if (res.IsError) return res;
            else return ProcessFrame(res);
        else if (!drainState.HasFlag(DrainState.FilterDrained))
            return DrainFilters();
        else return DrainEncoders();

    }

    private AVResult32 DrainEncoders()
    {
        if (!drainState.HasFlag(DrainState.EncoderDraining))
            foreach (var encoder in Sink.CodecContexts.Where(ctx => ctx != null))
                encoder!.SendFrame(null).ThrowIfError();
        drainState |= DrainState.EncoderDraining;
        if (drainState.HasFlag(DrainState.EncoderDrained))
            return AVResult32.EndOfFile;

        for (int i = 0; i < Sink.CodecContexts.Count; i++)
        {
            if (Sink.CodecContexts[i] == null) continue;
            var encoder = Sink.CodecContexts[i]!;
            var res = encoder.ReceivePacket(writePacket);
            if (res == AVResult32.EndOfFile) continue;
            if (res.IsError) return res;

            writePacket.PresentationTimestamp -= start / writePacket.TimeBase;
            writePacket.DecompressionTimestamp -= start / writePacket.TimeBase;
            writePacket.StreamIndex = i;

            return Sink.WritePacket(writePacket);

        }


        drainState |= DrainState.EncoderDrained;
        return AVResult32.EndOfFile;

    }

    private AVResult32 DrainFilters()
    {
        if (!drainState.HasFlag(DrainState.FilterDraining))
            foreach (var map in transCoderMap.SelectMany(kv => kv.Value)) _ = map.InputFilter?.SendFrame(null);
        drainState |= DrainState.FilterDraining;
        AVResult32 res;
        foreach (var map in transCoderMap.SelectMany(kv => kv.Value))
        {
            if (map.InputFilter != null)
            {
                res = map.OutputFilter!.ReceiveFrame(filteredFrame);
                if (res == AVResult32.EndOfFile) continue;
                if (res.IsError) return res;
                res = StepConvert(map, Sink.CodecContexts[map.OutputIndex]!);
                if (res.IsError) return res;
                res = StepEncode(map, Sink.CodecContexts[map.OutputIndex]!);
                if (res.IsTryAgain) continue;
                if (res.IsError) return res;
                res = Sink.WritePacket(writePacket);
                return res;
            }
        }
        drainState |= DrainState.FilterDrained;
        return AVResult32.TryAgain;
    }

    private AVResult32 ProcessFrame(int sourceIndex)
    {
        AVResult32 res = 0;
        if (!transCoderMap.TryGetValue(sourceIndex, out var maps))
            return AVResult32.TryAgain;
        foreach (var map in maps)
        {

            var res2 = ProcessFrame(map);
            if (res2.IsError) res = res2;
            if (res.IsError && !res.IsTryAgain)
                return res;

            res = Sink.WritePacket(writePacket);
            if (res.IsError) return res;
        }
        return res;

    }

    private AVResult32 ReadFrameDraining()
    {
        AVResult32 res = AVResult32.EndOfFile;
        if (drainState.HasFlag(DrainState.DecoderDrained)) return AVResult32.EndOfFile;
        foreach (var map in transCoderMap.SelectMany(kv => kv.Value))
            if (Sink.CodecContexts[map.OutputIndex] != null)
            {
                if (Source.CodecContexts[map.InputIndex].CodecType is MediaType.Video or MediaType.Video)
                    res = Source.CodecContexts[map.InputIndex].ReceiveFrame(readFrame);
                else continue;
                if (res == AVResult32.EndOfFile) continue;
                if (res.IsError) return res;
                return map.InputIndex;
            }
        drainState |= DrainState.DecoderDrained;
        return res;
    }

    private AVResult32 ProcessJustCopy()
    {
        AVResult32 res = 0;
        int sourceIndex = readPacket.StreamIndex;
        if (!transCoderMap.TryGetValue(sourceIndex, out var maps))
            return AVResult32.TryAgain;
        foreach (var map in maps)
        {
            int sinkIndex = map.OutputIndex;
            if (Sink.CodecContexts[sinkIndex] == null)
                PrepareCopyPacket(sinkIndex);
            else throw new NotImplementedException("Decoding/Encoding, filtering for other stream types is not supported");
            res = Sink.WritePacket(writePacket);
            if (res.IsError) return res;
        }
        return res;
    }

    private AVResult32 ProcessPacketSubtitle()
    {
        AVResult32 res = 0;
        int sourceIndex = readPacket.StreamIndex;
        if (!transCoderMap.TryGetValue(sourceIndex, out var maps))
            return AVResult32.TryAgain;
        bool subtitleDecoded = false;
        foreach (var map in maps)
        {
            int sinkIndex = map.OutputIndex;
            if (Sink.CodecContexts[sinkIndex] == null)
                PrepareCopyPacket(sinkIndex);
            else
            {
                if (!subtitleDecoded)
                {
                    res = Source.DecodeSubtitle(readPacket, subtitle);
                    if (res.IsError) return res;
                }
                // decode
                var res2 = ProcessSubtitle(map);
                if (res2.IsError) res = res2;
                if (res.IsError && !res.IsTryAgain)
                    return res;
            }
            res = Sink.WritePacket(writePacket);
            if (res.IsError) return res;
        }
        return 0;
    }

    private AVResult32 ProcessSubtitle(TranscodingMap map)
    {
        throw new NotImplementedException("Not yet implemented"); // ToDo
    }

    private AVResult32 ProcessPacketAV()
    {
        AVResult32 res = 0;
        int sourceIndex = readPacket.StreamIndex;
        if (!transCoderMap.TryGetValue(sourceIndex, out var maps))
            return AVResult32.TryAgain;
        bool frameDecoded = false;
        int updateIndexOnCopy = -1;
        foreach (var map in maps)
        {
            int sinkIndex = map.OutputIndex;
            if (Sink.CodecContexts[sinkIndex] == null)
            {
                PrepareCopyPacket(sinkIndex);
                if (PreviewOutputStreamIndex == sinkIndex)
                    updateIndexOnCopy = map.InputIndex;
            }
            else
            {
                if (!frameDecoded)
                {
                    res = Source.Decode(readPacket, readFrame);
                    if (res.IsError) return res;
                }
                // decode
                var res2 = ProcessFrame(map);
                if (res2.IsError) res = res2;
                if (res.IsError && !res.IsTryAgain)
                    return res;
                if (res.IsTryAgain) continue;
            }
            if(writePacket.StreamIndex == PreviewOutputStreamIndex)
                CurrentTimestamp = writePacket.PresentationTime;
            res = Sink.WritePacket(writePacket);
            if (res.IsError) return res;
        }
        if (updateIndexOnCopy >= 0)
            if (!frameDecoded)
                UpdatePreviewDecode(updateIndexOnCopy);
            else
            {
                PreviewFrame.Reference(readFrame);
            }
        return res;
    }
    private void UpdatePreview()
    {
        PreviewFrame.Reference(convertedFrame);
    }
    private void UpdatePreviewDecode(int sourceIndex)
    {
        if (lastUpdate + UpdateInterval < DateTime.Now && UpdateInterval >= TimeSpan.Zero)
        {
            CodecContext decoder = Source.CodecContexts[sourceIndex];
            var res = decoder.Decode(readPacket, readFrame);
            if (res.IsError) return;
            PreviewFrame.Reference(readFrame);
            decoder.FlushBuffers();
            lastUpdate = DateTime.Now;
        }
    }

    private AVResult32 ProcessFrame(TranscodingMap map)
    {
        var codecCtx = Sink.CodecContexts[map.OutputIndex]!;
        convertedFrame.Unreference();
        AVResult32 res;
        if (map.InputFilter != null)
        {
            res = map.InputFilter.SendFrame(readFrame);
            if (res.IsError) return res;
            res = map.OutputFilter!.ReceiveFrame(filteredFrame);
            if (res.IsError) return res;
        }
        else filteredFrame.Reference(readFrame);

        res = StepConvert(map, codecCtx);
        if (res.IsError) return res;

        if (!res.IsTryAgain && map.OutputIndex == PreviewOutputStreamIndex)
            UpdatePreview();

        res = StepEncode(map, codecCtx);
        return res;

    }

    private AVResult32 StepEncode(TranscodingMap map, CodecContext codecCtx)
    {
        var res = codecCtx.SendFrame(convertedFrame);
        if (res.IsError) return res;
        res = codecCtx.ReceivePacket(writePacket);
        if (res.IsError) return res;
        writePacket.PresentationTimestamp -= start / writePacket.TimeBase;
        writePacket.DecompressionTimestamp -= start / writePacket.TimeBase;
        writePacket.StreamIndex = map.OutputIndex;
        return res;
    }

    private AVResult32 StepConvert(TranscodingMap map, CodecContext codecCtx)
    {
        AVResult32 res = 0;
        if (filteredFrame.IsVideo &&
                (filteredFrame.Width != codecCtx.Width ||
                 filteredFrame.Height != codecCtx.Height ||
                 filteredFrame.PixelFormat != codecCtx.PixelFormat))
        {
            res = (map.ImageScaler = SwsContext.CheckContext(map.ImageScaler, filteredFrame, codecCtx)).Convert(filteredFrame, convertedFrame);
        }
        else if (filteredFrame.IsAudio &&
                    (filteredFrame.SampleFormat != codecCtx.SampleFormat ||
                     filteredFrame.SampleRate != codecCtx.SampleRate ||
                     filteredFrame.ChannelLayout != codecCtx.ChannelLayout))
        {
            map.AudioResampler ??= new(filteredFrame.ChannelLayout, filteredFrame.SampleFormat, filteredFrame.SampleRate, codecCtx.ChannelLayout, codecCtx.SampleFormat, codecCtx.SampleRate);
            res = map.AudioResampler.Convert(filteredFrame, convertedFrame);
        }
        else convertedFrame.Reference(filteredFrame);
        return res;
    }

    private void PrepareCopyPacket(int sinkIndex)
    {
        writePacket.Unreference();
        readPacket.CloneTo(writePacket);
        writePacket.PresentationTimestamp -= start / writePacket.TimeBase;
        writePacket.DecompressionTimestamp -= start / writePacket.TimeBase;
        writePacket.StreamIndex = sinkIndex;
        writePacket.RescaleTS(Sink.Streams[sinkIndex].TimeBase);
    }

    private AVResult32 ReadPacket()
    {
        while (Source.Streams.Any(s => !s.Discard.HasFlag(DiscardFlags.All)))
        {
            var res = Source.ReadPacket(readPacket);
            if (res.IsError)
                return res;
            if (readPacket.DecompressionTime > start + duration && duration > TimeSpan.Zero)
            {
                res = AVResult32.EndOfFile;
                Source.Streams[readPacket.StreamIndex].Discard = DiscardFlags.All;
                readPacket.Unreference();
            }
            else if (!Source.Streams[readPacket.StreamIndex].Discard.HasFlag(DiscardFlags.All)) return res; // packet to decode <3
        }
        return AVResult32.EndOfFile;
    }


    #region Dispose
    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Stop();
                tokenSource?.Dispose();
                Source.Dispose();
                Sink.Dispose();
                readFrame.Dispose();
                convertedFrame.Dispose();
                readPacket.Dispose();
                writePacket.Dispose();
                subtitle.Dispose();
                PreviewFrame.Dispose();
                foreach (var map in transCoderMap.SelectMany(kv => kv.Value)) map.Dispose();
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            disposedValue = true;
        }
    }

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~Transcoder()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Start/End
    Task? runningTask;
    CancellationTokenSource? tokenSource;
    private object sync = new();
    // call either ProcessNextPackage or 
    public TaskStatus TaskState => runningTask?.Status ?? TaskStatus.Created;
    public System.Runtime.CompilerServices.TaskAwaiter GetAwaiter()
    {
        if (runningTask == null) return Task.CompletedTask.GetAwaiter();
        return runningTask.GetAwaiter();
    }
    public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => runningTask?.ConfigureAwait(continueOnCapturedContext) ?? Task.CompletedTask.ConfigureAwait(continueOnCapturedContext);
    public void Start()
    {
        lock (sync)
        {
            if (tokenSource != null)
            {
                if (runningTask!.Status is TaskStatus.Running or TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun)
                    throw new InvalidOperationException("The task is already running");
                tokenSource.Cancel();
                tokenSource.Dispose();                
            }
            tokenSource = new();
            runningTask = Task.Run(() => Run(tokenSource.Token), tokenSource.Token);
        }
    }

    private void Run(CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            if(ProcessNextPacket() == AVResult32.EndOfFile) // ignore all error codes
            {
                WriteTrailer().ThrowIfError();
                return;
            }
        }
    }

    public void Stop()
    {
        lock(sync)
        {
            if (tokenSource == null) return;
            tokenSource.Cancel();
        }
    }

    #endregion


    private struct TranscodingMap
    {
        public int InputIndex;
        public int OutputIndex;
        public FilterContext? InputFilter;
        public FilterContext? OutputFilter;
        FilterGraph? Filter;
        public SwsContext? ImageScaler;
        public SwrContext? AudioResampler;
        public void Dispose()
        {
            Filter?.Dispose();
            ImageScaler?.Dispose();
            AudioResampler?.Dispose();
        }

        public static TranscodingMap Create(int inIndex, int outIndex)
        {
            return new()
            {
                InputIndex = inIndex,
                OutputIndex = outIndex,
            };
        }

        public static TranscodingMap Create(int inIndex, int outIndex, FilterGraph graph)
        {
            return new()
            {
                InputIndex = inIndex,
                OutputIndex = outIndex,
                InputFilter = graph.Filters.First(ctx => ctx.Name == "in"),
                OutputFilter = graph.Filters.First(ctx => ctx.Name == "out"),
            };
        }

        public static TranscodingMap Create(int inIndex, int outIndex, FilterGraph graph, SwsContext converter)
        {
            return new()
            {
                InputIndex = inIndex,
                OutputIndex = outIndex,
                InputFilter = graph.Filters.First(ctx => ctx.Name == "in"),
                OutputFilter = graph.Filters.First(ctx => ctx.Name == "out"),
                ImageScaler = converter
            };
        }

        public static TranscodingMap Create(int inIndex, int outIndex, FilterGraph graph, SwrContext converter)
        {
            return new()
            {
                InputIndex = inIndex,
                OutputIndex = outIndex,
                InputFilter = graph.Filters.First(ctx => ctx.Name == "in"),
                OutputFilter = graph.Filters.First(ctx => ctx.Name == "out"),
                AudioResampler = converter
            };
        }

        public static TranscodingMap Create(int inIndex, int outIndex, SwsContext converter)
        {
            return new()
            {
                InputIndex = inIndex,
                OutputIndex = outIndex,
                ImageScaler = converter
            };
        }

        public static TranscodingMap Create(int inIndex, int outIndex, SwrContext converter)
        {
            return new()
            {
                InputIndex = inIndex,
                OutputIndex = outIndex,
                AudioResampler = converter
            };
        }

    }
}
