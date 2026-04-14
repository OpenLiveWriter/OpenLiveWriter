// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Helpers for managing drag operations
    /// </summary>
    public class DragDropHelper
    {
        /// <summary>
        /// Compare the current point with the point at which the mouse went down to see
        /// if we are outside the DragSize region
        /// </summary>
        /// <param name="point">current point</param>
        /// <param name="mouseDownPoint">point where the mouse went down</param>
        /// <returns>true if the point is outside the d2rag size, otherwise false</returns>
        public static bool PointOutsideDragSize(Point point, Point mouseDownPoint)
        {
            return (Math.Abs(mouseDownPoint.X - point.X) > (SystemInformation.DragSize.Width / 2)) ||
                (Math.Abs(mouseDownPoint.Y - point.Y) > (SystemInformation.DragSize.Height / 2));
        }

        /// <summary>
        /// Convenience method to determine whether the AllowedEffect property specifies Copy.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the Copy flag is set; false otherwise.</returns>
        public static bool IsAllowedEffectCopy(System.Windows.Forms.DragEventArgs e)
        {
            return (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy;
        }

        /// <summary>
        /// Convenience method to determine whether the AllowedEffect property specifies Move.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the Move flag is set; false otherwise.</returns>
        public static bool IsAllowedEffectMove(System.Windows.Forms.DragEventArgs e)
        {
            return (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move;
        }

        /// <summary>
        /// Convenience method to determine whether the AllowedEffect property specifies Link.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the Move flag is set; false otherwise.</returns>
        public static bool IsAllowedEffectLink(System.Windows.Forms.DragEventArgs e)
        {
            return (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link;
        }

        /// <summary>
        /// Convenience method to determine whether the Effect property specifies Copy.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the Copy flag is set; false otherwise.</returns>
        public static bool IsEffectCopy(System.Windows.Forms.DragEventArgs e)
        {
            return (e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy;
        }

        /// <summary>
        /// Convenience method to determine whether the Effect property specifies Link.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the Link flag is set; false otherwise.</returns>
        public static bool IsEffectLink(System.Windows.Forms.DragEventArgs e)
        {
            return (e.Effect & DragDropEffects.Link) == DragDropEffects.Link;
        }

        /// <summary>
        /// Convenience method to determine whether the Effect property specifies Move.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the Move flag is set; false otherwise.</returns>
        public static bool IsEffectMove(System.Windows.Forms.DragEventArgs e)
        {
            return (e.Effect & DragDropEffects.Move) == DragDropEffects.Move;
        }

        /// <summary>
        /// Convenience method to determine whether the KeyState property specifies right mouse button.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the KeyState property specifies right mouse button; false otherwise.</returns>
        public static bool IsKeyStateRightMouseButton(System.Windows.Forms.DragEventArgs e)
        {
            return (e.KeyState & 2) == 2;
        }

        /// <summary>
        /// Convenience method to determine whether the KeyState property specifies Move.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the KeyState property specifies Move; false otherwise.</returns>
        public static bool IsKeyStateMove(System.Windows.Forms.DragEventArgs e)
        {
            return (e.KeyState & 4) == 4;
        }

        /// <summary>
        /// Convenience method to determine whether the KeyState property specifies Copy.
        /// </summary>
        /// <param name="e">DragEventArgs to inspect.</param>
        /// <returns>True if the KeyState property specifies Copy; false otherwise.</returns>
        public static bool IsKeyStateCopy(System.Windows.Forms.DragEventArgs e)
        {
            return (e.KeyState & 8) == 8;
        }
    }
}
