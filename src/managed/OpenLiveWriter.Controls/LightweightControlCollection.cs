// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// A collection that stores <see cref='OpenLiveWriter.Controls.LightweightControl'/> objects.
    /// </summary>
    /// <seealso cref='OpenLiveWriter.Controls.LightweightControlCollection'/>
    [Serializable()]
    public class LightweightControlCollection : CollectionBase
    {
        /// <summary>
        ///	The lightweight control container that owns this lightweight control collection.
        /// </summary>
        private ILightweightControlContainerControl owner;

        /// <summary>
        ///	Initializes a new instance of <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/>.
        /// </summary>
        /// <param name='owner'>The <see cref='OpenLiveWriter.Controls.ILightweightControlContainerControl'/> that is <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> is providing service for.</param>
        public LightweightControlCollection(ILightweightControlContainerControl owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Initializes a new instance of <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> based on another <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/>.
        /// </summary>
        /// <param name='value'>A <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> from which the contents are copied.</param>
        public LightweightControlCollection(LightweightControlCollection value)
        {
            AddRange(value);
        }

        /// <summary>
        ///	Initializes a new instance of <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> containing any array of <see cref='OpenLiveWriter.Controls.LightweightControl'/> objects.
        /// </summary>
        /// <param name='value'>
        /// A array of <see cref='OpenLiveWriter.Controls.LightweightControl'/> objects with which to initialize the collection
        /// </param>
        public LightweightControlCollection(LightweightControl[] value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Represents the entry at the specified index of the <see cref='OpenLiveWriter.Controls.LightweightControl'/>.
        /// </summary>
        /// <param name='index'>The zero-based index of the entry to locate in the collection.</param>
        /// <value>The entry at the specified index of the collection.</value>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
        public LightweightControl this[int index]
        {
            get
            {
                return ((LightweightControl)(List[index]));
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Adds a <see cref='OpenLiveWriter.Controls.LightweightControl'/> with the specified value to the
        /// <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> .
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.Controls.LightweightControl'/> to add.</param>
        /// <returns>The index at which the new element was inserted.</returns>
        /// <seealso cref='OpenLiveWriter.Controls.LightweightControlCollection.AddRange'/>
        public int Add(LightweightControl value)
        {
            //	Add.
            int index = List.Add(value);

            //	Set the lightweight control container of the lightweight control to the owner of
            //	this lightweight control collection.
            value.LightweightControlContainerControl = owner;

            //	Force the lightweight control to apply layout logic to child controls.
            value.PerformLayout();

            //	Force the owner of this lightweight control collection to apply layout logic to child controls.
            owner.PerformLayout();

            //	Return index.
            return index;
        }

        /// <summary>
        /// <para>Copies the elements of an array to the end of the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/>.</para>
        /// </summary>
        /// <param name='value'>An array of type <see cref='OpenLiveWriter.Controls.LightweightControl'/> containing the objects to add to the collection.</param>
        /// <seealso cref='OpenLiveWriter.Controls.LightweightControlCollection.Add'/>
        public void AddRange(LightweightControl[] value)
        {
            for (int i = 0; i < value.Length; i++)
                this.Add(value[i]);
        }

        /// <summary>
        /// Adds the contents of another <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> to the end of the collection.
        /// </summary>
        /// <param name='value'>A <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> containing the objects to add to the collection.</param>
        /// <seealso cref='OpenLiveWriter.Controls.LightweightControlCollection.Add'/>
        public void AddRange(LightweightControlCollection value)
        {
            for (int i = 0; i < value.Count; i++)
                this.Add(value[i]);
        }

        /// <summary>
        /// Clears the lightweight control collection.
        /// </summary>
        public new void Clear()
        {
            //	Make a working copy of the lightweight control list.
            LightweightControl[] lightweightControls = new LightweightControl[Count];
            CopyTo(lightweightControls, 0);

            //	Remove each lightweight control.
            foreach (LightweightControl lightweightControl in lightweightControls)
            {
                lightweightControl.LightweightControlContainerControl = null;
                lightweightControl.Dispose();
            }

            //	Force the owner to apply layout logic to child controls.
            owner.PerformLayout();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> contains the specified <see cref='OpenLiveWriter.Controls.LightweightControl'/>.
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.Controls.LightweightControl'/> to locate.</param>
        /// <returns><see langword='true'/> if the <see cref='OpenLiveWriter.Controls.LightweightControl'/> is contained in the collection; otherwise, <see langword='false'/>.</returns>
        /// <seealso cref='OpenLiveWriter.Controls.LightweightControlCollection.IndexOf'/>
        public bool Contains(LightweightControl value)
        {
            return List.Contains(value);
        }

        /// <summary>
        /// Copies the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the specified index.
        /// </summary>
        /// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> .</para></param>
        /// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
        /// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
        /// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
        /// <seealso cref='System.Array'/>
        public void CopyTo(LightweightControl[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        /// Returns the index of a <see cref='OpenLiveWriter.Controls.LightweightControl'/> in the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/>.
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.Controls.LightweightControl'/> to locate.</param>
        /// <returns>The index of the <see cref='OpenLiveWriter.Controls.LightweightControl'/> of <paramref name='value'/> in the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/>, if found; otherwise, -1.</returns>
        /// <seealso cref='OpenLiveWriter.Controls.LightweightControlCollection.Contains'/>
        public int IndexOf(LightweightControl value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// Inserts a <see cref='OpenLiveWriter.Controls.LightweightControl'/> into the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> at the specified index.
        /// </summary>
        /// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
        /// <param name=' value'>The <see cref='OpenLiveWriter.Controls.LightweightControl'/> to insert.</param>
        /// <seealso cref='OpenLiveWriter.Controls.LightweightControlCollection.Add'/>
        public void Insert(int index, LightweightControl value)
        {
            //	Insert.
            List.Insert(index, value);

            //	Set the lightweight control container of the lightweight control to the owner of
            //	this lightweight control collection.
            value.LightweightControlContainerControl = owner;

            //	Force the lightweight control to apply layout logic to child controls.
            value.PerformLayout();

            //	Force the owner of this lightweight control collection to apply layout logic to child controls.
            owner.PerformLayout();
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> .
        /// </summary>
        /// <seealso cref='System.Collections.IEnumerator'/>
        public new LightweightControlEnumerator GetEnumerator()
        {
            return new LightweightControlEnumerator(this);
        }

        /// <summary>
        /// Removes a specific <see cref='OpenLiveWriter.Controls.LightweightControl'/> from the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> .
        /// </summary>
        /// <param name='value'>The <see cref='OpenLiveWriter.Controls.LightweightControl'/> to remove from the <see cref='OpenLiveWriter.Controls.LightweightControlCollection'/> .</param>
        /// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
        public void Remove(LightweightControl value)
        {
            //	Remove the lightweight control.
            List.Remove(value);

            //	Set the lightweight control container of the lightweight control.
            value.LightweightControlContainerControl = null;

            //	Force the owner of this lightweight control collection to apply layout logic to child controls.
            owner.PerformLayout();
        }

        /// <summary>
        /// Brings the lightweight control to the front of the z-order.
        /// </summary>
        /// <param name="value"></param>
        public void BringToFront(LightweightControl value)
        {
            //	Remove the lightweight control from the list.
            List.Remove(value);

            //	Add the lightweight control back to the list, now as the front control in the z-order.
            List.Add(value);

            //	Force the owner of this lightweight control collection to apply layout logic to child controls.
            owner.PerformLayout();
        }

        /// <summary>
        /// Enumerator.
        /// </summary>
        public class LightweightControlEnumerator : object, IEnumerator
        {
            private IEnumerator baseEnumerator;
            private IEnumerable temp;

            /// <summary>
            /// Initializes a new instance of the LightweightControlEnumerator class.
            /// </summary>
            /// <param name="mappings"></param>
            public LightweightControlEnumerator(LightweightControlCollection mappings)
            {
                this.temp = ((IEnumerable)(mappings));
                this.baseEnumerator = temp.GetEnumerator();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public LightweightControl Current
            {
                get
                {
                    return ((LightweightControl)(baseEnumerator.Current));
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
