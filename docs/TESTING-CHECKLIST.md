# Open Live Writer Testing Checklist

## Overview

This comprehensive testing checklist covers WebView2 editor migration, x64 builds, and general application functionality. Use this document to validate changes before release.

---

## Test Environment Setup

### Prerequisites

- [ ] Windows 10 (version 1809+) or Windows 11
- [ ] Visual Studio 2022 with C++ build tools (v143+)
- [ ] .NET Framework 4.7.2 Runtime
- [ ] WebView2 Runtime installed (or will auto-install)
- [ ] Test blog account configured (WordPress, Blogger, or static site)

### Build Verification

#### x86 Build (Baseline)
```powershell
.\build.ps1 -Configuration Release
```
- [ ] Build completes with 0 errors, 0 warnings
- [ ] Output at `src\managed\OpenLiveWriter.PostEditor\bin\Release\`
- [ ] All DLLs present (check for missing dependencies)
- [ ] Ribbon DLL built: `OpenLiveWriter.Ribbon.dll`

#### x64 Build (Post-Migration)
```powershell
.\build.ps1 -Platform x64 -Configuration Release
```
- [ ] Build completes with 0 errors, 0 warnings
- [ ] Output at `src\managed\OpenLiveWriter.PostEditor\bin\x64\Release\`
- [ ] Verify x64 binaries: `dumpbin /headers OpenLiveWriter.exe | findstr machine`
  - Should show: `8664 machine (x64)`
- [ ] Ribbon DLL is x64: `dumpbin /headers OpenLiveWriter.Ribbon.dll | findstr machine`

---

## WebView2 Editor Testing

### Environment Variable
```cmd
set OLW_USE_WEBVIEW2_EDITOR=1
```
OR use the run script:
```cmd
.\run-webview2.cmd
```

### Startup and Initialization

- [ ] Application launches without errors
- [ ] Editor control loads (no blank screen)
- [ ] No console errors or warnings in debug output
- [ ] Editor is responsive to input within 2 seconds

### Basic Text Editing

#### Plain Text Entry
- [ ] Type text in editor
- [ ] Text appears immediately
- [ ] Cursor position correct
- [ ] Backspace deletes characters
- [ ] Delete key works
- [ ] Arrow keys navigate correctly

#### Text Selection
- [ ] Click and drag to select text
- [ ] Double-click selects word
- [ ] Triple-click selects paragraph
- [ ] Shift+Arrow extends selection
- [ ] Ctrl+A selects all
- [ ] Selected text highlighted correctly

#### Clipboard Operations
- [ ] Ctrl+C copies selected text
- [ ] Ctrl+X cuts selected text
- [ ] Ctrl+V pastes plain text
- [ ] Paste HTML from Word preserves formatting
- [ ] Paste from web browser works
- [ ] Cut/copy/paste from right-click menu

---

### Text Formatting

#### Basic Formatting (Ribbon Buttons)
- [ ] **Bold** (Ctrl+B) - applies `<strong>` or `<b>`
- [ ] *Italic* (Ctrl+I) - applies `<em>` or `<i>`
- [ ] <u>Underline</u> (Ctrl+U) - applies `<u>` or `text-decoration`
- [ ] ~~Strikethrough~~ - applies `<s>` or `<strike>`
- [ ] `Code` - applies `<code>` or monospace font
- [ ] Remove formatting - clears inline styles

#### Paragraph Formatting
- [ ] Paragraph alignment:
  - [ ] Left align
  - [ ] Center align
  - [ ] Right align
  - [ ] Justify
- [ ] Headings (H1-H6):
  - [ ] Apply heading style
  - [ ] Correct HTML tag used
  - [ ] Style appears visually different
- [ ] Blockquote
  - [ ] Apply blockquote
  - [ ] Nested blockquotes work
  - [ ] Correct indentation

#### Lists
- [ ] Bulleted list (unordered):
  - [ ] Create new list
  - [ ] Press Enter creates new item
  - [ ] Tab indents (nested list)
  - [ ] Shift+Tab outdents
  - [ ] Backspace at start of empty item exits list
- [ ] Numbered list (ordered):
  - [ ] Create new list
  - [ ] Numbers increment automatically
  - [ ] Nested numbered lists
  - [ ] Mix of bullet and numbered lists

#### Fonts and Colors
- [ ] Font family dropdown:
  - [ ] Change font
  - [ ] Font applies to selection
  - [ ] Font persists after typing
- [ ] Font size:
  - [ ] Increase font size
  - [ ] Decrease font size
  - [ ] Specific size value
- [ ] Text color:
  - [ ] Apply foreground color
  - [ ] Color picker works
  - [ ] Custom colors
- [ ] Background color (highlight):
  - [ ] Apply background color
  - [ ] Remove background color

---

### Links

#### Insert Hyperlink
- [ ] Select text, insert link
- [ ] Dialog shows correct default text
- [ ] Enter URL, click OK
- [ ] Link created with correct href
- [ ] Link is clickable (Ctrl+Click or right-click → Open)
- [ ] Link styling (underline, color)

#### Edit Hyperlink
- [ ] Click in existing link
- [ ] Edit Link button enabled
- [ ] Dialog pre-fills with current URL and text
- [ ] Modify URL, save
- [ ] Link updates correctly

#### Remove Hyperlink
- [ ] Select linked text
- [ ] Click Remove Link button
- [ ] Link removed, text remains
- [ ] Formatting preserved

#### Link Edge Cases
- [ ] Link with no text (should use URL as text)
- [ ] Link entire paragraph
- [ ] Link with formatted text (bold link)
- [ ] Email link (mailto:)
- [ ] Anchor link (#section)

---

### Images

#### Insert Image from File
- [ ] Click Insert Image button
- [ ] File picker opens
- [ ] Select local image (JPG)
  - [ ] Image appears in editor
  - [ ] Correct size and aspect ratio
  - [ ] Image is selectable
- [ ] Select local image (PNG)
  - [ ] Transparency preserved
- [ ] Select local image (GIF)
  - [ ] Animation works (if supported)

#### Virtual Host Mapping (WebView2 Specific)
- [ ] Insert image from `C:\` drive
  - [ ] Image displays (not blocked by security)
  - [ ] Check src attribute: `olw-local-c://path/to/image.jpg`
