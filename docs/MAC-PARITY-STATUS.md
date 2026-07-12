# Open Live Writer — macOS Parity Status

Single source of truth for the Mac (Avalonia) port. Goal: **feature and visual
parity with Open Live Writer for Windows.** Update this file as work lands.

Branch: `milestone4/webview-wysiwyg` · Runtime: .NET 10 / Avalonia · Last verified: 2026-07 (Publishing-completion band: **image upload-on-publish** via `newMediaObject`, **blog categories** fetch + picker, **RSD endpoint auto-detection**, and **re-publish → `editPost`**; on top of the Insert-tab + preview + selection-state band: real Preview render, Insert Table + table-tools ops, web-video embeds, emoticons, paste-special/clean paste, clear-break/extended-entry, caret font/size/color/block reflection)

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
- **View toggle:** Edit / Source / Preview. Source shows formatted HTML round-tripped
  from the WebView; **Preview now renders** the post body as it would look published
  (a neutral "article" layout composed by `PreviewRenderer` and shown in a read-only
  WebView, extended-entry marker joined).
- **Insert tab:** Insert Table (rows×columns + header row + width via `TableBuilder`)
  with basic Table Tools row/column insert-delete + delete-table bridge ops; Insert
  Video as a modern responsive `<iframe>` web embed (YouTube/Vimeo/generic URL or
  pasted embed normalized by `VideoEmbedBuilder` — replaces the dead Flash/service
  paths); Insert Emoticon (Unicode emoji picker); Paste Special (clean paste:
  plain-text / safe-HTML via `PasteCleaner`); Insert Clear Break + Insert Extended
  Entry (`<!--more-->`, shared with the publish split).
- **Caret-state reflection:** the toolbar heading combo and the ribbon Font
  family/size combos follow the caret's actual block tag / font / size (and the state
  also carries fore/highlight color), via the `stateChanged` → `FormatState` pipeline.
- **Editor commands wired to the ribbon + toolbar** (via `WebViewEditor.HandleCommandAsync`):
  - Character: Bold, Italic, Underline, Strikethrough, Subscript, Superscript, Clear Formatting
  - Lists/indent: Bullets, Numbers, Indent, Outdent
  - Paragraph: Align Left/Center/Right, Justify, Blockquote (toggle)
  - Editing: Undo, Redo, Select All
  - Insert: Horizontal line; `createLink`/`insertHtml` bridge methods exist
  - Block format: `formatBlock` — full semantic range (Normal/p, Heading 1-6,
    Preformatted) via the toolbar combo **and** the ribbon SemanticHtmlGallery flyout
  - Color: text color (`foreColor`) + highlight (`hiliteColor`, backColor fallback)
    via ribbon color-swatch flyouts (standard + highlight palettes)
  - Insert: image from file — file picker → inline base64 data-URI `<img>`
  - Editing: Word Count (statistics dialog) + Find / Find & Replace (native
    in-page highlight + HTML-aware Replace All)
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
- **Blog accounts + publishing (Home / Blog Account tabs):** cross-platform
  `BlogAccount` model + `IAccountStore`/`FileAccountStore` (JSON per account, current
  selection persisted) with passwords in the macOS Keychain via an `ICredentialStore`
  seam over `MacCredentialStorage`. Add/Configure/Manage-accounts dialogs, a Select-Blog
  picker, a host-populated ribbon blog-selector dropdown, and `PostAndPublish`
  (publish) / `PostAsDraft` (server draft) wired through `BlogAccountService` →
  `MetaWeblogXmlRpcClient` → `WebViewEditor.PublishAsync` (body from the live editor).
  See §9. *Live-endpoint validation is a documented manual step.*
- **Image upload-on-publish:** inline base64 data-URI `<img>`s are now uploaded to the
  blog via `metaWeblog.newMediaObject` and rewritten to the returned hosted URLs before
  the post is sent (dedup identical images, no-op when none, upload-failure aborts the
  publish). Cross-platform `ImagePublisher`; runs on both new and edit paths. See §10.
- **Categories:** `metaWeblog.getCategories` fetch (`BlogPostCategory` + tolerant parser)
  with a category **checklist dialog** (pre-checked selection, free-text entry, graceful
  when the provider returns none), wired to `ShowCategoryPopup`; chosen categories flow
  into the inline `newPost` categories array. See §10.
