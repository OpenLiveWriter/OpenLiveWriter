# OpenLiveWriter Research Documentation

This directory contains comprehensive research documentation for various aspects of the OpenLiveWriter codebase.

**Created:** January 2026  
**Purpose:** Technical research and documentation to support development decisions

---

## Available Research Documents

### 1. WebView2 Spell-Check Research
**File:** [webview2-spellcheck-research.md](./webview2-spellcheck-research.md)

**Summary:**
Investigates WebView2's built-in spell-checking capabilities and evaluates integration options for OpenLiveWriter. Compares current Windows Platform Spell Check API implementation with WebView2's native spell-check features.

**Key Topics:**
- Current spell-check architecture in OpenLiveWriter
- WebView2 built-in spell-check capabilities and limitations
- API availability and community feedback
- Integration options (native, hybrid, JavaScript libraries)
- Migration recommendations and tradeoffs

**Key Findings:**
- WebView2 has functional built-in spell-check but **no programmatic API** (as of Jan 2026)
- Language selection tied to OS settings, not programmatically controllable
- Recommendation: Use WebView2 native spell-check for simplicity, monitor API development
- Hybrid approach possible if programmatic control is critical

---

### 2. Preview Mode Research
**File:** [preview-mode-research.md](./preview-mode-research.md)

**Summary:**
Documents how Preview mode currently works in OpenLiveWriter and what components it requires to function properly.

**Key Topics:**
- Preview mode architecture and command infrastructure
- Blog editing template system and types
- Template detection and download process
- WriterEditingManifest (wlwmanifest.xml)
- Template application and HTML rendering
- Migration considerations for WebView2

**Key Findings:**
- Preview mode renders posts using blog-specific templates downloaded during configuration
- Template detection via temporary post publishing or manifest URL
- Four template types: Normal, Styled, Framed, Webpage
- WebView2 migration will improve rendering accuracy (Chromium vs MSHTML)
- Template system is well-architected and extensible

---

### 3. Image Upload/Publish Flow
**File:** [image-upload-publish-flow.md](./image-upload-publish-flow.md)

**Summary:**
Complete end-to-end mapping of the image upload and publishing workflow in OpenLiveWriter, from initial insertion through final publishing to the blog.

**Key Topics:**
- Image insertion entry points and user workflow
- Image registration and async initialization
- Supporting file management and storage
- Image processing, decorators, and optimization
- Upload mechanisms (WeblogBlogFileUploader, FTPBlogFileUploader)
- URL transformation and reference fixing
- Publishing coordination and error handling

**Key Findings:**
- Sophisticated multi-phase process with three-tier storage (Source, Inline, Linked)
- Shadow files for draft editing without affecting originals
- Pluggable decorator system for borders, effects, resizing
- Multiple upload destinations: Blog API, FTP, Image Services
- Smart upload tracking prevents duplicates
- Flexible filename formatting with template variables

---

## How to Use This Documentation

### For Developers

**Understanding Current Architecture:**
1. Read the relevant research document for your area of work
2. Review the "Current Implementation" sections for architecture details
3. Check "Technical References" for specific source file locations

**Planning Changes:**
1. Review "Migration Considerations" sections
2. Evaluate "Recommendations" for suggested approaches
3. Consider "Extensibility Points" for plugin development

**WebView2 Migration:**
1. Start with spell-check and preview mode research
2. Understand current MSHTML dependencies
3. Plan migration using documented recommendations
4. Test with existing templates and workflows

### For Project Managers

**Decision Support:**
- Each document includes "Recommendations" with short/medium/long-term suggestions
- "Key Findings" provide executive summary of research
- Trade-off analysis helps prioritize features

**Risk Assessment:**
- "Limitations" sections highlight current constraints
- "Migration Considerations" identify potential issues
- "Error Handling" sections document failure scenarios

### For Contributors

**Onboarding:**
- Use as architecture reference
- Understand component relationships
- Learn workflow sequences

**Feature Development:**
- Check "Extensibility Points" for plugin opportunities
- Review "Recommendations" for future enhancements
- Understand data structures and interfaces

---

## Research Methodology

### How This Documentation Was Created

1. **Code Exploration:**
   - Examined source files in key directories
   - Traced code execution paths
   - Analyzed class hierarchies and interfaces