- [ ] Insert image from other drive (if available, e.g., D:\)
  - [ ] Virtual host mapping works for all drives
- [ ] Insert image from network path (UNC `\\server\share`)
  - [ ] Works or shows appropriate error

#### Image Operations
- [ ] Resize image:
  - [ ] Click and drag corner handle
  - [ ] Proportions maintained
  - [ ] Width/height attributes updated
- [ ] Image alignment:
  - [ ] Left
  - [ ] Center
  - [ ] Right
- [ ] Delete image:
  - [ ] Select image, press Delete
  - [ ] Image removed from editor
- [ ] Copy/paste image:
  - [ ] Copy image in editor
  - [ ] Paste to new location
  - [ ] Both instances work

#### Image Edge Cases
- [ ] Very large image (>10MB)
  - [ ] Loads without crash
  - [ ] May show loading indicator
- [ ] Unicode path (e.g., `C:\Images\测试\test.jpg`)
  - [ ] Image displays correctly
  - [ ] Path encoding correct
- [ ] Path with spaces
  - [ ] Image displays correctly
- [ ] Relative vs. absolute paths
  - [ ] Check HTML source for path handling

---

### Tables

#### Create Table
- [ ] Insert table (e.g., 3x3)
- [ ] Table renders with visible borders
- [ ] Click in cells to edit
- [ ] Tab key moves to next cell
- [ ] Shift+Tab moves to previous cell

#### Table Editing
- [ ] Add row:
  - [ ] Above current row
  - [ ] Below current row
- [ ] Add column:
  - [ ] Left of current column
  - [ ] Right of current column
- [ ] Delete row
- [ ] Delete column
- [ ] Delete entire table
- [ ] Merge cells
- [ ] Split cells

#### Table Formatting
- [ ] Cell background color
- [ ] Cell borders
- [ ] Cell padding/spacing
- [ ] Table alignment

---

### Edit ↔ Source View Switching

#### Edit → Source
- [ ] Create formatted content in Edit view:
  - Bold, italic, link, image
