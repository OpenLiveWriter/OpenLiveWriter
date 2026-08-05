// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;

namespace OpenLiveWriter.WebView2Shim
{
    /// <summary>
    /// A WebView2-based HTML source editor using CodeMirror 5 for syntax highlighting.
    /// </summary>
    public class WebView2SourceEditorControl : UserControl
    {
        private WebView2 _webView;
        private bool _isInitialized;
        private bool _initStarted;
        private string _pendingContent;
        private string _currentContent = "";  // Track content in C# to avoid deadlocks
        private bool _isDirty;

        public new event EventHandler TextChanged;
        public new event EventHandler GotFocus;
        public new event EventHandler LostFocus;

        public WebView2SourceEditorControl()
        {
            InitializeComponent();
            // Defer WebView2 initialization until control is shown (avoids conflicts with main editor)
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl.OnVisibleChanged - Visible: {Visible}, _initStarted: {_initStarted}");
            // Initialize WebView2 only when first made visible
            if (Visible && !_initStarted)
            {
                _initStarted = true;
                InitializeWebView();
            }
        }

        private void InitializeComponent()
        {
            BackColor = Color.White;
            
            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = Color.White
            };
            Controls.Add(_webView);
        }

        private async void InitializeWebView()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl.InitializeWebView starting");
            try
            {
                // Use shared environment to avoid conflicts with main editor
                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: Getting shared environment");
                var env = await WebView2EnvironmentManager.GetEnvironmentAsync();
                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: Got environment, calling EnsureCoreWebView2Async");
                await _webView.EnsureCoreWebView2Async(env);
                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: EnsureCoreWebView2Async completed");
                
                _webView.DefaultBackgroundColor = Color.White;
                
                // Handle messages from CodeMirror
                _webView.CoreWebView2.WebMessageReceived += (s, e) =>
                {
                    try
                    {
                        // Use TryGetWebMessageAsString - WebMessageAsJson returns double-encoded JSON
                        var json = e.TryGetWebMessageAsString();
                        // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor: WebMessageReceived: {json}");
                        var message = JsonConvert.DeserializeObject<CodeMirrorMessage>(json);
                        if (message?.type == "ready")
                        {
                            // CodeMirror JS is loaded and ready
                            _isInitialized = true;
                            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] WebView2SourceEditorControl: CodeMirror ready");
                            
                            // Load any pending content
                            if (_pendingContent != null)
                            {
                                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: Loading pending content, {_pendingContent.Length} chars");
                                _ = SetContentAsync(_pendingContent);
                                _pendingContent = null;
                            }
                        }
                        else if (message?.type == "contentChanged")
                        {
                            // Update C# cache with content from JS
                            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor: contentChanged, {message.content?.Length ?? 0} chars");
                            if (message.content != null)
                            {
                                _currentContent = message.content;
                            }
                            _isDirty = true;
                            TextChanged?.Invoke(this, EventArgs.Empty);
                        }
                        else if (message?.type == "focus")
                        {
                            GotFocus?.Invoke(this, EventArgs.Empty);
                        }
                        else if (message?.type == "blur")
                        {
                            LostFocus?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor: Message parse error: {ex.Message}");
                    }
                };

                // Load the CodeMirror editor HTML
                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: Calling NavigateToString");
                
                // Add navigation completed handler - initialize here instead of waiting for JS message
                _webView.CoreWebView2.NavigationCompleted += async (sender, args) =>
                {
                    // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: NavigationCompleted - IsSuccess: {args.IsSuccess}, WebErrorStatus: {args.WebErrorStatus}");
                    
                    if (args.IsSuccess)
                    {
                        // Check if window.chrome.webview exists
                        try
                        {
                            var result = await _webView.CoreWebView2.ExecuteScriptAsync("typeof window.chrome !== 'undefined' && typeof window.chrome.webview !== 'undefined'");
                            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: chrome.webview available: {result}");
                            
                            // Mark as initialized and load pending content
                            _isInitialized = true;
                            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: Marked as initialized");
                            
                            if (_pendingContent != null)
                            {
                                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: Loading pending content: {_pendingContent.Length} chars");
                                await SetContentAsync(_pendingContent);
                                _pendingContent = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: Error checking webview: {ex.Message}");
                        }
                    }
                };
                
                _webView.CoreWebView2.NavigateToString(GetEditorHtml());
                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl: NavigateToString called");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditorControl init error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public new string Text
        {
            get => GetContent();
            set => SetContent(value);
        }

        public bool Modified
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public void SetContent(string html)
        {
            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor.SetContent called, isInitialized: {_isInitialized}, _initStarted: {_initStarted}, html length: {html?.Length ?? 0}");
            _currentContent = html ?? "";  // Always track the content
            
            // Start initialization if not started (handles hidden controls)
            if (!_initStarted)
            {
                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor.SetContent - triggering InitializeWebView");
                _initStarted = true;
                InitializeWebView();
            }
            
            if (!_isInitialized)
            {
                _pendingContent = html;
                // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor.SetContent - stored as pending");
                return;
            }
            _ = SetContentAsync(html);
        }

        private async Task SetContentAsync(string html)
        {
            if (_webView?.CoreWebView2 == null) return;
            
            // Pretty-print HTML for source editing
            html = FormatHtmlForDisplay(html ?? "");
            _currentContent = html; // Update the cache with formatted version
            
            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor.SetContentAsync - sending to JS, length: {html?.Length ?? 0}");
            var escaped = JsonConvert.SerializeObject(html);
            await _webView.CoreWebView2.ExecuteScriptAsync($"setContent({escaped})");
            _isDirty = false;
        }
        
        /// <summary>
        /// Simple HTML formatter - adds line breaks after block elements for readability
        /// </summary>
        internal static string FormatHtmlForDisplay(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            
            // Add line breaks after common block elements
            var blockTags = new[] { "</p>", "</div>", "</h1>", "</h2>", "</h3>", "</h4>", "</h5>", "</h6>", 
                                    "</ul>", "</ol>", "</li>", "</table>", "</tr>", "</blockquote>", "<br>", "<br/>" };
            
            foreach (var tag in blockTags)
            {
                // Case-insensitive replacement that adds newline if not already present
                var pattern = new System.Text.RegularExpressions.Regex(
                    System.Text.RegularExpressions.Regex.Escape(tag) + @"(?!\r?\n)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                html = pattern.Replace(html, tag + "\n");
            }
            
            return html.Trim();
        }

        public string GetContent()
        {
            // Return cached content - updated via contentChanged messages from JS
            // This avoids deadlock from calling async JS on UI thread
            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] WebView2SourceEditor.GetContent called, isInitialized: {_isInitialized}, content length: {_currentContent?.Length ?? 0}");
            if (!_isInitialized)
            {
                return _pendingContent ?? "";
            }
            return _currentContent;
        }

        public void SelectAll()
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                _ = _webView.CoreWebView2.ExecuteScriptAsync("selectAll()");
            }
        }

