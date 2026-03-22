// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Configuration;
using OpenLiveWriter.Ribbon.Managed.Controls;

namespace OpenLiveWriter.Tests.Ribbon
{
    /// <summary>
    /// Tests to verify that ribbon buttons and controls function correctly.
    /// </summary>
    [TestFixture]
    public class RibbonButtonFunctionalityTests
    {
        private RibbonCommandManager _commandManager;
        private List<TestCommand> _testCommands;

        [SetUp]
        public void SetUp()
        {
            _commandManager = new RibbonCommandManager();
            _testCommands = new List<TestCommand>();
        }

        [TearDown]
        public void TearDown()
        {
            _testCommands.Clear();
        }

        #region Button Click Tests

        [Test]
        public void Button_Click_ExecutesCommand()
        {
            var command = CreateAndRegisterCommand(CommandId.Paste);
            using var button = new RibbonButton
            {
                CommandId = CommandId.Paste,
                CommandManager = _commandManager
            };

            button.ExecuteCommand();

            Assert.That(command.ExecuteCount, Is.EqualTo(1));
        }

        [Test]
        public void Button_PerformClick_ExecutesCommand()
        {
            var command = CreateAndRegisterCommand(CommandId.Cut);
            using var button = new RibbonButton
            {
                CommandId = CommandId.Cut,
                CommandManager = _commandManager
            };

            button.PerformClick();

            Assert.That(command.ExecuteCount, Is.EqualTo(1));
        }

        [Test]
        public void Button_DisabledState_DoesNotExecuteCommand()
        {
            var command = CreateAndRegisterCommand(CommandId.Bold);
            command.Enabled = false;
            using var button = new RibbonButton
            {
                CommandId = CommandId.Bold,
                CommandManager = _commandManager
            };
            button.Enabled = false;

            button.ExecuteCommand();

            Assert.That(command.ExecuteCount, Is.EqualTo(0));
        }

        [Test]
        public void Button_MultipleClicks_ExecutesMultipleTimes()
        {
            var command = CreateAndRegisterCommand(CommandId.Undo);
            using var button = new RibbonButton
            {
                CommandId = CommandId.Undo,
                CommandManager = _commandManager
            };

            button.ExecuteCommand();
            button.ExecuteCommand();
            button.ExecuteCommand();

            Assert.That(command.ExecuteCount, Is.EqualTo(3));
        }

        #endregion

        #region Toggle Button Tests

        [Test]
        public void ToggleButton_HasCorrectType()
        {
            using var button = new RibbonButton
            {
                ButtonType = RibbonButtonType.ToggleButton
            };

            Assert.That(button.ButtonType, Is.EqualTo(RibbonButtonType.ToggleButton));
        }

        [Test]
        public void ToggleButton_CanBeAssignedCommand()
        {
            var command = CreateAndRegisterCommand(CommandId.Bold);
            using var button = new RibbonButton
            {
                CommandId = CommandId.Bold,
                CommandManager = _commandManager,
                ButtonType = RibbonButtonType.ToggleButton
            };

            Assert.That(button.CommandId, Is.EqualTo(CommandId.Bold));
        }

        #endregion

        #region Split Button Tests

        [Test]
        public void SplitButton_HasCorrectType()
        {
            using var button = new RibbonButton
            {
                ButtonType = RibbonButtonType.SplitButton
            };

            Assert.That(button.ButtonType, Is.EqualTo(RibbonButtonType.SplitButton));
        }

        [Test]
        public void DropDownButton_HasCorrectType()
        {
            using var button = new RibbonButton
            {
                ButtonType = RibbonButtonType.DropDownButton
            };

            Assert.That(button.ButtonType, Is.EqualTo(RibbonButtonType.DropDownButton));
        }

        [Test]
        public void SplitButton_CanBeConfiguredWithCommand()
        {
            var command = CreateAndRegisterCommand(CommandId.InsertImageSplit);
            using var button = new RibbonButton
            {
                ButtonType = RibbonButtonType.SplitButton,
                CommandId = CommandId.InsertImageSplit,
                CommandManager = _commandManager
            };

            Assert.That(button.CommandId, Is.EqualTo(CommandId.InsertImageSplit));
            Assert.That(button.ButtonType, Is.EqualTo(RibbonButtonType.SplitButton));
        }

        #endregion

        #region Button State Tests

        [Test]
        public void Button_EnabledProperty_AffectsVisualState()
        {
            using var button = new RibbonButton();
            
            button.Enabled = false;
            
            Assert.That(button.Enabled, Is.False);
        }

        [Test]
        public void Button_VisibleProperty_AffectsDisplay()
        {
            using var button = new RibbonButton();
            
            button.Visible = false;
            
            Assert.That(button.Visible, Is.False);
        }

        [Test]
        public void Button_SizeProperty_CanBeChanged()
        {
            using var button = new RibbonButton();
            
            button.CurrentSize = RibbonGroupSize.Small;
            
            Assert.That(button.CurrentSize, Is.EqualTo(RibbonGroupSize.Small));
        }

