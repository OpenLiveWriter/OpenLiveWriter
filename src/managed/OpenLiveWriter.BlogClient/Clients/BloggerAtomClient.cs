// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.HtmlParser.Parser.FormAgent;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("BloggerAtom", "Atom")]
    public class BloggerAtomClient : AtomClient
    {
        public BloggerAtomClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(AtomProtocolVersion.V10DraftBlogger, postApiUrl, credentials)
        {
        }

        public override bool IsSecure
        {
            get { return true; }
        }

        protected override TransientCredentials Login()
        {
            TransientCredentials tc = base.Login();
            VerifyAndRefreshCredentials(tc);
            return tc;
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
            VerifyAndRefreshCredentials(tc);
        }

        private void VerifyAndRefreshCredentials(TransientCredentials tc)
        {
            GDataCredentials gc = GDataCredentials.FromCredentials(tc);

            if (gc.IsValid(tc.Username, tc.Password, GDataCredentials.BLOGGER_SERVICE_NAME))
                return;
            else
            {
                bool showUI = !BlogClientUIContext.SilentModeForCurrentThread;
                gc.EnsureLoggedIn(tc.Username, tc.Password, GDataCredentials.BLOGGER_SERVICE_NAME, showUI);
                return;
            }
        }

        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsNewCategoriesInline = true;
            clientOptions.SupportsFileUpload = true;
            clientOptions.SupportsExtendedEntries = true;
        }

        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            string albumName = ApplicationEnvironment.ProductName;

            string path = uploadContext.GetContentsLocalFilePath();

            if (Options.FileUploadNameFormat != null && Options.FileUploadNameFormat.Length > 0)
            {
                string formattedFileName = uploadContext.FormatFileName(uploadContext.PreferredFileName);
                string[] chunks = StringHelper.Reverse(formattedFileName).Split(new char[] { '/' }, 2);
                if (chunks.Length == 2)
                    albumName = StringHelper.Reverse(chunks[1]);
            }

            string EDIT_MEDIA_LINK = "EditMediaLink";
            string srcUrl;
            string editUri = uploadContext.Settings.GetString(EDIT_MEDIA_LINK, null);
            if (editUri == null || editUri.Length == 0)
            {
                PostNewImage(albumName, path, out srcUrl, out editUri);
            }
            else
            {
                try
                {
                    UpdateImage(editUri, path, out srcUrl, out editUri);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    if (e is WebException)
                        HttpRequestHelper.LogException((WebException)e);

                    bool success = false;
                    srcUrl = null; // compiler complains without this line
                    try
                    {
                        // couldn't update existing image? try posting a new one
                        PostNewImage(albumName, path, out srcUrl, out editUri);
                        success = true;
                    }
                    catch
                    {
                    }
                    if (!success)
                        throw;  // rethrow the exception from the update, not the post
                }
            }
            uploadContext.Settings.SetString(EDIT_MEDIA_LINK, editUri);

            PicasaRefererBlockingWorkaround(uploadContext.BlogId, uploadContext.Role, ref srcUrl);

            return srcUrl;
        }

        /// <summary>
        /// "It looks like the problem with the inline image is due to referrer checking.
        /// The thumbnail image being used is protected for display only on certain domains.
        /// These domains include *.blogspot.com and *.google.com.  This user is using a
        /// feature in Blogger which allows him to display his blog directly on his own
        /// domain, which will not pass the referrer checking.
        ///
        /// "The maximum size of a thumbnail image that can be displayed on non-*.blogspot.com
        /// domains is 800px. (blogs don't actually appear at *.google.com).  However, if you
        /// request a 800px thumbnail, and the image is less than 800px for the maximum
        /// dimension, then the original image will be returned without the referrer
        /// restrictions.  That sounds like it will work for you, so feel free to give it a
        /// shot and let me know if you have any further questions or problems."
        ///   -- Anonymous Google Employee
        /// </summary>
        private void PicasaRefererBlockingWorkaround(string blogId, FileUploadRole role, ref string srcUrl)
        {
            if (role == FileUploadRole.LinkedImage && Options.UsePicasaS1600h)
            {
                try
                {
                    int lastSlash = srcUrl.LastIndexOf('/');
                    string srcUrl2 = srcUrl.Substring(0, lastSlash)
                                     + "/s1600-h"
                                     + srcUrl.Substring(lastSlash);
                    HttpWebRequest req = HttpRequestHelper.CreateHttpWebRequest(srcUrl2, true);
                    req.Method = "HEAD";
                    req.GetResponse().Close();
                    srcUrl = srcUrl2;
                    return;
                }
                catch (WebException we)
                {
                    Debug.Fail("Picasa s1600-h hack failed: " + we.ToString());
                }
            }

            try
            {
                if (!Options.UsePicasaImgMaxAlways)
                {
                    // This class doesn't have access to the homePageUrl, so this is a workaround to
                    // to get the homePageUrl while minimizing the amount of code we have to change (we're at MShip/ZBB)
                    foreach (string id in BlogSettings.GetBlogIds())
                    {
                        using (BlogSettings settings = BlogSettings.ForBlogId(id))
                        {
                            if (settings.ClientType == "BloggerAtom" && settings.HostBlogId == blogId)
                            {
                                switch (UrlHelper.GetDomain(settings.HomepageUrl).ToUpperInvariant())
                                {
                                    case "BLOGSPOT.COM":
                                    case "GOOGLE.COM":
                                        return;
                                }
                            }
                        }
                    }
                }
                srcUrl += ((srcUrl.IndexOf('?') >= 0) ? "&" : "?") + "imgmax=800";
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error while doing Picasa upload: " + ex.ToString());
            }
        }

        public override void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        public override BlogPostCategory[] GetCategories(string blogId)
        {
            // get metafeed
            Login();

            Uri metafeedUri = new Uri("http://www.blogger.com/feeds/default/blogs");
            XmlDocument xmlDoc = xmlRestRequestHelper.Get(ref metafeedUri, RequestFilter);

            XmlElement entryEl = xmlDoc.SelectSingleNode(@"atom:feed/atom:entry[atom:link[@rel='http://schemas.google.com/g/2005#post' and @href='" + blogId + "']]", _nsMgr) as XmlElement;
            Res.LOCME("Blogger error message");
            if (entryEl == null)
                throw new BlogClientInvalidServerResponseException("metafeed",
                    "The expected blog information was not returned by Blogger.",
                    null);

            ArrayList categoryList = new ArrayList();
            foreach (XmlNode categoryNode in entryEl.SelectNodes("atom:category[@scheme='http://www.blogger.com/atom/ns#']", _nsMgr))
            {
                string categoryName = ((XmlElement)categoryNode).GetAttribute("term");
                categoryList.Add(new BlogPostCategory(categoryName));
            }
            return (BlogPostCategory[])categoryList.ToArray(typeof(BlogPostCategory));
        }

        protected override string CategoryScheme
        {
            get
            {
                return "http://www.blogger.com/atom/ns#";
            }
        }

        protected override XmlDocument GetCategoryXml(ref string blogId)
        {
            throw new NotImplementedException();
        }

        // Used to guard against recursion when attempting delete post recovery.
        [ThreadStatic]
        private static bool inDeletePostRecovery;

        protected override bool AttemptDeletePostRecover(Exception e, string blogId, string postId, bool publish)
        {
            // There's a bug with Blogger Beta where their atom feeds are returning
            // edit URIs for entries that don't work for PUT and DELETE.  However, if you do a GET on the
            // edit URI, you can get a different edit URI that DOES work for PUT and DELETE.

            if (inDeletePostRecovery)
                return false;

            inDeletePostRecovery = true;
            try
            {
                if (IsBadRequestError(e))
                {
                    BlogPost post = GetPost(blogId, postId);
                    if (post.Id != postId)
                    {
                        DeletePost(blogId, post.Id, publish);
                        return true;
                    }
                }
            }
            catch (Exception e1)
            {
                Trace.Fail(e1.ToString());
            }
            finally
            {
                inDeletePostRecovery = false;
            }

            // it didn't work.
            return false;
        }

        // Used to guard against recursion when attempting edit post recovery.
        [ThreadStatic]
        private static bool inEditPostRecovery;

        protected override bool AttemptEditPostRecover(Exception e, string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost)
        {
            // There's a bug with Blogger Beta where their atom feeds are returning
            // edit URIs for entries that don't work for PUT and DELETE.  However, if you do a GET on the
            // edit URI, you can get a different edit URI that DOES work for PUT and DELETE.

            if (inEditPostRecovery)
            {
                etag = null;
                remotePost = null;
                return false;
            }

            inEditPostRecovery = true;
            try
            {
                if (IsBadRequestError(e))
                {
                    BlogPost oldPost = GetPost(blogId, post.Id);
                    if (post.Id != oldPost.Id)
                    {
                        // Temporarily change the ID to this alternate Edit URI.  In order to
                        // minimize the chance of unintended side effects, we revert the ID
                        // back to the original value once we're done trying to edit.

                        string originalId = post.Id;
                        try
                        {
                            post.Id = oldPost.Id;
                            if (EditPost(blogId, post, newCategoryContext, publish, out etag, out remotePost))
                                return true;
                        }
                        finally
                        {
                            post.Id = originalId;
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                Trace.Fail(e1.ToString());
            }
            finally
            {
                inEditPostRecovery = false;
            }
            etag = null;
            remotePost = null;
            return false;
        }

        public override BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            // By default, New Blogger returns blog posts in last-updated order. We
            // want them in last-published order, so they match up with how it looks
            // on the blog.

            string url = blogId;
            url += (url.IndexOf('?') < 0 ? '?' : '&') + "orderby=published";

            return GetRecentPostsInternal(url, maxPosts, includeCategories, now);
        }

        protected override bool AttemptAlternateGetRecentPostUrl(Exception e, ref string uri)
        {
            string alternateUri = uri;

            if (e is WebException)
            {
                HttpWebResponse response = ((WebException)e).Response as HttpWebResponse;
                if (response != null)
                {
                    /* We have two separate problems to deal with here.
                     *
                     * For New Blogger blogs, passing orderby=published to www.blogger.com
                     * will currently result in a 400 (Bad Request). We need to do the same
                     * request to www2.blogger.com.
                     *
                     * For Old Blogger blogs, passing orderby=published is going to fail no
                     * matter what. However, we don't know in advance whether this blog is
                     * Old Blogger or New Blogger. So we assume we are New Blogger, retry
                     * the request with www2.blogger.com as above, and if we are Old Blogger
                     * then that request will fail with a 401. When that happens we can try
                     * again on www.blogger.com without orderby=published.
                     */

                    if (response.StatusCode == HttpStatusCode.BadRequest
                        && uri.StartsWith("http://www.blogger.com/", StringComparison.OrdinalIgnoreCase)
                        && uri.IndexOf("orderby=published", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // www.blogger.com still can't handle orderby=published. Switch to
                        // www2.blogger.com and try the request again.

                        alternateUri = Regex.Replace(
                            uri,
                            "^" + Regex.Escape("http://www.blogger.com/"),
                            "http://www2.blogger.com/",
                            RegexOptions.IgnoreCase);
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized
                        && uri.StartsWith("http://www2.blogger.com/", StringComparison.OrdinalIgnoreCase)
                        && uri.IndexOf("orderby=published", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // This is Old Blogger after all. Switch to www.blogger.com and remove the
                        // orderby=published param.

                        // Need to be very careful when removing orderby=published. Blogger freaks
                        // out with a 400 when any little thing is wrong with the query string.
                        // Examples of URLs that cause errors:
                        // http://www2.blogger.com/feeds/7150790056788550577/posts/default?
                        // http://www2.blogger.com/feeds/7150790056788550577/posts/default?&start-index=26
                        // http://www2.blogger.com/feeds/7150790056788550577/posts/default?start-index=26&
                        // http://www2.blogger.com/feeds/7150790056788550577/posts/default?&
                        // http://www2.blogger.com/feeds/7150790056788550577/posts/default&

                        Regex r1 = new Regex("^" + Regex.Escape("http://www2.blogger.com/"), RegexOptions.IgnoreCase);
                        Regex r2 = new Regex(@"(\?|&)orderby=published\b(&?)");

                        if (r1.IsMatch(uri) && r2.IsMatch(uri))
                        {
                            alternateUri = r1.Replace(uri, "http://www.blogger.com/");
                            alternateUri = r2.Replace(alternateUri, new MatchEvaluator(OrderByClauseRemover));
                        }
                    }
                }
            }

            if (alternateUri != uri)
            {
                uri = alternateUri;
                return true;
            }
            return false;
        }

        private string OrderByClauseRemover(Match match)
        {
            string prefix = match.Groups[1].Value;
            bool hasSuffix = match.Groups[2].Success;

            return hasSuffix ? prefix : "";
        }

        private static bool IsBadRequestError(Exception e)
        {
            WebException we = e as WebException;
            if (we == null)
                return false;
            HttpWebResponse resp = we.Response as HttpWebResponse;
            return resp != null && resp.StatusCode == HttpStatusCode.BadRequest;
        }

        protected override HttpRequestFilter RequestFilter
        {
            get
            {
                return new HttpRequestFilter(BloggerAuthorizationFilter);
            }
        }

        private void BloggerAuthorizationFilter(HttpWebRequest request)
        {
            AuthorizeRequest(request, GDataCredentials.BLOGGER_SERVICE_NAME);
        }

        #region Picasa image uploading

        private void PicasaAuthorizationFilter(HttpWebRequest request)
        {
            AuthorizeRequest(request, GDataCredentials.PICASAWEB_SERVICE_NAME);
        }

        public string GetBlogImagesAlbum(string albumName)
        {
            const string FEED_REL = "http://schemas.google.com/g/2005#feed";
            const string GPHOTO_NS_URI = "http://schemas.google.com/photos/2007";

            //TransientCredentials transientCredentials = Credentials.TransientCredentials as TransientCredentials;
            Uri picasaUri = new Uri("http://picasaweb.google.com/data/feed/api/user/default");

            try
            {
                Uri reqUri = picasaUri;
                XmlDocument albumListDoc = xmlRestRequestHelper.Get(ref reqUri, new HttpRequestFilter(PicasaAuthorizationFilter), "kind", "album");
                foreach (XmlElement entryEl in albumListDoc.SelectNodes(@"/atom:feed/atom:entry", _nsMgr))
                {
                    XmlElement titleNode = entryEl.SelectSingleNode(@"atom:title", _nsMgr) as XmlElement;
                    if (titleNode != null)
                    {
                        string titleText = _atomVer.TextNodeToPlaintext(titleNode);
                        if (titleText == albumName)
                        {
                            XmlNamespaceManager nsMgr2 = new XmlNamespaceManager(new NameTable());
                            nsMgr2.AddNamespace("gphoto", "http://schemas.google.com/photos/2007");
                            XmlNode numPhotosRemainingNode = entryEl.SelectSingleNode("gphoto:numphotosremaining/text()", nsMgr2);
                            if (numPhotosRemainingNode != null)
                            {
                                int numPhotosRemaining;
                                if (int.TryParse(numPhotosRemainingNode.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out numPhotosRemaining))
                                {
                                    if (numPhotosRemaining < 1)
                                        continue;
                                }
                            }
                            string selfHref = AtomEntry.GetLink(entryEl, _nsMgr, FEED_REL, "application/atom+xml", null, reqUri);
                            if (selfHref.Length > 1)
                                return selfHref;
                        }
                    }
                }
            }
            catch (WebException we)
            {
                HttpWebResponse httpWebResponse = we.Response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    HttpRequestHelper.DumpResponse(httpWebResponse);
                    if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        BlogClientUIContext.ContextForCurrentThread.Invoke(new EventHandler(ShowPicasaSignupPrompt), null);
                        throw new BlogClientOperationCancelledException();
                    }
                }
                throw;
            }

            XmlDocument newDoc = new XmlDocument();
            XmlElement newEntryEl = newDoc.CreateElement("atom", "entry", _atomVer.NamespaceUri);
            newDoc.AppendChild(newEntryEl);

            XmlElement newTitleEl = newDoc.CreateElement("atom", "title", _atomVer.NamespaceUri);
            newTitleEl.SetAttribute("type", "text");
            newTitleEl.InnerText = albumName;
            newEntryEl.AppendChild(newTitleEl);

            XmlElement newSummaryEl = newDoc.CreateElement("atom", "summary", _atomVer.NamespaceUri);
            newSummaryEl.SetAttribute("type", "text");
            newSummaryEl.InnerText = Res.Get(StringId.BloggerImageAlbumDescription);
            newEntryEl.AppendChild(newSummaryEl);

            XmlElement newAccessEl = newDoc.CreateElement("gphoto", "access", GPHOTO_NS_URI);
            newAccessEl.InnerText = "private";
            newEntryEl.AppendChild(newAccessEl);

            XmlElement newCategoryEl = newDoc.CreateElement("atom", "category", _atomVer.NamespaceUri);
            newCategoryEl.SetAttribute("scheme", "http://schemas.google.com/g/2005#kind");
            newCategoryEl.SetAttribute("term", "http://schemas.google.com/photos/2007#album");
            newEntryEl.AppendChild(newCategoryEl);

            Uri postUri = picasaUri;
            XmlDocument newAlbumResult = xmlRestRequestHelper.Post(ref postUri, new HttpRequestFilter(PicasaAuthorizationFilter), "application/atom+xml", newDoc, null);
            XmlElement newAlbumResultEntryEl = newAlbumResult.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement;
            Debug.Assert(newAlbumResultEntryEl != null);
            return AtomEntry.GetLink(newAlbumResultEntryEl, _nsMgr, FEED_REL, "application/atom+xml", null, postUri);
        }

        private void ShowPicasaSignupPrompt(object sender, EventArgs e)
        {
            if (DisplayMessage.Show(MessageId.PicasawebSignup) == DialogResult.Yes)
            {
                ShellHelper.LaunchUrl("http://picasaweb.google.com");
            }
        }

        private void PostNewImage(string albumName, string filename, out string srcUrl, out string editUri)
        {
            TransientCredentials transientCredentials = Credentials.TransientCredentials as TransientCredentials;
            GDataCredentials.FromCredentials(transientCredentials).EnsureLoggedIn(transientCredentials.Username, transientCredentials.Password, GDataCredentials.PICASAWEB_SERVICE_NAME, false);

            string albumUrl = GetBlogImagesAlbum(albumName);
            HttpWebResponse response = RedirectHelper.GetResponse(albumUrl, new RedirectHelper.RequestFactory(new UploadFileRequestFactory(this, filename, "POST").Create));
            using (Stream s = response.GetResponseStream())
                ParseMediaEntry(s, out srcUrl, out editUri);
        }

        private void UpdateImage(string editUri, string filename, out string srcUrl, out string newEditUri)
        {
            for (int retry = 5; retry > 0; retry--)
            {
                HttpWebResponse response;
                bool conflict = false;
                try
                {
                    response = RedirectHelper.GetResponse(editUri, new RedirectHelper.RequestFactory(new UploadFileRequestFactory(this, filename, "PUT").Create));
                }
                catch (WebException we)
                {
                    if (retry > 1
                        && we.Response as HttpWebResponse != null
                        && ((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Conflict)
                    {
                        response = (HttpWebResponse)we.Response;
                        conflict = true;
                    }
                    else
                        throw;
                }
                using (Stream s = response.GetResponseStream())
                    ParseMediaEntry(s, out srcUrl, out newEditUri);
                if (!conflict)
                    return; // success!
                editUri = newEditUri;
            }

            Trace.Fail("Should never get here");
            throw new ApplicationException("Should never get here");
        }

        private void ParseMediaEntry(Stream s, out string srcUrl, out string editUri)
        {
            srcUrl = null;

            // First try <content src>
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(s);
            XmlElement contentEl = xmlDoc.SelectSingleNode("/atom:entry/atom:content", _nsMgr) as XmlElement;
            if (contentEl != null)
                srcUrl = XmlHelper.GetUrl(contentEl, "@src", _nsMgr, null);

            // Then try media RSS
            if (srcUrl == null || srcUrl.Length == 0)
            {
                contentEl = xmlDoc.SelectSingleNode("/atom:entry/media:group/media:content[@medium='image']", _nsMgr) as XmlElement;
                if (contentEl == null)
                    throw new ArgumentException("Picasa photo entry was missing content element");
                srcUrl = XmlHelper.GetUrl(contentEl, "@url", _nsMgr, null);
            }

            editUri = AtomEntry.GetLink(xmlDoc.SelectSingleNode("/atom:entry", _nsMgr) as XmlElement, _nsMgr, "edit-media", null, null, null);
        }

        private class UploadFileRequestFactory
        {
            private readonly BloggerAtomClient _parent;
            private readonly string _filename;
            private readonly string _method;

            public UploadFileRequestFactory(BloggerAtomClient parent, string filename, string method)
            {
                _parent = parent;
                _filename = filename;
                _method = method;
            }

            public HttpWebRequest Create(string uri)
            {
                // TODO: choose rational timeout values
                HttpWebRequest request = HttpRequestHelper.CreateHttpWebRequest(uri, false);

                _parent.PicasaAuthorizationFilter(request);

                request.ContentType = MimeHelper.GetContentType(Path.GetExtension(_filename));
                try
                {
                    request.Headers.Add("Slug", Path.GetFileNameWithoutExtension(_filename));
                }
                catch (ArgumentException)
                {
                    request.Headers.Add("Slug", "Image");
                }

                request.Method = _method;

                using (Stream s = request.GetRequestStream())
                {
                    using (Stream inS = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        StreamHelper.Transfer(inS, s);
                    }
                }

                return request;
            }
        }

        #endregion

        private bool AuthorizeRequest(HttpWebRequest request, string serviceName)
        {
            // This line is required to avoid Error 500 from non-beta Blogger blogs.
            // According to Pete Hopkins it is "something with .NET".
            request.Accept = "*/*";

            TransientCredentials transientCredentials = Login();
            GDataCredentials cred = GDataCredentials.FromCredentials(transientCredentials);
            return cred.AttachCredentialsIfValid(
                request,
                transientCredentials.Username,
                transientCredentials.Password,
                serviceName);
        }

        public override BlogInfo[] GetUsersBlogs()
        {
            Uri metafeed = new Uri("http://www.blogger.com/feeds/default/blogs");
            XmlDocument xmlDoc = xmlRestRequestHelper.Get(ref metafeed, RequestFilter);

            ArrayList blogInfos = new ArrayList();
            foreach (XmlElement entryEl in xmlDoc.SelectNodes(@"atom:feed/atom:entry", _nsMgr))
            {
                string feedUrl = string.Empty;
                string title = string.Empty;
                string homepageUrl = string.Empty;

                XmlElement feedUrlEl = entryEl.SelectSingleNode(@"atom:link[@rel='http://schemas.google.com/g/2005#post' and @type='application/atom+xml']", _nsMgr) as XmlElement;
                if (feedUrlEl != null)
                    feedUrl = XmlHelper.GetUrl(feedUrlEl, "@href", metafeed);
                XmlElement titleEl = entryEl.SelectSingleNode(@"atom:title", _nsMgr) as XmlElement;
                if (titleEl != null)
                    title = _atomVer.TextNodeToPlaintext(titleEl);
                XmlElement linkEl = entryEl.SelectSingleNode(@"atom:link[@rel='alternate' and @type='text/html']", _nsMgr) as XmlElement;
                if (linkEl != null)
                    homepageUrl = linkEl.GetAttribute("href");

                blogInfos.Add(new BlogInfo(feedUrl, title, homepageUrl));
            }

            return (BlogInfo[])blogInfos.ToArray(typeof(BlogInfo));
        }

        public override BlogPost Parse(XmlElement entryNode, bool includeCategories, Uri documentUri)
        {
            BlogPost post = new BlogPost();
            AtomEntry atomEntry = new AtomEntry(_atomVer, _atomNS, CategoryScheme, _nsMgr, documentUri, entryNode);

            post.Title = atomEntry.Title;
            post.Excerpt = atomEntry.Excerpt;
            post.Id = PostUriToPostId(atomEntry.EditUri);
            post.Permalink = atomEntry.Permalink;

            string content = atomEntry.ContentHtml;
            if (content.Trim() != string.Empty)
            {
                HtmlExtractor ex = new HtmlExtractor(content);
                int start, length;
                if (Options.SupportsExtendedEntries && ex.Seek("<a name=\"more\">").Success)
                {
                    start = ex.Element.Offset;
                    length = ex.Element.Length;
                    if (ex.Seek("</a>").Success)
                    {
                        post.SetContents(content.Substring(0, start), content.Substring(ex.Element.Offset + ex.Element.Length));
                    }
                    else
                    {
                        post.SetContents(content.Substring(0, start), content.Substring(start + length));
                    }
                }
                else
                {
                    post.Contents = content;
                }
            }

            post.DatePublished = atomEntry.PublishDate;
            if (Options.SupportsCategories && includeCategories)
                post.Categories = atomEntry.Categories;

            return post;
        }
    }

    public class GDataCredentials
    {
        public const string CLIENT_LOGIN_URL = "https://www.google.com/accounts/ClientLogin";
        public const string YOUTUBE_CLIENT_LOGIN_URL = "https://www.google.com/youtube/accounts/ClientLogin";
        public const string BLOGGER_SERVICE_NAME = "blogger";
        public const string PICASAWEB_SERVICE_NAME = "lh2";
        public const string YOUTUBE_SERVICE_NAME = "youtube";

        private readonly Hashtable _auths = new Hashtable();

        internal GDataCredentials()
        {
        }

        public static GDataCredentials FromCredentials(TransientCredentials credentials)
        {
            GDataCredentials cred = credentials.Token as GDataCredentials;
            if (cred == null)
            {
                credentials.Token = cred = new GDataCredentials();
            }
            return cred;
        }

        internal class AuthKey
        {
            private readonly string _username;
            private readonly string _password;
            private readonly string _service;

            public AuthKey(string username, string password, string service)
            {
                this._username = username;
                this._password = password;
                this._service = service;
            }

            public override int GetHashCode()
            {
                int result = _username.GetHashCode();
                result = 29 * result + _password.GetHashCode();
                result = 29 * result + _service.GetHashCode();
                return result;
            }

            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                AuthKey authKey = obj as AuthKey;
                if (authKey == null) return false;
                if (!Equals(_username, authKey._username)) return false;
                if (!Equals(_password, authKey._password)) return false;
                if (!Equals(_service, authKey._service)) return false;
                return true;
            }
        }

        private class AuthValue
        {
            private readonly string _authString;
            private readonly DateTime _dateCreatedUtc;
            public readonly string ReturnedUsername; // YouTube only - null for all others

            public AuthValue(string authString, string returnedUsername)
            {
                _authString = authString;
                _dateCreatedUtc = DateTimeHelper.UtcNow;
                ReturnedUsername = returnedUsername;
            }

            public string AuthString
            {
                get { return _authString; }
            }

            public DateTime DateCreatedUtc
            {
                get { return _dateCreatedUtc; }
            }
        }

        public bool IsValid(string username, string password, string service)
        {
            AuthValue authValue = _auths[new AuthKey(username, password, service)] as AuthValue;
            if (authValue == null)
                return false;

            TimeSpan age = DateTimeHelper.UtcNow - authValue.DateCreatedUtc;
            if (age < TimeSpan.Zero || age > TimeSpan.FromHours(23))
                return false;

            return true;
        }

        public void EnsureLoggedIn(string username, string password, string service, bool showUi)
        {
            EnsureLoggedIn(username, password, service, showUi, CLIENT_LOGIN_URL);
        }

        public void EnsureLoggedIn(string username, string password, string service, bool showUi, string uri)
        {
            try
            {
                if (IsValid(username, password, service))
                    return;

                string captchaToken = null;
                string captchaValue = null;

                string source = string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", ApplicationEnvironment.CompanyName, ApplicationEnvironment.ProductName, ApplicationEnvironment.ProductVersion);

                while (true)
                {
                    GoogleLoginRequestFactory glrf = new GoogleLoginRequestFactory(username,
                        password,
                        service,
                        source,
                        captchaToken,
                        captchaValue);
                    if (captchaToken != null && captchaValue != null)
                    {
                        captchaToken = null;
                        captchaValue = null;
                    }

                    HttpWebResponse response;
                    try
                    {
                        response = RedirectHelper.GetResponse(uri, new RedirectHelper.RequestFactory(glrf.Create));
                    }
                    catch (WebException we)
                    {
                        response = (HttpWebResponse)we.Response;
                        if (response == null)
                        {
                            Trace.Fail(we.ToString());
                            if (showUi)
                            {
                                showUi = false;
                                ShowError(MessageId.WeblogConnectionError, we.Message);
                            }
                            throw;
                        }
                    }

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Hashtable ht = ParseAuthResponse(response.GetResponseStream());
                        if (ht.ContainsKey("Auth"))
                        {
                            _auths[new AuthKey(username, password, service)] = new AuthValue((string)ht["Auth"], ht["YouTubeUser"] as string);
                            return;
                        }
                        else
                        {
                            if (showUi)
                            {
                                showUi = false;
                                ShowError(MessageId.GoogleAuthTokenNotFound);
                            }
                            throw new BlogClientInvalidServerResponseException(uri, "No Auth token was present in the response.", string.Empty);
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // login failed

                        Hashtable ht = ParseAuthResponse(response.GetResponseStream());
                        string error = ht["Error"] as string;
                        if (error != null && error == "CaptchaRequired")
                        {
                            captchaToken = (string)ht["CaptchaToken"];
                            string captchaUrl = (string)ht["CaptchaUrl"];

                            GDataCaptchaHelper helper = new GDataCaptchaHelper(
                                new Win32WindowImpl(BlogClientUIContext.ContextForCurrentThread.Handle),
                                captchaUrl);

                            BlogClientUIContext.ContextForCurrentThread.Invoke(new ThreadStart(helper.ShowCaptcha), null);

                            if (helper.DialogResult == DialogResult.OK)
                            {
                                captchaValue = helper.Reply;
                                continue;
                            }
                            else
                            {
                                throw new BlogClientOperationCancelledException();
                            }
                        }

                        if (showUi)
                        {
                            if (error == "NoLinkedYouTubeAccount")
                            {
                                if (DisplayMessage.Show(MessageId.YouTubeSignup, username) == DialogResult.Yes)
                                {
                                    ShellHelper.LaunchUrl(GLink.Instance.YouTubeRegister);
                                }
                                return;
                            }

                            showUi = false;

                            if (error == "BadAuthentication")
                            {
                                ShowError(MessageId.LoginFailed, ApplicationEnvironment.ProductNameQualified);
                            }
                            else
                            {
                                ShowError(MessageId.BloggerError, TranslateError(error));
                            }

                        }
                        throw new BlogClientAuthenticationException(error, TranslateError(error));
                    }
                    else
                    {
                        if (showUi)
                        {
                            showUi = false;
                            ShowError(MessageId.BloggerError, response.StatusCode + ": " + response.StatusDescription);
                        }
                        throw new BlogClientAuthenticationException(response.StatusCode + "", response.StatusDescription);
                    }
                }
            }
            catch (BlogClientOperationCancelledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                if (showUi)
                {
                    ShowError(MessageId.UnexpectedErrorLogin, e.Message);
                }
                throw;
            }
        }

        private class GoogleLoginRequestFactory
        {
            private readonly string _username;
            private readonly string _password;
            private readonly string _service;
            private readonly string _source;
            private readonly string _captchaToken;
            private readonly string _captchaValue;

            public GoogleLoginRequestFactory(string username, string password, string service, string source, string captchaToken, string captchaValue)
            {
                _username = username;
                _password = password;
                _service = service;
                _source = source;
                _captchaToken = captchaToken;
                _captchaValue = captchaValue;
            }

            public HttpWebRequest Create(string uri)
            {
                FormData formData = new FormData(false,
                    "Email", _username,
                    "Passwd", _password,
                    "service", _service,
                    "source", _source);

                if (_captchaToken != null && _captchaValue != null)
                {
                    formData.Add("logintoken", _captchaToken);
                    formData.Add("logincaptcha", _captchaValue);
                }

                HttpWebRequest request = HttpRequestHelper.CreateHttpWebRequest(uri, true);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                using (Stream inStream = formData.ToStream())
                using (Stream outStream = request.GetRequestStream())
                    StreamHelper.Transfer(inStream, outStream);

                return request;
            }

        }

        private void ShowError(MessageId messageId, params object[] args)
        {
            ShowErrorHelper helper = new ShowErrorHelper(BlogClientUIContext.ContextForCurrentThread, messageId, args);
            if (BlogClientUIContext.ContextForCurrentThread != null)
                BlogClientUIContext.ContextForCurrentThread.Invoke(new ThreadStart(helper.Show), null);
            else
                helper.Show();
        }

        private class ShowErrorHelper
        {
            private readonly IWin32Window _owner;
            private readonly MessageId _messageId;
            private readonly object[] _args;

            public ShowErrorHelper(IWin32Window owner, MessageId messageId, object[] args)
            {
                _owner = owner;
                _messageId = messageId;
                _args = args;
            }

            public void Show()
            {
                DisplayMessage.Show(_messageId, _owner, _args);
            }
        }

        private string TranslateError(string error)
        {
            switch (error)
            {
                case "BadAuthentication":
                    return Res.Get(StringId.BloggerBadAuthentication);
                case "NotVerified":
                    return Res.Get(StringId.BloggerNotVerified);
                case "TermsNotAgreed":
                    return Res.Get(StringId.BloggerTermsNotAgreed);
                case "Unknown":
                    return Res.Get(StringId.BloggerUnknown);
                case "AccountDeleted":
                    return Res.Get(StringId.BloggerAccountDeleted);
                case "AccountDisabled":
                    return Res.Get(StringId.BloggerAccountDisabled);
                case "ServiceUnavailable":
                    return Res.Get(StringId.BloggerServiceUnavailable);
                case "NoLinkedYouTubeAccount":
                    return Res.Get(StringId.YouTubeNoAccount);
                default:
                    return string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.BloggerGenericError), error);
            }
        }

        private static Hashtable ParseAuthResponse(Stream stream)
        {
            Hashtable ht = CollectionsUtil.CreateCaseInsensitiveHashtable();
            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while (null != (line = sr.ReadLine()))
                {
                    string[] chunks = line.Split(new char[] { '=' }, 2);
                    if (chunks.Length == 2)
                        ht[chunks[0]] = chunks[1];
                }
            }
            return ht;
        }

        public bool AttachCredentialsIfValid(HttpWebRequest request, string username, string password, string service)
        {
            string auth = GetCredentialsIfValid(username, password, service);
            if (auth != null)
            {
                request.Headers.Set("Authorization", auth);
                return true;
            }
            else
                return false;
        }

        public string GetCredentialsIfValid(string username, string password, string service)
        {
            if (IsValid(username, password, service))
            {
                AuthValue authValue = _auths[new AuthKey(username, password, service)] as AuthValue;
                if (authValue != null)
                {
                    string auth = authValue.AuthString;
                    if (auth != null)
                    {
                        return "GoogleLogin auth=" + auth;
                    }
                }
            }
            return null;
        }

        public string GetUserName(string username, string password, string service)
        {
            AuthValue authValue = _auths[new AuthKey(username, password, service)] as AuthValue;
            if (authValue != null && !string.IsNullOrEmpty(authValue.ReturnedUsername))
            {
                return authValue.ReturnedUsername;
            }
            return username;

        }
    }
}
