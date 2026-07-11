// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group A18 — getState sync. After applying bold, OLWBridge.getState() must
    /// report bold:true; after toggling off it must report bold:false. This drives
    /// the ribbon/toolbar toggle-button sync and needs the live WebView.
    /// </summary>
    [TestFixture]
    [Category("GroupA")]
    [Category(WebViewCategories.WebView)]
    [Explicit("Requires a live WKWebView backend — run on a real macOS session")]
    public class GroupA18_GetStateTests
    {
        [Test]
        public async Task GetState_ReportsBoldTrueAfterBold_FalseAfterToggleOff()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>State test</p>");

            await harness.SelectAllAsync();
            await harness.ExecAsync("bold");
            await Task.Delay(150);
            var on = await harness.GetStateAsync();
            Assert.That(on, Does.Contain("\"bold\":true"));

            await harness.SelectAllAsync();
            await harness.ExecAsync("bold");
            await Task.Delay(150);
            var off = await harness.GetStateAsync();
            Assert.That(off, Does.Contain("\"bold\":false"));
        }

        [Test]
        public async Task GetState_ReportsBlockTagForHeading()
        {
            await using var harness = await EditorTestHarness.CreateAsync();
            await harness.SetContentAsync("<p>Heading state</p>");
            await harness.SelectAllAsync();
            await harness.Editor.SetBlockFormatAsync("h2");
            await Task.Delay(150);
            var state = await harness.GetStateAsync();
            Assert.That(state, Does.Contain("\"blockTag\":\"h2\""));
        }
    }
}
