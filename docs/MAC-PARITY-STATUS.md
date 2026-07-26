# Open Live Writer — macOS Parity Status

Single source of truth for the Mac (Avalonia) port. Goal: **feature and visual
parity with Open Live Writer for Windows.** Update this file as work lands.

> See also `docs/MAC-VIABILITY-ASSESSMENT.md` (2026-07-20) — an independent,
> user-facing gap review. This file tracks what has *landed*; the assessment
> tracks what a Windows switcher would actually hit (P0 trust breakers first).

Branch: `milestone4/webview-wysiwyg` · Runtime: .NET 10 / Avalonia · Last verified: 2026-07 (**Theme band (P1-3):** Blog Account tab's "Use Theme" (per-account persisted toggle) + "Update Theme" (forced re-harvest) are live — Preview layers the blog homepage's harvested stylesheets over the neutral article style via a proxy-aware `ThemeStyleCache`, degrading to neutral on fetch failure; Preview tab's Close Preview wired; **Picture Tools band (P1-2):** click-to-select images → Picture Tools contextual tab; size spinners + aspect lock + Small/Medium/Large/Original presets, rotate CW/CCW baked into pixels (SkiaSharp), numeric crop dialog, baked Black & White / Sepia / Sharpen / Blur / Emboss effects, debounced contrast, text watermark dialog, border toggle, Picture Properties dialog (alt/title, Link To none/source/URL, alignment, margins, border) — applied as inline attrs/styles on the selected `<img>` with baked ops re-embedded as PNG data-URIs; only tilt/recolor stay disabled (see §12); **Server/publishing band:** Open from Blog — `metaWeblog.getRecentPosts`/`getPost` + `wp.getPages` into `OpenFromBlogDialog` (Posts/Pages, 10/25/50) so server content opens editable and re-publish edits in place; pages publish as pages via `wp.newPage`/`wp.editPage`; **WordPress provider** + detection heuristics (`/xmlrpc.php` probe, engine-aware RSD); publish date via Post Properties (F2) → `dateCreated` on post+page structs; **spelling band:** Hunspell engine (`WeCantSpell.Hunspell` + embedded en-US dictionaries), F7 spelling dialog (suggestions, ignore/add-to-dictionary, change/change-all), check-before-publish gate; **content band:** Picture from the Web (remote `<img>`, no base64), Print / Print Preview (print-styled doc → native WKWebView print panel, temp-PDF/browser handoffs); **Shell trust band:** macOS NativeMenu menu bar (File/Edit/View/Help + accelerators, Set Categories reachable), unsaved-changes close prompt, draft autosave (AutoRecover), handled-command registry with dead commands visibly disabled, Debug tab hidden unless `OLW_DEBUG_RIBBON=1`, real Cut/Copy/Paste routing; **Editor-bridge band:** debounced payload-free content sync, JSON-based JS escaping, px font sizes + caret reflection, find previous / "n of m" count / single-replace; **Publish band:** fully async XML-RPC transport (no UI freeze), view-post/close-window after publish, preferences dialog shows only enforced options, Account dialog Test Connection; on top of the Options/Preferences band: **JSON `FileSettingsPersister`**, tabbed **Preferences** dialog, **Options** command; Publishing-completion band: **image upload-on-publish** via `newMediaObject`, **blog categories** fetch + picker, **RSD endpoint auto-detection**, **re-publish → `editPost`**; Insert-tab band: Preview render, Insert Table + table-tools ops, web-video embeds, emoticons, paste-special/clean paste, clear-break/extended-entry, caret font/size/color/block reflection)

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
| M5 (#1002) | Packaging + Store submission (.app/DMG, sign/notarize, CI matrix, App Store) | 🟡 In progress |

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
  and a status bar. **UI layout (2026-07):** window is freely resizable with
  `MinWidth`/`MinHeight` 800×600; last size/position persisted via
  `AppPreferencesStore` WindowBounds (clamped to screen working area); ribbon + title +
  editor + status bar reflow so the WebView fills remaining space; ribbon tabs/groups
  scroll horizontally; secondary format strip removed (ribbon is primary — editor chrome
  keeps Edit/Source/Preview only); in-editor find bar (Cmd/Ctrl+F); status bar stays
  pinned at a fixed height; system window decorations (no client-area extend under
  traffic lights).
- **Editor:** WebView (WKWebView) `contenteditable` surface (`editor.html`) with a
  JS bridge (`OLWBridge`) for `execCommand`, selection save/restore, get/set content.
- **View toggle:** Edit / Source / Preview. Source shows formatted HTML round-tripped
  from the WebView; **Preview now renders** the post body as it would look published
  (a neutral "article" layout composed by `PreviewRenderer` and shown in a read-only
  WebView, extended-entry marker joined). With the Blog Account tab's **Use Theme**
  toggle on for the current blog, Preview layers the blog's harvested homepage
  stylesheets over the neutral style (see §13).
- **Insert tab:** Insert Table (rows×columns + header row + width via `TableBuilder`)
  with basic Table Tools row/column insert-delete + delete-table bridge ops; Insert
  Video as a modern responsive `<iframe>` web embed (YouTube/Vimeo/generic URL or
  pasted embed normalized by `VideoEmbedBuilder` — replaces the dead Flash/service
  paths); Insert Emoticon (Unicode emoji picker); Paste Special (clean paste:
  plain-text / safe-HTML via `PasteCleaner`); Insert Clear Break + Insert Extended
  Entry (`<!--more-->`, shared with the publish split).
- **Caret-state reflection:** the ribbon Font family/size combos follow the caret's
  actual block tag / font / size (and the state also carries fore/highlight color),
  via the `stateChanged` → `FormatState` pipeline.
- **Editor commands wired to the ribbon** (via `WebViewEditor.HandleCommandAsync`):
  - Character: Bold, Italic, Underline, Strikethrough, Subscript, Superscript, Clear Formatting
  - Lists/indent: Bullets, Numbers, Indent, Outdent
  - Paragraph: Align Left/Center/Right, Justify, Blockquote (toggle)
  - Editing: Undo, Redo, Select All
  - Insert: Horizontal line; `createLink`/`insertHtml` bridge methods exist
  - Block format: `formatBlock` — full semantic range (Normal/p, Heading 1-6,
    Preformatted) via the ribbon SemanticHtmlGallery flyout
  - Color: text color (`foreColor`) + highlight (`hiliteColor`, backColor fallback)
    via ribbon color-swatch flyouts (standard + highlight palettes)
  - Insert: image from file — file picker → inline base64 data-URI `<img>`
  - **Picture Tools (P1-2):** clicking an image selects it as a unit → the
    Picture Tools > Format contextual tab activates; width/height spinners with
    aspect-ratio lock, Small/Medium/Large/Original size presets, rotate CW/CCW
    (baked into pixels via SkiaSharp), numeric crop dialog, Black & White /
    Sepia effects (baked), border toggle, and a Picture Properties dialog
    (alt/title, Link To none/source/URL, alignment, uniform margin, border
    width+color). See §12.
  - Editing: Word Count (statistics dialog) + Find (in-editor bar) / Find & Replace
    (dialog fallback for Replace All; native in-page highlight + HTML-aware Replace All)
- **Format-state reporting:** `OLWBridge.getState()` reports bold/italic/underline/
  strike/sub/super/lists/alignment/blockTag (consumed by `FormatState`).
- **Live toggle-state sync:** editor posts `stateChanged` messages (via the Avalonia
  WebView `window.invokeCSharpAction` bridge) → `WebViewEditor.FormatStateChanged` →
  ribbon toggle buttons reflect the caret's current formatting.
- **Insert Link:** modal `LinkDialog` (URL + text + title + open-in-new-window),
  wired to `Ctrl+K` and the `InsertLink` ribbon command.
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
- **Options / Preferences:** JSON `FileSettingsPersister` on macOS (platform-resolved
  `~/Library/Application Support/OpenLiveWriter/Settings/`); tabbed **Preferences** dialog
  (General, Editing, Spelling, Web Proxy, Accounts) wired to the File menu **Options**
  command. **Enforced:** spelling toggle → `SetSpellcheckEnabledAsync`; status-bar word
  count → General preference; web proxy → MetaWeblog/RSD/image-upload HTTP via
  `PublishingHttpClientFactory`; autoreplace (smart quotes on typing + paste transforms) →
  `AutoreplaceController`/`AutoreplaceTransformer`; title/category publishing reminders →
  publish flow. **Stored only (not yet enforced):** post-window behavior, view-after-publish,
  close-on-publish, tag reminder, AutoRecover interval, paragraph-tag preference, emoticon
  image autoreplace (text emoticons on paste only). See §11.
- **Plug-ins (stub):** Add/Manage Plug-ins ribbon commands show an informational dialog —
  `OpenLiveWriter.Extensibility` compiles on macOS but the WinForms plug-in host is not ported.

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
    chrome, contextual tabs actually appearing on selection). *(M4)* — *incremental:*
    group-label weight/spacing polish + status bar blog name / word-count panes landed;
    **UI layout pass:** MainWindow min size + WebView stretch on resize, ribbon tab/
    group horizontal scroll (no overflow clip), pinned status bar, dialog MinWidth/
    resizable list dialogs.
    **UI polish pass (this band):** dual-toolbar collapse (ribbon primary; slim view-
    toggle chrome); adaptive compact ribbon below ~960px; readable glyph buttons
    (styled B/I/U instead of gray squares); window size/position persistence; system
    chrome (no extend-into-decorations); in-editor find bar.
    **Layout quality pass:** hit targets ≥24px; find-bar horizontal scroll; ribbon
    **More** overflow menu when compact/overflowing; headless `GroupP_LayoutHarnessTests`
    across 800–1920 (see `docs/UI-LAYOUT-QA.md`).
    **Remaining visual/usability debt:** real Fluent/SVG ribbon icons; Find match-count
    readout; print UI; Fluent-level overflow gallery (More lists commands today).
12. ✅ **Tables, video, emoticons, preview, paste-special, breaks, maps, tags, spellcheck UI, contextual tabs.** Done across recent bands.
    **Theme band (P1-3, this pass):** the Blog Account tab's **Use Theme** /
    **Update Theme** buttons are live and Preview can render with the blog's real
    stylesheets — see §13. Honest limitation vs Windows: no template region
    detection, so theme rules scoped to the blog's post containers don't apply.
    **Remaining:** full plug-in host (stub dialog only). *(M4)*
13. **M5 packaging:** `.app` bundle foundation (`build-mac.sh` + `mac-build.yml` CI
    artifact), code signing / notarization (`xcrun notarytool`), DMG, App Store
    submission. *Started:* self-contained `osx-arm64` publish + `CFBundleName`
    "Open Live Writer" `.app` assembly; optional DMG via `OLW_CREATE_DMG=1`;
    `scripts/validate-live-blog.sh` for opt-in live tests; sign/notarize env vars
    documented (not required in CI).

---

### UI layout (shell)

| Area | Fixed | Remaining |
| --- | --- | --- |
| MainWindow resize | `CanResize`, `MinWidth` 800 / `MinHeight` 600, DockPanel fill chain; WindowBounds persist + screen clamp | — |
| Editor WebView | ContentControl Stretch + NativeWebView Stretch so WKWebView tracks resize; headless layout placeholder for harness | Native control edge cases on extreme DPI |
| Editor chrome | Slim view toggles only (Edit/Source/Preview); format commands on ribbon; MinHeight ≥24 | — |
| Find | In-editor find bar (Cmd/Ctrl+F) with horizontal scroll at narrow widths; Replace dialog for Replace All | Match-count readout / true reverse-search polish |
| Ribbon | Tab/group horizontal scroll; compact Small layout below ~960px; glyph buttons; **More** overflow menu | Real Fluent/SVG icons; richer overflow gallery |
| Layout harness | `GroupP_LayoutHarnessTests` — sizes 800×600 … 1920×1080; status/editor/ribbon/find invariants | — |
| macOS chrome | `WindowDecorations=Full`, `ExtendClientAreaToDecorationsHint=false` | Custom title-bar inset only if extending client area later |
| Status bar | Pinned bottom, fixed height, ellipsis on long blog/status text; `x:Name=StatusBar` | — |
| Dialogs | Min sizes; Preferences/Accounts/Drafts/Categories/SelectBlog resizable | Visual polish vs Windows options UI |

## 4. Recommended next steps (for the following session)

1. ✅ **Font/highlight color pickers (P0-4)**, **Semantic HTML gallery (P0-5)**,
   **image insert (P1-7)**, **word count + find (P1-8)** — all done this session (the
   editor-content parity band). The Home/Insert-tab editing controls are now wired.
2. **Font combo refinement:** switch font size from the HTML 1–7 scale to explicit px
   and push the current selection's font family/size + color back into the ribbon
   combos/pickers (extend `FormatState` + `getState()`), same pattern as toggle sync.
3. **Find polish:** Find bar landed (Cmd/Ctrl+F); still want Find Previous /
   match-count readout / single Replace and live match-highlight-count. Image:
   alt-text/size prompt (upload-on-publish already done).
4. **Account setup / publish UI (P2-9/10):** ✅ Done (live validation remaining).
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
  `scripts/validate-live-blog.sh` or
  `dotnet test ... --filter "Category=LiveBlog" -- NUnit.Explicit=true`

Default run status: **784 passed / 0 failed** (includes **Group P** layout harness + UiReview captures, **Group Q** shell/menu/autosave/close-prompt/handled-commands, **Group R** editor bridge, **Group S** async transport + connection verifier, **Group T** server posts/pages/WordPress, **Group U** web-image/print/publish-date, **Group V** Picture Tools, **Group W** themed preview, **Group N** spelling flow, **Group Y/Z** pixel baking —
15 cases across 800×600…1920×1080). WebView-category, `PublishTdd`, and
`LiveBlog` tests are `[Explicit]` (excluded from the default run) so the headless gate
stays green. Layout quality docs: `docs/UI-LAYOUT-QA.md`.

- **Layout harness (Group P):** `GroupP_LayoutHarnessTests` — MainWindow min size;
  status bar / editor placeholder / Edit·Source·Preview bounds; ribbon tab+content
  scroll-or-fit; compact mode below 960px; More overflow; find-bar scroll; no
  zero-sized effectively-presented buttons. Uses `WebViewEditor.UseLayoutPlaceholder`.
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
- **Picture Tools (Group V):** `GroupV_PictureToolsTests` — `getState()` image-payload
  parsing (attrs/styles/link normalization), `ImageEditBuilder` applyImageAttrs JSON
  (set-members only, ClearSize nulls, border pair), aspect-ratio math + preset sanity,
  alignment normalization, Picture-properties link-choice mapping, and headless
  ribbon wiring (width/height spinner events + reflection, size-preset and
  Link To dropdown flyouts with dead items disabled). **Pixel baking (Groups Y/Z):**
  `GroupY_ImageEditingTests` — SkiaSharp op correctness (rotate/crop/resize/
  grayscale/sepia with pixel spot-checks), data-URI round-trip, selected-image
  JSON parsing, command registration, and the effects dropdown flyout;
  `GroupZ_ImageEffectsTests` — watermark (quadrant placement, zero-opacity
  identity, validation), contrast (identity at 0, mid-gray invariance, extreme
  push/pull, range checks), sharpen/blur/emboss convolutions (flat invariance,
  edge contrast, variance reduction, mid-gray bias), and the new command
  registration. JS-side
  selection/apply/baked-replacement/link tests are `[Explicit]` WebView cases.
- **Themed preview (Group W):** `GroupW_ThemingTests` — `ThemeStyleExtractor`
  (relative/absolute/protocol-relative hrefs, rel-token variants, non-stylesheet
  links ignored, dedup, inline `<style>` blocks, no-stylesheet/empty input),
  `ThemeStyleCache` (memory + disk round-trip, force refresh, homepage-change
  invalidation, failed refresh never poisons, throwing fetcher → null, corrupt
  disk file → miss), themed `PreviewRenderer` composition (styles present iff a
  theme is supplied, attribute escaping), and headless shell wiring (Use Theme
  toggle persists per-account, Update Theme status reporting, graceful
  no-account/fetch-failure messages, Close Preview returns to Edit view).

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

---

## 11. Options / Preferences (macOS)

Cross-platform preference model + macOS JSON persistence, replacing the Windows registry /
WinForms preferences stack for the Avalonia shell.

### 11.1 Settings persistence (`OpenLiveWriter.Platform.Mac`)

| Type | Role |
| --- | --- |
| `FileSettingsPersister` | JSON `ISettingsPersister` — one `{subKey}.json` file under the platform-resolved `Settings/` folder (`CreateUserSettingsPersister` via `MacPlatformServices`). Supports nested `GetSubSettings`, `BatchUpdate`, and atomic temp-file writes. |
| `MacPlatformServices.CreateUserSettingsPersister` | Resolves `~/Library/Application Support/OpenLiveWriter/Settings/` (never hardcoded). |

### 11.2 Preference model + UI (`App.Avalonia`)

| Type | Role |
| --- | --- |
| `AppPreferences` | Snapshot of General (post windows, publishing reminders, AutoRecover, word-count status bar), Editing (autoreplace toggles, paragraph tag), Spelling (`SpellcheckEnabled`), and Web Proxy fields — keyed to match Windows `PostEditorSettings` / `AutoreplaceSettings` / `WebProxySettings` layout. |
| `AppPreferencesStore` | Load/save through `ISettingsPersister` (`Preferences` root → `PostEditor`/`WordCount`/`Autoreplace`/`Spelling`/`WebProxy` sub-trees). |
| `PreferencesDialog` | Tabbed modal: General, Editing, Spelling, Web Proxy, Accounts (opens `AccountManagerDialog`). Shows only options the shell actually enforces (see §11.3). |
| `MainWindow` | **Options** `CommandId` opens the dialog; apply path calls `SetSpellcheckEnabledAsync`, refreshes the status-bar word-count pane, pushes autoreplace toggles to the editor bridge, and supplies proxy-aware `HttpClient` instances to publish transports via `CreatePublishingHttpClient()`. Publish commands honour **title** and **category** reminders when enabled, and the **view-after-publish** / **close-on-publish** follow-ups after a successful publish. |

### 11.3 Preference enforcement matrix

| Preference | Persisted | Enforced |
| --- | --- | --- |
| Spell-check enabled | ✅ | ✅ — `spellcheck` body attribute |
| Show real-time word count | ✅ | ✅ — status bar pane |
| Web proxy (host/port/user/pass) | ✅ | ✅ — `PublishingHttpClientFactory` → MetaWeblog, RSD Detect, image upload |
| Replace smart quotes / hyphens / special chars / emoticons | ✅ | ✅ — paste via `AutoreplaceTransformer`; typing smart quotes via JS bridge |
| Title reminder before publish | ✅ | ✅ — blocks publish when title empty |
| Category reminder before publish | ✅ | ✅ — confirm when no categories |
| AutoRecover (enabled + interval minutes) | ✅ | ✅ — `AutosaveController` on a `DispatcherTimer` at `AutoSaveMinutes` |
| View post after publish | ✅ | ✅ — opens the account's `HomepageUrl` in the default browser (via `BrowserLauncher`) after a successful publish only, never for server drafts |
| Close window after publish | ✅ | ✅ — `Close()` after a successful publish; the normal unsaved-changes prompt still guards a dirty draft |
| Post window behavior, tag reminder, paragraph tags | ✅ | removed from the UI (single-window shell; keywords/paragraph tags aren't wired) — stored-only fields kept on `AppPreferences` for forward-compat |

### 11.4 Tests

`GroupO_SettingsTests` (default headless suite, temp dir + in-memory persister):
`FileSettingsPersister` scalar/sub-settings round-trip, `AppPreferencesStore` file
round-trip, preference field persistence, spell-check bridge script mapping, proxy-password
unset when blank, `PublishingHttpClientFactory`/`WebProxyMapper`, and
`AutoreplaceTransformer`/`AutoreplaceController`.

`GroupS_ConnectionTests` covers the account-dialog **Test Connection** verifier
(end-to-end through the real XML-RPC transport over a fake `HttpMessageHandler`),
the button enable rule, the view-after-publish / close-on-publish preference mapping,
and the `BrowserLauncher` seam; the live verification test is `[Explicit]`.

---

## 12. Picture Tools band (macOS) — P1-2

Picture editing was Windows Live Writer's signature feature. The first pass
made the previously decorative Picture Tools contextual tab real for everything
that can be done honestly with HTML/CSS; the pixel-baking passes (SkiaSharp)
then made crop, baked rotate, the Black & White / Sepia / Sharpen / Blur /
Emboss effects, contrast, and text watermarks real too.

### 12.1 Selection + contextual tab

Clicking an `<img>` in the editor selects it as a unit (the bridge's click
handler wraps it in a single-node selection, like Windows' picture selection);
moving the caret away deselects. `getState()` reports `selectedElementType:
"image"` plus an `image` payload (src, natural/display size, alt/title,
alignment, uniform margin, rotation, border width/color, wrapping link href)
which `WebViewEditor.ParseFormatState` surfaces as `FormatState.Image`
(`ImageFormatState`). The existing contextual-tab pipeline
(`FormatStateChanged` → `ContextualTabResolver` →
`AvaloniaRibbonControl.ActivateContextualTabGroup`) shows Picture Tools >
Format while an image is selected and hides it otherwise.

### 12.2 What is live

- **Size:** width/height spinners (new ribbon `SpinnerValueChanged` → shell
  pipeline; spinner values reflect the selection via `SetSpinnerValue`) with an
  aspect-ratio lock toggle (on by default; computed from the natural
  dimensions), and the Custom size dropdown — Small 160 / Medium 320 / Large
  640 px presets (Windows' presets are user-configurable; these are the fixed
  mac defaults until a defaults dialog is ported) and Original (clears
  width/height back to natural size). Both the HTML attributes and matching
  inline styles are set — the editor stylesheet's `img { height: auto }` would
  otherwise override the height attribute.
- **Rotate:** CW/CCW rotate the image in 90° steps, **baked into the pixels**
  by `ImageEditorService.Rotate90` (SkiaSharp): the selected image's bytes are
  pulled from its data-URI `src` (or downloaded proxy-aware for web pictures),
  rotated, and re-embedded as a new PNG data URI via
  `OLWBridge.replaceSelectedImageSrc`, which also swaps any explicit display
  width/height and clears legacy CSS transforms. This matches Windows (CSS
  `transform` rotate was fragile for publishing). WebView undo does not cover
  the bridge rewrite (best-effort, documented).
- **Border:** a Border toggle applies/removes a solid inline border (default
  1px `#999999`, or the last-used color); width and color are editable in the
  Picture Properties dialog.
- **Properties:** the Picture Properties dialog (patterned after `LinkDialog`)
  edits alt text, title, Link To (no link / source picture / web address —
  "source picture" is only meaningful for remote web pictures; embedded
  data-URI pictures have no source URL), alignment (inline / float left /
  float right / centered block), a uniform margin in px, and border
  width/color. All applied via `OLWBridge.applyImageAttrs` /
  `OLWBridge.setImageLink`, which mark the draft dirty (debounced
  `contentChanged`) and re-report state.
- **Effects / contrast / watermark:** the Effects dropdown bakes Black & White,
  Sepia, Sharpen, Blur and Emboss (Windows' exact convolution kernels) into the
  pixels; the contrast spinner (-100..100) bakes a debounced cumulative
  adjustment; Watermark opens a dialog (text, px size, opacity, five anchor
  positions, preview) and bakes white text with a dark drop-shadow. See §12.3a.
- **Dropdown buttons now open menus:** `RibbonButtonControl` builds a flyout
  from a `DropDownButton`'s `MenuItems` (previously the items were dropped, so
  the size presets and Link To choices were unreachable). Items whose command
  has no handler render disabled.

### 12.3 Still dead (documented, disabled in the ribbon)

Tilt, the recolor gallery, the picture-styles gallery, Set custom size
defaults, and Save/Revert settings. Tilt is an arbitrary-angle perspective
transform — high implementation cost for a rarely used novelty. Recolor needs
Windows' temperature/tint gallery UX (a live-preview slider pair over the
decorator pipeline), not just a pixel op; the single-command slice has no
honest one-click behavior. The rest need a defaults/settings store. They stay
unhandled in `HandledCommands` and render disabled with the "not yet
available" tooltip; `GroupQ_HandledCommandsTests` pins both the live and the
dead sets.

### 12.3a Pixel baking (crop / baked rotate / effects / contrast / watermark)

`ImageEditing/ImageEditorService` (SkiaSharp, pure/headless) bakes pixels:
`Rotate90` (CW/CCW, dimensions swap), `Crop` (pixel rect, clamped to bounds),
`Resize` (cubic sampling — kept for flows that must rewrite pixels; the size
presets/spinners intentionally stay non-destructive attribute edits),
`Grayscale`/`Sepia` (color-matrix filters), `AdjustContrast` (-100..100 around
a mid-gray-invariant pivot), `Sharpen`/`Blur`/`Emboss` (3x3 matrix
convolutions with the exact kernels Windows' decorators used, edges padded
with a duplicate border like Windows' `Conv3x3`), and `AddTextWatermark`
(white text + 1px dark drop-shadow, five anchor positions). Input is any
Skia-decodable image; output is always PNG. The shell pipeline
(`MainWindow.PictureTools`) decodes the selected image's data-URI `src` inline
or downloads web pictures proxy-aware (`HttpImageFetcher` over
`PublishingHttpClientFactory`), bakes, and re-embeds via
`OLWBridge.replaceSelectedImageSrc` (size modes: keep / swap / set; clears CSS
rotation; fires debounced `contentChanged` + `stateChanged`). The crop UX is a
numeric X/Y/width/height dialog (`CropImageDialog`) and the watermark UX a
text/size/opacity/position dialog (`WatermarkDialog`), both with a preview —
an interactive rubber-band crop inside the WebView is out of scope. The
Effects dropdown wires Black & White, Sepia, Sharpen, Blur and Emboss (recolor
stays disabled). The contrast spinner (-100..100) is **debounced and
cumulative**: spinner changes coalesce into a single bake applied to the
*current* pixels (each committed value is a delta, like clicking a "more
contrast" button repeatedly), then the spinner resets to 0 (neutral) so it
never claims an absolute level the pixels no longer reflect — this keeps
repeated spinner ticks from baking dozens of compounding passes. Publish is
untouched: baked images stay data-URIs, which `ImagePublisher` uploads as
before. WebView undo does not cover the bridge rewrite (best-effort,
documented).

### 12.4 Tests

`GroupV_PictureToolsTests` (default suite): state-payload parsing,
`ImageEditBuilder` payload/aspect/alignment logic, dialog link mapping, and
headless ribbon wiring (spinner events/reflection, dropdown flyouts).
`GroupY_ImageEditingTests` (default suite): pixel-op correctness (dimensions,
pixel spot-checks, aspect handling, invalid input), data-URI decode/re-embed
round-trip, selected-image JSON parsing, command registration, and the effects
dropdown flyout. `GroupZ_ImageEffectsTests` (default suite): the second wave —
watermark quadrant/opacity/validation, contrast identity/mid-gray
invariance/extremes, sharpen edge contrast, blur variance reduction, emboss
mid-gray/relief, and the new command registration.
`GroupV_PictureToolsWebViewTests` ([Explicit], live
WKWebView): selection reporting, applyImageAttrs, baked-replacement size
swap/set, link wrap/unwrap. `GroupK` covers Picture Tools tab activation;
`GroupQ` pins the handled/dead registry; `GroupP_UiReviewDeepCaptureTests`
captures the dialogs and the Picture Tools ribbon band
(`tab-picturetools-1280x800.png`, `dialog-imageproperties.png`,
`dialog-crop.png`, `dialog-watermark.png`).

---

## 13. Theme band (macOS) — P1-3 themed preview

Windows detects the blog's editing template (`BlogEditingTemplateDetector`:
downloads the homepage + a sample post, locates the post region, and reuses the
real theme HTML/CSS for the Web Layout editing view and Preview). That pipeline
is MSHTML-heavy and deliberately not ported. This band ships the honest
cross-platform slice: Preview with the blog's real stylesheets, driven by the
previously dead Blog Account tab buttons.

### 13.1 What is live

- **Theme harvest (`App.Avalonia/Theming`):** `ThemeStyleCache` fetches the
  current account's homepage through the shell's proxy-aware `HttpClient` (the
  same `PublishingHttpClientFactory` path as publishing) behind an injectable
  `IThemeHtmlFetcher` seam (15s timeout, redirects followed by the client).
  `ThemeStyleExtractor` (pure, regex-based like the RSD parser) pulls the
  absolute URLs of every `<link rel="stylesheet">` (relative, root-relative,
  and protocol-relative hrefs resolved; duplicates removed) plus the contents
  of inline `<style>` blocks.
- **Per-account cache:** memory + JSON disk cache under the platform app-data
  `Themes/` folder, keyed by account id and stamped with the fetch time. A
  cached entry is reused only while the account's homepage URL still matches;
  **Update Theme** passes `forceRefresh` to re-harvest. A failed refresh
  returns null and leaves the previous cache entry untouched (the cache is
  never poisoned by a network hiccup).
- **Themed preview:** when **Use Theme** is on for the current blog,
  `PreviewRenderer` emits the theme's stylesheet links + inline styles after
  the neutral article style (so the blog's typography/colors win at equal
  specificity) and tags the body with an `olw-theme` class hook. `EditorPanel`
  gets the theme from a shell-provided `PreviewThemeProvider`; a provider
  failure yields the neutral document — Preview never breaks on a theme miss.
- **Use Theme toggle:** persisted per account (`BlogAccount.UseThemeForPreview`,
  the counterpart to Windows' `EditUsingBlogStyles` — scoped to Preview here
  since the macOS editor has no Web Layout view); the ribbon toggle reflects
  the current account's stored value and toggling re-composes an open preview.
- **Update Theme:** forced re-harvest with status-bar progress + result
  ("N stylesheet(s), M inline style block(s)"). Both commands need a current
  account with a homepage URL and say so on the status bar otherwise.
- **Close Preview:** the Preview tab's previously dead button now switches the
  editor back to the Edit view — all it does on Windows.

### 13.2 Honest limitation vs Windows

The preview keeps its neutral `<article>` wrapper; the theme stylesheets are
layered raw on top. Rules the theme scopes to its real post containers (e.g.
`.entry-content p`, `.post` layout, sidebar/masthead chrome) do not apply, so
the preview shows the blog's **typography and colors**, not its page layout.
Full template-region detection and a Web Layout editing view remain open
(P1-3 remainder).

### 13.3 Tests

`GroupW_ThemingTests` (default suite, 27 cases): pure extraction fixtures,
cache behavior over a fake fetcher + temp disk dir, themed/neutral
`PreviewRenderer` composition, and headless shell wiring (toggle persistence,
status messages, Close Preview). No live network — the only networked path is
the production `HttpThemeHtmlFetcher`, exercised manually against a real blog.
