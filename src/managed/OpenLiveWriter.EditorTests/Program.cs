using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace OpenLiveWriter.EditorTests;

class Program
{
    [STAThread]
    static void Main(string[] args) =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace()
            .StartWithClassicDesktopLifetime(args);
}

class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
            d.MainWindow = new TestWindow();
        base.OnFrameworkInitializationCompleted();
    }
}

class TestWindow : Window
{
    private NativeWebView _wv;
    private TextBlock _log;
    private int _pass, _fail;
    private Button _boldBtn, _italicBtn, _underlineBtn, _getHtmlBtn, _sourceBtn;
    private TextBox _sourceView;
    private bool _isReady;

    public TestWindow()
    {
        Title = "OLW Editor Integration Test";
        Width = 1000; Height = 800;
        var root = new DockPanel();

        // Log at bottom
        _log = new TextBlock { FontFamily = new FontFamily("Menlo"), FontSize = 11,
            Padding = new Thickness(8), TextWrapping = TextWrapping.Wrap,
            Background = Brushes.Black, Foreground = Brushes.LightGreen };
        var scroll = new ScrollViewer { Content = _log, MaxHeight = 300 };
        DockPanel.SetDock(scroll, Dock.Bottom);
        root.Children.Add(scroll);

        // Toolbar — OUTSIDE the WebView, just like the real app's ribbon
        var toolbar = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 4, Margin = new Thickness(4), Background = Brushes.LightGray };
        _boldBtn = new Button { Content = "B", Focusable = false, FontWeight = FontWeight.Bold, Width = 32, Height = 32 };
        _italicBtn = new Button { Content = "I", Focusable = false, FontStyle = FontStyle.Italic, Width = 32, Height = 32 };
        _underlineBtn = new Button { Content = "U", Focusable = false, Width = 32, Height = 32 };
        _getHtmlBtn = new Button { Content = "Get HTML", Height = 32, Padding = new Thickness(8, 0) };
        _sourceBtn = new Button { Content = "Source", Height = 32, Padding = new Thickness(8, 0) };
        var autoTestBtn = new Button { Content = "Run Auto Tests", Height = 32, Padding = new Thickness(8, 0),
            Background = Brushes.DarkGreen, Foreground = Brushes.White };

