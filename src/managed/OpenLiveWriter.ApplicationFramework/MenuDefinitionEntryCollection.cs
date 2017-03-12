// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Represents a collection of MenuDefinitionEntry objects.
    /// </summary>
    [Editor(typeof(MenuDefinitionEntryCollectionEditor), typeof(UITypeEditor))]
    public class MenuDefinitionEntryCollection : List<MenuDefinitionEntry>
    {
        /// <summary>
        /// Use strongly typed overload instead of this if possible!!
        /// </summary>
        public void Add(string commandIdentifier, bool separatorBefore, bool separatorAfter)
        {
            MenuDefinitionEntryCommand mde = new MenuDefinitionEntryCommand();
            mde.CommandIdentifier = commandIdentifier;
            mde.SeparatorBefore = separatorBefore;
            mde.SeparatorAfter = separatorAfter;
            Add(mde);
        }

        public void Add(CommandId commandIdentifier, bool separatorBefore, bool separatorAfter)
        {
            MenuDefinitionEntryCommand mde = new MenuDefinitionEntryCommand();
            mde.CommandIdentifier = commandIdentifier.ToString();
            mde.SeparatorBefore = separatorBefore;
            mde.SeparatorAfter = separatorAfter;
            Add(mde);
        }
        
    }
}
