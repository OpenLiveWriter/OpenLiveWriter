// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.Video.VideoListBox;

namespace OpenLiveWriter.PostEditor.Video.VideoService
{
    public interface IVideo
    {
        string Url { get; }
        string ThumbnailUrl { get; }
        string Title { get; }
        int LengthSeconds { get; }
        string Description { get; }
        Video GetVideo();
    }

    public interface IVideoService : IMediaService
    {
        IVideoRequestType[] SupportedRequests { get; }
        IVideo[] GetVideos(IVideoRequestType requestType, int timeoutMs, int maxPerPage, int page, out int videosAvailable);
    }

    public interface IVideoRequestType : ImageComboBox.IComboItem
    {
        string TypeName { get; }
        string DisplayName { get; }
        Bitmap Icon { get; }
    }
}
