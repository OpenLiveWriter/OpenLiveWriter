// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// A Panel that properly supports transparent background.
    /// Standard Panel doesn't have SupportsTransparentBackColor style set,
    /// which can cause rendering issues with transparent backgrounds.
    /// </summary>
    internal class TransparentPanel : Panel
    {
        public TransparentPanel()
        {
            // Use AllPaintingInWmPaint to ensure background paints before child controls
            // This prevents black areas on first render
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Fill with opaque group background to initialize buffer
            e.Graphics.Clear(RibbonColors.Current.GetOpaqueGroupBackground());
        }
    }

    /// <summary>
    /// A transparent spacer control that doesn't paint anything.
    /// Used to reserve space at the bottom of RibbonGroup for the label area
    /// without interfering with the custom painting done by RibbonRenderer.DrawGroup.
    /// </summary>
    internal class TransparentSpacer : Control
    {
        public TransparentSpacer()
        {
            // SupportsTransparentBackColor allows true transparency
            // Use UserPaint to prevent default background painting, but NOT AllPaintingInWmPaint
            // AllPaintingInWmPaint can block parent painting even when we don't paint anything
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.AllPaintingInWmPaint, false); // Explicitly disable to allow parent painting
            BackColor = Color.Transparent;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Don't paint anything - parent's OnPaint draws the label
            // Don't call base to avoid default painting
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Fill with opaque group background to prevent black on first render
            e.Graphics.Clear(RibbonColors.Current.GetOpaqueGroupBackground());
        }
    }
}
