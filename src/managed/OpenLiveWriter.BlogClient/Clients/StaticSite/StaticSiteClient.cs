using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;


namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    [BlogClient(StaticSiteClient.CLIENT_TYPE, StaticSiteClient.CLIENT_TYPE)]
    public class StaticSiteClient : BlogClientBase, IBlogClient
    {
        // The 'provider' concept doesn't really apply to local static sites
        // Store these required constants here so they're in one place
        public const string PROVIDER_ID = "D0E0062F-7540-4462-94FD-DC55004D95E6";
        public const string SERVICE_NAME = "Static Site Generator"; // TODO- Move to Strings
        public const string POST_API_URL = "http://localhost/"; // A valid URI is required for BlogClientManager to instantiate a URI object on.
        public const string CLIENT_TYPE = "StaticSite";

        public static Regex WEB_UNSAFE_CHARS = new Regex("[^A-Za-z0-9- ]*");

        public IBlogClientOptions Options { get; private set; }

        private StaticSiteConfig Config;

        public StaticSiteClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            Config = StaticSiteConfig.LoadConfigFromCredentials(credentials);

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
        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now) =>
            StaticSitePost.GetAllPosts(Config)
            .Select(post => post.BlogPost)
            .Where(post => post != null && (now == null || post.DatePublished < now))
            .OrderByDescending(post => post.DatePublished)
            .Take(maxPosts)
            .ToArray();

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            if(!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Static site does not support drafts, cannot post.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }
            remotePost = null;
            etag = "";

            // Create a StaticSitePost on the provided post
            var ssgPost = new StaticSitePost(Config, post);
            // Ensure the post has an ID
            var newPostId = ssgPost.EnsureId();
            // Ensure the post has a date
            ssgPost.EnsureDatePublished();
            // Save the post to disk
            ssgPost.SaveToFile();

            try
            {
                // Build the site, if required
                if (Config.BuildCommand != string.Empty) DoSiteBuild();

                // Publish the site 
                DoSitePublish();

                return newPostId;
            } catch (Exception ex)
            {
                // Clean up our output file
                File.Delete(ssgPost.FilePath);
                // Throw the exception up
                throw ex;
            }
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

        public AuthorInfo[] GetAuthors(string blogId) => throw new NotImplementedException();

        public bool? DoesFileNeedUpload(IFileUploadContext uploadContext) => false;
        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext) => "";

        public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        public string AddCategory(string blogId, BlogPostCategory category) =>
            throw new BlogClientMethodUnsupportedException("AddCategory");

        public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
            => throw new BlogClientMethodUnsupportedException("SuggestCategories");

        public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
            => throw new NotImplementedException("HTTP requests not implemented for static sites"); // TODO This is used for downloading writing manifest XMLs. Throw an exception for now.

        public BlogInfo[] GetImageEndpoints() 
            => throw new NotImplementedException();

        /// <summary>
        /// Returns if this StaticSiteGeneratorClient is secure
        /// Returns true for now as we trust the user publish script
        /// </summary>
        public bool IsSecure => true;

        /// <summary>
        /// Always false. It is not possible to perform remote detection on a static site, as
        /// it may not be published yet, or published to a web location.
        /// </summary>
        public override bool RemoteDetectionPossible { get; } = false;

        // Authentication is handled by publish script at the moment 
        protected override bool RequiresPassword => false;

        /// <summary>
        /// Build the static site
        /// </summary>
        private void DoSiteBuild()
        {
            var proc = RunSiteCommand(Config.BuildCommand);
            if (proc.ExitCode != 0)
            {
                throw new BlogClientException(
                    StringId.SSGBuildErrorTitle,
                    StringId.SSGBuildErrorText,
                    Res.Get(StringId.ProductNameVersioned),
                    proc.ExitCode.ToString(),
                    proc.StandardOutput.ReadToEnd(),
                    proc.StandardError.ReadToEnd()
                );
            }
        }

        /// <summary>
        /// Publish the static site
        /// </summary>
        private void DoSitePublish()
        {
            var proc = RunSiteCommand(Config.PublishCommand);
            if (proc.ExitCode != 0)
            {
                throw new BlogClientException(
                    StringId.SSGPublishErrorTitle,
                    StringId.SSGPublishErrorText,
                    Res.Get(StringId.ProductNameVersioned),
                    proc.ExitCode.ToString(),
                    proc.StandardOutput.ReadToEnd(),
                    proc.StandardError.ReadToEnd()
                );
            }
        }

        /// <summary>
        /// Run a command from the site directory
        /// </summary>
        /// <param name="localCommand">Command to run, releative to site directory</param>
        /// <returns></returns>
        private Process RunSiteCommand(string localCommand)
        {
            var proc = new Process();

            // If a 32-bit process on a 64-bit system, call the 64-bit cmd
            proc.StartInfo.FileName = (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem) ? 
                $"{Environment.GetEnvironmentVariable("windir")}\\Sysnative\\cmd.exe" : // 32-on-64, launch sysnative cmd
                "cmd.exe"; // Launch regular cmd

            // Set working directory to local site path
            proc.StartInfo.WorkingDirectory = Config.LocalSitePath;

            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            
            proc.StartInfo.Arguments = $"/C {localCommand}";
            proc.Start();
            proc.WaitForExit();

            // The Process will have all standard output waiting in buffer
            return proc;
        }

        /// <summary>
        /// Sets the relevant BlogClientOptions for this client based on values from the StaticSiteConfig
        /// </summary>
        /// <param name="clientOptions">A BlogClientOptions instance</param>
        private void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsPages = clientOptions.SupportsPageParent = Config.PagesEnabled;
            clientOptions.SupportsPostAsDraft = Config.DraftsEnabled;
            clientOptions.SupportsFileUpload = Config.ImagesEnabled;
            clientOptions.SupportsImageUpload = Config.ImagesEnabled ? SupportsFeature.Yes : SupportsFeature.No;
            clientOptions.SupportsScripts = clientOptions.SupportsEmbeds = SupportsFeature.Yes;

            // Categories treated as tags for the time being
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsKeywords = false;

            // The follwoing values would be written into YAML front-matter
            clientOptions.FuturePublishDateWarning = true;
            clientOptions.SupportsCustomDate = clientOptions.SupportsCustomDateUpdate = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsAuthor = false;
        }
    }
}
