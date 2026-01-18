# WebView2 Spell-Check Research

## Executive Summary

This document provides research findings on integrating WebView2's built-in spell-checking capabilities into Open Live Writer, comparing it with the current Windows spell-check implementation.

**Date:** January 2026  
**Research Task:** Investigate spell-check options for WebView2 (it has built-in spell check)

---

## Current Spell-Check Implementation in OpenLiveWriter

### Architecture Overview

OpenLiveWriter currently uses the **Windows Platform Spell Check API** through the `PlatformSpellCheck.SpellChecker` wrapper.

**Key Components:**

1. **WinSpellingChecker** (`src/managed/OpenLiveWriter.SpellChecker/WinSpellingChecker.cs`)
   - Implements `ISpellingChecker` interface
   - Wraps `PlatformSpellCheck.SpellChecker` class
   - Supports BCP-47 language codes
   - Provides word-by-word checking, suggestions, add to dictionary, ignore all

2. **SpellingManager** (`src/managed/OpenLiveWriter.SpellChecker/SpellingManager.cs`)
   - Coordinates spell-checking for MSHTML-based editor
   - Manages `SpellingHighlighter` for visual feedback
   - Handles commands (add to dictionary, ignore, replace)
   - Integrates with MarkupRange for word detection

3. **SpellingHighlighter**
   - Uses MSHTML rendering services to underline misspellings
   - Real-time checking with `SpellingTimer`
   - Maintains `SortedMarkupRangeList` of misspelled words

4. **Integration Points:**
   - BlogPostHtmlEditorControl - Hosts the spell-check context
   - BlogPostHtmlEditor - Provides spell-checking context menu
   - TextEditingCommandDispatcher - Command routing

### Current Features

✓ Multi-language support (Windows installed languages)  
✓ Custom dictionary (add words)  
✓ Ignore all/once functionality  
✓ Real-time spell checking  
✓ Context menu suggestions  
✓ Auto-correct capabilities  
✓ Integration with MSHTML word range detection  

### Current Limitations

- Tied to MSHTML control (Internet Explorer rendering engine)
- Requires Windows 8+ for Platform Spell Check API
- Manual word-by-word checking (not browser-native)
- Custom rendering layer for highlights

---

## WebView2 Built-In Spell Check

### Overview

WebView2 (Microsoft Edge WebView2), based on Chromium, provides **built-in spell check support** for editable HTML content. The spell checker is part of the Chromium engine and operates natively within the browser rendering context.

### How It Works

**Automatic Functionality:**
- Spell check is **enabled by default** in editable HTML elements:
  - `<input type="text">`
  - `<textarea>`
  - Elements with `contenteditable="true"`
- Misspellings are automatically highlighted with red squiggly underlines
- Right-click context menu provides spelling suggestions
- Language support inherited from Windows installed languages

**User Experience:**
- Native browser-style spell checking (familiar to users)
- No performance overhead from custom highlighting
- Consistent with Edge browser experience
- Multi-language detection (when available in Chromium)

### API Capabilities (Current State - January 2026)

**What's Available:**
- ✓ Spell check works automatically in contenteditable elements
- ✓ Context menu integration (if not disabled)
- ✓ Suggestions through right-click menu
- ✓ Language inheritance from OS settings

**What's NOT Available:**
- ✗ **No dedicated API to enable/disable spell checker programmatically**
- ✗ **No API to set/change spell check language at runtime**
- ✗ **No API to query available spell check languages**
- ✗ **Cannot programmatically add words to dictionary**
- ✗ **Cannot programmatically query if a word is misspelled**
- ✗ **No events for spell check state changes**

**Note on Language Setting:**
- `CoreWebView2EnvironmentOptions.Language` only affects UI language (context menus, dialogs)
- It does **NOT** reliably control the spell checker language
- Setting `lang` attribute on HTML elements has no confirmed effect on spell checker
- Spell checker uses languages from Windows language settings

### Community Feedback & Feature Requests

