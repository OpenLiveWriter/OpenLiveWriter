// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using JetBrains.Annotations;

    /// <summary>
    /// Options which control resizing behavior.
    /// </summary>
    public class ResizeOptions
    {
        /// <summary>
        /// Gets or sets the ID of an HTML element that should be used as the "target" for resizing. This is useful
        /// in the case where the ISmartContent object is principally represented by a single element (such as an image)
        /// but which also contains other elements (such as an image caption line). In this case proportional sizing
        /// should apply to the image rather than the entire object's HTML. If a ResizableElementId is specified then
        /// the Size parameters passed to the OnResize methods refer to the size of this element rather than to
        /// the size of the entire SmartContent object.
        /// </summary>
        [CanBeNull]
        public string ResizeableElementId { get; set; } = null;

        /// <summary>
        /// Gets or sets the aspect ratio to be enforced if the ResizeCapabilities.PreserveAspectRatio flag is specified. If the
        /// desired aspect ratio is statically known it is highly recommended that this ratio be specified within
        /// the OnResizeStart method (will eliminate the problem of "creeping" change to the aspect ratios with continued resizing).
        /// </summary>
        public double AspectRatio { get; set; } = 0;
    }
}