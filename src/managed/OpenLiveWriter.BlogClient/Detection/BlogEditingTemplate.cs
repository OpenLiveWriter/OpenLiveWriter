// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Detection
{
    public class BlogEditingTemplate
    {
        public static bool ValidateTemplate(string template)
        {
            return ValidateTemplate(template, true);
        }

        public static bool ValidateTemplate(string template, bool containsTitle)
        {
            return (template != null && (containsTitle == (template.IndexOf(POST_TITLE_MARKER, StringComparison.OrdinalIgnoreCase) != -1)) && template.IndexOf(POST_BODY_MARKER, StringComparison.OrdinalIgnoreCase) != -1);
        }

        public BlogEditingTemplate(bool containsTitle)
            : this(GetDefaultTemplateHtml(containsTitle), containsTitle)
        {
        }

        public BlogEditingTemplate(string template)
            : this(template, true)
        {
        }

        public readonly bool ContainsTitle;

        public BlogEditingTemplate(string template, bool containsTitle)
        {
            ContainsTitle = containsTitle;
            if (!ValidateTemplate(template, ContainsTitle))
            {
                Trace.WriteLine("Invalid editing template detected");
                template = GetDefaultTemplateHtml(containsTitle);
            }

            //sandbox the template in the Internet Security zone
            template = HTMLDocumentHelper.AddMarkOfTheWeb(template, "about:internet");
            Template = template;
        }
        public readonly string Template;

        public string ApplyTemplateToPostHtml(string titleText, string titleHtml, string postBody)
        {
            string templateHtml = Template.Replace(POST_TITLE_MARKER, titleHtml).Replace(POST_BODY_MARKER, postBody).Replace(POST_TITLE_READONLY_MARKER, titleText);
            return templateHtml;
        }
        public static readonly string POST_TITLE_MARKER = "{post-title}";
        public static readonly string POST_TITLE_READONLY_MARKER = "{post-titleReadOnly}";
        public static readonly string POST_BODY_MARKER = "{post-body}";

        public static string GetDefaultTemplateHtml(bool containsTitle)
        {
            return GetDefaultTemplateHtml(false, containsTitle);
        }

        public static string GetDefaultTemplateHtml(bool forceRTL, bool containsTitle)
        {
            string path = Path.Combine(ApplicationEnvironment.InstallationDirectory, @"template");

            //read in the html
            string htmlFile = Path.Combine(path, "default.htm");
            string template;
            using (StreamReader reader = new StreamReader(htmlFile, Encoding.UTF8))
                template = reader.ReadToEnd();
            if (forceRTL)
                template = AddRTL(template);

            string cssFile = Path.Combine(path, "defaultstyle.css");

            string css;
            using (StreamReader cssReader = new StreamReader(cssFile, Encoding.UTF8))
                css = cssReader.ReadToEnd();

            css = css.Replace("{body-font}", Res.Get(StringId.DefaultTemplateBodyFont));
            css = css.Replace("{title-font}", Res.Get(StringId.DefaultTemplateTitleFont));

            string cssPath = TempFileManager.Instance.CreateTempFile("default.css");
            using (StreamWriter writer = new StreamWriter(cssPath, true, Encoding.UTF8))
                writer.Write(css);

            return String.Format(CultureInfo.InvariantCulture, defaultDocType + "\r\n" + template, cssPath, containsTitle ? POST_TITLE_MARKER : "", POST_BODY_MARKER);
        }

        internal static string AddRTL(string html)
        {
            Debug.Assert(html.Contains("<HTML>"), "normal template lacks <HTML> tag in uppercase");
            return html.Replace("<HTML>", "<HTML dir=\"rtl\">");
        }

        public static string GetBlogTemplateDir(string blogId)
        {
            if (blogId == null || blogId == String.Empty)
                throw new ArgumentException("Must specify a blogId for GetBlogTemplateDir", "blogId");

            string blogTemplateDir = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "blogtemplates");
            blogTemplateDir = Path.Combine(blogTemplateDir, blogId);
            return blogTemplateDir;
        }

        private static string defaultDocType = "";

    }
}
