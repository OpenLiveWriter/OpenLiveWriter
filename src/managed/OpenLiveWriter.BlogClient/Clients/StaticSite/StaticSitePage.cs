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
        // Matches the published slug out of a on-disk page
        // page-test_sub-page-test.html -> sub-page-test
        // 0001-01-01-page-test.html -> 0001-01-01-page-test
        // _pages\my-page.html -> my-page
        private static Regex FILENAME_SLUG_REGEX = new Regex(@"^(?:(?:.*?)(?:\\|\/|_))*(.*?)\" + PUBLISH_FILE_EXTENSION + "$");

        private static int PARENT_CRAWL_MAX_LEVELS = 32;

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

        protected override string GetSlugFromPublishFileName(string publishFileName) => FILENAME_SLUG_REGEX.Match(publishFileName).Groups[1].Value;

        public override StaticSiteItemFrontMatter FrontMatter
        {
            get
            {
                var fm = base.FrontMatter;
                fm.Permalink = SitePath;
                return fm;
            }
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
        /// Get the site path ("permalink") for the published page
        /// eg. /about/, /page/sub-page/
        /// </summary>
        public override string SitePath
        {
            get
            {
                // Get slug for all parent posts and prepend
                var parentSlugs = string.Join("/", GetParentSlugs());
                if (parentSlugs != string.Empty) parentSlugs += "/"; // If parent slugs were collected, append slug separator

                return $"/{parentSlugs}{Slug}/"; // parentSlugs will include tailing slash
            }
        }

        /// <summary>
        /// Gets on-disk filename based on slug
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>File name with prepended date</returns>
        protected override string GetFileNameForProvidedSlug(string slug)
        {
            var parentSlugs = string.Join("_", GetParentSlugs());
            if (parentSlugs != string.Empty) parentSlugs += "_"; // If parent slugs were collected, append slug separator
            return $"{parentSlugs}{slug}{PUBLISH_FILE_EXTENSION}";
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
        /// Crawl parent tree and collect all slugs
        /// </summary>
        /// <returns>An array of strings containing the slugs of all parents, in order.</returns>
        private string[] GetParentSlugs()
        {
            List<string> parentSlugs = new List<string>();

            var parentId = BlogPost.PageParent.Id;
            int level = 0;
            while (!string.IsNullOrEmpty(parentId) && level < PARENT_CRAWL_MAX_LEVELS)
            {
                var parent = GetPageById(SiteConfig, parentId);
                if (parent == null)
                    throw new BlogClientException(
                        "Page parent not found",
                        "Could not locate parent for page '{0}' with specified parent ID.",
                        BlogPost.Title);
                parentSlugs.Insert(0, parent.Slug);

                parentId = parent.BlogPost.PageParent.Id;
                level++;
            }

            return parentSlugs.ToArray();
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
