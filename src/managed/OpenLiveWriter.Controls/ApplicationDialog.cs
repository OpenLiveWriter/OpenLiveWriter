// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Controls
{

    public class ApplicationDialog : BaseForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ApplicationDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //	Turn off CS_CLIPCHILDREN.
            //			User32.SetWindowLong(Handle, GWL.STYLE, User32.GetWindowLong(Handle, GWL.STYLE) & ~WS.CLIPCHILDREN);

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            //			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!DesignMode)
            {
                try
                {
                    PersistentWindowAttribute.MaybePersist(this);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception trying to persist form settings on " + this.GetType().FullName + ": " + ex.ToString());
                }
            }

            base.OnClosed(e);
        }

        /// <summary>
        /// Handle OnLoad -- set standard icon
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                PersistentWindowAttribute.MaybeRestore(this);
            }
            Form owner = this.Owner;
            if (owner != null)
                this.TopMost = owner.TopMost;
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
            // ApplicationDialog
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(292, 286);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ApplicationDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

        }
        #endregion
    }
}
