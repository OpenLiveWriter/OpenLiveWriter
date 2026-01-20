// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides "event bridging" between MenuItem objects and Command objects.
    /// </summary>
    internal class CommandMenuItemEventBridge
    {
        #region Private Member Variables & Properties
        /// <summary>
        /// The Command for this Command to MenuItem event bridge.
        /// </summary>
        private Command command;

        /// <summary>
        /// The MenuItem for this Command to MenuItem event bridge.
        /// </summary>
        private MenuItem menuItem;
        #endregion

        #region Class Initialization & Termination
        /// <summary>
        /// Initializes a new instance of the CommandMenuItemEventBridge class.
        /// </summary>
        /// <param name="command">The Command for this Command to MenuItem event bridge.</param>
        /// <param name="menuItem">The MenuItem for this Command to MenuItem event bridge.</param>
        public CommandMenuItemEventBridge(Command command, MenuItem menuItem)
        {
            //	Set the command.
            this.command = command;

            //	Set the menu item.
            this.menuItem = menuItem;

            //	Add event handlers for the command events.
            command.EnabledChanged += new EventHandler(command_EnabledChanged);

            //	Add event handlers for the menu item events.
            menuItem.Click += new EventHandler(menuItem_Click);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// The enabled property of the command changed.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event parameters.</param>
        public void Disconnect()
        {
            //	Add event handlers for command events.
            command.EnabledChanged -= new EventHandler(command_EnabledChanged);

            //	Bridge menu item events.
            menuItem.Click -= new EventHandler(menuItem_Click);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// The enabled property of the command changed.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event parameters.</param>
        private void command_EnabledChanged(object sender, System.EventArgs e)
        {
            menuItem.Enabled = command.Enabled;
        }

        /// <summary>
        /// The menu item was clicked.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event parameters.</param>
        private void menuItem_Click(object sender, System.EventArgs e)
        {
            command.PerformExecute();
        }
        #endregion
    }
}
