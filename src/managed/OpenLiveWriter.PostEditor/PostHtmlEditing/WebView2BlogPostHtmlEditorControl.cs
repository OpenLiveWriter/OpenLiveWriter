// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.WebView2Shim;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// WebView2-based blog post HTML editor that implements IBlogPostHtmlEditor.
    /// This wraps WebView2HtmlEditorControl to provide blog-specific functionality.
    /// </summary>
    internal class WebView2BlogPostHtmlEditorControl : IBlogPostHtmlEditor
    {
        private readonly WebView2HtmlEditorControl _editor;
        private readonly Panel _container;
        private string _baseUrl;
        private string _title;
        private bool _fullyEditableRegionActive;
        private bool _previewDocumentLoaded;

        /// <summary>
        /// When true, LoadHtmlFragment renders the post inside the blog editing
        /// template as a read-only document (Preview mode) instead of the
        /// editable editing shell used by Wysiwyg mode.
        /// </summary>
        public bool PreviewMode { get; set; }

#pragma warning disable CS0067 // Events not used yet
        public event EventHandler TitleChanged;
#pragma warning restore CS0067
        public event EventHandler EditableRegionFocusChanged;
#pragma warning disable CS0067 // Event not used yet
        public event EventHandler IsDirtyEvent;
#pragma warning restore CS0067

        public WebView2BlogPostHtmlEditorControl()
        {
            _container = new Panel { 
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };
            _editor = new WebView2HtmlEditorControl { Dock = DockStyle.Fill };
            _container.Controls.Add(_editor);
            
            // WebView2 editor is always fully editable
            _fullyEditableRegionActive = true;
            
            // Fire EditableRegionFocusChanged when the editor gets focus
            _editor.EditorControl.GotFocus += (s, e) =>
            {
                OnEditableRegionFocusChanged(!PreviewMode);
            };
            
            // Fire EditableRegionFocusChanged when WebView2 finishes loading
            _editor.ReadyForEditing += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] WebView2BlogPostHtmlEditorControl: ReadyForEditing received, firing EditableRegionFocusChanged");
                OnEditableRegionFocusChanged(!PreviewMode);
            };
            
            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] WebView2BlogPostHtmlEditorControl created");
        }
        
        private void OnEditableRegionFocusChanged(bool isFullyEditable)
        {
            _fullyEditableRegionActive = isFullyEditable;
            EditableRegionFocusChanged?.Invoke(this, new EditableRegionFocusChangedEventArgs(isFullyEditable));
        }

        #region IBlogPostHtmlEditor Implementation

        public void Focus()
        {
            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] WebView2BlogPostHtmlEditorControl.Focus called");
            _editor.EditorControl.Focus();
        }

        public void FocusTitle()
        {
            // WebView2 doesn't have separate title region yet - just focus control
            Focus();
        }

        public void FocusBody()
        {
            // Actually focus the body contenteditable element, not just the control
            _editor.FocusBody();
        }

        public bool DocumentHasFocus()
        {
            return _editor.EditorControl.ContainsFocus;
        }

        public IFocusableControl FocusControl => new WebView2FocusableControl(_editor.EditorControl);

        public void LoadHtmlFragment(string title, string postBodyHtml, string baseUrl, BlogEditingTemplate editingTemplate)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] LoadHtmlFragment called - title: '{title}', body: {postBodyHtml?.Length ?? 0} chars, preview: {PreviewMode}");
            
            _title = title ?? "";
            _baseUrl = baseUrl ?? "";
            
            if (PreviewMode && editingTemplate != null)
            {
                LoadPreviewDocument(postBodyHtml, editingTemplate);
                return;
            }
            
            // 0.6.2 parity: an empty title shows the "Enter a post title" hint,
            // rendered via CSS (:empty:before) so it never becomes part of the
            // synced Title value.
            var titlePlaceholder = System.Net.WebUtility.HtmlEncode(String.Format(
                CultureInfo.CurrentCulture, Res.Get(StringId.TitleDefaultText), Res.Get(StringId.PostLower)));

            // For now, just load the body HTML into the editor
            // NOTE: No inline script - listeners are set up via ExecuteScriptAsync after navigation
            var html = $@"<!DOCTYPE html>
<html>
<head>
    <base href=""{_baseUrl}"" />
    <style>
        html, body {{ 
            background-color: #ffffff; 
            color: #000000;
            margin: 0;
            padding: 0;
        }}
        body {{ 
            font-family: Segoe UI, Arial, sans-serif; 
            font-size: 14px; 
            padding: 10px; 
        }}
        #olw-title {{ 
            font-size: 24px; 
            font-weight: bold; 
            margin-bottom: 10px; 
            border-bottom: 1px solid #ccc; 
            padding-bottom: 10px; 
            background-color: #ffffff;
            outline: none;
        }}
        #olw-title:empty:before {{
            content: attr(data-placeholder);
            color: #767676;
            font-weight: normal;
        }}
        #olw-body {{ 
            min-height: 300px; 
            background-color: #ffffff;
            outline: none;
        }}
        [contenteditable]:focus {{
            outline: none;
        }}
    </style>
