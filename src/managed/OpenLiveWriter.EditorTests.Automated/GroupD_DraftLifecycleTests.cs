// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group D — document/draft lifecycle (P1-6). Drives the REAL cross-platform
    /// draft store (<see cref="FileDraftStore"/>) and <see cref="PostDocument"/> model
    /// plus the <see cref="DraftSession"/> controller against a per-test temp directory.
    /// No live WebView is needed: this is pure model + file I/O, so it runs in the
    /// default headless suite. Body equivalence is asserted via parsed DOM (AngleSharp)
    /// rather than brittle string compare.
    /// </summary>
    [TestFixture]
    [Category("GroupD")]
    public class GroupD_DraftLifecycleTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "OLWDraftTests", System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
            catch { /* best effort */ }
        }

        private FileDraftStore NewStore() => new FileDraftStore(_dir);

        private static bool BodyDomEqual(string a, string b) =>
            Dom.Parse(a).Body.InnerHtml == Dom.Parse(b).Body.InnerHtml;

        // --- Round-trip: create -> save -> load returns same title + body ---

        [Test]
        public void SaveThenLoad_RoundTripsTitleAndBody()
        {
            var store = NewStore();
            var doc = new PostDocument
            {
                Title = "My First Post",
                BodyHtml = "<p>Hello <b>world</b></p><ul><li>one</li><li>two</li></ul>"
            };

            PostDocument saved = store.Save(doc);
            Assert.That(saved.Id, Is.Not.Empty, "Save must assign an id to a new draft.");
            Assert.That(saved.IsDirty, Is.False, "Save must clear the dirty flag.");

            PostDocument loaded = store.Load(saved.Id);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.Title, Is.EqualTo("My First Post"));
            Assert.That(BodyDomEqual(loaded.BodyHtml, doc.BodyHtml), Is.True,
                "Loaded body must be DOM-equivalent to the saved body.");
        }

        [Test]
        public void Save_NewDocument_SetsCreatedAndModifiedTimestamps()
        {
            var store = NewStore();
            PostDocument saved = store.Save(new PostDocument { Title = "T", BodyHtml = "<p>x</p>" });

            Assert.That(saved.DateCreatedUtc, Is.Not.EqualTo(default(System.DateTime)));
            Assert.That(saved.DateModifiedUtc, Is.GreaterThanOrEqualTo(saved.DateCreatedUtc));
        }

        // --- Overwrite keeps id, refreshes modified ---

        [Test]
        public void Save_ExistingDocument_OverwritesInPlaceKeepingId()
        {
            var store = NewStore();
            PostDocument saved = store.Save(new PostDocument { Title = "V1", BodyHtml = "<p>v1</p>" });
            string id = saved.Id;
            var created = saved.DateCreatedUtc;

            saved.Title = "V2";
            saved.BodyHtml = "<p>v2</p>";
            PostDocument resaved = store.Save(saved);

            Assert.That(resaved.Id, Is.EqualTo(id), "Overwrite must keep the same id.");
            Assert.That(store.List().Count, Is.EqualTo(1), "Overwrite must not create a second file.");
            Assert.That(resaved.DateCreatedUtc, Is.EqualTo(created), "Created timestamp is preserved.");

            PostDocument loaded = store.Load(id);
            Assert.That(loaded.Title, Is.EqualTo("V2"));
            Assert.That(BodyDomEqual(loaded.BodyHtml, "<p>v2</p>"), Is.True);
        }

        // --- List / MRU ordering: most-recently-modified first ---

        [Test]
        public void List_OrdersByMostRecentlyModifiedFirst()
        {
            var store = NewStore();
            var a = store.Save(new PostDocument { Title = "A", BodyHtml = "<p>a</p>" });
            var b = store.Save(new PostDocument { Title = "B", BodyHtml = "<p>b</p>" });
            var c = store.Save(new PostDocument { Title = "C", BodyHtml = "<p>c</p>" });

            // Touch A last so it becomes most-recent.
            a.BodyHtml = "<p>a2</p>";
            store.Save(a);

            var ids = store.List().Select(d => d.Id).ToList();
            Assert.That(ids.First(), Is.EqualTo(a.Id), "Most recently modified draft must be first.");
            Assert.That(ids, Does.Contain(b.Id).And.Contain(c.Id));
            Assert.That(ids.Count, Is.EqualTo(3));
        }

        [Test]
        public void List_MissingDirectory_ReturnsEmpty()
        {
            var store = new FileDraftStore(Path.Combine(_dir, "does-not-exist"));
            Assert.That(store.List(), Is.Empty);
        }

        // --- Delete ---

        [Test]
        public void Delete_RemovesDraft()
        {
            var store = NewStore();
            var saved = store.Save(new PostDocument { Title = "ToDelete", BodyHtml = "<p>x</p>" });
            Assert.That(store.Exists(saved.Id), Is.True);

            store.Delete(saved.Id);

            Assert.That(store.Exists(saved.Id), Is.False);
            Assert.That(store.Load(saved.Id), Is.Null);
            Assert.That(store.List(), Is.Empty);
        }

        [Test]
        public void Delete_MissingDraft_IsNoOp()
        {
            var store = NewStore();
            Assert.DoesNotThrow(() => store.Delete("no-such-id"));
        }

        // --- Corrupt / missing file handling ---

        [Test]
        public void Load_MissingDraft_ReturnsNull()
        {
            var store = NewStore();
            Assert.That(store.Load("missing"), Is.Null);
        }

        [Test]
        public void Load_CorruptFile_ThrowsDraftStoreException()
        {
            var store = NewStore();
            Directory.CreateDirectory(_dir);
            string path = Path.Combine(_dir, "broken.oldraft.json");
            File.WriteAllText(path, "{ this is not valid json ");

            Assert.Throws<DraftStoreException>(() => store.Load("broken"));
        }

        [Test]
        public void List_SkipsCorruptFiles()
        {
            var store = NewStore();
            var good = store.Save(new PostDocument { Title = "Good", BodyHtml = "<p>ok</p>" });
            File.WriteAllText(Path.Combine(_dir, "bad.oldraft.json"), "{ broken ");

            var ids = store.List().Select(d => d.Id).ToList();
            Assert.That(ids, Does.Contain(good.Id));
            Assert.That(ids, Does.Not.Contain("bad"), "Corrupt files must be skipped, not listed.");
        }

        // --- PostDocument <-> BlogPost interop ---

        [Test]
        public void ToBlogPost_MapsTitleBodyAndCategories()
        {
            var doc = new PostDocument
            {
                Title = "Interop",
                BodyHtml = "<p>Intro</p>" + BlogPost.ExtendedEntryBreak + "<p>More</p>",
                IsPublished = false
            };
            doc.Categories.Add("News");

            BlogPost post = doc.ToBlogPost();

            Assert.That(post.Title, Is.EqualTo("Interop"));
            Assert.That(post.MainContents, Is.EqualTo("<p>Intro</p>"));
            Assert.That(post.ExtendedContents, Is.EqualTo("<p>More</p>"));
            Assert.That(post.IsPublished, Is.False);
            Assert.That(post.Categories, Does.Contain("News"));
        }

        [Test]
        public void FromBlogPost_CreatesEditableDocumentWithoutLocalId()
        {
            var post = new BlogPost { Title = "Server Post", Contents = "<p>body</p>" };
            post.Categories.Add("Tech");

            PostDocument doc = PostDocument.FromBlogPost(post);

            Assert.That(doc.Id, Is.Empty, "A post opened for editing starts as a new local draft.");
            Assert.That(doc.Title, Is.EqualTo("Server Post"));
            Assert.That(BodyDomEqual(doc.BodyHtml, "<p>body</p>"), Is.True);
            Assert.That(doc.Categories, Does.Contain("Tech"));
        }

        // --- DraftSession controller (dirty tracking + lifecycle, no WebView) ---

        [Test]
        public void DraftSession_TracksDirtyAndClearsOnSave()
        {
            var session = new DraftSession(NewStore());
            Assert.That(session.IsDirty, Is.False);

            session.UpdateTitle("Hello");
            Assert.That(session.IsDirty, Is.True, "Editing the title marks the document dirty.");

            session.Save();
            Assert.That(session.IsDirty, Is.False, "Save clears the dirty flag.");
            Assert.That(session.Current.IsSaved, Is.True);
        }

        [Test]
        public void DraftSession_UpdateTitle_SameValue_DoesNotMarkDirty()
        {
            var session = new DraftSession(NewStore());
            session.UpdateTitle("Same");
            session.Save();

            session.UpdateTitle("Same");
            Assert.That(session.IsDirty, Is.False, "Re-setting the same title must not mark dirty.");
        }

        [Test]
        public void DraftSession_NewPost_ResetsToEmptyDocument()
        {
            var session = new DraftSession(NewStore());
            session.Save("Kept", "<p>kept</p>");

            session.NewPost();
            Assert.That(session.Current.Id, Is.Empty);
            Assert.That(session.Current.Title, Is.Empty);
            Assert.That(session.Current.BodyHtml, Is.Empty);
        }

        [Test]
        public void DraftSession_OpenAndDeleteCurrent_RoundTripsThenResets()
        {
            var session = new DraftSession(NewStore());
            session.Save("Doc", "<p>content</p>");
            string id = session.Current.Id;

            session.NewPost();
            Assert.That(session.Open(id), Is.True);
            Assert.That(session.Current.Title, Is.EqualTo("Doc"));

            session.Delete(id);
            Assert.That(session.Current.Id, Is.Empty, "Deleting the current draft resets the session.");
            Assert.That(session.ListDrafts(), Is.Empty);
        }

        [Test]
        public void DraftSession_Open_MissingDraft_ReturnsFalse()
        {
            var session = new DraftSession(NewStore());
            Assert.That(session.Open("nope"), Is.False);
        }
    }
}
