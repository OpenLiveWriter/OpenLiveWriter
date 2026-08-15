# WebView2 HTML Editor Migration Plan

## Executive Summary

This document details the strategy for replacing the MSHTML-based HTML editor in Open Live Writer with WebView2. This is the **critical blocker** for .NET 10 migration, as MSHTML's COM interop (specifically `EnumeratorToEnumVariantMarshaler`) was removed in .NET Core.

**Estimated Effort:** 2-4 weeks for a senior developer  
**Risk Level:** Medium - core functionality, but paradigm is unchanged  
**Recommendation:** Use native `contenteditable` + `execCommand` (NO external libraries)

## Key Insight

**The paradigm is the same.** We're just changing the interop mechanism:

```
MSHTML:   C# → COM interop → IHTMLDocument2.execCommand("bold")
WebView2: C# → ExecuteScriptAsync → document.execCommand('bold')
```

Open Live Writer already has:
- ✅ Full ribbon UI with all formatting buttons
- ✅ Menu system
- ✅ Paste cleaning logic
- ✅ 15 years of edge case handling

We do NOT need TinyMCE, CKEditor, or any external library. Those solve problems we don't have (cross-browser compatibility, building UI from scratch).

---

## Current Architecture

### Class Hierarchy

```
MshtmlEditor (UserControl)
    ↓ used by
HtmlEditorControl (abstract)
    ↓ inherited by  
BlogPostHtmlEditorControl
    ↓ used by
BlogPostHtmlEditor
    ↓ contained in
ContentEditor / PostEditorForm
```

### Key Files

| File | Lines | Purpose |
|------|-------|---------|
| `OpenLiveWriter.Mshtml/MshtmlEditor.cs` | ~1,477 | Core MSHTML wrapper, implements `IDocHostUIHandler2`, `IHTMLEditDesignerRaw` |
| `OpenLiveWriter.HtmlEditor/HtmlEditorControl.cs` | ~4,500+ | Abstract editor with commands, selection, paste handling |
| `OpenLiveWriter.PostEditor/PostHtmlEditing/BlogPostHtmlEditorControl.cs` | ~2,700 | Blog-specific editor with spell check, plugins |

### MSHTML Interfaces Used

| Interface | Purpose | WebView2 Equivalent |
|-----------|---------|---------------------|
| `IHTMLDocument2/3/4` | DOM access | `ExecuteScriptAsync("document...")` |
| `IHTMLElement/2/3` | Element manipulation | JavaScript DOM APIs |
| `IHTMLTxtRange` | Text selection/ranges | `window.getSelection()` |
| `IMarkupServices` | Low-level markup editing | DOM Range API |
| `IHTMLEditDesignerRaw` | Event interception | `WebMessageReceived` + JS event listeners |
| `IDocHostUIHandler2` | Context menus, UI behavior | WebView2 events |
| `IHTMLChangeSink` | Dirty state tracking | MutationObserver or editor events |

### Key Editor Operations

1. **Loading/Saving HTML** - Load from file/string, get edited HTML
2. **Selection Management** - Get/set selection, find elements
3. **Formatting Commands** - Bold, italic, lists, alignment (via `execCommand`)
4. **Paste Handling** - Clean HTML, handle images, smart paste
5. **Drag & Drop** - Images, files, HTML content
6. **Undo/Redo** - Built into MSHTML, need equivalent
7. **Spell Checking** - Custom integration with squiggly underlines
8. **Element Behaviors** - Resize handles, smart content, tables
9. **Context Menus** - Custom right-click handling

---

## Migration Strategy: Native contenteditable + execCommand

### Why NOT Use TinyMCE/CKEditor

1. **OLW already has the UI** - The ribbon provides all formatting buttons, menus, dialogs
2. **We only target one browser** - WebView2/Edge. No cross-browser quirks.
3. **OLW has existing logic** - Paste cleaning, image handling, etc. already written in C#
4. **Adds complexity** - Would need to bridge TinyMCE's event model to OLW's
5. **Different behavior** - TinyMCE has its own opinions about formatting that may conflict

### The Simple Truth

The existing code does this:
```csharp
GetMshtmlCommand(IDM.BOLD).Execute();  // COM call to MSHTML
```

We just need to change it to:
```csharp
await _webView.ExecuteScriptAsync("document.execCommand('bold')");
```

### Command Mapping (IDM → execCommand)

