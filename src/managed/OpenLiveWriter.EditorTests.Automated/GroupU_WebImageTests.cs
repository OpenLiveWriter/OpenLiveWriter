// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group U — P1-11: Insert picture from the web. Covers the URL/width validation
    /// (stricter than LinkDialog: absolute http/https with a host), the
    /// <c>&lt;img&gt;</c> HTML build with escaping and optional width, and the
    /// dialog's Insert-button enable rule headlessly.
    /// </summary>
    [TestFixture]
    [Category("GroupU")]
    public class GroupU_WebImageTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("   ", false)]
        [TestCase("https://", false)]
        [TestCase("http://", false)]
        [TestCase("https://example.com/pic.png", true)]
        [TestCase("http://cdn.example.com/a/b.jpg?x=1", true)]
        [TestCase("ftp://example.com/pic.png", false)]   // images must be http(s)
        [TestCase("mailto:x@y.com", false)]
        [TestCase("/relative/pic.png", false)]           // relative URLs can't render remotely
        [TestCase("example.com/pic.png", false)]         // scheme required
        [TestCase("data:image/png;base64,AAAA", false)]  // not a remote URL
        public void IsValidHttpUrl_RequiresAbsoluteHttpOrHttps(string url, bool expected)
        {
            Assert.That(WebImageDialog.IsValidHttpUrl(url), Is.EqualTo(expected));
        }

        [TestCase(null, true, null)]       // blank = no width, allowed
        [TestCase("", true, null)]
        [TestCase("  ", true, null)]
        [TestCase("300", true, 300)]
        [TestCase(" 640 ", true, 640)]
        [TestCase("0", false, null)]
        [TestCase("-10", false, null)]
        [TestCase("abc", false, null)]
        [TestCase("12.5", false, null)]
        public void WidthValidation_OptionalPositivePixels(string width, bool valid, int? parsed)
        {
            Assert.That(WebImageDialog.IsValidWidth(width), Is.EqualTo(valid));
            Assert.That(WebImageDialog.ParseWidth(width), Is.EqualTo(parsed));
        }

        [Test]
        public void BuildImageHtml_RemoteUrlWithAltAndWidth()
        {
            string html = WebViewEditor.BuildImageHtml(
                "https://cdn.example.com/pic.png", "A \"quoted\" alt & <tag>", 300);
            var img = Dom.Parse(html).QuerySelector("img");

            Assert.That(img, Is.Not.Null);
            Assert.That(img.GetAttribute("src"), Is.EqualTo("https://cdn.example.com/pic.png"));
            Assert.That(img.GetAttribute("alt"), Is.EqualTo("A \"quoted\" alt & <tag>"));
            Assert.That(img.GetAttribute("width"), Is.EqualTo("300"));
        }

        [Test]
        public void BuildImageHtml_NoWidth_OmitsWidthAttribute()
        {
            string html = WebViewEditor.BuildImageHtml("https://x/y.png", "alt", widthPx: null);
            var img = Dom.Parse(html).QuerySelector("img");

            Assert.That(img.GetAttribute("width"), Is.Null);
        }

        [Test]
        public void BuildImageHtml_ExistingTwoArgForm_Unchanged()
        {
            // The file-insert path keeps its exact legacy markup (no width attribute).
            string html = WebViewEditor.BuildImageHtml("https://x/y.png", "alt");
            Assert.That(html, Is.EqualTo("<img src=\"https://x/y.png\" alt=\"alt\" />"));
        }

        [AvaloniaTest]
        public void WebImageDialog_InsertButton_DisabledUntilValidUrl()
        {
            var dialog = new WebImageDialog();
            var boxes = dialog.GetLogicalDescendants().OfType<TextBox>().ToList();
            TextBox urlBox = boxes[0];      // Address
            TextBox widthBox = boxes[2];    // Width
            Button insert = dialog.GetLogicalDescendants().OfType<Button>()
                .First(b => (b.Content as string) == "Insert");

            // Default text is "https://" — Insert must be disabled.
            Assert.That(insert.IsEnabled, Is.False);

            urlBox.Text = "https://example.com/pic.png";
            Assert.That(insert.IsEnabled, Is.True);

            // A non-numeric width blocks insertion; clearing it re-enables.
            widthBox.Text = "abc";
            Assert.That(insert.IsEnabled, Is.False);
            widthBox.Text = "300";
            Assert.That(insert.IsEnabled, Is.True);

            // Non-http(s) schemes are rejected.
            urlBox.Text = "ftp://example.com/pic.png";
            Assert.That(insert.IsEnabled, Is.False);
        }
    }
}
