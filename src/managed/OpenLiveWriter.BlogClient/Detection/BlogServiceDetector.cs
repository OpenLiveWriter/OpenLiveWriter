// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using mshtml;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using Google.Apis.Blogger.v3;
using Google.Apis.Services;

namespace OpenLiveWriter.BlogClient.Detection
{
    public class BlogServiceDetector : BlogServiceDetectorBase
    {
        private IBlogSettingsAccessor _blogSettings;

        public BlogServiceDetector(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, IBlogSettingsAccessor blogSettings, IBlogCredentialsAccessor credentials)
            : base(uiContext, hiddenBrowserParentControl, blogSettings.Id, blogSettings.HomepageUrl, credentials)
        {
            _blogSettings = blogSettings;
        }

        protected override object DetectBlogService(IProgressHost progressHost)
        {
            using (BlogClientUIContextSilentMode uiContextScope = new BlogClientUIContextSilentMode()) //supress prompting for credentials
            {
                try
                {
                    // get the weblog homepage and rsd service description if available
                    IHTMLDocument2 weblogDOM = GetWeblogHomepageDOM(progressHost);

                    // while we have the DOM available, scan for a writer manifest url
                    if (_manifestDownloadInfo == null)
                    {
                        string manifestUrl = WriterEditingManifest.DiscoverUrl(_homepageUrl, weblogDOM);
                        if (manifestUrl != String.Empty)
                            _manifestDownloadInfo = new WriterEditingManifestDownloadInfo(manifestUrl);
                    }

                    string html = weblogDOM != null ? HTMLDocumentHelper.HTMLDocToString(weblogDOM) : null;

                    bool detectionSucceeded = false;

                    if (!detectionSucceeded)
                        detectionSucceeded = AttemptGenericAtomLinkDetection(_homepageUrl, html, !ApplicationDiagnostics.PreferAtom);

                    if (!detectionSucceeded && _blogSettings.IsGoogleBloggerBlog)
                        detectionSucceeded = AttemptBloggerDetection(_homepageUrl, html);

                    if (!detectionSucceeded)
                    {
                        RsdServiceDescription rsdServiceDescription = GetRsdServiceDescription(progressHost, weblogDOM);

                        // if there was no rsd service description or we fail to auto-configure from the
                        // rsd description then move on to other auto-detection techniques
                        if (!(detectionSucceeded = AttemptRsdBasedDetection(progressHost, rsdServiceDescription)))
                        {
                            // try detection by analyzing the homepage url and contents
                            UpdateProgress(progressHost, 75, Res.Get(StringId.ProgressAnalyzingHomepage));
                            if (weblogDOM != null)
                                detectionSucceeded = AttemptHomepageBasedDetection(_homepageUrl, html);
                            else
                                detectionSucceeded = AttemptUrlBasedDetection(_homepageUrl);

                            // if we successfully detected then see if we can narrow down
                            // to a specific weblog
                            if (detectionSucceeded)
                            {
                                if (!BlogProviderParameters.UrlContainsParameters(_postApiUrl))
                                {
                                    // we detected the provider, now see if we can detect the weblog id
                                    // (or at lease the list of the user's weblogs)
                                    UpdateProgress(progressHost, 80, Res.Get(StringId.ProgressAnalyzingWeblogList));
                                    AttemptUserBlogDetection();
                                }
                            }
                        }
                    }

                    if (!detectionSucceeded && html != null)
                        AttemptGenericAtomLinkDetection(_homepageUrl, html, false);

                    // finished
                    UpdateProgress(progressHost, 100, String.Empty);
                }
                catch (OperationCancelledException)
                {
                    // WasCancelled == true
                }
                catch (BlogClientOperationCancelledException)
                {
                    Cancel();
                    // WasCancelled == true
                }
                catch (BlogAccountDetectorException ex)
                {
                    if (ApplicationDiagnostics.AutomationMode)
                        Trace.WriteLine(ex.ToString());
                    else
                        Trace.Fail(ex.ToString());
                    // ErrorOccurred == true
                }
                catch (Exception ex)
                {
                    // ErrorOccurred == true
                    Trace.Fail(ex.Message, ex.ToString());
                    ReportError(MessageId.WeblogDetectionUnexpectedError, ex.Message);
                }

                return this;
            }
        }

