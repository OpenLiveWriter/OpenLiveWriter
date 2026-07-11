// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Avalonia.Controls;
using Avalonia.Threading;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated.Infrastructure
{
    /// <summary>
    /// DRY helper for the WebView-backed editor tests. Constructs a real
    /// <see cref="WebViewEditor"/> hosted in a window, waits for the editor to
    /// reach <c>Ready</c>, and exposes SetContent / SelectAll / Exec / GetContent(Dom)
    /// helpers — mirroring the manual bench in OpenLiveWriter.EditorTests/Program.cs.
    ///
    /// A live WKWebView backend is required, which is generally unavailable in a
    /// headless <c>dotnet test</c> run, so tests using this harness are marked
    /// [Explicit] + [Category("WebView")] and are intended to run on a real macOS
    /// desktop session (see docs/MAC-PARITY-STATUS.md).
    /// </summary>
    public sealed class EditorTestHarness : IAsyncDisposable
    {
        private Window _window;
        private WebViewEditor _editor;

        public WebViewEditor Editor => _editor;

        /// <summary>
        /// Creates the harness, shows a host window, and waits until the editor's
        /// WebView signals Ready (or throws on timeout).
        /// </summary>
        public static async Task<EditorTestHarness> CreateAsync(int readyTimeoutMs = 8000)
        {
            var harness = new EditorTestHarness();
            await harness.InitializeAsync(readyTimeoutMs);
            return harness;
        }

        private async Task InitializeAsync(int readyTimeoutMs)
        {
            try
            {
                _editor = new WebViewEditor();
                _window = new Window { Width = 900, Height = 600, Content = _editor };
                _window.Show();
            }
            catch (System.InvalidOperationException ex) when (ex.Message.Contains("IWindowingPlatform"))
            {
                // No desktop windowing backend (headless dotnet test). WebView-category
                // tests require a real macOS desktop session; report as skipped, not failed.
                NUnit.Framework.Assert.Ignore(
                    "No windowing backend available — WebView tests require a real macOS " +
                    "desktop session. See docs/MAC-PARITY-STATUS.md (Test coverage).");
            }

            var start = DateTime.UtcNow;
            while (!_editor.IsReady)
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > readyTimeoutMs)
                    throw new TimeoutException(
                        "WebViewEditor never reached Ready — a live WKWebView backend is " +
                        "required. Run WebView-category tests on a real macOS desktop session.");
                await Task.Delay(50);
                Dispatcher.UIThread.RunJobs();
            }
        }

        public async Task SetContentAsync(string html)
        {
            await _editor.SetContentAsync(html);
            await Task.Delay(100);
        }

        public async Task SelectAllAsync()
        {
            _editor.WebView.Focus();
            await Task.Delay(50);
            await _editor.WebView.InvokeScript("document.execCommand('selectAll')");
            await _editor.WebView.InvokeScript("OLWBridge.saveSelection()");
            await Task.Delay(50);
        }

        public Task ExecAsync(string command, string value = null) =>
            _editor.ExecCommandAsync(command, value);

        public Task<string> GetContentAsync() => _editor.GetContentAsync();

        public async Task<IDocument> GetContentDomAsync()
        {
            var html = await _editor.GetContentAsync();
            return Dom.Parse(html ?? string.Empty);
        }

        public Task<string> GetStateAsync() =>
            _editor.WebView.InvokeScript("OLWBridge.getState()");

        public async ValueTask DisposeAsync()
        {
            try
            {
                _window?.Close();
            }
            catch { /* best effort during teardown */ }
            await Task.CompletedTask;
        }
    }

    /// <summary>Shared NUnit category names.</summary>
    public static class WebViewCategories
    {
        /// <summary>Tests that need a live WKWebView backend.</summary>
        public const string WebView = "WebView";

        /// <summary>TDD targets blocked on the BlogClient/PostEditor port.</summary>
        public const string PublishTdd = "PublishTdd";

        /// <summary>Tests that publish to a real, live blog endpoint (opt-in only).</summary>
        public const string LiveBlog = "LiveBlog";
    }
}
