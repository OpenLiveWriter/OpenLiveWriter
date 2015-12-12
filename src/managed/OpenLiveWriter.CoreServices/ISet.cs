// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Interface for sets of objects.  Sets are like lists,
    /// except they do not contain duplicates, and are usually
    /// optimized for fast search (Contains).
    /// </summary>
    public interface ISet : ICollection, IEnumerable
    {
        // Commented out because it's already specified by ICollection.
        // object SyncRoot { get; }

        // Commented out because it's already specified by ICollection.
        // bool IsSynchronized { get; }

        /// <summary>
        /// Adds a value to the set.  Returns false if the
        /// value is already in the set.
        /// </summary>
        bool Add(object o);

        /// <summary>
        /// Adds each of the items in the collection to the set.
        /// </summary>
        int AddAll(IEnumerable col);

        /// <summary>
        /// Returns true if the object was in the set before
        /// the remove operation.
        ///
        /// (After the call returns, the object will not be
        /// in the set, regardless of the return value.)
        /// </summary>
        bool Remove(object o);

        /// <summary>
        /// Removes all the elements of the given tree
        /// from this tree, if they are present.
        ///
        /// Returns the number of items actually found
        /// and removed.
        /// </summary>
        int RemoveAll(IEnumerable col);

        /// <summary>
        /// Clear all elements from the set.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns true if the object is in the set.
        /// </summary>
        bool Contains(object o);

        /// <summary>
        /// Returns true if the set contains no elements.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Returns the number of items in the set.
        /// If you only care about whether the set is
        /// empty, use the (possibly cheaper) IsEmpty
        /// property instead.
        /// </summary>
        // Commented out because it's already specified by ICollection.
        // int Count { get; }

        /// <summary>
        /// Returns the contents of the set as an array.
        /// </summary>
        object[] ToArray();

        /// <summary>
        /// Returns the contents of the set as an array of
        /// the given type.
        /// </summary>
        Array ToArray(Type type);
    }

    public class Set
    {
        // no initialization possible
        private Set()
        {
        }

        public static ISet Synchronized(ISet s)
        {
            return Synchronized(s, false);
        }

        public static ISet Synchronized(ISet s, bool force)
        {
            if (force || !s.IsSynchronized)
                return new SynchronizedSet(s);
            else
                return s;
        }
    }

    internal class SynchronizedSet : ISet
    {
        private readonly ISet s;

        internal SynchronizedSet(ISet internalSet)
        {
            this.s = internalSet;
        }

        #region ISet Members

        public object SyncRoot
        {
            get { return s.SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public bool Add(object o)
        {
            lock (SyncRoot)
                return s.Add(o);
        }

        public int AddAll(IEnumerable col)
        {
            lock (SyncRoot)
                return s.AddAll(col);
        }

        public bool Remove(object o)
        {
            lock (SyncRoot)
                return s.Remove(o);
        }

        public int RemoveAll(IEnumerable col)
        {
            lock (SyncRoot)
                return s.RemoveAll(col);
        }

        public void Clear()
        {
            lock (SyncRoot)
                s.Clear();
        }

        public bool Contains(object o)
        {
            lock (SyncRoot)
                return s.Contains(o);
        }

        public bool IsEmpty
        {
            get
            {
                lock (SyncRoot)
                    return s.IsEmpty;
            }
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                    return s.Count;
            }
        }

        public object[] ToArray()
        {
            lock (SyncRoot)
                return s.ToArray();
        }

        Array ISet.ToArray(Type type)
        {
            lock (SyncRoot)
                return s.ToArray(type);
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
                s.CopyTo(array, index);
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            lock (SyncRoot)
                return s.GetEnumerator();
        }

        #endregion

    }

}