- **Provider endpoint auto-detection (RSD):** MSHTML-free `RsdServiceDetector` discovers
  the MetaWeblog endpoint from the blog homepage (`EditURI` link → `rsd.xml` → MetaWeblog
  `apiLink`/`blogID`); a **Detect** button in the Add/Configure Account dialog auto-fills
  the endpoint (manual override retained). HTTP behind an `IRsdHttpFetcher` seam. See §10.
- **Re-publish:** a second publish of an already-published document (same blog) edits the
  same server post via `metaWeblog.editPost` (matched on recorded `PublishedPostId`)
  instead of creating a duplicate.

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
4. ✅ **Font/highlight color pickers.** Done — `FontColorPicker`/`FontBackgroundColor`
   render color-swatch flyouts (standard + highlight palettes) that serialize the
   choice to `#RRGGBB` and drive `foreColor`/`hiliteColor` (with a `backColor`
   fallback for highlight). Pure command→execCommand mapping + hex normalizer tested.
5. ✅ **Semantic HTML gallery.** Done — `SemanticHtmlGallery` opens a style flyout and
   the toolbar `HeadingCombo` now exposes the full range (Normal/p, Heading 1-6,
   Preformatted), both routed through the shared `SemanticHtmlStyles` → `formatBlock`.

### P1 — document lifecycle · M4 parity
6. ✅ **New/Open/Save draft, post model.** Done — cross-platform `PostDocument`
   model + `IDraftStore`/`FileDraftStore` (JSON, one file per draft under the
   platform-resolved app-data `Drafts` folder) + `DraftSession` controller, with
   New/Save/Open/Delete/MRU File-menu commands wired and unsaved-changes prompts.
   Replaced the `EditorModel` stub. See §8.
7. ✅ **Image insert from file.** Done — `InsertPictureFromFile` (toolbar + ribbon)
   opens the Avalonia storage-provider file picker and inserts the chosen image as a
   self-contained inline base64 data-URI `<img>`. *Follow-up (P2):* upload-on-publish
   rewrite of data URIs to hosted URLs once the BlogClient image path is ported.
8. ✅ **Word count, Find.** Done — cross-platform `WordCounter` (HTML→plain text +
   word/char/paragraph counts) surfaced via a `WordCount` statistics dialog; Find /
   Find & Replace via `FindButton`/`FindAndReplace` (native in-page highlight +
   pure `TextFinder` HTML-aware Replace All that leaves tags untouched).

### P2 — accounts & publishing · M4 parity (publish pipeline + accounts/UI landed; live validation pending)
9. ✅ **Account setup / blog config.** Done — cross-platform `BlogAccount` +
   `IAccountStore`/`FileAccountStore` (JSON metadata, no secret) + an `ICredentialStore`
   seam wired to the macOS Keychain (`MacCredentialStorage`) via a thin adapter, plus an
   in-memory fake for tests. `AddWeblog`/`ConfigureWeblog`/`Accounts` open Add / Manage
   dialogs; the password never touches the account JSON. The MetaWeblog API endpoint can
   now be **auto-detected** from the blog homepage via RSD (a **Detect** button), with
   manual override retained. See §9 / §10.
10. **Publish pipeline + UI.** ✅ *Ported + wired* — `OpenLiveWriter.Publishing`
    (net10.0) builds the `editor HTML → BlogPost → MetaWeblog XML-RPC` payload
    cross-platform; `BlogAccountService` resolves the current account + Keychain password,
    builds a `MetaWeblogXmlRpcClient`, and the shell's `PostAndPublish` (publish=true) /
    `PostAsDraft`(+EditOnline) (publish=false) commands publish the live editor content via
    `WebViewEditor.PublishAsync`, recording the returned post id on the document.
    `SelectBlog` tracks/persists the current blog. **Image upload-on-publish**
    (`newMediaObject`), **categories** (`getCategories` + picker), **RSD endpoint
    auto-detection**, and **re-publish → `editPost`** are now done (see §10). **Remaining:**
    a live round-trip against a real blog (covered by the `[Explicit]`/`[Category(LiveBlog)]`
    tests) and additional providers (Atom/WordPress/Blogger). See §7 / §9 / §10.

