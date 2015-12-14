// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Host control for Sidebar which allows us to derive from SidebarControl but still
    /// use a PrimaryWorkspaceControl to inherit UI framework functionality
    /// </summary>
    public class ImagePropertiesSidebarHostControl : SidebarControl
    {
        public ImagePropertiesSidebarHostControl(
            ISidebarContext sidebarContext,
            IHtmlEditorComponentContext editorContext,
            IBlogPostImageEditingContext imageEditingContext,
            CreateFileCallback createFileCallback)
        {
            // Instead of creating the image sidebar, we now create the manager for ribbon commands related to image editing.
            _pictureEditingManager = new PictureEditingManager(editorContext, imageEditingContext, createFileCallback);
        }

        public PictureEditingManager PictureEditingManager
        {
            get
            {
                return _pictureEditingManager;
            }
        }

        private readonly PictureEditingManager _pictureEditingManager;

        public override void UpdateView(object htmlSelection, bool force)
        {
            // delegate UpdateView
            PictureEditingManager.UpdateView(htmlSelection);
        }
    }
}
