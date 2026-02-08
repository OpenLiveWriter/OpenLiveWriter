# Open Live Writer - .NET 10 Migration Plan

## Overview
Upgrading Open Live Writer from .NET Framework 4.6.1 to .NET 10, with WebView2 replacing MSHTML (IE) as the HTML rendering/editing engine.

## Current State
- **Branch:** `feature/webview2` - https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2
- **Target Framework:** .NET Framework 4.7.2 (upgraded from 4.6.1 for WebView2 compatibility)
- **Build Command:** `.\build.ps1 /p:PlatformToolset=v145`

## The Problem
MSHTML (Internet Explorer's HTML engine) is deeply embedded throughout the codebase (~200+ files) and uses COM interop that is **not compatible with .NET Core/.NET 10**:
- `EnumeratorToEnumVariantMarshaler` was removed in .NET Core
- `IHTMLDocument2/3/4` and related interfaces require this marshaler
- Microsoft.Windows.Compatibility package does NOT include this marshaler

## Strategy
**Phase 1: WebView2 on .NET Framework (Current)**
- Replace MSHTML with WebView2 while staying on .NET Framework
- Can test incrementally, lower risk
- WebView2 works on both .NET Framework 4.6.2+ AND .NET 10

**Phase 2: .NET 10 Migration (Future)**
- Once MSHTML is removed, convert projects to SDK-style
- Retarget to .NET 10
- Update NuGet packages

---

## Phase 1 Progress: WebView2 Integration

### Completed ✅
1. **WebView2 NuGet Package Added** - `Microsoft.Web.WebView2.1.0.2903.40`
2. **WebView2BrowserControl.cs** - Implements `IBrowserControl` interface
3. **BrowserControlFactory.cs** - WebView2 is default; opt out via `OLW_USE_MSHTML=1` env var
4. **BrowserMiniForm.cs** - Updated to use factory pattern
5. **MapControl.cs** - Updated to use factory pattern
6. **TLS 1.2 Support** - Fixed HTTPS connectivity for modern servers
7. **Debug Logging** - `[OLW-DEBUG]` prefix for DebugView filtering

### Decisions Made 📝
- **Maps Feature**: Bing Maps API is dead. Options: Remove feature, replace with OpenStreetMap/Leaflet, or Google Maps. Recommendation: Remove for now to simplify migration.

### TODO 📋

#### Phase 1a: Simple Browser Uses
- [x] `OpenLiveWriter.CoreServices/HtmlScreenCaptureCore.cs` - STUBBED (returns null)
- [ ] `OpenLiveWriter.CoreServices/HTML/WebPageDownloader.cs` (uses old Project31 namespace)
- [ ] `OpenLiveWriter.CoreServices/BrowserOperationInvoker.cs` - Used by BackgroundColorDetector
- [x] `OpenLiveWriter.CoreServices/WebRequest/WebPageDownloader.cs` - REPLACED by WebView2PageDownloader
- [x] `OpenLiveWriter.InternalWriterPlugin/MapControl.cs` - Uses factory, but Bing Maps API is dead
- [x] `OpenLiveWriter.BlogClient/Detection/BackgroundColorDetector.cs` - STUBBED (returns default color)

#### Phase 1b: Feature Cleanup
- [x] Remove or replace dead Bing Maps feature - STUBBED with deprecation message
- [x] Video embed feature - STUBBED with deprecation message
- [ ] Audit other features using deprecated APIs

#### Phase 1c: DOM Interop Layer
- [ ] Create `WebView2DomHelper.cs` with common DOM operations
- [ ] Map IHTMLElement operations to JavaScript equivalents

#### Phase 1d: HTML Editor (Major Work)
- [ ] Analyze `OpenLiveWriter.Mshtml/MshtmlEditor.cs` (106+ IHTMLDocument refs)
- [ ] Design WebView2-based editing with contenteditable
- [ ] Handle paste, drag-drop, undo/redo, spell checking

---

## Build & Test

```powershell
# Build
.\build.ps1 /p:PlatformToolset=v145

# Run with IE (default)
.\src\managed\bin\Debug\i386\Writer\OpenLiveWriter.exe

# WebView2 is the default engine; to fall back to MSHTML/IE:
$env:OLW_USE_MSHTML = "1"
.\src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe
```

---

## Key Files
| File | Purpose |
|------|---------|
| `OpenLiveWriter.BrowserControl/IBrowserControl.cs` | Browser abstraction interface |
| `OpenLiveWriter.BrowserControl/WebView2BrowserControl.cs` | WebView2 implementation |
| `OpenLiveWriter.BrowserControl/BrowserControlFactory.cs` | Factory for switching |
| `OpenLiveWriter.Mshtml/MshtmlEditor.cs` | Main HTML editing engine (needs rewrite) |
| `writer.build.settings` | Build settings |

---

## Known Issues
- **dasBlog XML-RPC**: Server returns NullReferenceException on `blogger.getUsersBlogs` - this is a dasBlog Core bug, not OLW
- **Bing Maps**: Virtual Earth API is deprecated/dead - map feature non-functional
