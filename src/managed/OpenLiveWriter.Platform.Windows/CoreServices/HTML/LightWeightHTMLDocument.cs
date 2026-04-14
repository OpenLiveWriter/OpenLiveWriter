// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using mshtml;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.SHDocVw;

namespace OpenLiveWriter.CoreServices
{
    public class UrlInfo
    {
        public string TagName
        {
            get { return _tagName; }
        }

        public string Url
        {
            get { return _url; }
        }

        public string Name
        {
            get
            {
                if (_name == null || _name.Trim() == string.Empty)
                {
                    _name = UrlHelper.GetFileNameWithoutExtensionForUrl(Url);
                    if (_name == string.Empty)
                        _name = Url;
                }
                return _name;
            }
        }
        private string _name;
        private string _tagName;
        private string _url;

        public UrlInfo(string url, string tagName) : this(null, url, tagName)
        {

        }

        public UrlInfo(string name, string url, string tagName)
        {
            _name = name;
            if (_name != null)
            {
                _name = _name.Replace("\r\n", " ");
                _name = _name.Replace("\n", " ");
                _name = _name.Replace("\r", " ");
            }
            _url = url;
            _tagName = tagName;
        }
    }

    public class LightWeightTag
    {
        public LightWeightTag(BeginTag tag)
        {
            _beginTag = tag;
        }
        private BeginTag _beginTag = null;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        private string _name;

        public BeginTag BeginTag
        {
            get
            {
                return _beginTag;
            }
        }
    }

    /// <summary>
    /// Summary description for LightweightHTMLDocument.
    /// </summary>
    public class LightWeightHTMLDocument : LightWeightHTMLDocumentIterator
    {
        public static LightWeightHTMLDocument FromString(string html, string baseUrl)
        {
            return LightWeightHTMLDocument.FromString(html, baseUrl, true);
        }

        public static LightWeightHTMLDocument FromString(string html, string baseUrl, bool escapePaths)
        {
            return FromString(html, baseUrl, null, escapePaths);
        }

        public static LightWeightHTMLDocument FromString(string html, string baseUrl, string name, bool escapePaths)
        {
            string escapedHtml = html;
            if (escapePaths)
                escapedHtml = LightWeightHTMLUrlToAbsolute.ConvertToAbsolute(html, baseUrl);

            LightWeightHTMLDocument escapedDocument = new LightWeightHTMLDocument(escapedHtml, baseUrl, name);
            HTMLDocumentHelper.SpecialHeaders specialHeaders = HTMLDocumentHelper.GetSpecialHeaders(escapedHtml, baseUrl);
            escapedDocument._docType = specialHeaders.DocType;
            escapedDocument._savedFrom = specialHeaders.SavedFrom;
            escapedDocument.Parse();
            return escapedDocument;
        }

        public static LightWeightHTMLDocument FromStream(Stream stream, string url)
        {
            return FromStream(stream, url, null);
        }

        public static LightWeightHTMLDocument FromStream(Stream stream, string url, string name)
        {
            if (!stream.CanSeek)
            {
                string filePath = TempFileManager.Instance.CreateTempFile();
                using (FileStream file = new FileStream(filePath, FileMode.Open))
                    StreamHelper.Transfer(stream, file);

                return LightWeightHTMLDocument.FromFile(filePath, url, name);
            }
            else
            {
                Encoding currentEncoding = Encoding.Default;
                LightWeightHTMLDocument lwDoc = null;
                using (StreamReader reader = new StreamReader(stream, currentEncoding))
                {
                    lwDoc = LightWeightHTMLDocument.FromString(reader.ReadToEnd(), url, name, true);

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
                                    lwDoc = LightWeightHTMLDocument.FromString(reader2.ReadToEnd(), url, name, true);
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

                return lwDoc;
            }
        }

        public static LightWeightHTMLDocument FromFile(string filePath, string url)
        {
            return FromFile(filePath, url, null);
        }

        public static LightWeightHTMLDocument FromFile(string filePath, string url, string name)
        {
            using (Stream s = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192))
            {
                return FromStream(s, url, name);
            }
        }

        public static LightWeightHTMLDocument FromIHTMLDocument2(IHTMLDocument2 htmlDocument, string url)
        {
            if (htmlDocument == null)
                return null;

            return LightWeightHTMLDocument.FromIHTMLDocument2(htmlDocument, url, true);
        }

