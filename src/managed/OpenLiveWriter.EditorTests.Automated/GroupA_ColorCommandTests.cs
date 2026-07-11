// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group A — font color / highlight (P0-4). The live application of
    /// <c>foreColor</c>/<c>hiliteColor</c> runs inside the WKWebView, but the
    /// command → execCommand mapping and the hex-color serialization are pure and
    /// verified here headlessly. This pins the ribbon FontColorPicker /
    /// FontBackgroundColor wiring to the editor bridge.
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    public class GroupA_ColorCommandTests
    {
        [TestCase(CommandId.FontColorPicker, "foreColor")]
        [TestCase(CommandId.FontColor, "foreColor")]
        [TestCase(CommandId.FontBackgroundColor, "hiliteColor")]
        public void ColorCommandFor_MapsPickerToExecCommand(CommandId id, string expected)
        {
            Assert.That(WebViewEditor.ColorCommandFor(id), Is.EqualTo(expected));
        }

        [TestCase(CommandId.Bold)]
        [TestCase(CommandId.InsertLink)]
        [TestCase(CommandId.None)]
        public void ColorCommandFor_ReturnsNullForNonColorCommands(CommandId id)
        {
            Assert.That(WebViewEditor.ColorCommandFor(id), Is.Null);
        }

        [TestCase("#ff0000", "#FF0000")]
        [TestCase("ff0000", "#FF0000")]
        [TestCase("#FFF", "#FFFFFF")]
        [TestCase("abc", "#AABBCC")]
        [TestCase("  #00ff99  ", "#00FF99")]
        [TestCase("#123456", "#123456")]
        public void NormalizeColor_ProducesCanonicalHex(string input, string expected)
        {
            Assert.That(WebViewEditor.NormalizeColor(input), Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("#12")]
        [TestCase("#12345")]
        [TestCase("#1234567")]
        [TestCase("#gggggg")]
        [TestCase("red")]
        public void NormalizeColor_RejectsInvalidInput(string input)
        {
            Assert.That(WebViewEditor.NormalizeColor(input), Is.Null);
        }
    }
}
