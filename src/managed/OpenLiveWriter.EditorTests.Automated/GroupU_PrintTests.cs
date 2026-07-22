// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group U — P1-10: Print / Print Preview. The document composition
    /// (<see cref="PrintRenderer"/>) is asserted directly; the fulfillment-path
    /// selection (native print panel → temp PDF handoff → browser HTML handoff) is
    /// asserted against <see cref="PrintCoordinator"/> with injected seams, so no
    /// live WebView or browser is needed.
    /// </summary>
    [TestFixture]
    [Category("GroupU")]
    public class GroupU_PrintTests
    {
        private string _tempDir;
        private List<string> _opened;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "OLWPrintTests", Guid.NewGuid().ToString("N"));
            _opened = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }
            catch { /* best effort */ }
        }

        private PrintCoordinator NewCoordinator() =>
            new PrintCoordinator
            {
                TempDirectory = _tempDir,
                OpenFile = path => _opened.Add(path)
            };

        private string LastOpened =>
            _opened.Count > 0 ? _opened[_opened.Count - 1] : null;

        // ---- Composition ----

        [Test]
        public void BuildPrintDocument_WrapsBodyInArticleWithTitleAndPrintStyle()
        {
            string doc = PrintRenderer.BuildPrintDocument("<p>Hello print</p>", "My Post");

            Assert.That(doc, Does.Contain("<h1 class=\"olw-preview-title\">My Post</h1>"));
            Assert.That(doc, Does.Contain("<p>Hello print</p>"));
            Assert.That(doc, Does.Contain("@media print"));
            Assert.That(doc, Does.Contain("@page"));
        }

        [Test]
        public void BuildPrintDocument_StripsMoreMarker_AndEscapesTitle()
        {
            string doc = PrintRenderer.BuildPrintDocument(
                "<p>Main</p><!--more--><p>Extended</p>", "A <b> title");

            Assert.That(doc, Does.Not.Contain("<!--more-->"));
            Assert.That(doc, Does.Contain("<p>Main</p><p>Extended</p>"));
            Assert.That(doc, Does.Contain("A &lt;b&gt; title"));
        }

        [Test]
        public void BuildPrintDocument_NullBody_ProducesEmptyArticle()
        {
            string doc = PrintRenderer.BuildPrintDocument(null, null);
            Assert.That(doc, Does.Contain("<article>"));
            Assert.That(doc, Does.Not.Contain("olw-preview-title"));
        }

        // ---- Print path selection ----

        [Test]
        public async Task Print_PrefersNativePrintDialog_WhenWebViewAvailable()
        {
            var coordinator = NewCoordinator();
            bool nativeShown = false;
            coordinator.ShowNativePrintUIAsync = doc =>
            {
                nativeShown = true;
                Assert.That(doc, Does.Contain("@media print"), "native print must get the print document");
                return Task.FromResult(true);
            };
            coordinator.RenderPdfAsync = _ => throw new InvalidOperationException("PDF must not be attempted");

            PrintOutcome outcome = await coordinator.PrintAsync("<p>Body</p>", "Title");

            Assert.That(outcome, Is.EqualTo(PrintOutcome.NativePrintDialog));
            Assert.That(nativeShown, Is.True);
            Assert.That(_opened, Is.Empty, "no file handoff when the native dialog is shown");
        }

        [Test]
        public async Task Print_FallsBackToPdf_WhenNativePrintUnavailable()
        {
            var coordinator = NewCoordinator();
            coordinator.ShowNativePrintUIAsync = _ => Task.FromResult(false);
            coordinator.RenderPdfAsync = _ => Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // %PDF

            PrintOutcome outcome = await coordinator.PrintAsync("<p>Body</p>", "Title");

            Assert.That(outcome, Is.EqualTo(PrintOutcome.OpenedPdf));
            Assert.That(LastOpened, Is.Not.Null.And.EndsWith(".pdf"));
            Assert.That(File.Exists(LastOpened), Is.True);
            Assert.That(File.ReadAllBytes(LastOpened), Has.Length.EqualTo(4));
        }

        [Test]
        public async Task Print_FallsBackToPdf_WhenNativePrintThrows()
        {
            var coordinator = NewCoordinator();
            coordinator.ShowNativePrintUIAsync = _ => throw new InvalidOperationException("no backend");
            coordinator.RenderPdfAsync = _ => Task.FromResult(new byte[] { 1, 2, 3 });

            PrintOutcome outcome = await coordinator.PrintAsync("<p>Body</p>", "Title");

            Assert.That(outcome, Is.EqualTo(PrintOutcome.OpenedPdf));
            Assert.That(LastOpened, Does.EndWith(".pdf"));
        }

        [Test]
        public async Task Print_FallsBackToBrowserHtml_WhenNoWebViewAtAll()
        {
            var coordinator = NewCoordinator();
            // Both seams null: no live WebView backend (headless).
            PrintOutcome outcome = await coordinator.PrintAsync("<p>Body</p>", "Title");

            Assert.That(outcome, Is.EqualTo(PrintOutcome.OpenedHtml));
            Assert.That(LastOpened, Is.Not.Null.And.EndsWith(".html"));
            string html = File.ReadAllText(LastOpened);
            Assert.That(html, Does.Contain("<p>Body</p>"));
            Assert.That(html, Does.Contain("@media print"));
        }

        // ---- Print Preview path selection ----

        [Test]
        public async Task PrintPreview_OpensPdf_WhenRendererAvailable()
        {
            var coordinator = NewCoordinator();
            coordinator.ShowNativePrintUIAsync = _ => throw new InvalidOperationException("print UI must not be attempted");
            coordinator.RenderPdfAsync = _ => Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 });

            PrintOutcome outcome = await coordinator.PrintPreviewAsync("<p>Body</p>", "Title");

            Assert.That(outcome, Is.EqualTo(PrintOutcome.OpenedPdf));
            Assert.That(LastOpened, Does.EndWith(".pdf"));
        }

        [Test]
        public async Task PrintPreview_FallsBackToBrowserHtml_WhenPdfFails()
        {
            var coordinator = NewCoordinator();
            coordinator.RenderPdfAsync = _ => Task.FromResult<byte[]>(null);

            PrintOutcome outcome = await coordinator.PrintPreviewAsync("<p>Body</p>", "Title");

            Assert.That(outcome, Is.EqualTo(PrintOutcome.OpenedHtml));
            Assert.That(LastOpened, Does.EndWith(".html"));
        }
    }
}
