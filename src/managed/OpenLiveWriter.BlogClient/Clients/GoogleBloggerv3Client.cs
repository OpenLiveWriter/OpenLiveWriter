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

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("GoogleBloggerv3", "GoogleBloggerv3")]
    public class GoogleBloggerv3Client : BlogClientBase, IBlogClient
    {
        // These URLs map to OAuth2 permission scopes for Google Blogger.
        public static string PicasaServiceScope = "https://picasaweb.google.com/data";
        public static string BloggerServiceScope = BloggerService.Scope.Blogger;

        private static Stream ClientSecretsStream
        {
            get
            {
                // The secrets file is automatically generated at build time by OpenLiveWriter.BlogClient.csproj. It 
                // contains just a client ID and client secret, which are pulled from the user's environment variables.
                return ResourceHelper.LoadAssemblyResourceStream("Clients.GoogleBloggerv3Secrets.json");
            }
        }

        private IBlogClientOptions _clientOptions;

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

        private static IDataStore GetCredentialsDataStoreForBlog(string blogId)
        {
            // The Google APIs will automatically store the OAuth2 tokens in the given path.
            var folderPath = Path.Combine(ApplicationEnvironment.LocalApplicationDataDirectory, "GoogleBloggerv3");
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
                // TODO:OLW - Need to figure out how to make the UI for 'tags' show up in Writer
                // Keywords = string.Join(",", p.Labels)
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
                // TODO:OLW - Need to figure out how to make the UI for 'tags' show up in Writer
                // Keywords = string.Join(",", p.Labels)
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
                Labels = post.Keywords?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToList(),
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
            clientOptions.SupportsGetKeywords = false;
            clientOptions.SupportsPages = true;

            _clientOptions = clientOptions;
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

        private HttpRequestFilter CreateAuthorizationFilter(string requestUri)
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
            Trace.Fail("Google Blogger does not support GetKeywords!");
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
            throw new NotImplementedException();
        }

        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            throw new NotImplementedException();
        }

        public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
            throw new NotImplementedException();
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
            return BlogClientHelper.SendAuthenticatedHttpRequest(requestUri, filter, CreateAuthorizationFilter(requestUri));
        }

        public BlogInfo[] GetImageEndpoints()
        {
            throw new NotImplementedException();
        }
    }
}
