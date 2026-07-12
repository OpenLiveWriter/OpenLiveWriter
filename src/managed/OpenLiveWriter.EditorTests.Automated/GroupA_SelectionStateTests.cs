// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group A — selection-state reflection (polish). The editor's getState() now
    /// reports the caret's block tag, font family/size, and fore/highlight color.
    /// The parsing + normalization (rgb→hex, font-stack unquoting) that drives the
    /// ribbon combos is pure (<see cref="WebViewEditor.ParseFormatStateJson"/>) and
    /// asserted headlessly. The live caret round-trip stays [Explicit] (GroupA18).
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    public class GroupA_SelectionStateTests
    {
        [Test]
        public void ParseState_ReadsToggleFlagsAndBlockTag()
        {
            string json = "{\"bold\":true,\"italic\":false,\"underline\":true,\"blockTag\":\"h2\"}";
            FormatState state = WebViewEditor.ParseFormatStateJson(json);

            Assert.Multiple(() =>
            {
                Assert.That(state.Bold, Is.True);
                Assert.That(state.Italic, Is.False);
                Assert.That(state.Underline, Is.True);
                Assert.That(state.BlockTag, Is.EqualTo("h2"));
            });
        }

        [Test]
        public void ParseState_ReadsFontFamilySizeAndColors()
        {
            string json = "{\"blockTag\":\"p\",\"fontName\":\"Georgia\",\"fontSize\":\"4\"," +
                          "\"foreColor\":\"rgb(255, 0, 0)\",\"backColor\":\"rgb(255, 255, 0)\"}";
            FormatState state = WebViewEditor.ParseFormatStateJson(json);

            Assert.Multiple(() =>
            {
                Assert.That(state.FontFamily, Is.EqualTo("Georgia"));
                Assert.That(state.FontSize, Is.EqualTo("4"));
                Assert.That(state.ForeColor, Is.EqualTo("#FF0000"));
                Assert.That(state.HighlightColor, Is.EqualTo("#FFFF00"));
            });
        }

        [Test]
        public void ParseState_EmptyJson_YieldsDefaults()
        {
            FormatState state = WebViewEditor.ParseFormatStateJson(null);
            Assert.Multiple(() =>
            {
                Assert.That(state.Bold, Is.False);
                Assert.That(state.BlockTag, Is.EqualTo("p"));
                Assert.That(state.FontFamily, Is.Null);
                Assert.That(state.ForeColor, Is.Null);
            });
        }

        [TestCase("Georgia", "Georgia")]
        [TestCase("'Times New Roman', serif", "Times New Roman")]
        [TestCase("\"Helvetica Neue\", Arial", "Helvetica Neue")]
        [TestCase("  Verdana  ", "Verdana")]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void NormalizeFontName_UnquotesAndTakesFirstFamily(string input, string expected)
        {
            Assert.That(WebViewEditor.NormalizeFontName(input), Is.EqualTo(expected));
        }

        [TestCase("rgb(255, 0, 0)", "#FF0000")]
        [TestCase("rgb(0,128,255)", "#0080FF")]
        [TestCase("#abc", "#AABBCC")]
        [TestCase("#00ff00", "#00FF00")]
        [TestCase("", null)]
        [TestCase(null, null)]
        [TestCase("rgb(300, 0, 0)", null)]       // out of byte range
        [TestCase("not a color", null)]
        public void NormalizeReportedColor_HandlesRgbAndHex(string input, string expected)
        {
            Assert.That(WebViewEditor.NormalizeReportedColor(input), Is.EqualTo(expected));
        }
    }
}
