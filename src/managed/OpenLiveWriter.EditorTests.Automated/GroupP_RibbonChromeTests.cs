// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group P — ribbon chrome polish: font-size width, Styles combo sync,
    /// paragraph-style selector UX, list/quote glyphs, view-toggle padding.
    /// </summary>
    [TestFixture]
    [Category("GroupP")]
    public class GroupP_RibbonChromeTests
    {
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

        [Test]
        public void FontSize_PreferredWidth_IsWideEnoughForSelectedValue()
        {
            var config = DefaultRibbonConfiguration.Create();
            var fontSize = config.Tabs
                .SelectMany(t => t.Groups)
                .SelectMany(g => g.Controls)
                .OfType<ComboBoxConfig>()
                .FirstOrDefault(c => c.CommandId == CommandId.FontSize);

            Assert.That(fontSize, Is.Not.Null);
            Assert.That(fontSize.PreferredWidth, Is.GreaterThanOrEqualTo(80),
                "Font size combo must be wide enough to show selected values like 12/14/36 plus chrome");
        }

        [Test]
        public void GlyphForCommand_ListAndQuote_HaveReadableIcons()
        {
            Assert.Multiple(() =>
            {
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.Bullets), Is.EqualTo("\u2022"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.Numbers), Is.EqualTo("1."));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.Blockquote), Is.EqualTo("\u201C"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.Bold), Is.EqualTo("B"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.AlignLeft), Is.EqualTo("\u25E7"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.AlignCenter), Is.EqualTo("\u25A3"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.AlignRight), Is.EqualTo("\u25E8"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.Justify), Is.EqualTo("\u2630"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.InsertImageSplit), Is.EqualTo("\u25EB"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.InsertVideoSplit), Is.EqualTo("\u25B6"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.CopyCommand), Is.EqualTo("\u2750"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.Paste), Is.EqualTo("\u2398"));
                Assert.That(RibbonButtonControl.GlyphForCommand(CommandId.CopyCommand),
                    Is.Not.EqualTo(RibbonButtonControl.GlyphForCommand(CommandId.Paste)));
            });
        }

        [TestCase("p", "Normal")]
        [TestCase("h1", "Heading 1")]
        [TestCase("H2", "Heading 2")]
        [TestCase("pre", "Preformatted")]
        [TestCase("blockquote", null)]
        public void SemanticHtmlStyles_LabelForTag_MapsBlockTag(string tag, string expected)
        {
            Assert.That(SemanticHtmlStyles.LabelForTag(tag), Is.EqualTo(expected));
        }

        [AvaloniaTest]
        public void StylesCombo_ReflectsFormatStateBlockTag()
        {
            var window = CreateLaidOutWindow(1280, 800);
            try
            {
                var ribbon = FindRibbon(window);
                Assert.That(ribbon, Is.Not.Null);

                ribbon.SetComboSelection(CommandId.SemanticHtmlGallery, "h2");

                var combo = FindCombo(ribbon, CommandId.SemanticHtmlGallery);
                Assert.That(combo, Is.Not.Null, "Styles must render as a ComboBox, not an in-ribbon list");
                Assert.That(combo.SelectedItem, Is.InstanceOf<ComboBoxItem>());
                var selected = (ComboBoxItem)combo.SelectedItem;
                Assert.That(selected.Tag as string, Is.EqualTo("h2").IgnoreCase);
                Assert.That(selected.Content as string, Is.EqualTo("Heading 2"));

                // Unknown block tags (e.g. blockquote) clear the style selection.
                ribbon.SetComboSelection(CommandId.SemanticHtmlGallery, "blockquote");
                Assert.That(combo.SelectedItem, Is.Null);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void FontSizeCombo_MinWidth_IsAtLeastPreferredWidth()
        {
            var window = CreateLaidOutWindow(1280, 800);
            try
            {
                var ribbon = FindRibbon(window);
                var combo = FindCombo(ribbon, CommandId.FontSize);
                Assert.That(combo, Is.Not.Null);
                Assert.That(combo.MinWidth, Is.GreaterThanOrEqualTo(80));
                Assert.That(combo.Width, Is.GreaterThanOrEqualTo(80));
                Assert.That(combo.Bounds.Width, Is.GreaterThanOrEqualTo(80));
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void FontSizeCombo_PresentAndWideEnough_InCompactMode()
        {
            var window = CreateLaidOutWindow(800, 600);
            try
            {
                var ribbon = FindRibbon(window);
                Assert.That(ribbon.IsCompactMode, Is.True);
                var combo = FindCombo(ribbon, CommandId.FontSize);
                Assert.That(combo, Is.Not.Null, "Font size must remain a populated ComboBox in compact mode");
                Assert.That(combo.MinWidth, Is.GreaterThanOrEqualTo(80));
                Assert.That(combo.Items.Count, Is.GreaterThan(0));
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void ViewToggles_HaveEqualPaddingAndMinWidth()
        {
            var window = CreateLaidOutWindow(1280, 800);
            try
            {
                // View tabs live at the far right of the ribbon tab strip now.
                ToggleButton[] viewTabs = FindViewTabs(window);
                var edit = viewTabs.ElementAtOrDefault(0);
                var source = viewTabs.ElementAtOrDefault(1);
                var preview = viewTabs.ElementAtOrDefault(2);

                Assert.Multiple(() =>
                {
                    // Tabs share padding / minimum size; width sizes to the label.
                    Assert.That(edit.Padding, Is.EqualTo(source.Padding));
                    Assert.That(source.Padding, Is.EqualTo(preview.Padding));
                    Assert.That(edit.MinWidth, Is.EqualTo(source.MinWidth));
                    Assert.That(source.MinWidth, Is.EqualTo(preview.MinWidth));
                    Assert.That(edit.MinWidth, Is.EqualTo(68));
                    Assert.That(edit.Padding.Left, Is.EqualTo(12));
                    Assert.That(edit.Padding.Right, Is.EqualTo(12));
                    Assert.That(edit.MinHeight, Is.EqualTo(28));
                    Assert.That(edit.Bounds.Height, Is.EqualTo(source.Bounds.Height).Within(0.5));
                    Assert.That(source.Bounds.Height, Is.EqualTo(preview.Bounds.Height).Within(0.5));
                });
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void StylesControl_IsComboBoxNotExpandedGallery()
        {
            var window = CreateLaidOutWindow(1280, 800);
            try
            {
                var ribbon = FindRibbon(window);
                var combo = FindCombo(ribbon, CommandId.SemanticHtmlGallery);
                Assert.That(combo, Is.Not.Null);
                Assert.That(combo.Items.Count, Is.EqualTo(SemanticHtmlStyles.Styles.Count));
            }
            finally
            {
                window.Close();
            }
        }

        private static MainWindow CreateLaidOutWindow(double width, double height)
        {
            var window = new MainWindow
            {
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            window.Show();
            window.UpdateLayout();
            return window;
        }

        private static AvaloniaRibbonControl FindRibbon(MainWindow window)
        {
            var host = window.FindControl<Border>("RibbonHost");
            return host?.Child as AvaloniaRibbonControl
                   ?? window.GetLogicalDescendants().OfType<AvaloniaRibbonControl>().FirstOrDefault();
        }

        private static ComboBox FindCombo(AvaloniaRibbonControl ribbon, CommandId commandId)
        {
            return ribbon.GetLogicalDescendants()
                .OfType<RibbonGroupPanel>()
                .SelectMany(g => g.DropDowns)
                .Where(d => d.CommandId == commandId)
                .Select(d => d.ComboBox)
                .FirstOrDefault();
        }

        // View tabs are created in code (no XAML name-scope registration), so find
        // them through the ViewToggleTabs container in the ribbon's right dock.
        private static ToggleButton[] FindViewTabs(Control root)
        {
            var tabs = root.GetLogicalDescendants().OfType<OpenLiveWriter.App.Avalonia.Editor.ViewToggleTabs>()
                .FirstOrDefault();
            return tabs?.GetLogicalChildren().OfType<ToggleButton>().ToArray() ?? new ToggleButton[0];
        }
    }
}
