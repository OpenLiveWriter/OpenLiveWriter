# Markdown Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add GFM Markdown editing and publish options to the Avalonia Open Live Writer stack on `develop/markdown`.

**Architecture:** Shared `OpenLiveWriter.Markdown` library (Markdig GFM + HTML→Markdown with raw HTML passthrough). `BlogAccount` stores editing/publish formats. `PostDocument` stores `BodyMarkdown` when format is Markdown. `EditorPanel` converts at Source/Design boundaries. Publish converts when `PublishFormat == Html`.

**Tech Stack:** .NET 10, Markdig, AngleSharp (already in CPM), NUnit, Avalonia EditorPanel/WebViewEditor, OpenLiveWriter.Publishing

**Spec:** `docs/superpowers/specs/2026-08-15-markdown-support-design.md`

## Global Constraints

- Branch: `develop/markdown` only (worktree at repo `.worktrees/markdown`)
- Target framework: `net10.0`
- Central Package Management: versions only in `src/managed/Directory.Packages.props`
- Dialect: GFM (tables, strikethrough, task lists, autolinks)
- Per-blog format only (no per-post override)
- Default `PublishFormat` = Html when enabling Markdown
- Font family/size disabled in Markdown mode with tooltip explaining Markdown has no visual fonts
- Paragraph/heading styles remain enabled
- Unknown HTML preserved as raw HTML in Markdown
- One-time conversion prompt when switching a blog to Markdown
- TDD: failing test first for each behavior
- Do not modify classic WinForms/MSHTML paths beyond shared Publishing if referenced
- Commit after each task with a focused message

## File map

| Path | Responsibility |
|------|----------------|
| `src/managed/OpenLiveWriter.Markdown/*` | Conversion service |
| `src/managed/OpenLiveWriter.Markdown.Tests/*` | Unit + golden fixtures |
| `src/managed/OpenLiveWriter.Publishing/Accounts/BlogAccount.cs` | EditingFormat, PublishFormat |
| `src/managed/OpenLiveWriter.Publishing/PostDocument.cs` | BodyFormat, BodyMarkdown |
| `src/managed/OpenLiveWriter.Publishing/ContentFormat.cs` | Enum |
| `src/managed/OpenLiveWriter.Publishing/EditorContentPublisher.cs` | Publish body resolution |
| `src/managed/OpenLiveWriter.Publishing/DraftConversion.cs` | Bulk HTML→MD for a blog |
| `src/managed/OpenLiveWriter.App.Avalonia/Dialogs/AccountDialog.cs` | Format settings + prompt |
| `src/managed/OpenLiveWriter.App.Avalonia/Editor/EditorPanel.axaml.cs` | View sync |
| `src/managed/OpenLiveWriter.App.Avalonia/Editor/MarkdownEditingController.cs` | Mode helpers |
| `src/managed/OpenLiveWriter.App.Avalonia/MainWindow.Publishing.cs` | Wire publish formats |
| `src/managed/writer.macOS.slnf` / `writer.Windows.slnf` / `writer.sln` | Project includes |
| `src/managed/Directory.Packages.props` | Markdig version |

---

### Task 1: Markdown library + core conversion tests

**Files:**
- Create: `src/managed/OpenLiveWriter.Markdown/OpenLiveWriter.Markdown.csproj`
- Create: `src/managed/OpenLiveWriter.Markdown/IMarkdownService.cs`
- Create: `src/managed/OpenLiveWriter.Markdown/MarkdownService.cs`
- Create: `src/managed/OpenLiveWriter.Markdown/HtmlToMarkdownConverter.cs`
- Create: `src/managed/OpenLiveWriter.Markdown.Tests/OpenLiveWriter.Markdown.Tests.csproj`
- Create: `src/managed/OpenLiveWriter.Markdown.Tests/MarkdownServiceTests.cs`
- Create: `src/managed/OpenLiveWriter.Markdown.Tests/HtmlToMarkdownTests.cs`
- Create: `src/managed/OpenLiveWriter.Markdown.Tests/Fixtures/*.md` and `*.html` as needed
- Modify: `src/managed/Directory.Packages.props` — add Markdig
- Modify: `src/managed/writer.sln`, `writer.macOS.slnf`, `writer.Windows.slnf` — include new projects

**Interfaces:**
- Produces:
```csharp
public enum ContentFormat { Html = 0, Markdown = 1 }

public interface IMarkdownService
{
    string ToHtml(string markdown);
    string ToMarkdown(string html);
}
```

- [ ] **Step 1: Add Markdig to CPM**

Add to `Directory.Packages.props`:
```xml
<PackageVersion Include="Markdig" Version="0.41.3" />
```

- [ ] **Step 2: Create library csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>OpenLiveWriter.Markdown</AssemblyName>
    <RootNamespace>OpenLiveWriter.Markdown</RootNamespace>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Markdig" />
    <PackageReference Include="AngleSharp" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Write failing tests**

