// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// A data structure representing a set of unique objects, as
    /// determined by a specified <c>IComparer</c>.
    ///
    /// Implemented by a plain unbalanced binary search tree.
    /// </summary>
    /// <remarks>
    /// Most operations happen in O(log n) time on average, O(n)
    /// in the worst case (if the tree is completely unbalanced).
    ///
    /// <p>This implementation does no automatic balancing whatsoever.
    /// If you have many non-random items to store and performance
    /// is critical, maybe you don't want to use this implementation.</p>
    ///
    /// <p>This TreeSet always uses the comparer to determine equality.
    /// Therefore, if Equals() is not consistent with 0 == comparer(o1, o2),
    /// you may get unexpected results.</p>
    /// </remarks>
    public class TreeSet : ISet
    {
        /// <summary>
        /// The root node of the tree.  Null iif the tree is empty.
        /// </summary>
        private Node root;

        /// <summary>
        /// The comparer by which the tree will be sorted.
        /// </summary>
        private readonly IComparer comparer;

        private readonly object syncRoot = new object();

        #region public interface

        /// <summary>
        /// Initializes a TreeSet that uses the natural ordering of
        /// the elements to sort.  If items are added that do not
        /// implement IComparable, you will get errors or unpredictable
        /// results.
        /// </summary>
        public TreeSet() : this(new DefaultComparer()) { }

        /// <summary>
        /// Initializes a TreeSet using a custom comparer.
        /// </summary>
        public TreeSet(IComparer comparer)
        {
            this.comparer = comparer;
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return syncRoot; }
        }

        /// <summary>
        /// Adds a value to the set.  Returns false if the
        /// value is already in the set.
        /// </summary>
        public bool Add(object o)
        {
            return _Add(ref root, o);
        }

        /// <summary>
        /// Adds each of the items in the collection to the set.
        /// </summary>
        public int AddAll(IEnumerable col)
        {
            int count = 0;

            foreach (object o in col)
                if (_Add(ref root, o))
                    count++;

            return count;
        }

        public void Clear()
        {
            this.root = null;
        }

        /// <summary>
        /// Returns true if the object is in the set.
        /// </summary>
        public bool Contains(object o)
        {
            return _FindNode(root, o) != null;
        }

        /// <summary>
        /// Returns true if the object was in the set before
        /// the remove operation.
        ///
        /// (After the call returns, the object will not be
        /// in the set, regardless of the return value.)
        /// </summary>
        public bool Remove(object o)
        {
            return _Delete(ref root, o);
        }

        /// <summary>
        /// Removes all the elements of the given tree
        /// from this tree, if they are present.
        ///
        /// Returns the number of items actually found
        /// and removed.
        /// </summary>
        public int RemoveAll(IEnumerable col)
        {
            int count = 0;

            foreach (object o in col)
                if (_Delete(ref root, o))
                    count++;

            return count;
        }

        /// <summary>
        /// Returns true if the set contains no elements.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return root == null;
            }
        }

        /// <summary>
        /// Returns the number of items in the set.
        /// If you only care about whether the set is
        /// empty, use the (much cheaper) IsEmpty
        /// property instead.
        /// </summary>
        public int Count
        {
            get
            {
                return _Count(root);
            }
        }

        /// <summary>
        /// Get an enumerator.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return _ToArrayList().GetEnumerator();
        }

        /// <summary>
        /// Returns the contents of the set as an ArrayList,
        /// in sorted order.
        /// </summary>
        public ArrayList ToArrayList()
        {
            return _ToArrayList();
        }

        /// <summary>
        /// Returns the contents of the set as an array,
        /// in sorted order.
        /// </summary>
        public object[] ToArray()
        {
            return _ToArrayList().ToArray();
        }

        /// <summary>
        /// Returns the contents of the set as an array of
        /// the given type, in sorted order.
        /// </summary>
        public Array ToArray(Type type)
        {
            return _ToArrayList().ToArray(type);
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

        #endregion

        #region internal classes

        /// <summary>
        /// Tree node data structure.
        /// </summary>
        internal class Node
        {
            internal Node(object o)
            {
                this.Value = o;
            }

            internal object Value;
            internal Node Left;
            internal Node Right;
        }

        /// <summary>
        /// Compares items according to their natural order.
        /// </summary>
        internal class DefaultComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((IComparable)x).CompareTo(y);
            }
        }

        #endregion

        #region internal operations

        // Return values for _Compare.
        // These are ints instead of enums so they can
        // be used in case labels.
        private const int LT = -1;   // o1 < o2
        private const int GT = 1;    // o1 > o2
        private const int EQ = 0;    // o1 == o2

        /// <summary>
        /// Returns -1 if o1 less than o2,
        /// 1 if o1 greater than o2,
        /// 0 if they are equal.
        /// Use the above LT, GT, EQ constants for legibility.
        /// </summary>
        private int _Compare(object o1, object o2)
        {
            int result = comparer.Compare(o1, o2);
            return Math.Max(-1, Math.Min(1, result));
        }

        /// <summary>
        /// Add the value somewhere below this node.
        /// Returns false if the value already existed.
        /// </summary>
        private bool _Add(ref Node node, object val)
        {
            if (node == null)
            {
                node = new Node(val);
                return true;
            }

            switch (_Compare(val, node.Value))
            {
                // values are equal; value already exists in tree, nothing to do
                case EQ:
                    return false;

                // new value is less; recurse left.
                case LT:
                    return _Add(ref node.Left, val);

                // new value is greater; recurse right.
                case GT:
                    return _Add(ref node.Right, val);

                default:
                    Debug.Fail("Coding error in TreeSet");
                    return false;
            }
        }

        /// <summary>
        /// Finds a matching node in the subtree of "node".
        /// Returns null if not found.
        /// </summary>
        private Node _FindNode(Node node, object val)
        {
            if (node == null)
                return null;

            switch (_Compare(val, node.Value))
            {
                case EQ:
                    // node was found!
                    return node;

                case LT:
                    // not found, recurse left
                    return _FindNode(node.Left, val);

                case GT:
                    // not found, recurse right
                    return _FindNode(node.Right, val);

                default:
                    Debug.Fail("Coding error in TreeSet");
                    return null;
            }
        }

        /// <summary>
        /// Returns the number of nodes in the subtree of node.
        /// Call on root to return the number of nodes in the
        /// whole tree.
        /// </summary>
        private int _Count(Node node)
        {
            if (node == null)
                return 0;
            else
                return 1 + _Count(node.Left) + _Count(node.Right);
        }

        /// <summary>
        /// Adds the items in the subtree of node to the list,
        /// sorted by the comparer order.
        /// </summary>
        private void _Accumulate(Node node, ArrayList list)
        {
            if (node == null)
                return;

            _Accumulate(node.Left, list);
            list.Add(node.Value);
            _Accumulate(node.Right, list);
        }

        /// <summary>
        /// Copies the set contents to a new array list.
        /// </summary>
        private ArrayList _ToArrayList()
        {
            ArrayList list = new ArrayList(_Count(root));
            _Accumulate(root, list);
            return list;
        }

        /// <summary>
        /// Deletes a value from the subtree of node, and returns
        /// a (possibly new) subtree that the caller should use to
        /// replace node. The new subtree may be null.
        ///
        /// Iif the value was found and deleted, the out parameter
        /// "found" will be true.
        /// </summary>
        private bool _Delete(ref Node node, object val)
        {
            if (node == null)
                return false;

            switch (_Compare(val, node.Value))
            {
                case EQ:
                    // value was found.  delete the node.
                    _DeleteNode(ref node);
                    return true;

                case LT:
                    // recurse left.
                    return _Delete(ref node.Left, val);

                case GT:
                    // recurse right.
                    return _Delete(ref node.Right, val);

                default:
                    Debug.Fail("Coding error in TreeSet");
                    return false;
            }
        }

        /// <summary>
        /// Deletes a node.
        /// </summary>
        private void _DeleteNode(ref Node node)
        {
            if (node.Left == null)
            {
                // left child node is not present.
                // Use the right child node to replace
                // this node, even if the right child
                // node is null.
                node = node.Right;
            }
            else if (node.Right == null)
            {
                // left child node is present, right is
                // not.  Use the left child node to replace
                // this node.
                node = node.Left;
            }
            else
            {
                // Both left and right are present.  Rather than
                // removing this node, we're going to replace this
                // node's value with the maximum value contained in
                // the left subtree, and then delete that value from
                // the left subtree.  This will ensure that the
                // properties of the binary search tree are preserved.

                Node curr = node.Left;  // this is not null--we checked earlier
                Node parent = null;
                while (curr.Right != null)
                {
                    parent = curr;
                    curr = curr.Right;
                }

                node.Value = curr.Value;  // found the max value; copy it into this node

                // Delete the reference from whence it came.
                // If last is null, the value came from the direct left child.

                if (parent == null)
                    _DeleteNode(ref node.Left);
                else
                    _DeleteNode(ref parent.Right);
            }
        }
        #endregion


