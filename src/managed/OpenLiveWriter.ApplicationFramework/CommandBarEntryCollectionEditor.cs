// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel.Design;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a user interface for editing a CommandBarEntryCollection.
    /// </summary>
    internal class CommandBarEntryCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the CommandBarEntryCollectionEditor.
        /// </summary>
        public CommandBarEntryCollectionEditor() : base(typeof(CommandBarEntryCollection))
        {
        }

        /// <summary>
        /// Returns an array of data types that this collection can contain.
        /// </summary>
        /// <returns>An array of data types that this collection can contain.</returns>
        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(CommandBarButtonEntry), typeof(CommandBarSeparatorEntry), typeof(CommandBarLightweightControlEntry), typeof(CommandBarControlEntry), typeof(CommandBarLabelEntry) };
        }

        /// <summary>
        /// Gets an array of objects containing the specified collection.
        /// </summary>
        /// <param name="editValue">The collection to edit.</param>
        /// <returns>An array containing the collection objects.</returns>
        protected override object[] GetItems(object editValue)
        {
            CommandBarEntryCollection commandBarEntryCollection = (CommandBarEntryCollection)editValue;
            CommandBarEntry[] commandBarEntries = new CommandBarEntry[commandBarEntryCollection.Count];
            if (commandBarEntryCollection.Count > 0)
                commandBarEntryCollection.CopyTo(commandBarEntries, 0);
            return commandBarEntries;
        }

        /// <summary>
        /// Sets the specified array as the items of the collection.
        /// </summary>
        /// <param name="editValue">The collection to edit.</param>
        /// <param name="value">An array of objects to set as the collection items.</param>
        /// <returns>The newly created collection object or, otherwise, the collection indicated by the editValue parameter.</returns>
        protected override object SetItems(object editValue, object[] value)
        {
            CommandBarEntryCollection commandBarEntryCollection = (CommandBarEntryCollection)editValue;
            commandBarEntryCollection.Clear();
            foreach (CommandBarEntry commandBarEntry in value)
                commandBarEntryCollection.Add(commandBarEntry);
            return commandBarEntryCollection;
        }
    }
}
