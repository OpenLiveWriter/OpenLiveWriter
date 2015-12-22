// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.FileDestinations
{
    public class NewFolder : ApplicationDialog
    {
        private Button buttonOK;
        private Button buttonCancel;
        private TextBox newFolderName;
        private Label label1;
        private IContainer components = null;

        public NewFolder()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.label1.Text = Res.Get(StringId.NewFolderPrompt);
            this.Text = Res.Get(StringId.NewFolderTitle);
        }

        protected override void OnLoad(EventArgs e)
        {
            using (new AutoGrow(this, AnchorStyles.None, false))
            {
                DisplayHelper.AutoFitSystemLabel(label1, label1.Width, label1.Width);
                LayoutHelper.DistributeVertically(8, label1, newFolderName, new ControlGroup(buttonOK, buttonCancel));
                LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
            }

            base.OnLoad(e);
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.newFolderName = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // newFolderName
            //
            this.newFolderName.Location = new System.Drawing.Point(8, 40);
            this.newFolderName.Name = "newFolderName";
            this.newFolderName.Size = new System.Drawing.Size(232, 21);
            this.newFolderName.TabIndex = 0;
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(83, 80);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(165, 80);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(8, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "New folder name:";
            //
            // NewFolder
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(248, 110);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.newFolderName);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewFolder";
            this.Text = "Create New Folder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public string NewFolderName
        {
            get
            {
                return this.newFolderName.Text;
            }
        }
    }
}

