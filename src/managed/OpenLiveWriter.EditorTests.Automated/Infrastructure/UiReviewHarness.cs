// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;

namespace OpenLiveWriter.EditorTests.Automated.Infrastructure
{
    /// <summary>
    /// Headless visual review helper: lays out <see cref="MainWindow"/> at known sizes,
    /// writes PNG screenshots plus a JSON/Markdown layout dump of named chrome controls.
    /// Output lands under <c>artifacts/ui-review/</c> (gitignored).
    /// </summary>
    public static class UiReviewHarness
    {
        public static readonly (double W, double H, string Tag)[] DefaultSizes =
        {
            (800, 600, "800x600"),
            (1024, 768, "1024x768"),
            (1280, 800, "1280x800"),
            (1440, 900, "1440x900"),
        };

        /// <summary>
        /// Resolves the repo-root <c>artifacts/ui-review</c> folder. Walks up from the
        /// test assembly / cwd until a <c>.git</c> or <c>src/managed</c> sibling is found.
        /// </summary>
        public static string ResolveOutputDirectory()
        {
            string start = AppContext.BaseDirectory;
            DirectoryInfo dir = new DirectoryInfo(start);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")) ||
                    Directory.Exists(Path.Combine(dir.FullName, "src", "managed")))
                {
                    string outDir = Path.Combine(dir.FullName, "artifacts", "ui-review");
                    Directory.CreateDirectory(outDir);
                    return outDir;
                }
                dir = dir.Parent;
            }

