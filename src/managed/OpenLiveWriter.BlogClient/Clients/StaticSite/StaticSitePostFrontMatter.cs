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
    public class StaticSitePostFrontMatter
    {
        /// Hardcode these to Jekyll defaults for now
        /// TODO Load from StaticSiteConfig
        private StaticSiteConfigFrontMatterKeys frontMatterKeys
            = new StaticSiteConfigFrontMatterKeys()
            {
                TitleKey = "title",
                DateKey = "date",
                LayoutKey = "layout",
                TagsKey = "tags"
            };

        public string Title { get; set; }
        public string Date { get; set; }
        public string Layout { get; set; } = "post";
        public string[] Tags { get; set; }

        public StaticSitePostFrontMatter()
        {
            Tags = new string[] { }; // Initialize Tags to empty array
        }

        /// <summary>
        /// Converts the front matter to it's YAML representation
        /// </summary>
        /// <returns>YAML representation of post front-matter, lines separated by CRLF</returns>
        public string Serialize()
        {
            var root = new YamlMappingNode();

            if(Title != null) root.Add(frontMatterKeys.TitleKey, Title);
            if(Date != null) root.Add(frontMatterKeys.DateKey, Date);
            if(Layout != null) root.Add(frontMatterKeys.LayoutKey, Layout);
            if (Tags != null && Tags.Length > 0)
                root.Add(frontMatterKeys.TagsKey, new YamlSequenceNode(Tags.Select(
                    tag => new YamlScalarNode(tag))));

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

            // Load title
            var titleNodes = root.Where(kv => kv.Key.ToString() == frontMatterKeys.TitleKey);
            if (titleNodes.Count() > 0) Title = titleNodes.First().Value.ToString();

            // Load date
            var dateNodes = root.Where(kv => kv.Key.ToString() == frontMatterKeys.DateKey);
            if (dateNodes.Count() > 0) Date = dateNodes.First().Value.ToString();

            // Load layout
            var layoutNodes = root.Where(kv => kv.Key.ToString() == frontMatterKeys.LayoutKey);
            if (layoutNodes.Count() > 0) Layout = layoutNodes.First().Value.ToString();

            // Load tags
            var tagNodes = root.Where(kv => kv.Key.ToString() == frontMatterKeys.TagsKey);
            if (tagNodes.Count() > 0 && tagNodes.First().Value.NodeType == YamlNodeType.Sequence)
                Tags = ((YamlSequenceNode)tagNodes.First().Value).Select(node => node.ToString()).ToArray();
        }

        public void LoadFromBlogPost(BlogPost post)
        {
            Title = post.Title;
            Tags = post.Categories.Select(cat => cat.Name).ToArray();
            Date = (post.HasDatePublishedOverride ? post.DatePublishedOverride : post.DatePublished)
                .ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            Layout = post.IsPage ? "page" : "post";
        }

        public void SaveToBlogPost(BlogPost post)
        {
            post.Title = Title;
            post.Categories = Tags?.Select(t => new BlogPostCategory(t)).ToArray();
            try { post.DatePublished = DateTime.Parse(Date); } catch { }
            post.IsPage = Layout == "page";
        }

        public static StaticSitePostFrontMatter GetFromBlogPost(BlogPost post)
        {
            var frontMatter = new StaticSitePostFrontMatter();
            frontMatter.LoadFromBlogPost(post);
            return frontMatter;
        }

        public static StaticSitePostFrontMatter GetFromYaml(string yaml)
        {
            var frontMatter = new StaticSitePostFrontMatter();
            frontMatter.Deserialize(yaml);
            return frontMatter;
        }
    }
}
