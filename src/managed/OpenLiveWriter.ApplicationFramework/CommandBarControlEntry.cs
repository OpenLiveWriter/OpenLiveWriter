// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Command bar control entry.  Allows any control to be added to a command bar definition.
    /// </summary>
    [
        DesignTimeVisible(false),
            ToolboxItem(false)
    ]
    public class CommandBarControlEntry : CommandBarEntry
    {
        /// <summary>
        ///	The control for this CommandBarControlEntry.
        /// </summary>
        private Control control;

        /// <summary>
        /// The CommandBarControlLightweightControl for this entry.
        /// </summary>
        private CommandBarControlLightweightControl commandBarControlLightweightControl;

        /// <summary>
        /// Initializes a new instance of the CommandBarControlEntry class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarControlEntry(IContainer container)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlEntry class.
        /// </summary>
        public CommandBarControlEntry()
        {
        }

        /// <summary>
        ///	Gets or sets the control for this CommandBarControlEntry.
        /// </summary>
        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                control = value;
            }
        }

        /// <summary>
        /// Gets the lightweight control for this entry.
        /// </summary>
        /// <returns>Lightweight control.</returns>
        public override LightweightControl GetLightweightControl(CommandBarLightweightControl commandBarLightweightControl, bool rightAligned)
        {
            if (commandBarControlLightweightControl == null)
                commandBarControlLightweightControl = new CommandBarControlLightweightControl(control);
            return commandBarControlLightweightControl;
        }
    }
}
