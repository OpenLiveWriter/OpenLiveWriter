// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Defines the abstract interface for an object that can display tool tips.
    /// </summary>
    public interface IToolTipDisplay
    {
        /// <summary>
        /// Sets the tooltip.
        /// </summary>
        /// <param name="toolTipText">The tooltip text, or null if no tooltip text should be displayed.</param>
        void SetToolTip(string toolTipText);
    }
}
