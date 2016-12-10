// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System.Drawing;

    using JetBrains.Annotations;

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
        void UpdateStatusBar([NotNull] string statusText);

        /// <summary>
        /// Update the contents of the Sidebar status bar with an image and a string.
        /// </summary>
        /// <param name="image">Status image</param>
        /// <param name="statusText">Status text</param>
        void UpdateStatusBar([NotNull] Image image, [NotNull] string statusText);
    }
}
