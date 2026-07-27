// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Theming;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    public partial class EditorPanel : UserControl
    {
        private readonly CommandBridge _commandBridge;
        private WebViewEditor _webViewEditor;
        private string _currentView = "edit"; // "edit", "source", "preview"

        public event EventHandler<string> StatusChanged;

        /// <summary>
        /// Raised when the in-editor find bar's "Replace…" action should open the
        /// full Find &amp; Replace dialog (owned by the shell).
        /// </summary>
        public event EventHandler OpenFindReplaceRequested;

        public EditorPanel()
        {
            InitializeComponent();
            _commandBridge = new CommandBridge();

            // Syntax highlighting for the Source view (HTML spans -> dark palette).
            var sourceEditor = this.FindControl<global::AvaloniaEdit.TextEditor>("SourceEditor");
            sourceEditor?.TextArea.TextView.LineTransformers.Add(new HtmlSyntaxColorizer());
            InitializeWebViewEditor();
            SetupFindBar();
            SetupKeyboardShortcuts();
            RegisterCommandBridgeHandlers();
        }

        public CommandBridge CommandBridge => _commandBridge;
        public WebViewEditor WebViewEditor => _webViewEditor;

        private void InitializeWebViewEditor()
        {
            _webViewEditor = new WebViewEditor();
            // Format-state sync for toggle buttons lives on the ribbon (primary
            // command surface). The secondary format toolbar was removed to avoid
            // duplicating Home-tab chrome.
            var editorHost = this.FindControl<ContentControl>("EditorHost");
            if (editorHost != null)
            {
                editorHost.Content = _webViewEditor;
            }
        }

        /// <summary>
        /// The current editor surface ("edit", "source", or "preview") — read-only view
        /// state for the shell (e.g. to re-compose the preview after a theme change).
        /// </summary>
        public string CurrentView => _currentView;

        /// <summary>Raised after the editor surface changes (any switch path).</summary>
        public event EventHandler ViewChanged;

        /// <summary>
        /// Optional provider (set by the shell) that returns the blog theme to layer into
        /// the Preview document, or null for the neutral article style. A provider that
        /// throws is treated as null — a theme miss must never break Preview.
        /// </summary>
        public Func<Task<BlogThemeStyle>> PreviewThemeProvider { get; set; }

        /// <summary>Optional post title surfaced in the preview heading (set by the host).</summary>
        public string PreviewTitle { get; set; }

        /// <summary>
        /// Switches the editor surface ("edit", "source", or "preview") — the same
        /// path as the on-canvas view toggle buttons. Used by the View menu.
        /// Unknown view names are ignored.
        /// </summary>
        public void SetView(string view)
        {
            if (view != "edit" && view != "source" && view != "preview")
                return;
            _ = SwitchView(view);
        }

        /// <summary>Awaitable form of <see cref="SetView"/> for callers that must
        /// complete the swap before continuing (e.g. insert commands).</summary>
        public Task SetViewAsync(string view)
        {
            if (view != "edit" && view != "source" && view != "preview")
                return Task.CompletedTask;
            return SwitchView(view);
        }

        // Ensures editor-targeted commands (inserts, formatting) run against the
        // visible editor: in Source/Preview view the WebView is hidden, so a command
        // would silently apply to an invisible surface (Windows simply switches to
        // the Edit view in this case too).
        private Task EnsureEditViewAsync() =>
            _currentView == "edit" ? Task.CompletedTask : SwitchView("edit");

        private async Task SwitchView(string view)
        {
            var previousView = _currentView;
            _currentView = view;

            var editorHost = this.FindControl<ContentControl>("EditorHost");
            var sourceEditor = this.FindControl<global::AvaloniaEdit.TextEditor>("SourceEditor");
            var previewHost = this.FindControl<ContentControl>("PreviewHost");

            if (editorHost != null) editorHost.IsVisible = (view == "edit");
            if (sourceEditor != null) sourceEditor.IsVisible = (view == "source");
            if (previewHost != null) previewHost.IsVisible = (view == "preview");

            if (view == "source")
            {
                // Get HTML from WebView and show in source editor. Long base64
                // data-URIs (embedded images) are elided to short tokens — a
                // multi-MB single line stalls text layout and the pane renders
                // blank; tokens are re-expanded on the way back to Edit.
                if (_webViewEditor != null)
                {
                    var html = await _webViewEditor.GetContentAsync();
                    if (sourceEditor != null)
                    {
                        string display = SourceViewSanitizer.ElideDataUris(html ?? "", _sourceDataUris);
                        sourceEditor.Text = FormatHtml(display);
                    }
                }
                StatusChanged?.Invoke(this, "Source view");
            }
            else if (view == "edit")
            {
                // Coming from Source view: push the (possibly hand-edited) HTML back
                // into the WebView. ONLY from source — the SourceEditor holds a stale
                // snapshot after any further editing, so pushing it on a Preview->Edit
                // switch would wipe real content (including inserted images).
                if (previousView == "source")
                {
                    var source = this.FindControl<global::AvaloniaEdit.TextEditor>("SourceEditor");
                    if (source != null && !string.IsNullOrEmpty(source.Text) && _webViewEditor != null)
                    {
                        string restored = SourceViewSanitizer.RestoreDataUris(source.Text, _sourceDataUris);
                        await _webViewEditor.SetContentAsync(restored);
                    }
                }
                StatusChanged?.Invoke(this, "Edit view");
            }
            else if (view == "preview")
            {
                await PopulatePreviewAsync(previewHost);
                StatusChanged?.Invoke(this, "Preview view");
            }

            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        // Lazily created read-only WebView used to render the Preview surface.
        private NativeWebView _previewWebView;

        // Full data-URI values elided from the Source view (token order), restored
        // when the user pushes edited source back into the editor.
        private readonly List<string> _sourceDataUris = new();

        /// <summary>
        /// Renders the current editor body into the Preview host as it would look
        /// published, using <see cref="PreviewRenderer"/> to compose the article
        /// document. When the shell supplied a <see cref="PreviewThemeProvider"/> and
        /// it returns a theme ("Use Theme" on for the current blog), the blog's
        /// stylesheets are layered in; otherwise the preview stays neutral. The
        /// composition is pure/testable; the on-screen display uses a lightweight
        /// read-only WebView (navigated to a temp file, mirroring the editor's own
        /// file-load path). Failure to create/navigate the WebView — or to fetch the
        /// theme — is non-fatal: the neutral composition still renders.
        /// </summary>
        private async Task PopulatePreviewAsync(ContentControl previewHost)
        {
            if (previewHost == null || _webViewEditor == null)
                return;

            BlogThemeStyle theme = null;
            if (PreviewThemeProvider != null)
            {
                try
                {
                    theme = await PreviewThemeProvider();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OLW-Preview] Theme provider failed: {ex.Message}");
                }
            }

            string body = await _webViewEditor.GetContentAsync() ?? string.Empty;
            string document = PreviewRenderer.BuildPreviewDocument(body, PreviewTitle, theme: theme);

            try
            {
                if (_previewWebView == null)
                {
                    _previewWebView = new NativeWebView
                    {
                        HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
                        VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch
                    };
                    previewHost.Content = _previewWebView;
                }

                string tempDir = Path.Combine(Path.GetTempPath(), "OpenLiveWriter", "preview");
                Directory.CreateDirectory(tempDir);
                string tempFile = Path.Combine(tempDir, "preview.html");
                await File.WriteAllTextAsync(tempFile, document);
                _previewWebView.Navigate(new Uri("file://" + tempFile));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Preview] Render failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a standalone HTML document (e.g. the <see cref="PrintRenderer"/>
        /// composition) into the preview WebView and returns it once navigation
        /// completes, so the shell can invoke print/PDF APIs on the rendered page.
        /// Returns null when the WebView backend is unavailable or navigation times
        /// out (headless) — callers degrade to a file handoff in that case.
        /// </summary>
        public async Task<NativeWebView> LoadPreviewDocumentAsync(string document)
        {
            var previewHost = this.FindControl<ContentControl>("PreviewHost");
            if (previewHost == null || document == null)
                return null;

            try
            {
                if (_previewWebView == null)
                {
                    _previewWebView = new NativeWebView
                    {
                        HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
                        VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch
                    };
                    previewHost.Content = _previewWebView;
                }

                string tempDir = Path.Combine(Path.GetTempPath(), "OpenLiveWriter", "print");
                Directory.CreateDirectory(tempDir);
                string tempFile = Path.Combine(tempDir, "print-document.html");
                await File.WriteAllTextAsync(tempFile, document);

                _previewWebView.Navigate(new Uri("file://" + tempFile));

                // NavigationCompleted is unreliable in the macOS backend (the page
                // reaches readyState 'complete' but the event may never fire), so
                // poll the DOM: ready when OUR document is loaded, not about:blank.
                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(250);
                    try
                    {
                        string probe = await _previewWebView.InvokeScript(
                            "document.readyState + '|' + location.href");
                        if (probe != null &&
                            probe.StartsWith("complete|") &&
                            probe.Contains("print-document.html"))
                        {
                            return _previewWebView;
                        }
                    }
                    catch
                    {
                        // navigation still in flight — keep polling
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Print] Document load failed: {ex.Message}");
                return null;
            }
        }

        internal static string FormatHtml(string html)
        {
            // Basic HTML formatting for readability in source view
            if (string.IsNullOrEmpty(html)) return html;
            return html
                .Replace("><", ">\n<")
                .Replace("<br>", "<br>\n")
                .Replace("<br/>", "<br/>\n")
                .Replace("<br />", "<br />\n");
        }

        /// <summary>
        /// Maps a heading-combo selection index to the <c>formatBlock</c> tag.
        /// Kept for tests and ribbon SemanticHtmlGallery parity with
        /// <see cref="SemanticHtmlStyles"/>.
        /// </summary>
        internal static string MapHeadingIndexToTag(int index) =>
            SemanticHtmlStyles.TagForIndex(index);

        /// <summary>
        /// Applies a semantic block style by tag (e.g. from the ribbon
        /// SemanticHtmlGallery). Ignores unknown tags.
        /// </summary>
        internal async Task ApplySemanticStyleAsync(string tag)
        {
            if (_webViewEditor == null || !SemanticHtmlStyles.IsKnownTag(tag))
                return;
            await _webViewEditor.SetBlockFormatAsync(tag);
            StatusChanged?.Invoke(this, $"Applied {tag} formatting");
        }

        private void RegisterCommandBridgeHandlers()
        {
            _commandBridge.RegisterHandler(CommandId.Bold, () => _ = ExecuteFormatCommandAsync("bold"));
            _commandBridge.RegisterHandler(CommandId.Italic, () => _ = ExecuteFormatCommandAsync("italic"));
            _commandBridge.RegisterHandler(CommandId.Underline, () => _ = ExecuteFormatCommandAsync("underline"));
            _commandBridge.RegisterHandler(CommandId.Strikethrough, () => _ = ExecuteFormatCommandAsync("strikethrough"));
            _commandBridge.RegisterHandler(CommandId.InsertLink, () => _ = ShowInsertLinkDialogAsync());
            _commandBridge.RegisterHandler(CommandId.InsertPictureFromFile, () => _ = ShowInsertImageDialogAsync());
            _commandBridge.RegisterHandler(CommandId.InsertImageSplit, () => _ = ShowInsertImageDialogAsync());
            _commandBridge.RegisterHandler(CommandId.Bullets, () => _ = ExecuteFormatCommandAsync("bulletlist"));
            _commandBridge.RegisterHandler(CommandId.Numbers, () => _ = ExecuteFormatCommandAsync("numberlist"));
            _commandBridge.RegisterHandler(CommandId.Undo, () => StatusChanged?.Invoke(this, "Undo"));
            _commandBridge.RegisterHandler(CommandId.Redo, () => StatusChanged?.Invoke(this, "Redo"));
        }

        private void SetupFindBar()
        {
            if (FindNextButton != null)
                FindNextButton.Click += async (s, e) => await FindNextAsync(forward: true);
            if (FindPreviousButton != null)
                FindPreviousButton.Click += async (s, e) => await FindNextAsync(forward: false);
            if (FindCloseButton != null)
                FindCloseButton.Click += (s, e) => HideFindBar();
            if (FindReplaceDialogButton != null)
                FindReplaceDialogButton.Click += (s, e) => OpenFindReplaceRequested?.Invoke(this, EventArgs.Empty);
            if (FindMatchCaseCheck != null)
                FindMatchCaseCheck.IsCheckedChanged += async (s, e) => await UpdateMatchCountAsync();
            if (FindQueryBox != null)
            {
                FindQueryBox.KeyDown += async (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        await FindNextAsync(forward: !e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Escape)
                    {
                        HideFindBar();
                        e.Handled = true;
                    }
                };
                FindQueryBox.TextChanged += async (s, e) => await UpdateMatchCountAsync();
            }
        }

        /// <summary>Shows the in-editor find bar and focuses the query field.</summary>
        public void ShowFindBar(string initialQuery = null)
        {
            if (FindBar == null) return;
            FindBar.IsVisible = true;
            if (FindQueryBox != null)
            {
                if (!string.IsNullOrEmpty(initialQuery))
                    FindQueryBox.Text = initialQuery;
                FindQueryBox.Focus();
                FindQueryBox.SelectAll();
            }
        }

        /// <summary>Hides the in-editor find bar.</summary>
        public void HideFindBar()
        {
            if (FindBar != null)
                FindBar.IsVisible = false;
        }

        /// <summary>True when the in-editor find bar is visible.</summary>
        public bool IsFindBarVisible => FindBar?.IsVisible == true;

        private async Task FindNextAsync(bool forward)
        {
            if (_webViewEditor == null || FindQueryBox == null) return;
            string query = FindQueryBox.Text ?? string.Empty;
            if (string.IsNullOrEmpty(query))
            {
                StatusChanged?.Invoke(this, "Enter text to find.");
                return;
            }

            bool matchCase = FindMatchCaseCheck?.IsChecked == true;
            if (forward)
                await _webViewEditor.FindNextAsync(query, matchCase);
            else
                await _webViewEditor.FindPreviousAsync(query, matchCase);
            await UpdateMatchCountAsync();
            StatusChanged?.Invoke(this, forward ? $"Find: {query}" : $"Find previous: {query}");
        }

        // Refreshes the find bar's "n of m" readout from the live editor.
        private async Task UpdateMatchCountAsync()
        {
            if (FindMatchCountText == null)
                return;

            string query = FindQueryBox?.Text ?? string.Empty;
            if (_webViewEditor == null || string.IsNullOrEmpty(query))
            {
                FindMatchCountText.Text = string.Empty;
                return;
            }

            FindStats stats = await _webViewEditor.FindStatsAsync(
                query, FindMatchCaseCheck?.IsChecked == true);
            FindMatchCountText.Text = stats == null
                ? string.Empty
                : FormatMatchCount(stats.Current, stats.Total);
        }

        /// <summary>
        /// Formats the find bar's match-count readout: "n of m" when a match is
        /// selected, "No matches" when there are none, and "m matches" when matches
        /// exist but none is current. Pure/deterministic for headless testing.
        /// </summary>
        internal static string FormatMatchCount(int current, int total)
        {
            if (total <= 0) return "No matches";
            return current > 0 ? $"{current} of {total}" : $"{total} matches";
        }

        private void SetupKeyboardShortcuts()
        {
            this.KeyDown += (s, e) =>
            {
                bool metaOrCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Meta) ||
                                  e.KeyModifiers.HasFlag(KeyModifiers.Control);
                if (!metaOrCtrl)
                {
                    if (e.Key == Key.Escape && IsFindBarVisible)
                    {
                        HideFindBar();
                        e.Handled = true;
                    }
                    return;
                }

                switch (e.Key)
                {
                    case Key.B:
                        _ = ExecuteFormatCommandAsync("bold");
                        e.Handled = true;
                        break;
                    case Key.I:
                        _ = ExecuteFormatCommandAsync("italic");
                        e.Handled = true;
                        break;
                    case Key.U:
                        _ = ExecuteFormatCommandAsync("underline");
                        e.Handled = true;
                        break;
                    case Key.K:
                        _ = ShowInsertLinkDialogAsync();
                        e.Handled = true;
                        break;
                    case Key.F:
                        ShowFindBar();
                        e.Handled = true;
                        break;
                    case Key.G:
                        // Cmd+G / Ctrl+G → find next (common macOS / editor convention)
                        if (IsFindBarVisible)
                            _ = FindNextAsync(forward: !e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                        else
                            ShowFindBar();
                        e.Handled = true;
                        break;
                }
            };
        }

        private async Task ShowInsertLinkDialogAsync()
        {
            if (_webViewEditor == null) return;
            await EnsureEditViewAsync();

            var owner = TopLevel.GetTopLevel(this) as Window;
            var result = await LinkDialog.ShowAsync(owner);
            if (result == null || string.IsNullOrWhiteSpace(result.Url))
                return;

            await _webViewEditor.InsertLinkAsync(result.Url, result.Text, result.Title, result.OpenInNewWindow);
            StatusChanged?.Invoke(this, $"Inserted link: {result.Url}");
        }

        // Image file types offered by the Insert Picture file dialog.
        private static readonly FilePickerFileType ImageFileType = new("Images")
        {
            Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.webp", "*.svg" },
            AppleUniformTypeIdentifiers = new[] { "public.image" },
            MimeTypes = new[] { "image/*" }
        };

        /// <summary>
        /// Opens a file picker (via the Avalonia storage provider) to choose an
        /// image and inserts it into the editor; the file is copied into the current
        /// document's media folder and referenced by a <c>file://</c> src.
        /// </summary>
        private async Task ShowInsertImageDialogAsync()
        {
            if (_webViewEditor == null) return;
            await EnsureEditViewAsync();

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider == null)
            {
                StatusChanged?.Invoke(this, "Insert Image: file picker unavailable.");
                return;
            }

            IReadOnlyList<IStorageFile> files;
            try
            {
                files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Insert Picture",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { ImageFileType }
                });
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Insert Image failed: {ex.Message}");
                return;
            }

            if (files == null || files.Count == 0)
                return;

            string path = files[0].TryGetLocalPath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                StatusChanged?.Invoke(this, "Insert Image: could not resolve file path.");
                return;
            }

            await _webViewEditor.InsertImageFromFileAsync(path);
            StatusChanged?.Invoke(this, $"Inserted image: {Path.GetFileName(path)}");
        }

        private async Task ExecuteFormatCommandAsync(string format)
        {
            if (_webViewEditor == null) return;
            await EnsureEditViewAsync();

            switch (format)
            {
                case "bold": await _webViewEditor.ExecuteBoldAsync(); break;
                case "italic": await _webViewEditor.ExecuteItalicAsync(); break;
                case "underline": await _webViewEditor.ExecuteUnderlineAsync(); break;
                case "strikethrough": await _webViewEditor.ExecuteStrikethroughAsync(); break;
                case "bulletlist": await _webViewEditor.ExecuteUnorderedListAsync(); break;
                case "numberlist": await _webViewEditor.ExecuteOrderedListAsync(); break;
            }

            StatusChanged?.Invoke(this, $"Applied {format} formatting");
        }
    }
}
