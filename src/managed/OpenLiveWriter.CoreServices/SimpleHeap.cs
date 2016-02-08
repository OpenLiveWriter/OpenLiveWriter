// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.CoreServices
{
    public class SimpleHeap
    {
        private ArrayList list;
        public IComparer comparer;

        public SimpleHeap(IComparer comparer)
        {
            this.comparer = comparer;
            this.list = new ArrayList();
        }

        public SimpleHeap() : this(null)
        {
        }

        public virtual void Push(object o)
        {
            if (o == null)
                throw new ArgumentNullException("o");

            list.Add(o);
            list.Sort(comparer);
        }

        public virtual object Peek()
        {
            if (list.Count == 0)
                return null;
            else
                return list[0];
        }

        public virtual object Pop()
        {
            if (list.Count == 0)
                return null;
            else
            {
                object o = list[0];
                list.RemoveAt(0);
                return o;
            }
        }

        public virtual int Count
        {
            get { return list.Count; }
        }
    }
}
