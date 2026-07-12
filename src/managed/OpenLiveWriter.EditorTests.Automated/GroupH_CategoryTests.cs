// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.EditorTests.Automated.Publish;
using OpenLiveWriter.Publishing;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group H — blog categories. Covers the <c>metaWeblog.getCategories</c> response
    /// parsing (fixture XML), the category-selection merge logic, and that selected
    /// categories reach the real MetaWeblog newPost struct. All offline.
    /// </summary>
    [TestFixture]
    [Category("GroupH")]
    public class GroupH_CategoryTests
    {
        private static string CategoriesResponse(string membersXml) =>
            "<?xml version=\"1.0\"?><methodResponse><params><param><value><array><data>"
            + membersXml
            + "</data></array></value></param></params></methodResponse>";

        [Test]
        public void ParseCategories_TitleAndCategoryId()
        {
            string xml = CategoriesResponse(
                "<value><struct>"
                + "<member><name>categoryId</name><value><string>7</string></value></member>"
                + "<member><name>title</name><value><string>News</string></value></member>"
                + "</struct></value>"
                + "<value><struct>"
                + "<member><name>categoryId</name><value><string>9</string></value></member>"
                + "<member><name>title</name><value><string>Updates</string></value></member>"
                + "</struct></value>");

            var cats = MetaWeblogXmlRpcClient.ParseCategoriesResponse(xml);
            Assert.That(cats.Count, Is.EqualTo(2));
            Assert.That(cats.Select(c => c.Name), Is.EqualTo(new[] { "News", "Updates" }));
            Assert.That(cats[0].Id, Is.EqualTo("7"));
            Assert.That(cats[1].Id, Is.EqualTo("9"));
        }

        [Test]
        public void ParseCategories_DescriptionOnly_UsesDescriptionAsNameAndId()
        {
            string xml = CategoriesResponse(
                "<value><struct>"
                + "<member><name>description</name><value><string>Tech</string></value></member>"
                + "</struct></value>");

            var cats = MetaWeblogXmlRpcClient.ParseCategoriesResponse(xml);
            Assert.That(cats.Count, Is.EqualTo(1));
            Assert.That(cats[0].Name, Is.EqualTo("Tech"));
            Assert.That(cats[0].Id, Is.EqualTo("Tech"));
        }

        [Test]
        public void ParseCategories_CategoryNameFallback_AndParent()
        {
            string xml = CategoriesResponse(
                "<value><struct>"
                + "<member><name>categoryId</name><value><string>42</string></value></member>"
                + "<member><name>categoryName</name><value><string>Sub</string></value></member>"
                + "<member><name>parentId</name><value><string>7</string></value></member>"
                + "</struct></value>");

            var cats = MetaWeblogXmlRpcClient.ParseCategoriesResponse(xml);
            Assert.That(cats[0].Name, Is.EqualTo("Sub"));
            Assert.That(cats[0].Id, Is.EqualTo("42"));
            Assert.That(cats[0].Parent, Is.EqualTo("7"));
        }

        [Test]
        public void ParseCategories_NoCategories_ReturnsEmpty()
        {
            string xml = CategoriesResponse(string.Empty);
            var cats = MetaWeblogXmlRpcClient.ParseCategoriesResponse(xml);
            Assert.That(cats, Is.Empty);
        }

        [Test]
        public void ParseCategories_IndentedXml_TrimsNames()
        {
            // Servers that indent the payload must not leak whitespace into names/ids.
            string xml =
                "<?xml version=\"1.0\"?>\n<methodResponse>\n <params>\n  <param>\n   <value>\n    <array>\n"
                + "     <data>\n      <value>\n       <struct>\n"
                + "        <member><name>title</name><value><string>News</string></value></member>\n"
                + "       </struct>\n      </value>\n     </data>\n    </array>\n   </value>\n  </param>\n </params>\n</methodResponse>";
            var cats = MetaWeblogXmlRpcClient.ParseCategoriesResponse(xml);
            Assert.That(cats.Count, Is.EqualTo(1));
            Assert.That(cats[0].Name, Is.EqualTo("News"));
        }

        [Test]
        public void FakeClient_GetCategories_ReturnsConfigured()
        {
            var fake = new FakeBlogClient();
            fake.AvailableCategories.Add(new BlogPostCategory("1", "Alpha"));
            fake.AvailableCategories.Add(new BlogPostCategory("2", "Beta"));

            var cats = fake.GetCategories("blog-1");
            Assert.That(fake.GetCategoriesCount, Is.EqualTo(1));
            Assert.That(fake.LastGetCategoriesBlogId, Is.EqualTo("blog-1"));
            Assert.That(cats.Select(c => c.Name), Is.EqualTo(new[] { "Alpha", "Beta" }));
        }

        // ---- Selected categories reach the newPost struct ----

        [Test]
        public void SelectedCategories_ReachNewPostStruct()
        {
            var client = new MetaWeblogXmlRpcClient("http://example.test/xmlrpc", "user", "pass");
            BlogPost post = EditorContentPublisher.BuildPost(
                "Post", "<p>Body</p>", publish: true, "News", "Updates");

            string xml = client.BuildNewPostXml("blog-1", post, publish: true);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var names = doc
                .SelectNodes("/methodCall/params/param[4]/value/struct/member[name='categories']/value/array/data/value/string")
                .Cast<XmlNode>().Select(n => n.InnerText).ToArray();

            Assert.That(names, Is.EqualTo(new[] { "News", "Updates" }));
        }

        // ---- CategoryDialog merge logic (pure) ----

        [Test]
        public void MergeSelection_CombinesCheckedAndCustom_DedupsAndTrims()
        {
            var merged = CategoryDialog.MergeSelection(
                new[] { "News", "Updates" }, " Tech , News , ");

            Assert.That(merged, Is.EqualTo(new List<string> { "News", "Updates", "Tech" }));
        }

        [Test]
        public void MergeSelection_EmptyInputs_ReturnsEmpty()
        {
            var merged = CategoryDialog.MergeSelection(new string[0], "   ");
            Assert.That(merged, Is.Empty);
        }
    }
}
