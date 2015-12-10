// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Generic;

namespace OpenLiveWriter.PostEditor.JumpList
{
    /// <summary>
    /// Represents a collection of custom categories
    /// </summary>
    public class JumpListCustomCategoryCollection
        : ICollection<JumpListCustomCategory>
    {
        private List<JumpListCustomCategory> categories = new List<JumpListCustomCategory>();

        /// <summary>
        /// Determines if this collection is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// The number of items in this collection
        /// </summary>
        public int Count
        {
            get { return categories.Count; }
        }

        /// <summary>
        /// Add the specified category to this collection
        /// </summary>
        /// <param name="category">Category to add</param>
        public void Add(JumpListCustomCategory category)
        {
            categories.Add(category);
        }

        /// <summary>
        /// Remove the specified category from this collection
        /// </summary>
        /// <param name="category">Category item to remove</param>
        /// <returns>True if item was removed.</returns>
        public bool Remove(JumpListCustomCategory category)
        {
            return categories.Remove(category);
        }

        /// <summary>
        /// Clear all items from the collection
        /// </summary>
        public void Clear()
        {
            categories.Clear();
        }

        /// <summary>
        /// Determine if this collection contains the specified item
        /// </summary>
        /// <param name="category">Category to search for</param>
        /// <returns>True if category was found</returns>
        public bool Contains(JumpListCustomCategory category)
        {
            return categories.Contains(category);
        }

        /// <summary>
        /// Copy this collection to a compatible one-dimensional array,
        /// starting at the specified index of the target array
        /// </summary>
        /// <param name="array">Array to copy to</param>
        /// <param name="index">Index of target array to start copy</param>
        public void CopyTo(JumpListCustomCategory[] array, int index)
        {
            categories.CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>Enumerator to iterate through this collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return categories.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>Enumerator to iterate through this collection.</returns>
        IEnumerator<JumpListCustomCategory> IEnumerable<JumpListCustomCategory>.GetEnumerator()
        {
            return categories.GetEnumerator();
        }
    }
}
