// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{

    public class WebLayoutViewWarningForm : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox checkBoxDontShowMessageAgain;
        private System.Windows.Forms.Label labelCaption1;
        private System.Windows.Forms.Label labelCaption2;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WebLayoutViewWarningForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.checkBoxDontShowMessageAgain.Text = Res.Get(StringId.DontShowAgain);
            this.labelCaption1.Text = Res.Get(StringId.WebLayoutWarning1);
            this.labelCaption2.Text = Res.Get(StringId.WebLayoutWarning2);
            this.Text = Res.Get(StringId.WebLayoutWarningTitle);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(12, labelCaption1, labelCaption2, checkBoxDontShowMessageAgain);
                buttonOK.Top = checkBoxDontShowMessageAgain.Bottom + 16;
            }
        }

        public bool DontShowMessageAgain
        {
            get
            {
                return checkBoxDontShowMessageAgain.Checked;
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WebLayoutViewWarningForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxDontShowMessageAgain = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelCaption1 = new System.Windows.Forms.Label();
            this.labelCaption2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(116, 152);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            //
            // checkBoxDontShowMessageAgain
            //
            this.checkBoxDontShowMessageAgain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxDontShowMessageAgain.Location = new System.Drawing.Point(56, 112);
            this.checkBoxDontShowMessageAgain.Name = "checkBoxDontShowMessageAgain";
            this.checkBoxDontShowMessageAgain.Size = new System.Drawing.Size(240, 24);
            this.checkBoxDontShowMessageAgain.TabIndex = 1;
            this.checkBoxDontShowMessageAgain.Text = "Don\'t show this message again";
            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(39, 40);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            //
            // labelCaption1
            //
            this.labelCaption1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption1.Location = new System.Drawing.Point(56, 8);
            this.labelCaption1.Name = "labelCaption1";
            this.labelCaption1.Size = new System.Drawing.Size(240, 56);
            this.labelCaption1.TabIndex = 3;
            this.labelCaption1.Text = "You may have problems with editing content in Web Layout view with the current We" +
                "blog (for example, incorrect post layout or problems selecting and resi" +
                "zing content).";
            //
            // labelCaption2
            //
            this.labelCaption2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption2.Location = new System.Drawing.Point(56, 72);
            this.labelCaption2.Name = "labelCaption2";
            this.labelCaption2.Size = new System.Drawing.Size(240, 24);
            this.labelCaption2.TabIndex = 4;
            this.labelCaption2.Text = "If you do experience problems you should switch back to the Normal view.";
            //
            // WebLayoutViewWarningForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(306, 184);
            this.Controls.Add(this.labelCaption2);
            this.Controls.Add(this.labelCaption1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.checkBoxDontShowMessageAgain);
            this.Controls.Add(this.buttonOK);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WebLayoutViewWarningForm";
            this.Text = "Web Layout View";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
