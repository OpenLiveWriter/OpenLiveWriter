// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Globalization;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Detection
{
    public interface ISelfConfiguringClient : IBlogClient
    {
        void DetectSettings(IBlogSettingsDetectionContext context, BlogSettingsDetector detector);
    }

    public interface IBlogSettingsDetectionContext
    {
        string HomepageUrl { get; }
        string HostBlogId { get; }
        string PostApiUrl { get; }
        IBlogCredentialsAccessor Credentials { get; }
        string ProviderId { get; }
        IDictionary UserOptionOverrides { get; }

        WriterEditingManifestDownloadInfo ManifestDownloadInfo { get; set; }
        string ClientType { get; set; }
        byte[] FavIcon { get; set; }
        byte[] Image { get; set; }
        byte[] WatermarkImage { get; set; }
        BlogPostCategory[] Categories { get; set; }
        BlogPostKeyword[] Keywords { get; set; }
        IDictionary OptionOverrides { get; set; }
        IDictionary HomePageOverrides { get; set; }
        IBlogProviderButtonDescription[] ButtonDescriptions { get; set; }
    }

    public interface ITemporaryBlogSettingsDetectionContext : IBlogSettingsDetectionContext
    {
        BlogInfo[] AvailableImageEndpoints { get; set; }
    }

    public interface IBlogSettingsDetectionContextForCategorySchemeHack : IBlogSettingsDetectionContext
    {
        string InitialCategoryScheme { get; }
    }

    internal interface IBlogClientForCategorySchemeHack : IBlogClient
    {
        string DefaultCategoryScheme { set; }
    }

    public class BlogSettingsDetector
    {
        /// <summary>
        /// Create a blog client based on all of the context we currently have available
        /// </summary>
        /// <returns></returns>
        private IBlogClient CreateBlogClient()
        {
            return BlogClientManager.CreateClient(_context.ClientType, _context.PostApiUrl, _context.Credentials, _context.ProviderId, _context.OptionOverrides, _context.UserOptionOverrides, _context.HomePageOverrides);
        }

        private HttpWebResponse ExecuteHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
        {
            return CreateBlogClient().SendAuthenticatedHttpRequest(requestUri, timeoutMs, filter);
        }

        public BlogSettingsDetector(IBlogSettingsDetectionContext context)
        {
            // save the context
            _context = context;

            _homepageAccessor = new LazyHomepageDownloader(_context.HomepageUrl, new HttpRequestHandler(ExecuteHttpRequest));
        }

        public bool SilentMode
        {
            get { return _silentMode; }
            set { _silentMode = value; }
        }
        private bool _silentMode = false;

        public bool IncludeFavIcon
        {
            get { return _includeFavIcon && _context.Image == null; }
            set { _includeFavIcon = value; }
        }
        private bool _includeFavIcon = true;

        public bool IncludeImageEndpoints
        {
            get { return _includeImageEndpoints; }
            set { _includeImageEndpoints = value; }
        }
        private bool _includeImageEndpoints = true;

        public bool IncludeCategories
        {
            get { return _includeCategories; }
            set { _includeCategories = value; }
        }
        private bool _includeCategories = true;

        public bool IncludeCategoryScheme
        {
            get { return _includeCategoryScheme; }
            set { _includeCategoryScheme = value; }
        }
        private bool _includeCategoryScheme = true;

        public bool IncludeOptionOverrides
        {
            get { return _includeOptionOverrides; }
            set { _includeOptionOverrides = value; }
        }
        private bool _includeOptionOverrides = true;

        public bool IncludeInsecureOperations
        {
            get { return _includeInsecureOperations; }
            set { _includeInsecureOperations = value; }
        }
        private bool _includeInsecureOperations = true;

        public bool IncludeHomePageSettings
        {
            get { return _includeHomePageSettings; }
            set { _includeHomePageSettings = value; }
        }
        private bool _includeHomePageSettings = true;

        public bool IncludeImages
        {
            get { return _includeWatermark; }
            set { _includeWatermark = value; }
        }
        private bool _includeWatermark = true;

        public bool IncludeButtons
        {
            get { return _includeButtons; }
            set { _includeButtons = value; }
        }
        private bool _includeButtons = true;

        public bool UseManifestCache
        {
            get { return _useManifestCache; }
            set { _useManifestCache = value; }
        }
        private bool _useManifestCache = false;

        private LazyHomepageDownloader _homepageAccessor;

        public object DetectSettings(IProgressHost progressHost)
        {
            using (_silentMode ? new BlogClientUIContextSilentMode() : null)
            {
                if (IncludeButtons || IncludeOptionOverrides || IncludeImages)
                {
                    using (new ProgressContext(progressHost, 40, Res.Get(StringId.ProgressDetectingWeblogSettings)))
                    {
                        // attempt to download editing manifest
                        WriterEditingManifest editingManifest = SafeDownloadEditingManifest();

                        if (editingManifest != null)
                        {
                            // always update the download info
                            if (editingManifest.DownloadInfo != null)
                                _context.ManifestDownloadInfo = editingManifest.DownloadInfo;

                            // images
                            if (IncludeImages)
                            {
                                // image if provided
                                if (editingManifest.Image != null)
                                    _context.Image = editingManifest.Image;

                                // watermark if provided
                                if (editingManifest.Watermark != null)
                                    _context.WatermarkImage = editingManifest.Watermark;
                            }

                            // buttons if provided
                            if (IncludeButtons && (editingManifest.ButtonDescriptions != null))
                                _context.ButtonDescriptions = editingManifest.ButtonDescriptions;

                            // option overrides if provided
                            if (IncludeOptionOverrides)
                            {
                                if (editingManifest.ClientType != null)
                                    _context.ClientType = editingManifest.ClientType;

                                if (editingManifest.OptionOverrides != null)
                                    _context.OptionOverrides = editingManifest.OptionOverrides;
                            }
                        }
                    }
                }

                using (new ProgressContext(progressHost, 40, Res.Get(StringId.ProgressDetectingWeblogCharSet)))
                {
                    if (IncludeOptionOverrides && IncludeHomePageSettings)
                    {
                        DetectHomePageSettings();
                    }
                }

                IBlogClient blogClient = CreateBlogClient();
                if (IncludeInsecureOperations || blogClient.IsSecure)
                {
                    if (blogClient is ISelfConfiguringClient)
                    {
                        // This must happen before categories detection but after manifest!!
                        ((ISelfConfiguringClient)blogClient).DetectSettings(_context, this);
                    }

                    // detect categories
                    if (IncludeCategories)
                    {
                        using (
                            new ProgressContext(progressHost, 20, Res.Get(StringId.ProgressDetectingWeblogCategories)))
                        {
                            BlogPostCategory[] categories = SafeDownloadCategories();
                            if (categories != null)
                                _context.Categories = categories;

                            BlogPostKeyword[] keywords = SafeDownloadKeywords();
                            if (keywords != null)
                                _context.Keywords = keywords;
                        }
                    }

                    // detect favicon (only if requested AND we don't have a PNG already
                    // for the small image size)
                    if (IncludeFavIcon)
                    {
                        using (new ProgressContext(progressHost, 10, Res.Get(StringId.ProgressDetectingWeblogIcon)))
                        {
                            byte[] favIcon = SafeDownloadFavIcon();
                            if (favIcon != null)
                                _context.FavIcon = favIcon;
                        }
                    }

                    if (IncludeImageEndpoints)
                    {
                        Debug.WriteLine("Detecting image endpoints");
                        ITemporaryBlogSettingsDetectionContext tempContext =
                            _context as ITemporaryBlogSettingsDetectionContext;
                        Debug.Assert(tempContext != null,
                                     "IncludeImageEndpoints=true but non-temporary context (type " +
                                     _context.GetType().Name + ") was used");
                        if (tempContext != null)
                        {
                            tempContext.AvailableImageEndpoints = null;
                            try
                            {
                                BlogInfo[] imageEndpoints = blogClient.GetImageEndpoints();
                                tempContext.AvailableImageEndpoints = imageEndpoints;
                                Debug.WriteLine(imageEndpoints.Length + " image endpoints detected");
                            }
                            catch (NotImplementedException)
                            {
                                Debug.WriteLine("Image endpoints not implemented");
                            }
                            catch (Exception e)
                            {
                                Trace.Fail("Exception detecting image endpoints: " + e.ToString());
                            }
                        }
                    }
                }
                // completed
                progressHost.UpdateProgress(100, 100, Res.Get(StringId.ProgressCompletedSettingsDetection));
            }

            return this;
        }

        /// <summary>
        /// Any setting that is derivaed from the homepage html needs to be in this function.  This function is turned
        /// on and off when detecting blog seetings through the IncludeHomePageSettings.  None of these checks will be run
        /// if the internet is not active.  As each check is made, it does not need to be applied back the _content until the end
        /// at which time it will write the settings back to the registry.
        /// </summary>
        private void DetectHomePageSettings()
        {
            if (_homepageAccessor.HtmlDocument == null) return;

            IDictionary homepageSettings = new Hashtable();

            Debug.Assert(!UseManifestCache, "This code will not run correctly under the manifest cache, due to option overrides not being set");

            LightWeightHTMLMetaData metaData = new LightWeightHTMLMetaData(_homepageAccessor.HtmlDocument);
            if (metaData.Charset != null)
            {
                try
                {
                    homepageSettings.Add(BlogClientOptions.CHARACTER_SET, metaData.Charset);
                }
                catch (NotSupportedException)
                {
                    //not an actual encoding
                }

            }

            string docType = new LightWeightHTMLMetaData(_homepageAccessor.HtmlDocument).DocType;
            if (docType != null)
            {
                bool xhtml = docType.IndexOf("xhtml", StringComparison.OrdinalIgnoreCase) >= 0;
                if (xhtml)
                {
                    homepageSettings.Add(BlogClientOptions.REQUIRES_XHTML, true.ToString(CultureInfo.InvariantCulture));
                }
            }

            //checking whether blog is rtl
            HtmlExtractor extractor = new HtmlExtractor(_homepageAccessor.HtmlDocument.RawHtml);
            if (extractor.Seek(new OrPredicate(
                new SmartPredicate("<html dir>"),
                new SmartPredicate("<body dir>"))).Success)
            {
                BeginTag tag = (BeginTag)extractor.Element;
                string dir = tag.GetAttributeValue("dir");
                if (String.Compare(dir, "rtl", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    homepageSettings.Add(BlogClientOptions.TEMPLATE_IS_RTL, true.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (_homepageAccessor.HtmlDocument != null)
            {
                string html = _homepageAccessor.OriginalHtml;
                ImageViewer viewer = DhtmlImageViewers.DetectImageViewer(html, _context.HomepageUrl);
                if (viewer != null)
                {
                    homepageSettings.Add(BlogClientOptions.DHTML_IMAGE_VIEWER, viewer.Name);
                }
            }

            _context.HomePageOverrides = homepageSettings;
        }

        private byte[] SafeDownloadFavIcon()
        {

            byte[] favIcon = SafeProbeForFavIconFromFile();
            if (favIcon != null)
                return favIcon;
            else
                return SafeProbeForFavIconFromLinkTag();
        }

        private byte[] SafeProbeForFavIconFromFile()
        {
            try
            {
                string favIconUrl = UrlHelper.UrlCombine(_context.HomepageUrl, "favicon.ico");
                using (Stream favIconStream = SafeDownloadFavIcon(favIconUrl))
                {
                    if (favIconStream != null)
                        return FavIconArrayFromStream(favIconStream);
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                ReportException("attempting to download favicon", ex);
                return null;
            }
        }

        private byte[] SafeProbeForFavIconFromLinkTag()
        {
            try
            {
                if (_homepageAccessor.HtmlDocument != null)
                {
                    LightWeightTag[] linkTags = _homepageAccessor.HtmlDocument.GetTagsByName(HTMLTokens.Link);
                    foreach (LightWeightTag linkTag in linkTags)
                    {
                        string rel = linkTag.BeginTag.GetAttributeValue("rel");
                        string href = linkTag.BeginTag.GetAttributeValue("href");
                        if (rel != null && rel.Trim().ToUpperInvariant() == "SHORTCUT ICON" && href != null)
                        {
                            // now we have the favicon url, try to download it
                            string favIconUrl = UrlHelper.UrlCombineIfRelative(_context.HomepageUrl, href);
                            using (Stream favIconStream = SafeDownloadFavIcon(favIconUrl))
                            {
                                if (favIconStream != null)
                                    return FavIconArrayFromStream(favIconStream);
                            }
                        }
                    }
                }

                // didn't find the favicon this way
                return null;
            }
            catch (Exception ex)
            {
                ReportException("attempting to download favicon from link tag", ex);
                return null;
            }
        }

        private byte[] FavIconArrayFromStream(Stream favIconStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // copy it to a memory stream
                StreamHelper.Transfer(favIconStream, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // validate that it is indeed an icon
                try
                {
                    Icon icon = new Icon(memoryStream);
                    (icon as IDisposable).Dispose();

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.ToArray();
                }
                catch
                {
                    return null;
                }
            }
        }

        private Stream SafeDownloadFavIcon(string favIconUrl)
        {
            try
            {
                string favIconPath = UrlDownloadToFile.Download(favIconUrl, 3000);
                return new FileStream(favIconPath, FileMode.Open);
            }
            catch
            {
                return null;
            }
        }

        private BlogPostCategory[] SafeDownloadCategories()
        {
            try
            {
                IBlogClient blogClient = CreateBlogClient();

                if (blogClient is IBlogClientForCategorySchemeHack && _context is IBlogSettingsDetectionContextForCategorySchemeHack)
                {
                    ((IBlogClientForCategorySchemeHack)blogClient).DefaultCategoryScheme =
                        ((IBlogSettingsDetectionContextForCategorySchemeHack)_context).InitialCategoryScheme;
                }

                return blogClient.GetCategories(_context.HostBlogId);
            }
            catch (Exception ex)
            {
                ReportException("attempting to download categories", ex);
                return null;
            }
        }

        private BlogPostKeyword[] SafeDownloadKeywords()
        {
            try
            {
                IBlogClient blogClient = CreateBlogClient();
                if (blogClient.Options.SupportsGetKeywords)
                    return blogClient.GetKeywords(_context.HostBlogId);
                else
                    return null;
            }
            catch (Exception ex)
            {
                ReportException("attempting to download keywords", ex);
                return null;
            }
        }

        private WriterEditingManifest SafeDownloadEditingManifest()
        {
            WriterEditingManifest editingManifest = null;
            try
            {
                // create a blog client
                IBlogClient blogClient = CreateBlogClient();

                // can we get one based on cached download info
                IBlogCredentialsAccessor credentialsToUse = (IncludeInsecureOperations || blogClient.IsSecure) ? _context.Credentials : null;
                if (_context.ManifestDownloadInfo != null)
                {
                    string manifestUrl = _context.ManifestDownloadInfo.SourceUrl;
                    if (UseManifestCache)
                        editingManifest = WriterEditingManifest.FromDownloadInfo(_context.ManifestDownloadInfo, blogClient, credentialsToUse, true);
                    else
                        editingManifest = WriterEditingManifest.FromUrl(new Uri(manifestUrl), blogClient, credentialsToUse, true);
                }

                // if we don't have one yet then probe for one
                if (editingManifest == null)
                {
                    editingManifest = WriterEditingManifest.FromHomepage(_homepageAccessor, new Uri(_context.HomepageUrl), blogClient, credentialsToUse);
                }
            }
            catch (Exception ex)
            {
                ReportException("attempting to download editing manifest", ex);
            }

            // return whatever we found
            return editingManifest;
        }

        private void ReportException(string context, Exception ex)
        {
            string error = String.Format(CultureInfo.InvariantCulture, "Exception occurred {0} for weblog {1}: {2}", context, _context.HomepageUrl, ex.ToString());

            if (_silentMode)
            {
                Trace.WriteLine(error);
            }
            else
                Trace.Fail(error);

        }

        // detection context
        private IBlogSettingsDetectionContext _context;

        // helper class for wrapping progress around steps
        private class ProgressContext : IDisposable
        {
            public ProgressContext(IProgressHost progressHost, int complete, string message)
            {
                _progressHost = progressHost;
                _progressHost.UpdateProgress(complete, 100, message);
            }

            public void Dispose()
            {
                if (_progressHost.CancelRequested)
                    throw new OperationCancelledException();
            }

            private IProgressHost _progressHost;

        }
    }

}
