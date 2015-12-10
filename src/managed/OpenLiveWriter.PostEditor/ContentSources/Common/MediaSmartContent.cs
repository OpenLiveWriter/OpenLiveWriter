// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.ContentSources.Common
{
    public abstract class MediaSmartContent : ForceInvalidateSmartContent
    {
        protected ISmartContent _content;
        protected MediaSmartContent(ISmartContent content) : base(content)
        {
            _content = content;
        }

        /// <summary>
        /// This is the ID that Writer is using internally to identify the smart content
        /// This ID will change over time, so therefore should not be used for anything
        /// other then linking this smart content to the extension data list.  If you keep this
        /// value around for other things like hashing the smart content the value might change
        /// and you will now have a stale value.
        /// </summary>
        public string ContentId
        {
            get
            {
                return ((IInternalContent)_content).Id;
            }
        }

        protected const int DEFAULT_WIDTH = 340;
        protected const int DEFAULT_HEIGHT = 280;
        protected const string HTML_WIDTH = "MediaSmartContent.HtmlSize.Width";
        protected const string HTML_HEIGHT = "MediaSmartContent.HtmlSize.Height";
        protected Size _minSize = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
        protected Size MinSize
        {
            get
            {
                return _minSize;
            }
            set
            {
                _minSize = value;
            }
        }
        public Size HtmlSize
        {
            get
            {
                return ValidSize(new Size(_content.Properties.GetInt(HTML_WIDTH, DEFAULT_WIDTH), _content.Properties.GetInt(HTML_HEIGHT, DEFAULT_HEIGHT)));
            }
            set
            {
                _content.Properties.SetInt(HTML_WIDTH, value.Width);
                _content.Properties.SetInt(HTML_HEIGHT, value.Height);
            }
        }

        public virtual Size DefaultHtmlSize
        {
            get
            {
                return new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            }
        }

        /// <summary>
        /// Sets the HtmlSize to the default value, limiting the width to no more than maxWidth.
        /// </summary>
        /// <param name="maxWidth"></param>
        public void ResetHtmlSize(int maxWidth)
        {
            HtmlSize = new Size(Math.Min(maxWidth, DefaultHtmlSize.Width), DefaultHtmlSize.Height);
        }

        public bool HasSize
        {
            get { return _content.Properties.Contains(HTML_WIDTH) && _content.Properties.Contains(HTML_HEIGHT); }
        }

        private Size ValidSize(Size size)
        {
            if (size.Width < MinSize.Width)
                size.Width = MinSize.Width;
            if (size.Height < MinSize.Height)
                size.Height = MinSize.Height;
            return size;
        }

        public abstract string GeneratePublishHtml(IPublishingContext publishingContext);
        public abstract string GenerateEditorHtml(IPublishingContext publishingContext);

        public void SaveBitmap(Bitmap bitmap, string fileName, ImageFormat imageFormat)
        {
            _content.Files.AddImage(fileName, bitmap, imageFormat);
        }

        public void RemoveBitmap(string fileName)
        {
            _content.Files.Remove(fileName);
        }

    }

    public class ForceInvalidateSmartContent
    {
        public readonly static string FORCEINVALIDATE = "ForceInvalidateSmartContent.ForceInvalidate";

        private ISmartContent _content;
        public ForceInvalidateSmartContent(ISmartContent content)
        {
            _content = content;
        }

        public bool ForceInvalidate
        {
            get
            {
                return _content.Properties.GetBoolean(FORCEINVALIDATE, false);
            }
            set
            {
                _content.Properties.SetBoolean(FORCEINVALIDATE, value);
            }
        }
    }
}
