// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Cross-platform interface for sidebar editors of SmartContent.
    /// Implementations must provide UI for editing SmartContent properties.
    /// On Windows, the concrete implementation is SmartContentEditor (a UserControl).
    /// </summary>
    public interface ISmartContentEditor
    {
        /// <summary>
        /// Get or set the currently selected SmartContent object. The editor should adapt
        /// its state to the current selection when this property changes (notification
        /// of the change is provided via the SelectedContentChanged event).
        /// </summary>
        ISmartContent SelectedContent { get; set; }

        /// <summary>
        /// Notification that the currently selected SmartContent object
        /// has changed.
        /// </summary>
        event EventHandler SelectedContentChanged;

        /// <summary>
        /// Event fired by the SmartContentEditor whenever it makes a change
        /// to the underlying properties of the SmartContent.
        /// </summary>
        event EventHandler ContentEdited;
    }
}
