// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using mshtml;

namespace OpenLiveWriter.BlogClient.Detection
{

    internal class WriterEditingManifest
    {
        public static WriterEditingManifest FromHomepage(LazyHomepageDownloader homepageDownloader, Uri homepageUri, IBlogClient blogClient, IBlogCredentialsAccessor credentials)
        {
            if (homepageUri == null)
                return null;

            WriterEditingManifest editingManifest = null;
            try
            {
                // compute the "by-convention" url for the manifest
                string homepageUrl = UrlHelper.InsureTrailingSlash(UrlHelper.SafeToAbsoluteUri(homepageUri));
                string manifestUrl = UrlHelper.UrlCombine(homepageUrl, "wlwmanifest.xml");

                // test to see whether this url exists and has a valid manifest
                editingManifest = FromUrl(new Uri(manifestUrl), blogClient, credentials, false);

                // if we still don't have one then scan homepage contents for a link tag
                if (editingManifest == null)
                {
                    string manifestLinkTagUrl = ScanHomepageContentsForManifestLink(homepageUri, homepageDownloader);
                    if (manifestLinkTagUrl != null)
                    {
                        // test to see whether this url exists and has a valid manifest
                        try
                        {
                            editingManifest = FromUrl(new Uri(manifestLinkTagUrl), blogClient, credentials, true);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Error attempting to download manifest from " + manifestLinkTagUrl + ": " + ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Unexpected exception attempting to discover manifest from " + UrlHelper.SafeToAbsoluteUri(homepageUri) + ": " + ex.ToString());
            }

            // return whatever editing manifest we found
            return editingManifest;
        }

        public static WriterEditingManifest FromUrl(Uri manifestUri, IBlogClient blogClient, IBlogCredentialsAccessor credentials, bool expectedAvailable)
        {
            return FromDownloadInfo(new WriterEditingManifestDownloadInfo(UrlHelper.SafeToAbsoluteUri(manifestUri)), blogClient, credentials, expectedAvailable);
        }

        public static WriterEditingManifest FromDownloadInfo(WriterEditingManifestDownloadInfo downloadInfo, IBlogClient blogClient, IBlogCredentialsAccessor credentials, bool expectedAvailable)
        {
            if (downloadInfo == null)
                return null;

            try
            {
                // if the manifest is not yet expired then don't try a download at all
                if (downloadInfo.Expires > DateTimeHelper.UtcNow)
                    return new WriterEditingManifest(downloadInfo);

                // execute the download
                HttpWebResponse response = null;
                try
                {
                    if (credentials != null)
                        response = blogClient.SendAuthenticatedHttpRequest(downloadInfo.SourceUrl, REQUEST_TIMEOUT, new HttpRequestFilter(new EditingManifestFilter(downloadInfo).Filter));
                    else
                        response = HttpRequestHelper.SendRequest(downloadInfo.SourceUrl, new HttpRequestFilter(new EditingManifestFilter(downloadInfo).Filter));
                }
                catch (WebException ex)
                {
                    // Not modified -- return ONLY an updated downloadInfo (not a document)
                    HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.NotModified)
                    {
                        return new WriterEditingManifest(
                            new WriterEditingManifestDownloadInfo(
                                downloadInfo.SourceUrl,
                                HttpRequestHelper.GetExpiresHeader(errorResponse),
                                downloadInfo.LastModified,
                                HttpRequestHelper.GetETagHeader(errorResponse)));
                    }
                    else
                        throw;
                }

                // read headers
                DateTime expires = HttpRequestHelper.GetExpiresHeader(response);
                DateTime lastModified = response.LastModified;
                string eTag = HttpRequestHelper.GetETagHeader(response);

                // read document
                using (Stream stream = response.GetResponseStream())
                {
                    XmlDocument manifestXmlDocument = new XmlDocument();
                    manifestXmlDocument.Load(stream);

                    // return the manifest
                    return new WriterEditingManifest(
                        new WriterEditingManifestDownloadInfo(downloadInfo.SourceUrl, expires, lastModified, eTag),
                        manifestXmlDocument,
                        blogClient,
                        credentials);
                }
            }
            catch (Exception ex)
            {
                if (expectedAvailable)
                {
                    Trace.WriteLine("Error attempting to download manifest from " + downloadInfo.SourceUrl + ": " + ex.ToString());
                }
                return null;
            }
        }

        private class EditingManifestFilter
        {
            private WriterEditingManifestDownloadInfo _previousDownloadInfo;

            public EditingManifestFilter(WriterEditingManifestDownloadInfo previousDownloadInfo)
            {
                _previousDownloadInfo = previousDownloadInfo;
            }

            public void Filter(HttpWebRequest contentRequest)
            {
                if (_previousDownloadInfo.LastModified != DateTime.MinValue)
                    contentRequest.IfModifiedSince = _previousDownloadInfo.LastModified;

                if (_previousDownloadInfo.ETag != String.Empty)
                    contentRequest.Headers.Add("If-None-Match", _previousDownloadInfo.ETag);
            }
        }

        public static WriterEditingManifest FromResource(string resourcePath)
        {
            try
            {
                using (MemoryStream manifestStream = new MemoryStream())
                {
                    ResourceHelper.SaveAssemblyResourceToStream(resourcePath, manifestStream);
                    manifestStream.Seek(0, SeekOrigin.Begin);
                    XmlDocument manifestXmlDocument = new XmlDocument();
                    manifestXmlDocument.Load(manifestStream);
                    return new WriterEditingManifest(new WriterEditingManifestDownloadInfo("Clients.Manifests"), manifestXmlDocument, null, null);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception attempting to load manifest from resource: " + ex.ToString());
                return null;
            }
        }

        public static string DiscoverUrl(string homepageUrl, IHTMLDocument2 weblogDOM)
        {
            if (weblogDOM == null)
                return String.Empty;

            try
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
                            if (linkRel != null && (linkRel.ToUpperInvariant().Equals("WLWMANIFEST")))
                            {
                                if (linkElement.href != null && linkElement.href != String.Empty)
                                    return UrlHelper.UrlCombineIfRelative(homepageUrl, linkElement.href);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error attempting to discover manifest URL: " + ex.ToString());
            }

            // couldn't find one
            return String.Empty;
        }

        public WriterEditingManifestDownloadInfo DownloadInfo
        {
            get
            {
                return _downloadInfo;
            }
        }
        private readonly WriterEditingManifestDownloadInfo _downloadInfo;

        public string ClientType
        {
            get
            {
                return _clientType;
            }
        }
        private readonly string _clientType = null;

        public byte[] Image
        {
            get
            {
                return _image;
            }
        }
        private readonly byte[] _image = null;

        public byte[] Watermark
        {
            get
            {
                return _watermark;
            }
        }
        private readonly byte[] _watermark = null;

        public IDictionary OptionOverrides
        {
            get
            {
                return _optionOverrides;
            }
        }
        private readonly IDictionary _optionOverrides = null;

        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get
            {
                return _buttonDescriptions;
            }
        }
        private readonly IBlogProviderButtonDescription[] _buttonDescriptions = null;

        public string WebLayoutUrl
        {
            get
            {
                return _webLayoutUrl;
            }
        }
        private readonly string _webLayoutUrl;

        public string WebPreviewUrl
        {
            get
            {
                return _webPreviewUrl;
            }
        }
        private readonly string _webPreviewUrl;

        private IBlogClient _blogClient = null;
        private IBlogCredentialsAccessor _credentials = null;

        private WriterEditingManifest(WriterEditingManifestDownloadInfo downloadInfo)
            : this(downloadInfo, null, null, null)
        {
        }

        private WriterEditingManifest(WriterEditingManifestDownloadInfo downloadInfo, XmlDocument xmlDocument, IBlogClient blogClient, IBlogCredentialsAccessor credentials)
        {
            // record blog client and credentials
            _blogClient = blogClient;
            _credentials = credentials;

            // record download info
            if (UrlHelper.IsUrl(downloadInfo.SourceUrl))
                _downloadInfo = downloadInfo;

            // only process an xml document if we got one
            if (xmlDocument == null)
                return;

            // create namespace manager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsmgr.AddNamespace("m", "http://schemas.microsoft.com/wlw/manifest/weblog");

            // throw if the root element is not manifest
            if (xmlDocument.DocumentElement.LocalName.ToUpperInvariant() != "MANIFEST")
                throw new ArgumentException("Not a valid writer editing manifest");

            // get button descriptions
            _buttonDescriptions = new IBlogProviderButtonDescription[] { };
            XmlNode buttonsNode = xmlDocument.SelectSingleNode("//m:buttons", nsmgr);
            if (buttonsNode != null)
            {
                ArrayList buttons = new ArrayList();

                foreach (XmlNode buttonNode in buttonsNode.SelectNodes("m:button", nsmgr))
                {
                    try
                    {
                        // id
                        string id = XmlHelper.NodeText(buttonNode.SelectSingleNode("m:id", nsmgr));
                        if (id == String.Empty)
                            throw new ArgumentException("Missing id field");

                        // title
                        string description = XmlHelper.NodeText(buttonNode.SelectSingleNode("m:text", nsmgr));
                        if (description == String.Empty)
                            throw new ArgumentException("Missing text field");

                        // imageUrl
                        string imageUrl = XmlHelper.NodeText(buttonNode.SelectSingleNode("m:imageUrl", nsmgr));
                        if (imageUrl == String.Empty)
                            throw new ArgumentException("Missing imageUrl field");

                        // download the image
                        Bitmap image = DownloadImage(imageUrl, downloadInfo.SourceUrl);

                        // clickUrl
                        string clickUrl = BlogClientHelper.GetAbsoluteUrl(XmlHelper.NodeText(buttonNode.SelectSingleNode("m:clickUrl", nsmgr)), downloadInfo.SourceUrl);

                        // contentUrl
                        string contentUrl = BlogClientHelper.GetAbsoluteUrl(XmlHelper.NodeText(buttonNode.SelectSingleNode("m:contentUrl", nsmgr)), downloadInfo.SourceUrl);

                        // contentDisplaySize
                        Size contentDisplaySize = XmlHelper.NodeSize(buttonNode.SelectSingleNode("m:contentDisplaySize", nsmgr), Size.Empty);

                        // button must have either clickUrl or hasContent
                        if (clickUrl == String.Empty && contentUrl == String.Empty)
                            throw new ArgumentException("Must either specify a clickUrl or contentUrl");

                        // notificationUrl
                        string notificationUrl = BlogClientHelper.GetAbsoluteUrl(XmlHelper.NodeText(buttonNode.SelectSingleNode("m:notificationUrl", nsmgr)), downloadInfo.SourceUrl);

                        // add the button
                        buttons.Add(new BlogProviderButtonDescription(id, imageUrl, image, description, clickUrl, contentUrl, contentDisplaySize, notificationUrl));
                    }
                    catch (Exception ex)
                    {
                        // buttons fail silently and are not "all or nothing"
                        Trace.WriteLine("Error occurred reading custom button description: " + ex.Message);
                    }
                }

                _buttonDescriptions = buttons.ToArray(typeof(IBlogProviderButtonDescription)) as IBlogProviderButtonDescription[];
            }

            // get options
            _optionOverrides = new Hashtable();
            AddOptionsFromNode(xmlDocument.SelectSingleNode("//m:weblog", nsmgr), _optionOverrides);
            AddOptionsFromNode(xmlDocument.SelectSingleNode("//m:options", nsmgr), _optionOverrides);
            AddOptionsFromNode(xmlDocument.SelectSingleNode("//m:apiOptions[@name='" + _blogClient.ProtocolName + "']", nsmgr), _optionOverrides);
            XmlNode defaultViewNode = xmlDocument.SelectSingleNode("//m:views/m:default", nsmgr);
            if (defaultViewNode != null)
                _optionOverrides["defaultView"] = XmlHelper.NodeText(defaultViewNode);

            // separate out client type
            const string CLIENT_TYPE = "clientType";
            if (_optionOverrides.Contains(CLIENT_TYPE))
            {
                string type = _optionOverrides[CLIENT_TYPE].ToString();
                if (ValidateClientType(type))
                    _clientType = type;
                _optionOverrides.Remove(CLIENT_TYPE);
            }

            // separate out image
            const string IMAGE_URL = "imageUrl";
            _image = GetImageBytes(IMAGE_URL, _optionOverrides, downloadInfo.SourceUrl, new Size(16, 16));
            _optionOverrides.Remove(IMAGE_URL);

            // separate out watermark image
            const string WATERMARK_IMAGE_URL = "watermarkImageUrl";
            _watermark = GetImageBytes(WATERMARK_IMAGE_URL, _optionOverrides, downloadInfo.SourceUrl, new Size(84, 84));
            _optionOverrides.Remove(WATERMARK_IMAGE_URL);

            // get templates
            XmlNode webLayoutUrlNode = xmlDocument.SelectSingleNode("//m:views/m:view[@type='WebLayout']/@src", nsmgr);
            if (webLayoutUrlNode != null)
            {
                string webLayoutUrl = XmlHelper.NodeText(webLayoutUrlNode);
                if (webLayoutUrl != String.Empty)
                    _webLayoutUrl = BlogClientHelper.GetAbsoluteUrl(webLayoutUrl, downloadInfo.SourceUrl);
            }
            XmlNode webPreviewUrlNode = xmlDocument.SelectSingleNode("//m:views/m:view[@type='WebPreview']/@src", nsmgr);
            if (webPreviewUrlNode != null)
            {
                string webPreviewUrl = XmlHelper.NodeText(webPreviewUrlNode);
                if (webPreviewUrl != String.Empty)
                    _webPreviewUrl = BlogClientHelper.GetAbsoluteUrl(webPreviewUrl, downloadInfo.SourceUrl);
            }
        }

        private void AddOptionsFromNode(XmlNode optionsNode, IDictionary optionOverrides)
        {
            if (optionsNode != null)
            {
                foreach (XmlNode node in optionsNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                        optionOverrides.Add(node.LocalName, node.InnerText.Trim());
                }
            }
        }

        private bool ValidateClientType(string clientType)
        {
            clientType = clientType.Trim().ToUpperInvariant();
            return clientType == "METAWEBLOG" || clientType == "MOVABLETYPE" || clientType == "WORDPRESS";
        }

        private byte[] GetImageBytes(string elementName, IDictionary options, string basePath, Size requiredSize)
        {
            byte[] imageBytes = null;
            string imageUrl = options[elementName] as string;
            if (imageUrl != null && imageUrl != String.Empty)
            {
                try
                {
                    Bitmap bitmap = DownloadImage(imageUrl, basePath);
                    ImageFormat bitmapFormat = bitmap.RawFormat;
                    if (requiredSize != Size.Empty && bitmap.Size != requiredSize)
                    {
                        // shrink or grow the bitmap as appropriate
                        Bitmap correctedBitmap = new Bitmap(bitmap, requiredSize);

                        // dispose the original
                        bitmap.Dispose();

                        // return corrected
                        bitmap = correctedBitmap;
                    }

                    imageBytes = ImageHelper.GetBitmapBytes(bitmap, bitmapFormat);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Unexpected error downloading image from {0}: {1}", imageUrl, ex.ToString()));
                }
            }
            else
            {
                // indicates that no attempt to provide us an image was made
                imageBytes = new byte[0];
            }
            return imageBytes;
        }

        private Bitmap DownloadImage(string imageUrl, string basePath)
        {
            // non-url base path means embedded resource
            if (!UrlHelper.IsUrl(basePath))
            {
                string imagePath = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", basePath, imageUrl);
                Bitmap image = ResourceHelper.LoadAssemblyResourceBitmap(imagePath);
                if (image != null)
                    return image;
                else
                    throw new ArgumentException("Invalid Image Resource Path: " + imageUrl);
            }
            else
            {
                // calculate the image url
                imageUrl = UrlHelper.UrlCombineIfRelative(basePath, imageUrl);

                // try to get the credentials context
                WinInetCredentialsContext credentialsContext = null;
                try
                {
                    credentialsContext = BlogClientHelper.GetCredentialsContext(_blogClient, _credentials, imageUrl);
                }
                catch (BlogClientOperationCancelledException)
                {
                }

                // download the image
                return ImageHelper.DownloadBitmap(imageUrl, credentialsContext);
            }
        }

        private static string ScanHomepageContentsForManifestLink(Uri homepageUri, LazyHomepageDownloader homepageDownloader)
        {
            if (homepageDownloader.HtmlDocument != null)
            {
                LightWeightTag[] linkTags = homepageDownloader.HtmlDocument.GetTagsByName(HTMLTokens.Link);
                foreach (LightWeightTag linkTag in linkTags)
                {
                    string rel = linkTag.BeginTag.GetAttributeValue("rel");
                    string href = linkTag.BeginTag.GetAttributeValue("href");

                    if (rel != null && (rel.Trim().ToUpperInvariant().Equals("WLWMANIFEST") && href != null))
                    {
                        return UrlHelper.UrlCombineIfRelative(UrlHelper.SafeToAbsoluteUri(homepageUri), href);
                    }
                }
            }

            // didn't find it
            return null;
        }

        // 20-second request timeout
        private const int REQUEST_TIMEOUT = 20000;

    }

}
