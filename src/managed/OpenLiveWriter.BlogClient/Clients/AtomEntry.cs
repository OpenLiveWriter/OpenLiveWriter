// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients
{
    internal class AtomEntry
    {
        private readonly AtomProtocolVersion _atomVer;
        private readonly XmlNamespaceManager _nsMgr;
        private readonly Uri _documentUri;
        private readonly XmlElement _entryNode;
        private readonly Namespace _atomNS;
        private readonly string _categoryScheme;

        public AtomEntry(AtomProtocolVersion atomVer, Namespace atomNS, string categoryScheme, XmlNamespaceManager nsMgr, Uri documentUri, XmlElement entryNode)
        {
            _atomVer = atomVer;
            _categoryScheme = categoryScheme;
            _nsMgr = nsMgr;
            _documentUri = documentUri;
            _entryNode = entryNode;
            _atomNS = atomNS;
        }

        public string Title
        {
            get
            {
                return GetTextNodePlaintext("atom:title", string.Empty);
            }
            set
            {
                PopulateElement(value, _atomNS, "title");
            }
        }

        public string Excerpt
        {
            get
            {
                return GetTextNodePlaintext("atom:summary", string.Empty);
            }
            set
            {
                PopulateElement(value, _atomNS, "summary");
            }
        }

        public string ContentHtml
        {
            get
            {
                XmlElement contentEl = GetElement("atom:content");
                if (contentEl != null)
                    return _atomVer.TextNodeToHtml(contentEl);
                else
                    return "";
            }
            set
            {
                RemoveNodes("content", _atomNS);
                XmlElement contentNode = _atomVer.HtmlToTextNode(_entryNode.OwnerDocument, value + " ");
                _entryNode.AppendChild(contentNode);
            }
        }

        public BlogPostCategory[] Categories
        {
            get
            {
                return _atomVer.ExtractCategories(_entryNode, _categoryScheme, _documentUri);
            }
        }

        public void ClearCategories()
        {
            _atomVer.RemoveAllCategories(_entryNode, _categoryScheme, _documentUri);
        }

        public void AddCategory(BlogPostCategory category)
        {
            XmlElement catEl = _atomVer.CreateCategoryElement(_entryNode.OwnerDocument, category.Id, _categoryScheme, category.Name);
            _entryNode.AppendChild(catEl);
        }

        private DateTime GetPublishDate(DateTime defaultValue)
        {
            XmlNode target = _entryNode.SelectSingleNode("atom:published", _nsMgr);
            if (target == null)
                return defaultValue;
            string val = target.InnerText;
            if (val == null)
                return defaultValue;
            val = val.Trim();
            if (val.Length == 0)
                return defaultValue;

            return ParseRfc3339(val);
        }

        public DateTime PublishDate
        {
            get
            {
                return GetPublishDate(DateTime.MinValue);
            }
            set
            {
                if (value == DateTime.MinValue)
                    RemoveNodes(_atomVer.PublishedElName, _atomNS);
                else
                    PopulateElement(FormatRfc3339(value), _atomNS, _atomVer.PublishedElName);
            }
        }

        public string EditUri
        {
            get
            {
                return GetUrl("atom:link[@rel='edit']/@href", string.Empty);
            }
        }

        public string Permalink
        {
            get
            {
                return GetUrl("atom:link[@rel='alternate' and (@type='text/html' or not(@type))]/@href", string.Empty);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entryEl"></param>
        /// <param name="nsMgr"></param>
        /// <param name="rel"></param>
        /// <param name="contentType">e.g. application/atom+xml</param>
        /// <param name="contentSubType">e.g. "entry"</param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static string GetLink(XmlElement entryEl, XmlNamespaceManager nsMgr, string rel, string contentType, string contentSubType, Uri baseUri)
        {
            Debug.Assert(contentSubType == null || contentType != null, "contentSubType is only used if contentType is also provided");

            string xpath = string.Format(CultureInfo.InvariantCulture,
                                         @"atom:link[@rel='{0}']",
                                         rel);
            foreach (XmlElement link in entryEl.SelectNodes(xpath, nsMgr))
            {
                if (contentType != null)
                {
                    string mimeType = link.GetAttribute("type");
                    if (mimeType == null || mimeType == "")
                        continue;
                    IDictionary mimeData = MimeHelper.ParseContentType(mimeType, true);
                    if (contentType != (string)mimeData[""])
                        continue;
                    if (contentSubType != null && contentSubType != (string)mimeData["type"])
                        continue;
                }
                return XmlHelper.GetUrl(link, "@href", baseUri);
            }
            return "";
        }

        private XmlElement GetElement(string xpath)
        {
            return _entryNode.SelectSingleNode(xpath, _nsMgr) as XmlElement;
        }

        private string GetTextNodePlaintext(string xpath, string defaultValue)
        {
            XmlElement el = GetElement(xpath);
            if (el != null)
                return _atomVer.TextNodeToPlaintext(el);
            else
                return defaultValue;
        }

        private string GetUrl(string xpath, string defaultValue)
        {
            string val = XmlHelper.GetUrl(_entryNode, xpath, _nsMgr, _documentUri);
            return (val != null) ? val : defaultValue;
        }

        private XmlNode PopulateElement(string val, Namespace ns, string localName)
        {
            RemoveNodes(localName, ns);

            if (val != null)
            {
                XmlNode newNode = _entryNode.OwnerDocument.CreateElement(ns.Prefix, localName, ns.Uri);
                newNode.InnerText = val;
                _entryNode.AppendChild(newNode);
                return newNode;
            }
            else
            {
                return null;
            }
        }

        private void RemoveNodes(string localName, Namespace ns)
        {
            ArrayList nodes = new ArrayList();
            foreach (XmlNode n in _entryNode.SelectNodes("./" + ns.Prefix + ":" + localName, _nsMgr))
                nodes.Add(n);

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode n = (XmlNode)nodes[i];
                n.ParentNode.RemoveChild(n);
            }
        }

        #region RFC3339 date handling

        private DateTime ParseRfc3339(string dateTimeString)
        {
            Match m = Regex.Match(dateTimeString, @"
^
(?<year>\d{4})
-
(?<month>\d{2})
-
(?<day>\d{2})
T
(?<hour>\d{2})
\:
(?<minute>\d{2})
\:
(?<second>\d{2})
(\. (?<fraction>\d+) )?
(?<timezone>
    (?<utc>Z)
    |
    (?<offset>
        (?<offdir>[+-])
        (?<offhour>\d{2})
        (?:
            \:?
            (?<offmin>\d{2})
        )?
    )
)
$", RegexOptions.IgnorePatternWhitespace);

            int year = ParseMatchInt(m, "year", 0, 9999);
            int month = ParseMatchInt(m, "month", 1, 12);
            int day = ParseMatchInt(m, "day", 1, 31);
            int hour = ParseMatchInt(m, "hour", 0, 23);
            int minute = ParseMatchInt(m, "minute", 0, 59);
            int second = ParseMatchInt(m, "second", 0, 60);  // leap seconds

            int millis = 0;
            if (m.Groups["fraction"].Success)
                millis = (int)(1000 * double.Parse("0." + m.Groups["fraction"].Value, CultureInfo.InvariantCulture));

            bool utc = m.Groups["utc"].Success;
            DateTime dt = new DateTime(year, month, day, hour, minute, second, millis);

            if (!utc)
            {
                int direction = m.Groups["offdir"].Value == "-" ? 1 : -1;  // reverse because we are trying to get to UTC
                int offhour = int.Parse(m.Groups["offhour"].Value, CultureInfo.InvariantCulture) * direction;
                int offmin = int.Parse(m.Groups["offmin"].Value, CultureInfo.InvariantCulture) * direction;
                dt = dt.AddHours(offhour);
                dt = dt.AddMinutes(offmin);
            }

            return dt;
        }

        private string FormatRfc3339(DateTime dateTime)
        {
            TimeSpan offset = DateTimeHelper.GetUtcOffset(dateTime);
            StringBuilder dt = new StringBuilder((dateTime + offset).ToString(@"yyyy-MM-ddTHH:mm:ss", DateTimeFormatInfo.InvariantInfo));
            char direction;
            if (offset >= TimeSpan.Zero)
                direction = '+';
            else
            {
                direction = '-';
                offset = -offset;
            }
            dt.AppendFormat(CultureInfo.InvariantCulture, "{0}{1:d2}:{2:d2}",
                direction,
                offset.Hours,
                offset.Minutes
                );
            return dt.ToString();
        }

        private int ParseMatchInt(Match m, string label, int min, int max)
        {
            int val = int.Parse(m.Groups[label].Value, CultureInfo.InvariantCulture);
            if (val < min || val > max)
                throw new ArgumentOutOfRangeException(label);
            return val;
        }
        #endregion

        public void GenerateId()
        {
            if (_entryNode.SelectSingleNode("atom:id", _nsMgr) == null)
                PopulateElement("urn:uuid:" + Guid.NewGuid().ToString("d"), _atomNS, "id");
        }

        public void GenerateUpdated()
        {
            if (_entryNode.SelectSingleNode("atom:updated", _nsMgr) == null)
                PopulateElement(FormatRfc3339(DateTime.Now), _atomNS, "updated");
        }
    }
}
