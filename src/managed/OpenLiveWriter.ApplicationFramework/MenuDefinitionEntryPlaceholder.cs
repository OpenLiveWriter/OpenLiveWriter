// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// A menu item that has a submenu. The text is set by the MenuText property while the
    /// child entries are set by Entries property.
    /// </summary>
    [
        DesignTimeVisible(false),
            ToolboxItem(false)
    ]
    public class MenuDefinitionEntryPlaceholder : MenuDefinitionEntry
    {
        #region Component Designer Generated Code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        #endregion Component Designer Generated Code

        #region Private Member Variables

        /// <summary>
        /// The menu path.
        /// </summary>
        private string menuPath;

        /// <summary>
        /// Collection of MenuDefinitionEntry objects that defines the CommandContextMenu.
        /// </summary>
        private MenuDefinitionEntryCollection menuDefinitionEntryCollection = new MenuDefinitionEntryCollection();

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the MenuDefinitionEntryPlaceholder class.
        /// </summary>
        /// <param name="container">The IContainer that contains this component.</param>
        public MenuDefinitionEntryPlaceholder(IContainer container)
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
        public MenuDefinitionEntryPlaceholder()
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
        /// Gets or sets the menu path.
        /// </summary>
        public string MenuPath
        {
            get
            {
                return menuPath;
            }
            set
            {
                menuPath = value;
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
            //	Instantiate the placeholder OwnerDrawMenuItem.
            OwnerDrawMenuItem ownerDrawMenuItem = new OwnerDrawMenuItem(menuType, menuPath);

            //	Build child MenuItems.
            MenuItem[] menuItems = MenuBuilder.CreateMenuItems(commandManager, menuType, Entries);
            if (menuItems != null)
                ownerDrawMenuItem.MenuItems.AddRange(menuItems);

            //	Done.
            return ownerDrawMenuItem;
        }

        #endregion Protected Methods
    }
}
