# MSHTML Dead Code Analysis (WebView2 Migration Readiness)

**Date:** 2026-01-18  
**Status:** WebView2 NOT YET IMPLEMENTED  
**Purpose:** Identify MSHTML code paths that would become dead code when WebView2 is enabled

## Executive Summary

Open Live Writer currently uses **MSHTML (Internet Explorer's Trident engine)** for all HTML rendering and editing. WebView2 (Microsoft Edge's Chromium-based engine) is **not yet implemented** in this codebase.

This document identifies MSHTML-dependent code that would need to be replaced, abstracted, or removed during a future WebView2 migration.

### Key Findings

- **~60,000 lines** of MSHTML interop code
- **~1,500+ usages** of MSHTML APIs across the codebase (based on grep analysis)
- **No abstraction layer** currently exists for browser engine switching
- **High migration effort** estimated at several months of work

---

## MSHTML Architecture Overview

### Core Components

1. **OpenLiveWriter.Interop.Mshtml** (723 files, ~59,000 lines)
   - COM interop definitions for MSHTML (IHTMLDocument2, IHTMLElement, etc.)
   - Generated from MSHTML type library
   - **Status:** Would be entirely dead code with WebView2

2. **OpenLiveWriter.Mshtml** (49 files, ~8,000 lines)
   - High-level wrappers around MSHTML interop
   - Markup services, editing features, event handling
   - **Status:** Would need complete rewrite for WebView2

3. **OpenLiveWriter.BrowserControl** (5 files)
   - `ExplorerBrowserControl` - Wraps IE WebBrowser control
   - Browser command abstraction
   - **Status:** Would need parallel WebView2Control implementation

4. **OpenLiveWriter.HtmlEditor** (~50 files)
   - HTML editing control built on MSHTML
   - **Status:** Would need significant refactoring

---

## Dead Code Categories (When WebView2 Enabled)

### Category 1: Complete Removals (LOW EFFORT TO IDENTIFY)

These would be entirely unused with WebView2:

#### 1.1 MSHTML Interop Layer
**Location:** `src/managed/OpenLiveWriter.Interop.Mshtml/`  
**File Count:** 723 files  
**Line Count:** ~59,000 lines  

**Key interfaces that would be dead:**
- `IHTMLDocument2`, `IHTMLDocument3`, `IHTMLDocument4`, `IHTMLDocument5`
- `IHTMLElement`, `IHTMLElement2`, `IHTMLElement3`, `IHTMLElement4`
- `IMarkupServices`, `IMarkupPointer`, `IMarkupContainer`
- `IDisplayServices`, `IHTMLEditDesigner`, `IHTMLEditHost`
- 700+ other MSHTML COM interfaces

**Migration Path:** Delete entire directory, replace with WebView2 interop

#### 1.2 MSHTML Wrapper Classes
**Location:** `src/managed/OpenLiveWriter.Mshtml/`  
**File Count:** 49 files  
**Key classes:**

| Class | Purpose | WebView2 Equivalent |
|-------|---------|---------------------|
| `MshtmlEditor` | Core MSHTML editing control | Rewrite with WebView2 + contenteditable |
| `MshtmlControl` | MSHTML host control | WebView2 control |
| `MshtmlMarkupServices` | Markup manipulation | JavaScript-based manipulation |
| `MarkupPointer` | Text position tracking | Range/Selection API via JavaScript |
| `MarkupRange` | Text range operations | Range API via JavaScript |
| `MshtmlCommands` | Editing commands | execCommand or custom JS |
| `MshtmlDocumentEvents` | Document event handling | WebView2 event model |

**Migration Path:** Complete rewrite with WebView2 equivalents

#### 1.3 ExplorerBrowserControl
**Location:** `src/managed/OpenLiveWriter.BrowserControl/ExplorerBrowserControl.cs`  
**Line Count:** ~1,500 lines  

**Key dependencies:**
- `AxWebBrowser` - ActiveX wrapper for IE control
- IE-specific navigation and command handling
- MSHTML document access

**Migration Path:** 
- Create `WebView2BrowserControl` parallel implementation
- Implement `IBrowserControl` interface
- Abstract away browser-specific code

---

### Category 2: Conditional/Abstraction Needed (MEDIUM EFFORT)

These have both MSHTML-specific and potentially reusable logic:

#### 2.1 HTML Editor Components
**Location:** `src/managed/OpenLiveWriter.HtmlEditor/`

**Files requiring abstraction:**

| File | MSHTML Usage | Abstraction Strategy |
|------|--------------|---------------------|
| `HtmlEditorControl.cs` | Heavy MSHTML document manipulation | Create `IHtmlDocument` interface |
| `HtmlEditorElementBehavior.cs` | MSHTML element behaviors | JavaScript-based behaviors |
| `HtmlEditorMarshallingHandler.cs` | MSHTML-specific marshalling | Generic HTML serialization |
| `HtmlStyleHelper.cs` | MSHTML style APIs | CSS manipulation via JavaScript |

