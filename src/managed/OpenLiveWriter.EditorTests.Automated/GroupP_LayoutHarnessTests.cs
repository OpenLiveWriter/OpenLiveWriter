// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group P — layout harness. Simulates multiple client sizes and asserts
    /// shell/ribbon invariants so clipping / zero-size / unreachable-command
    /// regressions fail the default <c>dotnet test</c> run. Native WebView is
    /// replaced with a stretch Border via <see cref="WebViewEditor.UseLayoutPlaceholder"/>.
    /// See <c>docs/UI-LAYOUT-QA.md</c>.
    /// </summary>
    [TestFixture]
    [Category("GroupP")]
    public class GroupP_LayoutHarnessTests
    {
        private static readonly (double W, double H)[] TestSizes =
        {
            (800, 600),
            (1024, 768),
            (1280, 800),
            (1440, 900),
            (1920, 1080),
        };

        [SetUp]
        public void SetUp()
        {
            WebViewEditor.UseLayoutPlaceholder = true;
        }

        [TearDown]
        public void TearDown()
        {
            WebViewEditor.UseLayoutPlaceholder = false;
        }

        [AvaloniaTest]
        public void MainWindow_MinSize_MatchesWindowLayoutConstants()
        {
            var window = CreateLaidOutWindow(WindowLayout.DefaultWidth, WindowLayout.DefaultHeight);
            try
            {
                Assert.Multiple(() =>
                {
                    Assert.That(window.MinWidth, Is.EqualTo(WindowLayout.MinWidth));
                    Assert.That(window.MinHeight, Is.EqualTo(WindowLayout.MinHeight));
                    Assert.That(window.MinWidth, Is.EqualTo(800).Within(0.1));
                    Assert.That(window.MinHeight, Is.EqualTo(600).Within(0.1));
                });
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        [TestCase(800, 600)]
        [TestCase(1024, 768)]
        [TestCase(1280, 800)]
        [TestCase(1440, 900)]
        [TestCase(1920, 1080)]
        public void Shell_AtSize_StatusBarEditorAndViewToggles_HavePositiveBounds(double width, double height)
        {
            var window = CreateLaidOutWindow(width, height);
            try
            {
                var statusBar = window.FindControl<Border>("StatusBar");
                var editorPanel = window.FindControl<EditorPanel>("EditorPanel");
                var editorHost = editorPanel?.FindControl<ContentControl>("EditorHost");
                var title = window.FindControl<TextBox>("TitleEditor");
                var edit = editorPanel?.FindControl<ToggleButton>("EditViewButton");
                var source = editorPanel?.FindControl<ToggleButton>("SourceViewButton");
                var preview = editorPanel?.FindControl<ToggleButton>("PreviewViewButton");
                var placeholder = editorPanel?.GetLogicalDescendants()
                    .OfType<Border>()
                    .FirstOrDefault(b => b.Name == WebViewEditor.LayoutPlaceholderName);

                Assert.That(statusBar, Is.Not.Null);
                Assert.That(editorHost, Is.Not.Null);
                Assert.That(title, Is.Not.Null);
                Assert.That(edit, Is.Not.Null);
                Assert.That(source, Is.Not.Null);
                Assert.That(preview, Is.Not.Null);

                Assert.Multiple(() =>
                {
                    Assert.That(statusBar.Bounds.Height, Is.GreaterThan(0), "Status bar height");
                    Assert.That(statusBar.Bounds.Top, Is.LessThan(height), "Status bar within window");
                    Assert.That(statusBar.Bounds.Bottom, Is.LessThanOrEqualTo(height + 1), "Status bar bottom");

                    Assert.That(editorHost.Bounds.Width, Is.GreaterThan(0), "EditorHost width");
                    Assert.That(editorHost.Bounds.Height, Is.GreaterThan(0), "EditorHost height");
                    Assert.That(placeholder, Is.Not.Null, "Layout placeholder should host the editor slot");
                    Assert.That(placeholder.Bounds.Width, Is.GreaterThan(0), "Placeholder width");
                    Assert.That(placeholder.Bounds.Height, Is.GreaterThan(0), "Placeholder height");

                    Assert.That(title.Bounds.Width, Is.GreaterThan(100), "Title field should stretch");
                    Assert.That(title.IsVisible, Is.True);

                    Assert.That(edit.IsVisible, Is.True);
                    Assert.That(source.IsVisible, Is.True);
                    Assert.That(preview.IsVisible, Is.True);
                    Assert.That(edit.Bounds.Width, Is.GreaterThan(0));
                    Assert.That(source.Bounds.Width, Is.GreaterThan(0));
                    Assert.That(preview.Bounds.Width, Is.GreaterThan(0));
                    Assert.That(edit.Bounds.Height, Is.GreaterThanOrEqualTo(24));
                    Assert.That(source.Bounds.Height, Is.GreaterThanOrEqualTo(24));
                    Assert.That(preview.Bounds.Height, Is.GreaterThanOrEqualTo(24));
                });
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        [TestCase(800, 600)]
        [TestCase(1024, 768)]
        [TestCase(1280, 800)]
        [TestCase(1440, 900)]
        [TestCase(1920, 1080)]
        public void Ribbon_AtSize_TabsAndGroups_ScrollOrFit_AndButtonsHaveSize(double width, double height)
        {
            var window = CreateLaidOutWindow(width, height);
            try
            {
                var ribbon = FindRibbon(window);
                Assert.That(ribbon, Is.Not.Null);

                // Force layout after compact may have rebuilt groups.
                PumpLayout(window);

                var tabStrip = ribbon.TabStrip;
                Assert.That(tabStrip, Is.Not.Null);
                var tabScroll = tabStrip.TabScrollViewer;
                Assert.That(tabScroll, Is.Not.Null);

                // Tabs either fit or the strip is scrollable when content is wider.
                AssertScrollOrFit(tabScroll, "tab strip");

                var contentScroll = ribbon.ContentScrollViewer;
                Assert.That(contentScroll, Is.Not.Null);
                AssertScrollOrFit(contentScroll, "ribbon content");

                // When content overflows, More must be available as a second affordance.
                bool contentOverflows = contentScroll.Viewport.Width > 0 &&
                                        contentScroll.Extent.Width > contentScroll.Viewport.Width + 1;
                if (contentOverflows)
                {
                    Assert.That(ribbon.OverflowButton, Is.Not.Null);
                    Assert.That(ribbon.OverflowButton.IsVisible, Is.True,
                        "More overflow should be visible when content is wider than the viewport");
                }

                if (width < 960)
                    Assert.That(ribbon.IsCompactMode, Is.True, "Ribbon should be compact below 960px");

                var zeroSized = FindZeroSizedInteractiveButtons(ribbon).ToList();
                Assert.That(zeroSized, Is.Empty,
                    "Visible interactive buttons must have positive size: " +
                    string.Join(", ", zeroSized.Select(DescribeButton)));

                var undersized = ribbon.GetLogicalDescendants()
                    .OfType<RibbonButtonControl>()
                    .Where(b => b.IsVisible && b.MinHeight < 24)
                    .ToList();
                Assert.That(undersized, Is.Empty,
                    "Ribbon buttons should declare MinHeight >= 24");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void Ribbon_ContextualTabs_RemainScrollableAtMinimumWidth()
        {
            var window = CreateLaidOutWindow(800, 600);
            try
            {
                var ribbon = FindRibbon(window);
                ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.TableTools);
                PumpLayout(window);

                var tabScroll = ribbon.TabStrip.TabScrollViewer;
                Assert.That(tabScroll, Is.Not.Null);
                Assert.That(ribbon.TabStrip.TabButtons.Count, Is.GreaterThan(0));
                AssertScrollOrFit(tabScroll, "contextual tab strip");

                var zeroSized = FindZeroSizedInteractiveButtons(ribbon.TabStrip).ToList();
                Assert.That(zeroSized, Is.Empty,
                    "Contextual tab buttons must have positive size");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        [TestCase(800, 600)]
        [TestCase(1280, 800)]
        public void FindBar_AtNarrowWidth_ActionsReachableViaScroll(double width, double height)
        {
            var window = CreateLaidOutWindow(width, height);
            try
            {
                var editorPanel = window.FindControl<EditorPanel>("EditorPanel");
                Assert.That(editorPanel, Is.Not.Null);

                editorPanel.ShowFindBar("hello");
                PumpLayout(window);

                var findBar = editorPanel.FindControl<Border>("FindBar");
                var scroll = editorPanel.FindControl<ScrollViewer>("FindBarScrollViewer");
                var next = editorPanel.FindControl<Button>("FindNextButton");
                var previous = editorPanel.FindControl<Button>("FindPreviousButton");
                var query = editorPanel.FindControl<TextBox>("FindQueryBox");

                Assert.Multiple(() =>
                {
                    Assert.That(findBar.IsVisible, Is.True);
                    Assert.That(scroll, Is.Not.Null);
                    Assert.That(query.Bounds.Width, Is.GreaterThan(0));
                    Assert.That(next.Bounds.Width, Is.GreaterThan(0));
                    Assert.That(previous.Bounds.Width, Is.GreaterThan(0));
                    Assert.That(next.Bounds.Height, Is.GreaterThanOrEqualTo(24));
                    AssertScrollOrFit(scroll, "find bar");
                });
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void Shell_AllTestSizes_NoZeroSizedVisibleButtons()
        {
            foreach (var (w, h) in TestSizes)
            {
                var window = CreateLaidOutWindow(w, h);
                try
                {
                    var zeroSized = FindZeroSizedInteractiveButtons(window).ToList();
                    Assert.That(zeroSized, Is.Empty,
                        $"At {w}x{h}: " + string.Join(", ", zeroSized.Select(DescribeButton)));
                }
                finally
                {
                    window.Close();
                }
            }
        }

        // ---- helpers ----

        private static MainWindow CreateLaidOutWindow(double width, double height)
        {
            var window = new MainWindow
            {
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            window.Show();
            PumpLayout(window);
            // Compact mode / overflow visibility react to SizeChanged — pump again
            // after the ribbon has a real width.
            if (FindRibbon(window) is { } ribbon)
            {
                ribbon.InvalidateMeasure();
                ribbon.InvalidateArrange();
            }
            PumpLayout(window);
            return window;
        }

        private static void PumpLayout(Control root)
        {
            root.UpdateLayout();
            if (root is TopLevel top)
            {
                // Avalonia 12 headless: ensure measure/arrange flush after size changes.
                top.InvalidateMeasure();
                top.InvalidateArrange();
                top.UpdateLayout();
            }
        }

        private static AvaloniaRibbonControl FindRibbon(MainWindow window)
        {
            var host = window.FindControl<Border>("RibbonHost");
            return host?.Child as AvaloniaRibbonControl
                   ?? window.GetLogicalDescendants().OfType<AvaloniaRibbonControl>().FirstOrDefault();
        }

        private static void AssertScrollOrFit(ScrollViewer scroll, string label)
        {
            Assert.That(scroll, Is.Not.Null, label);
            double extent = scroll.Extent.Width;
            double viewport = scroll.Viewport.Width;
            if (viewport <= 0)
                return; // still measuring — don't false-fail

            if (extent > viewport + 1)
            {
                Assert.That(
                    scroll.HorizontalScrollBarVisibility,
                    Is.EqualTo(ScrollBarVisibility.Auto).Or.EqualTo(ScrollBarVisibility.Visible),
                    $"{label}: content wider than viewport requires a horizontal scroller");
            }
        }

        private static IEnumerable<Button> FindZeroSizedInteractiveButtons(Control root)
        {
            return root.GetLogicalDescendants()
                .OfType<Button>()
                .Where(b => IsInteractivelyPresented(b) &&
                            (b.Bounds.Width <= 0 || b.Bounds.Height <= 0));
        }

        /// <summary>
        /// True when the control is enabled and every logical ancestor (including
        /// self) is visible. Walks the logical tree because headless controls under
        /// a collapsed FindBar may not have a visual parent yet while still reporting
        /// <c>IsVisible=true</c> on themselves.
        /// </summary>
        private static bool IsInteractivelyPresented(Control control)
        {
            if (!control.IsEnabled)
                return false;

            Avalonia.LogicalTree.ILogical current = control;
            while (current != null)
            {
                if (current is Visual visual && !visual.IsVisible)
                    return false;
                current = current.LogicalParent;
            }

            return true;
        }

        private static string DescribeButton(Button b)
        {
            string content = b.Content switch
            {
                string s => s,
                TextBlock tb => tb.Text,
                _ => b.GetType().Name
            };
            return $"{content}@{b.Bounds}";
        }
    }
}
