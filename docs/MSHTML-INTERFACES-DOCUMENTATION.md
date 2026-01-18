# MSHTML Interfaces Documentation

## Overview

Open Live Writer historically used Microsoft's MSHTML (Trident) rendering engine - the same engine that powered Internet Explorer - for its rich HTML editor. This required implementing COM interop with numerous undocumented or poorly documented MSHTML interfaces to provide advanced editing capabilities beyond the standard `IHTMLDocument2` interface.

This document catalogs all MSHTML interfaces that were shimmed/implemented in the codebase, explains their purpose, and provides context for the WebView2 migration strategy.

**Note:** Active WebView2 migration work is happening in the [`feature/webview2` branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2). See that branch for:
- `docs/WEBVIEW2-BRANCH-REPORT.md` - Current implementation status
- `docs/WEBVIEW2-EDITOR-MIGRATION-PLAN.md` - Detailed migration plan
- Working WebView2 editor implementation

## Why Interface Shimming Was Necessary

The standard MSHTML COM interfaces exposed through the Primary Interop Assemblies (PIAs) were insufficient for building a professional HTML editor because:

1. **Limited DOM Manipulation**: Standard interfaces didn't provide precise control over DOM tree manipulation
2. **No Advanced Selection Control**: Basic selection APIs couldn't handle complex multi-range selections or programmatic selection manipulation
3. **Missing Rendering Services**: Visual feedback features (spell-check highlighting, caret positioning) required undocumented rendering interfaces
4. **Editing Behavior Customization**: Default editing behaviors needed to be overridden for a blog-focused editing experience
5. **Performance**: Direct markup pointer manipulation was much faster than text range operations

All shimmed interfaces are located in: `/src/managed/OpenLiveWriter.Mshtml/Mshtml_Interop/`

---

## Core Markup Manipulation Interfaces

### IMarkupServicesRaw
**GUID:** `3050f4a0-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Primary interface for advanced DOM manipulation, providing low-level control over HTML structure.

**Key Capabilities:**
- Create and manage markup pointers for precise positioning in the DOM tree
- Insert, remove, clone, and move elements
- Parse HTML strings into DOM structures
- Convert between markup pointers and text ranges
- Query element metadata (tag IDs, scoping information)

**Why It Was Needed:**
- Standard IHTMLDocument methods were too high-level and slow for complex editing operations
- Needed atomic operations on the DOM tree for undo/redo functionality
- Required precise positioning for features like table cell editing and list manipulation

**Usage in OLW:**
- `MshtmlMarkupServices.cs` - Wrapper class that exposes this functionality
- Used throughout spell-checking, content filtering, and editor commands

---

### IMarkupPointerRaw
**GUID:** `3050f49f-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Represents a position in the HTML markup tree, similar to a cursor but more powerful than IHTMLTextRange.

**Key Capabilities:**
- Navigate left/right through markup with awareness of elements vs. text
- Position relative to elements (before/after start/end tags)
- Compare positions (left of, right of, equal to)
- Move by logical units (characters, words, sentences)
- Find text within ranges
- Set gravity (whether pointer moves with insertions) and cling (whether pointer stays inside elements)

**Why It Was Needed:**
- Text ranges had poor performance and inconsistent behavior
- Needed stable references to DOM positions that survived DOM modifications
- Required fine-grained control for spell-checking word boundaries

**Usage in OLW:**
- `MarkupPointer.cs` - Managed wrapper
- `MarkupRange.cs` - Represents a range between two pointers
- Core primitive for all editing operations

---

### IMarkupContainerRaw
**GUID:** `3050f5f9-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Represents a container for markup (typically a document or document fragment).

**Key Capabilities:**
- Query document association
- Enumerate contained elements
- Create pointers within the container

**Why It Was Needed:**
- Required for working with detached DOM fragments
- Used in clipboard operations and content parsing
- Needed for isolating markup operations to specific subtrees

**Usage in OLW:**
- `MarkupContainer.cs` - Managed wrapper
- Used in content filtering and HTML sanitization

---

## Selection and Caret Interfaces

### ISelectionServicesRaw
**GUID:** `3050f684-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Provides programmatic control over user selection, including multi-range selections.

**Key Capabilities:**
- Set selection type (caret, text selection, control selection)
- Add/remove selection segments (supports discontinuous selections)
- Add element segments for control selections
- Query current selection container

**Why It Was Needed:**
- Default selection APIs couldn't handle table cell selections
- Needed to programmatically create selections for find/replace
- Required for implementing custom selection rendering