- [ ] Switch to Source view
- [ ] HTML is correct:
  - [ ] Proper tags (`<strong>`, `<em>`, `<a>`, `<img>`)
  - [ ] No extra whitespace or formatting
  - [ ] Image src uses virtual host mapping (if local)
- [ ] Indentation is readable

#### Source → Edit
- [ ] Enter HTML in Source view:
```html
<p>This is <strong>bold</strong> and <em>italic</em>.</p>
<img src="olw-local-c://Users/Public/Pictures/Sample.jpg" />
```
- [ ] Switch to Edit view
- [ ] Content renders correctly:
  - [ ] Bold and italic applied
  - [ ] Image displays
- [ ] No content loss

#### Round-Trip Testing (Critical for WebView2)
- [ ] Create complex content in Edit view:
  - Headings, lists, images, links, formatting
- [ ] Switch to Source, verify HTML
- [ ] Switch back to Edit
- [ ] Switch to Source again
- [ ] **HTML is identical** (no content accumulation or loss)
- [ ] Test with edge case content:
  - [ ] Multiple `</div>` tags (regex bug test)
  - [ ] Nested lists
  - [ ] Tables with images
  - [ ] Blockquotes with formatting

---

### Spell Checking

*Note: Spell checking may use browser-native or custom implementation*

#### Basic Spell Check
- [ ] Type misspelled word (e.g., "teh")
- [ ] Red squiggle appears (or browser underline)
- [ ] Right-click shows suggestions
- [ ] Click suggestion replaces word
- [ ] Ignore option works
- [ ] Add to dictionary works (if supported)

#### Language Support
- [ ] Change spell-check language (if supported)
- [ ] Verify words checked in correct language
- [ ] Unicode characters handled (e.g., accented letters)

---

### Undo/Redo

- [ ] Type text, Ctrl+Z undoes
- [ ] Ctrl+Y or Ctrl+Shift+Z redoes
- [ ] Undo formatting change
- [ ] Undo image insertion
- [ ] Undo delete
- [ ] Undo/redo stack preserves multiple operations
- [ ] Undo works after Edit ↔ Source switch

---

### Find and Replace

*If implemented*

- [ ] Ctrl+F opens Find dialog
- [ ] Enter search term, finds first match
- [ ] Find Next works
- [ ] Find Previous works
- [ ] Find with case sensitivity
- [ ] Replace single instance
- [ ] Replace all instances
- [ ] Find across formatted content
- [ ] Close find dialog

---

### Keyboard Shortcuts

#### Editing Shortcuts
- [ ] Ctrl+B - Bold
- [ ] Ctrl+I - Italic
- [ ] Ctrl+U - Underline
- [ ] Ctrl+K - Insert Link
- [ ] Ctrl+A - Select All
- [ ] Ctrl+C - Copy
- [ ] Ctrl+X - Cut
- [ ] Ctrl+V - Paste
- [ ] Ctrl+Z - Undo
- [ ] Ctrl+Y - Redo

#### Navigation Shortcuts
- [ ] Home - Start of line
- [ ] End - End of line
- [ ] Ctrl+Home - Start of document
- [ ] Ctrl+End - End of document
- [ ] Page Up/Down - Scroll

---

### Smart Content / Plugins

*If WebView2 branch includes plugin support*

#### Map Plugin (Deprecated in context)
- [ ] Insert map (if still present)
- [ ] Map renders
- [ ] Can edit location
- [ ] Can remove map
- [ ] Source view shows correct embed

#### Video Plugin (Deprecated in context)
- [ ] Insert video (if still present)
- [ ] Video embed renders
- [ ] Can preview video
- [ ] Can edit embed code
- [ ] Source view shows correct embed

#### Custom Plugins
- [ ] Plugin loads in WebView2 environment
- [ ] Plugin renders content
- [ ] Plugin settings dialog works
- [ ] Plugin content persists in source view

---

## Blog Integration Testing

### Account Setup

- [ ] Add blog account (WordPress, Blogger, etc.)
- [ ] Account wizard completes
- [ ] Can fetch categories
- [ ] Can fetch recent posts
- [ ] Account saved correctly

### Post Creation

- [ ] Create new post
- [ ] Enter title
- [ ] Enter content in WebView2 editor
- [ ] Add image
- [ ] Add link
- [ ] Select category
- [ ] Add tags

### Post Publishing

