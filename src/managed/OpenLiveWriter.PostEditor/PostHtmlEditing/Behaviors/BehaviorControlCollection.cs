// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// A collection that stores <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> objects.
    /// </summary>
    /// <seealso cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/>
    [Serializable()]
    public class BehaviorControlCollection : CollectionBase
    {
        public delegate void ControlEvent(BehaviorControl c);
        public event ControlEvent ControlAdded;
        public event ControlEvent ControlRemoved;

        /// <summary>
        ///	The Behavior control container that owns this Behavior control collection.
        /// </summary>
        private IBehaviorControlContainerControl owner;

        /// <summary>
        ///	Initializes a new instance of <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/>.
        /// </summary>
        /// <param name='owner'>The <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.IBehaviorControlContainerControl'/> that is <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> is providing service for.</param>
        public BehaviorControlCollection(IBehaviorControlContainerControl owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Initializes a new instance of <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> based on another <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/>.
        /// </summary>
        /// <param name='value'>A <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> from which the contents are copied.</param>
        public BehaviorControlCollection(BehaviorControlCollection value)
        {
            AddRange(value);
        }

        /// <summary>
        ///	Initializes a new instance of <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> containing any array of <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> objects.
        /// </summary>
        /// <param name='value'>
        /// A array of <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> objects with which to initialize the collection
        /// </param>
        public BehaviorControlCollection(BehaviorControl[] value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Represents the entry at the specified index of the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/>.
        /// </summary>
        /// <param name='index'>The zero-based index of the entry to locate in the collection.</param>
        /// <value>The entry at the specified index of the collection.</value>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
        public BehaviorControl this[int index]
        {
            get
            {
                return ((BehaviorControl)(List[index]));
            }
            set
            {
                OnControlAdded(value);
                List[index] = value;
            }
        }

        /// <summary>
        /// Adds a <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> with the specified value to the
        /// <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> .
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> to add.</param>
        /// <returns>The index at which the new element was inserted.</returns>
        /// <seealso cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection.AddRange'/>
        public int Add(BehaviorControl value)
        {
            //	Add.
            int index = List.Add(value);

            //	Set the Behavior control container of the Behavior control to the owner of
            //	this Behavior control collection.
            value.ContainerControl = owner;

            OnControlAdded(value);

            //	Force the Behavior control to apply layout logic to child controls.
            value.PerformLayout();

            //	Force the owner of this Behavior control collection to apply layout logic to child controls.
            owner.PerformLayout();

            //	Return index.
            return index;
        }

        /// <summary>
        /// <para>Copies the elements of an array to the end of the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/>.</para>
        /// </summary>
        /// <param name='value'>An array of type <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> containing the objects to add to the collection.</param>
        /// <seealso cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection.Add'/>
        public void AddRange(BehaviorControl[] value)
        {
            for (int i = 0; i < value.Length; i++)
                this.Add(value[i]);
        }

        /// <summary>
        /// Adds the contents of another <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> to the end of the collection.
        /// </summary>
        /// <param name='value'>A <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> containing the objects to add to the collection.</param>
        /// <seealso cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection.Add'/>
        public void AddRange(BehaviorControlCollection value)
        {
            for (int i = 0; i < value.Count; i++)
                this.Add(value[i]);
        }

        /// <summary>
        /// Clears the Behavior control collection.
        /// </summary>
        public new void Clear()
        {
            //	Make a working copy of the Behavior control list.
            BehaviorControl[] BehaviorControls = new BehaviorControl[Count];
            CopyTo(BehaviorControls, 0);

            //	Remove each Behavior control.
            foreach (BehaviorControl BehaviorControl in BehaviorControls)
            {
                OnControlRemoved(BehaviorControl);
                BehaviorControl.ContainerControl = null;
                BehaviorControl.Dispose();
            }

            //	Force the owner to apply layout logic to child controls.
            owner.PerformLayout();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> contains the specified <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/>.
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> to locate.</param>
        /// <returns><see langword='true'/> if the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> is contained in the collection; otherwise, <see langword='false'/>.</returns>
        /// <seealso cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection.IndexOf'/>
        public bool Contains(BehaviorControl value)
        {
            return List.Contains(value);
        }

        /// <summary>
        /// Copies the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the specified index.
        /// </summary>
        /// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> .</para></param>
        /// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
        /// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> is greater than the available space between <paramref name='index'/> and the end of <paramref name='array'/>.</para></exception>
        /// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is less than <paramref name='array'/>'s lowbound. </exception>
        /// <seealso cref='System.Array'/>
        public void CopyTo(BehaviorControl[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        /// Returns the index of a <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> in the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/>.
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> to locate.</param>
        /// <returns>The index of the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> of <paramref name='value'/> in the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/>, if found; otherwise, -1.</returns>
        /// <seealso cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection.Contains'/>
        public int IndexOf(BehaviorControl value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// Inserts a <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> into the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> at the specified index.
        /// </summary>
        /// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
        /// <param name=' value'>The <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> to insert.</param>
        /// <seealso cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection.Add'/>
        public void Insert(int index, BehaviorControl value)
        {
            //	Insert.
            List.Insert(index, value);

            OnControlAdded(value);

            //	Set the Behavior control container of the Behavior control to the owner of
            //	this Behavior control collection.
            value.ContainerControl = owner;

            //	Force the Behavior control to apply layout logic to child controls.
            value.PerformLayout();

            //	Force the owner of this Behavior control collection to apply layout logic to child controls.
            owner.PerformLayout();
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> .
        /// </summary>
        /// <seealso cref='System.Collections.IEnumerator'/>
        public new BehaviorControlEnumerator GetEnumerator()
        {
            return new BehaviorControlEnumerator(this);
        }

        /// <summary>
        /// Removes a specific <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> from the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> .
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControl'/> to remove from the <see cref='OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors.BehaviorControlCollection'/> .</param>
        /// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
        public void Remove(BehaviorControl value)
        {
            //	Remove the Behavior control.
            List.Remove(value);

            //	Set the Behavior control container of the Behavior control.
            value.ContainerControl = null;

            OnControlRemoved(value);

            //	Force the owner of this Behavior control collection to apply layout logic to child controls.
            owner.PerformLayout();
        }

        /// <summary>
        /// Brings the Behavior control to the front of the z-order.
        /// </summary>
        /// <param name="value"></param>
        public void BringToFront(BehaviorControl value)
        {
            //	Remove the Behavior control from the list.
            List.Remove(value);

            //	Add the Behavior control back to the list, now as the front control in the z-order.
            List.Add(value);

            //	Force the owner of this Behavior control collection to apply layout logic to child controls.
            owner.PerformLayout();
        }

        private void OnControlAdded(BehaviorControl c)
        {
            if (ControlAdded != null)
            {
                ControlAdded(c);
            }
        }

        private void OnControlRemoved(BehaviorControl c)
        {
            if (ControlAdded != null)
            {
                ControlRemoved(c);
            }
        }

        /// <summary>
        /// Enumerator.
        /// </summary>
        public class BehaviorControlEnumerator : object, IEnumerator
        {
            private IEnumerator baseEnumerator;
            private IEnumerable temp;

            /// <summary>
            /// Initializes a new instance of the BehaviorControlEnumerator class.
            /// </summary>
            /// <param name="mappings"></param>
            public BehaviorControlEnumerator(BehaviorControlCollection mappings)
            {
                this.temp = mappings;
                this.baseEnumerator = temp.GetEnumerator();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public BehaviorControl Current
            {
                get
                {
                    return ((BehaviorControl)(baseEnumerator.Current));
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return baseEnumerator.Current;
                }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                return baseEnumerator.MoveNext();
            }

            bool IEnumerator.MoveNext()
            {
                return baseEnumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                baseEnumerator.Reset();
            }

            void IEnumerator.Reset()
            {
                baseEnumerator.Reset();
            }
        }
    }
}
