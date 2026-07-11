# Open Live Writer — macOS Parity Status

Single source of truth for the Mac (Avalonia) port. Goal: **feature and visual
parity with Open Live Writer for Windows.** Update this file as work lands.

Branch: `milestone4/webview-wysiwyg` · Runtime: .NET 10 / Avalonia · Last verified: 2026-07 (document/draft lifecycle + File menu)

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
| `OpenLiveWriter.App.Avalonia` | ✅ builds + launches | Avalonia shell; WebView editor reaches Ready; references `OpenLiveWriter.Publishing` |
| `OpenLiveWriter.Publishing` | ✅ builds | **New** — cross-platform publish slice (BlogPost, editor-HTML cleanup, MetaWeblog XML-RPC), no WinForms/MSHTML |
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
must be ported or abstracted. The first **publish** slice has been carved out into
the cross-platform `OpenLiveWriter.Publishing` (see §7); the remaining BlogClient/
PostEditor surface is still Windows-only. Cross-platform prep also exists on the
unmerged `feature/macbuild` branch (BlogClient WinForms removal, platform nuget
path fix, thread-safety/caching fixes) — assessed in §7.2.

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
- **Document / draft lifecycle (File menu):** cross-platform `PostDocument` model
  (id/title/body/blog/categories/timestamps + dirty flag, interoperable with
  `Publishing.BlogPost`), an `IDraftStore` abstraction with a JSON file-per-draft
  `FileDraftStore`, and a UI-agnostic `DraftSession` controller. New Post/Page, Save
  (Save Draft), Open Draft (picker), Delete Draft, and `OpenDraftMRU0-9` File-menu
  commands are wired in the shell, with Save/Discard/Cancel unsaved-changes prompts
  on New/Open driven by the dirty flag. See §8.

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
6. ✅ **New/Open/Save draft, post model.** Done — cross-platform `PostDocument`
   model + `IDraftStore`/`FileDraftStore` (JSON, one file per draft under the
   platform-resolved app-data `Drafts` folder) + `DraftSession` controller, with
   New/Save/Open/Delete/MRU File-menu commands wired and unsaved-changes prompts.
   Replaced the `EditorModel` stub. See §8.
7. **Image insert from file.** `InsertPictureFromFile` → file picker + `<img>` insert.
8. **Word count, Find.** `WordCount`, `FindButton` unimplemented.

### P2 — accounts & publishing · M4 parity (publish pipeline slice ported; accounts/UI remain)
9. **Account setup / blog config.** `AddWeblog`, `ConfigureWeblog`, `Accounts` — no UI;
   `MacCredentialStorage` exists but is not exercised. Still needs blog-account model +
   detection wiring (see §7).
10. **Publish pipeline.** ✅ *First slice ported* — `OpenLiveWriter.Publishing`
    (net10.0) now builds the `editor HTML → BlogPost → MetaWeblog XML-RPC` payload
    cross-platform and is referenced by the Avalonia app via
    `WebViewEditor.PublishAsync`. Remaining: account/credentials wiring, live
    round-trip against a real blog, additional providers (Atom/WordPress/Blogger),
    draft persistence, and `PostAndPublish`/`PostAsDraft`/`SelectBlog` UI. See §7.

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
3. ✅ **Document/post model + draft save/open (P1-6)** — done (see §8). Follow-ups:
   surface a real Open Drafts list in the ribbon backstage (not just the modal
   picker), populate the `OpenDraftMRU*` menu labels from the store, and prompt on
   window-close when dirty.
4. **Image insert (P1-7)** and **word count (P1-8)** to finish the P1 lifecycle band.
5. Once P0–P1 editor parity is solid, start the **M5 packaging** track (`.app`/DMG,
   notarization, CI matrix).

## 5. Verification

- Build gate: `dotnet build` for each project in §1 (all green).
- Message bridge: app smoke-launch logs `[OLW-WebView] Ready` and delivers an initial
  `stateChanged` message through `window.invokeCSharpAction` (verified).
- Editor behavior: `OpenLiveWriter.EditorTests` GUI bench ("Run Auto Tests" button)
  exercises bold/italic/underline/strike/sub/super/alignment/list/HR/blockquote/link
  round-trips plus `getState()` assertions against the live WebView.
- Automated suite: `OpenLiveWriter.EditorTests.Automated` (see §6).

---

## 6. Test coverage

Automated suite: **`OpenLiveWriter.EditorTests.Automated`** (NUnit +
`Avalonia.Headless.NUnit` + AngleSharp). DOM assertions use AngleSharp (parsed
tags/attributes, not substring matching). The manual GUI bench
(`OpenLiveWriter.EditorTests`) is retained for live, eyes-on verification.

