# Preview Mode Research

## Executive Summary

This document researches how Preview mode currently works in OpenLiveWriter and what it needs to function properly.

**Date:** January 2026  
**Research Task:** Research how Preview mode works and what it needs

---

## Current Preview Mode Implementation

### Overview

OpenLiveWriter's Preview mode allows users to see how their blog post will appear when published to their blog, using the blog's actual styling and layout (templates).

**Key Concept:** Preview mode renders the post content within the blog's editing template, which is downloaded from the blog during configuration.

---

## Architecture & Components

### 1. EditingMode Enumeration

**Location:** `src/managed/OpenLiveWriter.PostEditor/ContentEditor/IContentEditorFactory.cs`

```csharp
public enum EditingMode
{
    Wysiwyg = 20,      // Visual HTML editor
    Source = 21,       // HTML source code view
    PlainText = 25,    // Plain text editor
    Preview = 26       // Blog preview with template
}
```

### 2. Command Infrastructure

**CommandViewWebPreview** (`src/managed/OpenLiveWriter.PostEditor/PostHtmlEditing/Commands/CommandViewWebPreview.cs`)
- Menu path: `&View@2/&Web Preview@103`
- Keyboard shortcut: `F12`
- Context menu: `&Web Preview@100`
- Command identifier: `"OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ViewPreview"`

**Integration Points:**
- Registered in ContentEditor command manager
- Latched state indicates Preview mode is active
- Enabled based on `GlobalEditorOptions.SupportsFeature(ContentEditorFeature.PreviewMode)`

### 3. ContentEditor Implementation

**Location:** `src/managed/OpenLiveWriter.PostEditor/ContentEditor/ContentEditor.cs`

**Key Methods:**

```csharp
public void ChangeToPreviewMode()
{
    _htmlEditorSidebarHost.Visible = false;  // Hide sidebar
    ChangeEditor(EditingMode.Preview, false); // Switch to preview, non-editable
}
```

**Mode Switching Logic:**
- Cleans up items from previous editing mode (`DisposeItemsOnEditorChange()`)
- Preserves dirty state across mode changes
- Sets current editing mode to `EditingMode.Preview`
- Manages command availability for the mode
- Calls `SetEditable(false)` on the normal HTML editor
- Calls `SetCurrentEditor()` to activate the appropriate view

**Editor Selection:**
```csharp
private void SetCurrentEditor()
{
    switch (CurrentEditingMode)
    {
        case EditingMode.Wysiwyg:
            ChangeToWysiwygMode();
            break;
        case EditingMode.Preview:
            ChangeToPreviewMode();
            break;
        case EditingMode.Source:
            ChangeToCodeMode();
            break;
        case EditingMode.PlainText:
            ChangeToPlainTextMode();
            break;
    }
}
```

**Command Availability in Preview Mode:**
- No contextual tabs (smart content, video, etc.)
- Most editing commands disabled
- View/navigation commands remain active
- Spell checking disabled

### 4. Template System

#### BlogEditingTemplate

**Location:** `src/managed/OpenLiveWriter.BlogClient/Detection/BlogEditingTemplate*.cs`

**Purpose:** Manages blog editing templates that define how content appears in preview mode.

**Template Types:**
```csharp
public enum BlogEditingTemplateType
{
    Normal,   // Basic template without framing
    Styled,   // Template with blog CSS styles
    Framed,   // Full blog page frame (header, sidebar, footer)
    Webpage   // Complete webpage template
}
```

**Template Files:**
- Stored in blog-specific directory: `BlogEditingTemplate.GetBlogTemplateDir(blogId)`
- Multiple template files can exist per blog (Normal, Styled, Framed, Webpage)
- Each template is an HTML file with placeholders for content

#### BlogEditingTemplateDetector

**Location:** `src/managed/OpenLiveWriter.BlogClient/Detection/BlogEditingTemplateDetector.cs`

**Purpose:** Automatically downloads and creates blog editing templates.

