// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{

    public class LiveClipboardFormat
    {
        public LiveClipboardFormat(string contentType)
            : this(contentType, String.Empty)
        {
        }

        public LiveClipboardFormat(string contentType, string type)
        {
            _contentType = contentType;

            // provide a hard guarantee that Type is never null
            // (simplifies code downstream)
            _type = (type != null) ? type : String.Empty;

            // id is ContentType plus "/type" if a type is specified
            _id = ContentType + (Type != String.Empty ? "/" + Type : String.Empty);
        }

        public string Id
        {
            get { return _id; }
        }
        private string _id;

        public string ContentType
        {
            get { return _contentType; }
        }
        private string _contentType;

        public string Type
        {
            get { return _type; }
        }
        private string _type = String.Empty;

        public override bool Equals(object obj)
        {
            LiveClipboardFormat lcFormat = obj as LiveClipboardFormat;
            return Id.Equals(lcFormat.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class LiveClipboardData
    {
        public static LiveClipboardData Create(IDataObject iDataObject)
        {
            TextData textData = TextData.Create(iDataObject);
            if (textData != null)
            {
                XmlDocument xmlDocument = ExtractLiveClipboardData(textData.Text);

                if (xmlDocument != null)
                    return new LiveClipboardData(xmlDocument);
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public XmlDocument Document
        {
            get { return _xmlDocument; }
        }

        public LiveClipboardFormat[] Formats
        {
            get
            {
                if (!_attemptedFormats)
                {
                    _attemptedFormats = true;
                    _formats = ExtractFormats();
                }
                return _formats;
            }

        }
        private bool _attemptedFormats = false;
        private LiveClipboardFormat[] _formats;

        public string HtmlPresentation
        {
            get
            {
                if (!_attemptedCreateHtmlPresentation)
                {
                    _attemptedCreateHtmlPresentation = true;
                    _htmlPresentation = ExtractHtmlPresentation();
                }
                return _htmlPresentation;
            }
        }
        private bool _attemptedCreateHtmlPresentation;
        private string _htmlPresentation = null;

        private LiveClipboardFormat[] ExtractFormats()
        {
            ArrayList formats = new ArrayList();
            try
            {
                // grab the format nodes
                XmlNodeList formatNodes = Document.SelectNodes("//lc:data/lc:format", _namespaceManager);
                foreach (XmlNode formatNode in formatNodes)
                {
                    string contentType = null;
                    string type = null;
                    foreach (XmlAttribute attribute in formatNode.Attributes)
                    {
                        switch (attribute.LocalName.ToUpperInvariant())
                        {
                            case "CONTENTTYPE":
                                contentType = attribute.InnerText;
                                break;

                            case "TYPE":
                                type = attribute.InnerText;
                                break;
                        }
                    }

                    if (contentType != null)
                        formats.Add(new LiveClipboardFormat(contentType, type));
                    else
                        Trace.Fail("found lc:format element without contenttype attribute");
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception attempt to extract LiveClipboard formats: " + ex.ToString());
            }

            // return the formats
            return formats.ToArray(typeof(LiveClipboardFormat)) as LiveClipboardFormat[];
        }

        private string ExtractHtmlPresentation()
        {
            try
            {
                foreach (string textFormat in _textFormats)
                {
                    string selectExpr = String.Format(CultureInfo.InvariantCulture, "//lc:presentations/lc:format[@contenttype='{0}']", textFormat);
                    XmlNode htmlPresentationNode = Document.SelectSingleNode(selectExpr, _namespaceManager);

                    if (htmlPresentationNode != null)
                    {
                        // TODO: additional clarification from LC team on expected content-types and encodings
                        // TODO: for the CDATA case, what are valid values for encoding?

                        // determine whether we need to take the InnerText or InnerXml based on whether
                        // there are any top-level XML elements contained within the node
                        bool hasTopLevelElement = false;
                        foreach (XmlNode xmlNode in htmlPresentationNode.ChildNodes)
                        {
                            if (xmlNode.NodeType == XmlNodeType.Element || xmlNode.NodeType == XmlNodeType.EndElement)
                            {
                                hasTopLevelElement = true;
                                break;
                            }
                        }

                        if (hasTopLevelElement)
                            return htmlPresentationNode.InnerXml;
                        else
                            return htmlPresentationNode.InnerText;
                    }
                }
                // none found
                return null;
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception attempt to extract LiveClipboard HTML representation: " + ex.ToString());
                return null;
            }
        }
        private const string TEXT_HTML = "text/html";
        private const string TEXT_PLAIN = "text/plain";
        private const string APPLICATION_XHTML = "application/xhtml+xml";
        private string[] _textFormats = new string[] { TEXT_HTML, APPLICATION_XHTML, TEXT_PLAIN };

        private static XmlDocument ExtractLiveClipboardData(string clipboardText)
        {
            try
            {
                // TODO: pre-parse to see if this is supposed to be liveclipboard data,
                // if it is an we get an XML exception then display an error dialog
                // saying the format is invalid

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(new StringReader(clipboardText));
                if (xmlDocument.DocumentElement.LocalName == LC_DOCUMENT_ELEMENT)
                    return xmlDocument;
                else
                    return null;
            }
            catch (XmlException)
            {
                return null; // string did not contain xml
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception extracting LiveClipboard data: " + ex.ToString());
                return null;
            }
        }

        private LiveClipboardData(XmlDocument xmlDocument)
        {
            // save a reference to the document
            _xmlDocument = xmlDocument;

            // create namespace manager
            _namespaceManager = new XmlNamespaceManager(_xmlDocument.NameTable);
            _namespaceManager.AddNamespace(LC_NS_PREFIX, LC_NS_URI);
        }

        private XmlDocument _xmlDocument;
        private XmlNamespaceManager _namespaceManager;

        private const string LC_DOCUMENT_ELEMENT = "liveclipboard";
        private const string LC_NS_PREFIX = "lc";
        private const string LC_NS_URI = "http://www.microsoft.com/schemas/liveclipboard";
    }
}
