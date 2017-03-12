// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Utility for extracting XMP metadata from files.
    /// XMP Reference: http://creativecommons.org/technology/xmp
    /// Code inspired by Omar Shahine: http://shahine.com/omar/ReadingXMPMetadataFromAJPEGUsingC.aspx
    /// Licensed under CC BY 3.0 (http://creativecommons.org/licenses/by/3.0/)
    /// </summary>
    public class XmpMetadata
    {
        private XmlDocument doc;
        private XmlNamespaceManager NamespaceManager;
        private XmpMetadata(string xmpxml)
        {
            LoadDoc(xmpxml);
        }

        public string Title
        {
            get
            {
                return GetRdfAltInnerText("/rdf:RDF/rdf:Description/dc:title/rdf:Alt");
            }
        }

        public string Description
        {
            get
            {
                return GetRdfAltInnerText("/rdf:RDF/rdf:Description/dc:description/rdf:Alt");
            }
        }

        public string[] Keywords
        {
            get
            {
                // get keywords
                XmlNode xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description/dc:subject/rdf:Bag", NamespaceManager);
                ArrayList keywords = new ArrayList();
                if (xmlNode != null)
                {
                    foreach (XmlNode li in xmlNode)
                    {
                        keywords.Add(li.InnerText);
                    }
                }
                return (string[])keywords.ToArray(typeof(string));
            }
        }

        public int Rating
        {
            get
            {
                // get ratings
                XmlNode xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description/xap:Rating", NamespaceManager);

                // Alternatively, there is a common form of RDF shorthand that writes simple properties as
                // attributes of the rdf:Description element.
                if (xmlNode == null)
                {
                    xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description", NamespaceManager);
                    xmlNode = xmlNode.Attributes["xap:Rating"];
                }

                int rating = -1;
                if (xmlNode != null)
                {
                    return Convert.ToInt32(xmlNode.InnerText, CultureInfo.InvariantCulture);
                }
                return rating;
            }
        }

        public static XmpMetadata FromFile(string filepath)
        {
            string xmpxml = GetXmpXmlFromFile(filepath);
            try
            {
                if (xmpxml != null)
                    return new XmpMetadata(xmpxml);
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetXmpXmlFromFile(string filename)
        {
            string contents = null;
            byte[] beginCapture = Encoding.UTF8.GetBytes("<rdf:RDF");
            byte[] endCapture = Encoding.UTF8.GetBytes("</rdf:RDF>");

            using (Stream s = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = StreamHelper.ExtractByteRegion(beginCapture, endCapture, s);
                if (bytes.Length > 0)
                    contents = Encoding.UTF8.GetString(bytes);
            }

            return contents;
        }

        private void LoadDoc(string xmpXmlDoc)
        {
            doc = new XmlDocument();

            try
            {
                doc.LoadXml(xmpXmlDoc);

                NamespaceManager = new XmlNamespaceManager(doc.NameTable);
                NamespaceManager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                NamespaceManager.AddNamespace("exif", "http://ns.adobe.com/exif/1.0/");
                NamespaceManager.AddNamespace("x", "adobe:ns:meta/");
                NamespaceManager.AddNamespace("xap", "http://ns.adobe.com/xap/1.0/");
                NamespaceManager.AddNamespace("tiff", "http://ns.adobe.com/tiff/1.0/");
                NamespaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occured while loading XML metadata from image. The error was: " + ex.Message);
            }
        }

        private string GetRdfAltInnerText(string xpath)
        {
            // get description
            XmlNode xmlNode = doc.SelectSingleNode(xpath, NamespaceManager);
            if (xmlNode != null && xmlNode.ChildNodes.Count > 0)
            {
                return xmlNode.ChildNodes[0].InnerText;
            }
            return null;
        }
    }
}
