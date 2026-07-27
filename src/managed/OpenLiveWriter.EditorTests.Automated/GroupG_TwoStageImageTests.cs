// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.ImageEditing;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using SkiaSharp;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group G3 — two-stage image upload on publish (Windows "Link to: source picture"
    /// behavior): an image displayed smaller than its natural size uploads BOTH a
    /// resized display copy (becomes the <c>&lt;img src&gt;</c>) and the original
    /// full-size bytes (the click-through link target), and the img is wrapped in
    /// <c>&lt;a href="{original-url}"&gt;</c>. Covers the pure display-size parsing
    /// (<see cref="ImagePublisher.TryGetDisplaySize"/>), the pure resize decision
    /// (<see cref="ImagePublisher.ShouldResizeForDisplay"/>), the two-upload flow
    /// through <see cref="FakeBlogClient"/> with a fake <see cref="PublishImageResizer"/>
    /// seam, the already-linked/no-dims/no-resize/no-seam fallbacks, and one
    /// end-to-end pass with the real SkiaSharp seam the shell wires.
    /// </summary>
    [TestFixture]
    [Category("GroupG")]
    public class GroupG_TwoStageImageTests
    {
        // 1x1 transparent PNG (content irrelevant to the fake-seam tests).
        private static readonly byte[] PngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

        // Fake seam: natural size 800x600; the "resized" payload is marker bytes
        // carrying the requested dims so tests can assert what was uploaded.
        private static PublishImageResizer FakeResizer() =>
            new PublishImageResizer(
                probeNaturalSize: _ => (800, 600),
                resize: (_, w, h) => new[] { (byte)(w / 256), (byte)(w % 256), (byte)(h / 256), (byte)(h % 256) });

        // ---- Pure display-size parsing ----

        [Test]
        public void DisplaySize_FromWidthHeightAttributes()
        {
            bool ok = ImagePublisher.TryGetDisplaySize(
                "<img src=\"file:///x/p.png\" width=\"320\" height=\"240\">", out int w, out int h);

            Assert.That(ok, Is.True);
            Assert.That(w, Is.EqualTo(320));
            Assert.That(h, Is.EqualTo(240));
        }

        [Test]
        public void DisplaySize_FromInlineStyle()
        {
            bool ok = ImagePublisher.TryGetDisplaySize(
                "<img src=\"file:///x/p.png\" style=\"width: 320px; height: 240px;\">", out int w, out int h);

            Assert.That(ok, Is.True);
            Assert.That(w, Is.EqualTo(320));
            Assert.That(h, Is.EqualTo(240));
        }

        [Test]
        public void DisplaySize_StyleOverridesAttributes()
        {
            // The editor sets both (attribute + matching style); when they disagree
            // the CSS value is what the browser renders, so it must win.
            bool ok = ImagePublisher.TryGetDisplaySize(
                "<img src=\"file:///x/p.png\" width=\"100\" height=\"100\" style=\"width: 320px; height: 240px;\">",
                out int w, out int h);

            Assert.That(ok, Is.True);
            Assert.That(w, Is.EqualTo(320));
            Assert.That(h, Is.EqualTo(240));
        }

        [Test]
        public void DisplaySize_MissingOrPartialOrNonPx_ReturnsFalse()
        {
            Assert.That(ImagePublisher.TryGetDisplaySize(
                "<img src=\"file:///x/p.png\">", out _, out _), Is.False, "no sizing");
            Assert.That(ImagePublisher.TryGetDisplaySize(
                "<img src=\"file:///x/p.png\" width=\"320\">", out _, out _), Is.False, "width only");
            Assert.That(ImagePublisher.TryGetDisplaySize(
                "<img src=\"file:///x/p.png\" style=\"width: 50%; height: 240px;\">", out _, out _),
                Is.False, "percentage width is not a px size");
            Assert.That(ImagePublisher.TryGetDisplaySize(null, out _, out _), Is.False);
        }

        // ---- Pure resize decision ----

        [TestCase(320, 240, 800, 600, true, "smaller in both dims")]
        [TestCase(800, 600, 800, 600, false, "natural size")]
        [TestCase(1024, 768, 800, 600, false, "larger in both dims")]
        [TestCase(320, 600, 800, 600, false, "height not smaller")]
        [TestCase(900, 240, 800, 600, false, "width larger, height smaller (mixed)")]
        [TestCase(0, 240, 800, 600, false, "zero width")]
        public void ShouldResize_Matrix(int dw, int dh, int nw, int nh, bool expected, string because)
        {
            Assert.That(ImagePublisher.ShouldResizeForDisplay(dw, dh, nw, nh),
                Is.EqualTo(expected), because);
        }

        // ---- Two-stage upload flow ----

        [Test]
        public async Task TwoStage_SmallerDisplay_UploadsOriginalThenResized_WrapsAnchor()
        {
            var fake = new FakeBlogClient();
            string html = "<p>Look:</p><img src=\"file:///x/photo.png\" width=\"320\" height=\"240\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: FakeResizer());

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(2), "original + resized display copy");
            Assert.That(fake.MediaUploads.Select(u => u.FileName),
                Is.EqualTo(new[] { "photo.png", "photo_320x240.png" }));
            Assert.That(fake.MediaUploads[0].Bits, Is.EqualTo(PngBytes), "original bytes uploaded first");
            Assert.That(fake.MediaUploads[1].MimeType, Is.EqualTo("image/png"));
            Assert.That(fake.MediaUploads[1].Bits, Is.EqualTo(new byte[] { 1, 64, 0, 240 }),
                "the seam was asked for a 320x240 resize");

            Assert.That(result, Does.Contain(
                "<a href=\"https://cdn.example.com/uploads/photo.png\">" +
                "<img src=\"https://cdn.example.com/uploads/photo_320x240.png\" width=\"320\" height=\"240\"></a>"),
                "src = resized URL, wrapped in an anchor to the original URL");
            Assert.That(result, Does.Not.Contain("file://"));
        }

        [Test]
        public async Task TwoStage_StyleSizedDisplay_AlsoResizes()
        {
            var fake = new FakeBlogClient();
            string html = "<img src=\"file:///x/photo.png\" style=\"width: 320px; height: 240px;\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: FakeResizer());

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(2));
            Assert.That(result, Does.Contain("uploads/photo_320x240.png"));
            Assert.That(result, Does.Contain("<a href=\"https://cdn.example.com/uploads/photo.png\">"));
        }

        [Test]
        public async Task TwoStage_AlreadyLinkedImage_RespectsExistingAnchor()
        {
            var fake = new FakeBlogClient();
            string html = "<a href=\"https://example.org/custom\"><img src=\"file:///x/photo.png\" width=\"320\" height=\"240\"></a>";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: FakeResizer());

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(2), "both copies still upload");
            Assert.That(result, Does.Contain("src=\"https://cdn.example.com/uploads/photo_320x240.png\""),
                "the src still becomes the resized URL");
            int anchorCount = result.Split(new[] { "<a " }, StringSplitOptions.None).Length - 1;
            Assert.That(anchorCount, Is.EqualTo(1), "no double-wrapping");
            Assert.That(result, Does.Contain("href=\"https://example.org/custom\""),
                "the existing link target is respected");
        }

        [Test]
        public async Task TwoStage_NoDisplayDims_SingleUploadNoAnchor()
        {
            var fake = new FakeBlogClient();
            string html = "<img src=\"file:///x/photo.png\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: FakeResizer());

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1));
            Assert.That(result, Does.Contain("src=\"https://cdn.example.com/uploads/photo.png\""));
            Assert.That(result, Does.Not.Contain("<a "));
        }

        [Test]
        public async Task TwoStage_DisplayNotSmaller_SingleUploadNoAnchor()
        {
            var fake = new FakeBlogClient();
            // 900x240: wider than the (fake) 800px natural width — upscaling must
            // not trigger a re-encode.
            string html = "<img src=\"file:///x/photo.png\" width=\"900\" height=\"240\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: FakeResizer());

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1));
            Assert.That(result, Does.Contain("src=\"https://cdn.example.com/uploads/photo.png\""));
            Assert.That(result, Does.Not.Contain("<a "));
        }

        [Test]
        public async Task TwoStage_NullResizer_KeepsSingleUploadBehavior()
        {
            var fake = new FakeBlogClient();
            string html = "<img src=\"file:///x/photo.png\" width=\"320\" height=\"240\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1), "no seam, no resizing");
            Assert.That(result, Does.Contain("src=\"https://cdn.example.com/uploads/photo.png\""));
            Assert.That(result, Does.Not.Contain("<a "));
        }

        [Test]
        public async Task TwoStage_ProbeFails_SingleUploadNoAnchor()
        {
            var fake = new FakeBlogClient();
            var undecodable = new PublishImageResizer(
                probeNaturalSize: _ => null,
                resize: (_, w, h) => new byte[] { 1 });
            string html = "<img src=\"file:///x/photo.png\" width=\"320\" height=\"240\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: undecodable);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1),
                "an image whose natural size can't be probed publishes as-is");
            Assert.That(result, Does.Not.Contain("<a "));
        }

        [Test]
        public async Task TwoStage_DuplicateImageSameDims_UploadsOnceEach_BothWrapped()
        {
            var fake = new FakeBlogClient();
            string html =
                "<img src=\"file:///x/photo.png\" width=\"320\" height=\"240\"><hr>" +
                "<img src=\"file:///x/photo.png\" width=\"320\" height=\"240\" alt=\"again\">";

            string result = await ImagePublisher.RewriteInlineImagesAsync(
                fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: FakeResizer());

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(2),
                "same image at the same display size: original once + resized once");
            int srcCount = result.Split(
                new[] { "src=\"https://cdn.example.com/uploads/photo_320x240.png\"" },
                StringSplitOptions.None).Length - 1;
            Assert.That(srcCount, Is.EqualTo(2));
            int anchorCount = result.Split(new[] { "<a " }, StringSplitOptions.None).Length - 1;
            Assert.That(anchorCount, Is.EqualTo(2), "each occurrence gets its own click-through anchor");
            Assert.That(result, Does.Contain("alt=\"again\""));
        }

        [Test]
        public void TwoStage_ResizeThrows_Aborts()
        {
            var fake = new FakeBlogClient();
            var broken = new PublishImageResizer(
                probeNaturalSize: _ => (800, 600),
                resize: (_, w, h) => throw new InvalidOperationException("boom"));
            string html = "<img src=\"file:///x/photo.png\" width=\"320\" height=\"240\">";

            var ex = Assert.ThrowsAsync<BlogClientPublishException>(
                async () => await ImagePublisher.RewriteInlineImagesAsync(
                    fake, "blog-1", html, readLocalFile: _ => PngBytes, resizer: broken));
            Assert.That(ex.Message, Does.Contain("photo.png"));
        }

        // ---- End-to-end with the real SkiaSharp seam the shell wires ----

        private static byte[] CreatePng(int width, int height)
        {
            using var bitmap = new SKBitmap(width, height);
            bitmap.Erase(new SKColor(200, 30, 30));
            using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        [Test]
        public async Task TwoStage_RealSkiaSeam_ThroughPublishPipeline()
        {
            byte[] bigPng = CreatePng(800, 600);
            var fake = new FakeBlogClient();
            // PublishOrEditAsync reads file:// images from disk, so use a real file.
            string dir = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), "OLWTwoStageTests", Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(dir);
            try
            {
                string path = System.IO.Path.Combine(dir, "big.png");
                System.IO.File.WriteAllBytes(path, bigPng);
                string fileUri = new Uri(path).AbsoluteUri;
                // The shape applyImageAttrs produces: attribute + matching inline style.
                string html = $"<p>Shot:</p><img src=\"{fileUri}\" width=\"320\" height=\"240\" " +
                              "style=\"width: 320px; height: 240px;\">";

                await EditorContentPublisher.PublishOrEditAsync(
                    fake, "blog-1", existingPostId: null, "Post", html, publish: true,
                    categories: Array.Empty<string>(),
                    imageResizer: PublishImageResizerFactory.Create());

                Assert.That(fake.NewMediaObjectCount, Is.EqualTo(2));
                Assert.That(fake.MediaUploads.Select(u => u.FileName),
                    Is.EqualTo(new[] { "big.png", "big_320x240.png" }));
                Assert.That(fake.MediaUploads[0].Bits, Is.EqualTo(bigPng), "original bytes unchanged");

                byte[] resized = fake.MediaUploads[1].Bits;
                Assert.That(ImageEditorService.TryGetDimensions(resized, out int rw, out int rh), Is.True);
                Assert.That(rw, Is.EqualTo(320));
                Assert.That(rh, Is.EqualTo(240));

                Assert.That(fake.LastPost.MainContents, Does.Contain(
                    "<a href=\"https://cdn.example.com/uploads/big.png\">" +
                    "<img src=\"https://cdn.example.com/uploads/big_320x240.png\""));
                Assert.That(fake.LastPost.MainContents, Does.Not.Contain("file://"));
            }
            finally
            {
                try { if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }
    }
}
