# WebView2 Branch Report - feature/webview2

**Date:** 2026-01-18  
**Branch:** `feature/webview2`  
**Commits:** 34 commits ahead of master  
**Files Changed:** 63 files, +7,044 / -1,340 lines  

---

## Executive Summary

This branch replaces MSHTML (Internet Explorer's HTML engine) with WebView2 (Edge/Chromium) as a **prerequisite for .NET 10 migration**. MSHTML uses COM interop patterns (`EnumeratorToEnumVariantMarshaler`) that don't exist in .NET Core/.NET 10.

**Current State:** WebView2 editor is **fully functional** for core editing tasks. Running on .NET Framework 4.7.2 with x86 builds. Ready for real-world testing.

---

## Why This Work Matters for .NET 10

### The Blocker
```
MSHTML (IHTMLDocument2, etc.) → COM Interop → EnumeratorToEnumVariantMarshaler
                                                    ↓
                                        REMOVED in .NET Core!
```

Microsoft.Windows.Compatibility package does NOT include this marshaler. There is no workaround - MSHTML must be replaced.

### The Solution Path
```
.NET Framework 4.7.2 + MSHTML  (current master)
         ↓
.NET Framework 4.7.2 + WebView2  (this branch) ← WE ARE HERE
         ↓
.NET 10 + WebView2  (your branch can now proceed)
```

---

## What's Been Accomplished

### 1. WebView2 Editor (Complete ✅)

Full HTML editor using WebView2 with contenteditable divs:

| Feature | Status |
|---------|--------|
| Write/edit title & body | ✅ |
| Bold, Italic, Underline, Strikethrough | ✅ |
| Font sizes (8-36pt) | ✅ |
| Headings (H1-H6) & Paragraph | ✅ |
| Blockquote toggle | ✅ |
| Bullets & numbered lists | ✅ |
| Text alignment | ✅ |
| Insert/edit/remove hyperlinks | ✅ |
| Edit ↔ Source view switching | ✅ |
| Copy/Cut/Paste (native) | ✅ |
| Undo/Redo | ✅ |
| **Image insertion** | ✅ |
| Keyboard shortcuts (Ctrl+B/I/U/K) | ✅ |

### 2. Architecture (Simple & Clean)

We chose the **simplest possible design** - a COM bridge for state sync:

```
C# (WinForms)                          WebView2 (Edge)
┌────────────────────┐                ┌─────────────────────┐
│ EditorContentBridge│ ←── COM ────→ │ contenteditable div │
│   .Title           │    sync       │ JS event listeners  │
│   .Body            │               │                     │
│   .Selection       │               │ execCommand('bold') │
└────────────────────┘                └─────────────────────┘
```

**Key insight:** No need to fake MSHTML interfaces. The editor just needs:
- Read content (JS syncs to C# via COM bridge)
- Write content (C# calls `execCommand` via JS)

### 3. Deprecated Features

| Feature | Reason | Resolution |
|---------|--------|------------|
| Map insertion | Bing Maps API dead | Shows deprecation dialog |
| Video insertion | Flash-based embeds dead | Shows deprecation dialog with iframe instructions |

### 4. Infrastructure Improvements

- **TLS 1.2 support** - Fixed HTTPS connectivity for modern servers
- **WebView2PageDownloader** - For fetching rendered HTML
- **HTMLDocumentDownloaderFactory** - Abstraction layer for IE vs WebView2
- **HtmlEditorFactory** - Toggle between MSHTML and WebView2 editors
- **Virtual host mapping** - Serve local files in WebView2 (for images)

---

## New Files Created

### Core WebView2 Editor
```
src/managed/OpenLiveWriter.WebView2Shim/
├── WebView2HtmlEditorControl.cs      # Main editor control (1200+ lines)
├── WebView2Bridge.cs                 # C# ↔ JS bridge
├── WebView2Document.cs               # IHTMLDocument2 shim (experimental)
├── WebView2Element.cs                # IHTMLElement shim (experimental)
├── WebView2ElementCollection.cs      # Collection shim
├── WebView2Selection.cs              # Selection shim
├── WebView2Style.cs                  # Style shim
├── WebView2TextRange.cs              # TextRange shim
└── Resources/
    └── olw-dom-bridge.js             # JavaScript DOM bridge
```

### Browser Abstraction
```
src/managed/OpenLiveWriter.BrowserControl/
├── BrowserControlFactory.cs          # Factory for IE vs WebView2
└── WebView2BrowserControl.cs         # WebView2 IBrowserControl impl

src/managed/OpenLiveWriter.CoreServices/WebRequest/
├── HTMLDocumentDownloaderFactory.cs  # HTML fetcher abstraction
└── WebView2PageDownloader.cs         # WebView2 page downloader
```

### Editor Integration
```
src/managed/OpenLiveWriter.HtmlEditor/
└── HtmlEditorFactory.cs              # Toggle MSHTML vs WebView2

src/managed/OpenLiveWriter.PostEditor/PostHtmlEditing/
└── WebView2BlogPostHtmlEditorControl.cs  # Blog-specific wrapper
```

### Documentation
```
docs/
├── NET10-MIGRATION-PLAN.md           # Overall .NET 10 strategy
└── WEBVIEW2-EDITOR-MIGRATION-PLAN.md # Detailed editor plan (669 lines)
```

---

## Environment Variables

WebView2 is now the **default** engine for both the editor and browser controls. No environment variable is needed to enable it.

```powershell
# To fall back to legacy MSHTML/IE engine (for both editor and browser controls):
$env:OLW_USE_MSHTML = "1"
```

> **Note:** The old `OLW_USE_WEBVIEW2` and `OLW_USE_WEBVIEW2_EDITOR` variables are no longer used.

---

## Build & Test

```powershell
# Build (requires VS 2022 C++ tools for Ribbon)
.\build.ps1 /p:PlatformToolset=v145

# Run (WebView2 is the default engine)
.\src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe

# To fall back to MSHTML/IE:
$env:OLW_USE_MSHTML = "1"
.\src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe

# Debug logging - use DebugView, filter by [OLW-DEBUG]
```

---

## Commits (34 total)

| Commit | Description |
|--------|-------------|
| `a1193bd` | Fix image insertion with WebView2 virtual host mapping |
| `d5e1d2f` | WebView2: Implement full formatting features |
| `527c323` | WebView2: Fix startup toolbar state, Ctrl+K, selection |
| `443a73e` | WebView2: Add hyperlink dialog and InsertHtml |
| `6445969` | WebView2: Implement toolbar commands via JS |
| `afab3df` | WebView2: Implement bidirectional content sync via COM bridge |
| `880ae65` | WebView2: Implement Edit/Source view roundtrip |
| `ea02715` | Fix post loading timing |
| `ea164e9` | Fix focus border on contenteditable |
| `ae5eceb` | Fix WebView2 async initialization |
| `181517f` | Fix black background |
| `4818b47` | Fix background in editor template |
| `2108e6b` | Fix NullReferenceException in CommandSource |
| `d5bd409` | Wire WebView2 editor into ContentEditor |
| `50fef8d` | Add HtmlEditorFactory |
| `765701a` | Add WebView2HtmlEditorControl |
| `b773537` | Add comprehensive test harness |
| `4537feb` | Fix WebView2Bridge deadlock |
| `b7854f0` | Implement all MSHTML interfaces on shims |
| `12920ca` | Implement IHTMLElement and IHTMLStyle |
| `b0b229c` | Fix WebView2Shim compilation |
| `90b290b` | Add WebView2Shim project |
| `fb3372b` | Add comprehensive migration plan |
| `27f878c` | Fix timing issues |
| `a8790af` | Stub out HtmlScreenCaptureCore |
| `d3e3a51` | Update BlogEditingTemplateDetector |
| `2eee51b` | Merge master (PR #978) |
| `d968470` | Update PageToDownloadFactory |
| `07500e3` | Add HTMLDocumentDownloaderFactory |
| `1622c8f` | Add WebView2PageDownloader |
| `b88e0c6` | Deprecate Video feature |
| `6edac32` | Deprecate Map feature |
| `6231430` | Update MapControl to use factory |
| `0c9440d` | Add .NET 10 migration plan |
| `39db3c0` | Add TLS 1.2 support and factory pattern |
| `82d46df` | Add WebView2BrowserControl |

---

## Remaining Work on This Branch

### Before Merge to Master
- [ ] Real-world testing (publish posts, save drafts)
- [ ] Edge case testing (drag/drop, clipboard, unicode paths)
- [ ] Remove/reduce debug logging

### After Merge (Can Happen in Parallel)
- [ ] x64 build support
- [ ] Remove MSHTML code paths (once WebView2 proven stable)

---

## Merge Strategy Proposal

### Option A: Sequential (Safer)
1. Merge `feature/webview2` to master
2. Test thoroughly
3. Your .NET 10 branch rebases on master
4. Continue .NET 10 work with WebView2 in place

### Option B: Parallel Merge (Faster)
1. Share this branch report
2. You pull WebView2 changes into your .NET 10 branch
3. Both branches evolve, merge when ready

### Recommended: Option A
WebView2 changes are extensive (63 files, 7000+ lines). Better to get them stable on master first, then .NET 10 work continues without MSHTML blockers.

---

## Questions / Coordination Needed

1. **Which files have you modified?** - Need to check for conflicts
2. **Have you hit the MSHTML/EnumeratorToEnumVariantMarshaler issue?** - This branch solves it
3. **What's your timeline?** - Can prioritize merge accordingly
4. **Do you want to test WebView2 editor on your branch first?** - Can share instructions

---

## Contact

Let's coordinate! This branch removes the biggest blocker for .NET 10.
