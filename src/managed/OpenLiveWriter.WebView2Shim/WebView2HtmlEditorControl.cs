// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.WebView2Shim
{
    /// <summary>
    /// COM-visible class exposed to JavaScript for bidirectional communication.
    /// JS updates these properties on input/selection change, C# reads them synchronously.
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class EditorContentBridge
    {
        private string _title = "";
        private string _body = "";
        private string _selection = "";
        
        public string Title 
        { 
            get => _title;
            set
            {
                _title = value;
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Bridge.Title SET: {value?.Length ?? 0} chars");
            }
        }
        
        public string Body 
        { 
            get => _body;
            set
            {
                _body = value;
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Bridge.Body SET: {value?.Length ?? 0} chars");
            }
        }
        
        /// <summary>
        /// Current selection text - updated by JS on selectionchange event
        /// </summary>
        public string Selection
        {
            get => _selection;
            set => _selection = value ?? "";
        }
        
        // Link state - synced when selection changes
        public bool IsInLink { get; set; } = false;
        public string LinkHref { get; set; } = "";
        public string LinkText { get; set; } = "";
        public string LinkTitle { get; set; } = "";
        public string LinkRel { get; set; } = "";
        public string LinkTarget { get; set; } = "";
        
        // Block state - synced when selection changes
        public bool IsInBlockquote { get; set; } = false;
        public string CurrentBlockTag { get; set; } = ""; // H1, H2, P, etc.
        
        public bool IsDirty { get; set; } = false;
        
        public void MarkDirty()
        {
            IsDirty = true;
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Bridge.MarkDirty() called");
        }
    }
    
    /// <summary>
    /// WebView2-based HTML editor control that implements IHtmlEditor.
    /// This is designed to be a drop-in replacement for the MSHTML-based editor.
    /// </summary>
    public class WebView2HtmlEditorControl : UserControl, IHtmlEditor
    {
        private static int _instanceCounter;
        private readonly int _instanceId;
        private WebView2 _webView;
        private WebView2Bridge _bridge;
        private WebView2Document _document;
        private bool _isInitialized;
        private bool _isDirty;
        private string _pendingHtml;
        private string _pendingFilePath;
        private WebView2HtmlEditorCommandSource _commandSource;
        private EditorContentBridge _contentBridge;
        private WebView2EditorSelection _editorSelection;
        
        /// <summary>
        /// Fired when the editor has finished loading and is ready for editing.
        /// </summary>
        public event EventHandler ReadyForEditing;
        
        /// <summary>
        /// Fired when the selection changes in the editor.
        /// </summary>
        public event EventHandler<EditorSelectionChangedEventArgs> SelectionChanged;
        
        /// <summary>
        /// Fired when HTML is inserted into the editor.
        /// </summary>
        public event EventHandler HtmlInserted;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                WebView2Document.UseParagraphTagsChanged -= OnUseParagraphTagsChanged;
            }
            base.Dispose(disposing);
        }

        public WebView2HtmlEditorControl()
        {
            _instanceId = ++_instanceCounter;
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2HtmlEditorControl#{_instanceId} created");
            _contentBridge = new EditorContentBridge();
            InitializeComponent();
            // Create command source immediately so it's never null
            _commandSource = new WebView2HtmlEditorCommandSource(this, null);
            
            // Subscribe to paragraph setting changes to apply immediately
            WebView2Document.UseParagraphTagsChanged += OnUseParagraphTagsChanged;
            
            InitializeWebView();
        }
        
        /// <summary>
        /// Gets the current editor selection.
        /// </summary>
        public IEditorSelection EditorSelection => _editorSelection;
        
        private async void OnUseParagraphTagsChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} OnUseParagraphTagsChanged fired, UseParagraphTags={WebView2Document.UseParagraphTags}");
            // Re-apply the paragraph separator setting when changed in Options
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                await ApplyParagraphSeparatorSetting();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} OnUseParagraphTagsChanged - NOT applying (initialized={_isInitialized}, webView null={_webView?.CoreWebView2 == null})");
            }
        }

        private void InitializeComponent()
        {
            BackColor = System.Drawing.Color.White;
            
            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = System.Drawing.Color.White
            };
            Controls.Add(_webView);
        }

        private async void InitializeWebView()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2HtmlEditorControl#{_instanceId}.InitializeWebView starting");
                
                // Use shared environment to avoid conflicts when multiple WebView2 controls initialize
                var env = await WebView2EnvironmentManager.GetEnvironmentAsync();
                await _webView.EnsureCoreWebView2Async(env);
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2HtmlEditorControl#{_instanceId}.EnsureCoreWebView2Async completed");
                
                // Set background color after initialization
                _webView.DefaultBackgroundColor = System.Drawing.Color.White;
                
                // Expose the content bridge to JavaScript - this allows synchronous read/write
                _webView.CoreWebView2.AddHostObjectToScript("olw", _contentBridge);
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} Host object 'olw' added to script");
                
                // Set up message handler for Ctrl+K and other JS-initiated actions
                _webView.CoreWebView2.WebMessageReceived += (s, e) =>
                {
                    try
                    {
                        var json = e.WebMessageAsJson;
                        if (json?.Contains("insertLink") == true)
                        {
                            // Get selection from bridge (synced by JS on selectionchange)
                            var selectedText = _contentBridge.Selection;
                            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Received insertLink from JS, selection from bridge: '{selectedText}'");
                            _commandSource.ShowInsertLinkDialog(selectedText);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebMessageReceived error: {ex.Message}");
                    }
                };
                
                // Set up virtual host mapping for local file access
                // WebView2 blocks file:// URLs for security, so we map drive letters to virtual hosts
                // file:///C:/path/image.png -> https://olw-local-c/path/image.png
                foreach (var drive in System.IO.DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == System.IO.DriveType.Fixed)
                    {
                        var driveLetter = drive.Name[0].ToString().ToLowerInvariant();
                        var hostName = $"olw-local-{driveLetter}";
                        var folderPath = drive.RootDirectory.FullName;
                        _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                            hostName, folderPath, CoreWebView2HostResourceAccessKind.Allow);
                        System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Mapped {hostName} -> {folderPath}");
                    }
                }
                
                // Mark as initialized once CoreWebView2 is ready - we can now navigate
                _isInitialized = true;
                
                _webView.CoreWebView2.NavigationStarting += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} NavigationStarting - URL: {e.Uri}");
                };
                
                _webView.CoreWebView2.NavigationCompleted += async (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} NavigationCompleted - IsSuccess: {e.IsSuccess}, URL: {_webView.CoreWebView2.Source}");
                    if (e.IsSuccess)
                    {
                        // Initialize the DOM bridge - must happen after each navigation
                        InitializeBridge();
                        
                        // Inject host object sync listeners after navigation completes
                        await SetupHostObjectListeners();
                        
                        // Set paragraph separator based on user preference
                        await ApplyParagraphSeparatorSetting();
                        
                        // Notify command source that WebView2 is ready
                        _commandSource.SetWebView(_webView);
                        
                        // Fire ReadyForEditing event - editor is now fully operational
                        ReadyForEditing?.Invoke(this, EventArgs.Empty);
                    }
                };
                
                // Handle keyboard shortcuts via CoreWebView2Controller
                _webView.CoreWebView2.GetDevToolsProtocolEventReceiver("cycler").DevToolsProtocolEventReceived += (s, e) => { };
                var controller = (Microsoft.Web.WebView2.Core.CoreWebView2Controller)typeof(WebView2)
                    .GetProperty("CoreWebView2Controller", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_webView);
                
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} CoreWebView2Controller obtained: {controller != null}");
                    
                if (controller != null)
                {
                    controller.AcceleratorKeyPressed += (s, e) =>
                    {
                        // Check for Ctrl+key combinations on KeyDown
                        if (e.KeyEventKind == Microsoft.Web.WebView2.Core.CoreWebView2KeyEventKind.KeyDown &&
                            (Control.ModifierKeys & Keys.Control) == Keys.Control)
                        {
                            var key = (Keys)e.VirtualKey;
                            bool handled = false;
                            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] AcceleratorKey: Ctrl+{key}");
                            
                            switch (key)
                            {
                                case Keys.B:
                                    _commandSource.ApplyBold();
                                    handled = true;
                                    break;
                                case Keys.I:
                                    _commandSource.ApplyItalic();
                                    handled = true;
                                    break;
                                case Keys.U:
                                    _commandSource.ApplyUnderline();
                                    handled = true;
                                    break;
                                case Keys.K:
                                    System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] Ctrl+K detected, calling InsertLink");
                                    _commandSource.InsertLink();
                                    handled = true;
                                    break;
                                case Keys.Z:
                                    _commandSource.Undo();
                                    handled = true;
                                    break;
                                case Keys.Y:
                                    _commandSource.Redo();
                                    handled = true;
                                    break;
                            }
                            
                            if (handled)
                            {
                                e.Handled = true;
                            }
                        }
                    };
                }

                // Check if we have pending html to load
                if (!string.IsNullOrEmpty(_pendingHtml))
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} Loading pending html, length: {_pendingHtml.Length}");
                    var html = _pendingHtml;
                    _pendingHtml = null;
                    _webView.CoreWebView2.NavigateToString(html);
                }
                else if (!string.IsNullOrEmpty(_pendingFilePath) && File.Exists(_pendingFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} Loading pending file: {_pendingFilePath}");
                    var html = File.ReadAllText(_pendingFilePath);
                    _pendingFilePath = null;
                    _webView.CoreWebView2.NavigateToString(html);
                }
                else
                {
                    // Load the editor template
                    var editorHtml = GetEditorTemplate();
                    _webView.CoreWebView2.NavigateToString(editorHtml);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2HtmlEditorControl init error: {ex.Message}");
            }
        }
        
        private async Task SetupHostObjectListeners()
        {
            try
            {
                // Inject JavaScript that sets up input listeners to sync content to the host object
                var script = @"
                    (function() {
                        if (window.olwListenersSetup) return 'already setup';
                        
                        var titleEl = document.getElementById('olw-title');
                        var bodyEl = document.getElementById('olw-body');
                        
                        if (!titleEl || !bodyEl) return 'elements not found';
                        if (!window.chrome || !window.chrome.webview || !window.chrome.webview.hostObjects) return 'hostObjects not available';
                        
                        window.olwListenersSetup = true;
                        var olw = window.chrome.webview.hostObjects.sync.olw;
                        
                        // Find parent element of given type
                        function findParent(node, tagName) {
                            while (node && node !== bodyEl) {
                                if (node.nodeType === 1 && node.tagName === tagName) return node;
                                node = node.parentNode;
                            }
                            return null;
                        }
                        
                        // Find closest block element (H1-H6, P, DIV, BLOCKQUOTE)
                        function findBlockParent(node) {
                            var blockTags = ['H1','H2','H3','H4','H5','H6','P','DIV','BLOCKQUOTE','PRE','LI'];
                            while (node && node !== bodyEl && node !== document.body) {
                                if (node.nodeType === 1 && blockTags.indexOf(node.tagName) >= 0) {
                                    return node;
                                }
                                node = node.parentNode;
                            }
                            return null;
                        }
                        
                        // Sync selection and context state to bridge
                        function syncSelectionState() {
                            var sel = window.getSelection();
                            olw.Selection = sel ? sel.toString() : '';
                            
                            // Get anchor node for context
                            var node = sel && sel.anchorNode ? sel.anchorNode : null;
                            if (node && node.nodeType === 3) node = node.parentNode; // text node -> parent
                            
                            // Check if we're inside a link
                            var anchor = findParent(node, 'A');
                            olw.IsInLink = !!anchor;
                            if (anchor) {
                                olw.LinkHref = anchor.href || '';
                                olw.LinkText = anchor.innerText || '';
                                olw.LinkTitle = anchor.title || '';
                                olw.LinkRel = anchor.rel || '';
                                olw.LinkTarget = anchor.target || '';
                            } else {
                                olw.LinkHref = '';
                                olw.LinkText = '';
                                olw.LinkTitle = '';
                                olw.LinkRel = '';
                                olw.LinkTarget = '';
                            }
                            
                            // Check if we're inside a blockquote
                            olw.IsInBlockquote = !!findParent(node, 'BLOCKQUOTE');
                            
                            // Get current block element type
                            var block = findBlockParent(node);
                            olw.CurrentBlockTag = block ? block.tagName : '';
                        }
                        
                        // Sync selection to bridge on every selection change
                        document.addEventListener('selectionchange', syncSelectionState);
                        
                        function syncContent() {
                            olw.Title = titleEl.innerHTML;
                            olw.Body = bodyEl.innerHTML;
                            olw.MarkDirty();
                        }
                        
                        titleEl.addEventListener('input', syncContent);
                        bodyEl.addEventListener('input', syncContent);
                        
                        // Handle Ctrl+K for hyperlink insertion via postMessage
                        document.addEventListener('keydown', function(e) {
                            if (e.ctrlKey && (e.key === 'k' || e.key === 'K')) {
                                e.preventDefault();
                                e.stopPropagation();
                                window.chrome.webview.postMessage(JSON.stringify({ type: 'insertLink' }));
                            }
                        });
                        
                        // Use MutationObserver to convert <div> to <p> when useParagraphTags is true
                        // Chromium's defaultParagraphSeparator doesn't work, so we post-process
                        var observer = new MutationObserver(function(mutations) {
                            if (window.olwUseParagraphTags === false) return;
                            mutations.forEach(function(mutation) {
                                mutation.addedNodes.forEach(function(node) {
                                    if (node.nodeName === 'DIV' && node.parentElement === bodyEl) {
                                        // Convert div to p
                                        var p = document.createElement('p');
                                        p.innerHTML = node.innerHTML;
                                        node.parentElement.replaceChild(p, node);
                                        // Move cursor to end of new p
                                        var sel = window.getSelection();
                                        var range = document.createRange();
                                        range.selectNodeContents(p);
                                        range.collapse(false);
                                        sel.removeAllRanges();
                                        sel.addRange(range);
                                    }
                                });
                            });
                        });
                        observer.observe(bodyEl, { childList: true });
                        
                        // Sync initial content
                        syncContent();
                        syncSelectionState();
                        
                        return 'listeners setup ok';
                    })();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} SetupHostObjectListeners result: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} SetupHostObjectListeners error: {ex.Message}");
            }
        }

        private async Task ApplyParagraphSeparatorSetting()
        {
            try
            {
                // Set JS variable that the Enter key handler uses
                // Chromium's defaultParagraphSeparator command doesn't work reliably
                var usePTags = WebView2Document.UseParagraphTags ? "true" : "false";
                var script = $"window.olwUseParagraphTags = {usePTags};";
                await _webView.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} Set olwUseParagraphTags to {usePTags}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} ApplyParagraphSeparatorSetting error: {ex.Message}");
            }
        }

        private async void InitializeBridge()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} InitializeBridge starting");
                
                // Create the bridge but DON'T call Initialize() - it uses sync message pump that deadlocks
                _bridge = new WebView2Bridge(_webView);
                
                // Instead, inject the bridge script directly using async API
                var bridgeScript = _bridge.GetBridgeScript();
                if (!string.IsNullOrEmpty(bridgeScript))
                {
                    await _webView.CoreWebView2.ExecuteScriptAsync(bridgeScript);
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} Bridge script injected");
                }
                
                _document = new WebView2Document(_bridge, _webView);
                _commandSource.SetDocument(_document);
                
                // Initialize the editor selection tracker
                _editorSelection = new WebView2EditorSelection(_bridge, async () => 
                {
                    return await _webView.CoreWebView2.ExecuteScriptAsync("");
                });
                
                // Set up content change monitoring for olw-body element
                await _webView.CoreWebView2.ExecuteScriptAsync(@"
                    var body = document.getElementById('olw-body');
                    if (body) {
                        body.addEventListener('input', function() {
                            window.chrome.webview.postMessage(JSON.stringify({ type: 'contentChanged' }));
                        });
                    }
                    console.log('[OLW] Content change monitoring setup');
                ");
                
                // Set up control selection (images, tables)
                var result = await _webView.CoreWebView2.ExecuteScriptAsync("typeof OLW");
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} typeof OLW = {result}");
                
                var setupResult = await _webView.CoreWebView2.ExecuteScriptAsync("OLW.setupControlSelection ? OLW.setupControlSelection() : 'not available'");
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} Control selection setup: {setupResult}");
                
                // Register message handler (only once)
                if (!_messageHandlerRegistered)
                {
                    _messageHandlerRegistered = true;
                    _webView.CoreWebView2.WebMessageReceived += (s, e) =>
                    {
                        try
                        {
                            var json = e.WebMessageAsJson;
                            // Handle contentChanged message
                            if (json?.Contains("contentChanged") == true)
                            {
                                IsDirty = true;
                            }
                            // Handle insertLink (Ctrl+K) message
                            else if (json?.Contains("insertLink") == true)
                            {
                                System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] Received insertLink message from JS (Ctrl+K)");
                                // Use BeginInvoke to avoid blocking WebView2
                                BeginInvoke(new Action(() => _commandSource.InsertLink()));
                            }
                            // Handle control selection (image clicked)
                            else if (json?.Contains("controlSelected") == true)
                            {
                                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Control selected: {json}");
                                // Parse the message to get tagName and editorId
                                var parsed = ParseControlSelectionMessage(json);
                                // Create explicit local copies for closure capture
                                string capturedTagName = parsed.TagName;
                                string capturedEditorId = parsed.EditorId;
                                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Parsed: tagName={capturedTagName}, editorId={capturedEditorId}");
                                // Use BeginInvoke to avoid deadlock with WebView2
                                BeginInvoke(new Action(() => 
                                {
                                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] In BeginInvoke: tagName={capturedTagName}, editorId={capturedEditorId}");
                                    _editorSelection?.UpdateFromControlSelection(capturedTagName, capturedEditorId);
                                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] After UpdateFromControlSelection: type={_editorSelection?.SelectionType}");
                                    OnControlSelected?.Invoke(this, EventArgs.Empty);
                                    SelectionChanged?.Invoke(this, new EditorSelectionChangedEventArgs(_editorSelection));
                                }));
                            }
                            // Handle control cleared
                            else if (json?.Contains("controlCleared") == true)
                            {
                                System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] Control selection cleared");
                                BeginInvoke(new Action(() => 
                                {
                                    // NOTE: We no longer sync body here - SetElementAttributes already syncs
                                    // after DOM changes. Syncing here caused race conditions where old body
                                    // would overwrite new values from resize commands.
                                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Selection cleared - skipping body sync, current length: {_contentBridge.Body?.Length ?? 0}");
                                    
                                    _editorSelection?.ClearSelection();
                                    OnControlCleared?.Invoke(this, EventArgs.Empty);
                                    SelectionChanged?.Invoke(this, new EditorSelectionChangedEventArgs(_editorSelection));
                                }));
                            }
                            // Handle double-click on control (open properties)
                            else if (json?.Contains("controlDoubleClick") == true)
                            {
                                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Control double-clicked: {json}");
                                BeginInvoke(new Action(() => OnControlDoubleClick?.Invoke(this, EventArgs.Empty)));
                            }
                        }
                        catch { }
                    };
                }
                
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} InitializeBridge complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Bridge init error: {ex.Message}");
            }
        }
        
        private bool _messageHandlerRegistered = false;
        
        /// <summary>
        /// Parse a control selection message to extract tagName and editorId.
        /// JSON format: {"type":"controlSelected","tagName":"IMG","id":"olw-1"}
        /// Note: WebView2's WebMessageAsJson wraps the message as a JSON string, so we need to unwrap it first.
        /// </summary>
        private (string TagName, string EditorId) ParseControlSelectionMessage(string json)
        {
            try
            {
                // WebView2 wraps the message in quotes, so we first need to parse it as a string
                // Input: "{\"type\":\"controlSelected\",...}" (a JSON string containing JSON)
                // First parse extracts the inner JSON string
                using var outerDoc = JsonDocument.Parse(json);
                var innerJson = outerDoc.RootElement.GetString();
                
                if (string.IsNullOrEmpty(innerJson))
                {
                    System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] ParseControlSelectionMessage: inner JSON is null/empty");
                    return (null, null);
                }
                
                // Now parse the actual JSON object
                using var doc = JsonDocument.Parse(innerJson);
                var root = doc.RootElement;
                var hasTagName = root.TryGetProperty("tagName", out var tagProp);
                var hasId = root.TryGetProperty("id", out var idProp);
                var tagName = hasTagName ? tagProp.GetString() : null;
                var editorId = hasId ? idProp.GetString() : null;
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ParseControlSelectionMessage: tagName={tagName}, editorId={editorId}");
                return (tagName, editorId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ParseControlSelectionMessage EXCEPTION: {ex.Message}");
                return (null, null);
            }
        }
        
        /// <summary>
        /// Fired when a control (image, table) is selected via click.
        /// </summary>
        public event EventHandler OnControlSelected;
        
        /// <summary>
        /// Fired when control selection is cleared.
        /// </summary>
        public event EventHandler OnControlCleared;
        
        /// <summary>
        /// Fired when a control is double-clicked (typically to open properties).
        /// </summary>
        public event EventHandler OnControlDoubleClick;

        private string GetEditorTemplate()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        html, body { 
            margin: 0; 
            padding: 0; 
            height: 100%;
            background-color: #ffffff;
        }
        body {
            font-family: Segoe UI, Arial, sans-serif;
            font-size: 14px;
            padding: 10px;
        }
        #olw-title {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 10px;
            border-bottom: 1px solid #ccc;
            padding-bottom: 10px;
            outline: none;
        }
        #olw-body {
            min-height: 300px;
            outline: none;
        }
        [contenteditable]:focus {
            outline: none;
        }
    </style>
