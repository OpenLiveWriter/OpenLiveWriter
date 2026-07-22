// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group T (part 4) — Open from Blog (Band 3a, P1-4): the server-post →
    /// <see cref="PostDocument"/> mapping (published flags, page flag, extended-entry
    /// rejoin), the <see cref="DraftSession.OpenDocument"/> adoption path, the
    /// <see cref="OpenFromBlogDialog"/> headless behavior (load / toggle / error), and
    /// the menu + handled-command registration that makes the feature reachable.
    /// </summary>
    [TestFixture]
    [Category("GroupT")]
    public class GroupT_OpenFromBlogTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "OLWOpenFromBlog", Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
            catch { /* best effort */ }
        }

        // ---- ServerPost → PostDocument mapping ----

        [Test]
        public void FromServerPost_PublishedPost_MarksDocumentForEditPath()
        {
            var post = new ServerPost
            {
                PostId = "412",
                Title = "Hello macOS",
                Description = "<p>Main</p>",
                TextMore = "<p>More</p>",
                Status = "publish",
                Categories = new[] { "News", "macOS" },
                Keywords = "avalonia, macos"
            };

            PostDocument doc = PostDocument.FromServerPost(post, "blog-7");

            Assert.That(doc.BlogId, Is.EqualTo("blog-7"));
            Assert.That(doc.PublishedPostId, Is.EqualTo("412"),
                "republish must edit the same server post, not create a duplicate");
            Assert.That(doc.IsPublished, Is.True);
            Assert.That(doc.IsPage, Is.False);
            Assert.That(doc.Title, Is.EqualTo("Hello macOS"));
            Assert.That(doc.BodyHtml,
                Is.EqualTo("<p>Main</p>" + BlogPost.ExtendedEntryBreak + "<p>More</p>"));
            Assert.That(doc.Categories, Is.EqualTo(new[] { "News", "macOS" }));
            Assert.That(doc.Keywords, Is.EqualTo(new[] { "avalonia", "macos" }));
            Assert.That(doc.Id, Is.Empty, "a server post is not a local draft yet");
            Assert.That(doc.IsDirty, Is.False);
        }

        [Test]
        public void FromServerPost_ServerDraft_StaysUnpublished()
        {
            var post = new ServerPost { PostId = "1", Status = "draft", Description = "<p>x</p>" };
            PostDocument doc = PostDocument.FromServerPost(post, "blog-1");
            Assert.That(doc.IsPublished, Is.False);
        }

        [Test]
        public void FromServerPost_Page_MarksIsPage()
        {
            var post = new ServerPost
            {
                PostId = "87",
                Title = "About",
                Description = "<p>About us</p>",
                Status = "publish",
                IsPage = true
            };

            PostDocument doc = PostDocument.FromServerPost(post, "blog-1");

            Assert.That(doc.IsPage, Is.True, "a fetched page must republish via wp.editPage");
            Assert.That(doc.PublishedPostId, Is.EqualTo("87"));
        }

        // ---- DraftSession.OpenDocument ----

        [Test]
        public void OpenDocument_AdoptsExternalDocument_RaisesCurrentChanged()
        {
            var session = new DraftSession(new FileDraftStore(_dir));
            bool raised = false;
            session.CurrentChanged += (s, e) => raised = true;

            var doc = new PostDocument { Title = "Server post", BlogId = "blog-1", PublishedPostId = "9" };
            session.OpenDocument(doc);

            Assert.That(raised, Is.True);
            Assert.That(session.Current, Is.SameAs(doc));
            Assert.That(session.ListDrafts(), Is.Empty, "opening a server post must not touch the draft store");
        }

        [Test]
        public void OpenDocument_Null_Throws()
        {
            var session = new DraftSession(new FileDraftStore(_dir));
            Assert.Throws<ArgumentNullException>(() => session.OpenDocument(null));
        }

        // ---- OpenFromBlogDialog (headless) ----

        private static List<ServerPost> SamplePosts() => new List<ServerPost>
        {
            new ServerPost
            {
                PostId = "412",
                Title = "Hello macOS",
                Description = "<p>Main</p>",
                Status = "publish",
                DateCreatedUtc = DateTime.UtcNow.AddDays(-1)
            },
            new ServerPost
            {
                PostId = "411",
                Title = "",
                Description = "<p>Untitled body</p>",
                Status = "draft",
                DateCreatedUtc = DateTime.UtcNow.AddDays(-2)
            }
        };

        [AvaloniaTest]
        public async Task Dialog_LoadsPosts_PopulatesList_ClearsStatus()
        {
            var dialog = new OpenFromBlogDialog((pages, count) =>
                Task.FromResult<IReadOnlyList<ServerPost>>(SamplePosts()));

            await dialog.RefreshAsync();

            Assert.That(dialog.ListedPosts.Count, Is.EqualTo(2));
            Assert.That(dialog.ListedPosts[0].PostId, Is.EqualTo("412"));
            Assert.That(dialog.StatusText, Is.Empty, "a successful load clears the status line");
        }

        [AvaloniaTest]
        public async Task Dialog_EmptyBlog_ShowsEmptyState()
        {
            var dialog = new OpenFromBlogDialog((pages, count) =>
                Task.FromResult<IReadOnlyList<ServerPost>>(Array.Empty<ServerPost>()));

            await dialog.RefreshAsync();

            Assert.That(dialog.ListedPosts, Is.Empty);
            Assert.That(dialog.StatusText, Does.Contain("No recent posts"));
        }

        [AvaloniaTest]
        public async Task Dialog_FetchFailure_ShowsError_NeverThrows()
        {
            var dialog = new OpenFromBlogDialog((pages, count) =>
                Task.FromException<IReadOnlyList<ServerPost>>(
                    new System.Net.Http.HttpRequestException("offline")));

            await dialog.RefreshAsync();

            Assert.That(dialog.ListedPosts, Is.Empty);
            Assert.That(dialog.StatusText, Does.Contain("Couldn't load"));
            Assert.That(dialog.StatusText, Does.Contain("offline"));
        }

        [AvaloniaTest]
        public async Task Dialog_PagesToggle_FetchesPages()
        {
            var calls = new List<(bool Pages, int Count)>();
            var dialog = new OpenFromBlogDialog((pages, count) =>
            {
                calls.Add((pages, count));
                IReadOnlyList<ServerPost> result = pages
                    ? new List<ServerPost> { new ServerPost { PostId = "87", Title = "About", IsPage = true } }
                    : SamplePosts();
                return Task.FromResult(result);
            });

            await dialog.RefreshAsync();
            await dialog.RefreshForOptionsAsync(pages: true, countIndex: 0);

            Assert.That(calls.Count, Is.EqualTo(2));
            Assert.That(calls[0], Is.EqualTo((false, 25)), "initial load: posts, default count 25");
            Assert.That(calls[1], Is.EqualTo((true, 10)), "toggle: pages with the selected count");
            Assert.That(dialog.ListedPosts.Count, Is.EqualTo(1));
            Assert.That(dialog.ListedPosts[0].IsPage, Is.True);
        }

        [AvaloniaTest]
        public async Task Dialog_CountSelector_PassesRequestedCount()
        {
            int requested = -1;
            var dialog = new OpenFromBlogDialog((pages, count) =>
            {
                requested = count;
                return Task.FromResult<IReadOnlyList<ServerPost>>(SamplePosts());
            });

            await dialog.RefreshForOptionsAsync(pages: false, countIndex: 2); // 50

            Assert.That(requested, Is.EqualTo(50));
        }

        [AvaloniaTest]
        public async Task Dialog_Selection_EnablesOpen()
        {
            var dialog = new OpenFromBlogDialog((pages, count) =>
                Task.FromResult<IReadOnlyList<ServerPost>>(SamplePosts()));
            await dialog.RefreshAsync();

            var list = dialog.GetLogicalDescendants().OfType<ListBox>().First();
            var open = dialog.GetLogicalDescendants().OfType<Button>()
                .First(b => (b.Content as string) == "Open");

            Assert.That(open.IsEnabled, Is.False, "Open starts disabled with no selection");
            list.SelectedIndex = 0;
            Assert.That(open.IsEnabled, Is.True);
        }

        // ---- Reachability: menu + handled-command registry ----

        [Test]
        public void FileMenu_ContainsOpenFromBlog()
        {
            ShellMenu file = ShellMenuBuilder.Build().First(m => m.Label == "File");
            ShellMenuItem item = file.Items.FirstOrDefault(
                i => !i.IsSeparator && i.CommandId == CommandId.OpenRecentPosts);

            Assert.That(item, Is.Not.Null, "File menu must offer Open from Blog");
            Assert.That(item.Label, Does.Contain("Open from Blog"));
            Assert.That(item.Gesture, Is.EqualTo("Cmd+Shift+O"));
        }

        [Test]
        public void HandledCommands_RegistersOpenRecentPosts()
        {
            Assert.That(HandledCommands.IsHandled(CommandId.OpenRecentPosts), Is.True,
                "the command must be registered or its UI renders disabled");
        }

        [AvaloniaTest]
        public void DraftPickerDialog_OffersOpenFromBlogButton()
        {
            // Headless construct only (no show): the pivot button must exist so the
            // ribbon/File OpenPost flow can reach the server list.
            var dialog = new DraftPickerDialog(Array.Empty<DraftInfo>());
            Button fromBlog = dialog.GetLogicalDescendants().OfType<Button>()
                .FirstOrDefault(b => (b.Content as string) == "Open from Blog\u2026");

            Assert.That(fromBlog, Is.Not.Null);
            Assert.That(dialog.RequestedOpenFromBlog, Is.False);
        }
    }
}
