// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace Project31.ApplicationFramework
{
    /// <summary>
    /// MergeMenuEntry is an internal class used to build Command-based menus.
    /// </summary>
    internal class MergeMenuEntry
    {
        /// <summary>
        /// Used to indicate that the menu item should include a separator.  Example: -File@0
        /// </summary>
        private static string SEPARATOR_TEXT = "-";

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
        /// Initializes a new instance of the MergeMenuEntry class.
        /// </summary>
        public MergeMenuEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MergeMenuEntry class.  This constructor is used for
        /// top level and "container" menu items.
        /// </summary>
        /// <param name="position">Merge menu entry position.</param>
        /// <param name="text">Merge menu entry text.</param>
        public MergeMenuEntry(int position, string text)
        {
            this.position = position;
            this.text = text;
            this.command = null;
        }

        /// <summary>
        /// Initializes a new instance of the MergeMenuEntry class.  This constructor is used for
        /// the command menu entry.
        /// </summary>
        /// <param name="position">Merge menu entry position.</param>
        /// <param name="text">Merge menu entry text.</param>
        /// <param name="command">Merge menu entry command.</param>
        public MergeMenuEntry(int position, string text, Command command)
        {
            this.position = position;
            this.text = text;
            this.command = command;
        }

        /// <summary>
        /// Indexer for access child merge menu entries.
        /// </summary>
        public MergeMenuEntry this [int position, string text]
        {
            get
            {
                string key = String.Format("{0}-{1}", position.ToString("D3"), text);
                return (MergeMenuEntry)childMergeMenuEntries[key];
            }
            set
            {
                string key = String.Format("{0}-{1}", position.ToString("D3"), text);
                childMergeMenuEntries[key] = value;
            }
        }

#if false
        /// <summary>
        /// Creates and returns a set of menu items from the child merge menu entries in this merge
        /// menu entry.
        /// </summary>
        /// <returns>Array of menu items.</returns>
        public MenuItem[] CreateMenuItems(bool mainMenu)
        {
            return CreateMenuItems(mainMenu);
        }
#endif

        /// <summary>
        /// Creates and returns a set of menu items from the child merge menu entries in this merge
        /// menu entry.
        /// </summary>
        /// <param name="mainMenu">The level at which the MenuItems will appear.</param>
        /// <returns>Array of menu items.</returns>
        public MenuItem[] CreateMenuItems(bool mainMenu)
        {
            //	If this merge menu entry has no child merge menu entries, return null.
            if (childMergeMenuEntries.Count == 0)
                return null;

            //	Construct an array list to hold the menu items being created.
            ArrayList menuItemArrayList = new ArrayList();

            //	Enumerate the child merge menu entries of this merge menu entry.
            foreach (MergeMenuEntry mergeMenuEntry in childMergeMenuEntries.Values)
            {
                //	Get the text of the merge menu entry.
                string text = mergeMenuEntry.Text;

                //	Create the menu item for this child merge menu entry.
                MenuItem menuItem;
                if (mainMenu)
                    menuItem = new OwnerDrawMenuItem();
                else
                {
                    //	If the text of the merge menu entry specifies that a separator menu item
                    //	should appear before it, insert a separator menu item.
                    if (text.StartsWith(SEPARATOR_TEXT))
                    {
                        //	Strip off the SEPARATOR_TEXT.
                        text = text.Substring(1);

                        //	Instantiate the separator menu item.
                        MenuItem separatorMenuItem = new OwnerDrawMenuItem();
                        separatorMenuItem.Text = SEPARATOR_TEXT;

                        //	Add the separator menu item to the array of menu items being returned.
                        menuItemArrayList.Add(separatorMenuItem);
                    }

                    //	Instantiate the menu item.
                    if (mergeMenuEntry.Command == null)
                        menuItem = new OwnerDrawMenuItem();
                    else
                        menuItem = new CommandOwnerDrawMenuItem(mergeMenuEntry.Command);
                }

                //	Set the menu item text.
                menuItem.Text = text;

                //	If this child merge menu entry has any child merge menu entries, recursively
                //	create their menu items.
                MenuItem[] childMenuItems = mergeMenuEntry.CreateMenuItems(false);
                if (childMenuItems != null)
                    menuItem.MenuItems.AddRange(childMenuItems);

                //	Add the menu item to the array of menu items being returned.
                menuItemArrayList.Add(menuItem);
            }

            //	Done.  Convert the array list into a MenuItem array and return it.
            return (MenuItem[])menuItemArrayList.ToArray(typeof(MenuItem));
        }
    }
}
