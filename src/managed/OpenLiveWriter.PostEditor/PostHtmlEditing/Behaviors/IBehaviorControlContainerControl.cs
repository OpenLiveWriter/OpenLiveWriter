// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Summary description for IBehaviorControlContainerControl.
    /// </summary>
    public interface IBehaviorControlContainerControl
    {
        /// <summary>
        /// Gets the BehaviorControlCollection for the IBehaviorControlContainerControl.
        /// </summary>
        BehaviorControlCollection Controls
        {
            get;
        }

        /// <summary>
        /// Gets the parent behavior for the IBehaviorControlContainerControl.
        /// </summary>
        ElementControlBehavior Parent
        {
            get;
        }

        /// <summary>
        /// Temporarily suspends the layout logic for the IBehaviorControlContainerControl.
        /// </summary>
        void SuspendLayout();

        /// <summary>
        /// Resumes normal layout logic for the IBehaviorControlContainerControl.
        /// </summary>
        void ResumeLayout();

        /// <summary>
        /// Forces the IBehaviorControlContainerControl to apply layout logic to its child lightweight controls.
        /// </summary>
        void PerformLayout();

        /// <summary>
        /// Translates a virtual client rectangle to a parent rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to translate.</param>
        /// <returns>The translated rectangle.</returns>
        Rectangle VirtualClientRectangleToParent(Rectangle rectangle);

        /// <summary>
        /// Translates a point to be relative to the the virtual client rectangle.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        Point PointToVirtualClient(Point point);

        /// <summary>
        /// Translates a virtual client point to be relative to a parent point.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        Point VirtualClientPointToParent(Point point);
    }
}