        private bool AttemptGenericAtomLinkDetection(string url, string html, bool preferredOnly)
        {
            const string GENERIC_ATOM_PROVIDER_ID = "D48F1B5A-06E6-4f0f-BD76-74F34F520792";

            if (html == null)
                return false;

            HtmlExtractor ex = new HtmlExtractor(html);
            if (ex
                .SeekWithin("<head>", "<body>")
                .SeekWithin("<link href rel='service' type='application/atomsvc+xml'>", "</head>")
                .Success)
            {
                IBlogProvider atomProvider = BlogProviderManager.FindProvider(GENERIC_ATOM_PROVIDER_ID);

                BeginTag bt = ex.Element as BeginTag;

                if (preferredOnly)
                {
                    string classes = bt.GetAttributeValue("class");
                    if (classes == null)
                        return false;
                    if (!Regex.IsMatch(classes, @"\bpreferred\b"))
                        return false;
                }

                string linkUrl = bt.GetAttributeValue("href");

                Debug.WriteLine("Atom service link detected in the blog homepage");

                _providerId = atomProvider.Id;
                _serviceName = atomProvider.Name;
                _clientType = atomProvider.ClientType;
                _blogName = string.Empty;
                _postApiUrl = linkUrl;

                IBlogClient client = BlogClientManager.CreateClient(atomProvider.ClientType, _postApiUrl, _credentials);
                client.VerifyCredentials();
                _usersBlogs = client.GetUsersBlogs();
                if (_usersBlogs.Length == 1)
                {
                    _hostBlogId = _usersBlogs[0].Id;
                    _blogName = _usersBlogs[0].Name;
                    /*
                                        if (_usersBlogs[0].HomepageUrl != null && _usersBlogs[0].HomepageUrl.Length > 0)
                                            _homepageUrl = _usersBlogs[0].HomepageUrl;
                    */
                }

                // attempt to read the blog name from the homepage title
                if (_blogName == null || _blogName.Length == 0)
                {
                    HtmlExtractor ex2 = new HtmlExtractor(html);
                    if (ex2.Seek("<title>").Success)
                    {
                        _blogName = ex2.CollectTextUntil("title");
                    }
                }

                return true;
            }
            return false;
        }

