# TODO/FIXME Tracking Document

**Generated:** 2026-01-18  
**Total Count:** 167 TODO/FIXME comments found

## Executive Summary

This document catalogs all TODO and FIXME comments found in the OpenLiveWriter C# codebase.
Comments are categorized by type to enable prioritized cleanup efforts.

### Breakdown by Category

| Category | Count | Priority | Description |
|----------|-------|----------|-------------|
| Generated Code (InitializeComponent) | 45 | LOW | Auto-generated designer comments, safe to ignore |
| Ribbon UI Migration | 26 | MEDIUM | Related to ribbon UI framework, may be historical |
| OLW Migration | 10 | MEDIUM | Related to Windows Live Writer → Open Live Writer transition |
| FIXME (Critical) | 9 | HIGH | Identified bugs or issues needing fixes |
| IE7/Browser Compatibility | 2 | LOW | Legacy browser compatibility notes |
| General Implementation | 75 | VARIES | Various implementation notes and improvements |

---

## HIGH PRIORITY - FIXME Comments (9)

These require immediate attention as they indicate known issues or bugs.

### 1. Video Feature FIXMEs (5)

**Impact:** Video publishing and management functionality

#### VideoHelper.cs:173
```csharp
// FIXME
```
**Context:** Dispose implementation  
**File:** `src/managed/OpenLiveWriter.PostEditor/Video/VideoHelper.cs`  
**Action:** Implement proper disposal pattern for video resources

#### VideoPublishSource.cs:363
```csharp
youtube.Init(null, this, blogId); //FIXME
```
**Context:** YouTube initialization with null parameter  
**File:** `src/managed/OpenLiveWriter.PostEditor/Video/VideoPublishSource.cs`  
**Action:** Review if null is appropriate or if proper value needed

#### VideoSmartContent.cs:494, 684, 696
Multiple FIXMEs in smart content handling:
- Line 494: General FIXME (needs investigation)
- Line 684: "It would be nice if this was copied to all the old versions"
- Line 696: "The problem with doing this is that once you undo a delete file..."

**File:** `src/managed/OpenLiveWriter.PostEditor/Video/VideoSmartContent.cs`  
**Action:** 
- Investigate line 494 context
- Consider version propagation for line 684
- Review undo/delete interaction for line 696

### 2. DPI Scaling FIXMEs (3)

**Impact:** High-DPI display rendering issues

#### PostEditorFooter.cs:237
```csharp
// FIXME: Kludge to fix height of footer on Hi-DPI, for some reason 
// the height becomes double what it should be.
```
**File:** `src/managed/OpenLiveWriter.PostEditor/PostEditorFooter.cs`  
**Action:** Investigate root cause of DPI scaling issue, implement proper fix

#### BlogPostHtmlEditorControl.cs:283, 656
```csharp
// FIXME: dynamically scale this on DPI change
// FIXME: Dynamically scale this padding on DPI change
```
**File:** `src/managed/OpenLiveWriter.PostEditor/PostHtmlEditing/BlogPostHtmlEditorControl.cs`  
**Action:** Implement proper DPI change event handling and dynamic scaling

### 3. Formatted HTML Printer (1)

#### FormattedHtmlPrinter.cs:55
```csharp
//FIXME: if the Right/Left operations returned the available text value, 
// this wouldn't be necessary.
```
**File:** `src/managed/OpenLiveWriter.PostEditor/PostHtmlEditing/FormattedHtmlPrinter.cs`  
**Action:** Refactor Right/Left operations to return available text value

---

## MEDIUM PRIORITY - Ribbon UI Migration (26)

These TODOs relate to the ribbon UI framework migration. Some may be historical and can be marked complete or removed.

### Code Cleanup TODOs (5)

Files with "Cleanly remove obsolete code" comments:
1. `BlogProviderButtonManager.cs:205` - Remove obsolete code
2. `BlogProviderButtonCommandBar.cs:21` - Remove obsolete code  
3. `PostEditorMainControl.cs:50` - Remove obsolete code
4. `WeblogCommandManager.cs:18` - Remove obsolete code
5. `BlogPostHtmlEditor.cs:28` - Remove obsolete code

**Action:** Audit these files to determine if ribbon migration is complete, remove obsolete code if safe

