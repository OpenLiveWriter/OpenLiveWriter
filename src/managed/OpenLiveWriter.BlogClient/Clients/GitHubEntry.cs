using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace OpenLiveWriter.BlogClient.Clients
{
    public class GitHubPage
    {
        public string url { get; set; }
        public string status { get; set; }
        public string cname { get; set; }
        public bool custom_404 { get; set; }
    }

    public class GitHubRepository
    {
        public int id { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
        public Owner owner { get; set; }
        public bool _private { get; set; }
        public string html_url { get; set; }
        public string description { get; set; }
        public bool fork { get; set; }
        public string url { get; set; }
        public string forks_url { get; set; }
        public string keys_url { get; set; }
        public string collaborators_url { get; set; }
        public string teams_url { get; set; }
        public string hooks_url { get; set; }
        public string issue_events_url { get; set; }
        public string events_url { get; set; }
        public string assignees_url { get; set; }
        public string branches_url { get; set; }
        public string tags_url { get; set; }
        public string blobs_url { get; set; }
        public string git_tags_url { get; set; }
        public string git_refs_url { get; set; }
        public string trees_url { get; set; }
        public string statuses_url { get; set; }
        public string languages_url { get; set; }
        public string stargazers_url { get; set; }
        public string contributors_url { get; set; }
        public string subscribers_url { get; set; }
        public string subscription_url { get; set; }
        public string commits_url { get; set; }
        public string git_commits_url { get; set; }
        public string comments_url { get; set; }
        public string issue_comment_url { get; set; }
        public string contents_url { get; set; }
        public string compare_url { get; set; }
        public string merges_url { get; set; }
        public string archive_url { get; set; }
        public string downloads_url { get; set; }
        public string issues_url { get; set; }
        public string pulls_url { get; set; }
        public string milestones_url { get; set; }
        public string notifications_url { get; set; }
        public string labels_url { get; set; }
        public string releases_url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime pushed_at { get; set; }
        public string git_url { get; set; }
        public string ssh_url { get; set; }
        public string clone_url { get; set; }
        public string svn_url { get; set; }
        public object homepage { get; set; }
        public int size { get; set; }
        public int stargazers_count { get; set; }
        public int watchers_count { get; set; }
        public string language { get; set; }
        public bool has_issues { get; set; }
        public bool has_downloads { get; set; }
        public bool has_wiki { get; set; }
        public bool has_pages { get; set; }
        public int forks_count { get; set; }
        public object mirror_url { get; set; }
        public int open_issues_count { get; set; }
        public int forks { get; set; }
        public int open_issues { get; set; }
        public int watchers { get; set; }
        public string default_branch { get; set; }
        public int network_count { get; set; }
        public int subscribers_count { get; set; }
    }

    public class Owner
    {
        public string login { get; set; }
        public int id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }

    internal class GitHubEntry
    {
        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string git_url { get; set; }
        public string download_url { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public string encoding { get; set; }
        public _Links _links { get; set; }

        public class _Links
        {
            public string self { get; set; }
            public string git { get; set; }
            public string html { get; set; }
        }

    }

    /// <summary>
    /// http://jekyllrb.com/docs/frontmatter/
    /// </summary>
    public class PageMetadata
    {
        // Predefined Global Variables
        public string layout;
        public string permalink;
        public bool published;
        public string category;
        public List<string> categories;
        public List<string> tags;
        // Excerpt/excerpt_separator is generated by Jekyll itself, but it can be overrided in YFM.
        public string excerpt;
        public string excerpt_separator;
        // Predefined Variables for Posts
        public DateTime date;
        // User defined dynamic variables
        public IDictionary<string, Object> userDefinedMetadata;

        public PageMetadata()
        {

        }

        public PageMetadata(YamlMappingNode rootNode)
        {
            this.userDefinedMetadata = new Dictionary<string, object>();
            this.categories = new List<string>();
            this.tags = new List<string>();
            if (rootNode != null)
            {
                foreach (var entry in rootNode.Children)
                {
                    var node = entry.Key as YamlScalarNode;
                    if (node == null)
                    {
                        continue;
                    }

                    switch (node.Value)
                    {
                        case "layout" : 
                            layout = entry.Value.ToString();
                            break;
                        case "permalink":
                            permalink = entry.Value.ToString();
                            break;
                        case "published":
                            bool valueBool;
                            if (bool.TryParse(entry.Value.ToString(), out valueBool))
                            {
                                published = valueBool;
                            }
                            else
                            {
                                published = false;
                            }
                            break;
                        case "excerpt":
                            this.excerpt = entry.Value.ToString();
                            break;
                        case "category":
                            this.category = entry.Value.ToString();
                            break;
                        case "categories":
                            var categoryNode = entry.Value as YamlScalarNode;
                            if (categoryNode != null)
                            {
                                var trySplitCategory = Regex.Split(entry.Value.ToString(), @"\s+");
                                if (trySplitCategory.Length > 1)
                                {
                                    this.categories.AddRange(trySplitCategory);
                                }
                                else {
                                    this.category = entry.Value.ToString();
                                }
                                break;
                            }
                            var categoriesNode = entry.Value as YamlSequenceNode;
                            this.categories.AddRange(categoriesNode.SelectMany(o => Regex.Split(o.ToString(), @"\s+")));
                            break;
                        case "tags":
                            var tagsNode = (YamlSequenceNode)entry.Value;
                            this.tags.AddRange(tagsNode.Select(o => o.ToString()));
                            break;
                        default:
                            this.userDefinedMetadata.Add(node.Value, entry.Value);
                            break;
                    }

                }
            }
        }
    }
    public static class StringExtension
    {
        public static Regex yfm = new Regex(@"(?s:^---(.*?)---)");
        public static PageMetadata YamlFrontMatter(this string text)
        {
            var matches = yfm.Matches(text);
            if (matches.Count == 0)
            {
                return default(PageMetadata);
            }

            var yaml = new YamlStream();
            yaml.Load(new StringReader(matches[0].Groups[1].Value));
            return new PageMetadata( (YamlMappingNode)yaml.Documents[0].RootNode );
        }

        public static string MarkdownContent(this string text)
        {
            var matches = yfm.Matches(text);
            if (matches.Count == 0)
            {
                return text;
            }

            return text.Replace(matches[0].Groups[0].Value, String.Empty)
                .TrimStart(Environment.NewLine.ToCharArray())
                .TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}
