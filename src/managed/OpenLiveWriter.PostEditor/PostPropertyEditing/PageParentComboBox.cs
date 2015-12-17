// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.BlogClient;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    internal class PageParentComboBox : DelayedFetchComboBox
    {
        public PageParentComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            IntegralHeight = false;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                // calculate text to paint
                PostIdAndNameField comboItem = Items[e.Index] as PostIdAndNameField;
                string text = comboItem.ToString();
                if (comboItem is ParentPageComboItem)
                {
                    if (e.Bounds.Width >= Width)
                    {
                        text = new String(' ', (comboItem as ParentPageComboItem).IndentLevel * 3) + text;
                    }
                }

                e.DrawBackground();

                using (Brush brush = new SolidBrush(e.ForeColor))
                    e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X, e.Bounds.Y + 1);

                e.DrawFocusRectangle();
            }
        }

    }

    internal class ParentPageComboItem : PostIdAndNameField, IComparable
    {
        public ParentPageComboItem(PageInfo pageInfo, int indentLevel)
            : this(pageInfo, indentLevel, new ArrayList())
        {
        }

        public ParentPageComboItem(PageInfo pageInfo, int indentLevel, ArrayList children)
            : base(pageInfo.Id, pageInfo.Title)
        {
            _pageInfo = pageInfo;
            _indentLevel = indentLevel;
            _children = children;
        }

        public int IndentLevel
        {
            get { return _indentLevel; }
        }
        private int _indentLevel;

        public ArrayList Children { get { return _children; } }
        private ArrayList _children;

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as ParentPageComboItem).Name);
        }

        public override object Clone()
        {
            return new ParentPageComboItem(_pageInfo, IndentLevel, Children.Clone() as ArrayList);
        }

        private PageInfo _pageInfo;
    }

    internal class BlogPageFetcher : BlogDelayedFetchHandler
    {
        public BlogPageFetcher(string blogId, int timeoutMs)
            : base(blogId, "list of pages", timeoutMs)
        {
        }

        protected override object BlogFetchOperation(Blog blog)
        {
            // refresh our page list
            blog.RefreshPageList();

            // return what we've got
            return ConvertPageList(blog.PageList);
        }

        protected override object[] GetDefaultItems(Blog blog)
        {
            return ConvertPageList(blog.PageList);
        }

        private object[] ConvertPageList(PageInfo[] pageList)
        {
            // accumulate available parents
            Hashtable availableParents = new Hashtable();
            foreach (PageInfo page in pageList)
                availableParents[page.Id] = null;

            // build an array list of pages, fixing up pages with "invalid"
            // parent ids to be root items
            ArrayList sourcePages = new ArrayList();
            foreach (PageInfo page in pageList)
            {
                string parent = page.ParentId;
                if (!availableParents.ContainsKey(page.ParentId))
                    parent = String.Empty;
                sourcePages.Add(new PageInfo(page.Id, page.Title, page.DatePublished, parent));
            }

            // get a tree of child items
            ArrayList pageListItems = ExtractChildItemsOfParent(sourcePages, String.Empty, 0);

            // flatten list  and return
            return FlattenList(pageListItems).ToArray();
        }

        private ArrayList ExtractChildItemsOfParent(ArrayList sourcePages, string parentId, int indentLevel)
        {
            // accumulate all categories with the specified parent
            ArrayList childItems = new ArrayList();
            foreach (PageInfo page in sourcePages.Clone() as ArrayList)
            {
                if (page.ParentId == parentId)
                {
                    // add category to our list and remove it from the source list
                    childItems.Add(new ParentPageComboItem(page, indentLevel));
                    sourcePages.Remove(page);
                }
            }

            // sort the list alphabetically
            childItems.Sort();

            // add the children of each item recursively
            foreach (ParentPageComboItem pageListItem in childItems)
                pageListItem.Children.AddRange(ExtractChildItemsOfParent(sourcePages, pageListItem.Id, pageListItem.IndentLevel + 1));

            // return the list
            return childItems;
        }

        private static ArrayList FlattenList(ArrayList sourceList)
        {
            ArrayList flattenedList = new ArrayList();
            foreach (ParentPageComboItem listItem in sourceList)
            {
                // add the item
                flattenedList.Add(listItem);

                if (listItem.Children.Count > 0)
                {
                    // add children recursively
                    flattenedList.AddRange(FlattenList(listItem.Children));

                    // reset children because we are flattened
                    listItem.Children.Clear();
                }
            }

            return flattenedList;
        }
    }

}
