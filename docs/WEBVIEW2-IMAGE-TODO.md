# WebView2 Image Editing - Remaining Work

**Status:** ~25% complete  
**Last Updated:** 2026-01-20

## ✅ Completed

- [x] Image click selection → Picture Tools ribbon activates
- [x] Width/height spinners update DOM attributes
- [x] Aspect ratio lock works
- [x] Link to Source Picture wraps image with `<a>` tag
- [x] No Link removes anchor wrapper
- [x] Selection abstraction layer (IEditorSelection, ISelectedImage, IHtmlImageElement)
- [x] WebView2 bridge for DOM manipulation
- [x] TaskDialog crash fix (replaced with MessageBox)
- [x] PropVariant x64 fix for ribbon spinners

---

## 🔴 Priority 1: Core Functionality (Must Have)

### 1.1 Actual File Resize
**Complexity:** High | **Impact:** Critical

Currently spinners only change HTML width/height attributes (browser scales image). Need actual bitmap resize.

**What needs to happen:**
- Wire `HtmlImageResizeDecorator.Decorate()` to WebView2 path
- Load original image file from `BlogPostImageData.ImageSourceFile`
- Resize bitmap using existing `ImageHelper` code
- Write resized image to temp file
- Update `<img src>` to point to resized file
- Track in `BlogPostImageData.InlineImageFile`

**Key files:**
- `HtmlImageResizeDecorator.cs` - decorator logic
- `ImageEditingPropertyHandler.cs` - `UpdateImageSource()` method
- `ImageInsertHandler.cs` - `WriteImages()` for file output
- `BlogPostImageData.cs` - file tracking

### 1.2 BlogPostImageData Integration
**Complexity:** High | **Impact:** Critical

The supporting files system tracks original, inline, and linked image files. WebView2 needs to hook into this.

**What needs to happen:**
- `GetImagePropertiesInfo()` must look up `BlogPostImageData` by src URI
- Create proper `ImagePropertiesInfo` with all decorator settings
- Wire `ApplyImageDecorations()` to trigger full decorator pipeline

**Key files:**
- `ImageEditingPropertyHandler.cs` - `GetImagePropertiesInfo()`
- `BlogPostImageDataList.cs` - image tracking
- `ImagePropertiesInfo.cs` - decorator settings bag

### 1.3 Image Properties Dialog
**Complexity:** Medium | **Impact:** High

Double-click on image should open properties dialog.

**What needs to happen:**
- Handle `controlDoubleClick` JS message (already sent)
- Wire to `ImagePropertiesForm` or equivalent
- Support alt text, title, link URL editing
- Apply changes back to DOM

**Key files:**
- `WebView2BlogPostHtmlEditorControl.cs` - handle double-click message
- `ImagePropertiesForm.cs` - dialog UI

---

## 🟡 Priority 2: Important Features

### 2.1 Alignment & Margins
**Complexity:** Medium | **Impact:** High

Left/Center/Right/Inline alignment and margin controls.

**What needs to happen:**
- Wire `AlignmentCommand` to WebView2 path
- Wire `MarginCommand` to WebView2 path
- Apply styles via `IHtmlImageElement.SetStyle()`

**Key files:**
- `HtmlAlignDecoratorSettings.cs`
- `HtmlMarginDecorator.cs`
- `ImagePropertiesSidebarControl.cs` - command handlers

### 2.2 Preset Sizes (Small/Medium/Large/Original)
**Complexity:** Medium | **Impact:** Medium

The dropdown with preset sizes is currently stubbed.

**What needs to happen:**
- Implement `customSizeCommand_Execute()` for WebView2
- Calculate dimensions based on natural size
- Apply via `SetDimensionsAsync()`

**Key files:**
- `ImagePropertiesSidebarControl.cs` - `customSizeCommand_Execute()`

### 2.3 Sidebar Panel UI
**Complexity:** Medium | **Impact:** Medium

The right-side image properties panel isn't showing (`Visible=False` in logs).

**What needs to happen:**
- Debug why `ActiveSidebarControl` isn't being set visible
- Ensure `ImagePropertiesSidebarHostControl` shows for WebView2
- Wire property changes to DOM

**Key files:**
- `HtmlEditorSidebarHost.cs` - `UpdateVisibility()`
- `ImagePropertiesSidebarHostControl.cs`

---

## 🟢 Priority 3: Nice to Have

### 3.1 Border Decorator
- Wire `HtmlBorderDecorator` to WebView2
- Apply border styles via `SetStyle()`

### 3.2 Effects (Tilt, Watermark, etc.)
- These modify the actual image file
- Need full decorator pipeline working first

### 3.3 Crop
- Complex UI with drag handles
- May need significant JS work

### 3.4 Rotate CW/CCW
- Modify image file
- Update dimensions

---

## 🔵 Priority 4: Polish & Verification

### 4.1 Round-trip Persistence
- Save post → reopen → verify decorators preserved
- Check `ImageDecoratorsList` serialization

### 4.2 Publish with Correct Images
- Verify resized images upload correctly
- Verify linked images (full-res) upload

### 4.3 Undo/Redo
- Image operations should be undoable
- May need `IUndoUnit` integration

---

## Architecture Notes

### Decorator Pipeline Flow
```
User changes property
    ↓
ImagePropertiesSidebarControl handles command
    ↓
ImagePropertiesInfo.InlineImageWidth = newValue
    ↓
ApplyImageDecorations(ImagePropertyType.InlineSize)
    ↓
ImageEditingPropertyHandler.UpdateImageSource()
    ↓
ImageInsertHandler.WriteImages()
    ↓
  - Load original bitmap
  - Apply each decorator in order
  - Write inline image (maybe _thumb)
  - Write linked image (if link-to-source)
    ↓
Update img src to new file path
```

### Key Insight
The decorators themselves work on **bitmaps** and don't need MSHTML. The MSHTML dependency is only in:
1. Creating `ImagePropertiesInfo` from DOM element
2. Updating DOM element after decoration

We've abstracted #2 with `IHtmlImageElement`. Need to complete #1 with proper `BlogPostImageData` lookup.

---

## Estimated Effort

| Item | Complexity | Est. Hours |
|------|------------|------------|
| 1.1 Actual File Resize | High | 6-8 |
| 1.2 BlogPostImageData Integration | High | 4-6 |
| 1.3 Image Properties Dialog | Medium | 3-4 |
| 2.1 Alignment & Margins | Medium | 2-3 |
| 2.2 Preset Sizes | Medium | 1-2 |
| 2.3 Sidebar Panel UI | Medium | 2-3 |
| 3.x Effects & Borders | Low-Med | 4-6 |
| 4.x Polish | Low | 2-3 |

**Total: ~25-35 hours remaining**

---

## Next Steps

1. **Start with 1.2** - BlogPostImageData integration, since 1.1 depends on it
2. **Then 1.1** - Actual file resize (the big win)
3. **Then 1.3** - Properties dialog for usability
4. **Then 2.x** - Fill in remaining UI features