- [ ] Click Publish
- [ ] Progress indicator appears
- [ ] Post uploads successfully
- [ ] Open in browser to verify:
  - [ ] Title correct
  - [ ] Content matches editor
  - [ ] Images display (uploaded to blog)
  - [ ] Formatting preserved
  - [ ] Links work

### Post Editing

- [ ] Open existing post
- [ ] Content loads in editor
- [ ] Make changes
- [ ] Publish changes
- [ ] Verify update on blog

### Draft Saving

- [ ] Create post, don't publish
- [ ] Save as draft
- [ ] Close application
- [ ] Reopen, find draft
- [ ] Draft content intact

---

## x64-Specific Testing

*Only applicable after x64 migration is complete*

### Build Verification

- [ ] x64 build completes without errors
- [ ] All binaries verified as x64 (not x86 or AnyCPU)
- [ ] Ribbon DLL is x64
- [ ] No mixed-mode assembly errors

### Platform Specific

#### Memory Handling
- [ ] Create blog post with 50+ images (2-5MB each)
  - [ ] Editor remains responsive
  - [ ] No out-of-memory errors (should improve on x64)
  - [ ] Images load and display
  - [ ] Can publish successfully

- [ ] Open multiple posts simultaneously (if supported)
  - [ ] Memory usage reasonable
  - [ ] No crashes

#### P/Invoke and Interop
- [ ] Ribbon UI works (tests P/Invoke to native DLL)
  - [ ] All buttons clickable
  - [ ] Dropdowns expand
  - [ ] Dialogs open
- [ ] Windows API calls work:
  - [ ] File dialogs (Open, Save)
  - [ ] Color picker
  - [ ] Font dialog
  - [ ] Print dialog (if applicable)

#### COM Interop (WebView2)
- [ ] WebView2 control initializes
  - [ ] Tests x64 COM registration
- [ ] JavaScript ↔ C# bridge works:
  - [ ] Can get editor content from C#
  - [ ] Can set editor content from C#
  - [ ] Can invoke C# methods from JS
  - [ ] Selection synchronization

### Performance Comparison

*Run on same machine, same content*

| Test | x86 Time | x64 Time | Expected |
|------|----------|----------|----------|
| App startup | ____ | ____ | Similar or better |
| Load post with 20 images | ____ | ____ | Better on x64 |
| Apply formatting to 10KB text | ____ | ____ | Similar |
| Publish 50-image post | ____ | ____ | Better on x64 |
| Memory usage (idle) | ____ MB | ____ MB | Slightly higher on x64 |
| Memory usage (50 images) | ____ MB | ____ MB | Can go higher on x64 |

### Compatibility

- [ ] Install on Windows 10 x64 - works
- [ ] Install on Windows 11 x64 - works
- [ ] Upgrade from x86 version:
  - [ ] Settings preserved
  - [ ] Drafts preserved
  - [ ] Account info preserved
  - [ ] No data loss

---

## Installation and Upgrade Testing

### Fresh Install (x86 or x64)

