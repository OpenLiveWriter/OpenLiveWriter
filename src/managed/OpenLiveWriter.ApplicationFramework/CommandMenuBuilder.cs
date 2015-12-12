// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Specifies a value indicating the type of menu.
    /// </summary>
    public enum MenuType
    {
        /// <summary>
        /// Main menu.
        /// </summary>
        Main,

        /// <summary>
        /// Context menu.
        /// </summary>
        Context,

        /// <summary>
        /// CommandBar context menu.
        /// </summary>
        CommandBarContext
    }

    /// <summary>
    /// Command-based menu builder.
    /// </summary>
    public sealed class CommandMenuBuilder
    {
        /// <summary>
        /// The root merge menu entry.  Commands are merged as child entries of this entry and
        /// returned as an array of MenuItems.
        /// </summary>
        private CommandMenuBuilderEntry rootCommandMenuBuilderEntry;

        /// <summary>
        /// The type of menu merge to perform.
        /// </summary>
        private MenuType menuType;

        /// <summary>
        /// Gets or sets the type of menu merge to perform.
        /// </summary>
        public MenuType MenuType
        {
            get
            {
                return menuType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the CommandMenuBuilder class.
        /// </summary>
        public CommandMenuBuilder(MenuType menuType)
        {
            this.menuType = menuType;
            rootCommandMenuBuilderEntry = new CommandMenuBuilderEntry(this);
        }

        /// <summary>
        /// Creates and returns a set of menu items for the commands that have been merged.
        /// </summary>
        /// <returns>Array of menu items.</returns>
        public MenuItem[] CreateMenuItems()
        {
            return rootCommandMenuBuilderEntry.CreateMenuItems();
        }

        /// <summary>
        /// Merge a Command into the merge menu.
        /// </summary>
        /// <param name="command">The command to merge.</param>
        /// <param name="menuPath">The menu path at which to merge the command.  Generally, this
        /// will be either command.MainMenuPath or command.ContextMenuPath, but it can also be
        ///	a totally custom menu path as well.</param>
        public void MergeCommand(Command command)
        {
            //	Choose the menu path to use.
            string menuPath = null;
            if (menuType == MenuType.Main)
                menuPath = command.MainMenuPath;
            else if (menuType == MenuType.Context)
            {
                Trace.Fail("Merge-based context menus are obsolete");
                //menuPath = command.ContextMenuPath;
            }
            else if (menuType == MenuType.CommandBarContext)
            {
                Trace.Fail("Merge-based command bar context menus are obsolete");
                //menuPath = command.ContextMenuPath;
            }

            //	Ensure that a menu path was specified.
            if (menuPath == null || menuPath.Length == 0)
                return;

            //	Parse the menu path into an array of menu path entries.
            string[] menuPathEntries = StringHelper.SplitWithEscape(menuPath, '/', '_'); //menuPath.Split(new char[] {'/'});

            //	Build the menu structure for this command from the array of menu path entries.  For
            //	example, &File@1/&Close@2 specifies that this command represents the Close command
            //	of the File menu.  It specifies that the Close MenuItem should appear at merge
            //	position 2 of the File menu, and that the File menu should appear at merge position
            //	1 of the main menu.
            int lastEntry = menuPathEntries.Length - 1;
            CommandMenuBuilderEntry parentCommandMenuBuilderEntry = rootCommandMenuBuilderEntry;
            for (int entry = 0; entry <= lastEntry; entry++)
            {
                //	Parse the menu path entry into text and position values.
                string text;
                int position;
                ParseMenuPathEntry(menuPathEntries[entry], out text, out position);

                //	See if we have a merge menu entry for this text and position already.
                CommandMenuBuilderEntry mergeMenuEntry = (CommandMenuBuilderEntry)parentCommandMenuBuilderEntry[position, text];
                if (entry == lastEntry)
                {
                    //	Create the merge menu entry for this menu path entry.
                    parentCommandMenuBuilderEntry[position, text] = new CommandMenuBuilderEntry(this, entry, position, text, command);
                }
                else
                {
                    //	If there isn't a merge menu entry for this intermediate menu path entry,
                    //	create it.
                    if (mergeMenuEntry == null)
                    {
                        mergeMenuEntry = new CommandMenuBuilderEntry(this, entry, position, text);
                        parentCommandMenuBuilderEntry[position, text] = mergeMenuEntry;
                    }

                    //	Set the root to this merge menu entry for the next loop iteration.
                    parentCommandMenuBuilderEntry = mergeMenuEntry;
                }
            }
        }

        /// <summary>
        /// Helper to parse a menu path entry describing a menu item in the form:
        ///		[-]text@position
        ///
        ///	'-'			Optional.  Specifies that a separator menu item should be inserted before the
        ///				menu item item.
        ///	text		Menu item text (i.e. &File)
        ///	position	Menu item position.  For example, &File@1 specifies File menu, first menu
        ///				position, and &Edit@2 specifies Edit menu, second menu position.
        /// </summary>
        /// <param name="menuName">The menu path entry to parse.</param>
        /// <param name="text">The return text for the menu item specified in this menu path entry.</param>
        /// <param name="position">The return position for the menu item specified in this menu path entry.</param>
        private static void ParseMenuPathEntry(string menuPathEntry, out string text, out int position)
        {
            //	Parse the menu path entry.
            string[] values = StringHelper.SplitWithEscape(menuPathEntry, '@', '_'); //menuPathEntry.Split(new char[] {'@'});
            Debug.Assert(values.Length == 2, "Invalid menu path entry.", String.Format(CultureInfo.InvariantCulture, "{0} is not a valid menu path entry.", menuPathEntry));

            //	Set the name.
            text = values[0];

            //	Set the position.
            position = 0;
            if (values.Length == 2)
            {
                try
                {
                    position = Int32.Parse(values[1], CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    //	Don't kill a running system because of this problem.  In this case, use a
                    //	position of 0 and keep on running.  For debug mode, throw an assertion.
                    Debug.Assert(false, "Invalid menu merge order specified.", String.Format(CultureInfo.InvariantCulture, "{0} contains an invalid menu merge order specification.", menuPathEntry));
                }
            }
        }
    }
}
