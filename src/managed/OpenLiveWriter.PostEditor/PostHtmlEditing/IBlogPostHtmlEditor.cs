// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for IPostHtmlEditor.
    /// </summary>
    internal interface IBlogPostHtmlEditor : IHtmlEditor
    {
        void Focus();
        void FocusTitle();
        void FocusBody();
        bool DocumentHasFocus();

        IFocusableControl FocusControl { get; }

        void LoadHtmlFragment(string title, string postBodyHtml, string baseUrl, BlogEditingTemplate editingTemplate);
        string GetEditedTitleHtml();
        bool FullyEditableRegionActive { get; set; }
        event EventHandler TitleChanged;
        event EventHandler EditableRegionFocusChanged;

        void UpdateEditingContext();
        void InsertExtendedEntryBreak();
        void InsertHorizontalLine(bool plainText);
        void InsertClearBreak();
        void ChangeSelection(SelectionPosition position);
        SmartContentEditor CurrentEditor { get; }

    }
}
