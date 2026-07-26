// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group P — ribbon icons: every rendered button command in the visible
    /// (non-Debug) tabs must map to a Fluent icon or be an explicit text-glyph
    /// opt-out; the embedded path data must parse; geometry is cached; buttons
    /// render a PathIcon and dim it when disabled.
    /// </summary>
    [TestFixture]
    [Category("GroupP")]
    public class GroupP_CommandIconTests
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

        // Commands rendered as ribbon buttons (ButtonConfig / ToggleButtonConfig,
        // plus dropdown galleries which render as buttons) across every visible
        // tab: all main tabs except the developer-only Debug tab, and all
        // contextual tab groups (Picture/Video/Table/Map/Tag Tools).
        private static List<CommandId> RenderedButtonCommands()
        {
            var config = DefaultRibbonConfiguration.Create();
            var tabs = config.Tabs
                .Where(t => t.CommandId != CommandId.DebugTab)
                .Concat(config.ContextualTabGroups.SelectMany(g => g.Tabs));

            var commands = new List<CommandId>();
            foreach (TabConfig tab in tabs)
            {
                foreach (GroupConfig group in tab.Groups)
                {
                    foreach (ControlConfig control in group.Controls)
                    {
                        switch (control)
                        {
                            case ButtonConfig button:
                                commands.Add(button.CommandId);
                                break;
                            case ToggleButtonConfig toggle:
                                commands.Add(toggle.CommandId);
                                break;
                            case GalleryConfig gallery
                                when gallery.GalleryType == RibbonGalleryType.DropDown:
                                commands.Add(gallery.CommandId);
                                break;
                        }
                    }
                }
            }
            return commands.Distinct().ToList();
        }

        [Test]
        public void RenderedButtonCommands_AllHaveIconOrTextGlyphOptOut()
        {
            List<CommandId> commands = RenderedButtonCommands();
            Assert.That(commands.Count, Is.GreaterThan(50),
                "sanity: the visible tabs render a substantial set of button commands");

            var missing = commands
                .Where(c => !CommandIconProvider.HasIcon(c) && !CommandIconProvider.UsesTextGlyph(c))
                .ToList();
            Assert.That(missing, Is.Empty,
                "every rendered button command needs an icon mapping or an explicit " +
                "text-glyph opt-out in CommandIconProvider");
        }

        // StreamGeometry.Parse needs the headless platform render interface,
        // so geometry tests run under [AvaloniaTest] even though they assert
        // no UI.
        [AvaloniaTest]
        public void IconPathData_AllMappingsParse()
        {
            int mapped = 0;
            foreach (CommandId commandId in System.Enum.GetValues(typeof(CommandId)))
            {
                string pathData = CommandIconProvider.GetIconPathData(commandId);
                if (pathData == null)
                    continue;

                mapped++;
                StreamGeometry geometry = null;
                Assert.DoesNotThrow(() => geometry = StreamGeometry.Parse(pathData),
                    $"icon path for {commandId} must parse");
                Assert.That(geometry, Is.Not.Null);
            }

            Assert.That(mapped, Is.EqualTo(CommandIconProvider.MappedCommandCount));
            Assert.That(mapped, Is.GreaterThan(50));
        }

        [AvaloniaTest]
        public void GetIcon_CachesGeometryInstances()
        {
            StreamGeometry first = CommandIconProvider.GetIcon(CommandId.Paste);
            StreamGeometry second = CommandIconProvider.GetIcon(CommandId.Paste);
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first), "geometry must be parsed once and cached");

            // Commands sharing one Fluent asset share the cached geometry too.
            Assert.That(CommandIconProvider.GetIcon(CommandId.FormatImageSelectLink),
                Is.SameAs(CommandIconProvider.GetIcon(CommandId.InsertLink)));
        }

        [Test]
        public void GetIcon_UnmappedCommand_ReturnsNull()
        {
            Assert.Multiple(() =>
            {
                Assert.That(CommandIconProvider.HasIcon(CommandId.TerminateProcess), Is.False);
                Assert.That(CommandIconProvider.GetIcon(CommandId.TerminateProcess), Is.Null);
                Assert.That(CommandIconProvider.GetIconPathData(CommandId.TerminateProcess), Is.Null);
            });
        }

        [Test]
        public void TextGlyphOptOuts_AreColorPickers()
        {
            Assert.Multiple(() =>
            {
                Assert.That(CommandIconProvider.UsesTextGlyph(CommandId.FontColorPicker), Is.True);
                Assert.That(CommandIconProvider.UsesTextGlyph(CommandId.FontBackgroundColor), Is.True);
                Assert.That(CommandIconProvider.UsesTextGlyph(CommandId.Bold), Is.False);
            });
        }

        [AvaloniaTest]
        public void RibbonButtons_RenderPathIcons()
        {
            var window = CreateLaidOutWindow(1280, 800);
            try
            {
                var ribbon = FindRibbon(window);
                Assert.That(ribbon, Is.Not.Null);

                var buttons = ribbon.GetLogicalDescendants()
                    .OfType<RibbonButtonControl>()
                    .ToList();

                RibbonButtonControl paste = buttons.FirstOrDefault(b => b.CommandId == CommandId.Paste);
                RibbonButtonControl bold = buttons.FirstOrDefault(b => b.CommandId == CommandId.Bold);

                Assert.That(paste, Is.Not.Null, "Paste button should render on the Home tab");
                Assert.That(bold, Is.Not.Null, "Bold toggle should render on the Home tab");

                PathIcon pasteIcon = paste.GetLogicalDescendants().OfType<PathIcon>().FirstOrDefault();
                PathIcon boldIcon = bold.GetLogicalDescendants().OfType<PathIcon>().FirstOrDefault();

                Assert.Multiple(() =>
                {
                    Assert.That(pasteIcon, Is.Not.Null, "Paste must render a PathIcon");
                    Assert.That(pasteIcon.Data, Is.Not.Null);
                    Assert.That(pasteIcon.Width, Is.GreaterThanOrEqualTo(20), "large-button icon ~20-24px");
                    Assert.That(boldIcon, Is.Not.Null, "Bold must render a PathIcon");
                    Assert.That(boldIcon.Data, Is.Not.Null);
                    Assert.That(boldIcon.Width, Is.LessThanOrEqualTo(16), "small-button icon ~16px");
                    Assert.That(pasteIcon.Opacity, Is.EqualTo(1.0));
                });
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void DisabledButton_DimsIcon()
        {
            var button = new RibbonButtonControl(new ButtonConfig
            {
                CommandId = CommandId.Cut,
                PreferredSize = RibbonGroupSize.Small
            });

            PathIcon icon = button.GetLogicalDescendants().OfType<PathIcon>().FirstOrDefault();
            Assert.That(icon, Is.Not.Null);

            button.IsEnabled = false;
            Assert.That(icon.Opacity, Is.LessThan(1.0), "disabled buttons must dim their icon");

            button.IsEnabled = true;
            Assert.That(icon.Opacity, Is.EqualTo(1.0));
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
    }
}
