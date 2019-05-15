// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Blogger.v3;
using Google.Apis.Drive.v3;
using GoogleDriveData = Google.Apis.Drive.v3.Data;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2.Flows;
using OpenLiveWriter.BlogClient.Providers;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;
using System.Globalization;
using System.Diagnostics;
using Google.Apis.Blogger.v3.Data;
using OpenLiveWriter.Controls;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("GoogleBloggerv3", "GoogleBloggerv3")]
    public class GoogleBloggerv3Client : BlogClientBase, IBlogClient
    {
        // These URLs map to OAuth2 permission scopes for Google Blogger.
        public static readonly string[] GoogleAPIScopes =
        {
            DriveService.Scope.DriveFile,
            BloggerService.Scope.Blogger
        };
        public static char LabelDelimiter = ',';

        /// <summary>
        /// Maximum number of results the Google Blogger v3 API will return in one request.
        /// </summary>
        public static int MaxResultsPerRequest = 500;

        public static Task<UserCredential> GetOAuth2AuthorizationAsync(string blogId, CancellationToken taskCancellationToken)
        {
            // This async task will either find cached credentials in the IDataStore provided, or it will pop open a 
            // browser window and prompt the user for permissions and then write those permissions to the IDataStore.
            return GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(ClientSecretsStream).Secrets,
                GoogleAPIScopes,
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
                Categories = post.Labels?.Select(x => new BlogPostCategory(x)).ToArray() ?? new BlogPostCategory[0]
            };
        }

        private static Page ConvertToGoogleBloggerPage(BlogPost page, IBlogClientOptions clientOptions)
        {
            return new Page()
            {
                Content = page.Contents,
                Published = GetDatePublishedOverride(page, clientOptions),
                Title = page.Title,
            };
        }

        private static Post ConvertToGoogleBloggerPost(BlogPost post, IBlogClientOptions clientOptions)
        {
            var labels = post.Categories?.Select(x => x.Name).ToList();
            labels?.AddRange(post.NewCategories?.Select(x => x.Name) ?? new List<string>());

            return new Post()
            {
                Content = post.Contents,
                Labels = labels ?? new List<string>(),
                Published = GetDatePublishedOverride(post, clientOptions),
                Title = post.Title,
            };
        }

        private static DateTime? GetDatePublishedOverride(BlogPost post, IBlogClientOptions clientOptions)
        {
            DateTime? datePublishedOverride = post.HasDatePublishedOverride ? post?.DatePublishedOverride : null;
            if (datePublishedOverride.HasValue && clientOptions.UseLocalTime)
            {
                datePublishedOverride = DateTimeHelper.UtcToLocal(datePublishedOverride.Value);
            }

            return datePublishedOverride;
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
        private const string GPHOTO_NS_URI = "http://schemas.google.com/photos/2007";

        private static readonly Namespace atomNS = new Namespace(AtomProtocolVersion.V10DraftBlogger.NamespaceUri, "atom");
        private static readonly Namespace pubNS = new Namespace(AtomProtocolVersion.V10DraftBlogger.PubNamespaceUri, "app");
        private static readonly Namespace photoNS = new Namespace(GPHOTO_NS_URI, "gphoto");

        private IBlogClientOptions _clientOptions;
        private XmlNamespaceManager _nsMgr;

        public GoogleBloggerv3Client(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            // configure client options
            BlogClientOptions clientOptions = new BlogClientOptions();
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsExcerpt = false;
            clientOptions.SupportsSlug = false;
            clientOptions.SupportsFileUpload = true;
            clientOptions.SupportsKeywords = false;
            clientOptions.SupportsGetKeywords = false;
            clientOptions.SupportsPages = true;
            clientOptions.SupportsExtendedEntries = true;
            clientOptions.UseLocalTime = true;
            _clientOptions = clientOptions;

            _nsMgr = new XmlNamespaceManager(new NameTable());
            _nsMgr.AddNamespace(atomNS.Prefix, atomNS.Uri);
            _nsMgr.AddNamespace(pubNS.Prefix, pubNS.Uri);
            _nsMgr.AddNamespace(photoNS.Prefix, photoNS.Uri);
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

        private DriveService GetDriveService()
        {
            TransientCredentials transientCredentials = Login();
            return new DriveService(new BaseClientService.Initializer()
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
                    Scopes = GoogleAPIScopes,
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
            return blogList.Items?.Select(b => new BlogInfo(b.Id, b.Name, b.Url)).ToArray() ?? new BlogInfo[0];
        }

        private const string CategoriesEndPoint = "/feeds/posts/summary?alt=json&max-results=0";
        public BlogPostCategory[] GetCategories(string blogId)
        {
            var categories = new BlogPostCategory[0];
            var blog = GetService().Blogs.Get(blogId).Execute();

            if (blog != null)
            {
                var categoriesUrl = string.Concat(blog.Url, CategoriesEndPoint);

                var response = SendAuthenticatedHttpRequest(categoriesUrl, 30, CreateAuthorizationFilter());
                if (response != null)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var json = reader.ReadToEnd();
                        var item = JsonConvert.DeserializeObject<CategoryResponse>(json);
                        var cats = item?.Feed?.CategoryArray?.Select(x => new BlogPostCategory(x.Term));
                        categories = cats?.ToArray() ?? new BlogPostCategory[0];
                    }
                }
            }

            return categories;
        }

        public BlogPostKeyword[] GetKeywords(string blogId)
        {
            // Google Blogger does not support get labels
            return new BlogPostKeyword[] { };
        }

        private PostList ListRecentPosts(string blogId, int maxPosts, DateTime? now, PostsResource.ListRequest.StatusEnum status, PostList previousPage)
        {
            if (previousPage != null && string.IsNullOrWhiteSpace(previousPage.NextPageToken))
            {
                // The previous page was also the last page, so do nothing and return an empty list.
                return new PostList();
            }

            var recentPostsRequest = GetService().Posts.List(blogId);
            if (now.HasValue)
            {
                recentPostsRequest.EndDate = now.Value;
            }
            recentPostsRequest.FetchImages = false;
            recentPostsRequest.MaxResults = maxPosts;
            recentPostsRequest.OrderBy = PostsResource.ListRequest.OrderByEnum.Published;
            recentPostsRequest.Status = status;
            recentPostsRequest.PageToken = previousPage?.NextPageToken;

            return recentPostsRequest.Execute();
        }

        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            // Blogger requires separate API calls to get drafts vs. live vs. scheduled posts. We aggregate each 
            // type of post separately.
            IList<Post> draftRecentPosts = new List<Post>();
            IList<Post> liveRecentPosts = new List<Post>();
            IList<Post> scheduledRecentPosts = new List<Post>();
            IEnumerable<Post> allPosts = new List<Post>();

            // We keep around the PostList returned by each request to support pagination.
            PostList draftRecentPostsList = null;
            PostList liveRecentPostsList = null;
            PostList scheduledRecentPostsList = null;

            // Google has a per-request results limit on their API.
            var maxResultsPerRequest = Math.Min(maxPosts, MaxResultsPerRequest);

            // We break out of the following loop depending on which one of these two cases we hit: 
            //   (a) the number of all blog posts ever posted to this blog is greater than maxPosts, so eventually 
            //       allPosts.count() will exceed maxPosts and we can stop making requests.
            //   (b) the number of all blog posts ever posted to this blog is less than maxPosts, so eventually our 
            //       calls to ListRecentPosts() will return 0 results and we need to stop making requests.
            do
            {
                draftRecentPostsList = ListRecentPosts(blogId, maxResultsPerRequest, now, PostsResource.ListRequest.StatusEnum.Draft, draftRecentPostsList);
                liveRecentPostsList = ListRecentPosts(blogId, maxResultsPerRequest, now, PostsResource.ListRequest.StatusEnum.Live, liveRecentPostsList);
                scheduledRecentPostsList = ListRecentPosts(blogId, maxResultsPerRequest, now, PostsResource.ListRequest.StatusEnum.Scheduled, scheduledRecentPostsList);

                draftRecentPosts = draftRecentPostsList?.Items ?? new List<Post>();
                liveRecentPosts = liveRecentPostsList?.Items ?? new List<Post>();
                scheduledRecentPosts = scheduledRecentPostsList?.Items ?? new List<Post>();
                allPosts = allPosts.Concat(draftRecentPosts).Concat(liveRecentPosts).Concat(scheduledRecentPosts);

            } while (allPosts.Count() < maxPosts && (draftRecentPosts.Count > 0 || liveRecentPosts.Count > 0 || scheduledRecentPosts.Count > 0));

            return allPosts
                .OrderByDescending(p => p.Published)
                .Take(maxPosts)
                .Select(ConvertToBlogPost)
                .ToArray() ?? new BlogPost[0];
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

            var bloggerPost = ConvertToGoogleBloggerPost(post, _clientOptions);
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

            var bloggerPost = ConvertToGoogleBloggerPost(post, _clientOptions);
            var updatePostRequest = GetService().Posts.Update(bloggerPost, blogId, post.Id);
            updatePostRequest.Publish = publish;

            var updatedPost = updatePostRequest.Execute();
            etag = updatedPost.ETag;
            return true;
        }

        public BlogPost GetPost(string blogId, string postId)
        {
            var getPostRequest = GetService().Posts.Get(blogId, postId);
            getPostRequest.View = PostsResource.GetRequest.ViewEnum.AUTHOR;
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
            getPageRequest.View = PagesResource.GetRequest.ViewEnum.AUTHOR;
            return ConvertToBlogPost(getPageRequest.Execute());
        }

        private PageList ListPages(string blogId, int? maxPages, PagesResource.ListRequest.StatusEnum status, PageList previousPage)
        {
            if (previousPage != null && string.IsNullOrWhiteSpace(previousPage.NextPageToken))
            {
                // The previous page was also the last page, so do nothing and return an empty list.
                return new PageList();
            }

            var getPagesRequest = GetService().Pages.List(blogId);
            if (maxPages.HasValue)
            {
                // Google has a per-request results limit on their API.
                getPagesRequest.MaxResults = Math.Min(maxPages.Value, MaxResultsPerRequest);
            }
            getPagesRequest.Status = status;
            return getPagesRequest.Execute();
        }

        private IEnumerable<Page> ListAllPages(string blogId, int? maxPages)
        {
            // Blogger requires separate API calls to get drafts vs. live vs. scheduled posts. We aggregate each 
            // type of post separately.
            IList<Page> draftPages = new List<Page>();
            IList<Page> livePages = new List<Page>();
            IEnumerable<Page> allPages = new List<Page>();

            // We keep around the PageList returned by each request to support pagination.
            PageList draftPagesList = null;
            PageList livePagesList = null;

            // We break out of the following loop depending on which one of these two cases we hit: 
            //   (a) the number of all blog pages ever posted to this blog is greater than maxPages, so eventually 
            //       allPages.count() will exceed maxPages and we can stop making requests.
            //   (b) the number of all blog pages ever posted to this blog is less than maxPages, so eventually our 
            //       calls to ListPages() will return 0 results and we need to stop making requests.
            do
            {
                draftPagesList = ListPages(blogId, maxPages, PagesResource.ListRequest.StatusEnum.Draft, draftPagesList);
                livePagesList = ListPages(blogId, maxPages, PagesResource.ListRequest.StatusEnum.Live, livePagesList);

                draftPages = draftPagesList?.Items ?? new List<Page>();
                livePages = livePagesList?.Items ?? new List<Page>();
                allPages = allPages.Concat(draftPages).Concat(livePages);

            } while (allPages.Count() < maxPages && (draftPages.Count > 0 || livePages.Count > 0));

            return allPages;
        }

        public PageInfo[] GetPageList(string blogId)
        {
            return ListAllPages(blogId, null)
                .OrderByDescending(p => p.Published)
                .Select(ConvertToPageInfo)
                .ToArray() ?? new PageInfo[0];
        }

        public BlogPost[] GetPages(string blogId, int maxPages)
        {
            return ListAllPages(blogId, maxPages)
                .OrderByDescending(p => p.Published)
                .Select(ConvertToBlogPost)
                .Take(maxPages)
                .ToArray() ?? new BlogPost[0];
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

            var bloggerPage = ConvertToGoogleBloggerPage(page, _clientOptions);
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

            var bloggerPage = ConvertToGoogleBloggerPage(page, _clientOptions);
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

            return PostNewImage(albumName, path);
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

        #region Google Drive image uploading, heavily adapted from Picasa image uploading - stolen from BloggerAtomClient

        private List<GoogleDriveData.File> GetAllFolders(DriveService drive)
        {
            // Navigate GDrive pagination and return a list of all the user's top level folders
            var folders = new List<GoogleDriveData.File>();
            GoogleDriveData.FileList fileList;
            string pageToken = null;
            do
            {
                var listRequest = drive.Files.List();
                listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
                fileList = listRequest.Execute();

                if (fileList.Files != null) foreach (var folder in fileList.Files) folders.Add(folder);
                pageToken = fileList.NextPageToken;
            } while (pageToken != null);
            return folders;
        }

        private GoogleDriveData.File GetBlogImagesFolder(DriveService drive, string folderName)
        {
            // Get the ID of the Google Drive 'Open Live Writer' folder, creating it if it doesn't exist
            var matchingFolders = GetAllFolders(drive).Where(folder => folder.Name == folderName);
            if (matchingFolders.Count() > 0) return matchingFolders.First();

            // Attempt to create and return the folder as it does not exist
            return drive.Files.Create(new GoogleDriveData.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            }).Execute();
        }

        private string PostNewImage(string imagesFolderName, string filename)
        {
            var drive = GetDriveService();
            var imagesFolder = GetBlogImagesFolder(drive, imagesFolderName);
            FilesResource.CreateMediaUpload uploadReq;

            // Create a FileStream for the image to upload
            using (var imageFileStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
                // Detect mime type for file based on extension
                var imageMime = MimeMapping.GetMimeMapping(filename);
                // Upload the image to the images folder, naming it with a GUID to prevent clashes
                uploadReq = drive.Files.Create(new GoogleDriveData.File()
                {
                    Name = Guid.NewGuid().ToString(),
                    Parents = new string[] { imagesFolder.Id },
                    OriginalFilename = Path.GetFileName(filename)
                }, imageFileStream, imageMime);
                uploadReq.Fields = "id,webContentLink"; // Retrieve Id and WebContentLink fields
                var uploadRes = uploadReq.Upload();
                if (uploadRes.Status != Google.Apis.Upload.UploadStatus.Completed)
                    throw new BlogClientFileTransferException(
                        String.Format(Res.Get(StringId.BCEFileTransferTransferringFile), Path.GetFileName(filename)), 
                        "BloggerDriveError",
                        $"Google Drive image upload for {Path.GetFileName(filename)} failed.\nDetails: {uploadRes.Exception}");
            }

            // Make the uploaded file public
            var imageFile = uploadReq.ResponseBody;
            drive.Permissions.Create(new GoogleDriveData.Permission()
            {
                Type = "anyone",
                Role = "reader"
            }, imageFile.Id).Execute();
            
            // Retrieve the appropiate URL for inlining the image, splitting off the download parameter
            return imageFile.WebContentLink.Split('&').First();
        }
        #endregion

        public class Category
        {
            [JsonProperty("term")]
            public string Term { get; set; }
        }

        public class Feed
        {
            [JsonProperty("category")]
            public Category[] CategoryArray { get; set; }
        }

        public class CategoryResponse
        {
            [JsonProperty("feed")]
            public Feed Feed { get; set; }
        }
    }
}
