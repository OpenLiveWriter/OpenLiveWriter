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

        public NativeWebView WebView => _webView;
        public bool IsReady => _isReady;

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
                _webView.AdapterCreated += OnAdapterCreated;
                _webView.NavigationCompleted += OnNavigationCompleted;
                Content = _webView;

                // Fallback: if AdapterCreated doesn't fire, try loading after delay
                _ = FallbackLoadAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-WebView] FAILED: {ex.Message}");
                Content = CreateFallbackEditor();
            }
        }

        private async Task FallbackLoadAsync()
        {
            await Task.Delay(3000);
            if (!_isReady)
            {
                Console.WriteLine("[OLW-WebView] Fallback: loading editor directly");
                await LoadEditorViaFile();
            }
        }

        private async void OnAdapterCreated(object sender, EventArgs e)
        {
            Console.WriteLine("[OLW-WebView] AdapterCreated");
            await LoadEditorViaFile();
        }

        private async Task LoadEditorViaFile()
        {
            if (_editorHtml == null) return;
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "OpenLiveWriter", "editor");
                Directory.CreateDirectory(tempDir);
                string tempFile = Path.Combine(tempDir, "editor.html");
                await File.WriteAllTextAsync(tempFile, _editorHtml);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _webView.Navigate(new Uri("file://" + tempFile));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-WebView] Load failed: {ex.Message}");
            }
        }

        private void OnNavigationCompleted(object sender, EventArgs e)
        {
            _isReady = true;
            Console.WriteLine("[OLW-WebView] Ready");

            if (_pendingHtml != null)
            {
                _ = RunJS($"OLWBridge.setContent('{EscapeJs(_pendingHtml)}')");
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
                AcceptsReturn = true, AcceptsTab = true,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
                FontSize = 16, Padding = new Thickness(16),
                BorderThickness = new Thickness(0),
                Background = global::Avalonia.Media.Brushes.White,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            panel.Children.Add(textBox);
            return panel;
        }

        // ---- Public async API matching the working test app pattern ----

        public async Task ExecCommandAsync(string command, string value = null)
        {
            if (_webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50); // Let focus settle
            string js = $"OLWBridge.execCommand('{command}', {(value != null ? $"'{value}'" : "null")})";
            await RunJS(js);
        }

        public Task ExecuteBoldAsync() => ExecCommandAsync("bold");
        public Task ExecuteItalicAsync() => ExecCommandAsync("italic");
        public Task ExecuteUnderlineAsync() => ExecCommandAsync("underline");
        public Task ExecuteStrikethroughAsync() => ExecCommandAsync("strikeThrough");
        public Task ExecuteOrderedListAsync() => ExecCommandAsync("insertOrderedList");
        public Task ExecuteUnorderedListAsync() => ExecCommandAsync("insertUnorderedList");
        public Task ExecuteIndentAsync() => ExecCommandAsync("indent");
        public Task ExecuteOutdentAsync() => ExecCommandAsync("outdent");
        public Task SetBlockFormatAsync(string tag) => ExecCommandAsync("formatBlock", tag);

        public async Task InsertHtmlAsync(string html)
        {
            if (_webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.insertHtml('{EscapeJs(html)}')");
        }

        public async Task SetContentAsync(string html)
        {
            if (_webView == null || !_isReady) { _pendingHtml = html; return; }
            await RunJS($"OLWBridge.setContent('{EscapeJs(html)}')");
        }

        public async Task<string> GetContentAsync()
        {
            if (_webView == null || !_isReady) return null;
            return await RunJSReturn("OLWBridge.getContent()");
        }

        public async Task<bool> HandleCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.Bold: await ExecuteBoldAsync(); return true;
                case CommandId.Italic: await ExecuteItalicAsync(); return true;
                case CommandId.Underline: await ExecuteUnderlineAsync(); return true;
                case CommandId.Strikethrough: await ExecuteStrikethroughAsync(); return true;
                case CommandId.Bullets: await ExecuteUnorderedListAsync(); return true;
                case CommandId.Numbers: await ExecuteOrderedListAsync(); return true;
                case CommandId.Indent: await ExecuteIndentAsync(); return true;
                case CommandId.Outdent: await ExecuteOutdentAsync(); return true;
                default: return false;
            }
        }

        // Sync wrappers for backward compat
        public void ExecCommand(string command, string value = null) => _ = ExecCommandAsync(command, value);
        public void ExecuteBold() => _ = ExecuteBoldAsync();
        public void ExecuteItalic() => _ = ExecuteItalicAsync();
        public void ExecuteUnderline() => _ = ExecuteUnderlineAsync();
        public void ExecuteStrikethrough() => _ = ExecuteStrikethroughAsync();
        public void ExecuteOrderedList() => _ = ExecuteOrderedListAsync();
        public void ExecuteUnorderedList() => _ = ExecuteUnorderedListAsync();
        public void SetBlockFormat(string tag) => _ = SetBlockFormatAsync(tag);
        public void SetContent(string html) => _ = SetContentAsync(html);
        public void GetContent(Action<string> callback) => _ = GetContentAsync().ContinueWith(t => callback?.Invoke(t.Result));
        public bool HandleCommand(CommandId commandId) { _ = HandleCommandAsync(commandId); return true; }

        private async Task RunJS(string script)
        {
            try
            {
                await _webView.InvokeScript(script);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-WebView] JS error: {ex.Message}");
            }
        }

        private async Task<string> RunJSReturn(string script)
        {
            try
            {
                return await _webView.InvokeScript(script);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-WebView] JS error: {ex.Message}");
                return null;
            }
        }

        private static string EscapeJs(string s) =>
            s?.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n") ?? "";
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
