// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Detection
{
    public class RsdServiceDetector
    {

        public static RsdServiceDescription DetectFromWeblog(string homepage, IHTMLDocument2 weblogDOM)
        {
            try
            {
                // get the edit uri
                string rsdFileUrl = ExtractEditUri(homepage, weblogDOM);

                // do the detection
                if (rsdFileUrl != null)
                    return DetectFromRsdUrl(rsdFileUrl);
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error attempting to detect Blog service: " + ex.ToString());
                return null;
            }
        }

        public static RsdServiceDescription DetectFromRsdUrl(string rsdFileUrl)
        {
            try
            {
                // see if we can download a local copy of the RSD file
                using (Stream rsdStream = HttpRequestHelper.SafeDownloadFile(rsdFileUrl))
                {
                    // if we got an RSD file then read the service info from there
                    if (rsdStream != null)
                    {
                        return ReadFromRsdStream(rsdStream, rsdFileUrl);
                    }
                    else
                    {
                        // if we couldn't do a successful RSD detection then return null
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error attempting to detect Blog service: " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// Try to extract the EditUri (RSD file URI) from the passed DOM
        /// </summary>
        /// <param name="weblogDOM"></param>
        /// <returns></returns>
        private static string ExtractEditUri(string homepageUrl, IHTMLDocument2 weblogDOM)
        {
            // look in the first HEAD tag
            IHTMLElementCollection headElements = ((IHTMLDocument3)weblogDOM).getElementsByTagName("HEAD");
            if (headElements.length > 0)
            {
                // get the first head element
                IHTMLElement2 firstHeadElement = (IHTMLElement2)headElements.item(0, 0);

                // look for link tags within the head
                foreach (IHTMLElement element in firstHeadElement.getElementsByTagName("LINK"))
                {
                    IHTMLLinkElement linkElement = element as IHTMLLinkElement;
                    if (linkElement != null)
                    {
                        string linkRel = linkElement.rel;
                        if (linkRel != null && (linkRel.ToUpperInvariant().Equals("EDITURI")))
                        {
                            return UrlHelper.UrlCombineIfRelative(homepageUrl, linkElement.href);
                        }
                    }
                }
            }

            // couldn't find one
            return null;
        }

        /// <summary>
        /// Skips leading whitespace in a reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static TextReader SkipLeadingWhitespace(TextReader reader)
        {
            while (true)
            {
                int c = reader.Peek();
                if (c != -1 && char.IsWhiteSpace((char)c))
                    reader.Read();
                else
                    return reader;
            }
        }

        /// <summary>
        /// Read the definition of a blog service from the passed RSD XML
        /// </summary>
        /// <param name="rsdStream"></param>
        /// <returns></returns>
        private static RsdServiceDescription ReadFromRsdStream(Stream rsdStream, string url)
        {
            // initialize a blog service to return
            RsdServiceDescription rsdServiceDescription = new RsdServiceDescription();
            rsdServiceDescription.SourceUrl = url;

            ArrayList blogApis = new ArrayList();

            try
            {
                // liberally parse the RSD xml
                XmlReader reader = new XmlTextReader(
                    SkipLeadingWhitespace(new StreamReader(rsdStream, new UTF8Encoding(false, false))));
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // windows-live writer extensions to rsd
                        if (UrlHelper.UrlsAreEqual(reader.NamespaceURI, WINDOWS_LIVE_WRITER_NAMESPACE))
                        {
                            /*
                            switch ( reader.LocalName.ToLower() )
                            {
                                case "manifestlink":
                                    rsdServiceDescription.WriterManifestUrl = reader.ReadString().Trim() ;
                                    break;
                            }
                            */
                        }
                        // standard rsd elements
                        else
                        {
                            switch (reader.LocalName.ToUpperInvariant())
                            {
                                case "ENGINENAME":
                                    rsdServiceDescription.EngineName = reader.ReadString().Trim();
                                    break;
                                case "ENGINELINK":
                                    rsdServiceDescription.EngineLink = ToAbsoluteUrl(url, reader.ReadString().Trim());
                                    break;
                                case "HOMEPAGELINK":
                                    rsdServiceDescription.HomepageLink = ToAbsoluteUrl(url, reader.ReadString().Trim());
                                    break;
                                case "API":
                                    RsdApi api = new RsdApi();
                                    for (int i = 0; i < reader.AttributeCount; i++)
                                    {
                                        reader.MoveToAttribute(i);
                                        switch (reader.LocalName.ToUpperInvariant())
                                        {
                                            case "NAME":
                                                api.Name = NormalizeApiName(reader.Value);
                                                break;
                                            case "PREFERRED":
                                                api.Preferred = "true" == reader.Value.Trim();
                                                break;
                                            case "APILINK":
                                            case "RPCLINK": // radio-userland uses rpcLink
                                                api.ApiLink = ToAbsoluteUrl(url, reader.Value.Trim());
                                                break;
                                            case "BLOGID":
                                                api.BlogId = reader.Value.Trim();
                                                break;
                                        }
                                    }
                                    blogApis.Add(api);
                                    break;
                                case "SETTING":
                                    if (blogApis.Count > 0)
                                    {
                                        RsdApi lastRsdApi = (RsdApi)blogApis[blogApis.Count - 1];
                                        string name = reader.GetAttribute("name");
                                        if (name != null)
                                        {
                                            string value = reader.ReadString().Trim();
                                            lastRsdApi.Settings.Add(name, value);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception attempting to read RSD file: " + ex.ToString());

                // don't re-propagate exceptions here becaus we found that TypePad's
                // RSD file was returning bogus HTTP crap at the end of the response
                // and the XML parser cholking on this caused us to fail autodetection
            }

            // if we got at least one API then return the service description
            if (blogApis.Count > 0)
            {
                rsdServiceDescription.Apis = (RsdApi[])blogApis.ToArray(typeof(RsdApi));
                return rsdServiceDescription;
            }
            else
            {
                return null;
            }

        }

        private static string ToAbsoluteUrl(string baseUrl, string url)
        {
            if (!UrlHelper.IsUrl(url))
            {
                url = UrlHelper.EscapeRelativeURL(baseUrl, url);
            }
            return url;
        }

        private static string NormalizeApiName(string name)
        {
            string nName = name.Trim().ToLower(CultureInfo.InvariantCulture);
            if (nName == "movable type") // typepad uses no space, wordpress uses a space, and so on...
                nName = "movabletype";
            return nName;
        }

        private const string WINDOWS_LIVE_WRITER_NAMESPACE = "http://www.microsoft.com/schemas/livewriter";
    }

}
