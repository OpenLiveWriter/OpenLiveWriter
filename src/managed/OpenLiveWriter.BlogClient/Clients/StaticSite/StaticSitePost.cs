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
        public StaticSitePost(StaticSiteConfig config) : base(config)
        {
        }

        public StaticSitePost(StaticSiteConfig config, BlogPost blogPost) : base(config, blogPost)
        {
        }

        public override string FilePathById {
            get
            {
                if (_filePathById != null) return _filePathById;
                var foundFile = Directory.GetFiles(Path.Combine(SiteConfig.LocalSitePath, SiteConfig.PostsPath), "*.html")
                .Where(postFile =>
                {
                    try
                    {
                        var post = StaticSitePost.LoadFromFile(Path.Combine(SiteConfig.LocalSitePath, SiteConfig.PostsPath, postFile), SiteConfig);
                        if (post.Id == Id) return true;
                    }
                    catch { }

                    return false;
                }).DefaultIfEmpty(null).FirstOrDefault();
                return _filePathById = (foundFile == null ? null : Path.Combine(SiteConfig.LocalSitePath, SiteConfig.PostsPath, foundFile));
            }

            protected set => _filePathById = value;
        }

        /// <summary>
        /// Gets filename based on slug with prepended date
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>File name with prepended date</returns>
        protected override string GetFileNameForProvidedSlug(string slug)
        {
            return $"{BlogPost.DatePublished.ToString("yyyy-MM-dd")}-{slug}{PUBLISH_FILE_EXTENSION}";
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
                    SiteConfig.PostsPath,
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
        public static IEnumerable<StaticSiteItem> GetAllPosts(StaticSiteConfig config) =>
            Directory.GetFiles(Path.Combine(config.LocalSitePath, config.PostsPath), "*.html")
            .Select(postFile =>
            {
                try
                {
                    return LoadFromFile(Path.Combine(config.LocalSitePath, config.PostsPath, postFile), config);
                }
                catch { return null; }
            })
            .Where(p => p != null);

        public static StaticSiteItem GetPostById(StaticSiteConfig config, string id)
            => GetAllPosts(config).Where(post => post.Id == id).DefaultIfEmpty(null).FirstOrDefault();

}
}
