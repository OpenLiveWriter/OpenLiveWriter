// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// HTMLMetaData provides access to metadata for an IHTMLDocument2.
    /// </summary>
    public class LightWeightHTMLMetaData
    {
        /// <summary>
        /// Constructs a new HTMLMetaData for an IHTMLDocument2
        /// </summary>
        /// <param name="HTMLDocument">The IHTMLDocument2 for which to fetch metadata.</param>
        public LightWeightHTMLMetaData(LightWeightHTMLDocument HTMLDocument)
        {
            m_HTMLDocument = HTMLDocument;
            _docType = HTMLDocument.DocType;
            _savedFrom = HTMLDocument.SavedFrom;
            LightWeightTag[] beginTags = HTMLDocument.GetTagsByName(HTMLTokens.Base);
            foreach (LightWeightTag baseTag in beginTags)
            {
                Attr href = baseTag.BeginTag.GetAttribute(HTMLTokens.Href);
                if (href != null)
                {
                    _base = href.Value;
                    break;
                }
            }
        }

        /// <summary>
        /// The Author of the page.
        /// </summary>
        public string Author
        {
            get
            {
                if (_author == null)
                    _author = GetMetaDataValue(AUTHOR);
                return _author;
            }
            set
            {
                _author = value;
            }
        }
        private string _author = null;

        public string Base
        {
            get
            {
                return _base;
            }
            set
            {
                _base = value;
            }
        }
        private string _base = null;

        /// <summary>
        /// The description of the page.
        /// </summary>
        public string Description
        {
            get
            {
                if (_description == null)
                {
                    _description = GetMetaDataValue(DESCRIPTION);
                    if (_description != null)
                    {
                        _description = TextHelper.CompactWhiteSpace(_description);
                    }
                }
                return _description;
            }
            set
            {
                _description = value;
            }
        }
        private string _description = null;

        /// <summary>
        /// The keywords used to describe this page.
        /// </summary>
        public string[] Keywords
        {
            get
            {
                if (_keywords == null)
                {
                    _keywords = GetKeywordsFromString(TextHelper.CompactWhiteSpace(KeywordString));
                }
                return _keywords;
            }
            set
            {
                _keywords = value;
            }
        }
        private string[] _keywords = null;

        /// <summary>
        /// The keywords used to describe this page (as a string)
        /// </summary>
        public string KeywordString
        {
            get
            {
                return GetMetaDataValue(KEYWORDS);
            }
        }

        /// <summary>
        /// Indicators for controlling the behavior of robots crawling websites.
        /// </summary>
        public string Robots
        {
            get
            {
                if (_robots == null)
                {
                    _robots = GetMetaDataValue(ROBOTS);
                }
                return _robots;
            }
            set
            {
                _robots = value;
            }
        }
        private string _robots = null;

        public string generator
        {
            set { _generator = value; }
        }

        /// <summary>
        /// The application or tool that generated this page.
        /// </summary>
        public string Generator
        {
            get
            {
                if (_generator == null)
                    _generator = GetMetaDataValue(GENERATOR);
                return _generator;
            }
            set
            {
                _generator = value;
            }
        }
        private string _generator;

        public string DocType
        {
            get
            {
                return _docType;
            }
            set
            {
                _docType = value;
            }
        }
        private string _docType = null;

        public string SavedFrom
        {
            get { return _savedFrom; }
            set { _savedFrom = value; }
        }

        private string _savedFrom = null;

        /// <summary>
        /// Copyright information for the page.
        /// </summary>
        public string CopyRight
        {
            get
            {
                if (_copyright == null)
                    _copyright = GetMetaDataValue(COPYRIGHT);
                return _copyright;
            }
            set
            {
                _copyright = value;
            }
        }
        private string _copyright = null;

        /// <summary>
        /// Hint to the consumer of a page about the appropriate caching behavior
        /// </summary>
        public string Pragma
        {
            get
            {
                if (_pragma == null)
                    _pragma = GetMetaDataValue(PRAGMA);
                return _pragma;
            }
            set
            {
                _pragma = value;
            }
        }
        private string _pragma = null;

        /// <summary>
        /// Returns the character set as encoded in the HTML Meta Tag
        /// </summary>
        public string Charset
        {
            // Don't get tricked by the charset attribute on IHTMLMetaElement
            // This sets an attribute 'charset' rather than the charset
            // property in content type
            get
            {
                if (m_charset == null)
                {
                    // parse the charset out and return it
                    string contentType = GetMetaDataValue(CONTENTTYPE);
                    if (contentType != null
                        && contentType.IndexOf(";", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        string[] contentParts = contentType.Split(';');

                        foreach (string part in contentParts)
                        {
                            // find out if this part has an =
                            if (part.IndexOf("=", StringComparison.OrdinalIgnoreCase) > -1)
                            {
                                string[] subParts = part.Split('=');
                                if (subParts[1] != null && subParts[0].Trim().ToUpperInvariant() == "CHARSET")
                                {
                                    m_charset = subParts[1];
                                    break;
                                }

                            }
                        }
                    }
                }

                if (m_charset == null)
                {
                    LightWeightTag[] metaTags = BaseHTMLDocument.GetTagsByName("META");
                    foreach (LightWeightTag metaTag in metaTags)
                    {
                        Attr charset = metaTag.BeginTag.GetAttribute(CHARSET);
                        if (charset != null)
                            m_charset = charset.Value;
                        break;
                    }
                }

                return m_charset;
            }
            set
            {
                m_charset = value;
            }
        }
        private string m_charset;

        /// <summary>
        /// The underlying IHTMLDocument2
        /// </summary>
        public LightWeightHTMLDocument BaseHTMLDocument
        {
            get
            {
                return m_HTMLDocument;
            }
        }
        private LightWeightHTMLDocument m_HTMLDocument = null;

        /// <summary>
        /// Gets a particular metadata tag's value.  Return null if the meta tag doesn't exist
        /// in this document.
        /// </summary>
        /// <param name="metaDataName">The name of the metadata element to retrieve</param>
        /// <returns>The value</returns>
        private string GetMetaDataValue(string metaDataName)
        {

            if (MetaTags.Contains(metaDataName))
                return (string)MetaTags[metaDataName];
            else
                return null;
        }

        private Hashtable MetaTags
        {
            get
            {
                if (!m_metaTableGenerated)
                {
                    // Get the meta tags
                    LightWeightTag[] metaTags = this.BaseHTMLDocument.GetTagsByName("META");
                    foreach (LightWeightTag metaTag in metaTags)
                    {
                        foreach (string nameAttribute in NAME_ATTRIBUTES)
                        {
                            Attr attr = metaTag.BeginTag.GetAttribute(nameAttribute);
                            if (attr != null)
                            {
                                string metaName = attr.Value.ToUpper(CultureInfo.InvariantCulture);
                                Attr content = metaTag.BeginTag.GetAttribute("CONTENT");
                                if (content != null && !m_metaTags.ContainsKey(metaName))
                                {
                                    m_metaTags.Add(metaName, content.Value);
                                    break;
                                }

                            }
                        }
                    }
                    m_metaTableGenerated = true;
                }

                return m_metaTags;
            }
        }
        private Hashtable m_metaTags = new Hashtable();
        private bool m_metaTableGenerated = false;

        private static string[] NAME_ATTRIBUTES = { "NAME", "HTTP-EQUIV" };

        private static readonly Regex multiSpaces = new Regex("[ \t]+");

        private string[] GetKeywordsFromString(string keywords)
        {
            if (keywords == null)
                return new string[0];

            keywords = multiSpaces.Replace(keywords, " ");
            if (keywords.IndexOf(",", StringComparison.OrdinalIgnoreCase) > -1)
                return keywords.Split(',');
            else if (keywords.IndexOf(" ", StringComparison.OrdinalIgnoreCase) > -1)
                return keywords.Split(' ');
            else
                return new string[0];
        }

        private const string AUTHOR = "AUTHOR";
        private const string DESCRIPTION = "DESCRIPTION";
        private const string KEYWORDS = "KEYWORDS";
        private const string ROBOTS = "ROBOTS";
        private const string GENERATOR = "GENERATOR";
        private const string COPYRIGHT = "COPYRIGHT";
        private const string PRAGMA = "PRAGMA";
        private const string CONTENTTYPE = "CONTENT-TYPE";
        private const string CHARSET = "charset";
    }
}
