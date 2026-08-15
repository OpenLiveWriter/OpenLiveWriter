// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.Tests.Ribbon
{
    /// <summary>
    /// Verifies every File menu command resolves and executes through the
    /// managed ribbon's command bridge, the same path ApplicationMenu uses
    /// (_commandManager.Execute(commandId)). Guards against the silent no-op
    /// failure mode where a menu item has no source command and clicking it
    /// does nothing.
    /// </summary>
    [TestFixture]
    public class FileMenuCommandTests
    {
        private static List<CommandId> GetFileMenuCommandIds()
        {
            var config = DefaultRibbonConfiguration.Create();
            var ids = new List<CommandId>();
            foreach (var group in config.ApplicationMenu.MenuGroups)
            {
                foreach (var item in group.Items)
                {
                    if (!item.IsSeparator)
                        ids.Add(item.CommandId);
                }
            }
            return ids;
        }

        private static OpenLiveWriter.ApplicationFramework.CommandManager CreateSourceCommandManager(
            out List<CommandId> ids, System.Action<CommandId> onExecute)
        {
            ids = GetFileMenuCommandIds();
            var manager = new OpenLiveWriter.ApplicationFramework.CommandManager();
            foreach (var id in ids)
            {
                manager.Add(id, (sender, args) => onExecute(id));
            }
            return manager;
        }

        [Test]
        public void AllFileMenuCommands_ResolveThroughBridge()
        {
            var source = CreateSourceCommandManager(out var ids, _ => { });
            var bridge = new CommandManagerBridge(source);
            var ribbonManager = bridge.RibbonCommandManager;

            foreach (var id in ids)
            {
                Assert.NotNull(ribbonManager.GetCommand(id),
                    $"File menu command {id} does not resolve through the bridge");
            }
        }

        [Test]
        public void AllFileMenuCommands_ExecuteThroughBridge()
        {
            var invoked = new List<CommandId>();
            var source = CreateSourceCommandManager(out var ids, id => invoked.Add(id));
            var bridge = new CommandManagerBridge(source);

            // This is the execution path ApplicationMenu uses on a menu click.
            foreach (var id in ids)
            {
                var command = bridge.GetOrCreateBridgedCommand(id);
                command.PerformExecute();
            }

            Assert.That(invoked, Is.EquivalentTo(ids),
                "Every File menu command must reach its source handler when executed; " +
                "a command that does not fire has no source command and would be a dead menu item");
        }

        [Test]
        public void FileMenuItems_AreDistinctAndNonEmpty()
        {
            var ids = GetFileMenuCommandIds();
            Assert.Greater(ids.Count, 0, "File menu has no items");
            Assert.That(ids, Is.Unique,
                "File menu contains a duplicate CommandId");
        }
    }
}
