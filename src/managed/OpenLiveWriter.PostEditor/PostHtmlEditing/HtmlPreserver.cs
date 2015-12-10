// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// The WYSIWYG editor mangles some types of HTML. When we detect
    /// such chunks of HTML, we wrap them in a <span class="wlWriterPreserve">...</span>
    /// and save the original contents for later. Whenever pulling data
    /// out of the WYSIWYG editor, we go back and restore those spans to
    /// their original state.
    /// </summary>
    public class HtmlPreserver
    {
        public const string PRESERVE_CLASS = "wlWriterPreserve";

        private readonly Dictionary<string, string> preserved = new Dictionary<string, string>();

        public HtmlPreserver()
        {
        }

        public void Reset()
        {
            preserved.Clear();
        }

        public string ScanAndPreserve(string html)
        {
            StringBuilder sb = new StringBuilder(html.Length);
            SimpleHtmlParser p = new SimpleHtmlParser(html);
            Element e;
            while (null != (e = p.Next()))
            {
                if (!(e is BeginTag))
                {
                    sb.Append(html, e.Offset, e.Length);
                    continue;
                }

                BeginTag bt = (BeginTag)e;

                if (bt.NameEquals("div"))
                {
                    switch (bt.GetAttributeValue("class"))
                    {
                        case ContentSourceManager.EDITABLE_SMART_CONTENT:
                        case ContentSourceManager.SMART_CONTENT:
                            sb.Append(html, e.Offset, e.Length);
                            sb.Append(p.CollectHtmlUntil("div"));
                            sb.Append("</div>");
                            continue;
                    }
                }

                if (!(bt.NameEquals("object")
                    || bt.NameEquals("embed")
                    || bt.NameEquals("noembed")
                    || bt.NameEquals("script")))
                {
                    sb.Append(html, e.Offset, e.Length);
                    continue;
                }
                else
                {
                    string collected = p.CollectHtmlUntil(bt.Name);
                    string preserve = bt.RawText + collected + "</" + bt.Name + ">";

                    string preserveId = Guid.NewGuid().ToString("N");
                    preserved[preserveId] = preserve;

                    sb.AppendFormat("<span id=\"preserve{0}\" class=\"{1}\">", preserveId, PRESERVE_CLASS);
                    sb.Append(preserve);
                    sb.Append("</span>");
                }
            }
            return sb.ToString();
        }

        public string RestorePreserved(string html)
        {
            StringBuilder sb = new StringBuilder();
            HtmlExtractor ex = new HtmlExtractor(html);
            int pos = 0;
            while (ex.Seek("<span class='" + PRESERVE_CLASS + "'>").Success)
            {
                sb.Append(html, pos, ex.Element.Offset - pos);
                pos = ex.Element.Offset;
                BeginTag bt = (BeginTag)ex.Element;
                string elementId = bt.GetAttributeValue("id");
                Match m = Regex.Match(elementId ?? "", @"^preserve([a-zA-Z0-9]+)$");
                if (m.Success)
                {
                    string preserveId = m.Groups[1].Value;
                    string preservedValue;
                    if (preserved.TryGetValue(preserveId, out preservedValue))
                    {
                        sb.Append(preservedValue);
                        ex.CollectTextUntil("span");
                        if (ex.Element == null)
                            pos = html.Length;
                        else
                            pos = ex.Parser.Position;
                    }
                }
            }
            sb.Append(html, pos, html.Length - pos);
            return sb.ToString();
        }
    }
}
