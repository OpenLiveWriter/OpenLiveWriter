// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.ImageEditing;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group G2 — file-path image references (the Windows behavior): inserted images
    /// are copied into a per-draft <c>Media/{mediaId}/</c> folder and referenced by
    /// <c>file://</c> src; on publish they are uploaded and rewritten to hosted URLs.
    /// Covers the <see cref="MediaStore"/> copy/dedup/URI/delete, the
    /// <see cref="ImagePublisher"/> file:// scan/upload/rewrite (including the mixed
    /// data-URI + file:// case and the missing-file abort), the
    /// <see cref="HttpImageFetcher"/> file:// read used by Picture Tools baking, and
    /// the draft JSON round-trip with file:// srcs.
    /// </summary>
    [TestFixture]
    [Category("GroupG")]
    public class GroupG_FileImageTests
    {
        // 1x1 transparent PNG.
        private static readonly byte[] PngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "OLWFileImgTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
            catch { /* best effort */ }
        }

        private string WriteSourceImage(string name = "photo.png")
        {
            string path = Path.Combine(_dir, name);
            File.WriteAllBytes(path, PngBytes);
            return path;
        }

        // ---- MediaStore ----

        [Test]
        public void MediaStore_AddImage_CopiesFile_AndReturnsFileUri()
        {
            var store = new MediaStore(Path.Combine(_dir, "appdata"));
            string source = WriteSourceImage();

            string uri = store.AddImage("doc1", source);

            Assert.That(uri, Does.StartWith("file://"));
            string copiedPath = new Uri(uri).LocalPath;
            Assert.That(copiedPath, Is.EqualTo(
                Path.Combine(_dir, "appdata", "Media", "doc1", "photo.png")));
            Assert.That(File.ReadAllBytes(copiedPath), Is.EqualTo(PngBytes));
        }

        [Test]
        public void MediaStore_AddImage_DedupesFileNames()
        {
            var store = new MediaStore(Path.Combine(_dir, "appdata"));
            string source = WriteSourceImage();

            string first = new Uri(store.AddImage("doc1", source)).LocalPath;
            string second = new Uri(store.AddImage("doc1", source)).LocalPath;
            string third = new Uri(store.AddImage("doc1", source)).LocalPath;

            Assert.That(Path.GetFileName(first), Is.EqualTo("photo.png"));
            Assert.That(Path.GetFileName(second), Is.EqualTo("photo-2.png"));
            Assert.That(Path.GetFileName(third), Is.EqualTo("photo-3.png"));
        }

        [Test]
        public void MediaStore_AddImage_SeparateDocuments_GetSeparateFolders()
        {
            var store = new MediaStore(Path.Combine(_dir, "appdata"));
            string source = WriteSourceImage();

            string a = new Uri(store.AddImage("docA", source)).LocalPath;
            string b = new Uri(store.AddImage("docB", source)).LocalPath;

            Assert.That(Path.GetFileName(a), Is.EqualTo("photo.png"));
            Assert.That(Path.GetFileName(b), Is.EqualTo("photo.png"));
            Assert.That(a, Does.Contain(Path.Combine("Media", "docA")));
            Assert.That(b, Does.Contain(Path.Combine("Media", "docB")));
        }

        [Test]
        public void MediaStore_AddImage_MissingSource_Throws()
        {
            var store = new MediaStore(Path.Combine(_dir, "appdata"));
            Assert.Throws<FileNotFoundException>(
                () => store.AddImage("doc1", Path.Combine(_dir, "nope.png")));
        }

        [Test]
        public void MediaStore_DeleteMedia_RemovesFolder_AndToleratesMissing()
        {
            var store = new MediaStore(Path.Combine(_dir, "appdata"));
            string source = WriteSourceImage();
            store.AddImage("doc1", source);
            string mediaDir = store.GetMediaDirectory("doc1");
            Assert.That(Directory.Exists(mediaDir), Is.True);

            store.DeleteMedia("doc1");
            Assert.That(Directory.Exists(mediaDir), Is.False);

            Assert.DoesNotThrow(() => store.DeleteMedia("doc1"));
            Assert.DoesNotThrow(() => store.DeleteMedia(null));
            Assert.DoesNotThrow(() => store.DeleteMedia(string.Empty));
        }

        // ---- ImagePublisher: file:// scanning ----

        [Test]
        public void FindLocalFileImages_FindsAndDedups_SkipsAnchors()
        {
            string html =
                "<p><img src=\"file:///Users/x/Media/doc1/a.png\"></p>" +
                "<p><img src=\"file:///Users/x/Media/doc1/a.png\"></p>" +
                "<a href=\"file:///Users/x/Media/doc1/b.png\">not an image</a>";

            var found = ImagePublisher.FindLocalFileImages(html);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].FileUri, Is.EqualTo("file:///Users/x/Media/doc1/a.png"));
            Assert.That(found[0].LocalPath, Is.EqualTo("/Users/x/Media/doc1/a.png"));
            Assert.That(found[0].FileName, Is.EqualTo("a.png"));
        }

        // ---- ImagePublisher: file:// upload + rewrite ----

        [Test]
        public async Task Rewrite_FileImage_UploadsBytes_AndRewritesSrc()
        {
            var fake = new FakeBlogClient();
            string source = WriteSourceImage();
            string fileUri = new Uri(source).AbsoluteUri;
            string html = $"<p>Pic:</p><img src=\"{fileUri}\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(fake, "blog-3", html);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1));
            var upload = fake.MediaUploads.Single();
            Assert.That(upload.BlogId, Is.EqualTo("blog-3"));
            Assert.That(upload.FileName, Is.EqualTo("photo.png"), "original file name is kept");
            Assert.That(upload.MimeType, Is.EqualTo("image/png"));
            Assert.That(upload.Bits, Is.EqualTo(PngBytes));
            Assert.That(result, Does.Contain("https://cdn.example.com/uploads/photo.png"));
            Assert.That(result, Does.Not.Contain("file://"));
        }

        [Test]
        public async Task Rewrite_DuplicateFileImage_UploadsOnce_ReplacesBoth()
        {
            var fake = new FakeBlogClient();
            string source = WriteSourceImage();
            string fileUri = new Uri(source).AbsoluteUri;
            string html = $"<img src=\"{fileUri}\"><hr><img src=\"{fileUri}\" alt=\"again\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(fake, "blog-1", html);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1), "identical file refs upload once");
            int occurrences = result.Split(
                new[] { "https://cdn.example.com/uploads/photo.png" }, StringSplitOptions.None).Length - 1;
            Assert.That(occurrences, Is.EqualTo(2));
            Assert.That(result, Does.Contain("alt=\"again\""), "other img attributes survive the rewrite");
        }

        [Test]
        public async Task Rewrite_MixedDataUriAndFileImage_UploadsBoth()
        {
            var fake = new FakeBlogClient();
            string source = WriteSourceImage();
            string fileUri = new Uri(source).AbsoluteUri;
            string html =
                $"<img src=\"data:image/png;base64,{Convert.ToBase64String(PngBytes)}\">" +
                $"<img src=\"{fileUri}\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(fake, "blog-1", html);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(2));
            Assert.That(fake.MediaUploads.Select(u => u.FileName),
                Is.EqualTo(new[] { "image1.png", "photo.png" }));
            Assert.That(result, Does.Contain("uploads/image1.png"));
            Assert.That(result, Does.Contain("uploads/photo.png"));
            Assert.That(result, Does.Not.Contain("data:image"));
            Assert.That(result, Does.Not.Contain("file://"));
        }

        [Test]
        public void Rewrite_MissingLocalFile_Aborts_WithoutBrokenHtml()
        {
            var fake = new FakeBlogClient();
            string fileUri = "file:///definitely/not/here/ghost.png";
            string html = $"<p>Before</p><img src=\"{fileUri}\"><p>After</p>";

            var ex = Assert.ThrowsAsync<BlogClientPublishException>(
                async () => await ImagePublisher.RewriteInlineImagesAsync(fake, "blog-1", html));
            Assert.That(ex.Message, Does.Contain("ghost.png"));
            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(0), "nothing was uploaded before the abort");
        }

        [Test]
        public void Rewrite_FileReadSeamFailure_Aborts()
        {
            var fake = new FakeBlogClient();
            string html = "<img src=\"file:///x/y.png\">";

            var ex = Assert.ThrowsAsync<BlogClientPublishException>(
                async () => await ImagePublisher.RewriteInlineImagesAsync(
                    fake, "blog-1", html, readLocalFile: _ => null));
            Assert.That(ex.Message, Does.Contain("y.png"));
        }

        [Test]
        public async Task Rewrite_FileReadSeam_SuppliesBytes()
        {
            var fake = new FakeBlogClient();
            string html = "<img src=\"file:///x/icon.gif\">";
            byte[] seamBytes = { 1, 2, 3 };

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => seamBytes);

            var upload = fake.MediaUploads.Single();
            Assert.That(upload.FileName, Is.EqualTo("icon.gif"));
            Assert.That(upload.MimeType, Is.EqualTo("image/gif"));
            Assert.That(upload.Bits, Is.EqualTo(seamBytes));
            Assert.That(result, Does.Contain("uploads/icon.gif"));
        }

        [Test]
        public async Task Rewrite_FileUriInAnchor_IsLeftAlone()
        {
            var fake = new FakeBlogClient();
            string html = "<a href=\"file:///x/report.png\">download</a>";

            string result = await ImagePublisher.RewriteInlineImagesAsync(fake, "blog-1", html);

            Assert.That(result, Is.EqualTo(html));
            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(0));
        }

        // ---- ImageFetcher: file:// read (Picture Tools bake path) ----

        [Test]
        public async Task ImageFetcher_FileUri_ReadsBytesFromDisk()
        {
            string source = WriteSourceImage();
            var fetcher = new HttpImageFetcher(new HttpClient());

            byte[] bytes = await fetcher.FetchAsync(new Uri(source).AbsoluteUri);

            Assert.That(bytes, Is.EqualTo(PngBytes));
        }

        [Test]
        public async Task ImageFetcher_FileUriMissing_ReturnsNull()
        {
            var fetcher = new HttpImageFetcher(new HttpClient());
            byte[] bytes = await fetcher.FetchAsync("file:///definitely/not/here.png");
            Assert.That(bytes, Is.Null);
        }

        // ---- Draft persistence: file:// srcs + MediaId ----

        [Test]
        public void Draft_RoundTrip_PreservesFileUriBodyAndMediaId()
        {
            var store = new FileDraftStore(Path.Combine(_dir, "drafts"));
            var doc = new PostDocument
            {
                Title = "Local pics",
                BodyHtml = "<p>Look:</p><img src=\"file:///Users/x/Media/abc/photo.png\">"
            };
            string mediaId = doc.MediaId;

            PostDocument saved = store.Save(doc);
            PostDocument loaded = store.Load(saved.Id);

            Assert.That(loaded.BodyHtml, Does.Contain("file:///Users/x/Media/abc/photo.png"));
            Assert.That(loaded.MediaId, Is.EqualTo(mediaId), "the media folder key is stable across save/load");
        }

        [Test]
        public void PostDocument_MediaId_AssignedAtCreation_AndUnique()
        {
            var a = new PostDocument();
            var b = new PostDocument();

            Assert.That(a.MediaId, Is.Not.Null.And.Not.Empty);
            Assert.That(a.MediaId, Is.Not.EqualTo(b.MediaId));
            Assert.That(a.IsSaved, Is.False, "MediaId must not make an unsaved document look saved");
        }

        [Test]
        public void PostDocument_OldDraftWithoutMediaId_GetsFreshIdOnLoad()
        {
            string draftsDir = Path.Combine(_dir, "drafts");
            Directory.CreateDirectory(draftsDir);
            string id = "legacy123";
            File.WriteAllText(Path.Combine(draftsDir, id + ".oldraft.json"),
                "{\"Id\":\"" + id + "\",\"Title\":\"Old\",\"BodyHtml\":\"<p>x</p>\"}");

            PostDocument loaded = new FileDraftStore(draftsDir).Load(id);

            Assert.That(loaded.MediaId, Is.Not.Null.And.Not.Empty,
                "a pre-MediaId draft gets a folder key so image insertion works on it");
        }
    }
}
