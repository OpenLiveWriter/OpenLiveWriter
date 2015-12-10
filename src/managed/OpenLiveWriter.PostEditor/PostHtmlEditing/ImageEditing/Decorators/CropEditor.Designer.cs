using System;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    partial class CropEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.lblAspectRatio = new System.Windows.Forms.Label();
            this.cbAspectRatio = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkGrid = new System.Windows.Forms.CheckBox();
            this.buttonRotate = new OpenLiveWriter.Controls.XPBitmapButton();
            this.btnRemoveCrop = new System.Windows.Forms.Button();
            this.imageCropControl = new OpenLiveWriter.Controls.ImageCropControl();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(372, 290);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(453, 290);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // lblAspectRatio
            //
            this.lblAspectRatio.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblAspectRatio.Location = new System.Drawing.Point(7, 10);
            this.lblAspectRatio.Name = "lblAspectRatio";
            this.lblAspectRatio.Size = new System.Drawing.Size(124, 17);
            this.lblAspectRatio.TabIndex = 0;
            this.lblAspectRatio.Text = "&Proportion:";
            this.lblAspectRatio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cbAspectRatio
            //
            this.cbAspectRatio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAspectRatio.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbAspectRatio.FormattingEnabled = true;
            this.cbAspectRatio.Location = new System.Drawing.Point(137, 6);
            this.cbAspectRatio.Name = "cbAspectRatio";
            this.cbAspectRatio.Size = new System.Drawing.Size(122, 21);
            this.cbAspectRatio.TabIndex = 1;
            this.cbAspectRatio.SelectedIndexChanged += new System.EventHandler(this.cbAspectRatio_SelectedIndexChanged);
            //
            // panel1
            //
            this.panel1.Controls.Add(this.chkGrid);
            this.panel1.Controls.Add(this.lblAspectRatio);
            this.panel1.Controls.Add(this.buttonRotate);
            this.panel1.Controls.Add(this.cbAspectRatio);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(536, 32);
            this.panel1.TabIndex = 99;
            //
            // chkGrid
            //
            this.chkGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkGrid.Location = new System.Drawing.Point(458, 8);
            this.chkGrid.Name = "chkGrid";
            this.chkGrid.Size = new System.Drawing.Size(69, 18);
            this.chkGrid.TabIndex = 5;
            this.chkGrid.Text = "Show &Grid";
            this.chkGrid.UseVisualStyleBackColor = true;
            this.chkGrid.CheckedChanged += new System.EventHandler(this.chkGrid_CheckedChanged);
            //
            // buttonRotate
            //
            this.buttonRotate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonRotate.Location = new System.Drawing.Point(266, 5);
            this.buttonRotate.Name = "buttonRotate";
            this.buttonRotate.Size = new System.Drawing.Size(99, 23);
            this.buttonRotate.TabIndex = 2;
            this.buttonRotate.Text = "Rotate &Frame";
            this.buttonRotate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonRotate.UseVisualStyleBackColor = true;
            this.buttonRotate.Click += new System.EventHandler(this.buttonRotate_Click);
            //
            // btnRemoveCrop
            //
            this.btnRemoveCrop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveCrop.AutoSize = true;
            this.btnRemoveCrop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRemoveCrop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnRemoveCrop.Location = new System.Drawing.Point(6, 291);
            this.btnRemoveCrop.Name = "btnRemoveCrop";
            this.btnRemoveCrop.Size = new System.Drawing.Size(86, 22);
            this.btnRemoveCrop.TabIndex = 2;
            this.btnRemoveCrop.Text = "&Remove Crop";
            this.btnRemoveCrop.UseVisualStyleBackColor = true;
            this.btnRemoveCrop.Click += new System.EventHandler(this.btnRemoveCrop_Click);
            //
            // imageCropControl
            //
            this.imageCropControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imageCropControl.AspectRatio = null;
            this.imageCropControl.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.imageCropControl.Bitmap = null;
            this.imageCropControl.GridLines = false;
            this.imageCropControl.Location = new System.Drawing.Point(0, 32);
            this.imageCropControl.Name = "imageCropControl";
            this.imageCropControl.Size = new System.Drawing.Size(536, 243);
            this.imageCropControl.TabIndex = 1;
            //
            // CropEditor
            //
            this.Controls.Add(this.btnRemoveCrop);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.imageCropControl);
            this.Name = "CropEditor";
            this.Size = new System.Drawing.Size(536, 320);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenLiveWriter.Controls.ImageCropControl imageCropControl;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label lblAspectRatio;
        private System.Windows.Forms.ComboBox cbAspectRatio;
        private XPBitmapButton buttonRotate;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRemoveCrop;
        private System.Windows.Forms.CheckBox chkGrid;
    }
}