Run:

- Default (headless, no WebView backend needed): `dotnet test src/managed/OpenLiveWriter.EditorTests.Automated`
- Live editor tests (real macOS desktop session with a WKWebView backend):
  `dotnet test src/managed/OpenLiveWriter.EditorTests.Automated --filter "Category=WebView"`
- Publish TDD targets: `dotnet test ... --filter "Category=PublishTdd"`

Default run status: **63 passed / 0 failed / 0 skipped.** WebView-category tests
are `[Explicit]` (excluded from the default run) so the headless gate stays green.
The two Group C `RealPipeline_*` probes are no longer `[Explicit]` (the publish
slice is ported) and run in the default suite, lifting the count 44 → 46. The new
`GroupD_DraftLifecycleTests` (real `FileDraftStore`/`PostDocument`/`DraftSession`
against a temp dir — no WebView needed) added 17 more, lifting 46 → 63; the
previously `[Explicit]` D4 "draft save/open" target is now implemented and folded
into that fixture.

### Why some tests are `[Explicit]`

- **WebView category:** `document.execCommand` formatting runs *inside* WKWebView.
  A live WebView needs a real windowing backend and does not initialize under a
  headless `dotnet test`; those tests report *skipped* (via `Assert.Ignore`) with
  guidance rather than failing. They are structured/ready and re-use
  `EditorTestHarness` — run them on a real macOS desktop session, or rely on the
  manual bench for live verification.
- **Publish TDD:** the first publish slice is now ported to
  `OpenLiveWriter.Publishing` (net10.0), so Group C exercises the **real** ported
  types (`BlogPost`, `EditorContentPublisher`, `MetaWeblogXmlRpcClient`,
  `XmlCharacterHelper`) and the reflection probes pass in the default run.
  Remaining publish work (accounts, transport against a live blog, more providers)
  is tracked in §7, not as `[Explicit]` tests.

### Scenario map

| Scenario | Coverage | Default run |
| --- | --- | --- |
| A: bold / italic / underline / strike / sub / sup | `GroupA_EditorCommandTests` | ⏭ WebView |
| A: un/ordered lists, indent/outdent (idempotent) | `GroupA_EditorCommandTests` | ⏭ WebView |
| A: align L/C/R/justify | `GroupA_EditorCommandTests` | ⏭ WebView |
| A: blockquote toggle (present → reverts to `<p>`) | `GroupA_EditorCommandTests` | ⏭ WebView |
| A: headings h1–h6 + p + pre (via bridge `formatBlock`) | `GroupA_EditorCommandTests` | ⏭ WebView |
| A: toolbar HeadingCombo only reaches h1–h3 (gap) | `GroupA_ToolbarGapTests` | ✅ pass |
| A: createLink / link text+title+new-window + escaping | `GroupA_EditorCommandTests` (live) + `GroupA_LinkHtmlTests` (pure) | ✅ pass (pure) / ⏭ WebView (live) |
| A: horizontal rule, clear formatting, partial-selection bold | `GroupA_EditorCommandTests` | ⏭ WebView |
| A: font family / size | `GroupA_EditorCommandTests` | ⏭ WebView |
| A16: well-formedness / publish-readiness gate | `GroupA_WellFormednessTests` (samples) + `GroupA_EditorCommandTests` (live) | ✅ pass (samples) / ⏭ WebView (live) |
| A18: getState sync (bold on→true, off→false; blockTag) | `GroupA18_GetStateTests` | ⏭ WebView |
| B1–B3: source/preview round-trip (WYSIWYG↔source, hand-edited h2+ul) | `GroupB_RoundtripTests` (via `EditorPanel.FormatHtml`) | ✅ pass |
| B: live round-trip + preview render | `GroupB_RoundtripTests` | ⏭ WebView / preview `[Explicit]` |
| C: post model, MetaWeblog payload (description=MainContents, publish flag), draft, extended split | `GroupC_PublishTests` (real `OpenLiveWriter.Publishing` + `FakeBlogClient`) | ✅ pass (real types) |
| C: `XmlCharacterHelper` XML-char scrub | `GroupC_PublishTests` (real ported helper) | ✅ pass |
| C: real ported pipeline present (app refs `Publishing`, `WebViewEditor.PublishAsync`) | `GroupC_PublishTests` (reflection probes) | ✅ pass |
| D1: LinkDialog validation (Insert disabled for empty/`https://`) | `GroupD_DialogTests` (logic + headless UI) | ✅ pass |
| D4: draft save/load round-trip (DOM equiv), overwrite, MRU order, delete, corrupt/missing, BlogPost interop, DraftSession dirty tracking | `GroupD_DraftLifecycleTests` (real `FileDraftStore`/`PostDocument`/`DraftSession`, temp dir) | ✅ pass |
| D: image insert / account setup / word count | `GroupD_DialogTests` | ⏭ `[Explicit]` |

