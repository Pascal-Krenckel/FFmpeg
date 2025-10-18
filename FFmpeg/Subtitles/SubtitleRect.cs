using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FFmpeg.Subtitles;
/// <summary>
/// Represents a rectangular subtitle area and its associated metadata, such as position, size, color count, and text or bitmap data.
/// This struct wraps an <see cref="AutoGen._AVSubtitleRect"/> pointer from FFmpeg.
/// </summary>
public unsafe struct SubtitleRect
{
    internal AutoGen._AVSubtitleRect* rects;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleRect"/> struct with the specified FFmpeg subtitle rectangle.
    /// </summary>
    /// <param name="rects">Pointer to the underlying FFmpeg <see cref="AutoGen._AVSubtitleRect"/>.</param>
    internal SubtitleRect(AutoGen._AVSubtitleRect* rects) => this.rects = rects;

    /// <summary>
    /// Gets or sets the X-coordinate of the top-left corner of the subtitle rectangle.
    /// </summary>
    public int X
    {
        get => rects->x;
        set => rects->x = value;
    }

    /// <summary>
    /// Gets or sets the Y-coordinate of the top-left corner of the subtitle rectangle.
    /// </summary>
    public int Y
    {
        get => rects->y;
        set => rects->y = value;
    }

    /// <summary>
    /// Gets or sets the width of the subtitle rectangle.
    /// </summary>
    public int Width
    {
        get => rects->w;
        set => rects->w = value;
    }

    /// <summary>
    /// Gets or sets the height of the subtitle rectangle.
    /// </summary>
    public int Height
    {
        get => rects->h;
        set => rects->h = value;
    }

    /// <summary>
    /// Gets or sets the number of colors in the subtitle rectangle's color palette.
    /// This is only applicable for bitmap subtitles.
    /// </summary>
    public int ColorCount
    {
        get => rects->nb_colors;
        set => rects->nb_colors = value;
    }

    /// <summary>
    /// Gets a span over the bitmap data associated with the subtitle. 
    /// This span references up to 4 planes of image data for bitmap subtitles.
    /// </summary>
    public Span<IntPtr> BitmapData => new Span<IntPtr>(&rects->data, 4);

    /// <summary>
    /// Gets a span over the line sizes (stride) for each plane of the bitmap data.
    /// This span is used to determine how many bytes each line of the bitmap takes.
    /// </summary>
    public Span<int> LineSize => new Span<int>(&rects->linesize, 4);

    /// <summary>
    /// Gets or sets the plain text subtitle content. 
    /// This is typically used for simple, unformatted subtitles.
    /// </summary>
    public string Text
    {
        get => Marshal.PtrToStringUTF8((nint)rects->text);
        set
        {
            ffmpeg.av_freep(rects->text); // Free the existing text memory
            if (value != null)
                rects->text = ffmpeg.av_strdup(value); // Duplicate the new text string
        }
    }

    /// <summary>
    /// Gets or sets the ASS/SSA formatted subtitle content.
    /// This field is used for complex, formatted subtitles (e.g., with styling, positioning).
    /// </summary>
    public string ASS
    {
        get => Marshal.PtrToStringUTF8((nint)rects->ass);
        set
        {
            ffmpeg.av_freep(rects->ass); // Free the existing ASS memory
            if (value != null)
                rects->ass = ffmpeg.av_strdup(value); // Duplicate the new ASS string
        }
    }

    /// <summary>
    /// Gets or sets the type of the subtitle, indicating whether the subtitle is a bitmap, plain text, or formatted text (e.g., ASS).
    /// </summary>
    public SubtitleType Type
    {
        get => (SubtitleType)rects->type;
        set => rects->type = (AutoGen._AVSubtitleType)value;
    }

    /// <summary>
    /// Returns a string representation of the subtitle. 
    /// If plain text is present, it will return that; otherwise, it returns the ASS-formatted content.
    /// </summary>
    /// <returns>The plain text or ASS subtitle content as a string.</returns>
    public override string ToString()
    {
        if (Text != null) return Text;
        return ASS;
    }
}
