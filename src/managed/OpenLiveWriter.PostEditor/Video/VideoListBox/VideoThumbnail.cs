// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor.Video.VideoListBox
{
    /// <summary>
    /// Wrapper class for SoapboxThumbnail images that performs the following services:
    ///		- Create and draw an image based on the Stream it is initialized with.
    ///		- Delay creation of the image until the Draw method is called so that
    ///		  Bitmaps are created and used on the UI thread
    ///		- Dispose the image and underlying stream when it is disposed
    /// </summary>
    class VideoThumbnail : IDisposable
    {
        /// <summary>
        /// Initialize a thumbnail based on a Stream containing a bitmap
        /// </summary>
        public VideoThumbnail(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Dispose the thumbnail by disposing the image and its underlying stream
        /// </summary>
        public virtual void Dispose()
        {
            if (_stream != null)
                _stream.Dispose();

            if (_image != null)
                _image.Dispose();
        }

        /// <summary>
        /// Draw the thumbnail by rendering the image in the provided bounds
        /// </summary>
        public virtual void Draw(BidiGraphics g, Font font, Rectangle thumbnailRect)
        {
            g.DrawImage(false, Image, thumbnailRect);
        }

        /// <summary>
        /// Delay creation of the image until it is needed for drawing
        /// (ensures that it is always used on the main UI thread)
        /// </summary>
        protected Image Image
        {
            get
            {
                if (_image == null)
                    _image = new Bitmap(_stream);
                return _image;
            }
        }

        private readonly Stream _stream;
        private Image _image;
    }

    /// <summary>
    /// A thumbnail that includes a base image and a text overlay
    /// </summary>
    class TextOverlayVideoThumbnail : VideoThumbnail
    {
        /// <summary>
        /// Initialize with the underlying image and the text overlay
        /// </summary>
        public TextOverlayVideoThumbnail(Stream stream, string textOverlay)
            : base(stream)
        {
            _textOverlay = textOverlay;

            _overlayFormat.Alignment = StringAlignment.Center;
            _overlayFormat.LineAlignment = StringAlignment.Center;
        }

        /// <summary>
        /// Draw the image and the text overlay
        /// </summary>
        public override void Draw(BidiGraphics g, Font font, Rectangle thumbnailRect)
        {
            // draw the image
            base.Draw(g, font, thumbnailRect);

            // calculate the rectangle for the text then draw it
            Rectangle textRect = new Rectangle(thumbnailRect.X + (int)(thumbnailRect.Width * 0.1F), thumbnailRect.Y, (int)(thumbnailRect.Width * 0.8F), thumbnailRect.Height);
            g.DrawText(_textOverlay, font, textRect, Color.Gray, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
        }

        private readonly StringFormat _overlayFormat = new StringFormat();
        private readonly string _textOverlay;
    }

    /// <summary>
    /// Special "Downloading Preview..." thumbnail
    /// </summary>
    class DownloadingVideoThumbnail : TextOverlayVideoThumbnail
    {
        public DownloadingVideoThumbnail()
            : base(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(DownloadingVideoThumbnail), "Images.DownloadingThumbnail.png"),
                   Res.Get(StringId.Plugin_Video_Soapbox_Thumbnail_Downloading))
        {
        }
    }

    /// <summary>
    /// Special "No Preview Available" thumbnail
    /// </summary>
    class NoAvailableVideoThumbnail : TextOverlayVideoThumbnail
    {
        public NoAvailableVideoThumbnail()
            : base(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(DownloadingVideoThumbnail), "Images.NoAvailableThumbnail.png"),
                   Res.Get(StringId.Plugin_Video_Soapbox_Thumbnail_None))
        {
        }
    }
}