2. **Component Analysis:**
   - Identified major subsystems
   - Documented component relationships
   - Mapped data flows and state transitions

3. **External Research:**
   - WebView2 official documentation
   - Community feedback and GitHub issues
   - Web standards (HTML5 spell-check, ATOM, XML-RPC)

4. **Integration Mapping:**
   - Documented how components interact
   - Created workflow diagrams
   - Identified integration points

### Documentation Standards

Each research document follows a consistent structure:

- **Executive Summary** - Overview and key findings
- **Current Implementation** - Detailed architecture documentation
- **Component Details** - Deep dives into major subsystems
- **Workflows** - Step-by-step processes with diagrams
- **Recommendations** - Actionable suggestions for improvement
- **Technical References** - Source file locations and external links
- **Conclusion** - Summary and decision support

---

## Document Maintenance

### Updating Documentation

**When to Update:**
- After major architecture changes
- When migrating to new technologies (e.g., WebView2)
- When community feedback provides new information
- When APIs become available (e.g., WebView2 spell-check API)

**How to Update:**
1. Update relevant sections with new information
2. Add "Updated: [date]" note to Executive Summary
3. Maintain backward compatibility notes
4. Update recommendations based on new capabilities

### Contributing New Research

**Guidelines for New Documents:**
1. Follow the established structure
2. Include executive summary with key findings
3. Document both current state and recommendations
4. Provide technical references and external links
5. Include diagrams/workflows where helpful
6. Focus on actionable information

**Topics for Future Research:**
- Plugin architecture and extensibility
- Content source system
- BlogClient architecture and providers
- PostEditor state management
- Smart content framework
- Internationalization and localization

---

## Quick Reference

### Source Code Directories

| Area | Primary Location |
|------|------------------|
| Spell Checker | `src/managed/OpenLiveWriter.SpellChecker/` |
| Content Editor | `src/managed/OpenLiveWriter.PostEditor/ContentEditor/` |
| Image Processing | `src/managed/OpenLiveWriter.PostEditor/ImageInsertion/` |
| Blog Client | `src/managed/OpenLiveWriter.BlogClient/` |
| Template Detection | `src/managed/OpenLiveWriter.BlogClient/Detection/` |
| Extensibility | `src/managed/OpenLiveWriter.Extensibility/` |

### Key Interfaces

| Interface | Purpose | Documentation |
|-----------|---------|---------------|
| `ISpellingChecker` | Spell checking | [webview2-spellcheck-research.md](./webview2-spellcheck-research.md) |
| `ISupportingFileService` | Supporting files | [image-upload-publish-flow.md](./image-upload-publish-flow.md) |
| `IBlogClient` | Blog API access | [image-upload-publish-flow.md](./image-upload-publish-flow.md) |
| `IImageServiceUploader` | Image hosting | [image-upload-publish-flow.md](./image-upload-publish-flow.md) |

### Common Workflows

| Workflow | Documentation |
|----------|---------------|
| Inserting an image | [image-upload-publish-flow.md](./image-upload-publish-flow.md#phase-1-image-insertion) |
| Publishing a post | [image-upload-publish-flow.md](./image-upload-publish-flow.md#phase-5-publishing---file-upload) |
| Previewing a post | [preview-mode-research.md](./preview-mode-research.md#preview-mode-workflow) |
| Spell checking | [webview2-spellcheck-research.md](./webview2-spellcheck-research.md#current-spell-check-implementation-in-openlivewriter) |

---

## Related Documentation

### Official OpenLiveWriter Docs
- [README.md](../../README.md) - Project overview
- [CONTRIBUTING.md](../../CONTRIBUTING.md) - Contribution guidelines
- [roadmap.md](../../roadmap.md) - Project roadmap

### External Resources
- [WebView2 Documentation](https://learn.microsoft.com/en-us/microsoft-edge/webview2/webview2-api-reference)
- [Windows Spell Check API](https://learn.microsoft.com/en-us/windows/win32/api/_intl/)

---

## Contact & Feedback

For questions about this research or to request additional documentation:

1. Open an issue on GitHub
2. Tag with `documentation` label
3. Reference the specific research document

For corrections or updates to existing research:

1. Submit a pull request with changes
2. Explain the reason for the update
3. Include sources for new information

---

**Last Updated:** January 18, 2026
