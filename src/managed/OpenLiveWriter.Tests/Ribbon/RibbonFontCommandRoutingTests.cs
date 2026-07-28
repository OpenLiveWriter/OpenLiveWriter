// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using NUnit.Framework;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Controls;

namespace OpenLiveWriter.Tests.Ribbon
{
    /// <summary>
    /// Tests that ribbon font controls route user selections into the underlying
    /// commands (the TextEditingCommandDispatcher gallery/color commands in the app).
    /// </summary>
    [TestFixture]
    public class RibbonFontCommandRoutingTests
    {
        [Test]
        [Apartment(ApartmentState.STA)]
        public void ComboBox_SelectionChange_PushesSelectedIndexToGalleryCommandAndExecutes()
        {
            var commandManager = new RibbonCommandManager();
            var galleryCommand = new TestGalleryCommand(CommandId.FontFamily, "Arial", "Calibri", "Times New Roman");
            commandManager.RegisterCommand(galleryCommand);

            using var comboBox = new RibbonComboBox
            {
                CommandId = CommandId.FontFamily,
                CommandManager = commandManager
            };

            comboBox.SelectedIndex = 2;

            Assert.That(galleryCommand.SelectedIndex, Is.EqualTo(2));
            Assert.That(galleryCommand.ExecuteCallCount, Is.EqualTo(1));
        }

        [Test]
        public void BridgedCommand_SelectedColor_PassedToSourceCommandAsExecuteArg()
        {
            var existingCommandManager = new CommandManager();
            Color? receivedColor = null;
            var sourceCommand = new Command(CommandId.FontColorPicker);
            sourceCommand.ExecuteWithArgs += (s, e) => receivedColor = e.GetColor("SelectedColor");
            existingCommandManager.Add(sourceCommand);

            var bridge = new CommandManagerBridge(existingCommandManager);
            var bridgedCommand = bridge.GetOrCreateBridgedCommand(CommandId.FontColorPicker);

            bridgedCommand.SelectedColor = Color.Red;
            bridgedCommand.PerformExecute();

            Assert.That(receivedColor, Is.EqualTo(Color.Red));
            // The color is consumed by the execution and cleared afterwards.
            Assert.That(bridgedCommand.SelectedColor, Is.Null);
        }

        [Test]
        public void BridgedCommand_GalleryExecute_UsesSelectedIndexPushedByControl()
        {
            var existingCommandManager = new CommandManager();
            int? receivedIndex = null;
            var sourceCommand = new TestGallerySourceCommand(CommandId.FontSize);
            sourceCommand.ExecuteWithArgs += (s, e) => receivedIndex = e.GetInt(CommandId.FontSize.ToString());
            existingCommandManager.Add(sourceCommand);

            var bridge = new CommandManagerBridge(existingCommandManager);
            var bridgedCommand = (BridgedCommand)bridge.RibbonCommandManager.GetCommand(CommandId.FontSize);

            // Mirror what RibbonComboBox now does on selection change.
            bridgedCommand.SelectedIndex = 3;
            bridgedCommand.PerformExecute();

            Assert.That(receivedIndex, Is.EqualTo(3));
        }

        /// <summary>
        /// Test implementation of IGalleryCommand used to verify combo box routing.
        /// </summary>
        private class TestGalleryCommand : IGalleryCommand
        {
            private readonly List<CommandGalleryItem> _items = new List<CommandGalleryItem>();

            public TestGalleryCommand(CommandId id, params string[] itemLabels)
            {
                Id = id;
                foreach (var label in itemLabels)
                {
                    _items.Add(new CommandGalleryItem(label));
                }
                SelectedIndex = -1;
            }

            public CommandId Id { get; }
            public string Label => Id.ToString();
            public string Tooltip => Label;
            public string Keytip => "";
            public Image LargeImage => null;
            public Image SmallImage => null;
            public bool Enabled { get; set; } = true;
            public bool Visible { get; set; } = true;
            public bool Checked { get; set; }

            public IReadOnlyList<CommandGalleryItem> GalleryItems => _items;
            public int SelectedIndex { get; set; }
            public int ExecuteCallCount { get; private set; }

            public event EventHandler Execute;
#pragma warning disable CS0067 // Events are never used - required by interface
            public event EventHandler StateChanged;
            public event EventHandler ItemsChanged;
#pragma warning restore CS0067

            public void PerformExecute()
            {
                ExecuteCallCount++;
                Execute?.Invoke(this, EventArgs.Empty);
            }

            public void Invalidate() { }
        }

        /// <summary>
        /// A source Command with a settable SelectedIndex property, standing in for
        /// GalleryCommand&lt;T&gt; so the bridged gallery execution path is taken.
        /// </summary>
        private class TestGallerySourceCommand : Command
        {
            public TestGallerySourceCommand(CommandId commandId)
                : base(commandId)
            {
            }

            public int SelectedIndex { get; set; } = -1;
        }
    }
}
