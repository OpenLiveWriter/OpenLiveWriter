// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.PostEditor;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for HtmlGenerator.
    /// </summary>
    internal class HtmlGenerator : BasicHtmlGenerationService
    {
        private IHtmlEditorHost _editorHost;
        internal HtmlGenerator(IHtmlEditorHost editorHost)
            : base(editorHost.DefaultBlockElement)
        {
            _editorHost = editorHost;
        }

        public override string GenerateHtmlFromFiles(string[] files)
        {
            return _editorHost.TransformHtml(base.GenerateHtmlFromFiles(files));
        }

        public override string GenerateHtmlFromLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            return _editorHost.TransformHtml(base.GenerateHtmlFromLink(url, linkText, linkTitle, rel, newWindow));
        }

        public override string GenerateHtmlFromHtmlFragment(string html, string baseUrl)
        {
            return _editorHost.TransformHtml(base.GenerateHtmlFromHtmlFragment(html, baseUrl));
        }

        public override string CleanupHtml(string html, string baseUrl, HtmlCleanupRule cleanupRule)
        {
            html = HtmlCleaner.CleanupHtml(html, baseUrl, true, (cleanupRule == HtmlCleanupRule.Normal ? false : true));
            return _editorHost.TransformHtml(html);
        }

        public override string GenerateHtmlFromPlainText(string text)
        {
            string html = TextHelper.GetHTMLFromText(text, true, _editorHost.DefaultBlockElement);
            return _editorHost.TransformHtml(html);
        }
    }

    public enum ImageInsertEntryPoint
    {
        Inline,
        Album,
        DragDrop,
        ClipboardPaste,
        MockVideo
    }

    public interface IHtmlEditorHost
    {
        void InsertImages(string[] imagePaths, ImageInsertEntryPoint entryPoint);
        string TransformHtml(string html);
        void InsertSmartContentFromFile(string[] files, string contentSourceID, HtmlInsertionOptions insertionOptions, object context);
        bool ShouldComposeHostHandlePhotos();
        DefaultBlockElement DefaultBlockElement { get; }
    }
}
