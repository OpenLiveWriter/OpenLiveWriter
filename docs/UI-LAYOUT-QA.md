# UI Layout QA — Open Live Writer (macOS / Avalonia)

Deep visual/usability pass for the Avalonia shell on branch
`milestone4/webview-wysiwyg`. Goal: every ribbon command for the active tab is
**visible or reachable** (scroll / overflow menu) at widths **800 → 1920**, and
the editor + status bar layout remains intentional.

## Resolutions covered

| Size | Role |
| --- | --- |
| 800×600 | Minimum client (`MainWindow` / `WindowLayout.Min*`) |
| 1024×768 | Compact laptop |
| 1280×800 | Common laptop |
| 1440×900 | Mid desktop |
| 1920×1080 | Full HD |

Automated assertions live in `OpenLiveWriter.EditorTests.Automated`
`GroupP_LayoutHarnessTests` (Avalonia.Headless). Native WebView is replaced with
a stretch `Border` placeholder for size checks (`WebViewEditor.UseLayoutPlaceholder`).

### How to run the harness

```bash
dotnet test src/managed/OpenLiveWriter.EditorTests.Automated --filter "Category=GroupP"
# or full suite (includes GroupP):
dotnet test src/managed/OpenLiveWriter.EditorTests.Automated
```

After resizing in a test: set `Width`/`Height`, call `UpdateLayout()`, and (when
needed) pump a headless frame so `ActualWidth`/`Bounds` settle.

## Visual review tooling (screenshots + layout dump)

For agent/human visual iteration — not just unit bounds — capture real Skia PNGs
and a JSON/Markdown dump of named chrome controls:

```bash
./scripts/ui-review.sh           # build, capture, print paths, open Finder (macOS)
./scripts/ui-review.sh --no-open # skip opening the folder
# equivalent:
dotnet test src/managed/OpenLiveWriter.EditorTests.Automated --filter "Category=UiReview"
```

Artifacts land in `artifacts/ui-review/` (gitignored):

| File | Purpose |
| --- | --- |
| `main-{WxH}.png` | Full MainWindow at that client size |
| `ribbon-home-{WxH}.png` | Home ribbon band crop |
| `layout-{WxH}.json` / `.md` | Bounds/visibility for ribbon, Style/FontSize combos, Edit/Source/Preview, More, status, title |
| `INDEX.md` | Manifest + flags |

Implementation: `UiReviewHarness` + `GroupP_UiReviewCaptureTests` (`[Explicit]` /
`Category=UiReview`). Headless uses `.UseSkia()` with `UseHeadlessDrawing=false`
so `CaptureRenderedFrame` / `RenderTargetBitmap` write real pixels.

Always-on golden checks (run with every `dotnet test`): `GroupP_RibbonChromeTests`
(font-size min width, Styles ComboBox, equal view-toggle padding, list/quote glyphs).

## Analysis findings (file:line evidence)

Inspected: `MainWindow.axaml`, `MainWindow.WindowLayout.cs`, `EditorPanel.axaml(+.cs)`,
`AvaloniaRibbonControl`, `RibbonTabStrip`, `RibbonGroupPanel`, `RibbonButtonControl`,
status bar, find bar, dialogs.

### Fixed / mitigated in this pass

| Issue | Evidence | Mitigation |
| --- | --- | --- |
| Small ribbon buttons below ~24px hit target | `RibbonButtonControl.BuildSmallButton` previously `MinHeight = 22` | Raise small / color-picker hit targets to ≥24 |
| Color picker chrome `MinHeight = 22` | `RibbonGroupPanel.CreateColorPicker` | `MinHeight = 24` |
| Find bar actions can exceed narrow width | `EditorPanel.axaml` FindBar `DockPanel` with right-side button cluster (~400px+) | Horizontal `ScrollViewer` so actions stay reachable at 800px |
| Ribbon groups wider than window | Home tab Font + Clipboard + Paragraph etc. | Existing content `ScrollViewer`; plus pinned **More** overflow menu listing active-tab commands |
| Compact mode only on `SizeChanged` | `AvaloniaRibbonControl.OnRibbonSizeChanged` | Also apply compact on initial show / measure so 800px starts compact |
| Status bar unnamed for tests | `MainWindow.axaml` bottom `Border` | `x:Name="StatusBar"` |
| Headless WebView zero-size / unavailable | `WebViewEditor.InitializeWebView` → `NativeWebView` | `UseLayoutPlaceholder` stretch `Border` for layout harness |
| Tab strip overflow | Contextual tabs add width | Existing tab `ScrollViewer` (`RibbonTabStrip`) — harness asserts Extent vs Viewport |

### Already in good shape (prior polish)

| Area | Notes |
| --- | --- |
| Min size | `MainWindow` / `WindowLayout` 800×600 |
| Editor fill | DockPanel + `EditorHost` Stretch + WebView Stretch |
| Dual toolbar | Removed; Edit/Source/Preview only in slim chrome |
| macOS traffic lights | `ExtendClientAreaToDecorationsHint=false`, `WindowDecorations=Full` |
| Window geometry | Persist + clamp to working area |
| Dialogs | MinWidth/MinHeight on Preferences, Accounts, Drafts, Find, Link, etc. |
| Status ellipsis | `TextTrimming` on blog/status/word-count panes |
| Compact ribbon | Threshold ~960px → Small buttons, shorter content band |

### Residual visual debt (delight, not blockers)

- Real Fluent/SVG ribbon icons (glyphs are readable stand-ins).
- Find bar: match-count readout / true reverse search polish.
- More menu lists commands but is not a full Office-style “hidden groups → popup gallery” scaler.
- Extreme DPI / native WKWebView edge cases outside headless coverage.

## Manual smoke checklist

1. `dotnet run --project src/managed/OpenLiveWriter.App.Avalonia` → `[OLW-WebView] Ready`
2. Resize to ~800×600: ribbon scrolls or **More** works; Edit/Source/Preview visible; status bar pinned; title editable
3. Open Find (⌘F): query + Next/Previous reachable without clipping off-window
4. Activate a contextual tab (e.g. table): tab strip still scrollable
5. Widen to 1920: compact mode off; large buttons restore
