// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text.Json;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group R — editor-bridge robustness (milestone4/webview-wysiwyg). Covers the
    /// pure C# side of the bridge hardening: JS string-literal encoding
    /// (<see cref="WebViewEditor.EscapeJs"/>), pixel font-size normalization, and
    /// find-statistics parsing/readout. JS-side behavior is covered by [Explicit]
    /// WebView harness tests (see <see cref="GroupA_EditorCommandTests"/>).
    /// </summary>
    [TestFixture]
    [Category("GroupR")]
    public class GroupR_EditorBridgeTests
    {
        // --- EscapeJs: JSON-encoded JS string literals ---

        [Test]
        public void EscapeJs_RoundTripsNastyInputs()
        {
            string[] nasty =
            {
                "plain",
                "with 'single' and \"double\" quotes",
                "back\\slash",
                "crlf\r\nand lone\rand\nlf",
                "tabs\tand\x0001control\x001Fchars",
                "line\u2028separator\u2029here",
                "emoji 😀 and accents café",
                "</script><script>alert(1)</script>",
                "url?a=1&b=2",
                "100% sure",
            };

            foreach (string input in nasty)
            {
                string literal = WebViewEditor.EscapeJs(input);
                // A JSON string literal is a valid JS string literal; decoding it
                // must yield the original string exactly.
                Assert.That(JsonSerializer.Deserialize<string>(literal), Is.EqualTo(input),
                    $"round-trip failed for: {input}");
            }
        }

        [Test]
        public void EscapeJs_EscapesScriptBreakoutAndLineSeparators()
        {
            string literal = WebViewEditor.EscapeJs("</script>\r\n\u2028\u2029");

            Assert.Multiple(() =>
            {
                Assert.That(literal, Does.Not.Contain("</script>"),
                    "a literal </script> could terminate the host document's script element");
                Assert.That(literal, Does.Not.Contain("\u2028"),
                    "U+2028 is a line terminator in JS and would break the literal");
                Assert.That(literal, Does.Not.Contain("\u2029"),
                    "U+2029 is a line terminator in JS and would break the literal");
                Assert.That(literal, Does.Not.Contain("\r"));
                Assert.That(literal, Does.Not.Contain("\n"));
            });
        }

        [Test]
        public void EscapeJs_NullAndEmpty_YieldEmptyLiteral()
        {
            Assert.That(WebViewEditor.EscapeJs(null), Is.EqualTo("\"\""));
            Assert.That(WebViewEditor.EscapeJs(string.Empty), Is.EqualTo("\"\""));
        }

        [Test]
        public void EscapeJs_ProducesSelfDelimitedLiteral()
        {
            string literal = WebViewEditor.EscapeJs("abc");
            Assert.That(literal, Is.EqualTo("\"abc\""),
                "the literal includes its own quotes — call sites must not add their own");
        }

        // --- NormalizeFontSizePx ---

        [TestCase("12", "12")]
        [TestCase(" 14 ", "14")]
        [TestCase("12px", "12")]
        [TestCase("14pt", "14")]
        [TestCase("12.6", "13")]
        [TestCase("2", "6")]      // clamped to minimum
        [TestCase("500", "144")]  // clamped to maximum
        [TestCase("", null)]
        [TestCase(null, null)]
        [TestCase("abc", null)]
        [TestCase("0", null)]
        [TestCase("-12", null)]
        public void NormalizeFontSizePx_ParsesRoundsAndClamps(string input, string expected)
        {
            Assert.That(WebViewEditor.NormalizeFontSizePx(input), Is.EqualTo(expected));
        }

        // --- ParseFindStats ---

        [Test]
        public void ParseFindStats_ReadsCurrentAndTotal()
        {
            FindStats stats = WebViewEditor.ParseFindStats("2,5");
            Assert.Multiple(() =>
            {
                Assert.That(stats.Current, Is.EqualTo(2));
                Assert.That(stats.Total, Is.EqualTo(5));
            });
        }

        [TestCase(null, 0, 0)]
        [TestCase("", 0, 0)]
        [TestCase("garbage", 0, 0)]
        [TestCase("1,2,3", 0, 0)]
        [TestCase("1", 0, 0)]
        [TestCase("-2,-5", 0, 0)]
        public void ParseFindStats_Malformed_YieldsZeros(string input, int current, int total)
        {
            FindStats stats = WebViewEditor.ParseFindStats(input);
            Assert.Multiple(() =>
            {
                Assert.That(stats.Current, Is.EqualTo(current));
                Assert.That(stats.Total, Is.EqualTo(total));
            });
        }

        // --- Find-bar readout formatting ---

        [TestCase(2, 5, "2 of 5")]
        [TestCase(1, 1, "1 of 1")]
        [TestCase(0, 3, "3 matches")]
        [TestCase(0, 0, "No matches")]
        public void FormatMatchCount_RendersReadout(int current, int total, string expected)
        {
            Assert.That(EditorPanel.FormatMatchCount(current, total), Is.EqualTo(expected));
        }
    }
}
