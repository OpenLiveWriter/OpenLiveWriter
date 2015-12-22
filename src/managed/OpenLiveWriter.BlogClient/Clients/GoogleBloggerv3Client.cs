// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Blogger.v3;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2.Flows;
using OpenLiveWriter.BlogClient.Providers;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;
using System.Globalization;
using System.Diagnostics;
using Google.Apis.Blogger.v3.Data;
using System.Net.Http.Headers;
using OpenLiveWriter.Controls;
using System.Windows.Forms;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("GoogleBloggerv3", "GoogleBloggerv3")]
    public class GoogleBloggerv3Client : BlogClientBase, IBlogClient
    {
        // These URLs map to OAuth2 permission scopes for Google Blogger.
        public static string PicasaServiceScope = "https://picasaweb.google.com/data";
        public static string BloggerServiceScope = BloggerService.Scope.Blogger;
        public static char LabelDelimiter = ',';

        public static Task<UserCredential> GetOAuth2AuthorizationAsync(string blogId, CancellationToken taskCancellationToken)
        {
            // This async task will either find cached credentials in the IDataStore provided, or it will pop open a 
            // browser window and prompt the user for permissions and then write those permissions to the IDataStore.
            return GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(ClientSecretsStream).Secrets,
                new List<string>() { BloggerServiceScope, PicasaServiceScope },
                blogId,
                taskCancellationToken,
                GetCredentialsDataStoreForBlog(blogId));
        }

        private static Stream ClientSecretsStream
        {
            get
            {
                // The secrets file is automatically generated at build time by OpenLiveWriter.BlogClient.csproj. It 
                // contains just a client ID and client secret, which are pulled from the user's environment variables.
                return ResourceHelper.LoadAssemblyResourceStream("Clients.GoogleBloggerv3Secrets.json");
            }
        }

        private static IDataStore GetCredentialsDataStoreForBlog(string blogId)
        {
            // The Google APIs will automatically store the OAuth2 tokens in the given path.
            var folderPath = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "GoogleBloggerv3");
            return new FileDataStore(folderPath, true);
        }

        private static BlogPost ConvertToBlogPost(Page page)
        {
            return new BlogPost()
            {
                Title = page.Title,
                Id = page.Id,
                Permalink = page.Url,
                Contents = page.Content,
                DatePublished = page.Published.Value,
                //Keywords = string.Join(LabelDelimiter, page.Labels)
            };
        }

        private static BlogPost ConvertToBlogPost(Post post)
        {
            return new BlogPost()
            {
                Title = post.Title,
                Id = post.Id,
                Permalink = post.Url,
                Contents = post.Content,
                DatePublished = post.Published.Value,
                Keywords = string.Join(new string(LabelDelimiter,1), post.Labels ?? new List<string>())
            };
        }

        private static Page ConvertToGoogleBloggerPage(BlogPost page)
        {
            return new Page()
            {
                Content = page.Contents,
                // TODO:OLW - DatePublishedOverride didn't work quite right. Either the date published override was off by several hours, 
                // needs to be normalized to UTC or the Blogger website thinks I'm in the wrong time zone.
                Published = page.HasDatePublishedOverride ? page?.DatePublishedOverride : null,
                Title = page.Title,
            };
        }

        private static Post ConvertToGoogleBloggerPost(BlogPost post)
        {
            return new Post()
            {
                Content = post.Contents,
                Labels = post.Keywords?.Split(new char[] { LabelDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToList(),
                // TODO:OLW - DatePublishedOverride didn't work quite right. Either the date published override was off by several hours, 
                // needs to be normalized to UTC or the Blogger website thinks I'm in the wrong time zone.
                Published = post.HasDatePublishedOverride ? post?.DatePublishedOverride : null,
                Title = post.Title,
            };
        }

        private static PageInfo ConvertToPageInfo(Page page)
        {
            // Google Blogger doesn't support parent/child pages, so we pass string.Empty.
            return new PageInfo(page.Id, page.Title, page.Published.GetValueOrDefault(DateTime.Now), string.Empty);
        }

        private const int MaxRetries = 5;

        private const string ENTRY_CONTENT_TYPE = "application/atom+xml;type=entry";
        private const string XHTML_NS = "http://www.w3.org/1999/xhtml";
        private const string FEATURES_NS = "http://purl.org/atompub/features/1.0";
        private const string MEDIA_NS = "http://search.yahoo.com/mrss/";
        private const string LIVE_NS = "http://api.live.com/schemas";

        private static readonly Namespace atomNS = new Namespace(AtomProtocolVersion.V10DraftBlogger.NamespaceUri, "atom");
        private static readonly Namespace pubNS = new Namespace(AtomProtocolVersion.V10DraftBlogger.PubNamespaceUri, "app");

        private IBlogClientOptions _clientOptions;
        private XmlNamespaceManager _nsMgr;

        public GoogleBloggerv3Client(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            // configure client options
            BlogClientOptions clientOptions = new BlogClientOptions();
            clientOptions.SupportsCategories = false;
            clientOptions.SupportsMultipleCategories = false;
            clientOptions.SupportsNewCategories = false;
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsExcerpt = false;
            clientOptions.SupportsSlug = false;
            clientOptions.SupportsFileUpload = true;
            clientOptions.SupportsKeywords = true;
            clientOptions.SupportsGetKeywords = true;
            clientOptions.SupportsPages = true;
            clientOptions.SupportsExtendedEntries = true;
            _clientOptions = clientOptions;

            _nsMgr = new XmlNamespaceManager(new NameTable());
            _nsMgr.AddNamespace(atomNS.Prefix, atomNS.Uri);
            _nsMgr.AddNamespace(pubNS.Prefix, pubNS.Uri);
            _nsMgr.AddNamespace(AtomClient.xhtmlNS.Prefix, AtomClient.xhtmlNS.Uri);
            _nsMgr.AddNamespace(AtomClient.featuresNS.Prefix, AtomClient.featuresNS.Uri);
            _nsMgr.AddNamespace(AtomClient.mediaNS.Prefix, AtomClient.mediaNS.Uri);
            _nsMgr.AddNamespace(AtomClient.liveNS.Prefix, AtomClient.liveNS.Uri);
        }

        public IBlogClientOptions Options
        {
            get
            {
                return _clientOptions;
            }
        }

        public bool IsSecure
        {
            get { return true; }
        }

        private BloggerService GetService()
        {
            TransientCredentials transientCredentials = Login();
            return new BloggerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = (UserCredential)transientCredentials.Token,
                ApplicationName = string.Format(CultureInfo.InvariantCulture, "{0} {1}", ApplicationEnvironment.ProductName, ApplicationEnvironment.ProductVersion),
            });
        }

        private bool IsValidToken(TokenResponse token)
        {
            // If the token is expired but we have a non-null RefreshToken, we can assume the token will be 
            // automatically refreshed when we query Google Blogger and is therefore valid.
            return token != null && (!token.IsExpired(SystemClock.Default) || token.RefreshToken != null);
        }

        protected override TransientCredentials Login()
        {
            var transientCredentials = Credentials.TransientCredentials as TransientCredentials ?? 
                new TransientCredentials(Credentials.Username, Credentials.Password, null);
            VerifyAndRefreshCredentials(transientCredentials);
            Credentials.TransientCredentials = transientCredentials;
            return transientCredentials;
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
            VerifyAndRefreshCredentials(tc);
        }

        private void VerifyAndRefreshCredentials(TransientCredentials tc)
        {
            var userCredential = tc.Token as UserCredential;
            var token = userCredential?.Token;

            if (IsValidToken(token))
            {
                // We already have a valid OAuth token.
                return;
            }

            if (userCredential == null)
            {
                // Attempt to load a cached OAuth token.
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecretsStream = ClientSecretsStream,
                    DataStore = GetCredentialsDataStoreForBlog(tc.Username),
                    Scopes = new List<string>() { BloggerServiceScope, PicasaServiceScope },
                });

                var loadTokenTask = flow.LoadTokenAsync(tc.Username, CancellationToken.None);
                loadTokenTask.Wait();
                if (loadTokenTask.IsCompleted)
                {
                    // We were able re-create the user credentials from the cache.
                    userCredential = new UserCredential(flow, tc.Username, loadTokenTask.Result);
                    token = loadTokenTask.Result;
                }
            }

            if (!IsValidToken(token))
            {
                // The token is invalid, so we need to login again. This likely includes popping out a new browser window.
                if (BlogClientUIContext.SilentModeForCurrentThread)
                {
                    // If we're in silent mode where prompting isn't allowed, throw the verification exception
                    throw new BlogClientAuthenticationException(String.Empty, String.Empty);
                }

                // Start an OAuth flow to renew the credentials.
                var authorizationTask = GetOAuth2AuthorizationAsync(tc.Username, CancellationToken.None);
                authorizationTask.Wait();
                if (authorizationTask.IsCompleted)
                {
                    userCredential = authorizationTask.Result;
                    token = userCredential?.Token;
                }
            }

            if (!IsValidToken(token))
            {
                // The token is still invalid after all of our attempts to refresh it. The user did not complete the 
                // authorization flow, so we interpret that as a cancellation.
                throw new BlogClientOperationCancelledException();
            }

            // Stash the valid user credentials.
            tc.Token = userCredential;
        }

        private void RefreshAccessToken(TransientCredentials transientCredentials)
        {
            // Using the BloggerService automatically refreshes the access token, but we call the Picasa endpoint 
            // directly and therefore need to force refresh the access token on occasion.
            var userCredential = transientCredentials.Token as UserCredential;
            userCredential?.RefreshTokenAsync(CancellationToken.None).Wait();
        }

        private HttpRequestFilter CreateAuthorizationFilter()
        {
            var transientCredentials = Login();
            var userCredential = (UserCredential)transientCredentials.Token;
            var accessToken = userCredential.Token.AccessToken;

            return (HttpWebRequest request) =>
            {
                // OAuth uses a Bearer token in the HTTP Authorization header.
                request.Headers.Add(
                    HttpRequestHeader.Authorization,
                    string.Format(CultureInfo.InvariantCulture, "Bearer {0}", accessToken));
            };
        }

        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            _clientOptions = newClientOptions;
        }

        public BlogInfo[] GetUsersBlogs()
        {
            var blogList = GetService().Blogs.ListByUser("self").Execute();
            return blogList.Items.Select(b => new BlogInfo(b.Id, b.Name, b.Url)).ToArray();
        }

        public BlogPostCategory[] GetCategories(string blogId)
        {
            // Google Blogger does not support categories
            return new BlogPostCategory[] { };
        }

        public BlogPostKeyword[] GetKeywords(string blogId)
        {
            // Google Blogger does not support get labels
            return new BlogPostKeyword[] { };
        }

        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            var recentPostsRequest = GetService().Posts.List(blogId);
            if (now.HasValue)
            {
                recentPostsRequest.EndDate = now.Value;
            }
            recentPostsRequest.FetchImages = false;
            recentPostsRequest.MaxResults = maxPosts;
            recentPostsRequest.OrderBy = PostsResource.ListRequest.OrderByEnum.Published;
            recentPostsRequest.Status = PostsResource.ListRequest.StatusEnum.Live;

            var recentPosts = recentPostsRequest.Execute();
            return recentPosts.Items.Select(p => ConvertToBlogPost(p)).ToArray();
        }

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPost = ConvertToGoogleBloggerPost(post);
            var newPostRequest = GetService().Posts.Insert(bloggerPost, blogId);
            newPostRequest.IsDraft = !publish;

            var newPost = newPostRequest.Execute();
            etag = newPost.ETag;
            return newPost.Id;
        }

        public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPost = ConvertToGoogleBloggerPost(post);
            var updatePostRequest = GetService().Posts.Update(bloggerPost, blogId, post.Id);
            updatePostRequest.Publish = publish;

            var updatedPost = updatePostRequest.Execute();
            etag = updatedPost.ETag;
            return true;
        }

        public BlogPost GetPost(string blogId, string postId)
        {
            var getPostRequest = GetService().Posts.Get(blogId, postId);
            return ConvertToBlogPost(getPostRequest.Execute());
        }

        public void DeletePost(string blogId, string postId, bool publish)
        {
            var deletePostRequest = GetService().Posts.Delete(blogId, postId);
            deletePostRequest.Execute();
        }

        public BlogPost GetPage(string blogId, string pageId)
        {
            var getPageRequest = GetService().Pages.Get(blogId, pageId);
            return ConvertToBlogPost(getPageRequest.Execute());
        }

        public PageInfo[] GetPageList(string blogId)
        {
            var getPagesRequest = GetService().Pages.List(blogId);

            var pageList = getPagesRequest.Execute();
            return pageList.Items.Select(p => ConvertToPageInfo(p)).ToArray();
        }

        public BlogPost[] GetPages(string blogId, int maxPages)
        {
            var getPagesRequest = GetService().Pages.List(blogId);
            getPagesRequest.MaxResults = maxPages;

            var pageList = getPagesRequest.Execute();
            return pageList.Items.Select(p => ConvertToBlogPost(p)).ToArray();
        }

        public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPage = ConvertToGoogleBloggerPage(page);
            var newPageRequest = GetService().Pages.Insert(bloggerPage, blogId);
            newPageRequest.IsDraft = !publish;

            var newPage = newPageRequest.Execute();
            etag = newPage.ETag;
            return newPage.Id;
        }

        public bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPage = ConvertToGoogleBloggerPage(page);
            var updatePostRequest = GetService().Pages.Update(bloggerPage, blogId, page.Id);
            updatePostRequest.Publish = publish;

            var updatedPage = updatePostRequest.Execute();
            etag = updatedPage.ETag;
            return true;
        }

        public void DeletePage(string blogId, string pageId)
        {
            var deletePostRequest = GetService().Pages.Delete(blogId, pageId);
            deletePostRequest.Execute();
        }

        public AuthorInfo[] GetAuthors(string blogId)
        {
            throw new NotImplementedException();
        }

        public bool? DoesFileNeedUpload(IFileUploadContext uploadContext)
        {
            return null;
        }

        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            string albumName = ApplicationEnvironment.ProductName;

            string path = uploadContext.GetContentsLocalFilePath();

            if (Options.FileUploadNameFormat != null && Options.FileUploadNameFormat.Length > 0)
            {
                string formattedFileName = uploadContext.FormatFileName(uploadContext.PreferredFileName);
                string[] chunks = StringHelper.Reverse(formattedFileName).Split(new char[] { '/' }, 2);
                if (chunks.Length == 2)
                    albumName = StringHelper.Reverse(chunks[1]);
            }

            string EDIT_MEDIA_LINK = "EditMediaLink";
            string srcUrl;
            string editUri = uploadContext.Settings.GetString(EDIT_MEDIA_LINK, null);
            if (editUri == null || editUri.Length == 0)
            {
                PostNewImage(albumName, path, out srcUrl, out editUri);
            }
            else
            {
                try
                {
                    UpdateImage(editUri, path, out srcUrl, out editUri);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    if (e is WebException)
                        HttpRequestHelper.LogException((WebException)e);

                    bool success = false;
                    srcUrl = null; // compiler complains without this line
                    try
                    {
                        // couldn't update existing image? try posting a new one
                        PostNewImage(albumName, path, out srcUrl, out editUri);
                        success = true;
                    }
                    catch
                    {
                    }
                    if (!success)
                        throw;  // rethrow the exception from the update, not the post
                }
            }
            uploadContext.Settings.SetString(EDIT_MEDIA_LINK, editUri);

            PicasaRefererBlockingWorkaround(uploadContext.BlogId, uploadContext.Role, ref srcUrl);

            return srcUrl;
        }

        /// <summary>
        /// "It looks like the problem with the inline image is due to referrer checking.
        /// The thumbnail image being used is protected for display only on certain domains.
        /// These domains include *.blogspot.com and *.google.com.  This user is using a
        /// feature in Blogger which allows him to display his blog directly on his own
        /// domain, which will not pass the referrer checking.
        ///
        /// "The maximum size of a thumbnail image that can be displayed on non-*.blogspot.com
        /// domains is 800px. (blogs don't actually appear at *.google.com).  However, if you
        /// request a 800px thumbnail, and the image is less than 800px for the maximum
        /// dimension, then the original image will be returned without the referrer
        /// restrictions.  That sounds like it will work for you, so feel free to give it a
        /// shot and let me know if you have any further questions or problems."
        ///   -- Anonymous Google Employee
        /// </summary>
        private void PicasaRefererBlockingWorkaround(string blogId, FileUploadRole role, ref string srcUrl)
        {
            if (role == FileUploadRole.LinkedImage && Options.UsePicasaS1600h)
            {
                try
                {
                    int lastSlash = srcUrl.LastIndexOf('/');
                    string srcUrl2 = srcUrl.Substring(0, lastSlash)
                                     + "/s1600-h"
                                     + srcUrl.Substring(lastSlash);
                    HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest(srcUrl2, true);
                    req.Method = "HEAD";
                    req.GetResponse().Close();
                    srcUrl = srcUrl2;
                    return;
                }
                catch (WebException we)
                {
                    Debug.Fail("Picasa s1600-h hack failed: " + we.ToString());
                }
            }

            try
            {
                srcUrl += ((srcUrl.IndexOf('?') >= 0) ? "&" : "?") + "imgmax=800";
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error while doing Picasa upload: " + ex.ToString());
            }
        }

        public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
            // Nothing to do.
        }

        public string AddCategory(string blogId, BlogPostCategory category)
        {
            throw new BlogClientMethodUnsupportedException("AddCategory");
        }

        public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
        {
            throw new BlogClientMethodUnsupportedException("SuggestCategories");
        }

        public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
        {
            return BlogClientHelper.SendAuthenticatedHttpRequest(requestUri, filter, CreateAuthorizationFilter());
        }

        public BlogInfo[] GetImageEndpoints()
        {
            throw new NotImplementedException();
        }

        #region Picasa image uploading - stolen from BloggerAtomClient

        public string GetBlogImagesAlbum(string albumName)
        {
            const string FEED_REL = "http://schemas.google.com/g/2005#feed";
            const string GPHOTO_NS_URI = "http://schemas.google.com/photos/2007";
            
            Uri picasaUri = new Uri("https://picasaweb.google.com/data/feed/api/user/default");

            try
            {
                Uri reqUri = picasaUri;
                XmlDocument albumListDoc = AtomClient.xmlRestRequestHelper.Get(ref reqUri, CreateAuthorizationFilter(), "kind", "album");
                foreach (XmlElement entryEl in albumListDoc.SelectNodes(@"/atom:feed/atom:entry", _nsMgr))
                {
                    XmlElement titleNode = entryEl.SelectSingleNode(@"atom:title", _nsMgr) as XmlElement;
                    if (titleNode != null)
                    {
                        string titleText = AtomProtocolVersion.V10DraftBlogger.TextNodeToPlaintext(titleNode);
                        if (titleText == albumName)
                        {
                            XmlNamespaceManager nsMgr2 = new XmlNamespaceManager(new NameTable());
                            nsMgr2.AddNamespace("gphoto", "http://schemas.google.com/photos/2007");
                            XmlNode numPhotosRemainingNode = entryEl.SelectSingleNode("gphoto:numphotosremaining/text()", nsMgr2);
                            if (numPhotosRemainingNode != null)
                            {
                                int numPhotosRemaining;
                                if (int.TryParse(numPhotosRemainingNode.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out numPhotosRemaining))
                                {
                                    if (numPhotosRemaining < 1)
                                        continue;
                                }
                            }
                            string selfHref = AtomEntry.GetLink(entryEl, _nsMgr, FEED_REL, "application/atom+xml", null, reqUri);
                            if (selfHref.Length > 1)
                                return selfHref;
                        }
                    }
                }
            }
            catch (WebException we)
            {
                HttpWebResponse httpWebResponse = we.Response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    HttpRequestHelper.DumpResponse(httpWebResponse);
                    if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new BlogClientOperationCancelledException();
                    }
                }
                throw;
            }

            XmlDocument newDoc = new XmlDocument();
            XmlElement newEntryEl = newDoc.CreateElement("atom", "entry", AtomProtocolVersion.V10DraftBlogger.NamespaceUri);
            newDoc.AppendChild(newEntryEl);

            XmlElement newTitleEl = newDoc.CreateElement("atom", "title", AtomProtocolVersion.V10DraftBlogger.NamespaceUri);
            newTitleEl.SetAttribute("type", "text");
            newTitleEl.InnerText = albumName;
            newEntryEl.AppendChild(newTitleEl);

            XmlElement newSummaryEl = newDoc.CreateElement("atom", "summary", AtomProtocolVersion.V10DraftBlogger.NamespaceUri);
            newSummaryEl.SetAttribute("type", "text");
            newSummaryEl.InnerText = Res.Get(StringId.BloggerImageAlbumDescription);
            newEntryEl.AppendChild(newSummaryEl);

            XmlElement newAccessEl = newDoc.CreateElement("gphoto", "access", GPHOTO_NS_URI);
            newAccessEl.InnerText = "private";
            newEntryEl.AppendChild(newAccessEl);

            XmlElement newCategoryEl = newDoc.CreateElement("atom", "category", AtomProtocolVersion.V10DraftBlogger.NamespaceUri);
            newCategoryEl.SetAttribute("scheme", "http://schemas.google.com/g/2005#kind");
            newCategoryEl.SetAttribute("term", "http://schemas.google.com/photos/2007#album");
            newEntryEl.AppendChild(newCategoryEl);

            Uri postUri = picasaUri;
            XmlDocument newAlbumResult = AtomClient.xmlRestRequestHelper.Post(ref postUri, CreateAuthorizationFilter(), "application/atom+xml", newDoc, null);
            XmlElement newAlbumResultEntryEl = newAlbumResult.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement;
            Debug.Assert(newAlbumResultEntryEl != null);
            return AtomEntry.GetLink(newAlbumResultEntryEl, _nsMgr, FEED_REL, "application/atom+xml", null, postUri);
        }

        private void ShowPicasaSignupPrompt(object sender, EventArgs e)
        {
            if (DisplayMessage.Show(MessageId.PicasawebSignup) == DialogResult.Yes)
            {
                ShellHelper.LaunchUrl("http://picasaweb.google.com");
            }
        }

        private void PostNewImage(string albumName, string filename, out string srcUrl, out string editUri)
        {
            for (int retry = 0; retry < MaxRetries; retry++)
            {
                var transientCredentials = Login();
                try
                {
                    string albumUrl = GetBlogImagesAlbum(albumName);
                    HttpWebResponse response = RedirectHelper.GetResponse(albumUrl, new RedirectHelper.RequestFactory(new UploadFileRequestFactory(this, filename, "POST").Create));
                    using (Stream s = response.GetResponseStream())
                    {
                        ParseMediaEntry(s, out srcUrl, out editUri);
                        return;
                    }
                }
                catch (WebException we)
                {
                    if (retry < MaxRetries - 1 &&
                        we.Response as HttpWebResponse != null &&
                        ((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        // HTTP 403 Forbidden means our OAuth access token is not valid.
                        RefreshAccessToken(transientCredentials);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Trace.Fail("Should never get here");
            throw new ApplicationException("Should never get here");
        }

        private void UpdateImage(string editUri, string filename, out string srcUrl, out string newEditUri)
        {
            for (int retry = 0; retry < MaxRetries; retry++)
            {
                var transientCredentials = Login();
                HttpWebResponse response;
                bool conflict = false;
                try
                {
                    response = RedirectHelper.GetResponse(editUri, new RedirectHelper.RequestFactory(new UploadFileRequestFactory(this, filename, "PUT").Create));
                }
                catch (WebException we)
                {
                    if (retry < MaxRetries - 1 && 
                        we.Response as HttpWebResponse != null)
                    {
                        if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Conflict)
                        {
                            response = (HttpWebResponse)we.Response;
                            conflict = true;
                        }
                        else if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Forbidden)
                        {
                            // HTTP 403 Forbidden means our OAuth access token is not valid.
                            RefreshAccessToken(transientCredentials);
                            continue;
                        }
                    }

                    throw;
                }

                using (Stream s = response.GetResponseStream())
                {
                    ParseMediaEntry(s, out srcUrl, out newEditUri);
                }

                if (!conflict)
                {
                    return; // success!
                }

                editUri = newEditUri;
            }

            Trace.Fail("Should never get here");
            throw new ApplicationException("Should never get here");
        }

        private void ParseMediaEntry(Stream s, out string srcUrl, out string editUri)
        {
            srcUrl = null;

            // First try <content src>
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(s);
            XmlElement contentEl = xmlDoc.SelectSingleNode("/atom:entry/atom:content", _nsMgr) as XmlElement;
            if (contentEl != null)
                srcUrl = XmlHelper.GetUrl(contentEl, "@src", _nsMgr, null);

            // Then try media RSS
            if (srcUrl == null || srcUrl.Length == 0)
            {
                contentEl = xmlDoc.SelectSingleNode("/atom:entry/media:group/media:content[@medium='image']", _nsMgr) as XmlElement;
                if (contentEl == null)
                    throw new ArgumentException("Picasa photo entry was missing content element");
                srcUrl = XmlHelper.GetUrl(contentEl, "@url", _nsMgr, null);
            }

            editUri = AtomEntry.GetLink(xmlDoc.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement, _nsMgr, "edit-media", null, null, null);
        }

        private class UploadFileRequestFactory
        {
            private readonly GoogleBloggerv3Client _parent;
            private readonly string _filename;
            private readonly string _method;

            public UploadFileRequestFactory(GoogleBloggerv3Client parent, string filename, string method)
            {
                _parent = parent;
                _filename = filename;
                _method = method;
            }

            public HttpWebRequest Create(string uri)
            {
                // TODO: choose rational timeout values
                HttpWebRequest request = HttpRequestHelper.CreateHttpWebRequest(uri, false);

                _parent.CreateAuthorizationFilter().Invoke(request);

                request.ContentType = MimeHelper.GetContentType(Path.GetExtension(_filename));
                try
                {
                    request.Headers.Add("Slug", Path.GetFileNameWithoutExtension(_filename));
                }
                catch (ArgumentException)
                {
                    request.Headers.Add("Slug", "Image");
                }

                request.Method = _method;

                using (Stream s = request.GetRequestStream())
                {
                    using (Stream inS = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        StreamHelper.Transfer(inS, s);
                    }
                }

                return request;
            }
        }

        #endregion

    }
}
