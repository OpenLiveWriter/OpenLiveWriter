// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group Q — P0 trust breakers, menu bar + Debug tab gating. The macOS menu
    /// structure is asserted from <see cref="ShellMenuBuilder"/> descriptors
    /// (label → CommandId → gesture), including that every menu command is one the
    /// shell actually handles, and that the Debug ribbon tab is developer chrome
    /// hidden unless OLW_DEBUG_RIBBON is set.
    /// </summary>
    [TestFixture]
    [Category("GroupQ")]
    public class GroupQ_ShellMenuTests
    {
        private static ShellMenu Menu(string label) =>
            ShellMenuBuilder.Build().First(m => m.Label == label);

        private static ShellMenuItem Item(ShellMenu menu, CommandId commandId) =>
            menu.Items.FirstOrDefault(i => !i.IsSeparator && i.CommandId == commandId);

        [Test]
        public void MenuBar_HasFileEditViewHelp()
        {
            Assert.That(ShellMenuBuilder.Build().Select(m => m.Label),
                Is.EqualTo(new[] { "File", "Edit", "View", "Help" }));
        }

        [TestCase(CommandId.NewPost, "Cmd+N")]
        [TestCase(CommandId.NewPage, "Cmd+Shift+N")]
        [TestCase(CommandId.OpenDrafts, "Cmd+O")]
        [TestCase(CommandId.SavePost, "Cmd+S")]
        [TestCase(CommandId.DeleteDraft, null)]
        [TestCase(CommandId.ShowCategoryPopup, null)]
        [TestCase(CommandId.Options, "Cmd+,")]
        [TestCase(CommandId.About, null)]
        [TestCase(CommandId.Close, "Cmd+W")]
        public void FileMenu_MapsCommandsAndGestures(CommandId commandId, string gesture)
        {
            ShellMenuItem item = Item(Menu("File"), commandId);
            Assert.That(item, Is.Not.Null, $"File menu must contain {commandId}");
            Assert.That(item.Gesture, Is.EqualTo(gesture));
        }

        [TestCase(CommandId.Undo, "Cmd+Z")]
        [TestCase(CommandId.Redo, "Cmd+Shift+Z")]
        [TestCase(CommandId.Cut, "Cmd+X")]
        [TestCase(CommandId.CopyCommand, "Cmd+C")]
        [TestCase(CommandId.Paste, "Cmd+V")]
        [TestCase(CommandId.PasteSpecial, "Cmd+Shift+V")]
        [TestCase(CommandId.SelectAll, "Cmd+A")]
        [TestCase(CommandId.FindButton, "Cmd+F")]
        public void EditMenu_MapsCommandsAndGestures(CommandId commandId, string gesture)
        {
            ShellMenuItem item = Item(Menu("Edit"), commandId);
            Assert.That(item, Is.Not.Null, $"Edit menu must contain {commandId}");
            Assert.That(item.Gesture, Is.EqualTo(gesture));
        }

        [Test]
        public void ViewMenu_SwitchesEditSourcePreview()
        {
            var view = Menu("View");
            Assert.That(view.Items.Where(i => !i.IsSeparator).Select(i => i.CommandId),
                Is.EqualTo(new[] { CommandId.ViewNormal, CommandId.ViewSource, CommandId.ViewPreview }));
        }

        [Test]
        public void CategoriesMenuItem_RoutesShowCategoryPopup()
        {
            // P1-1: the categories picker was implemented but unreachable.
            ShellMenuItem item = Item(Menu("File"), CommandId.ShowCategoryPopup);
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Label, Does.Contain("Categories"));
        }

        [Test]
        public void EveryMenuCommand_IsHandledByTheShell()
        {
            // Menus must never point at a dead command.
            var unhandled = ShellMenuBuilder.Build()
                .SelectMany(m => m.Items)
                .Where(i => !i.IsSeparator && !HandledCommands.IsHandled(i.CommandId))
                .Select(i => i.CommandId)
                .ToList();
            Assert.That(unhandled, Is.Empty);
        }

        [Test]
        public void EveryMenuGesture_ParsesAsKeyGesture()
        {
            foreach (string gesture in ShellMenuBuilder.Build()
                         .SelectMany(m => m.Items)
                         .Where(i => i.Gesture != null)
                         .Select(i => i.Gesture))
            {
                KeyGesture parsed = null;
                Assert.DoesNotThrow(() => parsed = KeyGesture.Parse(gesture), gesture);
                Assert.That(parsed, Is.Not.Null, gesture);
            }
        }

        // ---- Debug tab gating (P0-5) ----

        private string _savedDebugEnv;

        [SetUp]
        public void SaveDebugEnv()
        {
            _savedDebugEnv = Environment.GetEnvironmentVariable("OLW_DEBUG_RIBBON");
            Environment.SetEnvironmentVariable("OLW_DEBUG_RIBBON", null);
        }

        [TearDown]
        public void RestoreDebugEnv()
        {
            Environment.SetEnvironmentVariable("OLW_DEBUG_RIBBON", _savedDebugEnv);
        }

        [Test]
        public void DefaultActiveModes_ExcludesDebug()
        {
            Assert.That(AvaloniaRibbonControl.DefaultActiveModes.HasFlag(RibbonApplicationMode.Debug),
                Is.False, "Debug tab must not ship enabled by default");
        }

        [TestCase("1")]
        [TestCase("true")]
        public void DefaultActiveModes_DebugEnvVar_EnablesDebugTab(string value)
        {
            Environment.SetEnvironmentVariable("OLW_DEBUG_RIBBON", value);
            Assert.That(AvaloniaRibbonControl.DefaultActiveModes.HasFlag(RibbonApplicationMode.Debug),
                Is.True);
        }

        [AvaloniaTest]
        public void Ribbon_DefaultModes_HidesDebugTab()
        {
            var ribbon = new AvaloniaRibbonControl();
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());

            Assert.That(ribbon.TabStrip.Tabs.Select(t => t.Label),
                Is.Not.Contains("Debug"));
            Assert.That(ribbon.TabStrip.Tabs.Select(t => t.Label),
                Is.EqualTo(new[] { "Home", "Insert", "Blog Account" }));
        }

        [AvaloniaTest]
        public void Ribbon_DebugModeEnabled_ShowsDebugTab()
        {
            var ribbon = new AvaloniaRibbonControl();
            ribbon.ActiveModes |= RibbonApplicationMode.Debug;
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());

            Assert.That(ribbon.TabStrip.Tabs.Select(t => t.Label),
                Does.Contain("Debug"));
        }
    }
}
