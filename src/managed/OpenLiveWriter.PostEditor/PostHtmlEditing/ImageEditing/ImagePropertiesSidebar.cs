// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class ImagePropertiesSidebar : ISidebar
    {
        public ImagePropertiesSidebar(
            IHtmlEditorComponentContext editorContext,
            IBlogPostImageEditingContext imageEditingContext,
            CreateFileCallback createFileCallback)
        {
            _editorContext = editorContext;
            _dataContext = imageEditingContext;
            _createFileCallback = createFileCallback;
        }

        public bool AppliesToSelection(object htmlSelection)
        {
            // Support new WebView2 selection model
            if (htmlSelection is IEditorSelection editorSelection)
            {
                return SelectionHelper.SelectionIsImage(editorSelection);
            }
            
            // Fall back to old MSHTML selection model
            if (htmlSelection is IHtmlEditorSelection msHtmlSelection)
            {
                return HTMLSelectionHelper.SelectionIsImage(msHtmlSelection.HTMLSelectionObject);
            }
            
            return false;
        }

        public SidebarControl CreateSidebarControl(ISidebarContext sidebarContext)
        {
            return new ImagePropertiesSidebarHostControl(
                sidebarContext, _editorContext, _dataContext, _createFileCallback);
        }

        private IHtmlEditorComponentContext _editorContext;
        private IBlogPostImageEditingContext _dataContext;
        private CreateFileCallback _createFileCallback;
    }
}
