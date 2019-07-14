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
                    BlogPost.Slug = GenerateNewSlug();
                return BlogPost.Slug;
            }
        }

        /// <summary>
        /// Get the on-disk file name for the published post, based on slug
        /// </summary>
        public string FileName
        {
            get => GetFileNameForProvidedSlug(Slug);
        }

        /// <summary>
        /// Get the on-disk file path for the published post, based on slug
        /// </summary>
        public string FilePath
        {
            get => GetFilePathForProvidedSlug(Slug);
        }

        /// <summary>
        /// Get the site path for the published post
        /// eg. /2019/01/slug.html
        /// TODO consider removing this feature and associated config setting as we don't actually need this information 
        /// </summary>
        public string SitePath
        {
            get
            {
                if (BlogPost.IsPage) throw new NotImplementedException(); // TODO 

                return SiteConfig.PostUrlFormat
                .Replace("%y", BlogPost.DatePublished.ToString("yyyy"))
                .Replace("%m", BlogPost.DatePublished.ToString("MM"))
                .Replace("%d", BlogPost.DatePublished.ToString("dd"))
                .Replace("%f", $"{Slug}{PUBLISH_FILE_EXTENSION}");
            }
        }

        /// <summary>
        /// Generate a slug for this post based on it's title or a preferred slug
        /// </summary>
        /// <returns>A safe, on-disk slug for this post</returns>
        private string GenerateNewSlug() => GenerateNewSlug("");

        /// <summary>
        /// Generate a slug for this post based on it's title or a preferred slug
        /// </summary>
        /// <returns>A safe, on-disk slug for this post</returns>
        private string GenerateNewSlug(string preferredSlug)
        {
            // Try the filename without a duplicate identifier, then duplicate identifiers up until 999 before throwing an exception
            for(int i = 0; i < 1000; i++)
            {
                // "Hello World!" -> "hello-world"
                string slug = StaticSiteClient.WEB_UNSAFE_CHARS
                    .Replace((preferredSlug == string.Empty ? BlogPost.Title : preferredSlug).ToLower(), "")
                    .Replace(" ", "-");
                if (i > 0) slug += $"-{i}";
                if (!File.Exists(GetFilePathForProvidedSlug(slug))) return slug;
            }
            // Couldn't find an available filename, return a GUID.
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get the Post filenpame for the provided slug
        /// </summary>
        /// <param name="slug"></param>
        private string GetFileNameForProvidedSlug(string slug)
        {
            if(BlogPost.IsPage && SiteConfig.PagesEnabled)
            {
                return $"{BlogPost.DatePublished.ToString("yyyy-MM-dd")}-{slug}{PUBLISH_FILE_EXTENSION}";
            }
            else
            {
                return $"{BlogPost.DatePublished.ToString("yyyy-MM-dd")}-{slug}{PUBLISH_FILE_EXTENSION}";
            }
        }

        private string GetFilePathForProvidedSlug(string slug)
        {
            if (BlogPost.IsPage && SiteConfig.PagesEnabled)
            {
                return Path.Combine(
                    SiteConfig.LocalSitePath,
                    SiteConfig.PagesPath,
                    GetFileNameForProvidedSlug(slug));
            }
            else
            {
                return Path.Combine(
                    SiteConfig.LocalSitePath,
                    SiteConfig.PostsPath,
                    GetFileNameForProvidedSlug(slug));
            }
        }

        /// <summary>
        /// Save the post to the correct directory
        /// </summary>
        public void SaveToDisk() => File.WriteAllText(FilePath, ToString());
    }
}
