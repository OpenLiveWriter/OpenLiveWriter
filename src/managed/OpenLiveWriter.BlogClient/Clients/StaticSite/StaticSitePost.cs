using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSitePost : StaticSiteItem
    {
        // Matches the published slug out of a on-disk post
        // 2014-02-02-test.html -> test
        // _posts\2014-02-02-my-post-test.html -> my-post-test
        private static Regex FILENAME_SLUG_REGEX = new Regex(@"^(?:(?:.*?)(?:\\|\/))*(?:\d\d\d\d-\d\d-\d\d-)(.*?)\" + PUBLISH_FILE_EXTENSION + "$");

        public StaticSitePost(StaticSiteConfig config) : base(config)
        {
        }

        public StaticSitePost(StaticSiteConfig config, BlogPost blogPost) : base(config, blogPost)
        {
        }

        public StaticSitePost(StaticSiteConfig config, BlogPost blogPost, bool isDraft) : base(config, blogPost, isDraft)
        {
        }

        protected override string GetSlugFromPublishFileName(string publishFileName) => FILENAME_SLUG_REGEX.Match(publishFileName).Groups[1].Value;

        public override string FilePathById {
            get
            {
                if (_filePathById != null) return _filePathById;

                var foundFile = Directory.GetFiles(Path.Combine(SiteConfig.LocalSitePath, ItemRelativeDir), "*.html")
                .Where(postFile =>
                {
                    try
                    {
                        var post = LoadFromFile(Path.Combine(SiteConfig.LocalSitePath, ItemRelativeDir, postFile), SiteConfig);
                        if (post.Id == Id) return true;
                    }
                    catch { }

                    return false;
                }).DefaultIfEmpty(null).FirstOrDefault();
                return _filePathById = (foundFile == null ? null : Path.Combine(SiteConfig.LocalSitePath, ItemRelativeDir, foundFile));
            }

            protected set => _filePathById = value;
        }

        /// <summary>
        /// We currently do not take configuration for specifiying a post path format
        /// </summary>
        public override string SitePath => throw new NotImplementedException();

        /// <summary>
        /// Gets filename based on slug with prepended date
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>File name with prepended date</returns>
        protected override string GetFileNameForProvidedSlug(string slug)
        {
            return $"{DatePublished.ToString("yyyy-MM-dd")}-{slug}{PUBLISH_FILE_EXTENSION}";
        }

        /// <summary>
        /// Gets a path based on file name and posts path
        /// </summary>
        /// <param name="slug"></param>
        /// <returns>Path containing posts path</returns>
        protected override string GetFilePathForProvidedSlug(string slug)
        {
            return Path.Combine(
                    SiteConfig.LocalSitePath,
                    ItemRelativeDir,
                    GetFileNameForProvidedSlug(slug));
        }

        /// <summary>
        /// Load published post from a specified file path
        /// </summary>
        /// <param name="postFilePath">Path to published post file</param>
        /// <param name="config">StaticSiteConfig to instantiate post with</param>
        /// <returns>A loaded StaticSitePost</returns>
        public static StaticSitePost LoadFromFile(string postFilePath, StaticSiteConfig config)
        {
            var post = new StaticSitePost(config);
            post.LoadFromFile(postFilePath);
            return post;
        }

        /// <summary>
        /// Get all valid posts in PostsPath
        /// </summary>
        /// <returns>An IEnumerable of StaticSitePost</returns>
        public static IEnumerable<StaticSiteItem> GetAllPosts(StaticSiteConfig config, bool includeDrafts) =>
            Directory.GetFiles(Path.Combine(config.LocalSitePath, config.PostsPath), "*.html")
            .Select(fileName => Path.Combine(config.LocalSitePath, config.PostsPath, fileName)) // Create full paths
            .Concat(includeDrafts && config.DraftsEnabled ? // Collect drafts if they're enabled
                    Directory.GetFiles(Path.Combine(config.LocalSitePath, config.DraftsPath), "*.html")
                    .Select(fileName => Path.Combine(config.LocalSitePath, config.DraftsPath, fileName)) // Create full paths
                : 
                    new string[] { } // Drafts are not enabled or were not requested
                )
            .Select(postFile =>
            {
                try
                {
                    return LoadFromFile(postFile, config);
                }
                catch { return null; }
            })
            .Where(p => p != null);

        public static StaticSiteItem GetPostById(StaticSiteConfig config, string id)
            => GetAllPosts(config, true).Where(post => post.Id == id).DefaultIfEmpty(null).FirstOrDefault();
    }
}