**Estimated Affected Lines:** ~5,000 lines

#### 2.2 Post Editor Integration
**Location:** `src/managed/OpenLiveWriter.PostEditor/`

**Key integration points:**

| Component | MSHTML Dependency | Migration Complexity |
|-----------|-------------------|---------------------|
| `BlogPostHtmlEditorControl.cs` | Extends `HtmlEditorControl` (MSHTML-based) | HIGH - Core editing |
| `ContentEditor.cs` | Direct MSHTML document access | HIGH - Content management |
| `ContentEditorProxy.cs` | `IHTMLDocument2` interfaces | MEDIUM - Interface abstraction |
| `HtmlDocument2Wrapper.cs` | Wraps `IHTMLDocument2` for clipboard | MEDIUM - Clipboard integration |

**Key methods requiring abstraction:**
```csharp
IHTMLDocument2 GetPublishDocument()  // Used in ~20 locations
IHTMLElement PostBodyElement(IHTMLDocument2 doc)  // Element access
```

**Estimated Affected Lines:** ~8,000 lines

#### 2.3 Content Source Plugins
**Location:** Various

**Smart content integration points:**
- `SmartContentInsertionHelper.cs` - Uses `IHTMLDocument2 doc = sc.GetDocument()`
- Plugin sidebar integration - MSHTML document access
- Content source HTML injection

**Estimated Affected Lines:** ~2,000 lines

#### 2.4 Web Operations
**Location:** `src/managed/OpenLiveWriter.CoreServices/`

**Components:**

| Component | MSHTML Usage | Purpose |
|-----------|--------------|---------|
| `WebPageDownloader.cs` | `ExplorerBrowserControl` | Download pages with JS execution |
| `HtmlScreenCaptureCore.cs` | `ExplorerBrowserControl` | Capture rendered HTML |
| `BrowserOperationInvoker.cs` | `ExplorerBrowserControl` | Execute browser operations |

**Migration Path:** WebView2 equivalent operations

---

### Category 3: Indirect Dependencies (LOW-MEDIUM EFFORT)

Code that doesn't directly use MSHTML but depends on MSHTML-based components:

#### 3.1 Spell Checking
**Location:** `src/managed/OpenLiveWriter.SpellChecker/`

- `MshtmlWordRange.cs` - Uses MSHTML markup services for word boundary detection
- `SpellingManager.cs` - Integrates with MSHTML editor

**Migration:** Use JavaScript-based word boundary detection or browser spell-check API

#### 3.2 Image Editing
**Location:** `src/managed/OpenLiveWriter.PostEditor/PostHtmlEditing/ImageEditing/`

- Decorators interact with MSHTML element behaviors
- Resize editors manipulate MSHTML elements

**Migration:** JavaScript-based image manipulation overlays

#### 3.3 Table Editing
**Location:** `src/managed/OpenLiveWriter.PostEditor/Tables/`

- `TableHelper.cs` - MSHTML element queries
- Table editors use MSHTML selection

**Migration:** JavaScript-based table manipulation

---

## Migration Strategy Recommendations

### Phase 1: Create Abstraction Layer (3-4 months)

**Goal:** Allow MSHTML and WebView2 to coexist

1. **Define Browser Abstraction Interfaces**
   ```csharp
   public interface IBrowserDocument
   {
       string GetHtml();
       void SetHtml(string html);
       IBrowserElement GetElementById(string id);
       // ... more methods
   }
   
   public interface IBrowserElement
   {
       string InnerHtml { get; set; }
       string OuterHtml { get; }
       IBrowserStyle Style { get; }
       // ... more properties
   }
   ```

2. **Implement MSHTML Adapter**
   - Wrap existing MSHTML code to implement new interfaces
   - No functionality changes, just indirection
   - Estimated effort: 6-8 weeks

3. **Refactor Consumers**
   - Update ~300 call sites to use abstractions
   - Remove direct MSHTML interop references
   - Estimated effort: 4-6 weeks

### Phase 2: Implement WebView2 Adapter (4-6 months)

**Goal:** Functional parity with MSHTML implementation

1. **Core WebView2 Integration**
   - Implement `IBrowserDocument` using WebView2 + JavaScript
   - Handle async JavaScript execution
   - Estimated effort: 8-10 weeks

2. **Editing Functionality**
   - Implement contenteditable-based editing
   - Command handling (bold, italic, lists, etc.)
   - Estimated effort: 8-12 weeks

3. **Advanced Features**
   - Spell checking integration
   - Custom behaviors (image resize, table editing)
   - Clipboard integration
   - Estimated effort: 6-8 weeks

