// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
	/// <summary>
	/// Represents a collection of MenuDefinitionEntry objects.
	/// </summary>
	[Editor(typeof(MenuDefinitionEntryCollectionEditor), typeof(UITypeEditor))]
	public class MenuDefinitionEntryCollection : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of the MenuDefinitionEntryCollection class.
		/// </summary>
		public MenuDefinitionEntryCollection() 
		{
		}

		/// <summary>
		/// Initializes a new instance of the MenuDefinitionEntryCollection class.
		/// </summary>
		/// <param name="value">Command bar entry collection to initializes this command bar entry collection with.</param>
		public MenuDefinitionEntryCollection(MenuDefinitionEntryCollection value)
		{
			AddRange(value);
		}
        
		/// <summary>
		/// Initializes a new instance of the MenuDefinitionEntryCollection class.
		/// </summary>
		/// <param name="value">Array of commands to initializes this command collection with.</param>
		public MenuDefinitionEntryCollection(MenuDefinitionEntry[] value)
		{
			AddRange(value);
		}

		/// <summary>
		/// Gets or sets the command bar entry at the specified index.
		/// </summary>
		public MenuDefinitionEntry this[int index] 
		{
			get 
			{
				return (MenuDefinitionEntry)List[index];
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
		public int Add(MenuDefinitionEntry value) 
		{
			return List.Add(value);
		}
		
		/// <summary>
		/// Use strongly typed overload instead of this if possible!!
		/// </summary>
		public int Add(string commandIdentifier, bool separatorBefore, bool separatorAfter)
		{
			MenuDefinitionEntryCommand mde = new MenuDefinitionEntryCommand();
			mde.CommandIdentifier = commandIdentifier;
			mde.SeparatorBefore = separatorBefore;
			mde.SeparatorAfter = separatorAfter;
			return Add(mde);
		}

		public int Add(CommandId commandIdentifier, bool separatorBefore, bool separatorAfter)
		{
			MenuDefinitionEntryCommand mde = new MenuDefinitionEntryCommand();
			mde.CommandIdentifier = commandIdentifier.ToString();
			mde.SeparatorBefore = separatorBefore;
			mde.SeparatorAfter = separatorAfter;
			return Add(mde);
		}

		/// <summary>
		/// Adds the entries from the specified MenuDefinitionEntryCollection to the end of this MenuDefinitionEntryCollection.
		/// </summary>
		/// <param name="value">The MenuDefinitionEntryCollection to be added to the end of this MenuDefinitionEntryCollection.</param>
		public void AddRange(MenuDefinitionEntryCollection value) 
		{
			foreach (MenuDefinitionEntry commandBarEntry in value) 
				Add(commandBarEntry);
		}

		/// <summary>
		/// Adds the specified array of MenuDefinitionEntry values to the end of the MenuDefinitionEntryCollection.
		/// </summary>
		/// <param name="value">The array of MenuDefinitionEntry values to be added to the end of the MenuDefinitionEntryCollection.</param>
		public void AddRange(MenuDefinitionEntry[] value) 
		{
			foreach (MenuDefinitionEntry commandBarEntry in value) 
				this.Add(commandBarEntry);
		}

		/// <summary>
		/// Determines whether the MenuDefinitionEntryCollection contains a specific element.
		/// </summary>
		/// <param name="value">The MenuDefinitionEntry to locate in the MenuDefinitionEntryCollection.</param>
		/// <returns>true if the MenuDefinitionEntryCollection contains the specified value; otherwise, false.</returns>
		public bool Contains(MenuDefinitionEntry value) 
		{
			return List.Contains(value);
		}
        
		/// <summary>
		/// Copies the entire MenuDefinitionEntryCollection to a one-dimensional Array, starting at the
		/// specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional Array that is the destination of the elements copied from MenuDefinitionEntryCollection. The Array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(MenuDefinitionEntry[] array, int index)
		{
			List.CopyTo(array, index);
		}
        
		/// <summary>
		/// Searches for the specified MenuDefinitionEntry and returns the zero-based index of the
		/// first occurrence within the entire MenuDefinitionEntryCollection.
		/// </summary>
		/// <param name="value">The MenuDefinitionEntry to locate in the MenuDefinitionEntryCollection.</param>
		/// <returns>The zero-based index of the first occurrence of value within the entire MenuDefinitionEntryCollection, if found; otherwise, -1.</returns>
		public int IndexOf(MenuDefinitionEntry value)
		{
			return List.IndexOf(value);
		}
        
		/// <summary>
		/// Inserts an element into the MenuDefinitionEntryCollection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The MenuDefinitionEntry to insert.</param>
		public void Insert(int index, MenuDefinitionEntry value) 
		{
			List.Insert(index, value);
		}
        
		/// <summary>
		/// Returns an enumerator that can iterate through the MenuDefinitionEntryCollection.
		/// </summary>
		/// <returns>An MenuDefinitionEntryEnumerator for the MenuDefinitionEntryCollection instance.</returns>
		public new MenuDefinitionEntryEnumerator GetEnumerator() 
		{
			return new MenuDefinitionEntryEnumerator(this);
		}
        
		/// <summary>
		/// Removes the first occurrence of a specific Command from the CommandCollection.
		/// </summary>
		/// <param name="value">The Command to remove.</param>
		public void Remove(MenuDefinitionEntry value)
		{
			List.Remove(value);
		}
        
		/// <summary>
		/// Supports a simple iteration over a MenuDefinitionEntryCollection.
		/// </summary>
		public class MenuDefinitionEntryEnumerator : object, IEnumerator
		{
			/// <summary>
			/// Private data.
			/// </summary>            
			private IEnumerator baseEnumerator;
			private IEnumerable temp;
            
			/// <summary>
			/// Initializes a new instance of the MenuDefinitionEntryEnumerator class.
			/// </summary>
			/// <param name="mappings">The MenuDefinitionEntryCollection to enumerate.</param>
			public MenuDefinitionEntryEnumerator(MenuDefinitionEntryCollection mappings) 
			{
				temp = (IEnumerable)mappings;
				baseEnumerator = temp.GetEnumerator();
			}
            
			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public MenuDefinitionEntry Current 
			{
				get 
				{
					return (MenuDefinitionEntry)baseEnumerator.Current;
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
