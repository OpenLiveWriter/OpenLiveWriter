// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// The interface of a selection manager.
    /// </summary>
    public interface ICommandManager
    {
        /// <summary>
        /// Activates the specified command.
        /// </summary>
        /// <param name="command">The Command to activate.</param>
        void ActivateCommand(Command command);

        /// <summary>
        /// Deactivates the specified command.
        /// </summary>
        /// <param name="command">The Command to deactivate.</param>
        void DeactivateCommand(Command command);

        /// <summary>
        /// Activates the specified command list.
        /// </summary>
        /// <param name="commandProvider">The CommandList to activate.</param>
        void ActivateCommandList(CommandList commandList);

        /// <summary>
        /// Deactivates the specified command list.
        /// </summary>
        /// <param name="commandProvider">The CommandList to deactivate.</param>
        void DeactivateCommandList(CommandList commandList);

        /// <summary>
        /// Gets the command with the specified command identifier.
        /// </summary>
        /// <param name="commandIdentifier">The command identifier of the command to get.</param>
        /// <returns>The command, or null if a command with the specified command identifier cannot be found.</returns>
        Command GetCommand(string commandIdentifier);

        /// <summary>
        /// Gets the command with the specified shortcut.
        /// </summary>
        /// <param name="commandIdentifier">The shortcut of the command to get.</param>
        /// <returns>The command, or null if a command with the specified shortcut cannot be found.</returns>
        Command GetCommand(Shortcut shortcut);
    }
}
