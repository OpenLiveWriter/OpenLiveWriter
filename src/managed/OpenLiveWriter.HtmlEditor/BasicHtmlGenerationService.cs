// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Basic implementation of an IHtmlGenerationService
    /// </summary>
    public class BasicHtmlGenerationService : IHtmlGenerationService
    {
        private DefaultBlockElement _defaultBlockElement;

        public BasicHtmlGenerationService(DefaultBlockElement defaultBlockElement)
        {
            _defaultBlockElement = defaultBlockElement;
        }

        #region IHtmlGenerationService Members

        public virtual string GenerateHtmlFromFiles(string[] files)
        {
            StringBuilder html = new StringBuilder();
            foreach (string file in files)
            {
                if (html.Length > 0)
                    html.Append("\r\n");
                if (PathHelper.IsPathImage(file))
                {
                    html.AppendFormat("<img src=\"{0}\" />",
                        HtmlUtils.EscapeEntities(UrlHelper.CreateUrlFromPath(file)));
                }
                else
                {
                    html.AppendFormat("<a href=\"{0}\">{1}</a>",
                        HtmlUtils.EscapeEntities(UrlHelper.CreateUrlFromPath(file)),
                        HtmlUtils.EscapeEntities(Path.GetFileNameWithoutExtension(file)));
                }
            }
            return html.ToString();
        }

        public virtual string GenerateHtmlFromLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            StringBuilder link = new StringBuilder("<a href=\"{0}\"");
            url = HtmlUtils.EscapeEntities(url);
            linkText = TextHelper.GetHTMLFromText(linkText, false);
            linkTitle = HtmlUtils.EscapeEntities(linkTitle);
            rel = HtmlUtils.EscapeEntities(rel);

            if (newWindow)
            {
                link.Append(" target='_blank'");
            }
            if (String.Empty != linkTitle && null != linkTitle)
            {
                link.Append(" title=\"{2}\"");
            }
            if (String.Empty != rel && null != rel)
            {
                link.Append(" rel=\"{3}\"");
            }
            link.Append(">{1}</a>");
            return String.Format(CultureInfo.InvariantCulture, link.ToString(), url, linkText, linkTitle, rel);
        }

        public virtual string GenerateHtmlFromHtmlFragment(string html, string baseUrl)
        {
            if (baseUrl != null)
            {
                html = new RelativeUrlReplacer(html, baseUrl).DoReplace();
            }
            return html;
        }

        public virtual string CleanupHtml(string html, string baseUrl, HtmlCleanupRule cleanupRule)
        {
            return html;
        }

        public virtual string GenerateHtmlFromPlainText(string text)
        {
            return TextHelper.GetHTMLFromText(text, true, _defaultBlockElement);
        }

        #endregion

        /// <summary>
        /// Converts relative URLs in an HTML fragment to absolute URLs.
        /// </summary>
        public class RelativeUrlReplacer : LightWeightHTMLDocumentIterator
        {
            private string _baseUrl;
            private StringBuilder _htmlBuilder = new StringBuilder();

            public RelativeUrlReplacer(string html, string baseUrl) : base(html)
            {
                _baseUrl = baseUrl;
            }

            public string DoReplace()
            {
                Parse();
                string html = _htmlBuilder.ToString();
                _htmlBuilder = new StringBuilder();

                return html;
            }

            protected void Emit(string text)
            {
                _htmlBuilder.Append(text);
            }

            protected override void DefaultAction(Element el)
            {
                Emit(el.ToString());
            }

            protected override void OnBeginTag(BeginTag tag)
            {
                if (tag != null && LightWeightHTMLDocument.AllUrlElements.ContainsKey(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
                {
                    Attr attr = tag.GetAttribute((string)LightWeightHTMLDocument.AllUrlElements[tag.Name.ToUpper(CultureInfo.InvariantCulture)]);
                    if (attr != null)
                    {
                        string url = attr.Value;
                        if (!UrlHelper.IsUrl(url))
                            attr.Value = UrlHelper.EscapeRelativeURL(_baseUrl, url);
                    }
                }
                base.OnBeginTag(tag);
            }
        }
    }
}