            string fallback = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "ui-review"));
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        public static MainWindow CreateLaidOutWindow(double width, double height)
        {
            WebViewEditor.UseLayoutPlaceholder = true;
            var window = new MainWindow
            {
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            window.Show();
            PumpLayout(window);
            if (FindRibbon(window) is { } ribbon)
            {
                ribbon.InvalidateMeasure();
                ribbon.InvalidateArrange();
            }
            PumpLayout(window);
            // Seed combos so screenshots show selected values rather than placeholders.
            FindRibbon(window)?.SetComboSelection(CommandId.FontSize, "3");
            FindRibbon(window)?.SetComboSelection(CommandId.FontFamily, "Arial");
            FindRibbon(window)?.SetComboSelection(CommandId.SemanticHtmlGallery, "p");
            PumpLayout(window);
            return window;
        }

        public static void PumpLayout(Control root)
        {
            root.UpdateLayout();
            if (root is TopLevel top)
            {
                top.InvalidateMeasure();
                top.InvalidateArrange();
                top.UpdateLayout();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }
        }

        public static AvaloniaRibbonControl FindRibbon(MainWindow window)
        {
            var host = window.FindControl<Border>("RibbonHost");
            return host?.Child as AvaloniaRibbonControl
                   ?? window.GetLogicalDescendants().OfType<AvaloniaRibbonControl>().FirstOrDefault();
        }

        /// <summary>
        /// Captures full-window + ribbon-band PNGs and a layout dump for one size.
        /// Returns absolute paths written.
        /// </summary>
        public static CaptureResult CaptureAtSize(double width, double height, string tag, string outputDir = null)
        {
            outputDir ??= ResolveOutputDirectory();
            Directory.CreateDirectory(outputDir);

            var window = CreateLaidOutWindow(width, height);
            var written = new List<string>();
            var flags = new List<string>();
            try
            {
                string mainPng = Path.Combine(outputDir, $"main-{tag}.png");
                string ribbonPng = Path.Combine(outputDir, $"ribbon-home-{tag}.png");
                if (TrySaveScreenshot(window, mainPng))
                    written.Add(mainPng);
                else
                    flags.Add($"screenshot-failed:main-{tag}");

                var ribbon = FindRibbon(window);
                if (ribbon != null && TrySaveControlScreenshot(ribbon, ribbonPng, (int)Math.Max(1, width), (int)Math.Max(1, ribbon.Bounds.Height > 0 ? ribbon.Bounds.Height : 120)))
                    written.Add(ribbonPng);
                else
                    flags.Add($"screenshot-failed:ribbon-home-{tag}");

                var dump = BuildLayoutDump(window, width, height, tag);
                flags.AddRange(dump.Flags);

                string jsonPath = Path.Combine(outputDir, $"layout-{tag}.json");
                string mdPath = Path.Combine(outputDir, $"layout-{tag}.md");
                File.WriteAllText(jsonPath, JsonSerializer.Serialize(dump, new JsonSerializerOptions { WriteIndented = true }));
                File.WriteAllText(mdPath, FormatMarkdown(dump));
                written.Add(jsonPath);
                written.Add(mdPath);

                return new CaptureResult(tag, width, height, written, flags, dump);
            }
            finally
            {
                window.Close();
                WebViewEditor.UseLayoutPlaceholder = false;
            }
        }

        public static IReadOnlyList<CaptureResult> CaptureDefaultSizes(string outputDir = null)
        {
            outputDir ??= ResolveOutputDirectory();
            var results = new List<CaptureResult>();
            foreach (var (w, h, tag) in DefaultSizes)
                results.Add(CaptureAtSize(w, h, tag, outputDir));

            string indexPath = Path.Combine(outputDir, "INDEX.md");
            File.WriteAllText(indexPath, BuildIndex(results, outputDir));
            return results;
        }

        private static bool TrySaveScreenshot(TopLevel window, string path)
        {
            try
            {
                using Bitmap frame = window.CaptureRenderedFrame();
                if (frame == null)
                    return TrySaveControlScreenshot(window, path, (int)window.Width, (int)window.Height);

                frame.Save(path);
                return File.Exists(path) && new FileInfo(path).Length > 0;
            }
            catch
            {
                return TrySaveControlScreenshot(window, path, (int)Math.Max(1, window.Width), (int)Math.Max(1, window.Height));
            }
        }

        private static bool TrySaveControlScreenshot(Visual visual, string path, int pixelWidth, int pixelHeight)
        {
            try
            {
                pixelWidth = Math.Max(1, pixelWidth);
                pixelHeight = Math.Max(1, pixelHeight);
                var size = new PixelSize(pixelWidth, pixelHeight);
                using var rtb = new RenderTargetBitmap(size, new Vector(96, 96));
                rtb.Render(visual);
                rtb.Save(path);
                return File.Exists(path) && new FileInfo(path).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public static LayoutDump BuildLayoutDump(MainWindow window, double width, double height, string tag)
        {
            var editorPanel = window.FindControl<EditorPanel>("EditorPanel");
            var ribbon = FindRibbon(window);
            var dump = new LayoutDump
            {
                Tag = tag,
                WindowWidth = width,
                WindowHeight = height,
                CapturedUtc = DateTime.UtcNow.ToString("o"),
            };

            dump.Controls.Add(DescribeNamed("RibbonHost", window.FindControl<Border>("RibbonHost")));
            dump.Controls.Add(DescribeNamed("Ribbon", ribbon));
            dump.Controls.Add(DescribeNamed("OverflowMore", ribbon?.OverflowButton));
            dump.Controls.Add(DescribeNamed("StatusBar", window.FindControl<Border>("StatusBar")));
            dump.Controls.Add(DescribeNamed("TitleEditor", window.FindControl<TextBox>("TitleEditor")));
            dump.Controls.Add(DescribeNamed("EditViewButton", editorPanel?.FindControl<ToggleButton>("EditViewButton")));
            dump.Controls.Add(DescribeNamed("SourceViewButton", editorPanel?.FindControl<ToggleButton>("SourceViewButton")));
            dump.Controls.Add(DescribeNamed("PreviewViewButton", editorPanel?.FindControl<ToggleButton>("PreviewViewButton")));

            var fontSize = FindCombo(ribbon, CommandId.FontSize);
            var styles = FindCombo(ribbon, CommandId.SemanticHtmlGallery);
            var fontFamily = FindCombo(ribbon, CommandId.FontFamily);
            dump.Controls.Add(DescribeNamed("FontSizeCombo", fontSize));
            dump.Controls.Add(DescribeNamed("FontFamilyCombo", fontFamily));
            dump.Controls.Add(DescribeNamed("StylesCombo", styles));

            // Invariant flags for agent / human review.
            if (fontSize != null && fontSize.Bounds.Width + 0.5 < 56)
                dump.Flags.Add($"FontSizeCombo width {fontSize.Bounds.Width:0.##} < 56");
            if (styles != null && styles.Bounds.Width + 0.5 < 120)
                dump.Flags.Add($"StylesCombo width {styles.Bounds.Width:0.##} < 120");

            var edit = editorPanel?.FindControl<ToggleButton>("EditViewButton");
            var source = editorPanel?.FindControl<ToggleButton>("SourceViewButton");
            var preview = editorPanel?.FindControl<ToggleButton>("PreviewViewButton");
            if (edit != null && source != null && preview != null)
            {
                if (edit.Padding != source.Padding || source.Padding != preview.Padding)
                    dump.Flags.Add("View toggles have unequal Padding");
                if (!NearlyEqual(edit.Bounds.Height, source.Bounds.Height) ||
                    !NearlyEqual(source.Bounds.Height, preview.Bounds.Height))
                    dump.Flags.Add("View toggles have unequal Height");
            }

            if (ribbon?.OverflowButton is { IsVisible: true } more && more.Bounds.Width <= 0)
                dump.Flags.Add("More button visible but zero-sized");

            return dump;
        }

        private static bool NearlyEqual(double a, double b) => Math.Abs(a - b) < 0.5;

        private static ComboBox FindCombo(AvaloniaRibbonControl ribbon, CommandId commandId)
        {
            if (ribbon == null)
                return null;
            return ribbon.GetLogicalDescendants()
                .OfType<RibbonGroupPanel>()
                .SelectMany(g => g.DropDowns)
                .Where(d => d.CommandId == commandId)
                .Select(d => d.ComboBox)
                .FirstOrDefault();
        }

        private static ControlSnapshot DescribeNamed(string name, Control control)
        {
            if (control == null)
            {
                return new ControlSnapshot
                {
                    Name = name,
                    Present = false,
                };
            }

            return new ControlSnapshot
            {
                Name = name,
                Present = true,
                IsVisible = control.IsVisible,
                IsEnabled = control.IsEnabled,
                ActualWidth = control.Bounds.Width,
                ActualHeight = control.Bounds.Height,
                Bounds = $"{control.Bounds.X:0.##},{control.Bounds.Y:0.##},{control.Bounds.Width:0.##},{control.Bounds.Height:0.##}",
                MinWidth = control.MinWidth,
                Width = double.IsNaN(control.Width) ? null : control.Width,
                Padding = control is Decorator d
                    ? $"{d.Padding.Left},{d.Padding.Top},{d.Padding.Right},{d.Padding.Bottom}"
                    : control is TemplatedControl tc
                        ? $"{tc.Padding.Left},{tc.Padding.Top},{tc.Padding.Right},{tc.Padding.Bottom}"
                        : null,
            };
        }

        private static string FormatMarkdown(LayoutDump dump)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Layout dump — {dump.Tag} ({dump.WindowWidth}×{dump.WindowHeight})");
            sb.AppendLine();
            sb.AppendLine($"Captured UTC: {dump.CapturedUtc}");
            sb.AppendLine();
            if (dump.Flags.Count > 0)
            {
                sb.AppendLine("## Flags");
                foreach (var f in dump.Flags)
                    sb.AppendLine($"- {f}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("## Flags");
                sb.AppendLine("- (none)");
                sb.AppendLine();
            }

            sb.AppendLine("| Control | Visible | Enabled | Bounds | Actual W×H | MinWidth | Padding |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (var c in dump.Controls)
            {
                if (!c.Present)
                {
                    sb.AppendLine($"| {c.Name} | — | — | missing | — | — | — |");
                    continue;
                }
                sb.AppendLine($"| {c.Name} | {c.IsVisible} | {c.IsEnabled} | `{c.Bounds}` | {c.ActualWidth:0.##}×{c.ActualHeight:0.##} | {c.MinWidth:0.##} | {c.Padding ?? "—"} |");
            }
            return sb.ToString();
        }

        private static string BuildIndex(IReadOnlyList<CaptureResult> results, string outputDir)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# UI review artifacts");
            sb.AppendLine();
            sb.AppendLine($"Output directory: `{outputDir}`");
            sb.AppendLine();
            sb.AppendLine("| Size | Main PNG | Ribbon PNG | Layout | Flags |");
            sb.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (var r in results)
            {
                string flags = r.Flags.Count == 0 ? "ok" : string.Join("; ", r.Flags);
                sb.AppendLine($"| {r.Tag} | `main-{r.Tag}.png` | `ribbon-home-{r.Tag}.png` | `layout-{r.Tag}.md` | {flags} |");
            }
            sb.AppendLine();
            sb.AppendLine("## How to regenerate");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("./scripts/ui-review.sh");
            sb.AppendLine("# or:");
            sb.AppendLine("dotnet test src/managed/OpenLiveWriter.EditorTests.Automated --filter \"Category=UiReview\"");
            sb.AppendLine("```");
            return sb.ToString();
        }

        public sealed class CaptureResult
        {
            public CaptureResult(string tag, double width, double height, IReadOnlyList<string> files, IReadOnlyList<string> flags, LayoutDump dump)
            {
                Tag = tag;
                Width = width;
                Height = height;
                Files = files;
                Flags = flags;
                Dump = dump;
            }

            public string Tag { get; }
            public double Width { get; }
            public double Height { get; }
            public IReadOnlyList<string> Files { get; }
            public IReadOnlyList<string> Flags { get; }
            public LayoutDump Dump { get; }
        }

        public sealed class LayoutDump
        {
            public string Tag { get; set; }
            public double WindowWidth { get; set; }
            public double WindowHeight { get; set; }
            public string CapturedUtc { get; set; }
            public List<ControlSnapshot> Controls { get; set; } = new();
            public List<string> Flags { get; set; } = new();
        }

        public sealed class ControlSnapshot
        {
            public string Name { get; set; }
            public bool Present { get; set; }
            public bool IsVisible { get; set; }
            public bool IsEnabled { get; set; }
            public double ActualWidth { get; set; }
            public double ActualHeight { get; set; }
            public string Bounds { get; set; }
            public double MinWidth { get; set; }
            public double? Width { get; set; }
            public string Padding { get; set; }
        }
    }
}
