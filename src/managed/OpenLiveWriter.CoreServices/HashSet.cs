// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for HashSet.
    /// </summary>
    public class HashSet : ISet
    {
        private readonly Hashtable ht;

        public HashSet()
        {
            this.ht = new Hashtable();
        }

        public HashSet(int capacity)
        {
            this.ht = new Hashtable(capacity);
        }

        public HashSet(Hashtable ht)
        {
            this.ht = (Hashtable)ht.Clone();
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return ht.SyncRoot; }
        }

        public bool Add(object o)
        {
            if (!ht.ContainsKey(o))
            {
                ht.Add(o, null);
                return true;
            }
            else
                return false;

        }

        public int AddAll(IEnumerable col)
        {
            int count = 0;

            foreach (object o in col)
                if (Add(o))
                    count++;

            return count;
        }

        public bool Remove(object o)
        {
            if (ht.ContainsKey(o))
            {
                ht.Remove(o);
                return true;
            }
            else
                return false;
        }

        public int RemoveAll(IEnumerable col)
        {
            int count = 0;

            foreach (object o in col)
                if (Remove(o))
                    count++;

            return count;
        }

        public void Clear()
        {
            ht.Clear();
        }

        public bool Contains(object o)
        {
            return ht.Contains(o);
        }

        public bool IsEmpty
        {
            get
            {
                return ht.Count <= 0;
            }
        }

        public int Count
        {
            get
            {
                return ht.Count;
            }
        }

        public ArrayList ToArrayList()
        {
            return new ArrayList(ht.Keys);
        }

        public object[] ToArray()
        {
            return ToArrayList().ToArray();
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Rank != 1)
                throw new ArgumentException("Array is multidimensional.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (index >= array.Length)
                throw new ArgumentException("Invalid value for index.");
            if ((index + Count) > array.Length)
                throw new ArgumentException("Array too small.");

            foreach (object o in this)
                array.SetValue(o, index++);
        }

        public Array ToArray(Type type)
        {
            return ToArrayList().ToArray(type);
        }

        public IEnumerator GetEnumerator()
        {
            return ht.Keys.GetEnumerator();
        }
    }
}
