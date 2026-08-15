# Markdown Support Design

**Date:** 2026-08-15  
**Branch:** `develop/markdown` (from `origin/develop/macos` Avalonia/WebView stack)  
**Status:** Approved for implementation (Approach 1)

## Summary

Add GFM Markdown as a per-blog editing format on the cross-platform Avalonia editor. Markdown is the on-disk source of truth for Markdown blogs. Design (WYSIWYG) remains editable and syncs via HTML↔Markdown conversion at view/save boundaries. A separate per-blog preference controls whether publish sends Markdown or HTML (default HTML). Font family/size are disabled in Markdown mode with an explanatory tooltip; paragraph and heading styles remain available.

## Goals

1. Source tab edits **Markdown** (GFM) when the active blog is in Markdown mode.
2. Design/WYSIWYG **renders and edits** that content (round-trip via HTML).
3. Settings (account/weblog) can set **editing format** to Markdown.
4. Separate setting: **publish as Markdown** or **publish as HTML** (convert on publish). Default: HTML.
5. In Markdown mode: **font family/size greyed out** with hover text explaining why; **paragraph/heading styles still work**.
6. Strong **unit + integration** tests to prevent regressions.
7. Shared C# library usable by Mac and Windows Avalonia builds.

## Non-goals (v1)

- Classic WinForms/MSHTML (`master`) port.
- Dual-pane live split view.
- Per-post format override (format is per-blog only).
- Inventing Markdown equivalents for every plugin; unknown HTML is preserved as raw HTML blocks.
- Perfect lossless round-trip for arbitrary HTML (best-effort with a tested golden suite).

## Locked decisions

| Topic | Decision |
|-------|----------|
| Source of truth | Markdown body when blog editing format is Markdown |
| Format scope | Per-blog only |
| Design mode | Editable WYSIWYG; convert at boundaries |
| Publish format | Separate per-blog setting; default **HTML** |
| Dialect | GitHub Flavored Markdown (GFM) |
| HTML→Markdown migration | One-time conversion prompt when enabling Markdown on a blog |
| Platform base | Single branch from Avalonia/WebView stack |
| Plugins/embeds | Core Markdown + passthrough HTML blocks |
| Architecture | Shared C# Markdown service; convert on boundaries (Approach 1) |

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ Avalonia shell (EditorPanel, AccountDialog, MainWindow)     │
└─────────────┬───────────────────────────────┬───────────────┘
              │                               │
              ▼                               ▼
