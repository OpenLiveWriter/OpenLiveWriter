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
            InitializeWebViewEditor();
            SetupViewToggle();
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

        private void SetupViewToggle()
        {
            if (EditViewButton != null)
                EditViewButton.Click += (s, e) => SwitchView("edit");
            if (SourceViewButton != null)
                SourceViewButton.Click += (s, e) => SwitchView("source");
            if (PreviewViewButton != null)
                PreviewViewButton.Click += (s, e) => SwitchView("preview");
        }

        private async void SwitchView(string view)
        {
            _currentView = view;

            // Update toggle states
            if (EditViewButton != null) EditViewButton.IsChecked = (view == "edit");
            if (SourceViewButton != null) SourceViewButton.IsChecked = (view == "source");
            if (PreviewViewButton != null) PreviewViewButton.IsChecked = (view == "preview");

            var editorHost = this.FindControl<ContentControl>("EditorHost");
            var sourceEditor = this.FindControl<TextBox>("SourceEditor");
            var previewHost = this.FindControl<ContentControl>("PreviewHost");

            if (editorHost != null) editorHost.IsVisible = (view == "edit");
            if (sourceEditor != null) sourceEditor.IsVisible = (view == "source");
            if (previewHost != null) previewHost.IsVisible = (view == "preview");

            if (view == "source")
            {
                // Get HTML from WebView and show in source editor
                if (_webViewEditor != null)
                {
                    var html = await _webViewEditor.GetContentAsync();
                    if (sourceEditor != null)
                        sourceEditor.Text = FormatHtml(html ?? "");
                }
                StatusChanged?.Invoke(this, "Source view");
            }
            else if (view == "edit")
            {
                // If coming from source view, push HTML back to WebView
                var source = this.FindControl<TextBox>("SourceEditor");
                if (source != null && !string.IsNullOrEmpty(source.Text) && _webViewEditor != null)
                {
                    await _webViewEditor.SetContentAsync(source.Text);
                }
                StatusChanged?.Invoke(this, "Edit view");
            }
            else if (view == "preview")
            {
                await PopulatePreviewAsync(previewHost);
                StatusChanged?.Invoke(this, "Preview view");
            }
        }

        // Lazily created read-only WebView used to render the Preview surface.
        private NativeWebView _previewWebView;

        /// <summary>
        /// Renders the current editor body into the Preview host as it would look
        /// published, using <see cref="PreviewRenderer"/> to compose a neutral article
        /// document. The composition is pure/testable; the on-screen display uses a
        /// lightweight read-only WebView (navigated to a temp file, mirroring the
        /// editor's own file-load path). Failure to create/navigate the WebView is
        /// non-fatal — the source composition is still available for tests.
        /// </summary>
        private async Task PopulatePreviewAsync(ContentControl previewHost)
        {
            if (previewHost == null || _webViewEditor == null)
                return;

            string body = await _webViewEditor.GetContentAsync() ?? string.Empty;
            string document = PreviewRenderer.BuildPreviewDocument(body, PreviewTitle);

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

        /// <summary>Optional post title surfaced in the preview heading (set by the host).</summary>
        public string PreviewTitle { get; set; }

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
            // window.find does not support backwards search natively in all engines;
            // forward find is the primary path. Previous reuses FindNext for now.
            await _webViewEditor.FindNextAsync(query, matchCase);
            StatusChanged?.Invoke(this, forward ? $"Find: {query}" : $"Find previous: {query}");
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
        /// image and inserts it into the editor as an inline data-URI <c>&lt;img&gt;</c>.
        /// </summary>
        private async Task ShowInsertImageDialogAsync()
        {
            if (_webViewEditor == null) return;

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
