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
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public abstract class StaticSiteItem
    {
        /// <summary>
        /// The extension of published posts to the site project, including dot.
        /// </summary>
        public static string PUBLISH_FILE_EXTENSION = ".html";

        private static Regex POST_PARSE_REGEX = new Regex("^---\r?\n((?:.*\r?\n)*?)---\r?\n\r?\n((?:.*\r?\n?)*)");

        protected StaticSiteConfig SiteConfig;
        public BlogPost BlogPost { get; private set; }
        public bool IsDraft { get; set; } = false;

        public StaticSiteItem(StaticSiteConfig config)
        {
            SiteConfig = config;
            BlogPost = null;
        }

        public StaticSiteItem(StaticSiteConfig config, BlogPost blogPost)
        {
            SiteConfig = config;
            BlogPost = blogPost;
        }

        public StaticSiteItem(StaticSiteConfig config, BlogPost blogPost, bool isDraft)
        {
            SiteConfig = config;
            BlogPost = blogPost;
            IsDraft = isDraft;
        }

        public virtual StaticSiteItemFrontMatter FrontMatter
        {
            get => StaticSiteItemFrontMatter.GetFromBlogPost(SiteConfig.FrontMatterKeys, BlogPost);
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
        /// Unique ID of the BlogPost
        /// </summary>
        public string Id
        {
            get => BlogPost.Id;
            set => BlogPost.Id = value;
        }

        /// <summary>
        /// The safe slug for the post
        /// </summary>
        public string Slug
        {
            get => _safeSlug;
            set => BlogPost.Slug = _safeSlug = value;
        }

        /// <summary>
        /// Confirmed safe slug; does not conflict with any existing post on disk or points to this post on disk.
        /// </summary>
        private string _safeSlug;

        /// <summary>
        /// Get the current on-disk slug from the on-disk post with this ID
        /// </summary>
        public string DiskSlugFromFilePathById
        {
            get => FilePathById == null ? null : GetSlugFromPublishFileName(FilePathById);
        }

        public DateTime DatePublished
        {
            get => BlogPost.HasDatePublishedOverride ? BlogPost.DatePublishedOverride : BlogPost.DatePublished;
            set => BlogPost.DatePublished = BlogPost.DatePublishedOverride = value;
        }

        /// <summary>
        /// Get the on-disk file path for the published post, based on slug
        /// </summary>
        public string FilePathBySlug
        {
            get => GetFilePathForProvidedSlug(Slug);
        }

        protected string _filePathById;
        /// <summary>
        /// Get the on-disk file path for the published post, based on ID
        /// </summary>
        public abstract string FilePathById
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get the site path for the published item
        /// eg. /2019/01/slug.html
        /// </summary>
        public abstract string SitePath { get; }
            
        /// <summary>
        /// Generate a safe slug if the post doesn't already have one. Returns the current or new Slug.
        /// </summary>
        /// <returns>The current or new Slug.</returns>
        public string EnsureSafeSlug()
        {
            if (_safeSlug == null || _safeSlug == string.Empty) Slug = FindNewSlug(BlogPost.Slug, safe: true);
            return Slug;
        }

        /// <summary>
        /// Generate a new Id and save it to the BlogPost if requried. Returns the current or new Id.
        /// </summary>
        /// <returns>The current or new Id</returns>
        public string EnsureId()
        {
            if(Id == null || Id == string.Empty) Id = Guid.NewGuid().ToString();
            return Id;
        }

        /// <summary>
        /// Set post published DateTime to current DateTime if one isn't already set, or current one is default.
        /// </summary>
        /// <returns>The current or new DatePublished.</returns>
        public DateTime EnsureDatePublished()
        {
            if (DatePublished == null || DatePublished == new DateTime(1, 1, 1)) DatePublished = DateTime.UtcNow;
            return DatePublished;
        }

        /// <summary>
        /// Generate a slug for this post based on it's title or a preferred slug
        /// </summary>
        /// <param name="preferredSlug">The text to base the preferred slug off of. default: post title</param>
        /// <param name="safe">Safe mode; if true the returned slug will not conflict with any existing file</param>
        /// <returns>An on-disk slug for this post</returns>
        public string FindNewSlug(string preferredSlug, bool safe)
        {
            // Try the filename without a duplicate identifier, then duplicate identifiers up until 999 before throwing an exception
            for(int i = 0; i < 1000; i++)
            {
                // "Hello World!" -> "hello-world"
                string slug = StaticSiteClient.WEB_UNSAFE_CHARS
                    .Replace((preferredSlug == string.Empty ? BlogPost.Title : preferredSlug).ToLower(), "")
                    .Replace(" ", "-");
                if (!safe) return slug; // If unsafe mode, return the generated slug immediately.

                if (i > 0) slug += $"-{i}";
                if (!File.Exists(GetFilePathForProvidedSlug(slug))) return slug;
            }

            // Couldn't find an available filename, use the post's ID.
            return StaticSiteClient.WEB_UNSAFE_CHARS.Replace(EnsureId(), "").Replace(" ", "-");
        }
        
        /// <summary>
        /// Get the on-disk filename for the provided slug
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>The on-disk filename</returns>
        protected abstract string GetFileNameForProvidedSlug(string slug);

        /// <summary>
        /// Get the on-disk path for the provided slug
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>The on-disk path, including filename from GetFileNameForProvidedSlug</returns>
        protected abstract string GetFilePathForProvidedSlug(string slug);

        protected abstract string GetSlugFromPublishFileName(string publishFileName);

        /// <summary>
        /// If the item is a Post and a Draft, returns the Drafts dir, otherwise returns the regular dir
        /// </summary>
        protected string ItemRelativeDir => 
            IsDraft && !BlogPost.IsPage && SiteConfig.DraftsEnabled ? 
                SiteConfig.DraftsPath 
            : (
                BlogPost.IsPage ?
                    SiteConfig.PagesPath
                :
                    SiteConfig.PostsPath
            );


        /// <summary>
        /// Save the post to the correct directory
        /// </summary>
        public void SaveToFile(string postFilePath)
        {
            // Save the post to disk
            File.WriteAllText(postFilePath, ToString());
        }

        /// <summary>
        /// Load published post from a specified file path
        /// </summary>
        /// <param name="postFilePath">Path to published post file</param>
        public virtual void LoadFromFile(string postFilePath)
        {
            // Attempt to load file contents
            var fileContents = File.ReadAllText(postFilePath);

            // Parse out everything between triple-hyphens into front matter parser
            var frontMatterMatchResult = POST_PARSE_REGEX.Match(fileContents);
            if (!frontMatterMatchResult.Success || frontMatterMatchResult.Groups.Count < 3)
                throw new BlogClientException(Res.Get(StringId.SSGErrorItemLoadTitle), Res.Get(StringId.SSGErrorItemLoadTextFM));
            var frontMatterYaml = frontMatterMatchResult.Groups[1].Value;
            var postContent = frontMatterMatchResult.Groups[2].Value;

            // Create a new BlogPost
            BlogPost = new BlogPost();
            // Parse front matter and save in
            StaticSiteItemFrontMatter.GetFromYaml(SiteConfig.FrontMatterKeys, frontMatterYaml).SaveToBlogPost(BlogPost);

            // Throw error if post does not have an ID
            if (Id == null || Id == string.Empty)
                throw new BlogClientException(Res.Get(StringId.SSGErrorItemLoadTitle), Res.Get(StringId.SSGErrorItemLoadTextId));

            // FilePathById will be the path we loaded this post from
            FilePathById = postFilePath;

            // Load the content into blogpost
            BlogPost.Contents = postContent;

            // Set slug to match file name
            Slug = GetSlugFromPublishFileName(Path.GetFileName(postFilePath));
        }
    }
}
