// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// WYSIWYG editor control that wraps a WebView (WKWebView on macOS) with
    /// a contenteditable HTML page and JS bridge for formatting commands.
    /// Falls back to a plain TextBox if the WebView package is unavailable.
    /// </summary>
    public class WebViewEditor : UserControl
    {
        private object _webView; // NativeWebView - using object to handle if package isn't available
        private bool _isReady;

        // These events will be raised when the JS bridge sends messages back
        // from the WebView. Suppressing CS0067 since the events are part of
        // the public API contract for consumers to subscribe to.
#pragma warning disable CS0067
        public event EventHandler<FormatState> FormatStateChanged;
        public event EventHandler ContentChanged;
#pragma warning restore CS0067

        public WebViewEditor()
        {
            InitializeWebView();
        }

        private void InitializeWebView()
        {
            try
            {
                // Try to create NativeWebView from Avalonia.Controls.WebView package
                var webViewType = Type.GetType("Avalonia.Controls.WebView.NativeWebView, Avalonia.Controls.WebView");
                if (webViewType != null)
                {
                    _webView = Activator.CreateInstance(webViewType);
                    Content = (Control)_webView;

                    // Load the editor HTML
                    LoadEditorHtml();
                    return;
                }
            }
            catch { /* WebView package not available */ }

            // Fallback: show a message that WebView is not available
            // and use the existing TextBox-based editor
            Content = CreateFallbackEditor();
        }

        private Control CreateFallbackEditor()
        {
            var panel = new DockPanel();

            var notice = new Border
            {
                Background = global::Avalonia.Media.Brushes.LightYellow,
                Padding = new Thickness(8),
                Child = new TextBlock
                {
                    Text = "WebView not available \u2014 using plain text editor. Install Avalonia.Controls.WebView for WYSIWYG editing.",
                    FontSize = 12,
                    Foreground = global::Avalonia.Media.Brushes.DarkGoldenrod
                }
            };
            DockPanel.SetDock(notice, Dock.Top);
            panel.Children.Add(notice);

            var textBox = new TextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                FontSize = 16,
                FontFamily = new global::Avalonia.Media.FontFamily("Georgia, Times New Roman, serif"),
                Padding = new Thickness(16),
                BorderThickness = new Thickness(0),
                Background = global::Avalonia.Media.Brushes.White,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            panel.Children.Add(textBox);

            return panel;
        }

        private void LoadEditorHtml()
        {
            // Load editor.html from embedded resource
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "OpenLiveWriter.App.Avalonia.Editor.Resources.editor.html";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string html = reader.ReadToEnd();
                        // Navigate to the HTML content
                        NavigateToString(html);
                    }
                }
            }
        }

        private void NavigateToString(string html)
        {
            if (_webView == null) return;

            try
            {
                // Try NavigateToString method
                var method = _webView.GetType().GetMethod("NavigateToString");
                if (method != null)
                {
                    method.Invoke(_webView, new object[] { html });
                    _isReady = true;
                }
            }
            catch { /* Method not available in this version */ }
        }

        /// <summary>
        /// Execute a document editing command via the JS bridge.
        /// </summary>
        public void ExecCommand(string command, string value = null)
        {
            ExecuteJavaScript($"OLWBridge.execCommand('{command}', {(value != null ? $"'{value}'" : "null")})");
        }

        public void ExecuteBold() => ExecCommand("bold");
        public void ExecuteItalic() => ExecCommand("italic");
        public void ExecuteUnderline() => ExecCommand("underline");
        public void ExecuteStrikethrough() => ExecCommand("strikeThrough");
        public void ExecuteOrderedList() => ExecCommand("insertOrderedList");
        public void ExecuteUnorderedList() => ExecCommand("insertUnorderedList");
        public void ExecuteIndent() => ExecCommand("indent");
        public void ExecuteOutdent() => ExecCommand("outdent");

        public void SetBlockFormat(string tag) => ExecCommand("formatBlock", tag);

        /// <summary>
        /// Insert raw HTML at the current cursor position.
        /// </summary>
        public void InsertHtml(string html)
        {
            string escaped = html.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n");
            ExecuteJavaScript($"OLWBridge.insertHtml('{escaped}')");
        }

        /// <summary>
        /// Replace the entire editor content with the given HTML.
        /// </summary>
        public void SetContent(string html)
        {
            string escaped = html.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n");
            ExecuteJavaScript($"OLWBridge.setContent('{escaped}')");
        }

        /// <summary>
        /// Asynchronously retrieve the current editor HTML content.
        /// </summary>
        public async void GetContent(Action<string> callback)
        {
            var result = await ExecuteJavaScriptAsync("OLWBridge.getContent()");
            callback?.Invoke(result);
        }

        private void ExecuteJavaScript(string script)
        {
            if (_webView == null || !_isReady) return;

            try
            {
                var method = _webView.GetType().GetMethod("ExecuteScriptAsync")
                    ?? _webView.GetType().GetMethod("InvokeScriptAsync");
                method?.Invoke(_webView, new object[] { script });
            }
            catch { /* JS execution not available */ }
        }

        private async System.Threading.Tasks.Task<string> ExecuteJavaScriptAsync(string script)
        {
            if (_webView == null || !_isReady) return null;

            try
            {
                var method = _webView.GetType().GetMethod("ExecuteScriptAsync")
                    ?? _webView.GetType().GetMethod("InvokeScriptAsync");
                if (method != null)
                {
                    var task = method.Invoke(_webView, new object[] { script });
                    if (task is System.Threading.Tasks.Task<string> stringTask)
                        return await stringTask;
                }
            }
            catch { /* silent */ }
            return null;
        }

        /// <summary>
        /// Handle a ribbon CommandId by dispatching to the appropriate formatting method.
        /// Returns true if the command was handled.
        /// </summary>
        public bool HandleCommand(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.Bold: ExecuteBold(); return true;
                case CommandId.Italic: ExecuteItalic(); return true;
                case CommandId.Underline: ExecuteUnderline(); return true;
                case CommandId.Strikethrough: ExecuteStrikethrough(); return true;
                case CommandId.Bullets: ExecuteUnorderedList(); return true;
                case CommandId.Numbers: ExecuteOrderedList(); return true;
                case CommandId.Indent: ExecuteIndent(); return true;
                case CommandId.Outdent: ExecuteOutdent(); return true;
                default: return false;
            }
        }
    }

    /// <summary>
    /// Represents the current formatting state at the cursor position in the editor.
    /// Populated from the JS bridge's queryCommandState results.
    /// </summary>
    public class FormatState
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikethrough { get; set; }
        public bool OrderedList { get; set; }
        public bool UnorderedList { get; set; }
        public string BlockTag { get; set; } = "p";
    }
}
