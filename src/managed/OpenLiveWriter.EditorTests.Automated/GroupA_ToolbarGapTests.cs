// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group A10 — heading/semantic-block reachability. The WYSIWYG bridge can apply
    /// any block via formatBlock (h1-h6/p/pre, see GroupA_EditorCommandTests.FormatBlock_*).
    /// Previously the toolbar HeadingCombo only mapped indices to h1/h2/h3 (a documented
    /// gap); now the SemanticHtmlGallery is wired and the combo is extended, so the
    /// full range h1-h6 + preformatted is reachable through the pure combo → tag
    /// mapping. These assertions run headlessly against the real
    /// <see cref="EditorPanel.MapHeadingIndexToTag"/> / <see cref="SemanticHtmlStyles"/>
    /// logic — no live WebView required.
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    public class GroupA_ToolbarGapTests
    {
        [TestCase(0, "p")]   // Normal (paragraph)
        [TestCase(1, "h1")]
        [TestCase(2, "h2")]
        [TestCase(3, "h3")]
        public void HeadingCombo_ReachesNormalAndH1ToH3(int index, string expected)
        {
            Assert.That(EditorPanel.MapHeadingIndexToTag(index), Is.EqualTo(expected));
        }

        // Previously collapsed to "p" (the gap); now reachable after wiring the
        // SemanticHtmlGallery and extending the toolbar combo.
        [TestCase(4, "h4")]
        [TestCase(5, "h5")]
        [TestCase(6, "h6")]
        [TestCase(7, "pre")]
        public void HeadingCombo_NowReachesH4ToH6AndPre(int index, string expected)
        {
            Assert.That(EditorPanel.MapHeadingIndexToTag(index), Is.EqualTo(expected),
                "Toolbar combo + SemanticHtmlGallery must reach h4-h6/pre.");
        }

        // Out-of-range selection indices degrade to a plain paragraph.
        [TestCase(-1)]
        [TestCase(8)]
        [TestCase(99)]
        public void HeadingCombo_OutOfRange_FallsBackToParagraph(int index)
        {
            Assert.That(EditorPanel.MapHeadingIndexToTag(index), Is.EqualTo("p"));
        }

        [Test]
        public void SemanticHtmlStyles_ExposesAllHeadingLevelsAndPre()
        {
            var tags = new System.Collections.Generic.List<string>();
            foreach (var (_, tag) in SemanticHtmlStyles.Styles)
                tags.Add(tag);

            Assert.That(tags, Is.EquivalentTo(new[]
            {
                "p", "h1", "h2", "h3", "h4", "h5", "h6", "pre"
            }));
        }

        [TestCase("h4", true)]
        [TestCase("pre", true)]
        [TestCase("P", true)]      // case-insensitive
        [TestCase("h7", false)]
        [TestCase("div", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void SemanticHtmlStyles_IsKnownTag(string tag, bool expected)
        {
            Assert.That(SemanticHtmlStyles.IsKnownTag(tag), Is.EqualTo(expected));
        }
    }
}