- [ ] Download installer
- [ ] Run installer
  - [ ] Wizard completes without errors
  - [ ] Installs to correct directory:
    - x86: `C:\Program Files (x86)\Open Live Writer\`
    - x64: `C:\Program Files\Open Live Writer\`
- [ ] Shortcuts created (Start Menu, Desktop if selected)
- [ ] Launch from shortcut - works
- [ ] First run wizard:
  - [ ] Add blog account
  - [ ] Complete setup

### Upgrade Scenarios

#### x86 → x86 (Same Architecture)
- [ ] Install older x86 version
- [ ] Create test blog post, save as draft
- [ ] Install newer x86 version
- [ ] Settings migrated
- [ ] Drafts preserved
- [ ] Application works

#### x86 → x64 (Architecture Change)
- [ ] Install x86 version
- [ ] Create test content
- [ ] Install x64 version
  - Installer should detect existing installation
- [ ] Settings migrated to new x64 install location (if path differs)
- [ ] All content accessible
- [ ] No configuration loss

### Uninstall

- [ ] Uninstall via Control Panel
- [ ] Application removed from Program Files
- [ ] Shortcuts removed
- [ ] (Optional) User data preserved in AppData (expected behavior)

---

## Edge Cases and Error Handling

### File System

- [ ] Unicode filenames:
  - [ ] Insert image with Chinese/Japanese/Arabic filename
  - [ ] Image displays
  - [ ] Publish works (image uploaded)

- [ ] Very long paths (>260 characters):
  - [ ] Insert image from deep folder structure
  - [ ] Graceful handling or appropriate error

- [ ] Network paths:
  - [ ] Insert image from UNC path `\\server\share\image.jpg`
  - [ ] Works or clear error message

- [ ] Missing files:
  - [ ] Open post with image that no longer exists
  - [ ] Placeholder shown or error message
  - [ ] Application doesn't crash

### Content Edge Cases

- [ ] Empty post:
  - [ ] Try to publish empty post
  - [ ] Warning or validation error

- [ ] Very large post (>100KB text):
  - [ ] Editor remains responsive
  - [ ] Publishes successfully

- [ ] Special characters:
  - [ ] Type emoji: 😀🎉✨
  - [ ] Type mathematical symbols: ∫∑√
  - [ ] Type accented characters: café, niño, Zürich
  - [ ] All render correctly in Edit and Source views
  - [ ] Publish and verify on blog

- [ ] HTML injection:
  - [ ] Paste malicious script in Source view: `<script>alert('XSS')</script>`
  - [ ] Switch to Edit view
  - [ ] Script sanitized or escaped (not executed)
  - [ ] Publish and verify script not executed on blog

### Network Edge Cases

- [ ] Offline mode:
  - [ ] Disconnect network
  - [ ] Try to publish
  - [ ] Clear error message (not crash)
  - [ ] Draft saved locally

- [ ] Slow connection:
  - [ ] Simulate slow network (if possible)
  - [ ] Publish progress indicator shows
  - [ ] Publish eventually completes or times out gracefully

- [ ] API changes:
  - [ ] Blog API returns error (simulate with test server)
  - [ ] Error message displayed
  - [ ] Post not lost

### Application Crashes and Recovery

- [ ] Terminate application mid-edit:
  - [ ] Kill process
  - [ ] Restart application
  - [ ] Autosaved draft recoverable (if feature exists)

- [ ] Corrupt settings:
  - [ ] Manually corrupt config file
  - [ ] Launch application
  - [ ] Defaults restored or repair attempted

---

## Accessibility Testing

*Basic accessibility checks*

- [ ] Keyboard navigation:
  - [ ] Tab through all UI elements
  - [ ] No keyboard traps
  - [ ] Focus indicator visible

- [ ] Screen reader (NVDA/JAWS):
  - [ ] UI elements announced
  - [ ] Editor content readable
  - [ ] Buttons labeled correctly

- [ ] High contrast mode:
  - [ ] Application usable in high contrast
  - [ ] Text readable
  - [ ] Buttons visible

---

## Regression Testing (Continuous)

After any code change, verify core functionality:

- [ ] Application launches
- [ ] Can type in editor
- [ ] Can format text (bold, italic)
- [ ] Can insert image
- [ ] Can insert link
- [ ] Can switch Edit ↔ Source without data loss
- [ ] Can publish to blog (smoke test with real account)

---

## Test Sign-Off

### Tested By
- Name: _______________
- Date: _______________
- Build: _______________
- Platform: [ ] x86  [ ] x64
- WebView2: [ ] Enabled  [ ] Disabled (MSHTML)

### Overall Result
- [ ] All critical tests passed
- [ ] Minor issues noted (list below)
- [ ] Blocking issues found (list below)

### Issues Found

| Issue # | Severity | Description | Status |
|---------|----------|-------------|--------|
| | | | |
| | | | |

### Notes

---

## Automated Testing

*For future implementation*

### Unit Tests
- [ ] Run all unit tests: `dotnet test`
- [ ] All tests pass

### Integration Tests
- [ ] WebView2 initialization test
- [ ] Content round-trip test (Edit → Source → Edit)
- [ ] Image insertion test
- [ ] Blog API communication test

### Performance Tests
- [ ] Startup time < 3s
- [ ] Editor load time < 1s
- [ ] Publish 10-image post < 30s (depends on connection)

---

*This checklist should be updated as features are added, removed, or modified. Use as a living document for quality assurance.*
