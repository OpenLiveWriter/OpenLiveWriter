// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Theming;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group W — P1-3 theme-based preview. Covers the pure homepage style extraction
    /// (stylesheet links + inline styles, URL resolution), the per-account theme cache
    /// (memory + disk, force refresh, failure never poisons), the themed
    /// <see cref="PreviewRenderer"/> composition (styles present iff a theme is
    /// supplied), and the shell wiring of "Use Theme" / "Update Theme" / Close Preview.
    /// All network is behind a fake <see cref="IThemeHtmlFetcher"/>; nothing live.
    /// </summary>
    [TestFixture]
    [Category("GroupW")]
    public class GroupW_ThemingTests
    {
        private const string Homepage = "https://doug.example.com/blog/";

        private string _dir;

        [SetUp]
        public void SetUp()
        {
            WebViewEditor.UseLayoutPlaceholder = true;
            _dir = Path.Combine(Path.GetTempPath(), "OLWThemingTests", Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            WebViewEditor.UseLayoutPlaceholder = false;
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
            catch { /* best effort */ }
        }

        // ---- ThemeStyleExtractor (pure) ----

        [Test]
        public void Extract_StylesheetLinks_ResolvedAgainstHomepage()
        {
            string html =
                "<html><head>" +
                "<link rel=\"stylesheet\" href=\"https://cdn.example.com/absolute.css\">" +
                "<link rel=\"stylesheet\" href=\"/root-relative.css\">" +
                "<link rel=\"stylesheet\" href=\"relative.css\">" +
                "<link rel=\"stylesheet\" href=\"//protocol-relative.example.com/p.css\">" +
                "</head><body></body></html>";

            BlogThemeStyle theme = ThemeStyleExtractor.Extract(html, Homepage);

            Assert.That(theme.StylesheetUrls, Is.EqualTo(new[]
            {
                "https://cdn.example.com/absolute.css",
                "https://doug.example.com/root-relative.css",
                "https://doug.example.com/blog/relative.css",
                "https://protocol-relative.example.com/p.css",
            }));
        }

        [Test]
        public void Extract_RelTokenVariants_AndCaseInsensitivity()
        {
            string html =
                "<LINK REL='StyleSheet' HREF='one.css'>" +
                "<link href=\"two.css\" rel=\"stylesheet\">" + // attribute order flipped
                "<link rel=\"alternate stylesheet\" href=\"three.css\">" +
                "<link rel=stylesheet href=four.css>"; // unquoted attributes

            BlogThemeStyle theme = ThemeStyleExtractor.Extract(html, Homepage);

            Assert.That(theme.StylesheetUrls, Has.Count.EqualTo(4));
            Assert.That(theme.StylesheetUrls[0], Does.EndWith("one.css"));
            Assert.That(theme.StylesheetUrls[3], Does.EndWith("four.css"));
        }

        [Test]
        public void Extract_NonStylesheetLinks_AreIgnored()
        {
            string html =
                "<link rel=\"EditURI\" type=\"application/rsd+xml\" href=\"rsd.xml\">" +
                "<link rel=\"icon\" href=\"favicon.ico\">" +
                "<link rel=\"canonical\" href=\"https://doug.example.com/blog/\">" +
                "<link rel=\"stylesheet\" href=\"theme.css\">";

            BlogThemeStyle theme = ThemeStyleExtractor.Extract(html, Homepage);

            Assert.That(theme.StylesheetUrls, Is.EqualTo(new[]
            {
                "https://doug.example.com/blog/theme.css",
            }));
        }

        [Test]
        public void Extract_DuplicateHrefs_AreDeduplicated()
        {
            string html =
                "<link rel=\"stylesheet\" href=\"theme.css\">" +
                "<link rel=\"stylesheet\" href=\"theme.css\">" +
                "<link rel=\"stylesheet\" href=\"/blog/theme.css\">";

            BlogThemeStyle theme = ThemeStyleExtractor.Extract(html, Homepage);

            Assert.That(theme.StylesheetUrls, Has.Count.EqualTo(1));
        }

        [Test]
        public void Extract_InlineStyleBlocks_Captured_EmptySkipped()
        {
            string html =
                "<style>body { color: rebeccapurple; }</style>" +
                "<style type=\"text/css\">\n  h1 { font-family: Comic Sans; }\n</style>" +
                "<style>   </style>";

            BlogThemeStyle theme = ThemeStyleExtractor.Extract(html, Homepage);

            Assert.That(theme.InlineStyles, Has.Count.EqualTo(2));
            Assert.That(theme.InlineStyles[0], Does.Contain("rebeccapurple"));
            Assert.That(theme.InlineStyles[1], Does.Contain("Comic Sans"));
        }

        [Test]
        public void Extract_NoStylesheets_ReturnsEmpty()
        {
            BlogThemeStyle theme = ThemeStyleExtractor.Extract(
                "<html><head><title>plain</title></head><body><p>hi</p></body></html>", Homepage);

            Assert.That(theme.IsEmpty, Is.True);
            Assert.That(theme.SourceUrl, Is.EqualTo(Homepage));
        }

        [Test]
        public void Extract_NullOrEmptyHtml_ReturnsEmpty()
        {
            Assert.That(ThemeStyleExtractor.Extract(null, Homepage).IsEmpty, Is.True);
            Assert.That(ThemeStyleExtractor.Extract(string.Empty, Homepage).IsEmpty, Is.True);
        }

        // ---- ThemeStyleCache (fake fetcher, temp disk dir) ----

        [Test]
        public async Task Cache_FetchOnce_ThenServesFromMemory()
        {
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            var cache = new ThemeStyleCache(fetcher, _dir);

            BlogThemeStyle first = await cache.GetThemeAsync("acct-1", Homepage);
            BlogThemeStyle second = await cache.GetThemeAsync("acct-1", Homepage);

            Assert.That(fetcher.FetchCount, Is.EqualTo(1));
            Assert.That(first.IsEmpty, Is.False);
            Assert.That(second.StylesheetUrls, Is.EqualTo(first.StylesheetUrls));
            Assert.That(first.FetchedUtc, Is.GreaterThan(DateTime.MinValue), "fetch records a timestamp");
        }

        [Test]
        public async Task Cache_ForceRefresh_RefetchesAndUpdates()
        {
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml("v1.css"));
            var cache = new ThemeStyleCache(fetcher, _dir);

            BlogThemeStyle first = await cache.GetThemeAsync("acct-1", Homepage);

            fetcher.Body = FixtureHtml("v2.css");
            BlogThemeStyle refreshed = await cache.GetThemeAsync("acct-1", Homepage, forceRefresh: true);

            Assert.That(fetcher.FetchCount, Is.EqualTo(2));
            Assert.That(first.StylesheetUrls[0], Does.Contain("v1.css"));
            Assert.That(refreshed.StylesheetUrls[0], Does.Contain("v2.css"));

            // The refreshed entry is now what a normal lookup serves.
            BlogThemeStyle after = await cache.GetThemeAsync("acct-1", Homepage);
            Assert.That(after.StylesheetUrls[0], Does.Contain("v2.css"));
            Assert.That(fetcher.FetchCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Cache_HomepageChange_InvalidatesCachedEntry()
        {
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            var cache = new ThemeStyleCache(fetcher, _dir);

            await cache.GetThemeAsync("acct-1", Homepage);
            BlogThemeStyle moved = await cache.GetThemeAsync("acct-1", "https://new.example.com/");

            Assert.That(fetcher.FetchCount, Is.EqualTo(2), "a different homepage must refetch");
            Assert.That(moved.SourceUrl, Is.EqualTo("https://new.example.com/"));
        }

        [Test]
        public async Task Cache_FetchFailure_ReturnsNull_AndKeepsPreviousEntry()
        {
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            var cache = new ThemeStyleCache(fetcher, _dir);

            BlogThemeStyle good = await cache.GetThemeAsync("acct-1", Homepage);
            Assert.That(good, Is.Not.Null);

            fetcher.Body = null; // simulate network failure
            BlogThemeStyle failed = await cache.GetThemeAsync("acct-1", Homepage, forceRefresh: true);
            Assert.That(failed, Is.Null, "a failed refresh reports failure (neutral preview)");

            // The previous good entry survives the failed refresh.
            BlogThemeStyle after = await cache.GetThemeAsync("acct-1", Homepage);
            Assert.That(after, Is.Not.Null);
            Assert.That(fetcher.FetchCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Cache_ThrowingFetcher_ReturnsNull_NeverThrows()
        {
            var cache = new ThemeStyleCache(new ThrowingThemeHtmlFetcher(), _dir);

            BlogThemeStyle theme = await cache.GetThemeAsync("acct-1", Homepage);

            Assert.That(theme, Is.Null);
        }

        [Test]
        public async Task Cache_DiskRoundTrip_SecondInstanceServesWithoutFetch()
        {
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            var first = new ThemeStyleCache(fetcher, _dir);
            await first.GetThemeAsync("acct-1", Homepage);

            // A new cache over the same directory (an app restart) loads from disk.
            var second = new ThemeStyleCache(fetcher, _dir);
            BlogThemeStyle theme = await second.GetThemeAsync("acct-1", Homepage);

            Assert.That(fetcher.FetchCount, Is.EqualTo(1), "disk cache must avoid a refetch after restart");
            Assert.That(theme.IsEmpty, Is.False);
            Assert.That(theme.FetchedUtc, Is.GreaterThan(DateTime.MinValue));
        }

        [Test]
        public async Task Cache_CorruptDiskFile_TreatedAsMiss()
        {
            Directory.CreateDirectory(_dir);
            File.WriteAllText(Path.Combine(_dir, "acct-1.oltheme.json"), "{ not json !!");

            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            var cache = new ThemeStyleCache(fetcher, _dir);
            BlogThemeStyle theme = await cache.GetThemeAsync("acct-1", Homepage);

            Assert.That(fetcher.FetchCount, Is.EqualTo(1));
            Assert.That(theme.IsEmpty, Is.False);
        }

        [Test]
        public async Task Cache_StylesheetlessHomepage_EmptyResultIsCached()
        {
            var fetcher = new FakeThemeHtmlFetcher("<html><body><p>no styles here</p></body></html>");
            var cache = new ThemeStyleCache(fetcher, _dir);

            BlogThemeStyle first = await cache.GetThemeAsync("acct-1", Homepage);
            BlogThemeStyle second = await cache.GetThemeAsync("acct-1", Homepage);

            Assert.That(first, Is.Not.Null, "a successful fetch with no stylesheets is not a failure");
            Assert.That(first.IsEmpty, Is.True);
            Assert.That(fetcher.FetchCount, Is.EqualTo(1), "the empty result is cached too");
        }

        [Test]
        public async Task Cache_NoHomepage_ReturnsNull_WithoutFetching()
        {
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            var cache = new ThemeStyleCache(fetcher, _dir);

            Assert.That(await cache.GetThemeAsync("acct-1", null), Is.Null);
            Assert.That(await cache.GetThemeAsync("acct-1", "  "), Is.Null);
            Assert.That(fetcher.FetchCount, Is.EqualTo(0));
        }

        // ---- PreviewRenderer themed composition (pure) ----

        [Test]
        public void Preview_WithTheme_IncludesStylesheetsAndInlineStyles()
        {
            var theme = new BlogThemeStyle
            {
                StylesheetUrls = new[] { "https://doug.example.com/theme.css", "https://doug.example.com/print.css" },
                InlineStyles = new[] { "body { color: hotpink; }" },
                SourceUrl = Homepage
            };

            string document = PreviewRenderer.BuildPreviewDocument("<p>Hello</p>", "Title", theme: theme);

            Assert.Multiple(() =>
            {
                Assert.That(document, Does.Contain("<link rel=\"stylesheet\" href=\"https://doug.example.com/theme.css\">"));
                Assert.That(document, Does.Contain("<link rel=\"stylesheet\" href=\"https://doug.example.com/print.css\">"));
                Assert.That(document, Does.Contain("body { color: hotpink; }"));
                Assert.That(document, Does.Contain("<body class=\"olw-theme\">"));
                Assert.That(document, Does.Contain(PreviewRenderer.PreviewStyle),
                    "the neutral article style stays as the base layer");
                Assert.That(document, Does.Contain("<p>Hello</p>"));
            });
        }

        [Test]
        public void Preview_WithoutTheme_StaysNeutral()
        {
            string document = PreviewRenderer.BuildPreviewDocument("<p>Hello</p>", "Title");

            Assert.Multiple(() =>
            {
                Assert.That(document, Does.Not.Contain("rel=\"stylesheet\""));
                Assert.That(document, Does.Not.Contain("olw-theme"));
                Assert.That(document, Does.Contain(PreviewRenderer.PreviewStyle));
            });
        }

        [Test]
        public void Preview_EmptyTheme_StaysNeutral()
        {
            string document = PreviewRenderer.BuildPreviewDocument(
                "<p>Hello</p>", theme: new BlogThemeStyle { SourceUrl = Homepage });

            Assert.That(document, Does.Not.Contain("olw-theme"));
            Assert.That(document, Does.Not.Contain("rel=\"stylesheet\""));
        }

        [Test]
        public void Preview_ThemeStylesheetUrl_IsAttributeEscaped()
        {
            var theme = new BlogThemeStyle
            {
                StylesheetUrls = new[] { "https://doug.example.com/t.css?a=1&b=\"x\"" },
                SourceUrl = Homepage
            };

            string document = PreviewRenderer.BuildPreviewDocument("<p>x</p>", theme: theme);

            Assert.That(document, Does.Contain("t.css?a=1&amp;b=&quot;x&quot;"));
        }

        // ---- Shell wiring (headless) ----

        [AvaloniaTest]
        public async Task ClosePreview_SwitchesEditorBackToEditView()
        {
            var window = new MainWindow();
            var panel = window.FindControl<EditorPanel>("EditorPanel");
            Assert.That(panel, Is.Not.Null);

            panel.SetView("preview");
            Assert.That(panel.CurrentView, Is.EqualTo("preview"));

            await window.ExecuteCommandAsync(CommandId.ClosePreview);

            Assert.That(panel.CurrentView, Is.EqualTo("edit"));
        }

        [AvaloniaTest]
        public async Task UseTheme_NoAccount_GracefulStatusMessage()
        {
            var window = new MainWindow();
            // An account service over an empty temp store — no accounts configured.
            window.AccountService = new BlogAccountService(
                new FileAccountStore(Path.Combine(_dir, "accounts-empty")),
                new InMemoryCredentialStore());

            await window.ExecuteCommandAsync(CommandId.ViewUseStyles);

            Assert.That(StatusText(window), Does.Contain("no blog is selected"));
        }

        [AvaloniaTest]
        public async Task UseTheme_TogglesPerAccount_AndPersists()
        {
            var window = new MainWindow();
            window.AccountService = NewAccountService(out FileAccountStore store);

            await window.ExecuteCommandAsync(CommandId.ViewUseStyles);

            BlogAccount reloaded = store.Load("acct-1");
            Assert.That(reloaded.UseThemeForPreview, Is.True, "the toggle persists on the account");
            Assert.That(StatusText(window), Does.Contain("theme in Preview"));

            await window.ExecuteCommandAsync(CommandId.ViewUseStyles);

            reloaded = store.Load("acct-1");
            Assert.That(reloaded.UseThemeForPreview, Is.False, "toggling again turns it back off");
        }

        [AvaloniaTest]
        public async Task UpdateTheme_FetchesAndReportsResult()
        {
            var window = new MainWindow();
            window.AccountService = NewAccountService(out _);
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            window.ThemeCache = new ThemeStyleCache(fetcher, _dir);

            await window.ExecuteCommandAsync(CommandId.UpdateWeblogStyle);

            Assert.That(fetcher.FetchCount, Is.EqualTo(1));
            Assert.That(StatusText(window), Does.Contain("Theme updated"));
            Assert.That(StatusText(window), Does.Contain("1 stylesheet(s)"));
            Assert.That(StatusText(window), Does.Contain("1 inline style block(s)"));
        }

        [AvaloniaTest]
        public async Task UpdateTheme_FetchFailure_GracefulStatusMessage()
        {
            var window = new MainWindow();
            window.AccountService = NewAccountService(out _);
            window.ThemeCache = new ThemeStyleCache(new FakeThemeHtmlFetcher(null), _dir);

            await window.ExecuteCommandAsync(CommandId.UpdateWeblogStyle);

            Assert.That(StatusText(window), Does.Contain("Update Theme failed"));
        }

        [AvaloniaTest]
        public async Task PreviewThemeProvider_ReturnsThemeOnlyWhenEnabled()
        {
            var window = new MainWindow();
            window.AccountService = NewAccountService(out _);
            var fetcher = new FakeThemeHtmlFetcher(FixtureHtml());
            window.ThemeCache = new ThemeStyleCache(fetcher, _dir);

            var panel = window.FindControl<EditorPanel>("EditorPanel");
            Assert.That(panel.PreviewThemeProvider, Is.Not.Null, "the shell wires the provider");

            // Toggle off → neutral (null), and nothing is fetched.
            BlogThemeStyle off = await panel.PreviewThemeProvider();
            Assert.That(off, Is.Null);
            Assert.That(fetcher.FetchCount, Is.EqualTo(0));

            // Toggle on → the harvested theme flows to the preview composition.
            await window.ExecuteCommandAsync(CommandId.ViewUseStyles);
            BlogThemeStyle on = await panel.PreviewThemeProvider();
            Assert.That(on, Is.Not.Null);
            Assert.That(on.StylesheetUrls, Has.Count.EqualTo(1));
            Assert.That(fetcher.FetchCount, Is.EqualTo(1));
        }

        [AvaloniaTest]
        public async Task PreviewThemeProvider_FetchFailure_ReturnsNull_WithStatusMessage()
        {
            var window = new MainWindow();
            window.AccountService = NewAccountService(out _);
            window.ThemeCache = new ThemeStyleCache(new FakeThemeHtmlFetcher(null), _dir);
            await window.ExecuteCommandAsync(CommandId.ViewUseStyles);

            var panel = window.FindControl<EditorPanel>("EditorPanel");
            BlogThemeStyle theme = await panel.PreviewThemeProvider();

            Assert.That(theme, Is.Null, "a fetch failure degrades to the neutral preview");
            Assert.That(StatusText(window), Does.Contain("neutral preview"));
        }

        // ---- helpers ----

        private static string FixtureHtml(string cssName = "theme.css") =>
            "<html><head>" +
            $"<link rel=\"stylesheet\" href=\"{cssName}\">" +
            "<style>body { font-family: 'Theme Serif', serif; }</style>" +
            "</head><body><p>homepage</p></body></html>";

        private BlogAccountService NewAccountService(out FileAccountStore store)
        {
            store = new FileAccountStore(Path.Combine(_dir, "accounts-" + Guid.NewGuid().ToString("N")));
            var service = new BlogAccountService(store, new InMemoryCredentialStore());
            service.SaveAccount(new BlogAccount
            {
                Id = "acct-1",
                DisplayName = "Test Blog",
                HomepageUrl = Homepage,
                ApiEndpointUrl = "https://doug.example.com/xmlrpc.php",
                BlogId = "1",
                Username = "doug"
            }, "password");
            return service;
        }

        private static string StatusText(MainWindow window) =>
            window.FindControl<TextBlock>("StatusText")?.Text ?? string.Empty;

        private sealed class FakeThemeHtmlFetcher : IThemeHtmlFetcher
        {
            public FakeThemeHtmlFetcher(string body) { Body = body; }

            public string Body { get; set; }
            public int FetchCount { get; private set; }

            public Task<string> FetchAsync(string url)
            {
                FetchCount++;
                return Task.FromResult(Body);
            }
        }

        private sealed class ThrowingThemeHtmlFetcher : IThemeHtmlFetcher
        {
            public Task<string> FetchAsync(string url) =>
                throw new InvalidOperationException("simulated fetcher fault");
        }
    }
}
