// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// HTML of the current selection - updated by JS on selectionchange event
        /// </summary>
        public string SelectionHtml { get; set; } = "";

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
        private bool _isInitialized;
        private bool _isDirty;
        private string _pendingHtml;
        private string _pendingFilePath;
        private WebView2HtmlEditorCommandSource _commandSource;
        private EditorContentBridge _contentBridge;
        
        /// <summary>
        /// Fired when the editor has finished loading and is ready for editing.
        /// </summary>
        public event EventHandler ReadyForEditing;

        /// <summary>
        /// The error that prevented WebView2 initialization, or null when
        /// initialization succeeded or is still in progress. Surfaced for
        /// diagnostics and tests; initialization errors are otherwise only
        /// written to the debug log.
        /// </summary>
        internal Exception InitializationError { get; private set; }
        
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
            _commandSource = new WebView2HtmlEditorCommandSource(this);
            
            // Subscribe to paragraph setting changes to apply immediately
            WebView2Document.UseParagraphTagsChanged += OnUseParagraphTagsChanged;
            
            InitializeWebView();
        }
        
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

        private bool _formActivatedHooked;

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                HookParentFormActivated();
                NudgeWebViewRepaint();
            }
        }

        /// <summary>
        /// WebView2 intermittently renders fully black after view switches or
        /// dialog popups (observed heavily under x64 emulation on ARM64 Windows);
        /// the surface recovers on its own only after a minimize/maximize. Force
        /// a frame whenever the editor resurfaces or the host window reactivates.
        /// </summary>
        private void HookParentFormActivated()
        {
            if (_formActivatedHooked) return;
            var form = FindForm();
            if (form == null) return;
            _formActivatedHooked = true;
            form.Activated += (s, args) => NudgeWebViewRepaint();
        }

        private void NudgeWebViewRepaint()
        {
            try
            {
                if (_webView == null || _webView.IsDisposed || !_webView.IsHandleCreated) return;
                // A 1px resize-and-restore makes the WebView2 controller re-emit
                // its bounds, which forces the compositor to produce a new frame.
                var size = _webView.Size;
                if (size.Width > 1 && size.Height > 1)
                {
                    _webView.Size = new Size(size.Width - 1, size.Height);
                    _webView.Size = size;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} NudgeWebViewRepaint error: {ex.Message}");
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
                        // Inject host object sync listeners after navigation completes
                        await SetupHostObjectListeners();
                        
                        // Set paragraph separator based on user preference
                        await ApplyParagraphSeparatorSetting();
                        
                        // Notify command source that WebView2 is ready
                        _commandSource.SetWebView(_webView);
                        
                        // Fire ReadyForEditing event - editor is now fully operational
                        ReadyForEditing?.Invoke(this, EventArgs.Empty);
                        
                        // Fire CommandStateChanged to update command enablement (e.g., Find button)
                        _commandSource.OnCommandStateChanged();
                    }
                };
                
                // Keyboard shortcuts: Ctrl+B/I/U/Z/Y are handled natively by
                // Chromium's contenteditable; Ctrl+K is handled by the JS keydown
                // handler injected in SetupHostObjectListeners.

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
                InitializationError = ex;
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
                        
                        // Selection text with line breaks between blocks, so the
                        // hyperlink dialog's link text does not concatenate a
                        // multi-block selection into one unbroken run.
                        function olwSelectionText(sel) {
                            if (!sel || sel.rangeCount === 0) return '';
                            if (sel.isCollapsed) return '';
                            var container = document.createElement('div');
                            container.appendChild(sel.getRangeAt(0).cloneContents());
                            var html = container.innerHTML
                                .replace(/<br\s*\/?>/gi, '\n')
                                .replace(/<\/(p|div|li|h[1-6]|blockquote|tr|table|ul|ol)>/gi, '\n');
                            var tmp = document.createElement('div');
                            tmp.innerHTML = html;
                            return (tmp.textContent || '')
                                .replace(/\n{3,}/g, '\n\n')
                                .replace(/^\n+|\n+$/g, '');
                        }

                        // Sync selection and context state to bridge
                        function syncSelectionState() {
                            var sel = window.getSelection();
                            olw.Selection = olwSelectionText(sel);

                            // Capture selection HTML (for SelectedHtml consumers)
                            var selHtml = '';
                            if (sel && sel.rangeCount > 0 && !sel.isCollapsed) {
                                var container = document.createElement('div');
                                container.appendChild(sel.getRangeAt(0).cloneContents());
                                selHtml = container.innerHTML;
                            }
                            olw.SelectionHtml = selHtml;
                            
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

                        // Apply a formatting command in CSS mode so Chromium emits
                        // span+style markup instead of deprecated <font> tags, then
                        // re-sync content to the bridge. Mirrors the macOS editor's
                        // transient styleWithCSS pattern in editor.html.
                        window.olwExecCss = function(command, value) {
                            document.execCommand('styleWithCSS', false, true);
                            document.execCommand(command, false, value);
                            document.execCommand('styleWithCSS', false, false);
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                        };

                        // Apply an exact point font size. execCommand('fontSize') only
                        // supports the 1-7 scale (keyword sizes in CSS mode), so apply
                        // size 7 and rewrite the resulting xxx-large spans to the
                        // requested pt value (same trick as the macOS setFontSizePx).
                        window.olwApplyFontSizePt = function(pt) {
                            document.execCommand('styleWithCSS', false, true);
                            document.execCommand('fontSize', false, '7');
                            document.execCommand('styleWithCSS', false, false);
                            // Match on the serialized style attribute; the parsed
                            // style.fontSize value is engine-specific for keywords.
                            var spans = bodyEl.querySelectorAll('span[style*=""xxx-large""]');
                            for (var i = 0; i < spans.length; i++) {
                                spans[i].style.fontSize = pt + 'pt';
                            }
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                            return 'rewrote ' + spans.length + ' span(s) to ' + pt + 'pt';
                        };
                        
                        // Handle Ctrl+K for hyperlink insertion via postMessage
                        document.addEventListener('keydown', function(e) {
                            if (e.ctrlKey && (e.key === 'k' || e.key === 'K')) {
                                e.preventDefault();
                                e.stopPropagation();
                                window.chrome.webview.postMessage(JSON.stringify({ type: 'insertLink' }));
                            }
                        });
                        
                        // Track Chromium history operations (undo/redo). The div-to-p
                        // observer must not mutate the document while an undo/redo
                        // transaction is being applied: observer edits are not part of
                        // Chromium's undo stack, so mutating then corrupts it (e.g. undo
                        // after applying a heading duplicates the restored paragraphs).
                        var olwHistoryOpPending = false;
                        bodyEl.addEventListener('beforeinput', function(e) {
                            if (e.inputType === 'historyUndo' || e.inputType === 'historyRedo') {
                                olwHistoryOpPending = true;
                                // Safety net: clear on the next tick if the observer
                                // batch for this operation never fires.
                                setTimeout(function() { olwHistoryOpPending = false; }, 0);
                            }
                        });

                        // Undo/redo entry point for host-driven commands. execCommand
                        // does not reliably fire beforeinput, so flag the operation
                        // explicitly before running it.
                        window.olwHistoryCommand = function(cmd) {
                            olwHistoryOpPending = true;
                            document.execCommand(cmd);
                            setTimeout(function() { olwHistoryOpPending = false; }, 0);
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                        };

                        // Use MutationObserver to convert <div> to <p> when useParagraphTags is true
                        // Chromium's defaultParagraphSeparator doesn't work, so we post-process
                        var observer = new MutationObserver(function(mutations) {
                            if (window.olwUseParagraphTags === false) return;
                            if (olwHistoryOpPending) {
                                // Skip corrections during undo/redo so the history
                                // transaction applies exactly as Chromium recorded it.
                                olwHistoryOpPending = false;
                                return;
                            }
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
                                    } else if (node.nodeType === 3 && node.parentNode === bodyEl && node.textContent.trim() !== '') {
                                        // Wrap bare text added directly to the body
                                        // (e.g. the first line of a multi-line paste)
                                        // in a paragraph like the other lines.
                                        var wrapper = document.createElement('p');
                                        bodyEl.insertBefore(wrapper, node);
                                        wrapper.appendChild(node);
                                    }
                                });
                            });
                            // Observer corrections do not fire input events, so
                            // push the corrected markup to the bridge explicitly;
                            // otherwise GetEditedHtml lags one edit behind.
                            syncContent();
                        });
                        observer.observe(bodyEl, { childList: true });

                        // True when the element carries no content worth keeping.
                        // A lone <br> counts as content: that is how Chromium
                        // represents an intentional blank paragraph.
                        function olwIsEmpty(el) {
                            if (!el) return true;
                            if (el.querySelector('img,br,hr,table,iframe,object,embed,video,audio')) return false;
                            return (el.textContent || '').replace(/\u00a0/g, '').trim() === '';
                        }

                        // Chromium's insertUnorderedList/insertOrderedList can nest
                        // the new list inside the current <p>, producing invalid
                        // <p><ul>...</ul></p> markup. Hoist such lists out of the
                        // paragraph, splitting it when text surrounds the list.
                        function olwFixNestedLists() {
                            var lists = bodyEl.querySelectorAll('p > ul, p > ol');
                            for (var i = 0; i < lists.length; i++) {
                                var list = lists[i];
                                var p = list.parentNode;
                                if (!p || p.tagName !== 'P') continue;
                                var afterP = document.createElement('p');
                                var node = list.nextSibling;
                                while (node) {
                                    var next = node.nextSibling;
                                    afterP.appendChild(node);
                                    node = next;
                                }
                                p.parentNode.insertBefore(list, p.nextSibling);
                                if (!olwIsEmpty(afterP)) {
                                    p.parentNode.insertBefore(afterP, list.nextSibling);
                                }
                                if (olwIsEmpty(p)) {
                                    p.parentNode.removeChild(p);
                                }
                            }
                        }

                        // Tidy execCommand DOM artifacts: drop empty lists, merge
                        // adjacent same-type lists, and drop empty paragraphs left
                        // adjacent to lists/blockquotes by block commands.
                        window.olwCleanupBlocks = function() {
                            olwFixNestedLists();
                            var changed = true;
                            while (changed) {
                                changed = false;
                                var all = bodyEl.querySelectorAll('ul, ol, blockquote');
                                for (var i = 0; i < all.length; i++) {
                                    var el = all[i];
                                    // The NodeList is static: nodes detached by an
                                    // earlier merge in this pass must be skipped.
                                    if (!el.parentNode) continue;
                                    if (olwIsEmpty(el)) {
                                        el.parentNode.removeChild(el);
                                        changed = true;
                                        break;
                                    }
                                    var sib = el.nextElementSibling;
                                    if (sib && sib.tagName === el.tagName) {
                                        while (sib.firstChild) el.appendChild(sib.firstChild);
                                        sib.parentNode.removeChild(sib);
                                        changed = true;
                                        break;
                                    }
                                }
                            }
                            var ps = bodyEl.querySelectorAll('p');
                            for (var j = ps.length - 1; j >= 0; j--) {
                                var p = ps[j];
                                if (p.children.length > 0 || (p.textContent || '').trim() !== '') continue;
                                var prev = p.previousElementSibling;
                                var nextSib = p.nextElementSibling;
                                var nearBlock = function(n) {
                                    return n && (n.tagName === 'UL' || n.tagName === 'OL' || n.tagName === 'BLOCKQUOTE');
                                };
                                if (nearBlock(prev) || nearBlock(nextSib)) {
                                    p.parentNode.removeChild(p);
                                }
                            }
                        };

                        // Insert a list and repair the DOM so ul/ol end up as
                        // siblings of p, never nested inside one.
                        window.olwInsertList = function(ordered) {
                            document.execCommand(ordered ? 'insertOrderedList' : 'insertUnorderedList');
                            window.olwCleanupBlocks();
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                        };

                        // Apply a block format (H1-H6, P, ...) to every block in the
                        // selection. Chromium's formatBlock collapses a multi-block
                        // selection into a single heading joined by <br>; formatting
                        // each block individually matches the classic OLW behavior.
                        window.olwFormatBlock = function(tag) {
                            var sel = window.getSelection();
                            if (!sel || sel.rangeCount === 0 || sel.getRangeAt(0).collapsed) {
                                document.execCommand('formatBlock', false, tag);
                                bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                                return;
                            }
                            var range = sel.getRangeAt(0);
                            var blockTags = ['P','DIV','H1','H2','H3','H4','H5','H6','LI','BLOCKQUOTE','PRE'];
                            var blocks = [];
                            var walker = document.createTreeWalker(bodyEl, NodeFilter.SHOW_ELEMENT, null);
                            var node;
                            while ((node = walker.nextNode())) {
                                if (blockTags.indexOf(node.tagName) < 0) continue;
                                if (!range.intersectsNode(node)) continue;
                                // Skip blocks already covered by a collected ancestor.
                                var anc = node.parentNode;
                                var covered = false;
                                while (anc && anc !== bodyEl) {
                                    if (blocks.indexOf(anc) >= 0) { covered = true; break; }
                                    anc = anc.parentNode;
                                }
                                if (!covered) blocks.push(node);
                            }
                            if (blocks.length <= 1) {
                                document.execCommand('formatBlock', false, tag);
                                bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                                return;
                            }
                            var firstNew = null;
                            var lastNew = null;
                            for (var i = 0; i < blocks.length; i++) {
                                if (!blocks[i].parentNode) continue; // detached by an earlier step
                                var r = document.createRange();
                                r.selectNodeContents(blocks[i]);
                                sel.removeAllRanges();
                                sel.addRange(r);
                                document.execCommand('formatBlock', false, tag);
                                var newBlock = findBlockParent(sel.anchorNode);
                                if (newBlock) {
                                    if (!firstNew) firstNew = newBlock;
                                    lastNew = newBlock;
                                }
                            }
                            // Re-select across the formatted blocks.
                            if (firstNew && lastNew) {
                                var restore = document.createRange();
                                restore.setStart(firstNew, 0);
                                restore.setEnd(lastNew, lastNew.childNodes.length);
                                sel.removeAllRanges();
                                sel.addRange(restore);
                            }
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                        };

                        // Wrap the selection in a blockquote (or outdent out of one)
                        // and repair the execCommand artifacts: Chromium splits a
                        // quoted list into one list per item and litters empty
                        // paragraphs around the blockquote.
                        window.olwApplyBlockquote = function() {
                            document.execCommand('formatBlock', false, 'blockquote');
                            window.olwCleanupBlocks();
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                        };

                        window.olwRemoveBlockquote = function() {
                            document.execCommand('outdent');
                            window.olwCleanupBlocks();
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                        };

                        function olwDepth(el) {
                            var d = 0;
                            while (el.parentNode) { d++; el = el.parentNode; }
                            return d;
                        }

                        // Clear formatting, matching the classic (0.6.2) behavior:
                        // removeFormat only strips inline markup, so additionally
                        // convert headings/list items back to paragraphs, unwrap
                        // lists and blockquotes, and reset block alignment.
                        window.olwClearFormatting = function() {
                            var sel = window.getSelection();
                            var range = (sel && sel.rangeCount > 0) ? sel.getRangeAt(0) : null;
                            document.execCommand('removeFormat');
                            if (range) {
                                var i;
                                // Rename headings and list items in the selection to p.
                                var toRename = [];
                                var all = bodyEl.querySelectorAll('h1,h2,h3,h4,h5,h6,li');
                                for (i = 0; i < all.length; i++) {
                                    if (range.intersectsNode(all[i])) toRename.push(all[i]);
                                }
                                for (i = 0; i < toRename.length; i++) {
                                    var el = toRename[i];
                                    if (!el.parentNode) continue;
                                    var p = document.createElement('p');
                                    while (el.firstChild) p.appendChild(el.firstChild);
                                    el.parentNode.replaceChild(p, el);
                                }
                                // Unwrap lists and blockquotes, innermost first.
                                var wrappers = [];
                                all = bodyEl.querySelectorAll('ul,ol,blockquote');
                                for (i = 0; i < all.length; i++) {
                                    if (range.intersectsNode(all[i])) wrappers.push(all[i]);
                                }
                                wrappers.sort(function(a, b) { return olwDepth(b) - olwDepth(a); });
                                for (i = 0; i < wrappers.length; i++) {
                                    var w = wrappers[i];
                                    if (!w.parentNode) continue;
                                    while (w.firstChild) w.parentNode.insertBefore(w.firstChild, w);
                                    w.parentNode.removeChild(w);
                                }
                                // Reset alignment on the remaining blocks.
                                all = bodyEl.querySelectorAll('p,div,h1,h2,h3,h4,h5,h6,li,blockquote');
                                for (i = 0; i < all.length; i++) {
                                    if (range.intersectsNode(all[i])) {
                                        all[i].removeAttribute('align');
                                        all[i].style.textAlign = '';
                                    }
                                }
                            }
                            window.olwCleanupBlocks();
                            bodyEl.dispatchEvent(new Event('input', { bubbles: true }));
                        };
                        
                        // Sync initial content
                        syncContent();
                        syncSelectionState();
                        
                        return 'listeners setup ok';
                    })();
                ";
                
                string result = null;
                // The editing shell elements and the host object bridge can lag
                // NavigationCompleted by a beat; retry a few times before giving up.
                for (var attempt = 0; attempt < 4; attempt++)
                {
                    result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} SetupHostObjectListeners result: {result} (attempt {attempt + 1})");
                    if (result == null ||
                        (!result.Contains("elements not found") && !result.Contains("hostObjects not available")))
                    {
                        break;
                    }
                    await Task.Delay(150);
                }
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

        private string GetEditorContent()
        {
            return _contentBridge.Body ?? "";
        }
        
        public string GetEditedTitleHtml()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] #{_instanceId} GetEditedTitleHtml - bridge title length: {_contentBridge.Title?.Length ?? 0}, value: '{_contentBridge.Title}'");
            return _contentBridge.Title ?? "";
        }

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
        
        /// <summary>
        /// Navigates to a complete HTML document, replacing the current page.
        /// Used for the read-only preview document, which lacks the editing
        /// shell elements (#olw-title/#olw-body) that LoadHtmlFile patches in place.
        /// </summary>
        public void NavigateToHtmlDocument(string html)
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                _webView.CoreWebView2.NavigateToString(html);
            }
            else
            {
                // Stash until initialization completes; InitializeWebView navigates to it.
                _pendingHtml = html;
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
            // Read directly from the content bridge - JS syncs on every input event
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
                // Synced from JS on selectionchange (see SetupHostObjectListeners)
                return _contentBridge?.SelectionHtml ?? "";
            }
        }

        public void EmptySelection()
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                _ = _webView.CoreWebView2.ExecuteScriptAsync("window.getSelection().removeAllRanges()");
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
                // Replacing a multi-item list selection with the link leaves an
                // empty list behind; clean up the artifacts (queued after the
                // insert script, so execution order is preserved).
                _ = _webView.CoreWebView2.ExecuteScriptAsync(
                    "window.olwCleanupBlocks && window.olwCleanupBlocks()");
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
        private WebView2 _webView;

        public WebView2HtmlEditorCommandSource(WebView2HtmlEditorControl editor)
        {
            _editor = editor;
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

        /// <summary>
        /// Executes a formatting command in CSS mode via the olwExecCss helper
        /// (injected by SetupHostObjectListeners) so the document gains
        /// span+style markup instead of deprecated font tags.
        /// </summary>
        private void ExecuteCssCommand(string command, string value)
        {
            if (_webView?.CoreWebView2 == null) return;

            var script = $"window.olwExecCss && window.olwExecCss('{command}', '{value}')";

            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ExecuteCssCommand: {script}");
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

        /// <summary>
        /// Executes an arbitrary script against the editor document.
        /// </summary>
        private void ExecuteScript(string script)
        {
            if (_webView?.CoreWebView2 == null) return;

            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ExecuteScript: {script}");
            _ = _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        // ISimpleTextEditorCommandSource
        public bool HasFocus => _editor?.ContainsFocus ?? false;
        public bool CanUndo => QueryCommandEnabled("undo");
        public void Undo() => ExecuteScript("window.olwHistoryCommand && window.olwHistoryCommand('undo')");
        public bool CanRedo => QueryCommandEnabled("redo");
        public void Redo() => ExecuteScript("window.olwHistoryCommand && window.olwHistoryCommand('redo')");
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
#pragma warning disable CS0067 // Event is never used - required by interface
        public event EventHandler AggressiveCommandStateChanged;
#pragma warning restore CS0067

        // IHtmlEditorCommandSource
        public void ViewSource() { /* TODO */ }
        public void ClearFormatting() => ExecuteScript("window.olwClearFormatting && window.olwClearFormatting()");
        public bool CanApplyFormatting(CommandId? commandId) => _webView?.CoreWebView2 != null;

        public string SelectionFontFamily => null; // TODO
        public void ApplyFontFamily(string fontFamily) => ExecuteCssCommand("fontName", fontFamily);

        public float SelectionFontSize => 0; // TODO: would need to sync this via bridge

        /// <summary>
        /// Applies an exact point font size. Browser execCommand only supports the
        /// 1-7 scale, so the injected olwApplyFontSizePt helper applies size 7 in
        /// CSS mode and rewrites the resulting keyword-sized spans to the exact pt
        /// value, producing span+style markup instead of font tags.
        /// </summary>
        public void ApplyFontSize(float fontSize)
        {
            if (_webView?.CoreWebView2 == null) return;

            var script = string.Format(CultureInfo.InvariantCulture,
                "window.olwApplyFontSizePt && window.olwApplyFontSizePt({0})", fontSize);

            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ApplyFontSize: {script}");
            _ = _webView.CoreWebView2.ExecuteScriptAsync(script).ContinueWith(t =>
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] ApplyFontSize result: {t.Result}"));
        }

        public int SelectionForeColor => 0;
        public void ApplyFontForeColor(int color)
        {
            var c = Color.FromArgb(color);
            ExecuteCssCommand("foreColor", $"#{c.R:X2}{c.G:X2}{c.B:X2}");
        }

        public int SelectionBackColor => 0;
        public void ApplyFontBackColor(int? color)
        {
            if (color.HasValue)
            {
                var c = Color.FromArgb(color.Value);
                ExecuteCssCommand("hiliteColor", $"#{c.R:X2}{c.G:X2}{c.B:X2}");
            }
        }

        public string SelectionStyleName => _editor?.ContentBridge?.CurrentBlockTag;
        
        /// <summary>
        /// Applies HTML formatting style (H1, H2, P, etc.) per selected block via
        /// the olwFormatBlock helper. Raw formatBlock collapses a multi-paragraph
        /// selection into a single heading joined by br separators.
        /// </summary>
        public void ApplyHtmlFormattingStyle(IHtmlFormattingStyle style)
        {
            if (style == null) return;
            var elementName = style.ElementName?.ToUpperInvariant();
            if (string.IsNullOrEmpty(elementName)) return;

            // formatBlock needs angle brackets for the tag
            ExecuteScript($"window.olwFormatBlock && window.olwFormatBlock('<{elementName}>')");
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
        public void ApplyBullets() => ExecuteScript("window.olwInsertList && window.olwInsertList(false)");

        public bool SelectionNumbered => QueryCommandState("insertOrderedList");
        public void ApplyNumbers() => ExecuteScript("window.olwInsertList && window.olwInsertList(true)");

        public bool CanIndent => true;
        public void ApplyIndent() => ExecuteCommand("indent");

        public bool CanOutdent => true;
        public void ApplyOutdent() => ExecuteCommand("outdent");

        /// <summary>
        /// Toggles blockquote. If already in blockquote, outdents. Otherwise wraps in blockquote.
        /// Both paths run a cleanup pass that merges the per-item list fragments
        /// Chromium creates and removes empty paragraph litter.
        /// </summary>
        public void ApplyBlockquote()
        {
            if (SelectionBlockquoted)
            {
                // Remove blockquote by outdenting
                ExecuteScript("window.olwRemoveBlockquote && window.olwRemoveBlockquote()");
            }
            else
            {
                // Wrap in blockquote using formatBlock
                ExecuteScript("window.olwApplyBlockquote && window.olwApplyBlockquote()");
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

        public bool CanFind => _editor?.IsInitialized == true && _webView?.CoreWebView2 != null;
        
        public void Find()
        {
            if (!CanFind) return;
            
            // Show a simple Find dialog
            using (var dialog = new FindTextForm())
            {
                var parentForm = _editor?.FindForm();
                if (dialog.ShowDialog(parentForm) == DialogResult.OK && !string.IsNullOrEmpty(dialog.SearchText))
                {
                    FindText(dialog.SearchText, dialog.MatchCase, dialog.SearchBackward);
                }
            }
        }
        
        private string _lastSearchText;
        private bool _lastMatchCase;
        private bool _lastSearchBackward;
        
        /// <summary>
        /// Find text in the document using JavaScript window.find()
        /// </summary>
        private void FindText(string searchText, bool matchCase, bool searchBackward)
        {
            if (string.IsNullOrEmpty(searchText) || _webView?.CoreWebView2 == null)
                return;
            
            _lastSearchText = searchText;
            _lastMatchCase = matchCase;
            _lastSearchBackward = searchBackward;
            
            // Use window.find() - works in Chromium-based browsers
            // Parameters: searchText, caseSensitive, backwards, wrapAround, wholeWord, searchInFrames, showDialog
            var escapedText = searchText.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
            var script = $"window.find('{escapedText}', {matchCase.ToString().ToLower()}, {searchBackward.ToString().ToLower()}, true, false, false, false);";
            
            _ = _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        
        /// <summary>
        /// Find next occurrence of the last search text
        /// </summary>
        public void FindNext()
        {
            if (!string.IsNullOrEmpty(_lastSearchText))
            {
                FindText(_lastSearchText, _lastMatchCase, false);
            }
        }
        
        /// <summary>
        /// Find previous occurrence of the last search text
        /// </summary>
        public void FindPrevious()
        {
            if (!string.IsNullOrEmpty(_lastSearchText))
            {
                FindText(_lastSearchText, _lastMatchCase, true);
            }
        }

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

        public void OnCommandStateChanged()
        {
            CommandStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
