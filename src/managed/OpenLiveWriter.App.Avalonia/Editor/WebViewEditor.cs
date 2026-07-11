// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
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

        public event EventHandler<FormatState> FormatStateChanged;
        public event EventHandler<string> ContentChanged;

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
                _webView.WebMessageReceived += OnWebMessageReceived;
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

            // Push the initial formatting state so toggle buttons start in sync.
            _ = RunJS("OLWBridge.reportState()");
        }

        // Handles JSON messages posted from editor.html via the WebView bridge.
        // Two message types are used: 'stateChanged' (formatting state as the caret
        // moves) and 'contentChanged' (body HTML after an edit).
        private void OnWebMessageReceived(object sender, WebMessageReceivedEventArgs e)
        {
            var body = e?.Body;
            if (string.IsNullOrEmpty(body))
                return;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProp))
                    return;

                var type = typeProp.GetString();
                if (type == "stateChanged" && root.TryGetProperty("state", out var stateEl))
                {
                    var state = ParseFormatState(stateEl);
                    Dispatcher.UIThread.Post(() => FormatStateChanged?.Invoke(this, state));
                }
                else if (type == "contentChanged" && root.TryGetProperty("html", out var htmlEl))
                {
                    var html = htmlEl.GetString();
                    Dispatcher.UIThread.Post(() => ContentChanged?.Invoke(this, html));
                }
            }
            catch (JsonException)
            {
                // Non-JSON or malformed message — ignore.
            }
        }

        private static FormatState ParseFormatState(JsonElement el)
        {
            bool B(string name) => el.TryGetProperty(name, out var p) &&
                                   p.ValueKind == JsonValueKind.True;
            string S(string name) => el.TryGetProperty(name, out var p) ? p.GetString() : null;

            return new FormatState
            {
                Bold = B("bold"),
                Italic = B("italic"),
                Underline = B("underline"),
                Strikethrough = B("strikethrough"),
                Subscript = B("subscript"),
                Superscript = B("superscript"),
                OrderedList = B("orderedList"),
                UnorderedList = B("unorderedList"),
                AlignLeft = B("alignLeft"),
                AlignCenter = B("alignCenter"),
                AlignRight = B("alignRight"),
                AlignFull = B("alignFull"),
                BlockTag = S("blockTag") ?? "p"
            };
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
        public Task ExecuteSubscriptAsync() => ExecCommandAsync("subscript");
        public Task ExecuteSuperscriptAsync() => ExecCommandAsync("superscript");
        public Task ExecuteOrderedListAsync() => ExecCommandAsync("insertOrderedList");
        public Task ExecuteUnorderedListAsync() => ExecCommandAsync("insertUnorderedList");
        public Task ExecuteIndentAsync() => ExecCommandAsync("indent");
        public Task ExecuteOutdentAsync() => ExecCommandAsync("outdent");
        public Task ExecuteAlignLeftAsync() => ExecCommandAsync("justifyLeft");
        public Task ExecuteAlignCenterAsync() => ExecCommandAsync("justifyCenter");
        public Task ExecuteAlignRightAsync() => ExecCommandAsync("justifyRight");
        public Task ExecuteJustifyAsync() => ExecCommandAsync("justifyFull");
        public Task ExecuteUndoAsync() => ExecCommandAsync("undo");
        public Task ExecuteRedoAsync() => ExecCommandAsync("redo");
        public Task ExecuteSelectAllAsync() => ExecCommandAsync("selectAll");
        public Task ExecuteClearFormattingAsync() => ExecCommandAsync("removeFormat");
        public Task InsertHorizontalLineAsync() => ExecCommandAsync("insertHorizontalRule");
        public Task SetBlockFormatAsync(string tag) => ExecCommandAsync("formatBlock", tag);
        public Task SetFontFamilyAsync(string family) => ExecCommandAsync("fontName", family);
        public Task SetFontSizeAsync(string htmlSize) => ExecCommandAsync("fontSize", htmlSize);

        /// <summary>
        /// Applies a text (foreground) color to the current selection. The color is
        /// normalized to <c>#RRGGBB</c> before dispatch; invalid values are ignored.
        /// </summary>
        public Task SetFontColorAsync(string color)
        {
            string hex = NormalizeColor(color);
            return hex == null ? Task.CompletedTask : ExecCommandAsync("foreColor", hex);
        }

        /// <summary>
        /// Applies a highlight (background) color to the current selection. Uses the
        /// <c>setHighlight</c> bridge helper which prefers <c>hiliteColor</c> with a
        /// <c>backColor</c> fallback for engines that don't honor it.
        /// </summary>
        public async Task SetHighlightColorAsync(string color)
        {
            string hex = NormalizeColor(color);
            if (hex == null || _webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.setHighlight('{EscapeJs(hex)}')");
        }

        /// <summary>
        /// Maps a color-picker command to the <c>document.execCommand</c> name it
        /// drives. Pure/deterministic for headless testing. Returns null for
        /// commands that are not color pickers.
        /// </summary>
        internal static string ColorCommandFor(CommandId commandId) => commandId switch
        {
            CommandId.FontColorPicker => "foreColor",
            CommandId.FontColor => "foreColor",
            CommandId.FontBackgroundColor => "hiliteColor",
            _ => null
        };

        /// <summary>
        /// Normalizes a color string to canonical <c>#RRGGBB</c> (uppercase),
        /// expanding 3-digit shorthand and tolerating a missing leading '#'.
        /// Returns null when the input is not a valid hex color. Pure so the
        /// serialization is unit-testable without a live WebView.
        /// </summary>
        internal static string NormalizeColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color)) return null;
            string s = color.Trim();
            if (s.StartsWith("#")) s = s.Substring(1);

            if (s.Length == 3)
                s = new string(new[] { s[0], s[0], s[1], s[1], s[2], s[2] });

            if (s.Length != 6) return null;
            foreach (char c in s)
            {
                if (!Uri.IsHexDigit(c)) return null;
            }
            return "#" + s.ToUpperInvariant();
        }

        public async Task ToggleBlockAsync(string tag)
        {
            if (_webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.toggleBlock('{EscapeJs(tag)}')");
        }

        public async Task CreateLinkAsync(string url)
        {
            if (_webView == null || !_isReady || string.IsNullOrEmpty(url)) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.createLink('{EscapeJs(url)}')");
        }

        /// <summary>
        /// Inserts a hyperlink. When <paramref name="text"/> is provided a full
        /// anchor element is inserted; otherwise the current selection is wrapped
        /// via createLink.
        /// </summary>
        public async Task InsertLinkAsync(string url, string text, string title, bool openInNewWindow)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (string.IsNullOrEmpty(text))
            {
                await CreateLinkAsync(url);
                return;
            }

            await InsertHtmlAsync(BuildAnchorHtml(url, text, title, openInNewWindow));
        }

        /// <summary>
        /// Builds a well-formed, HTML-escaped anchor element for the given link
        /// parameters. Pure/deterministic so it can be unit-tested without a live
        /// WebView backend.
        /// </summary>
        internal static string BuildAnchorHtml(string url, string text, string title, bool openInNewWindow)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<a href=\"").Append(EscapeHtmlAttr(url)).Append('"');
            if (!string.IsNullOrEmpty(title))
                sb.Append(" title=\"").Append(EscapeHtmlAttr(title)).Append('"');
            if (openInNewWindow)
                sb.Append(" target=\"_blank\" rel=\"noopener\"");
            sb.Append('>').Append(EscapeHtmlText(text)).Append("</a>");
            return sb.ToString();
        }

        internal static string EscapeHtmlAttr(string s) =>
            s?.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";

        internal static string EscapeHtmlText(string s) =>
            s?.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";

        public Task ExecuteBlockquoteAsync() => ToggleBlockAsync("blockquote");

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

        /// <summary>
        /// Publish entry point: pulls the current editor HTML and pushes it through
        /// the cross-platform publish pipeline (trim/scrub/split → <see cref="Publishing.BlogPost"/>
        /// → MetaWeblog XML-RPC) via the supplied transport. Returns the new post id.
        /// </summary>
        public async Task<string> PublishAsync(Publishing.IBlogClient client, string blogId, string title,
            bool publish, params string[] categories)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            string html = await GetContentAsync() ?? string.Empty;
            return Publishing.EditorContentPublisher.Publish(client, blogId, title, html, publish, categories);
        }

        public async Task<bool> HandleCommandAsync(CommandId commandId)
        {
            switch (commandId)
            {
                // Character formatting
                case CommandId.Bold: await ExecuteBoldAsync(); return true;
                case CommandId.Italic: await ExecuteItalicAsync(); return true;
                case CommandId.Underline: await ExecuteUnderlineAsync(); return true;
                case CommandId.Strikethrough: await ExecuteStrikethroughAsync(); return true;
                case CommandId.Subscript: await ExecuteSubscriptAsync(); return true;
                case CommandId.Superscript: await ExecuteSuperscriptAsync(); return true;
                case CommandId.ClearFormatting: await ExecuteClearFormattingAsync(); return true;

                // Lists and indentation
                case CommandId.Bullets: await ExecuteUnorderedListAsync(); return true;
                case CommandId.Numbers: await ExecuteOrderedListAsync(); return true;
                case CommandId.Indent: await ExecuteIndentAsync(); return true;
                case CommandId.Outdent: await ExecuteOutdentAsync(); return true;

                // Paragraph alignment
                case CommandId.AlignLeft: await ExecuteAlignLeftAsync(); return true;
                case CommandId.AlignCenter: await ExecuteAlignCenterAsync(); return true;
                case CommandId.AlignRight: await ExecuteAlignRightAsync(); return true;
                case CommandId.Justify: await ExecuteJustifyAsync(); return true;
                case CommandId.Blockquote: await ExecuteBlockquoteAsync(); return true;

                // Editing
                case CommandId.Undo: await ExecuteUndoAsync(); return true;
                case CommandId.Redo: await ExecuteRedoAsync(); return true;
                case CommandId.SelectAll: await ExecuteSelectAllAsync(); return true;

                // Insert
                case CommandId.InsertHorizontalLine: await InsertHorizontalLineAsync(); return true;

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
        public bool Subscript { get; set; }
        public bool Superscript { get; set; }
        public bool OrderedList { get; set; }
        public bool UnorderedList { get; set; }
        public bool AlignLeft { get; set; }
        public bool AlignCenter { get; set; }
        public bool AlignRight { get; set; }
        public bool AlignFull { get; set; }
        public string BlockTag { get; set; } = "p";
    }
}
