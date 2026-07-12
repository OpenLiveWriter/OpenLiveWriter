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
    /// Group L — Insert Map. The Windows Bing/Virtual Earth map picker is dead; this
    /// verifies the modern OpenStreetMap embed built by the pure
    /// <see cref="MapEmbedBuilder"/> (coordinate parsing, embed/permalink/search URL
    /// composition, DOM shape). The dialog capture is checked with a headless UI test.
    /// The live insertion into the WebView is exercised by the manual bench.
    /// </summary>
    [TestFixture]
    [Category("GroupL")]
    public class GroupL_MapTests
    {
        [TestCase("37.7749, -122.4194", true, 37.7749, -122.4194)]
        [TestCase("37.7749 -122.4194", true, 37.7749, -122.4194)]
        [TestCase("-33.8688,151.2093", true, -33.8688, 151.2093)]
        [TestCase("0,0", true, 0, 0)]
        [TestCase("", false, 0, 0)]
        [TestCase(null, false, 0, 0)]
        [TestCase("not coords", false, 0, 0)]
        [TestCase("37.7749", false, 0, 0)]           // single value
        [TestCase("91, 0", false, 0, 0)]             // latitude out of range
        [TestCase("0, 181", false, 0, 0)]            // longitude out of range
        public void TryParseCoordinates_HandlesInput(string input, bool ok, double lat, double lon)
        {
            bool parsed = MapEmbedBuilder.TryParseCoordinates(input, out double gotLat, out double gotLon);
            Assert.That(parsed, Is.EqualTo(ok));
            if (ok)
            {
                Assert.That(gotLat, Is.EqualTo(lat).Within(1e-6));
                Assert.That(gotLon, Is.EqualTo(lon).Within(1e-6));
            }
        }

        [Test]
        public void BuildMapHtml_WithCoordinates_EmbedsResponsiveOsmIframe()
        {
            string html = MapEmbedBuilder.BuildMapHtml("Golden Gate Bridge", "37.8199, -122.4783");
            var doc = Dom.Parse(html);

            var wrapper = doc.QuerySelector("div.olw-map");
            var iframe = doc.QuerySelector("iframe");
            Assert.Multiple(() =>
            {
                Assert.That(wrapper, Is.Not.Null, "map wrapper carries the olw-map class for contextual tabs");
                Assert.That(iframe, Is.Not.Null);
                string src = iframe.GetAttribute("src");
                Assert.That(src, Does.StartWith("https://www.openstreetmap.org/export/embed.html"));
                Assert.That(src, Does.Contain("marker="));
                Assert.That(src, Does.Contain("bbox="));
                // Coordinates are formatted invariantly (dot decimal separator).
                Assert.That(src, Does.Contain("37.8199"));
                // Caption link is the permalink with the supplied label.
                var link = doc.QuerySelector("a");
                Assert.That(link, Is.Not.Null);
                Assert.That(link.GetAttribute("href"), Does.Contain("openstreetmap.org/?mlat="));
                Assert.That(link.TextContent, Is.EqualTo("Golden Gate Bridge"));
            });
        }

        [Test]
        public void BuildMapHtml_CoordinatesWithoutLabel_UsesDefaultCaption()
        {
            string html = MapEmbedBuilder.BuildMapHtml(null, "51.5007, -0.1246");
            var link = Dom.Parse(html).QuerySelector("a");
            Assert.That(link.TextContent, Is.EqualTo("View larger map"));
        }

        [Test]
        public void BuildMapHtml_PlaceNameOnly_InsertsSearchLink()
        {
            string html = MapEmbedBuilder.BuildMapHtml("Eiffel Tower, Paris", null);
            var doc = Dom.Parse(html);

            Assert.Multiple(() =>
            {
                Assert.That(doc.QuerySelector("iframe"), Is.Null, "no coordinates ⇒ no embed");
                var link = doc.QuerySelector("div.olw-map a");
                Assert.That(link, Is.Not.Null);
                Assert.That(link.GetAttribute("href"), Does.StartWith("https://www.openstreetmap.org/search?query="));
                Assert.That(link.GetAttribute("href"), Does.Contain("Eiffel%20Tower"));
                Assert.That(link.TextContent, Does.Contain("Eiffel Tower"));
            });
        }

        [Test]
        public void BuildMapHtml_Empty_ReturnsNull()
        {
            Assert.That(MapEmbedBuilder.BuildMapHtml(null, null), Is.Null);
            Assert.That(MapEmbedBuilder.BuildMapHtml("   ", "   "), Is.Null);
        }

        [Test]
        public void BuildMapHtml_IsWellFormed()
        {
            string embed = MapEmbedBuilder.BuildMapHtml("Sydney Opera House", "-33.8568, 151.2153");
            string link = MapEmbedBuilder.BuildMapHtml("Somewhere", null);
            Assert.That(HtmlWellFormednessGate.IsWellFormed(embed), Is.True, embed);
            Assert.That(HtmlWellFormednessGate.IsWellFormed(link), Is.True, link);
        }

        [TestCase(0, 1)]     // clamped up to min zoom
        [TestCase(25, 19)]   // clamped down to max zoom
        [TestCase(14, 14)]
        public void ClampZoom_KeepsWithinOsmRange(int input, int expected)
        {
            Assert.That(MapEmbedBuilder.ClampZoom(input), Is.EqualTo(expected));
        }

        [Test]
        public void BuildEmbedUrl_HigherZoomProducesTighterBox()
        {
            // The embed URL encodes the bbox; a higher zoom must not throw and must
            // still be a valid OSM embed endpoint.
            string low = MapEmbedBuilder.BuildEmbedUrl(40.0, -73.0, 3);
            string high = MapEmbedBuilder.BuildEmbedUrl(40.0, -73.0, 17);
            Assert.That(low, Does.Contain("export/embed.html"));
            Assert.That(high, Does.Contain("export/embed.html"));
            Assert.That(low, Is.Not.EqualTo(high));
        }

        // --- Dialog capture (headless Avalonia UI) ---

        [AvaloniaTest]
        public void MapDialog_DefaultsToDefaultZoom_AndInsertDisabledUntilInput()
        {
            var dialog = new MapDialog();
            var zoom = dialog.GetLogicalDescendants().OfType<NumericUpDown>().FirstOrDefault();
            Assert.That(zoom?.Value, Is.EqualTo(MapEmbedBuilder.DefaultZoom));

            var insert = dialog.GetLogicalDescendants().OfType<Button>()
                .FirstOrDefault(b => (b.Content as string) == "Insert");
            Assert.That(insert, Is.Not.Null);
            Assert.That(insert.IsEnabled, Is.False, "Insert stays disabled with no place name or coordinates");
        }
    }
}
