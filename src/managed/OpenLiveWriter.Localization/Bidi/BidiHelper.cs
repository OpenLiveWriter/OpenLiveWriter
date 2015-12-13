// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace OpenLiveWriter.Localization.Bidi
{
    public interface IRtlAware
    {
        void Layout();
    }

    public class BidiHelper
    {
        public static bool IsRightToLeft
        {
            get
            {
                CultureInfo currentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                if (currentUICulture.IetfLanguageTag.ToUpperInvariant() == "PRS-AF")
                {
                    // WinLive 393987 - The "prs-af" Writer build isn't mirrored to RTL
                    // .NET 2.0 and 3.5 incorrectly define IsRightToLeft=false in the "prs-af" culture (though this is
                    // fixed in .NET 4.0). We need to override the call to .NET to return the correct value of true.
                    return true;
                }

                return currentUICulture.TextInfo.IsRightToLeft;
            }
        }

        public static MessageBoxOptions RTLMBOptions
        {
            get
            {
                if (IsRightToLeft)
                    return (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return (MessageBoxOptions)0;
            }
        }

        public static void RtlLayoutFixup(Control control)
        {
            RtlLayoutFixup(control, true);
        }

        public static void RtlLayoutFixup(Control control, bool recursive)
        {
            Control[] childControls = ToArray(control.Controls);
            RtlLayoutFixup(control, recursive, childControls);
        }

        private static Control[] ToArray(IList controls)
        {
            Control[] childControls = new Control[controls.Count];
            for (int i = 0; i < childControls.Length; i++)
                childControls[i] = (Control)controls[i];
            return childControls;
        }

        public static void RtlLayoutFixup(Control control, bool recursive, params Control[] childControls)
        {
            RtlLayoutFixup(control, recursive, false, childControls);
        }

        public static void RtlLayoutFixup(Control control, bool recursive, bool forceAutoLayout, IList childControls)
        {
            RtlLayoutFixup(control, recursive, forceAutoLayout, ToArray(childControls));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="control">The control to fixup.</param>
        /// <param name="recursive">Whether or not to fixup child controls as well.</param>
        /// <param name="forceAutoLayout">If true, ignores IRtlAware interface on the control param and lays out as normal. (IRtlAware will be used for children of control, regardless of this setting.)</param>
        /// <param name="childControls">The child controls to reposition (doesn't have to include all the children of control).</param>
        public static void RtlLayoutFixup(Control control, bool recursive, bool forceAutoLayout, params Control[] childControls)
        {
            if (IsRightToLeft && control.RightToLeft != RightToLeft.No)
            {
                if (!forceAutoLayout && control is IRtlAware)
                {
                    ((IRtlAware)control).Layout();
                }
                else
                {
                    bool isMirroredForm = control is Form
                                          && ((Form)control).RightToLeftLayout;

                    foreach (Control childControl in childControls)
                    {
                        if (!isMirroredForm)
                        {
                            childControl.Left = control.Width - childControl.Right;

                            switch (childControl.Dock)
                            {
                                case DockStyle.Left:
                                    childControl.Dock = DockStyle.Right;
                                    break;
                                case DockStyle.Right:
                                    childControl.Dock = DockStyle.Left;
                                    break;
                            }

                            if (childControl.Dock == DockStyle.None)
                            {
                                switch (childControl.Anchor & (AnchorStyles.Left | AnchorStyles.Right))
                                {
                                    case AnchorStyles.Left:
                                        childControl.Anchor &= ~AnchorStyles.Left;
                                        childControl.Anchor |= AnchorStyles.Right;
                                        break;
                                    case AnchorStyles.Right:
                                        childControl.Anchor &= ~AnchorStyles.Right;
                                        childControl.Anchor |= AnchorStyles.Left;
                                        break;
                                    case AnchorStyles.Left | AnchorStyles.Right:
                                        // do nothing
                                        break;
                                }
                            }
                        }

                        if (recursive)
                            RtlLayoutFixup(childControl);
                    }

                    if (!isMirroredForm)
                    {
                        int leftMargin = control.Margin.Left;
                        int rightMargin = control.Margin.Right;
                        if (leftMargin != rightMargin)
                        {
                            control.Margin = new Padding(
                                rightMargin,
                                control.Margin.Top,
                                leftMargin,
                                control.Margin.Bottom);
                        }

                        // NOTE: This handles ScrollableControl.DockPadding as well!
                        int leftPadding = control.Padding.Left;
                        int rightPadding = control.Padding.Right;
                        if (leftPadding != rightPadding)
                        {
                            control.Padding = new Padding(
                                rightPadding,
                                control.Padding.Top,
                                leftPadding,
                                control.Padding.Bottom);
                        }
                    }
                }
            }
        }

        public static Bitmap Mirror(Bitmap bitmap)
        {
            if (!IsRightToLeft)
                return bitmap;
            Bitmap mirrored = new Bitmap(bitmap);
            mirrored.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return mirrored;
        }

    }
}

