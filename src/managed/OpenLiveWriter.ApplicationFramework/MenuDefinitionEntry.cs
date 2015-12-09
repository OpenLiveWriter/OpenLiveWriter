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
    public class MenuDefinitionEntry : Component
    {
        #region Component Designer Generated Code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        #endregion Component Designer Generated Code

        #region Private Member Variables

        /// <summary>
        ///	A value which indicates whether a separator will be placed before the entry.
        /// </summary>
        private bool separatorBefore;

        /// <summary>
        ///	A value which indicates whether a separator will be placed after the entry.
        /// </summary>
        private bool separatorAfter;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the MenuDefinitionEntry class.
        /// </summary>
        /// <param name="container">The IContainer that contains this component.</param>
        public MenuDefinitionEntry(IContainer container)
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
        public MenuDefinitionEntry()
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
        ///	A value which indicates whether a separator will be placed before the entry.
        /// </summary>
        [
            Category("Menu"),
                Localizable(false),
                Description("Indicates whether a separator will be placed before the entry.")
        ]
        public bool SeparatorBefore
        {
            get
            {
                return separatorBefore;
            }
            set
            {
                separatorBefore = value;
            }
        }

        /// <summary>
        ///	A value which indicates whether a separator will be placed before the entry.
        /// </summary>
        [
            Category("Menu"),
                Localizable(false),
                Description("Indicates whether a separator will be placed after the entry.")
        ]
        public bool SeparatorAfter
        {
            get
            {
                return separatorAfter;
            }
            set
            {
                separatorAfter = value;
            }
        }

        public bool On
        {
            get
            {
                return _on;
            }
            set
            {
                _on = value;
            }
        }
        private bool _on = true;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets the set of menu items for this MenuDefinitionEntry.
        /// </summary>
        /// <param name="commandManager">The CommandManager to use.</param>
        /// <param name="menuType">The MenuType.</param>
        /// <returns>Array of menu items for this MenuDefinitionEntry.</returns>
        public MenuItem[] GetMenuItems(CommandManager commandManager, MenuType menuType)
        {
            if (!On)
                return null;

            //	Get the MenuItem for this MenuDefinitionEntry.  If it's null, return null.
            MenuItem menuItem = GetMenuItem(commandManager, menuType);
            if (menuItem == null)
                return null;

            //	Sort out whether we'll be adding separator menu items.
            int count = 1;
            if (separatorBefore)
                count++;
            if (separatorAfter)
                count++;

            //	Return the array of menu items for this MenuDefinitionEntry.
            int index = 0;
            MenuItem[] menuItems = new MenuItem[count];
            if (separatorBefore)
                menuItems[index++] = MakeSeparatorMenuItem(MenuType.Context);
            menuItems[index++] = menuItem;
            if (separatorAfter)
                menuItems[index] = MakeSeparatorMenuItem(MenuType.Context);
            return menuItems;
        }

        /// <summary>
        /// Helper to make a separator menu item.
        /// </summary>
        /// <returns>A MenuItem that is a separator MenuItem.</returns>
        private MenuItem MakeSeparatorMenuItem(MenuType menuType)
        {
            //	Instantiate the separator menu item.
            MenuItem separatorMenuItem = new OwnerDrawMenuItem(menuType, "-");
            return separatorMenuItem;
        }

        #endregion

        /// <summary>
        /// Gets the MenuItem for this MenuDefinitionEntry.
        /// </summary>
        /// <param name="commandManager">The CommandManager to use.</param>
        /// <param name="menuType">The MenuType.</param>
        /// <returns>The menu item for this MenuDefinitionEntry.</returns>
        protected virtual MenuItem GetMenuItem(CommandManager commandManager, MenuType menuType)
        {
            return null;
        }
    }
}
