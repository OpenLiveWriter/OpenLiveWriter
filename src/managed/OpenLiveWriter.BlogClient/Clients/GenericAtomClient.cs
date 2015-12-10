// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

/*
 * TODO
 *
 * Make sure all required fields are filled out.
 * Remove the HTML title from the friendly error message
 * Test ETags where HEAD not supported
 * Test experience when no media collection configured
 * Add command line option for preferring Atom over RSD
 * See if blogproviders.xml can override Atom vs. RSD preference
 */

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("Atom", "Atom")]
    public class GenericAtomClient : AtomClient, ISelfConfiguringClient, IBlogClientForCategorySchemeHack
    {
        static GenericAtomClient()
        {
            AuthenticationManager.Register(new GoogleLoginAuthenticationModule());
        }

        private string _defaultCategoryScheme_HACK;

        public GenericAtomClient(Uri postApiUrl, IBlogCredentialsAccessor credentials) : base(AtomProtocolVersion.V10, postApiUrl, credentials)
        {
        }

        protected override void ConfigureClientOptions(OpenLiveWriter.BlogClient.Providers.BlogClientOptions clientOptions)
        {
            base.ConfigureClientOptions(clientOptions);

            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsExcerpt = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsFileUpload = true;
        }

        public virtual void DetectSettings(IBlogSettingsDetectionContext context, BlogSettingsDetector detector)
        {
            if (detector.IncludeOptionOverrides)
            {
                if (detector.IncludeCategoryScheme)
                {
                    Debug.Assert(!detector.UseManifestCache,
                                 "This code will not run correctly under the manifest cache, due to option overrides not being set");

                    IDictionary optionOverrides = context.OptionOverrides;
                    if (optionOverrides == null)
                        optionOverrides = new Hashtable();

                    bool hasNewCategories = optionOverrides.Contains(BlogClientOptions.SUPPORTS_NEW_CATEGORIES);
                    bool hasScheme = optionOverrides.Contains(BlogClientOptions.CATEGORY_SCHEME);
                    if (!hasNewCategories || !hasScheme)
                    {
                        string scheme;
                        bool supportsNewCategories;
                        GetCategoryInfo(context.HostBlogId,
                                        optionOverrides[BlogClientOptions.CATEGORY_SCHEME] as string, // may be null
                                        out scheme,
                                        out supportsNewCategories);

                        if (scheme == null)
                        {
                            // no supported scheme was found or provided
                            optionOverrides[BlogClientOptions.SUPPORTS_CATEGORIES] = false.ToString();
                        }
                        else
                        {
                            if (!optionOverrides.Contains(BlogClientOptions.SUPPORTS_NEW_CATEGORIES))
                                optionOverrides.Add(BlogClientOptions.SUPPORTS_NEW_CATEGORIES, supportsNewCategories.ToString());
                            if (!optionOverrides.Contains(BlogClientOptions.CATEGORY_SCHEME))
                                optionOverrides.Add(BlogClientOptions.CATEGORY_SCHEME, scheme);
                        }

                        context.OptionOverrides = optionOverrides;
                    }
                }

                // GetFeaturesXml(context.HostBlogId);
            }
        }

        private void GetFeaturesXml(string blogId)
        {
            Uri uri = FeedServiceUrl;
            XmlDocument serviceDoc = xmlRestRequestHelper.Get(ref uri, RequestFilter);

            foreach (XmlElement entryEl in serviceDoc.SelectNodes("app:service/app:workspace/app:collection", _nsMgr))
            {
                string href = XmlHelper.GetUrl(entryEl, "@href", uri);
                if (blogId == href)
                {
                    XmlDocument results = new XmlDocument();
                    XmlElement rootElement = results.CreateElement("featuresInfo");
                    results.AppendChild(rootElement);
                    foreach (XmlElement featuresNode in entryEl.SelectNodes("f:features", _nsMgr))
                    {
                        AddFeaturesXml(featuresNode, rootElement, uri);
                    }
                    return;
                }
            }
            Trace.Fail("Couldn't find collection in service document:\r\n" + serviceDoc.OuterXml);
        }

        private void AddFeaturesXml(XmlElement featuresNode, XmlElement containerNode, Uri baseUri)
        {
            if (featuresNode.HasAttribute("href"))
            {
                string href = XmlHelper.GetUrl(featuresNode, "@href", baseUri);
                if (href != null && href.Length > 0)
                {
                    Uri uri = new Uri(href);
                    if (baseUri == null || !uri.Equals(baseUri)) // detect simple cycles
                    {
                        XmlDocument doc = xmlRestRequestHelper.Get(ref uri, RequestFilter);
                        XmlElement features = (XmlElement)doc.SelectSingleNode(@"f:features", _nsMgr);
                        if (features != null)
                            AddFeaturesXml(features, containerNode, uri);
                    }
                }
            }
            else
            {
                foreach (XmlElement featureEl in featuresNode.SelectNodes("f:feature"))
                    containerNode.AppendChild(containerNode.OwnerDocument.ImportNode(featureEl, true));
            }

        }

        string IBlogClientForCategorySchemeHack.DefaultCategoryScheme
        {
            set { _defaultCategoryScheme_HACK = value; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="inScheme">The scheme that should definitely be used (i.e. from wlwmanifest), or null.
        /// If inScheme is non-null, then outScheme will equal inScheme.</param>
        /// <param name="outScheme">The scheme that should be used, or null if categories are not supported.</param>
        /// <param name="supportsNewCategories">Ignore this value if outScheme == null.</param>
        private void GetCategoryInfo(string blogId, string inScheme, out string outScheme, out bool supportsNewCategories)
        {
            XmlDocument xmlDoc = GetCategoryXml(ref blogId);
            foreach (XmlElement categoriesNode in xmlDoc.DocumentElement.SelectNodes("app:categories", _nsMgr))
            {
                bool hasScheme = categoriesNode.HasAttribute("scheme");
                string scheme = categoriesNode.GetAttribute("scheme");
                bool isFixed = categoriesNode.GetAttribute("fixed") == "yes";

                // <categories fixed="no" />
                if (!hasScheme && inScheme == null && !isFixed)
                {
                    outScheme = "";
                    supportsNewCategories = true;
                    return;
                }

                // <categories scheme="inScheme" fixed="yes|no" />
                if (hasScheme && scheme == inScheme)
                {
                    outScheme = inScheme;
                    supportsNewCategories = !isFixed;
                    return;
                }

                // <categories scheme="" fixed="yes|no" />
                if (hasScheme && inScheme == null && scheme == "")
                {
                    outScheme = "";
                    supportsNewCategories = !isFixed;
                    return;
                }
            }

            outScheme = inScheme; // will be null if no scheme was externally specified
            supportsNewCategories = false;
        }

        /*
                protected override OpenLiveWriter.CoreServices.HttpRequestFilter RequestFilter
                {
                    get
                    {
                        return new HttpRequestFilter(WordPressCookieFilter);
                    }
                }

                private void WordPressCookieFilter(HttpWebRequest request)
                {
                    request.CookieContainer = new CookieContainer();
                    string COOKIE_STRING =
                        "wordpressuser_6c27d03220bea936360daa76ec007cd7=admin; wordpresspass_6c27d03220bea936360daa76ec007cd7=696d29e0940a4957748fe3fc9efd22a3; __utma=260458847.291972184.1164155988.1176250147.1176308376.43; __utmz=260458847.1164155988.1.1.utmccn=(direct)|utmcsr=(direct)|utmcmd=(none); dbx-postmeta=grabit:0+|1+|2+|3+|4+|5+&advancedstuff:0-|1-|2-; dbx-pagemeta=grabit=0+,1+,2+,3+,4+,5+&advancedstuff=0+";
                    foreach (string cookie in StringHelper.Split(COOKIE_STRING, ";"))
                    {
                        string[] pair = cookie.Split('=');
                        request.CookieContainer.Add(new Cookie(pair[0], pair[1], "/wp22test/", "www.unknown.com"));
                    }
                }
        */

        protected override string CategoryScheme
        {
            get
            {
                string scheme = Options.CategoryScheme;
                if (scheme == null)
                    scheme = _defaultCategoryScheme_HACK;
                return scheme;
            }
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
            // This sucks. We really want to authenticate against the actual feed,
            // not just the service document.
            Uri uri = FeedServiceUrl;
            xmlRestRequestHelper.Get(ref uri, RequestFilter);
        }

        protected void EnsureLoggedIn()
        {

        }

        #region image upload support

        public override void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            AtomMediaUploader uploader = new AtomMediaUploader(_nsMgr, RequestFilter, Options.ImagePostingUrl, Options);
            return uploader.DoBeforePublishUploadWork(uploadContext);
        }

        public override BlogInfo[] GetImageEndpoints()
        {
            EnsureLoggedIn();

            Uri serviceDocUri = FeedServiceUrl;
            XmlDocument xmlDoc = xmlRestRequestHelper.Get(ref serviceDocUri, RequestFilter);

            ArrayList blogInfos = new ArrayList();
            foreach (XmlElement coll in xmlDoc.SelectNodes("/app:service/app:workspace/app:collection", _nsMgr))
            {
                // does this collection accept entries?
                XmlNodeList acceptNodes = coll.SelectNodes("app:accept", _nsMgr);
                string[] acceptTypes = new string[acceptNodes.Count];
                for (int i = 0; i < acceptTypes.Length; i++)
                    acceptTypes[i] = acceptNodes[i].InnerText;

                if (AcceptsImages(acceptTypes))
                {
                    string feedUrl = XmlHelper.GetUrl(coll, "@href", serviceDocUri);
                    if (feedUrl == null || feedUrl.Length == 0)
                        continue;

                    // form title
                    StringBuilder titleBuilder = new StringBuilder();
                    foreach (XmlElement titleContainerNode in new XmlElement[] { coll.ParentNode as XmlElement, coll })
                    {
                        Debug.Assert(titleContainerNode != null);
                        if (titleContainerNode != null)
                        {
                            XmlElement titleNode = titleContainerNode.SelectSingleNode("atom:title", _nsMgr) as XmlElement;
                            if (titleNode != null)
                            {
                                string titlePart = _atomVer.TextNodeToPlaintext(titleNode);
                                if (titlePart.Length != 0)
                                {
                                    Res.LOCME("loc the separator between parts of the blog name");
                                    if (titleBuilder.Length != 0)
                                        titleBuilder.Append(" - ");
                                    titleBuilder.Append(titlePart);
                                }
                            }
                        }
                    }

                    blogInfos.Add(new BlogInfo(feedUrl, titleBuilder.ToString().Trim(), ""));
                }
            }

            return (BlogInfo[])blogInfos.ToArray(typeof(BlogInfo));
        }

        private static bool AcceptsImages(string[] contentTypes)
        {
            bool acceptsPng = false, acceptsJpeg = false, acceptsGif = false;

            foreach (string contentType in contentTypes)
            {
                IDictionary values = MimeHelper.ParseContentType(contentType, true);
                string mainType = values[""] as string;

                switch (mainType)
                {
                    case "*/*":
                    case "image/*":
                        return true;
                    case "image/png":
                        acceptsPng = true;
                        break;
                    case "image/gif":
                        acceptsGif = true;
                        break;
                    case "image/jpeg":
                        acceptsJpeg = true;
                        break;
                }
            }

            return acceptsPng && acceptsJpeg && acceptsGif;
        }

        #endregion
    }

    public class GoogleLoginAuthenticationModule : IAuthenticationModule
    {
        private static GDataCredentials _gdataCred = new GDataCredentials();

        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            if (!challenge.StartsWith("GoogleLogin ", StringComparison.OrdinalIgnoreCase))
                return null;

            HttpWebRequest httpRequest = (HttpWebRequest)request;

            string service;
            string realm;
            ParseChallenge(challenge, out realm, out service);
            if (realm != "http://www.google.com/accounts/ClientLogin")
                return null;

            NetworkCredential cred = credentials.GetCredential(request.RequestUri, AuthenticationType);

            string auth = _gdataCred.GetCredentialsIfValid(cred.UserName, cred.Password, service);
            if (auth != null)
            {
                return new Authorization(auth, true);
            }
            else
            {
                try
                {
                    _gdataCred.EnsureLoggedIn(cred.UserName, cred.Password, service, !BlogClientUIContext.SilentModeForCurrentThread);
                    auth = _gdataCred.GetCredentialsIfValid(cred.UserName, cred.Password, service);
                    if (auth != null)
                        return new Authorization(auth, true);
                    else
                        return null;
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    return null;
                }
            }
        }

        private void ParseChallenge(string challenge, out string realm, out string service)
        {
            Match m = Regex.Match(challenge, @"\brealm=""([^""]*)""");
            realm = m.Groups[1].Value;
            Match m2 = Regex.Match(challenge, @"\bservice=""([^""]*)""");
            service = m2.Groups[1].Value;
        }

        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            throw new NotImplementedException();
        }

        public bool CanPreAuthenticate
        {
            get { return false; }
        }

        public string AuthenticationType
        {
            get { return "GoogleLogin"; }
        }
    }
}
