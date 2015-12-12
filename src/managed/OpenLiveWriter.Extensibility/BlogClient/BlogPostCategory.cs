// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    public class BlogPostCategory : IComparable, ICloneable
    {
        public BlogPostCategory(string name)
            : this(name, name)
        {
        }

        public BlogPostCategory(string id, string name)
            : this(id, name, String.Empty)
        {
        }

        public BlogPostCategory(string id, string name, string parent)
        {
            Id = id;
            Name = name;
            Parent = parent;
        }

        public readonly string Id;
        public readonly string Name;
        public readonly string Parent;

        private bool HasParent
        {
            get { return !string.IsNullOrEmpty(Parent) && Parent != "0"; }
        }

        private bool IsCookedUpId
        {
            get { return Id == Name || string.IsNullOrEmpty(Id); }
        }

        public override bool Equals(object obj)
        {
            if (obj is BlogPostCategory)
                return Equals(this, (BlogPostCategory)obj, false);
            else
                return false;
        }

        public static bool Equals(BlogPostCategory x, BlogPostCategory y, bool lenientNameComparison)
        {
            if (y == null)
                return false;

            if (!x.IsCookedUpId && !y.IsCookedUpId)
                return x.Id == y.Id;

            // WordPress uses the string "0" to indicate no parent.
            // It's hard to tell by looking at this, but the fact that
            // HasParent takes this into account means we get the
            // correct behavior.
            if ((x.HasParent || y.HasParent) && x.Parent != y.Parent)
                return false;

            string selfNameUpper = x.Name.ToUpperInvariant();
            string otherNameUpper = y.Name.ToUpperInvariant();
            if (selfNameUpper == otherNameUpper)
                return true;

            if (lenientNameComparison
                    && HtmlUtils.UnEscapeEntities(selfNameUpper, HtmlUtils.UnEscapeMode.Default)
                        == HtmlUtils.UnEscapeEntities(otherNameUpper, HtmlUtils.UnEscapeMode.Default))
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            // BlogPostCategory hash code is useless, due to complexity of .Equals()
            return 0;
        }

        public int CompareTo(object obj)
        {
            BlogPostCategory category = obj as BlogPostCategory;
            if (category == null)
                throw new ArgumentException("Object can't be compared to a BlogPostCategory");

            bool thisIsCategoryNone = BlogPostCategoryNone.IsCategoryNone(this);
            bool otherIsCategoryNone = BlogPostCategoryNone.IsCategoryNone(category);

            if (thisIsCategoryNone && otherIsCategoryNone)
                return 0;
            if (thisIsCategoryNone)
                return -1;
            if (otherIsCategoryNone)
                return 1;

            return String.Compare(Name, category.Name, StringComparison.Ordinal);
        }

        public object Clone()
        {
            return new BlogPostCategory(Id, Name, Parent);
        }

    }

    public class BlogPostCategoryNone : BlogPostCategory
    {
        public BlogPostCategoryNone()
            : base(NONE_ID, Res.Get(StringId.BlogCategoryNone))
        {
        }

        public static bool IsCategoryNone(BlogPostCategory category)
        {
            return category.Id.Equals(NONE_ID);
        }

        private const string NONE_ID = "CD9B8072-AB3A-43B0-A8B8-0AD5DF91D192";
    }
}
