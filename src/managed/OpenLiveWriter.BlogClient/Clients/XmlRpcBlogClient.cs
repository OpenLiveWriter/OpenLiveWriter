// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.BlogClient.Clients
{

    public abstract class XmlRpcBlogClient : BlogClientBase, IBlogClient
    {
        public XmlRpcBlogClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            _postApiUrl = UrlHelper.SafeToAbsoluteUri(postApiUrl);

            // configure client options
            BlogClientOptions clientOptions = new BlogClientOptions();
            ConfigureClientOptions(clientOptions);
            _clientOptions = clientOptions;

        }

        public virtual bool IsSecure
        {
            get
            {
                return Options.SupportsHttps || (_postApiUrl ?? "").StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            }
        }

        public IBlogClientOptions Options
        {
            get
            {
                return _clientOptions;
            }
        }

        /// <summary>
        /// Enable external users of the class to completely replace
        /// the client options
        /// </summary>
        /// <param name="newClientOptions"></param>
        public void OverrideOptions(IBlogClientOptions newClientOptions)
        {
            _clientOptions = newClientOptions;
        }

        /// <summary>
        /// Enable subclasses to change the default client options
        /// </summary>
        /// <param name="clientOptions"></param>
        protected virtual void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
        }

        public abstract BlogInfo[] GetUsersBlogs();

        public BlogInfo[] GetImageEndpoints()
        {
            throw new NotImplementedException();
        }

        public abstract BlogPostCategory[] GetCategories(string blogId);
        public abstract BlogPostKeyword[] GetKeywords(string blogId);

        public abstract BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now);

        public string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = null;
            remotePost = null;
            return NewPost(blogId, post, newCategoryContext, publish);
        }
        public abstract string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish);

        public bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = null;
            remotePost = null;
            return EditPost(blogId, post, newCategoryContext, publish);
        }
        public abstract bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish);

        public abstract BlogPost GetPost(string blogId, string postId);

        public abstract void DeletePost(string blogId, string postId, bool publish);

        public virtual BlogPost GetPage(string blogId, string pageId)
        {
            throw new BlogClientMethodUnsupportedException("GetPage");
        }

        public virtual PageInfo[] GetPageList(string blogId)
        {
            throw new BlogClientMethodUnsupportedException("GetPageList");
        }

        public virtual BlogPost[] GetPages(string blogId, int maxPages)
        {
            throw new BlogClientMethodUnsupportedException("GetPages");
        }

        public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = null;
            remotePost = null;
            return NewPage(blogId, page, publish);
        }

        protected virtual string NewPage(string blogId, BlogPost page, bool publish)
        {
            throw new BlogClientMethodUnsupportedException("NewPage");
        }

        public bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            etag = null;
            remotePost = null;
            return EditPage(blogId, page, publish);
        }

        protected virtual bool EditPage(string blogId, BlogPost page, bool publish)
        {
            throw new BlogClientMethodUnsupportedException("EditPage");
        }

        public virtual void DeletePage(string blogId, string pageId)
        {
            throw new BlogClientMethodUnsupportedException("DeletePage");
        }

        public virtual AuthorInfo[] GetAuthors(string blogId)
        {
            throw new BlogClientMethodUnsupportedException("GetAuthors");
        }

        public abstract string DoBeforePublishUploadWork(IFileUploadContext uploadContext);

        public virtual void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        public virtual HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter)
        {
            return BlogClientHelper.SendAuthenticatedHttpRequest(requestUri, filter, CreateCredentialsFilter(requestUri));
        }

        protected virtual HttpRequestFilter CreateCredentialsFilter(string requestUri)
        {
            TransientCredentials tc = Login();
            if (tc != null)
                return HttpRequestCredentialsFilter.Create(tc.Username, tc.Password, requestUri, true);
            else
                return null;
        }

        public virtual bool? DoesFileNeedUpload(IFileUploadContext uploadContext)
        {
            return null;
        }

        /// <summary>
        /// Hook that allows subclasses to augment the HttpWebRequest used to make XML-RPC requests.
        /// </summary>
        /// <param name="request"></param>
        protected virtual void BeforeHttpRequest(HttpWebRequest request)
        {
            // WARNING: Derived classes do not currently make it a practice to call this method
            //          so don't count on the code executing if the method is overriden!
        }

        public virtual BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
        {
            throw new BlogClientMethodUnsupportedException("SuggestCategories");
        }

        public virtual string AddCategory(string blogId, BlogPostCategory category)
        {
            throw new BlogClientMethodUnsupportedException("AddCategory");
        }

        protected readonly string UserAgent = ApplicationEnvironment.UserAgent;

        public string Username
        {
            get { return Login().Username; }
        }

        public string Password
        {
            get { return Login().Password; }
        }

        /// <summary>
        /// Call a method and return the XML result. Will throw an exception of type BlogClientException
        /// if an error occurs.
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected XmlNode CallMethod(string methodName, params XmlRpcValue[] parameters)
        {
            string url = _postApiUrl;
            if (Options.SupportsHttps)
            {
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    url = "https://" + url.Substring("http://".Length);
            }

            try
            {
                // create an RpcClient
                XmlRpcClient xmlRpcClient = new XmlRpcClient(url, ApplicationEnvironment.UserAgent, new HttpRequestFilter(BeforeHttpRequest), _clientOptions.CharacterSet);

                // call the method
                XmlRpcMethodResponse response = xmlRpcClient.CallMethod(methodName, parameters);

                // if success, return the response
                if (!response.FaultOccurred)
                {
                    return response.Response;
                }
                else // throw error indicating problem
                {
                    // prepare to throw exception
                    BlogClientException exception;

                    // allow underlying provider to return a more descriptive exception type
                    exception = ExceptionForFault(response.FaultCode, response.FaultString);

                    if (exception == null) // if it couldn't just go generic
                        exception = new BlogClientProviderException(response.FaultCode, response.FaultString);

                    // throw the exception
                    throw exception;
                }
            }
            catch (IOException ex)
            {
                throw new BlogClientIOException("calling XML-RPC method " + methodName, ex);
            }
            catch (WebException ex)
            {
                HttpRequestHelper.LogException(ex);

                // see if this was a 404 not found
                switch (ex.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        HttpWebResponse response = ex.Response as HttpWebResponse;
                        if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            throw new BlogClientPostUrlNotFoundException(url, ex.Message);
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            throw new BlogClientAuthenticationException(response.StatusCode.ToString(), ex.Message, ex);
                        else
                            throw new BlogClientHttpErrorException(url, string.Format(CultureInfo.InvariantCulture, "{0} {1}", (int)response.StatusCode, response.StatusDescription), ex);
                    default:
                        throw new BlogClientConnectionErrorException(url, ex.Message);
                }
            }
            catch (XmlRpcClientInvalidResponseException ex)
            {
                throw new BlogClientInvalidServerResponseException(methodName, ex.Message, ex.Response);
            }
        }

        protected abstract BlogClientProviderException ExceptionForFault(string faultCode, string faultString);

        protected XmlRpcArray ArrayFromStrings(string[] strings)
        {
            ArrayList stringValues = new ArrayList();
            foreach (string str in strings)
                stringValues.Add(new XmlRpcString(str));
            return new XmlRpcArray((XmlRpcValue[])stringValues.ToArray(typeof(XmlRpcValue)));
        }

        protected static string NodeText(XmlNode node)
        {
            if (node != null)
                return node.InnerText.Trim();
            else
                return String.Empty;
        }

        /// <summary>
        /// Parse a date returned from a weblog. Returns the parsed date as a UTC DateTime value.
        /// If the date does not have a timezone designator then it will be presumed to be in the
        /// local time zone of this PC. This method is virtual so that for weblogs that produce
        /// undesignated date/time strings in some other timezone (like the timezone of the
        /// hosting providor) subclasses can do whatever offset is appropriate.
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        protected virtual DateTime ParseBlogDate(XmlNode xmlNode)
        {
            string date = NodeText(xmlNode);
            if (date != String.Empty)
            {
                try
                {
                    return ParseBlogDate(date);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception parsing date: " + date + " (" + ex.Message + ")");
                    return DateTime.MinValue;
                }
            }
            else
            {
                return DateTime.MinValue;
            }

        }

        /// <summary>
        /// Overridable hook to allow custom handling of setting the post title (in plain text form).
        /// This method is called when reading the post title out of an XML-RPC response. Most providers
        /// represent the post title value as HTML, so the default implementation here unescapes the encoded HTML
        /// back into plain text.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="blogPostTitle"></param>
        protected void SetPostTitleFromXmlValue(BlogPost post, string blogPostTitle)
        {
            bool isHtml = (Options.ReturnsHtmlTitlesOnGet != SupportsFeature.Unknown)
                            ? Options.ReturnsHtmlTitlesOnGet == SupportsFeature.Yes
                            : Options.RequiresHtmlTitles;

            if (!isHtml)
                post.Title = blogPostTitle;
            else
                post.Title = HtmlUtils.UnEscapeEntities(blogPostTitle, HtmlUtils.UnEscapeMode.Default);
        }

        /// <summary>
        /// Overridable hook to allow custom handling of reading the post title for insertion into an XML-RPC request.
        /// This method is called when reading the post title for insertion in an XML-RPC response. Most providers
        /// represent the post title value as HTML, so the default implementation here escapes plain text post Title
        /// into HTML format.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        protected string GetPostTitleForXmlValue(BlogPost post)
        {
            if (!this.Options.RequiresHtmlTitles)
                return post.Title;
            else
                return HtmlUtils.EscapeEntities(post.Title);
        }

        /// <summary>
        /// Convert a blog date to a UTC date/time
        /// </summary>
        /// <param name="date">w3c date-time</param>
        /// <returns></returns>
        private static DateTime ParseBlogDate(string date)
        {
            IFormatProvider culture = new CultureInfo("en-US", true);
            DateTime dateTime;
            try
            {
                dateTime = DateTime.ParseExact(date, DATE_FORMATS, culture, DateTimeStyles.AllowWhiteSpaces);
                return DateTimeHelper.LocalToUtc(dateTime);
            }
            catch (Exception)
            {
                //Now try the W3C UTC date formats
                //Since .NET doesn't realize the the 'Z' is an indicator of the GMT timezone,
                //ParseExact will return the date exactly as parsed (no shift for GMT)
                return DateTime.ParseExact(date, DATE_FORMATS_UTC, culture, DateTimeStyles.AllowWhiteSpaces);
            }
        }
        private static readonly string[] DATE_FORMATS = new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz", "yyyyMMdd'T'HH':'mm':'ss", "yyyy-MM-ddTHH:mm:ss" };
        private static readonly string[] DATE_FORMATS_UTC = new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", "yyyy'-'MM'-'dd'T'HH':'mm'Z'", "yyyyMMdd'T'HH':'mm':'ss'Z'" };

        private readonly string _postApiUrl;
        private IBlogClientOptions _clientOptions;

    }
}
