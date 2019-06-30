using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using YamlDotNet.Serialization;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("StaticSiteGenerator", "StaticSiteGenerator")]
    public class StaticSiteGeneratorClient : BlogClientBase, IBlogClient
    {
        public IBlogClientOptions Options { get; private set; }

        public StaticSiteGeneratorClient(IBlogCredentialsAccessor credentials) : base(credentials)
        {
            // Set the client options
            var options = new BlogClientOptions();
            ConfigureClientOptions(options);
            Options = options;
        }

        protected override void VerifyCredentials(TransientCredentials transientCredentials)
        {
            
        }

        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            Options = newClientOptions;
        }

        public BlogInfo[] GetUsersBlogs() => new BlogInfo[0];

        public BlogPostCategory[] GetCategories(string blogId) => new BlogPostCategory[0];
        public BlogPostKeyword[] GetKeywords(string blogId) => new BlogPostKeyword[0];

        /// <summary>
        /// Returns recent posts
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="maxPosts"></param>
        /// <param name="includeCategories"></param>
        /// <param name="now">If null, then includes future posts.  If non-null, then only includes posts before the *UTC* 'now' time.</param>
        /// <returns></returns>
        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now) => new BlogPost[0];

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = "";
            remotePost = new XmlDocument();
            return "";
        }

        public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = "";
            remotePost = new XmlDocument();
            return false;
        }

        /// <summary>
        /// Attempt to get a post with the specified id (note: may return null
        /// if the post could not be found on the remote server)
        /// </summary>
        public BlogPost GetPost(string blogId, string postId) => new BlogPost();

        public void DeletePost(string blogId, string postId, bool publish)
        {
        }

        public BlogPost GetPage(string blogId, string pageId) => new BlogPost();
        public PageInfo[] GetPageList(string blogId) => new PageInfo[0];
        public BlogPost[] GetPages(string blogId, int maxPages) => new BlogPost[0];

        public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = "";
            remotePost = new XmlDocument();
            return "";
        }

        public bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = "";
            remotePost = new XmlDocument();
            return false;
        }

        public void DeletePage(string blogId, string pageId)
        {
        }

        public AuthorInfo[] GetAuthors(string blogId) => new AuthorInfo[0];
        public bool? DoesFileNeedUpload(IFileUploadContext uploadContext) => false;
        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext) => "";

        public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        public string AddCategory(string blogId, BlogPostCategory category) => "";

        public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
            => new BlogPostCategory[0];

        public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
            => throw new NotImplementedException();

        public BlogInfo[] GetImageEndpoints() => new BlogInfo[0];

        /// <summary>
        /// Returns if this StaticSiteGeneratorClient is secure
        /// Returns true for now as we trust the user publish script
        /// </summary>
        public bool IsSecure => true;

        // Authentication is handled by publish script at the moment 
        protected override bool RequiresPassword => false;

        /// <summary>
        /// Sets the relevant BlogClientOptions for this client
        /// </summary>
        /// <param name="clientOptions">A BlogClientOptions instance</param>
        private void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            // Pages are supported via filesystem
            clientOptions.SupportsPages = true;

            // The follwoing values would be written into YAML front-matter
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsFileUpload = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsAuthor = true;
        }

        /// <summary>
        /// Get a PostFrontMatter instance for a post
        /// </summary>
        /// <param name="post">Post to generate front matter for</param>
        /// <returns></returns>
        private PostFrontMatter GetFrontMatterForPost(BlogPost post) => 
            new PostFrontMatter()
            {
                title = post.Title,
                author = post.Author.Name,
                date = post.DatePublished.ToString("yyyy-MM-dd HH:mm:ss"),
                categories = post.Categories.Select(cat => cat.Name).ToArray(),
                tags = post.Keywords
            };

        private class PostFrontMatter
        {
            public string title { get; set; }
            public string author { get; set; }
            public string date { get; set; }

            public string[] categories { get; set; }
            public string tags { get; set; }
        }
    }
}
