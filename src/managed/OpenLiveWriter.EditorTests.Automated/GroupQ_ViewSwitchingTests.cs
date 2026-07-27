// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// View-switching behaviors that were previously only driver-verified:
    /// the Edit/Source/Preview tab control's state machine, and the shell rule
    /// that editor-targeted commands (inserts, formatting, document loads)
    /// always switch to the visible Edit view first.
    /// </summary>
    [TestFixture]
    [Category("GroupQ")]
    public class GroupQ_ViewSwitchingTests
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

        // ---- ViewToggleTabs state machine ----

        [AvaloniaTest]
        public void ViewTabs_StartOnEdit()
        {
            var tabs = new ViewToggleTabs();
            Assert.That(tabs.ActiveView, Is.EqualTo("edit"));
            Assert.That(Buttons(tabs)[0].IsChecked, Is.True);
            Assert.That(Buttons(tabs)[1].IsChecked, Is.False);
        }

        [AvaloniaTest]
        [TestCase("source", 1)]
        [TestCase("preview", 2)]
        [TestCase("edit", 0)]
        public void ViewTabs_ActiveViewChecksOnlyThatTab(string view, int checkedIndex)
        {
            var tabs = new ViewToggleTabs { ActiveView = view };
            var buttons = Buttons(tabs);
            for (int i = 0; i < buttons.Length; i++)
                Assert.That(buttons[i].IsChecked, Is.EqualTo(i == checkedIndex),
                    $"button {i} checked state for view '{view}'");
        }

        [AvaloniaTest]
        public void ViewTabs_ClickRaisesViewRequested()
        {
            var tabs = new ViewToggleTabs();
            string requested = null;
            tabs.ViewRequested += (s, view) => requested = view;

            Buttons(tabs)[2].RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

            Assert.That(requested, Is.EqualTo("preview"));
            Assert.That(tabs.ActiveView, Is.EqualTo("preview"));
        }

        [AvaloniaTest]
        public void ViewTabs_InvalidViewIgnored()
        {
            var tabs = new ViewToggleTabs { ActiveView = "bogus" };
            Assert.That(tabs.ActiveView, Is.EqualTo("edit"));
        }

        // ---- EditorPanel transitions (placeholder editor) ----

        [AvaloniaTest]
        public async Task SwitchView_RoundTripsAndRaisesViewChanged()
        {
            var panel = new EditorPanel();
            int changed = 0;
            panel.ViewChanged += (s, e) => changed++;

            await panel.SetViewAsync("source");
            Assert.That(panel.CurrentView, Is.EqualTo("source"));
            await panel.SetViewAsync("preview");
            Assert.That(panel.CurrentView, Is.EqualTo("preview"));
            await panel.SetViewAsync("edit");
            Assert.That(panel.CurrentView, Is.EqualTo("edit"));
            Assert.That(changed, Is.EqualTo(3));
        }

        [AvaloniaTest]
        public async Task PreviewToEdit_DoesNotThrowOrWipe()
        {
            // The stale-source wipe fix: switching Preview -> Edit must not push the
            // stale SourceEditor snapshot into the editor. With the placeholder
            // editor this is a smoke test that the transition completes cleanly.
            var panel = new EditorPanel();
            await panel.SetViewAsync("source");
            await panel.SetViewAsync("preview");
            await panel.SetViewAsync("edit");
            Assert.That(panel.CurrentView, Is.EqualTo("edit"));
        }

        // ---- Shell: commands switch to the Edit view first ----

        [AvaloniaTest]
        public async Task FormatCommand_FromPreview_SwitchesToEditView()
        {
            var window = new MainWindow();
            try
            {
                window.Show();
                var panel = window.FindControl<EditorPanel>("EditorPanel");
                panel.SetView("preview");
                Assert.That(panel.CurrentView, Is.EqualTo("preview"));

                await window.ExecuteCommandAsync(CommandId.Bold);

                Assert.That(panel.CurrentView, Is.EqualTo("edit"),
                    "a formatting command must bring the Edit view forward before applying");
            }
            finally
            {
                window.Close();
            }
        }

        private static ToggleButton[] Buttons(ViewToggleTabs tabs) =>
            tabs.GetLogicalChildren().OfType<ToggleButton>().ToArray();
    }
}