### P3 — visual parity & advanced · M4 parity + M5 packaging
11. **Ribbon visual fidelity** vs. the Windows Fluent ribbon (spacing, icons, group
    chrome, contextual tabs actually appearing on selection). *(M4)*
12. ✅ **Tables, video, emoticons, preview, paste-special, breaks.** Done this band —
    Insert Table (+ table-tools ops), web-video embeds, emoticon picker, real Preview
    render, clean paste, and clear-break/extended-entry. **Remaining:** maps, tags,
    plugins, spellcheck UI, print, and *contextual-tab activation* (the Table Tools
    ops are wired but the contextual tab does not yet auto-appear on selection). *(M4)*
13. **M5 packaging:** `.app` bundle + DMG, code signing / notarization
    (`xcrun notarytool`), cross-platform GitHub Actions build matrix, App Store
    submission. Start once editor + ribbon parity (P0–P1) is solid.

---

## 4. Recommended next steps (for the following session)

1. ✅ **Font/highlight color pickers (P0-4)**, **Semantic HTML gallery (P0-5)**,
   **image insert (P1-7)**, **word count + find (P1-8)** — all done this session (the
   editor-content parity band). The Home/Insert-tab editing controls are now wired.
2. **Font combo refinement:** switch font size from the HTML 1–7 scale to explicit px
   and push the current selection's font family/size + color back into the ribbon
   combos/pickers (extend `FormatState` + `getState()`), same pattern as toggle sync.
3. **Find polish:** add Find Previous / match-count readout / Replace (single) and a
   live match-highlight-count; wire an in-editor find bar (currently a non-modal
   dialog). Image: alt-text/size prompt and the P2 upload-on-publish rewrite.
4. **Account setup / publish UI (P2-9/10):** blog-account model + `MacCredentialStorage`
   wiring, `PostAndPublish`/`PostAsDraft`/`SelectBlog` UI (the publish pipeline slice
   is already ported — see §7).
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
- Live blog publish (opt-in; posts to a real endpoint): set `OLW_LIVEBLOG_ENDPOINT`,
  `OLW_LIVEBLOG_BLOGID`, `OLW_LIVEBLOG_USER`, `OLW_LIVEBLOG_PASS` (optionally
  `OLW_LIVEBLOG_PUBLISH=true`; defaults to posting an unpublished draft) then
  `dotnet test ... --filter "Category=LiveBlog" -- NUnit.Explicit=true`

Default run status: **270 passed / 0 failed.** WebView-category, `PublishTdd`, and
`LiveBlog` tests are `[Explicit]` (excluded from the default run) so the headless gate
stays green. The publishing-completion band lifted the count 235 → 270 with pure/headless
coverage (all offline via `FakeBlogClient` / a fake RSD fetcher):

- **Image upload (Group G):** `GroupG_ImageUploadTests` — `ImagePublisher` scan (mime +
  byte decode, dedup, jpeg→jpg), rewrite/upload (no-op, single/duplicate/multiple images,
  numbered filenames, upload-failure aborts), integration through
  `EditorContentPublisher`/`BlogAccountService`, and the real `newMediaObject` struct shape.
- **Categories (Group H):** `GroupH_CategoryTests` — `getCategories` response parsing
  (title/description/categoryName + categoryId permutations, parent, indented XML, empty),
  `FakeBlogClient.GetCategories`, selected categories reaching the `newPost` struct, and
  `CategoryDialog.MergeSelection` (checked + custom, dedup/trim).
- **RSD detection (Group I):** `GroupI_RsdDetectionTests` — `FindRsdUrl` (EditURI rel,
  rsd+xml type, relative/absolute resolution, none), `ParseRsd` (engine + apis, relative
  apiLink, no-apis), `SelectMetaWeblogApi`, and the full `Detect` flow via a fake fetcher
  (success, no-link, fetch-failure). A live detection test is `[Explicit]`.
- **Re-publish (Group J):** `GroupJ_RepublishTests` — `PublishOrEdit` new-vs-edit and the
  `BlogAccountService` republish-edits-same-post (no duplicate `NewPost`) flow.

Earlier, the Insert-tab + preview + selection-state band lifted the count 160 → 235
with pure/headless coverage:

