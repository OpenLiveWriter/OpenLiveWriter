// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group Q — P0 trust breakers, dead-command disabling. Pins the
    /// <see cref="HandledCommands"/> registry against the ribbon configuration:
    /// every command the ribbon renders as a button resolves through the registry,
    /// known-dead commands (Picture Tools, theme, Print, tag providers, Debug
    /// leftovers) report unhandled, and the ribbon control actually renders those
    /// buttons disabled with a "not yet available" tooltip.
    /// </summary>
    [TestFixture]
    [Category("GroupQ")]
    public class GroupQ_HandledCommandsTests
    {
        // Every command the ribbon renders as a clickable button (regular buttons,
        // toggles, and dropdown-gallery buttons) across tabs and contextual groups.
        private static List<CommandId> RibbonButtonCommands()
        {
            var config = DefaultRibbonConfiguration.Create();
            var groups = config.Tabs.SelectMany(t => t.Groups)
                .Concat(config.ContextualTabGroups.SelectMany(g => g.Tabs).SelectMany(t => t.Groups));

            var commands = new List<CommandId>();
            foreach (GroupConfig group in groups)
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
                        case GalleryConfig gallery when
                            gallery.GalleryType != RibbonGalleryType.InRibbon &&
                            gallery.GalleryType != RibbonGalleryType.CompactDropDown:
                            commands.Add(gallery.CommandId);
                            break;
                    }
                }
            }
            return commands.Distinct().ToList();
        }

        [Test]
        public void Registry_CoversEveryRibbonButtonCommand()
        {
            // The registry must give a definitive handled/disabled answer for every
            // button the ribbon can render — nothing may fall through the cracks.
            List<CommandId> buttons = RibbonButtonCommands();
            Assert.That(buttons.Count, Is.GreaterThan(40), "ribbon inventory sanity check");

            int handled = buttons.Count(HandledCommands.IsHandled);
            int disabled = buttons.Count - handled;
            Assert.That(handled, Is.GreaterThan(30), "core editing/file/publish commands stay enabled");
            Assert.That(disabled, Is.GreaterThan(10), "known-dead commands must render disabled");
        }

        [TestCase(CommandId.Bold)]
        [TestCase(CommandId.Paste)]
        [TestCase(CommandId.Cut)]
        [TestCase(CommandId.CopyCommand)]
        [TestCase(CommandId.SavePost)]
        [TestCase(CommandId.NewPost)]
        [TestCase(CommandId.PostAndPublish)]
        [TestCase(CommandId.InsertTable)]
        [TestCase(CommandId.DeleteRow)]
        [TestCase(CommandId.CheckSpelling)]
        [TestCase(CommandId.WordCount)]
        [TestCase(CommandId.SelectBlog)]
        public void Registry_CoreCommands_AreHandled(CommandId commandId)
        {
            Assert.That(HandledCommands.IsHandled(commandId), Is.True,
                $"{commandId} has a real handler and must not be disabled");
        }

        // Commands with no handler anywhere — must render disabled (P0-4).
        [TestCase(CommandId.ImageCrop)]
        [TestCase(CommandId.ImageRotateCW)]
        [TestCase(CommandId.CustomSizeGallery)]
        [TestCase(CommandId.ImageBorderGallery)]
        [TestCase(CommandId.Watermark)]
        [TestCase(CommandId.FormatImageSelectLink)]
        [TestCase(CommandId.FormatImageAltText)]
        [TestCase(CommandId.ImageSaveDefaults)]
        [TestCase(CommandId.FormatImageRevertSettings)]
        [TestCase(CommandId.Print)]
        [TestCase(CommandId.PrintPreview)]
        [TestCase(CommandId.UpdateWeblogStyle)]
        [TestCase(CommandId.ViewUseStyles)]
        [TestCase(CommandId.AddTagProvider)]
        [TestCase(CommandId.ManageTagProviders)]
        [TestCase(CommandId.MoveRowUp)]
        [TestCase(CommandId.MoveColumnLeft)]
        [TestCase(CommandId.ClearCell)]
        [TestCase(CommandId.FormatTablePropertiesSplit)]
        [TestCase(CommandId.VideoWebPreview)]
        [TestCase(CommandId.TerminateProcess)]
        [TestCase(CommandId.RaiseAssertion)]
        [TestCase(CommandId.InsertLoremIpsum)]
        [TestCase(CommandId.ValidateHtml)]
        [TestCase(CommandId.ClosePreview)]
        public void Registry_DeadCommands_AreNotHandled(CommandId commandId)
        {
            Assert.That(HandledCommands.IsHandled(commandId), Is.False,
                $"{commandId} has no handler and must render disabled");
        }

        [AvaloniaTest]
        public void Ribbon_DisablesUnhandledButtonsWithTooltip()
        {
            var ribbon = new AvaloniaRibbonControl
            {
                CommandFilter = HandledCommands.IsHandled
            };
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());

            // Home tab (default): handled commands stay enabled.
            RibbonButtonControl bold = FindButton(ribbon, CommandId.Bold);
            Assert.That(bold, Is.Not.Null);
            Assert.That(bold.IsEnabled, Is.True, "Bold is handled and must stay enabled");

            // Picture Tools contextual tab: decorative commands render disabled.
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);
            RibbonButtonControl crop = FindButton(ribbon, CommandId.ImageCrop);
            Assert.That(crop, Is.Not.Null, "Image Tools tab should render an ImageCrop button");
            Assert.That(crop.IsEnabled, Is.False, "ImageCrop has no handler and must be disabled");
            Assert.That(Avalonia.Controls.ToolTip.GetTip(crop) as string,
                Does.Contain("not yet available"));

            // Table Tools contextual tab: bridge-backed commands stay enabled.
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.TableTools);
            RibbonButtonControl deleteRow = FindButton(ribbon, CommandId.DeleteRow);
            Assert.That(deleteRow, Is.Not.Null);
            Assert.That(deleteRow.IsEnabled, Is.True, "DeleteRow routes through the editor bridge");
        }

        private static RibbonButtonControl FindButton(AvaloniaRibbonControl ribbon, CommandId commandId) =>
            ribbon.GetLogicalDescendants()
                .OfType<RibbonButtonControl>()
                .FirstOrDefault(b => b.CommandId == commandId);
    }
}
