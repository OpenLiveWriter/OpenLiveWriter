// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using System.Security.Permissions;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Summary description for ColorPickerForm.
    /// </summary>
    public class ColorPickerForm : MiniForm
    {
        private OpenLiveWriter.ApplicationFramework.ColorPresetControl colorPresets;
        private OpenLiveWriter.ApplicationFramework.ColorDefaultColorControl colorDefaultColorControl;
        private OpenLiveWriter.ApplicationFramework.ColorDialogLauncherControl colorDialogLauncherControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private int currentSubControl;
        private IColorPickerSubControl[] subctrls;
        private Color m_color;
        public event ColorSelectedEventHandler ColorSelected;

        public ColorPickerForm()
        {
            //
            // Required for Windows Form Designer support
            //
            RightToLeftLayout = false; // prevents the single-pixel border from drawing in a funky way (right side drops out, even when using BidiGraphics)
            InitializeComponent();
            DismissOnDeactivate = true;
            subctrls = new IColorPickerSubControl[] { colorDefaultColorControl, colorPresets, colorDialogLauncherControl };

            // The ColorDialogLauncherControl needs to Close the ColorPickerForm before the ColorDialog can be shown.
            // If not, the Color Dialog will return DialogResult.Cancel immediately.
            colorDialogLauncherControl.Close += new EventHandler(colorDialogLauncherControl_Close);

            colorDefaultColorControl.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.ColorPickerDefaultColor));
        }

        void colorDialogLauncherControl_Close(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right | AnchorStyles.Bottom, true))
            {
                int origHeight = colorDefaultColorControl.Height;
                int origWidth = colorDefaultColorControl.Width;
                colorDefaultColorControl.NaturalizeHeight();
                int deltaY = colorDefaultColorControl.Height - origHeight;

                new ControlGroup(colorPresets, colorDialogLauncherControl).Top += deltaY;

                colorDialogLauncherControl.NaturalizeHeight();

                colorDialogLauncherControl.Width = colorDefaultColorControl.Width =
                    Math.Max(origWidth, Math.Max(colorDialogLauncherControl.Width, colorDefaultColorControl.Width));
            }
            colorPresets.Left = (ClientSize.Width - colorPresets.Width) / 2;

            Focus();
        }

        public Color Color
        {
            get { return m_color; }
            set
            {
                m_color = value;

                foreach (IColorPickerSubControl sc in subctrls)
                {
                    sc.Color = value;
                }

                if (ColorSelected != null)
                    ColorSelected(this, new ColorSelectedEventArgs(value));
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.colorPresets = new OpenLiveWriter.ApplicationFramework.ColorPresetControl(this.colorPickerForm_ColorSelected, this.sc_Navigate);
            this.colorDefaultColorControl = new OpenLiveWriter.ApplicationFramework.ColorDefaultColorControl(this.colorPickerForm_ColorSelected, this.sc_Navigate);
            this.colorDialogLauncherControl = new OpenLiveWriter.ApplicationFramework.ColorDialogLauncherControl(this.colorPickerForm_ColorSelected, this.sc_Navigate);
            this.SuspendLayout();
            //
            // colorPresets
            //
            this.colorPresets.Location = new System.Drawing.Point(11, 39);
            this.colorPresets.Name = "colorPresets";
            this.colorPresets.Size = new System.Drawing.Size(96, 72);
            this.colorPresets.TabIndex = 1;
            this.colorPresets.TabStop = true;
            //
            // colorDefaultColorControl
            //
            this.colorDefaultColorControl.Location = new System.Drawing.Point(5, 5);
            this.colorDefaultColorControl.Name = "colorDefaultColorControl";
            this.colorDefaultColorControl.Size = new System.Drawing.Size(108, 32);
            this.colorDefaultColorControl.TabIndex = 0;
            this.colorDefaultColorControl.TabStop = true;
            //
            // colorDialogLauncherControl
            //
            this.colorDialogLauncherControl.Location = new System.Drawing.Point(5, 112);
            this.colorDialogLauncherControl.Name = "colorDialogLauncherControl";
            this.colorDialogLauncherControl.Size = new System.Drawing.Size(108, 24);
            this.colorDialogLauncherControl.TabIndex = 2;
            this.colorDialogLauncherControl.TabStop = true;

            //
            // ColorPickerForm
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(118, 141);
            this.ControlBox = false;
            this.Controls.Add(this.colorDialogLauncherControl);
            this.Controls.Add(this.colorDefaultColorControl);
            this.Controls.Add(this.colorPresets);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorPickerForm";
            this.Text = "ColorPickerForm";
            this.ResumeLayout(false);

        }
        #endregion

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;

                // Borderless windows show in the alt+tab window, so this fakes
                // out windows into thinking its a tool window (which doesn't
                // show up in the alt+tab window).
                createParams.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW

                return createParams;
            }
        }

        private void SelectNextSubControl(int current, bool forward)
        {
            int nextSubControl = forward ? current + 1 : current - 1;
            nextSubControl = (nextSubControl + subctrls.Length) % subctrls.Length;

            currentSubControl = nextSubControl;
            subctrls[currentSubControl].SelectControl(forward);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            else if (keyData == Keys.Tab)
            {
                Close();
                return true;
            }
            else if (keyData == (Keys.Tab | Keys.Shift))
            {
                Close();
                return true;
            }
            else if (keyData == Keys.Up | keyData == Keys.Left)
            {
                SelectNextSubControl(currentSubControl, false);
                return true;
            }
            else if (keyData == Keys.Down | keyData == Keys.Right)
            {
                SelectNextSubControl(currentSubControl, true);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            using (Brush b = new SolidBrush(BackColor))
                g.FillRectangle(b, ClientRectangle);
            using (Pen p = new Pen(SystemColors.Highlight, 1))
                g.DrawRectangle(p, new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1));
        }

        private void colorPickerForm_ColorSelected(object sender, ColorSelectedEventArgs e)
        {
            Color = e.SelectedColor;
            Close();
        }

        private void sc_Navigate(object sender, NavigateEventArgs e)
        {
            int navFrom = -1;
            for (int i = 0; i < subctrls.Length; i++)
            {
                if (ReferenceEquals(sender, subctrls[i]))
                {
                    navFrom = i;
                    break;
                }
            }

            SelectNextSubControl(navFrom, e.Forward);
        }
    }

    public delegate void NavigateEventHandler(object sender, NavigateEventArgs args);
    public delegate void ColorSelectedEventHandler(object sender, ColorSelectedEventArgs args);
    public class ColorSelectedEventArgs : EventArgs
    {
        private readonly Color selectedColor;

        public ColorSelectedEventArgs(Color selectedColor)
        {
            this.selectedColor = selectedColor;
        }

        public Color SelectedColor
        {
            get { return selectedColor; }
        }
    }

    public abstract class IColorPickerSubControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected System.ComponentModel.Container components = null;

        public IColorPickerSubControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate)
        {
            _ColorSelected += colorSelected;
            _Navigate = navigate;
        }

        /// <summary>
        /// Raised by a subcontrol to inform ColorPickerForm that it should navigate
        /// (either for forward or backward) to the next control.
        /// </summary>
        public event NavigateEventHandler _Navigate;

        /// <summary>
        /// Called by a subcontrol (derived class of IColorPickerSubControl) to raise the _Navigate event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void Navigate(NavigateEventArgs e)
        {
            _Navigate(this, e);
        }

        /// <summary>
        ///  Raised by a subcontrol to inform ColorPickerForm that the user has selected a color
        /// </summary>
        private event ColorSelectedEventHandler _ColorSelected;

        /// <summary>
        /// Called by a subcontrol (derived class of IColorPickerSubControl) to raise the _ColorSelected event
        /// </summary>
        /// <param name="e">ColorSelectedEventArgs</param>
        protected virtual void ColorSelected(ColorSelectedEventArgs e)
        {
            _ColorSelected(this, e);
        }

        /// <summary>
        /// Used by ColorPickerForm to facilitate navigation (up/down arrow keys) across the color picker menu.
        /// </summary>
        public virtual void SelectControl(bool forward)
        {
            this.Select();
        }

        /// Used by ColorPickerForm to inform a subcontrol of the currently selected color.
        public virtual Color Color
        {
            get { return _color; }
            set { _color = value; }
        }
        protected Color _color;

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == '\r' || e.KeyChar == ' ')
            {
                ColorSelected(new ColorSelectedEventArgs(Color));
                e.Handled = true;
                return;
            }
        }

        protected override void OnPaint(PaintEventArgs args)
        {
            base.OnPaint(args);

            BidiGraphics g = new BidiGraphics(args.Graphics, ClientRectangle);
            g.DrawText(Text, Font, ClientRectangle, SystemColors.ControlText, TextFormatFlags);
        }

        protected TextFormatFlags TextFormatFlags
        {
            get
            {
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                if (!ShowKeyboardCues)
                    flags |= TextFormatFlags.HidePrefix;
                return flags;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public void NaturalizeHeight()
        {
            Size size = TextRenderer.MeasureText(
                Text,
                Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags);
            Height = Math.Max(Height, size.Height);
            Width = size.Width;
        }

        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessMnemonic(char charCode)
        {
            if (IsMnemonic(charCode, this.Text))
            {
                ColorSelected(new ColorSelectedEventArgs(Color));
                return true;
            }
            return base.ProcessMnemonic(charCode);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ColorSelected(new ColorSelectedEventArgs(Color));
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Highlight = true;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Highlight = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Highlight = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Highlight = false;
        }

        protected bool _highlight;

        protected virtual bool Highlight
        {
            get { return _highlight; }
            set
            {
                if (_highlight != value)
                {
                    _highlight = value;
                    if (_highlight)
                    {
                        BackColor = SystemColors.Highlight;
                        ForeColor = SystemColors.HighlightText;
                    }
                    else
                    {
                        BackColor = Parent.BackColor;
                        ForeColor = SystemColors.ControlText;
                    }
                    Invalidate();
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                Navigate(new NavigateEventArgs(false));
                return true;
            }
            else if (keyData == Keys.Down)
            {
                Navigate(new NavigateEventArgs(true));
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
