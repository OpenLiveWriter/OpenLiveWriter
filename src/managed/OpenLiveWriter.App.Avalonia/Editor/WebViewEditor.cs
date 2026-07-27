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

        /// <summary>
        /// Raised when an image is right-clicked in the editor (the image is
        /// selected as a unit first); the shell opens the Picture properties dialog.
        /// </summary>
        public event EventHandler ImageContextMenuRequested;

        /// <summary>
        /// Raised (debounced, ~400ms) when the editor body changes. Carries no
        /// content payload — pull the full HTML on demand via <see cref="GetContentAsync"/>.
        /// </summary>
        public event EventHandler ContentChanged;

        /// <summary>Autoreplace toggles applied on paste and pushed to the JS bridge.</summary>
        public AutoreplaceOptions AutoreplaceOptions { get; set; } = new AutoreplaceOptions();

        public NativeWebView WebView => _webView;
        public bool IsReady => _isReady;

        /// <summary>
        /// When true, <see cref="InitializeWebView"/> hosts a stretch <see cref="Border"/>
        /// instead of <see cref="NativeWebView"/>. Used by the headless layout harness so
        /// editor-slot size can be asserted without a WKWebView backend.
        /// </summary>
        public static bool UseLayoutPlaceholder { get; set; }

        /// <summary>Name of the layout-placeholder border (tests locate this control).</summary>
        public const string LayoutPlaceholderName = "EditorLayoutPlaceholder";

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
                // Stretch so the native WKWebView tracks the editor panel on window resize.
                // Avalonia ContentControl defaults to Left/Top content alignment; without
                // explicit Stretch here the WebView can stay at its desired size (often 0).
                HorizontalAlignment = HorizontalAlignment.Stretch;
                VerticalAlignment = VerticalAlignment.Stretch;

                if (UseLayoutPlaceholder)
                {
                    Content = CreateLayoutPlaceholder();
                    return;
                }

                _webView = new NativeWebView
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
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

        /// <summary>
        /// Stretch stand-in for the editor WebView slot (layout harness only).
        /// </summary>
        private static Control CreateLayoutPlaceholder()
        {
            return new Border
            {
                Name = LayoutPlaceholderName,
                Background = global::Avalonia.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinWidth = 1,
                MinHeight = 1
            };
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
                _ = RunJS($"OLWBridge.setContent({EscapeJs(_pendingHtml)})");
                _pendingHtml = null;
            }

            // Push the initial formatting state so toggle buttons start in sync.
            _ = RunJS("OLWBridge.reportState()");
            _ = SetAutoreplaceOptionsAsync(AutoreplaceOptions);
        }

        // Handles JSON messages posted from editor.html via the WebView bridge.
        // Three message types are used: 'stateChanged' (formatting state as the
        // caret moves), 'contentChanged' (debounced edit signal — no HTML payload;
        // pull content on demand via GetContentAsync), and 'imageContextMenu'
        // (right-click on an image — open the Picture properties dialog).
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
                else if (type == "contentChanged")
                {
                    Dispatcher.UIThread.Post(() => ContentChanged?.Invoke(this, EventArgs.Empty));
                }
                else if (type == "imageContextMenu")
                {
                    Dispatcher.UIThread.Post(() => ImageContextMenuRequested?.Invoke(this, EventArgs.Empty));
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
                BlockTag = S("blockTag") ?? "p",
                FontFamily = NormalizeFontName(S("fontName")),
                FontSize = S("fontSize"),
                ForeColor = NormalizeReportedColor(S("foreColor")),
                HighlightColor = NormalizeReportedColor(S("backColor")),
                InTable = B("inTable"),
                SelectedElementType = NormalizeElementType(S("selectedElementType")),
                Image = ParseImageState(el)
            };
        }

        /// <summary>
        /// Parses the optional <c>image</c> sub-object of <c>getState()</c> into an
        /// <see cref="ImageFormatState"/>. Returns null when no image is selected
        /// (property absent or JSON null).
        /// </summary>
        internal static ImageFormatState ParseImageState(JsonElement el)
        {
            if (!el.TryGetProperty("image", out var img) || img.ValueKind != JsonValueKind.Object)
                return null;
            return ParseImageObject(img);
        }

        // Parses a getState() "image" sub-object (also the getSelectedImage()
        // payload shape) into an ImageFormatState.
        private static ImageFormatState ParseImageObject(JsonElement img)
        {
            int N(string name) => img.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number &&
                                      p.TryGetInt32(out int v) ? v : 0;
            int? NN(string name) => img.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number &&
                                    p.TryGetInt32(out int v) ? v : null;
            string S(string name) => img.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
                ? p.GetString() : null;

            return new ImageFormatState
            {
                Src = S("src"),
                NaturalWidth = N("naturalWidth"),
                NaturalHeight = N("naturalHeight"),
                Width = N("width"),
                Height = N("height"),
                Alt = S("alt"),
                Title = S("title"),
                Alignment = ImageEditBuilder.NormalizeAlignment(S("alignment")),
                MarginPx = NN("margin"),
                RotationDeg = N("rotation"),
                BorderWidthPx = NN("borderWidth"),
                BorderColor = NormalizeReportedColor(S("borderColor")),
                LinkHref = string.IsNullOrEmpty(S("link")) ? null : S("link")
            };
        }

        /// <summary>
        /// Parses the <c>getSelectedImage()</c> JSON payload (the image object
        /// itself, or "null" when no image is selected) into an
        /// <see cref="ImageFormatState"/>. Pure/deterministic for headless tests.
        /// </summary>
        internal static ImageFormatState ParseSelectedImageJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return null;
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                ? ParseImageObject(doc.RootElement)
                : null;
        }

        /// <summary>
        /// Normalizes the reported selected-element type to a lower-case canonical
        /// token (image/video/map/tag) or null when nothing rich is selected.
        /// </summary>
        internal static string NormalizeElementType(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;
            string s = type.Trim().ToLowerInvariant();
            return s.Length == 0 ? null : s;
        }

        /// <summary>
        /// Parses a <c>getState()</c> JSON payload into a <see cref="FormatState"/>.
        /// Pure/deterministic so the caret-state → ribbon mapping (block tag, font
        /// family/size, colors) is unit-testable without a live WebView.
        /// </summary>
        internal static FormatState ParseFormatStateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new FormatState();
            using var doc = JsonDocument.Parse(json);
            return ParseFormatState(doc.RootElement);
        }

        /// <summary>
        /// Cleans a reported <c>fontName</c>: strips surrounding quotes and takes the
        /// first family in a CSS font stack (WebKit reports the resolved stack). Null
        /// or empty input yields null.
        /// </summary>
        internal static string NormalizeFontName(string fontName)
        {
            if (string.IsNullOrWhiteSpace(fontName))
                return null;
            string first = fontName.Split(',')[0].Trim();
            first = first.Trim('\'', '"').Trim();
            return first.Length == 0 ? null : first;
        }

        /// <summary>
        /// Normalizes a reported color to canonical <c>#RRGGBB</c>. Accepts
        /// <c>rgb(r, g, b)</c> (as WebKit reports queryCommandValue colors) and hex
        /// forms. Returns null when the input is empty or unparseable.
        /// </summary>
        internal static string NormalizeReportedColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return null;

            string s = color.Trim();
            if (s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
            {
                int open = s.IndexOf('(');
                int close = s.IndexOf(')');
                if (open >= 0 && close > open)
                {
                    string[] parts = s.Substring(open + 1, close - open - 1).Split(',');
                    if (parts.Length >= 3 &&
                        int.TryParse(parts[0].Trim(), out int r) &&
                        int.TryParse(parts[1].Trim(), out int g) &&
                        int.TryParse(parts[2].Trim(), out int b) &&
                        InByte(r) && InByte(g) && InByte(b))
                    {
                        return $"#{r:X2}{g:X2}{b:X2}";
                    }
                }
                return null;
            }

            return NormalizeColor(s);
        }

        private static bool InByte(int v) => v >= 0 && v <= 255;

        private Control CreateFallbackEditor()
        {
            var panel = new DockPanel { LastChildFill = true };
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
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
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
            string js = $"OLWBridge.execCommand({EscapeJs(command)}, {(value != null ? EscapeJs(value) : "null")})";
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
        /// <summary>Cut via the execCommand bridge (WKWebView honors this inside contenteditable).</summary>
        public Task ExecuteCutAsync() => ExecCommandAsync("cut");
        /// <summary>Copy via the execCommand bridge.</summary>
        public Task ExecuteCopyAsync() => ExecCommandAsync("copy");
        public Task ExecuteClearFormattingAsync() => ExecCommandAsync("removeFormat");
        public Task InsertHorizontalLineAsync() => ExecCommandAsync("insertHorizontalRule");
        public Task SetBlockFormatAsync(string tag) => ExecCommandAsync("formatBlock", tag);
        public Task SetFontFamilyAsync(string family) => ExecCommandAsync("fontName", family);

        /// <summary>
        /// Applies a pixel font size to the current selection via the bridge's
        /// <c>setFontSizePx</c> helper. Unparseable/out-of-range values are ignored.
        /// </summary>
        public async Task SetFontSizeAsync(string pxSize)
        {
            string px = NormalizeFontSizePx(pxSize);
            if (px == null || _webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.setFontSizePx({px})");
        }

        /// <summary>
        /// Normalizes a user/combo font-size entry to a canonical integer px string.
        /// Accepts plain numbers ("12", "12pt", " 14 "), rounds to the nearest
        /// integer, and clamps to a sane 6–144px range. Returns null when the input
        /// is not a parseable positive size. Pure so it is unit-testable headlessly.
        /// </summary>
        internal static string NormalizeFontSizePx(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
                return null;

            string s = size.Trim();
            if (s.EndsWith("px", StringComparison.OrdinalIgnoreCase) ||
                s.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(0, s.Length - 2).Trim();

            if (!double.TryParse(s, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double value) ||
                value <= 0)
                return null;

            int px = (int)Math.Round(value);
            px = Math.Max(6, Math.Min(144, px));
            return px.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

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
            await RunJS($"OLWBridge.setHighlight({EscapeJs(hex)})");
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
            await RunJS($"OLWBridge.toggleBlock({EscapeJs(tag)})");
        }

        public async Task CreateLinkAsync(string url)
        {
            if (_webView == null || !_isReady || string.IsNullOrEmpty(url)) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.createLink({EscapeJs(url)})");
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

        /// <summary>
        /// Injectable media store + current-document media id for file-based image
        /// insertion. When both are set (wired by the shell), inserted images are
        /// copied into the document's media folder and referenced by a <c>file://</c>
        /// src — the Windows behavior — instead of being embedded as base64.
        /// </summary>
        public ImageEditing.MediaStore MediaStore { get; set; }

        /// <summary>Returns the current document's media id (<see cref="MediaStore"/> key).</summary>
        public Func<string> DocumentMediaIdProvider { get; set; }

        /// <summary>
        /// Inserts an image loaded from a local file at the caret. When a media
        /// store and document media id are available the file is copied into the
        /// document's media folder and the <c>&lt;img src&gt;</c> is the copy's
        /// <c>file://</c> URI (the publish pipeline uploads it and rewrites the src
        /// to the hosted URL). Without them, falls back to an inline base64
        /// <c>data:</c> URI so the editor stays self-contained.
        /// </summary>
        public async Task InsertImageFromFileAsync(string filePath, string altText = null)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            string mediaId = DocumentMediaIdProvider?.Invoke();
            if (MediaStore != null && !string.IsNullOrEmpty(mediaId))
            {
                string fileUri = MediaStore.AddImage(mediaId, filePath);
                string alt = altText ?? Path.GetFileNameWithoutExtension(filePath);
                await InsertHtmlAsync(BuildImageHtml(fileUri, alt));
                return;
            }

            string html = BuildImageHtmlFromFile(filePath, altText);
            await InsertHtmlAsync(html);
        }

        /// <summary>Inserts an <c>&lt;img&gt;</c> element for the given source.</summary>
        public Task InsertImageAsync(string src, string altText = null) =>
            InsertHtmlAsync(BuildImageHtml(src, altText));

        /// <summary>
        /// Inserts a picture from the web at the caret. The remote URL is kept as-is
        /// (no download/base64 embedding), so the publish pipeline's ImagePublisher —
        /// which only rewrites inline data-URIs — leaves it alone.
        /// </summary>
        public Task InsertWebImageAsync(string url, string altText = null, int? widthPx = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return Task.CompletedTask;
            return InsertHtmlAsync(BuildImageHtml(url.Trim(), altText, widthPx));
        }

        /// <summary>
        /// Reads an image file and builds a self-contained <c>&lt;img&gt;</c> whose
        /// <c>src</c> is an inline base64 data URI. Used only as the fallback when no
        /// media store is wired; kept for legacy drafts and unit tests. Alt text
        /// defaults to the file name.
        /// </summary>
        internal static string BuildImageHtmlFromFile(string filePath, string altText = null)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            string mimeType = GuessImageMimeType(filePath);
            string dataUri = BuildDataUri(mimeType, bytes);
            string alt = altText ?? Path.GetFileNameWithoutExtension(filePath);
            return BuildImageHtml(dataUri, alt);
        }

        /// <summary>Builds a well-formed, attribute-escaped <c>&lt;img&gt;</c> element.</summary>
        internal static string BuildImageHtml(string src, string altText) =>
            BuildImageHtml(src, altText, widthPx: null);

        /// <summary>
        /// Builds a well-formed, attribute-escaped <c>&lt;img&gt;</c> element, adding a
        /// <c>width</c> attribute when <paramref name="widthPx"/> is supplied.
        /// </summary>
        internal static string BuildImageHtml(string src, string altText, int? widthPx)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<img src=\"").Append(EscapeHtmlAttr(src)).Append('"');
            if (!string.IsNullOrEmpty(altText))
                sb.Append(" alt=\"").Append(EscapeHtmlAttr(altText)).Append('"');
            if (widthPx.HasValue && widthPx.Value > 0)
                sb.Append(" width=\"").Append(widthPx.Value).Append('"');
            sb.Append(" />");
            return sb.ToString();
        }

        /// <summary>Builds an inline base64 <c>data:</c> URI for the given bytes.</summary>
        internal static string BuildDataUri(string mimeType, byte[] bytes) =>
            $"data:{mimeType};base64,{Convert.ToBase64String(bytes ?? Array.Empty<byte>())}";

        /// <summary>Maps a file extension to an image MIME type (defaults to PNG).</summary>
        internal static string GuessImageMimeType(string filePath)
        {
            string ext = Path.GetExtension(filePath)?.ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "image/png"
            };
        }

        /// <summary>
        /// Highlights the next occurrence of <paramref name="query"/> in the editor
        /// using the browser's native find (visual selection runs inside the WebView).
        /// </summary>
        public async Task FindNextAsync(string query, bool matchCase)
        {
            if (_webView == null || !_isReady || string.IsNullOrEmpty(query)) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.findNext({EscapeJs(query)}, {(matchCase ? "true" : "false")})");
        }

        /// <summary>Highlights the previous occurrence (backwards native find).</summary>
        public async Task FindPreviousAsync(string query, bool matchCase)
        {
            if (_webView == null || !_isReady || string.IsNullOrEmpty(query)) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.findPrevious({EscapeJs(query)}, {(matchCase ? "true" : "false")})");
        }

        /// <summary>
        /// Returns the current-match position and total match count for
        /// <paramref name="query"/> (drives the find bar's "n of m" readout).
        /// Returns null when the editor is not ready.
        /// </summary>
        public async Task<FindStats> FindStatsAsync(string query, bool matchCase)
        {
            if (_webView == null || !_isReady || string.IsNullOrEmpty(query))
                return null;
            string result = await RunJSReturn(
                $"OLWBridge.findStats({EscapeJs(query)}, {(matchCase ? "true" : "false")})");
            return ParseFindStats(result);
        }

        /// <summary>
        /// Parses a <c>findStats()</c> "current,total" payload. Pure/deterministic so
        /// the mapping is unit-testable without a live WebView. Malformed input
        /// yields a zeroed result.
        /// </summary>
        internal static FindStats ParseFindStats(string stats)
        {
            if (!string.IsNullOrWhiteSpace(stats))
            {
                string[] parts = stats.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), out int current) &&
                    int.TryParse(parts[1].Trim(), out int total))
                {
                    return new FindStats(Math.Max(0, current), Math.Max(0, total));
                }
            }
            return new FindStats(0, 0);
        }

        /// <summary>
        /// Replaces the currently selected match (as highlighted by Find Next /
        /// Find Previous) with <paramref name="replacement"/>, honoring the case
        /// option. Returns true when a replacement was made.
        /// </summary>
        public async Task<bool> ReplaceCurrentAsync(string query, string replacement, bool matchCase)
        {
            if (_webView == null || !_isReady || string.IsNullOrEmpty(query)) return false;
            _webView.Focus();
            await Task.Delay(50);
            string result = await RunJSReturn(
                $"OLWBridge.replaceCurrent({EscapeJs(query)}, {EscapeJs(replacement)}, {(matchCase ? "true" : "false")})");
            return string.Equals(result, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Replaces every occurrence of <paramref name="query"/> in the editor body
        /// (text content only, tags preserved) and returns the number replaced. The
        /// matching/replacement is done by the pure <see cref="TextFinder"/> so the
        /// behavior is deterministic and testable independent of the WebView.
        /// </summary>
        public async Task<int> ReplaceAllAsync(string query, string replacement, bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(query) || _webView == null || !_isReady) return 0;
            string html = await GetContentAsync() ?? string.Empty;
            string updated = TextFinder.ReplaceAllInHtml(html, query, replacement ?? string.Empty,
                matchCase, wholeWord, out int count);
            if (count > 0)
                await SetContentAsync(updated);
            return count;
        }

        /// <summary>
        /// Inserts a table built from the given dimensions at the caret. The table
        /// HTML is produced by the pure <see cref="TableBuilder"/> (well-formed
        /// thead/tbody/th/td) so the markup is testable independent of the WebView.
        /// </summary>
        public Task InsertTableAsync(int rows, int columns, bool headerRow, string width) =>
            InsertHtmlAsync(TableBuilder.BuildTableHtml(rows, columns, headerRow, width));

        /// <summary>Invokes a table-editing bridge helper (insert/delete row/column, delete table).</summary>
        private async Task RunTableOpAsync(string js)
        {
            if (_webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS(js);
        }

        public Task InsertTableRowAboveAsync() => RunTableOpAsync("OLWBridge.insertTableRow(false)");
        public Task InsertTableRowBelowAsync() => RunTableOpAsync("OLWBridge.insertTableRow(true)");
        public Task InsertTableColumnLeftAsync() => RunTableOpAsync("OLWBridge.insertTableColumn(false)");
        public Task InsertTableColumnRightAsync() => RunTableOpAsync("OLWBridge.insertTableColumn(true)");
        public Task DeleteTableRowAsync() => RunTableOpAsync("OLWBridge.deleteTableRow()");
        public Task DeleteTableColumnAsync() => RunTableOpAsync("OLWBridge.deleteTableColumn()");
        public Task DeleteTableAsync() => RunTableOpAsync("OLWBridge.deleteTable()");

        /// <summary>Inserts a clearing line break (clears floated content) at the caret.</summary>
        public Task InsertClearBreakAsync() => InsertHtmlAsync(EditorMarkup.ClearBreakHtml);

        // ---- Picture Tools (operate on the image selected as a unit) ----

        /// <summary>
        /// Applies the given attribute/style changes to the selected image via the
        /// bridge's <c>applyImageAttrs</c>. Only non-null members are touched.
        /// </summary>
        public async Task ApplyImageAttrsAsync(ImageAttributes attrs)
        {
            if (_webView == null || !_isReady || attrs == null) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.applyImageAttrs({ImageEditBuilder.BuildAttrsJson(attrs)})");
        }

        /// <summary>
        /// Pulls the selected image's current attribute payload fresh from the
        /// bridge (<c>getSelectedImage()</c>), or null when no image is selected
        /// or the editor is not ready. The pixel-baking commands use this so they
        /// never act on a stale cached state.
        /// </summary>
        public async Task<ImageFormatState> GetSelectedImageAsync()
        {
            if (_webView == null || !_isReady) return null;
            string json = await RunJSReturn("OLWBridge.getSelectedImage()");
            return ParseSelectedImageJson(json);
        }

        /// <summary>
        /// Replaces the selected image's pixels with a baked (re-encoded) source
        /// — typically a PNG data URI produced by
        /// <see cref="ImageEditing.ImageEditorService"/>. <paramref name="sizeMode"/>
        /// controls the display size: keep as-is (color effects), swap explicit
        /// width/height (90-degree rotation), or set explicit px dimensions
        /// (crop/resize). Any CSS rotation is cleared because the rotation is now
        /// baked into the pixels. The bridge fires the debounced contentChanged
        /// (dirty) and stateChanged notifications itself. Note: WebView undo does
        /// not cover this bridge rewrite (documented limitation — best-effort).
        /// </summary>
        public async Task ReplaceSelectedImageSrcAsync(string newSrc,
            ImageEditing.BakedImageSizeMode sizeMode, int width = 0, int height = 0)
        {
            if (_webView == null || !_isReady || string.IsNullOrEmpty(newSrc)) return;
            _webView.Focus();
            await Task.Delay(50);
            string mode = sizeMode switch
            {
                ImageEditing.BakedImageSizeMode.Swap => "swap",
                ImageEditing.BakedImageSizeMode.Set => "set",
                _ => "keep"
            };
            await RunJS($"OLWBridge.replaceSelectedImageSrc({EscapeJs(newSrc)}, {EscapeJs(mode)}, {width}, {height})");
        }

        /// <summary>
        /// Wraps the selected image in a link to <paramref name="url"/>, retargets
        /// an existing link, or (when null) removes the link.
        /// </summary>
        public async Task SetImageLinkAsync(string url)
        {
            if (_webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50);
            string js = string.IsNullOrEmpty(url)
                ? "OLWBridge.setImageLink(null)"
                : $"OLWBridge.setImageLink({EscapeJs(url)})";
            await RunJS(js);
        }

        /// <summary>
        /// Inserts the extended-entry ("more") break marker at the caret. The publish
        /// pipeline splits the post on this marker into main / extended contents.
        /// </summary>
        public Task InsertExtendedEntryAsync() => InsertHtmlAsync(EditorMarkup.ExtendedEntryBreakHtml);

        /// <summary>Inserts pasted content as plain text (formatting removed) at the caret.</summary>
        public Task PastePlainTextAsync(string clipboardHtmlOrText)
        {
            string text = PasteCleaner.ToPlainText(clipboardHtmlOrText);
            text = AutoreplaceTransformer.TransformPlainText(text, AutoreplaceOptions);
            return InsertHtmlAsync(PasteCleaner.BuildPlainTextInsertion(text));
        }

        /// <summary>Inserts pasted HTML after sanitizing it to a safe subset at the caret.</summary>
        public Task PasteCleanHtmlAsync(string clipboardHtml)
        {
            string cleaned = PasteCleaner.CleanHtml(clipboardHtml);
            string plain = PasteCleaner.ToPlainText(cleaned);
            plain = AutoreplaceTransformer.TransformPlainText(plain, AutoreplaceOptions);
            return InsertHtmlAsync(PasteCleaner.BuildPlainTextInsertion(plain));
        }

        /// <summary>
        /// Pushes autoreplace toggles to the editor bridge for live typing replacements.
        /// </summary>
        public async Task SetAutoreplaceOptionsAsync(AutoreplaceOptions options)
        {
            AutoreplaceOptions = options ?? new AutoreplaceOptions();
            if (_webView == null || !_isReady) return;
            await RunJS(AutoreplaceController.BuildSetAutoreplaceScript(AutoreplaceOptions));
        }

        /// <summary>
        /// Enables/disables the editor body's native spell-check underlines by toggling
        /// its <c>spellcheck</c> attribute via the bridge. The actual checking is done by
        /// macOS/WebKit; this only flips the attribute.
        /// </summary>
        public async Task SetSpellcheckEnabledAsync(bool enabled)
        {
            if (_webView == null || !_isReady) return;
            await RunJS(SpellCheckController.BuildSetSpellcheckScript(enabled));
        }

        public Task ExecuteBlockquoteAsync() => ToggleBlockAsync("blockquote");

        public async Task InsertHtmlAsync(string html)
        {
            if (_webView == null || !_isReady) return;
            _webView.Focus();
            await Task.Delay(50);
            await RunJS($"OLWBridge.insertHtml({EscapeJs(html)})");
        }

        public async Task SetContentAsync(string html)
        {
            if (_webView == null || !_isReady) { _pendingHtml = html; return; }
            await RunJS($"OLWBridge.setContent({EscapeJs(html)})");
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
            return await PublishAsync(client, blogId, existingPostId: null, title, publish, categories);
        }

        /// <summary>
        /// Publish overload that edits an existing server post when
        /// <paramref name="existingPostId"/> is supplied (re-publish of an already-published
        /// document), otherwise creates a new post. Inline images are hosted first.
        /// </summary>
        public async Task<string> PublishAsync(Publishing.IBlogClient client, string blogId,
            string existingPostId, string title, bool publish, params string[] categories)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            string html = await GetContentAsync() ?? string.Empty;
            return await Publishing.EditorContentPublisher.PublishOrEditAsync(
                client, blogId, existingPostId, title, html, publish, categories);
        }

        /// <summary>
        /// Publish overload that also carries post keywords/tags (sent as
        /// <c>mt_keywords</c>). Categories are passed as an enumerable to disambiguate
        /// from the params overload.
        /// </summary>
        public async Task<string> PublishAsync(Publishing.IBlogClient client, string blogId,
            string existingPostId, string title, bool publish,
            System.Collections.Generic.IEnumerable<string> categories, string keywords)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            string html = await GetContentAsync() ?? string.Empty;
            return await Publishing.EditorContentPublisher.PublishOrEditAsync(
                client, blogId, existingPostId, title, html, publish, categories, keywords);
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
                case CommandId.Cut: await ExecuteCutAsync(); return true;
                case CommandId.CopyCommand: await ExecuteCopyAsync(); return true;

                // Insert
                case CommandId.InsertHorizontalLine: await InsertHorizontalLineAsync(); return true;
                case CommandId.InsertClearBreak: await InsertClearBreakAsync(); return true;
                case CommandId.InsertExtendedEntry: await InsertExtendedEntryAsync(); return true;

                // Table Tools (contextual) — operate on the table containing the caret
                case CommandId.InsertRowAbove: await InsertTableRowAboveAsync(); return true;
                case CommandId.InsertRowBelow: await InsertTableRowBelowAsync(); return true;
                case CommandId.InsertColumnLeft: await InsertTableColumnLeftAsync(); return true;
                case CommandId.InsertColumnRight: await InsertTableColumnRightAsync(); return true;
                case CommandId.DeleteRow: await DeleteTableRowAsync(); return true;
                case CommandId.DeleteColumn: await DeleteTableColumnAsync(); return true;
                case CommandId.DeleteTable: await DeleteTableAsync(); return true;

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

        /// <summary>
        /// Encodes a string as a complete, self-delimited JavaScript string literal
        /// (surrounding quotes included) via JSON encoding — a valid JS string-literal
        /// subset. Control characters, quotes, backslashes, U+2028/U+2029, and
        /// HTML-sensitive characters (<c>&lt;</c> <c>&gt;</c> <c>&amp;</c>) are all
        /// escaped, so interpolated content cannot break out of the literal (e.g. a
        /// <c>&lt;/script&gt;</c> payload). Null becomes <c>""</c>.
        /// </summary>
        internal static string EscapeJs(string s) => JsonSerializer.Serialize(s ?? string.Empty);
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

        /// <summary>The selection's font family (first family in the stack), or null.</summary>
        public string FontFamily { get; set; }

        /// <summary>The selection's font size in px (computed at the caret), or null.</summary>
        public string FontSize { get; set; }

        /// <summary>The selection's foreground color as <c>#RRGGBB</c>, or null.</summary>
        public string ForeColor { get; set; }

        /// <summary>The selection's highlight/background color as <c>#RRGGBB</c>, or null.</summary>
        public string HighlightColor { get; set; }

        /// <summary>True when the caret is inside a table cell (drives Table Tools).</summary>
        public bool InTable { get; set; }

        /// <summary>
        /// The kind of rich element the selection sits within (image/video/map/tag),
        /// or null. Drives contextual-tab activation.
        /// </summary>
        public string SelectedElementType { get; set; }

        /// <summary>
        /// Attributes/styles of the image selected as a unit, or null when no image
        /// is selected. Drives the Picture Tools spinners and properties dialog.
        /// </summary>
        public ImageFormatState Image { get; set; }
    }

    /// <summary>
    /// A snapshot of the selected image's attributes and inline styles, as reported
    /// by <c>getState()</c>. Sizes are in px; colors are normalized <c>#RRGGBB</c>.
    /// </summary>
    public class ImageFormatState
    {
        /// <summary>The image source (data URI for embedded pictures, URL for web pictures).</summary>
        public string Src { get; set; }

        /// <summary>Natural (file) pixel width; 0 when the image hasn't finished loading.</summary>
        public int NaturalWidth { get; set; }

        /// <summary>Natural (file) pixel height; 0 when the image hasn't finished loading.</summary>
        public int NaturalHeight { get; set; }

        /// <summary>Current display width in px (attribute or rendered).</summary>
        public int Width { get; set; }

        /// <summary>Current display height in px (attribute or rendered).</summary>
        public int Height { get; set; }

        /// <summary>Alt text, or null/empty when unset.</summary>
        public string Alt { get; set; }

        /// <summary>Title attribute, or null/empty when unset.</summary>
        public string Title { get; set; }

        /// <summary>Layout: inline/left/right/center.</summary>
        public string Alignment { get; set; } = "inline";

        /// <summary>Uniform margin in px, or null when no (or non-uniform) margin.</summary>
        public int? MarginPx { get; set; }

        /// <summary>CSS-transform rotation in degrees (0 = none).</summary>
        public int RotationDeg { get; set; }

        /// <summary>Solid border width in px, or null when borderless.</summary>
        public int? BorderWidthPx { get; set; }

        /// <summary>Border color as <c>#RRGGBB</c>, or null when borderless.</summary>
        public string BorderColor { get; set; }

        /// <summary>The wrapping anchor's href, or null when the image is not linked.</summary>
        public string LinkHref { get; set; }

        /// <summary>True when the source is a remote http(s) URL (not an embedded data URI).</summary>
        public bool HasRemoteSource =>
            Src != null && (Src.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            Src.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find-bar match statistics: the 1-based position of the currently selected
    /// match (0 when none is selected) and the total number of matches.
    /// </summary>
    public sealed class FindStats
    {
        public FindStats(int current, int total)
        {
            Current = current;
            Total = total;
        }

        public int Current { get; }
        public int Total { get; }
    }
}
