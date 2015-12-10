// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient
{

    public delegate object BlogClientOperation(Blog blog);

    public sealed class BlogClientHelper
    {
        public const string BlogHomepageUrlToken = "{blog-homepage-url}";
        public const string BlogPostApiUrlToken = "{blog-postapi-url}";
        public const string BlogIdToken = "{blog-id}";
        public const string PostIdToken = "{post-id}";

        public static string GetAbsoluteUrl(string url, string sourceUrl)
        {
            if (url == String.Empty)
                return String.Empty;
            else if (url.IndexOf(BlogClientHelper.BlogHomepageUrlToken, StringComparison.OrdinalIgnoreCase) != -1)
                return url;
            else if (url.IndexOf(BlogClientHelper.BlogPostApiUrlToken, StringComparison.OrdinalIgnoreCase) != -1)
                return url;
            else
                return UrlHelper.UrlCombineIfRelative(sourceUrl, url);
        }

        public static string FormatUrl(string url, string homepageUrl, string postApiUrl, string hostBlogId)
        {
            return FormatUrl(url, homepageUrl, postApiUrl, hostBlogId, null);
        }

        public static string FormatUrl(string url, string homepageUrl, string postApiUrl, string hostBlogId, string postId)
        {
            if (homepageUrl != null)
            {
                homepageUrl = UrlHelper.InsureTrailingSlash(homepageUrl);

                // If the url starts with a homepage url token, then
                // the rest of the URL is basically a relative url from
                // the homepage. In this case we need to do special
                // processing to make sure that any index document carried
                // on the homepage url is not substituted
                if (url.StartsWith(BlogHomepageUrlToken, StringComparison.OrdinalIgnoreCase) && (url.Length > BlogHomepageUrlToken.Length))
                {
                    string relativePart = url.Substring(BlogHomepageUrlToken.Length);
                    url = UrlHelper.UrlCombine(homepageUrl, relativePart);
                }
                else
                {
                    // note sure why anyone would use the homepage url token
                    // not at the start, but handle this case anyway
                    url = url.Replace(BlogHomepageUrlToken, homepageUrl);
                }
            }
            if (postApiUrl != null)
            {
                url = url.Replace(BlogPostApiUrlToken, postApiUrl);
            }
            if (hostBlogId != null)
            {
                url = url.Replace(BlogIdToken, hostBlogId);
            }
            if (postId != null)
            {
                url = url.Replace(PostIdToken, postId);
            }

            return url;
        }

        public static HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, HttpRequestFilter filter, HttpRequestFilter credentialsFilter)
        {
            // build filter list
            ArrayList filters = new ArrayList();
            if (filter != null)
                filters.Add(filter);
            if (credentialsFilter != null)
                filters.Add(credentialsFilter);

            // send request
            try
            {
                return HttpRequestHelper.SendRequest(
                    requestUri,
                    CompoundHttpRequestFilter.Create(filters.ToArray(typeof(HttpRequestFilter)) as HttpRequestFilter[]));
            }
            catch (BlogClientOperationCancelledException)
            {
                // if we are in silent mode and  failed to authenticate then try again w/o requiring auth
                if (BlogClientUIContext.SilentModeForCurrentThread)
                {
                    return HttpRequestHelper.SendRequest(requestUri, filter);
                }
                else
                {
                    throw;
                }
            }
        }

        public static WinInetCredentialsContext GetCredentialsContext(string blogId, string url)
        {
            using (BlogSettings blogSettings = BlogSettings.ForBlogId(blogId))
            {
                IBlogSettingsAccessor settings = blogSettings as IBlogSettingsAccessor;
                return GetCredentialsContext(BlogClientManager.CreateClient(settings), settings.Credentials, url);
            }
        }

        public static WinInetCredentialsContext GetCredentialsContext(IBlogClient blogClient, IBlogCredentialsAccessor credentials, string url)
        {
            // determine cookies and/or network credentials
            CookieString cookieString = null;
            NetworkCredential credential = null;

            if (credentials != null && credentials.Username != String.Empty)
            {
                credential = new NetworkCredential(credentials.Username, credentials.Password);
            }

            if (cookieString != null || credential != null)
                return new WinInetCredentialsContext(credential, cookieString);
            else
                return null;
        }

        public static object PerformBlogOperationWithTimeout(string blogId, BlogClientOperation operation, int timeoutMs)
        {
            return PerformBlogOperationWithTimeout(blogId, operation, timeoutMs, true);
        }

        /// <summary>
        /// Perform a blog operation with a timeout. If the operation could not be completed for any reason (including
        /// an invalid blog, no credentials, a network error, or a timeout) then null is returned.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="blogId"></param>
        /// <param name="operation"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public static object PerformBlogOperationWithTimeout(string blogId, BlogClientOperation operation, int timeoutMs, bool verifyCredentials)
        {
            try
            {
                // null for invalid blogs
                if (!BlogSettings.BlogIdIsValid(blogId))
                    return null;

                // null if the user can't authenticate
                if (verifyCredentials)
                {
                    using (Blog blog = new Blog(blogId))
                    {
                        if (!blog.VerifyCredentials())
                            return null;
                    }
                }

                // fire up the thread
                BlogClientOperationThread operationThread = new BlogClientOperationThread(blogId, operation);
                Thread thread = ThreadHelper.NewThread(new ThreadStart(operationThread.ThreadMain), "BlogClientOperationThread", true, false, true);
                thread.Start();

                // wait for it to complete
                thread.Join(timeoutMs);

                // return the result
                return operationThread.Result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception occurred performing BlogOperation: " + ex.ToString());
                return null;
            }
        }

        private class BlogClientOperationThread
        {
            public BlogClientOperationThread(string blogId, BlogClientOperation operation)
            {
                _blogId = blogId;
                _operation = operation;
            }

            public void ThreadMain()
            {
                try
                {
                    using (Blog blog = new Blog(_blogId))
                    {
                        _result = _operation(blog);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception occurred performing BlogOperation: " + ex.ToString());
                }
            }

            public object Result
            {
                get { return _result; }
            }
            private volatile object _result = null;

            private readonly string _blogId;
            private readonly BlogClientOperation _operation;
        }
    }
}