### UI Enhancement TODOs (8)

1. `BlogProviderButtonManager.cs:176` - Add an icon for the admin link item
2. `VideoEditorControl.cs:60` - Other overrides for image/string props
3. `MarginCommand.cs:34` - Have a way to initialize this
4. `TextEditingCommandDispatcher.cs:56` - Render preview images of each font family
5. `TextEditingCommandDispatcher.cs:502` - More efficient invalidation
6. `TextEditingCommandDispatcher.cs:1178` - Rationalize StyleCommand with SemanticHtmlStyleCommand
7. `TextEditingCommandDispatcher.cs:1697` - Unify Execute and ExecuteWithArgs events
8. `HtmlImageResizeEditor.cs:26` - RepresentativeString should be localizable

**Action:** Evaluate if these enhancements are still desired, prioritize or close

### Implementation TODOs (5)

1. `HtmlImageResizeEditor.cs:146` - Use logic from ImageSizeControl for resizing
2. `HTMLElementHelper.cs:848` - Need to remove spaces also?
3. `HtmlScreenCaptureCore.cs:642` - Make this work for RTL
4. `HtmlEditorControl.cs:3835` - Figure out why int conversion sometimes fails
5. `HtmlStyleHelper.cs:188` - More sophisticated tag elimination

**Action:** Review and implement or document as wontfix

### Build/Tooling (1)

`LocUtil/AppMain.cs:319` - Union of Commands.xml and Ribbon.xml → CommandId enum

**Action:** Verify this is working as intended or needs implementation

---

## MEDIUM PRIORITY - OLW Migration TODOs (10)

These relate to the Windows Live Writer → Open Live Writer migration. May be historical.

### ApplicationMain.cs (6 instances)

All in `src/managed/OpenLiveWriter/ApplicationMain.cs`:
- Lines 148, 153, 178, 385, 390, 408

**Context:** Various initialization and configuration logic  
**Action:** Review if migration is complete, remove TODO comments if done

### Other Migration TODOs

1. `WeblogConfigurationWizardPanelGoogleBloggerAuthentication.cs:181` - Authentication migration
2. `ContentEditorProxy.cs:187` - Explore options
3. `OpenLiveWriterSettingsProvider.cs:56` - Used to get the language
4. `BlogProviderButtonManager.cs:20` - P0 TODO (Priority 0)

**Action:** Audit completion status, update or remove

---

## LOW PRIORITY - Generated Designer Code (45)

Standard Visual Studio designer-generated comments: "TODO: Add any constructor code after InitializeComponent call"

**Files include:**
- Multiple test forms and display message dialogs
- Command classes
- Configuration wizards  
- Various UI controls

**Action:** These can generally be ignored as they're standard generated comments. Consider:
1. Adding `.editorconfig` rule to suppress in designer files
2. Mass removal if desired for cleanliness (low risk)

**Sample files:**
- `TestAutoDetection.cs:54`
- `TestMainForm.cs:33`
- `RequiredFieldOmittedDisplayMessage.cs:30, 42`
- `NonImageTypeFileSelectedDisplayMessage.cs:25, 37`
- `AdvancedPanel.cs:57`
- And 40+ more...

---

## LOW PRIORITY - Browser Compatibility (2)

### IE7 Compatibility

1. **SmartContentInsertionHelper.cs:299**
   ```csharp
   //TODO: after IE7 goes final, check to see if this hack is still necessary.
   ```
   **Action:** IE7 is long deprecated. Evaluate if hack still needed for modern IE/Edge compatibility, remove TODO

2. **WeblogConfigurationWizardPanelAutoDetection.cs:47**
   ```csharp
   // TODO: replace with auto-detection animation
   ```
   **Action:** Evaluate UX improvement priority

---

## GENERAL IMPLEMENTATION TODOs (75)

### Spellchecking (3)

1. `MshtmlWordRange.cs:50` - Only spellcheck inside editable regions
2. `SpellingManager.cs:266` - Only spellcheck inside editable regions  
3. `HighlightSegmentTracker.cs:111` - Change with new cultures added

**Files:** `src/managed/OpenLiveWriter.SpellChecker/`

### Content Sources & Plugins (5)

