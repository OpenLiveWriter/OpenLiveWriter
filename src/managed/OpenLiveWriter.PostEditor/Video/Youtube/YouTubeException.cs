// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;

namespace OpenLiveWriter.PostEditor.Video.YouTube
{
    class YouTubeException : VideoServiceException
    {
        public YouTubeException(string title, string description)
            : base(title, description)
        {
        }
    }
}