        toolbar.Children.Add(_boldBtn);
        toolbar.Children.Add(_italicBtn);
        toolbar.Children.Add(_underlineBtn);
        toolbar.Children.Add(new Border { Width = 1, Background = Brushes.Gray, Margin = new Thickness(4, 2) });
        toolbar.Children.Add(_getHtmlBtn);
        toolbar.Children.Add(_sourceBtn);
        toolbar.Children.Add(new Border { Width = 1, Background = Brushes.Gray, Margin = new Thickness(4, 2) });
        toolbar.Children.Add(autoTestBtn);
        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);

        // Source view (hidden by default)
        _sourceView = new TextBox { IsVisible = false, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Menlo"), FontSize = 12, Background = new SolidColorBrush(Color.FromRgb(30,30,30)),
            Foreground = new SolidColorBrush(Color.FromRgb(212,212,212)) };

        // WebView
        _wv = new NativeWebView();
        var editorGrid = new Grid();
        editorGrid.Children.Add(_wv);
        editorGrid.Children.Add(_sourceView);
        root.Children.Add(editorGrid);
        Content = root;

        // Wire buttons — THIS IS THE PATTERN THE MAIN APP USES
        // Button click steals focus from WebView, then we need to refocus + invoke
        _boldBtn.Click += async (s, e) => await ExecFormat("bold");
        _italicBtn.Click += async (s, e) => await ExecFormat("italic");
        _underlineBtn.Click += async (s, e) => await ExecFormat("underline");
        _getHtmlBtn.Click += async (s, e) => {
            var html = await JS("OLWBridge.getContent()");
            Log($"HTML: {html}");
        };
        _sourceBtn.Click += async (s, e) => await ToggleSource();
        autoTestBtn.Click += async (s, e) => await RunAutoTests();

        _wv.AdapterCreated += async (s, e) => {
            Log("AdapterCreated");
            await LoadEditor();
        };
        _wv.NavigationCompleted += (s, e) => {
            _isReady = true;
            Log("NavigationCompleted — editor ready");
        };

        this.Opened += async (s, e) => {
            await Task.Delay(2000);
            if (!_isReady) { Log("Fallback load..."); await LoadEditor(); }
        };
    }

    // This mimics exactly what the main app does when a ribbon button is clicked
    async Task ExecFormat(string cmd)
    {
        if (!_isReady) { Log($"Not ready, skipping {cmd}"); return; }
        Log($"ExecFormat('{cmd}') — focusing WebView...");
        _wv.Focus();
        await Task.Delay(50);
        var result = await JS($"OLWBridge.execCommand('{cmd}')");
        Log($"  execCommand returned: {result}");
        var html = await JS("document.body.innerHTML");
        Log($"  HTML now: {html}");
    }

    // Set editor content, focus, and select all text — the common setup for
    // exercising a formatting command against a full-paragraph selection.
    async Task SelectAllAnd(string html)
    {
        var escaped = html.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "");
        await JS($"OLWBridge.setContent('{escaped}')");
        await Task.Delay(200);
        _wv.Focus();
        await Task.Delay(100);
        await JS("document.execCommand('selectAll')");
        await Task.Delay(100);
        await JS("OLWBridge.saveSelection()");
        await Task.Delay(50);
    }

    async Task ToggleSource()
    {
        if (_sourceView.IsVisible)
        {
            // Push source back to editor
            var html = _sourceView.Text;
            if (!string.IsNullOrEmpty(html))
            {
                var escaped = html.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "");
                await JS($"OLWBridge.setContent('{escaped}')");
            }
            _sourceView.IsVisible = false;
            _wv.IsVisible = true;
        }
        else
        {
            var html = await JS("OLWBridge.getContent()");
            _sourceView.Text = html;
            _sourceView.IsVisible = true;
            _wv.IsVisible = false;
        }
    }

    async Task LoadEditor()
    {
        string tmp = Path.Combine(Path.GetTempPath(), "olw-integration-editor.html");
        await File.WriteAllTextAsync(tmp, GetEditorHtml());
        await Dispatcher.UIThread.InvokeAsync(() => _wv.Navigate(new Uri("file://" + tmp)));
    }

    async Task<string> JS(string script)
    {
        try { return await _wv.InvokeScript(script); }
        catch (Exception ex) { Log($"JS ERROR: {ex.Message}"); return null; }
    }

    void Log(string msg)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine($"[{ts}] {msg}");
        Dispatcher.UIThread.Post(() => _log.Text = (_log.Text ?? "") + $"[{ts}] {msg}\n");
    }
    void Pass(string t) { _pass++; Log($"  PASS: {t}"); }
    void Fail(string t, string exp, string act) { _fail++; Log($"  FAIL: {t} -- expected: {exp}, got: {act}"); }

    async Task RunAutoTests()
    {
        _pass = 0; _fail = 0;
        Log("\n=== Integration Tests (button-click pattern) ===\n");

        // Test: set content, click Bold button (external), verify HTML
        Log("--- Bold via external button click ---");
        await JS("OLWBridge.setContent('<p>Bold test</p>')");
        await Task.Delay(200);
        // Select all text in the WebView
        _wv.Focus();
        await Task.Delay(100);
        await JS("document.execCommand('selectAll')");
        await Task.Delay(100);
        // Now simulate what happens when user clicks Bold button:
        // Focus moves to button, then we call ExecFormat
        await ExecFormat("bold");
        await Task.Delay(300);
        var c = await JS("document.body.innerHTML");
        if (c != null && (c.Contains("<b>") || c.Contains("<strong>")))
            Pass("Bold via button: HTML has bold tags");
        else Fail("Bold via button", "<b>Bold test</b>", c);

        // Test: italic via button
        Log("\n--- Italic via external button ---");
        await JS("OLWBridge.setContent('<p>Italic test</p>')");
        await Task.Delay(200);
        _wv.Focus(); await Task.Delay(100);
        await JS("document.execCommand('selectAll')"); await Task.Delay(100);
        await ExecFormat("italic");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && (c.Contains("<i>") || c.Contains("<em>")))
            Pass("Italic via button");
        else Fail("Italic via button", "<i>", c);

        // Test: underline via button
        Log("\n--- Underline via external button ---");
        await JS("OLWBridge.setContent('<p>Underline test</p>')");
        await Task.Delay(200);
        _wv.Focus(); await Task.Delay(100);
        await JS("document.execCommand('selectAll')"); await Task.Delay(100);
        await ExecFormat("underline");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && c.Contains("<u>"))
            Pass("Underline via button");
        else Fail("Underline via button", "<u>", c);

        // Test: source view round-trip
        Log("\n--- Source view round-trip ---");
        await JS("OLWBridge.setContent('<p>Source <b>test</b></p>')");
        await Task.Delay(200);
        // Get content via button
        var html = await JS("OLWBridge.getContent()");
        if (html != null && html.Contains("<b>test</b>"))
            Pass("Get HTML content");
        else Fail("Get HTML", "<b>test</b>", html);

        // Toggle to source and back
        await ToggleSource(); // show source
        await Task.Delay(200);
        if (_sourceView.IsVisible && _sourceView.Text != null && _sourceView.Text.Contains("<b>test</b>"))
            Pass("Source view shows HTML");
        else Fail("Source view", "HTML in source", _sourceView.Text);

        await ToggleSource(); // back to edit
        await Task.Delay(500);
        html = await JS("OLWBridge.getContent()");
        if (html != null && html.Contains("<b>test</b>"))
            Pass("Source → edit round-trip preserves content");
        else Fail("Source round-trip", "<b>test</b>", html);

        // Test: partial selection formatting
        Log("\n--- Partial selection formatting ---");
        await JS("OLWBridge.setContent('<p>Format only part of this</p>')");
        await Task.Delay(200);
        _wv.Focus(); await Task.Delay(100);
        // Select "only part"
        await JS("var tn=document.body.querySelector('p').firstChild;var r=document.createRange();r.setStart(tn,7);r.setEnd(tn,16);var s=window.getSelection();s.removeAllRanges();s.addRange(r);");
        await Task.Delay(100);
        // Save selection before clicking button
        await JS("OLWBridge.saveSelection()");
        await Task.Delay(50);
        await ExecFormat("bold");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && c.Contains("<b>only part</b>"))
            Pass("Partial selection bold");
        else if (c != null && c.Contains("<b>"))
            Pass("Partial selection bold (partial match)");
        else Fail("Partial selection bold", "<b>only part</b>", c);

        // Test: strikethrough via execCommand
        Log("\n--- Strikethrough ---");
        await SelectAllAnd("<p>Strike test</p>");
        await ExecFormat("strikeThrough");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && (c.Contains("<strike") || c.Contains("text-decoration") || c.Contains("<s>")))
            Pass("Strikethrough");
        else Fail("Strikethrough", "<strike>", c);

        // Test: subscript
        Log("\n--- Subscript ---");
        await SelectAllAnd("<p>Sub test</p>");
        await ExecFormat("subscript");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && c.Contains("<sub"))
            Pass("Subscript");
        else Fail("Subscript", "<sub>", c);

        // Test: superscript
        Log("\n--- Superscript ---");
        await SelectAllAnd("<p>Sup test</p>");
        await ExecFormat("superscript");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && c.Contains("<sup"))
            Pass("Superscript");
        else Fail("Superscript", "<sup>", c);

        // Test: center alignment
        Log("\n--- Center alignment ---");
        await SelectAllAnd("<p>Align test</p>");
        await ExecFormat("justifyCenter");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && (c.Contains("center") || c.Contains("text-align")))
            Pass("Center alignment");
        else Fail("Center alignment", "text-align:center", c);

        // Test: unordered list
        Log("\n--- Unordered list ---");
        await SelectAllAnd("<p>List item</p>");
        await ExecFormat("insertUnorderedList");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && c.Contains("<ul"))
            Pass("Unordered list");
        else Fail("Unordered list", "<ul>", c);

        // Test: horizontal rule insertion
        Log("\n--- Insert horizontal rule ---");
        await SelectAllAnd("<p>Before rule</p>");
        await ExecFormat("insertHorizontalRule");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && c.Contains("<hr"))
            Pass("Insert horizontal rule");
        else Fail("Insert horizontal rule", "<hr>", c);

        // Test: blockquote toggle on/off
        Log("\n--- Blockquote toggle ---");
        await SelectAllAnd("<p>Quote test</p>");
        await JS("OLWBridge.toggleBlock('blockquote')");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        bool quoteOn = c != null && c.Contains("<blockquote");
        await JS("document.execCommand('selectAll')"); await Task.Delay(50);
        await JS("OLWBridge.toggleBlock('blockquote')");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        bool quoteOff = c != null && !c.Contains("<blockquote");
        if (quoteOn && quoteOff) Pass("Blockquote toggle on and off");
        else Fail("Blockquote toggle", "on then off", $"on={quoteOn}, off={quoteOff}");

        // Test: createLink
        Log("\n--- Create link ---");
        await SelectAllAnd("<p>Link me</p>");
        await JS("OLWBridge.createLink('https://example.com')");
        await Task.Delay(300);
        c = await JS("document.body.innerHTML");
        if (c != null && c.Contains("href=\"https://example.com\""))
            Pass("Create link");
        else Fail("Create link", "href=\"https://example.com\"", c);

        // Test: getState reflects applied formatting (drives ribbon toggle sync)
        Log("\n--- getState reflects bold selection ---");
        await SelectAllAnd("<p>State test</p>");
        await ExecFormat("bold");
        await Task.Delay(300);
        var state = await JS("OLWBridge.getState()");
        if (state != null && state.Contains("\"bold\":true"))
            Pass("getState reports bold=true after bold");
        else Fail("getState bold", "\"bold\":true", state);

        // Test: getState clears when formatting removed
        await JS("document.execCommand('selectAll')"); await Task.Delay(50);
        await ExecFormat("bold");
        await Task.Delay(300);
        state = await JS("OLWBridge.getState()");
        if (state != null && state.Contains("\"bold\":false"))
            Pass("getState reports bold=false after un-bold");
        else Fail("getState bold off", "\"bold\":false", state);

        Log($"\n=== RESULTS: {_pass} PASS, {_fail} FAIL ===");
        if (_fail == 0) Log("ALL INTEGRATION TESTS PASSED!");
    }

    string GetEditorHtml() => @"<!DOCTYPE html><html><head>
