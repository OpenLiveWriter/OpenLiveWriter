// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// DEFERRED FEATURE STUBS for .NET 10 Migration
// These features are stubbed out but tracked for future implementation.
// See docs/NET10-MIGRATION-STATUS.md for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.PostEditor.Autoreplace
{
    /// <summary>
    /// STUB: Autoreplace preferences panel - deferred for .NET 10 migration.
    /// </summary>
    public class AutoreplacePreferencesPanel : PreferencesPanel
    {
        public AutoreplacePreferencesPanel()
        {
            PanelName = "Auto Replace";
        }

        public override void Save()
        {
            // Stub - no-op
        }
    }

    /// <summary>
    /// STUB: Autoreplace management control - deferred for .NET 10 migration.
    /// </summary>
    public class AutoreplaceManagementControl : UserControl
    {
        public AutoreplaceManagementControl()
        {
        }
    }
}

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// STUB: Video browser form - deferred for .NET 10 migration.
    /// </summary>
    public class VideoBrowserForm : Form
    {
        public VideoBrowserForm()
        {
            Text = "Video Browser (Not Available)";
        }

        public static DialogResult InsertVideo(IWin32Window owner)
        {
            MessageBox.Show(owner, 
                "Video insertion is not yet available in this .NET 10 build.", 
                "Feature Not Available", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
            return DialogResult.Cancel;
        }
    }

    /// <summary>
    /// STUB: Video helper - deferred for .NET 10 migration.
    /// </summary>
    public static class VideoHelper
    {
        public static bool IsVideoFile(string path) => false;
        public static string GetVideoThumbnail(string path) => null;
    }
}

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    /// <summary>
    /// STUB: Default sidebar control - deferred for .NET 10 migration.
    /// </summary>
    public class DefaultSidebarControl : UserControl
    {
        public DefaultSidebarControl()
        {
        }
    }

    /// <summary>
    /// STUB: HTML editor sidebar title - deferred for .NET 10 migration.
    /// </summary>
    public class HtmlEditorSidebarTitle : UserControl
    {
        public HtmlEditorSidebarTitle()
        {
        }
    }

    /// <summary>
    /// STUB: Sidebar gutter - deferred for .NET 10 migration.
    /// </summary>
    public class SidebarGutter : UserControl
    {
        public SidebarGutter()
        {
        }
    }
}
