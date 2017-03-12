// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Command-based menu builder.
    /// </summary>
    public sealed class MenuBuilder
    {
        /// <summary>
        /// Initializes a new instance of the MenuBuilder class.
        /// </summary>
        private MenuBuilder()
        {
        }

        public static MenuItem[] CreateMenuItems(CommandManager commandManager, CommandContextMenuDefinition commandContextMenuDefinition)
        {
            return CreateMenuItems(commandManager, commandContextMenuDefinition.CommandBar ? MenuType.CommandBarContext : MenuType.Context, commandContextMenuDefinition.Entries);
        }

        /// <summary>
        /// Creates menu items for the specified MenuDefinitionEntryCollection.
        /// </summary>
        ///	<param name="commandManager">The CommandManager to use.</param>
        /// <param name="menuDefinitionEntryCollection">The MenuDefinitionEntryCollection to create menu items for.</param>
        /// <returns>The menu items.</returns>
        public static MenuItem[] CreateMenuItems(CommandManager commandManager, MenuType menuType, MenuDefinitionEntryCollection menuDefinitionEntryCollection)
        {
            ArrayList menuItemArrayList = new ArrayList();
            for (int position = 0; position < menuDefinitionEntryCollection.Count; position++)
            {
                MenuItem[] menuItems = menuDefinitionEntryCollection[position].GetMenuItems(commandManager, menuType);
                if (menuItems != null)
                    menuItemArrayList.AddRange(menuItems);
            }

            // remove leading, trailing, and adjacent separators
            for (int i = menuItemArrayList.Count - 1; i >= 0; i--)
            {
                if (((MenuItem)menuItemArrayList[i]).Text == "-")
                {
                    if (i == 0 ||  // leading
                        i == menuItemArrayList.Count - 1 ||  // trailing
                        ((MenuItem)menuItemArrayList[i - 1]).Text == "-")  // adjacent
                    {
                        menuItemArrayList.RemoveAt(i);
                    }
                }
            }

            return (menuItemArrayList.Count == 0) ? null : (MenuItem[])menuItemArrayList.ToArray(typeof(MenuItem));
        }
    }
}