        public static LightWeightHTMLDocument FromIHTMLDocument2(IHTMLDocument2 htmlDocument, string url, bool escapePaths)
        {
            if (htmlDocument == null)
                return null;

            return LightWeightHTMLDocument.FromIHTMLDocument2(htmlDocument, url, null, escapePaths);
        }

        private static LightWeightHTMLDocument FromIHTMLDocument2(IHTMLDocument2 htmlDocument, string url, string name)
        {
            return LightWeightHTMLDocument.FromIHTMLDocument2(htmlDocument, url, name, true);
        }

        public static LightWeightHTMLDocument FromIHTMLDocument2(IHTMLDocument2 htmlDocument, string url, bool escapePaths, bool escapeEmptyString)
        {
            if (htmlDocument == null)
                return null;

            return LightWeightHTMLDocument.FromIHTMLDocument2(htmlDocument, url, null, escapePaths, escapeEmptyString);
        }

        private static LightWeightHTMLDocument FromIHTMLDocument2(IHTMLDocument2 htmlDocument, string url, string name, bool escapePaths)
        {
            return LightWeightHTMLDocument.FromIHTMLDocument2(htmlDocument, url, name, escapePaths, true);
        }

        private static LightWeightHTMLDocument FromIHTMLDocument2(IHTMLDocument2 htmlDocument, string url, string name, bool escapePaths, bool escapeEmptyString)
        {
            string escapedHtml = HTMLDocumentHelper.HTMLDocToString(htmlDocument);
            if (escapedHtml == null)
                return null;

            if (escapePaths)
                escapedHtml = LightWeightHTMLUrlToAbsolute.ConvertToAbsolute(escapedHtml, url, true, escapeEmptyString);

            LightWeightHTMLDocument finalDocument = new LightWeightHTMLDocument(escapedHtml, url, name);

            // Set the Frames
            finalDocument.SetFrames(GetLightWeightDocumentForFrames(htmlDocument));

            // Set the styles
            finalDocument.SetStyleReferences(HTMLDocumentHelper.GetStyleReferencesForDocument(htmlDocument, url));

            // Set the DocType
            HTMLDocumentHelper.SpecialHeaders specialHeaders = HTMLDocumentHelper.GetSpecialHeaders(htmlDocument);
            finalDocument._docType = specialHeaders.DocType;
            finalDocument._savedFrom = specialHeaders.SavedFrom;

            finalDocument.Parse();
            return finalDocument;
        }

        public static LightWeightHTMLDocument[] GetLightWeightDocumentForFrames(IHTMLDocument2 htmlDocument)
        {
            ArrayList frameLightWeightDocuments = new ArrayList();

            // Get the IOleContainer for the for the html document (this requires that
            // the document is the root document in the browser)
            IOleContainer oleContainer = (IOleContainer)htmlDocument;
            OpenLiveWriter.Interop.Com.IEnumUnknown enumUnknown;

            // Enumerate the controls in the browser
            oleContainer.EnumObjects(OLECONTF.EMBEDDINGS, out enumUnknown);

            // Iterate through the controls
            object unknown;
            for (int i = 0; HRESULT.S_OK == enumUnknown.Next(1, out unknown, IntPtr.Zero); i++)
            {
                // Only subframes should cast to IWebBrowser2
                IWebBrowser2 webBrowser = unknown as IWebBrowser2;

                // Since it is a subframe, we can also get the base frame implementation for it
                IHTMLFrameBase frameBase = unknown as IHTMLFrameBase;

                // It's a frame, add this to the list!
                if (webBrowser != null)
                {
                    try
                    {
                        IHTMLDocument2 frameDocument = webBrowser.Document as IHTMLDocument2;

                        if (frameDocument != null)
                        {
                            LightWeightHTMLDocument document = LightWeightHTMLDocument.FromIHTMLDocument2(frameDocument, frameDocument.url, frameBase.name);
                            if (document != null)
                                frameLightWeightDocuments.Add(document);
                        }
                    }
                    catch (InvalidCastException)
                    {

                        string html = "<HTML></HTML>";
                        LightWeightHTMLDocument document = LightWeightHTMLDocument.FromString(html, webBrowser.LocationURL, webBrowser.LocationURL, true);
                        if (document != null)
                            frameLightWeightDocuments.Add(document);
                    }
                }
            }
            return (LightWeightHTMLDocument[])frameLightWeightDocuments.ToArray(typeof(LightWeightHTMLDocument));
        }

