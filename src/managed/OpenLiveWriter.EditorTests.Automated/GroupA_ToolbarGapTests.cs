// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group A (documented gap) — the WYSIWYG bridge can apply any block via
    /// formatBlock (h1–h6/p/pre, see GroupA_EditorCommandTests.FormatBlock_*), but
    /// the toolbar HeadingCombo in EditorPanel currently only maps indices to
    /// h1/h2/h3 (everything else falls through to "p"). These tests pin that gap so
    /// wiring the SemanticHtmlGallery / extending the combo (backlog P0-5) will make
    /// the "unreachable" assertions flip.
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    [Category("Gap")]
    public class GroupA_ToolbarGapTests
    {
        // Mirrors EditorPanel.SetupToolbarButtons' HeadingCombo.SelectionChanged
        // switch: index 1->h1, 2->h2, 3->h3, anything else -> "p".
        private static string MapComboIndexToTag(int index) => index switch
        {
            1 => "h1",
            2 => "h2",
            3 => "h3",
            _ => "p"
        };

        [TestCase(1, "h1")]
        [TestCase(2, "h2")]
        [TestCase(3, "h3")]
        public void HeadingCombo_ReachesH1ToH3(int index, string expected)
        {
            Assert.That(MapComboIndexToTag(index), Is.EqualTo(expected));
        }

        [TestCase(4)] // would be h4
        [TestCase(5)] // would be h5
        [TestCase(6)] // would be h6
        [TestCase(7)] // would be pre
        public void HeadingCombo_DoesNotReachH4ToH6OrPre_DocumentsGap(int index)
        {
            // Currently every index past 3 collapses to a plain paragraph. When the
            // SemanticHtmlGallery is wired, extend the map and update this test.
            Assert.That(MapComboIndexToTag(index), Is.EqualTo("p"),
                "Toolbar cannot yet reach h4-h6/pre; only the bridge formatBlock can.");
        }
    }
}