### Phase 3: Feature Flag & Testing (2-3 months)

**Goal:** Ship WebView2 option to users

1. **Feature Toggle**
   ```csharp
   public enum BrowserEngine
   {
       MSHTML,    // Legacy
       WebView2   // Modern
   }
   ```

2. **Comprehensive Testing**
   - All editing scenarios
   - All plugin scenarios
   - Content import/export
   - Estimated effort: 6-8 weeks

3. **Performance Optimization**
   - JavaScript execution optimization
   - Memory usage improvements
   - Estimated effort: 2-4 weeks

### Phase 4: Deprecate MSHTML (1-2 releases later)

**Goal:** Remove MSHTML code entirely

1. **Default to WebView2**
2. **Deprecation warnings** for MSHTML mode
3. **Final removal** of all MSHTML code (~68,000 lines deleted)

---

## Code Volume Analysis

### Would Become Dead Code (Total: ~68,000 lines)

| Component | Files | Lines | % of Codebase |
|-----------|-------|-------|---------------|
| OpenLiveWriter.Interop.Mshtml | 723 | ~59,000 | 40% |
| OpenLiveWriter.Mshtml | 49 | ~8,000 | 5% |
| ExplorerBrowserControl | 1 | ~1,500 | 1% |
| **TOTAL DEAD CODE** | **773** | **~68,500** | **46%** |

### Requires Significant Refactoring (~15,000 lines)

| Component | Files | Lines | Effort |
|-----------|-------|-------|--------|
| HtmlEditor | ~50 | ~5,000 | High |
| PostEditor MSHTML integration | ~30 | ~8,000 | High |
| CoreServices web operations | ~10 | ~2,000 | Medium |
| **TOTAL REFACTORING** | **~90** | **~15,000** | **High** |

### Minor Updates (~5,000 lines)

- Spell checker integration
- Image editing behaviors  
- Table editing
- Plugin APIs

---

## Risk Assessment

### High Risk Areas

1. **Editing Fidelity**
   - MSHTML has extensive editing features
   - WebView2 contenteditable may behave differently
   - Risk: User complaints about editing experience

2. **Plugin Compatibility**
   - Plugins may directly use MSHTML APIs
   - Risk: Breaking existing plugins

3. **Content Compatibility**
   - Different HTML parsing/serialization
   - Risk: Content corruption or formatting loss

4. **Performance**
   - WebView2 is heavier than MSHTML control
   - Risk: Slower startup, higher memory usage

### Medium Risk Areas

1. **Clipboard Integration**
2. **Spell Checking**
3. **Undo/Redo**
4. **Accessibility**

---

## Current State Summary

### What Exists Today (2026-01-18)

✅ **MSHTML-based architecture** (100% of HTML rendering)  
✅ **Mature, working implementation** (years of refinement)  
✅ **Full feature set** (editing, plugins, content sources)  
❌ **NO WebView2 implementation**  
❌ **NO abstraction layer**  
❌ **NO migration plan in code**

### When WebView2 Is Enabled

The following would become dead code:
- ❌ **773 files** (~68,500 lines) - Complete removal
- ⚠️ **~90 files** (~15,000 lines) - Significant refactoring  
- ⚠️ **~50 files** (~5,000 lines) - Minor updates

**Total Impact:** ~913 files affected, ~88,500 lines (nearly 60% of managed codebase)

---

## Recommendations

### Immediate Actions

1. ✅ **Document current MSHTML architecture** (this document)
2. 📋 **Create GitHub epic** for WebView2 migration tracking
3. 📋 **Prototype WebView2 integration** - Spike to validate feasibility

### Short Term (3-6 months)

4. 📋 **Design abstraction layer** - IBrowserDocument, IBrowserElement interfaces
5. 📋 **Implement MSHTML adapter** - Wrap existing code
6. 📋 **Begin consumer refactoring** - Update call sites

### Long Term (12-18 months)

7. 📋 **Implement WebView2 adapter** - Full feature parity
8. 📋 **Feature flag integration** - Allow user choice
9. 📋 **Testing & refinement** - Ensure quality

### Future (24+ months)

10. 📋 **Default to WebView2** - Make it the default
11. 📋 **Deprecate MSHTML** - Warn users
12. 📋 **Remove MSHTML code** - Delete ~68,500 lines

---

## Conclusion

**WebView2 migration would be a major undertaking** affecting nearly 60% of the codebase. Without an abstraction layer, all MSHTML code is tightly coupled to IE's rendering engine.

**Estimated Total Effort:** 12-18 months of focused development  
**Estimated Risk:** HIGH (core functionality, user experience impact)  
**Estimated Benefit:** Modern browser engine, better security, future-proof

**Current Status:** No work has begun. This analysis provides the roadmap for when the project decides to pursue WebView2 migration.
