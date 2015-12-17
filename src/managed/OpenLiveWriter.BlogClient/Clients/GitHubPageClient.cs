// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Newtonsoft.Json;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("GitHubPage", "GitHubPage")]
    public class GitHubPageClient : BlogClientBase, IBlogClient
    {
        private const string RepositoryUri = "https://api.github.com/repos";
        private readonly Uri _postApiUrl;
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
            get
            {
                try
                {
                    return String.Equals(_postApiUrl.Scheme, "https", StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        public GitHubPageClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            _postApiUrl = new Uri(UrlHelper.SafeToAbsoluteUri(postApiUrl));
            var clientOptions = new BlogClientOptions
            {
                SupportsCategories = false,
                SupportsMultipleCategories = false,
                SupportsNewCategories = false
            };

            _clientOptions = clientOptions;
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
        }

        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            _clientOptions = newClientOptions;
        }

        public BlogInfo[] GetUsersBlogs()
        {
            return GetUsersBlogs(Credentials.Username, Credentials.Password);
        }

        public BlogInfo[] GetUsersBlogs(string username, string password)
        {
            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
            {
                return null;
            }

            var httpWebRequest = (HttpWebRequest) WebRequest.Create(String.Concat(RepositoryUri, _postApiUrl.AbsolutePath));

            String encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Concat(username, ":", password)));
            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
            httpWebRequest.UserAgent = "DD local";
            var response = (HttpWebResponse)httpWebRequest.GetResponse();

            if ((int) response.StatusCode/100 != 2)
            {
                return null;
            }

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                var blog = JsonConvert.DeserializeObject<GitHubRepository>(sr.ReadToEnd());
                return new [] { new BlogInfo(blog.full_name, blog.full_name, blog.full_name) };
            }
        }

        public BlogPostCategory[] GetCategories(string blogId)
        {
            return new BlogPostCategory[]{};
        }

        public BlogPostKeyword[] GetKeywords(string blogId)
        {
            throw new NotImplementedException();
        }

        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="post"></param>
        /// <param name="newCategoryContext"></param>
        /// <param name="publish"></param>
        /// <param name="etag"></param>
        /// <param name="remotePost"></param>
        /// <returns>Post Id: Absolute path in repository</returns>
        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            // TODO: Follow Jekyll config: slugify mode, https://github.com/jekyll/jekyll/blob/master/lib/jekyll/utils.rb#L158
            var slug = (new Regex("[^a-zA-Z0-9]")).Replace(post.Title, "-");
            // TODO: For posts in Jekyll, the filename contains publish date while draft and pages are dateless. https://github.com/jekyll/jekyll/blob/master/lib/jekyll/document.rb#L9
            var fileName = String.Concat(post.DatePublishedOverride.ToString("yyyy-MM-dd"), "-", slug, ".md");

            var httpWebRequest = (HttpWebRequest) WebRequest.Create(String.Concat(RepositoryUri, _postApiUrl.AbsolutePath, "/contents/_posts/", fileName));
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Concat(Credentials.Username, ":", Credentials.Password)));
            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
            httpWebRequest.UserAgent = "DD local";
            httpWebRequest.Method = "PUT";
            httpWebRequest.ContentType = "application/json; charset=utf-8";

            var json = JsonConvert.SerializeObject(new
            {
                // TODO: make commit message configurable?
                message = "new blog post: " + post.Title,
                content = Convert.ToBase64String(Encoding.UTF8.GetBytes(post.Contents))
            });

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = (HttpWebResponse)httpWebRequest.GetResponse();

            etag = "";
            remotePost = null;

            return (int)response.StatusCode / 100 == 2 ? String.Concat("_posts/", fileName) : null;
        }

        public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="postId">Absolute path in repository.</param>
        /// <returns></returns>
        public BlogPost GetPost(string blogId, string postId)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(String.Concat(RepositoryUri, _postApiUrl.AbsolutePath, "/contents/" + postId));

            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.Username + ":" + Credentials.Password));
            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
            httpWebRequest.UserAgent = "DD local";

            var response = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                var githubEntry = JsonConvert.DeserializeObject<GitHubEntry>(sr.ReadToEnd());
                var blogPost = new BlogPost
                {
                    Title = githubEntry.name,
                    Id = githubEntry.path,
                    Contents = Convert.FromBase64String(githubEntry.content).ToString()
                };

                return blogPost;
            }

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
            throw new BlogClientAuthenticationException("404", "Not found");
        }

        public BlogInfo[] GetImageEndpoints()
        {
            throw new NotImplementedException();
        }
    }
}
