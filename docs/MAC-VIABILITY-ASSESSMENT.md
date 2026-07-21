# Open Live Writer for macOS — Viability Assessment

> **Status addendum (2026-07-20, same branch):** P0-1…P0-5, P1-1, P1-8, P1-12
> and the §3 editor-bridge/prefs-honesty items are **addressed** (uncommitted
> work): macOS menu bar with accelerators (File/Edit/View/Help, incl. Set
> Categories), unsaved-changes close prompt, draft autosave, dead commands now
> visibly disabled via a handled-command registry, Debug tab hidden unless
> `OLW_DEBUG_RIBBON=1`, fully async publish transport, debounced payload-free
> content sync, JSON-based JS escaping, px font sizes, find previous/count/
> single-replace, preferences dialog shows only enforced options (+ working
> view-after-publish / close-after-publish), Account dialog "Test Connection".
> Suite: **489 passed / 0 failed** (+118 since this assessment). Remaining
> open: P1-2…P1-7, P1-9…P1-11 (picture editing, theme preview, server posts,
> pages, more providers, spelling dialog, post properties, print, web images)
> and the §4 long tail.

**Date:** 2026-07-20 · **Branch:** `milestone4/webview-wysiwyg` · **Audience:** engineering

Independent deep review of the Avalonia macOS port as a candidate *drop-in
replacement* for Open Live Writer on Windows. This document is deliberately more
skeptical than `MAC-PARITY-STATUS.md`: that file tracks what has *landed*; this
one tracks what a **user switching from Windows would actually hit**.

## Method

- Full codebase read of `OpenLiveWriter.App.Avalonia`, `Ribbon.Avalonia`,
  `Platform.Mac`, `Publishing` (ground truth, not the status doc).
- Full feature-surface inventory of the Windows app (`Ribbon.xml`,
  `PostEditor`, `BlogClient`, preferences panels) as the parity benchmark.
- Built the shipping artifact (`build-mac.sh`), launched the `.app`, attempted
  live screen capture (blocked by macOS Screen Recording TCC for window
  capture; full-screen capture confirmed the app runs with **zero menu-bar
  menus**).
- Extended the headless UI-review harness (`GroupP_UiReviewDeepCaptureTests`,
  `Category=UiReview`) to capture **every ribbon tab and all 16 dialogs** as
  PNGs into `artifacts/ui-review/` — 24 new screenshots, used for the visual
  findings below. Run `./scripts/ui-review.sh` to regenerate.
- Test suite: 371 passed / 0 failed (default headless run) — green, but see
  "Test coverage reality" below.
- Command coverage counted mechanically: **193 distinct commands are declared
  in the mac ribbon configuration; only ~74 appear in any command handler;
  132 declared commands appear nowhere in app code.**

## Verdict

The port is a solid *foundation* (editor shell, publishing pipeline, drafts,
accounts, tests) wrapped in a UI that **actively misleads its user**: most
ribbon buttons do nothing, the File/draft lifecycle is unreachable, the
Preferences dialog offers switches that aren't wired, and quitting discards
unsaved work without warning. It is currently a demo, not a replacement.

Rough distance to "viable replacement": the 5 items in **P0** below plus a
curated subset of P1. The full Windows feature surface (picture editing,
theme preview, all providers, plug-ins) is a much longer tail — see §4.

---

## 1. P0 — Trust breakers (fix before anyone uses this daily)

These cause data loss or make core workflows unreachable.

| # | Issue | Evidence |
| --- | --- | --- |
| P0-1 | **Quit loses work silently.** The window `Closing` handler only persists layout — no unsaved-changes prompt. Combined with no autosave (P0-2), closing the window destroys dirty edits. | `MainWindow.WindowLayout.cs:52` |
| P0-2 | **No autosave / AutoRecover**, yet the Preferences dialog shows "Save AutoRecover information periodically" and the model carries `AutoSaveDrafts`/`AutoSaveMinutes`. The UI promises protection that doesn't exist. | `Settings/AppPreferences.cs:28-29`, `Dialogs/PreferencesDialog.cs:218`; no timer/store anywhere |
| P0-3 | **No macOS menu bar at all** — running app shows only "Open Live Writer" in the menu bar, no File/Edit/View/Help menus (no `NativeMenu` anywhere). The entire File-menu command set (New Post/Page, Save, Open Draft, Delete Draft, MRU, Print, Options, About, Quit) is implemented but **has no UI entry point** — the ribbon's `ApplicationMenu`/`QuickAccessToolbar` config is dead code (`AvaloniaRibbonControl.BuildRibbon` never renders it). No Cmd+N/Cmd+S/Cmd+O either. | `AvaloniaRibbonControl.cs:195`, `MainWindow.axaml.cs:201-239`; live screenshot |
| P0-4 | **Dead buttons that look alive.** Paste / Cut / Copy on the Home ribbon do nothing (status bar flashes "Command: Paste"). 132 of 193 declared commands have no handler. Unhandled commands are never disabled, so the UI lies about its capabilities. | command-routing fallthrough in `MainWindow.axaml.cs`; grep count |
| P0-5 | **Debug tab ships enabled by default** (`ActiveModes` includes `RibbonApplicationMode.Debug`) — Terminate Process, Raise Assertion, etc. visible to users. | `AvaloniaRibbonControl.cs:60-64` |

