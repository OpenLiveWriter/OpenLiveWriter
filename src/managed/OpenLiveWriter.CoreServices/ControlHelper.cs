// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    public delegate void ControlWalker(Control control, object state);

    public class ControlHelper
    {
        [Conditional("DEBUG")]
        public static void AttachLoggersForBoundsChange(Control c)
        {
            c.LocationChanged += (sender, args) => Debug.WriteLine(c.Name + " location changed: " + c.Location + "\r\n" + Environment.StackTrace);
            c.SizeChanged += (sender, args) => Debug.WriteLine(c.Name + " size changed: " + c.Size + "\r\n" + Environment.StackTrace);
        }

        public static bool ControlCanHandleInvoke(Control control)
        {
            return (control != null) && control.Created && control.IsHandleCreated && !control.Disposing && !control.IsDisposed;
        }

        public static bool BeginInvokeIfControlCanHandleInvoke(Control control, Delegate method)
        {
            if (ControlCanHandleInvoke(control))
            {
                control.BeginInvoke(method);
                return true;
            }

            return false;
        }

        public static void SetCueBanner(TextBoxBase textBox, string bannerText)
        {
            Color foreColor = textBox.ForeColor;
            Font font = textBox.Font;

            Font cueFont = font;

            textBox.GotFocus += delegate
                    {
                        if (textBox.Text == bannerText && textBox.ForeColor == SystemColors.GrayText)
                        {
                            textBox.Text = "";
                            textBox.ForeColor = foreColor;
                            textBox.Font = font;
                        }
                    };
            textBox.LostFocus += delegate
                    {
                        if (textBox.Text == "")
                        {
                            textBox.Text = bannerText;
                            textBox.ForeColor = SystemColors.GrayText;
                            textBox.Font = cueFont;
                        }
                    };
            textBox.TextChanged += delegate
                    {
                        if (!textBox.Focused && textBox.Text == "")
                        {
                            textBox.Text = bannerText;
                            textBox.ForeColor = SystemColors.GrayText;
                            textBox.Font = cueFont;
                        }
                        else
                        {
                            textBox.ForeColor = foreColor;
                            textBox.Font = font;
                        }
                    };

            if (textBox.Text == "" && !textBox.Focused)
            {
                textBox.Text = bannerText;
                textBox.ForeColor = SystemColors.GrayText;
                textBox.Font = cueFont;
            }
        }

        private static float AdjustScale(float scale)
        {
            if (scale < 0.92f)
            {
                return (scale + 0.08f);
            }
            if (scale < 1f)
            {
                return 1f;
            }
            if (scale > 1.01f)
            {
                return (scale + 0.08f);
            }
            return scale;
        }

        /// <summary>
        /// Walks a tree of controls, executing the provided delegate on
        /// each control it encounters.
        /// </summary>
        /// <param name="walker">Delegate to be executed on each.</param>
        /// <param name="control">Root control to start at.</param>
        /// <param name="state">Optional state for use by the walker delegate.</param>
        public static void Walk(ControlWalker walker, Control control, object state)
        {
            walker(control, state);
            foreach (Control c in control.Controls)
            {
                Walk(walker, c, state);
            }
        }

        /// <summary>
        /// Given a control that contains focus, finds the focused control.
        /// Null if none found.
        /// </summary>
        public static Control FindFocused(Control control)
        {
            if (control == null)
                return null;
            if (!control.ContainsFocus)
                return null;
            if (control.Focused)
                return control;
            Control.ControlCollection collection = control.Controls;
            if (collection == null)
                return control;
            else
            {
                foreach (Control child in collection)
                {
                    if (child != null && child.ContainsFocus)
                        return FindFocused(child);
                }
                return control;
            }
        }

        /// <summary>
        /// Crawls the given control's parents until it finds a control that
        /// matches the given type (or returns itself if it matches).
        /// Null if none found.
        /// </summary>
        public static Control FindParentOfType(Control control, Type type)
        {
            if (control == null)
                return null;
            if (type.IsAssignableFrom(control.GetType()))
                return control;
            return FindParentOfType(control.Parent, type);
        }

        /// <summary>
        /// Finds the focused control, then looks for a parent that matches type
        /// (or returns itself if it matches).
        /// Null if none found.
        /// </summary>
        public static Control FindFocusedOfType(Control control, Type type)
        {
            Control focused = FindFocused(control);
            if (focused == null)
                return null;
            return FindParentOfType(focused, type);
        }

        public static void ActivateTopLevelWindow(Control control)
        {
            //	Prevent stupidity.
            if (control == null || !control.IsHandleCreated)
                return;

            //	Find the form that contains the control.  If it's found, we get off easy, and can
            //	activate it natively through .NET.
            Form form = control.FindForm();
            if (form != null)
                form.Activate();
            else
                User32.BringWindowToTop(control.Handle);
        }

        public static bool ControlHasVerticalScrollbar(Control control)
        {
            return (User32.GetWindowLong(control.Handle, GWL.STYLE) & WS.VSCROLL) > 0;
        }

        public static void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            int diameter = radius * 2;

            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddLine(rect.Left + radius, rect.Top, rect.Right - radius, rect.Top);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom - radius);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddLine(rect.Right - radius, rect.Bottom, rect.Left + radius, rect.Bottom);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.AddLine(rect.Left, rect.Top + radius, rect.Left, rect.Bottom - radius);
            g.DrawPath(pen, path);
        }

        public static void ShowFocus(Control c)
        {
            User32.SendMessage(c.Handle, WM.CHANGEUISTATE,
                MessageHelper.MAKELONG(UIS.CLEAR, UISF.HIDEFOCUS), IntPtr.Zero);
        }

        public static void HideFocus(Control c)
        {
            User32.SendMessage(c.Handle, WM.CHANGEUISTATE,
                MessageHelper.MAKELONG(UIS.SET, UISF.HIDEFOCUS), IntPtr.Zero);
        }

        public static void ShowAccelerators(Control c)
        {
            User32.SendMessage(c.Handle, WM.CHANGEUISTATE,
                MessageHelper.MAKELONG(UIS.CLEAR, UISF.HIDEACCEL), IntPtr.Zero);
        }

        public static void HideAccelerators(Control c)
        {
            User32.SendMessage(c.Handle, WM.CHANGEUISTATE,
                MessageHelper.MAKELONG(UIS.SET, UISF.HIDEACCEL), IntPtr.Zero);
        }

        public static void ShowActive(Control c)
        {
            User32.SendMessage(c.Handle, WM.CHANGEUISTATE,
                MessageHelper.MAKELONG(UIS.SET, UISF.ACTIVE), IntPtr.Zero);
        }

        public static void HideActive(Control c)
        {
            User32.SendMessage(c.Handle, WM.CHANGEUISTATE,
                MessageHelper.MAKELONG(UIS.CLEAR, UISF.ACTIVE), IntPtr.Zero);
        }

        public static bool FocusControl(Control c, bool forceDrawFocus)
        {
            //focus the control to display its focus rectangle (simulates the behavior of hitting the tab key).
            if (forceDrawFocus)
                ControlHelper.ShowFocus(c);
            if (c.TabStop)
                return c.Focus();
            else
                return c.SelectNextControl(null, true, true, true, true);
        }

        /// <summary>
        /// Converts an input string into a nice value for an AccessibileName.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToAccessibleName(string val)
        {
            string accName = _accNameStripChars.Replace(val, String.Empty);
            return accName;
        }
        private static Regex _accNameStripChars = new Regex("(&|:)");
    }

    public class CenterControlInControlBehavior
    {
        public CenterControlInControlBehavior(Control control, Control contextControl)
        {
            _control = control;
            _contextControl = contextControl;
            _contextControl.SizeChanged += new EventHandler(_contextControl_SizeChanged);
            CenterControl();
        }

        private void CenterControl()
        {
            _control.Location = new Point(
                _contextControl.Left + (_contextControl.Width / 2) - (_control.Width / 2),
                _contextControl.Top + (_contextControl.Height / 2) - (_control.Height / 2));
        }

        private Control _control;
        private Control _contextControl;

        private void _contextControl_SizeChanged(object sender, EventArgs e)
        {
            CenterControl();
        }
    }

    public class ControlGroup : IEnumerable
    {
        private readonly Control[] _controls;

        public ControlGroup(params Control[] controls)
        {
            this._controls = controls;
        }

        public int Left
        {
            get
            {
                int left = int.MaxValue;
                foreach (Control c in _controls)
                    left = Math.Min(c.Left, left);
                return left;
            }
            set
            {
                Location = new Point(value, Top);
            }
        }

        public int Top
        {
            get
            {
                int top = int.MaxValue;
                foreach (Control c in _controls)
                    top = Math.Min(c.Top, top);
                return top;
            }
            set
            {
                Location = new Point(Left, value);
            }
        }

        public int Right
        {
            get
            {
                int right = int.MinValue;
                foreach (Control c in _controls)
                    right = Math.Max(c.Right, right);
                return right;
            }
        }

        public int Bottom
        {
            get
            {
                int bottom = int.MinValue;
                foreach (Control c in _controls)
                    bottom = Math.Max(c.Bottom, bottom);
                return bottom;
            }
        }

        public Point Location
        {
            get
            {
                return new Point(Left, Top);
            }
            set
            {
                Point currLoc = Location;
                if (currLoc != value)
                {
                    int deltaX = value.X - currLoc.X;
                    int deltaY = value.Y - currLoc.Y;

                    Point[] newLocs = new Point[_controls.Length];
                    for (int i = 0; i < _controls.Length; i++)
                        newLocs[i] = new Point(_controls[i].Left + deltaX, _controls[i].Top + deltaY);
                    for (int i = 0; i < _controls.Length; i++)
                        _controls[i].Location = newLocs[i];
                }
            }
        }

        public void GrowAll(int x, int y)
        {
            for (int i = 0; i < _controls.Length; i++)
            {
                if (x != 0)
                    _controls[i].Width += x;
                if (y != 0)
                    _controls[i].Height += y;
            }
        }

        public int Width
        {
            get { return Right - Left; }
        }

        public int Height
        {
            get { return Bottom - Top; }
        }

        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(Location, Size); }
        }

        public IEnumerator GetEnumerator()
        {
            return new ArrayList(_controls).GetEnumerator();
        }

    }

}
