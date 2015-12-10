// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor
{
    public interface IPostEditorPostSource
    {
        string UniqueId { get; }
        string Name { get; }
        string EmptyPostListMessage { get; }

        bool VerifyCredentials();

        bool IsSlow { get; }
        bool HasMultipleWeblogs { get; }

        bool SupportsDelete { get; }
        bool SupportsPages { get; }

        RecentPostCapabilities RecentPostCapabilities { get; }
        PostInfo[] GetRecentPosts(RecentPostRequest request);
        PostInfo[] GetPages(RecentPostRequest request);

        IBlogPostEditingContext GetPost(string postId);
        bool DeletePost(string postId, bool isPage);
    }

    public class PostInfo
    {
        public static string UntitledPost { get { return Res.Get(StringId.UntitledPost); } }
        public static string UntitledPage { get { return Res.Get(StringId.UntitledPage); } }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _id = String.Empty;

        public bool IsPage
        {
            get { return _isPage; }
            set { _isPage = value; }
        }
        private bool _isPage;

        public string Title
        {
            get
            {
                if (_title == String.Empty)
                    return IsPage ? UntitledPage : UntitledPost;
                else
                    return _title;
            }
            set { _title = value; }
        }
        private string _title = String.Empty;

        public string Permalink
        {
            get { return _permalink; }
            set { _permalink = value; }
        }
        private string _permalink = String.Empty;

        public string BlogName
        {
            get { return _blogName; }
            set { _blogName = value; }
        }
        private string _blogName = String.Empty;

        public string BlogId
        {
            get { return _blogId; }
            set { _blogId = value; }
        }
        private string _blogId = String.Empty;

        public string BlogPostId
        {
            get { return _blogPostId; }
            set { _blogPostId = value; }
        }
        private string _blogPostId = String.Empty;

        public string Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }
        private string _contents = String.Empty;

        public string PlainTextContents
        {
            get
            {
                if (_plainTextContents == null)
                    _plainTextContents = HtmlUtils.HTMLToPlainText(Contents);
                return _plainTextContents;
            }
        }
        private string _plainTextContents = null;

        public DateTime DateModified
        {
            get { return _dateModified; }
            set { _dateModified = value; }
        }
        private DateTime _dateModified = DateTime.MinValue;

        public bool DateModifiedSpecified
        {
            get { return DateModified != DateTime.MinValue; }
        }

        public string PrettyDateDisplay
        {
            get
            {
                if (DateModified == DateTime.MinValue)
                    return null;

                // convert to local
                DateTime date = DateTimeHelper.UtcToLocal(DateModified);

                string format = CultureHelper.GetDateTimeCombinedPattern("{0}", "{1}");

                string time = date.ToShortTimeString();
                // default format
                string formattedDate = CultureHelper.GetDateTimeCombinedPattern(date.ToShortDateString(), time);

                // see if we can shorten it
                DateTime dateNow = DateTime.Now;

                if (date.Year == dateNow.Year)
                {
                    if (date.DayOfYear == dateNow.DayOfYear)
                    {
                        formattedDate = String.Format(CultureInfo.CurrentCulture, format, Res.Get(StringId.Today), time);
                    }
                    else
                    {
                        int dayDiff = dateNow.DayOfYear - date.DayOfYear;
                        if (dayDiff > 0 && dayDiff <= 6)
                        {
                            formattedDate = String.Format(CultureInfo.CurrentCulture, format, date.ToString("dddd", CultureInfo.CurrentCulture), time);
                        }
                    }
                }

                return formattedDate;
            }
        }

        public class TitleComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (x as PostInfo).Title.CompareTo((y as PostInfo).Title);
            }
        }

        public class DateDescendingComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                DateAscendingComparer dateAscendingComparer = new DateAscendingComparer();
                return -dateAscendingComparer.Compare(x, y);
            }
        }

        public class DateAscendingComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (x as PostInfo).DateModified.CompareTo((y as PostInfo).DateModified);
            }
        }
    }

    public class RemoteWeblogBlogPostSource : IPostEditorPostSource
    {
        public RemoteWeblogBlogPostSource(string blogId)
        {
            _blogId = blogId;
            using (Blog blog = new Blog(blogId))
            {
                _name = blog.Name;
                _supportsPages = blog.ClientOptions.SupportsPages;
            }
        }

        public string UniqueId
        {
            get
            {
                return _blogId;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string EmptyPostListMessage
        {
            get
            {
                return null;
            }
        }

        public bool IsSlow
        {
            get
            {
                return true;
            }
        }

        public bool HasMultipleWeblogs
        {
            get
            {
                return false;
            }
        }

        public bool SupportsDelete
        {
            get
            {
                return true;
            }
        }

        public bool SupportsPages
        {
            get
            {
                return _supportsPages;
            }
        }

        public bool VerifyCredentials()
        {
            using (Blog blog = new Blog(_blogId))
                return blog.VerifyCredentials();

        }

        public RecentPostCapabilities RecentPostCapabilities
        {
            get
            {
                int maxPosts;
                using (Blog blog = new Blog(_blogId))
                {
                    maxPosts = blog.ClientOptions.MaxRecentPosts;
                    if (maxPosts < 0)
                        maxPosts = RecentPostRequest.ALL_POSTS;
                }

                int[] defaultNums = { 20, 50, 100, 500, 1000, 3000 };
                ArrayList arrRequests = new ArrayList();
                foreach (int num in defaultNums)
                {
                    if (num < maxPosts)
                    {
                        arrRequests.Add(new RecentPostRequest(num));
                    }
                    else
                    {

                        break;
                    }
                }

                arrRequests.Add(new RecentPostRequest(maxPosts));

                RecentPostRequest[] requests = (RecentPostRequest[])arrRequests.ToArray(typeof(RecentPostRequest));

                return new RecentPostCapabilities(
                    // valid post fetches
                    requests,

                    new RecentPostRequest(SupportsPages ? 10 : 5)
                    // Larger number by default for blogs that support
                    // pages (this is so pages are not "clipped" out of
                    // view in the default case). Alternatively we could
                    // support separate defaults for Posts and Pages
                    // however this would have introduced too much new
                    // complexity into the OpenPostForm. That said, if
                    // we are hell bent on fixing this it is definitely
                    // do-able with a few hours of careful refactoring
                    // of OpenPostForm.
                    );

            }
        }

        public PostInfo[] GetRecentPosts(RecentPostRequest request)
        {
            return GetPosts(request, false);
        }

        public PostInfo[] GetPages(RecentPostRequest request)
        {
            if (!SupportsPages)
                throw new InvalidOperationException("This post source does not support pages!");

            return GetPosts(request, true);
        }

        public IBlogPostEditingContext GetPost(string postId)
        {
            foreach (BlogPost blogPost in _blogPosts)
            {
                using (Blog blog = new Blog(_blogId))
                {
                    if (blogPost.Id == postId)
                    {
                        // Fix bug 457160 - New post created with a new category
                        // becomes without a category when opened in WLW
                        //
                        // See also RecentPostSynchronizer.DoWork()
                        //
                        // Necessary even in the case of inline categories,
                        // since the inline categories may contain categories
                        // that Writer is not yet aware of
                        try
                        {
                            blog.RefreshCategories();
                        }
                        catch (Exception e)
                        {
                            Trace.Fail("Exception while attempting to refresh categories: " + e.ToString());
                        }

                        // Get the full blog post--necessary because Atom ETag and remote post data is
                        // only available from the full call
                        BlogPost blogPostWithCategories = blog.GetPost(postId, blogPost.IsPage);
                        return new BlogPostEditingContext(
                            _blogId,
                            blogPostWithCategories);
                    }

                }
            }

            // if we get this far then it was a bad postId
            throw new ArgumentException("PostId was not part of the headers fetched");
        }

        public bool DeletePost(string postId, bool isPage)
        {
            bool deletedRemotePost = PostDeleteHelper.SafeDeleteRemotePost(_blogId, postId, isPage);
            if (!deletedRemotePost)
            {
                DialogResult result = DisplayMessage.Show(MessageId.LocalDeleteConfirmation, isPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower));
                if (result == DialogResult.No)
                    return false;
            }

            return PostDeleteHelper.SafeDeleteLocalPost(_blogId, postId);
        }

        private PostInfo[] GetPosts(RecentPostRequest request, bool getPages)
        {
            using (Blog blog = new Blog(_blogId))
            {
                if (getPages)
                    _blogPosts = blog.GetPages(request.NumberOfPosts);
                else
                    _blogPosts = blog.GetRecentPosts(request.NumberOfPosts, false);

                ArrayList recentPosts = new ArrayList();
                foreach (BlogPost blogPost in _blogPosts)
                {
                    PostInfo postInfo = new PostInfo();
                    postInfo.Id = blogPost.Id;
                    postInfo.IsPage = blogPost.IsPage;
                    postInfo.Title = blogPost.Title;
                    postInfo.Permalink = blogPost.Permalink;
                    postInfo.BlogId = blog.Id;
                    postInfo.BlogName = blog.Name;
                    postInfo.BlogPostId = blogPost.Id;
                    postInfo.Contents = blogPost.Contents;
                    postInfo.DateModified = blogPost.DatePublished;
                    recentPosts.Add(postInfo);
                }
                return (PostInfo[])recentPosts.ToArray(typeof(PostInfo));
            }
        }

        private string _blogId;
        private string _name;
        private bool _supportsPages;
        private BlogPost[] _blogPosts;
    }

    /// <summary>
    /// Chooser source for local blog storage
    /// </summary>
    public abstract class LocalStoragePostSource : IPostEditorPostSource
    {
        protected LocalStoragePostSource(string name, DirectoryInfo directory, bool supportsDelete, RecentPostRequest defaultRequest)
        {
            _name = name;
            _directory = directory;
            _supportsDelete = supportsDelete;
            _defaultRequest = defaultRequest;
        }

        public abstract string UniqueId
        {
            get;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public virtual string EmptyPostListMessage
        {
            get
            {
                return null;
            }
        }

        public bool IsSlow
        {
            get
            {
                return false;
            }
        }

        public bool HasMultipleWeblogs
        {
            get
            {
                return true;
            }
        }

        public bool SupportsDelete
        {
            get
            {
                return _supportsDelete;
            }
        }

        public bool SupportsPages
        {
            get
            {
                // local storage doesn't differentiate between
                // posts and pages -- perhaps we want to differentiate
                // using e.g. "About Me (Page)"
                return false;
            }
        }

        public PostInfo[] GetPages(RecentPostRequest request)
        {
            throw new NotSupportedException("Pages are not suppported for this post source!");
        }

        public RecentPostCapabilities RecentPostCapabilities
        {
            get
            {
                return new RecentPostCapabilities(new RecentPostRequest[]
                    {
                        new RecentPostRequest(25),
                        RecentPostRequest.All
                    },
                    _defaultRequest
                );
            }
        }

        public bool VerifyCredentials()
        {
            return true;
        }

        public PostInfo[] GetRecentPosts(RecentPostRequest request)
        {
            return PostEditorFile.GetRecentPosts(_directory, request);
        }

        public IBlogPostEditingContext GetPost(string postId)
        {
            PostEditorFile postEditorFile = PostEditorFile.GetExisting(new FileInfo(postId));
            return postEditorFile.Load();
        }

        public abstract bool DeletePost(string postId, bool isPage);

        private string _name;
        private DirectoryInfo _directory;
        private bool _supportsDelete;
        private RecentPostRequest _defaultRequest;
    }

    public class LocalDraftsPostSource : LocalStoragePostSource
    {
        public LocalDraftsPostSource()
            : base(Res.Get(StringId.Drafts), PostEditorFile.DraftsFolder, true, RecentPostRequest.All)
        {
        }

        public override string UniqueId
        {
            get { return "95809BD4-B8BF-4D05-9465-3BEF5C7FB1EB"; }
        }

        public override string EmptyPostListMessage
        {
            get
            {
                return Res.Get(StringId.OpenPostNoDraftsAvailable);
            }
        }

        public override bool DeletePost(string postId, bool isPage)
        {
            return PostDeleteHelper.SafeDeleteLocalPost(new FileInfo(postId));
        }

    }

    public class LocalRecentPostsPostSource : LocalStoragePostSource
    {
        public LocalRecentPostsPostSource()
            : base(Res.Get(StringId.RecentlyPosted), PostEditorFile.RecentPostsFolder, true, new RecentPostRequest(15))
        {
        }

        public override string UniqueId
        {
            get { return "7AD7C37E-80DA-4516-B439-ACD2247B714D"; }
        }

        public override bool DeletePost(string postId, bool isPage)
        {
            FileInfo postFile = new FileInfo(postId);
            PostInfo postInfo = PostEditorFile.GetPostInfo(postFile);
            if (postInfo != null)
            {
                bool deletedRemotePost = PostDeleteHelper.SafeDeleteRemotePost(postInfo.BlogId, postInfo.BlogPostId, postInfo.IsPage);
                if (!deletedRemotePost)
                {
                    DialogResult result = DisplayMessage.Show(MessageId.LocalDeleteConfirmation, isPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower));
                    if (result == DialogResult.No)
                        return false;
                }

                return PostDeleteHelper.SafeDeleteLocalPost(postFile);
            }
            else
            {
                return false;
            }
        }
    }

    public class RecentPostCapabilities
    {
        public RecentPostCapabilities(RecentPostRequest[] validRequests, RecentPostRequest defaultRequest)
        {
            _validRequests = validRequests;
            _defaultRequest = defaultRequest;
        }

        public RecentPostRequest[] ValidRequests
        {
            get
            {
                return _validRequests;
            }
        }
        private readonly RecentPostRequest[] _validRequests;

        public RecentPostRequest DefaultRequest
        {
            get
            {
                return _defaultRequest;
            }
        }
        private readonly RecentPostRequest _defaultRequest;
    }

    public class RecentPostRequest
    {
        public RecentPostRequest(int numberOfPosts)
        {
            // record number of posts
            _numberOfPosts = numberOfPosts;

            // calculate display name
            if (_numberOfPosts == ALL_POSTS)
            {
                _displayName = Res.Get(StringId.ShowAllPosts);
            }
            else
                _displayName = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.ShowXPosts), _numberOfPosts);
        }

        public static RecentPostRequest All
        {
            get
            {
                return _all;
            }
        }
        private static readonly RecentPostRequest _all = new RecentPostRequest(ALL_POSTS);

        public string DisplayName
        {
            get { return _displayName; }
        }

        public int NumberOfPosts
        {
            get { return _numberOfPosts; }
        }

        public bool AllPosts
        {
            get { return NumberOfPosts == ALL_POSTS; }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object obj)
        {
            RecentPostRequest equalTo = obj as RecentPostRequest;
            return equalTo != null && NumberOfPosts == equalTo.NumberOfPosts;
        }

        public override int GetHashCode()
        {
            return NumberOfPosts.GetHashCode();
        }

        private string _displayName;
        private int _numberOfPosts;
        public const int ALL_POSTS = Int32.MaxValue;
    }

}