        public string SelectedText
        {
            get
            {
                // Selection isn't tracked in C#, return empty for now
                // This could deadlock if called from UI thread
                return "";
            }
        }

        public int SelectionStart
        {
            get { return 0; }  // Not tracked - would deadlock
            set
            {
                if (_isInitialized && _webView?.CoreWebView2 != null)
                {
                    _ = _webView.CoreWebView2.ExecuteScriptAsync($"setSelectionStart({value})");
                }
            }
        }

        public int SelectionLength
        {
            get { return 0; }  // Not tracked - would deadlock
            set
            {
                if (_isInitialized && _webView?.CoreWebView2 != null)
                {
                    _ = _webView.CoreWebView2.ExecuteScriptAsync($"setSelectionLength({value})");
                }
            }
        }

        public void Select(int start, int length)
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                _ = _webView.CoreWebView2.ExecuteScriptAsync($"setSelection({start}, {length})");
            }
        }

        public void ScrollToCaret()
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                _ = _webView.CoreWebView2.ExecuteScriptAsync("scrollToCursor()");
            }
        }

        public new void Focus()
        {
            if (_isInitialized && _webView?.CoreWebView2 != null)
            {
                _ = _webView.CoreWebView2.ExecuteScriptAsync("focusEditor()");
            }
            base.Focus();
        }

        private static string GetEmbeddedResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"OpenLiveWriter.WebView2Shim.Resources.CodeMirror.{name}";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] Missing embedded resource: {resourceName}");
                    return "";
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetEditorHtml()
        {
            // Load CodeMirror from embedded resources (offline support)
            var cmJs = GetEmbeddedResource("codemirror.min.js");
            var cmCss = GetEmbeddedResource("codemirror.min.css");
            var xmlJs = GetEmbeddedResource("xml.min.js");
            var jsJs = GetEmbeddedResource("javascript.min.js");
            var cssJs = GetEmbeddedResource("css.min.js");
            var htmlMixedJs = GetEmbeddedResource("htmlmixed.min.js");
            var foldCodeJs = GetEmbeddedResource("foldcode.min.js");
            var foldGutterJs = GetEmbeddedResource("foldgutter.min.js");
            var xmlFoldJs = GetEmbeddedResource("xml-fold.min.js");
            var foldGutterCss = GetEmbeddedResource("foldgutter.min.css");

            // CodeMirror 5 - syntax highlighted HTML editor with embedded resources
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>{cmCss}</style>
    <style>{foldGutterCss}</style>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        html, body {{ 
            height: 100%; 
            overflow: hidden;
            background: #fff;
        }}
        .CodeMirror {{
            height: 100%;
            font-family: 'Consolas', 'Courier New', monospace;
            font-size: 13px;
        }}
        .CodeMirror-gutters {{
            background: #f5f5f5;
            border-right: 1px solid #ddd;
        }}
        .CodeMirror-linenumber {{
            color: #999;
        }}
        .cm-tag {{ color: #170; }}
        .cm-attribute {{ color: #00c; }}
        .cm-string {{ color: #a11; }}
        .cm-comment {{ color: #a50; }}
    </style>
</head>
<body>
    <textarea id=""editor""></textarea>
    <script>{cmJs}</script>
    <script>{xmlJs}</script>
    <script>{jsJs}</script>
    <script>{cssJs}</script>
    <script>{htmlMixedJs}</script>
    <script>{foldCodeJs}</script>
    <script>{foldGutterJs}</script>
    <script>{xmlFoldJs}</script>
    <script>
        var editor = null;
        
        // Initialize CodeMirror
        editor = CodeMirror.fromTextArea(document.getElementById('editor'), {{
            mode: 'htmlmixed',
            lineNumbers: true,
            lineWrapping: true,
            foldGutter: true,
            gutters: ['CodeMirror-linenumbers', 'CodeMirror-foldgutter'],
            matchBrackets: true,
            indentUnit: 4,
            tabSize: 4,
            indentWithTabs: false
        }});

        // Notify C# when content changes
        editor.on('change', function() {{
            if (window.chrome && window.chrome.webview) {{
                window.chrome.webview.postMessage(JSON.stringify({{ 
                    type: 'contentChanged',
                    content: editor.getValue()
                }}));
            }}
        }});

        editor.on('focus', function() {{
            if (window.chrome && window.chrome.webview) {{
                window.chrome.webview.postMessage(JSON.stringify({{ type: 'focus' }}));
            }}
        }});

        editor.on('blur', function() {{
            if (window.chrome && window.chrome.webview) {{
                window.chrome.webview.postMessage(JSON.stringify({{ type: 'blur' }}));
            }}
        }});

        // API for C#
        window.setContent = function(text) {{
            editor.setValue(text || '');
            // Programmatic loads must not be undoable: without this the initial
            // setValue is the first undo step, so Ctrl+Z reverts the document
            // to empty and switching back to Edit wipes the post body.
            editor.clearHistory();
        }};

        window.getContent = function() {{
            return editor.getValue();
        }};

        window.getSelection = function() {{
            return editor.getSelection();
        }};

        window.getSelectionStart = function() {{
            var cursor = editor.getCursor('from');
            return editor.indexFromPos(cursor);
        }};

        window.getSelectionLength = function() {{
            return editor.getSelection().length;
        }};

        window.setSelection = function(start, length) {{
            var from = editor.posFromIndex(start);
            var to = editor.posFromIndex(start + length);
            editor.setSelection(from, to);
        }};

        window.selectAll = function() {{
            editor.execCommand('selectAll');
        }};

        window.scrollToCursor = function() {{
            editor.scrollIntoView(editor.getCursor());
        }};

        window.focusEditor = function() {{
            editor.focus();
        }};
    </script>
</body>
</html>";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webView?.Dispose();
            }
            base.Dispose(disposing);
        }

        private class CodeMirrorMessage
        {
            public string type { get; set; }
            public string content { get; set; }
        }
    }
}
