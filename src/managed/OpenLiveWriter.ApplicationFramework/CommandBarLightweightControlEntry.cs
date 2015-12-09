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
    public class CommandBarLightweightControlEntry : CommandBarEntry
    {
        /// <summary>
        ///	The lightweight control for this CommandBarLightweightControlEntry.
        /// </summary>
        private LightweightControl lightweightControl;

        /// <summary>
        /// Gets or sets The lightweight control for this CommandBarLightweightControlEntry.
        /// </summary>
        /// <returns>Lightweight control.</returns>
        public LightweightControl LightweightControl
        {
            get
            {
                return lightweightControl;
            }
            set
            {
                lightweightControl = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlEntry class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarLightweightControlEntry(IContainer container)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlEntry class.
        /// </summary>
        public CommandBarLightweightControlEntry()
        {
        }

        /// <summary>
        /// Gets the control for this entry.
        /// </summary>
        /// <returns>Lightweight control.</returns>
        public override LightweightControl GetLightweightControl(CommandBarLightweightControl commandBarLightweightControl, bool rightAligned)
        {
            lightweightControl.LightweightControlContainerControl = commandBarLightweightControl;
            return lightweightControl;
        }
    }
}
