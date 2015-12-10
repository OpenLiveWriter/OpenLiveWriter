// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    /// <summary>
    /// Summary description for TitleReminderForm.
    /// </summary>
    public class FutureDateWarningForm : ApplicationDialog
    {
        private System.Windows.Forms.CheckBox cbDontShowAgain;
        private System.Windows.Forms.Label labelExplanation;
        private System.Windows.Forms.Label labelError;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Button buttonNo;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public FutureDateWarningForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.cbDontShowAgain.Text = Res.Get(StringId.FuturePostWarningDontShowAgain);
            this.labelExplanation.Text = Res.Get(StringId.FuturePostWarningExplanation);
            this.labelError.Text = Res.Get(StringId.FuturePostWarningTitle);
            this.buttonYes.Text = Res.Get(StringId.PublishNow);
            this.buttonNo.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.FuturePostWarningDialogTitle);

            this.labelError.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                DisplayHelper.AutoFitSystemButton(buttonYes, buttonYes.Width, int.MaxValue);
                DisplayHelper.AutoFitSystemButton(buttonNo, buttonNo.Width, int.MaxValue);
                LayoutHelper.DistributeHorizontally(8, buttonYes, buttonNo);
                LayoutHelper.NaturalizeHeight(labelError, labelExplanation, cbDontShowAgain);
                LayoutHelper.DistributeVertically(12,
                    labelError,
                    labelExplanation,
                    new ControlGroup(buttonYes, buttonNo),
                    cbDontShowAgain);
                cbDontShowAgain.Left = labelError.Left;
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (DialogResult != DialogResult.Cancel)
                PostEditorSettings.FuturePublishDateWarning = !cbDontShowAgain.Checked;
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FutureDateWarningForm));
            this.cbDontShowAgain = new System.Windows.Forms.CheckBox();
            this.labelExplanation = new System.Windows.Forms.Label();
            this.labelError = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            //
            // cbDontShowAgain
            //
            this.cbDontShowAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDontShowAgain.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbDontShowAgain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbDontShowAgain.Location = new System.Drawing.Point(86, 169);
            this.cbDontShowAgain.Name = "cbDontShowAgain";
            this.cbDontShowAgain.Size = new System.Drawing.Size(244, 37);
            this.cbDontShowAgain.TabIndex = 4;
            this.cbDontShowAgain.Text = "&Don\'t confirm future dates prior to publishing";
            this.cbDontShowAgain.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // labelExplanation
            //
            this.labelExplanation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
            this.labelExplanation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelExplanation.Location = new System.Drawing.Point(86, 55);
            this.labelExplanation.Name = "labelExplanation";
            this.labelExplanation.Size = new System.Drawing.Size(244, 80);
            this.labelExplanation.TabIndex = 1;
            this.labelExplanation.Text = resources.GetString("labelExplanation.Text");
            //
            // labelError
            //
            this.labelError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        )));
            this.labelError.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelError.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelError.Location = new System.Drawing.Point(86, 18);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(244, 27);
            this.labelError.TabIndex = 0;
            this.labelError.Text = "Confirm Future Publish Date";
            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(10, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(53, 42);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            //
            // buttonYes
            //
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonYes.Location = new System.Drawing.Point(85, 151);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(90, 26);
            this.buttonYes.TabIndex = 3;
            this.buttonYes.Text = "Yes";
            //
            // buttonNo
            //
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonNo.Location = new System.Drawing.Point(181, 151);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(90, 26);
            this.buttonNo.TabIndex = 2;
            this.buttonNo.Text = "No";
            //
            // FutureDateWarningForm
            //
            this.AcceptButton = this.buttonNo;
            this.CancelButton = this.buttonNo;
            this.ClientSize = new System.Drawing.Size(361, 215);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Controls.Add(this.cbDontShowAgain);
            this.Controls.Add(this.labelExplanation);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FutureDateWarningForm";
            this.Text = "Confirm Future Publish Date";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}
