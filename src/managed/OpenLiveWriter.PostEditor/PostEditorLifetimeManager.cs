// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor
{

    public sealed class PostEditorLifetimeManager
    {
        /// <summary>
        /// global initialization which may show error dialogs or cause
        /// failure of the entire product to load
        /// </summary>
        public static bool Initialize()
        {
            // can show error dialog if plugin has missing or incorrect attributes
            ContentSourceManager.Initialize();

            // auto-update manager
            //BlogSettingsAutoUpdateManager.Instance.Startup();

            return true;
        }

        public static void Uninitialize()
        {
            EditingTemplateLoader.CleanupUnusedTemplates();

            //BlogSettingsAutoUpdateManager.Instance.Shutdown();
        }
    }
}
