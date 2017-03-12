// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Diagnostics;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Enumeration of the allowable command context values.
    /// </summary>
    public enum CommandContext
    {
        /// <summary>
        /// A command that is added to the CommandManager at all times.
        /// </summary>
        Normal,

        /// <summary>
        /// A command that is added to the CommandManager when active.
        /// </summary>
        Activated,

        /// <summary>
        /// A command that is added to the CommandManager when entered.
        /// </summary>
        Entered
    }

    /// <summary>
    /// Provides services for managing commands.
    /// </summary>
    public class CommandContextManager
    {
        /// <summary>
        ///	The CommandManager.
        /// </summary>
        private CommandManager commandManager;

        /// <summary>
        /// A value which indicates whether this CommandContextManager is "Activated".
        /// </summary>
        private bool activated = false;

        /// <summary>
        /// A value which indicates whether this CommandContextManager is "Entered".
        /// </summary>
        private bool entered = false;

        /// <summary>
        /// The command context table.
        /// </summary>
        private Hashtable commandContextTable = new Hashtable();

        /// <summary>
        /// Collection of commands that are added to the CommandManager at all times.
        /// </summary>
        private CommandCollection normalCommandCollection = new CommandCollection();

        /// <summary>
        /// Collection of commands that are added to the CommandManager when the user of this
        /// CommandContextManager is "Activated".
        /// </summary>
        private CommandCollection activatedCommandCollection = new CommandCollection();

        /// <summary>
        /// Collection of commands that are added to the CommandManager when the user of this
        /// CommandContextManager is "Entered".
        /// </summary>
        private CommandCollection enteredCommandCollection = new CommandCollection();

        /// <summary>
        /// Initializes a new instance of the CommandContextManager class.
        /// </summary>
        public CommandContextManager(CommandManager commandManager)
        {
            this.commandManager = commandManager;
        }

        /// <summary>
        /// Begins update to CommandContextManager allowing multiple change events to be batched.
        /// </summary>
        public void BeginUpdate()
        {
            commandManager.BeginUpdate();
        }

        /// <summary>
        /// Ends update to CommandContextManager allowing multiple change events to be batched.
        /// </summary>
        public void EndUpdate()
        {
            commandManager.EndUpdate();
        }

        /// <summary>
        /// Closes the CommandContextManager, ensuring that all commands have been removed from
        /// the CommandManager.
        /// </summary>
        public void Close()
        {
            //	Begin the batch update.
            BeginUpdate();

            //	If entered, leave.
            if (entered)
                Leave();

            //	If activated, deactivate.
            if (activated)
                Deactivate();

            //	Remove normal commands.
            commandManager.Remove(normalCommandCollection);

            //	End the batch update.
            EndUpdate();

            //	Clear our internal tables.
            normalCommandCollection.Clear();
            activatedCommandCollection.Clear();
            enteredCommandCollection.Clear();
            commandContextTable.Clear();
        }

        /// <summary>
        /// Adds a command to the CommandContextManager.
        /// </summary>
        /// <param name="command">The Command to add.</param>
        /// <param name="commandContext">The context in which the command is added to the CommandManager.</param>
        public void AddCommand(Command command, CommandContext commandContext)
        {
            //	Ensure that the command is not null.
            Debug.Assert(command != null, "Command cannot be null");
            if (command == null)
                return;

            //	Ensure the the command has not already been added.
            if (commandContextTable.Contains(command))
            {
                Debug.Fail("Command " + command.Identifier + " was already added.");
                return;
            }

            //	Handle the command, adding it to the appropriate command collection.
            switch (commandContext)
            {
                //	Normal commands.
                case CommandContext.Normal:
                    normalCommandCollection.Add(command);
                    commandManager.Add(command);
                    break;

                //	Activated commands.
                case CommandContext.Activated:
                    activatedCommandCollection.Add(command);
                    if (activated)
                        commandManager.Add(command);
                    break;

                //	Entered commands.
                case CommandContext.Entered:
                    enteredCommandCollection.Add(command);
                    if (entered)
                        commandManager.Add(command);
                    break;

                //	Can't happen.
                default:
                    Debug.Fail("Unknown CommandContext");
                    return;
            }

            //	Add the command to the command context table.
            commandContextTable[command] = commandContext;
        }

        /// <summary>
        /// Removes a command from the CommandContextManager.
        /// </summary>
        /// <param name="command">The Command to remove.</param>
        /// <param name="commandContext">The context in which the command is added to the CommandManager.</param>
        public void RemoveCommand(Command command)
        {
            //	Ensure that the command is not null.
            Debug.Assert(command != null, "Command cannot be null");
            if (command == null)
                return;

            //	Ensure the the command has been added.
            if (!commandContextTable.Contains(command))
            {
                Debug.Fail("Command " + command.Identifier + " was not added.");
                return;
            }

            //	Handle the command, removing it from the appropriate command collection.
            switch ((CommandContext)commandContextTable[command])
            {
                //	Normal commands.
                case CommandContext.Normal:
                    normalCommandCollection.Remove(command);
                    commandManager.Remove(command);
                    break;

                //	Activated commands.
                case CommandContext.Activated:
                    activatedCommandCollection.Remove(command);
                    if (activated)
                        commandManager.Remove(command);
                    break;

                //	Entered commands.
                case CommandContext.Entered:
                    enteredCommandCollection.Remove(command);
                    if (entered)
                        commandManager.Remove(command);
                    break;

                //	Can't happen.
                default:
                    Debug.Fail("Unknown CommandContext");
                    return;
            }

            //	Remove the command from the command context table.
            commandContextTable.Remove(command);
        }

        public bool Activated
        {
            get
            {
                return activated;
            }
        }

        /// <summary>
        /// Set the "Activated" state of the CommandContextManager.
        /// </summary>
        public void Activate()
        {
            Debug.Assert(!activated, "CommandContextManager already activated.");
            if (!activated)
            {
                commandManager.Add(activatedCommandCollection);
                activated = true;
            }
        }

        /// <summary>
        /// Clears the "Activated" state of the CommandContextManager.
        /// </summary>
        public void Deactivate()
        {
            Debug.Assert(activated, "CommandContextManager not activated.");
            if (activated)
            {
                commandManager.Remove(activatedCommandCollection);
                activated = false;
            }
        }

        public bool Entered
        {
            get
            {
                return entered;
            }
        }

        /// <summary>
        /// Set the "Entered" state of the CommandContextManager.
        /// </summary>
        public void Enter()
        {
            //Debug.Assert(!entered, "CommandContextManager already entered.");
            if (!entered)
            {
                commandManager.Add(enteredCommandCollection);
                entered = true;
            }
        }

        /// <summary>
        /// Clears the "Entered" state of the CommandContextManager.
        /// </summary>
        public void Leave()
        {
            //Debug.Assert(entered, "CommandContextManager not entered.");
            if (entered)
            {
                commandManager.Remove(enteredCommandCollection);
                entered = false;
            }
        }
    }
}
