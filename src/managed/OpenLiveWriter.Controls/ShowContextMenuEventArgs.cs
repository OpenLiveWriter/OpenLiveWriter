// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Provides data for the ShowContextMenu events.
    /// </summary>
    public class ShowContextMenuEventArgs : EventArgs
    {
        #region Private Member Variables

        /// <summary>
        /// The context menu being shown.
        /// </summary>
        private ContextMenu contextMenu;

        /// <summary>
        /// The horizontal position of the context menu, in screen coordinates.
        /// </summary>
        private int x;

        /// <summary>
        /// The vertical position of the context menu, in screen coordinates.
        /// </summary>
        private int y;

        /// <summary>
        /// A value indicating whether the event was handled.
        /// </summary>
        private bool handled;

        #endregion Private Member Variables

        #region Public Properties

        /// <summary>
        /// Gets or sets the context menu being shown.
        /// </summary>
        public ContextMenu ContextMenu
        {
            get
            {
                return contextMenu;
            }
            set
            {
                contextMenu = value;
            }
        }

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
        /// Gets or sets a value indicating whether the event was handled.
        /// </summary>
        public bool Handled
        {
            get
            {
                return handled;
            }
            set
            {
                handled = value;
            }
        }

        #endregion Public Properties

        #region Class Initialization

        /// <summary>
        /// Initializes a new instance of the ShowContextMenuEventArgs class.
        /// </summary>
        public ShowContextMenuEventArgs(int x, int y, ContextMenu contextMenu)
        {
            this.x = x;
            this.y = y;
            this.contextMenu = contextMenu;
        }

        #endregion Class Initialization
    }
}
