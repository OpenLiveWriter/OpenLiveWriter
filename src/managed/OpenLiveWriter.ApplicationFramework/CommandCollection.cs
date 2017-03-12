// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Represents a collection of commands.
    /// </summary>
    [Editor(typeof(CommandCollectionEditor), typeof(UITypeEditor))]
    public class CommandCollection : List<Command>
    {
        public CommandCollection() : base() { }

        public CommandCollection(int capacity) : base(capacity) { }

        public CommandCollection(IEnumerable<Command> commands) : base(commands) { }
    }
}