## 2. P1 — Parity gaps a switcher hits in week one

| # | Gap | Windows counterpart |
| --- | --- | --- |
| P1-1 | **Categories picker is unreachable** — fully implemented (`ShowCategoryPopup` handler + `CategoryDialog`) but no ribbon/menu entry point. | Categories in post properties band + Home tab |
| P1-2 | **Picture Tools contextual tab is decorative** — crop/rotate/resize/effects/borders/watermark/alt-text/link-to all render but do nothing. Image handling is base64-in-HTML only; no sizing, no "link to original", no defaults. This was one of Windows Live Writer's signature features. | `PostHtmlEditing/ImageEditing/` decorator pipeline |
| P1-3 | **No theme-based WYSIWYG or themed preview.** Preview is a generic article render; "Use Theme"/"Update Theme" buttons on the Blog Account tab are dead. | `BlogEditingTemplateDetector`, Web Layout view |
| P1-4 | **Can't open/edit posts from the server** — no `metaWeblog.getRecentPosts`/`getPost`; "Open Post" only lists local drafts. Editing an already-published post (a core OLW workflow) is impossible unless published from this machine. | `RecentPostSynchronizer`, Open Post dialog |
| P1-5 | **Pages don't actually publish as pages** — `NewPage`/`IsPage` exist but the client only sends post structs (no `wp.newPage`/`editPage`). | `CommandNewPage` (Ctrl+G) |
| P1-6 | **MetaWeblog only.** `BlogClientFactory` throws for WordPress API, Blogger, AtomPub, MovableType, LiveJournal, SharePoint, static sites. | `OpenLiveWriter.BlogClient/Clients/` (9+ clients) |
| P1-7 | **Spellcheck is underline-only** — native WKWebView checking, no dialog, no suggestions UI, no F7 flow; `MacSpellCheckProvider` is an explicit stub. The ribbon Spelling button shows an info dialog. | `OpenLiveWriter.SpellChecker` (squiggles + dialog + autocorrect) |
| P1-8 | **Publish blocks the UI thread** — sync `HttpClient.Send` in the XML-RPC client; whole app freezes during publish/image upload. | `MetaWeblogXmlRpcClient.cs:336` |
| P1-9 | **Post properties missing**: publish date/scheduling, slug, excerpt, ping/trackback URLs. | `PostPropertiesForm` (F2), properties band |
| P1-10 | **No Print / Print Preview** (commands declared, unhandled). | `FormattedHtmlPrinter` |
| P1-11 | **Insert-from-web gaps**: "Picture from the Web" (`WebImage`) unhandled; all four Video commands open the same URL dialog. | `ImageUrlForm`, `VideoBrowserForm` |
| P1-12 | **Font size is the HTML 1–7 scale**, not px; size/family combos don't reflect the full selection state. | font combo with real pt sizes |

## 3. P2 — Quality, robustness, honesty

- **Whole-document HTML shipped per keystroke**: every `input` event posts the
  entire body (`editor.html:405-408`) — with base64 images that's megabytes of
  JSON per keystroke; expect typing lag and memory churn on image-heavy posts.
