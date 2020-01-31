using OpenLiveWriter.Extensibility.BlogClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using YamlDotNet.RepresentationModel;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSiteItemFrontMatter
    {
        private StaticSiteConfigFrontMatterKeys _frontMatterKeys;

        public string Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Layout { get; set; } = "post";
        public string Slug { get; set; }
        public string[] Tags { get; set; }
        public string ParentId { get; set; } = "";
        public string Permalink { get; set; }

        public StaticSiteItemFrontMatter(StaticSiteConfigFrontMatterKeys frontMatterKeys)
        {
            _frontMatterKeys = frontMatterKeys;
            Tags = new string[] { }; // Initialize Tags to empty array
        }

        /// <summary>
        /// Converts the front matter to it's YAML representation
        /// </summary>
        /// <returns>YAML representation of post front-matter, lines separated by CRLF</returns>
        public string Serialize()
        {
            var root = new YamlMappingNode();

            if (Id != null && Id.Length > 0) root.Add(_frontMatterKeys.IdKey, Id);
            if (Title != null) root.Add(_frontMatterKeys.TitleKey, Title);
            if(Date != null) root.Add(_frontMatterKeys.DateKey, Date);
            if(Layout != null) root.Add(_frontMatterKeys.LayoutKey, Layout);
            if (Tags != null && Tags.Length > 0)
                root.Add(_frontMatterKeys.TagsKey, new YamlSequenceNode(Tags.Select(
                    tag => new YamlScalarNode(tag))));
            if (!string.IsNullOrEmpty(ParentId)) root.Add(_frontMatterKeys.ParentIdKey, ParentId);
            if (!string.IsNullOrEmpty(Permalink)) root.Add(_frontMatterKeys.PermalinkKey, Permalink);

            var stream = new YamlStream(new YamlDocument(root));
            var stringWriter = new StringWriter();
            stream.Save(stringWriter);
            // Trim off end-of-doc
            return new Regex("\\.\\.\\.\r\n$").Replace(stringWriter.ToString(), "", 1);
        }

        /// <summary>
        /// Converts the front matter to it's YAML representation
        /// </summary>
        /// <returns>YAML representation of post front-matter, lines separated by CRLF</returns>
        public override string ToString() => Serialize();

        public void Deserialize(string yaml)
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            var root = (YamlMappingNode)stream.Documents[0].RootNode;

            // Load id
            var idNodes = root.Where(kv => kv.Key.ToString() == _frontMatterKeys.IdKey);
            if (idNodes.Count() > 0) Id = idNodes.First().Value.ToString();

            // Load title
            var titleNodes = root.Where(kv => kv.Key.ToString() == _frontMatterKeys.TitleKey);
            if (titleNodes.Count() > 0) Title = titleNodes.First().Value.ToString();

            // Load date
            var dateNodes = root.Where(kv => kv.Key.ToString() == _frontMatterKeys.DateKey);
            if (dateNodes.Count() > 0) Date = dateNodes.First().Value.ToString();

            // Load layout
            var layoutNodes = root.Where(kv => kv.Key.ToString() == _frontMatterKeys.LayoutKey);
            if (layoutNodes.Count() > 0) Layout = layoutNodes.First().Value.ToString();

            // Load tags
            var tagNodes = root.Where(kv => kv.Key.ToString() == _frontMatterKeys.TagsKey);
            if (tagNodes.Count() > 0 && tagNodes.First().Value.NodeType == YamlNodeType.Sequence)
                Tags = ((YamlSequenceNode)tagNodes.First().Value).Select(node => node.ToString()).ToArray();

            // Load parent ID
            var parentIdNodes = root.Where(kv => kv.Key.ToString() == _frontMatterKeys.ParentIdKey);
            if (parentIdNodes.Count() > 0) ParentId = parentIdNodes.First().Value.ToString();

            // Permalink is never loaded, only saved
        }

        public void LoadFromBlogPost(BlogPost post)
        {
            Id = post.Id;
            Title = post.Title;
            Tags = post.Categories.Union(post.NewCategories).Select(cat => cat.Name).ToArray();
            Date = (post.HasDatePublishedOverride ? post.DatePublishedOverride : post.DatePublished)
                 .ToString("yyyy-MM-dd HH:mm:ss");
            Layout = post.IsPage ? "page" : "post";
            if(post.IsPage) ParentId = post.PageParent.Id;
        }

        public void SaveToBlogPost(BlogPost post)
        {
            post.Id = Id;
            post.Title = Title;
            post.Categories = Tags?.Select(t => new BlogPostCategory(t)).ToArray();
            try { post.DatePublished = post.DatePublishedOverride = DateTime.Parse(Date); } catch { }
            post.IsPage = Layout == "page";
            if (post.IsPage) post.PageParent = new PostIdAndNameField(ParentId, string.Empty);

        }

        public static StaticSiteItemFrontMatter GetFromBlogPost(StaticSiteConfigFrontMatterKeys frontMatterKeys, BlogPost post)
        {
            var frontMatter = new StaticSiteItemFrontMatter(frontMatterKeys);
            frontMatter.LoadFromBlogPost(post);
            return frontMatter;
        }

        public static StaticSiteItemFrontMatter GetFromYaml(StaticSiteConfigFrontMatterKeys frontMatterKeys, string yaml)
        {
            var frontMatter = new StaticSiteItemFrontMatter(frontMatterKeys);
            frontMatter.Deserialize(yaml);
            return frontMatter;
        }
    }
}
