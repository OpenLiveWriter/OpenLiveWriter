// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenLiveWriter.PostEditor.JumpList
{
    /// <summary>
    /// Represents a custom category on the taskbar's jump list
    /// </summary>
    public class JumpListCustomCategory
    {
        /// <summary>
        /// Creates a new custom category instance
        /// </summary>
        /// <param name="categoryName">Category name</param>
        public JumpListCustomCategory(string categoryName)
        {
            Name = categoryName;

            JumpListItems = new JumpListItemCollection<IJumpListItem>();
        }

        /// <summary>
        /// Creates a new custom category instance
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="maxItems">The maximum number of items the custom category should hold</param>
        public JumpListCustomCategory(string categoryName, int maxItems)
        {
            Name = categoryName;
            JumpListItems = new JumpListItemCollection<IJumpListItem>();
            MaxItems = maxItems;
        }

        public int MaxItems { get; private set; }
        private string name;

        internal JumpListItemCollection<IJumpListItem> JumpListItems
        {
            get;
            private set;
        }

        /// <summary>
        /// Category name
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (String.Compare(name, value, false, CultureInfo.CurrentUICulture) != 0)
                {
                    name = value;
                }
            }
        }

        /// <summary>
        /// Add JumpList items for this category
        /// </summary>
        /// <param name="items">The items to add to the JumpList.</param>
        public void AddJumpListItems(params IJumpListItem[] items)
        {
            foreach (IJumpListItem item in items)
                JumpListItems.Add(item);
        }

        /// <summary>
        /// Add a JumpList item to the bottom of the category. If the category is already full,
        /// the item will not be added.
        /// </summary>
        /// <param name="item">The item to add to the JumpList.</param>
        /// <returns>true if the item was added successfully, false otherwise.</returns>
        public bool AddJumpListItem(IJumpListItem item)
        {
            // If the item is already on the jumplist, remove it first.
            RemoveJumpListItem(item.Path);

            if (MaxItems > 0 && JumpListItems.Count >= MaxItems)
                return false;

            JumpListItems.Add(item);
            return true;
        }

        /// <summary>
        /// Inserts a JumpList item at the top of the category. If the category is already full,
        /// the bottom item will be removed.
        /// </summary>
        /// <param name="item">The item to insert into the JumpList.</param>
        public void InsertJumpListItem(IJumpListItem item)
        {
            // If the item is already on the jumplist, remove it first.
            RemoveJumpListItem(item.Path);

            if (MaxItems > 0 && JumpListItems.Count >= MaxItems)
                JumpListItems.Remove(JumpListItems[JumpListItems.Count - 1]);

            JumpListItems.Insert(0, item);
        }

        internal void RemoveJumpListItem(string path)
        {
            List<IJumpListItem> itemsToRemove = new List<IJumpListItem>();

            // Check for items to remove
            foreach (IJumpListItem item in JumpListItems)
            {
                if (string.Compare(path, item.Path, false, CultureInfo.CurrentUICulture) == 0)
                {
                    itemsToRemove.Add(item);
                }
            }

            // Remove matching items
            for (int i = 0; i < itemsToRemove.Count; i++)
            {
                JumpListItems.Remove(itemsToRemove[i]);
            }
        }
    }
}
