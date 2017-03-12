// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    public interface IBlogPostContentEditor
    {
        string SelectedText { get; }

        string SelectedHtml { get; }

        bool FullyEditableRegionActive { get; }

        void InsertHtml(string content, bool moveSelectionRight);

        void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow);
    }
}
