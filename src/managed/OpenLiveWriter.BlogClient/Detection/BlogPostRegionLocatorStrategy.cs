// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using mshtml;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Detection
{
    /// <summary>
    /// Represents the set of blog post-related elements in an HTML document.
    /// </summary>
    internal class BlogPostRegions
    {
        public IHTMLDocument Document;
        public IHTMLElement[] TitleRegions;
        public IHTMLElement BodyRegion;
    }

    /// <summary>
    /// Implements a strategy for scanning a blog URL and identifying the blog post regions within the page.
    /// </summary>
    internal abstract class BlogPostRegionLocatorStrategy
    {
        protected IBlogClient _blogClient;
        protected BlogAccount _blogAccount;
        protected IBlogCredentialsAccessor _credentials;
        protected string _blogHomepageUrl;
        protected PageDownloader _pageDownloader;

        public BlogPostRegionLocatorStrategy(IBlogClient blogClient, BlogAccount blogAccount, IBlogCredentialsAccessor credentials, string blogHomepageUrl, PageDownloader pageDownloader)
        {
            _blogClient = blogClient;
            _blogAccount = blogAccount;
            _credentials = credentials;
            _blogHomepageUrl = blogHomepageUrl;
            _pageDownloader = pageDownloader;
        }

        public abstract void PrepareRegions(IProgressHost progress);
        public virtual void FetchTemporaryPostPage(IProgressHost progress, string url) { }
        public abstract BlogPostRegions LocateRegionsOnUIThread(IProgressHost progress, string pageUrl);
        public abstract void CleanupRegions(IProgressHost progress);

        public virtual bool CanRefetchPage => false;

        protected void CheckCancelRequested(IProgressHost progress)
        {
            if (progress.CancelRequested)
                throw new OperationCancelledException();
        }
    }

    public delegate bool BlogPostRegionLocatorBooleanCallback();

    /// <summary>
    /// Region detection strategy that uses a temporary post with clearly identifiable content to
    /// locate each region of the blog post.
    /// </summary>
    internal class TemporaryPostRegionLocatorStrategy : BlogPostRegionLocatorStrategy
    {
        BlogPost temporaryPost;
        Stream blogPageContents;
        BlogPostRegionLocatorBooleanCallback containsBlogPosts;

        public override bool CanRefetchPage => true;

        private const string TEMPORARY_POST_STABLE_GUID = "3bfe001a-32de-4114-a6b4-4005b770f6d7";
        private string TEMPORARY_POST_BODY_GUID = Guid.NewGuid().ToString();
        private string TEMPORARY_POST_TITLE_GUID = Guid.NewGuid().ToString();
        private string TEMPORARY_POST_BODY
        {
            get { return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.TemporaryPostBody), TEMPORARY_POST_BODY_GUID, TEMPORARY_POST_STABLE_GUID); }
        }
        private string TEMPORARY_POST_TITLE
        {
            get { return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.TemporaryPostTitle), TEMPORARY_POST_TITLE_GUID, TEMPORARY_POST_STABLE_GUID); }
        }

        public TemporaryPostRegionLocatorStrategy(IBlogClient blogClient, BlogAccount blogAccount,
            IBlogCredentialsAccessor credentials, string blogHomepageUrl, PageDownloader pageDownloader, BlogPostRegionLocatorBooleanCallback promptForTempPost)
            : base(blogClient, blogAccount, credentials, blogHomepageUrl, pageDownloader)
        {
            this.containsBlogPosts = promptForTempPost;
        }

        public override void PrepareRegions(IProgressHost progress)
        {
            TempPostWarningHelper helper = new TempPostWarningHelper(BlogClientUIContext.ContextForCurrentThread);

            BlogClientUIContext.ContextForCurrentThread.Invoke(new ThreadStart(helper.ShowWarningDialog), null);

            if (helper.DialogResult == DialogResult.Yes)
            {
                PrepareRegionsUsingTemporaryPost(progress);
            }
            else
            {
                throw new OperationCancelledException();
            }
        }

        private void PrepareRegionsUsingTemporaryPost(IProgressHost progress)
        {
            // Publish a temporary post so that we can examine HTML that will surround posts created with the editor
            temporaryPost = PostTemplate(new ProgressTick(progress, 25, 100));
            CheckCancelRequested(progress);
            FetchTemporaryPostPage(progress, _blogHomepageUrl);
        }

        /// <summary>
        /// Fetch a blog page from the URL specified and transfer it into blogPageContents
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="url"></param>
        public override void FetchTemporaryPostPage(IProgressHost progress, string url)
        {
            blogPageContents = new MemoryStream();

            // Download the webpage that is contains the temporary blog post
            // WARNING, DownloadBlogPage uses an MSHTML Document on a non-UI thread...which is a no-no!
            //   its been this way through several betas without problem, so we'll keep it that way for now, but
            //   it needs to be fixed eventually.
            Stream postHtmlContents = DownloadBlogPage(url, progress);
            CheckCancelRequested(progress);

            using (postHtmlContents)
            {
                StreamHelper.Transfer(postHtmlContents, blogPageContents);
            }
            progress.UpdateProgress(100, 100);
        }

        public override BlogPostRegions LocateRegionsOnUIThread(IProgressHost progress, string pageUrl)
        {
            blogPageContents.Seek(0, SeekOrigin.Begin);
            return ParseBlogPostIntoTemplate(blogPageContents, pageUrl, progress);
        }

        public override void CleanupRegions(IProgressHost progress)
        {
            if (temporaryPost != null)
            {
                // Delete the temporary post.
                DeletePost(temporaryPost, progress);
            }
        }

        internal class TempPostWarningHelper
        {
            private readonly IWin32Window _owner;
            private DialogResult _dialogResult;

            public TempPostWarningHelper(IWin32Window owner)
            {
                _owner = owner;
            }

            public DialogResult DialogResult { get { return _dialogResult; } }

            public void ShowWarningDialog()
            {
                _dialogResult = DisplayMessage.Show(MessageId.TempPostPermission, _owner, ApplicationEnvironment.ProductName_Short);
            }
        }

        private BlogPost PostTemplate(IProgressHost progress)
        {
            progress.UpdateProgress(50, 100);
            // make a test post that we can use to do analysis
            BlogPost testPost = new BlogPost();
            testPost.IsTemporary = true;
            testPost.Title = TEMPORARY_POST_TITLE;

            //plant a <p> around the contents so that the blog provider doesn't try to add their own
            //and so we'll know it can be safely removed later
            //Note: this fixes an issue where the editor's postBody <div> was being parented improperly by
            //a <p> element that the blog service was adding...
            string postContents = String.Format(CultureInfo.InvariantCulture, "<p>{0}</p>", TEMPORARY_POST_BODY);
            testPost.Contents = postContents;

            string etag;
            XmlDocument remotePost;
            testPost.Id = _blogClient.NewPost(_blogAccount.BlogId, testPost, new IgnoreNewCategoryContext(), true, out etag, out remotePost);
            testPost.ETag = etag;
            testPost.AtomRemotePost = remotePost;
            progress.UpdateProgress(100, 100);
            return testPost;
        }

        private class IgnoreNewCategoryContext : INewCategoryContext
        {
            public void NewCategoryAdded(BlogPostCategory category)
            {
            }
        }

        /// <summary>
        /// Downloads a webpage from a blog and searches for TEMPORARY_POST_TITLE_GUID.
        /// </summary>
        /// <param name="blogPageUrl"></param>
        /// <param name="progress"></param>
        /// <returns>Stream containing document which contains TEMPORARY_POST_TITLE_GUID.</returns>
        private Stream DownloadBlogPage(string blogPageUrl, IProgressHost progress)
        {
            ProgressTick tick = new ProgressTick(progress, 50, 100);
            MemoryStream memStream = new MemoryStream();
            IHTMLDocument2 doc2 = null;
            // WinLive 221984: Theme detection timing out intermittently on WordPress.com
            // The temp post *often* takes more than a minute to show up on the blog home page.
            // The download progress dialog has a cancel button, we'll try a lot before giving up.
            for (int i = 0; i < 30 && doc2 == null; i++)
            {
                if (progress.CancelRequested)
                    throw new OperationCancelledException();
                tick.UpdateProgress(0, 0, Res.Get(StringId.ProgressDownloadingWeblogEditingStyle));
                // Sleep to give the post enough time to show up.
                // We'll make 10 attempts with a 1 second delay.
                // Subsequent attempts will use a 10 second delay.
                // This means we'll try for 5 minutes (10s + 290s = 300s) before we consider the operation timed out.
                Thread.Sleep(i < 10 ? 1000 : 10000);

                // Add random parameter to URL to bypass cache
                var urlRandom = UrlHelper.AppendQueryParameters(blogPageUrl, new string[] { Guid.NewGuid().ToString() });

                HttpWebResponse resp = _pageDownloader(urlRandom, 60000);
                memStream = new MemoryStream();
                using (Stream respStream = resp.GetResponseStream())
                    StreamHelper.Transfer(respStream, memStream);

                //read in the HTML file and determine if it contains the title element
                memStream.Seek(0, SeekOrigin.Begin);
                doc2 = HTMLDocumentHelper.GetHTMLDocumentFromStream(memStream, urlRandom);
                if (HTMLDocumentHelper.FindElementContainingText(doc2, TEMPORARY_POST_TITLE_GUID) == null)
                    doc2 = null;
            }
            if (doc2 == null)
            {
                throw new OperationTimedOutException();
            }
            tick.UpdateProgress(100, 100);

            //return the stream
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;

        }

        private void DeletePost(BlogPost post, IProgressHost progress)
        {
            try
            {
                progress.UpdateProgress(Res.Get(StringId.ProgressFinalizingEditingTemplateConfig));
                _blogClient.DeletePost(_blogAccount.BlogId, post.Id, true);
                progress.UpdateProgress(100, 100);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception occurred while deleting temporary post: " + ex.ToString());
                //show a message that user needs to delete their post
                DisplayMessage.Show(MessageId.TempPostDeleteFailed);
            }
        }

        private BlogPostRegions ParseBlogPostIntoTemplate(Stream stream, string postSourceUrl, IProgressHost progress)
        {
            progress.UpdateProgress(Res.Get(StringId.ProgressCreatingEditingTemplate));

            //parse the document to create the blog template
            IHTMLDocument2 doc2 = HTMLDocumentHelper.GetHTMLDocumentFromStream(stream, postSourceUrl);
            IHTMLDocument3 doc = (IHTMLDocument3)doc2;
            IHTMLElement[] titleElements = HTMLDocumentHelper.FindElementsContainingText(doc2, TEMPORARY_POST_TITLE_GUID);

            IHTMLElement bodyElement = HTMLDocumentHelper.FindElementContainingText(doc2, TEMPORARY_POST_BODY_GUID);
            if (bodyElement != null && bodyElement.tagName == "P")
            {
                //the body element is the <p> we planted, so replace it with a DIV since that will be the safest
                //element to have a as parent to all post content.
                IHTMLElement div = doc2.createElement("div");
                (bodyElement.parentElement as IHTMLDOMNode).replaceChild(div as IHTMLDOMNode, bodyElement as IHTMLDOMNode);
                bodyElement = div;
            }

            //locate the title element.  Note that is there are more than 1 copies of the title text detected, we use the one
            //that is anchored closest to the left or the body element.
            if (titleElements.Length > 0)
            {
                BlogPostRegions regions = new BlogPostRegions();
                regions.Document = (IHTMLDocument)doc;
                regions.TitleRegions = titleElements;
                regions.BodyRegion = bodyElement;

                progress.UpdateProgress(100, 100);
                return regions;
            }
            else
            {
                throw new Exception("unable to access test post.");
            }
        }
    }

    /// <summary>
    /// Region detection strategy that matches the content associated with the most recent blog post.
    /// </summary>
    internal class RecentPostRegionLocatorStrategy : BlogPostRegionLocatorStrategy
    {
        private string _titleText;
        private string _bodyText;
        private MemoryStream blogPageContents;
        BlogPost mostRecentPost;
        private int recentPostCount = -1;
        public RecentPostRegionLocatorStrategy(IBlogClient blogClient, BlogAccount blogAccount,
            IBlogCredentialsAccessor credentials, string blogHomepageUrl, PageDownloader pageDownloader)
            : base(blogClient, blogAccount, credentials, blogHomepageUrl, pageDownloader)
        {
        }

        public override void PrepareRegions(IProgressHost progress)
        {
            BlogPost[] posts = _blogClient.GetRecentPosts(_blogAccount.BlogId, 1, false, DateTime.UtcNow);
            if (posts == null || posts.Length == 0)
            {
                recentPostCount = 0;
                throw new Exception("No recent posts available");
            }
            else
                recentPostCount = posts.Length;

            mostRecentPost = posts[0];
            _titleText = HTMLDocumentHelper.HTMLToPlainText(mostRecentPost.Title);
            _bodyText = HTMLDocumentHelper.HTMLToPlainText(mostRecentPost.MainContents);

            string normalizedTitleText = NormalizeText(_titleText);
            string normalizedBodyText = NormalizeText(_bodyText);

            //verify the normalized content is unique enough to distinctly identify the post regions
            if (normalizedTitleText.Length < 4)
                throw new ArgumentException("Title text is not unique enough to use for style detection");
            if (normalizedBodyText.Length < 8 || normalizedBodyText.IndexOf(' ') == -1)
                throw new ArgumentException("Content text is not unique enough to use for style detection");
            if (normalizedBodyText.IndexOf(normalizedTitleText, StringComparison.CurrentCulture) != -1) //title text is a subset of the body text
                throw new ArgumentException("Title text is not unique enough to use for style detection");
            if (normalizedTitleText.IndexOf(normalizedBodyText, StringComparison.CurrentCulture) != -1) //body text is a subset of the title text
                throw new ArgumentException("Content text is not unique enough to use for style detection");

            blogPageContents = DownloadBlogPage(_blogHomepageUrl, progress);
        }

        public override BlogPostRegions LocateRegionsOnUIThread(IProgressHost progress, string pageUrl)
        {
            blogPageContents.Seek(0, SeekOrigin.Begin);
            IHTMLDocument2 doc2 = HTMLDocumentHelper.GetHTMLDocumentFromStream(blogPageContents, pageUrl);

            // Ensure that the document is fully loaded.
            // If it is not fully loaded, then viewing its current style is non-deterministic.
            DateTime startedDoingEvents = DateTime.Now;
            while (!progress.CancelRequested && !HTMLDocumentHelper.IsReady(doc2))
            {
                if (DateTime.Now.Subtract(startedDoingEvents).TotalMilliseconds > 10000)
                {
                    // Timing out here is not fatal.
                    Trace.WriteLine("Timed out while loading blog homepage for theme detection.");
                    break;
                }

                Application.DoEvents();
            }

            //The Google/Blogger dynamic templates load the pages dynmaically usig Ajax, so we dont have any template to use.
            if (IsUsingDynamicTemplate(doc2))
                throw new BlogClientAbortGettingTemplateException();

            IHTMLElement[] titles = FindText(_titleText, doc2.body);
            IHTMLElement[] bodies = FindText(_bodyText, doc2.body);
            if (titles.Length == 0 || bodies.Length == 0)
                throw new Exception("Unable to locate blog post elements using most recent post");

            if (IsSmartContent(bodies[0]))
                throw new Exception("Most recent post is smart content");

            BlogPostRegions regions = new BlogPostRegions();
            regions.TitleRegions = titles;

            //scrub the post body element to avoid improperly including extraneous parent elements
            regions.BodyRegion = ScrubPostBodyRegionParentElements(bodies[0]);
            regions.Document = doc2;

            progress.UpdateProgress(100, 100);

            return regions;
        }

        public bool HasBlogPosts()
        {
            return recentPostCount > 0;
        }

        private static bool IsSmartContent(IHTMLElement element)
        {
            // search up the parent hierarchy
            while (element != null)
            {
                if (0 == String.Compare(element.tagName, "div", StringComparison.OrdinalIgnoreCase))
                {
                    string className = element.getAttribute("className", 0) as string;
                    if (className == "wlWriterSmartContent" || className == "wlWriterEditableSmartContent")
                        return true;
                }

                // search parent
                element = element.parentElement;
            }
            // not in smart content
            return false;
        }

        private static bool IsUsingDynamicTemplate(IHTMLDocument2 doc2)
        {
            IHTMLElement head = GetHeadElement(doc2);

            if(head != null)
            {
                var template = GetMetaTagContent(head, "blogger-template");

                return template == "dynamic";
            }

            return false;
        }

        private static IHTMLElement GetHeadElement(IHTMLDocument2 doc2)
        {
            foreach(IHTMLElement element in doc2.all)
            {
                if(element.tagName.ToLower() == "head")
                {
                    return element;
                }
            }
            return null;
        }

        private static string GetMetaTagContent(IHTMLElement head, string name)
        {
            foreach(IHTMLElement element in (IHTMLElementCollection)head.children)
            {
                if(element.tagName.ToLower() == "meta" && (string)element.getAttribute("name") == name)
                {
                    return (string)element.getAttribute("content");
                }
            }
            return null;
        }

        private IHTMLElement ScrubPostBodyRegionParentElements(IHTMLElement postBodyElement)
        {
            //Note: This method prevents the case where the post content consists of a single line of text, so surrounding elements
            //like that are part of the post content (like <p> and <b>) are improperly included in the template.

            //since the text-based find strategy returns the common parent element that contains the text,
            //it is possible for the element to be an inline element (like <b> or <i>) that entirely surrounds
            //the content.  It would be improper to include these inline elements in the template, so we delete all inline
            //elements above the post body.
            try
            {
                IHTMLDocument2 postContentDoc = HTMLDocumentHelper.StringToHTMLDoc(mostRecentPost.Contents, this._blogHomepageUrl);
                IHTMLElement[] postContentsElements = FindText(HTMLDocumentHelper.HTMLToPlainText(postContentDoc.body.innerHTML), postContentDoc.body);
                if (postContentsElements.Length > 0)
                {
                    IHTMLElement postContentsElement = postContentsElements[0];
                    if (postContentsElement is IHTMLBodyElement)
                    {
                        //there are no extraneous surrounding tags to clean up, so just return the current body element
                        return postBodyElement;
                    }

                    //postBodyElement and postContentsElement should now be the equivalent element in each document.
                    //Now delete the parent elements of postContentsElement that appear in the postBodyElement DOM since
                    //these will clearly be elements associated with the post, and not the template.
                    ArrayList markedForDelete = new ArrayList();
                    while (!(postContentsElement is IHTMLBodyElement) && !(postBodyElement is IHTMLBodyElement))
                    {
                        if (postContentsElement.tagName == postBodyElement.tagName)
                        {
                            markedForDelete.Add(postBodyElement);
                        }
                        postContentsElement = postContentsElement.parentElement;
                        postBodyElement = postBodyElement.parentElement;
                    }

                    if (markedForDelete.Count > 0)
                    {
                        //delete all of the marked elements except the last one
                        for (int i = markedForDelete.Count - 2; i >= 0; i--)
                        {
                            IHTMLDOMNode domNode = (IHTMLDOMNode)markedForDelete[i];
                            domNode.removeNode(false);
                        }

                        //delete the last node and return its parent as the new post body region element
                        IHTMLElement lastMarkForDeleteElement = (IHTMLElement)markedForDelete[markedForDelete.Count - 1];
                        IHTMLElement newPostBodyElement = lastMarkForDeleteElement.parentElement;
                        (lastMarkForDeleteElement as IHTMLDOMNode).removeNode(false);
                        return newPostBodyElement;
                    }
                }
            }
            catch (Exception e)
            {
                //This is an error we should look at, but it should not abort template detection.
                Debug.Fail("Cleanup logic failed with an error", e.ToString());
            }
            return postBodyElement;
        }

        public override void CleanupRegions(IProgressHost progress)
        {
            if (blogPageContents != null)
            {
                blogPageContents.Close();
                blogPageContents = null;
            }

            progress.UpdateProgress(100, 100);
        }

        /// <summary>
        /// Downloads a webpage from a blog.
        /// </summary>
        /// <param name="blogHomepageUrl"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private MemoryStream DownloadBlogPage(string blogHomepageUrl, IProgressHost progress)
        {
            ProgressTick tick = new ProgressTick(progress, 50, 100);
            if (progress.CancelRequested)
                throw new OperationCancelledException();
            tick.UpdateProgress(0, 0, Res.Get(StringId.ProgressDownloadingWeblogEditingStyle));

            HttpWebResponse resp = _pageDownloader(blogHomepageUrl, 60000);
            MemoryStream memStream = new MemoryStream();
            using (Stream respStream = resp.GetResponseStream())
                StreamHelper.Transfer(respStream, memStream);

            //read in the HTML file and determine if it contains the title element
            memStream.Seek(0, SeekOrigin.Begin);

            tick.UpdateProgress(100, 100);

            //return the stream
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        public static IHTMLElement[] FindText(string text, IHTMLElement fromElement)
        {
            try
            {
                string normalizedText = NormalizeText(text);

                ArrayList elementList = new ArrayList();
                AddElementsContainingText(normalizedText, fromElement, elementList);
                IHTMLElement[] elements = HTMLElementHelper.ToElementArray(elementList);
                return elements;
            }
            catch (ArgumentException e)
            {
                Trace.WriteLine(e.Message);
                return new IHTMLElement[0];
            }
            catch (Exception e)
            {
                Trace.Fail("exception during findText", e.ToString());
                return new IHTMLElement[0];
            }
        }

        private static void AddElementsContainingText(string normalizedText, IHTMLElement fromElement, ArrayList elementList)
        {
            int elementCount = 0;
            IHTMLDOMNode fromNode = fromElement as IHTMLDOMNode;
            foreach (IHTMLDOMNode childNode in (IHTMLDOMChildrenCollection)fromNode.childNodes)
            {
                IHTMLElement child = childNode as IHTMLElement;
                if (child != null)
                {
                    if (ContainsNormalizedText(normalizedText, child))
                    {
                        elementCount++;
                        AddElementsContainingText(normalizedText, child, elementList);
                    }
                }
            }
            if (elementCount == 0)
            {
                if (ContainsNormalizedText(normalizedText, fromElement))
                    elementList.Add(fromElement);
            }
        }

        private static bool ContainsNormalizedText(string normalizedText, IHTMLElement e)
        {
            string elementText = e.innerText;
            if (elementText != null)
            {
                // The normalizedText has run through HTMLDocumentHelper.HTMLToPlainText and NormalizeText.
                // We need to do the same to e.innerHTML to ensure we're comparing apples to apples.
                elementText = NormalizeText(HTMLDocumentHelper.HTMLToPlainText(e.innerHTML));

                int index = elementText.IndexOf(normalizedText, StringComparison.CurrentCulture);
                return index > -1;
            }
            else
                return false;
        }

        private static string NormalizeText(string text)
        {
            StringBuilder sb = new StringBuilder();
            bool whitespaceMode = false;
            foreach (char ch in text)
            {
                if (Char.IsWhiteSpace(ch))
                {
                    if (!whitespaceMode)
                    {
                        sb.Append(" ");
                        whitespaceMode = true;
                    }
                }
                else if (Char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    whitespaceMode = false;
                }
            }
            string normalizedText = sb.ToString().Trim();
            return normalizedText;
        }
    }
}
