// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Represents a collection of CommandBarEntry objects.
    /// </summary>
    [Editor(typeof(CommandBarEntryCollectionEditor), typeof(UITypeEditor))]
    public class CommandBarEntryCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new instance of the CommandBarEntryCollection class.
        /// </summary>
        public CommandBarEntryCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarEntryCollection class.
        /// </summary>
        /// <param name="value">Command bar entry collection to initializes this command bar entry collection with.</param>
        public CommandBarEntryCollection(CommandBarEntryCollection value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarEntryCollection class.
        /// </summary>
        /// <param name="value">Array of commands to initializes this command collection with.</param>
        public CommandBarEntryCollection(CommandBarEntry[] value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Gets or sets the command bar entry at the specified index.
        /// </summary>
        public CommandBarEntry this[int index]
        {
            get
            {
                return (CommandBarEntry)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Adds the specified command bar entry to the end of the command bar entry collection.
        /// </summary>
        /// <param name="value">The command bar entry to be added to the end of the command bar entry collection.</param>
        /// <returns>The index at which the command bar entry has been added.</returns>
        public int Add(CommandBarEntry value)
        {
            return List.Add(value);
        }

        /// <summary>
        /// Adds the entries from the specified CommandBarEntryCollection to the end of this CommandBarEntryCollection.
        /// </summary>
        /// <param name="value">The CommandBarEntryCollection to be added to the end of this CommandBarEntryCollection.</param>
        public void AddRange(CommandBarEntryCollection value)
        {
            foreach (CommandBarEntry commandBarEntry in value)
                Add(commandBarEntry);
        }

        /// <summary>
        /// Adds the specified array of CommandBarEntry values to the end of the CommandBarEntryCollection.
        /// </summary>
        /// <param name="value">The array of CommandBarEntry values to be added to the end of the CommandBarEntryCollection.</param>
        public void AddRange(CommandBarEntry[] value)
        {
            foreach (CommandBarEntry commandBarEntry in value)
                this.Add(commandBarEntry);
        }

        /// <summary>
        /// Determines whether the CommandBarEntryCollection contains a specific element.
        /// </summary>
        /// <param name="value">The CommandBarEntry to locate in the CommandBarEntryCollection.</param>
        /// <returns>true if the CommandBarEntryCollection contains the specified value; otherwise, false.</returns>
        public bool Contains(CommandBarEntry value)
        {
            return List.Contains(value);
        }

        /// <summary>
        /// Copies the entire CommandBarEntryCollection to a one-dimensional Array, starting at the
        /// specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from CommandBarEntryCollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(CommandBarEntry[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        /// Searches for the specified CommandBarEntry and returns the zero-based index of the
        /// first occurrence within the entire CommandBarEntryCollection.
        /// </summary>
        /// <param name="value">The CommandBarEntry to locate in the CommandBarEntryCollection.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire CommandBarEntryCollection, if found; otherwise, -1.</returns>
        public int IndexOf(CommandBarEntry value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the CommandBarEntryCollection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The CommandBarEntry to insert.</param>
        public void Insert(int index, CommandBarEntry value)
        {
            List.Insert(index, value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the CommandBarEntryCollection.
        /// </summary>
        /// <returns>An CommandBarEntryEnumerator for the CommandBarEntryCollection instance.</returns>
        public new CommandBarEntryEnumerator GetEnumerator()
        {
            return new CommandBarEntryEnumerator(this);
        }

        /// <summary>
        /// Removes the first occurrence of a specific Command from the CommandCollection.
        /// </summary>
        /// <param name="value">The Command to remove.</param>
        public void Remove(CommandBarEntry value)
        {
            List.Remove(value);
        }

        /// <summary>
        /// Supports a simple iteration over a CommandBarEntryCollection.
        /// </summary>
        public class CommandBarEntryEnumerator : object, IEnumerator
        {
            /// <summary>
            /// Private data.
            /// </summary>
            private IEnumerator baseEnumerator;
            private IEnumerable temp;

            /// <summary>
            /// Initializes a new instance of the CommandBarEntryEnumerator class.
            /// </summary>
            /// <param name="mappings">The CommandBarEntryCollection to enumerate.</param>
            public CommandBarEntryEnumerator(CommandBarEntryCollection mappings)
            {
                temp = (IEnumerable)mappings;
                baseEnumerator = temp.GetEnumerator();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public CommandBarEntry Current
            {
                get
                {
                    return (CommandBarEntry)baseEnumerator.Current;
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
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
            public bool MoveNext()
            {
                return baseEnumerator.MoveNext();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
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

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            void IEnumerator.Reset()
            {
                baseEnumerator.Reset();
            }
        }
    }
}