</head>
<body>
    <div id=""olw-title"" contenteditable=""true"" data-placeholder=""{titlePlaceholder}"">{System.Web.HttpUtility.HtmlEncode(_title)}</div>
    <div id=""olw-body"" contenteditable=""true"">{postBodyHtml}</div>
</body>
</html>";
            
            if (_previewDocumentLoaded)
            {
                // The current document is the read-only preview, which lacks the
                // editing shell elements that LoadHtmlFile patches in place, so
                // navigate to a fresh editing shell instead.
                _previewDocumentLoaded = false;
                _editor.NavigateToHtmlDocument(html);
                return;
            }
            
            // Use temporary file approach - always update the pending path
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"olw_edit_{Guid.NewGuid():N}.html");
            System.IO.File.WriteAllText(tempPath, html);
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] LoadHtmlFragment - wrote {html.Length} chars to {tempPath}");
            _editor.LoadHtmlFile(tempPath);
            // Note: EditableRegionFocusChanged will fire via ReadyForEditing event when navigation completes
        }

        /// <summary>
        /// Renders the post inside the blog editing template as a read-only
        /// document (no contenteditable editing surface) for Preview mode.
        /// </summary>
        private void LoadPreviewDocument(string postBodyHtml, BlogEditingTemplate editingTemplate)
        {
            // Strip the extended-entry break marker so the preview shows the
            // joined (main + extended) post, matching the Mac preview.
            string body = (postBodyHtml ?? String.Empty).Replace("<!--more-->", String.Empty);

            string titleHtml = System.Web.HttpUtility.HtmlEncode(_title);
            string html = editingTemplate.ApplyTemplateToPostHtml(titleHtml, titleHtml, body);

            // Add a base tag so relative resource references in the post resolve.
            html = Regex.Replace(html, "</head>",
                String.Format("<base href=\"{0}\"></head>", _baseUrl), RegexOptions.IgnoreCase);

            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] LoadPreviewDocument - navigating to preview document, {html.Length} chars");
            _previewDocumentLoaded = true;
            _editor.NavigateToHtmlDocument(html);
        }

        public string GetEditedTitleHtml()
        {
            // Get title from WebView2 editor's #olw-title element
            return _editor.GetEditedTitleHtml();
        }

        public bool FullyEditableRegionActive
        {
            get => _fullyEditableRegionActive;
            set => _fullyEditableRegionActive = value;
        }

        public void UpdateEditingContext()
        {
            // No-op for now
        }

        public void InsertExtendedEntryBreak()
        {
            InsertHtml("<!--more-->", true);
        }

        public void InsertHorizontalLine(bool plainText)
        {
            InsertHtml("<hr />", true);
        }

        public void InsertClearBreak()
        {
            InsertHtml("<br clear=\"all\" />", true);
        }

        public void ChangeSelection(SelectionPosition position)
        {
            // TODO: Implement selection positioning
        }

        public SmartContentEditor CurrentEditor => null;

        #endregion

        #region IHtmlEditor Implementation

        public Control EditorControl => _container;

        public void LoadHtmlFile(string filePath)
        {
            _editor.LoadHtmlFile(filePath);
        }

        public string GetEditedHtml(bool preferWellFormed)
        {
            return _editor.GetEditedHtml(preferWellFormed);
        }

        public string GetEditedHtmlFast()
        {
            return _editor.GetEditedHtmlFast();
        }

        public string SelectedText => _editor.SelectedText;

        public string SelectedHtml => _editor.SelectedHtml;

        public void EmptySelection()
        {
            _editor.EmptySelection();
        }

        public void InsertHtml(string content, bool moveSelectionRight)
        {
            _editor.InsertHtml(content, moveSelectionRight);
        }

        public void InsertHtml(string content, HtmlInsertionOptions options)
        {
            _editor.InsertHtml(content, options);
        }

        public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            _editor.InsertLink(url, linkText, linkTitle, rel, newWindow);
        }

        public bool IsDirty
        {
            get => _editor.IsDirty;
            set => _editor.IsDirty = value;
        }

        public IHtmlEditorCommandSource CommandSource => _editor.CommandSource;

        public bool SuspendAutoSave => false;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _editor?.Dispose();
            _container?.Dispose();
        }

        #endregion

        /// <summary>
        /// Simple focusable control wrapper
        /// </summary>
        private class WebView2FocusableControl : IFocusableControl
        {
            private readonly Control _control;

            public WebView2FocusableControl(Control control)
            {
                _control = control;
            }

            public bool Focus()
            {
                return _control.Focus();
            }

            public bool ContainsFocus => _control.ContainsFocus;
            public bool Visible => _control.Visible;
        }
    }
}