In `MarkdownServiceTests.cs` cover at least:
- `# Hello` → contains `<h1>`
- paragraph, `**bold**`, `*italic*`, lists, `[a](url)`, `![alt](src)`
- GFM table, `~~strike~~`, `- [ ] task`
- `ToMarkdown(ToHtml(md))` preserves headings/paragraphs/emphasis for a fixture set
- HTML passthrough: `<div class="x">y</div>` survives ToMarkdown
- `<!--more-->` preserved through ToHtml/ToMarkdown where feasible

- [ ] **Step 4: Run tests — expect fail**

```powershell
dotnet test src/managed/OpenLiveWriter.Markdown.Tests/OpenLiveWriter.Markdown.Tests.csproj
```

Expected: fail (types missing)

- [ ] **Step 5: Implement MarkdownService + HtmlToMarkdownConverter**

Use Markdig pipeline:
```csharp
new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
```
(`UseAdvancedExtensions` includes GFM-oriented extensions.)

Html→Markdown via AngleSharp walk: h1–h6, p, strong/b, em/i, ul/ol/li, a, img, table, del/s, input[type=checkbox], pre/code, blockquote; unknown elements → raw outer HTML.

- [ ] **Step 6: Run tests — expect pass**

- [ ] **Step 7: Wire into sln/slnf**

- [ ] **Step 8: Commit**

```bash
git add src/managed/OpenLiveWriter.Markdown src/managed/OpenLiveWriter.Markdown.Tests src/managed/Directory.Packages.props src/managed/writer.sln src/managed/writer.macOS.slnf src/managed/writer.Windows.slnf
git commit -m "feat(markdown): add GFM conversion library and unit tests"
```

---

### Task 2: Publishing model — formats + document body

**Files:**
- Create: `src/managed/OpenLiveWriter.Publishing/ContentFormat.cs`
- Modify: `src/managed/OpenLiveWriter.Publishing/Accounts/BlogAccount.cs`
- Modify: `src/managed/OpenLiveWriter.Publishing/PostDocument.cs`
- Modify: `src/managed/OpenLiveWriter.Publishing/OpenLiveWriter.Publishing.csproj` — ProjectReference to Markdown
- Create: `src/managed/OpenLiveWriter.Publishing.Tests/OpenLiveWriter.Publishing.Tests.csproj` (or place under Platform.Tests if simpler — prefer new Publishing.Tests)
- Create: `src/managed/OpenLiveWriter.Publishing.Tests/PostDocumentMarkdownTests.cs`
- Create: `src/managed/OpenLiveWriter.Publishing.Tests/BlogAccountFormatTests.cs`
- Modify: sln/slnf to include Publishing.Tests

**Interfaces:**
- Consumes: `IMarkdownService` only for conversion helpers, not required on BlogAccount
- Produces:
```csharp
// BlogAccount
public ContentFormat EditingFormat { get; set; } = ContentFormat.Html;
public ContentFormat PublishFormat { get; set; } = ContentFormat.Html;
// Clone() must copy both

// PostDocument
public ContentFormat BodyFormat { get; set; } = ContentFormat.Html;
public string BodyMarkdown { get; set; } = string.Empty;
// When BodyFormat==Markdown, BodyMarkdown is authoritative; BodyHtml may hold last HTML cache or empty
```

- [ ] **Step 1: Failing tests** for defaults, Clone, JSON round-trip of BodyMarkdown + BodyFormat via FileDraftStore or JsonSerializer

- [ ] **Step 2: Implement enum + properties + Clone updates**

- [ ] **Step 3: Tests pass + commit**

```bash
git commit -m "feat(markdown): add per-blog and document content format fields"
```

---

### Task 3: Publish pipeline body resolution

**Files:**
- Modify: `src/managed/OpenLiveWriter.Publishing/EditorContentPublisher.cs`
- Create: `src/managed/OpenLiveWriter.Publishing/PublishBodyResolver.cs`
- Create: `src/managed/OpenLiveWriter.Publishing.Tests/PublishBodyResolverTests.cs`
- Modify: `src/managed/OpenLiveWriter.App.Avalonia/MainWindow.Publishing.cs` to use resolver

**Interfaces:**
```csharp
public static class PublishBodyResolver
{
    // Returns the string to feed into BuildPost / transport as Contents
    public static string Resolve(string canonicalBody, ContentFormat bodyFormat, ContentFormat publishFormat, IMarkdownService markdown);
}
```

Rules:
- body Html + publish Html → canonicalBody
- body Markdown + publish Html → `markdown.ToHtml(canonicalBody)`
- body Markdown + publish Markdown → canonicalBody
- body Html + publish Markdown → `markdown.ToMarkdown(canonicalBody)` (edge case)

- [ ] **Step 1: Failing tests for all four combinations**
- [ ] **Step 2: Implement + wire MainWindow publish to pass formats**
- [ ] **Step 3: Commit**

```bash
git commit -m "feat(markdown): resolve publish body as HTML or Markdown"
```

---

### Task 4: Draft conversion helper + prompt contract