**Usage in OLW:**
- Used in table editing for multi-cell selection
- Find/replace functionality
- Custom selection highlighting

---

### IHTMLCaretRaw
**GUID:** `3050f604-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Controls the text insertion caret (blinking cursor).

**Key Capabilities:**
- Move caret to specific locations
- Show/hide caret
- Query caret position and size
- Set caret appearance

**Why It Was Needed:**
- Needed precise control over caret positioning for editing commands
- Required for implementing custom keyboard navigation
- Used to maintain caret visibility during programmatic DOM changes

**Usage in OLW:**
- Command implementations (bold, italic, etc.)
- Custom keyboard handlers

---

## Display and Rendering Interfaces

### IDisplayServicesRaw
**GUID:** `3050f69d-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Provides access to MSHTML's rendering and layout services.

**Key Capabilities:**
- Create display pointers for visual positioning (different from logical markup pointers)
- Transform coordinates between different coordinate systems (screen, client, content)
- Get computed styles at specific positions
- Access the caret object
- Scroll elements into view
- Query layout flow properties

**Why It Was Needed:**
- Needed to position UI elements (like the floating toolbar) relative to selection
- Required for spell-check squiggle rendering at correct visual positions
- Used to determine visible regions for on-demand rendering

**Usage in OLW:**
- `DisplayServices.cs` - Managed wrapper
- Spell-check highlighting
- Smart content positioning

---

### IDisplayPointerRaw
**GUID:** `3050f69e-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Represents a display position (visual/rendered position vs. logical markup position).

**Key Capabilities:**
- Move to markup pointers (convert logical to visual position)
- Move by display lines
- Get position in display coordinates
- Query line height and baseline information

**Why It Was Needed:**
- Visual positioning differs from logical markup order (e.g., right-to-left text, float positioning)
- Needed for accurate spell-check underline rendering
- Used for visual line navigation

**Usage in OLW:**
- Spell-check visual highlighting
- Selection rendering

---

### IHighlightRenderingServicesRaw
**GUID:** `3050F606-98B5-11CF-BB82-00AA00BDCE0B`

**Purpose:** Allows rendering custom highlights/overlays on top of document content.

**Key Capabilities:**
- Add highlight segments with custom rendering styles
- Move segments to new positions
- Remove highlights

**Why It Was Needed:**
- Core interface for spell-check squiggle rendering
- Provided visual feedback without modifying the actual DOM
- Allowed multiple overlapping highlights (spelling, grammar, search results)

**Usage in OLW:**
- `SpellingHighlighter.cs` - Renders spell-check underlines
- `HighlightSegmentTracker.cs` - Manages highlight lifecycle

---

### IHighlightSegmentRaw
**GUID:** (Returned by highlight rendering services)

**Purpose:** Represents a single highlighted region.

**Key Capabilities:**
- Query segment range
- Modify segment appearance

**Why It Was Needed:**
- Needed stable references to individual highlights
- Used to update highlights as text changed

**Usage in OLW:**
- Spell-check highlight management

---

### IHTMLPainterRaw
**GUID:** `3050f6a6-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Custom rendering interface allowing drawing on top of or behind HTML elements.

**Key Capabilities:**
- Draw custom graphics in element rendering layers
- Receive notifications about rendering events
- Access rendering surface and painting information

**Why It Was Needed:**
- Used for advanced visual customizations
- Allowed plugin-based rendering extensions
- Provided visual feedback for edit operations

**Usage in OLW:**
- Custom element rendering for plugins

---

## Editing Behavior Interfaces

### IHTMLEditServicesRaw
**GUID:** `3050f663-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Entry point for registering custom editing behaviors and accessing editing subsystems.

**Key Capabilities:**
- Add/remove edit designers (behavior customizers)
- Get selection services for a markup container
- Move selection programmatically
- Set selection ranges with specific selection types

**Why It Was Needed:**
- Required to register custom edit designers
- Provided access to selection services
- Allowed overriding default editing behaviors

**Usage in OLW:**
- `MshtmlEditor.cs` - Registers custom edit designer
- Integrates custom editing behaviors

---

### IHTMLEditDesignerRaw
**GUID:** `3050f662-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Interface implemented by custom edit designers to receive editing notifications.

**Key Capabilities:**
- Intercept pre-handling of edit events
- Perform post-processing after edit operations
- Translate keyboard accelerators
- Handle special keys

**Why It Was Needed:**
- Needed to customize default MSHTML editing behaviors
- Allowed implementing blog-specific editing features
- Provided hooks for undo/redo, content filtering

