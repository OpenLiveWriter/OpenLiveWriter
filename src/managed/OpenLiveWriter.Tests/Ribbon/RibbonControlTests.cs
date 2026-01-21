// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Controls;

namespace OpenLiveWriter.Tests.Ribbon
{
    [TestFixture]
    public class RibbonControlTests
    {
        [Test]
        public void RibbonButton_DefaultSizeIsLarge()
        {
            using var button = new RibbonButton();
            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Large));
        }

        [Test]
        public void RibbonButton_ChangingCurrentSize_UpdatesSize()
        {
            using var button = new RibbonButton();
            button.CurrentSize = RibbonGroupSize.Small;

            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Small));
        }

        [Test]
        public void RibbonButton_CanSetCommandId()
        {
            using var button = new RibbonButton();
            button.CommandId = CommandId.Bold;

            Assert.That(button.CommandId, Is.EqualTo(CommandId.Bold));
        }

        [Test]
        public void RibbonButton_ButtonTypeDefaultsToButton()
        {
            using var button = new RibbonButton();
            Assert.That(button.ButtonType, Is.EqualTo(RibbonButtonType.Button));
        }

        [Test]
        public void RibbonGroup_CanAddControls()
        {
            using var group = new RibbonGroup();
            using var button = new RibbonButton();

            group.AddControl(button);

            Assert.That(group.Controls.Count, Is.EqualTo(1));
        }

        [Test]
        public void RibbonGroup_CanSetLabel()
        {
            using var group = new RibbonGroup();
            group.Label = "Clipboard";

            Assert.That(group.Label, Is.EqualTo("Clipboard"));
        }

        [Test]
        public void RibbonGroup_CanSetVisibleModes()
        {
            using var group = new RibbonGroup();
            group.VisibleModes = RibbonApplicationMode.Normal | RibbonApplicationMode.Preview;

            Assert.That(group.VisibleModes, Is.EqualTo(RibbonApplicationMode.Normal | RibbonApplicationMode.Preview));
        }

        [Test]
        public void RibbonTab_CanAddGroups()
        {
            using var tab = new RibbonTab();
            using var group = new RibbonGroup();

            tab.AddGroup(group);

            Assert.That(tab.Groups.Count, Is.EqualTo(1));
        }

        [Test]
        public void RibbonTab_CanSetLabel()
        {
            using var tab = new RibbonTab();
            tab.Label = "Home";

            Assert.That(tab.Label, Is.EqualTo("Home"));
        }

        [Test]
        public void RibbonTab_DefaultContextualGroupIsNone()
        {
            using var tab = new RibbonTab();
            Assert.That(tab.ContextualGroup, Is.EqualTo(RibbonContextualTabGroup.None));
        }

        [Test]
        public void RibbonSeparator_HasCorrectDefaultWidth()
        {
            using var separator = new RibbonSeparator();
            Assert.That(separator.Width, Is.GreaterThan(0));
        }

        [Test]
        public void RibbonButton_ExecuteCommand_WhenCommandManagerSet()
        {
            using var button = new RibbonButton();
            var manager = new RibbonCommandManager();
            var command = new TestRibbonCommand(CommandId.Paste);
            manager.RegisterCommand(command);

            button.CommandManager = manager;
            button.CommandId = CommandId.Paste;
            button.ExecuteCommand();

            Assert.That(command.ExecuteCallCount, Is.EqualTo(1));
        }

        [Test]
        public void RibbonButton_PerformClick_ExecutesCommand()
        {
            using var button = new RibbonButton();
            var manager = new RibbonCommandManager();
            var command = new TestRibbonCommand(CommandId.Cut);
            manager.RegisterCommand(command);

            button.CommandManager = manager;
            button.CommandId = CommandId.Cut;
            button.PerformClick();

            Assert.That(command.ExecuteCallCount, Is.EqualTo(1));
        }

        /// <summary>
        /// Test implementation of IRibbonCommand
        /// </summary>
        private class TestRibbonCommand : IRibbonCommand
        {
            public CommandId Id { get; }
            public string Label => Id.ToString();
            public string Tooltip => Label;
            public string Keytip => "";
            public Image LargeImage => null;
            public Image SmallImage => null;
            public bool Enabled { get; set; } = true;
            public bool Visible { get; set; } = true;
            public bool Checked { get; set; }
            public int ExecuteCallCount { get; private set; }

            public event EventHandler Execute;
#pragma warning disable CS0067 // Event is never used
            public event EventHandler StateChanged;
#pragma warning restore CS0067

            public TestRibbonCommand(CommandId id)
            {
                Id = id;
            }

            public void PerformExecute()
            {
                ExecuteCallCount++;
                Execute?.Invoke(this, EventArgs.Empty);
            }

            public void Invalidate() { }
        }
    }
}
