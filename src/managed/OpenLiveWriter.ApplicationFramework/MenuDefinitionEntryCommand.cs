// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Base class of MenuDefinition entries.
    /// </summary>
    [
        DesignTimeVisible(false),
            ToolboxItem(false)
    ]
    public class MenuDefinitionEntryCommand : MenuDefinitionEntry
    {
        #region Component Designer Generated Code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        #endregion Component Designer Generated Code

        #region Private Member Variables

        /// <summary>
        /// The command identifier.
        /// </summary>
        private string commandIdentifier;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the MenuDefinitionEntryCommand class.
        /// </summary>
        /// <param name="container">The IContainer that contains this component.</param>
        public MenuDefinitionEntryCommand(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        public MenuDefinitionEntryCommand()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion Class Initialization & Termination

        #region Component Designer Generated Code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
        }

        #endregion Component Designer Generated Code

        #region Public Properties

        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        public string CommandIdentifier
        {
            get
            {
                return commandIdentifier;
            }
            set
            {
                commandIdentifier = value;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the MenuItem for this MenuDefinitionEntry.
        /// </summary>
        /// <param name="commandManager">The CommandManager to use.</param>
        /// <param name="menuType">The MenuType.</param>
        /// <returns>The menu item for this MenuDefinitionEntry.</returns>
        protected override MenuItem GetMenuItem(CommandManager commandManager, MenuType menuType)
        {
            Command command = commandManager.Get(commandIdentifier);
            if (command == null || !command.On)
                return null;
            if ((menuType == MenuType.Context || menuType == MenuType.CommandBarContext) && !command.VisibleOnContextMenu)
                return null;

            //	Instantiate and initialize the CommandOwnerDrawMenuItem.
            CommandOwnerDrawMenuItem commandOwnerDrawMenuItem = new CommandOwnerDrawMenuItem(menuType, command, command.MenuText);
            return commandOwnerDrawMenuItem;
        }

        #endregion Protected Methods
    }
}
