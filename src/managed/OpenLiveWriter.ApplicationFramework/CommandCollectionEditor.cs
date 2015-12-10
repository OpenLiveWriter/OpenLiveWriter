// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel.Design;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a user interface for editing a CommandCollection.
    /// </summary>
    internal class CommandCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the CommandCollectionEditor.
        /// </summary>
        public CommandCollectionEditor() : base(typeof(CommandCollection))
        {
        }

        /// <summary>
        /// Gets an array of objects containing the specified collection.
        /// </summary>
        /// <param name="editValue">The collection to edit.</param>
        /// <returns>An array containing the collection objects.</returns>
        protected override object[] GetItems(object editValue)
        {
            CommandCollection commandCollection = (CommandCollection)editValue;
            Command[] commands = new Command[commandCollection.Count];
            if (commandCollection.Count > 0)
                commandCollection.CopyTo(commands, 0);
            return commands;
        }

        /// <summary>
        /// Sets the specified array as the items of the collection.
        /// </summary>
        /// <param name="editValue">The collection to edit.</param>
        /// <param name="value">An array of objects to set as the collection items.</param>
        /// <returns>The newly created collection object or, otherwise, the collection indicated by the editValue parameter.</returns>
        protected override object SetItems(object editValue, object[] value)
        {
            CommandCollection commandCollection = (CommandCollection)editValue;
            commandCollection.Clear();
            foreach (Command command in value)
                commandCollection.Add(command);
            return commandCollection;
        }
    }
}
