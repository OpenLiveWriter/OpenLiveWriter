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
    }
}
