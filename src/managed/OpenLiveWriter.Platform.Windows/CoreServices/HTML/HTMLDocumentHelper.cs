// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define oldschool
#define DISABLE_SCRIPT_INJECTION
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using mshtml;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for HTMLDocumentHelper.
    /// </summary>
    public class HTMLDocumentHelper
    {
        /// <summary>
        /// Replaces one Url in an html string with a new url
        /// </summary>
        /// <param name="html">The html string</param>
        /// <param name="url">The url to replace</param>
        /// <param name="newUrl">The new url</param>
        /// <returns>The html with the new Url</returns>
        [Obsolete("Please use LightweightHTMLReplacer instead!")]
        public static string EscapePath(string html, string url, string newUrl)
        {
            if (html == null || url == null || newUrl == null)
                return html;

            // TODO: Determine the right semantics for escaping urls
            // ideally, we should encode or decode both for compare, but since we're doing
            // search and replace, this doesn't work.
            // Note that the below only works for attribs like:
            //
            //	<a href="news.html">
            //	<a href=news.html>
            //	<a href= news.html>
            //
            // If we don't require the quoting or equals sign, we'll occasionally hose absolute paths like:
            //				http://www.goo.com/hello/world/index.html
            //				when we're replacing a relative path like: index.html
            // see: http://www.cs.tut.fi/~jkorpela/html/iframe.html for an example

            EscapePathHelper helper = new EscapePathHelper(newUrl);
            string cleanedUpUrl = UrlHelper.CleanUpUrl(url);

#if oldschool
            // Old school is much much faster than the regex
            html = html.Replace("\"" + UrlHelper.CleanUpUrl(url) + "\"", "\"" + newUrl + "\"");      // "_"
            html = html.Replace("=" + UrlHelper.CleanUpUrl(url) + " ", "=\"" + newUrl + "\" ");      // =_.
            html = html.Replace("=" + UrlHelper.CleanUpUrl(url) + ">", "=\"" + newUrl + "\">");      // =_>
            html = html.Replace("= " + UrlHelper.CleanUpUrl(url) + " ", "= \"" + newUrl + " \"");    // =._.
            html = html.Replace("= " + UrlHelper.CleanUpUrl(url) + " >", "= \"" + newUrl + " \">");  // =._.>
            html = html.Replace("'" + UrlHelper.CleanUpUrl(url) + "'", "'" + newUrl + "'");          // '_'
            html = html.Replace("url(" + UrlHelper.CleanUpUrl(url) + ")", "Url(" + newUrl + ")");    // url(_)
            html = html.Replace("url( " + UrlHelper.CleanUpUrl(url) + " )", "Url(" + newUrl + ")");  // url( _ )
#else
            string pattern1 = @"(=?\s*(?:'|"")?)(" + Regex.Escape(cleanedUpUrl) + @")((?:""|')?\s*>?)";
            html = Regex.Replace(html, pattern1, new MatchEvaluator(helper.MatchEvaluator1));

            string pattern2 = @"url\(\s*" + Regex.Escape(cleanedUpUrl) + @"\s*\)";
            html = Regex.Replace(html, pattern2, "Url(" + newUrl + ")", RegexOptions.IgnoreCase);
#endif

            // HACK: Bug 1380. Be careful, because there can be some crazy things, like escaped tabs in URLs!
            if (url.IndexOf("\t", StringComparison.OrdinalIgnoreCase) > -1)
                html = html.Replace(url.Replace("\t", "&#9;"), newUrl);

            // Pages saved as web page complete escape the urls, so we should also try
            // replacing a decoded version of the current reference.
#if oldschool
            html = html.Replace("\"" + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + "\"", "\"" + newUrl + "\"");
            html = html.Replace("=" + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + " ", "=\"" + newUrl + "\" ");
            html = html.Replace("=" + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + ">", "=\"" + newUrl + "\">");
            html = html.Replace("= " + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + " ", "= \"" + newUrl + " \"");
            html = html.Replace("= " + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + " >", "= \"" + newUrl + " \">");
            html = html.Replace("'" + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + "'", "'" + newUrl + "'");
            html = html.Replace("url(" + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + ")", "Url(" + newUrl + ")");
            html = html.Replace("url( " + HttpUtility.UrlDecode(UrlHelper.CleanUpUrl(url)) + " )", "Url(" + newUrl + ")");
#else
            string decodedUrl = HttpUtility.UrlDecode(cleanedUpUrl);

            string pattern3 = @"(=?\s*(?:'|"")?)(" + Regex.Escape(decodedUrl) + @")((?:""|')?\s*>?)";
            html = Regex.Replace(html, pattern3, new MatchEvaluator(helper.MatchEvaluator1));

            string pattern4 = @"url\(\s*" + Regex.Escape(decodedUrl) + @"\s*\)";
            html = Regex.Replace(html, pattern4, "Url(" + newUrl + ")", RegexOptions.IgnoreCase);
#endif

            // When an absolute path doesn't end with a file or a slash (i.e. http://www.example.net)
            // converting it to a Uri automatically adds the trailing slash (i.e. http://www.example.net/).
            // When we fix up this escaping, we need to account for the fact that the source may lack the trailing
            // slash, but should still be replaced with the new url.  This is hacked in below.
            if (url.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                html = EscapePath(html, url.TrimEnd('/'), newUrl);

            return html;
        }

        public static bool IsReady(IHTMLDocument2 document)
        {
            return String.Compare(document.readyState, "complete", StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static IHTMLElement FindElementContainingText(IHTMLDocument2 document, string text)
        {
            IHTMLDocument3 doc3 = document as IHTMLDocument3;
            IHTMLDOMNode node = FindElementContainingText(doc3.documentElement as IHTMLDOMNode, text, new Hashtable());
            return node as IHTMLElement;
        }

        public static IHTMLElement[] FindElementsContainingText(IHTMLDocument2 document, string text)
        {
            ArrayList elements = new ArrayList();
            IHTMLDocument3 doc3 = document as IHTMLDocument3;
            AddElementsContainingText(doc3.documentElement as IHTMLDOMNode, text, elements, new Hashtable());
            return HTMLElementHelper.ToElementArray(elements);
        }

        private static IHTMLDOMNode FindElementContainingText(IHTMLDOMNode node, string text, Hashtable visitedNodes)
        {
            IHTMLElement element = node as IHTMLElement;
            if (element != null)
            {
                if (visitedNodes.ContainsKey(element.sourceIndex))
                {
                    return null;
                }
                else
                    visitedNodes.Add(element.sourceIndex, text);
            }

            IHTMLDOMChildrenCollection children = (IHTMLDOMChildrenCollection)node.childNodes;
            IHTMLDOMNode elementNode = null;
            foreach (IHTMLDOMNode childNode in children)
            {
                if (childNode.nodeType == HTMLDOMNodeTypes.TextNode)
                {
                    if (TextNodeContainsText(childNode, text))
                        elementNode = node;
                }
                else
                    elementNode = FindElementContainingText(childNode, text, visitedNodes);

                if (elementNode != null)
                    break;
            }

            return elementNode;
        }

        private static void AddElementsContainingText(IHTMLDOMNode node, string text, ArrayList list, Hashtable visitedNodes)
        {
            IHTMLElement element = node as IHTMLElement;
            if (element != null)
            {
                if (visitedNodes.ContainsKey(element.sourceIndex))
                {
                    return;
                }
                else
                    visitedNodes.Add(element.sourceIndex, text);
            }
            IHTMLDOMChildrenCollection children = (IHTMLDOMChildrenCollection)node.childNodes;
            IHTMLDOMNode elementNode = null;
            foreach (IHTMLDOMNode childNode in children)
            {
                if (childNode.nodeType == HTMLDOMNodeTypes.TextNode)
                {
                    if (TextNodeContainsText(childNode, text))
                        list.Add(node);
                }
                else
                    AddElementsContainingText(childNode, text, list, visitedNodes);

                if (elementNode != null)
                    break;
            }
        }

        /// <summary>
        /// Returns true if a text node contains the specified text.
        /// </summary>
        /// <param name="textNode"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool TextNodeContainsText(IHTMLDOMNode textNode, string text)
        {
            Debug.Assert(textNode.nodeType == HTMLDOMNodeTypes.TextNode);
            return textNode.nodeValue.ToString().Trim().IndexOf(text.Trim(), StringComparison.OrdinalIgnoreCase) != -1;
        }

        /// <summary>
        /// HTMLDomNode types - used to determine whether a node is a text node or an element (tag) node.
        /// </summary>
        public struct HTMLDOMNodeTypes
        {
            public const int TextNode = 3;
            public const int ElementNode = 1;
        }

        private struct EscapePathHelper
        {
            private readonly string newUrl;

            public EscapePathHelper(string newUrl)
            {
                this.newUrl = newUrl;
            }

            public string MatchEvaluator1(Match match)
            {
                string prefix = match.Groups[1].Value;
                if (prefix.Trim().Length > 0)
                {
                    string suffix = match.Groups[3].Value;
                    return prefix + newUrl + suffix;
                }
                else
                {
                    return match.Groups[0].Value;
                }
            }
        }

        public static bool IsFriendlyErrorPage(string originalUrl, IHTMLDocument2 document)
        {
            if (document == null || document.url == null)
                return false;

            string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string friendlyErrorPath = "res://" + Path.Combine(systemDir, "shdoclc.dll");

            return (document.url.StartsWith(friendlyErrorPath, StringComparison.OrdinalIgnoreCase));
        }

        public static bool DocumentContainsFeed(IHTMLDocument2 document)
        {
            try
            {
                IHTMLDocument3 doc = document as IHTMLDocument3;
                IHTMLElementCollection linkElements = doc.getElementsByTagName("LINK");
                foreach (IHTMLElement linkElement in linkElements)
                {
                    string type = linkElement.getAttribute("type", 2) as string;
                    string rel = linkElement.getAttribute("rel", 2) as string;
                    if (type != null)
                    {
                        type = type.ToUpperInvariant(); //type values are case insensitive
                        rel = rel.ToUpperInvariant();

                        if (type.StartsWith("application/rss", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        else if (type.StartsWith("application/atom", StringComparison.OrdinalIgnoreCase) && (rel == "ALTERNATE" || rel == "SERVICE.FEED"))
                        {
                            return true;
                        }
                    }
                }

                // didn't find a feed
                return false;
            }
            catch (Exception e)
            {
                Debug.Fail("Unexpected exception detecting feed: " + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Converts an HTML fragment into a valid HTML document (adding
        /// 'html' and other necessary tags.
        /// </summary>
        /// <param name="html">The html fragment</param>
        /// <param name="sourceUrl">The source url for the html fragment</param>
        /// <returns>A valid HTML string</returns>
        public static string CreateValidHTMLDocument(string html, string sourceUrl)
        {
            // Place a starting token marking the beginning the HTML passed in
            // This prevents the parser from removing any leading comments
            const string UNIQUE_TOKEN = "Unique_40A6E996-4AAE-4E90-AAEA-A316E26D0D74_Token";
            html = UNIQUE_TOKEN + html;

            // Get an HTMLDocument for the html fragment
            IHTMLDocument2 hdoc = StringToHTMLDoc(html, sourceUrl);

            // Get an enumerator for all the elements in the document
            IHTMLElementCollection elements = hdoc.all;
            IEnumerator elementEnum = elements.GetEnumerator();

            // Find the HTML tag, and get its outerHTML (this is the HTML
            // string representing this document)
            string htmlText = null;
            while (elementEnum.MoveNext())
            {
                IHTMLElement e = (IHTMLElement)elementEnum.Current;
                if (e.tagName == "HTML")
                {
                    htmlText = e.outerHTML;
                    break;
                }
            }

            // Get rid of the leading token
            htmlText = htmlText.Remove(htmlText.IndexOf(UNIQUE_TOKEN, StringComparison.OrdinalIgnoreCase), UNIQUE_TOKEN.Length);

            return htmlText;
        }

        /// <summary>
        /// Gets the body text for a given url
        /// </summary>
        /// <param name="url">The url to get the text for</param>
        /// <param name="timeout">The request timeout, in MS</param>
        /// <returns></returns>
        public static IHTMLDocument2 GetHTMLDocumentForUrl(string url, int timeout)
        {
            return GetHTMLDocumentForUrl(url, timeout, SilentProgressHost.Instance);
        }

        /// <summary>
        /// Gets the body text for a given url
        /// </summary>
        /// <param name="url">The url to get the text for</param>
        /// <param name="timeout">The request timeout, in MS</param>
        /// <returns></returns>
        public static IHTMLDocument2 GetHTMLDocumentForUrl(string url, int timeout, IProgressHost progressHost)
        {
            WebRequestWithCache wr = new WebRequestWithCache(url);

            // return the html document
            return GetHTMLDocumentFromStream(wr.GetResponseStream(WebRequestWithCache.CacheSettings.CHECKCACHE, timeout), url);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sourceUrl"></param>
        /// <returns></returns>
        public static IHTMLDocument2 GetHTMLDocumentFromStream(Stream stream, string sourceUrl)
        {
            string filePath = TempFileManager.Instance.CreateTempFile();
            using (FileStream file = new FileStream(filePath, FileMode.Open))
                StreamHelper.Transfer(stream, file);

            return GetHTMLDocumentFromFile(filePath, sourceUrl);
        }

        /// <summary>
        /// Gets an IHTMLDocument2 from a stream of html
        /// </summary>
        /// <param name="filePath">The filePath of html</param>
        /// <param name="sourceUrl">The source url for the document</param>
        /// <returns>The IHTMLDocument2</returns>
        public static IHTMLDocument2 GetHTMLDocumentFromFile(string filePath, string sourceUrl)
        {
            IHTMLDocument2 htmlDoc = null;
            Encoding currentEncoding = Encoding.Default;
            using (StreamReader reader = new StreamReader(filePath))
            {
                htmlDoc = StringToHTMLDoc(reader.ReadToEnd(), sourceUrl, false);
                currentEncoding = reader.CurrentEncoding;
            }

            // If there no dom, just return null
            if (htmlDoc != null)
            {
                // If there is no metadata that disagrees with our encoding, just return the DOM read with default decoding
                HTMLMetaData metaData = new HTMLMetaData(htmlDoc);
                if (metaData != null && metaData.Charset != null)
                {
                    try
                    {
                        // The decoding is different than the encoding used to read this document, reread it with correct encoding
                        Encoding encoding = Encoding.GetEncoding(metaData.Charset);
                        if (encoding != currentEncoding)
                        {
                            using (StreamReader reader = new StreamReader(filePath, encoding))
                            {
                                htmlDoc = StringToHTMLDoc(reader.ReadToEnd(), sourceUrl, false);
                            }
                        }
                    }
                    catch (NotSupportedException)
                    {
                        // The encoding isn't supported on this system
                    }
                    catch (ArgumentException)
                    {
                        // The encoding isn't an encoding that the OS even knows about (its probably
                        // not well formatted or misspelled or something)
                    }
                }
            }

            return htmlDoc;
        }

        /// <summary>
        /// Gets an IHTMLDocument2 from a stream of html
        /// </summary>
        /// <param name="filePath">The filePath of html</param>
        /// <param name="sourceUrl">The source url for the document</param>
        /// <returns>The IHTMLDocument2</returns>
        public static IHTMLDocument2 SafeGetHTMLDocumentFromUrl(string url, out string responseUri)
        {
            using (Stream stream = HttpRequestHelper.SafeDownloadFile(url, out responseUri))
            {
                if (stream != null)
                    return GetHTMLDocumentFromStream(stream, url);
                else
                    return null;
            }
        }

        /// <summary>
        /// This creates an HTML string that should be correctly formatted with matching closing tags, etc. . .
        /// </summary>
        /// <param name="html">The html to format</param>
        /// <returns>The normalized html</returns>
        public static string NormalizeHTML(string html)
        {
            IHTMLDocument2 document = StringToHTMLDoc(html, null);
            return document.body.innerHTML;
        }

        /// <summary>
        /// Gets the body text for a given Url
        /// </summary>
        /// <param name="url">The url</param>
        /// <param name="timeout">The timeout, in ms</param>
        /// <param name="useCache">Whether to use the cache</param>
        /// <returns>The body text</returns>
        public static string GetBodyTextForUrl(string url, int timeout)
        {
            IHTMLDocument2 document = GetHTMLDocumentForUrl(url, timeout);
            string text = null;
            if (document != null)
                text = HTMLToPlainText(HTMLDocToString(document));
            return text;
        }

        /// <summary>
        /// Determines whether an html document contains dangerous scripts
        /// </summary>
        /// <param name="document">The IHTMLDocument2 to check</param>
        /// <returns>true if the page contains dangerous scripts, otherwise false</returns>
        public static bool ContainsDangerousScripts(IHTMLDocument2 document)
        {
            IHTMLElementCollection scripts = document.scripts;
            foreach (IHTMLScriptElement script in scripts)
            {
                // Scripts that modify the location of the page are dangerous- they
                // will likely redirect a page
                if (Regex.Match(script.text, @"location[^;]*=") != Match.Empty ||
                    Regex.Match(script.text, @"location\.(assign|replace)") != Match.Empty)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Converts an HTML string into an IHTMLDocument2
        /// </summary>
        /// <param name="html">The HTML string to convert into an IHTMLDocument2</param>
        /// <returns>The IHTMLDocument2 created from the string.  Note that any script statements are
        /// stripped from the string before converting into an HTML doc (this prevent scripts errors
        /// that result from an incomplete DOM)</returns>
        public static IHTMLDocument2 StringToHTMLDoc(string html, string baseUrl)
        {
            return StringToHTMLDoc(html, baseUrl, true);
        }

        /// <summary>
        /// Gets an IHTMLElement collection of the links in a given document
        /// </summary>
        /// <param name="document">The document</param>
        /// <returns>An IHTMLElementCollection containing the links</returns>
        public static IHTMLElementCollection GetLinksForDocument(IHTMLDocument2 document)
        {
            IHTMLDocument3 html3 = (IHTMLDocument3)document;
            return html3.getElementsByTagName("A");
        }

        public static IHTMLElementCollection GetImageMapsLinksForDocument(IHTMLDocument2 document)
        {
            IHTMLDocument3 html3 = (IHTMLDocument3)document;
            return html3.getElementsByTagName("AREA");
        }


        /// <summary>
        /// Converts an HTML string into an IHTMLDocument2
        /// </summary>
        /// <param name="html">The HTML string to convert into an IHTMLDocument2</param>
        /// <returns>The IHTMLDocument2 created from the string.  Note that any script statements are
        /// stripped from the string before converting into an HTML doc (this prevent scripts errors
        /// that result from an incomplete DOM)</returns>
        public static IHTMLDocument2 StringToHTMLDoc(string html, string baseUrl, bool escapeNonResourcePaths)
        {
            return StringToHTMLDoc(html, baseUrl, escapeNonResourcePaths, false);
        }

        /// <summary>
        /// Converts an HTML string into an IHTMLDocument2
        /// </summary>
        /// <param name="html">The HTML string to convert into an IHTMLDocument2</param>
        /// <returns>The IHTMLDocument2 created from the string.  Note that any script statements are
        /// stripped from the string before converting into an HTML doc (this prevent scripts errors
        /// that result from an incomplete DOM)</returns>
        public static IHTMLDocument2 StringToHTMLDoc(string html, string baseUrl, bool escapeNonResourcePaths, bool escapeResourcePaths)
        {
            // Write the escaped HTML to the document and return it
            IHTMLDocument2 htmlDoc = (IHTMLDocument2)new HTMLDocumentClass();

            // We call InitNew on the Document so later mail can call IPersistStreamInit->Save (if we don't
            // call InitNew(), Save will fail)
            ((IPersistStreamInit)htmlDoc).InitNew();

            // Protect the <meta name="Generator" ...> tag by executing the IDM_PROTECTMETATAGS command.
            object input = true;
            Guid MshtmlCommandGroupGuid = new Guid("DE4BA900-59CA-11CF-9592-444553540000");
            const uint IDM_PROTECTMETATAGS = 7101;
            IOleCommandTargetNullOutputParam commandTarget = (IOleCommandTargetNullOutputParam)htmlDoc;
            commandTarget.Exec(MshtmlCommandGroupGuid, IDM_PROTECTMETATAGS, OLECMDEXECOPT.DODEFAULT, ref input, IntPtr.Zero);

            // Disable VML by wrapping the document in a service provider.
            IOleObject oleObject = (IOleObject)htmlDoc;
            oleObject.SetClientSite(new VersionHostServiceProvider(new DisableVmlVersionHost()));

            // Enabling design stops scripts from executing (which prevents jscript errors)
            htmlDoc.designMode = "On";
            htmlDoc.write(html);
            htmlDoc.close();

            if (escapeNonResourcePaths && baseUrl != null)
                EscapeNonResourceRelativePaths(htmlDoc, baseUrl);

            if (escapeResourcePaths && baseUrl != null)
                EscapeResourceRelativePaths(htmlDoc, baseUrl);

            return htmlDoc;
        }

        public static IHTMLDocument2 StreamToHTMLDoc(Stream stream, string baseUrl, bool escapePaths)
        {
            if (!stream.CanSeek)
            {
                MemoryStream mStream = new MemoryStream();
                StreamHelper.Transfer(stream, mStream);
                mStream.Seek(0, SeekOrigin.Begin);
                stream = mStream;
            }
            string htmlContent = null;
            Encoding currentEncoding = Encoding.Default;
            LightWeightHTMLDocument lwDoc = null;
            using (StreamReader reader = new StreamReader(stream, currentEncoding))
            {
                htmlContent = reader.ReadToEnd();
                lwDoc = LightWeightHTMLDocument.FromString(htmlContent, baseUrl, true);

                // If there is no metadata that disagrees with our encoding, just return the DOM read with default decoding
                LightWeightHTMLMetaData metaData = new LightWeightHTMLMetaData(lwDoc);
                if (metaData != null && metaData.Charset != null)
                {
                    try
                    {
                        // The decoding is different than the encoding used to read this document, reread it with correct encoding
                        Encoding encoding = Encoding.GetEncoding(metaData.Charset);
                        if (encoding != currentEncoding)
                        {
                            reader.DiscardBufferedData();
                            stream.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader2 = new StreamReader(stream, encoding))
                            {
                                htmlContent = reader2.ReadToEnd();
                            }
                        }
                    }
                    catch (NotSupportedException)
                    {
                        // The encoding isn't supported on this system
                    }
                    catch (ArgumentException)
                    {
                        // The encoding isn't an encoding that the OS even knows about (its probably
                        // not well formatted or misspelled or something)
                    }
                }
            }

            //now that the html content is in loaded in the right encoding, convert it into a document.
            IHTMLDocument2 doc2 = StringToHTMLDoc(htmlContent, baseUrl, escapePaths, escapePaths);
            return doc2;
        }

        /// <summary>
        /// Reads HTML content from a stream with proper encoding detection (WebView2-compatible).
        /// This is the same logic as StreamToHTMLDoc but returns a string instead of IHTMLDocument2.
        /// </summary>
        /// <param name="stream">The stream containing HTML content</param>
        /// <param name="baseUrl">The base URL for the document</param>
        /// <returns>The HTML content as a string with correct encoding</returns>
        public static string StreamToHTMLString(Stream stream, string baseUrl)
        {
            if (!stream.CanSeek)
            {
                MemoryStream mStream = new MemoryStream();
                StreamHelper.Transfer(stream, mStream);
                mStream.Seek(0, SeekOrigin.Begin);
                stream = mStream;
            }
            
            string htmlContent = null;
            Encoding currentEncoding = Encoding.Default;
            
            using (StreamReader reader = new StreamReader(stream, currentEncoding))
            {
                htmlContent = reader.ReadToEnd();
                LightWeightHTMLDocument lwDoc = LightWeightHTMLDocument.FromString(htmlContent, baseUrl, true);

                // If there is metadata that specifies a different encoding, re-read with that encoding
                LightWeightHTMLMetaData metaData = new LightWeightHTMLMetaData(lwDoc);
                if (metaData != null && metaData.Charset != null)
                {
                    try
                    {
                        Encoding encoding = Encoding.GetEncoding(metaData.Charset);
                        if (encoding != currentEncoding)
                        {
                            reader.DiscardBufferedData();
                            stream.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader2 = new StreamReader(stream, encoding))
                            {
                                htmlContent = reader2.ReadToEnd();
                            }
                        }
                    }
                    catch (NotSupportedException)
                    {
                        // The encoding isn't supported on this system
                    }
                    catch (ArgumentException)
                    {
                        // The encoding isn't an encoding that the OS even knows about
                    }
                }
            }

            return htmlContent;
        }

        /// <summary>
        /// Gets a string representing the HTML selection in a html document
        /// </summary>
        /// <param name="document">The document from which to select</param>
        /// <returns>The HTML string</returns>
        public static string GetSelection(IHTMLDocument2 document)
        {
            IHTMLTxtRange textRange = (IHTMLTxtRange)document.selection.createRange();
            return textRange.htmlText;
        }

        /// <summary>
        /// Trys to find the base url given an html document
        /// </summary>
        /// <param name="document">The IHTMLDocument2</param>
        /// <returns>The base url, null if there is no base url</returns>
        public static string GetBaseUrlFromDocument(IHTMLDocument2 document)
        {
            string baseUrl = null;
            // Try to find the base tag in the document
            IHTMLDocument3 document3 = (IHTMLDocument3)document;
            IHTMLElementCollection baseTags = document3.getElementsByTagName("Base");
            if (baseTags.length > 0)
            {
                IEnumerator tagEnum = baseTags.GetEnumerator();
                tagEnum.MoveNext();
                IHTMLBaseElement baseElement = tagEnum.Current as IHTMLBaseElement;
                if (baseElement != null && baseElement.href != string.Empty)
                {
                    baseUrl = baseElement.href;
                }
            }
            return baseUrl;
        }

        public static string GetBaseUrl(string html, string defaultUrl)
        {
            string url = defaultUrl;
            HtmlExtractor extractor = new HtmlExtractor(html);
            if (extractor.Seek("<base href>").Success)
            {
                string newUrl = ((BeginTag)extractor.Element).GetAttributeValue("href");
                if (UrlHelper.IsUrl(newUrl))
                    url = newUrl;
            }
            return url;
        }

        /// <summary>
        /// Structure of special headers that may begin the HTML at the beginning of an
        /// HTMLDocument, but be omitted from the DOM
        /// </summary>
        public class SpecialHeaders
        {
            public string DocType;
            public string SavedFrom;
        }

        public static SpecialHeaders GetSpecialHeaders(IHTMLDocument2 document)
        {
            SpecialHeaders specialHeaders = new SpecialHeaders();
            IHTMLDocument3 doc3 = document as IHTMLDocument3;
            if (doc3 != null)
            {
                IHTMLDOMNode node = doc3.documentElement as IHTMLDOMNode;
                while (node.previousSibling != null)
                {
                    node = node.previousSibling;

                    IHTMLCommentElement commentElement = node as IHTMLCommentElement;
                    if (commentElement != null)
                    {
                        if (commentElement.text.StartsWith(docTypeMatch, StringComparison.OrdinalIgnoreCase))
                            specialHeaders.DocType = commentElement.text;

                        if (commentElement.text.StartsWith(savedFromMatch, StringComparison.OrdinalIgnoreCase))
                            specialHeaders.SavedFrom = commentElement.text;
                    }
                }

            }
            return specialHeaders;
        }

        public static SpecialHeaders GetSpecialHeaders(string html, string url)
        {
            SpecialHeaders headers = null;
            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(html);
            writer.Flush();
            stream.Position = 0;
            headers = GetSpecialHeaders(stream, url);

            return headers;
        }

        public static string AddMarkOfTheWeb(String html, String webUrl)
        {
            Regex docType = new Regex("<!DOCTYPE[^>]*>");
            Regex savedFrom = new Regex("<!-- saved from url.* -->");

            //remove the existing savedFrom
            Match m = savedFrom.Match(html);
            if (m.Success)
            {
                html = html.Remove(m.Index, m.Length);
            }

            int markOffset = 0;
            m = docType.Match(html);
            if (m.Success && html.Substring(0, m.Index).Trim() == String.Empty)
            {
                markOffset = m.Index + m.Length;
            }

            String markOfTheWeb = UrlHelper.GetSavedFromString(webUrl);
            html = html.Insert(markOffset, markOfTheWeb);

            if (markOffset == 0)
            {
                //prepend a default docType declaration (fixes bug 487389)
                html = DEFAULT_MOTW_DOCTYPE + "\r\n" + html;
            }

            return html;
        }
        private const String DEFAULT_MOTW_DOCTYPE = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";

        /// <summary>
        /// Gets the document type string for a given URL (note that this could actually download the page!)
        /// </summary>
        /// <param name="url">The url for which to get the document type</param>
        /// <returns>The entire document type string</returns>
        public static SpecialHeaders GetSpecialHeaders(Stream stream, string url)
        {
            SpecialHeaders specialHeaders = new SpecialHeaders();
            using (StreamReader reader = new StreamReader(stream))
            {
                // Read character by character until we hit the first brace
                // Doctype is always required to be the first tag in the page!
                StringBuilder builder = new StringBuilder();

                bool keepReading = true;
                char c;
                int i = 0;
                int foundCount = 0;

                while (keepReading)
                {
                    c = (char)reader.Read();
                    builder.Append(c);
                    if (c == '>')
                    {
                        string potentialMatch = builder.ToString().Trim();
                        if (potentialMatch.StartsWith(docTypeMatch, StringComparison.OrdinalIgnoreCase))
                            specialHeaders.DocType = potentialMatch;
                        else if (potentialMatch.StartsWith(savedFromMatch, StringComparison.OrdinalIgnoreCase))
                            specialHeaders.SavedFrom = potentialMatch;

                        builder = new StringBuilder();
                        foundCount++;
                    }

                    if (i > 1024 || foundCount > 2)
                        keepReading = false;

                    i++;
                }

            }

            return specialHeaders;
        }

        /// <summary>
        /// Gets the document type string for a given URL (note that this could actually download the page!)
        /// </summary>
        /// <param name="url">The url for which to get the document type</param>
        /// <returns>The entire document type string</returns>
        public static SpecialHeaders GetSpecialHeaders(string url)
        {
            WebRequestWithCache wr = new WebRequestWithCache(url);
            Stream stream = Stream.Null;
            if (UrlHelper.IsFileUrl(url) && UrlHelper.IsUrl(url) && PathHelper.IsWebPage(new Uri(url).LocalPath))
                stream = new FileStream(new Uri(url).LocalPath, FileMode.Open, FileAccess.Read);
            else
                stream = wr.GetResponseStream(WebRequestWithCache.CacheSettings.CACHEONLY, 5000);

            return GetSpecialHeaders(stream, url);
        }
        private const string docTypeMatch = "<!DOCTYPE";
        private const string savedFromMatch = "<!-- SAVED FROM URL=";

        /// <summary>
        /// Converts an IHTMLDocument2 into a HTML string
        /// </summary>
        /// <param name="document">The IHTMLDocument2</param>
        /// <returns>The HTML string</returns>
        public static string HTMLDocToString(IHTMLDocument2 document)
        {
            IHTMLDocument3 finalDocument = (IHTMLDocument3)document;

            try
            {
                IHTMLElement rootElement = finalDocument.documentElement;
                return rootElement.outerHTML;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Converts HTML Strings to their plain text representation by stripping
        /// out HTML tags.
        /// </summary>
        /// <param name="html">The HTML string to convert to plain text</param>
        /// <returns>The plain text</returns>
        public static string HTMLToPlainText(string html)
        {
            return HTMLToPlainText(html, false);
        }

        public static string HTMLToPlainText(string html, bool forIndexing)
        {
            return HtmlUtils.HTMLToPlainText(html, forIndexing);
        }

        [Obsolete("Use HtmlUtils.EscapeEntities", true)]
        public static string EscapeEntities(string plaintext)
        {
            return HtmlUtils.EscapeEntities(plaintext);
        }

        [Obsolete("Use HtmlUtils.EscapeEntity", true)]
        public static string EscapeEntity(char c)
        {
            return HtmlUtils.EscapeEntity(c);
        }

        [Obsolete("Use HtmlUtils.UnEscapeEntities", true)]
        public static string UnEscapeEntities(string html)
        {
            return HtmlUtils.UnEscapeEntities(html);
        }

        /// <summary>
        /// Resource Elements are elements that can be downloaded when a page or snippet is captured
        /// </summary>
        private static Hashtable ResourceElements
        {
            get
            {
                if (m_resourceElements == null)
                {
                    m_resourceElements = new Hashtable();
                    m_resourceElements.Add(HTMLTokens.Img, HTMLTokens.Src);
                    m_resourceElements.Add(HTMLTokens.Link, HTMLTokens.Href);
                    m_resourceElements.Add(HTMLTokens.Object, HTMLTokens.Src);
                    m_resourceElements.Add(HTMLTokens.Embed, HTMLTokens.Src);
                    m_resourceElements.Add(HTMLTokens.Script, HTMLTokens.Src);
                    m_resourceElements.Add(HTMLTokens.Body, HTMLTokens.Background);
                    m_resourceElements.Add(HTMLTokens.Input, HTMLTokens.Src);
                    m_resourceElements.Add(HTMLTokens.Td, HTMLTokens.Background);
                    m_resourceElements.Add(HTMLTokens.Tr, HTMLTokens.Background);
                    m_resourceElements.Add(HTMLTokens.Table, HTMLTokens.Background);
                }
                return m_resourceElements;
            }
        }
        private static Hashtable m_resourceElements = null;

        /// <summary>
        /// User visible elements are references in a page that are visible to a user
        /// </summary>
        private static Hashtable UserVisibleElements
        {
            get
            {
                if (m_userVisibleElements == null)
                {
                    m_userVisibleElements = new Hashtable();
                    m_userVisibleElements.Add(HTMLTokens.Img, HTMLTokens.Src);
                    m_userVisibleElements.Add(HTMLTokens.Object, HTMLTokens.Src);
                    m_userVisibleElements.Add(HTMLTokens.Embed, HTMLTokens.Src);
                    m_userVisibleElements.Add(HTMLTokens.Body, HTMLTokens.Background);
                    m_userVisibleElements.Add(HTMLTokens.Input, HTMLTokens.Src);
                    m_userVisibleElements.Add(HTMLTokens.Td, HTMLTokens.Background);
                    m_userVisibleElements.Add(HTMLTokens.Tr, HTMLTokens.Background);
                    m_userVisibleElements.Add(HTMLTokens.Table, HTMLTokens.Background);
                }
                return m_userVisibleElements;
            }
        }
        private static Hashtable m_userVisibleElements = null;

        /// <summary>
        /// Non resource elements are elements that cannot be downloaded when a page or snippet is captured
        /// </summary>
        private static Hashtable NonResourceElements
        {
            get
            {
                if (m_nonResourceElements == null)
                {
                    m_nonResourceElements = new Hashtable();
                    m_nonResourceElements.Add(HTMLTokens.Form, HTMLTokens.Action);
                    m_nonResourceElements.Add(HTMLTokens.A, HTMLTokens.Href);
                    m_nonResourceElements.Add(HTMLTokens.Area, HTMLTokens.Href);
                }
                return m_nonResourceElements;
            }
        }
        private static Hashtable m_nonResourceElements = null;

        private static Hashtable IframeElements
        {
            get
            {
                if (m_iframeElements == null)
                {
                    m_iframeElements = new Hashtable();
                    m_iframeElements.Add(HTMLTokens.IFrame, HTMLTokens.Src);
                }
                return m_iframeElements;
            }
        }
        private static Hashtable m_iframeElements = null;

        /// <summary>
        /// Gets a list of the iframe items in an HTMLDocument
        /// The list contains one entry per referenced item no matter how many times that item
        /// is referenced by the document.
        /// </summary>
        /// <param name="htmlDocument">The IHTMLDocument2 for which to get the resources</param>
        /// <returns>A ArrayList of the iframe elements</returns>
        public static ArrayList GetIframeElementsFromDocument(IHTMLDocument2 htmlDocument)
        {
            ArrayList resources = new ArrayList();
            IEnumerator elementCollectionEnum = GetElementCollection(htmlDocument, IframeElements).GetEnumerator();
            while (elementCollectionEnum.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)elementCollectionEnum.Current;
                AddAttributesToList((string)entry.Value, (IHTMLElementCollection)entry.Key, resources);
            }
            return resources;

        }

        /// <summary>
        /// Escapes relative paths in a HTMLDocument
        /// </summary>
        /// <param name="htmlDocument">The HTMLDocument in which to escape relative paths</param>
        /// <returns>an IHTMLDocument2 with absolute paths for all paths (except frames / iframes)</returns>
        public static void EscapeNonResourceRelativePaths(IHTMLDocument2 htmlDocument, string baseUrl)
        {
            IHTMLElementCollection collection = htmlDocument.all;

            // Special handling for params
            IHTMLElementCollection elements = (IHTMLElementCollection)collection.tags(HTMLTokens.Param);
            foreach (IHTMLElement param in elements)
            {
                string relativePath = HTMLDocumentHelper.GetParamValue(param, new string[] { HTMLTokens.Movie, HTMLTokens.Src });
                if (relativePath != null)
                {
                    if (relativePath == string.Empty)
                        relativePath = "\"\"";

                    param.outerHTML =
                        param.outerHTML.Replace(relativePath, UrlHelper.EscapeRelativeURL(baseUrl, relativePath));
                }
            }

            // Iterate through and escape the relative paths
            IEnumerator elementCollectionEnum = GetElementCollection(htmlDocument, NonResourceElements).GetEnumerator();
            while (elementCollectionEnum.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)elementCollectionEnum.Current;
                ResetPaths((string)entry.Value, (IHTMLElementCollection)entry.Key, baseUrl);
            }
        }

        /// <summary>
        /// Escapes relative paths in a HTMLDocument
        /// </summary>
        /// <param name="htmlDocument">The HTMLDocument in which to escape relative paths</param>
        /// <returns>an IHTMLDocument2 with absolute paths for all paths (except frames / iframes)</returns>
        public static void EscapeResourceRelativePaths(IHTMLDocument2 htmlDocument, string baseUrl)
        {
            // Iterate through and escape the relative paths
            IEnumerator elementCollectionEnum = GetElementCollection(htmlDocument, ResourceElements).GetEnumerator();
            while (elementCollectionEnum.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)elementCollectionEnum.Current;
                ResetPaths((string)entry.Value, (IHTMLElementCollection)entry.Key, baseUrl);
            }
        }

        public static string[] GetResourceRelativePathsFromStyleSheetText(string styleSheetText)
        {
            ArrayList matchingUrls = new ArrayList();
            string pattern = @"url\(.*?\)";
            MatchCollection matches = Regex.Matches(styleSheetText, pattern);
            foreach (Match match in matches)
            {
                string matchString = styleSheetText.Substring(match.Index);
                if (matchString != null)
                    matchString = matchString.Trim();

                matchingUrls.Add(matchString.Substring(4, matchString.Length - 5));
            }
            return (string[])matchingUrls.ToArray(typeof(string));

        }

        /// <summary>
        /// Gets a list of the referenced items in an HTMLDocument (things like images, etc).
        /// The list contains one entry per referenced item no matter how many times that item
        /// is referenced by the document.
        /// </summary>
        /// <param name="htmlDocument">The IHTMLDocument2 for which to get the resources</param>
        /// <returns>A ArrayList of the resources</returns>
        public static ArrayList GetResourceUrlsFromDocument(IHTMLDocument2 htmlDocument)
        {
            return GetResourcesFromDocument(htmlDocument, ResourceElements);
        }

        /// <summary>
        /// Gets a list of the user visible references items in an HTMLDocument.
        /// The list contains one entry per referenced item no matter how many times that item
        /// is referenced by the document.
        /// </summary>
        /// <param name="htmlDocument">The IHTMLDocument2 for which to get the resources</param>
        /// <returns>A ArrayList of the resources</returns>
        public static ResourceUrlInfo[] GetUserVisibleResourcesFromDocument(IHTMLDocument2 htmlDocument)
        {
            return (ResourceUrlInfo[])GetResourcesFromDocument(htmlDocument, UserVisibleElements).ToArray(typeof(ResourceUrlInfo));
        }

        private static ArrayList GetResourcesFromDocument(IHTMLDocument2 htmlDocument, Hashtable resourceElements)
        {
            ArrayList resources = new ArrayList();

            IHTMLElementCollection collection = htmlDocument.all;
            IHTMLElementCollection elements = (IHTMLElementCollection)collection.tags(HTMLTokens.Param);
            foreach (IHTMLElement param in elements)
            {
                string relativePath = HTMLDocumentHelper.GetParamValue(param, new string[] { HTMLTokens.Movie, HTMLTokens.Src });
                if (relativePath != null)
                {
                    AddAttributeToList("VALUE", param, resources);
                }
            }

            IHTMLElementCollection embeds = (IHTMLElementCollection)collection.tags(HTMLTokens.Embed);
            foreach (IHTMLElement embed in embeds)
            {
                AddAttributeToList("SRC", embed, resources);
            }

            IEnumerator elementCollectionEnum = GetElementCollection(htmlDocument, resourceElements).GetEnumerator();
            while (elementCollectionEnum.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)elementCollectionEnum.Current;
                AddAttributesToList((string)entry.Value, (IHTMLElementCollection)entry.Key, resources);
            }

            AddStyleReferencesToList(resources, htmlDocument, htmlDocument.url);

            return resources;
        }

        public static ResourceUrlInfo[] GetStyleReferencesForDocument(IHTMLDocument2 document, string baseUrl)
        {
            ArrayList list = new ArrayList();
            AddStyleReferencesToList(list, document, baseUrl);
            return (ResourceUrlInfo[])list.ToArray(typeof(ResourceUrlInfo));
        }

        /// <summary>
        /// Helper that adds files referenced within the styles to the list of resources
        /// </summary>
        /// <param name="list">The list to add the new entries to</param>
        /// <param name="htmlDocument">The htmlDocument in which to find the style references</param>
        private static void AddStyleReferencesToList(ArrayList list, IHTMLDocument2 htmlDocument, string baseUrl)
        {
            IHTMLStyleSheetsCollection styleSheets = (IHTMLStyleSheetsCollection)htmlDocument.styleSheets;
            IEnumerator styleEnum = styleSheets.GetEnumerator();
            while (styleEnum.MoveNext())
            {
                IHTMLStyleSheet styleSheet = (IHTMLStyleSheet)styleEnum.Current;
                AddSheetReferencesToList(list, styleSheet, baseUrl);
            }
        }

        private static void AddSheetReferencesToList(ArrayList list, IHTMLStyleSheet styleSheet, string baseUrl)
        {
            AddSheetReferencesToList(list, styleSheet, baseUrl, 0);
        }

        // Depth is here because of an IE bug-
        /*
            index.htm
            <style>
                @import(url1.css)
            </style>

            url1.css
            @import(url2.css)

            url2.css
            @import(url3.css)

        Enumerating the import statement in url2.css will cause an out of memory exception
        and destabilize / crash IE.  The depth restriction causes us to skip imports that
        are at that depth or deeper.
        */
        private static void AddSheetReferencesToList(ArrayList list, IHTMLStyleSheet styleSheet, string baseUrl, int depth)
        {
            try
            {

                if (styleSheet.href != null)
                    baseUrl = UrlHelper.EscapeRelativeURL(baseUrl, styleSheet.href);

                // handle style sheet imports
                if (styleSheet.imports.length > 0 && depth < 2)
                {
                    IEnumerator importEnum = styleSheet.imports.GetEnumerator();
                    while (importEnum.MoveNext())
                    {
                        // Add this style sheet to the reference list
                        IHTMLStyleSheet importSheet = (IHTMLStyleSheet)importEnum.Current;
                        string sheetPath = importSheet.href;
                        if (baseUrl != "about:blank" && !UrlHelper.IsUrl(importSheet.href))
                            sheetPath = UrlHelper.EscapeRelativeURL(baseUrl, importSheet.href);

                        ResourceUrlInfo urlInfo = new ResourceUrlInfo();
                        urlInfo.ResourceUrl = importSheet.href;
                        urlInfo.ResourceAbsoluteUrl = sheetPath;
                        urlInfo.ResourceType = HTMLTokens.Style;
                        list.Add(urlInfo);

                        // Add the sheets references to the list
                        AddSheetReferencesToList(list, importSheet, baseUrl, depth + 1);
                    }
                }

                IHTMLStyleSheetRulesCollection rules = (IHTMLStyleSheetRulesCollection)styleSheet.rules;
                for (int i = 0; i < rules.length; i++)
                {
                    IHTMLStyleSheetRule rule = (IHTMLStyleSheetRule)rules.item(i);
                    IHTMLRuleStyle styleRule = (IHTMLRuleStyle)rule.style;

                    AddStyleReference(baseUrl, list, styleRule.backgroundImage);
                    AddStyleReference(baseUrl, list, styleRule.background);
                    AddStyleReference(baseUrl, list, styleRule.listStyleImage);
                    AddStyleReference(baseUrl, list, styleRule.listStyle);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // We weren't permitted to access the style sheets, try manual parse
            }
            catch (Exception ex)
            {
                Trace.Fail("An unexpected exception occurred while attempting to enumerate stylesheet references. " + ex.ToString());
                // IE is a total pile, I hate it
            }
        }

        private static void AddStyleReference(string baseUrl, ArrayList list, string ruleName)
        {
            string listImagePath = GetPathFromStyleUrl(ruleName);
            if (listImagePath != null)
            {
                ResourceUrlInfo urlInfo = new ResourceUrlInfo();
                urlInfo.ResourceUrl = listImagePath;
                urlInfo.ResourceAbsoluteUrl = UrlHelper.EscapeRelativeURL(baseUrl, listImagePath);
                urlInfo.ResourceType = HTMLTokens.Img;
                list.Add(urlInfo);
            }
        }

        private static string GetPathFromStyleUrl(string style)
        {
            if (style == null)
                return null;

            string path = null;
            int urlPosition = style.IndexOf("URL", StringComparison.OrdinalIgnoreCase);
            if (urlPosition > -1)
            {
                int firstQuote = style.IndexOf("(", urlPosition, StringComparison.OrdinalIgnoreCase);
                int lastQuote = style.LastIndexOf(")", StringComparison.OrdinalIgnoreCase);
                if (firstQuote > -1 && lastQuote > -1)
                {
                    firstQuote++;
                    path = style.Substring(firstQuote, lastQuote - firstQuote);
                }
            }
            return path;
        }

        /// <summary>
        /// Information about the url
        /// </summary>
        public class ResourceUrlInfo
        {
            public string ResourceUrl;
            public string ResourceType;
            public string ResourceAbsoluteUrl;
            public string InnerText;
        }

        /// <summary>
        /// Uses a hashtable of tags and the corresponding attributes to create a hashtable of all
        /// matching elements in an HTMLDocument
        /// </summary>
        /// <param name="htmlDocument">The IHTMLDocument2 for which to get the elements</param>
        /// <param name="Elements">A Hashtable of resource or nonresource elements</param>
        /// <returns>A hashtable of html elements and the corresponding attribute</returns>
        private static Hashtable GetElementCollection(IHTMLDocument2 htmlDocument, Hashtable Elements)
        {
            IHTMLElementCollection allHtmlElements = htmlDocument.all;
            Hashtable elementCollection = new Hashtable();
            IEnumerator enumElements = Elements.GetEnumerator();
            while (enumElements.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)enumElements.Current;
                elementCollection.Add((IHTMLElementCollection)allHtmlElements.tags(entry.Key), entry.Value);
            }
            return elementCollection;
        }

        /// <summary>
        /// Escapes characters with ascii values between 128 and 256 with
        /// their HTML escape characters (i.e. &#149;)
        /// </summary>
        /// <param name="chars">The characters to parse and escape</param>
        /// <returns>The HTML string with escaped characters</returns>
        public static string EscapeHighAscii(char[] chars)
        {
            // Escape high-ascii characters
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (c > 127 && c < 256)
                {
                    builder.Append("&#" + Convert.ToInt32(c) + ";");
                }
                else
                    builder.Append(c);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Escapes unicode characters and replaces them with the HTML escaped representation
        /// </summary>
        /// <param name="chars">The characters to escape</param>
        /// <returns>The escaped HTML string</returns>
        public static string EscapeUnicodeCharacters(char[] chars)
        {
            StringBuilder outHtml = new StringBuilder();
            for (int i = 0; i < chars.Length; i++)
            {
                if ((int)chars[i] < 127)
                {
                    outHtml.Append(chars[i]);
                }
                else
                    outHtml.Append("&#" + Convert.ToInt32(chars[i]) + ";");
            }
            return outHtml.ToString();
        }

        public static string EscapeUnicodeCharacters(string str)
        {
            if (str == null)
                return null;
            else
                return EscapeUnicodeCharacters(str.ToCharArray());
        }

        /// <summary>
        /// Check whether the passed HTML element contains the specified attribute
        /// </summary>
        /// <param name="element">element to check</param>
        /// <param name="attribute">name of attribute</param>
        /// <returns>true if the element has the attribute, else false</returns>
        public static bool ElementHasAttribute(IHTMLElement element, string attribute)
        {
            if (element != null)
            {
                object attrib = element.getAttribute(attribute, 0);
                return attrib != DBNull.Value;
            }
            else
            {
                return false;
            }
        }

        private static void ResetPaths(string attributeName, IHTMLElementCollection elements, string baseUrl)
        {
            foreach (IHTMLElement element in elements)
            {
                ResetPath(attributeName, element, baseUrl);
            }
        }

        private static void ResetPath(string attributeName, IHTMLElement element, string baseUrl)
        {
            // For this element, try to find the attribute containing a relative path
            string relativePath = null;

            Object pathObject = element.getAttribute(attributeName, HTMLAttributeFlags.DoNotEscapePaths);
            if (pathObject != DBNull.Value)
            {
                if (pathObject is String)
                    relativePath = (string)pathObject;
            }

            // If a relative path was discovered and its not an internal anchor, reset it
            if (relativePath != null && !relativePath.StartsWith("#", StringComparison.OrdinalIgnoreCase) && !UrlHelper.IsUrl(relativePath))
            {
                // Reset the value of the attribute to the escaped Path
                element.setAttribute(attributeName,
                UrlHelper.EscapeRelativeURL(baseUrl, relativePath),
                HTMLAttributeFlags.CaseInSensitive);
            }
        }

        private static void AddAttributesToList(string attributeName, IHTMLElementCollection elements, ArrayList resources)
        {
            foreach (IHTMLElement element in elements)
            {
                AddAttributeToList(attributeName, element, resources);
            }
        }

        private static void AddAttributeToList(string attributeName, IHTMLElement element, ArrayList resources)
        {
            // For this element, try to find the attribute containing a relative path
            string path = null;

            Object pathObject = element.getAttribute(attributeName, HTMLAttributeFlags.DoNotEscapePaths);
            if (pathObject != DBNull.Value)
            {
                path = (string)pathObject;
            }

            // If a valid path was discovered, add it to the list
            if (path != null)
            {
                ResourceUrlInfo info = new ResourceUrlInfo();
                info.ResourceUrl = path;
                info.ResourceType = element.tagName;
                info.InnerText = element.innerText;
                resources.Add(info);
            }
        }

        private static string GetParamValue(IHTMLElement param, string[] attributesToSearch)
        {
            string relativePath = null;
            Object name = param.getAttribute("NAME", HTMLAttributeFlags.CaseInSensitive);
            if (name != DBNull.Value)
            {
                // For this element, try to find the attribute containing the value
                for (int i = 0; i < attributesToSearch.Length; i++)
                {
                    if (((string)name).ToUpper(CultureInfo.InvariantCulture) == attributesToSearch[i].ToUpper(CultureInfo.InvariantCulture))
                    {
                        Object pathObject = param.getAttribute("VALUE", HTMLAttributeFlags.CaseInSensitive);
                        if (pathObject != DBNull.Value)
                        {
                            relativePath = (string)pathObject;
                        }
                    }
                }
            }
            return relativePath;
        }

#if DISABLE_SCRIPT_INJECTION
        /// <summary>
        /// Insert a new top level scripting object into the runtime environment of the specified
        /// HTML document.
        /// </summary>
        /// <param name="document">document to insert into</param>
        /// <param name="objectName">top-level name to refer to the object in script</param>
        /// <param name="obj">object to insert (must be marked with ComVisible attribute)</param>
        public static void InjectObjectIntoScriptingEnvironment(IHTMLDocument2 document, string objectName, object obj)
        {
            // ensure that it is marked com-visible
            ComVisibleAttribute[] comVisibleAttributes = (ComVisibleAttribute[])obj.GetType().GetCustomAttributes(typeof(ComVisibleAttribute), false);
            if (comVisibleAttributes.Length == 0 || !comVisibleAttributes[0].Value)
            {
                Debug.Fail("Objects inserted into IHTMLDocument2 scripting environment must be marked with the ComVisible(true) attribute");
                return;
            }

            // implementation constants
            const uint fdexNameEnsure = 0x00000002;
            const uint LOCALE_USER_DEFAULT = 1024;
            const Int16 DISPATCH_PROPERTYPUT = 4;
            const Int32 DISPID_PROPERTYPUT = -3;

            // buffers we will allocate (must free before exiting)
            IntPtr pPropertyPut = IntPtr.Zero;
            IntPtr pObject = IntPtr.Zero;
            try
            {
                // get pointer to IDispatchEx
                object script = document.Script;
                IDispatchEx dispatchEx = (IDispatchEx)script;

                // insert the object into the scripting environment (requires several steps)

                // insert a new dispatch-id for the object
                IntPtr dispId;
                dispatchEx.GetDispID(objectName, fdexNameEnsure, out dispId);

                // initialize structure used to pass the object in
                System.Runtime.InteropServices.ComTypes.DISPPARAMS dispParams = new System.Runtime.InteropServices.ComTypes.DISPPARAMS();
                dispParams.cArgs = 1;
                dispParams.cNamedArgs = 1;

                // indicate that this call to InvokeEx is a property put
                pPropertyPut = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Int32)));
                Marshal.WriteInt32(pPropertyPut, DISPID_PROPERTYPUT);
                dispParams.rgdispidNamedArgs = pPropertyPut;

                // specify the object to pass to InvokeEx (we think the size of a VARIANT is ~14
                // bytes but we initialize WAY more memory just to be safe)
                pObject = Marshal.AllocCoTaskMem(256);
                Marshal.GetNativeVariantForObject(obj, pObject);
                dispParams.rgvarg = pObject;

                // initialize error structure
                System.Runtime.InteropServices.ComTypes.EXCEPINFO ei = new System.Runtime.InteropServices.ComTypes.EXCEPINFO();

                // set the object into the specified objectName (creates a new top level scripting object)
                dispatchEx.InvokeEx(
                    dispId,					// disp-id of objectName
                    LOCALE_USER_DEFAULT,	// use default locale (should this be en-US?)
                    DISPATCH_PROPERTYPUT,   // specify a property set operation
                    ref dispParams,			// parameters to pass (value to set property to)
                    IntPtr.Zero,			// pointer to result (no result for property put)
                    ref ei,					// pointer to exception structure
                    IntPtr.Zero);			// optional service provider not specified
            }
            finally
            {
                if (pPropertyPut != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pPropertyPut);
                if (pObject != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pObject);
            }
        }
#endif

        /// <summary>
        /// Flags that control how tag attribute matching is performed when using getAttribute on an HTMLDOMNode
        /// </summary>
        public struct HTMLAttributeFlags
        {
            public const int CaseInSensitive = 0;
            public const int CaseSensitive = 1;
            public const int DoNotEscapePaths = 2;
        }

        public static string MonikerToString(IMoniker moniker, uint codepage, out string url)
        {
            // Create binding context that will be needed to get the url from the moniker
            IBindCtx bindCtx;
            int hr = Ole32.CreateBindCtx(0, out bindCtx);
            if (hr != HRESULT.S_OK)
                throw new COMException("Error creating binding context", hr);

            // Get the url of the moniker
            string name;
            moniker.GetDisplayName(bindCtx, null, out name);
            url = name;

            // Get a stream to the content of the url
            IStream stream;
            ComHelper.Chk(UrlMon.URLOpenBlockingStream(IntPtr.Zero, name, out stream, 0, null));

            // Read the contents of the url, which should be the html to an email message
            using (ComStream comStream = new ComStream(stream, false))
            {

                using (StreamReader sr = new StreamReader(comStream, Encoding.GetEncoding((int)codepage)))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static bool IsQuirksMode(IHTMLDocument2 htmlDocument)
        {
            return ((IHTMLDocument5)htmlDocument).compatMode.Equals("BackCompat", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Event args used to describe events that occur within HTML documents
    /// </summary>
    public class HtmlDocumentEventArgs : EventArgs
    {
        public HtmlDocumentEventArgs(IHTMLEventObj htmlEventObj)
        {
            _HTMLEventObj = htmlEventObj;
        }

        /// <summary>
        /// Underlying HTML document event that caused this event
        /// </summary>
        public IHTMLEventObj HTMLEventObj
        {
            get
            {
                return _HTMLEventObj;
            }
        }
        private readonly IHTMLEventObj _HTMLEventObj;

        /// <summary>
        /// Flag which indicates whether the event was handled (if true
        /// then further processing of the event is suppressed)
        /// </summary>
        public bool Handled = false;
    }

    /// <summary>
    /// Event that occurs within an HTML document
    /// </summary>
    public delegate void HtmlDocumentEventHandler(HtmlDocumentEventArgs ea);

}