        #endregion

        #region Configuration Integration Tests

        [Test]
        public void Configuration_HomeTabClipboardGroup_AllButtonsHaveCommands()
        {
            var config = DefaultRibbonConfiguration.Create();
            var homeTab = config.Tabs[0];
            var clipboardGroup = homeTab.Groups[0];

            foreach (var control in clipboardGroup.Controls)
            {
                Assert.That(control.CommandId, Is.Not.EqualTo(CommandId.None),
                    $"Control in Clipboard group has no command assigned");
            }
        }

        [Test]
        public void Configuration_InsertTabMediaGroup_AllButtonsHaveCommands()
        {
            var config = DefaultRibbonConfiguration.Create();
            var insertTab = config.Tabs[1];
            var mediaGroup = insertTab.Groups.Find(g => g.CommandId == CommandId.MediaGroup);

            Assert.That(mediaGroup, Is.Not.Null);
            foreach (var control in mediaGroup.Controls)
            {
                Assert.That(control.CommandId, Is.Not.EqualTo(CommandId.None),
                    $"Control in Media group has no command assigned");
            }
        }

        [Test]
        public void Configuration_BlogAccountTab_AllButtonsHaveCommands()
        {
            var config = DefaultRibbonConfiguration.Create();
            var blogTab = config.Tabs[2];

            foreach (var group in blogTab.Groups)
            {
                foreach (var control in group.Controls)
                {
                    Assert.That(control.CommandId, Is.Not.EqualTo(CommandId.None),
                        $"Control in {group.Label} group has no command assigned");
                }
            }
        }

        #endregion

        #region Command Manager Integration Tests

        [Test]
        public void CommandManager_ExecuteCommand_FiresExecuteEvent()
        {
            var command = CreateAndRegisterCommand(CommandId.SavePost);
            var eventFired = false;
            command.Execute += (s, e) => eventFired = true;

            _commandManager.Execute(CommandId.SavePost);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void CommandManager_GetCommand_ReturnsRegisteredCommand()
        {
            var command = CreateAndRegisterCommand(CommandId.NewPost);

            var retrieved = _commandManager.GetCommand(CommandId.NewPost);

            Assert.That(retrieved, Is.EqualTo(command));
        }

        [Test]
        public void CommandManager_UnregisteredCommand_ReturnsNull()
        {
            var retrieved = _commandManager.GetCommand(CommandId.About);

            Assert.That(retrieved, Is.Null);
        }

        #endregion

        #region Button Type Tests for All Groups

        [Test]
        public void HomeTab_PublishButton_IsSplitOrRegular()
        {
            var config = DefaultRibbonConfiguration.Create();
            var homeTab = config.Tabs[0];
            var publishGroup = homeTab.Groups[1];
            var publishButton = publishGroup.Controls[0] as ButtonConfig;

            Assert.That(publishButton, Is.Not.Null);
            Assert.That(publishButton.CommandId, Is.EqualTo(CommandId.PostAndPublish));
        }

        [Test]
        public void InsertTab_TableButton_IsDropDown()
        {
            var config = DefaultRibbonConfiguration.Create();
            var insertTab = config.Tabs[1];
            var tablesGroup = insertTab.Groups.Find(g => g.CommandId == CommandId.TablesGroup);
            var tableButton = tablesGroup?.Controls[0] as ButtonConfig;

            Assert.That(tableButton, Is.Not.Null);
            Assert.That(tableButton.ButtonType, Is.EqualTo(RibbonButtonType.DropDownButton));
        }

        [Test]
        public void BlogAccountTab_ThemeToggle_IsToggleButton()
        {
            var config = DefaultRibbonConfiguration.Create();
            var blogTab = config.Tabs[2];
            var themeGroup = blogTab.Groups.Find(g => g.CommandId == CommandId.BlogProviderThemeGroup);
            var themeToggle = themeGroup?.Controls[0] as ToggleButtonConfig;

            Assert.That(themeToggle, Is.Not.Null);
            Assert.That(themeToggle.CommandId, Is.EqualTo(CommandId.ViewUseStyles));
        }

        #endregion

        #region Helpers

        private TestCommand CreateAndRegisterCommand(CommandId id)
        {
            var command = new TestCommand(id);
            _testCommands.Add(command);
            _commandManager.RegisterCommand(command);
            return command;
        }

        private class TestCommand : IRibbonCommand
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
            public int ExecuteCount { get; private set; }

            public event EventHandler Execute;
#pragma warning disable CS0067
            public event EventHandler StateChanged;
#pragma warning restore CS0067

            public TestCommand(CommandId id)
            {
                Id = id;
            }

            public void PerformExecute()
            {
                if (Enabled)
                {
                    ExecuteCount++;
                    Execute?.Invoke(this, EventArgs.Empty);
                }
            }

            public void Invalidate() { }
        }

        #endregion
    }
}