**Detection Process:**

1. **Publish Temporary Post:**
   - Creates a temporary blog post with marker content
   - Publishes to the blog using blog API
   - Records the post ID and permalink

2. **Download Published Post:**
   - Fetches the published post's HTML from the blog's website
   - Downloads complete webpage with all styling

3. **Parse Template:**
   - Extracts the blog's HTML structure (header, footer, navigation)
   - Identifies the content area (where post body appears)
   - Captures CSS styles, JavaScript, and other assets
   - Creates placeholders for dynamic content

4. **Save Template Files:**
   - Generates multiple template variants (Normal, Styled, Framed)
   - Stores in local template directory
   - Records post body background color
   - Saves template metadata

5. **Delete Temporary Post:**
   - Removes the temporary post from the blog
   - Cleans up test content

**Template Strategy:**
- `BlogPostHtmlEditorControl.TemplateStrategy` determines which template to use
- Strategies can be: UseBodyBackgroundColor, PreserveBodyBackgroundColor, etc.

#### WriterEditingManifest

**Location:** `src/managed/OpenLiveWriter.BlogClient/Detection/WriterEditingManifest.cs`

**Purpose:** Provides blog-specific editing configuration via `wlwmanifest.xml`.

**Manifest Discovery:**
1. Check for `wlwmanifest.xml` at blog root (by convention)
2. Parse blog homepage for `<link>` tag pointing to manifest
3. Download manifest XML if found

**Manifest Contents:**
- Blog service information
- Template download URLs
- Custom buttons/services
- Image upload settings
- API capabilities
- Preferred editing options

**Template Download from Manifest:**
- Manifest can specify template URLs instead of auto-detection
- Templates downloaded directly from specified locations
- Avoids need to publish temporary post
- Faster configuration, more reliable

**Caching:**
- Manifest includes `Expires` header for caching
- `LastModified` and `ETag` for conditional requests
- Avoids repeated downloads

### 5. HTML Rendering in Preview Mode

**Template Application Process:**

1. **Get Current Post Content:**
   - Extract HTML from the editor
   - Include post title and body

2. **Load Template:**
   - Read template file for current blog
   - Template contains HTML structure with placeholders

3. **Replace Placeholders:**
   - `{post-title}` → Actual post title
   - `{post-body}` → Post HTML content
   - `{post-date}` → Current date/time
   - Other blog-specific variables

4. **Render in Editor:**
   - Load complete HTML into MSHTML control
   - Apply blog's CSS stylesheets
   - Execute blog's JavaScript (if allowed)
   - Render images, embeds, etc.

5. **Make Non-Editable:**
   - Set MSHTML to non-editable mode
   - Prevent user modifications in preview
   - Disable spell checking
   - Hide editing UI elements

**Template HTML Structure (Example):**
```html
<!DOCTYPE html>
<html>
<head>
    <title>{post-title}</title>
    <link rel="stylesheet" href="blog-styles.css">
    <!-- Blog's CSS and scripts -->
</head>
<body>
    <div id="header">
        <!-- Blog header -->
    </div>
    <div id="content">
        <h1 class="post-title">{post-title}</h1>
        <div class="post-meta">{post-date}</div>
        <div class="post-body">
            {post-body}
        </div>
    </div>
    <div id="sidebar">
        <!-- Blog sidebar -->
    </div>
    <div id="footer">
        <!-- Blog footer -->
    </div>
</body>
</html>
```

### 6. Preview Mode Limitations

**Current Limitations:**

1. **Template Availability:**
   - Preview only works if template was successfully downloaded
   - Not all blogs support template detection
   - Some blogs block automated template extraction
   - Manifest may not be available

2. **Dynamic Content:**
   - Server-side rendering (PHP, ASP.NET) not executed
   - Database queries don't run
   - Dynamic sidebars/widgets show static snapshot
   - Comments not visible
   - Trackbacks/pingbacks not shown

