using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpegTest;

using System.Text.Json.Serialization;

using System;
using System.Text.Json.Serialization;

public class FFProbeJson
{
    private static readonly JsonSerializerOptions OPTIONS = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    [JsonPropertyName("streams")]
    public Stream[] Streams { get; set; } = [];

    [JsonPropertyName("format")]
    public Format Format { get; set; } = new Format();

    public static FFProbeJson? Parse(string json) => JsonSerializer.Deserialize<FFProbeJson>(json, OPTIONS);
    public static FFProbeJson? Parse(System.IO.Stream stream) => JsonSerializer.Deserialize<FFProbeJson>(stream, OPTIONS);
}

public class Format
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("nb_streams")]
    public int NbStreams { get; set; } = 0;

    [JsonPropertyName("nb_programs")]
    public int NbPrograms { get; set; } = 0;

    [JsonPropertyName("nb_stream_groups")]
    public int NbStreamGroups { get; set; } = 0;

    [JsonPropertyName("format_name")]
    public string FormatName { get; set; } = string.Empty;

    [JsonPropertyName("format_long_name")]
    public string FormatLongName { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public double StartTime { get; set; } = 0.0;

    [JsonPropertyName("duration")]
    public double Duration { get; set; } = 0.0;

    [JsonPropertyName("size")]
    public long Size { get; set; } = 0L;

    [JsonPropertyName("bit_rate")]
    public long BitRate { get; set; } = 0L;

    [JsonPropertyName("probe_score")]
    public int ProbeScore { get; set; } = 0;

    [JsonPropertyName("tags")]
    public FormatTags Tags { get; set; } = new FormatTags();
}

public class FormatTags
{
    [JsonPropertyName("major_brand")]
    public string MajorBrand { get; set; } = string.Empty;

    [JsonPropertyName("minor_version")]
    public string MinorVersion { get; set; } = string.Empty;

    [JsonPropertyName("compatible_brands")]
    public string CompatibleBrands { get; set; } = string.Empty;

    [JsonPropertyName("encoder")]
    public string Encoder { get; set; } = string.Empty;
}

public class Stream
{
    [JsonPropertyName("index")]
    public int Index { get; set; } = 0;

    [JsonPropertyName("codec_name")]
    public string CodecName { get; set; } = string.Empty;

    [JsonPropertyName("codec_long_name")]
    public string CodecLongName { get; set; } = string.Empty;

    [JsonPropertyName("profile")]
    public string Profile { get; set; } = string.Empty;

    [JsonPropertyName("codec_type")]
    public string CodecType { get; set; } = string.Empty;

    [JsonPropertyName("codec_tag_string")]
    public string CodecTagString { get; set; } = string.Empty;

    [JsonPropertyName("codec_tag")]
    public string CodecTag { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; } = 0;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 0;

    [JsonPropertyName("coded_width")]
    public int CodedWidth { get; set; } = 0;

    [JsonPropertyName("coded_height")]
    public int CodedHeight { get; set; } = 0;

    [JsonPropertyName("has_b_frames")]
    public int HasBFrames { get; set; } = 0;

    [JsonPropertyName("sample_aspect_ratio")]
    public string SampleAspectRatio { get; set; } = string.Empty;

    [JsonPropertyName("display_aspect_ratio")]
    public string DisplayAspectRatio { get; set; } = string.Empty;

    [JsonPropertyName("pix_fmt")]
    public string PixFmt { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int Level { get; set; } = 0;

    [JsonPropertyName("chroma_location")]
    public string ChromaLocation { get; set; } = string.Empty;

    [JsonPropertyName("field_order")]
    public string FieldOrder { get; set; } = string.Empty;

    [JsonPropertyName("refs")]
    public int Refs { get; set; } = 0;

    [JsonPropertyName("is_avc")]
    public string IsAvc { get; set; } = string.Empty;

    [JsonPropertyName("nal_length_size")]
    public int NalLengthSize { get; set; } = 0;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("r_frame_rate")]
    public string RFrameRate { get; set; } = string.Empty;

    [JsonPropertyName("avg_frame_rate")]
    public string AvgFrameRate { get; set; } = string.Empty;

    [JsonPropertyName("time_base")]
    public string TimeBase { get; set; } = string.Empty;

    [JsonPropertyName("start_pts")]
    public long StartPts { get; set; } = 0L;

    [JsonPropertyName("start_time")]
    public double StartTime { get; set; } = 0.0;

    [JsonPropertyName("duration_ts")]
    public long DurationTs { get; set; } = 0L;

    [JsonPropertyName("duration")]
    public double Duration { get; set; } = 0.0;

    [JsonPropertyName("bit_rate")]
    public long BitRate { get; set; } = 0L;

    [JsonPropertyName("bits_per_raw_sample")]
    public int BitsPerRawSample { get; set; } = 0;

    [JsonPropertyName("nb_frames")]
    public long NbFrames { get; set; } = 0L;

    [JsonPropertyName("extradata_size")]
    public int ExtradataSize { get; set; } = 0;

    [JsonPropertyName("disposition")]
    public Disposition Disposition { get; set; } = new Disposition();

    [JsonPropertyName("tags")]
    public StreamTags Tags { get; set; } = new StreamTags();

    [JsonPropertyName("side_data_list")]
    public SideDataList[] SideDataList { get; set; } = new SideDataList[0];
}

public class Disposition
{
    [JsonPropertyName("default")]
    public int Default { get; set; } = 0;

    [JsonPropertyName("dub")]
    public int Dub { get; set; } = 0;

    [JsonPropertyName("original")]
    public int Original { get; set; } = 0;

    [JsonPropertyName("comment")]
    public int Comment { get; set; } = 0;

    [JsonPropertyName("lyrics")]
    public int Lyrics { get; set; } = 0;

    [JsonPropertyName("karaoke")]
    public int Karaoke { get; set; } = 0;

    [JsonPropertyName("forced")]
    public int Forced { get; set; } = 0;

    [JsonPropertyName("hearing_impaired")]
    public int HearingImpaired { get; set; } = 0;

    [JsonPropertyName("visual_impaired")]
    public int VisualImpaired { get; set; } = 0;

    [JsonPropertyName("clean_effects")]
    public int CleanEffects { get; set; } = 0;

    [JsonPropertyName("attached_pic")]
    public int AttachedPic { get; set; } = 0;

    [JsonPropertyName("timed_thumbnails")]
    public int TimedThumbnails { get; set; } = 0;

    [JsonPropertyName("non_diegetic")]
    public int NonDiegetic { get; set; } = 0;

    [JsonPropertyName("captions")]
    public int Captions { get; set; } = 0;

    [JsonPropertyName("descriptions")]
    public int Descriptions { get; set; } = 0;

    [JsonPropertyName("metadata")]
    public int Metadata { get; set; } = 0;

    [JsonPropertyName("dependent")]
    public int Dependent { get; set; } = 0;

    [JsonPropertyName("still_image")]
    public int StillImage { get; set; } = 0;

    [JsonPropertyName("multilayer")]
    public int Multilayer { get; set; } = 0;
}

public class StreamTags
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("handler_name")]
    public string HandlerName { get; set; } = string.Empty;

    [JsonPropertyName("vendor_id")]
    public string VendorId { get; set; } = string.Empty;
}

public class SideDataList
{
    [JsonPropertyName("side_data_type")]
    public string SideDataType { get; set; } = string.Empty;

    [JsonPropertyName("service_type")]
    public int ServiceType { get; set; } = 0;
}

