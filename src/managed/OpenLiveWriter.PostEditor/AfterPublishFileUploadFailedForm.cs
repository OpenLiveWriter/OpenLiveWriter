// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Summary description for FileUploadFailedForm.
    /// </summary>
    public class AfterPublishFileUploadFailedForm : ApplicationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelCaption1;
        private System.Windows.Forms.Label labelCaption2;
        private System.Windows.Forms.TextBox textBoxDetails;

        public AfterPublishFileUploadFailedForm(Exception ex, bool isPage)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            string itemName = isPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower);
            this.Text = Res.Get(StringId.PostPublishFileUploadErrorTitle);
            this.labelCaption1.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PostPublishFileUploadErrorCaption1), itemName);
            this.labelCaption2.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.PostPublishFileUploadErrorCaption2), itemName);
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);

            if (ex is DisplayableException)
            {
                DisplayableException displayableEx = ex as DisplayableException;
                textBoxDetails.Text = String.Format(CultureInfo.CurrentCulture, "{0}\r\n\r\n{1}", displayableEx.Title, displayableEx.Text);
            }
            else
            {
                textBoxDetails.Text = ex.ToString();
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AfterPublishFileUploadFailedForm));
            this.labelCaption1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelCaption2 = new System.Windows.Forms.Label();
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // labelCaption1
            //
            this.labelCaption1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption1.Location = new System.Drawing.Point(56, 16);
            this.labelCaption1.Name = "labelCaption1";
            this.labelCaption1.Size = new System.Drawing.Size(288, 40);
            this.labelCaption1.TabIndex = 1;
            this.labelCaption1.Text = "The post was successfully published however an error occured while uploading supp" +
                "orting files:";
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
            // labelCaption2
            //
            this.labelCaption2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption2.Location = new System.Drawing.Point(56, 176);
            this.labelCaption2.Name = "labelCaption2";
            this.labelCaption2.Size = new System.Drawing.Size(288, 64);
            this.labelCaption2.TabIndex = 3;
            this.labelCaption2.Text = "Depending upon the nature of the error, you may be able to successfully publish t" +
                "he supporting files by retrying. Alternatively, you may wish to delete the post " +
                "from your weblog.";
            //
            // textBoxDetails
            //
            this.textBoxDetails.Location = new System.Drawing.Point(56, 64);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.ReadOnly = true;
            this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDetails.Size = new System.Drawing.Size(277, 96);
            this.textBoxDetails.TabIndex = 2;
            this.textBoxDetails.Text = "";
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(140, 248);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            //
            // AfterPublishFileUploadFailedForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(354, 280);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelCaption2);
            this.Controls.Add(this.labelCaption1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textBoxDetails);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AfterPublishFileUploadFailedForm";
            this.Text = "Error Uploading Supporting Files";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
