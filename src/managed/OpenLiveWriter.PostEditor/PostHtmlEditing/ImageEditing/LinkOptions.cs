// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class LinkOptions : ILinkOptions
    {
        public LinkOptions(bool showInNewWindow, bool useImageViewer, string imageViewerGroupName)
        {
            _showInNewWindow = showInNewWindow;
            _useImageViewer = useImageViewer;
            _imageViewerGroupName = imageViewerGroupName;
        }

        public bool ShowInNewWindow
        {
            get { return _showInNewWindow; }
            set { _showInNewWindow = value; }
        }

        public bool UseImageViewer
        {
            get { return _useImageViewer; }
            set { _useImageViewer = value; }
        }

        public string ImageViewerGroupName
        {
            get { return _imageViewerGroupName; }
            set { _imageViewerGroupName = value; }
        }

        private bool _showInNewWindow;
        private bool _useImageViewer;
        private string _imageViewerGroupName;
    }
}