### Production seams added for testability

Small, behavior-preserving `internal` seams in `App.Avalonia` (exposed via
`InternalsVisibleTo`): `WebViewEditor.BuildAnchorHtml`/`EscapeHtml*` (link build +
escaping), `EditorPanel.FormatHtml` (source view formatter), `LinkDialog.IsValidUrl`.

### Publish tests target the real pipeline

Group C now drives the ported `OpenLiveWriter.Publishing` types directly
(`BlogPost` main/extended split, real `MetaWeblogXmlRpcClient` payload assertions,
`XmlCharacterHelper` scrub) with `FakeBlogClient` only standing in for the network
transport. The reflection probes (`app references Publishing`,
`WebViewEditor.PublishAsync` exists) pass in the default run.

---

## 7. Publish pipeline port (macOS)

### 7.1 What is ported (this slice)

`OpenLiveWriter.Publishing` (net10.0, no WinForms/MSHTML) — the smallest set of
types for a real MetaWeblog/XML-RPC publish of a simple post:

| Type | Ported from | Notes |
| --- | --- | --- |
| `BlogPost` | `Extensibility.BlogClient.BlogPost` | Title/Contents XML-scrub, main/extended split at `<!--more-->`, string categories, `IsPublished` |
| `ExtendedEntry` | `BlogPost.ExtendedEntryBreak` split | `BreakMarker` + `Split()` |
| `XmlCharacterHelper` | `CoreServices.XmlCharacterHelper` | Verbatim valid-XML-char ranges + scrub |
| `Xml/XmlRpc*` | `CoreServices.XmlRpcClient` value classes | `XmlRpcString/Int/Boolean/Array/Struct/Member`, method-response parser |
| `IBlogClient` / `IBlogClientOptions` | `Extensibility.BlogClient.IBlogClient` | Minimal `NewPost`/`EditPost` + `SupportsExtendedEntries`/`SupportsCategoriesInline` |
| `MetaWeblogXmlRpcClient` | `BlogClient.Clients.MetaweblogClient` | `GeneratePostStruct` (title, description=MainContents, mt_text_more=ExtendedContents, categories) + `BuildNewPostXml`/`BuildEditPostXml` (offline) + HttpClient transmit |
| `EditorContentPublisher` | `PostEditor` GetEditedHtml step | Trim/linebreak strip → scrub → split → `BlogPost`; `Publish()` orchestration |

Referenced by `OpenLiveWriter.App.Avalonia`; entry point
`WebViewEditor.PublishAsync(client, blogId, title, publish, categories)` feeds
`GetContentAsync()` output through `EditorContentPublisher`.

**UI-coupled pieces intentionally left out** (stay behind `OpenLiveWriter.Platform`
abstractions for the full port): credential prompts (`ICredentialsPrompter`),
dialogs (`IDialogService`), captcha, MSHTML-based blog/template detection, image
upload, and the full `IBlogClient` surface (categories/keywords/pages/authors).

### 7.2 `feature/macbuild` prep assessment

The unmerged `feature/macbuild` branch has 4 commits ahead of this branch's
merge-base (`e6ab7f6b`): `.claude` gitignore, an SDK nuspec path fix, Platform
thread-safety/caching fixes, and **`a6f571b4 refactor(blogclient): Remove WinForms
dependencies`** (47 files: retargets the *whole* BlogClient, moves login/captcha
dialogs to `Platform.Windows`, adds `ICredentialsPrompter`/`IDialogService`,
excludes MSHTML detection).

**Recommendation: do not cherry-pick for this slice; re-derive a focused assembly
(done).** Rationale: `a6f571b4` is a large in-place refactor of the entire
BlogClient and only pays off once BlogClient's full dependency chain
(Extensibility, CoreServices, Api, MSHTML detection) builds cross-platform — a
multi-session effort. It yields no compiling macOS artifact on its own, so folding
it in now would add risk without a verifiable slice. It remains the reference for
the **full** BlogClient port (§7.3); its `ICredentialsPrompter`/`IDialogService`
abstractions and dialog relocation should be adopted then. (The Platform
thread-safety fixes in `a1ab901f` are a good low-risk standalone cherry-pick
candidate for a later pass.)

### 7.3 Remaining publish backlog

