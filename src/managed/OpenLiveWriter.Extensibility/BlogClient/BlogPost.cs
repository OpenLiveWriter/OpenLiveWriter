// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Extensibility.BlogClient
{

    public class BlogPost : ICloneable, IPostInfo
    {
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _id = String.Empty;

        public bool IsNew
        {
            get { return Id == String.Empty; }
        }

        public void ResetPostForNewBlog(IBlogClientOptions options)
        {
            Id = String.Empty;
            DatePublished = DateTime.MinValue;

            // if the current blog doesn't support pages then force
            // the post to not be a page
            if (!options.SupportsPages)
                IsPage = false;

            // author and parent are tied to blog, reset them as well
            Author = PostIdAndNameField.Empty;
            PageParent = PostIdAndNameField.Empty;

            ETag = string.Empty;
            AtomRemotePost = null;

            // categories are tied to blog, reset them
            Categories = null;
            NewCategories = null;

            // ping urls are tied to blog, reset them so they are sent again for a new blog
            UncommitPingUrls();
        }

        public bool IsPage
        {
            get { return _isPage; }
            set
            {
                _isPage = value;

                if (!_isPage)
                {
                    PageParent = PostIdAndNameField.Empty;
                    PageOrder = String.Empty;
                }
            }
        }
        private bool _isPage = false;

        public string Title
        {
            get { return XmlCharacterHelper.RemoveInvalidXmlChars(_title); }
            set { _title = XmlCharacterHelper.RemoveInvalidXmlChars(value); }
        }
        private string _title = String.Empty;

        public string Contents
        {
            get
            {
                string contents;
                if (ExtendedContents.Length > 0)
                    contents = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", MainContents, ExtendedEntryBreak, ExtendedContents);
                else
                    contents = MainContents;

                if (contents != null)
                    contents = XmlCharacterHelper.RemoveInvalidXmlChars(contents);

                return contents;
            }
            set { SetContents(XmlCharacterHelper.RemoveInvalidXmlChars(value)); }
        }

        public string MainContents
        {
            get { return _mainContents; }
        }
        private string _mainContents = String.Empty;

        public string ExtendedContents
        {
            get { return _extendedContents; }
        }
        private string _extendedContents = String.Empty;

        public string Permalink
        {
            get { return _permaLink; }
            set
            {
                // normalize to String.Empty
                if (value != null)
                    _permaLink = value;
                else
                    _permaLink = String.Empty;
            }
        }
        private string _permaLink = String.Empty;

        public BlogPostCategory[] Categories
        {
            get { return _categories; }
            set
            {
                if (value != null)
                    _categories = value;
                else
                    _categories = new BlogPostCategory[] { };
            }
        }
        private BlogPostCategory[] _categories = new BlogPostCategory[] { };

        ICategoryInfo[] IPostInfo.Categories
        {
            get
            {
                List<PostCategoryInfo> categories = new List<PostCategoryInfo>();
                foreach (BlogPostCategory cat in Categories)
                    categories.Add(new PostCategoryInfo(cat.Id, cat.Name, false));
                foreach (BlogPostCategory cat in NewCategories)
                    categories.Add(new PostCategoryInfo(cat.Id, cat.Name, true));
                return categories.ToArray();
            }
        }

        private class PostCategoryInfo : ICategoryInfo
        {
            private readonly string id;
            private readonly string name;
            private readonly bool isNew;

            public PostCategoryInfo(string id, string name, bool isNew)
            {
                this.id = id;
                this.name = name;
                this.isNew = isNew;
            }

            public string Id
            {
                get { return id; }
            }

            public string Name
            {
                get { return name; }
            }

            public bool IsNew
            {
                get { return isNew; }
            }
        }

        public BlogPostCategory[] NewCategories
        {
            get { return _newCategories; }
            set
            {
                if (value != null)
                    _newCategories = value;
                else
                    _newCategories = new BlogPostCategory[] { };
            }
        }
        private BlogPostCategory[] _newCategories = new BlogPostCategory[] { };

        public void CommitNewCategory(BlogPostCategory newCategory)
        {
            // revised category list
            ArrayList categories = new ArrayList(Categories);
            categories.Add(newCategory);
            Categories = categories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[];

            // revised new category list
            ArrayList newCategories = new ArrayList();
            foreach (BlogPostCategory category in NewCategories)
                if (!category.Name.Equals(newCategory.Name))
                    newCategories.Add(category);
            NewCategories = newCategories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[];
        }

        public DateTime DatePublished
        {
            get { return _datePublished; }
            set { _datePublished = value; }
        }
        private DateTime _datePublished = DateTime.MinValue;

        public DateTime DatePublishedOverride
        {
            get { return _datePublishedOverride; }
            set { _datePublishedOverride = value; }
        }
        private DateTime _datePublishedOverride = DateTime.MinValue;

        public bool HasDatePublishedOverride
        {
            get { return DatePublishedOverride != DateTime.MinValue; }
        }

        public BlogCommentPolicy CommentPolicy
        {
            get { return _commentPolicy; }
            set { _commentPolicy = value; }
        }
        private BlogCommentPolicy _commentPolicy = BlogCommentPolicy.Unspecified;

        public BlogTrackbackPolicy TrackbackPolicy
        {
            get { return _trackbackPolicy; }
            set { _trackbackPolicy = value; }
        }
        private BlogTrackbackPolicy _trackbackPolicy = BlogTrackbackPolicy.Unspecified;

        public string Keywords
        {
            get { return XmlCharacterHelper.RemoveInvalidXmlChars(_keywords); }
            set { XmlCharacterHelper.RemoveInvalidXmlChars(_keywords = value); }
        }
        private string _keywords = String.Empty;

        public string Excerpt
        {
            get { return XmlCharacterHelper.RemoveInvalidXmlChars(_excerpt); }
            set { _excerpt = XmlCharacterHelper.RemoveInvalidXmlChars(value); }
        }
        private string _excerpt = String.Empty;

        public string[] PingUrlsPending
        {
            get { return _pingUrlsPending.Clone() as string[]; }
            set { _pingUrlsPending = value.Clone() as string[]; }
        }
        private string[] _pingUrlsPending = new string[0];

        public string[] PingUrlsSent
        {
            get { return _pingUrlsSent.Clone() as string[]; }
            set { _pingUrlsSent = value.Clone() as string[]; }
        }
        private string[] _pingUrlsSent = new string[0];

        public void CommitPingUrls()
        {
            PingUrlsSent = ArrayHelper.Union(PingUrlsSent, PingUrlsPending);
            PingUrlsPending = new string[0];
        }

        public void UncommitPingUrls()
        {
            PingUrlsPending = ArrayHelper.Union(PingUrlsSent, PingUrlsPending);
            PingUrlsSent = new string[0];
        }

        public string Slug
        {
            get { return _slug; }
            set { _slug = value; }
        }
        private string _slug = String.Empty;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        private string _password = String.Empty;

        public PostIdAndNameField Author
        {
            get { return _author; }
            set { _author = value; }
        }
        private PostIdAndNameField _author = PostIdAndNameField.Empty;

        public PostIdAndNameField PageParent
        {
            get { return _parentPageParent; }
            set { _parentPageParent = value; }
        }
        private PostIdAndNameField _parentPageParent = PostIdAndNameField.Empty;

        public string PageOrder
        {
            get { return _pageOrder; }
            set { _pageOrder = value; }
        }
        private string _pageOrder = String.Empty;

        /// <summary>
        /// True if the post is a temporary style detection post.
        /// </summary>
        public bool IsTemporary
        {
            get { return _temp; }
            set { _temp = value; }
        }
        private bool _temp = false;

        public string ContentsVersionSignature
        {
            get
            {
                if (_publishedPostHash == null)
                    _publishedPostHash = MD5Hash(Contents);
                return _publishedPostHash;
            }
            set { _publishedPostHash = value; }
        }
        private string _publishedPostHash = null;

        public string ETag
        {
            get { return _etag; }
            set { _etag = value; }
        }
        private string _etag = String.Empty;

        public XmlDocument AtomRemotePost
        {
            get
            {
                if (_atomRemotePost != null)
                    return (XmlDocument)_atomRemotePost.Clone();
                else
                    return null;
            }
            set
            {
                if (value == null)
                    _atomRemotePost = null;
                else
                    _atomRemotePost = (XmlDocument)value.Clone();
            }
        }
        private XmlDocument _atomRemotePost = null;

        /// <summary>
        /// Sets the main and extended contents of the post.
        /// </summary>
        /// <param name="mainContents"></param>
        /// <param name="extendedContents"></param>
        public void SetContents(string mainContents, string extendedContents)
        {
            _mainContents = mainContents;
            _extendedContents = extendedContents;
        }

        /// <summary>
        /// Sets the contents of the post.
        /// (if the content contains the <!--more-->, it will automatically be split into
        /// the main and extended contents).
        /// </summary>
        /// <param name="contents"></param>
        private void SetContents(string contents)
        {
            Match match = ExtendedEntryBreakRegex.Match(contents);
            if (match.Success)
            {
                _mainContents = contents.Substring(0, match.Index);
                _extendedContents = contents.Substring(match.Index + match.Length);
            }
            else
            {
                _mainContents = contents;
                _extendedContents = String.Empty;
            }
        }

        public static readonly string ClearBreak = "<br clear=\"all\" />"; // float clearing break
        public static readonly string HorizontalLine = "<hr />"; // horizontal rule
        public static readonly string PlainTextHorizontalLine = "--------------------------------------------------------------------------------"; // 80 dashes
        public static readonly string ExtendedEntryBreak = "<!--more-->";  //the more text comment
        public static readonly string ExtendedEntryBreakMoreText = "more"; //the value of the more text comment
        private static Regex ExtendedEntryBreakRegex = new Regex("<!--more-->"); //regex for detecting the more text comment

        public object Clone()
        {
            BlogPost newPost = new BlogPost();
            newPost.CopyFrom(this);
            return newPost;
        }

        public void CopyFrom(BlogPost sourcePost)
        {
            Id = sourcePost.Id;
            IsPage = sourcePost.IsPage;
            Title = sourcePost.Title;
            Contents = sourcePost.Contents;
            Permalink = sourcePost.Permalink;
            Categories = sourcePost.Categories;
            NewCategories = sourcePost.NewCategories;
            DatePublished = sourcePost.DatePublished;
            DatePublishedOverride = sourcePost.DatePublishedOverride;
            CommentPolicy = sourcePost.CommentPolicy;
            TrackbackPolicy = sourcePost.TrackbackPolicy;
            Keywords = sourcePost.Keywords;
            Excerpt = sourcePost.Excerpt;
            PingUrlsPending = sourcePost.PingUrlsPending;
            PingUrlsSent = sourcePost.PingUrlsSent;
            PageParent = sourcePost.PageParent.Clone() as PostIdAndNameField;
            PageOrder = sourcePost.PageOrder;
            Slug = sourcePost.Slug;
            Password = sourcePost.Password;
            Author = sourcePost.Author.Clone() as PostIdAndNameField;

            IsTemporary = sourcePost.IsTemporary;
            ContentsVersionSignature = sourcePost.ContentsVersionSignature;
            ETag = sourcePost.ETag;
            AtomRemotePost = sourcePost.AtomRemotePost != null
                ? (XmlDocument)sourcePost.AtomRemotePost.Clone()
                : null;
        }

        public static string CalculateContentsSignature(BlogPost post)
        {
            if (post.Contents == null)
                return null;
            try
            {
                return MD5Hash(post.Contents);
            }
            catch (Exception e)
            {
                Debug.Fail("Error creating contents version signature", e.ToString());
                return null;
            }
        }

        private static string MD5Hash(string str)
        {
            byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
    }

    public enum BlogCommentPolicy
    {
        Unspecified,
        None,
        Open,
        Closed
    }

    public enum BlogTrackbackPolicy
    {
        Unspecified,
        Allow,
        Deny
    }

    public class PostIdAndNameField : ICloneable
    {
        public static PostIdAndNameField Empty
        {
            get
            {
                return _empty;
            }
        }
        private static readonly PostIdAndNameField _empty = new PostIdAndNameField();

        public PostIdAndNameField()
            : this(String.Empty)
        {
        }

        public PostIdAndNameField(string emptyFieldText)
            : this(String.Empty, String.Empty)
        {
            _emptyFieldText = emptyFieldText;
        }

        public PostIdAndNameField(string id, string name)
        {
            _id = id;
            if (name != String.Empty)
                _name = name;
            else
                _name = _id;
        }

        public bool IsEmpty
        {
            get { return Id == String.Empty; }
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is PostIdAndNameField
                && ((PostIdAndNameField)obj).Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            if (_id != String.Empty)
            {
                return Name;
            }
            else
            {
                return _emptyFieldText;
            }
        }

        public virtual object Clone()
        {
            return new PostIdAndNameField(_id, _name);
        }

        private string _id = String.Empty;
        private string _name = String.Empty;
        private string _emptyFieldText = String.Empty;
    }
}

