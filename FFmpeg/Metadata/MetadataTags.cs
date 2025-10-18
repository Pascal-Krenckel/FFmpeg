namespace FFmpeg.Metadata;

/// <summary>
/// Contains constant string values for metadata tags used in the FFmpeg library.
/// </summary>
public static class MetadataTags
{
    /// <summary>
    /// Name of the set this work belongs to.
    /// </summary>
    public const string Album = "album";

    /// <summary>
    /// Main creator of the set/album, if different from artist.
    /// </summary>
    public const string AlbumArtist = "album_artist";

    /// <summary>
    /// Main creator of the work.
    /// </summary>
    public const string Artist = "artist";

    /// <summary>
    /// Any additional description of the file.
    /// </summary>
    public const string Comment = "comment";

    /// <summary>
    /// Who composed the work, if different from artist.
    /// </summary>
    public const string Composer = "composer";

    /// <summary>
    /// Name of copyright holder.
    /// </summary>
    public const string Copyright = "copyright";

    /// <summary>
    /// Date when the file was created, preferably in ISO 8601.
    /// </summary>
    public const string CreationTime = "creation_time";

    /// <summary>
    /// Date when the work was created, preferably in ISO 8601.
    /// </summary>
    public const string Date = "date";

    /// <summary>
    /// Number of a subset, e.g., disc in a multi-disc collection.
    /// </summary>
    public const string Disc = "disc";

    /// <summary>
    /// Name/settings of the software/hardware that produced the file.
    /// </summary>
    public const string Encoder = "encoder";

    /// <summary>
    /// Person/group who created the file.
    /// </summary>
    public const string EncodedBy = "encoded_by";

    /// <summary>
    /// Original name of the file.
    /// </summary>
    public const string Filename = "filename";

    /// <summary>
    /// Genre of the work.
    /// </summary>
    public const string Genre = "genre";

    /// <summary>
    /// Main language in which the work is performed, preferably in ISO 639-2 format.
    /// Multiple languages can be specified by separating them with commas.
    /// </summary>
    public const string Language = "language";

    /// <summary>
    /// Artist who performed the work, if different from artist.
    /// </summary>
    public const string Performer = "performer";

    /// <summary>
    /// Name of the label/publisher.
    /// </summary>
    public const string Publisher = "publisher";

    /// <summary>
    /// Name of the service in broadcasting (channel name).
    /// </summary>
    public const string ServiceName = "service_name";

    /// <summary>
    /// Name of the service provider in broadcasting.
    /// </summary>
    public const string ServiceProvider = "service_provider";

    /// <summary>
    /// Name of the work.
    /// </summary>
    public const string Title = "title";

    /// <summary>
    /// Number of this work in the set, can be in the form current/total.
    /// </summary>
    public const string Track = "track";

    /// <summary>
    /// The total bitrate of the bitrate variant that the current stream is part of.
    /// </summary>
    public const string VariantBitrate = "variant_bitrate";

    /// <summary>
    /// Gets the string representation of the given <see cref="MetadataTag"/> enum value.
    /// </summary>
    /// <param name="tag">The <see cref="MetadataTag"/> enum value.</param>
    /// <returns>The corresponding string value for the given enum value.</returns>
    public static string GetTagString(MetadataTag tag) => tag switch
    {
        MetadataTag.Album => Album,
        MetadataTag.AlbumArtist => AlbumArtist,
        MetadataTag.Artist => Artist,
        MetadataTag.Comment => Comment,
        MetadataTag.Composer => Composer,
        MetadataTag.Copyright => Copyright,
        MetadataTag.CreationTime => CreationTime,
        MetadataTag.Date => Date,
        MetadataTag.Disc => Disc,
        MetadataTag.Encoder => Encoder,
        MetadataTag.EncodedBy => EncodedBy,
        MetadataTag.Filename => Filename,
        MetadataTag.Genre => Genre,
        MetadataTag.Language => Language,
        MetadataTag.Performer => Performer,
        MetadataTag.Publisher => Publisher,
        MetadataTag.ServiceName => ServiceName,
        MetadataTag.ServiceProvider => ServiceProvider,
        MetadataTag.Title => Title,
        MetadataTag.Track => Track,
        MetadataTag.VariantBitrate => VariantBitrate,
        _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
    };
}

/// <summary>
/// Enum representing metadata tags used in the FFmpeg library.
/// </summary>
public enum MetadataTag
{
    /// <summary>
    /// Name of the set this work belongs to.
    /// </summary>
    Album,

    /// <summary>
    /// Main creator of the set/album, if different from artist.
    /// </summary>
    AlbumArtist,

    /// <summary>
    /// Main creator of the work.
    /// </summary>
    Artist,

    /// <summary>
    /// Any additional description of the file.
    /// </summary>
    Comment,

    /// <summary>
    /// Who composed the work, if different from artist.
    /// </summary>
    Composer,

    /// <summary>
    /// Name of copyright holder.
    /// </summary>
    Copyright,

    /// <summary>
    /// Date when the file was created, preferably in ISO 8601.
    /// </summary>
    CreationTime,

    /// <summary>
    /// Date when the work was created, preferably in ISO 8601.
    /// </summary>
    Date,

    /// <summary>
    /// Number of a subset, e.g., disc in a multi-disc collection.
    /// </summary>
    Disc,

    /// <summary>
    /// Name/settings of the software/hardware that produced the file.
    /// </summary>
    Encoder,

    /// <summary>
    /// Person/group who created the file.
    /// </summary>
    EncodedBy,

    /// <summary>
    /// Original name of the file.
    /// </summary>
    Filename,

    /// <summary>
    /// Genre of the work.
    /// </summary>
    Genre,

    /// <summary>
    /// Main language in which the work is performed, preferably in ISO 639-2 format.
    /// Multiple languages can be specified by separating them with commas.
    /// </summary>
    Language,

    /// <summary>
    /// Artist who performed the work, if different from artist.
    /// </summary>
    Performer,

    /// <summary>
    /// Name of the label/publisher.
    /// </summary>
    Publisher,

    /// <summary>
    /// Name of the service in broadcasting (channel name).
    /// </summary>
    ServiceName,

    /// <summary>
    /// Name of the service provider in broadcasting.
    /// </summary>
    ServiceProvider,

    /// <summary>
    /// Name of the work.
    /// </summary>
    Title,

    /// <summary>
    /// Number of this work in the set, can be in the form current/total.
    /// </summary>
    Track,

    /// <summary>
    /// The total bitrate of the bitrate variant that the current stream is part of.
    /// </summary>
    VariantBitrate
}
