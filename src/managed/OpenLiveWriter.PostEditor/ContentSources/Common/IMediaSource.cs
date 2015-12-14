// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor.ContentSources.Common
{
    public interface IMediaSource : IDisposable
    {
        /// <summary>
        /// Logo that will be shown to the user in certain times
        /// </summary>
        Bitmap Image { get; }
        /// <summary>
        /// An object that can control the authentication for the service
        /// </summary>
        IAuth Auth { get; }
        /// <summary>
        /// Localized name of the service
        /// </summary>
        string ServiceName { get; }
        /// <summary>
        /// An ID that can be used to identify the service.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Initializes the source with the current blog, a dialog it can use when it displays UI, and smart content
        /// that is used to when editing the content
        /// </summary>
        /// <param name="media"></param>
        /// <param name="DialogOwner"></param>
        /// <param name="blogId"></param>
        void Init(MediaSmartContent media, IWin32Window DialogOwner, string blogId);
    }

    public interface IMediaService : IMediaSource
    {

    }

    public interface IMediaPublisher : IMediaSource
    {
        /// <summary>
        /// Lists the files that the user can select to be published to the source
        /// </summary>
        string FileFilter { get; }
    }
}