┌─────────────────────────┐     ┌─────────────────────────────┐
│ OpenLiveWriter.Markdown │     │ OpenLiveWriter.Publishing   │
│  MarkdownService        │◄────│  PostDocument, BlogAccount  │
│  GfmPipeline (Markdig)  │     │  EditorContentPublisher     │
│  HtmlToMarkdown         │     └─────────────────────────────┘
└─────────────────────────┘
```

### Components

1. **`OpenLiveWriter.Markdown`** (new `net10.0` library)  
   - `IMarkdownService` / `MarkdownService`  
   - `ToHtml(markdown)` — Markdig with GFM extensions  
   - `ToMarkdown(html)` — HTML→GFM with raw HTML passthrough for unknown nodes  
   - `IsEffectivelyEmpty`, normalization helpers  
   - No UI dependencies

2. **`OpenLiveWriter.Markdown.Tests`** (new NUnit project)  
   - Golden-file round-trips, GFM fixtures, publish conversion cases

3. **Publishing model**  
   - `BlogAccount.EditingFormat`: `Html` | `Markdown` (default `Html`)  
   - `BlogAccount.PublishFormat`: `Html` | `Markdown` (default `Html`)  
   - `PostDocument.BodyFormat`: `Html` | `Markdown`  
   - `PostDocument.BodyHtml` remains the persisted body field for HTML posts  
   - `PostDocument.BodyMarkdown` holds canonical Markdown when `BodyFormat == Markdown`  
   - When Markdown: Design loads `ToHtml(BodyMarkdown)`; save/switch persists `ToMarkdown(editorHtml)` into `BodyMarkdown`

4. **Editor (`EditorPanel`)**  
   - If blog/document is Markdown:  
     - Source view shows `BodyMarkdown` (AvaloniaEdit; Markdown-oriented coloring optional v1)  
     - Enter Design: `SetContentAsync(ToHtml(markdown))`  
     - Leave Design / save: pull HTML → `ToMarkdown` → update `BodyMarkdown`  
     - Preview: render HTML from Markdown (existing preview path on converted HTML)  
   - Heading/paragraph commands stay enabled; apply block format in Design, then convert back to MD on save  
   - Font family/size commands disabled; tooltip: *"Font family and size are not available in Markdown mode because Markdown does not encode visual fonts."*

5. **Settings UI (`AccountDialog` / account editor)**  
   - Combo: Content format — HTML | Markdown  
   - Combo: Publish as — HTML | Markdown (enabled when editing format is Markdown; when editing is HTML, publish is always HTML)  
   - On switching a blog HTML → Markdown: modal offering one-time conversion of existing local drafts for that blog (Yes convert / Keep as HTML / Cancel)

6. **Publish (`EditorContentPublisher` / `MainWindow.Publishing`)**  
   - Resolve publish body:  
     - If `PublishFormat == Html` and body is Markdown → `ToHtml` then existing trim/scrub/image pipeline  
     - If `PublishFormat == Markdown` → send Markdown string as post contents (after image URL rewrite where applicable)  
   - Image upload still runs against HTML representation when publishing as HTML; for Markdown publish, rewrite `![](file/data)` to hosted URLs in the Markdown text

## Data flow

### Edit session (Markdown blog)

1. Open draft → load `BodyMarkdown`  
2. Design: show `ToHtml(BodyMarkdown)` in WebView (editable)  
3. Source: show raw Markdown  
4. Switch Design → Source: HTML → `ToMarkdown` → Source text  
5. Switch Source → Design: Source text → `ToHtml` → WebView  
6. Save: canonical `BodyMarkdown` + `BodyFormat = Markdown`

### Publish

1. Read canonical body + `PublishFormat`  
2. Convert if needed  
3. Existing image upload + MetaWeblog/WordPress path

### Enable Markdown on blog

1. User sets Editing format = Markdown  
2. Prompt: convert existing local drafts for this blog?  
3. If Yes: for each draft with `BodyFormat == Html`, set `BodyMarkdown = ToMarkdown(BodyHtml)`, `BodyFormat = Markdown`, save  
4. New posts inherit Markdown format from the blog

## Error handling

- Conversion failures: keep previous canonical body; show non-destructive error toast/dialog  
- Empty Markdown ↔ empty HTML treated as equivalent  
- Malformed Markdown still renders best-effort via Markdig

## Testing strategy

| Layer | Coverage |
|-------|----------|
| Unit | `ToHtml` / `ToMarkdown` for headings, paragraphs, emphasis, lists, links, images, GFM tables/strikethrough/task lists, HTML passthrough, `<!--more-->` preservation |
| Unit | `BlogAccount` defaults; `PostDocument` serialization with both formats |
| Unit | Publish body selection (MD→HTML vs raw MD) |
| Integration | EditorPanel view switch Markdown↔HTML (headless where possible); draft save/load round-trip |
| Regression | Golden files under `OpenLiveWriter.Markdown.Tests/Fixtures/` |

## Risks & mitigations

| Risk | Mitigation |
|------|------------|
| Lossy HTML→MD | Golden tests; passthrough raw HTML for unknown constructs |
| Plugin HTML | Preserve as HTML blocks in Markdown |
| Font UX confusion | Disabled controls + explicit tooltip |
| Image publish in MD mode | Normalize to HTML for upload rewrite, or MD-aware URL rewrite; tested |

## Rollout

1. Library + tests  
2. Model + settings + conversion prompt  
3. Editor Source/Design sync  
4. Publish path  
5. Font/style command gating  
6. Push `develop/markdown`; CI `dotnet test` for Markdown + Publishing + Platform tests
