// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text;
using System.Text.RegularExpressions;
using mshtml;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices.HTML
{
    /*
        HTML enters the PostEditor "from the wild" in 4 ways:

        - HTML marshalling (HtmlHandler.DoInsertData). This entry point calls the HtmlGenerationService
          CleanupHtml method the PostEditor implementation of which calls PostEditorHtmlCleaner.CleanupHtml

        - LC marshalling (LiveClipboardHtmlFormatHandler.DoInsertData). This entry point calls
          PostEditorHtmlCleaner.RemoveScripts to remove only scripts, the assumption being that
          LC presentations only need security transformations not formatting transformations.

        - Plugins can insert HTML in two places. Simple content sources (ultimately) go through
          BlogPostHtmlEditor.InsertHtml, which ultimately calls HtmlEditorControl.InsertContent.
          Smart content sources call SmartContentInsertionHelper to do their insertion. Both of
          these paths allow "raw" access to HTML insertion. We will ultimately need to provide a
          service to plugins to do security and formatting oriented transformations of HTML
          that they retreive "from the wild".
    */

    public class HtmlCleaner
    {

        public static string RemoveScripts(string html)
        {
            return UnsafeHtmlFragmentHelper.SterilizeHtml(html, UnsafeHtmlFragmentHelper.Flag.RemoveDocumentTags | UnsafeHtmlFragmentHelper.Flag.RemoveScriptTags | UnsafeHtmlFragmentHelper.Flag.RemoveScriptAttributes);
        }

        /// <summary>
        /// Standard HTML cleanup for inbound HTML to the post editor
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string CleanupHtml(string html, string baseUrl, bool preserveImages, bool preserveTables)
        {
            return CleanupHtml(html, baseUrl, preserveImages, false, preserveTables);
        }

        public static string CleanupHtml(string html, string baseUrl, bool preserveImages, bool strip, bool preserveTables)
        {
            // sterilize the HTML
            html = UnsafeHtmlFragmentHelper.SterilizeHtml(html, UnsafeHtmlFragmentHelper.Flag.AllFlags ^ UnsafeHtmlFragmentHelper.Flag.RemoveStyles);

            html = StripNamespacedTags(html);

            // get the text into a DOM to ensure tags are balanced
            IHTMLDocument2 document = HTMLDocumentHelper.StringToHTMLDoc(html, null, false);

            if (document.body == null)
                return string.Empty;

            // thin it
            if (preserveTables)
                html = LightWeightHTMLThinner2.Thin(document.body.innerHTML, preserveImages, strip, LightWeightHTMLThinner2.PreserveTables);
            else
                html = LightWeightHTMLThinner2.Thin(document.body.innerHTML, preserveImages, strip);
            html = LightWeightHTMLUrlToAbsolute.ConvertToAbsolute(html, baseUrl, false, true, true);

            // balance it
            string balancedHtml = HTMLBalancer.Balance(html);

            // return
            return balancedHtml;
        }

        public static string PreserveFormatting(string html, string baseUrl)
        {
            UnsafeHtmlFragmentHelper.Flag flags = UnsafeHtmlFragmentHelper.Flag.RemoveDocumentTags |
                                                  UnsafeHtmlFragmentHelper.Flag.RemoveScriptTags |
                                                  UnsafeHtmlFragmentHelper.Flag.RemoveScriptAttributes |
                                                  UnsafeHtmlFragmentHelper.Flag.RemoveMarkupDirectives |
                                                  UnsafeHtmlFragmentHelper.Flag.RemoveComments;
            html = UnsafeHtmlFragmentHelper.SterilizeHtml(html, flags);
            html = LightWeightHTMLUrlToAbsolute.ConvertToAbsolute(html, baseUrl, false, true, true);
            return html;
        }

        /// <summary>
        /// Namespaced tags come with Office 2007 clipboard data and result in weird
        /// namespace declarations being inserted as text into the DOM. (Bug 303784)
        /// </summary>
        private static string StripNamespacedTags(string html)
        {
            StringBuilder output = new StringBuilder(html.Length);
            SimpleHtmlParser parser = new SimpleHtmlParser(html);
            for (Element el; null != (el = parser.Next());)
            {
                if (el is Tag && ((Tag)el).Name.IndexOf(':') >= 0)
                    continue;
                output.Append(el.RawText);
            }
            html = output.ToString();
            return html;
        }

        /// <summary>
        /// Namespaced tags come with Office 2007 clipboard data and result in weird
        /// namespace declarations being inserted as text into the DOM.
        /// </summary>
        public static string StripNamespacedTagsAndCommentsAndMarkupDirectives(string html)
        {
            StringBuilder output = new StringBuilder(html.Length);
            SimpleHtmlParser parser = new SimpleHtmlParser(html);
            for (Element el; null != (el = parser.Next());)
            {
                if (el is Tag && ((Tag)el).Name.IndexOf(':') >= 0)
                    continue;
                if (el is Comment)
                    continue;
                if (el is MarkupDirective)
                    continue;
                if (el is BeginTag)
                {
                    foreach (Attr attr in ((BeginTag)el).Attributes)
                    {
                        if (ILLEGAL_ATTR_REGEX.IsMatch(attr.Name))
                            ((BeginTag)el).RemoveAttribute(attr.Name);
                    }
                }
                output.Append(el.ToString());
            }
            html = output.ToString();
            return html;
        }
        private static Regex ILLEGAL_ATTR_REGEX = new Regex("^(([^x][^m][^l].*|.?.?):.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase); //Matches all strings with a colon that do not start with "xml"

    }
}
