// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using mshtml;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// HTMLMetaData provides access to metadata for an IHTMLDocument2.
    /// </summary>
    public class HTMLMetaData
    {
        /// <summary>
        /// Constructs a new HTMLMetaData for an IHTMLDocument2
        /// </summary>
        /// <param name="HTMLDocument">The IHTMLDocument2 for which to fetch metadata.</param>
        public HTMLMetaData(IHTMLDocument2 HTMLDocument)
        {
            m_HTMLDocument = HTMLDocument;
        }

        /// <summary>
        /// The Author of the page.
        /// </summary>
        public string Author
        {
            get
            {
                return GetMetaDataValue(AUTHOR);
            }
        }

        /// <summary>
        /// The description of the page.
        /// </summary>
        public string Description
        {
            get
            {
                string description = GetMetaDataValue(DESCRIPTION);
                if (description != null)
                    description = description.Replace("\r\n", string.Empty);
                return description;
            }
        }

        /// <summary>
        /// The keywords used to describe this page.
        /// </summary>
        public string[] Keywords
        {
            get
            {
                return GetKeywordsFromString(KeywordString);
            }
        }

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
                return GetMetaDataValue(ROBOTS);
            }
        }

        /// <summary>
        /// The application or tool that generated this page.
        /// </summary>
        public string Generator
        {
            get
            {
                return GetMetaDataValue(GENERATOR);
            }
        }

        /// <summary>
        /// Copyright information for the page.
        /// </summary>
        public string CopyRight
        {
            get
            {
                return GetMetaDataValue(COPYRIGHT);
            }
        }

        /// <summary>
        /// Hint to the consumer of a page about the appropriate caching behavior
        /// </summary>
        public string Pragma
        {
            get
            {
                return GetMetaDataValue(PRAGMA);
            }
        }

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
                                if (subParts[1] != null && subParts[0].IndexOf("charset", StringComparison.OrdinalIgnoreCase) > -1)
                                {
                                    m_charset = subParts[1];
                                    break;
                                }

                            }
                        }
                    }

                    // We didn't find the charset that way, so just see if one of the elements
                    // has charset
                    if (m_charset == null)
                    {
                        foreach (IHTMLElement element in this.MetaDataElements)
                        {
                            IHTMLMetaElement metaElement = (IHTMLMetaElement)element;
                            if (metaElement.charset != null)
                                m_charset = metaElement.charset;
                        }
                    }
                }
                return m_charset;
            }
            set
            {
                // update an existing charset
                bool metaElementFound = false;
                StringBuilder finalBuilder = new StringBuilder();
                string contentType = GetMetaDataValue(CONTENTTYPE);

                if (contentType != null
                    && contentType.IndexOf(";", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    string[] contentParts = contentType.Split(';');
                    foreach (string part in contentParts)
                    {
                        string partToWrite = part;
                        if (part.IndexOf("=", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            string[] subParts = part.Split('=');
                            if (subParts[1] != null && subParts[0].IndexOf("charset", StringComparison.OrdinalIgnoreCase) > -1)
                            {
                                partToWrite = "charset=" + value;
                            }
                        }
                        finalBuilder.Append(partToWrite + "; ");
                    }

                    metaElementFound = true;
                    SetMetaData(CONTENTTYPE, "Content", finalBuilder.ToString());
                }
                else
                {
                    // see if there is a charset only metadata tag we can just update
                    foreach (IHTMLElement element in this.MetaDataElements)
                    {
                        IHTMLMetaElement metaElement = (IHTMLMetaElement)element;
                        if (metaElement.charset != null)
                        {
                            metaElement.charset = value;
                            metaElementFound = true;
                        }
                    }
                }

                if (!metaElementFound)
                {
                    // There isn't an element, so just pound one in!
                    IHTMLMetaElement element = (IHTMLMetaElement)m_HTMLDocument.createElement("meta");
                    element.httpEquiv = CONTENTTYPE;
                    element.content = "text/html; charset=" + value;

                    IHTMLDOMNode metaNode = (IHTMLDOMNode)element;
                    IHTMLDOMNode node = (IHTMLDOMNode)DocumentHead;
                    node.appendChild(metaNode);
                }
                m_charset = value;
            }
        }
        private string m_charset;

        /// <summary>
        /// The underlying IHTMLDocument2
        /// </summary>
        public IHTMLDocument2 BaseHTMLDocument
        {
            get
            {
                return m_HTMLDocument;
            }
        }
        private IHTMLDocument2 m_HTMLDocument = null;

        /// <summary>
        /// The Document head
        /// </summary>
        private IHTMLElement DocumentHead
        {
            get
            {
                if (m_documentHead == null)
                {
                    IHTMLElementCollection headCollection = (IHTMLElementCollection)BaseHTMLDocument.all.tags("HEAD");
                    Debug.Assert(headCollection.length == 1, "More than one head in the collection!");
                    m_documentHead = (IHTMLElement)headCollection.item(0, 0);
                }
                return m_documentHead;
            }
        }
        private IHTMLElement m_documentHead = null;

        /// <summary>
        /// Gets a particular metadata tag's value.  Return null if the meta tag doesn't exist
        /// in this document.
        /// </summary>
        /// <param name="metaDataName">The name of the metadata element to retrieve</param>
        /// <returns>The value</returns>
        private string GetMetaDataValue(string metaDataName)
        {
            if (!m_metaTableGenerated)
            {
                // Get the meta tags
                IEnumerator tagEnumerator = MetaDataElements.GetEnumerator();

                // Go through all the tags in the head and pull out the meta tags
                while (tagEnumerator.MoveNext())
                {
                    IHTMLElement thisTag = (IHTMLElement)tagEnumerator.Current;

                    // in the meta tags, grab the attributes and put them in the table
                    foreach (string nameAttribute in NAME_ATTRIBUTES)
                    {
                        string name = (string)thisTag.getAttribute(nameAttribute, 0);
                        if (name != null)
                        {
                            name = name.ToUpper(CultureInfo.InvariantCulture);  // for case insensitive comparison
                            string content = (string)thisTag.getAttribute("CONTENT", 0);
                            if (!m_metaTags.ContainsKey(name))
                            {
                                m_metaTags.Add(name, content);
                                break;
                            }
                        }
                    }

                }
                m_metaTableGenerated = true;
            }

            if (m_metaTags.Contains(metaDataName))
                return (string)m_metaTags[metaDataName];
            else
                return null;
        }
        private Hashtable m_metaTags = new Hashtable();
        private bool m_metaTableGenerated = false;

        private IHTMLMetaElement GetElement(string metaDataName)
        {
            metaDataName = metaDataName.ToUpper(CultureInfo.InvariantCulture);
            if (!m_metaElementsGenerated)
            {
                // Get the meta tags
                IEnumerator tagEnumerator = MetaDataElements.GetEnumerator();

                // Go through all the tags in the head and pull out the meta tags
                while (tagEnumerator.MoveNext())
                {
                    IHTMLMetaElement thisTag = (IHTMLMetaElement)tagEnumerator.Current;
                    if (thisTag.name != null)
                    {
                        if (!m_metaElements.ContainsKey(thisTag.name))
                        {
                            m_metaElements.Add(thisTag.name.ToUpper(CultureInfo.InvariantCulture), thisTag);
                        }
                    }
                    else if (thisTag.httpEquiv != null)
                    {
                        if (!m_metaElements.ContainsKey(thisTag.httpEquiv))
                        {
                            m_metaElements.Add(thisTag.httpEquiv.ToUpper(CultureInfo.InvariantCulture), thisTag);
                        }
                    }
                }
                m_metaTableGenerated = true;
            }
            return (IHTMLMetaElement)m_metaElements[metaDataName];
        }
        private bool m_metaElementsGenerated = false;
        private Hashtable m_metaElements = new Hashtable();

        private IHTMLElementCollection MetaDataElements
        {
            get
            {
                if (m_metaDataElements == null)
                {
                    IHTMLDocument3 html3 = (IHTMLDocument3)m_HTMLDocument;
                    m_metaDataElements = html3.getElementsByTagName("META");
                }
                return m_metaDataElements;
            }
        }
        private IHTMLElementCollection m_metaDataElements;

        private static string[] NAME_ATTRIBUTES = { "NAME", "HTTPEQUIV" };

        private void SetMetaData(string metaDataName, string metaDataAttribute, string metaDataValue)
        {
            IEnumerator tagEnumerator = MetaDataElements.GetEnumerator();

            // Go through all the tags in the head and pull out the meta tags
            while (tagEnumerator.MoveNext())
            {
                IHTMLElement thisTag = (IHTMLElement)tagEnumerator.Current;

                // in the meta tag set the right attribute
                foreach (string nameAttribute in NAME_ATTRIBUTES)
                {
                    string name = (string)thisTag.getAttribute(nameAttribute, 0);
                    if (name != null && (name.ToUpper(CultureInfo.InvariantCulture) == metaDataName.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        thisTag.setAttribute(metaDataAttribute, metaDataValue, 0);
                    }
                }

            }
            m_metaTableGenerated = true;
        }

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