- **Preview (B4):** `GroupB_RoundtripTests` — the previously `[Explicit]` failing
  preview stub is replaced by headless `PreviewRenderer` composition tests (article
  wrapper + body survival + more-marker join + empty-body safety); a live display
  test stays `[Explicit]`.
- **Insert Table:** `GroupF_TableTests` — `TableBuilder` dimensions, header/body
  split, well-formedness, width normalization, clamping + a dialog-defaults UI test.
- **Insert Video:** `GroupF_VideoTests` — `VideoEmbedBuilder` URL normalization
  (YouTube/Vimeo/shorts/embed), iframe-src extraction, protocol-relative + generic
  URLs, rejection of unembeddable input, embed well-formedness.
- **Emoticons:** `GroupF_EmoticonTests` — `EmoticonGallery` catalog + insertion payload.
- **Selection state:** `GroupA_SelectionStateTests` — `ParseFormatStateJson` +
  `NormalizeFontName`/`NormalizeReportedColor` (rgb→hex).
- **Paste / breaks:** `GroupF_PasteAndBreaksTests` — `PasteCleaner` plain-text +
  clean-HTML sanitizers, clear-break, and that the inserted extended-entry marker is
  still recognized by the publish split.

Earlier bands: the accounts/publishing band lifted the count 133 → 160 (new
`GroupE_AccountTests` + the flipped Group D D3 + `AccountDialog` validation), on top of
the editor-content parity band that had lifted it 63 → 133 with pure/headless coverage:

- **Semantic styles (P0-5):** `GroupA_ToolbarGapTests` was flipped from documenting
  the h4-h6/pre *gap* to asserting the now-reachable full range via
  `EditorPanel.MapHeadingIndexToTag` / `SemanticHtmlStyles`.
- **Color (P0-4):** `GroupA_ColorCommandTests` — command→`execCommand` mapping and
  `#RRGGBB` hex normalization.
- **Image (P1-7):** the previously `[Explicit]` Group D `ImageInsertDialog_InsertsImgTag`
  is implemented as `ImageInsert_*` (data-URI `<img>` build, MIME guess, alt escaping).
- **Word count (P1-8):** the previously `[Explicit]` `WordCount_CountsWords` is
  implemented; full coverage in `GroupD_WordCountTests`.
- **Find (P1):** `GroupD_FindReplaceTests` — `TextFinder` case/whole-word/wrap +
  HTML-aware Replace All (tags preserved) + dialog field capture (headless UI).

