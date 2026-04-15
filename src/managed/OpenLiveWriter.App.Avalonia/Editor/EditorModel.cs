// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Simple editor model that tracks content and supports basic formatting.
    /// This is a stepping stone — M4 will replace with WebView-based editor.
    /// </summary>
    public class EditorModel
    {
        public string Title { get; set; } = "";
        public string HtmlContent { get; set; } = "";
        public string PlainTextContent { get; set; } = "";

        public event EventHandler ContentChanged;

        public void SetContent(string html, string plainText)
        {
            HtmlContent = html;
            PlainTextContent = plainText;
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
