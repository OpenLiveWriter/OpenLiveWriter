// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for LinkToOptionsForm.
    /// </summary>
    public class LinkToOptionsForm : BaseForm
    {
        private Panel panelEditor;
        private Button buttonCancel;
        private Button buttonOK;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public LinkToOptionsForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.Text = Res.Get(StringId.ImgSBLinkOptions);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Resize the form so that the panel will fit the new value's Size
            Size preferredSize = EditorControl.Size;
            int deltaHeight = preferredSize.Height - panelEditor.Height;
            int deltaWidth = preferredSize.Width - panelEditor.Width;
            Size = new Size(Width + deltaWidth, Height + deltaHeight);

            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
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
            this.panelEditor = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // panelEditor
            //
            this.panelEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelEditor.Location = new System.Drawing.Point(9, 12);
            this.panelEditor.Name = "panelEditor";
            this.panelEditor.Size = new System.Drawing.Size(342, 144);
            this.panelEditor.TabIndex = 0;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(277, 164);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(196, 164);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            //
            // LinkToOptionsForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.AutoValidate = AutoValidate.Disable;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(362, 196);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.panelEditor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LinkToOptionsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Link Options";
            this.ResumeLayout(false);

        }
        #endregion

        public Control EditorControl
        {
            get
            {
                return _editorControl;
            }
            set
            {
                SuspendLayout();
                panelEditor.Controls.Clear();
                _editorControl = value;

                panelEditor.Controls.Add(value);

                ResumeLayout();
            }
        }
        private Control _editorControl;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == DialogResult.OK && !ValidateChildren())
            {
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }
    }
}
