// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Print / Print Preview behavior for the shell. The print document is composed
    /// by <see cref="PrintRenderer"/> (article wrapper + print stylesheet); the
    /// fulfillment path is chosen by <see cref="PrintCoordinator"/>: the native
    /// WKWebView print panel when a WebView backend is live, otherwise a temp PDF
    /// opened in the default viewer, otherwise print-styled HTML opened in the
    /// browser (the user prints from there with Cmd+P).
    /// </summary>
    public partial class MainWindow
    {
        private PrintCoordinator _printCoordinator;

        private PrintCoordinator GetPrintCoordinator()
        {
            if (_printCoordinator == null)
            {
                _printCoordinator = new PrintCoordinator
                {
                    ShowNativePrintUIAsync = ShowNativePrintUIAsync,
                    RenderPdfAsync = RenderPrintPdfAsync
                };
            }
            return _printCoordinator;
        }

        private async Task PrintCurrentAsync()
        {
            var (body, title) = await GetPrintSourceAsync();
            PrintOutcome outcome = await GetPrintCoordinator().PrintAsync(body, title);
            UpdateStatus(outcome switch
            {
                PrintOutcome.NativePrintDialog => "Print dialog opened.",
                PrintOutcome.OpenedPdf => "Print PDF opened in the default viewer.",
                _ => "Print document opened in the browser — print with Cmd+P."
            });
        }

        private async Task PrintPreviewCurrentAsync()
        {
            var (body, title) = await GetPrintSourceAsync();
            PrintOutcome outcome = await GetPrintCoordinator().PrintPreviewAsync(body, title);
            UpdateStatus(outcome == PrintOutcome.OpenedPdf
                ? "Print preview opened in the default PDF viewer."
                : "Print preview opened in the browser — print with Cmd+P.");
        }

        // The print source mirrors the publish source: the live editor body plus the
        // title field (falling back to the draft's stored title).
        private async Task<(string Body, string Title)> GetPrintSourceAsync()
        {
            WebViewEditor editor = GetEditor();
            string body = editor != null ? await editor.GetContentAsync() : null;
            string title = _titleEditor?.Text ?? _draftSession?.Current.Title;
            return (body ?? string.Empty, title);
        }

        // Seam: renders the print document into the preview WebView and shows the
        // native print panel. False when no WebView backend is available.
        private async Task<bool> ShowNativePrintUIAsync(string document)
        {
            NativeWebView webView = await LoadPrintWebViewAsync(document);
            if (webView == null)
                return false;
            try
            {
                webView.ShowPrintUI();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Print] Native print UI failed: {ex.Message}");
                return false;
            }
        }

        // Seam: renders the print document into the preview WebView and captures it
        // as PDF bytes (WKWebView print-to-PDF). Null when unavailable.
        private async Task<byte[]> RenderPrintPdfAsync(string document)
        {
            NativeWebView webView = await LoadPrintWebViewAsync(document);
            if (webView == null)
                return null;
            try
            {
                using Stream pdf = await webView.PrintToPdfStreamAsync();
                if (pdf == null)
                    return null;
                using var memory = new MemoryStream();
                await pdf.CopyToAsync(memory);
                return memory.Length > 0 ? memory.ToArray() : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Print] PDF render failed: {ex.Message}");
                return null;
            }
        }

        private async Task<NativeWebView> LoadPrintWebViewAsync(string document)
        {
            var editorPanel = this.FindControl<EditorPanel>("EditorPanel");
            if (editorPanel == null)
                return null;
            try
            {
                return await editorPanel.LoadPreviewDocumentAsync(document);
            }
            catch
            {
                return null;
            }
        }
    }
}
