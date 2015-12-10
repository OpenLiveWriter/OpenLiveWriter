// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Summary description for MiniForm.
    /// </summary>
    public class MiniForm : BaseForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private IWin32Window _parentFrame;
        private IMiniFormOwner _owner;
        private bool _floatAboveMainFrame = false;
        private bool _dismissOnDeactivate = false;

        public MiniForm()
            : this(Win32WindowImpl.ForegroundWin32Window)
        {
        }

        public MiniForm(IWin32Window parentFrame)
        {
            _parentFrame = parentFrame;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // double-buffered painting
            User32.SetWindowLong(Handle, GWL.STYLE, User32.GetWindowLong(Handle, GWL.STYLE) & ~WS.CLIPCHILDREN);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        public bool DismissOnDeactivate
        {
            get
            {
                return _dismissOnDeactivate;
            }
            set
            {
                _dismissOnDeactivate = value;
            }
        }

        public void FloatAboveOwner(IMiniFormOwner owner)
        {
            _floatAboveMainFrame = true;
            _owner = owner;
            _owner.AddOwnedForm(this);
        }

        protected IWin32Window ParentFrame
        {
            get
            {
                return _parentFrame;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // copy base create params
                CreateParams createParams = base.CreateParams;

                // add system standard drop shadow
                if (ShowDropShadow)
                {
                    const int CS_DROPSHADOW = 0x20000;
                    createParams.ClassStyle |= CS_DROPSHADOW;
                }

                // prevent appearance in alt-tab window
                createParams.ClassStyle |= 0x00000080; // WS_EX_TOOLWINDOW

                return createParams;
            }
        }

        protected virtual bool ShowDropShadow
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Override out Activated event to allow parent form to retains its 'activated'
        /// look (caption bar color, etc.) even when we are active
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            // call base
            base.OnActivated(e);

            // send the parent form a WM_NCACTIVATE message to cause it to to retain it's
            // activated title bar appearance
            User32.SendMessage(ParentFrame.Handle, WM.NCACTIVATE, new UIntPtr(1), IntPtr.Zero);
        }

        /// <summary>
        /// Automatically close when the form is deactivated
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (DismissOnDeactivate)
            {
                // set a timer that will result in the closing of the form
                // (we do this because if actually call Close right here it
                // will prevent the mouse event that resulted in the deactivation
                // of the form from actually triggering in the new target
                // window -- this allows the mouse event to trigger and the
                // form to go away almost instantly
                Timer closeDelayTimer = new Timer();
                closeDelayTimer.Tick += new EventHandler(closeDelayTimer_Tick);
                closeDelayTimer.Interval = 10;
                closeDelayTimer.Start();
            }
        }

        /// <summary>
        /// Actually close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeDelayTimer_Tick(object sender, EventArgs e)
        {
            // stop and dispose the timer
            Timer closeDelayTimer = (Timer)sender;
            closeDelayTimer.Stop();
            closeDelayTimer.Dispose();

            // cancel the form
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_floatAboveMainFrame)
                _owner.RemoveOwnedForm(this);
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
            //
            // MiniForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MiniForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "MiniForm";

        }
        #endregion
    }

    public interface IMiniFormOwner
    {
        void AddOwnedForm(Form f);
        void RemoveOwnedForm(Form f);
    }
}
