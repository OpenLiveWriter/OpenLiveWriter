// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;

    /// <summary>
    /// Resize capabilities for a SmartContentSource.
    /// </summary>
    [Flags]
    public enum ResizeCapabilities
    {
        /// <summary>
        /// SmartContentSource is not resizable (size grippers will not appear when the object is selected in the editor).
        /// </summary>
        None = 0,

        /// <summary>
        /// SmartContentSource is resizable (size grippers will appear when the object is selected within the editor).
        /// If this flag is specified as part of ResizeCapabilities then the OnResizeComplete method should also be
        /// overridden to update the ISmartContent as necessary with the new size of the SmartContent object.
        /// </summary>
        Resizable = 1,

        /// <summary>
        /// Preserve the aspect ratio of the object during resizing. The default aspect ratio to be enforced is the
        /// ratio of the object prior to resizing. If the desired aspect ratio is statically known it is highly recommended
        /// that this ratio be specified within an override of the OnResizeStart method (will eliminate the problem
        /// of "creeping" change to the aspect ratios with continued resizing).
        /// </summary>
        PreserveAspectRatio = 2,

        /// <summary>
        /// Update the appearance of the smart content object in real time as the user resizes the object. If this
        /// flag is specified then the OnResizing method should be overridden to update the state of the ISmartContent
        /// object as resizing occurs. The editor will first call this method and then call the GenerateEditorHtml
        /// method to update the display as the user resizes.
        /// </summary>
        LiveResize = 4
    }
}