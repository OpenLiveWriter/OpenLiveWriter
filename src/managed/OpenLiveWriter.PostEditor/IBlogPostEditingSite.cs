// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.HtmlEditor.Controls;
using OpenLiveWriter.PostEditor.Configuration;
using OpenLiveWriter.Interop.Com.Ribbon;

namespace OpenLiveWriter.PostEditor
{
    public delegate void WeblogHandler(string blogId);

    public delegate void WeblogSettingsChangedHandler(string blogId, bool templateChanged);

    public interface IBlogPostEditingSite
    {
        IMainFrameWindow FrameWindow { get; }
        IUIFramework RibbonFramework { get; }

        string CurrentAccountId { get; }
        event WeblogHandler WeblogChanged;
        event EventHandler WeblogListChanged;
        event WeblogSettingsChangedHandler WeblogSettingsChanged;
        event WeblogSettingsChangedHandler GlobalWeblogSettingsChanged;

        // allow others to notify the site of independent edits
        // to weblog settings and/or the weblog account list)
        void NotifyWeblogSettingsChanged(bool templateChanged);
        void NotifyWeblogSettingsChanged(string blogId, bool templateChanged);
        void NotifyWeblogAccountListEdited();

        void ConfigureWeblog(string blogId, Type selectedPanel);
        void ConfigureWeblogFtpUpload(string blogId);

        bool UpdateWeblogTemplate(string blogId);
        void AddWeblog();

        void OpenLocalPost(PostInfo postInfo);
        void DeleteLocalPost(PostInfo postInfo);
        event EventHandler PostListChanged;

        IHtmlStylePicker StyleControl { get; }

        CommandManager CommandManager { get; }
    }
}
