// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>How a print/print-preview request was ultimately fulfilled.</summary>
    public enum PrintOutcome
    {
        /// <summary>The native print panel was shown on a rendered WebView.</summary>
        NativePrintDialog,

        /// <summary>The document was rendered to a temp PDF and opened in the default viewer.</summary>
        OpenedPdf,

        /// <summary>The print-styled HTML was written to a temp file and opened in the browser.</summary>
        OpenedHtml
    }

    /// <summary>
    /// Drives Print / Print Preview for the shell. The document composition is
    /// <see cref="PrintRenderer"/>; this class only decides the fulfillment path:
    ///
    ///  - Print prefers the native print panel (<see cref="ShowNativePrintUIAsync"/>,
    ///    WKWebView print via the shell's preview WebView), then degrades to a temp
    ///    PDF opened in the default viewer, and finally to print-styled HTML opened
    ///    in the browser (the user prints from there with Cmd+P).
    ///  - Print Preview renders a temp PDF and opens it in the default PDF viewer
    ///    (Preview.app — macOS's de-facto print preview), degrading to the browser
    ///    HTML handoff when no WebView backend is available.
    ///
    /// Every environment-dependent step is an injectable seam so the path selection
    /// is headless-testable; the shell wires the seams to the live WebView.
    /// </summary>
    public sealed class PrintCoordinator
    {
        /// <summary>
        /// Seam: loads the print document into a WebView and shows the native print
        /// panel. Returns false when no WebView backend is available (headless).
        /// </summary>
        public Func<string, Task<bool>> ShowNativePrintUIAsync { get; set; }

        /// <summary>
        /// Seam: renders the print document to PDF bytes via a WebView
        /// (<c>PrintToPdfStreamAsync</c>). Returns null when unavailable.
        /// </summary>
        public Func<string, Task<byte[]>> RenderPdfAsync { get; set; }

        /// <summary>
        /// Seam: opens a temp file in the default handler (browser for HTML, PDF
        /// viewer for PDF). Defaults to <see cref="BrowserLauncher"/>.
        /// </summary>
        public Action<string> OpenFile { get; set; } = path => BrowserLauncher.Open(new Uri(path).AbsoluteUri);

        /// <summary>Directory the temp print artifacts are written to.</summary>
        public string TempDirectory { get; set; } =
            Path.Combine(Path.GetTempPath(), "OpenLiveWriter", "print");

        /// <summary>
        /// Fulfills a Print request: native print panel when available, else the PDF
        /// handoff, else the browser HTML handoff.
        /// </summary>
        public async Task<PrintOutcome> PrintAsync(string bodyHtml, string title = null)
        {
            string document = PrintRenderer.BuildPrintDocument(bodyHtml, title);

            if (ShowNativePrintUIAsync != null)
            {
                try
                {
                    if (await ShowNativePrintUIAsync(document).ConfigureAwait(false))
                        return PrintOutcome.NativePrintDialog;
                }
                catch
                {
                    // Native print is best-effort — fall through to the file handoffs.
                }
            }

            byte[] pdf = await TryRenderPdfAsync(document).ConfigureAwait(false);
            if (pdf != null)
            {
                OpenFile(WriteTempFile("print.pdf", pdf));
                return PrintOutcome.OpenedPdf;
            }

            OpenFile(WriteTempHtml(document));
            return PrintOutcome.OpenedHtml;
        }

        /// <summary>
        /// Fulfills a Print Preview request: a rendered PDF in the default viewer
        /// when possible, else the print-styled HTML in the browser.
        /// </summary>
        public async Task<PrintOutcome> PrintPreviewAsync(string bodyHtml, string title = null)
        {
            string document = PrintRenderer.BuildPrintDocument(bodyHtml, title);

            byte[] pdf = await TryRenderPdfAsync(document).ConfigureAwait(false);
            if (pdf != null)
            {
                OpenFile(WriteTempFile("print-preview.pdf", pdf));
                return PrintOutcome.OpenedPdf;
            }

            OpenFile(WriteTempHtml(document));
            return PrintOutcome.OpenedHtml;
        }

        private async Task<byte[]> TryRenderPdfAsync(string document)
        {
            if (RenderPdfAsync == null)
                return null;
            try
            {
                return await RenderPdfAsync(document).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        private string WriteTempHtml(string document)
        {
            Directory.CreateDirectory(TempDirectory);
            string path = Path.Combine(TempDirectory, "print-preview.html");
            File.WriteAllText(path, document);
            return path;
        }

        private string WriteTempFile(string fileName, byte[] bytes)
        {
            Directory.CreateDirectory(TempDirectory);
            string path = Path.Combine(TempDirectory, fileName);
            File.WriteAllBytes(path, bytes);
            return path;
        }
    }
}
