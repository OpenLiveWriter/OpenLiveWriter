// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Generic;

namespace OpenLiveWriter.PostEditor.JumpList
{
    /// <summary>
    /// Represents a collection of jump list items.
    /// </summary>
    /// <typeparam name="T">The type of elements in this collection.</typeparam>
    internal class JumpListItemCollection<T> : ICollection<T>
    {
        private List<T> items = new List<T>();

        /// <summary>
        /// Gets or sets a value that determines if this collection is read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets a count of the items currently in this collection.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }

        /// <summary>
        /// Adds the specified item to this collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            items.Add(item);
        }

        /// <summary>
        /// Inserts the specified item into the collection.
        /// </summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, T item)
        {
            items.Insert(index, item);
        }

        /// <summary>
        /// Removes the first instance of the specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><b>true</b> if an item was removed, otherwise <b>false</b> if no items were removed.</returns>
        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        /// <summary>
        /// Clears all items from this collection.
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Determines if this collection contains the specified item.
        /// </summary>
        /// <param name="item">The search item.</param>
        /// <returns><b>true</b> if an item was found, otherwise <b>false</b>.</returns>
        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Returns the item at the given index.
        /// </summary>
        public T this[int arg]
        {
            get { return this[arg]; }
        }

        /// <summary>
        /// Copies this collection to a compatible one-dimensional array,
        /// starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The array name.</param>
        /// <param name="index">The index of the starting element.</param>
        public void CopyTo(T[] array, int index)
        {
            items.CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator to iterate through this collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection of a specified type.
        /// </summary>
        /// <returns>An enumerator to iterate through this collection.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
