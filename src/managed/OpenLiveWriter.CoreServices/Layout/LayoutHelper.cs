// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.Layout
{
    public class LayoutHelper
    {
        public static void FixupOKCancel(Button buttonOK, Button buttonCancel)
        {
            EqualizeButtonWidthsHoriz(AnchorStyles.Right, buttonCancel.Width, int.MaxValue, buttonOK, buttonCancel);
        }

        public static void FixupGroupBox(GroupBox groupBox)
        {
            FixupGroupBox(8, groupBox);
        }

        public static void FixupGroupBox(int pixelsBetween, GroupBox groupBox)
        {
            NaturalizeHeightAndDistribute(pixelsBetween, groupBox.Controls);
            AutoSizeGroupBox(groupBox);
        }

        public static void AutoSizeGroupBox(GroupBox groupBox)
        {
            ArrayList childControls = new ArrayList(groupBox.Controls);
            if (childControls.Count < 1)
                return;
            childControls.Sort(new SortByVerticalPosition());
            groupBox.Height = ((Control)childControls[childControls.Count - 1]).Bottom + 12;
        }

        /// <summary>
        /// Naturalizes height, then distributes vertically ACCORDING TO THEIR Y COORDINATE
        /// </summary>
        public static void NaturalizeHeightAndDistribute(int pixelsBetween, Control.ControlCollection controls)
        {
            Control[] carr = (Control[])new ArrayList(controls).ToArray(typeof(Control));
            NaturalizeHeight(carr);
            DistributeVertically(pixelsBetween, true, carr);
        }

        /// <summary>
        /// Naturalizes height, then distributes vertically ACCORDING TO THE ORDER YOU PASSED THEM IN
        /// </summary>
        public static void NaturalizeHeightAndDistribute(int pixelsBetween, params object[] controls)
        {
            NaturalizeHeight(controls);
            DistributeVertically(pixelsBetween, false, controls);
        }

        /// <param name="controls">Objects of type Control or ControlGroup</param>
        public static void NaturalizeHeight(params object[] controls)
        {
            foreach (object o in controls)
            {
                if (!(o is ControlGroup))
                {
                    Control c = (Control)o;

                    // TODO: Fix Windows Forms RTL mirroring!
                    // This alignRight code below is incorrect and may cause a control's location to inadvertently change! Unfortunately,
                    // I'm leaving it there for backwards compatibility. Its very hard to figure out if a control should be aligned right
                    // by just looking at its RightToLeft and Anchor property. For instance:
                    //
                    //      Form form1 = new Form();
                    //      form1.RightToLeft = RightToLeft.Yes;
                    //      form1.RightToLeftLayout = true;
                    //
                    //      Label label1 = new Label();
                    //      label1.Anchor = AnchorStyles.Left;
                    //      label1.RightToLeft = RightToLeft.Yes;
                    //
                    //      Panel panel1 = new Panel();
                    //      panel1.RightToLeft = RightToLeft.Yes;
                    //
                    //      Label label2 = new Label();
                    //      label2.Anchor = AnchorStyles.Left;
                    //      label2.RightToLeft = RightToLeft.Yes;
                    //
                    //      form1.Controls.Add(label1);
                    //      panel1.Controls.Add(label2);
                    //      form1.Controls.Add(panel1);
                    //
                    //      BidiHelper.RtlLayoutFixup(form1); // Basically a no-op, we'll let the form be auto-mirrored by WinForms.
                    //      BidiHelper.RtlLayoutFixup(panel1); // We'll do work in here to fix up the panel.
                    //
                    //      // label1's parent is a Form with Form.RightToLeftLayout == true so it will be automatically mirrored by
                    //      // WinForms. It's Anchor won't actually change value, but when WinForms lays it out, it will *act* like it
                    //      // has its Anchor set to AnchorStyles.Right.
                    //      Debug.Assert(label1.Parent == form1);
                    //      Debug.Assert(label1.Anchor == AnchorStyles.Left);
                    //
                    //      // label2's parent is a Panel whose parent is a Form with Form.RightToLeftLayout == true, however the
                    //      // controls inside panel1 will *not* get automatically mirrored by WinForms. In the call to
                    //      // BidiHelper.RtlLayoutFixup we'll manually flip label2's Anchor property to force it to act like it's RTL.
                    //      Debug.Assert(label2.Parent == panel1);
                    //      Debug.Assert(panel1.Parent == form1);
                    //      Debug.Assert(label2.Anchor == AnchorStyles.Right);
                    //
                    // See http://www.microsoft.com/middleeast/msdn/WinFormsAndArabic.aspx#_Toc136842131 for more information regarding
                    // the interesting side effects of RTL in WinForms.
                    bool alignRight;
                    if (c.RightToLeft != RightToLeft.Yes)
                        alignRight = (c.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) == AnchorStyles.Right;
                    else
                        alignRight = (c.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) != AnchorStyles.Left;

                    Size newSize = GetNaturalSize(c);
                    int deltaW = c.Width - newSize.Width;
                    c.Size = newSize;
                    if (alignRight)
                        c.Left += deltaW;
                }
            }
        }

        public static void DistributeVertically(int pixelsBetween, params object[] controls)
        {
            DistributeVertically(pixelsBetween, false, controls);
        }

        public static void DistributeVertically(int pixelsBetween, bool sortByVerticalPosition, params object[] controls)
        {
            if (controls.Length < 2)
                return;
            /*
                        Control parent = controls[0].Parent;
            #if DEBUG
                        for (int i = 1; i < controls.Length; i++)
                            if (parent != controls[i].Parent)
                                Debug.Fail("LayoutHelper.DistributeVertically() called on controls with different parents");
            #endif
            */

            pixelsBetween = Ceil(DisplayHelper.ScaleY(pixelsBetween));
            if (sortByVerticalPosition)
                Array.Sort(controls, new SortByVerticalPosition());
            int pos = ControlAdapter.Create(controls[0]).Bottom + pixelsBetween;
            for (int i = 1; i < controls.Length; i++)
            {
                ControlAdapter ca = ControlAdapter.Create(controls[i]);
                if (ca.Visible || !sortByVerticalPosition)
                {
                    ca.Top = pos;
                    pos = ca.Bottom + pixelsBetween;
                }
            }
        }

        public static void DistributeHorizontally(int pixelsBetween, params object[] controls)
        {
            if (controls.Length < 2)
                return;
            /*
                        Control parent = controls[0].Parent;
            #if DEBUG
                        for (int i = 1; i < controls.Length; i++)
                            if (parent != controls[i].Parent)
                                Debug.Fail("LayoutHelper.DistributeHorizontally() called on controls with different parents");
            #endif
            */

            pixelsBetween = Ceil(DisplayHelper.ScaleX(pixelsBetween));
            Array.Sort(controls, new SortByHorizontalPosition());
            int pos = ControlAdapter.Create(controls[0]).Right + pixelsBetween;
            for (int i = 1; i < controls.Length; i++)
            {
                ControlAdapter ca = ControlAdapter.Create(controls[i]);
                if (ca.Visible)
                {
                    ca.Left = pos;
                    pos = ca.Right + pixelsBetween;
                }
            }
        }

        public static Size GetNaturalSize(object o)
        {
            // TODO: Reimplement our usage of Windows Forms!
            // This method is just a band-aid over the real problem that we don't properly use Windows Forms. By using
            // TextRender.MeasureText, we are estimating the size of the WinForms control, but our estimate can be wrong if
            // the control is drawn with different TextFormatFlags than the ones used to measure the control. This entire
            // method is really just an educated guess of the size of the control. We should not be calling this method on the
            // same control multiple times and it should NOT be called during a control's OnLoad (which we do in many places)
            // due to performance issues. We should be able to work around this by using the Control.AutoSize property along
            // with anchoring and docking in TableLayoutPanels and FlowLayoutPanels.

            Debug.Assert(o is Control || o is ControlGroup, "GetNaturalSize called with invalid value");

            if (o is ControlGroup)
            {
                return ((ControlGroup)o).Size;
            }

            Control c = (Control)o;

            if (!(c is CheckBox || c is RadioButton || c is Label))
            {
                return c.Size;
            }

            int width = -1;
            bool useGdi;
            TextFormatFlags formatFlags = TextFormatFlags.WordBreak;
            int minHeight = 0;
            int measuredWidthPadding = 0;
            int measuredHeightPadding = 0;

            if (c is CheckBox || c is RadioButton)
            {
                width = c.Width;
                useGdi = !((ButtonBase)c).UseCompatibleTextRendering || ((ButtonBase)c).FlatStyle == FlatStyle.System;

                Size proposedSize = new Size(c.Width, int.MaxValue);
                Size prefSize = c.GetPreferredSize(proposedSize);
                minHeight = prefSize.Height;
                Size textOnly;
                if (useGdi)
                    textOnly = TextRenderer.MeasureText(c.Text, c.Font);
                else
                {
                    Debug.Assert(useGdi, "Use FlatStyle.System for control " + c.Name);
                    using (Graphics g = c.CreateGraphics())
                        textOnly = Size.Ceiling(g.MeasureString(c.Text, c.Font));
                }
                measuredWidthPadding = prefSize.Width - textOnly.Width;
                measuredHeightPadding = (prefSize.Height - textOnly.Height);
                width -= measuredWidthPadding;
            }
            else if (c is LinkLabel)
            {
                // In .NET 1.1, LinkLabel was always GDI+.
                // In .NET 2.0, LinkLabel:
                // - Is always GDI+ if the link area doesn't cover the whole link
                // - Is always GDI+ if UseCompatibleTextRendering is true
                // Both LinkLabel and Label do c.GetPreferredSize without wrapping if FlatStyle = FlatStyle.System

                LinkLabel link = (LinkLabel)c;

                if (link.FlatStyle != FlatStyle.System)
                    return c.GetPreferredSize(new Size(c.Width, 0));

                useGdi = !link.UseCompatibleTextRendering;
            }
            else if (c is Label)
            {
                // Both LinkLabel and Label do c.GetPreferredSize without wrapping if FlatStyle = FlatStyle.System
                //

                if (((Label)c).FlatStyle != FlatStyle.System)
                    return c.GetPreferredSize(new Size(c.Width, 0));

                useGdi = true;

                if (!((Label)c).UseMnemonic)
                    formatFlags |= TextFormatFlags.NoPrefix;
            }
            else
            {
                Debug.Fail("This should never happen");
                throw new ArgumentException("This should never happen");
            }

            if (width < 0)
                width = c.Width;

            Font font = c.Font;
            string text = c.Text;
            Size measuredSize;
            if (!useGdi)
            {
                Debug.Fail(string.Format(CultureInfo.InvariantCulture, "Control {0} is unexpectedly using GDI+.", c.Name));
                // using (Graphics g = c.CreateGraphics())
                //   measuredSize = GetNaturalSizeInternal(g, text, font, width);
            }

            measuredSize = TextRenderer.MeasureText(text, font, new Size(width, 0), formatFlags);

            // The +1 is because Windows Forms likes to be sneaky and shrink Labels by 1 pixel when the label becomes visible,
            // which is just enough to make the label force a line break when measuring text with TextFormatFlags.TextBoxControl
            // specified. This adds an extra line of whitespace below the Label, which is not what we want. To work around this,
            // we allow the measuredSize to grow the control by at most one pixel, otherwise we will remeasure the control and
            // force it to stay within the width we have provided.
            if (measuredSize.Width > width + 1)
            {
                Debug.WriteLine("BUG: TextRenderer.MeasureText returned a larger width than expected: " + new StackTrace());

                // WinLive 116755: Using the following flags forces the TextRenderer to measure text with correct character-wrapping.
                // Without these flags, its possible for the TextRenderer to return a width wider than the proposed width if the text
                // contains a very long word. I used the TextFormatFlags Utility (TextRendering.exe) from the March 2006 MSDN Magazine
                // article "Build World-Ready Apps Using Complex Scripts In Windows Forms Controls" to help visualize what each flag
                // does: http://download.microsoft.com/download/f/2/7/f279e71e-efb0-4155-873d-5554a0608523/TextRendering.exe
                formatFlags |= TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;
                measuredSize = TextRenderer.MeasureText(text, font, new Size(width, 0), formatFlags);

                Debug.Assert(measuredSize.Width <= width, "TextRenderer.MeasureText returned a larger width than expected, text may be clipped!");
            }

            measuredSize.Height = Math.Max(minHeight, measuredSize.Height + measuredHeightPadding);
            return new Size(measuredSize.Width + measuredWidthPadding, measuredSize.Height);

        }

        public static IDisposable SuspendAnchoring(params Control[] controls)
        {
            return new AnchorSuspension(controls);
        }

        private class AnchorSuspension : IDisposable
        {
            private Control[] _controls;
            private readonly AnchorStyles[] _anchorStyles;

            public AnchorSuspension(Control[] controls)
            {
                _anchorStyles = new AnchorStyles[controls.Length];
                _controls = controls;
                for (int i = 0; i < _controls.Length; i++)
                {
                    Control c = _controls[i];
                    if (c != null)
                    {
                        _anchorStyles[i] = c.Anchor;
                        c.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    }
                }
            }

            public void Dispose()
            {
                Control[] controls = _controls;
                _controls = null;
                if (controls != null)
                {
                    for (int i = 0; i < controls.Length; i++)
                    {
                        Control c = controls[i];
                        if (c != null)
                        {
                            c.Anchor = _anchorStyles[i];
                        }
                    }
                }
            }
        }

        private static Size GetNaturalSizeInternal(Graphics graphics, string text, Font font, int width)
        {
            return Size.Ceiling(graphics.MeasureString(text, font, width));
        }

        private static int Ceil(float f)
        {
            return (int)Math.Ceiling(f);
        }

        private class SortByVerticalPosition : IComparer
        {
            public int Compare(object x, object y)
            {
                return ControlAdapter.Create(x).Top - ControlAdapter.Create(y).Top;
            }
        }

        private class SortByHorizontalPosition : IComparer
        {
            public int Compare(object x, object y)
            {
                return ControlAdapter.Create(x).Left - ControlAdapter.Create(y).Left;
            }
        }

        public static void FitControlsBelow(int pixelPadding, Control c)
        {
            ArrayList controls = new ArrayList(c.Parent.Controls);
            controls.Sort(new SortByVerticalPosition());
            int cIndex = controls.IndexOf(c);
            if (cIndex < 0)
            {
                Debug.Fail("Invalid call to FitControlsBelow, the control was not found in the provided control collection");
            }
            else
            {
                if (cIndex == controls.Count - 1)
                {
                    // control is the bottommost control
                    return;
                }

                Control oneDown = (Control)controls[cIndex + 1];
                int delta = (c.Bottom + pixelPadding) - oneDown.Top;

                for (int i = cIndex + 1; i < controls.Count; i++)
                {
                    ((Control)controls[i]).Top += delta;
                }
            }
        }

        public static void EqualizeButtonWidthsVert(AnchorStyles anchorStyles, int minWidth, int maxWidth, params Button[] buttons)
        {
            switch (anchorStyles)
            {
                case AnchorStyles.Left:
                case AnchorStyles.Right:
                    break;
                default:
                    Trace.Fail("Invalid anchorStyles specified");
                    throw new ArgumentException("Invalid anchorStyles specified");
            }

            int newWidth = DisplayHelper.GetMaxDesiredButtonWidth(false, buttons);
            newWidth = Math.Max(minWidth, Math.Min(maxWidth, newWidth));

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                int delta = newWidth - button.Width;
                button.Width = newWidth;
                if (anchorStyles == AnchorStyles.Right)
                    button.Left -= delta;
            }
        }

        public static void EqualizeButtonWidthsHoriz(AnchorStyles anchorStyles, int minWidth, int maxWidth, params Button[] buttons)
        {
            switch (anchorStyles)
            {
                case AnchorStyles.Left:
                case AnchorStyles.Right:
                case AnchorStyles.None:
                    break;
                default:
                    Trace.Fail("Invalid anchorStyles specified");
                    throw new ArgumentException("Invalid anchorStyles specified");
            }

            int newWidth = DisplayHelper.GetMaxDesiredButtonWidth(false, buttons);
            newWidth = Math.Max(minWidth, Math.Min(maxWidth, newWidth));

            int[] deltas = new int[buttons.Length];

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                deltas[i] = newWidth - button.Width;
                button.Width = newWidth;
            }

            if (anchorStyles == AnchorStyles.Left)
            {
                int total = 0;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].Left += total;
                    total += deltas[i];
                }
            }
            else if (anchorStyles == AnchorStyles.Right)
            {
                int total = 0;
                for (int i = buttons.Length - 1; i >= 0; i--)
                {
                    total += deltas[i];
                    buttons[i].Left -= total;
                }
            }
        }

        public static int AutoFitLabels(params Label[] labels)
        {
            ControlGroup cg = new ControlGroup(labels);
            int startWidth = cg.Width;

            foreach (Label label in labels)
                DisplayHelper.AutoFitSystemLabel(label, 0, int.MaxValue);

            return cg.Width - startWidth;
        }
    }
}
