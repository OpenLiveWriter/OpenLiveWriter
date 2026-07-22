// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group U — P1-9-lite: publish date. The optional date flows Post Properties
    /// dialog → <see cref="PostDocument.PublishDateUtc"/> → <see cref="BlogPost.DateCreatedUtc"/>
    /// → the MetaWeblog <c>dateCreated</c> member (<c>dateTime.iso8601</c>) on both
    /// the post and page structs, only when set (null = publish immediately, member
    /// omitted). Also covers the draft round-trip and the dialog's OK enable rule.
    /// </summary>
    [TestFixture]
    [Category("GroupU")]
    public class GroupU_PublishDateTests
    {
        private static readonly DateTime SampleUtc = new DateTime(2026, 3, 10, 14, 22, 31, DateTimeKind.Utc);

        private static MetaWeblogXmlRpcClient NewClient() =>
            new MetaWeblogXmlRpcClient("http://example.test/xmlrpc", "user", "pass");

        private static string StructMember(string methodCallXml, string name, int paramIndex = 4)
        {
            var doc = new XmlDocument();
            doc.LoadXml(methodCallXml);
            XmlNode member = doc.SelectSingleNode(
                $"/methodCall/params/param[{paramIndex}]/value/struct/member[name='{name}']/value/dateTime.iso8601");
            return member?.InnerText;
        }

        // ---- dateCreated on the wire ----

        [Test]
        public void PostStruct_IncludesDateCreated_WhenSet()
        {
            var post = new BlogPost { Title = "T", DateCreatedUtc = SampleUtc };
            string xml = NewClient().BuildNewPostXml("blog-1", post, publish: true);

            Assert.That(StructMember(xml, "dateCreated"), Is.EqualTo("20260310T14:22:31"));
        }

        [Test]
        public void PostStruct_OmitsDateCreated_WhenNotSet()
        {
            var post = new BlogPost { Title = "T" };
            string xml = NewClient().BuildNewPostXml("blog-1", post, publish: true);

            Assert.That(xml, Does.Not.Contain("dateCreated"));
        }

        [Test]
        public void PageStruct_IncludesDateCreated_WhenSet()
        {
            var post = new BlogPost { Title = "T", IsPage = true, DateCreatedUtc = SampleUtc };
            string xml = NewClient().BuildNewPageXml("blog-1", post, publish: true);

            Assert.That(StructMember(xml, "dateCreated"), Is.EqualTo("20260310T14:22:31"));
        }

        [Test]
        public void PageStruct_OmitsDateCreated_WhenNotSet()
        {
            var post = new BlogPost { Title = "T", IsPage = true };
            string xml = NewClient().BuildNewPageXml("blog-1", post, publish: true);

            Assert.That(xml, Does.Not.Contain("dateCreated"));
        }

        // ---- publisher pass-through ----

        [Test]
        public async Task PublishOrEdit_CarriesPublishDate_ToPost()
        {
            var fake = new FakeBlogClient();
            await EditorContentPublisher.PublishOrEditAsync(
                fake, "blog-1", existingPostId: null, "T", "<p>Body</p>", publish: true,
                categories: Enumerable.Empty<string>(), publishDateUtc: SampleUtc);

            Assert.That(fake.NewPostCount, Is.EqualTo(1));
            Assert.That(fake.LastPost.DateCreatedUtc, Is.EqualTo(SampleUtc));
        }

        [Test]
        public async Task PublishOrEdit_CarriesPublishDate_ToPage()
        {
            var fake = new FakeBlogClient();
            await EditorContentPublisher.PublishOrEditAsync(
                fake, "blog-1", existingPostId: null, "T", "<p>Body</p>", publish: true,
                categories: Enumerable.Empty<string>(), isPage: true, publishDateUtc: SampleUtc);

            Assert.That(fake.NewPageCount, Is.EqualTo(1));
            Assert.That(fake.LastPost.DateCreatedUtc, Is.EqualTo(SampleUtc));
        }

        [Test]
        public async Task PublishOrEdit_NoPublishDate_LeavesDateCreatedNull()
        {
            var fake = new FakeBlogClient();
            await EditorContentPublisher.PublishAsync(fake, "blog-1", "T", "<p>Body</p>", publish: true);

            Assert.That(fake.LastPost.DateCreatedUtc, Is.Null);
        }

        // ---- document mapping + draft round-trip ----

        [Test]
        public void ToBlogPost_MapsPublishDateUtc()
        {
            var doc = new PostDocument { Title = "T", PublishDateUtc = SampleUtc };
            Assert.That(doc.ToBlogPost().DateCreatedUtc, Is.EqualTo(SampleUtc));

            var unset = new PostDocument { Title = "T" };
            Assert.That(unset.ToBlogPost().DateCreatedUtc, Is.Null);
        }

        [Test]
        public void DraftRoundTrip_PersistsPublishDateUtc()
        {
            string dir = Path.Combine(Path.GetTempPath(), "OLWPublishDate", Guid.NewGuid().ToString("N"));
            try
            {
                var session = new DraftSession(new FileDraftStore(dir));
                session.UpdateTitle("Scheduled post");
                session.Current.PublishDateUtc = SampleUtc;
                PostDocument saved = session.Save();

                var reopened = new DraftSession(new FileDraftStore(dir));
                Assert.That(reopened.Open(saved.Id), Is.True);
                Assert.That(reopened.Current.PublishDateUtc, Is.EqualTo(SampleUtc));
            }
            finally
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }

        // ---- dialog date/time parsing (pure) ----

        [TestCase("13:30", 13, 30)]
        [TestCase("7:05", 7, 5)]
        [TestCase("00:00", 0, 0)]
        [TestCase("23:59", 23, 59)]
        public void TryParseTimeOfDay_ValidClockTimes(string text, int hour, int minute)
        {
            Assert.That(PostPropertiesDialog.TryParseTimeOfDay(text, out int h, out int m), Is.True);
            Assert.That(h, Is.EqualTo(hour));
            Assert.That(m, Is.EqualTo(minute));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("1330")]
        [TestCase("13")]
        [TestCase("24:00")]
        [TestCase("25:00")]   // a plain TimeSpan parse would wrap this to the next day
        [TestCase("13:60")]
        [TestCase("-1:30")]
        public void TryParseTimeOfDay_RejectsOutOfRange(string text)
        {
            Assert.That(PostPropertiesDialog.TryParseTimeOfDay(text, out _, out _), Is.False);
        }

        [Test]
        public void CombineToUtc_CombinesLocalDateAndTime()
        {
            var date = new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero);
            DateTime? utc = PostPropertiesDialog.CombineToUtc(date, "13:30");

            Assert.That(utc, Is.Not.Null);
            DateTime roundTrip = utc.Value.ToLocalTime();
            Assert.That(roundTrip.Year, Is.EqualTo(2026));
            Assert.That(roundTrip.Month, Is.EqualTo(3));
            Assert.That(roundTrip.Day, Is.EqualTo(10));
            Assert.That(roundTrip.Hour, Is.EqualTo(13));
            Assert.That(roundTrip.Minute, Is.EqualTo(30));
        }

        [Test]
        public void CombineToUtc_NullDateOrBadTime_YieldsNull()
        {
            Assert.That(PostPropertiesDialog.CombineToUtc(null, "13:30"), Is.Null);
            Assert.That(PostPropertiesDialog.CombineToUtc(
                new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero), "nope"), Is.Null);
        }

        // ---- dialog OK enable rule (headless) ----

        [AvaloniaTest]
        public void PostPropertiesDialog_OkRequiresValidTime_WhenScheduling()
        {
            var dialog = new PostPropertiesDialog();
            var radios = dialog.GetLogicalDescendants().OfType<RadioButton>().ToList();
            RadioButton schedule = radios.First(r => (r.Content as string).Contains("Set publish date"));
            TextBox timeBox = dialog.GetLogicalDescendants().OfType<TextBox>().First();
            Button ok = dialog.GetLogicalDescendants().OfType<Button>()
                .First(b => (b.Content as string) == "OK");

            // Immediate (default): always OK.
            Assert.That(ok.IsEnabled, Is.True);

            schedule.IsChecked = true;
            Assert.That(ok.IsEnabled, Is.True, "prefilled time should be valid");

            timeBox.Text = "25:00";
            Assert.That(ok.IsEnabled, Is.False, "out-of-range time must block OK");

            timeBox.Text = "13:30";
            Assert.That(ok.IsEnabled, Is.True);
        }
    }
}
