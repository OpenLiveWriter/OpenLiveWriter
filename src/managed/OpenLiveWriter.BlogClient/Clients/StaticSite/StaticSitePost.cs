using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSitePost
    {
        public BlogPost BlogPost { get; private set; }

        public StaticSitePost(BlogPost blogPost)
        {
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
    }
}
