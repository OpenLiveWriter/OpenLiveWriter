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
    /// Group F — Insert Table. The table HTML is produced by the pure
    /// <see cref="TableBuilder"/>, so dimensions/structure/width are asserted
    /// headlessly on the parsed DOM. The dialog capture is verified with a headless
    /// Avalonia UI test. The live insertion + table-editing operations run inside
    /// the WebView and are exercised by the manual bench.
    /// </summary>
    [TestFixture]
    [Category("GroupF")]
    public class GroupF_TableTests
    {
        [Test]
        public void Table_BuildsCorrectDimensions_WithHeaderRow()
        {
            string html = TableBuilder.BuildTableHtml(rows: 3, columns: 4, headerRow: true, width: null);
            var doc = Dom.Parse(html);

            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("table"), Is.Not.Null);
                // Header row: 1 thead row of <th>, count = columns.
                Assert.That(doc.QuerySelectorAll("thead > tr"), Has.Length.EqualTo(1));
                Assert.That(doc.QuerySelectorAll("thead th"), Has.Length.EqualTo(4));
                // Body rows: total rows - header row = 2, each with 4 <td>.
                Assert.That(doc.QuerySelectorAll("tbody > tr"), Has.Length.EqualTo(2));
                Assert.That(doc.QuerySelectorAll("tbody td"), Has.Length.EqualTo(8));
            });
        }

        [Test]
        public void Table_NoHeaderRow_AllRowsInBody()
        {
            string html = TableBuilder.BuildTableHtml(rows: 2, columns: 3, headerRow: false, width: null);
            var doc = Dom.Parse(html);

            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("thead"), Is.Null, "no header row requested");
                Assert.That(doc.QuerySelectorAll("tbody > tr"), Has.Length.EqualTo(2));
                Assert.That(doc.QuerySelectorAll("tbody td"), Has.Length.EqualTo(6));
                Assert.That(doc.QuerySelectorAll("th"), Has.Length.EqualTo(0));
            });
        }

        [Test]
        public void Table_IsWellFormed()
        {
            string html = TableBuilder.BuildTableHtml(4, 4, true, "100%");
            Assert.That(HtmlWellFormednessGate.IsWellFormed(html), Is.True, html);
        }

        [TestCase("100%", "width:100%")]
        [TestCase("500", "width:500px")]
        [TestCase("500px", "width:500px")]
        [TestCase("abc%", null)]     // non-numeric percentage is rejected
        public void Table_AppliesWidthStyle(string input, string expectedFragment)
        {
            string html = TableBuilder.BuildTableHtml(2, 2, true, input);
            if (expectedFragment == null)
                Assert.That(html, Does.Not.Contain("width:"));
            else
                Assert.That(html, Does.Contain(expectedFragment));
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("   ", null)]
        [TestCase("100%", "100%")]
        [TestCase("500", "500px")]
        [TestCase("0", null)]        // non-positive
        [TestCase("-5", null)]
        [TestCase("abc", null)]
        public void Table_NormalizeWidth(string input, string expected)
        {
            Assert.That(TableBuilder.NormalizeWidth(input), Is.EqualTo(expected));
        }

        [Test]
        public void Table_ClampsDimensionsToAtLeastOne()
        {
            string html = TableBuilder.BuildTableHtml(0, 0, false, null);
            var doc = Dom.Parse(html);
            Assert.That(doc.QuerySelectorAll("tbody > tr"), Has.Length.EqualTo(1));
            Assert.That(doc.QuerySelectorAll("tbody td"), Has.Length.EqualTo(1));
        }

        // --- Dialog capture (headless Avalonia UI) ---

        [AvaloniaTest]
        public void TableDialog_DefaultsToTwoByTwoWithHeader()
        {
            var dialog = new TableDialog();
            var updowns = dialog.GetLogicalDescendants().OfType<NumericUpDown>().ToList();
            Assert.That(updowns, Has.Count.EqualTo(2));
            Assert.That(updowns[0].Value, Is.EqualTo(2)); // rows
            Assert.That(updowns[1].Value, Is.EqualTo(2)); // columns

            var header = dialog.GetLogicalDescendants().OfType<CheckBox>().FirstOrDefault();
            Assert.That(header?.IsChecked, Is.True);
        }
    }
}
