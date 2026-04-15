// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Threading;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    public class WebViewEditor : UserControl
    {
        private NativeWebView _webView;
        private bool _isReady;
        private string _pendingHtml;
        private string _editorHtml;

#pragma warning disable CS0067
        public event EventHandler<FormatState> FormatStateChanged;
        public event EventHandler ContentChanged;
#pragma warning restore CS0067

        public WebViewEditor()
        {
            LoadEditorHtmlResource();
            InitializeWebView();
        }

        private void LoadEditorHtmlResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "OpenLiveWriter.App.Avalonia.Editor.Resources.editor.html";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        _editorHtml = reader.ReadToEnd();
                    }
                }
            }
        }

        private void InitializeWebView()
        {
            try
            {
                _webView = new NativeWebView();

                // Wait for the adapter to be created before navigating
                _webView.AdapterCreated += OnAdapterCreated;
                _webView.NavigationCompleted += OnNavigationCompleted;

                Content = _webView;
                Trace.WriteLine("[OLW-WebView] NativeWebView created, waiting for adapter...");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[OLW-WebView] Failed to create NativeWebView: {ex.Message}");
                Content = CreateFallbackEditor();
            }
        }

        private async void OnAdapterCreated(object sender, EventArgs e)
        {
            Trace.WriteLine("[OLW-WebView] Adapter created, navigating to editor HTML...");
            try
            {
                if (_editorHtml != null)
                {
                    // Write HTML to a temp file and navigate to it (more reliable than NavigateToString)
                    string tempDir = Path.Combine(Path.GetTempPath(), "OpenLiveWriter", "editor");
                    Directory.CreateDirectory(tempDir);
                    string tempFile = Path.Combine(tempDir, "editor.html");
                    await File.WriteAllTextAsync(tempFile, _editorHtml);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _webView.Navigate(new Uri("file://" + tempFile));
                        Trace.WriteLine($"[OLW-WebView] Navigating to: file://{tempFile}");
                    });
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[OLW-WebView] Navigation failed: {ex.Message}");
            }
        }

        private void OnNavigationCompleted(object sender, EventArgs e)
        {
            _isReady = true;
            Trace.WriteLine("[OLW-WebView] Navigation completed, editor is ready");

            // Apply any pending content
            if (_pendingHtml != null)
            {
                SetContent(_pendingHtml);
                _pendingHtml = null;
            }
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
                    Text = "WebView not available \u2014 using plain text editor.",
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

        private async Task InvokeScriptSafe(string script)
        {
            if (_webView == null || !_isReady) return;
            try
            {
                await _webView.InvokeScript(script);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[OLW-WebView] InvokeScript failed: {ex.Message}");
            }
        }

        public void ExecCommand(string command, string value = null)
        {
            if (_webView == null || !_isReady) return;
            string js = $"OLWBridge.execCommand('{command}', {(value != null ? $"'{value}'" : "null")})";
            _ = InvokeScriptSafe(js);
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

        public void InsertHtml(string html)
        {
            if (_webView == null || !_isReady) return;
            string escaped = html.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n");
            _ = InvokeScriptSafe($"OLWBridge.insertHtml('{escaped}')");
        }

        public void SetContent(string html)
        {
            if (_webView == null || !_isReady)
            {
                _pendingHtml = html;
                return;
            }
            string escaped = html.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n");
            _ = InvokeScriptSafe($"OLWBridge.setContent('{escaped}')");
        }

        public async void GetContent(Action<string> callback)
        {
            if (_webView == null || !_isReady) { callback?.Invoke(null); return; }
            try
            {
                var result = await _webView.InvokeScript("OLWBridge.getContent()");
                callback?.Invoke(result);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[OLW-WebView] GetContent failed: {ex.Message}");
                callback?.Invoke(null);
            }
        }

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