There are no remaining `[Explicit]` non-WebView TDD stubs: `AccountSetup_StoresCredentials`
(Group D D3) is now implemented and green. The only opt-in target left is the new
`LiveBlogPublishTests` (`[Category(LiveBlog)]`), which performs a real `metaWeblog.newPost`
against an endpoint supplied via `OLW_LIVEBLOG_*` env vars (see below). Earlier milestones:
the two Group C `RealPipeline_*` probes and `GroupD_DraftLifecycleTests` remain green.

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
| A10: toolbar HeadingCombo + SemanticHtmlGallery reach Normal/h1–h6/pre | `GroupA_ToolbarGapTests` (real `MapHeadingIndexToTag`/`SemanticHtmlStyles`) | ✅ pass |
| A: font color / highlight — command→execCommand map + `#RRGGBB` serialization | `GroupA_ColorCommandTests` | ✅ pass |
| A: createLink / link text+title+new-window + escaping | `GroupA_EditorCommandTests` (live) + `GroupA_LinkHtmlTests` (pure) | ✅ pass (pure) / ⏭ WebView (live) |
| A: horizontal rule, clear formatting, partial-selection bold | `GroupA_EditorCommandTests` | ⏭ WebView |
| A: font family / size | `GroupA_EditorCommandTests` | ⏭ WebView |
| A16: well-formedness / publish-readiness gate | `GroupA_WellFormednessTests` (samples) + `GroupA_EditorCommandTests` (live) | ✅ pass (samples) / ⏭ WebView (live) |
| A18: getState sync (bold on→true, off→false; blockTag) | `GroupA18_GetStateTests` | ⏭ WebView |
| B1–B3: source/preview round-trip (WYSIWYG↔source, hand-edited h2+ul) | `GroupB_RoundtripTests` (via `EditorPanel.FormatHtml`) | ✅ pass |
| B4: preview render — article wrapper + body survival + more-marker join + empty-body | `GroupB_RoundtripTests` (real `PreviewRenderer`) | ✅ pass |
| B: live round-trip + live preview display | `GroupB_RoundtripTests` | ⏭ WebView |
| F: Insert Table — dimensions, header/body split, width, well-formed, clamp, dialog | `GroupF_TableTests` (real `TableBuilder`) | ✅ pass |
| F: table-tools ops (insert/delete row+column, delete table) | editor.html bridge + `WebViewEditor.HandleCommandAsync` | ⏭ WebView |
| F: Insert Video — URL→embed normalization, iframe extract, generic, reject, well-formed | `GroupF_VideoTests` (real `VideoEmbedBuilder`) | ✅ pass |
| F: Insert Emoticon — catalog + Unicode insertion payload | `GroupF_EmoticonTests` (real `EmoticonGallery`) | ✅ pass |
| F: Paste Special — plain-text + clean-HTML sanitizers (drop scripts/attrs/js: URLs) | `GroupF_PasteAndBreaksTests` (real `PasteCleaner`) | ✅ pass |
| F: Insert Clear Break + Extended Entry marker recognized by publish split | `GroupF_PasteAndBreaksTests` (real `EditorMarkup`/`ExtendedEntry`) | ✅ pass |
| A: selection-state parse + font/color normalization (rgb→hex, font-stack) | `GroupA_SelectionStateTests` (real `WebViewEditor.ParseFormatStateJson`) | ✅ pass |
| C: post model, MetaWeblog payload (description=MainContents, publish flag), draft, extended split | `GroupC_PublishTests` (real `OpenLiveWriter.Publishing` + `FakeBlogClient`) | ✅ pass (real types) |
| C: `XmlCharacterHelper` XML-char scrub | `GroupC_PublishTests` (real ported helper) | ✅ pass |
| C: real ported pipeline present (app refs `Publishing`, `WebViewEditor.PublishAsync`) | `GroupC_PublishTests` (reflection probes) | ✅ pass |
| D1: LinkDialog validation (Insert disabled for empty/`https://`) | `GroupD_DialogTests` (logic + headless UI) | ✅ pass |
| D4: draft save/load round-trip (DOM equiv), overwrite, MRU order, delete, corrupt/missing, BlogPost interop, DraftSession dirty tracking | `GroupD_DraftLifecycleTests` (real `FileDraftStore`/`PostDocument`/`DraftSession`, temp dir) | ✅ pass |
| D2: image insert from file — data-URI `<img>` build, MIME guess, alt escaping | `GroupD_DialogTests` (`ImageInsert_*`) | ✅ pass |
| D5: word count — words/chars/paragraphs from HTML, entity decode | `GroupD_WordCountTests` + `GroupD_DialogTests` | ✅ pass |
| D: find / replace — case, whole-word, wrap, HTML-aware Replace All, dialog capture | `GroupD_FindReplaceTests` (pure `TextFinder` + headless UI) | ✅ pass |
| D3: account setup stores credentials (metadata→store, password→credential store, not JSON) | `GroupD_DialogTests` (`AccountSetup_StoresCredentials`) + `AccountDialog` save-enable rule | ✅ pass |
| E: account store round-trip (save/load/list/delete, overwrite single-file, missing dir, corrupt-file skip + Load throws) | `GroupE_AccountTests` (real `FileAccountStore`, temp dir) | ✅ pass |
| E: current-blog selection persists across store instances; corrupt pointer ⇒ none | `GroupE_AccountTests` | ✅ pass |
| E: credential seam (store/retrieve/overwrite/delete) | `GroupE_AccountTests` (`InMemoryCredentialStore` fake) | ✅ pass |
| E: full publish flow — NewPost payload (title/MainContents/mt_text_more/categories) + publish flag for Publish and Post-as-draft; returned id recorded; no-account/no-credential graceful | `GroupE_AccountTests` (`BlogAccountService` + `FakeBlogClient`) | ✅ pass |
| E: BlogClientFactory builds MetaWeblog client with account options; unsupported provider throws | `GroupE_AccountTests` | ✅ pass |
| Live: real `metaWeblog.newPost` against a blog endpoint from `OLW_LIVEBLOG_*` | `LiveBlogPublishTests` | ⏭ `[Explicit]` / `[Category(LiveBlog)]` |

