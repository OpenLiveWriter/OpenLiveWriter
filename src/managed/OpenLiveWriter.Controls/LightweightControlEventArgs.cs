// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Provides data for LightweightControl events.
    /// </summary>
    public class LightweightControlEventArgs : EventArgs
    {
        /// <summary>
        /// The LightweightControl.
        /// </summary>
        private LightweightControl lightweightControl;

        /// <summary>
        /// Gets the LightweightControl.
        /// </summary>
        public LightweightControl LightweightControl
        {
            get
            {
                return lightweightControl;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ObjectStoreEventArgs class.
        /// </summary>
        /// <param name="lightweightControl">The LightweightControl.</param>
        public LightweightControlEventArgs(LightweightControl lightweightControl)
        {
            this.lightweightControl = lightweightControl;
        }
    }
}
