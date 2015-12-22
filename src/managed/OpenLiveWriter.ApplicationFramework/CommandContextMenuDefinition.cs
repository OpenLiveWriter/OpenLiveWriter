// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a Command-based ContextMenu definition.
    /// </summary>
    public class CommandContextMenuDefinition : Component
    {
        #region Component Designer Generated Code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        #endregion Component Designer Generated Code

        #region Private Member Variables

        /// <summary>
        /// A value indicating whether shortcuts should be shown on the CommandContextMenu.
        /// </summary>
        private bool commandBar = false;

        /// <summary>
        /// Collection of MenuDefinitionEntry objects that defines the CommandContextMenu.
        /// </summary>
        private MenuDefinitionEntryCollection menuDefinitionEntryCollection = new MenuDefinitionEntryCollection();

        #endregion Private Member Variables

        #region Public Events

        /// <summary>
        /// Occurs before the CommandContextMenuDefinition is used to show a context menu.
        /// </summary>
        [
            Category("Menu"),
                Description("Occurs before the CommandContextMenuDefinition is used to show a context menu.")
        ]
        public event EventHandler BeforeShowMenu;

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the CommandContextMenu class.
        /// </summary>
        /// <param name="container"></param>
        public CommandContextMenuDefinition(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandContextMenu class.
        /// </summary>
        public CommandContextMenuDefinition()
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether shortcuts should be shown on the CommandContextMenu.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(false),
                Description("Specifies whether shortcuts should be shown on the CommandContextMenu.")
        ]
        public bool CommandBar
        {
            get
            {
                return commandBar;
            }
            set
            {
                commandBar = value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of command bar entries that define the command bar.
        /// </summary>
        [
            Localizable(true),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public MenuDefinitionEntryCollection Entries
        {
            get
            {
                return menuDefinitionEntryCollection;
            }
            set
            {
                menuDefinitionEntryCollection = value;
            }
        }

        #endregion Public Properties

        #region Internal Methods

        /// <summary>
        /// Raises the BeforeShowMenu event.
        /// </summary>
        internal void RaiseBeforeShowMenu()
        {
            OnBeforeShowMenu(EventArgs.Empty);
        }

        #endregion Internal Methods

        #region Protected Events

        /// <summary>
        /// Raises the BeforeShowMenu event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnBeforeShowMenu(EventArgs e)
        {
            if (BeforeShowMenu != null)
                BeforeShowMenu(this, e);
        }

        #endregion
    }
}