**Open GitHub Issues:**
- [API for Spell Checker #3758](https://github.com/MicrosoftEdge/WebView2Feedback/issues/3758)
  - Request for enable/disable API
  - Request for language selection API
  - Request for multi-language support
  - **Status:** Tracked by WebView2 team, not yet implemented

**Common Developer Needs:**
- Programmatic control over spell check on/off
- Language selection independent of OS settings
- Access to spell check status from host application
- Custom dictionary management
- Integration with application settings

### Technical Implementation Details

**How Chromium Spell Check Works:**
1. Hunspell-based spell checker (open source)
2. Dictionary files (.bdic format) stored with browser
3. Language detection based on:
   - OS installed languages
   - Browser language preferences
   - HTML `lang` attributes (in full Chrome, but not exposed in WebView2)
4. Context menu handled by Chromium rendering engine

**Content Editable Integration:**
```html
<!-- Spell check works automatically -->
<div contenteditable="true">
  User can type here and spell check works
</div>

<!-- Can be explicitly controlled via HTML attribute -->
<div contenteditable="true" spellcheck="true">
  Spell check enabled
</div>

<div contenteditable="true" spellcheck="false">
  Spell check disabled
</div>
```

**JavaScript Access:**
```javascript
// HTML5 spellcheck attribute can be read/written
element.spellcheck = true;  // Enable
element.spellcheck = false; // Disable

// But no API to:
// - Get spell check language
// - Set spell check language
// - Get suggestions programmatically
// - Check if word is misspelled
```

---

## Integration Options for OpenLiveWriter

### Option 1: Rely on WebView2 Native Spell Check

**Approach:**
- Replace MSHTML editor with WebView2-based content editable
- Let Chromium handle spell checking natively
- Use HTML `spellcheck` attribute for on/off toggle
- Leverage browser context menu for suggestions

**Pros:**
- ✓ No custom spell check code needed
- ✓ Native browser performance
- ✓ Automatic language support
- ✓ Familiar user experience (matches Edge/Chrome)
- ✓ Reduced maintenance burden
- ✓ Future improvements from Chromium updates

**Cons:**
- ✗ Loss of programmatic control
- ✗ Cannot integrate with custom UI for spell check dialog
- ✗ Cannot programmatically add words to dictionary
- ✗ Cannot implement "ignore all" across sessions
- ✗ Language selection limited to OS settings
- ✗ Cannot implement custom spell check preferences panel

**Implementation Effort:** Low (rely on defaults)

### Option 2: Hybrid Approach (WebView2 + Windows Spell Check API)

**Approach:**
- Use WebView2 for rendering and editing
- Disable WebView2 native spell check (`spellcheck="false"`)
- Continue using Windows Platform Spell Check API
- Implement custom highlighting via JavaScript/DOM manipulation
- Maintain current SpellingManager architecture

**Pros:**
- ✓ Retain full programmatic control
- ✓ Keep existing spell check features
- ✓ Custom dictionary management
- ✓ Integration with OpenLiveWriter preferences
- ✓ "Ignore all" functionality
- ✓ Custom spell check dialog

**Cons:**
- ✗ More complex implementation
- ✗ Need to reimplement highlighting for WebView2/DOM
- ✗ Performance overhead from custom checking
- ✗ Maintenance of custom spell check layer

**Implementation Effort:** Medium-High (port existing logic to WebView2)

### Option 3: Wait for WebView2 Spell Check API

**Approach:**
- Monitor WebView2 feedback issues for API additions
- Use native spell check temporarily
- Plan migration once API becomes available

**Pros:**
- ✓ Future-proof for better API
- ✓ Native performance in the meantime
- ✓ Minimal current investment

**Cons:**
- ✗ Unknown timeline for API availability
- ✗ Feature gap in current implementation
- ✗ Uncertain if API will meet all needs

**Implementation Effort:** Very Low initially, unknown future effort

### Option 4: Web-Based Spell Check Library

**Approach:**
- Use JavaScript spell check library (e.g., Typo.js, nspell)
- Bundle dictionary files with application
- Implement spell checking entirely in JavaScript
- Full control from WebView2 JavaScript context

**Pros:**
- ✓ Complete programmatic control
- ✓ Custom dictionary support
- ✓ Cross-platform potential
- ✓ No dependency on OS spell check
- ✓ Can bundle any language dictionaries

**Cons:**
- ✗ Large dictionary files to distribute
- ✗ Performance concerns for large documents
- ✗ Need to maintain dictionary updates
- ✗ Complex integration with WebView2 host
- ✗ Duplicate spell check resources (Chromium + custom)

**Implementation Effort:** High

---

## Recommendations

### Short-Term (Current Development)

**If migrating to WebView2 soon:**
1. **Use WebView2 native spell check** as the default
2. Accept limited programmatic control as acceptable tradeoff
3. Leverage HTML `spellcheck` attribute for user on/off toggle
4. Document limitations vs. current MSHTML implementation
5. Monitor WebView2 API development for future enhancements

**Justification:**
- Native spell check is "good enough" for most users
- Reduced code complexity and maintenance
- Better long-term alignment with modern web standards
- Users familiar with Edge/Chrome spell checking

### Medium-Term (6-12 Months)

**Track WebView2 Spell Check API progress:**
1. Subscribe to GitHub issue #3758
2. Participate in WebView2 feedback community
3. Test preview builds when API becomes available
4. Prepare for migration to enhanced API

**Consider hybrid if programmatic control is critical:**
- Evaluate user feedback on native spell check limitations
- Implement hybrid approach only if users demand advanced features
- Prioritize features: language selection, custom dictionary, ignore all

### Long-Term (1+ Years)

**Goal:** Full-featured spell checking with WebView2 API
- Language selection UI
- Custom dictionary management
- Ignore all/once functionality
- Integration with OpenLiveWriter preferences
- Spell check dialog (if WebView2 API supports programmatic checking)

---

## Migration Considerations

### If Replacing MSHTML with WebView2

**Spell Check Migration Steps:**

1. **Assessment Phase:**
   - Document current spell check features used by users
   - Survey user feedback on spell check importance
   - Identify must-have vs. nice-to-have features

2. **Implementation Phase:**
   - Set `contenteditable="true"` on editor container
   - Default `spellcheck="true"` (can be toggled via attribute)
   - Remove custom SpellingHighlighter code
   - Simplify SpellingManager or remove if not needed
   - Update preferences UI to reflect available options

3. **Testing Phase:**
   - Test spell check in multiple languages
   - Verify context menu suggestions work
   - Test spellcheck on/off toggle
   - Compare with current MSHTML spell check

4. **Documentation Phase:**
   - Update user documentation
   - Note differences from previous version
   - Document workarounds for missing features

### Backward Compatibility

**Settings Migration:**
- `SpellingSettings.EnableRealTimeSpellChecking` → `spellcheck` attribute
- `SpellingSettings.Language` → OS language settings (no direct mapping)
- Custom dictionary entries → No migration path (stored in Windows)

**Feature Parity:**
- ✓ Real-time spell checking: Available
- ✓ Context menu suggestions: Available
- ✗ Programmatic language selection: Not available
- ✗ Spell check dialog: Not available
- ✗ Add to dictionary: Context menu only
- ✗ Ignore all: Not available

---

## Technical References

### WebView2 Documentation
- [WebView2 API Reference](https://learn.microsoft.com/en-us/microsoft-edge/webview2/webview2-api-reference)
- [Overview of WebView2 APIs](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/overview-features-apis)
- [WebView2 Samples](https://github.com/MicrosoftEdge/WebView2Samples)

### Community Feedback
- [API for Spell Checker #3758](https://github.com/MicrosoftEdge/WebView2Feedback/issues/3758)
- [How to change spell check language in WebView2](https://stackoverflow.com/questions/66274495/how-can-i-change-the-language-used-for-spell-checking-in-microsoft-edge-webview2)
- [WebView2 spell checker language switching](https://stackoverflow.com/questions/72198430/how-do-you-switch-spellchecker-language-in-webview2-winforms)

### HTML5 Spell Check Standard
- [MDN: spellcheck attribute](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/spellcheck)
- [HTML Living Standard: spellcheck](https://html.spec.whatwg.org/multipage/interaction.html#spelling-and-grammar-checking)

### Current OpenLiveWriter Implementation
- `src/managed/OpenLiveWriter.SpellChecker/WinSpellingChecker.cs`
- `src/managed/OpenLiveWriter.SpellChecker/SpellingManager.cs`
- `src/managed/OpenLiveWriter.SpellChecker/SpellingHighlighter.cs`
- `src/managed/OpenLiveWriter.SpellChecker/ISpellingChecker.cs`

---

## Conclusion

WebView2 provides **functional but limited** spell-check capabilities suitable for basic content editing. The lack of programmatic API is the primary limitation. For OpenLiveWriter:

**Best Path Forward:**
1. Start with WebView2 native spell check (simplest, works for most users)
2. Document feature gaps compared to current implementation
3. Monitor WebView2 API development for future enhancements
4. Consider hybrid approach only if user feedback demands it

**Key Tradeoff:**
- **Simplicity & native integration** vs. **programmatic control & advanced features**

The recommendation is to **accept the tradeoff in favor of simplicity** unless specific user requirements demand the hybrid approach.

---

**End of Research Document**
