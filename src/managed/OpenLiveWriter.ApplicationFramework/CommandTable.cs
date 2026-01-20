// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides command table management.
    /// </summary>
    public class CommandTable : IEnumerable
    {
        /// <summary>
        /// The set of active commands, keyed by command identifier.
        /// </summary>
        private Hashtable commandTable = new Hashtable();

        /// <summary>
        ///	The cross-referenced set of active shortcuts.  Keyed by shortcut.  This table is not
        ///	maintained until it is used.  A call to GetCommand with a shortcut will cause the table
        ///	to be loaded.  Once loaded, it is maintained.
        /// </summary>
        private Hashtable shortcutTable = null;

        /// <summary>
        /// Initializes a new instance of the CommandTable class.
        /// </summary>
        public CommandTable()
        {
        }

        /// <summary>
        /// Clears the command table.
        /// </summary>
        public void Clear()
        {
            commandTable.Clear();
            if (shortcutTable != null)
                shortcutTable.Clear();
        }

        /// <summary>
        /// Adds the specified command.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddCommand(Command command)
        {
            //	Ensure that this is not a duplicate.  In debug mode, assert that this is true.
            //	In release mode, fail silently (replacing the existing command) so as not to take
            //	down a running system.
            Debug.Assert(!commandTable.Contains(command.Identifier), "Duplicate command encountered.", String.Format("Command {0} already exists.", command.Identifier));

            //	Add the command to the command table.
            commandTable[command.Identifier] = command;

            //	Add the shortcut for the command to the shortcut table.
            AddShortcut(command);
        }

        /// <summary>
        /// Removes the specified command.
        /// </summary>
        /// <param name="command">The command to remove.</param>
        public void RemoveCommand(Command command)
        {
            //	Ensure that the command is in the table.  In debug mode, assert that this is true.
            //	In release mode, fail silently (replacing the existing command) so as not to take
            //	down a running system.
            Debug.Assert(commandTable.Contains(command.Identifier), "Command not found.", String.Format("Command {0} does not exist.", command.Identifier));

            //	Remove the command from the command table.
            commandTable.Remove(command.Identifier);

            //	Remove the shortcut for the command from the shortcut table.
            RemoveShortcut(command);
        }

        /// <summary>
        /// Adds the specified command list.
        /// </summary>
        /// <param name="commandList">The CommandList to add.</param>
        public void AddCommandList(CommandList commandList)
        {
            //	Add all the commands to the command table.
            foreach (Command command in commandList.Commands)
                AddCommand(command);
        }

        /// <summary>
        /// Removes the specified command list.
        /// </summary>
        /// <param name="commandProvider">The CommandList to remove.</param>
        public void RemoveCommandList(CommandList commandList)
        {
            //	Remove all the commands from the command table.
            foreach (Command command in commandList.Commands)
                RemoveCommand(command);
        }

        /// <summary>
        /// Gets the command with the specified command identifier.
        /// </summary>
        /// <param name="commandIdentifier">The command identifier of the command to get.</param>
        /// <returns>The command, or null if a command with the specified command identifier cannot be found.</returns>
        public Command GetCommand(string commandIdentifier)
        {
            return (Command)commandTable[commandIdentifier];
        }

        /// <summary>
        /// Gets the command with the specified shortcut.
        /// </summary>
        /// <param name="shortcut">The shortcut of the command to get.</param>
        /// <returns>The command, or null if a command with the specified shortcut cannot be found.</returns>
        public Command GetCommand(Shortcut shortcut)
        {
            //	If the shortcut table has not been built, build it.
            if (shortcutTable == null)
            {
                //	Instantiate the shortcut table.
                shortcutTable = new Hashtable();

                //	Load all the commands into the shortcut table.
                foreach (Command command in this)
                    AddShortcut(command);
            }

            //	Return the command with the specified shortcut.
            return (Command)shortcutTable[shortcut];
        }

        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return commandTable.Values.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified command to the shortcut table.
        /// </summary>
        /// <param name="command">The command to add to the shortcut table.</param>
        private void AddShortcut(Command command)
        {
            //	If we have a shortcut table, and the command has a shortcut, add it.
            if (shortcutTable != null && command.Shortcut != Shortcut.None)
            {
                //	Ensure that this is not a duplicate.  In debug mode, assert that this is true.
                //	In release mode, fail silently (replacing the existing command) so as not to take
                //	down a running system.
                Debug.Assert(!shortcutTable.Contains(command.Shortcut), "Duplicate shortcut encountered.", String.Format("Shortcut {0} already exists.", command.Shortcut.ToString()));
                shortcutTable[command.Shortcut] = command;
            }
        }

        /// <summary>
        /// Adds the specified command to the shortcut table.
        /// </summary>
        /// <param name="command">The command to add to the shortcut table.</param>
        private void RemoveShortcut(Command command)
        {
            //	If the command has a shortcut, remove it from the shortcut table.
            if (shortcutTable != null && command.Shortcut != Shortcut.None)
            {
                //	Ensure that the shortcut is in the table.  In debug mode, assert that this is true.
                //	In release mode, fail silently (replacing the existing command) so as not to take
                //	down a running system.
                Debug.Assert(shortcutTable.Contains(command.Shortcut), "Shortcut not found.", String.Format("Shortcut {0} does not exists.", command.Shortcut.ToString()));
                shortcutTable.Remove(command.Shortcut);
            }
        }
    }
}