| IDM Constant | Value | execCommand | Notes |
|--------------|-------|-------------|-------|
| `IDM.BOLD` | 52 | `bold` | Toggle bold |
| `IDM.ITALIC` | 56 | `italic` | Toggle italic |
| `IDM.UNDERLINE` | 63 | `underline` | Toggle underline |
| `IDM.STRIKETHROUGH` | 91 | `strikeThrough` | Toggle strikethrough |
| `IDM.JUSTIFYLEFT` | 59 | `justifyLeft` | Left align |
| `IDM.JUSTIFYCENTER` | 57 | `justifyCenter` | Center align |
| `IDM.JUSTIFYRIGHT` | 60 | `justifyRight` | Right align |
| `IDM.JUSTIFYFULL` | 50 | `justifyFull` | Justify |
| `IDM.INDENT` | - | `indent` | Increase indent |
| `IDM.OUTDENT` | - | `outdent` | Decrease indent |
| `IDM.FONTNAME` | 18 | `fontName` | Set font family |
| `IDM.FONTSIZE` | 19 | `fontSize` | Set font size (1-7) |
| `IDM.FORECOLOR` | 55 | `foreColor` | Text color |
| `IDM.BACKCOLOR` | 51 | `backColor` | Background color |
| `IDM.UNDO` | - | `undo` | Undo |
| `IDM.REDO` | - | `redo` | Redo |
| `IDM.COPY` | - | `copy` | Copy |
| `IDM.CUT` | - | `cut` | Cut |
| `IDM.DELETE` | 17 | `delete` | Delete selection |
| `IDM.REMOVEFORMAT` | - | `removeFormat` | Clear formatting |

### What WebView2 Provides Natively

1. **contenteditable** - Built-in editable div
2. **execCommand** - All standard formatting commands (deprecated but works)
3. **Selection API** - `window.getSelection()`, Range objects
4. **Clipboard API** - Paste event interception
5. **Spell check** - Browser's native spell checker
6. **Undo/Redo** - Browser tracks edit history automatically
7. **Context menu** - `ContextMenuRequested` event in WebView2
8. **Keyboard handling** - Standard DOM events
- Last major release was 2019
- Missing features (tables, advanced formatting)
- Less mature than TinyMCE/CKEditor

**Verdict:** ❌ Not feature-complete enough

---

## Architecture

### Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    C# WinForms Application                   │
├─────────────────────────────────────────────────────────────┤
│  WebView2HtmlEditorControl                                   │
│  ├── WebView2 Control                                       │
│  ├── Command mapping (IDM → execCommand)                    │
│  └── Event handlers                                         │
├─────────────────────────────────────────────────────────────┤
│                         WebView2                             │
├─────────────────────────────────────────────────────────────┤
│  editor.html                                                 │
│  ├── <div contenteditable="true">                           │
│  ├── olw-bridge.js (C# ↔ JS communication)                  │
│  └── CSS from blog template                                 │
└─────────────────────────────────────────────────────────────┘
```

### Minimal editor.html

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <link rel="stylesheet" href="template.css"> <!-- Blog template styles -->
    <style>
        html, body { margin: 0; padding: 0; height: 100%; }
        #editor { 
            height: 100%; 
            padding: 10px;
            outline: none;
        }
    </style>
</head>
<body>
    <div id="editor" contenteditable="true" spellcheck="true"></div>
    <script src="olw-bridge.js"></script>
</body>
</html>
```

### olw-bridge.js

```javascript
const OLW = {
    // Get/set content
    getContent: () => document.getElementById('editor').innerHTML,
    setContent: (html) => { document.getElementById('editor').innerHTML = html; },
    
    // Execute formatting commands
    exec: (cmd, value) => document.execCommand(cmd, false, value),
    
    // Query command state (for ribbon button highlighting)
    queryState: (cmd) => document.queryCommandState(cmd),
    queryValue: (cmd) => document.queryCommandValue(cmd),
    
    // Selection
    getSelectedHtml: () => {
        const sel = window.getSelection();
        if (sel.rangeCount === 0) return '';
        const range = sel.getRangeAt(0);
        const div = document.createElement('div');
        div.appendChild(range.cloneContents());
        return div.innerHTML;
    },
    
    getSelectedText: () => window.getSelection().toString(),
    
    // Insert HTML at cursor
    insertHtml: (html) => {
        document.execCommand('insertHTML', false, html);
    }
};

// Notify C# of changes
const editor = document.getElementById('editor');
editor.addEventListener('input', () => {
    window.chrome.webview.postMessage({ type: 'contentChanged' });
});

// Intercept paste for cleaning
editor.addEventListener('paste', (e) => {
    e.preventDefault();
    const html = e.clipboardData.getData('text/html') || e.clipboardData.getData('text/plain');
    // Send to C# for cleaning
    window.chrome.webview.postMessage({ type: 'paste', html: html });
});

// Handle keyboard shortcuts (optional - ribbon already handles these)
editor.addEventListener('keydown', (e) => {
    if (e.ctrlKey) {
        switch(e.key.toLowerCase()) {
            case 's': 
                e.preventDefault();
                window.chrome.webview.postMessage({ type: 'save' });
                break;
        }
    }
});
```

### C# WebView2HtmlEditorControl

```csharp
public class WebView2HtmlEditorControl : UserControl, IHtmlEditor
{
    private WebView2 _webView;
    private bool _isDirty;
    
    public async Task InitializeAsync()
    {
        await _webView.EnsureCoreWebView2Async(null);
        _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        
        // Load editor HTML from resources
        string editorPath = Path.Combine(Application.StartupPath, "Resources", "editor.html");
        _webView.CoreWebView2.Navigate($"file:///{editorPath}");
    }
    
    // Execute formatting command
    public async Task ExecuteCommand(string command, object value = null)
    {
        string valueArg = value != null ? $", {JsonConvert.SerializeObject(value)}" : "";
        await _webView.ExecuteScriptAsync($"OLW.exec('{command}'{valueArg})");
    }
    
    // Ribbon calls these
    public async Task Bold() => await ExecuteCommand("bold");
    public async Task Italic() => await ExecuteCommand("italic");
    public async Task Underline() => await ExecuteCommand("underline");
    
    // Query state for ribbon button highlighting
    public async Task<bool> IsBold()
    {
        string result = await _webView.ExecuteScriptAsync("OLW.queryState('bold')");
        return result == "true";
    }
    
    // Get edited HTML
    public async Task<string> GetEditedHtml()
    {
        string json = await _webView.ExecuteScriptAsync("OLW.getContent()");
        return JsonUnquote(json);  // Remove JSON string encoding
    }
    
    // Set HTML content
    public async Task SetHtml(string html)
    {
        string escaped = JsonConvert.SerializeObject(html);
        await _webView.ExecuteScriptAsync($"OLW.setContent({escaped})");
    }
    
    // Handle messages from JavaScript
    private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var message = JsonConvert.DeserializeObject<dynamic>(e.WebMessageAsJson);
        switch ((string)message.type)
        {
            case "contentChanged":
                _isDirty = true;
                IsDirtyEvent?.Invoke(this, EventArgs.Empty);
                break;
            case "paste":
                // Clean HTML using existing OLW paste cleaning logic
                string cleanHtml = PasteHandler.CleanHtml((string)message.html);
                _ = ExecuteCommand("insertHTML", cleanHtml);
                break;
            case "save":
                SaveRequested?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}
```

```html
<!DOCTYPE html>
<html>
<head>
    <script src="tinymce/tinymce.min.js"></script>
    <script src="olw-bridge.js"></script>
    <style>
        html, body { margin: 0; padding: 0; height: 100%; }
        #editor { height: 100%; }
    </style>
</head>
<body>
    <div id="editor"></div>
    <script>
        tinymce.init({
            selector: '#editor',
            plugins: 'lists link image table paste',
            toolbar: false, // OLW uses ribbon
            menubar: false,
            statusbar: false,
            paste_preprocess: function(plugin, args) {
                // Call C# for paste processing
                args.content = window.chrome.webview.hostObjects.sync.olwBridge.ProcessPaste(args.content);
            },
            setup: function(editor) {
                editor.on('Change', function() {
                    window.chrome.webview.hostObjects.olwBridge.NotifyContentChanged();
                });
            }
        });
    </script>
</body>
</html>
```

#### 2. `olw-bridge.js`
Helper functions for C#↔JS communication.

```javascript
const OLW = {
    // Called from C# via ExecuteScriptAsync
    getContent: () => tinymce.activeEditor.getContent(),
    setContent: (html) => tinymce.activeEditor.setContent(html),
    insertContent: (html) => tinymce.activeEditor.insertContent(html),
    
    execCommand: (cmd, value) => tinymce.activeEditor.execCommand(cmd, false, value),
    
    getSelection: () => tinymce.activeEditor.selection.getContent(),
    
    // More complex operations
    insertLink: (url, text, title, rel, newWindow) => {
        const attrs = { href: url, title: title };
        if (rel) attrs.rel = rel;
        if (newWindow) attrs.target = '_blank';
        tinymce.activeEditor.execCommand('mceInsertLink', false, attrs);
    },
    
    isDirty: () => tinymce.activeEditor.isDirty(),
    clearDirty: () => tinymce.activeEditor.setDirty(false)
};
```

---

## Migration Phases

### Phase 1: Prototype (1-2 weeks)
**Goal:** Prove the concept works

1. Create standalone `WebView2HtmlEditorControl` prototype
2. Embed TinyMCE and verify basic editing works
3. Implement core `IHtmlEditor` interface methods
4. Test bidirectional C#↔JS communication
5. Verify HTML round-trips correctly (load → edit → save)

**Deliverable:** Working prototype that can edit HTML

### Phase 2: Feature Parity - Core Editing (1-2 weeks)
**Goal:** Match essential editor functionality

1. Implement all `IHtmlEditorCommandSource` commands:
   - Formatting (bold, italic, underline, strikethrough)
   - Alignment (left, center, right, justify)
   - Lists (bullet, numbered)
   - Indentation
   - Undo/Redo
   
2. Implement selection management:
   - Get/set selection
   - Select all
   - Find element at point
   
3. Implement clipboard operations:
   - Copy, Cut, Paste
   - Paste special handling (clean HTML from Word, etc.)
   
4. Verify spell checking works (TinyMCE has browser spell check support)

**Deliverable:** Editor with all basic commands working

### Phase 3: Feature Parity - Advanced (2-3 weeks)
**Goal:** Match OLW-specific features

1. **Smart Content / Element Behaviors**
   - Image resize handles
   - Table editing
   - Plugin content (embeds)
   
2. **Drag and Drop**
   - Images from file system
   - HTML content
   - Files
   
3. **Context Menus**
   - Custom right-click menus
   - Element-specific options
   
4. **Link Handling**
   - Link insertion dialog integration
   - Link glossary
   
5. **Template/Theme Support**
   - Load blog template CSS
   - Match blog appearance

**Deliverable:** Full feature parity with existing editor

### Phase 4: Integration & Testing (1-2 weeks)
**Goal:** Replace MshtmlEditor in the application

1. Create `HtmlEditorFactory` to choose implementation
2. Update `HtmlEditorControl` to work with `WebView2HtmlEditorControl`
3. WebView2 is the default; `OLW_USE_MSHTML=1` env var opts out to legacy IE engine
4. Extensive testing:
   - New post creation
   - Edit existing post
   - Copy/paste from various sources
   - Image handling
   - Table editing
   - Multiple blogs
   
5. Performance testing and optimization

**Deliverable:** Working application with WebView2 editor

### Phase 5: Cleanup & Polish (1 week)
**Goal:** Production ready

1. Remove MSHTML fallback code (optional, or keep for comparison)
2. Fix edge cases and bugs found in testing
3. Update documentation
4. Performance optimization

**Deliverable:** Ready for .NET 10 migration

---

## Technical Challenges & Solutions

### Challenge 1: Async Nature of WebView2
**Problem:** MSHTML was synchronous, WebView2 is async.

**Solution:**
- Use `async/await` throughout
- For properties that must be sync, cache values and update via events
- Consider `TaskCompletionSource` for request-response patterns

### Challenge 2: Element Behaviors (Resize Handles, etc.)
**Problem:** MSHTML had native element behaviors for resize handles.

**Solution:**
- TinyMCE has built-in image resize
- For custom elements, use TinyMCE's `noneditable_noneditable_class` and custom UI
- May need custom JavaScript for OLW-specific smart content

### Challenge 3: Spell Checking Integration
**Problem:** OLW has custom spell check integration.

**Solution:**
- Use browser's native spell check (works in WebView2/Edge)
- Or integrate TinyMCE's spell check plugin
- Custom dictionary support may need additional work

### Challenge 4: Template Loading
**Problem:** Blog templates are loaded as local HTML files.

**Solution:**
- TinyMCE's `content_css` option loads external CSS
- Can inject template HTML as editor wrapper
- May need `file://` protocol handling in WebView2

### Challenge 5: Paste Handling
**Problem:** OLW has sophisticated paste cleaning (Word, web content, etc.)

**Solution:**
- TinyMCE has `paste_preprocess` hook
- Call C# code via bridge for complex cleaning
- Reuse existing paste cleaning logic

---

## Files to Create

```
src/managed/
├── OpenLiveWriter.WebView2Editor/           # New project
│   ├── WebView2HtmlEditorControl.cs         # Main editor control
│   ├── EditorBridge.cs                      # C#↔JS bridge
│   ├── WebView2EditorCommands.cs            # Command implementations
│   ├── WebView2EditorSelection.cs           # Selection management
│   └── Resources/
│       ├── editor.html                      # Editor HTML page
│       ├── olw-bridge.js                    # JS bridge code
│       └── tinymce/                         # TinyMCE files
│           ├── tinymce.min.js
│           └── themes/skins/plugins/
│
├── OpenLiveWriter.HtmlEditor/
│   └── HtmlEditorFactory.cs                 # Factory for editor selection
```

## Files to Modify

1. `OpenLiveWriter.HtmlEditor/HtmlEditorControl.cs`
   - Make more methods virtual for override
   - Add factory pattern integration
   
2. `OpenLiveWriter.PostEditor/PostHtmlEditing/BlogPostHtmlEditorControl.cs`
   - Create WebView2 version or make abstract

3. Various files using `MshtmlEditor` directly
   - Audit and update to use factory/interface

---

## Dependencies to Add

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Web.WebView2` | Latest | WebView2 runtime |
| TinyMCE | 7.x | Editor (embedded, not NuGet) |

TinyMCE is self-hosted (download and include in Resources), not loaded from CDN.

---

## Testing Strategy

### Unit Tests
- Editor bridge message passing
- HTML content round-trip
- Command execution

### Integration Tests
- Create new post
- Edit existing post
- Paste from clipboard
- Drag and drop
- Multiple editor instances

### Manual Testing
- All ribbon commands
- Context menus
- Spell checking
- Template/theme loading
- Different blog providers

---

## Rollback Plan

Keep `MshtmlEditor` code intact and use factory pattern:

```csharp
public static IHtmlEditor CreateEditor(...)
{
    if (UseWebView2Editor)
        return new WebView2HtmlEditorControl(...);
    else
        return new MshtmlHtmlEditorControl(...); // Existing code
}
```

Can toggle via environment variable or config during testing.

---

## Success Criteria

1. ✅ All existing editor features work
2. ✅ HTML content saves correctly (no corruption)
3. ✅ Performance is acceptable (< 2 second load time)
4. ✅ Copy/paste from various sources works
5. ✅ Images can be inserted and resized
6. ✅ Tables work
7. ✅ Spell check works
8. ✅ No regressions in existing functionality

---

## Open Questions

1. **Spell Check Dictionary:** Do we need custom dictionary support? How to integrate?

2. **Smart Content:** How deep is the smart content integration? Need to analyze plugins.

3. **Performance:** Is TinyMCE fast enough for large posts? Need benchmarking.

4. **Offline:** TinyMCE must work fully offline (self-hosted). Verify no CDN calls.

5. **Accessibility:** Does TinyMCE meet accessibility requirements?

---

## References

- [TinyMCE Documentation](https://www.tiny.cloud/docs/)
- [WebView2 Documentation](https://learn.microsoft.com/en-us/microsoft-edge/webview2/)
- [WebView2 Host Objects](https://learn.microsoft.com/en-us/microsoft-edge/webview2/how-to/hostobject)
- [TinyMCE GitHub](https://github.com/tinymce/tinymce) - MIT License
- [Meziantou's WebView2 Bridge](https://www.meziantou.net/sharing-object-between-dotnet-host-and-webview2.htm)

---

## Appendix: MSHTML Removal Checklist

After WebView2 editor is complete, these files can be cleaned up:

### Can Remove
- [ ] `OpenLiveWriter.Mshtml/MshtmlEditor.cs`
- [ ] `OpenLiveWriter.Mshtml/MshtmlControl.cs`
- [ ] `OpenLiveWriter.Mshtml/Mshtml_Interop/*`
- [ ] Most of `OpenLiveWriter.Interop/Com/Mshtml*.cs`

### Must Keep (used elsewhere)
- [ ] Audit all `using mshtml;` statements
- [ ] Some HTML parsing may still use MSHTML types
- [ ] `LightWeightHTMLDocument` already has WebView2-compatible methods

### Final Step
Once all MSHTML references removed:
- Convert to SDK-style projects
- Retarget to .NET 10
- Remove COM references
- Update build scripts