        public LightWeightTag[] GetTagsByName(string name)
        {
            string key = name.ToUpper(CultureInfo.InvariantCulture);
            if (_tagTable.ContainsKey(key))
                return (LightWeightTag[])_tagTable[key];
            else
                return new LightWeightTag[0];
        }

        private Hashtable _tagTable = new Hashtable();

        public LightWeightHTMLDocument[] Frames
        {
            get
            {
                return _frames;
            }
        }

        public HTMLDocumentHelper.ResourceUrlInfo[] StyleResourcesUrls
        {
            get
            {
                return _styleResourceUrls;
            }
        }

        public string DocType
        {
            get
            {
                return _docType;
            }
        }
        private string _docType = null;

        public string ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }
        private string _contentType = null;

        public string SavedFrom
        {
            get
            {
                return _savedFrom;
            }
        }
        private string _savedFrom = null;

        /// Changed this from private to public
        public void SetFrames(LightWeightHTMLDocument[] frames)
        {
            _frames = frames;
        }

        public void UpdateBasedUponHTMLDocumentData(IHTMLDocument2 document, string baseUrl)
        {
            if (_frames == null)
                SetFrames(GetLightWeightDocumentForFrames(document));

            if (_styleResourceUrls == null)
                SetStyleReferences(HTMLDocumentHelper.GetStyleReferencesForDocument(document, baseUrl));
        }

        /// <summary>
        /// Updates frames and style information based on HTML content string (WebView2-compatible).
        /// Note: Frame contents cannot be extracted from HTML string alone - only frame URLs.
        /// </summary>
        /// <param name="html">The HTML content string</param>
        /// <param name="baseUrl">The base URL for resolving relative paths</param>
        public void UpdateBasedUponHTMLContent(string html, string baseUrl)
        {
            // For frames, we can only extract frame tag info, not actual frame content
            // The caller will need to download frame content separately if needed
            if (_frames == null)
            {
                SetFrames(GetFrameDocumentsFromHtml(html, baseUrl));
            }

            if (_styleResourceUrls == null)
            {
                SetStyleReferences(GetStyleReferencesFromHtml(html, baseUrl));
            }
        }

        /// <summary>
        /// Extracts frame URLs and creates placeholder LightWeightHTMLDocuments from HTML string.
        /// </summary>
        private static LightWeightHTMLDocument[] GetFrameDocumentsFromHtml(string html, string baseUrl)
        {
            ArrayList frameDocuments = new ArrayList();
            LightWeightHTMLDocument tempDoc = new LightWeightHTMLDocument(html, baseUrl, null);
            tempDoc.Parse();

            // Get FRAME tags
            foreach (LightWeightTag tag in tempDoc.GetTagsByName(HTMLTokens.Frame))
            {
                string src = tag.BeginTag.GetAttributeValue("src");
                string name = tag.BeginTag.GetAttributeValue("name");
                if (!string.IsNullOrEmpty(src))
                {
                    string absoluteUrl = UrlHelper.EscapeRelativeURL(baseUrl, src);
                    // Create placeholder document - content will need to be downloaded separately
                    LightWeightHTMLDocument frameDoc = new LightWeightHTMLDocument(string.Empty, absoluteUrl, name);
                    frameDocuments.Add(frameDoc);
                }
            }

            // Get IFRAME tags
            foreach (LightWeightTag tag in tempDoc.GetTagsByName(HTMLTokens.IFrame))
            {
                string src = tag.BeginTag.GetAttributeValue("src");
                string name = tag.BeginTag.GetAttributeValue("name");
                if (!string.IsNullOrEmpty(src))
                {
                    string absoluteUrl = UrlHelper.EscapeRelativeURL(baseUrl, src);
                    // Create placeholder document - content will need to be downloaded separately
                    LightWeightHTMLDocument frameDoc = new LightWeightHTMLDocument(string.Empty, absoluteUrl, name);
                    frameDocuments.Add(frameDoc);
                }
            }

            return (LightWeightHTMLDocument[])frameDocuments.ToArray(typeof(LightWeightHTMLDocument));
        }

