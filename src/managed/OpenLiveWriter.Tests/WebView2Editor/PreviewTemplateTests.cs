// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using System.Reflection;
using AngleSharp.Dom;
using NUnit.Framework;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using StringAssert = NUnit.Framework.Legacy.StringAssert;
using AngleSharpHtmlParser = AngleSharp.Html.Parser.HtmlParser;

namespace OpenLiveWriter.Tests.WebView2Editor
{
    /// <summary>
    /// Tests for the default blog editing template used to compose preview HTML.
    /// Honest scope: the WebView2 Preview mode currently ignores the blog template
    /// and the read-only flag (known gap), so these tests cover the template
    /// infrastructure (load, marker substitution, parseability) rather than a
    /// live preview render.
    /// </summary>
    [TestFixture]
    public class PreviewTemplateTests
    {
        private static readonly AngleSharpHtmlParser Parser = new AngleSharpHtmlParser();

        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            // GetDefaultTemplateHtml resolves template/default.htm relative to the
            // installation directory, which is the test output directory here.
            // Use a non-default product name: with the default product name
            // Initialize() throws when the profile has no Personal folder (e.g.
            // the SYSTEM account in a headless test session).
            if (ApplicationEnvironment.InstallationDirectory == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                ApplicationEnvironment.Initialize(assembly, Path.GetDirectoryName(assembly.Location),
                    "Software\\OpenLiveWriter.Tests", "Open Live Writer Tests");
            }
        }

        [Test]
        public void DefaultTemplate_LoadsAndContainsMarkers()
        {
            string template = BlogEditingTemplate.GetDefaultTemplateHtml(true);

            Assert.IsNotEmpty(template, "default editing template failed to load");
            StringAssert.Contains(BlogEditingTemplate.POST_TITLE_MARKER, template);
            StringAssert.Contains(BlogEditingTemplate.POST_BODY_MARKER, template);
            Assert.IsTrue(BlogEditingTemplate.ValidateTemplate(template), "default template failed validation");
        }

        [Test]
        public void ApplyTemplate_SubstitutesTitleAndBodyVerbatim()
        {
            string templateHtml = BlogEditingTemplate.GetDefaultTemplateHtml(true);
            var template = new BlogEditingTemplate(templateHtml, true);

            const string title = "My Preview Title";
            const string body = "<p>Hello <b>preview</b> world</p>";
            string preview = template.ApplyTemplateToPostHtml(title, title, body);

            StringAssert.Contains(title, preview, "title not substituted into preview HTML");
            StringAssert.Contains(body, preview, "body not substituted into preview HTML");
            Assert.IsFalse(preview.Contains(BlogEditingTemplate.POST_TITLE_MARKER), "title marker left behind");
            Assert.IsFalse(preview.Contains(BlogEditingTemplate.POST_BODY_MARKER), "body marker left behind");
        }

        [Test]
        public void PreviewHtml_ParsesWithTitleAndBodyIntact()
        {
            string templateHtml = BlogEditingTemplate.GetDefaultTemplateHtml(true);
            var template = new BlogEditingTemplate(templateHtml, true);

            const string title = "Parsing Check";
            const string body = "<p>Body <i>content</i> here</p>";
            string preview = template.ApplyTemplateToPostHtml(title, title, body);

            IDocument doc = Parser.ParseDocument(preview);
            IElement titleElement = doc.QuerySelector("div.title");
            IElement bodyElement = doc.QuerySelector("div.body");
            Assert.IsNotNull(titleElement, "preview document has no title element");
            Assert.IsNotNull(bodyElement, "preview document has no body element");
            StringAssert.Contains(title, titleElement.TextContent);
            Assert.IsNotNull(bodyElement.QuerySelector("p > i"), "body markup corrupted in preview document");
            Assert.AreEqual("content", bodyElement.QuerySelector("i").TextContent);
        }
    }
}
