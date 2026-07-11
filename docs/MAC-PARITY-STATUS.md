# Open Live Writer — macOS Parity Status

Single source of truth for the Mac (Avalonia) port. Goal: **feature and visual
parity with Open Live Writer for Windows.** Update this file as work lands.

Branch: `milestone4/webview-wysiwyg` · Runtime: .NET 10 / Avalonia · Last verified: 2026-07

## Official milestone plan

Tracked in the org project *"Cross-Platform Migration (macOS + Windows)"*
(`github.com/orgs/OpenLiveWriter/projects/1`) via milestone issues in
`OpenLiveWriter/OpenLiveWriter`:

| Milestone | Scope | Status |
| --- | --- | --- |
| M1 (#998) | Platform Abstraction Layer | ✅ Complete |
| M2 (#999) | Retarget core libs to net10.0 (+ macOS console PoC) | ✅ Complete |
| M3 (#1000) | Avalonia UI shell + Platform.Mac (ribbon, toolbar, Keychain, dialogs) | ✅ Complete |
| M4 (#1001) | WebView editor (WKWebView) with JS bridge | ✅ Complete |
| M5 (#1002) | Packaging + Store submission (.app/DMG, sign/notarize, CI matrix, App Store) | ⬜ Not started |

M1–M4 landed the buildable Avalonia stack described below. The backlog in §3 is
**M4 editor polish / parity** work (finishing the editor + ribbon to true feature
parity) plus the **M5 packaging** track; each item is tagged accordingly.

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
- **Live toggle-state sync:** editor posts `stateChanged` messages (via the Avalonia
  WebView `window.invokeCSharpAction` bridge) → `WebViewEditor.FormatStateChanged` →
  ribbon + toolbar toggle buttons reflect the caret's current formatting.
- **Insert Link:** modal `LinkDialog` (URL + text + title + open-in-new-window),
  wired to the Link toolbar button, `Ctrl+K`, and the `InsertLink` ribbon command.
- **Font family / size:** ribbon Font group combos populated and wired to the editor
  (`fontName` / `fontSize`). *Note: size uses the HTML 1–7 scale; refine to px later.*

---

## 3. Parity gap — prioritized backlog

All P0–P2 items below are **M4 editor polish / parity** (finishing the shipped
editor to feature parity). P3 packaging/distribution is the **M5** track. Publishing
(P2) additionally depends on porting `BlogClient`/`PostEditor` off WinForms.

### P0 — core editing correctness (highest value) · M4 parity
1. ✅ **Ribbon toggle-button state sync.** Done — `stateChanged` message →
   `FormatStateChanged` → ribbon + toolbar toggle `IsChecked`.
2. ✅ **Insert Link dialog.** Done — `LinkDialog` wired to toolbar / `Ctrl+K` /
   `InsertLink` command via `InsertLinkAsync`.
3. ✅ **Font family / size combos.** Done — ribbon Font combos wired to
   `fontName` / `fontSize`. *Follow-up:* font size uses the HTML 1–7 scale; move to
   explicit px sizing and reflect the current selection's font back into the combos.
4. **Font/highlight color pickers.** `FontColorPicker`/`FontBackgroundColor` need a
   color picker UI wired to `foreColor`/`hiliteColor`.
5. **Semantic HTML gallery.** `SemanticHtmlGallery` (h1–h6/p/pre styles) not wired to
   `formatBlock`.

### P1 — document lifecycle · M4 parity
6. **New/Open/Save draft, post model.** No post/document model on the Mac side
   (`EditorModel` is a stub). No local draft persistence.
7. **Image insert from file.** `InsertPictureFromFile` → file picker + `<img>` insert.
8. **Word count, Find.** `WordCount`, `FindButton` unimplemented.

### P2 — accounts & publishing · M4 parity (blocked on BlogClient/PostEditor WinForms port)
9. **Account setup / blog config.** `AddWeblog`, `ConfigureWeblog`, `Accounts` — no UI;
   `MacCredentialStorage` exists but is not exercised.
10. **Publish pipeline.** `PostAndPublish`, `PostAsDraft`, `SelectBlog` gallery — depend
    on porting `BlogClient`/`PostEditor` off WinForms.

### P3 — visual parity & advanced · M4 parity + M5 packaging
11. **Ribbon visual fidelity** vs. the Windows Fluent ribbon (spacing, icons, group
    chrome, contextual tabs actually appearing on selection). *(M4)*
12. Tables, video, maps, tags, plugins, spellcheck UI, print/preview. *(M4)*
13. **M5 packaging:** `.app` bundle + DMG, code signing / notarization
    (`xcrun notarytool`), cross-platform GitHub Actions build matrix, App Store
    submission. Start once editor + ribbon parity (P0–P1) is solid.

---

## 4. Recommended next steps (for the following session)

1. **Font/highlight color pickers (P0-4)** and **Semantic HTML gallery (P0-5)** —
   finish the remaining Home-tab editing controls. Color pickers need a small
   Avalonia flyout wired to `foreColor`/`hiliteColor`; the gallery maps to
   `formatBlock` (h1–h6/p/pre) and can reuse the existing block-format bridge.
2. **Font combo refinement:** switch font size from the HTML 1–7 scale to explicit px
   and push the current selection's font family/size back into the ribbon combos
   (extend `FormatState` + `getState()`), same pattern as toggle sync.
3. Begin **document/post model + draft save/open (P1-6)** to unlock the File menu.
4. Once P0–P1 editor parity is solid, start the **M5 packaging** track (`.app`/DMG,
   notarization, CI matrix).

## 5. Verification

- Build gate: `dotnet build` for each project in §1 (all green).
- Message bridge: app smoke-launch logs `[OLW-WebView] Ready` and delivers an initial
  `stateChanged` message through `window.invokeCSharpAction` (verified).
- Editor behavior: `OpenLiveWriter.EditorTests` GUI bench ("Run Auto Tests" button)
  exercises bold/italic/underline/strike/sub/super/alignment/list/HR/blockquote/link
  round-trips plus `getState()` assertions against the live WebView.