- **Fragile JS bridge escaping** (`WebViewEditor.EscapeJs` escapes only
  `\`, `'`, `\n`) — `\r` and control chars will corrupt posts.
- **Keychain via `/usr/bin/security` with string-built args** — fragile
  parsing, quoting hazards; should use the Security framework API.
- **Undo stack destroyed** by any C# `setContent` (Replace All, open draft) —
  WKWebView contenteditable undo is not preserved across host-side rewrites.
- **"Clean HTML paste" actually pastes plain text**; native paste inside the
  WebView is raw/uncleaned. Neither matches Windows Paste Special.
- **Preferences honesty**: post-window behavior, view-after-publish,
  close-on-publish, tag reminder, paragraph tags are persisted but never
  enforced — hide or implement them.
- **No localization mechanism at all** — hardcoded English everywhere;
  `CommandLabelHelper` derives labels by PascalCase splitting. The existing
  `intl/lba` pipeline is unwired.
- **Zero localization of shortcuts**: only Cmd+B/I/U/K/F/G exist; Windows has
  ~25 accelerators (F2/F7/F11/F12, Ctrl+S/P/O/G/L…).
- **Find**: forward-only ("Previous" reuses next), no match count, no single
  Replace (Replace All only).
- **Stale comments/TODOs** claiming features are unimplemented when they are
  (e.g. `WebViewEditor.cs:494` image upload TODO; `BlogAccount.cs:37`
  detection TODO) — the status doc and code comments over/under-claim in
  places; this assessment should be the baseline going forward.
- **Test coverage reality**: 371 green tests are pure-logic/headless; the
  riskiest surface (WKWebView bridge, execCommand fidelity) only runs under
  `[Explicit]` WebView tests + a manual GUI bench. No CI runs the live tests.

## 4. The long tail (full Windows parity — not required for "viable")

From the Windows inventory (`src/unmanaged/OpenLiveWriter.Ribbon/Ribbon.xml`,
`PostEditor`, `BlogClient`): picture effects/borders/watermark editor, table
properties dialogs + row/column move, map interactive picker, tag providers
(Technorati/del.icio.us-era — arguably obsolete), plug-in host
(`WriterPlugin`/`ContentSource` ecosystem), glossary auto-linking, ping
servers, Blog This integrations, jump lists, LiveClipboard, RTL paragraph
direction, weblog admin shortcuts, About box, account wizard with provider
gallery + Blogger OAuth + SharePoint auth, `.wpost` file interop, auto-update.
Several of these are legitimately *dead* on Windows too (defunct services) —
part of this work is deciding what *not* to port.

## 5. Visual/UI observations (from the extended capture harness)

### Side-by-side: Windows (live VM capture) vs macOS (headless render)

Live Windows capture: `artifacts/ui-review/windows-home-tab-live.png`
(Windows 11 VM, OLW Home tab). Mac renders: `main-1280x800.png`,
`ribbon-home-1280x800.png`, `tab-insert-1280x800.png`.

Direct comparison of the Home tab:

| Element | Windows | macOS port |
| --- | --- | --- |
| File menu + QAT | Blue **File** tab + Save/Undo/Redo quick-access icons | **Absent** — no File menu, no QAT, no menu bar |
| Blog selector | Shows real account name ("DougRathbone.com") with globe icon | Generic "Blog" dropdown |
| Font group | Calibri **11** (real point size), eraser clear-formatting icon, real B/I/U/S/x₂/x²/highlight/color icons | Arial **12** (HTML 1–7 scale), text-glyph buttons |
| Paragraph group | Bullets/numbering/indent + quote + 4 align buttons with icons | Tiny glyph boxes |
| HTML styles | **In-ribbon gallery** — Paragraph + Heading 1–6 rendered as styled thumbnails | "Normal" dropdown only |
| Categories | **"Set categories" link** directly under the ribbon | Implemented but **no entry point** (P1-1) |
| Status bar | Edit/Preview/Source tabs bottom-left; "Draft – Unsaved" state bottom-right | Blog name / message / word count; no draft-state indicator |
| Icons | Full Fluent icon set | Letter-box placeholders ("T", "M", "E") — biggest visual credibility gap |

### Other UI notes

- Preferences dialog: "Accounts" tab header wraps onto a second line
  (tab strip overflow bug).
- Insert tab Map dialog is a manual place/coordinates form (no interactive
  map) — acceptable, but call it what it is.
- Dialogs are plain but clean (Link, Table, Draft picker, Account are the
  best); Account dialog has no "verify credentials" step — users only
  discover bad settings at publish time.
- Status bar, layout harness, resize behavior are in good shape.

## 6. Recommended sequencing

**Band 1 — stop the bleeding (days):**
P0-1 close-prompt · P0-3 menu bar (renders File menu + accelerators, which
also fixes P0-4's worst offenders and P1-1) · P0-5 hide Debug tab in release ·
disable (not just ignore) unhandled commands.

**Band 2 — honest editor (1–2 weeks):**
P0-2 autosave · P1-8 async publish · per-keystroke diff instead of whole-body
posts · EscapeJs hardening · wire or remove unenforced preferences ·
P1-12 px font sizes · find previous/count/single-replace.

**Band 3 — publishing credibility (2–4 weeks):**
P1-4 getRecentPosts/edit-from-server · P1-5 real pages · P1-6 WordPress API
client (Blogger next) · account "verify credentials" · P1-7 spelling dialog.

**Band 4 — signature features (the real moat, 4+ weeks):**
P1-2 picture editing (resize/border/alt/link at minimum) · P1-3 themed
preview · P1-11 web images · print.

**Cross-cutting:** real ribbon icons; localization plumbing before the string
surface grows further; live WebView tests in CI; App Store-facing work
(signing/notarization is M5, already tracked).

## Appendix — tooling built for this assessment

- `GroupP_UiReviewDeepCaptureTests` + `UiReviewHarness` extensions
  (`Category=UiReview`): ribbon tab bands for Home/Insert/Blog Account/
  Preview/Debug + PNGs of all 16 dialogs → `artifacts/ui-review/INDEX.md`.
  Regenerate with `./scripts/ui-review.sh`.
- Command-coverage counting: one-liner diff of `CommandId.*` in
  `DefaultRibbonConfiguration.cs` vs handler `case` labels (193 vs ~74).
- Parallels side-by-side: captured the live Windows OLW Home tab from the
  Windows 11 VM (`artifacts/ui-review/windows-home-tab-live.png`; see §5
  comparison table). `prlctl send-key-event` did not reliably reach the guest,
  so tab switching inside Windows was not automated; the Windows feature
  inventory was taken from the source (`Ribbon.xml`, `PostEditor`), which is
  authoritative for the feature list. Live capture of the mac window was
  blocked by macOS Screen Recording TCC for per-window capture — full-screen
  capture confirmed the app runs with **zero menu-bar menus**; all other mac
  screenshots are faithful headless renders of the same XAML via the harness.