**Usage in OLW:**
- Custom editor implementation intercepts all editing operations
- Implements blog-specific keyboard shortcuts

---

### IHTMLEditHostRaw
**GUID:** `3050f6a0-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Interface for hosting and customizing the MSHTML editor.

**Key Capabilities:**
- Snap element positions to grid
- Handle multi-selection
- Customize editing behaviors

**Why It Was Needed:**
- Required for advanced layout control
- Needed for custom editing host implementation

**Usage in OLW:**
- Editor host implementation

---

## Element Behavior Interfaces

### IElementBehaviorRaw
**GUID:** `3050f425-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Allows attaching custom behaviors to HTML elements.

**Key Capabilities:**
- Initialize and cleanup behaviors
- Receive notifications about element lifecycle

**Why It Was Needed:**
- Used to implement smart content (maps, videos, tables)
- Allowed adding rich interactive features to specific elements
- Provided hooks for element-specific rendering and editing

**Usage in OLW:**
- `MshtmlElementBehavior.cs` - Base class for element behaviors
- Smart content implementations

---

### IElementBehaviorFactoryRaw
**GUID:** `3050f429-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Factory for creating element behaviors.

**Key Capabilities:**
- Create behavior instances for specific element types

**Why It Was Needed:**
- Required to register behavior factories with MSHTML
- Allows dynamic behavior attachment based on element attributes

**Usage in OLW:**
- `ElementBehaviorFactoryForExistingBehavior.cs` - Factory implementation
- Registers smart content behaviors

---

## Security and Hosting Interfaces

### IDocHostUIHandler
**GUID:** `bd3f23c0-d43e-11cf-893b-00aa00bdce1a`

**Purpose:** Customizes the MSHTML hosting environment and UI.

**Key Capabilities:**
- Show custom context menus
- Customize drag-drop behavior
- Control double-click behavior
- Override default UI elements
- Set external UI handler

**Why It Was Needed:**
- Needed to integrate MSHTML into custom application UI
- Required for custom context menus
- Allowed controlling security settings

**Usage in OLW:**
- Editor hosting infrastructure
- Custom context menu implementation

---

### IDocHostShowUI
**GUID:** `c4d244b0-d43e-11cf-893b-00aa00bdce1a`

**Purpose:** Controls display of MSHTML UI elements and dialogs.

**Key Capabilities:**
- Show/suppress help
- Display custom error messages
- Control UI activation

**Why It Was Needed:**
- Needed to suppress IE-specific UI
- Required for custom error handling

**Usage in OLW:**
- `IDocHostShowUIBaseImpl.cs` - Base implementation

---

### IInternetSecurityManager
**GUID:** `79eac9ee-baf9-11ce-8c82-00aa004ba90b`

**Purpose:** Controls URL security zones and permissions.

**Key Capabilities:**
- Map URLs to security zones
- Query URL actions (script execution, ActiveX, etc.)
- Set security zone policies

**Why It Was Needed:**
- Required to allow local file access for image editing
- Needed to enable scripts in controlled environment
- Used to relax security for trusted content

**Usage in OLW:**
- `InternetSecurityManager.cs` - Manages security zones
- Allows local images and controlled script execution

---

### ICustomDoc
**GUID:** `3050f3f0-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Allows customizing document-level behaviors.

**Key Capabilities:**
- Set custom UI handler
- Override default document behaviors

**Why It Was Needed:**
- Required to install custom IDocHostUIHandler
- Needed for full editing customization

**Usage in OLW:**
- Editor initialization

---

### IObjectSafety
**GUID:** `cb5bdc81-93c1-11cf-8f20-00805f2cd064`

**Purpose:** Indicates object safety for scripting and initialization.

**Key Capabilities:**
- Query safety options
- Set safety flags for scripting/initialization

**Why It Was Needed:**
- Required for ActiveX control hosting
- Needed to safely host embedded objects

**Usage in OLW:**
- Smart content hosting
- Plugin infrastructure

---

## Advanced Editing Interfaces

### IMarkupServices2Raw
**GUID:** `3050f682-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Extended version of IMarkupServices with additional capabilities.

**Key Capabilities:**
- All IMarkupServices capabilities
- Additional parsing and manipulation options
- Enhanced element attribute handling

**Why It Was Needed:**
- Provided advanced features not in original interface
- Better attribute handling for complex scenarios

**Usage in OLW:**
- Enhanced markup manipulation where available

---

### IMarkupPointer2Raw
**GUID:** `3050f675-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Extended markup pointer with additional positioning capabilities.

