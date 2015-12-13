// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.DisplayMessages
{
    /// <summary>
    /// Summary description for UnsupportedFileUploadsMessage.
    /// </summary>
    public class UnsupportedFileUploadsMessage : BaseForm
    {
        private PictureBox pictureBox1;
        private Label label1;
        private ColumnHeader columnHeaderFile;
        private Button buttonCancel;
        private Button buttonOK;
        private ListView listViewFiles;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public UnsupportedFileUploadsMessage(string[] files)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.label1.Text = Res.Get(StringId.NoFileUploadText);
            this.buttonCancel.Text = Res.Get(StringId.No);
            this.buttonOK.Text = Res.Get(StringId.Yes);
            this.label2.Text = Res.Get(StringId.NoFileUploadPrompt);
            this.Text = Res.Get(StringId.NoFileUploadTitle);

            listViewFiles.BeginUpdate();
            listViewFiles.Items.Clear();
            foreach(string file in files)
                listViewFiles.Items.Add(file);
            listViewFiles.EndUpdate();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(UnsupportedFileUploadsMessage));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(44, 40);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(52, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 28);
            this.label1.TabIndex = 1;
            this.label1.Text = "The following images cannot be published because image uploading has not been con" +
                "figured for this weblog:";
            //
            // listViewFiles
            //
            this.listViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                            this.columnHeaderFile});
            this.listViewFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewFiles.Location = new System.Drawing.Point(52, 52);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.listViewFiles.Size = new System.Drawing.Size(286, 120);
            this.listViewFiles.TabIndex = 2;
            this.listViewFiles.View = System.Windows.Forms.View.List;
            //
            // columnHeaderFile
            //
            this.columnHeaderFile.Text = "";
            this.columnHeaderFile.Width = 328;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(262, 214);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "No";
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(182, 214);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "Yes";
            //
            // label2
            //
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(52, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(282, 28);
            this.label2.TabIndex = 5;
            this.label2.Text = "Would you like to configure the image upload settings for your weblog now?";
            //
            // UnsupportedFileUploadsMessage
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(346, 244);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnsupportedFileUploadsMessage";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Image Upload Not Configured";
            this.ResumeLayout(false);

        }
        #endregion


    }
}
