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

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Summary description for PasteSpecialFormText.
    /// </summary>
    [PersistentWindow("PasteSpecialFormText", Location = false, Size = false)]
    public class PasteSpecialFormText : ApplicationDialog
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rdPlainText;
        private System.Windows.Forms.RadioButton rdFormatted;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox bevel;
        private System.Windows.Forms.PictureBox pictureBox1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public PasteSpecialFormText()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.rdPlainText.Text = Res.Get(StringId.PasteSpecialTextPlainText);
            this.label1.Text = Res.Get(StringId.PasteSpecialTextPlainTextDesc);
            this.rdFormatted.Text = Res.Get(StringId.PasteSpecialTextFormatted);
            this.label2.Text = Res.Get(StringId.PasteSpecialTextFormattedDesc);
            this.btnOK.Text = Res.Get(StringId.OKButtonText);
            this.btnCancel.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.PasteSpecial);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int origY = label2.Bottom;

            LayoutHelper.NaturalizeHeight(label1, label2);
            LayoutHelper.DistributeVertically(12,
                new ControlGroup(rdPlainText, label1),
                new ControlGroup(rdFormatted, label2));

            Height += Math.Max(0, label2.Bottom - origY);

            LayoutHelper.FixupOKCancel(btnOK, btnCancel);

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
            this.rdPlainText = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.rdFormatted = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.bevel = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            //
            // rdPlainText
            //
            this.rdPlainText.Checked = true;
            this.rdPlainText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdPlainText.Location = new System.Drawing.Point(12, 14);
            this.rdPlainText.Name = "rdPlainText";
            this.rdPlainText.Size = new System.Drawing.Size(239, 18);
            this.rdPlainText.TabIndex = 0;
            this.rdPlainText.TabStop = true;
            this.rdPlainText.Text = "&Plain Text";
            this.rdPlainText.Click += new System.EventHandler(this.rdPlainText_Click);
            //
            // label1
            //
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.FlatStyle = FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(29, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 34);
            this.label1.TabIndex = 1;
            this.label1.Text = "Standard plain text paste. Any html tags will be escaped.";
            //
            // rdFormatted
            //
            this.rdFormatted.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdFormatted.Location = new System.Drawing.Point(12, 89);
            this.rdFormatted.Name = "rdFormatted";
            this.rdFormatted.Size = new System.Drawing.Size(239, 18);
            this.rdFormatted.TabIndex = 2;
            this.rdFormatted.Text = "&HTML";
            this.rdFormatted.Click += new System.EventHandler(this.rdFormatted_Click);
            //
            // label2
            //
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.FlatStyle = FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(29, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(219, 45);
            this.label2.TabIndex = 3;
            this.label2.Text = "Interprets text as html source code. Copied content will appear with formatting. " +
                "Use for imgs, embeds, etc. ";
            //
            // btnOK
            //
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(345, 269);
            this.btnOK.Name = "btnOK";
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(423, 269);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            //
            // bevel
            //
            this.bevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)));
            this.bevel.Location = new System.Drawing.Point(267, 2);
            this.bevel.Name = "bevel";
            this.bevel.Size = new System.Drawing.Size(2, 295);
            this.bevel.TabIndex = 6;
            this.bevel.TabStop = false;
            //
            // pictureBox1
            //
            this.pictureBox1.Location = new System.Drawing.Point(302, 17);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(173, 226);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            //
            // PasteSpecialFormText
            //
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(505, 301);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.bevel);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rdFormatted);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rdPlainText);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasteSpecialFormText";
            this.Text = "Paste Special";
            this.Load += new System.EventHandler(this.PasteSpecialFormText_Load);
            this.ResumeLayout(false);

        }
        #endregion

        public PasteType ChosenFormatting
        {
            get
            {
                return _chosenFormatting;
            }
        }

        private PasteType _chosenFormatting = PasteType.Standard;

        private void rdFormatted_Click(object sender, System.EventArgs e)
        {
            this.pictureBox1.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.KeepFormattingSample.png");
            _chosenFormatting = PasteType.KeepFormatting;
        }

        private void rdPlainText_Click(object sender, System.EventArgs e)
        {
            this.pictureBox1.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.PlainTextSample.png");
            _chosenFormatting = PasteType.Standard;
        }

        private void PasteSpecialFormText_Load(object sender, System.EventArgs e)
        {

        }

        public enum PasteType { Standard, KeepFormatting }
    }
}
