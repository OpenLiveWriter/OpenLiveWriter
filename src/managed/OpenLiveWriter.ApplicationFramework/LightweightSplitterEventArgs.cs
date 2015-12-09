// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Lightweight splitter event arguments.
    /// </summary>
    public class LightweightSplitterEventArgs : EventArgs
    {
        /// <summary>
        /// The position of the splitter.
        /// </summary>
        private int position;

        /// <summary>
        /// Gets or sets the position of the splitter.
        /// </summary>
        public int Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the LightweightSplitterEventArgs class.
        /// </summary>
        /// <param name="position">The position of the splitter.</param>
        public LightweightSplitterEventArgs(int position)
        {
            this.position = position;
        }
    }
}
