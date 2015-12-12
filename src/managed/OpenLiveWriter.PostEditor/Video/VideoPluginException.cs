// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Video
{
    class VideoPluginException : ApplicationException
    {
        public VideoPluginException(string title, string description)
            : base(String.Format(CultureInfo.InvariantCulture, "{0} - {1}", title, description))
        {
            _title = title;
            _description = description;
        }

        public string Title { get { return _title; } }
        public string Description { get { return _description; } }

        private string _title;
        private string _description;
    }

    class VideoPluginUnexpectedException : VideoPluginException
    {
        public VideoPluginUnexpectedException(Exception ex)
            : base(Res.Get(StringId.Plugin_Video_Error_Unexpected_Title), ex.ToString())
        {
        }

    }
}