#if TEST
        /// <summary>
        /// Brute-force correctness test of insert/delete operations.
        /// </summary>
        public static void Test()
        {
            TreeSet ts = new TreeSet();
            int count = 0;

            for (int loops = 0; ; loops++)
            {
                count += ts.AddAll(RandomArrayList(100000 - ts.Count));

                InOrder(ts);

                ArrayList data = ts.ToArrayList();

                RandomizeOrder(ref data);

                for (int i = 0; i < 50000 && i < data.Count; i++)
                {
                    if (ts.Remove(data[i]))
                        count--;
                }

                Debug.Assert(count == ts.Count);
                InOrder(ts);

                if ((loops % 10) == 0)
                Console.WriteLine("So far so good: " + loops + " " + count);
            }
        }

        public static void InOrder(TreeSet tree)
        {
            object last = null;
            foreach (object o in tree)
            {
                if (last != null)
                    Debug.Assert(((IComparable)last).CompareTo(o) < 0, "Out of order");
            }
        }

        public static void RandomizeOrder(ref ArrayList list)
        {
            Hashtable table = new Hashtable(list.Count);
            foreach (object o in list)
                table[o] = string.Empty;
            list = new ArrayList(table.Keys);
        }

        public static ArrayList RandomArrayList(int size)
        {
            Random r = new Random();

            ArrayList al = new ArrayList(size);
            for (int i = 0; i < size; i++)
                al.Add(r.Next());

            return al;
        }
#endif
    }
}