3. **JavaScript Execution:**
   - May be restricted for security reasons
   - Blog's JavaScript may not work correctly in MSHTML
   - Analytics/tracking scripts won't function
   - Interactive widgets may fail

4. **Styling Accuracy:**
   - CSS rendering differences between MSHTML and blog's browser
   - Missing fonts
   - External resources may not load
   - Responsive design may not work (fixed viewport)

5. **Content Restrictions:**
   - Only shows post title and body
   - Categories/tags not rendered
   - Custom fields not shown
   - Post metadata not displayed

---

## What Preview Mode Needs

### Essential Requirements

#### 1. Blog Editing Template
**Must Have:**
- Valid HTML template file for the blog
- Template stored in blog-specific directory
- Placeholders for content injection
- CSS stylesheets (inline or linked)

**Acquisition Methods:**
- Auto-detection via temporary post publishing
- Manual download from manifest URL
- User-provided template file
- Fallback to generic template

#### 2. Content Transformation
**Components:**
- Post title and body extraction
- HTML sanitization (remove editing artifacts)
- Placeholder replacement
- URL resolution (relative → absolute)

#### 3. Rendering Engine
**Requirements:**
- HTML/CSS rendering capability (currently MSHTML)
- JavaScript execution (optional, security-restricted)
- Image loading
- CSS stylesheet processing

#### 4. Template Management
**Infrastructure:**
- Template storage and versioning
- Template update mechanism (manual or automatic)
- Multiple template type support
- Template validation

### Optional Enhancements

#### 1. Live Preview
- Real-time updates as user types
- Debounced rendering (avoid performance issues)
- Background template application

#### 2. Multiple Template Views
- Switch between Normal/Styled/Framed templates
- Quick template comparison
- User preference for default template

#### 3. Responsive Preview
- Different viewport sizes (desktop, tablet, mobile)
- Simulate different screen resolutions
- CSS media query testing

#### 4. Template Customization
- User-editable templates
- Custom CSS overrides
- Template variable configuration

---

## Preview Mode Workflow

### User Perspective

```
1. User clicks "Web Preview" (F12) or View > Web Preview
   ↓
2. OpenLiveWriter switches to Preview mode
   ↓
3. Content is rendered with blog template
   ↓
4. User sees post as it will appear on blog
   ↓
5. User clicks "Edit" or switches mode to return to editing
```

### Technical Flow

```
User triggers commandViewWebPreview.Execute
   ↓
ContentEditor.ChangeToPreviewMode()
   ├─ Hide sidebar
   ├─ Set EditingMode.Preview
   └─ SetEditable(false)
   
   ↓
BlogPostHtmlEditorControl applies template
   ├─ Load template file for current blog
   ├─ Get post title and content
   ├─ Replace template placeholders
   ├─ Generate complete HTML
   └─ Render in MSHTML control
   
   ↓
User views preview
   
   ↓
User switches back to Edit mode
   ├─ SetEditable(true)
   ├─ Restore editor state
   └─ Show sidebar
```

### Template Acquisition Flow

```
Blog Configuration
   ↓
BlogEditingTemplateDetector.DetectTemplate()
   ├─ Check for WriterEditingManifest
   │  ├─ Try wlwmanifest.xml at blog root
   │  └─ Parse homepage for manifest link
   │
   ├─ If manifest has template URL:
   │  └─ Download template from URL
   │
   └─ If no manifest template:
      ├─ Publish temporary post to blog
      ├─ Download published post HTML
      ├─ Parse and extract template structure
      ├─ Generate template files (Normal, Styled, Framed)
      └─ Delete temporary post
   
   ↓
Save template to BlogEditingTemplate directory
   
   ↓
Preview mode uses saved template
```

---

## Migration Considerations for WebView2

### If Replacing MSHTML with WebView2

**Impact on Preview Mode:**

1. **Rendering Engine Change:**
   - MSHTML → Chromium/Blink rendering
   - Better CSS3 support
   - Better JavaScript compatibility
   - Modern web standards compliance