**Files:**
- Create: `src/managed/OpenLiveWriter.Publishing/DraftConversion.cs`
- Create: `src/managed/OpenLiveWriter.Publishing.Tests/DraftConversionTests.cs`
- Modify: `src/managed/OpenLiveWriter.App.Avalonia/Dialogs/AccountDialog.cs`
- Create: conversion confirmation dialog helper (MessageDialog or small Window)

**Interfaces:**
```csharp
public static class DraftConversion
{
    public static int ConvertBlogDraftsToMarkdown(IDraftStore store, string blogId, IMarkdownService markdown);
}
```

AccountDialog:
- Add EditingFormat ComboBox (HTML / Markdown)
- Add PublishFormat ComboBox (HTML / Markdown); when Editing=Html, force Publish=Html and disable control
- On save, if transitioning Html→Markdown and drafts exist for blog: ask Yes/No/Cancel via dialog
  - Yes → ConvertBlogDraftsToMarkdown
  - No → only update account formats; leave drafts as Html until opened (open path may still treat by BodyFormat)
  - Cancel → abort save

Tooltip/strings:
- Font disabled (Task 5): `"Font family and size are not available in Markdown mode because Markdown does not encode visual fonts."`

- [ ] **Step 1: Unit tests for DraftConversion**
- [ ] **Step 2: Implement conversion**
- [ ] **Step 3: AccountDialog UI + prompt**
- [ ] **Step 4: Commit**

```bash
git commit -m "feat(markdown): account format settings and one-time draft conversion"
```

---

### Task 5: EditorPanel Markdown mode sync + font gating

**Files:**
- Create: `src/managed/OpenLiveWriter.App.Avalonia/Editor/MarkdownEditingController.cs`
- Modify: `src/managed/OpenLiveWriter.App.Avalonia/Editor/EditorPanel.axaml.cs`
- Modify: ribbon/command enable paths that call `SetFontFamilyAsync` / `SetFontSizeAsync` (search MainWindow / ribbon bindings)
- Modify: `src/managed/OpenLiveWriter.App.Avalonia/OpenLiveWriter.App.Avalonia.csproj` — reference Markdown
- Create/extend automated tests under `OpenLiveWriter.EditorTests.Automated` for conversion helpers (pure, no WebView if flaky)

**Behavior:**
- `MarkdownEditingController` holds `IMarkdownService`, `IsMarkdownMode`, sync methods:
  - `HtmlFromCanonical(string)` / `CanonicalFromHtml(string)`
  - Source view uses Markdown text when mode on
  - SwitchView paths updated
- Disable font family/size when `IsMarkdownMode`; set tooltip on disabled controls
- Keep `SetBlockFormatAsync` / SemanticHtmlStyles enabled

- [ ] **Step 1: Unit-test controller conversion helpers**
- [ ] **Step 2: Integrate EditorPanel SwitchView**
- [ ] **Step 3: Font command gating + tooltip**
- [ ] **Step 4: Commit**

```bash
git commit -m "feat(markdown): Source/Design sync and disable fonts in Markdown mode"
```

---

### Task 6: Save/load draft session wiring + integration tests

**Files:**
- Modify: `DraftSession.cs` / save paths that set `BodyHtml` to also set `BodyMarkdown`/`BodyFormat` from active blog
- Create: `src/managed/OpenLiveWriter.Markdown.Tests/RoundTripIntegrationTests.cs` (library-level)
- Create: `src/managed/OpenLiveWriter.Publishing.Tests/DraftStoreMarkdownRoundTripTests.cs`

- [ ] **Step 1: Tests — save Markdown draft, reload, BodyFormat and BodyMarkdown intact**
- [ ] **Step 2: Wire DraftSession / MainWindow document load**
- [ ] **Step 3: Commit**

```bash
git commit -m "feat(markdown): persist and reload Markdown drafts"
```

---

### Task 7: Full test suite + push branch

- [ ] **Step 1: Run**

```powershell
dotnet test src/managed/OpenLiveWriter.Markdown.Tests/OpenLiveWriter.Markdown.Tests.csproj
dotnet test src/managed/OpenLiveWriter.Publishing.Tests/OpenLiveWriter.Publishing.Tests.csproj
dotnet test src/managed/OpenLiveWriter.Platform.Tests/OpenLiveWriter.Platform.Tests.csproj
dotnet test src/managed/OpenLiveWriter.EditorTests.Automated/OpenLiveWriter.EditorTests.Automated.csproj
```

- [ ] **Step 2: Fix failures**
- [ ] **Step 3: Push**

```powershell
git push -u fork develop/markdown
# if fork remote unavailable, use origin with user confirmation
```

- [ ] **Step 4: Report test counts and branch URL**

---

## Parallelism note

Tasks 1 must complete first. After Task 1: Tasks 2 can proceed. After Task 2: Tasks 3 and 4 can run in parallel worktrees/agents if careful about BlogAccount/PostDocument. Task 5 depends on 1–2. Task 6 depends on 2+5. Task 7 last.
