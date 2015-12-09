using System;
using System.Collections.Generic;
using System.Text;
using BlogRunner.Core.Config;
using OpenLiveWriter.Extensibility.BlogClient;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.IO;
using OpenLiveWriter.HtmlParser.Parser;
using System.Net.Cache;

namespace BlogRunner.Core
{
    public class BlogUtil
    {
        private Blog blog;
        private IBlogClient client;

        public BlogUtil(Blog blog, IBlogClient client)
        {
            this.blog = blog;
            this.client = client;
        }

        public static string ShortGuid
        {
            get
            {
                byte[] bytes = Guid.NewGuid().ToByteArray();
                long longVal = BitConverter.ToInt64(bytes, 0) ^ BitConverter.ToInt64(bytes, 8);
                return Convert.ToBase64String(BitConverter.GetBytes(longVal)).TrimEnd('=');
            }
        }

        public string NewPost(string title, string body, DateTime? dateTime)
        {
            BlogPost post = new BlogPost();
            post.Title = title;
            post.Contents = body;
            if (dateTime != null)
                post.DatePublishedOverride = dateTime.Value;

            string eTag;
            XmlDocument remotePost;
            return client.NewPost(blog.BlogId, post, null, true, out eTag, out remotePost);
        }

        public string DownloadHomepage()
        {
            /*
            using (WebClient client = new WebClient())
            {
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
                return client.DownloadString(blog.HomepageUrl);
            }
             */
            HttpWebRequest req = HttpWebRequest.Create(blog.HomepageUrl) as HttpWebRequest;
            using (StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        public Match MatchHomepageText(Regex regex)
        {
            string html = DownloadHomepage();
            HtmlExtractor ex = new HtmlExtractor(html);
            if (ex.Seek(new DelegatePredicate(e => e is Text && regex.IsMatch(e.ToString()))).Success)
                return regex.Match(ex.Element.ToString());
            return Match.Empty;
        }

        private class DelegatePredicate : IElementPredicate
        {
            public delegate bool MatchPredicate(Element e);

            private MatchPredicate predicate;

            public DelegatePredicate(MatchPredicate predicate)
            {
                this.predicate = predicate;
            }

            public bool IsMatch(Element e)
            {
                return predicate(e);
            }
        }
    }
}
