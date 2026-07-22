// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group K — contextual-tab activation. The selection-context → active-tab-group
    /// mapping (<see cref="ContextualTabResolver"/>) is pure and asserted headlessly,
    /// as is the parsing of the new <c>inTable</c>/<c>selectedElementType</c> fields
    /// out of <c>getState()</c>. A headless Avalonia UI test exercises the ribbon
    /// control actually showing/hiding the contextual tab. The live caret round-trip
    /// (getState reporting the context inside a real WebView) stays manual.
    /// </summary>
    [TestFixture]
    [Category("GroupK")]
    public class GroupK_ContextualTabTests
    {
        // ---- Pure mapping: selection context → contextual tab group ----

        [Test]
        public void Resolve_InTable_ShowsTableTools()
        {
            var state = new FormatState { InTable = true };
            Assert.That(ContextualTabResolver.Resolve(state),
                Is.EqualTo(RibbonContextualTabGroup.TableTools));
        }

        [TestCase("image", RibbonContextualTabGroup.ImageTools)]
        [TestCase("video", RibbonContextualTabGroup.VideoTools)]
        [TestCase("map", RibbonContextualTabGroup.MapTools)]
        [TestCase("tag", RibbonContextualTabGroup.TagTools)]
        public void Resolve_SelectedElement_ShowsMatchingTools(string type, RibbonContextualTabGroup expected)
        {
            var state = new FormatState { SelectedElementType = type };
            Assert.That(ContextualTabResolver.Resolve(state), Is.EqualTo(expected));
        }

        [Test]
        public void Resolve_SelectedElementTakesPriorityOverTable()
        {
            // Selecting an image inside a table shows Picture Tools, not Table Tools.
            var state = new FormatState { InTable = true, SelectedElementType = "image" };
            Assert.That(ContextualTabResolver.Resolve(state),
                Is.EqualTo(RibbonContextualTabGroup.ImageTools));
        }

        [Test]
        public void Resolve_PlainText_ShowsNone()
        {
            Assert.That(ContextualTabResolver.Resolve(new FormatState()),
                Is.EqualTo(RibbonContextualTabGroup.None));
            Assert.That(ContextualTabResolver.Resolve(null),
                Is.EqualTo(RibbonContextualTabGroup.None));
        }

        [Test]
        public void Resolve_UnknownElementType_ShowsNone()
        {
            var state = new FormatState { SelectedElementType = "widget" };
            Assert.That(ContextualTabResolver.Resolve(state),
                Is.EqualTo(RibbonContextualTabGroup.None));
        }

        // ---- Parsing the new context fields out of getState() JSON ----

        [Test]
        public void ParseState_ReadsTableAndSelectedElementContext()
        {
            string json = "{\"blockTag\":\"p\",\"inTable\":true,\"selectedElementType\":\"Image\"}";
            FormatState state = WebViewEditor.ParseFormatStateJson(json);

            Assert.Multiple(() =>
            {
                Assert.That(state.InTable, Is.True);
                Assert.That(state.SelectedElementType, Is.EqualTo("image")); // normalized lower-case
            });
        }

        [Test]
        public void ParseState_NoContext_DefaultsToBodyText()
        {
            FormatState state = WebViewEditor.ParseFormatStateJson("{\"blockTag\":\"p\"}");
            Assert.Multiple(() =>
            {
                Assert.That(state.InTable, Is.False);
                Assert.That(state.SelectedElementType, Is.Null);
                Assert.That(ContextualTabResolver.Resolve(state),
                    Is.EqualTo(RibbonContextualTabGroup.None));
            });
        }

        [TestCase("Image", "image")]
        [TestCase("  video  ", "video")]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void NormalizeElementType_Canonicalizes(string input, string expected)
        {
            Assert.That(WebViewEditor.NormalizeElementType(input), Is.EqualTo(expected));
        }

        // ---- Ribbon actually shows / hides the contextual tab (headless UI) ----

        [AvaloniaTest]
        public void Ribbon_ActivatesAndDeactivatesTableToolsTab()
        {
            var ribbon = new AvaloniaRibbonControl();
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());

            Assert.That(ribbon.ActiveContextualGroup, Is.EqualTo(RibbonContextualTabGroup.None));
            Assert.That(HasTab(ribbon, "Layout"), Is.False, "Table Tools tab should be hidden initially");

            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.TableTools);
            Assert.That(ribbon.ActiveContextualGroup, Is.EqualTo(RibbonContextualTabGroup.TableTools));
            Assert.That(HasTab(ribbon, "Layout"), Is.True, "Table Tools tab should appear");

            // Switching to a different contextual group swaps the visible contextual tab.
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);
            Assert.That(HasTab(ribbon, "Layout"), Is.False, "Table Tools tab should be gone");

            // Returning to body text hides all contextual tabs.
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.None);
            Assert.That(ribbon.ActiveContextualGroup, Is.EqualTo(RibbonContextualTabGroup.None));
            Assert.That(HasTab(ribbon, "Layout"), Is.False);
            // Base tabs remain present throughout.
            Assert.That(HasTab(ribbon, "Home"), Is.True);
        }

        [AvaloniaTest]
        public void Ribbon_ActivatesAndDeactivatesPictureToolsTab()
        {
            var ribbon = new AvaloniaRibbonControl();
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());

            Assert.That(HasTab(ribbon, "Format"), Is.False, "Picture Tools tab should be hidden initially");

            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);
            Assert.That(ribbon.ActiveContextualGroup, Is.EqualTo(RibbonContextualTabGroup.ImageTools));
            Assert.That(HasTab(ribbon, "Format"), Is.True, "Picture Tools Format tab should appear");

            // Deselecting the image (back to body text) hides the contextual tab.
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.None);
            Assert.That(HasTab(ribbon, "Format"), Is.False);
            Assert.That(HasTab(ribbon, "Home"), Is.True);
        }

        [AvaloniaTest]
        public void Ribbon_ReactivatingSameGroupIsNoOp()
        {
            var ribbon = new AvaloniaRibbonControl();
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());

            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.TableTools);
            Assert.That(HasTab(ribbon, "Layout"), Is.True);
            // Idempotent — no throw, tab remains.
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.TableTools);
            Assert.That(HasTab(ribbon, "Layout"), Is.True);
        }

        private static bool HasTab(AvaloniaRibbonControl ribbon, string label) =>
            ribbon.GetLogicalDescendants()
                .OfType<ToggleButton>()
                .Any(b => b.Content as string == label);
    }
}
