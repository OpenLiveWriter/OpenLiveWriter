// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Summary description for TitleReminderForm.
    /// </summary>
    public class TitleReminderForm : ApplicationDialog
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

        public TitleReminderForm(bool isPage)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.cbDontShowAgain.Text = Res.Get(StringId.TitleRemindDontShowAgain);
            this.labelExplanation.Text = Res.Get(StringId.TitleRemindExplanation);
            this.labelError.Text = Res.Get(StringId.TitleRemindError);
            this.buttonYes.Text = Res.Get(StringId.Yes);
            this.buttonNo.Text = Res.Get(StringId.No);
            this.Text = Res.Get(StringId.TitleRemindTitle);

            this.labelError.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
            labelError.Text = String.Format(CultureInfo.CurrentCulture, labelError.Text, isPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post));
            labelExplanation.Text = String.Format(CultureInfo.CurrentCulture, labelExplanation.Text, isPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(12,
                    labelError,
                    labelExplanation,
                    new ControlGroup(buttonYes, buttonNo),
                    cbDontShowAgain);
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
                PostEditorSettings.TitleReminder = !cbDontShowAgain.Checked;
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TitleReminderForm));
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
            this.cbDontShowAgain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        )));
            this.cbDontShowAgain.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbDontShowAgain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbDontShowAgain.Location = new System.Drawing.Point(72, 128);
            this.cbDontShowAgain.Name = "cbDontShowAgain";
            this.cbDontShowAgain.Size = new System.Drawing.Size(208, 32);
            this.cbDontShowAgain.TabIndex = 4;
            this.cbDontShowAgain.Text = "&Don\'t remind me about missing titles in the future";
            this.cbDontShowAgain.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // labelExplanation
            //
            this.labelExplanation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        )));
            this.labelExplanation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelExplanation.Location = new System.Drawing.Point(72, 48);
            this.labelExplanation.Name = "labelExplanation";
            this.labelExplanation.Size = new System.Drawing.Size(216, 44);
            this.labelExplanation.TabIndex = 1;
            this.labelExplanation.Text = "You have not specified a title for this {0}. Do you still want to continue with p" +
                "ublishing?";
            //
            // labelError
            //
            this.labelError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        )));
            this.labelError.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelError.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelError.Location = new System.Drawing.Point(72, 16);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(216, 23);
            this.labelError.TabIndex = 0;
            this.labelError.Text = "{0} Title Not Specified";
            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
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
            this.buttonYes.Location = new System.Drawing.Point(71, 97);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(75, 23);
            this.buttonYes.TabIndex = 3;
            this.buttonYes.Text = "Yes";
            //
            // buttonNo
            //
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonNo.Location = new System.Drawing.Point(151, 97);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(75, 23);
            this.buttonNo.TabIndex = 2;
            this.buttonNo.Text = "No";
            //
            // TitleReminderForm
            //
            this.AcceptButton = this.buttonNo;
            this.CancelButton = this.buttonNo;
            this.ClientSize = new System.Drawing.Size(314, 168);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Controls.Add(this.cbDontShowAgain);
            this.Controls.Add(this.labelExplanation);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TitleReminderForm";
            this.Text = "Title Reminder";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

    }
}