</head>
<body>
    <div id='olw-title' contenteditable='true'></div>
    <div id='olw-body' contenteditable='true'></div>
</body>
</html>";
        }

        private void SetEditorContent(string html)
        {
            if (_isInitialized && _document != null)
            {
                var editor = _document.getElementById("olw-editor");
                if (editor != null)
                {
                    editor.innerHTML = html ?? "";
                }
            }
        }
        
        private string GetEditorContent()
        {
            return _contentBridge.Body ?? "";
        }
        
        public string GetEditedTitleHtml()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} GetEditedTitleHtml - bridge title length: {_contentBridge.Title?.Length ?? 0}, value: '{_contentBridge.Title}'");
            return _contentBridge.Title ?? "";
        }

        public WebView2Document Document => _document;
        public bool IsInitialized => _isInitialized;

        #region IHtmlEditor Implementation

        public Control EditorControl => this;

        public void LoadHtmlFile(string filePath)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} LoadHtmlFile called - path: {filePath}, exists: {File.Exists(filePath)}, isInitialized: {_isInitialized}");
            if (File.Exists(filePath))
            {
                // Read the HTML content directly
                var htmlContent = File.ReadAllText(filePath);
                _pendingHtml = htmlContent;
                
                if (_isInitialized && _webView.CoreWebView2 != null)
                {
                    // Use JavaScript to update the content directly
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} LoadHtmlFile - updating content via JS, html length: {htmlContent.Length}");
                    UpdateContentViaJavaScript(htmlContent);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} LoadHtmlFile - NOT READY, storing pending html");
                }
            }
        }
        
        private async void UpdateContentViaJavaScript(string html)
        {
            try
            {
                // Extract title and body from the HTML
                // Note: Body can contain nested divs, so we match to the last </div> before </body> or end
                var titleMatch = System.Text.RegularExpressions.Regex.Match(html, @"<div id=""olw-title""[^>]*>(.*?)</div>", System.Text.RegularExpressions.RegexOptions.Singleline);
                
                // For body, find the start tag and capture everything until we hit the closing pattern
                // The body div is followed by </body> or end of document
                var bodyStartMatch = System.Text.RegularExpressions.Regex.Match(html, @"<div id=""olw-body""[^>]*>", System.Text.RegularExpressions.RegexOptions.Singleline);
                var body = "";
                if (bodyStartMatch.Success)
                {
                    var startIndex = bodyStartMatch.Index + bodyStartMatch.Length;
                    // Find the closing </div> that's followed by </body> or whitespace then </body>
                    var endMatch = System.Text.RegularExpressions.Regex.Match(html.Substring(startIndex), @"</div>\s*</body>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (endMatch.Success)
                    {
                        body = html.Substring(startIndex, endMatch.Index);
                    }
                    else
                    {
                        // Fallback: take everything to the last </div>
                        var lastDivIndex = html.LastIndexOf("</div>", StringComparison.OrdinalIgnoreCase);
                        if (lastDivIndex > startIndex)
                        {
                            body = html.Substring(startIndex, lastDivIndex - startIndex);
                        }
                    }
                }
                
                var title = titleMatch.Success ? titleMatch.Groups[1].Value : "";
                
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} UpdateContentViaJavaScript - title length: {title.Length}, body length: {body.Length}");
                
                // Update the content bridge so C# has initial values
                _contentBridge.Title = title;
                _contentBridge.Body = body;
                
                // Escape for JavaScript string
                var escapedTitle = title.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
                var escapedBody = body.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
                
                // Update content AND setup host object sync
                var script = $@"
                    document.getElementById('olw-title').innerHTML = '{escapedTitle}';
                    document.getElementById('olw-body').innerHTML = '{escapedBody}';
                    
                    // Setup change listeners using host object (sync to C#)
                    if (!window.olwListenersSetup) {{
                        window.olwListenersSetup = true;
                        
                        // Get the host object - this is synchronous COM bridge to C#
                        const olw = window.chrome.webview.hostObjects.sync.olw;
                        
                        function syncContentToHost() {{
                            olw.Title = document.getElementById('olw-title').innerHTML;
                            olw.Body = document.getElementById('olw-body').innerHTML;
                            olw.MarkDirty();
                        }}
                        
                        document.getElementById('olw-title').addEventListener('input', syncContentToHost);
                        document.getElementById('olw-body').addEventListener('input', syncContentToHost);
                        
                        console.log('OLW: Host object sync setup');
                    }}
                    
                    // Setup control selection (images, tables) after content is loaded
                    if (typeof OLW !== 'undefined' && OLW.setupControlSelection) {{
                        OLW.setupControlSelection();
                    }}
                    'done';
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} UpdateContentViaJavaScript - ExecuteScriptAsync returned: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} UpdateContentViaJavaScript error: {ex.Message}");
            }
        }

        public string GetEditedHtml(bool preferWellFormed)
        {
            // Sync body from DOM before returning - ensures any programmatic changes are captured
            // This is safe here because GetEditedHtml is called during view switch, not during ribbon commands
            if (_isInitialized && _bridge != null)
            {
                try
                {
                    var syncScript = "(() => { var b = document.getElementById('olw-body'); " +
                        "if (b && window.chrome && window.chrome.webview && window.chrome.webview.hostObjects) { " +
                        "  window.chrome.webview.hostObjects.sync.olw.Body = b.innerHTML; return 'synced'; " +
                        "} return 'no-sync'; })()";
                    var result = _bridge.ExecuteScript(syncScript);
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} GetEditedHtml - sync result: {result}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} GetEditedHtml - sync failed: {ex.Message}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} GetEditedHtml - bridge body length: {_contentBridge.Body?.Length ?? 0}");
            return _contentBridge.Body ?? "";
        }

        public string GetEditedHtmlFast()
        {
            // Same as GetEditedHtml - bridge is always current
            return _contentBridge.Body ?? "";
        }

        public string SelectedText => _contentBridge?.Selection ?? "";
        
        /// <summary>
        /// Access to the content bridge for the command source to read link/block state.
        /// </summary>
        internal EditorContentBridge ContentBridge => _contentBridge;

        public string SelectedHtml
        {
            get
            {
                if (_isInitialized && _document != null)
                {
                    var selection = _document.selection;
                    if (selection.type != "None")
                    {
                        var range = selection.createRange();
                        var html = range.htmlText;
                        range.Dispose();
                        return html;
                    }
                }
                return "";
            }
        }

        public void EmptySelection()
        {
            if (_isInitialized && _document != null)
            {
                _document.selection.empty();
            }
        }
        
        /// <summary>
        /// Focuses the body contenteditable element (not the title).
        /// Called before inserting images to ensure they go in the right place.
        /// </summary>
        public void FocusBody()
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                var script = @"
                    var body = document.getElementById('olw-body');
                    if (body) {
                        body.focus();
                        // Move cursor to end if no selection
                        var sel = window.getSelection();
                        if (sel.rangeCount === 0) {
                            var range = document.createRange();
                            range.selectNodeContents(body);
                            range.collapse(false);
                            sel.removeAllRanges();
                            sel.addRange(range);
                        }
                    }
                ";
                _ = _webView.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] FocusBody called");
            }
        }

        public void InsertHtml(string content, bool moveSelectionRight)
        {
            InsertHtml(content, moveSelectionRight ? HtmlInsertionOptions.MoveCursorAfter : HtmlInsertionOptions.Default);
        }

        private static string GetMimeType(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                case ".svg":
                    return "image/svg+xml";
                case ".ico":
                    return "image/x-icon";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// Converts file:// URLs to virtual host URLs that WebView2 can serve.
        /// file:///C:/path/image.png -> https://olw-local-c/path/image.png
        /// </summary>
        private string ConvertFileUrlsToVirtualHost(string html)
        {
            // Match file:// URLs (file:///C:/... or file:///D:/...)
            var regex = new System.Text.RegularExpressions.Regex(
                @"file:///([A-Za-z]):/",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return regex.Replace(html, match =>
            {
                var driveLetter = match.Groups[1].Value.ToLowerInvariant();
                var result = $"https://olw-local-{driveLetter}/";
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Converted file URL: {match.Value} -> {result}");
                return result;
            });
        }

        public void InsertHtml(string content, HtmlInsertionOptions options)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] InsertHtml RECEIVED: {content}");
            
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                // Convert file:// URLs to virtual host URLs for WebView2
                content = ConvertFileUrlsToVirtualHost(content);
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] InsertHtml AFTER conversion: {content}");
                
                // Properly escape for JavaScript string literal
                // Double-backslash the hex codes so JS string parsing doesn't interpret them
                // We want the regex to find literal "\x3c" and replace with "<"
                var escaped = content
                    .Replace("\\", "\\\\")  // Backslash first!
                    .Replace("\"", "\\\"")  // Double quotes
                    .Replace("'", "\\'")    // Single quotes
                    .Replace("\r", "")      // Remove CR
                    .Replace("\n", "\\n")   // Newlines
                    .Replace("\t", "\\t")   // Tabs
                    .Replace("<", "\\\\x3c")  // Double-escape: C# \\\\x3c -> JS \\x3c -> literal \x3c
                    .Replace(">", "\\\\x3e"); // Same for >
                
                // Focus body first to ensure content goes there, then insert
                // Note: We decode the escaped HTML back in JS before inserting
                // Double-escape the hex codes so JS doesn't interpret them as escape sequences
                var script = @"
                    (function() {
                        var body = document.getElementById('olw-body');
                        var sel = window.getSelection();
                        var activeEl = document.activeElement;
                        
                        // If focus is in title or no selection in body, focus body first
                        if (activeEl && activeEl.id === 'olw-title') {
                            body.focus();
                            // Move to end of body
                            var range = document.createRange();
                            range.selectNodeContents(body);
                            range.collapse(false);
                            sel.removeAllRanges();
                            sel.addRange(range);
                        }
                        
                        // The content comes in with literal backslash-x-3-c sequences (8 chars: \\x3c)
                        // Replace them with actual < and > characters
                        var rawStr = """ + escaped + @""";
                        console.log('[OLW-JS] rawStr before replace:', rawStr.substring(0, 200));
                        var html = rawStr.replace(/\\x3c/g, '<').replace(/\\x3e/g, '>');
                        console.log('[OLW-JS] html after replace:', html.substring(0, 200));
                        document.execCommand('insertHTML', false, html);
                        console.log('[OLW-JS] body.innerHTML after insert:', body.innerHTML.substring(0, 200));
                    })();
                ";
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] InsertHtml: {content.Substring(0, Math.Min(100, content.Length))}...");
                _ = _webView.CoreWebView2.ExecuteScriptAsync(script);
                IsDirty = true;
                
                // Fire HtmlInserted event so images can be processed
                HtmlInserted?.Invoke(this, EventArgs.Empty);
            }
        }

        public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                var target = newWindow ? " target=\"_blank\"" : "";
                var relAttr = !string.IsNullOrEmpty(rel) ? $" rel=\"{System.Net.WebUtility.HtmlEncode(rel)}\"" : "";
                var titleAttr = !string.IsNullOrEmpty(linkTitle) ? $" title=\"{System.Net.WebUtility.HtmlEncode(linkTitle)}\"" : "";
                var html = $"<a href=\"{System.Net.WebUtility.HtmlEncode(url)}\"{titleAttr}{relAttr}{target}>{System.Net.WebUtility.HtmlEncode(linkText)}</a>";
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] InsertLink: {html}");
                InsertHtml(html, HtmlInsertionOptions.MoveCursorAfter);
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    IsDirtyEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public IHtmlEditorCommandSource CommandSource => _commandSource;

        public event EventHandler IsDirtyEvent;

        public bool SuspendAutoSave => false;

        public new void Dispose()
        {
            _bridge?.Dispose();
            _webView?.Dispose();
            base.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Command source for WebView2 editor, handling formatting commands.
    /// </summary>
    internal class WebView2HtmlEditorCommandSource : IHtmlEditorCommandSource
    {
        private readonly WebView2HtmlEditorControl _editor;
        private WebView2Document _document;
        private WebView2 _webView;

        public WebView2HtmlEditorCommandSource(WebView2HtmlEditorControl editor, WebView2Document document)
        {
            _editor = editor;
            _document = document;
        }

        /// <summary>
        /// Updates the document reference after WebView2 initialization.
        /// </summary>
        public void SetDocument(WebView2Document document)
        {
            _document = document;
        }
        
        /// <summary>
        /// Sets the WebView2 reference for direct JavaScript execution.
        /// </summary>
        public void SetWebView(WebView2 webView)
        {
            _webView = webView;
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] CommandSource.SetWebView called, webView null: {webView == null}");
        }
        
        private void ExecuteCommand(string command, string value = null)
        {
            if (_webView?.CoreWebView2 == null) return;
            
            var script = value != null 
                ? $"document.execCommand('{command}', false, '{value}')"
                : $"document.execCommand('{command}')";
            
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ExecuteCommand: {script}");
            _ = _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        
        private bool QueryCommandState(string command)
        {
            // For now, return false - async query is complex
            return false;
        }
        
        private bool QueryCommandEnabled(string command)
        {
            return _webView?.CoreWebView2 != null;
        }

        // ISimpleTextEditorCommandSource
        public bool HasFocus => _editor?.ContainsFocus ?? false;
        public bool CanUndo => QueryCommandEnabled("undo");
        public void Undo() => ExecuteCommand("undo");
        public bool CanRedo => QueryCommandEnabled("redo");
        public void Redo() => ExecuteCommand("redo");
        public bool CanCut => QueryCommandEnabled("cut");
        public void Cut() => ExecuteCommand("cut");
        public bool CanCopy => QueryCommandEnabled("copy");
        public void Copy() => ExecuteCommand("copy");
        public bool CanPaste => true; // WebView2 handles paste internally
        public void Paste() => ExecuteCommand("paste");
        public bool CanClear => true;
        public void Clear() => ExecuteCommand("delete");
        public void SelectAll() => ExecuteCommand("selectAll");
        public void InsertEuroSymbol() => _editor?.InsertHtml("€", false);
        public bool ReadOnly => false;
        public event EventHandler CommandStateChanged;
        public event EventHandler AggressiveCommandStateChanged;

        // IHtmlEditorCommandSource
        public void ViewSource() { /* TODO */ }
        public void ClearFormatting() => ExecuteCommand("removeFormat");
        public bool CanApplyFormatting(CommandId? commandId) => _webView?.CoreWebView2 != null;

        public string SelectionFontFamily => null; // TODO
        public void ApplyFontFamily(string fontFamily) => ExecuteCommand("fontName", fontFamily);

        public float SelectionFontSize => 0; // TODO: would need to sync this via bridge
        
        /// <summary>
        /// Applies font size. Browser execCommand uses size 1-7, not points.
        /// OLW uses point sizes, so we map: 8pt->1, 10pt->2, 12pt->3, 14pt->4, 18pt->5, 24pt->6, 36pt->7
        /// </summary>
        public void ApplyFontSize(float fontSize)
        {
            // Map point size to browser fontSize (1-7)
            int browserSize;
            if (fontSize <= 8) browserSize = 1;
            else if (fontSize <= 10) browserSize = 2;
            else if (fontSize <= 12) browserSize = 3;
            else if (fontSize <= 14) browserSize = 4;
            else if (fontSize <= 18) browserSize = 5;
            else if (fontSize <= 24) browserSize = 6;
            else browserSize = 7;
            
            ExecuteCommand("fontSize", browserSize.ToString());
        }

        public int SelectionForeColor => 0;
        public void ApplyFontForeColor(int color) 
        {
            var c = Color.FromArgb(color);
            ExecuteCommand("foreColor", $"#{c.R:X2}{c.G:X2}{c.B:X2}");
        }

        public int SelectionBackColor => 0;
        public void ApplyFontBackColor(int? color)
        {
            if (color.HasValue)
            {
                var c = Color.FromArgb(color.Value);
                ExecuteCommand("hiliteColor", $"#{c.R:X2}{c.G:X2}{c.B:X2}");
            }
        }

        public string SelectionStyleName => _editor?.ContentBridge?.CurrentBlockTag;
        
        /// <summary>
        /// Applies HTML formatting style (H1, H2, P, etc.) using formatBlock command.
        /// </summary>
        public void ApplyHtmlFormattingStyle(IHtmlFormattingStyle style)
        {
            if (style == null) return;
            var elementName = style.ElementName?.ToUpperInvariant();
            if (string.IsNullOrEmpty(elementName)) return;
            
            // formatBlock needs angle brackets for the tag
            ExecuteCommand("formatBlock", $"<{elementName}>");
        }

        public bool SelectionBold => QueryCommandState("bold");
        public void ApplyBold()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ApplyBold called, _webView null: {_webView == null}");
            ExecuteCommand("bold");
        }

        public bool SelectionItalic => QueryCommandState("italic");
        public void ApplyItalic()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ApplyItalic called, _webView null: {_webView == null}");
            ExecuteCommand("italic");
        }

        public bool SelectionUnderlined => QueryCommandState("underline");
        public void ApplyUnderline()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ApplyUnderline called, _webView null: {_webView == null}");
            ExecuteCommand("underline");
        }

        public bool SelectionStrikethrough => QueryCommandState("strikeThrough");
        public void ApplyStrikethrough() => ExecuteCommand("strikeThrough");

        public bool SelectionSuperscript => QueryCommandState("superscript");
        public void ApplySuperscript() => ExecuteCommand("superscript");

        public bool SelectionSubscript => QueryCommandState("subscript");
        public void ApplySubscript() => ExecuteCommand("subscript");

        public bool SelectionIsLTR => true;
        public void InsertLTRTextBlock() { /* TODO */ }
        public bool SelectionIsRTL => false;
        public void InsertRTLTextBlock() { /* TODO */ }

        public EditorTextAlignment GetSelectionAlignment() => EditorTextAlignment.None; // TODO
        public void ApplyAlignment(EditorTextAlignment alignment)
        {
            switch (alignment)
            {
                case EditorTextAlignment.Left: ExecuteCommand("justifyLeft"); break;
                case EditorTextAlignment.Center: ExecuteCommand("justifyCenter"); break;
                case EditorTextAlignment.Right: ExecuteCommand("justifyRight"); break;
                case EditorTextAlignment.Justify: ExecuteCommand("justifyFull"); break;
            }
        }

        public bool SelectionBulleted => QueryCommandState("insertUnorderedList");
        public void ApplyBullets() => ExecuteCommand("insertUnorderedList");

        public bool SelectionNumbered => QueryCommandState("insertOrderedList");
        public void ApplyNumbers() => ExecuteCommand("insertOrderedList");

        public bool CanIndent => true;
        public void ApplyIndent() => ExecuteCommand("indent");

        public bool CanOutdent => true;
        public void ApplyOutdent() => ExecuteCommand("outdent");

        /// <summary>
        /// Toggles blockquote. If already in blockquote, outdents. Otherwise wraps in blockquote.
        /// </summary>
        public void ApplyBlockquote()
        {
            if (SelectionBlockquoted)
            {
                // Remove blockquote by outdenting
                ExecuteCommand("outdent");
            }
            else
            {
                // Wrap in blockquote using formatBlock
                ExecuteCommand("formatBlock", "<blockquote>");
            }
        }
        
        public bool SelectionBlockquoted => _editor?.ContentBridge?.IsInBlockquote ?? false;

        public bool CanInsertLink => _webView?.CoreWebView2 != null;
        
        /// <summary>
        /// Called by ribbon button - reads selection from bridge (synced by JS) and shows dialog.
        /// If cursor is in an existing link, populates the dialog with link info for editing.
        /// </summary>
        public void InsertLink()
        {
            var bridge = _editor?.ContentBridge;
            var isEditing = bridge?.IsInLink ?? false;
            
            string selectedText;
            string existingUrl = null;
            string existingTitle = null;
            string existingRel = null;
            bool existingNewWindow = false;
            
            if (isEditing)
            {
                // Editing existing link - use the link's text and attributes
                selectedText = bridge.LinkText ?? "";
                existingUrl = bridge.LinkHref;
                existingTitle = bridge.LinkTitle;
                existingRel = bridge.LinkRel;
                existingNewWindow = bridge.LinkTarget == "_blank";
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] InsertLink (ribbon) - editing existing link: '{existingUrl}'");
            }
            else
            {
                // New link - use selection
                selectedText = _editor?.SelectedText ?? "";
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] InsertLink (ribbon), selection from bridge: '{selectedText}'");
            }
            
            ShowInsertLinkDialog(selectedText, existingUrl, existingTitle, existingRel, existingNewWindow, isEditing);
        }
        
        /// <summary>
        /// Shows the hyperlink dialog with the given parameters
        /// </summary>
        public void ShowInsertLinkDialog(string selectedText, string url = null, string title = null, string rel = null, bool newWindow = false, bool isEditing = false)
        {
            using (new WaitCursor())
            {
                // Create a temporary CommandManager for the dialog
                using (var commandManager = new CommandManager())
                using (var hyperlinkForm = new HyperlinkForm(commandManager, true))
                {
                    if (!string.IsNullOrEmpty(selectedText))
                        hyperlinkForm.LinkText = selectedText;
                    
                    if (!string.IsNullOrEmpty(url))
                        hyperlinkForm.Hyperlink = url;
                    
                    if (!string.IsNullOrEmpty(title))
                        hyperlinkForm.LinkTitle = title;
                    
                    if (!string.IsNullOrEmpty(rel))
                        hyperlinkForm.Rel = rel;
                    
                    hyperlinkForm.NewWindow = newWindow;
                    hyperlinkForm.EditStyle = isEditing;
                    
                    var owner = _editor?.FindForm();
                    if (hyperlinkForm.ShowDialog(owner) == DialogResult.OK)
                    {
                        if (isEditing)
                        {
                            // When editing, we need to remove the old link first, then insert new one
                            // The user's cursor is in the link, so unlink then insert new
                            ExecuteCommand("unlink");
                        }
                        
                        _editor?.InsertLink(
                            hyperlinkForm.Hyperlink, 
                            hyperlinkForm.LinkText, 
                            hyperlinkForm.LinkTitle, 
                            hyperlinkForm.Rel, 
                            hyperlinkForm.NewWindow);
                    }
                }
            }
        }

        /// <summary>
        /// Can remove link if cursor is inside a link element.
        /// </summary>
        public bool CanRemoveLink => _editor?.ContentBridge?.IsInLink ?? false;
        
        /// <summary>
        /// Removes the link at the cursor position.
        /// </summary>
        public void RemoveLink() => ExecuteCommand("unlink");

        public void OpenLink() { /* TODO */ }
        public void AddToGlossary() { /* TODO */ }

        public bool CanPasteSpecial => false;
        public bool AllowPasteSpecial => false;
        public void PasteSpecial() { /* TODO */ }

        public bool CanFind => false;
        public void Find() { /* TODO */ }

        public bool CanPrint => false;
        public void Print() { /* TODO */ }
        public void PrintPreview() { /* TODO */ }

        /// <summary>
        /// Returns information about the current link at cursor position.
        /// Used by other parts of the app to check link state.
        /// </summary>
        public LinkInfo DiscoverCurrentLink()
        {
            var bridge = _editor?.ContentBridge;
            if (bridge == null || !bridge.IsInLink)
            {
                // Not in a link - return empty info with selection text if any
                return new LinkInfo(_editor?.SelectedText, null, null, null, false);
            }
            
            // In a link - return the link info from the bridge
            bool newWindow = bridge.LinkTarget == "_blank";
            return new LinkInfo(
                bridge.LinkText,
                bridge.LinkHref,
                bridge.LinkTitle,
                bridge.LinkRel,
                newWindow
            );
        }

        public bool CheckSpelling() => true; // TODO

        public bool FullyEditableRegionActive => true;

        public CommandManager CommandManager => null; // TODO

        protected void OnCommandStateChanged()
        {
            CommandStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
