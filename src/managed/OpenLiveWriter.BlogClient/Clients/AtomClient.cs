// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Clients
{
    public abstract class AtomClient : BlogClientBase, IBlogClient
    {
        public const string ENTRY_CONTENT_TYPE = "application/atom+xml;type=entry";
        public const string SKYDRIVE_ENTRY_CONTENT_TYPE = ENTRY_CONTENT_TYPE;//"application/atom+xml";

        private const string XHTML_NS = "http://www.w3.org/1999/xhtml";
        private const string FEATURES_NS = "http://purl.org/atompub/features/1.0";
        private const string MEDIA_NS = "http://search.yahoo.com/mrss/";
        private const string LIVE_NS = "http://api.live.com/schemas";

        internal static readonly Namespace xhtmlNS = new Namespace(XHTML_NS, "xhtml");
        internal static readonly Namespace featuresNS = new Namespace(FEATURES_NS, "f");
        internal static readonly Namespace mediaNS = new Namespace(MEDIA_NS, "media");
        internal static readonly Namespace liveNS = new Namespace(LIVE_NS, "live");
        protected internal static XmlRestRequestHelper xmlRestRequestHelper = new XmlRestRequestHelper();

        protected internal AtomProtocolVersion _atomVer;
        internal readonly Namespace _atomNS;
        internal readonly Namespace _pubNS;
        protected internal XmlNamespaceManager _nsMgr;

        private readonly Uri _feedServiceUrl;
        private IBlogClientOptions _clientOptions;

        public AtomClient(AtomProtocolVersion atomVer, Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            _feedServiceUrl = postApiUrl;

            // configure client options
            BlogClientOptions clientOptions = new BlogClientOptions();
            ConfigureClientOptions(clientOptions);
            _clientOptions = clientOptions;

            _atomVer = atomVer;
            _atomNS = new Namespace(atomVer.NamespaceUri, "atom");
            _pubNS = new Namespace(atomVer.PubNamespaceUri, "app");
            _nsMgr = new XmlNamespaceManager(new NameTable());
            _nsMgr.AddNamespace(_atomNS.Prefix, _atomNS.Uri);
            _nsMgr.AddNamespace(_pubNS.Prefix, _pubNS.Uri);
            _nsMgr.AddNamespace(xhtmlNS.Prefix, xhtmlNS.Uri);
            _nsMgr.AddNamespace(featuresNS.Prefix, featuresNS.Uri);
            _nsMgr.AddNamespace(mediaNS.Prefix, mediaNS.Uri);
            _nsMgr.AddNamespace(liveNS.Prefix, liveNS.Uri);
        }

        protected virtual Uri FeedServiceUrl { get { return _feedServiceUrl; } }

        public IBlogClientOptions Options
        {
            get
            {
                return _clientOptions;
            }
        }

        /// <summary>
        /// Enable external users of the class to completely replace
        /// the client options
        /// </summary>
        /// <param name="newClientOptions"></param>
        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            _clientOptions = newClientOptions;
        }

        /// <summary>
        /// Enable subclasses to change the default client options
        /// </summary>
        /// <param name="clientOptions"></param>
        protected virtual void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
        }

        public virtual BlogPostCategory[] GetCategories(string blogId)
        {
            ArrayList categoryList = new ArrayList();

            XmlDocument xmlDoc = GetCategoryXml(ref blogId);
            foreach (XmlElement categoriesNode in xmlDoc.DocumentElement.SelectNodes("app:categories", _nsMgr))
            {
                string categoriesScheme = categoriesNode.GetAttribute("scheme");
                foreach (XmlElement categoryNode in categoriesNode.SelectNodes("atom:category", _nsMgr))
                {
                    string categoryScheme = categoryNode.GetAttribute("scheme");
                    if (categoryScheme == "")
                        categoryScheme = categoriesScheme;
                    if (CategoryScheme == categoryScheme)
                    {
                        string categoryName = categoryNode.GetAttribute("term");
                        string categoryLabel = categoryNode.GetAttribute("label");
                        if (categoryLabel == "")
                            categoryLabel = categoryName;

                        categoryList.Add(new BlogPostCategory(categoryName, categoryLabel));
                    }
                }
            }

            return (BlogPostCategory[])categoryList.ToArray(typeof(BlogPostCategory));
        }

        public virtual BlogPostKeyword[] GetKeywords(string blogId)
        {
            Trace.Fail("AtomClient does not support GetKeywords!");
            return new BlogPostKeyword[] { };
        }

        protected virtual void FixupBlogId(ref string blogId)
        {
        }

        protected virtual XmlDocument GetCategoryXml(ref string blogId)
        {
            // Get the service document
            Login();

            FixupBlogId(ref blogId);

            Uri uri = FeedServiceUrl;
            XmlDocument xmlDoc = xmlRestRequestHelper.Get(ref uri, RequestFilter);

            foreach (XmlElement entryEl in xmlDoc.SelectNodes("app:service/app:workspace/app:collection", _nsMgr))
            {
                string href = XmlHelper.GetUrl(entryEl, "@href", uri);
                if (blogId == href)
                {
                    XmlDocument results = new XmlDocument();
                    XmlElement rootElement = results.CreateElement("categoryInfo");
                    results.AppendChild(rootElement);
                    foreach (XmlElement categoriesNode in entryEl.SelectNodes("app:categories", _nsMgr))
                    {
                        AddCategoriesXml(categoriesNode, rootElement, uri);
                    }
                    return results;
                }
            }
            Trace.Fail("Couldn't find collection in service document:\r\n" + xmlDoc.OuterXml);
            return new XmlDocument();
        }

        private void AddCategoriesXml(XmlElement categoriesNode, XmlElement containerNode, Uri baseUri)
        {
            if (categoriesNode.HasAttribute("href"))
            {
                string href = XmlHelper.GetUrl(categoriesNode, "@href", baseUri);
                if (href != null && href.Length > 0)
                {
                    Uri uri = new Uri(href);
                    if (baseUri == null || !uri.Equals(baseUri)) // detect simple cycles
                    {
                        XmlDocument doc = xmlRestRequestHelper.Get(ref uri, RequestFilter);
                        XmlElement categories = (XmlElement)doc.SelectSingleNode(@"app:categories", _nsMgr);
                        if (categories != null)
                            AddCategoriesXml(categories, containerNode, uri);
                    }
                }
            }
            else
            {
                containerNode.AppendChild(containerNode.OwnerDocument.ImportNode(categoriesNode, true));
            }
        }

        protected virtual HttpRequestFilter RequestFilter
        {
            get
            {
                return new HttpRequestFilter(AuthorizationFilter);
            }
        }

        private void AuthorizationFilter(HttpWebRequest request)
        {
            //			request.KeepAlive = true;
            //			request.ProtocolVersion = HttpVersion.Version11;
            request.Credentials = new NetworkCredential(Credentials.Username, Credentials.Password);
        }

        public void DeletePost(string blogId, string postId, bool publish)
        {
            Login();

            FixupBlogId(ref blogId);
            Uri editUri = PostIdToPostUri(postId);

            try
            {
                RedirectHelper.SimpleRequest sr = new RedirectHelper.SimpleRequest("DELETE", new HttpRequestFilter(DeleteRequestFilter));
                HttpWebResponse response = RedirectHelper.GetResponse(UrlHelper.SafeToAbsoluteUri(editUri), new RedirectHelper.RequestFactory(sr.Create));
                response.Close();
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    WebException we = (WebException)e;
                    if (we.Response != null && we.Response is HttpWebResponse)
                    {
                        switch (((HttpWebResponse)we.Response).StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                            case HttpStatusCode.Gone:
                                return; // these are acceptable responses to a DELETE
                        }
                    }
                }

                if (!AttemptDeletePostRecover(e, blogId, UrlHelper.SafeToAbsoluteUri(editUri), publish))
                    throw;
            }
        }

        private void DeleteRequestFilter(HttpWebRequest request)
        {
            request.Headers["If-match"] = "*";
            RequestFilter(request);
        }

        protected virtual bool AttemptDeletePostRecover(Exception e, string blogId, string postId, bool publish)
        {
            return false;
        }

        public virtual BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            return GetRecentPostsInternal(blogId, maxPosts, includeCategories, now);
        }

        protected BlogPost[] GetRecentPostsInternal(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            Login();

            FixupBlogId(ref blogId);

            HashSet seenIds = new HashSet();

            ArrayList blogPosts = new ArrayList();
            try
            {
                while (true)
                {
                    XmlDocument doc;
                    Uri thisUri = new Uri(blogId);

                    // This while-loop nonsense is necessary because New Blogger has a bug
                    // where the official URL for getting recent posts doesn't work when
                    // the orderby=published flag is set, but there's an un-official URL
                    // that will work correctly. Therefore, subclasses need the ability
                    // to inspect exceptions that occur, along with the URI that was used
                    // to make the request, and determine whether an alternate URI should
                    // be used.
                    while (true)
                    {
                        try
                        {
                            doc = xmlRestRequestHelper.Get(ref thisUri, RequestFilter);
                            break;
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.ToString());
                            if (AttemptAlternateGetRecentPostUrl(e, ref blogId))
                                continue;
                            else
                                throw;
                        }
                    }

                    XmlNodeList nodeList = doc.SelectNodes("/atom:feed/atom:entry", _nsMgr);
                    if (nodeList.Count == 0)
                        break;
                    foreach (XmlElement node in nodeList)
                    {
                        BlogPost blogPost = this.Parse(node, includeCategories, thisUri);
                        if (blogPost != null)
                        {
                            if (seenIds.Contains(blogPost.Id))
                                throw new DuplicateEntryIdException();
                            seenIds.Add(blogPost.Id);

                            if (!now.HasValue || blogPost.DatePublished.CompareTo(now.Value) < 0)
                                blogPosts.Add(blogPost);
                        }
                        if (blogPosts.Count >= maxPosts)
                            break;
                    }
                    if (blogPosts.Count >= maxPosts)
                        break;

                    XmlElement nextNode = doc.SelectSingleNode("/atom:feed/atom:link[@rel='next']", _nsMgr) as XmlElement;
                    if (nextNode == null)
                        break;
                    blogId = XmlHelper.GetUrl(nextNode, "@href", thisUri);
                    if (blogId.Length == 0)
                        break;
                }
            }
            catch (DuplicateEntryIdException)
            {

                if (ApplicationDiagnostics.AutomationMode)
                    Trace.WriteLine("Duplicate IDs detected in feed");
                else
                    Trace.Fail("Duplicate IDs detected in feed");
            }
            return (BlogPost[])blogPosts.ToArray(typeof(BlogPost));
        }

        /// <summary>
        /// Subclasses should override this if there are particular exception conditions
        /// that can be repaired by modifying the URI. Return true if the request should
        /// be retried using the (possibly modified) URI, or false if the exception should
        /// be thrown by the caller.
        /// </summary>
        protected virtual bool AttemptAlternateGetRecentPostUrl(Exception e, ref string uri)
        {
            return false;
        }

        private class DuplicateEntryIdException : Exception
        {
        }

        public BlogPost GetPost(string blogId, string postId)
        {
            Login();

            FixupBlogId(ref blogId);

            Uri uri = PostIdToPostUri(postId);
            WebHeaderCollection responseHeaders;
            XmlDocument doc = xmlRestRequestHelper.Get(ref uri, RequestFilter, out responseHeaders);
            XmlDocument remotePost = (XmlDocument)doc.Clone();
            XmlElement entryNode = doc.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement;
            if (entryNode == null)
                throw new BlogClientInvalidServerResponseException("GetPost", "No post entry returned from server", doc.OuterXml);

            BlogPost post = Parse(entryNode, true, uri);
            post.Id = postId;
            post.ETag = FilterWeakEtag(responseHeaders["ETag"]);
            post.AtomRemotePost = remotePost;
            return post;
        }

        private static string FilterWeakEtag(string etag)
        {
            if (etag != null && etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
                return null;
            return etag;
        }

        protected virtual Uri PostIdToPostUri(string postId)
        {
            return new Uri(postId);
        }

        protected virtual string PostUriToPostId(string postUri)
        {
            return postUri;
        }

        public virtual bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            Login();

            FixupBlogId(ref blogId);

            XmlDocument doc = post.AtomRemotePost;
            XmlElement entryNode = doc.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement;

            // No documentUri is needed because we ensure xml:base is set on the root
            // when we retrieve from XmlRestRequestHelper
            Populate(post, null, entryNode, publish);
            string etagToMatch = FilterWeakEtag(post.ETag);

            try
            {
            retry:
                try
                {
                    Uri uri = PostIdToPostUri(post.Id);
                    WebHeaderCollection responseHeaders;
                    xmlRestRequestHelper.Put(ref uri, etagToMatch, RequestFilter, ENTRY_CONTENT_TYPE, doc, _clientOptions.CharacterSet, true, out responseHeaders);
                }
                catch (WebException we)
                {
                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.PreconditionFailed)
                        {
                            if (etagToMatch != null && etagToMatch.Length > 0)
                            {
                                HttpRequestHelper.LogException(we);

                                string currentEtag = GetEtag(UrlHelper.SafeToAbsoluteUri(PostIdToPostUri(post.Id)));

                                if (currentEtag != null && currentEtag.Length > 0
                                    && currentEtag != etagToMatch)
                                {
                                    if (ConfirmOverwrite())
                                    {
                                        etagToMatch = currentEtag;
                                        goto retry;
                                    }
                                    else
                                    {
                                        throw new BlogClientOperationCancelledException();
                                    }
                                }
                            }
                        }
                    }
                    throw;
                }
            }
            catch (Exception e)
            {
                if (!AttemptEditPostRecover(e, blogId, post, newCategoryContext, publish, out etag, out remotePost))
                {
                    // convert to a provider exception if this is a 404 (allow us to
                    // catch this case explicitly and attempt a new post to recover)
                    if (e is WebException)
                    {
                        WebException webEx = e as WebException;
                        HttpWebResponse response = webEx.Response as HttpWebResponse;
                        if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            throw new BlogClientProviderException("404", e.Message);
                    }

                    // no special handling, just re-throw
                    throw;
                }
            }

            Uri getUri = PostIdToPostUri(post.Id);
            WebHeaderCollection getResponseHeaders;
            remotePost = xmlRestRequestHelper.Get(ref getUri, RequestFilter, out getResponseHeaders);
            etag = FilterWeakEtag(getResponseHeaders["ETag"]);
            Trace.Assert(remotePost != null, "After successful PUT, remote post could not be retrieved");

            if (Options.SupportsNewCategories)
                foreach (BlogPostCategory category in post.NewCategories)
                    newCategoryContext.NewCategoryAdded(category);

            return true;
        }

        public string GetEtag(string uri)
        {
            return GetEtag(uri, RequestFilter);
        }

        public static string GetEtag(string uri, HttpRequestFilter requestFilter)
        {
            return GetEtagImpl(uri, requestFilter, "HEAD", "GET");
        }

        /// <param name="uri"></param>
        /// <param name="methods">An array of HTTP methods that should be tried until one of them does not return 405.</param>
        private static string GetEtagImpl(string uri, HttpRequestFilter requestFilter, params string[] methods)
        {
            try
            {
                HttpWebResponse response = RedirectHelper.GetResponse(uri,
                    new RedirectHelper.RequestFactory(new RedirectHelper.SimpleRequest(methods[0], requestFilter).Create));
                try
                {
                    return FilterWeakEtag(response.Headers["ETag"]);
                }
                finally
                {
                    response.Close();
                }
            }
            catch (WebException we)
            {
                if (methods.Length > 1 && we.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse resp = we.Response as HttpWebResponse;
                    if (resp != null && (resp.StatusCode == HttpStatusCode.MethodNotAllowed || resp.StatusCode == HttpStatusCode.NotImplemented))
                    {
                        string[] newMethods = new string[methods.Length - 1];
                        Array.Copy(methods, 1, newMethods, 0, newMethods.Length);
                        return GetEtagImpl(uri, requestFilter, newMethods);
                    }
                }
                throw;
            }
        }

        public static bool ConfirmOverwrite()
        {
            return DialogResult.Yes == BlogClientUIContext.ShowDisplayMessageOnUIThread(MessageId.ConfirmOverwrite);
        }

        protected virtual bool AttemptEditPostRecover(Exception e, string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = null;
            remotePost = null;
            return false;
        }

        public virtual BlogInfo[] GetUsersBlogs()
        {
            Login();
            return GetUsersBlogsInternal();
        }

        protected BlogInfo[] GetUsersBlogsInternal()
        {
            Uri serviceUri = FeedServiceUrl;
            XmlDocument xmlDoc = xmlRestRequestHelper.Get(ref serviceUri, RequestFilter);

            // Either the FeedServiceUrl points to a service document OR a feed.

            if (xmlDoc.SelectSingleNode("/app:service", _nsMgr) != null)
            {
                ArrayList blogInfos = new ArrayList();
                foreach (XmlElement coll in xmlDoc.SelectNodes("/app:service/app:workspace/app:collection", _nsMgr))
                {
                    bool promote = ShouldPromote(coll);

                    // does this collection accept entries?
                    XmlNodeList acceptNodes = coll.SelectNodes("app:accept", _nsMgr);
                    bool acceptsEntries = false;
                    if (acceptNodes.Count == 0)
                    {
                        acceptsEntries = true;
                    }
                    else
                    {
                        foreach (XmlElement acceptNode in acceptNodes)
                        {
                            if (AcceptsEntry(acceptNode.InnerText))
                            {
                                acceptsEntries = true;
                                break;
                            }
                        }
                    }

                    if (acceptsEntries)
                    {
                        string feedUrl = XmlHelper.GetUrl(coll, "@href", serviceUri);
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

                        // get homepage URL
                        string homepageUrl = "";
                        string dummy = "";
                        Uri tempFeedUrl = new Uri(feedUrl);
                        XmlDocument feedDoc = xmlRestRequestHelper.Get(ref tempFeedUrl, RequestFilter);
                        ParseFeedDoc(feedDoc, tempFeedUrl, false, ref homepageUrl, ref dummy);

                        // TODO: Sniff out the homepage URL
                        BlogInfo blogInfo = new BlogInfo(feedUrl, titleBuilder.ToString().Trim(), homepageUrl);
                        if (promote)
                            blogInfos.Insert(0, blogInfo);
                        else
                            blogInfos.Add(blogInfo);
                    }
                }

                return (BlogInfo[])blogInfos.ToArray(typeof(BlogInfo));
            }
            else
            {
                string title = string.Empty;
                string homepageUrl = string.Empty;

                ParseFeedDoc(xmlDoc, serviceUri, true, ref homepageUrl, ref title);

                return new BlogInfo[] { new BlogInfo(UrlHelper.SafeToAbsoluteUri(FeedServiceUrl), title, homepageUrl) };
            }
        }

        protected virtual bool ShouldPromote(XmlElement collection)
        {
            return false;
        }

        private static bool AcceptsEntry(string contentType)
        {
            IDictionary values = MimeHelper.ParseContentType(contentType, true);
            string mainType = values[""] as string;

            switch (mainType)
            {
                case "entry":
                case "*/*":
                case "application/*":
                    return true;
                case "application/atom+xml":
                    string subType = values["type"] as string;
                    if (subType != null)
                        subType = subType.Trim().ToUpperInvariant();

                    if (subType == "ENTRY")
                        return true;
                    else
                        return false;

                default:
                    return false;
            }
        }

        private void ParseFeedDoc(XmlDocument xmlDoc, Uri baseUri, bool includeTitle, ref string homepageUrl, ref string title)
        {
            if (includeTitle)
            {
                XmlElement titleEl = xmlDoc.SelectSingleNode(@"atom:feed/atom:title", _nsMgr) as XmlElement;
                if (titleEl != null)
                    title = _atomVer.TextNodeToPlaintext(titleEl);
            }

            foreach (XmlElement linkEl in xmlDoc.SelectNodes(@"atom:feed/atom:link[@rel='alternate']", _nsMgr))
            {
                IDictionary contentTypeInfo = MimeHelper.ParseContentType(linkEl.GetAttribute("type"), true);
                switch (contentTypeInfo[""] as string)
                {
                    case "text/html":
                    case "application/xhtml+xml":
                        homepageUrl = XmlHelper.GetUrl(linkEl, "@href", baseUri);
                        return;
                }
            }
        }

        public virtual BlogInfo[] GetImageEndpoints()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsSecure
        {
            get
            {
                try
                {
                    return UrlHelper.SafeToAbsoluteUri(FeedServiceUrl).StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        public virtual string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            throw new BlogClientMethodUnsupportedException("UploadFileBeforePublish");
        }

        public virtual void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
            throw new BlogClientMethodUnsupportedException("UploadFileAfterPublish");
        }

        public virtual BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
        {
            throw new BlogClientMethodUnsupportedException("SuggestCategories");
        }

        public virtual HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
        {
            return BlogClientHelper.SendAuthenticatedHttpRequest(requestUri, filter, CreateCredentialsFilter(requestUri));
        }

        protected virtual HttpRequestFilter CreateCredentialsFilter(string requestUri)
        {
            TransientCredentials tc = Login();
            return HttpRequestCredentialsFilter.Create(tc.Username, tc.Password, requestUri, true);
        }

        public virtual string AddCategory(string blogId, BlogPostCategory categohowry)
        {
            throw new BlogClientMethodUnsupportedException("AddCategory");
        }

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            Login();

            FixupBlogId(ref blogId);

            XmlDocument doc = new XmlDocument();
            XmlElement entryNode = doc.CreateElement(_atomNS.Prefix, "entry", _atomNS.Uri);
            doc.AppendChild(entryNode);
            Populate(post, null, entryNode, publish);

            string slug = null;
            if (Options.SupportsSlug)
                slug = post.Slug;

            WebHeaderCollection responseHeaders;
            Uri uri = new Uri(blogId);
            XmlDocument result = xmlRestRequestHelper.Post(
                ref uri,
                new HttpRequestFilter(new NewPostRequest(this, slug).RequestFilter),
                ENTRY_CONTENT_TYPE,
                doc,
                _clientOptions.CharacterSet,
                out responseHeaders);

            etag = FilterWeakEtag(responseHeaders["ETag"]);
            string location = responseHeaders["Location"];
            if (string.IsNullOrEmpty(location))
            {
                throw new BlogClientInvalidServerResponseException("POST", "The HTTP response was missing the required Location header.", "");
            }
            if (location != responseHeaders["Content-Location"] || result == null)
            {
                Uri locationUri = new Uri(location);
                WebHeaderCollection getResponseHeaders;
                result = xmlRestRequestHelper.Get(ref locationUri, RequestFilter, out getResponseHeaders);
                etag = FilterWeakEtag(getResponseHeaders["ETag"]);
            }

            remotePost = (XmlDocument)result.Clone();
            Parse(result.DocumentElement, true, uri);

            if (Options.SupportsNewCategories)
                foreach (BlogPostCategory category in post.NewCategories)
                    newCategoryContext.NewCategoryAdded(category);

            return PostUriToPostId(location);
        }

        private class NewPostRequest
        {
            private readonly AtomClient _parent;
            private readonly string _slug;

            public NewPostRequest(AtomClient parent, string slug)
            {
                _parent = parent;
                _slug = slug;
            }

            public void RequestFilter(HttpWebRequest request)
            {
                _parent.RequestFilter(request);
                if (_parent.Options.SupportsSlug && _slug != null && _slug.Length > 0)
                    request.Headers.Add("Slug", SlugHeaderValue);
            }

            private string SlugHeaderValue
            {
                get
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(_slug);
                    StringBuilder sb = new StringBuilder(bytes.Length * 2);
                    foreach (byte b in bytes)
                    {
                        if (b > 0x7F || b == '%')
                        {
                            sb.AppendFormat("%{0:X2}", b);
                        }
                        else if (b == '\r' || b == '\n')
                        {
                            // no \r or \n allowed in slugs
                        }
                        else if (b == 0)
                        {
                            Debug.Fail("null byte in slug string, this should never happen");
                        }
                        else
                        {
                            sb.Append((char)b);
                        }
                    }
                    return sb.ToString();
                }
            }
        }

        public virtual BlogPost GetPage(string blogId, string pageId)
        {
            throw new BlogClientMethodUnsupportedException("GetPage");
        }

        public virtual PageInfo[] GetPageList(string blogId)
        {
            throw new BlogClientMethodUnsupportedException("GetPageList");
        }

        public virtual BlogPost[] GetPages(string blogId, int maxPages)
        {
            throw new BlogClientMethodUnsupportedException("GetPages");
        }

        public virtual string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            throw new BlogClientMethodUnsupportedException("NewPage");
        }

        public virtual bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            throw new BlogClientMethodUnsupportedException("EditPage");
        }

        public virtual void DeletePage(string blogId, string pageId)
        {
            throw new BlogClientMethodUnsupportedException("DeletePage");
        }

        public virtual AuthorInfo[] GetAuthors(string blogId)
        {
            throw new BlogClientMethodUnsupportedException("GetAuthors");
        }

        protected virtual string CategoryScheme
        {
            get { return ""; }
        }

        public virtual BlogPost Parse(XmlElement entryNode, bool includeCategories, Uri documentUri)
        {
            BlogPost post = new BlogPost();
            AtomEntry atomEntry = new AtomEntry(_atomVer, _atomNS, CategoryScheme, _nsMgr, documentUri, entryNode);

            post.Title = atomEntry.Title;
            post.Excerpt = atomEntry.Excerpt;
            post.Id = PostUriToPostId(atomEntry.EditUri);
            post.Permalink = atomEntry.Permalink;
            post.Contents = atomEntry.ContentHtml;
            post.DatePublished = atomEntry.PublishDate;
            if (Options.SupportsCategories && includeCategories)
                post.Categories = atomEntry.Categories;

            return post;
        }

        /// <summary>
        /// Take the blog post data and put it into the XML node.
        /// </summary>
        protected virtual void Populate(BlogPost post, Uri documentUri, XmlElement node, bool publish)
        {
            AtomEntry atomEntry = new AtomEntry(_atomVer, _atomNS, CategoryScheme, _nsMgr, documentUri, node);

            if (post.IsNew)
                atomEntry.GenerateId();
            atomEntry.Title = post.Title;
            if (Options.SupportsExcerpt && post.Excerpt != null && post.Excerpt.Length > 0)
                atomEntry.Excerpt = post.Excerpt;
            // extra space is to work around AOL Journals XML parsing bug
            atomEntry.ContentHtml = post.Contents + " ";
            if (Options.SupportsCustomDate && post.HasDatePublishedOverride)
            {
                atomEntry.PublishDate = post.DatePublishedOverride;
            }

            if (Options.SupportsCategories)
            {
                atomEntry.ClearCategories();

                foreach (BlogPostCategory cat in post.Categories)
                    if (!BlogPostCategoryNone.IsCategoryNone(cat))
                        atomEntry.AddCategory(cat);

                if (Options.SupportsNewCategories)
                    foreach (BlogPostCategory cat in post.NewCategories)
                        if (!BlogPostCategoryNone.IsCategoryNone(cat))
                            atomEntry.AddCategory(cat);
            }

            if (Options.SupportsPostAsDraft)
            {
                // remove existing draft nodes
                while (true)
                {
                    XmlNode draftNode = node.SelectSingleNode(@"app:control/app:draft", _nsMgr);
                    if (draftNode == null)
                        break;
                    draftNode.ParentNode.RemoveChild(draftNode);
                }

                if (!publish)
                {
                    // ensure control node exists
                    XmlNode controlNode = node.SelectSingleNode(@"app:control", _nsMgr);
                    if (controlNode == null)
                    {
                        controlNode = node.OwnerDocument.CreateElement(_pubNS.Prefix, "control", _pubNS.Uri);
                        node.AppendChild(controlNode);
                    }
                    // create new draft node
                    XmlElement newDraftNode = node.OwnerDocument.CreateElement(_pubNS.Prefix, "draft", _pubNS.Uri);
                    newDraftNode.InnerText = "yes";
                    controlNode.AppendChild(newDraftNode);
                }
            }

            //post.Categories;
            //post.CommentPolicy;
            //post.CopyFrom;
            //post.Excerpt;
            //post.HasDatePublishedOverride;
            //post.Id;
            //post.IsNew;
            //post.IsTemporary;
            //post.Keywords;
            //post.Link;
            //post.Permalink;
            //post.PingUrls;
            //post.ResetToNewPost;
            //post.TrackbackPolicy;
        }

        public bool? DoesFileNeedUpload(IFileUploadContext uploadContext)
        {
            return null;
        }
    }

    internal struct Namespace
    {
        public Namespace(string uri, string prefix)
        {
            Uri = uri;
            Prefix = prefix;
        }

        public readonly string Uri;
        public readonly string Prefix;
    }
}
