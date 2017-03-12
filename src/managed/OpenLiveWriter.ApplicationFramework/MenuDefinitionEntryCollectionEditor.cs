// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel.Design;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a user interface for editing a MenuDefinitionEntryCollection.
    /// </summary>
    internal class MenuDefinitionEntryCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the MenuDefinitionEntryCollectionEditor.
        /// </summary>
        public MenuDefinitionEntryCollectionEditor() : base(typeof(MenuDefinitionEntryCollection))
        {
        }

        /// <summary>
        /// Returns an array of data types that this collection can contain.
        /// </summary>
        /// <returns>An array of data types that this collection can contain.</returns>
        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(MenuDefinitionEntryCommand), typeof(MenuDefinitionEntryPlaceholder) };
        }

        /// <summary>
        /// Gets an array of objects containing the specified collection.
        /// </summary>
        /// <param name="editValue">The collection to edit.</param>
        /// <returns>An array containing the collection objects.</returns>
        protected override object[] GetItems(object editValue)
        {
            MenuDefinitionEntryCollection commandContextMenuDefinitionEntryCollection = (MenuDefinitionEntryCollection)editValue;
            MenuDefinitionEntry[] commandContextMenuDefinitionEntries = new MenuDefinitionEntry[commandContextMenuDefinitionEntryCollection.Count];
            if (commandContextMenuDefinitionEntryCollection.Count > 0)
                commandContextMenuDefinitionEntryCollection.CopyTo(commandContextMenuDefinitionEntries, 0);
            return commandContextMenuDefinitionEntries;
        }

        /// <summary>
        /// Sets the specified array as the items of the collection.
        /// </summary>
        /// <param name="editValue">The collection to edit.</param>
        /// <param name="value">An array of objects to set as the collection items.</param>
        /// <returns>The newly created collection object or, otherwise, the collection indicated by the editValue parameter.</returns>
        protected override object SetItems(object editValue, object[] value)
        {
            MenuDefinitionEntryCollection commandContextMenuDefinitionEntryCollection = (MenuDefinitionEntryCollection)editValue;
            commandContextMenuDefinitionEntryCollection.Clear();
            foreach (MenuDefinitionEntry commandContextMenuDefinitionEntry in value)
                commandContextMenuDefinitionEntryCollection.Add(commandContextMenuDefinitionEntry);
            return commandContextMenuDefinitionEntryCollection;
        }
    }
}