1. `SmartContentInsertionHelper.cs:142` - Document handling
2. `ContentSourceManager.cs:659` - Consolidated dialog instead of prompting for each
3. `PluginHttpRequest.cs:75-77` - Fix for Flickr4Writer WebProxy casting (see Pragma Warning Audit, item #5)
4. `MapContentSource.cs` - Related to pragma warning

### Content Editor (5)

1. `ContentEditor.cs:1298` - Check for unbalanced HTML
2. `ContentEditor.cs:2293` - LocalFileReferenceFixer call shouldn't be necessary
3. `ContentEditor.cs:2431` - Auto correct custom file
4. `ContentEditorProxy.cs:187` - Explore options
5. `ContentEditorProxy.cs:402` - Document formation comment

### Tables (2)

1. `TableAlignmentControl.cs:31` - Initialization after InitializeComponent
2. `TableHelper.cs:78` - clientWidth for empty block element

### HTML Editing (6)

1. `HtmlEditorControl.cs` - Multiple TODOs for event handling and rendering
2. `HtmlStyleHelper.cs` - Style application improvements
3. Various element behavior classes

### Commands (10)

1. `CommandToolsMenu.cs:30, 42`
2. `CommandOpenRecentPosts.cs:30, 42`
3. `CommandOpenDrafts.cs:30, 42`
4. `CommandInsertMenu.cs:30, 42`
5. `MarginCommand.cs:34`
6. `CommandClipboardMenu.cs` - Multiple instances

### Blog Client (8)

1. `BloggerAtomClient.cs` - Authentication and API handling
2. `AtomMediaUploader.cs` - Media upload improvements
3. `BlogSettings.cs` - Configuration handling
4. `BackgroundColorDetector.cs` - Browser-based detection

### Core Services (15)

1. `BidiGraphics.cs:6` - Six TODO comments for BiDi text rendering
2. `LiveClipboardData.cs:3` - Three TODOs for clipboard handling
3. `WebPageDownloader.cs` - Browser control usage
4. `UrlHelper.cs` - URL processing
5. `LayoutHelper.cs` - Layout calculations
6. `Exceptor.cs` - Exception handling
7. Various diagnostics and utility classes

### Post Property Editing (3)

`TreeCategorySelector.cs` - Three TODOs for category selection UI

### Utilities (5)

1. `User.cs:13` - Constructor logic (BlogServer utility)
2. `BrowseButton.cs` - Button functionality
3. `NewFolderButton.cs` - Folder creation
4. Various control implementations

---

## Recommended Actions

### Immediate (High Priority)

1. **Fix all 9 FIXME comments** - These represent known issues
   - Focus on DPI scaling issues (affects user experience)
   - Review video feature FIXMEs (may affect functionality)
   - Estimated effort: 2-4 weeks

### Short Term (1-2 months)

2. **Audit Ribbon Migration TODOs**
   - Determine completion status
   - Remove obsolete code or update TODOs
   - Estimated effort: 1 week

3. **Review OLW Migration TODOs**
   - Migration is years old, likely complete
   - Remove completed TODOs
   - Estimated effort: 1-2 days

4. **Clean up IE7 TODOs**
   - IE7 is obsolete, evaluate modern compatibility
   - Remove or update comments
   - Estimated effort: 2-3 hours

### Long Term (3-6 months)

5. **Address General Implementation TODOs**
   - Prioritize based on user impact
   - Consider creating GitHub issues for each substantive TODO
   - Estimated effort: Ongoing

6. **Clean up Designer TODOs**
   - Low priority but improves code cleanliness
   - Can be done in bulk with careful review
   - Estimated effort: 1 day

---

## Statistics

**Total:** 167 comments  
**High Priority (FIXME):** 9 (5.4%)  
**Medium Priority (Ribbon + OLW):** 36 (21.6%)  
**Low Priority (Generated + Browser):** 47 (28.1%)  
**General Implementation:** 75 (44.9%)

---

## Tracking

Consider creating GitHub issues for:
- [ ] Each HIGH priority FIXME
- [ ] Ribbon migration completion audit
- [ ] OLW migration completion audit
- [ ] General implementation TODOs (grouped by feature area)

This will enable better tracking and community contribution opportunities.
