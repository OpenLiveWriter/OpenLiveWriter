// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    internal interface IBlogCategorySettings
    {
        BlogPostCategory[] RefreshCategories(bool ignoreErrors);
        void UpdateCategories(BlogPostCategory[] category);
    }

    internal class CategoryContext
    {

        public CategoryContext()
        {
            _selectedCategories = new BlogPostCategory[0];
            SetBlogCategories(new BlogPostCategory[0]);
            SetNewCategories(new BlogPostCategory[0]);
            _selectionMode = SelectionModes.MultiSelect;
            _supportsAddingCategories = false;
            _supportsHeirarchicalCategories = false;

        }

        public BlogPostCategory[] SelectedExistingCategories
        {
            get
            {
                ArrayList selectedExistingCategories = new ArrayList();
                foreach (BlogPostCategory category in SelectedCategories)
                    if (_blogCategories.Contains(category))
                        selectedExistingCategories.Add(category);
                return selectedExistingCategories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[];
            }
        }

        public BlogPostCategory[] SelectedNewCategories
        {
            get
            {
                ArrayList selectedNewCategories = new ArrayList();
                foreach (BlogPostCategory category in SelectedCategories)
                    if (_newCategories.Contains(category))
                        selectedNewCategories.Add(category);
                return selectedNewCategories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[];
            }
        }

        public BlogPostCategory[] SelectedCategories
        {
            get
            {
                return _selectedCategories;
            }
            set
            {
                _selectedCategories = InsureSelectedValuesAreInCategoryList(value, true); DoChange(ChangeType.SelectedCategory);
            }
        }
        private BlogPostCategory[] _selectedCategories = new BlogPostCategory[0];

        public BlogPostCategory[] BlogCategories
        {
            get { return _blogCategories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[]; }
        }

        public BlogPostCategory[] Categories
        {
            get
            {
                // first return the new categories, then return the blog categories
                ArrayList categories = new ArrayList();
                categories.AddRange(_newCategories);
                categories.AddRange(_blogCategories);
                return categories.ToArray(typeof(BlogPostCategory)) as BlogPostCategory[];
            }
        }

        public void SetBlogCategories(BlogPostCategory[] categories)
        {
            _blogCategories.Clear();
            _blogCategories.AddRange(categories);
            DoChange(ChangeType.Category);
        }
        private ArrayList _blogCategories = new ArrayList();

        public void SetNewCategories(BlogPostCategory[] categories)
        {
            _newCategories.Clear();

            // de-dup with any categories that already exist for this blog
            foreach (BlogPostCategory category in categories)
                if (!_blogCategories.Contains(category))
                    _newCategories.Add(category);

            DoChange(ChangeType.Category);
        }

        public void AddNewCategory(BlogPostCategory newCategory)
        {
            // add the new category
            _newCategories.Add(newCategory);

            // fire the change event
            DoChange(ChangeType.Category);
        }
        private ArrayList _newCategories = new ArrayList();

        public void CommitNewCategory(BlogPostCategory newCategory)
        {
            // remove from the new category list
            for (int i = 0; i < _newCategories.Count; i++)
            {
                if ((_newCategories[i] as BlogPostCategory).Name.Equals(newCategory.Name))
                {
                    _newCategories.RemoveAt(i);
                    break;
                }
            }

            // add to blog categories list and save the list
            _blogCategories.Add(newCategory);
            _blogCategorySettings.UpdateCategories(BlogCategories);

            // refresh selected categories (makes sure they pickup the category id)
            SelectedCategories = InsureSelectedValuesAreInCategoryList(_selectedCategories, false);

            // notify of change
            DoChange(ChangeType.Category);
        }

        public string Text
        {
            get
            {
                switch (SelectedCategories.Length)
                {
                    case 0:
                        return SelectionMode == SelectionModes.MultiSelect ? Res.Get(StringId.CategoryControlSetCategories) : Res.Get(StringId.CategoryControlSetCategory);
                    default:
                        return HtmlUtils.UnEscapeEntities(StringHelper.Join(SelectedCategories, CultureInfo.CurrentCulture.TextInfo.ListSeparator + " "), HtmlUtils.UnEscapeMode.Attribute);
                }
            }
        }

        public string FormattedCategoryList
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    (SelectionMode == SelectionModes.MultiSelect) ? Res.Get(StringId.CategoryControlCategories) : Res.Get(StringId.CategoryControlCategory),
                    (SelectedCategories.Length == 0) ? Res.Get(StringId.CategoryControlNoCategories) : HtmlUtils.UnEscapeEntities(StringHelper.Join(SelectedCategories, CultureInfo.CurrentCulture.TextInfo.ListSeparator + " "), HtmlUtils.UnEscapeMode.Default));
            }
        }

        public SelectionModes SelectionMode
        {
            get
            {
                return _selectionMode;
            }
            set
            {
                if (_selectionMode != value)
                {
                    _selectionMode = value;
                    DoChange(ChangeType.SelectionMode);
                }
            }
        }
        private SelectionModes _selectionMode = SelectionModes.MultiSelect;

        public bool SupportsAddingCategories
        {
            get { return _supportsAddingCategories; }
            set { _supportsAddingCategories = value; }
        }
        private bool _supportsAddingCategories = false;

        public bool SupportsHierarchicalCategories
        {
            get { return _supportsHeirarchicalCategories; }
            set { _supportsHeirarchicalCategories = value; }
        }
        private bool _supportsHeirarchicalCategories = false;

        public int MaxCategoryNameLength
        {
            get { return _maxCategoryNameLength; }
            set { _maxCategoryNameLength = value; }
        }
        private int _maxCategoryNameLength = 0;

        public enum SelectionModes
        {
            SingleSelect,
            MultiSelect
        }

        public void Refresh()
        {
            Refresh(false);
        }

        private void Refresh(bool ignoreErrors)
        {
            if (_blogCategorySettings != null)
            {
                SetBlogCategories(_blogCategorySettings.RefreshCategories(ignoreErrors));
                SelectedCategories = InsureSelectedValuesAreInCategoryList(_selectedCategories, false);
            }
        }

        public IBlogCategorySettings BlogCategorySettings
        {
            get
            {
                return _blogCategorySettings;
            }
            set
            {
                _blogCategorySettings = value;
            }
        }
        private IBlogCategorySettings _blogCategorySettings;

        public event CategoryChangedEventHandler Changed;
        protected virtual void OnChanged(object sender, CategoryChangedEventArgs e)
        {
            if (Changed != null)
                Changed(sender, e);
        }

        private BlogPostCategory[] InsureSelectedValuesAreInCategoryList(BlogPostCategory[] selectedCategories, bool allowAutoRefresh)
        {
            ArrayList updatedCategoryList = new ArrayList();

            // for each potential selection, validate that there is an existing category
            // with the same id or name and "match" it by adding the category to our
            // list of selected categories
            foreach (BlogPostCategory selectedCategory in selectedCategories)
            {
                // find a match in the list
                foreach (BlogPostCategory category in Categories)
                {
                    if (category.Equals(selectedCategory))
                    {
                        updatedCategoryList.Add(category);
                        break;
                    }
                }
            }

            return (BlogPostCategory[])updatedCategoryList.ToArray(typeof(BlogPostCategory));
        }

        private void DoChange(ChangeType changeType)
        {
            OnChanged(this, new CategoryChangedEventArgs(changeType));
        }

        public enum ChangeType
        {
            Category,
            SelectedCategory,
            SelectionMode
        }

        public delegate void CategoryChangedEventHandler(object sender, CategoryChangedEventArgs eventArgs);

        public class CategoryChangedEventArgs : EventArgs
        {
            private ChangeType _changeType;

            public CategoryChangedEventArgs(ChangeType changeType) : base()
            {
                _changeType = changeType;
            }

            public ChangeType ChangeType
            {
                get
                {
                    return _changeType;
                }
            }
        }

    }

    internal class BlogPostCategoryListItem : IComparable, ICloneable
    {
        public BlogPostCategoryListItem(BlogPostCategory category, int indentLevel)
            : this(category, indentLevel, new ArrayList())
        {
        }

        public BlogPostCategoryListItem(BlogPostCategory category, int indentLevel, ArrayList children)
        {
            _category = category;
            _indentLevel = indentLevel;
            _children = children;
        }

        public BlogPostCategory Category { get { return _category; } }
        private BlogPostCategory _category;

        public int IndentLevel { get { return _indentLevel; } }
        private int _indentLevel;

        public ArrayList Children { get { return _children; } }
        private ArrayList _children;

        public int CompareTo(object obj)
        {
            return Category.CompareTo((obj as BlogPostCategoryListItem).Category);
        }

        public object Clone()
        {
            return new BlogPostCategoryListItem(Category, IndentLevel, Children.Clone() as ArrayList);
        }

        public static BlogPostCategoryListItem[] BuildList(BlogPostCategory[] categories, bool flatten)
        {
            // accumulate available parents
            Hashtable availableParents = new Hashtable();
            foreach (BlogPostCategory category in categories)
                availableParents[category.Id] = null;

            // build an array list of categories, fixing up categories with "invalid"
            // parent ids to be root items
            ArrayList sourceCategories = new ArrayList();
            foreach (BlogPostCategory category in categories)
            {
                string parent = category.Parent;
                if (!availableParents.ContainsKey(category.Parent))
                    parent = String.Empty;
                sourceCategories.Add(new BlogPostCategory(category.Id, category.Name, parent));
            }

            // get a tree of child items
            ArrayList categoryListItems = ExtractChildItemsOfParent(sourceCategories, String.Empty, 0);

            // flatten list if requested
            if (flatten)
                categoryListItems = FlattenList(categoryListItems);

            // return list
            return categoryListItems.ToArray(typeof(BlogPostCategoryListItem)) as BlogPostCategoryListItem[];
        }

        private static ArrayList ExtractChildItemsOfParent(ArrayList sourceCategories, string parentId, int indentLevel)
        {
            // accumulate all categories with the specified parent
            ArrayList childItems = new ArrayList();
            foreach (BlogPostCategory category in sourceCategories.Clone() as ArrayList)
            {
                if (category.Parent == parentId)
                {
                    // add category to our list and remove it from the source list
                    childItems.Add(new BlogPostCategoryListItem(category, indentLevel));
                    sourceCategories.Remove(category);
                }
            }

            // sort the list alphabetically
            childItems.Sort();

            // add the children of each item recursively
            foreach (BlogPostCategoryListItem categoryListItem in childItems)
                categoryListItem.Children.AddRange(ExtractChildItemsOfParent(sourceCategories, categoryListItem.Category.Id, categoryListItem.IndentLevel + 1));

            // return the list
            return childItems;
        }

        private static ArrayList FlattenList(ArrayList sourceList)
        {
            ArrayList flattenedList = new ArrayList();
            foreach (BlogPostCategoryListItem listItem in sourceList)
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