### Production seams added for testability

Small, behavior-preserving `internal`/`public` seams in `App.Avalonia` (exposed via
`InternalsVisibleTo`): `WebViewEditor.BuildAnchorHtml`/`EscapeHtml*` (link build +
escaping), `EditorPanel.FormatHtml` (source view formatter), `LinkDialog.IsValidUrl`,
`EditorPanel.MapHeadingIndexToTag` + `SemanticHtmlStyles` (block-style mapping),
`WebViewEditor.ColorCommandFor`/`NormalizeColor` (color command + hex serialization),
`WebViewEditor.BuildImageHtml*`/`GuessImageMimeType` (image build), `WordCounter`
(HTML→plain-text counts), `TextFinder` (find/replace incl. HTML-aware Replace All),
`PreviewRenderer` (preview article composition), `TableBuilder` (table HTML +
width normalization), `VideoEmbedBuilder` (URL→embed normalization + responsive
iframe), `EmoticonGallery` (emoji catalog + payload), `PasteCleaner` (plain-text +
clean-HTML sanitizers), `EditorMarkup` (clear-break + extended-entry marker), and
`WebViewEditor.ParseFormatStateJson`/`NormalizeFontName`/`NormalizeReportedColor`
(caret-state parsing for the ribbon combos).

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

1. ✅ **Accounts/credentials wiring.** Done — see §9. `BlogAccount` +
   `FileAccountStore` + an `ICredentialStore` seam over `MacCredentialStorage`
   (Keychain) construct a `MetaWeblogXmlRpcClient` with real endpoint/user/pass.
   *(Interactive `ICredentialsPrompter`-style re-auth is still a follow-up.)*
2. **Live transport validation.** *(pending — manual step)* Exercise `NewPost`/`EditPost`
   against a real MetaWeblog endpoint (self-hosted WordPress / `utilities/BlogServer`) via
   the opt-in `LiveBlogPublishTests` (`OLW_LIVEBLOG_*`). Handle redirects, encoding, and
   auth headers via the existing `HttpClient` path.
3. **Additional providers.** Port Atom (AtomPub), WordPress, and Blogger v3 clients
   (larger; some depend on MSHTML detection that must be abstracted first).
   `BlogClientFactory` currently supports only `MetaWeblog` and throws for others.
4. ✅ **Draft persistence + document model.** Done — see §8. The publish path can
   now build a `BlogPost` directly from the edited `PostDocument`
   (`PostDocument.ToBlogPost()`), so `PostAsDraft` maps to a local draft save while
   `PostAndPublish` reuses the same document for the transport.
5. ✅ **Publish UI.** Done — see §9/§10. `PostAndPublish`/`PostAsDraft`(+EditOnline)
   commands, `SelectBlog` (host-populated ribbon dropdown + picker), a **category picker**
   (`ShowCategoryPopup`), and progress/errors surfaced via a `MessageDialog` + status bar.
6. ✅ **Provider auto-detection.** Done — MSHTML-free `RsdServiceDetector` (homepage
   `EditURI` link → `rsd.xml` → MetaWeblog `apiLink`) with a **Detect** button in the
   Account dialog; HTTP behind an `IRsdHttpFetcher` seam. See §10. *Follow-up: broaden
   heuristics (WordPress `/xmlrpc.php` guess, `<meta>` generator hints) and more engines.*
7. ✅ **Image upload-on-publish.** Done — `ImagePublisher` uploads inline data-URI `<img>`s
   via `metaWeblog.newMediaObject` and rewrites them to hosted URLs before the post is
   sent (dedup, no-op, upload-failure aborts). See §10.
