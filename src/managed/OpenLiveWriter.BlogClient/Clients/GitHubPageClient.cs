// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Newtonsoft.Json;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("GitHubPage", "GitHubPage")]
    public class GitHubPageClient : BlogClientBase, IBlogClient
    {
        private static XmlRestRequestHelper xmlRestRequestHelper = new XmlRestRequestHelper();
        private static JSONRestRequestHelper jsonRestRequestHelper = new JSONRestRequestHelper();
        private const string GitHubPageApiUriFormat = "https://api.github.com/repos/{0}/{1}/pages";
        private const string GitHubPageRecentPostsFormat = "https://api.github.com/repos/{0}/{1}/contents/_posts";
        private const string GitHubPagePostFormat = "https://api.github.com/repos/{0}/{1}/contents/_posts/{2}";
        private const string BasicAuthFormat = "{0}:{1}";
        private const string BlogIdFormat = "{0}/{1}";
        private const string BlogManifestFormat = "{0}/olwmanifest.xml";

        private readonly Uri _postApiUrl;
        private string _githubRepoOwner;
        private string _githubRepoName;

        // TODO: GitHub Username may only contain alphanumeric characters or single hyphens, and cannot begin or end with a hyphen
        // TODO: Need to confirm if GitHub will convert all non-alphanumeric characters in repository names to hyphens.
        private readonly Regex blogDetector =
            new Regex("(http|https)://(www.)?github.com/(?<username>([a-zA-Z0-9]+[-]?)+)/(?<repository>[a-zA-Z0-9-.]+)$");

        private IBlogClientOptions _clientOptions;

        public IBlogClientOptions Options
        {
            get { return _clientOptions; }
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

            // TODO: validate postApiUrl
            var repositoryMatch = blogDetector.Match(postApiUrl.AbsoluteUri);
            _githubRepoOwner = repositoryMatch.Groups["username"].Value;
            _githubRepoName = repositoryMatch.Groups["repository"].Value;

            var clientOptions = new BlogClientOptions
            {
                SupportsKeywords = true,
                SupportsCategories = true,
                SupportsNewCategories = true,
                SupportsCustomDate = true,
                SupportsExcerpt = true,
                SupportsHierarchicalCategories = false,
                SupportsMultipleCategories = true,
                SupportsPostAsDraft = true,
                SupportsPages = true,
                KeywordsAsTags = true,
                SupportsGetKeywords = true
            };

            _clientOptions = clientOptions;
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
            try
            {
                WebHeaderCollection responseHeaders;
                var githubApi = new Uri("https://api.github.com");
                jsonRestRequestHelper.Get(ref githubApi, CreateAuthorizationFilter(), out responseHeaders);
            }
            catch (BlogClientAuthenticationException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (!BlogClientUIContext.SilentModeForCurrentThread)
                    ShowError(e.Message);
                throw;
            }
        }

        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            _clientOptions = newClientOptions;
        }

        public BlogInfo[] GetUsersBlogs()
        {
            return GetUsersBlogsInternal();
        }

        private BlogInfo[] GetUsersBlogsInternal()
        {
            WebHeaderCollection responseHeaders;
            var getUsersBlogsUri = new Uri(String.Format(GitHubPageApiUriFormat, _githubRepoOwner, _githubRepoName));
            var responseContent = jsonRestRequestHelper.Get(ref getUsersBlogsUri, CreateAuthorizationFilter(),
                out responseHeaders);

            var blog = JsonConvert.DeserializeObject<GitHubPage>(responseContent);
            var blogId = String.IsNullOrEmpty(blog.cname)
                ? String.Format(BlogIdFormat, _githubRepoOwner, _githubRepoName)
                : blog.cname;
            string homepageUrl;

            if (String.IsNullOrEmpty(blog.cname))
            {
                homepageUrl = String.Equals(_githubRepoName, String.Format("{0}.github.io", _githubRepoOwner),
                    StringComparison.InvariantCultureIgnoreCase)
                    ? _githubRepoName
                    : String.Format("{0}.github.io/{1}", _githubRepoOwner, _githubRepoName);

            }
            else
            {
                homepageUrl = blog.cname;
            }

            return new[] {new BlogInfo(blogId, blogId, new UriBuilder(homepageUrl).Uri.AbsoluteUri)};
        }

        public BlogPostCategory[] GetCategories(string blogId)
        {
            var manifest = GetManifest(blogId);
            var categories =
                manifest.GetElementsByTagName("category")
                    .Cast<XmlNode>()
                    .Select(o => new BlogPostCategory(o.InnerText, o.InnerText))
                    .ToArray();

            return categories;
        }

        public BlogPostKeyword[] GetKeywords(string blogId)
        {
            var manifest = GetManifest(blogId);
            var tags =
                manifest.GetElementsByTagName("tag")
                    .Cast<XmlNode>()
                    .Select(o => new BlogPostKeyword(o.InnerText))
                    .ToArray();
            return tags;
        }

        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            WebHeaderCollection responseHeaders;
            var recentPostsUri = new Uri(String.Format(GitHubPageRecentPostsFormat, _githubRepoOwner, _githubRepoName));
            var responseContent = jsonRestRequestHelper.Get(ref recentPostsUri, CreateAuthorizationFilter(),
                out responseHeaders);
            var githubEntryList = JsonConvert.DeserializeObject<List<GitHubEntry>>(responseContent);
            return githubEntryList.Select(o => new BlogPost
            {
                Title = o.name,
                Id = o.path.Split('/')[1]
            }).ToArray();
        }

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish,
            out string etag, out XmlDocument remotePost)
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

            var json = JsonConvert.SerializeObject(new
            {
                // TODO: make commit message configurable?
                message = "new blog post: " + post.Title,
                content = Convert.ToBase64String(Encoding.UTF8.GetBytes(post.Contents)),
                branch = GetBranchNameByBlogId(blogId)
            });

            Uri uri = new Uri(String.Format(GitHubPagePostFormat, _githubRepoOwner, _githubRepoName, fileName));

            var response = jsonRestRequestHelper.Put(ref uri, "", CreateAuthorizationFilter(),
                "application/json; charset=utf-8", json, "UTF-8", true);

            etag = "";
            remotePost = null;

            return fileName;
        }

        public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish,
            out string etag, out XmlDocument remotePost)
        {
            var githubEntry = GetPostInternal(blogId, post.Id);
            var uri = PostIdToPostUri(post.Id);

            var json = JsonConvert.SerializeObject(new
            {
                message = "delete blog post: " + post.Id,
                branch = GetBranchNameByBlogId(blogId),
                sha = githubEntry.sha
            });

            var response = jsonRestRequestHelper.Delete(ref uri, "", CreateAuthorizationFilter(), "", json, "UTF-8",
                false);

            etag = "";
            remotePost = null;

            return true;
        }

        public BlogPost GetPost(string blogId, string postId)
        {
            var githubEntry = GetPostInternal(blogId, postId);
            var fileBlob = Encoding.UTF8.GetString(Convert.FromBase64String(githubEntry.content));
            var postMetadata = fileBlob.YamlFrontMatter();
            var postContent = fileBlob.MarkdownContent();
            var blogPost = new BlogPost
            {
                Title = githubEntry.name,
                Id = postId,
                Contents = postContent,
                Categories = postMetadata.categories != null ? postMetadata.categories.Select( o => new BlogPostCategory(o) ).ToArray()
                        : new BlogPostCategory[] { new BlogPostCategory(postMetadata.category) },
                Keywords = String.Join(",", postMetadata.tags),
                Excerpt = postMetadata.excerpt
            };

            return blogPost;
        }

        private GitHubEntry GetPostInternal(string blogId, string postId)
        {
            WebHeaderCollection responseHeaders;
            var getPostUri = PostIdToPostUri(postId);
            var responseContent = jsonRestRequestHelper.Get(ref getPostUri, CreateAuthorizationFilter(),
                out responseHeaders);
            return JsonConvert.DeserializeObject<GitHubEntry>(responseContent);
        }

        public void DeletePost(string blogId, string postId, bool publish)
        {
            var githubEntry = GetPostInternal(blogId, postId);
            var uri = PostIdToPostUri(postId);

            var json = JsonConvert.SerializeObject(new
            {
                message = "delete blog post: " + postId,
                branch = GetBranchNameByBlogId(blogId),
                sha = githubEntry.sha
            });

            var response = jsonRestRequestHelper.Delete(ref uri, "", CreateAuthorizationFilter(), "", json, "UTF-8",
                false);
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
            // this has nothing to do with GitHub Page for now.
            return new AuthorInfo[] {};
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
            // Acutally we don't need to do anything as Jekyll won't store categories separately, it's stored in posts' YFM.
            return category.Id;
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

        #region private method

        private XmlDocument GetManifest(string blogId)
        {
            var ownerNrepo = blogId.Split('/');
            if (String.IsNullOrEmpty(ownerNrepo[0]) || String.IsNullOrEmpty(ownerNrepo[1]))
            {
                return null;
            }

            var homePageUrl = GetHomePageUrl(ownerNrepo[0], ownerNrepo[1]);
            var manifestUrl = new UriBuilder(String.Format(BlogManifestFormat, homePageUrl)).Uri;
            return xmlRestRequestHelper.Get(ref manifestUrl, null);
        }

        private string GetHomePageUrl(string repoOwner, string repoName)
        {
            if (String.Format("{0}.github.io", repoOwner).Equals(repoName, StringComparison.InvariantCultureIgnoreCase))
            {
                return repoName;
            }

            WebHeaderCollection responseHeaders;
            var getPostApiUri = new Uri(String.Format(GitHubPageApiUriFormat, repoOwner, repoName));
            var responseContent = jsonRestRequestHelper.Get(ref getPostApiUri, CreateAuthorizationFilter(),
                out responseHeaders);

            var blog = JsonConvert.DeserializeObject<GitHubPage>(responseContent);
            if (String.IsNullOrEmpty(blog.cname))
            {
                return blog.cname;
            }

            return String.Format("{0}.github.io/{1}", repoOwner, repoName);
        }

        private string GetBranch(string repoOwner, string repoName)
        {
            if (String.Equals(repoName, String.Format("{0}.github.io", repoOwner),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return "master";
            }

            return "gh-pages";
        }

        private void ShowError(string error)
        {
            ShowErrorHelper helper =
                new ShowErrorHelper(BlogClientUIContext.ContextForCurrentThread, MessageId.UnexpectedErrorLogin,
                    new object[] {error});
            if (BlogClientUIContext.ContextForCurrentThread != null)
                BlogClientUIContext.ContextForCurrentThread.Invoke(new ThreadStart(helper.Show), null);
            else
                helper.Show();
        }

        private class ShowErrorHelper
        {
            private readonly IWin32Window _owner;
            private readonly MessageId _messageId;
            private readonly object[] _args;

            public ShowErrorHelper(IWin32Window owner, MessageId messageId, object[] args)
            {
                _owner = owner;
                _messageId = messageId;
                _args = args;
            }

            public void Show()
            {
                DisplayMessage.Show(_messageId, _owner, _args);
            }
        }

        private HttpRequestFilter CreateAuthorizationFilter()
        {
            var tc = Login();
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Concat(tc.Username, ":", tc.Password)));

            return request =>
            {
                // OAuth uses a Bearer token in the HTTP Authorization header.
                request.Headers.Add(
                    HttpRequestHeader.Authorization,
                    "Basic " + encoded
                    );
                request.UserAgent = tc.Username;
            };
        }

        private Uri PostIdToPostUri(string postId)
        {
            var postUri = String.Format(GitHubPagePostFormat, _githubRepoOwner, _githubRepoName, postId);
            return new Uri(postUri);
        }

        private string GetBranchNameByBlogId(string blogId)
        {
            if (String.IsNullOrEmpty(blogId))
            {
                return "master";
            }

            var ownerNrepo = blogId.Split('/');
            if (String.IsNullOrEmpty(ownerNrepo[0]) || String.IsNullOrEmpty(ownerNrepo[1]))
            {
                return "master";
            }

            return GetBranch(ownerNrepo[0], ownerNrepo[1]);
        }

        #endregion
    }
}