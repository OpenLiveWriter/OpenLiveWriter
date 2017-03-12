// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a Command-based ContextMenu.
    /// </summary>
    public class CommandContextMenu : ContextMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// A value indicating whether the CommandContextMenu is being shown on a CommandBar.
        /// </summary>
        private bool commandBar = false;

        /// <summary>
        /// Shows a CommandContextMenu modally.
        /// </summary>
        ///	<param name="commandManager">The CommandManager to use.</param>
        /// <param name="parentWindow">The parent window.</param>
        /// <param name="position">The position to show the menu, in screen coordinates.</param>
        /// <param name="commandContextMenuDefinition">The CommandContextMenuDefinition of the commands to show in the menu.  These commands must be present in the CommandManager.</param>
        /// <returns>The command that was selected; or null if no command was selected.</returns>
        public static Command ShowModal(CommandManager commandManager, Control parentWindow, Point position, CommandContextMenuDefinition commandContextMenuDefinition)
        {
            return ShowModal(commandManager, parentWindow, position, position.X, commandContextMenuDefinition);
        }

        /// <summary>
        /// Shows a CommandContextMenu modally.
        /// </summary>
        ///	<param name="commandManager">The CommandManager to use.</param>
        /// <param name="parentWindow">The parent window.</param>
        /// <param name="position">The position to show the menu, in screen coordinates.</param>
        /// <param name="alternateXPosition">An alternate X-position in case the showing of the
        /// menu results in the menu going offscreen to the right</param>
        /// <param name="commandContextMenuDefinition">The CommandContextMenuDefinition of the commands to show in the menu.  These commands must be present in the CommandManager.</param>
        /// <returns>The command that was selected; or null if no command was selected.</returns>
        public static Command ShowModal(CommandManager commandManager, Control parentWindow, Point position, int alternateXPosition, CommandContextMenuDefinition commandContextMenuDefinition)
        {
            //	If the command context menu definition was null, or contained no Command identifiers, we're done.
            Debug.Assert(commandContextMenuDefinition != null, "Cannot show a CommandContextMenu without a CommandContextMenuDefinition");
            if (commandContextMenuDefinition == null || commandContextMenuDefinition.Entries.Count == 0)
                return null;

            //	Obtain the parent window's context menu.
            ContextMenu parentContextMenu = parentWindow.ContextMenu;

            //	Instantiate the CommandContextMenu from the command context menu definition.
            CommandContextMenu commandContextMenu = new CommandContextMenu(commandManager, commandContextMenuDefinition);

            //	Set the context menu as our parent window's context menu so that keyboard mnemonics work.
            parentWindow.ContextMenu = commandContextMenu;

            //	Run the context menu.
            Command command = commandContextMenu.ShowModal(parentWindow, position, alternateXPosition);

            //	Restore our parent window's contetx menu.
            parentWindow.ContextMenu = parentContextMenu;

            //	Dipose of the context menu.
            commandContextMenu.Dispose();

            //	Return the selected command.
            return command;
        }

        /// <summary>
        /// Shows a CommandContextMenu modally.
        /// </summary>
        /// <param name="parentWindow">The parent window.</param>
        /// <param name="position">The position to show the menu, in screen coordinates.</param>
        /// <param name="commandCollection">The CommandCollection to show in the menu.  These commands must be present in the CommandManager.</param>
        /// <param name="commandBar">true if this CommandContextMenu is being shown from a CommandBar; false otherwise.</param>
        /// <returns>The command that was selected; or null if no command was selected.</returns>
        public static Command ShowModal(Control parentWindow, Point position, CommandCollection commandCollection, bool commandBar)
        {
            return ShowModal(parentWindow, position, position.X, commandCollection, commandBar);
        }

        /// <summary>
        /// Shows a CommandContextMenu modally.
        /// </summary>
        /// <param name="parentWindow">The parent window.</param>
        /// <param name="position">The position to show the menu, in screen coordinates.</param>
        /// <param name="alternateXPosition">An alternate X-position in case the showing of the
        /// menu results in the menu going offscreen to the right</param>
        /// <param name="commandCollection">The CommandCollection to show in the menu.  These commands must be present in the CommandManager.</param>
        /// <param name="commandBar">true if this CommandContextMenu is being shown from a CommandBar; false otherwise.</param>
        /// <returns>The command that was selected; or null if no command was selected.</returns>
        public static Command ShowModal(Control parentWindow, Point position, int alternateXPosition, CommandCollection commandCollection, bool commandBar)
        {
            //	If the command collection was null, or contained no Commands, we're done.
            Debug.Assert(commandCollection != null, "Cannot show a CommandContextMenu without a CommandCollection");
            if (commandCollection == null || commandCollection.Count == 0)
                return null;

            //	Obtain the parent window's context menu.
            ContextMenu parentContextMenu = parentWindow.ContextMenu;

            //	Instantiate the CommandContextMenu from the command collection.
            CommandContextMenu commandContextMenu = new CommandContextMenu(commandCollection, commandBar);

            //	Set the context menu as our parent window's context menu so that keyboard mnemonics work.
            parentWindow.ContextMenu = commandContextMenu;

            //	Run the context menu.
            Command command = commandContextMenu.ShowModal(parentWindow, position, alternateXPosition);

            //	Restore our parent window's contetx menu.
            parentWindow.ContextMenu = parentContextMenu;

            //	Dipose of the context menu.
            commandContextMenu.Dispose();

            //	Return the selected command.
            return command;
        }

        /// <summary>
        /// Shows a CommandContextMenu modally.
        /// </summary>
        /// <param name="parentWindow">The parent window.</param>
        /// <param name="position">The position to show the menu, in screen coordinates.</param>
        /// <param name="alternateXPosition">An alternate X-position in case the showing of the
        /// menu results in the menu going offscreen to the right</param>
        /// <param name="commandCollection">The CommandCollection to show in the menu.  These commands must be present in the CommandManager.</param>
        /// <param name="commandBar">true if this CommandContextMenu is being shown from a CommandBar; false otherwise.</param>
        /// <returns>The command that was selected; or null if no command was selected.</returns>
        public static Command ShowModal(Control parentWindow, Point position, int alternateXPosition, MenuItem[] menuItems)
        {
            //	If the command collection was null, or contained no Commands, we're done.
            Debug.Assert(menuItems != null, "Cannot show a CommandContextMenu without menu items");
            if (menuItems == null || menuItems.Length == 0)
                return null;

            //	Obtain the parent window's context menu.
            ContextMenu parentContextMenu = parentWindow.ContextMenu;

            //	Instantiate the CommandContextMenu from the command collection.
            CommandContextMenu commandContextMenu = new CommandContextMenu(menuItems);

            //	Set the context menu as our parent window's context menu so that keyboard mnemonics work.
            parentWindow.ContextMenu = commandContextMenu;

            //	Run the context menu.
            Command command = commandContextMenu.ShowModal(parentWindow, position, alternateXPosition);

            //	Restore our parent window's contetx menu.
            parentWindow.ContextMenu = parentContextMenu;

            //	Dipose of the context menu.
            commandContextMenu.Dispose();

            //	Return the selected command.
            return command;
        }

        /// <summary>
        /// Show the menu (modally) and return the type of the command that was chosen
        /// </summary>
        /// <param name="commandTypes">command types to include in the menu</param>
        /// <param name="point">point to show the menu at</param>
        /// <param name="parentControl">parent control for menu</param>
        /// <param name="menuTextParams">substitution parameters for the menu text</param>
        /// <returns>Type of the command that was chosen (null if no command chosen)</returns>
        public static Type ShowModal(CommandManager commandManager, Control parentWindow, Point position, Type[] commandTypes)
        {
            //	Dynamically construct the collection of commands to be shown.
            CommandCollection commandCollection = new CommandCollection();
            foreach (Type commandType in commandTypes)
            {
                // verify that the type is correct
                if (!typeof(Command).IsAssignableFrom(commandType))
                    throw new ArgumentException(
                        "Type passed is not a subclass of Command!");

                // create an instance of the command and add it to the collection of commands to be shown.
                Command command = Activator.CreateInstance(commandType) as Command;
                commandCollection.Add(command);
            }

            //	Add the commands to the system command manager.
            commandManager.Add(commandCollection);

            //	Show the context menu modally.
            Command commandSelected = ShowModal(parentWindow, position, commandCollection, false);
            Type type = commandSelected == null ? null : commandSelected.GetType();
            commandSelected = null;

            //	Remove the commands from the system command manager.
            commandManager.Remove(commandCollection);

            //	Done!
            return type;
        }

        /// <summary>
        /// Initializes a new instance of the CommandContextMenu class.
        /// </summary>
        /// <param name="container"></param>
        private CommandContextMenu(CommandManager commandManager, CommandContextMenuDefinition commandContextMenuDefinition)
        {
            BuildMenuItems(commandManager, commandContextMenuDefinition);
        }

        /// <summary>
        /// Initializes a new instance of the CommandContextMenu class.
        /// </summary>
        /// <param name="container"></param>
        private CommandContextMenu(CommandCollection commandCollection, bool commandBar)
        {
            this.commandBar = commandBar;
            BuildMenuItems(commandCollection);
        }

        private CommandContextMenu(MenuItem[] menuItems)
        {
            this.MenuItems.AddRange(menuItems);
        }

        /// <summary>
        /// Initializes a new instance of the CommandContextMenu class.
        /// </summary>
        /// <param name="container"></param>
        private CommandContextMenu(IContainer container)
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
        private CommandContextMenu()
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        /// <summary>
        /// Builds the menu items of this CommandContextMenu from the specified CommandContextMenuDefinition.
        /// </summary>
        /// <param name="commandContextMenuDefinition">The CommandContextMenuDefinition.</param>
        private void BuildMenuItems(CommandManager commandManager, CommandContextMenuDefinition commandContextMenuDefinition)
        {
            //	Raise the BeforeShowMenu event in the CommandContextMenuDefinition, allowing
            //	interested parties to manage commands.
            commandContextMenuDefinition.RaiseBeforeShowMenu();

            //	Build the menu items.
            if (commandContextMenuDefinition.Entries.Count != 0)
                MenuItems.AddRange(MenuBuilder.CreateMenuItems(commandManager, commandContextMenuDefinition));
        }

        /// <summary>
        /// Builds the menu items of this CommandContextMenu from the specified CommandCollection.
        /// </summary>
        /// <param name="commandContextMenuDefinition">The CommandCollection.</param>
        private void BuildMenuItems(CommandCollection commandCollection)
        {
            CommandMenuBuilder commandMenuBuilder = new CommandMenuBuilder(commandBar ? MenuType.CommandBarContext : MenuType.Context);
            foreach (Command command in commandCollection)
                if (command.On)
                    commandMenuBuilder.MergeCommand(command);
            MenuItems.AddRange(commandMenuBuilder.CreateMenuItems());
        }

        /// <summary>
        /// Displays the CommandContextMenu at the specified location and returns the command that
        /// was selected by the user.
        /// </summary>
        /// <param name="parentWindow">Window that owns the shortcut menu.</param>
        /// <param name="position">Specifies the location of the shortcut menu, in screen coordinates.</param>
        /// <param name="alternateXPosition">An alternate X-position in case the showing of the
        /// menu results in the menu going offscreen to the right</param>
        /// <returns>Command that was selected by the user; or null if a command was not selected.</returns>
        private Command ShowModal(Control parentWindow, Point position, int alternateXPosition)
        {
            //	Recursively raise the BeforeShow event.
            RecursivelyRaiseBeforeShow(MenuItems);

            bool rtl = BidiHelper.IsRightToLeft;

            uint layout = TPM.LEFTALIGN;
            if (rtl)
            {
                layout |= TPM.LAYOUTRTL;

                // When the parent is a RTL form, we don't need to do all
                // this--it gets done implicitly
                if (!(parentWindow is Form))
                {
                    layout |= TPM.RIGHTALIGN;

                    int x = position.X;
                    position.X = alternateXPosition;
                    alternateXPosition = x;
                }
            }

            //	Calculate the right edge of the menu
            const int ESTIMATED_MENU_WIDTH = 250;
            Point menuRightEdge = position;
            menuRightEdge.Offset(ESTIMATED_MENU_WIDTH * (rtl ? -1 : 1), 0);

            //	Check to see whether the menu is likely to go offscreen to the right
            if (!Screen.FromControl(parentWindow).Bounds.Contains(menuRightEdge))
                position.X = alternateXPosition;

            //	Pop it up!  (.NET should have provided this functionality, damn it.)
            int menuID = User32.TrackPopupMenu(Handle,
                                                TPM.NONOTIFY | TPM.RETURNCMD | layout,
                                                position.X,
                                                position.Y,
                                                0,
                                                parentWindow.Handle,
                                                   IntPtr.Zero);

            //	If the user didn't make a selection, return null.
            if (menuID == 0)
                return null;

            //	Return the command that the user selected.
            CommandOwnerDrawMenuItem commandOwnerDrawMenuItem = LocateCommandOwnerDrawMenuItem(MenuItems, menuID);
            Debug.Assert(commandOwnerDrawMenuItem != null, "CommandContextMenu.ShowModal was not able to locate the CommandOwnerDrawMenuItem corresponding to the user's selection.");
            return commandOwnerDrawMenuItem.Command;
        }

        /// <summary>
        /// Helper to recursively raise the BeforeShow event on every OwnerDrawMenuItem.
        /// </summary>
        /// <param name="menuItemCollection">The MenuItemCollection to search.</param>
        /// <param name="menuID">The menu id to search for.</param>
        /// <returns>The CommandOwnerDrawMenuItem, or null if it could not be found.</returns>
        private void RecursivelyRaiseBeforeShow(MenuItemCollection menuItemCollection)
        {
            //	Locate the matching CommandOwnerDrawMenuItem that was selected.
            foreach (MenuItem menuItem in menuItemCollection)
            {
                //	If this menu item is a OwnerDrawMenuItem, raise its BeforeShow event.
                if (menuItem is OwnerDrawMenuItem)
                {
                    //	Raise the BeforeShow event.
                    ((OwnerDrawMenuItem)menuItem).InvokeBeforeShow(EventArgs.Empty);

                    //	Raise the BeforeShow event on every child MenuItem.
                    RecursivelyRaiseBeforeShow(menuItem.MenuItems);
                }
            }
        }

        /// <summary>
        /// Helper to recursively locate a CommandOwnerDrawMenuItem with the specified menu id.
        /// </summary>
        /// <param name="menuItemCollection">The MenuItemCollection to search.</param>
        /// <param name="menuID">The menu id to search for.</param>
        /// <returns>The CommandOwnerDrawMenuItem, or null if it could not be found.</returns>
        private CommandOwnerDrawMenuItem LocateCommandOwnerDrawMenuItem(MenuItemCollection menuItemCollection, int menuID)
        {
            //	Locate the matching CommandOwnerDrawMenuItem that was selected.
            foreach (MenuItem menuItem in menuItemCollection)
            {
                if (menuItem is CommandOwnerDrawMenuItem)
                {
                    CommandOwnerDrawMenuItem commandOwnerDrawMenuItem = (CommandOwnerDrawMenuItem)menuItem;
                    if (commandOwnerDrawMenuItem.GetMenuID() == menuID)
                        return commandOwnerDrawMenuItem;
                }
                else
                {
                    CommandOwnerDrawMenuItem commandOwnerDrawMenuItem = LocateCommandOwnerDrawMenuItem(menuItem.MenuItems, menuID);
                    if (commandOwnerDrawMenuItem != null)
                        return commandOwnerDrawMenuItem;
                }
            }
            return null;
        }
    }
}