        /// <summary>
        /// Extracts style sheet references from HTML string (WebView2-compatible).
        /// Note: Only extracts external stylesheets from LINK tags. Inline @import is not supported
        /// as it would require parsing HTML content between tags which is complex.
        /// </summary>
        private static HTMLDocumentHelper.ResourceUrlInfo[] GetStyleReferencesFromHtml(string html, string baseUrl)
        {
            ArrayList styleUrls = new ArrayList();
            LightWeightHTMLDocument tempDoc = new LightWeightHTMLDocument(html, baseUrl, null);
            tempDoc.Parse();

            // Get LINK tags with rel="stylesheet"
            foreach (LightWeightTag tag in tempDoc.GetTagsByName(HTMLTokens.Link))
            {
                string rel = tag.BeginTag.GetAttributeValue("rel");
                string href = tag.BeginTag.GetAttributeValue("href");
                if (!string.IsNullOrEmpty(href) && 
                    (string.IsNullOrEmpty(rel) || rel.ToLower(CultureInfo.InvariantCulture).Contains("stylesheet")))
                {
                    string absoluteUrl = UrlHelper.EscapeRelativeURL(baseUrl, href);
                    var resourceInfo = new HTMLDocumentHelper.ResourceUrlInfo();
                    resourceInfo.ResourceUrl = href;
                    resourceInfo.ResourceAbsoluteUrl = absoluteUrl;
                    resourceInfo.ResourceType = "stylesheet";
                    styleUrls.Add(resourceInfo);
                }
            }

            return (HTMLDocumentHelper.ResourceUrlInfo[])styleUrls.ToArray(typeof(HTMLDocumentHelper.ResourceUrlInfo));
        }

        public void SetStyleReferences(HTMLDocumentHelper.ResourceUrlInfo[] styleResourceUrls)
        {
            _styleResourceUrls = styleResourceUrls;
        }
        private HTMLDocumentHelper.ResourceUrlInfo[] _styleResourceUrls = null;

        private LightWeightHTMLDocument[] _frames = null;

        public LightWeightHTMLDocument Clone()
        {
            return new LightWeightHTMLDocument(Html, Url);
        }

        public string Title
        {
            get
            {
                if (_title == null)
                    _title = Url;
                return _title;
            }
        }
        private string _title = null;

        public string Name
        {
            get
            {
                return _name;
            }
        }
        private string _name = null;

        public bool HasFramesOrStyles
        {
            get
            {
                if (!_hasFramesOrStylesInit)
                {
                    _hasFramesOrStyles = (GetTagsByName(HTMLTokens.Style).Length > 0 ||
                                            GetTagsByName(HTMLTokens.Frame).Length > 0 ||
                                            GetTagsByName(HTMLTokens.IFrame).Length > 0 ||
                                            GetTagsByName(HTMLTokens.Link).Length > 0);
                    _hasFramesOrStylesInit = true;
                }
                return _hasFramesOrStyles;
            }
        }
        private bool _hasFramesOrStyles = false;
        private bool _hasFramesOrStylesInit = false;

        public LightWeightHTMLMetaData MetaData
        {
            get
            {
                if (_metaData == null)
                    _metaData = new LightWeightHTMLMetaData(this);
                return _metaData;
            }
        }
        private LightWeightHTMLMetaData _metaData = null;

