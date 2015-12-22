// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Strips non-relevant line breaks from HTML
    /// </summary>
    public class HtmlLinebreakStripper : LightWeightHTMLDocumentIterator
    {
        StringBuilder htmlData;
        HtmlLinebreakStripper(string html) : base(html)
        {
            htmlData = new StringBuilder();
        }

        /// <summary>
        /// Strips non-relevant line breaks from a chunk of HTML
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string RemoveLinebreaks(string html)
        {
            HtmlLinebreakStripper converter = new HtmlLinebreakStripper(html);
            converter.Parse();
            return converter.GetHtml();
        }

        public string GetHtml()
        {
            return htmlData.ToString();
        }

        int preserveLinebreaksDepth;
        protected override void OnBeginTag(BeginTag tag)
        {
            if (IsPreserveWhitespaceTag(tag.Name))
                preserveLinebreaksDepth++;
            base.OnBeginTag(tag);
        }

        protected override void OnEndTag(EndTag tag)
        {
            if (preserveLinebreaksDepth > 0 && IsPreserveWhitespaceTag(tag.Name))
                preserveLinebreaksDepth++;
            base.OnEndTag(tag);
        }

        protected override void OnText(Text text)
        {
            if (preserveLinebreaksDepth == 0)
            {
                string cleanText = newLine.Replace(text.ToString(), " ");
                if (cleanText.Length != text.Length)
                    text = new Text(cleanText, 0, cleanText.Length);
            }
            base.OnText(text);
        }
        static Regex newLine = new Regex(@"\r?\n", RegexOptions.Compiled);

        public static bool IsPreserveWhitespaceTag(string tagName)
        {
            switch (tagName.ToLower(CultureInfo.InvariantCulture))
            {
                case "pre":
                case "xmp":
                    return true;
                default:
                    return false;
            }
        }

        protected override void DefaultAction(Element el)
        {
            htmlData.Append(el.ToString());
        }
    }
}
