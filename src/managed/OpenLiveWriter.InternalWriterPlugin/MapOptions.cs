// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.InternalWriterPlugin
{
    internal class MapOptions
    {
        public MapOptions(IProperties mapOptions)
        {
            _mapOptions = mapOptions;
        }

        public string PushpinUrl
        {
            get { return _mapOptions.GetString(PUSHPIN_URL, String.Empty); }
            set { _mapOptions.SetString(PUSHPIN_URL, value); }
        }
        private const string PUSHPIN_URL = "PushpinUrl";

        public string MoreAboutPushpinsUrl
        {
            get { return _mapOptions.GetString(MORE_ABOUT_PUSHPINS_URL, String.Empty); }
        }
        private const string MORE_ABOUT_PUSHPINS_URL = "MoreAboutPushpinsUrl";

        public string CaptionHtmlFormat
        {
            get { return _mapOptions.GetString(CAPTION_HTML_FORMAT, "<br><label for=\"{1}\" style=\"font-size:.8em;\">{0}</label>"); }
            set { _mapOptions.SetString(CAPTION_HTML_FORMAT, value); }
        }
        private const string CAPTION_HTML_FORMAT = "CaptionFormat";

        public Size GetDefaultMapSize(string blogId)
        {
            IProperties blogProps = GetBlogSettings(blogId);
            int defaultWidth = blogProps.GetInt(DEFAULT_MAP_WIDTH, DefaultMapSize.Width);
            int defaultHeight = blogProps.GetInt(DEFAULT_MAP_HEIGHT, DefaultMapSize.Height);
            return MapSettings.EnsurePositiveSize(new Size(defaultWidth, defaultHeight), DefaultMapSize);
        }

        public void SetDefaultMapSize(string blogId, Size size)
        {
            IProperties blogProps = GetBlogSettings(blogId);
            blogProps.SetInt(DEFAULT_MAP_WIDTH, size.Width);
            blogProps.SetInt(DEFAULT_MAP_HEIGHT, size.Height);
        }
        private const string DEFAULT_MAP_WIDTH = "DefaultMapWidth";
        private const string DEFAULT_MAP_HEIGHT = "DefaultMapHeight";

        private IProperties GetBlogSettings(string blogId)
        {
            return _mapOptions.GetSubProperties(BLOG_SETTINGS).GetSubProperties(blogId);
        }

        private const string BLOG_SETTINGS = "BlogSettings";

        public Size DefaultDialogSize
        {
            get
            {
                int defaultWidth = _mapOptions.GetInt(DEFAULT_DIALOG_WIDTH, 640);
                int defaultHeight = _mapOptions.GetInt(DEFAULT_DIALOG_HEIGHT, 480);
                return new Size(defaultWidth, defaultHeight);
            }
            set
            {
                _mapOptions.SetInt(DEFAULT_DIALOG_WIDTH, value.Width);
                _mapOptions.SetInt(DEFAULT_DIALOG_HEIGHT, value.Height);
            }
        }
        private const string DEFAULT_DIALOG_WIDTH = "DefaultDialogWidth";
        private const string DEFAULT_DIALOG_HEIGHT = "DefaultDialogHeight";

        private IProperties _mapOptions;

        public static Size DefaultMapSize
        {
            get
            {
                return new Size(320, 240);
            }
        }
    }
}