        public UrlInfo[] ResourceUrlInfos
        {
            get
            {
                if (!_generated)
                {
                    // Handle regular tags
                    ArrayList urls = GetUrlInfosForTable(ResourceElements);

                    // Handle Literals
                    foreach (UrlInfo urlInfo in _literalUrlInfos)
                    {
                        urls.Add(urlInfo);
                    }

                    // Handle Params
                    LightWeightTag[] paramTags = GetTagsByName(HTMLTokens.Param);
                    foreach (LightWeightTag param in paramTags)
                    {
                        foreach (string paramValue in ParamsUrlElements)
                        {
                            Attr attr = param.BeginTag.GetAttribute(HTMLTokens.Name);
                            if (attr != null)
                            {
                                if (attr.Value != null && attr.Value.ToUpperInvariant() == paramValue.ToUpperInvariant())
                                {
                                    Attr valueAttr = param.BeginTag.GetAttribute(HTMLTokens.Value);
                                    if (valueAttr != null)
                                        urls.Add(new UrlInfo(valueAttr.Value, HTMLTokens.Param));
                                }
                            }
                        }
                    }

                    // Handle Links
                    LightWeightTag[] linkTags = GetTagsByName(HTMLTokens.Link);
                    foreach (LightWeightTag link in linkTags)
                    {
                        foreach (string linkRelValue in LinksUrlElements)
                        {
                            Attr attr = link.BeginTag.GetAttribute(HTMLTokens.Rel);
                            if (attr != null)
                            {
                                if (attr.Value != null && attr.Value.ToLower(CultureInfo.InvariantCulture) == linkRelValue.ToLower(CultureInfo.InvariantCulture))
                                {
                                    Attr hrefAttr = link.BeginTag.GetAttribute(HTMLTokens.Href);
                                    if (hrefAttr != null)
                                        urls.Add(new UrlInfo(hrefAttr.Value, HTMLTokens.Link));

                                }
                            }
                        }
                    }

                    _resourceUrlInfos.AddRange(urls);
                    _generated = true;
                }
                return (UrlInfo[])_resourceUrlInfos.ToArray(typeof(UrlInfo));
            }
        }
        private ArrayList _resourceUrlInfos = new ArrayList();
        private bool _generated = false;

        public void AddReference(UrlInfo urlInfo)
        {
            if (!_resourceUrlInfos.Contains(urlInfo))
                _resourceUrlInfos.Add(urlInfo);
        }

        public UrlInfo[] UserVisibleUrlInfos
        {
            get
            {
                if (_userVisibleUrlInfos == null)
                    _userVisibleUrlInfos = (UrlInfo[])GetUrlInfosForTable(UserVisibleElements).ToArray(typeof(UrlInfo));
                return _userVisibleUrlInfos;
            }
        }
        private UrlInfo[] _userVisibleUrlInfos = null;

        public UrlInfo[] Anchors
        {
            get
            {
                if (_anchors == null)
                    _anchors = (UrlInfo[])GetUrlInfosForTable(AnchorElements).ToArray(typeof(UrlInfo));
                return _anchors;
            }
        }
        private UrlInfo[] _anchors = null;

        public UrlInfo[] NonResourceUrlInfos
        {
            get
            {
                if (_nonResourceUrlInfos == null)
                    _nonResourceUrlInfos = (UrlInfo[])GetUrlInfosForTable(NonResourceElements).ToArray(typeof(UrlInfo));
                return _nonResourceUrlInfos;
            }
        }
        private UrlInfo[] _nonResourceUrlInfos = null;

        public string GenerateHtml()
        {
            return Generator.DoReplace();
        }

        public string RawHtml
        {
            get
            {
                return this.Html;
            }
        }

        private LightWeightHTMLReplacer Generator
        {
            get
            {
                if (_generator == null)
                    _generator = new LightWeightHTMLReplacer(Html, Url, MetaData);
                return _generator;
            }
        }
        private LightWeightHTMLReplacer _generator = null;

        public void AddUrlToEscape(UrlToReplace urlToReplace)
        {
            Generator.AddUrlToReplace(urlToReplace);
        }

        protected LightWeightHTMLDocument(string html, string url) : this(html, url, null)
        {

        }

        protected LightWeightHTMLDocument(string html, string url, string name) : base(html)
        {
            _url = url;
            _name = name;
        }

        public string Url
        {
            get
            {
                if (_url == null)
                    _url = string.Empty;
                return _url;
            }
            set
            {
                _url = value;
            }
        }
        private string _url;

        public void AddSubstitionUrl(UrlToReplace urlToReplace)
        {
            Generator.AddUrlToReplace(urlToReplace);
        }

