// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using BlogRunner.Core.Config;
using System.Xml;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;

namespace BlogRunner.Core
{
    public abstract class Test
    {
        protected const string YES = "Yes";
        protected const string NO = "No";

        const string NAMESPACE_BLOGRUNNER = "http://writer.live.com/blogrunner/2007";

        public abstract void DoTest(Blog blog, IBlogClient blogClient, ITestResults results);

        public virtual void DoTest(Blog blog, IBlogClient blogClient, XmlElement providerEl)
        {
            TestResultImpl results = new TestResultImpl();
            DoTest(blog, blogClient, results);
            results.ForEach(delegate (string key, string value)
            {
                XmlElement optionsEl = (XmlElement)providerEl.SelectSingleNode("options");
                if (optionsEl == null)
                {
                    optionsEl = providerEl.OwnerDocument.CreateElement("options");
                    providerEl.AppendChild(optionsEl);
                }

                XmlElement el = (XmlElement)optionsEl.SelectSingleNode(key);
                if (el == null)
                {
                    el = providerEl.OwnerDocument.CreateElement(key);
                    optionsEl.AppendChild(el);
                }

                if (!el.HasAttribute("readonly", NAMESPACE_BLOGRUNNER))
                {
                    el.InnerText = value;
                }
            });
        }

        static Test()
        {
            CleanUpPosts = true;
        }

        /// <summary>
        /// Retries action until either it returns true or the timeout time elapses.
        /// </summary>
        protected static void RetryUntilTimeout(TimeSpan timeout, TimeoutAction action)
        {
            DateTime due = DateTime.UtcNow + timeout;
            do
            {
                if (action())
                    return;
            }
            while (DateTime.UtcNow < due);
            throw new TimeoutException("The operation has timed out");
        }
        protected delegate bool TimeoutAction();

        protected static bool CleanUpPosts;
    }

    public abstract class PostTest : Test
    {
        protected internal abstract void PreparePost(BlogPost blogPost, ref bool? publish);
        protected internal abstract void HandleResult(string homepageHtml, ITestResults results);

        /// <summary>
        /// Return true if the timeout condition was handled. False means
        /// the caller should deal with the timeout (by throwing TimeoutException).
        /// </summary>
        protected internal virtual bool HandleTimeout(TimeoutException te, ITestResults results)
        {
            return false;
        }

        protected virtual TimeSpan TimeoutDuration
        {
            get { return TimeSpan.FromMinutes(2.0); }
        }

        public sealed override void DoTest(Blog blog, IBlogClient blogClient, ITestResults results)
        {
            BlogPost blogPost = new BlogPost();
            bool? publish = null;
            PreparePost(blogPost, ref publish);

            string token = BlogUtil.ShortGuid;
            blogPost.Title = token + ":" + blogPost.Title;

            string etag;
            XmlDocument remotePost;
            string postId = blogClient.NewPost(blog.BlogId, blogPost, null, publish ?? true, out etag, out remotePost);

            try
            {
                RetryUntilTimeout(TimeoutDuration, delegate
                {
                    using (HttpWebResponse response = HttpRequestHelper.SendRequest(blog.HomepageUrl))
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            string html = Encoding.ASCII.GetString(StreamHelper.AsBytes(stream));

                            if (html.Contains(token))
                            {
                                HandleResult(html, results);
                                return true;
                            }

                            Thread.Sleep(1000);
                            return false;
                        }
                    }
                });
            }
            catch (TimeoutException te)
            {
                if (!HandleTimeout(te, results))
                    throw;
            }

            if (postId != null && CleanUpPosts)
                blogClient.DeletePost(blog.BlogId, postId, false);
        }
    }

    public abstract class BodyContentPostTest : PostTest
    {
        public abstract string BodyContentString { get; }
        public abstract void HandleContentResult(string result, ITestResults results);

        private string guid1, guid2;

        protected internal sealed override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            guid1 = BlogUtil.ShortGuid;
            guid2 = BlogUtil.ShortGuid;

            blogPost.Contents += "\r\n<br />\r\n" + guid1 + BodyContentString + guid2;
        }

        protected internal sealed override void HandleResult(string homepageHtml, ITestResults results)
        {
            Regex regex = new Regex(Regex.Escape(guid1) + "(.*?)" + Regex.Escape(guid2));
            Match m = regex.Match(homepageHtml);
            string result;
            if (!m.Success)
                result = null;
            else
                result = m.Groups[1].Value;
            HandleContentResult(result, results);
        }
    }

    public abstract class RoundtripTest : Test
    {
        protected internal abstract void PreparePost(Blog blog, IBlogClient blogClient, BlogPost blogPost, ref bool? publish);
        protected internal abstract void HandleResult(BlogPost blogPost, ITestResults results);

        public sealed override void DoTest(Blog blog, IBlogClient blogClient, ITestResults results)
        {
            BlogPost blogPost = new BlogPost();
            bool? publish = null;
            PreparePost(blog, blogClient, blogPost, ref publish);

            string token = BlogUtil.ShortGuid;
            blogPost.Title = token + ":" + blogPost.Title;

            string etag;
            XmlDocument remotePost;
            string postId = blogClient.NewPost(blog.BlogId, blogPost, null, publish ?? true, out etag, out remotePost);
            BlogPost newPost = blogClient.GetPost(blog.BlogId, postId);
            HandleResult(newPost, results);
            if (postId != null && CleanUpPosts)
                blogClient.DeletePost(blog.BlogId, postId, false);
        }
    }
}
