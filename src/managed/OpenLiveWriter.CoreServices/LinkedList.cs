// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Singly linked list.  Count and Add are constant-time operations.
    /// Insert, Remove, Contains, IndexOf, and [] are bound by O(n).
    /// </summary>
    [Serializable]
    public class LinkedList : IList
    {
        private HeadNode head = new HeadNode();
        private Node tail;
        private int count = 0;

        public LinkedList()
        {
            tail = head;
        }

        internal int sequenceNum = 0;

        private void Modify()
        {
            sequenceNum++;
        }

        #region IList Members

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public object this[int index]
        {
            get
            {
                return FindNode(index).Value;
            }
            set
            {
                Modify();

                Node parent = FindNode(index - 1);
                Node oldVal = parent.Next;
                Node newVal = new Node(value);
                parent.Next = newVal;
                if (oldVal != null && oldVal.Next != null)
                    newVal.Next = oldVal.Next;
                if (newVal.Next == null)
                    this.tail = newVal;
            }
        }

        private Node FindNode(int index)
        {
            Node thisNode = head;
            try
            {
                for (int i = 0; i < index + 1; i++)
                {
                    thisNode = thisNode.Next;
                }
            }
            catch (NullReferenceException)
            {
                throw new IndexOutOfRangeException();
            }
            return thisNode;
        }

        public void RemoveAt(int index)
        {
            Modify();

            Node parent = FindNode(index - 1);
            Node oldVal = parent.Next;
            if (oldVal == null)
                throw new IndexOutOfRangeException();
            parent.Next = oldVal.Next;
            if (parent.Next == null)
                this.tail = parent;
            count--;
        }

        public void Insert(int index, object val)
        {
            Modify();

            Node parent = FindNode(index - 1);
            Node oldVal = parent.Next;
            Node newVal = new Node(val);
            parent.Next = newVal;
            newVal.Next = oldVal;
            if (newVal.Next == null)
                this.tail = newVal;
            count++;
        }

        public void Remove(object val)
        {
            Modify();

            Node parent;
            Node target;
            int index;
            head.Find(val, out parent, out target, out index);
            if (target == null)
                return;

            parent.Next = target.Next;
            if (parent.Next == null)
                this.tail = parent;
            count--;
        }

        public bool Contains(object val)
        {
            Node parent;
            Node target;
            int index;
            head.Find(val, out parent, out target, out index);
            return (target != null);
        }

        public void Clear()
        {
            Modify();

            head.Next = null;
            this.tail = this.head;
            count = 0;
        }

        public int IndexOf(object val)
        {
            Node parent;
            Node target;
            int index;
            head.Find(val, out parent, out target, out index);
            if (target == null)
                return -1;
            return index;
        }

        public int Add(object val)
        {
            Modify();

            Node newNode = new Node(val);
            this.tail.Next = newNode;
            this.tail = newNode;
            this.count++;
            return this.count;
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region ICollection Members

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public void CopyTo(Array array, int index)
        {
            Node thisNode = head;
            while (null != (thisNode = thisNode.Next))
            {
                array.SetValue(thisNode.Value, index++);
            }
        }

        public object SyncRoot
        {
            get
            {
                return head;
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new LLEnum(this);
        }

        private class LLEnum : IEnumerator
        {

            private int seqnum;
            private LinkedList list;
            private Node currNode;
            private bool atHead = true;

            public LLEnum(LinkedList list)
            {
                seqnum = list.sequenceNum;
                this.list = list;
                this.currNode = list.head;
            }

            private void EnsureUnchanged()
            {
                if (seqnum != list.sequenceNum)
                    throw new InvalidOperationException("Underlying list has changed");
            }

            #region IEnumerator Members

            public void Reset()
            {
                EnsureUnchanged();
                currNode = list.head;
                atHead = true;
            }

            public object Current
            {
                get
                {
                    EnsureUnchanged();
                    if (atHead)
                        throw new IndexOutOfRangeException();
                    return currNode.Value;
                }
            }

            public bool MoveNext()
            {
                EnsureUnchanged();
                currNode = currNode.Next;
                return !(atHead = !(currNode != null));
            }

            #endregion

        }

        #endregion

        #region Internal data structures
        [Serializable]
        private class Node
        {
            object val;
            Node next;

            public Node()
            {
            }

            public Node(object val)
            {
                this.val = val;
            }

            public Node Next
            {
                get
                {
                    return next;
                }
                set
                {
                    this.next = value;
                }
            }

            public virtual object Value
            {
                get
                {
                    return val;
                }
                set
                {
                    this.val = value;
                }
            }

            protected Node FindParent(object val, ref int depth)
            {
                depth++;
                if (this.Next == null)
                    return null;
                else if (this.Next.Value.Equals(val))
                    return this;
                else
                    return this.Next.FindParent(val, ref depth);
            }

            protected int Count(int index)
            {
                if (this.Next == null)
                    return ++index;
                else
                    return this.Next.Count(++index);
            }
        }

        [Serializable]
        private class HeadNode : Node
        {
            public override object Value
            {
                get
                {
                    //Debug.Fail("Get head value");
                    return null;
                }
                set
                {
                    //Debug.Fail("Set head value");
                }
            }

            public void Find(object val, out Node parent, out Node result, out int index)
            {
                int idx = -1;
                Node parentNode = this.FindParent(val, ref idx);
                index = idx;
                if (parentNode == null)
                {
                    parent = null;
                    result = null;
                }
                else
                {
                    parent = parentNode;
                    result = parent.Next;
                }
            }

            public int Count()
            {
                return this.Count(-1);
            }
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (object o in this)
            {
                sb.Append(o.ToString());
                sb.Append(";");
            }
            sb.Append("}");
            return sb.ToString();
        }

#if FALSE
        public string String
        {
            get
            {
                return this.ToString();
            }
        }
#endif
    }
}
