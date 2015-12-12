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
    /// Summary description for PasteSpecialForm.
    /// </summary>
    [PersistentWindow("PasteSpecialForm", Location = false, Size = false)]
    public class PasteSpecialForm : ApplicationDialog
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rdPlainText;
        private System.Windows.Forms.RadioButton rdThinned;
        private System.Windows.Forms.RadioButton rdKeepFormatting;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox bevel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public PasteSpecialForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.rdPlainText.Text = Res.Get(StringId.PasteSpecialPlaintextLabel);
            this.label1.Text = Res.Get(StringId.PasteSpecialPlaintextDesc);
            this.rdThinned.Text = Res.Get(StringId.PasteSpecialThinnedLabel);
            this.label2.Text = Res.Get(StringId.PasteSpecialThinnedDesc);
            this.rdKeepFormatting.Text = Res.Get(StringId.PasteSpecialHtmlLabel);
            this.label3.Text = Res.Get(StringId.PasteSpecialHtmlDesc);
            this.btnOK.Text = Res.Get(StringId.OKButtonText);
            this.btnCancel.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.PasteSpecial);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int origY = label3.Bottom;

            LayoutHelper.NaturalizeHeight(label1, label2, label3);
            LayoutHelper.DistributeVertically(12,
                new ControlGroup(rdPlainText, label1),
                new ControlGroup(rdThinned, label2),
                new ControlGroup(rdKeepFormatting, label3));

            Height += Math.Max(0, label3.Bottom - origY);

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
            this.rdThinned = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.rdKeepFormatting = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.bevel = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            //
            // rdPlainText
            //
            this.rdPlainText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdPlainText.Location = new System.Drawing.Point(12, 12);
            this.rdPlainText.Name = "rdPlainText";
            this.rdPlainText.Size = new System.Drawing.Size(237, 18);
            this.rdPlainText.TabIndex = 0;
            this.rdPlainText.Text = "&Remove Formatting";
            this.rdPlainText.Click += new System.EventHandler(this.rdPlainText_Click);
            //
            // label1
            //
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.FlatStyle = FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(29, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(220, 42);
            this.label1.TabIndex = 1;
            this.label1.Text = "Removes all formatting except for line breaks. Preserves links and images. Use to" +
                " copy content only.";
            //
            // rdThinned
            //
            this.rdThinned.Checked = true;
            this.rdThinned.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdThinned.Location = new System.Drawing.Point(12, 92);
            this.rdThinned.Name = "rdThinned";
            this.rdThinned.Size = new System.Drawing.Size(238, 16);
            this.rdThinned.TabIndex = 2;
            this.rdThinned.TabStop = true;
            this.rdThinned.Text = "&Thinned HTML";
            this.rdThinned.Click += new System.EventHandler(this.rdThinned_Click);
            //
            // label2
            //
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.FlatStyle = FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(29, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(220, 61);
            this.label2.TabIndex = 3;
            this.label2.Text = "Removes extra formatting, such as MS Office specific formatting, CSS styles, and " +
                "tables. Preserves relative heading sizes, basic formatting, and aligns with your" +
                " blog style.";
            //
            // rdKeepFormatting
            //
            this.rdKeepFormatting.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rdKeepFormatting.Location = new System.Drawing.Point(12, 176);
            this.rdKeepFormatting.Name = "rdKeepFormatting";
            this.rdKeepFormatting.Size = new System.Drawing.Size(239, 18);
            this.rdKeepFormatting.TabIndex = 4;
            this.rdKeepFormatting.Text = "&Keep Formatting";
            this.rdKeepFormatting.Click += new System.EventHandler(this.rdKeepFormatting_Click);
            //
            // label3
            //
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.FlatStyle = FlatStyle.System;
            this.label3.Location = new System.Drawing.Point(29, 196);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(220, 53);
            this.label3.TabIndex = 5;
            this.label3.Text = "Maintains all the formatting, including those which may not display correctly in " +
                "your blog. For security reasons, scripts are stripped out.";
            //
            // btnOK
            //
            this.btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(345, 269);
            this.btnOK.Name = "btnOK";
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            //
            // btnCancel
            //
            this.btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(423, 269);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            //
            // pictureBox1
            //
            this.pictureBox1.Location = new System.Drawing.Point(302, 17);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(173, 226);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            //
            // bevel
            //
            this.bevel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.bevel.Location = new System.Drawing.Point(267, 2);
            this.bevel.Name = "bevel";
            this.bevel.Size = new System.Drawing.Size(2, 295);
            this.bevel.TabIndex = 0;
            this.bevel.TabStop = false;
            //
            // PasteSpecialForm
            //
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(505, 301);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.rdKeepFormatting);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rdThinned);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rdPlainText);
            this.Controls.Add(this.bevel);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasteSpecialForm";
            this.Text = "Paste Special";
            this.Load += new System.EventHandler(this.PasteSpecialForm_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private void rdPlainText_Click(object sender, System.EventArgs e)
        {
            this.pictureBox1.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.PlainTextSample.png");
            _chosenFormatting = PasteType.RemoveFormatting;
        }

        private void rdThinned_Click(object sender, System.EventArgs e)
        {
            this.pictureBox1.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.ThinnedHTMLSample.png");
            _chosenFormatting = PasteType.Standard;
        }

        private void rdKeepFormatting_Click(object sender, System.EventArgs e)
        {
            this.pictureBox1.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.KeepFormattingSample.png");
            _chosenFormatting = PasteType.KeepFormatting;
        }

        public PasteType ChosenFormatting
        {
            get
            {
                return _chosenFormatting;
            }
        }

        private PasteType _chosenFormatting = PasteType.Standard;

        private void PasteSpecialForm_Load(object sender, System.EventArgs e)
        {

        }

        public enum PasteType { RemoveFormatting, Standard, KeepFormatting }

    }
}
