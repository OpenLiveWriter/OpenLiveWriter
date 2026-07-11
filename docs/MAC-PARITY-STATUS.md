# Open Live Writer — macOS Parity Status

Single source of truth for the Mac (Avalonia) port. Goal: **feature and visual
parity with Open Live Writer for Windows.** Update this file as work lands.

Branch: `milestone4/webview-wysiwyg` · Runtime: .NET 10 / Avalonia · Last verified: 2026-07

---

## 1. Build status (macOS, Apple Silicon, `dotnet build`)

Verified building cleanly on macOS:

| Project | Status | Notes |
| --- | --- | --- |
| `OpenLiveWriter.Platform` | ✅ builds | Abstraction interfaces |
| `OpenLiveWriter.Platform.Mac` | ✅ builds | Credential/spellcheck/display/bidi services |
| `OpenLiveWriter.Ribbon.Avalonia` | ✅ builds | Config-driven ribbon |
| `OpenLiveWriter.App.Avalonia` | ✅ builds + launches | Avalonia shell; WebView editor reaches Ready |
| `OpenLiveWriter.EditorTests` | ✅ builds | GUI E2E bench for the WebView editor |
| `OpenLiveWriter.Localization` | ✅ builds | Shared |
| `OpenLiveWriter.HtmlParser` | ✅ builds | Shared |
| `OpenLiveWriter.CoreServices` | ✅ builds (warnings) | MSB3277 WebView2 WPF ref warning only |

**Runnable:** `dotnet run --project src/managed/OpenLiveWriter.App.Avalonia`
launches the shell; console logs `[OLW-WebView] Ready` once the editor loads.

**Not yet building on macOS:** the WinForms-heavy projects in `writer.sln`
(`net10.0-windows`) — e.g. `OpenLiveWriter.ApplicationFramework`,
`OpenLiveWriter.PostEditor`, `OpenLiveWriter.Controls`, `OpenLiveWriter.Mshtml`,
`OpenLiveWriter` (main app). These depend on `System.Windows.Forms` / MSHTML and
must be ported or abstracted (some cross-platform prep exists on the unmerged
`feature/macbuild` branch: BlogClient WinForms removal, platform nuget path fix,
thread-safety/caching fixes).

---

## 2. What works today

- **Shell:** Avalonia MainWindow with a config-driven ribbon (`DefaultRibbonConfiguration`)
  and a status bar.
- **Editor:** WebView (WKWebView) `contenteditable` surface (`editor.html`) with a
  JS bridge (`OLWBridge`) for `execCommand`, selection save/restore, get/set content.
- **View toggle:** Edit / Source / Preview (source shows formatted HTML round-tripped
  from the WebView).
- **Editor commands wired to the ribbon + toolbar** (via `WebViewEditor.HandleCommandAsync`):
  - Character: Bold, Italic, Underline, Strikethrough, Subscript, Superscript, Clear Formatting
  - Lists/indent: Bullets, Numbers, Indent, Outdent
  - Paragraph: Align Left/Center/Right, Justify, Blockquote (toggle)
  - Editing: Undo, Redo, Select All
  - Insert: Horizontal line; `createLink`/`insertHtml` bridge methods exist
  - Block format: `formatBlock` (headings via toolbar combo)
- **Format-state reporting:** `OLWBridge.getState()` reports bold/italic/underline/
  strike/sub/super/lists/alignment/blockTag (consumed by `FormatState`).

---

## 3. Parity gap — prioritized backlog

### P0 — core editing correctness (highest value)
1. **Ribbon toggle-button state sync.** `getState()` reports format state but the
   ribbon/toolbar toggle buttons do not yet reflect it (Bold stays un-pressed when
   the caret is inside bold text). Wire the WebView `stateChanged` message →
   `FormatStateChanged` → ribbon button `IsChecked`.
2. **Insert Link dialog.** `createLink` bridge exists but there is no URL/text input
   dialog; toolbar/ribbon Link is still "not implemented".
3. **Font family / size combos.** Ribbon `FontFamily`/`FontSize` combos are not wired
   to `fontName`/`fontSize` execCommand.
4. **Font/highlight color pickers.** `FontColorPicker`/`FontBackgroundColor` need a
   color picker UI wired to `foreColor`/`hiliteColor`.
5. **Semantic HTML gallery.** `SemanticHtmlGallery` (h1–h6/p/pre styles) not wired to
   `formatBlock`.

### P1 — document lifecycle
6. **New/Open/Save draft, post model.** No post/document model on the Mac side
   (`EditorModel` is a stub). No local draft persistence.
7. **Image insert from file.** `InsertPictureFromFile` → file picker + `<img>` insert.
8. **Word count, Find.** `WordCount`, `FindButton` unimplemented.

### P2 — accounts & publishing
9. **Account setup / blog config.** `AddWeblog`, `ConfigureWeblog`, `Accounts` — no UI;
   `MacCredentialStorage` exists but is not exercised.
10. **Publish pipeline.** `PostAndPublish`, `PostAsDraft`, `SelectBlog` gallery — depend
    on porting `BlogClient`/`PostEditor` off WinForms.

### P3 — visual parity & advanced
11. **Ribbon visual fidelity** vs. the Windows Fluent ribbon (spacing, icons, group
    chrome, contextual tabs actually appearing on selection).
12. Tables, video, maps, tags, plugins, spellcheck UI, print/preview.

---

## 4. Recommended next steps (for the following session)

1. **Ribbon toggle-state sync (P0-1).** Highest leverage, self-contained: consume the
   WebView `stateChanged` postMessage in `WebViewEditor`, surface via
   `FormatStateChanged`, and update ribbon toggle buttons + toolbar toggles. Verify in
   the EditorTests bench (extend it to assert `getState()` after a command).
2. **Insert Link dialog (P0-2)** and **Font family/size combos (P0-3)** — both small,
   both use bridge methods that now exist.
3. Begin **document/post model + draft save/open (P1-6)** to unlock the File menu.

## 5. Verification

- Build gate: `dotnet build` for each project in §1.
- Editor behavior: `OpenLiveWriter.EditorTests` GUI bench ("Run Auto Tests" button)
  exercises bold/italic/underline/strike/sub/super/alignment/list/HR/blockquote/link
  round-trips against the live WebView.
