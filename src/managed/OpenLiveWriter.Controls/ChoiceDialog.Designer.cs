using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    partial class ChoiceDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelHeading = new System.Windows.Forms.Label();
            this.panelBackground = new System.Windows.Forms.Panel();
            this.panelOptions = new System.Windows.Forms.Panel();
            this.labelSubheading = new System.Windows.Forms.Label();
            this.panelBackground.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(318, 238);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // labelHeading
            //
            this.labelHeading.AutoSize = true;
            this.labelHeading.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelHeading.Location = new System.Drawing.Point(20, 19);
            this.labelHeading.Name = "labelHeading";
            this.labelHeading.Size = new System.Drawing.Size(38, 15);
            this.labelHeading.TabIndex = 0;
            this.labelHeading.Text = "label1";
            //
            // panelBackground
            //
            this.panelBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBackground.BackColor = System.Drawing.SystemColors.Window;
            this.panelBackground.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelBackground.Controls.Add(this.panelOptions);
            this.panelBackground.Controls.Add(this.labelSubheading);
            this.panelBackground.Controls.Add(this.labelHeading);
            this.panelBackground.Location = new System.Drawing.Point(-2, -2);
            this.panelBackground.Name = "panelBackground";
            this.panelBackground.Size = new System.Drawing.Size(410, 230);
            this.panelBackground.TabIndex = 4;
            //
            // panelOptions
            //
            this.panelOptions.Location = new System.Drawing.Point(35, 67);
            this.panelOptions.Name = "panelOptions";
            this.panelOptions.Size = new System.Drawing.Size(239, 135);
            this.panelOptions.TabIndex = 2;
            //
            // labelSubheading
            //
            this.labelSubheading.AutoSize = true;
            this.labelSubheading.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelSubheading.Location = new System.Drawing.Point(22, 34);
            this.labelSubheading.Name = "labelSubheading";
            this.labelSubheading.Size = new System.Drawing.Size(38, 15);
            this.labelSubheading.TabIndex = 1;
            this.labelSubheading.Text = "label2";
            //
            // ChoiceDialog
            //
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(406, 272);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panelBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChoiceDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ChoiceDialog";
            this.panelBackground.ResumeLayout(false);
            this.panelBackground.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelHeading;
        private System.Windows.Forms.Panel panelBackground;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.Label labelSubheading;

    }
}