        private class BloggerGeneratorCriterion : IElementPredicate
        {
            public bool IsMatch(Element e)
            {
                BeginTag tag = e as BeginTag;
                if (tag == null)
                    return false;

                if (!tag.NameEquals("meta"))
                    return false;

                if (tag.GetAttributeValue("name") != "generator")
                    return false;

                string generator = tag.GetAttributeValue("content");
                if (generator == null || CaseInsensitiveComparer.DefaultInvariant.Compare("blogger", generator) != 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Do special Blogger-specific detection logic.  We want to
        /// use the Blogger Atom endpoints specified in the HTML, not
        /// the Blogger endpoint in the RSD.
        /// </summary>
        private bool AttemptBloggerDetection(string homepageUrl, string html)
        {
            Debug.Assert(string.IsNullOrEmpty(homepageUrl), "Google Blogger blogs don't know the homepageUrl");
            Debug.Assert(string.IsNullOrEmpty(html), "Google Blogger blogs don't know the homepageUrl");

            const string BLOGGER_V3_PROVIDER_ID = "343F1D83-1098-43F4-AE86-93AFC7602855";
            IBlogProvider bloggerProvider = BlogProviderManager.FindProvider(BLOGGER_V3_PROVIDER_ID);
            if (bloggerProvider == null)
            {
                Trace.Fail("Couldn't retrieve Blogger provider");
                return false;
            }

            BlogAccountDetector blogAccountDetector = new BlogAccountDetector(bloggerProvider.ClientType, bloggerProvider.PostApiUrl, _credentials);
            if (blogAccountDetector.ValidateService())
            {
                CopySettingsFromProvider(bloggerProvider);

                _usersBlogs = blogAccountDetector.UsersBlogs;
                if (_usersBlogs.Length == 1)
                {
                    _hostBlogId = _usersBlogs[0].Id;
                    _blogName = _usersBlogs[0].Name;
                    _homepageUrl = _usersBlogs[0].HomepageUrl;
                }

                // If we didn't find the specific blog, we'll prompt the user with the list of blogs
                return true;
            }
            else
            {
                AuthenticationErrorOccurred = blogAccountDetector.Exception is BlogClientAuthenticationException;
                ReportErrorAndFail(blogAccountDetector.ErrorMessageType, blogAccountDetector.ErrorMessageParams);
                return false;
            }
        }

        private bool AttemptRsdBasedDetection(IProgressHost progressHost, RsdServiceDescription rsdServiceDescription)
        {
            // always return alse for null description
            if (rsdServiceDescription == null)
                return false;

            string providerId = String.Empty;
            BlogAccount blogAccount = null;

            // check for a match on rsd engine link
            foreach (IBlogProvider provider in BlogProviderManager.Providers)
            {
                blogAccount = provider.DetectAccountFromRsdHomepageLink(rsdServiceDescription);
                if (blogAccount != null)
                {
                    providerId = provider.Id;
                    break;
                }
            }

            // if none found on engine link, match on engine name
            if (blogAccount == null)
            {
                foreach (IBlogProvider provider in BlogProviderManager.Providers)
                {
                    blogAccount = provider.DetectAccountFromRsdEngineName(rsdServiceDescription);
                    if (blogAccount != null)
                    {
                        providerId = provider.Id;
                        break;
                    }
                }
            }

            // No provider associated with the RSD file, try to gin one up (will only
            // work if the RSD file contains an API for one of our supported client types)
            if (blogAccount == null)
            {
                // try to create one from RSD
                blogAccount = BlogAccountFromRsdServiceDescription.Create(rsdServiceDescription);
            }

            // if we have an rsd-detected weblog
            if (blogAccount != null)
            {
                // confirm that the credentials are OK
                UpdateProgress(progressHost, 65, Res.Get(StringId.ProgressVerifyingInterface));
                BlogAccountDetector blogAccountDetector = new BlogAccountDetector(
                    blogAccount.ClientType, blogAccount.PostApiUrl, _credentials);

                if (blogAccountDetector.ValidateService())
                {
                    // copy basic account info
                    _providerId = providerId;
                    _serviceName = blogAccount.ServiceName;
                    _clientType = blogAccount.ClientType;
                    _hostBlogId = blogAccount.BlogId;
                    _postApiUrl = blogAccount.PostApiUrl;

                    // see if we can improve on the blog name guess we already
                    // have from the <title> element of the homepage
                    BlogInfo blogInfo = blogAccountDetector.DetectAccount(_homepageUrl, _hostBlogId);
                    if (blogInfo != null)
                        _blogName = blogInfo.Name;
                }
                else
                {
                    // report user-authorization error
                    ReportErrorAndFail(blogAccountDetector.ErrorMessageType, blogAccountDetector.ErrorMessageParams);
                }

                // success!
                return true;
            }
            else
            {
                // couldn't do it
                return false;
            }
        }

        private bool AttemptUrlBasedDetection(string url)
        {
            // matched provider
            IBlogProvider blogAccountProvider = null;

            // do url-based matching
            foreach (IBlogProvider provider in BlogProviderManager.Providers)
            {
                if (provider.IsProviderForHomepageUrl(url))
                {
                    blogAccountProvider = provider;
                    break;
                }
            }

            if (blogAccountProvider != null)
            {
                CopySettingsFromProvider(blogAccountProvider);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AttemptContentBasedDetection(string homepageContent)
        {
            // matched provider
            IBlogProvider blogAccountProvider = null;

            // do url-based matching
            foreach (IBlogProvider provider in BlogProviderManager.Providers)
            {
                if (provider.IsProviderForHomepageContent(homepageContent))
                {
                    blogAccountProvider = provider;
                    break;
                }
            }

            if (blogAccountProvider != null)
            {
                CopySettingsFromProvider(blogAccountProvider);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AttemptHomepageBasedDetection(string homepageUrl, string homepageContent)
        {
            if (AttemptUrlBasedDetection(homepageUrl))
            {
                return true;
            }
            else
            {
                return AttemptContentBasedDetection(homepageContent);
            }
        }

        private RsdServiceDescription GetRsdServiceDescription(IProgressHost progressHost, IHTMLDocument2 weblogDOM)
        {
            if (weblogDOM != null)
            {
                // try to download an RSD description
                UpdateProgress(progressHost, 50, Res.Get(StringId.ProgressAnalyzingInterface));
                return RsdServiceDetector.DetectFromWeblog(_homepageUrl, weblogDOM);
            }
            else
            {
                return null;
            }
        }

        private class BlogAccountFromRsdServiceDescription : BlogAccount
        {
            public static BlogAccount Create(RsdServiceDescription rsdServiceDescription)
            {
                try
                {
                    return new BlogAccountFromRsdServiceDescription(rsdServiceDescription);
                }
                catch (NoSupportedRsdClientTypeException)
                {
                    return null;
                }
            }

            private BlogAccountFromRsdServiceDescription(RsdServiceDescription rsdServiceDescription)
            {
                // look for supported apis from highest fidelity to lowest
                RsdApi rsdApi = rsdServiceDescription.ScanForApi("WordPress");
                if (rsdApi == null)
                    rsdApi = rsdServiceDescription.ScanForApi("MovableType");
                if (rsdApi == null)
                    rsdApi = rsdServiceDescription.ScanForApi("MetaWeblog");

                if (rsdApi != null)
                {
                    Init(rsdServiceDescription.EngineName, rsdApi.Name, rsdApi.ApiLink, rsdApi.BlogId);
                    return;
                }
                else
                {
                    // couldn't find a supported api type so we fall through to here
                    throw new NoSupportedRsdClientTypeException();
                }
            }

        }

        private class NoSupportedRsdClientTypeException : ApplicationException
        {
            public NoSupportedRsdClientTypeException()
                : base("No supported Rsd client-type")
            {
            }
        }
    }

    /// <summary>
    /// Blog settings detector for SharePoint blogs.
    /// </summary>
    public class SharePointBlogDetector : BlogServiceDetectorBase
    {
        private IBlogCredentials _blogCredentials;
        public SharePointBlogDetector(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, string localBlogId, string homepageUrl, IBlogCredentialsAccessor credentials, IBlogCredentials blogCredentials)
            : base(uiContext, hiddenBrowserParentControl, localBlogId, homepageUrl, credentials)
        {
            _blogCredentials = blogCredentials;
        }

        protected override object DetectBlogService(IProgressHost progressHost)
        {
            using (BlogClientUIContextSilentMode uiContextScope = new BlogClientUIContextSilentMode()) //supress prompting for credentials
            {
                try
                {
                    // copy basic account info
                    IBlogProvider provider = BlogProviderManager.FindProvider("4AA58E69-8C24-40b1-BACE-3BB14237E8F9");
                    _providerId = provider.Id;
                    _serviceName = provider.Name;
                    _clientType = provider.ClientType;

                    //calculate the API url based on the homepage Url.
                    //  API URL Format: <blogurl>/_layouts/metaweblog.aspx
                    string homepagePath = UrlHelper.SafeToAbsoluteUri(new Uri(_homepageUrl)).Split('?')[0];
                    if (homepagePath == null)
                        homepagePath = "/";

                    //trim off any file information included in the URL (ex: /default.aspx)
                    int lastPathPartIndex = homepagePath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    if (lastPathPartIndex != -1)
                    {
                        string lastPathPart = homepagePath.Substring(lastPathPartIndex);
                        if (lastPathPart.IndexOf('.') != -1)
                        {
                            homepagePath = homepagePath.Substring(0, lastPathPartIndex);
                            if (homepagePath == String.Empty)
                                homepagePath = "/";
                        }
                    }
                    if (homepagePath != "/" && homepagePath.EndsWith("/", StringComparison.OrdinalIgnoreCase)) //trim off trailing slash
                        homepagePath = homepagePath.Substring(0, homepagePath.Length - 1);

                    //Update the homepage url
                    _homepageUrl = homepagePath;

                    _postApiUrl = String.Format(CultureInfo.InvariantCulture, "{0}/_layouts/metaweblog.aspx", homepagePath);

                    if (VerifyCredentialsAndDetectAuthScheme(_postApiUrl, _blogCredentials, _credentials))
                    {
                        AuthenticationErrorOccurred = false;
                        //detect the user's blog ID.
                        if (!BlogProviderParameters.UrlContainsParameters(_postApiUrl))
                        {
                            // we detected the provider, now see if we can detect the weblog id
                            // (or at lease the list of the user's weblogs)
                            UpdateProgress(progressHost, 80, Res.Get(StringId.ProgressAnalyzingWeblogList));
                            AttemptUserBlogDetection();
                        }
                    }
                    else
                        AuthenticationErrorOccurred = true;
                }
                catch (OperationCancelledException)
                {
                    // WasCancelled == true
                }
                catch (BlogClientOperationCancelledException)
                {
                    Cancel();
                    // WasCancelled == true
                }
                catch (BlogAccountDetectorException)
                {
                    // ErrorOccurred == true
                }
                catch (BlogClientAuthenticationException)
                {
                    AuthenticationErrorOccurred = true;
                    // ErrorOccurred == true
                }
                catch (Exception ex)
                {
                    // ErrorOccurred == true
                    ReportError(MessageId.WeblogDetectionUnexpectedError, ex.Message);
                }
            }
            return this;
        }

        /*private string DiscoverPostApiUrl(string baseUrl, string blogPath)
        {

        }*/

        /// <summary>
        /// Verifies the user credentials and determines whether SharePoint is configure to use HTTP or MetaWeblog authentication
        /// </summary>
        /// <param name="postApiUrl"></param>
        /// <param name="blogCredentials"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private static bool VerifyCredentialsAndDetectAuthScheme(string postApiUrl, IBlogCredentials blogCredentials, IBlogCredentialsAccessor credentials)
        {
            BlogClientAttribute blogClientAttr = (BlogClientAttribute)typeof(SharePointClient).GetCustomAttributes(typeof(BlogClientAttribute), false)[0];
            SharePointClient client = (SharePointClient)BlogClientManager.CreateClient(blogClientAttr.TypeName, postApiUrl, credentials);

            return SharePointClient.VerifyCredentialsAndDetectAuthScheme(blogCredentials, client);
        }
    }

    public abstract class BlogServiceDetectorBase : MultipartAsyncOperation, ITemporaryBlogSettingsDetectionContext
    {
        public BlogServiceDetectorBase(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, string localBlogId, string homepageUrl, IBlogCredentialsAccessor credentials)
            : base(uiContext)
        {
            // save references
            _uiContext = uiContext;
            _localBlogId = localBlogId;
            _homepageUrl = homepageUrl;
            _credentials = credentials;

            // add blog service detection
            AddProgressOperation(
                new ProgressOperation(DetectBlogService),
                35);

            // add settings downloading (note: this operation will be a no-op
            // in the case where we don't succesfully detect a weblog)
            AddProgressOperation(
                new ProgressOperation(DetectWeblogSettings),
                new ProgressOperationCompleted(DetectWeblogSettingsCompleted),
                30);

            // add template downloading (note: this operation will be a no-op in the
            // case where we don't successfully detect a weblog)
            _blogEditingTemplateDetector = new BlogEditingTemplateDetector(uiContext, hiddenBrowserParentControl);
            AddProgressOperation(
                new ProgressOperation(_blogEditingTemplateDetector.DetectTemplate),
                35);
        }

        public BlogInfo[] UsersBlogs
        {
            get { return _usersBlogs; }
        }

        public string ProviderId
        {
            get { return _providerId; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
        }

        public string ClientType
        {
            get { return _clientType; }
        }

        public string PostApiUrl
        {
            get { return _postApiUrl; }
        }

        public string HostBlogId
        {
            get { return _hostBlogId; }
        }

        public string BlogName
        {
            get { return _blogName; }
        }

        public IDictionary OptionOverrides
        {
            get { return _optionOverrides; }
        }

        public IDictionary HomePageOverrides
        {
            get { return _homePageOverrides; }
        }

        public IDictionary UserOptionOverrides
        {
            get { return null; }
        }

        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get { return _buttonDescriptions; }
        }

        public BlogPostCategory[] Categories
        {
            get { return _categories; }
        }

        public BlogPostKeyword[] Keywords
        {
            get { return _keywords; }
        }

        public byte[] FavIcon
        {
            get { return _favIcon; }
        }

        public byte[] Image
        {
            get { return _image; }
        }

        public byte[] WatermarkImage
        {
            get { return _watermarkImage; }
        }

        public BlogEditingTemplateFile[] BlogTemplateFiles
        {
            get { return _blogEditingTemplateDetector.BlogTemplateFiles; }
        }

        public Color? PostBodyBackgroundColor
        {
            get { return _blogEditingTemplateDetector.PostBodyBackgroundColor; }
        }

        public bool WasCancelled
        {
            get { return CancelRequested; }
        }

        public bool ErrorOccurred
        {
            get { return _errorMessageType != MessageId.None; }
        }

        public bool AuthenticationErrorOccurred
        {
            get { return _authenticationErrorOccured; }
            set { _authenticationErrorOccured = value; }
        }
        private bool _authenticationErrorOccured = false;

        public bool TemplateDownloadFailed
        {
            get { return _blogEditingTemplateDetector.ExceptionOccurred; }
        }

        IBlogCredentialsAccessor IBlogSettingsDetectionContext.Credentials
        {
            get { return _credentials; }
        }

        string IBlogSettingsDetectionContext.HomepageUrl
        {
            get { return _homepageUrl; }
        }

        public WriterEditingManifestDownloadInfo ManifestDownloadInfo
        {
            get { return _manifestDownloadInfo; }
            set { _manifestDownloadInfo = value; }
        }

        string IBlogSettingsDetectionContext.ClientType
        {
            get { return _clientType; }
            set { _clientType = value; }
        }

        byte[] IBlogSettingsDetectionContext.FavIcon
        {
            get { return _favIcon; }
            set { _favIcon = value; }
        }

        byte[] IBlogSettingsDetectionContext.Image
        {
            get { return _image; }
            set { _image = value; }
        }

        byte[] IBlogSettingsDetectionContext.WatermarkImage
        {
            get { return _watermarkImage; }
            set { _watermarkImage = value; }
        }

        BlogPostCategory[] IBlogSettingsDetectionContext.Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        BlogPostKeyword[] IBlogSettingsDetectionContext.Keywords
        {
            get { return _keywords; }
            set { _keywords = value; }
        }

        IDictionary IBlogSettingsDetectionContext.OptionOverrides
        {
            get { return _optionOverrides; }
            set { _optionOverrides = value; }
        }

        IDictionary IBlogSettingsDetectionContext.HomePageOverrides
        {
            get { return _homePageOverrides; }
            set { _homePageOverrides = value; }
        }

        IBlogProviderButtonDescription[] IBlogSettingsDetectionContext.ButtonDescriptions
        {
            get { return _buttonDescriptions; }
            set { _buttonDescriptions = value; }
        }

        public BlogInfo[] AvailableImageEndpoints
        {
            get { return availableImageEndpoints; }
            set { availableImageEndpoints = value; }
        }

        public void ShowLastError(IWin32Window owner)
        {
            if (ErrorOccurred)
            {
                DisplayMessage.Show(_errorMessageType, owner, _errorMessageParams);
            }
            else
            {
                Trace.Fail("Called ShowLastError when no error occurred");
            }
        }

        public static byte[] SafeDownloadFavIcon(string homepageUrl)
        {
            try
            {
                string favIconUrl = UrlHelper.UrlCombine(homepageUrl, "favicon.ico");
                using (Stream favIconStream = HttpRequestHelper.SafeDownloadFile(favIconUrl))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        StreamHelper.Transfer(favIconStream, memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        protected abstract object DetectBlogService(IProgressHost progressHost);

        protected void AttemptUserBlogDetection()
        {
            BlogAccountDetector blogAccountDetector = new BlogAccountDetector(
                _clientType, _postApiUrl, _credentials);

            if (blogAccountDetector.ValidateService())
            {
                BlogInfo blogInfo = blogAccountDetector.DetectAccount(_homepageUrl, _hostBlogId);
                if (blogInfo != null)
                {
                    // save the detected info
                    // TODO: Commenting out next line for Spaces demo tomorrow.
                    // need to decide whether to keep it commented out going forward.
                    // _homepageUrl = blogInfo.HomepageUrl;
                    _hostBlogId = blogInfo.Id;
                    _blogName = blogInfo.Name;
                }

                // always save the list of user's blogs
                _usersBlogs = blogAccountDetector.UsersBlogs;
            }
            else
            {
                AuthenticationErrorOccurred = blogAccountDetector.Exception is BlogClientAuthenticationException;
                ReportErrorAndFail(blogAccountDetector.ErrorMessageType, blogAccountDetector.ErrorMessageParams);
            }
        }

        protected IHTMLDocument2 GetWeblogHomepageDOM(IProgressHost progressHost)
        {
            // try download the weblog home page
            UpdateProgress(progressHost, 25, Res.Get(StringId.ProgressAnalyzingHomepage));
            string responseUri;
            IHTMLDocument2 weblogDOM = HTMLDocumentHelper.SafeGetHTMLDocumentFromUrl(_homepageUrl, out responseUri);
            if (responseUri != null && responseUri != _homepageUrl)
            {
                _homepageUrl = responseUri;
            }
            if (weblogDOM != null)
            {
                // default the blog name to the title of the document
                if (weblogDOM.title != null)
                {
                    _blogName = weblogDOM.title;

                    // drop anything to the right of a "|", as it usually is a site name
                    int index = _blogName.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (index > 0)
                    {
                        string newname = _blogName.Substring(0, index).Trim();
                        if (newname != String.Empty)
                            _blogName = newname;
                    }
                }
            }

            return weblogDOM;
        }

        protected void CopySettingsFromProvider(IBlogProvider blogAccountProvider)
        {
            _providerId = blogAccountProvider.Id;
            _serviceName = blogAccountProvider.Name;
            _clientType = blogAccountProvider.ClientType;
            _postApiUrl = ProcessPostUrlMacros(blogAccountProvider.PostApiUrl);
        }

        private string ProcessPostUrlMacros(string postApiUrl)
        {
            return postApiUrl.Replace("<username>", _credentials.Username);
        }

        private object DetectWeblogSettings(IProgressHost progressHost)
        {
            using (BlogClientUIContextSilentMode uiContextScope = new BlogClientUIContextSilentMode()) //supress prompting for credentials
            {
                // no-op if we don't have a blog-id to work with
                if (HostBlogId == String.Empty)
                    return this;

                try
                {
                    // detect settings
                    BlogSettingsDetector blogSettingsDetector = new BlogSettingsDetector(this);
                    blogSettingsDetector.DetectSettings(progressHost);
                }
                catch (OperationCancelledException)
                {
                    // WasCancelled == true
                }
                catch (BlogClientOperationCancelledException)
                {
                    Cancel();
                    // WasCancelled == true
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected error occurred while detecting weblog settings: " + ex.ToString());
                }

                return this;
            }
        }

        private void DetectWeblogSettingsCompleted(object result)
        {
            // no-op if we don't have a blog detected
            if (HostBlogId == String.Empty)
                return;

            // get the editing template directory
            string blogTemplateDir = BlogEditingTemplate.GetBlogTemplateDir(_localBlogId);

            // set context for template detector
            BlogAccount blogAccount = new BlogAccount(ServiceName, ClientType, PostApiUrl, HostBlogId);
            _blogEditingTemplateDetector.SetContext(blogAccount, _credentials, _homepageUrl, blogTemplateDir, _manifestDownloadInfo, false, _providerId, _optionOverrides, null, _homePageOverrides);

        }

        protected void UpdateProgress(IProgressHost progressHost, int percent, string message)
        {
            if (CancelRequested)
                throw new OperationCancelledException();

            progressHost.UpdateProgress(percent, 100, message);
        }

        protected void ReportError(MessageId errorMessageType, params object[] errorMessageParams)
        {
            _errorMessageType = errorMessageType;
            _errorMessageParams = errorMessageParams;
        }

        protected void ReportErrorAndFail(MessageId errorMessageType, params object[] errorMessageParams)
        {
            ReportError(errorMessageType, errorMessageParams);
            throw new BlogAccountDetectorException();
        }

        protected class BlogAccountDetectorException : ApplicationException
        {
            public BlogAccountDetectorException() : base("Blog account detector did not succeed")
            {
            }
        }

        /// <summary>
        /// Blog account we are scanning
        /// </summary>
        protected string _localBlogId;
        protected string _homepageUrl;
        protected WriterEditingManifestDownloadInfo _manifestDownloadInfo = null;
        protected IBlogCredentialsAccessor _credentials;

        // BlogTemplateDetector
        private BlogEditingTemplateDetector _blogEditingTemplateDetector;

        /// <summary>
        /// Results of scanning
        /// </summary>
        protected string _providerId = String.Empty;
        protected string _serviceName = String.Empty;
        protected string _clientType = String.Empty;
        protected string _postApiUrl = String.Empty;
        protected string _hostBlogId = String.Empty;
        protected string _blogName = String.Empty;

        protected BlogInfo[] _usersBlogs = new BlogInfo[] { };

        // if we are unable to detect these values then leave them null
        // as an indicator that their values are "unknown" vs. "empty"
        // callers can then choose to not overwrite any existing settings
        // in this case
        protected IDictionary _homePageOverrides = null;
        protected IDictionary _optionOverrides = null;
        private BlogPostCategory[] _categories = null;
        private BlogPostKeyword[] _keywords = null;
        private byte[] _favIcon = null;
        private byte[] _image = null;
        private byte[] _watermarkImage = null;
        private IBlogProviderButtonDescription[] _buttonDescriptions = null;

        // error info
        private MessageId _errorMessageType;
        private object[] _errorMessageParams;
        protected IBlogClientUIContext _uiContext;
        private BlogInfo[] availableImageEndpoints;
    }
}
