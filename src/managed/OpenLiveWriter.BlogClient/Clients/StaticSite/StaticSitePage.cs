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
    public class StaticSitePage : StaticSiteItem
    {
        public StaticSitePage(StaticSiteConfig config) : base(config)
        {
        }

        public StaticSitePage(StaticSiteConfig config, BlogPost blogPost) : base(config, blogPost)
        {
        }

        public PageInfo PageInfo
        {
            get => new PageInfo(BlogPost.Id, BlogPost.Title, DatePublished, BlogPost.PageParent?.Id);
        }

        public override string FilePathById
        {
            get
            {
                if (_filePathById != null) return _filePathById;
                var foundFile = Directory.GetFiles(Path.Combine(SiteConfig.LocalSitePath, SiteConfig.PagesPath), "*.html")
                .Where(pageFile =>
                {
                    try
                    {
                        var page = LoadFromFile(Path.Combine(SiteConfig.LocalSitePath, SiteConfig.PagesPath, pageFile), SiteConfig);
                        if (page.Id == Id) return true;
                    }
                    catch { }

                    return false;
                }).DefaultIfEmpty(null).FirstOrDefault();
                return _filePathById = (foundFile == null ? null : Path.Combine(SiteConfig.LocalSitePath, SiteConfig.PagesPath, foundFile));
            }

            protected set => _filePathById = value;
        }

        /// <summary>
        /// Get the site path for the published post
        /// eg. /2019/01/slug.html
        /// </summary>
        public override string SitePath
        {
            // TODO Generate permalink based on parent IDs
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// Gets on-disk filename based on slug
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>File name with prepended date</returns>
        protected override string GetFileNameForProvidedSlug(string slug)
        {
            // TODO, get slug for all previous posts and prepend
            return $"{slug}{PUBLISH_FILE_EXTENSION}";
        }

        /// <summary>
        /// Gets a path based on file name and posts path
        /// </summary>
        /// <param name="slug"></param>
        /// <returns>Path containing pages path</returns>
        protected override string GetFilePathForProvidedSlug(string slug)
        {
            return Path.Combine(
                    SiteConfig.LocalSitePath,
                    SiteConfig.PagesPath,
                    GetFileNameForProvidedSlug(slug));
        }

        public StaticSitePage ResolveParent()
        {
            if(!BlogPost.PageParent.IsEmpty)
            {
                // Attempt to locate and load parent 
                var parent = GetPageById(SiteConfig, BlogPost.PageParent.Id);
                if (parent == null)
                {
                    // Parent not found, set PageParent to empty
                    BlogPost.PageParent = PostIdAndNameField.Empty;
                }
                else
                {
                    // Populate Name field
                    BlogPost.PageParent = new PostIdAndNameField(parent.Id, parent.BlogPost.Title);
                }
                return parent;
            }
            return null;
        }

        /// <summary>
        /// Load published page from a specified file path
        /// </summary>
        /// <param name="pageFilePath">Path to published page file</param>
        /// <param name="config">StaticSiteConfig to instantiate page with</param>
        /// <returns>A loaded StaticSitePage</returns>
        public static StaticSitePage LoadFromFile(string pageFilePath, StaticSiteConfig config)
        {
            var page = new StaticSitePage(config);
            page.LoadFromFile(pageFilePath);
            return page;
        }

        /// <summary>
        /// Get all valid pages in PagesPath
        /// </summary>
        /// <returns>An IEnumerable of StaticSitePage</returns>
        public static IEnumerable<StaticSitePage> GetAllPages(StaticSiteConfig config) =>
            Directory.GetFiles(Path.Combine(config.LocalSitePath, config.PagesPath), "*.html")
            .Select(pageFile =>
            {
                try
                {
                    return LoadFromFile(Path.Combine(config.LocalSitePath, config.PagesPath, pageFile), config);
                }
                catch { return null; }
            })
            .Where(p => p != null);

        public static StaticSitePage GetPageById(StaticSiteConfig config, string id)
            => GetAllPages(config).Where(page => page.Id == id).DefaultIfEmpty(null).FirstOrDefault();

    }
}
