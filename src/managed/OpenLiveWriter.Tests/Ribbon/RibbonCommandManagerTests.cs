// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;

namespace OpenLiveWriter.Tests.Ribbon
{
    [TestFixture]
    public class RibbonCommandManagerTests
    {
        private RibbonCommandManager _commandManager;

        [SetUp]
        public void SetUp()
        {
            _commandManager = new RibbonCommandManager();
        }

        [Test]
        public void RegisterCommand_AddsCommandToManager()
        {
            var command = new TestRibbonCommand(CommandId.Paste);
            _commandManager.RegisterCommand(command);

            Assert.That(_commandManager.HasCommand(CommandId.Paste), Is.True);
        }

        [Test]
        public void GetCommand_ReturnsRegisteredCommand()
        {
            var command = new TestRibbonCommand(CommandId.Cut);
            _commandManager.RegisterCommand(command);

            var retrieved = _commandManager.GetCommand(CommandId.Cut);

            Assert.That(retrieved, Is.SameAs(command));
        }

        [Test]
        public void GetCommand_ReturnsNullForUnregisteredCommand()
        {
            var retrieved = _commandManager.GetCommand(CommandId.Undo);

            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void UnregisterCommand_RemovesCommand()
        {
            var command = new TestRibbonCommand(CommandId.Cut);
            _commandManager.RegisterCommand(command);
            _commandManager.UnregisterCommand(CommandId.Cut);

            Assert.That(_commandManager.HasCommand(CommandId.Cut), Is.False);
        }

        [Test]
        public void SetEnabled_UpdatesCommandState()
        {
            var command = new TestRibbonCommand(CommandId.Bold) { Enabled = true };
            _commandManager.RegisterCommand(command);

            _commandManager.SetEnabled(CommandId.Bold, false);

            Assert.That(command.Enabled, Is.False);
        }

        [Test]
        public void SetVisible_UpdatesCommandState()
        {
            var command = new TestRibbonCommand(CommandId.Italic) { Visible = true };
            _commandManager.RegisterCommand(command);

            _commandManager.SetVisible(CommandId.Italic, false);

            Assert.That(command.Visible, Is.False);
        }

        [Test]
        public void SetChecked_UpdatesCommandState()
        {
            var command = new TestRibbonCommand(CommandId.Underline) { Checked = false };
            _commandManager.RegisterCommand(command);

            _commandManager.SetChecked(CommandId.Underline, true);

            Assert.That(command.Checked, Is.True);
        }

        [Test]
        public void Execute_CallsPerformExecute()
        {
            var command = new TestRibbonCommand(CommandId.SelectAll);
            _commandManager.RegisterCommand(command);

            _commandManager.Execute(CommandId.SelectAll);

            Assert.That(command.ExecuteCallCount, Is.EqualTo(1));
        }

        [Test]
        public void CommandStateChanged_RaisedWhenCommandChanges()
        {
            var command = new TestRibbonCommand(CommandId.Redo);
            _commandManager.RegisterCommand(command);

            CommandId? changedCommandId = null;
            _commandManager.CommandStateChanged += (s, e) => changedCommandId = e.CommandId;

            command.Enabled = false;

            Assert.That(changedCommandId, Is.EqualTo(CommandId.Redo));
        }

        /// <summary>
        /// Test implementation of IRibbonCommand
        /// </summary>
        private class TestRibbonCommand : IRibbonCommand
        {
            private bool _enabled = true;
            private bool _visible = true;
            private bool _checked;

            public CommandId Id { get; }
            public string Label => Id.ToString();
            public string Tooltip => Label;
            public string Keytip => "";
            public System.Drawing.Image LargeImage => null;
            public System.Drawing.Image SmallImage => null;

            public bool Enabled
            {
                get => _enabled;
                set
                {
                    if (_enabled != value)
                    {
                        _enabled = value;
                        StateChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            public bool Visible
            {
                get => _visible;
                set
                {
                    if (_visible != value)
                    {
                        _visible = value;
                        StateChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            public bool Checked
            {
                get => _checked;
                set
                {
                    if (_checked != value)
                    {
                        _checked = value;
                        StateChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            public int ExecuteCallCount { get; private set; }

            public event EventHandler Execute;
            public event EventHandler StateChanged;

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
