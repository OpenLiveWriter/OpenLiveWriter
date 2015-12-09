// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Represents a collection of commands.
    /// </summary>
    [Editor(typeof(CommandCollectionEditor), typeof(UITypeEditor))]
    public class CommandCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new instance of the CommandCollection class.
        /// </summary>
        public CommandCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CommandCollection class.
        /// </summary>
        /// <param name="value">Command collection to initializes this command collection with.</param>
        public CommandCollection(CommandCollection value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the CommandCollection class.
        /// </summary>
        /// <param name="value">Array of commands to initializes this command collection with.</param>
        public CommandCollection(Command[] value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Gets or sets the command at the specified index.
        /// </summary>
        public Command this[int index]
        {
            get
            {
                return (Command)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Adds the specified command to the end of the command collection.
        /// </summary>
        /// <param name="value">The command to be added to the end of the command collection.</param>
        /// <returns>The index at which the command has been added.</returns>
        public int Add(Command value)
        {
            return List.Add(value);
        }

        /// <summary>
        /// Adds the entries from the specified CommandCollection to the end of this CommandCollection.
        /// </summary>
        /// <param name="value">The CommandCollection to be added to the end of this CommandCollection.</param>
        public void AddRange(CommandCollection value)
        {
            foreach (Command command in value)
                Add(command);
        }

        /// <summary>
        /// Adds the specified array of Command values to the end of the CommandCollection.
        /// </summary>
        /// <param name="value">The array of Command values to be added to the end of the CommandCollection.</param>
        public void AddRange(Command[] value)
        {
            foreach (Command command in value)
                Add(command);
        }

        /// <summary>
        /// Determines whether the CommandCollection contains a specific element.
        /// </summary>
        /// <param name="value">The Command to locate in the CommandCollection.</param>
        /// <returns>true if the CommandCollection contains the specified value; otherwise, false.</returns>
        public bool Contains(Command value)
        {
            return List.Contains(value);
        }

        /// <summary>
        /// Copies the entire CommandCollection to a one-dimensional Array, starting at the
        /// specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from CommandCollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Command[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        /// Searches for the specified Command and returns the zero-based index of the first
        /// occurrence within the entire CommandCollection.
        /// </summary>
        /// <param name="value">The Command to locate in the CommandCollection.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire CommandCollection, if found; otherwise, -1.</returns>
        public int IndexOf(Command value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the CommandCollection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Command to insert.</param>
        public void Insert(int index, Command value)
        {
            List.Insert(index, value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the CommandCollection.
        /// </summary>
        /// <returns>An CommandEnumerator for the CommandCollection instance.</returns>
        public new CommandEnumerator GetEnumerator()
        {
            return new CommandEnumerator(this);
        }

        /// <summary>
        /// Removes the first occurrence of a specific Command from the CommandCollection.
        /// </summary>
        /// <param name="value">The Command to remove.</param>
        public void Remove(Command value)
        {
            List.Remove(value);
        }

        /// <summary>
        /// Supports a simple iteration over a CommandCollection.
        /// </summary>
        public class CommandEnumerator : object, IEnumerator
        {
            /// <summary>
            /// Private data.
            /// </summary>
            private IEnumerator baseEnumerator;
            private IEnumerable temp;

            /// <summary>
            /// Initializes a new instance of the CommandEnumerator class.
            /// </summary>
            /// <param name="mappings">The CommandCollection to enumerate.</param>
            public CommandEnumerator(CommandCollection mappings)
            {
                temp = (IEnumerable)mappings;
                baseEnumerator = temp.GetEnumerator();
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public Command Current
            {
                get
                {
                    return (Command)baseEnumerator.Current;
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
