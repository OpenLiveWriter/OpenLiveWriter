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
            // The Google APIs will automatically store the OAuth2 tokens in the given path. We use a unique path per 
            // blog to support multiple Blogger accounts.
            var folderPath = Path.Combine(ApplicationEnvironment.LocalApplicationDataDirectory, "GoogleBloggerv3");
            return new FileDataStore(folderPath, true);
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
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsExcerpt = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsFileUpload = true;
            _clientOptions = clientOptions;
        }

        protected override TransientCredentials Login()
        {
            TransientCredentials transientCredentials = Credentials.TransientCredentials as TransientCredentials;
            VerifyAndRefreshCredentials(transientCredentials);
            return transientCredentials;
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
            VerifyAndRefreshCredentials(tc);
        }

        private void VerifyAndRefreshCredentials(TransientCredentials tc)
        {
            var flowInitializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecretsStream = ClientSecretsStream,
                DataStore = GetCredentialsDataStoreForBlog(tc.Username),
                Scopes = new List<string>() { BloggerServiceScope, PicasaServiceScope },
            };
            var flow = new GoogleAuthorizationCodeFlow(flowInitializer);
            var cancellationTokenSource = new CancellationTokenSource();

            // Attempt to load a cached OAuth token.
            var loadTokenTask = flow.LoadTokenAsync(tc.Username, cancellationTokenSource.Token);
            loadTokenTask.Wait();
            if (loadTokenTask.IsCompleted)
            {
                var token = loadTokenTask.Result;
                if (token == null || (token.RefreshToken == null && token.IsExpired(flow.Clock)))
                {
                    // The token is invalid, so we need to login again.
                    if (BlogClientUIContext.SilentModeForCurrentThread)
                    {
                        // If we're in silent mode where prompting isn't allowed, throw the verification exception
                        throw new BlogClientAuthenticationException(String.Empty, String.Empty);
                    }

                    var authorizationTask = GetOAuth2AuthorizationAsync(tc.Username, cancellationTokenSource.Token);
                    authorizationTask.Wait();

                    if (authorizationTask.Result?.Token == null)
                    {
                        throw new BlogClientOperationCancelledException();
                    }
                }
            }
        }

        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            _clientOptions = newClientOptions;
        }

        public BlogInfo[] GetUsersBlogs()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var userCredentialsTask = GetOAuth2AuthorizationAsync(Credentials.Username, cancellationTokenSource.Token);
            userCredentialsTask.Wait(cancellationTokenSource.Token);

            BloggerService service = new BloggerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentialsTask.Result
            });

            var listBlogsTask = service.Blogs.ListByUser("self").ExecuteAsync();
            listBlogsTask.Wait(cancellationTokenSource.Token);
            return listBlogsTask.Result?.Items?.Select(x => new BlogInfo(x.Id, x.Name, x.Url)).ToArray();
        }

        public BlogPostCategory[] GetCategories(string blogId)
        {
            throw new NotImplementedException();
        }

        public BlogPostKeyword[] GetKeywords(string blogId)
        {
            throw new NotImplementedException();
        }

        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            throw new NotImplementedException();
        }

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            throw new NotImplementedException();
        }

        public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            throw new NotImplementedException();
        }

        public BlogPost GetPost(string blogId, string postId)
        {
            throw new NotImplementedException();
        }

        public void DeletePost(string blogId, string postId, bool publish)
        {
            throw new NotImplementedException();
        }

        public BlogPost GetPage(string blogId, string pageId)
        {
            throw new NotImplementedException();
        }

        public PageInfo[] GetPageList(string blogId)
        {
            throw new NotImplementedException();
        }

        public BlogPost[] GetPages(string blogId, int maxPages)
        {
            throw new NotImplementedException();
        }

        public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            throw new NotImplementedException();
        }

        public bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            throw new NotImplementedException();
        }

        public void DeletePage(string blogId, string pageId)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
        {
            throw new NotImplementedException();
        }

        public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
        {
            throw new NotImplementedException();
        }

        public BlogInfo[] GetImageEndpoints()
        {
            throw new NotImplementedException();
        }
    }
}
