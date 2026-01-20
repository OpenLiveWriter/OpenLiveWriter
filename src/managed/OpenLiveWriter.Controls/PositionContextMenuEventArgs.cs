// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Provides data for the PositionContextMenu events.
    /// </summary>
    public class PositionContextMenuEventArgs : System.EventArgs
    {
        /// <summary>
        /// The horizontal position of the context menu, in screen coordinates.
        /// </summary>
        private int x;

        /// <summary>
        /// The vertical position of the context menu, in screen coordinates.
        /// </summary>
        private int y;

        /// <summary>
        /// Gets or sets the horizontal position of the context menu, in screen coordinates.
        /// </summary>
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical position of the context menu, in screen coordinates.
        /// </summary>
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the PositionContextMenuEventArgs class.
        /// </summary>
        public PositionContextMenuEventArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
