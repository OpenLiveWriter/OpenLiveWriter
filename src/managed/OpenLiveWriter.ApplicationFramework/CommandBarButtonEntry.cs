// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Command bar button entry.
    /// </summary>
    [
        DesignTimeVisible(false),
            ToolboxItem(false)
    ]
    public class CommandBarButtonEntry : CommandBarEntry
    {
        /// <summary>
        /// The command identifier for this command bar button entry.
        /// </summary>
        private string commandIdentifier;

        /// <summary>
        /// Gets or sets the command identifier for this command bar button entry.
        /// </summary>
        [
            Category("Design"),
                Localizable(false),
                Description("The command identifier of the command for this command bar button.")
        ]
        public string CommandIdentifier
        {
            get
            {
                return commandIdentifier;
            }
            set
            {
                commandIdentifier = value;
            }
        }

        /// <summary>
        /// The control for this entry.
        /// </summary>
        private CommandBarButtonLightweightControl commandBarButtonLightweightControl;

        /// <summary>
        /// Initializes a new instance of the CommandBarButtonEntry class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarButtonEntry(IContainer container)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarButtonEntry class.
        /// </summary>
        public CommandBarButtonEntry()
        {
        }

        /// <summary>
        /// Gets the control for this entry.
        /// </summary>
        /// <returns>Lightweight control.</returns>
        public override LightweightControl GetLightweightControl(CommandBarLightweightControl commandBarLightweightControl, bool rightAligned)
        {
            if (commandBarButtonLightweightControl == null)
                commandBarButtonLightweightControl = new CommandBarButtonLightweightControl(commandBarLightweightControl, commandIdentifier, rightAligned);
            return commandBarButtonLightweightControl;
        }
    }
}