        private LightWeightHTMLDocument GetFrameDocumentByName(string name)
        {
            if (name == null)
                return null;

            foreach (LightWeightHTMLDocument frameDoc in _frames)
                if (frameDoc.Name == name)
                    return frameDoc;

            return null;
        }

        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag != null)
            {
                // Reset any frame urls
                // This is done because the HTML that is often in this document may have
                // incorrect urls for frames.  The frames enumeration is accurate, so if the
                // name from the frames enumeration is the same as this frame, we should fix its
                // url up.
                if (tag.NameEquals(HTMLTokens.Frame))
                {
                    Attr name = tag.GetAttribute(HTMLTokens.Name);
                    if (name != null && this._frames != null)
                    {
                        LightWeightHTMLDocument frameDoc = GetFrameDocumentByName(name.Value);
                        if (frameDoc != null)
                        {
                            Attr src = tag.GetAttribute(HTMLTokens.Src);
                            if (src != null && src.Value != frameDoc.Url)
                                Generator.AddSubstitionUrl(new UrlToReplace(src.Value, frameDoc.Url));
                        }
                    }
                }

                LightWeightTag currentTag = new LightWeightTag(tag);
                // The key we'll use for the table
                string key = tag.Name.ToUpper(CultureInfo.InvariantCulture);
                if (!_tagTable.ContainsKey(key))
                    _tagTable[key] = new LightWeightTag[0];

                LightWeightTag[] currentTags = (LightWeightTag[])_tagTable[key];
                LightWeightTag[] grownTags = new LightWeightTag[currentTags.Length + 1];
                currentTags.CopyTo(grownTags, 0);
                grownTags[currentTags.Length] = currentTag;
                _tagTable[key] = grownTags;

                // Accumulate the title text
                if (tag.NameEquals(HTMLTokens.Title) && !tag.Complete)
                    _nextTextIsTitleText = true;
                else if (tag.NameEquals(HTMLTokens.A) && !tag.Complete && tag.GetAttribute(HTMLTokens.Href) != null)
                {
                    if (_collectingForTag != null)
                    {
                        if (tag.NameEquals(HTMLTokens.A))
                            _collectingForTagDepth++;
                    }
                    else
                        _collectingForTag = currentTag;
                }

            }
            base.OnBeginTag(tag);
        }
        private bool _nextTextIsTitleText = false;
        private LightWeightTag _collectingForTag = null;
        private int _collectingForTagDepth = 0;

        protected override void OnEndTag(EndTag tag)
        {
            if (_collectingForTag != null)
            {
                if (tag.NameEquals(HTMLTokens.A))
                {
                    if (_collectingForTagDepth == 0)
                        _collectingForTag = null;
                    else
                        _collectingForTagDepth--;
                }
            }
            base.OnEndTag(tag);
        }

        protected override void OnText(Text text)
        {
            if (_nextTextIsTitleText)
            {
                _title = HttpUtility.HtmlDecode(text.ToString());
                _nextTextIsTitleText = false;
            }
            if (_collectingForTag != null)
                _collectingForTag.Name = _collectingForTag.Name + text.RawText;
            base.OnText(text);
        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {
            _literalUrlInfos.Add(new UrlInfo(styleUrl.LiteralText, HTMLTokens.Style));
            base.OnStyleUrl(styleUrl);
        }
        private ArrayList _literalUrlInfos = new ArrayList();

        protected override void OnStyleImport(StyleImport styleImport)
        {
            _styleImports.Add(new UrlInfo(styleImport.LiteralText, HTMLTokens.Import));
            base.OnStyleImport(styleImport);
        }
        private ArrayList _styleImports = new ArrayList();

        private ArrayList GetUrlInfosForTable(Hashtable elementTable)
        {
            ArrayList urlInfos = new ArrayList();
            foreach (string tagName in elementTable.Keys)
            {
                LightWeightTag[] tags = GetTagsByName(tagName);
                foreach (LightWeightTag tag in tags)
                {
                    string attrValue = tag.BeginTag.GetAttributeValue((string)elementTable[tagName]);

                    if (attrValue != null)
                    {
                        if (!UrlHelper.IsUrl(attrValue))
                            attrValue = UrlHelper.EscapeRelativeURL(_url, attrValue);
                        urlInfos.Add(new UrlInfo(tag.Name, attrValue, tag.BeginTag.Name));
                    }
                }
            }
            return urlInfos;
        }

        static LightWeightHTMLDocument()
        {
            m_resourceElements = new Hashtable();
            m_resourceElements.Add(HTMLTokens.Img, HTMLTokens.Src);
            m_resourceElements.Add(HTMLTokens.Object, HTMLTokens.Src);
            m_resourceElements.Add(HTMLTokens.Embed, HTMLTokens.Src);
            m_resourceElements.Add(HTMLTokens.Param, HTMLTokens.Value); //required for movies embedded using object tag
            m_resourceElements.Add(HTMLTokens.Script, HTMLTokens.Src);
            m_resourceElements.Add(HTMLTokens.Body, HTMLTokens.Background);
            m_resourceElements.Add(HTMLTokens.Input, HTMLTokens.Src);
            m_resourceElements.Add(HTMLTokens.Td, HTMLTokens.Background);
            m_resourceElements.Add(HTMLTokens.Tr, HTMLTokens.Background);
            m_resourceElements.Add(HTMLTokens.Table, HTMLTokens.Background);

            m_anchors = new Hashtable();
            m_anchors.Add(HTMLTokens.A, HTMLTokens.Href);
            m_anchors.Add(HTMLTokens.Area, HTMLTokens.Href);

            m_userVisibleElements = new Hashtable();
            m_userVisibleElements.Add(HTMLTokens.Img, HTMLTokens.Src);
            m_userVisibleElements.Add(HTMLTokens.Object, HTMLTokens.Src);
            m_userVisibleElements.Add(HTMLTokens.Embed, HTMLTokens.Src);
            m_userVisibleElements.Add(HTMLTokens.Body, HTMLTokens.Background);
            m_userVisibleElements.Add(HTMLTokens.Input, HTMLTokens.Src);
            m_userVisibleElements.Add(HTMLTokens.Td, HTMLTokens.Background);
            m_userVisibleElements.Add(HTMLTokens.Tr, HTMLTokens.Background);
            m_userVisibleElements.Add(HTMLTokens.Table, HTMLTokens.Background);

            m_nonResourceElements = new Hashtable();
            m_nonResourceElements.Add(HTMLTokens.Form, HTMLTokens.Action);
            m_nonResourceElements.Add(HTMLTokens.A, HTMLTokens.Href);
            m_nonResourceElements.Add(HTMLTokens.Link, HTMLTokens.Href);
            m_nonResourceElements.Add(HTMLTokens.Area, HTMLTokens.Href);

            _frameElements = new Hashtable();
            _frameElements.Add(HTMLTokens.IFrame, HTMLTokens.Src);
            _frameElements.Add(HTMLTokens.Frame, HTMLTokens.Src);

            _paramsUrlElements = new ArrayList();
            _paramsUrlElements.Add(HTMLTokens.Movie);
            _paramsUrlElements.Add(HTMLTokens.Src);

            _linkUrlElements = new ArrayList();
            _linkUrlElements.Add("Stylesheet");

            _allUrlElements = new Hashtable();
            foreach (string token in FrameElements.Keys)
                _allUrlElements.Add(token, FrameElements[token]);

            foreach (string token in NonResourceElements.Keys)
                _allUrlElements.Add(token, NonResourceElements[token]);

            foreach (string token in ResourceElements.Keys)
                _allUrlElements.Add(token, ResourceElements[token]);
        }

        /// <summary>
        /// Resource Elements are elements that can be downloaded when a page or snippet is captured
        /// </summary>
        public static Hashtable ResourceElements
        {
            get
            {
                return m_resourceElements;
            }
        }
        private static Hashtable m_resourceElements = null;

        public static Hashtable AnchorElements
        {
            get
            {
                return m_anchors;
            }
        }
        private static Hashtable m_anchors = null;

        /// <summary>
        /// User visible elements are references in a page that are visible to a user
        /// </summary>
        public static Hashtable UserVisibleElements
        {
            get
            {
                return m_userVisibleElements;
            }
        }
        private static Hashtable m_userVisibleElements = null;

        /// <summary>
        /// Non resource elements are elements that cannot be downloaded when a page or snippet is captured
        /// </summary>
        public static Hashtable NonResourceElements
        {
            get
            {
                return m_nonResourceElements;
            }
        }
        private static Hashtable m_nonResourceElements = null;

        public static Hashtable FrameElements
        {
            get
            {
                return _frameElements;
            }
        }
        private static Hashtable _frameElements = null;

        public static ArrayList ParamsUrlElements
        {
            get
            {
                return _paramsUrlElements;
            }
        }
        private static ArrayList _paramsUrlElements = null;

        public static ArrayList LinksUrlElements
        {
            get
            {
                return _linkUrlElements;
            }
        }
        private static ArrayList _linkUrlElements = null;

        public static Hashtable AllUrlElements
        {
            get
            {
                return _allUrlElements;
            }
        }
        private static Hashtable _allUrlElements = null;
    }
}
