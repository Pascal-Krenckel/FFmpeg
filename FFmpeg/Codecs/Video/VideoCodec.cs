using System;
using System.Collections.Generic;
using System.Linq;
using FFmpeg.Images;
using FFmpeg.Utils;

namespace FFmpeg.Codecs.Video
{
    /// <summary>
    /// Represents a base class for configuring and creating video codec contexts.
    /// This abstract class provides common functionality for different video codecs.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="VideoCodec"/> class with specified codec, width, height, and time base.
    /// </remarks>
    /// <param name="codec">The codec associated with this video codec configuration.</param>
    /// <param name="width">The width of the video frame in pixels.</param>
    /// <param name="height">The height of the video frame in pixels.</param>
    /// <param name="timeBase">The time base used for timestamp calculations.</param>
    public abstract class VideoCodec(Codec codec, int width, int height, Rational timeBase)
    {
        /// <summary>
        /// Gets or sets the width of the video frame in pixels.
        /// </summary>
        public int Width { get; set; } = width;

        /// <summary>
        /// Gets or sets the height of the video frame in pixels.
        /// </summary>
        public int Height { get; set; } = height;

        /// <summary>
        /// Gets the codec associated with this video codec configuration.
        /// </summary>
        public Codec Codec { get; private set; } = codec;

        /// <summary>
        /// Gets or sets the time base used for timestamp calculations in the video stream.
        /// </summary>
        public Rational TimeBase { get; set; } = timeBase;

        /// <summary>
        /// Gets or sets the pixel format used for encoding or decoding the video frames.
        /// Defaults to <see cref="PixelFormat.None"/> if not specified.
        /// </summary>
        public PixelFormat PixelFormat { get; set; } = PixelFormat.None;

        /// <summary>
        /// Adds codec-specific options to the codec context.
        /// This method must be implemented by derived classes to specify additional codec options.
        /// </summary>
        /// <param name="context">The codec context to which options will be added.</param>
        protected abstract void AddCodecOptions(CodecContext context);

        /// <summary>
        /// Creates a new <see cref="CodecContext"/> configured with the current settings.
        /// </summary>
        /// <returns>A configured <see cref="CodecContext"/> instance.</returns>
        public CodecContext CreateCodecContext()
        {
            var context = new CodecContext(Codec)
            {Width = Width,Height = Height,TimeBase = TimeBase,PixelFormat = PixelFormat};

            // Ensure that the selected pixel format is supported by the codec
            if (!Codec.SupportedPixelFormats.Contains(PixelFormat))
            {
                context.PixelFormat = Codec.SupportedPixelFormats.FirstOrDefault(PixelFormat.None);
            }
            // Notify additional parameters and apply codec-specific options
            context.SetOption(AdditionalParameters).ThrowIfError();
            AddCodecOptions(context);
            return context;
        }

        /// <summary>
        /// Gets a dictionary of additional parameters to be set on the codec context.
        /// Parameters set here will be applied during the creation of the codec context.
        /// </summary>
        public Dictionary<string, string> AdditionalParameters { get; } = [];
    }
}
