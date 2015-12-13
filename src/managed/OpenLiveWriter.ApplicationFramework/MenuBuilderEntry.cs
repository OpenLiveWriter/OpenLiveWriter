// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace Project31.ApplicationFramework
{
    /// <summary>
    /// MenuBuilderEntry is an internal class used to build Command-based menus.
    /// </summary>
    internal sealed class MenuBuilderEntry
    {
        /// <summary>
        /// The MenuBuilder for this MenuBuilderEntry.
        /// </summary>
        private MenuBuilder menuBuilder;

        /// <summary>
        ///	Merge menu entry level.
        /// </summary>
        private int level;

        /// <summary>
        /// Gets the merge menu entry level.
        /// </summary>
        public int Level
        {
            get
            {
                return level;
            }
        }

        /// <summary>
        ///	Merge menu entry position.
        /// </summary>
        private int position;

        /// <summary>
        /// Gets the merge menu entry position.
        /// </summary>
        public int Position
        {
            get
            {
                return position;
            }
        }

        /// <summary>
        /// Merge menu entry text.
        /// </summary>
        private string text;

        /// <summary>
        /// Gets the merge menu entry text.
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
        }

        /// <summary>
        /// Merge menu entry command.
        /// </summary>
        private Command command;

        /// <summary>
        /// Gets the merge menu entry command.
        /// </summary>
        public Command Command
        {
            get
            {
                return command;
            }
        }

        /// <summary>
        /// Child merge menu entries.
        /// </summary>
        private SortedList childMergeMenuEntries = new SortedList();

        /// <summary>
        /// Initializes a new instance of the MenuBuilderEntry class.
        /// </summary>
        public MenuBuilderEntry(MenuBuilder menuBuilder)
        {
            this.menuBuilder = menuBuilder;
            this.level = -1;
        }

        /// <summary>
        /// Initializes a new instance of the MenuBuilderEntry class.  This constructor is used for
        /// top level and "container" menu items.
        /// </summary>
        /// <param name="position">Merge menu entry position.</param>
        /// <param name="text">Merge menu entry text.</param>
        public MenuBuilderEntry(MenuBuilder menuBuilder, int level, int position, string text)
        : this(menuBuilder, level, position, text, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MenuBuilderEntry class.  This constructor is used for
        /// the command menu entry.
        /// </summary>
        /// <param name="position">Merge menu entry position.</param>
        /// <param name="text">Merge menu entry text.</param>
        /// <param name="command">Merge menu entry command.</param>
        public MenuBuilderEntry(MenuBuilder menuBuilder, int level, int position, string text, Command command)
        {
            this.menuBuilder = menuBuilder;
            this.level = level;
            this.position = position;
            this.text = text;
            this.command = command;
        }

        /// <summary>
        /// Indexer for access child merge menu entries.
        /// </summary>
        public MenuBuilderEntry this [int position, string text]
        {
            get
            {
                string key = String.Format("{0}-{1}", position.ToString("D3"), text);
                return (MenuBuilderEntry)childMergeMenuEntries[key];
            }
            set
            {
                string key = String.Format("{0}-{1}", position.ToString("D3"), text);
                childMergeMenuEntries[key] = value;
            }
        }

        /// <summary>
        /// Creates and returns a set of menu items from the child merge menu entries in this merge
        /// menu entry.
        /// </summary>
        /// <param name="mainMenu">The level at which the MenuItems will appear.</param>
        /// <returns>Array of menu items.</returns>
        public MenuItem[] CreateMenuItems()
        {
            //	If this merge menu entry has no child merge menu entries, return null.
            if (childMergeMenuEntries.Count == 0)
                return null;

            //	Construct an array list to hold the menu items being created.
            ArrayList menuItemArrayList = new ArrayList();

            //	Enumerate the child merge menu entries of this merge menu entry.
            foreach (MenuBuilderEntry mergeMenuEntry in childMergeMenuEntries.Values)
            {
                //	Get the text of the merge menu entry.
                string text = mergeMenuEntry.Text;

                //	Create the menu item for this child merge menu entry.
                MenuItem menuItem;
                bool separatorBefore, separatorAfter;
                if (menuBuilder.MenuType == MenuType.Main && mergeMenuEntry.level == 0)
                {
                    //	Level zero of a main menu.
                    menuItem = new OwnerDrawMenuItem(menuBuilder.MenuType);
                    separatorBefore = separatorAfter = false;
                }
                else
                {
                    //	Determine whether a separator before and a separator after the menu item
                    //	should be inserted.
                    separatorBefore = text.StartsWith(SEPARATOR_TEXT);
                    separatorAfter = text.EndsWith(SEPARATOR_TEXT);
                    if (separatorBefore || separatorAfter)
                        text = text.Replace(SEPARATOR_TEXT, string.Empty);

                    //	Instantiate the menu item.
                    if (mergeMenuEntry.Command == null)
                        menuItem = new OwnerDrawMenuItem(menuBuilder.MenuType);
                    else
                        menuItem = new CommandOwnerDrawMenuItem(menuBuilder.MenuType, mergeMenuEntry.Command);
                }

                //	Set the menu item text.
                menuItem.Text = text;

                //	If this child merge menu entry has any child merge menu entries, recursively
                //	create their menu items.
                MenuItem[] childMenuItems = mergeMenuEntry.CreateMenuItems();
                if (childMenuItems != null)
                    menuItem.MenuItems.AddRange(childMenuItems);

                //	Add the separator menu item, as needed.
                if (separatorBefore)
                    menuItemArrayList.Add(MakeSeparatorMenuItem(menuBuilder.MenuType));

                //	Add the menu item to the array of menu items being returned.
                menuItemArrayList.Add(menuItem);

                //	Add the separator menu item, as needed.
                if (separatorAfter)
                    menuItemArrayList.Add(MakeSeparatorMenuItem(menuBuilder.MenuType));
            }

            //	Done.  Convert the array list into a MenuItem array and return it.
            return (MenuItem[])menuItemArrayList.ToArray(typeof(MenuItem));
        }

        /// <summary>
        /// Helper to make a separator menu item.
        /// </summary>
        /// <returns>A MenuItem that is a separator MenuItem.</returns>
        private static MenuItem MakeSeparatorMenuItem(MenuType menuType)
        {
            //	Instantiate the separator menu item.
            MenuItem separatorMenuItem = new OwnerDrawMenuItem(menuType);
            separatorMenuItem.Text = SEPARATOR_TEXT;
            return separatorMenuItem;
        }
    }
}