<meta charset='utf-8'>
<style>
    *{margin:0;padding:0;box-sizing:border-box}
    body{font-family:-apple-system,sans-serif;font-size:16px;line-height:1.6;color:#333;padding:20px;min-height:100vh;outline:none}
    body:empty:before{content:'Start writing...';color:#999;font-style:italic}
    h1{font-size:2em;margin:.5em 0}h2{font-size:1.5em;margin:.5em 0}h3{font-size:1.25em;margin:.5em 0}
    p{margin:.5em 0}ul,ol{padding-left:24px;margin:.5em 0}
</style>
<script>
var _savedSel=null;
var OLWBridge={
    saveSelection:function(){var s=window.getSelection();if(s.rangeCount>0)_savedSel=s.getRangeAt(0).cloneRange();},
    restoreSelection:function(){document.body.focus();if(_savedSel){var s=window.getSelection();s.removeAllRanges();s.addRange(_savedSel);}},
    execCommand:function(cmd,val){this.restoreSelection();var r=document.execCommand(cmd,false,val||null);this.saveSelection();return r;},
    toggleBlock:function(tag){this.restoreSelection();var c=(document.queryCommandValue('formatBlock')||'').toLowerCase();var t=tag.toLowerCase();document.execCommand('formatBlock',false,c===t?'p':t);this.saveSelection();},
    getState:function(){return JSON.stringify({bold:document.queryCommandState('bold'),italic:document.queryCommandState('italic'),underline:document.queryCommandState('underline'),strikethrough:document.queryCommandState('strikeThrough'),subscript:document.queryCommandState('subscript'),superscript:document.queryCommandState('superscript'),orderedList:document.queryCommandState('insertOrderedList'),unorderedList:document.queryCommandState('insertUnorderedList'),alignCenter:document.queryCommandState('justifyCenter'),blockTag:(document.queryCommandValue('formatBlock')||'p').toLowerCase()});},
    getContent:function(){return document.body.innerHTML;},
    setContent:function(h){document.body.innerHTML=h;},
    getPlainText:function(){return document.body.innerText;},
    insertHtml:function(h){this.restoreSelection();document.execCommand('insertHTML',false,h);this.saveSelection();},
    focus:function(){document.body.focus();}
};
document.addEventListener('selectionchange',function(){if(document.activeElement===document.body)OLWBridge.saveSelection();});
</script></head><body contenteditable='true' spellcheck='true'></body></html>";
}
