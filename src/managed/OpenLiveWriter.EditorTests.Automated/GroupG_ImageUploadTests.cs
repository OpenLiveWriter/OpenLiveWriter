// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group G — image upload-on-publish (<c>metaWeblog.newMediaObject</c>). Exercises the
    /// cross-platform <see cref="ImagePublisher"/> scan/upload/rewrite/dedup and its
    /// integration into the publish path (<see cref="EditorContentPublisher"/> /
    /// <see cref="BlogAccountService"/>). A <see cref="FakeBlogClient"/> records every
    /// upload and returns fake hosted URLs so everything runs offline.
    /// </summary>
    [TestFixture]
    [Category("GroupG")]
    public class GroupG_ImageUploadTests
    {
        // 1x1 transparent PNG.
        private const string PngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
        // A tiny distinct GIF payload (valid base64 bytes, content not a real image but decodes fine).
        private const string GifBase64 = "R0lGODlhAQABAAAAACwAAAAAAQABAAA=";

        private static string ImgTag(string mime, string base64) =>
            $"<img src=\"data:{mime};base64,{base64}\">";

        // ---- Pure scanning ----

        [Test]
        public void FindInlineImages_NoImages_ReturnsEmpty()
        {
            var found = ImagePublisher.FindInlineImages("<p>Just text, <a href=\"https://x/y.png\">a link</a></p>");
            Assert.That(found, Is.Empty);
        }

        [Test]
        public void FindInlineImages_ParsesMimeAndDecodesBytes()
        {
            string html = "<p>Look:</p>" + ImgTag("image/png", PngBase64);
            var found = ImagePublisher.FindInlineImages(html);

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].MimeType, Is.EqualTo("image/png"));
            Assert.That(found[0].FileExtension, Is.EqualTo("png"));
            Assert.That(found[0].DecodedBytes, Is.EqualTo(Convert.FromBase64String(PngBase64)));
        }

        [Test]
        public void FindInlineImages_DedupsIdenticalDataUris()
        {
            string html = ImgTag("image/png", PngBase64) + ImgTag("image/png", PngBase64);
            var found = ImagePublisher.FindInlineImages(html);
            Assert.That(found.Count, Is.EqualTo(1), "identical data URIs collapse to one entry");
        }

        [Test]
        public void FindInlineImages_JpegMapsToJpgExtension()
        {
            var found = ImagePublisher.FindInlineImages(ImgTag("image/jpeg", GifBase64));
            Assert.That(found[0].FileExtension, Is.EqualTo("jpg"));
        }

        // ---- Rewrite + upload ----

        [Test]
        public void Rewrite_NoImages_IsNoOp_AndNoUploads()
        {
            var fake = new FakeBlogClient();
            string html = "<p>No images here</p>";
            string result = ImagePublisher.RewriteInlineImages(fake, "blog-1", html);

            Assert.That(result, Is.EqualTo(html));
            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(0));
        }

        [Test]
        public void Rewrite_UploadsAndReplacesDataUriWithHostedUrl()
        {
            var fake = new FakeBlogClient();
            string html = "<p>Pic:</p>" + ImgTag("image/png", PngBase64);

            string result = ImagePublisher.RewriteInlineImages(fake, "blog-9", html);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1));
            Assert.That(result, Does.Not.Contain("data:image"), "the data URI must be gone");
            Assert.That(result, Does.Contain("https://cdn.example.com/uploads/image1.png"));

            var upload = fake.MediaUploads.Single();
            Assert.That(upload.BlogId, Is.EqualTo("blog-9"));
            Assert.That(upload.FileName, Is.EqualTo("image1.png"));
            Assert.That(upload.MimeType, Is.EqualTo("image/png"));
            Assert.That(upload.Bits, Is.EqualTo(Convert.FromBase64String(PngBase64)));
        }

        [Test]
        public void Rewrite_DuplicateImage_UploadsOnce_ReplacesBothOccurrences()
        {
            var fake = new FakeBlogClient();
            string html = ImgTag("image/png", PngBase64) + "<hr>" + ImgTag("image/png", PngBase64);

            string result = ImagePublisher.RewriteInlineImages(fake, "blog-1", html);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1), "identical images upload once");
            int occurrences = result.Split(new[] { "https://cdn.example.com/uploads/image1.png" }, StringSplitOptions.None).Length - 1;
            Assert.That(occurrences, Is.EqualTo(2), "both <img> refs point at the single hosted URL");
            Assert.That(result, Does.Not.Contain("data:image"));
        }

        [Test]
        public void Rewrite_MultipleDistinctImages_NumberedAndAllReplaced()
        {
            var fake = new FakeBlogClient();
            string html = ImgTag("image/png", PngBase64) + ImgTag("image/gif", GifBase64);

            string result = ImagePublisher.RewriteInlineImages(fake, "blog-1", html);

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(2));
            Assert.That(fake.MediaUploads.Select(u => u.FileName),
                Is.EqualTo(new[] { "image1.png", "image2.gif" }));
            Assert.That(result, Does.Contain("uploads/image1.png"));
            Assert.That(result, Does.Contain("uploads/image2.gif"));
            Assert.That(result, Does.Not.Contain("data:image"));
        }

        [Test]
        public void Rewrite_UploadFailure_Throws_AndDoesNotReturnBrokenHtml()
        {
            var fake = new FakeBlogClient { FailUploadForFileName = "image1.png" };
            string html = ImgTag("image/png", PngBase64);

            var ex = Assert.Throws<BlogClientPublishException>(
                () => ImagePublisher.RewriteInlineImages(fake, "blog-1", html));
            Assert.That(ex.Message, Does.Contain("image1.png"));
        }

        // ---- Integration through the publish path ----

        [Test]
        public void Publish_ThroughEditorContentPublisher_HostsImagesBeforeNewPost()
        {
            var fake = new FakeBlogClient();
            string html = "<p>Body</p>" + ImgTag("image/png", PngBase64);

            EditorContentPublisher.Publish(fake, "blog-1", "Post", html, publish: true, "News");

            Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1));
            Assert.That(fake.NewPostCount, Is.EqualTo(1));
            // The post body sent to the server must reference the hosted URL, not base64.
            Assert.That(fake.LastPost.MainContents, Does.Contain("uploads/image1.png"));
            Assert.That(fake.LastPost.MainContents, Does.Not.Contain("data:image"));
        }

        [Test]
        public void Publish_ThroughBlogAccountService_HostsImagesBeforeNewPost()
        {
            string dir = Path.Combine(Path.GetTempPath(), "OLWImgTests", Guid.NewGuid().ToString("N"));
            try
            {
                var fake = new FakeBlogClient();
                var service = new BlogAccountService(
                    new FileAccountStore(dir), new InMemoryCredentialStore(), (a, p) => fake);
                var account = new BlogAccount
                {
                    DisplayName = "Blog",
                    ApiEndpointUrl = "https://blog.example.com/xmlrpc.php",
                    BlogId = "blog-7",
                    Username = "author"
                };
                BlogAccount saved = service.SaveAccount(account, "pw");
                service.SetCurrentAccount(saved.Id);

                var doc = new PostDocument { Title = "With image" };
                string html = "<p>Hello</p>" + ImgTag("image/png", PngBase64);

                PublishOutcome outcome = service.Publish(doc, html, publish: true);

                Assert.That(outcome.Succeeded, Is.True);
                Assert.That(fake.NewMediaObjectCount, Is.EqualTo(1));
                Assert.That(fake.MediaUploads.Single().BlogId, Is.EqualTo("blog-7"));
                Assert.That(fake.LastPost.MainContents, Does.Contain("uploads/image1.png"));
                Assert.That(fake.LastPost.MainContents, Does.Not.Contain("data:image"));
            }
            finally
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }

        [Test]
        public void MetaWeblogClient_BuildsNewMediaObjectStruct_WithNameTypeBits()
        {
            // Assert the REAL XML-RPC payload shape for newMediaObject (offline build).
            byte[] bytes = Convert.FromBase64String(PngBase64);
            string xml = MetaWeblogXmlRpcClient.BuildMethodCallXml(
                "metaWeblog.newMediaObject",
                new OpenLiveWriter.Publishing.Xml.XmlRpcString("blog-1"),
                new OpenLiveWriter.Publishing.Xml.XmlRpcString("user"),
                new OpenLiveWriter.Publishing.Xml.XmlRpcString("pass", true),
                new OpenLiveWriter.Publishing.Xml.XmlRpcStruct(new[]
                {
                    new OpenLiveWriter.Publishing.Xml.XmlRpcMember("name", new OpenLiveWriter.Publishing.Xml.XmlRpcString("image1.png")),
                    new OpenLiveWriter.Publishing.Xml.XmlRpcMember("type", new OpenLiveWriter.Publishing.Xml.XmlRpcString("image/png")),
                    new OpenLiveWriter.Publishing.Xml.XmlRpcMember("bits", new OpenLiveWriter.Publishing.Xml.XmlRpcBase64(bytes)),
                }));

            var docXml = new System.Xml.XmlDocument();
            docXml.LoadXml(xml);
            var mediaStruct = docXml.SelectSingleNode("/methodCall/params/param[4]/value/struct");
            Assert.That(mediaStruct, Is.Not.Null);
            Assert.That(mediaStruct.SelectSingleNode("member[name='name']/value/string")?.InnerText, Is.EqualTo("image1.png"));
            Assert.That(mediaStruct.SelectSingleNode("member[name='type']/value/string")?.InnerText, Is.EqualTo("image/png"));
            string base64 = mediaStruct.SelectSingleNode("member[name='bits']/value/base64")?.InnerText;
            Assert.That(base64, Is.Not.Null.And.Not.Empty);
            Assert.That(Convert.FromBase64String(base64.Trim()), Is.EqualTo(bytes));
        }
    }
}
