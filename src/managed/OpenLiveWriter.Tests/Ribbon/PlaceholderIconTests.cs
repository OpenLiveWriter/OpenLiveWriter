// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;

namespace OpenLiveWriter.Tests.Ribbon
{
    /// <summary>
    /// The Debug tab commands have real icons (added after the managed ribbon
    /// showed bare text and then Missing placeholders). The bridge also filters
    /// Missing placeholders so icon fallbacks (like the collapsed group popup
    /// using a child button's icon) keep working.
    /// </summary>
    [TestFixture]
    public class PlaceholderIconTests
    {
        private static readonly CommandId[] DebugCommands =
        {
            CommandId.TerminateProcess,
            CommandId.RaiseAssertion,
            CommandId.DiagnosticsConsole,
            CommandId.BlogClientOptions,
            CommandId.ViewSource,
            CommandId.InsertLoremIpsum,
            CommandId.ValidateHtml,
            CommandId.ValidateXhtml,
            CommandId.ValidateLocalizedResources,
        };

        [Test]
        public void DebugCommands_HaveRealIcons()
        {
            var commandManager = new CommandManager();
            foreach (var id in DebugCommands)
                commandManager.Add(new Command(id));

            var bridge = new CommandManagerBridge(commandManager);

            foreach (var id in DebugCommands)
            {
                var command = bridge.GetOrCreateBridgedCommand(id);
                Assert.NotNull(command.SmallImage, $"{id} should have a real small icon");
                Assert.NotNull(command.LargeImage, $"{id} should have a real large icon");
                Assert.IsFalse(ReferenceEquals(command.SmallImage, CommandResourceLoader.MissingSmall),
                    $"{id} must use its real icon, not the Missing placeholder");
                Assert.IsFalse(ReferenceEquals(command.LargeImage, CommandResourceLoader.MissingLarge),
                    $"{id} must use its real icon, not the Missing placeholder");
            }
        }

        [Test]
        public void MissingLargePlaceholder_FallsBackToUpscaledSmallIcon()
        {
            // SemanticHtmlGroup has only a small icon; the bridge must not use
            // the Missing placeholder for the large image, but a point-sampled
            // upscale of the real small icon (this is what collapsed group
            // popups like HTML styles display).
            var commandManager = new CommandManager();
            commandManager.Add(new Command(CommandId.SemanticHtmlGroup));

            var bridge = new CommandManagerBridge(commandManager);
            var command = bridge.GetOrCreateBridgedCommand(CommandId.SemanticHtmlGroup);

            Assert.NotNull(command.LargeImage, "large image should fall back to the upscaled small icon");
            Assert.IsFalse(ReferenceEquals(command.LargeImage, CommandResourceLoader.MissingLarge),
                "large image must not be the Missing placeholder");
            Assert.AreEqual(32, command.LargeImage.Width, "upscaled large image should be 32px");
        }

        [Test]
        public void CollapsedPopupGroups_HaveRealLargeIcons()
        {
            // The collapsed Paragraph, Font, HTML styles, and Editing group
            // popup buttons use the group command's large icon. All four must
            // resolve real 32px artwork (no Missing placeholder, no upscale of
            // the 16px small icon) so the popup icons render crisply.
            var groupCommands = new[]
            {
                CommandId.ParagraphGroup,
                CommandId.FontGroup,
                CommandId.SemanticHtmlGroup,
                CommandId.TextEditingGroup,
            };

            var commandManager = new CommandManager();
            foreach (var id in groupCommands)
                commandManager.Add(new Command(id));

            var bridge = new CommandManagerBridge(commandManager);

            foreach (var id in groupCommands)
            {
                var command = bridge.GetOrCreateBridgedCommand(id);
                Assert.NotNull(command.LargeImage, $"{id} popup needs a large icon");
                Assert.IsFalse(ReferenceEquals(command.LargeImage, CommandResourceLoader.MissingLarge),
                    $"{id} popup icon must not be the Missing placeholder");
                Assert.AreEqual(32, command.LargeImage.Width,
                    $"{id} popup icon should be native 32px artwork, not an upscale");
            }
        }
    
        [Test]
        public void CheckForUpdates_HasLabelAndIcons()
        {
            // The File menu's Check for Updates item shipped with the enum-name
            // label and no icon; it must resolve the real label and artwork.
            var commandManager = new CommandManager();
            commandManager.Add(new Command(CommandId.CheckForUpdates));

            var bridge = new CommandManagerBridge(commandManager);
            var command = bridge.GetOrCreateBridgedCommand(CommandId.CheckForUpdates);

            Assert.AreEqual("Check for updates", command.Label);
            Assert.NotNull(command.SmallImage, "Check for updates should have a menu icon");
        }
    }
}
