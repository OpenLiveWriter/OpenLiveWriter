// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Interface for a lightweight control container.
    /// </summary>
    public interface ILightweightControlContainerControl : IToolTipDisplay
    {
        LightweightControl ActiveLightweightControl { get; set; }

        /// <summary>
        /// Gets the LightweightControlCollection for the ILightweightControlContainerControl.
        /// </summary>
        LightweightControlCollection LightweightControls
        {
            get;
        }

        /// <summary>
        /// Gets the parent control for the ILightweightControlContainerControl.
        /// </summary>
        Control Parent
        {
            get;
        }

        /// <summary>
        /// Temporarily suspends the layout logic for the ILightweightControlContainerControl.
        /// </summary>
        void SuspendLayout();

        /// <summary>
        /// Resumes normal layout logic for the ILightweightControlContainerControl.
        /// </summary>
        void ResumeLayout();

        /// <summary>
        /// Causes the ILightweightControlContainerControl to redraw the invalidated regions within its client area.
        /// </summary>
        void Update();

        /// <summary>
        /// Forces the ILightweightControlContainerControl to apply layout logic to its child lightweight controls.
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

        /// <summary>
        /// Translates a virtual client point to be relative to a screen point.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        Point VirtualClientPointToScreen(Point point);
    }
}
