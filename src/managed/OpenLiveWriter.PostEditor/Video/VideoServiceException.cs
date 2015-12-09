// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;

namespace OpenLiveWriter.PostEditor.Video.YouTube
{
    class VideoServiceException : ApplicationException
    {
        public VideoServiceException(string title, string description)
            : base(String.Format(CultureInfo.InvariantCulture, "{0} - {1}", title, description))
        {
            _title = title;
            _description = description;
        }

        public string Title { get { return _title; } }
        public string Description { get { return _description; } }

        private readonly string _title;
        private readonly string _description;
    }
}
