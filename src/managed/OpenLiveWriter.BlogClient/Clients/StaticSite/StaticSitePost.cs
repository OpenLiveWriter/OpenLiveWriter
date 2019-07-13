using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSitePost
    {
        public static string PUBLISH_FILE_EXTENSION = ".html";

        private StaticSiteConfig SiteConfig;
        public BlogPost BlogPost { get; private set; }

        public StaticSitePost(StaticSiteConfig config, BlogPost blogPost)
        {
            SiteConfig = config;
            BlogPost = blogPost;
        }

        public StaticSitePostFrontMatter FrontMatter
        {
            get => StaticSitePostFrontMatter.GetFromBlogPost(BlogPost);
        }

        /// <summary>
        /// Converts the post to a string, ready to be written to disk
        /// </summary>
        /// <returns>String representation of the post, including front-matter, lines separated by LF</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("---");
            builder.Append(FrontMatter.ToString());
            builder.AppendLine("---");
            builder.AppendLine();
            builder.Append(BlogPost.Contents);
            return builder.ToString().Replace("\r\n", "\n");
        }

        /// <summary>
        /// Get the slug for the post, or generate it if it currently does not have one
        /// </summary>
        public string Slug
        {
            get
            {
                if (BlogPost.Slug == string.Empty)
                    BlogPost.Slug = GenerateSlugFromTitle();
                return BlogPost.Slug;
            }
        }

        /// <summary>
        /// Get the on-disk file name for the published post, based on slug
        /// </summary>
        public string FileName
        {
            get => $"{BlogPost.DatePublished.ToString("yyyy-MM-dd")}-{Slug}{PUBLISH_FILE_EXTENSION}";
        }

        /// <summary>
        /// Get the on-disk file path for the published post, based on slug
        /// </summary>
        public string FilePath
        {
            get => Path.Combine(
                    SiteConfig.LocalSitePath,
                    (BlogPost.IsPage && SiteConfig.PagesEnabled) ? SiteConfig.PagesPath : SiteConfig.PostsPath,
                    FileName);
        }

        /// <summary>
        /// Get the site path for the published post
        /// eg. /2019/01/slug.html
        /// </summary>
        public string SitePath
        {
            get => SiteConfig.PostUrlFormat
                .Replace("%y", BlogPost.DatePublished.ToString("yyyy"))
                .Replace("%m", BlogPost.DatePublished.ToString("MM"))
                .Replace("%d", BlogPost.DatePublished.ToString("dd"))
                .Replace("%f", $"{Slug}{PUBLISH_FILE_EXTENSION}");
        }

        /// <summary>
        /// Generate a slug for this post based on it's title
        /// </summary>
        /// <returns>The on-disk file name for this post</returns>
        public string GenerateSlugFromTitle()
        {
            // Try the filename without a duplicate identifier, then duplicate identifiers up until 999 before throwing an exception
            for(int i = 0; i < 1000; i++)
            {
                // "Hello World!" -> "hello-world"
                string slug = StaticSiteClient.WEB_UNSAFE_CHARS.Replace(BlogPost.Title.ToLower(), "").Replace(" ", "-");
                if (i > 0) slug += $"-{i}";

                var fileName = $"{BlogPost.DatePublished.ToString("yyyy-MM-dd")}-{slug}{PUBLISH_FILE_EXTENSION}";
                var filePath = Path.Combine(
                    SiteConfig.LocalSitePath,
                    (BlogPost.IsPage && SiteConfig.PagesEnabled) ? SiteConfig.PagesPath : SiteConfig.PostsPath,
                    fileName);

                if (!File.Exists(filePath)) return slug;
            }

            // Couldn't find an available filename, return a GUID.
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Save the post to the correct directory
        /// </summary>
        public void SaveToDisk() => File.WriteAllText(FilePath, ToString());
    }
}
