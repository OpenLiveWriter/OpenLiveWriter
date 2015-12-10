// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.HtmlEditor
{
    [Flags]
    // Make sure this stays in sync with HtmlInsertOptions
    public enum HtmlInsertionOptions
    {
        Default = 0,
        SuppressSpellCheck = 1,
        Indent = 2,
        MoveCursorAfter = 4,
        InsertAtBeginning = 8,
        InsertAtEnd = 16,
        ClearUndoStack = 32,
        PlainText = 64,
        InsertNewLineBefore = 128,
        ExternalContent = 256,
        ApplyDefaultFont = 512,
        SelectFirstControl = 1024,
        AllowBlockBreakout = 2048
    }

    public interface IHtmlEditor : IDisposable
    {
        Control EditorControl { get; }

        void LoadHtmlFile(string filePath);

        string GetEditedHtml(bool preferWellFormed);
        string GetEditedHtmlFast();

        string SelectedText { get; }

        string SelectedHtml { get; }

        void EmptySelection();

        void InsertHtml(string content, bool moveSelectionRight);
        void InsertHtml(string content, HtmlInsertionOptions options);

        void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow);

        bool IsDirty { get; set; }

        IHtmlEditorCommandSource CommandSource { get; }

        event EventHandler IsDirtyEvent;

        /// <summary>
        /// Returns true if the editor is not currently in a state
        /// where an auto-save would be appropriate. The auto-save
        /// will be tried again later.
        /// </summary>
        bool SuspendAutoSave { get; }
    }

}
