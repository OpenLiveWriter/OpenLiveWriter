using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Extensibility.BlogClient;
using YamlDotNet.Serialization;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("StaticSiteGenerator", "StaticSiteGenerator")]
    class StaticSiteGeneratorClient : BlogClientBase, IBlogClient
    {
        /// <summary>
        /// Path to site directory
        /// </summary>
        private string sitePath;

        /// <summary>
        /// Name of posts directory, appended to sitePath
        /// </summary>
        private string postsPathRel;

        /// <summary>
        /// Name of pages directory, appended to sitePath
        /// </summary>
        private string pagesPathRel;

        /// <summary>
        /// Path to build command, executed from working directory of sitePath
        /// </summary>
        private string buildCmd;

        /// <summary>
        /// Path to publish command, executed from working directory of sitePath
        /// </summary>
        private string publishCmd;

        public BlogClientOptions Options { get; private set; }

        public StaticSiteGeneratorClient(IBlogCredentialsAccessor credentials) : base(credentials)
        {
            LoadConfigurationFromCredentials();

            // Set the client options
            Options = new BlogClientOptions();
            ConfigureClientOptions(Options);
        }

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
        /// Load SSG configuration from client credentials
        /// </summary>
        private void LoadConfigurationFromCredentials()
        {
            sitePath     = Credentials.GetCustomValue("sitePath");
            postsPathRel = Credentials.GetCustomValue("postsPathRel");
            pagesPathRel = Credentials.GetCustomValue("pagesPathRel");
            buildCmd     = Credentials.GetCustomValue("buildCmd");
            publishCmd   = Credentials.GetCustomValue("publishCmd");
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