1. **Accounts/credentials wiring.** Blog-account model + `MacCredentialStorage`
   (Keychain) → construct `MetaWeblogXmlRpcClient` with real endpoint/user/pass;
   `ICredentialsPrompter` for interactive auth.
2. **Live transport validation.** Exercise `NewPost`/`EditPost` against a real
   MetaWeblog endpoint (self-hosted WordPress / `utilities/BlogServer`); handle
   redirects, encoding, and auth headers via the existing `HttpClient` path.
3. **Additional providers.** Port Atom (AtomPub), WordPress, and Blogger v3 clients
   (larger; some depend on MSHTML detection that must be abstracted first).
4. ✅ **Draft persistence + document model.** Done — see §8. The publish path can
   now build a `BlogPost` directly from the edited `PostDocument`
   (`PostDocument.ToBlogPost()`), so `PostAsDraft` maps to a local draft save while
   `PostAndPublish` reuses the same document for the transport.
5. **Publish UI.** `PostAndPublish`/`PostAsDraft` commands, `SelectBlog` gallery,
   category picker, progress/errors surfaced via `IDialogService`.
6. **Fold in `feature/macbuild` abstractions** (`ICredentialsPrompter`,
   `IDialogService`, dialog relocation) during the full BlogClient port.

---

## 8. Document / draft lifecycle (macOS)

Reproduces the Windows File-menu behavior (New/Save/Open/Delete draft) with a
Mac-friendly store, replacing the Windows `.wpost` OLE structured storage that is
impractical cross-platform.

### 8.1 Model + store (cross-platform, in `OpenLiveWriter.Publishing`)

| Type | Role |
| --- | --- |
| `PostDocument` | Editable unit: `Id`, `BlogId`, `Title`, `BodyHtml` (full body incl. `<!--more-->`), `Categories`, `IsPage`, `IsPublished`, `DateCreatedUtc`/`DateModifiedUtc`, transient `IsDirty`. `ToBlogPost()`/`FromBlogPost()` convert to/from the transport `BlogPost` so the *same* document is saved as a draft and published — no competing model. |
| `IDraftStore` | Persistence seam: `Save` (new + overwrite), `Load`, `List`, `Delete`, `Exists`. Hides the on-disk format so it can change without touching callers. |
| `FileDraftStore` | JSON, one file per draft (`{id}.oldraft.json`, `Guid.NewGuid("N")` ids). Writes via temp-file-then-move (no truncated files on crash). **Robust:** missing dir ⇒ empty store (created lazily on save); corrupt file ⇒ `Load` throws `DraftStoreException` but `List` skips it. `List()` is ordered most-recently-modified first (drives Open Drafts + MRU). |
| `DraftInfo` | Lightweight list entry (id/title/modified) for the picker + MRU without loading bodies. |

### 8.2 File location (platform-resolved, never hardcoded)

`DraftStoreFactory.CreateDefault()` (in `App.Avalonia`) resolves the folder via
`PlatformContext.Services.GetApplicationDataDirectory()` + `"Drafts"` — i.e.
`~/Library/Application Support/OpenLiveWriter/Drafts` on macOS, resolved through
`OpenLiveWriter.Platform`, not a literal path.

### 8.3 Command wiring + unsaved-changes handling (`App.Avalonia`)

- **`DraftSession`** (UI-agnostic controller): owns the current `PostDocument`,
  tracks dirty (title/body edits mark dirty only on real change; save clears it),
  and drives `IDraftStore` for New/Save/Open/Delete/List. Testable headlessly.
- **`MainWindow`** routes File-menu `CommandId`s through `TryHandleFileCommandAsync`
  *before* editor formatting: `NewPost`/`NewPage`, `SavePost` (Save Draft),
  `OpenDrafts`/`OpenPost` (modal `DraftPickerDialog`), `DeleteDraft` (confirm), and
  `OpenDraftMRU0-9` (nth of the MRU-ordered list). The shell's existing `TitleEditor`
  supplies the title; body comes from `WebViewEditor.GetContentAsync()`.
- **Unsaved changes:** New/Open (and MRU) call `ConfirmDialog.ShowUnsavedChangesAsync`
  when dirty — Save proceeds after saving, Don't Save discards, Cancel aborts. A null
  owner (headless) defaults to the safe/cancel path so logic stays testable.

### 8.4 Tests

`GroupD_DraftLifecycleTests` (default headless suite, temp dir, no WebView):
save/load round-trip with **DOM equivalence**, new-doc timestamps, in-place
overwrite (same id, single file), MRU ordering, delete, missing/corrupt-file
handling, `BlogPost` interop, and `DraftSession` dirty tracking + New/Open/Delete.
