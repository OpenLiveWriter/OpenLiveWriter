using System;
using System.Windows.Forms;

using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.Tests.PostEditor.Tables
{
    class TestHtmlEditor : IHtmlEditor
    {

        public string Html { get; set; } = string.Empty;

        public void Dispose()
        {
        }

        public Control EditorControl { get; }

        public void LoadHtmlFile(string filePath)
        {
        }

        public string GetEditedHtml(bool preferWellFormed)
        {
            return null;
        }

        public string GetEditedHtmlFast()
        {
            return null;
        }

        public string SelectedText { get; }
        public string SelectedHtml { get; }
        public void EmptySelection()
        {
        }

        public void InsertHtml(string content, bool moveSelectionRight)
        {
            Html += content;
        }

        public void InsertHtml(string content, HtmlInsertionOptions options)
        {
        }

        public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
        }

        public bool IsDirty { get; set; }
        public IHtmlEditorCommandSource CommandSource { get; }

#pragma warning disable CS0067
        public event EventHandler IsDirtyEvent;
        public bool SuspendAutoSave { get; }
    }
}