2. **Template Compatibility:**
   - Existing templates will likely render better
   - Need to test all template types
   - May need template format updates
   - CSS vendor prefixes may change

3. **JavaScript Execution:**
   - More reliable script execution
   - Better debugging tools
   - Security model differences
   - May need to adjust script restrictions

4. **Performance:**
   - Potentially faster rendering
   - Better memory management
   - Async operations supported

**Required Changes:**

1. **Template Rendering:**
   - Port template application logic to WebView2
   - Use `CoreWebView2.NavigateToString()` for template HTML
   - Handle resource loading (images, CSS, scripts)

2. **Non-Editable Mode:**
   - Disable `contenteditable` in preview
   - Prevent user interactions via JavaScript
   - Handle clicks/navigation

3. **Template Storage:**
   - Same local file storage
   - May need different file path handling
   - Resource URL resolution changes

**Benefits of WebView2 for Preview:**
- More accurate rendering (matches modern blogs)
- Better CSS/JavaScript support
- Responsive design testing
- DevTools integration for debugging

---

## Recommendations

### Short-Term (Current MSHTML Implementation)

1. **Improve Template Detection:**
   - Better error handling for failed detection
   - Fallback to generic templates
   - User notification of template issues

2. **Template Refresh:**
   - Allow manual template re-download
   - Detect when blog theme changes
   - Provide template update notifications

3. **Documentation:**
   - Document template requirements
   - Provide troubleshooting guide
   - Explain preview limitations

### Medium-Term (WebView2 Migration)

1. **Port Preview to WebView2:**
   - Reimplement template rendering
   - Test with various blog platforms
   - Ensure backward compatibility with existing templates

2. **Enhanced Preview Features:**
   - Live preview option
   - Multiple viewport sizes
   - Better JavaScript support

3. **Template Improvements:**
   - Support for modern blog themes
   - Better CSS rendering
   - Responsive design preview

### Long-Term Enhancements

1. **Advanced Preview:**
   - Real-time collaboration preview
   - Multi-device preview
   - A/B testing view

2. **Template Marketplace:**
   - Share custom templates
   - Download community templates
   - Template editing tools

3. **Integration:**
   - Preview with actual blog data (via API)
   - Comments preview
   - Related posts preview

---

## Technical References

### Current Implementation Files
- `src/managed/OpenLiveWriter.PostEditor/ContentEditor/ContentEditor.cs`
- `src/managed/OpenLiveWriter.PostEditor/PostHtmlEditing/Commands/CommandViewWebPreview.cs`
- `src/managed/OpenLiveWriter.BlogClient/Detection/BlogEditingTemplateDetector.cs`
- `src/managed/OpenLiveWriter.BlogClient/Detection/BlogEditingTemplate.cs`
- `src/managed/OpenLiveWriter.BlogClient/Detection/WriterEditingManifest.cs`

### Related Components
- `BlogPostHtmlEditorControl` - Main editor control
- `MshtmlOptions` - MSHTML configuration
- `BlogEditingTemplateType` - Template type enumeration
- `BlogAccount` - Blog configuration

### External Standards
- wlwmanifest.xml schema (Windows Live Writer manifest)
- Blog template placeholders convention
- HTML/CSS rendering standards

---

## Conclusion

Preview mode in OpenLiveWriter provides a valuable feature for users to see how their posts will appear when published. It relies on:

1. **Blog editing templates** downloaded during configuration
2. **Template application** system to inject content
3. **MSHTML rendering** engine (currently)
4. **Mode switching** infrastructure in ContentEditor

**Key Requirements:**
- Valid blog template files
- Placeholder replacement mechanism
- HTML rendering engine
- Non-editable view capability

**Future Direction:**
- Migration to WebView2 will improve rendering accuracy
- Better CSS3 and JavaScript support
- Potential for enhanced preview features (responsive, live, etc.)

Preview mode is well-architected and should transition smoothly to WebView2 with improved capabilities.

---

**End of Research Document**