8. **Fold in `feature/macbuild` abstractions** (`ICredentialsPrompter`,
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

---

## 9. Blog accounts + publishing UI (macOS)

Reproduces the Windows add-account / select-blog / publish flow against the ported
transport (§7), keeping all account logic cross-platform and testable with fakes.

### 9.1 Account / credential design (cross-platform, in `OpenLiveWriter.Publishing.Accounts`)

| Type | Role |
| --- | --- |
| `BlogAccount` | Account metadata: `Id`, `DisplayName`, `HomepageUrl`, `ApiEndpointUrl` (MetaWeblog XML-RPC), `BlogId`, `Username`, `ProviderType` (default `MetaWeblog`), `SupportsPages`/`SupportsCategories`/`SupportsExtendedEntries`. **Carries no secret** — the password is never serialized here. |
| `IAccountStore` / `FileAccountStore` | Persistence seam + JSON-file-per-account impl (`{id}.olaccount.json`, temp-file-then-move). Robust: missing dir ⇒ empty store; corrupt account ⇒ `Load` throws `AccountStoreException` but `List` skips it; the current-selection pointer (`current.json`) resolves to "none" if corrupt. Also persists `CurrentAccountId` (last-selected blog). |
| `ICredentialStore` / `InMemoryCredentialStore` | Secret-storage seam (`Store`/`Retrieve`/`Delete`/`Exists`) + in-memory fake for tests. Mirrors `Platform.ICredentialStorage` but lives in the publishing assembly so account logic has no Platform dependency. |
| `BlogClientFactory` | Builds a `MetaWeblogXmlRpcClient` from a `BlogAccount` + password (throws `NotSupportedException` for non-MetaWeblog providers). |
| `BlogAccountService` | UI-agnostic orchestrator: `SaveAccount(account, password)` (metadata→store, password→credential store; makes first account current), `ListAccounts`, `DeleteAccount` (removes credential too), `CurrentAccount`/`SetCurrentAccount`, `CreateClient`, and the account-aware `Publish(document, editorHtml, publish)` returning a `PublishOutcome` (Success / NoAccountConfigured / NoCredential). Client factory is injectable for tests. |

**What lives where:** account metadata → JSON on disk; **password → credential store
(macOS Keychain)**, keyed by the account id. Verified by a test asserting the secret is
absent from the account JSON.

### 9.2 File location + Keychain wiring (platform-resolved, never hardcoded)

`AccountServiceFactory.CreateDefault()` (in `App.Avalonia`) resolves the folder via
`PlatformContext.Services.GetApplicationDataDirectory()` + `"Accounts"` — i.e.
`~/Library/Application Support/OpenLiveWriter/Accounts` on macOS. The credential store is
`PlatformCredentialStore` (an `App.Avalonia` adapter that implements the publishing-layer
`ICredentialStore` over `Platform.ICredentialStorage` = `MacCredentialStorage`, the
Keychain via the `security` CLI). **Tests never touch the Keychain** — they inject
`InMemoryCredentialStore`.

### 9.3 Dialogs + command wiring (`App.Avalonia`)

- **`AccountDialog`** — add/configure a blog account (name, blog URL, MetaWeblog API
  endpoint, blog id, username, password); Save enables only with a non-trivial endpoint +
  username (+ password for new accounts; blank on edit keeps the stored secret). Endpoint
  is entered manually (auto-detection TODO).
- **`AccountManagerDialog`** / **`SelectBlogDialog`** / **`MessageDialog`** — manage
  (add/edit/delete/set-current), quick blog pick, and OK-only info/error surface.
- **`MainWindow`** routes publish/account `CommandId`s through `TryHandlePublishCommandAsync`:
  `AddWeblog` → Add; `ConfigureWeblog`/`Accounts` → Manager; `SelectBlog` → picker;
  `PostAndPublish` → publish (publish=true); `PostAsDraft`/`PostAsDraftAndEditOnline` →
  server draft (publish=false). The ribbon blog-selector `CompactDropDown` is host-populated
  from stored accounts and raises the current-blog change (persisted). Publish pulls the
  live body via `WebViewEditor.PublishAsync`, records the returned server post id on the
  document, and surfaces "no account" / "no blog" / "missing password" / transport errors
  gracefully.

### 9.4 Tests

`GroupE_AccountTests` (default headless suite, temp dir, in-memory credential fake):
account store round-trip + overwrite/list/delete + missing/corrupt handling, current-blog
selection persistence across store instances, credential seam, `BlogAccountService`
save/update/delete/current, the **full publish flow** through a `FakeBlogClient` (asserts
NewPost gets the right title/MainContents/`mt_text_more`/categories + publish flag for both
Publish and Post-as-draft and records the returned post id), and `BlogClientFactory`.
Group D **D3** (`AccountSetup_StoresCredentials`) is implemented + green. The opt-in
`LiveBlogPublishTests` (`[Explicit]`/`[Category(LiveBlog)]`) performs a real
`metaWeblog.newPost` from `OLW_LIVEBLOG_*` env vars — the manual live-validation step.

### 9.5 What still needs live validation / follow-up

- **Real endpoint round-trip** (the one manual step): run `LiveBlogPublishTests` against a
  self-hosted WordPress/MetaWeblog endpoint to confirm auth, encoding, and redirects —
  including a real `newMediaObject` image upload and `getCategories` fetch.
- **Live RSD detection**: run the `[Explicit]` `LiveDetect_FromHomepage_FindsEndpoint`
  (`OLW_RSD_HOMEPAGE`) against a real blog.
- **Remaining**: **additional providers** (Atom/WordPress/Blogger — `BlogClientFactory`
  still only builds MetaWeblog), broader **auto-detection heuristics** (WordPress
  `/xmlrpc.php` guess, `<meta name="generator">` hints, WLW manifest), interactive
  credential re-prompt, and image alt-text/resize on insert.

---

## 10. Publishing completion band (macOS) — images, categories, RSD, re-publish

Completes the publishing story so real posts (images + categories) publish correctly,
and re-publishing edits the same server post. All logic is cross-platform and offline-
testable with fakes; only true live round-trips remain manual (`[Explicit]`).

### 10.1 Image upload-on-publish (`OpenLiveWriter.Publishing.ImagePublisher`)

The editor embeds inserted images as base64 `data:` URIs. Before a post is transmitted,
`ImagePublisher` scans the body for those data-URI `<img>`s, uploads each **unique** image
via `IBlogClient.NewMediaObject` (`metaWeblog.newMediaObject`, faithful `name`/`type`/`bits`
struct + new `XmlRpcBase64`), and rewrites every `src` to the returned hosted URL. No-ops
when there are no images; identical images upload once and share the URL; an upload failure
raises `BlogClientPublishException` so the publish aborts rather than sending broken HTML.
Runs on both the new-post and edit paths (`EditorContentPublisher` /
`WebViewEditor.PublishAsync` / `BlogAccountService.Publish`).

### 10.2 Categories

`BlogPostCategory` model + `IBlogClient.GetCategories(blogId)` (`metaWeblog.getCategories`)
with a pure, fixture-tested `MetaWeblogXmlRpcClient.ParseCategories` tolerant of the common
member permutations (description/title/categoryName; categoryid/categoryId; parentId) and
indentation. The shell's `ShowCategoryPopup` command opens `CategoryDialog` — a checklist
pre-checked with the current selection, a free-text field for categories the provider
didn't list, and a graceful "none reported" fallback — storing the chosen names on the
draft so they flow into the inline `newPost` `categories` array.

### 10.3 Provider endpoint auto-detection (RSD)

`RsdServiceDetector` (in `OpenLiveWriter.Publishing.Accounts`) is a cross-platform,
MSHTML-free port: pure `FindRsdUrl` (homepage `<link rel="EditURI">` / `application/rsd+xml`
→ resolved RSD URL), `ParseRsd` (engine name/link, `<api>` name/apiLink/blogID, relative-URL
resolution, tolerant of trailing junk), and `SelectMetaWeblogApi`. The full `Detect` flow is
orchestrated behind an `IRsdHttpFetcher` seam (`HttpRsdFetcher` default) so unit tests run
offline; a **Detect** button in `AccountDialog` auto-fills the endpoint + blog id from the
Blog URL, with manual override retained. The real-network test is `[Explicit]`.

### 10.4 Re-publish → `editPost`

`EditorContentPublisher.PublishOrEdit` and the publish path record the server
`PublishedPostId`; a subsequent publish of the same document to the same blog edits that
post via `metaWeblog.editPost` (no duplicate). The shell reflects publish-vs-update in its
progress/result messages.

### 10.5 Tests

Groups **G** (image upload), **H** (categories), **I** (RSD detection), **J** (re-publish)
— see §6. `FakeBlogClient` now records `newMediaObject` uploads + serves `getCategories`.