**Key Capabilities:**
- All IMarkupPointer capabilities
- Enhanced navigation options
- Better handling of edge cases

**Why It Was Needed:**
- Provided more robust positioning
- Better behavior at element boundaries

**Usage in OLW:**
- Complex navigation scenarios

---

### IMarkupContainer2Raw
**GUID:** `3050f681-98b5-11cf-bb82-00aa00bdce0b`

**Purpose:** Extended markup container interface.

**Key Capabilities:**
- Enhanced container management
- Better fragment handling

**Why It Was Needed:**
- Improved performance for large documents
- Better clipboard operations

**Usage in OLW:**
- Document fragment operations

---

### IProtectFocus
**GUID:** `d81f90a3-8156-44f7-ad28-5abb87003274`

**Purpose:** Protects focus from being changed during operations.

**Key Capabilities:**
- Prevent focus changes
- Allow controlled focus transitions

**Why It Was Needed:**
- Prevented focus flicker during editing operations
- Maintained user experience during programmatic changes

**Usage in OLW:**
- Focus management during commands

---

## WebView2 Migration Strategy

### Why MSHTML Needs to be Replaced

1. **Deprecated Technology**: MSHTML (IE11 engine) is deprecated and no longer receives updates
2. **Security Concerns**: Unpatched vulnerabilities accumulate in unmaintained engine
3. **Modern Web Standards**: Lack of support for CSS3, ES6+, modern APIs
4. **.NET Core/.NET 5+ Migration**: MSHTML relies on .NET Framework Windows Forms WebBrowser control not available in .NET Core
5. **x64 Support**: Better compatibility and performance on 64-bit systems with modern browser engines

### WebView2 Replacement Approach

**Phase 1: Editor Core (Completed in feature/webview2 branch)**
- Replace MSHTML editor control with WebView2
- Implement contenteditable-based editor using modern browser (Edge/Chromium)
- Use JavaScript bridge for C#↔JS communication instead of COM interop
- Virtual host mapping replaces security zone manipulation for local file access

**Phase 2: Content Services**
- Replace spell-checking with browser-native or Web API-based solution
- Implement selection/highlighting using DOM Range API instead of IMarkupPointer
- Use MutationObserver instead of edit designer notifications
- Replace custom rendering with CSS-based visual feedback

**Phase 3: Smart Content**
- Port element behaviors to Web Components
- Use modern embed APIs (iframe, web components) instead of ActiveX-style behaviors
- Implement plugin system using JavaScript modules

**Key Interface Replacements:**

| MSHTML Interface | WebView2 Equivalent |
|-----------------|---------------------|
| IMarkupServicesRaw | DOM APIs (createElement, insertBefore, removeChild, etc.) |
| IMarkupPointerRaw | DOM Range and Selection APIs |
| IDisplayServicesRaw | getBoundingClientRect, IntersectionObserver |
| IHighlightRenderingServicesRaw | CSS custom properties, ::before/::after pseudo-elements |
| IHTMLEditDesignerRaw | MutationObserver, event listeners |
| ISelectionServicesRaw | Selection API, Range API |
| IElementBehaviorRaw | Web Components (custom elements) |
| IInternetSecurityManager | WebView2 virtual host mapping |

### Benefits of WebView2 Approach

- ✅ Modern, maintained browser engine (Chromium/Edge)
- ✅ Better performance on modern systems
- ✅ Standards-compliant HTML5, CSS3, ES6+ support
- ✅ Compatible with .NET Core/.NET 5+ for future migration
- ✅ Built-in developer tools for debugging
- ✅ Better security model
- ✅ Native x64 support

### Challenges Addressed

- **Image Access**: Virtual host mapping allows local file access without security zone hacks
- **C# Communication**: PostMessage bridge replaces COM interop
- **State Synchronization**: Explicit bridge calls replace ambient property access
- **Content Extraction**: JavaScript functions serialize editor state instead of IHTMLDocument traversal

---

## References

- [feature/webview2 branch](https://github.com/OpenLiveWriter/OpenLiveWriter/tree/feature/webview2) - Active WebView2 migration work (34+ commits)
- [MSHTML Reference (archived)](https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/)
- [WebView2 Documentation](https://docs.microsoft.com/en-us/microsoft-edge/webview2/)
- OpenLiveWriter source: `/src/managed/OpenLiveWriter.Mshtml/`

---

*This documentation represents the state of MSHTML usage in Open Live Writer as of the WebView2 migration effort. All interface definitions can be found in the source code at `/src/managed/OpenLiveWriter.Mshtml/Mshtml_Interop/`.*
