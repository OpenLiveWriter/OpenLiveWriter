// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Represents a collection of CommandBarEntry objects.
    /// </summary>
    [Editor(typeof(CommandBarEntryCollectionEditor), typeof(UITypeEditor))]
    public class CommandBarEntryCollection : List<CommandBarEntry>
    {
    }
}
