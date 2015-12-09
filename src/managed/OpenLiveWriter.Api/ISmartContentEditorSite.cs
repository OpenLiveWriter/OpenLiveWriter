// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Connection between a SmartContent sidebar editor and its context. Provides
    /// notification of inline image resizing and allows the editor to manipulate
    /// the status bar of the Sidebar.
    /// </summary>
    public interface ISmartContentEditorSite
    {
        /// <summary>
        /// Occurs when the SmartContent is resized directly by the user inline.
        /// </summary>
        event ContentResizedEventHandler ContentResized;

        /// <summary>
        /// Update the contents of the Sidebar status bar with a string.
        /// </summary>
        /// <param name="statusText">Status text</param>
        void UpdateStatusBar(string statusText);

        /// <summary>
        /// Update the contents of the Sidebar status bar with an image and a string.
        /// </summary>
        /// <param name="image">Status image</param>
        /// <param name="statusText">Status text</param>
        void UpdateStatusBar(Image image, string statusText);
    }

    /// <summary>
    /// Event handler that is fired when a content is resized.
    /// </summary>
    /// <param name="newSize">The new size of the content</param>
    /// <param name="completed">Whether the sizing operation has completed or not</param>
    public delegate void ContentResizedEventHandler(Size newSize, bool completed);
}
