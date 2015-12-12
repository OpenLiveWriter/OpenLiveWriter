// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Summary description for FileUploadFailedForm.
    /// </summary>
    public class FileUploadFailedForm : ApplicationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.ListView listViewFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelFTP;
        private System.Windows.Forms.Label labelFTP2;

        public static DialogResult Show(IWin32Window owner, string[] files)
        {
            using (FileUploadFailedForm form = new FileUploadFailedForm(files))
            {
                return form.ShowDialog(owner);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, label1, listViewFiles, labelFTP, labelFTP2, buttonYes);
                buttonNo.Top = buttonYes.Top;
            }
        }

        private FileUploadFailedForm(string[] images)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonYes.Text = Res.Get(StringId.YesButton);
            this.buttonNo.Text = Res.Get(StringId.NoButton);
            this.label1.Text = Res.Get(StringId.FileUploadFailedCaption);
            this.labelFTP.Text = Res.Get(StringId.FileUploadFailedFTP);
            this.labelFTP2.Text = Res.Get(StringId.FileUploadFailedFTP2);
            this.Text = Res.Get(StringId.FileUploadFailedTitle);

            listViewFiles.BeginUpdate();
            listViewFiles.Items.Clear();
            foreach (string image in images)
                listViewFiles.Items.Add(image);
            listViewFiles.EndUpdate();
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FileUploadFailedForm));
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelFTP = new System.Windows.Forms.Label();
            this.labelFTP2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // buttonYes
            //
            this.buttonYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonYes.Location = new System.Drawing.Point(192, 224);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.TabIndex = 0;
            this.buttonYes.Text = "&Yes";
            //
            // buttonNo
            //
            this.buttonNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonNo.Location = new System.Drawing.Point(272, 224);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.TabIndex = 1;
            this.buttonNo.Text = "&No";
            //
            // listViewFiles
            //
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                            this.columnHeaderFile});
            this.listViewFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewFiles.Location = new System.Drawing.Point(56, 40);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.listViewFiles.Size = new System.Drawing.Size(280, 96);
            this.listViewFiles.TabIndex = 5;
            this.listViewFiles.View = System.Windows.Forms.View.List;
            //
            // columnHeaderFile
            //
            this.columnHeaderFile.Text = "";
            this.columnHeaderFile.Width = 328;
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(56, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(288, 28);
            this.label1.TabIndex = 4;
            this.label1.Text = "The following images cannot be published because the weblog does not support imag" +
                "e publishing:";
            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(44, 40);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            //
            // labelFTP
            //
            this.labelFTP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFTP.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFTP.Location = new System.Drawing.Point(56, 144);
            this.labelFTP.Name = "labelFTP";
            this.labelFTP.Size = new System.Drawing.Size(288, 32);
            this.labelFTP.TabIndex = 7;
            this.labelFTP.Text = "Open Live Writer can be configured to publish images to an FTP account.";
            //
            // labelFTP2
            //
            this.labelFTP2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFTP2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFTP2.Location = new System.Drawing.Point(56, 176);
            this.labelFTP2.Name = "labelFTP2";
            this.labelFTP2.Size = new System.Drawing.Size(296, 32);
            this.labelFTP2.TabIndex = 8;
            this.labelFTP2.Text = "Do you want to configure an FTP account for image publishing now?";
            //
            // FileUploadFailedForm
            //
            this.AcceptButton = this.buttonYes;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonNo;
            this.ClientSize = new System.Drawing.Size(362, 256);
            this.Controls.Add(this.labelFTP2);
            this.Controls.Add(this.labelFTP);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileUploadFailedForm";
            this.Text = "Image Upload Not Supported By Weblog";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
